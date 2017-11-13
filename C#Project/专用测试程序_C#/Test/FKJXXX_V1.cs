using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJXXX_V1
//说明：这个类适用于带以下功能的1000张发证机：
//              1.使用电动读卡器（固定）；
//              2.无X轴右限位传感器（更换成Y轴数卡槽传感器）；
//              3.使用欧姆龙作为出卡口；
//              4.中转机构只用一个槽式传感器(10号传感器)；
//备注：1.此类暂无使用Y轴数卡槽传感器；
//      2.此类为X，Y轴异步移动；  
//      3.当发卡机用的是TTCE-D2000的时候，读卡器用的是欧姆龙；
//      4.当发卡机用TY900的时候，读卡器用的是TYRM3000；
//最后修改时间：2016/9/28
//==========================================================

namespace MotorControlBoard
{
    public class FKJXXX_V1 : IFZJ
    {
        protected int mCardSlotCount = 250;
        //protected int CurrentCardSlot = 0;//中转区当前卡槽

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJXXX_V1(int PortNum, byte devIndex, string scriptPath)
        {
            if (devIndex >= 100)
            {
                udM = new UsbDeviceManager(0x0483, 0x5702);
                if (udM.GetDevicesCount() == 0)
                {
                    throw new Exception("没有可用的USB设备");
                }
            }
            else
            {
                udM = new SerialDeviceManager();
                uint r = udM.OpenDev((byte)PortNum);
                if (r != 0)
                    throw new Exception("打开串口出错:COM" + PortNum);
            }
            SetDevInfor(udM, 0, scriptPath);
        }

        private void SetDevInfor(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            mScriptPath = scriptPath;
            udM = dev;
            udM.ScriptPath = scriptPath;
            mDevIndex = devIndex;
        }

        public FKJXXX_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

        /// <summary>
        /// 发证机初始化函数
        /// </summary>
        /// <returns>0为初始化成功，其余为失败</returns>
        public override uint DevInit()
        {
            string hexData;
            if (SensorState(8).Equals("01"))//对射传感检测到有东西，则不允许运行
            {
                return ERR_CARD_WORRY_PLACE;
            }
            this.ClearDevCondition();
            this.SendCommand("0217", "", out hexData);

            string filePath = "01_整机初始化.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_INIT;

            if (this.SendCommand("0216", "", out hexData) == 0)
            {
                if (!"00".Equals(hexData.Replace(" ", "")))
                    return ERR_MOVE_MOTOR;
            }
            return r;
        }

        /// <summary>
        /// 检测发卡读卡位置中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInIcReader()
        {
            string script = "CARD1=TYRM3000(" + this.mInCardReaderPort.ToString() + ",h,c,A)";
            if (0 == this.udM.RunScript(this.mDevIndex, script))
            {
                string strState = udM.GetParam(this.mDevIndex, "CARD1");
                string cardState = strState.Substring(10, 2);
                if ("61".Equals(cardState))//没有卡
                    return false;
                else
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 检测出卡读卡位置中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInIcReaderOutput()
        {
            string script = "A=V2CF(" + this.mOutCardReaderPort.ToString() + ",11)";
            if (0 == this.udM.RunScript(this.mDevIndex, script))
            {
                string re = udM.GetParam(0, "A");
                if (re.Equals("0000303030303030303030303030"))
                {
                    return false;
                }
                else if (re.Equals("0002313030303030303030303030"))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 检测出卡口位置是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInOutput()
        {
            string script = "A=V2CF(" + this.mOutCardReaderPort.ToString() + ",11)";
            if (0 == this.udM.RunScript(this.mDevIndex, script))
            {
                string re = udM.GetParam(0, "A");
                if (re.Equals("0000303030303030303030303030"))
                {
                    return false;
                }
                else if (re.Equals("0001303030303031303130303030") || re.Equals("0001303030303031313130303030"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测中转机构中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInTransfer()
        {
            if (SensorState(10).Equals("01"))
            {
                return true;
            }
            return false;
        }

        public override string PutCardSensorInfor(int x)
        {
            if (x == 0)
            {
                return "0B";
            }
            else
            {
                return "09";
            }
        }

        public override string GetCardJDQ(int x)
        {
            return x.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// 检测和返回传感器状态函数
        /// </summary>
        /// <param name="num">传感器号</param>
        /// <returns>返回"01"为高电平，"00"为低电平</returns>
        public string SensorState(int num)
        {
            string rHexData;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    udM.SendCommandAndWaitAck(this.mDevIndex, "0204", "", out rHexData, 500);
                    string[] sensorState = rHexData.Split(' ');
                    return sensorState[num];
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            throw new Exception("发送命令错误");
        }

        /// <summary>
        /// 检测发卡兜内是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsReady()
        {
            if (cfgTKDeviceType == 5)//TY900
            {
                string script = "CARD1=TY900(" + mTKQPort + ",RF)";
                for (int i = 0; i < 5; i++)
                {
                    if (0 == FKQRunScript(script, 0, 0, 0))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        if ("30".Equals(strState.Substring(10, 2)))//准备好
                            return true;
                        else if ("50".Equals(strState.Substring(10, 2)))
                        {
                            System.Threading.Thread.Sleep(400);
                        }
                        else if ("33".Equals(strState.Substring(10, 2)) || "35".Equals(strState.Substring(10, 2)))
                        {
                            return false;
                        }
                        else if (strState.StartsWith("FFFFFFFFFFFFFF"))
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                    System.Threading.Thread.Sleep(200);
                }
            }
            else if (cfgTKDeviceType == 4)
            {
                string script = "CARD1=D2000(" + this.mTKQPort.ToString() + ",RF)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(7 * 2, 2);
                    if ("34".Equals(cardState))//有卡
                        return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 入卡到发卡读卡位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToICReader()
        {
            uint r;
            this.ClearDevCondition();
            //发卡机内是否有卡
            if (!CardIsReady())
                return ERR_PUT_CONTAINER_EMPTY;
            //判断IC读卡器里是不是已经有卡
            if (this.CardIsInIcReader())
                return ERR_PUT_IC_READER_BLOCK;

            if (cfgTKDeviceType == 5)//当发卡机用TY900的时候，读卡器用的是天翼的TYRM3000
            {
                string script1 = "1=TY900(" + mTKQPort.ToString() + ",DC)";
                string script2 = "1=DELAY(200)\r\nTKQ1=TY900(" + mTKQPort.ToString() + ",RF)";

                for (int i = 0; i < 5; i++)
                {
                    r = this.FKQRunScript(script1, 0, 0, 0);
                    if (r != 0)
                        return ERR_EXECUTE_SCRIPT | 1;
                    for (int j = 0; j < 10; j++)
                    {
                        r = this.FKQRunScript(script2, 0, 0, 0);
                        if (r != 0)
                            return ERR_EXECUTE_SCRIPT | 2;
                        string tkqState = udM.GetParam(mDevIndex, "TKQ1");
                        if ("31".Equals(tkqState.Substring(10, 2)))//发送成功
                            return 0;
                        if (this.CardIsInIcReader())
                            return 0;
                    }

                }
                return ERR_PUT_CARD_TO_ICREADER;
            }
            else if (cfgTKDeviceType == 4)//当发卡机用的是TTCE-D2000的时候，读卡器用的是欧姆龙
            {
                string script1 = "1=D2000(" + mTKQPort.ToString() + ",DC)";
                string script2 = "1=";

                return ERR_PUT_CARD_TO_ICREADER;

            }
            return ERR_PUT_CARD_TO_ICREADER;

            #region IC读卡器后进卡版
            //string script1 = "1=TY900(" + mTKQPort.ToString() + ",DC)";
            //string script2 = "1=DELAY(200)\r\nTKQ1=TY900(" + mTKQPort.ToString() + ",RF)";
            //string script3 = "1=DELAY(150)\r\n1=TYRM3000(" + mICReaderPort.ToString() + ",h,h,A)";

            //for (int i = 0; i < 5; i++)
            //{
            //    r = this.FKQRunScript(script1, 0, 0, 0);
            //    if (r != 0)
            //        return ERR_EXECUTE_SCRIPT | 1;
            //    for (int j = 0; j < 10; j++)
            //    {
            //        r = this.FKQRunScript(script3, 0, 0, 0);
            //        if (r != 0)
            //            return ERR_EXECUTE_SCRIPT | 3;
            //        r = this.FKQRunScript(script2, 0, 0, 0);
            //        if (r != 0)
            //            return ERR_EXECUTE_SCRIPT | 2;
            //        string tkqState = udM.GetParam(mDevIndex, "TKQ1");
            //        if ("31".Equals(tkqState.Substring(10, 2)))//发送成功
            //            return 0;
            //        if (this.CardIsInIcReader())
            //            return 0;
            //    }

            //}
            //return ERR_PUT_CARD_TO_ICREADER;
            #endregion
        }

        /// <summary>
        /// 发卡读卡器位置入卡到中转机构函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCar()
        {
            uint r;
            string hexData;
            int motor5Position = 0;
            int motor10Position = 0;
            //int position = 0;
            this.ClearDevCondition();
            //检查中转机构中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //检测读卡器中是否有卡
            if (!CardIsInIcReader())
                return ERR_PUT_IC_READER_EMPTY;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测机箱内是否有阻碍物
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            for (int i = 0; i <= 20; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 20)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);

                //移动X轴
                r = this.MotoGoToPosition(5, this.cfgX_InCard, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;

                //移动Y轴
                r = this.MotoGoToPosition(10, this.cfgY_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                MotoGetPosition(5, out motor5Position);
                if (motor5Position != cfgX_InCard)  //么有到达指定位置
                    return ERR_CARD_WRONG_POSITION;

                MotoGetPosition(10, out motor10Position);
                if (motor10Position != cfgY_InCard)//么有到达指定位置
                    return ERR_CARD_WRONG_POSITION;

                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 20)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                    else
                        break;
                }
            }

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("02_02_电动读卡至中转区.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            }
            return r;
        }

        /// <summary>
        /// 中转区入卡到坏卡槽
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //检测中转区是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            r = this.MotoGoToPosition(5, this.cfgX_BadSlot, true, false);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            r = this.MotoGoToPosition(10, this.cfgY_BadSlot, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 10;

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("04_01中转区到坏卡收集.txt");
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_PUT_CARD_TO_BAD_CARD_SLOT;

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4 ;
            }
            return 0;
        }

        /// <summary>
        /// 中转机构入卡到卡槽函数
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <param name="y">卡槽行数</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToCardSlot(int x, int y)
        {
            uint r;
            string hexData, script;
            this.ClearDevCondition();
            //检测中转区中是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            for (int i = 0; i <= 20; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 20)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);

                //去到卡槽位置
                r = MotoGotoCardSlot(false, x, y, false);
                if (r != 0)
                    return r;

                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 20)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                    else
                        break;
                }
            }

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_02抬起中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("02_03_放卡入卡槽.txt");
            r = this.FKQRunScript(script, x, y, 0);
            System.Threading.Thread.Sleep(500);//延时500毫秒
            if (SensorState(8).Equals("01"))//如果放射传感器有感应，说明卡没有放正
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(500);
                script = udM.LoadScriptFromFile("03_01取卡到临时区.txt");
                r = this.FKQRunScript(script, x, y, 0);
                System.Threading.Thread.Sleep(500);
                script = udM.LoadScriptFromFile("02_03_放卡入卡槽.txt");
                r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
            }
            if (SensorState(8).Equals("01"))
            {
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | 4;
                }
                return ERR_PUT_CARD_TO_CARD_SLOT;
            }
            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }
            return r;
        }

        /// <summary>
        /// 卡槽推卡入中转机构函数
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <param name="y">卡槽行数</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r;
            string hexData;
            this.ClearDevCondition();
            //判断卡是否在读卡器里
            if (this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_BLOCK;
            //中转区中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            for (int i = 0; i <= 20; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 20)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);
                //去到卡槽位置
                r = this.MotoGotoCardSlot(false, x, y, false);
                if (r != 0)
                    return r;
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 20)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                    else
                        break;
                }
            }

            string script = udM.LoadScriptFromFile("04_02抬起中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }

            script = udM.LoadScriptFromFile("03_01取卡到临时区.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | 4;
                }
                return ERR_GET_CARD_TO_TRANSFER_CAR;
            }

            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }
            return 0;
        }

        protected override string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = str.Replace("##ICPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##JDQ##", this.GetCardJDQ(x));
            str = str.Replace("##V2CPORT##", this.mOutCardReaderPort.ToString());
            //str = str.Replace("##CGQ##", this.get(x));
            return str;
        }

        /// <summary>
        /// 中转机构取卡片到出卡口读卡位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToICReader()
        {
            uint r;
            string hexData;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //判断中转区是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            for (int i = 0; i <= 20; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 20)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);

                //X轴移动到出卡口
                r = this.MotoGoToPosition(5, this.cfgX_OutCard, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;
                //Y轴移动到出卡口
                r = this.MotoGoToPosition(10, this.cfgY_OutCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 5)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(2);
                    }
                    else
                        break;
                }
            }

            string script = udM.LoadScriptFromFile("03_02取卡到电动读卡器.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_GET_CARD_TO_ICREADER;
            return r;

            #region 欧姆龙后出卡版
            //string script1 = "1=V2CF(" + mV2CPort.ToString() + ",:0)\r\n1=Delay(200)";
            //string script2 = udM.LoadScriptFromFile("03_02取卡到电动读卡器.txt");
            //string script3 = "1=Delay(1000)\r\nV2CQ1=V2CF(" + mV2CPort.ToString() + ",11)";
            //string script4 = "1=V2CF(" + mV2CPort.ToString() + ",60)";
            //string script5 = "1=V2CF(" + mV2CPort.ToString() + ",05)";

            //r = this.FKQRunScript(script1, 0, 0, 0);
            //if (r != 0)
            //    return ERR_EXECUTE_SCRIPT | 1;
            //r = this.FKQRunScript(script2, 0, 0, 0);
            //if (r != 0)
            //    return ERR_EXECUTE_SCRIPT | 2;
            //r = this.FKQRunScript(script3, 0, 0, 0);
            //if (r != 0)
            //    return ERR_EXECUTE_SCRIPT | 3;
            //string v2cState = udM.GetParam(mDevIndex, "V2CQ1");
            //if (!v2cState.Equals("0002303030313130303030303030"))//发送到后成功
            //{
            //    r = this.FKQRunScript(script1, 0, 0, 0);
            //    if (r != 0)
            //        return ERR_EXECUTE_SCRIPT | 1;
            //    r = this.FKQRunScript(script3, 0, 0, 0);
            //    if (r != 0)
            //        return ERR_EXECUTE_SCRIPT | 3;
            //    if (!v2cState.Equals("0002303030313130303030303030"))
            //    {
            //        return ERR_GET_CARD_TO_ICREADER;
            //    }
            //    r = this.FKQRunScript(script5, 0, 0, 0);
            //    if (r != 0)
            //        return ERR_EXECUTE_SCRIPT | 5;
            //    r = this.FKQRunScript(script3, 0, 0, 0);
            //    if (r != 0)
            //        return ERR_EXECUTE_SCRIPT | 3;
            //    if (!v2cState.Equals("0002313030303030303030303030"))
            //    {
            //        return ERR_GET_CARD_TO_ICREADER;
            //    }
            //}
            // return 0;
            #endregion
        }

        /// <summary>
        /// 出卡读卡位置取卡到出卡口函数
        /// </summary>
        /// <param name="isHoldCard">是否夹卡（暂时不用）</param>
        /// <param name="step">步数（暂时不用）</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r;
            this.ClearDevCondition();

            //判断出卡口IC读卡器中是否有卡
            if (!this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_EMPTY;

            string script = udM.LoadScriptFromFile("03_03取卡到用户.txt");
            r = FKQRunScript(script, 0, 0, 0);
            //if (r == 0)
            //{
            //    if (CardIsInIcReaderOutput())
            //    {
            //        System.Threading.Thread.Sleep(800);
            //        r = FKQRunScript(script, 0, 0, 0);
            //    }
            //    if (r==0)
            //        return 0;
            //}
            //return ERR_GET_CARD_TO_USER;
            if (r != 0)
            {
                return ERR_GET_CARD_TO_USER;
            }
            return r;
        }

        /// <summary>
        /// 从出卡口或者出卡读卡位置取卡到废卡槽函数
        /// </summary>
        /// <param name="param">废卡槽号（暂时不用）</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;
            string hexData;

            //将卡从出卡口读卡器放回中转机构
            if (PutCardToTransferCarFromOutput() != 0)
                return ERR_PUT_CARD_TO_TRANSFER_CAR;

            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //判断中转区中是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            for (int i = 0; i <= 20; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 20)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);

                //X，Y轴移动到废卡槽
                r = this.MotoGoToPosition(5, this.cfgX_BadSlot, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;
                r = this.MotoGoToPosition(10, this.cfgY_BadSlot, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 5)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(2);
                    }
                    else
                        break;
                }
            }

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("04_01中转区到坏卡收集.txt");
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_GET_CARD_TO_BAD_CARD_SLOT;

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }
            return r;
        }

        /// <summary>
        /// 出卡器读卡位置（或者出卡口位置）返卡到中转机构函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCarFromOutput()
        {
            uint r;
            string hexData;

            //判断读卡器或者是出卡口是否有卡
            if (!this.CardIsInIcReaderOutput() && !this.CardIsInOutput())
                    return ERR_GET_IC_READER_EMPTY;
                
            //中转区中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            for (int i = 0; i <= 5; i++)
            {
                if (SensorState(8).Equals("01"))
                {
                    if (i == 5)
                        return ERR_CARD_WORRY_PLACE;
                    else
                    {
                        System.Threading.Thread.Sleep(2);
                        continue;
                    }
                }
                this.SendCommand("0217", "", out hexData);
                //X轴移动到出卡口
                r = this.MotoGoToPosition(5, this.cfgX_OutCard, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;
                //Y轴移动到出卡口
                r = this.MotoGoToPosition(10, this.cfgY_OutCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 5)
                            return ERR_MOVE_MOTOR | 10;
                        else
                            System.Threading.Thread.Sleep(2000);
                    }
                }
            }

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("02_02_OML读卡至中转区.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            return r;
        }

        /// <summary>
        /// 检测Y轴传感器跳变次数函数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int GetSensorCheckNum(int num)
        {
            int slotNum = 0;
            string hexOut;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    this.SendCommand("0401", "", out hexOut);
                    string[] sensorCheckNum = hexOut.Split(' ');
                    slotNum = Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
                    return slotNum;
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            throw new Exception("发送命令错误");
        }

        /// <summary>
        /// 将电机松开（伺服电机除外）函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint MotorUnlock()
        {
            uint r;

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("05_01松开电机.txt");
            r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 将电磁铁复原到初始位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public uint ElectromagnetTakeBack()
        {
            uint r;

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("05_02继电器收回.txt");
            r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 清空传感器跳变次数
        /// </summary>
        public void CleanSensorCheckNum()
        {
            string hexOut;
            this.SendCommand("0402", "", out hexOut);
        }

    }
}
