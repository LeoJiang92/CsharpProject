using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJ1000_V1
//说明：这个类适用于带以下功能的1000张发证机：
//              1.使用电动读卡器（带左右移动）；
//              2.无X轴右限位传感器（更换成Y轴数卡槽传感器）；
//备注：1.此类暂无使用Y轴数卡槽传感器；
//      2.此类为X，Y轴异步移动；      
//最后修改时间：2016/6/15
//==========================================================

namespace MotorControlBoard
{
    public class FKJ1000_V1 : IFZJ
    {
        protected int mCardSlotCount = 250;
        //protected int CurrentCardSlot = 0;//中转区当前卡槽

        public FKJ1000_V1(int PortNum, byte devIndex, string scriptPath)
        {
            UsbDeviceManager dev = null;
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

        public FKJ1000_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

        public override uint DevInit()
        {
            string hexData;
            if (SensorState(8).Equals("01"))//如果检测到有东西，则不允许运行
            {
                return ERR_CARD_WORRY_PLACE;
            }
            this.ClearDevCondition();
            this.SendCommand("0217", "", out hexData);
            string filePath = "01_整机初始化.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return r;
            if (this.SendCommand("0216", "", out hexData) == 0)
            {
                if (!"00".Equals(hexData.Replace(" ", "")))
                    return ERR_MOVE_MOTOR;
            }

            //this.CleanSensorCheckNum();//清空传感器跳变次数
            //r = this.MotoGoToPosition(10, 200*cfgY_Interval+cfgY_Slot0, true, true);//复位后移动到200卡槽位置
            //if (0 != r)
            //    return ERR_MOVE_MOTOR | 10;
            return r;
        }

        //是否还有卡没有发
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

        public override bool CardIsInIcReaderOutput()
        {
            return this.CardIsInIcReader();
        }

        public override bool CardIsInTransfer()
        {
            if (SensorState(9).Equals("00") || SensorState(10).Equals("00"))
            {
                return true;
            }
            return false;
        }

        public override bool CardIsInOutput()
        {
            return true;
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
        /// 检测和返回传感器状态
        /// </summary>
        /// <param name="num"></param>
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

        public override bool CardIsReady()
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
            return false;
        }

        /// <summary>
        /// 把卡放入到IC卡读卡器
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToICReader()
        {
            uint r;
            this.ClearDevCondition();
            //发卡机内是否有卡
            if (!this.CardIsReady())
                return ERR_PUT_CONTAINER_EMPTY;
            //判断IC读卡器里是不是已经有卡
            if (this.CardIsInIcReader())
                return ERR_PUT_IC_READER_BLOCK;
            ////判断中转区是否有卡
            //if (this.CardIsInTransfer())
            //    return ERR_TRANSFER_CAR_BLOCK;
            //移动电动读卡器
            r = this.MotoGoToPosition(2, this.cfgICReader_InCard, false, true);
            if (0 != r)
                return ERR_MOVE_MOTOR | 2;

            string script1 = "1=TY900(" + mTKQPort.ToString() + ",DC)";
            string script2 = "1=DELAY(200)\r\nTKQ1=TY900(" + mTKQPort.ToString() + ",RF)";
            string script3 = "1=DELAY(150)\r\n1=TYRM3000(" + this.mInCardReaderPort.ToString() + ",h,h,A)";

            for (int i = 0; i < 5; i++)
            {
                r = this.FKQRunScript(script1, 0, 0, 0);
                if (r != 0)
                    return ERR_EXECUTE_SCRIPT | 1;
                for (int j = 0; j < 10; j++)
                {
                    r = this.FKQRunScript(script3, 0, 0, 0);
                    if (r != 0)
                        return ERR_EXECUTE_SCRIPT | 3;
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

        /// <summary>
        /// 把卡放入中转机构
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCar()
        {
            uint r;
            string hexData;
            //int position = 0;
            this.ClearDevCondition();
            //检查中转机构里有没有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //准备备放卡
            if (!CardIsInIcReader())
                return ERR_PUT_IC_READER_EMPTY;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
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
                //电动读卡器
                r = this.MotoGoToPosition(2, this.cfgICReader_InCard, false, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 2;
                //移动X轴
                r = this.MotoGoToPosition(5, this.cfgX_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;

                //this.CleanSensorCheckNum();//清空传感器跳变次数
                //MotoGetPosition(10,out position);//获取当前步数

                //移动Y轴
                r = this.MotoGoToPosition(10, this.cfgY_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                //if (position >= cfgY_InCard)//判断当前步数大小
                //{
                //    this.CurrentCardSlot = CurrentCardSlot - GetSensorCheckNum(3) / 2;//获取卡槽位
                //}
                //else
                //{
                //    this.CurrentCardSlot = CurrentCardSlot + GetSensorCheckNum(3) / 2;//获取卡槽位
                //}

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
            string script = udM.LoadScriptFromFile("02_02_电动读卡至中转区.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
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
            this.ClearDevCondition();
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //先把临时区的卡放到坏槽
            string rHexData;
            if (udM.SendCommandAndWaitAck(this.mDevIndex, "0204", "", out rHexData, 500) == 0)
            {
                rHexData = rHexData.Replace(" ", "");
                if ("00".Equals(rHexData.Substring(18, 2)) || "00".Equals(rHexData.Substring(20, 2)))
                {
                    this.MotoGoToPosition(5, this.cfgX_BadSlot, true, false);
                    this.MotoGoToPosition(10, this.cfgY_BadSlot, true, true);
                    string script = udM.LoadScriptFromFile("04_01中转区到坏卡收集.txt");
                    r = FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                        return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
                    return 0;
                }
            }
            return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
        }

        /// <summary>
        /// 中转区入卡到卡槽
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override uint PutCardToCardSlot(int x, int y)
        {
            uint r;
            string hexData;
            //int position = 0;
            //int secPosition = 0;
            this.ClearDevCondition();
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

                //this.CleanSensorCheckNum();//清空传感跳变
                //this.MotoGetPosition(10, out position);//获取当前步数

                //去到卡槽位置
                r = MotoGotoCardSlot(false, x, y, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                //this.MotoGetPosition(10, out secPosition);//再次获取步数
                //if (position >= secPosition)
                //{
                //    this.CurrentCardSlot = CurrentCardSlot - GetSensorCheckNum(3)/2;
                //}
                //else
                //{
                //    this.CurrentCardSlot = CurrentCardSlot + GetSensorCheckNum(3)/2;
                //}

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
                return ERR_MOVE_MOTOR | 4;

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
                return ERR_PUT_CARD_TO_CARD_SLOT;
            }
            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
                return ERR_MOVE_MOTOR | 4;
            return r;
        }

        /// <summary>
        /// 从卡槽推卡到中转区
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
                r = this.MotoGotoCardSlot(false, x, y, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
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
                return ERR_MOVE_MOTOR | 41;

            script = udM.LoadScriptFromFile("03_01取卡到临时区.txt");
            r = this.FKQRunScript(script, x, y, 0);

            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                return ERR_GET_CARD_TO_TRANSFER_CAR;
            }
            script = udM.LoadScriptFromFile("04_03放平中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
                return ERR_MOVE_MOTOR | 42;
            return 0;
        }

        protected override string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = str.Replace("##ICPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##JDQ##", this.GetCardJDQ(x));
            //str = str.Replace("##CGQ##", this.get(x));
            return str;
        }

        /// <summary>
        /// 发卡机发卡到读卡器
        /// </summary>
        /// <returns></returns>
        public override uint GetCardToICReader()
        {
            uint r;
            string hexData;
            this.ClearDevCondition();
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
                //移动电动读卡器
                r = this.MotoGoToPosition(2, this.cfgICReader_OutCard, false, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 2;
                //X轴移动到出卡口
                r = this.MotoGoToPosition(5, this.cfgX_OutCard, true, true);
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
        }

        /// <summary>
        /// 出卡到用户
        /// </summary>
        /// <param name="isHoldCard"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r;
            this.ClearDevCondition();
            //移动电动读卡器
            r = this.MotoGoToPosition(2, this.cfgICReader_OutCard, false, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 2;

            string script = udM.LoadScriptFromFile("03_03取卡到用户.txt");
            r = FKQRunScript(script, 0, 0, 0);
            if (r == 0)
            {
                if (CardIsInIcReaderOutput())
                {
                    System.Threading.Thread.Sleep(800);
                    r = FKQRunScript(script, 0, 0, 0);
                }
                if (r == 0)
                    return 0;
            }
            return ERR_GET_CARD_TO_USER;
        }

        /// <summary>
        /// 出卡到坏卡槽
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;
            string rHexData;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            if (udM.SendCommandAndWaitAck(this.mDevIndex, "0204", "", out rHexData, 500) == 0)
            {
                rHexData = rHexData.Replace(" ", "");
                if ("00".Equals(rHexData.Substring(18, 2)) || "00".Equals(rHexData.Substring(20, 2)))
                {
                    this.MotoGoToPosition(5, this.cfgX_BadSlot, true, false);
                    r = this.MotoGoToPosition(10, this.cfgY_BadSlot, true, true);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;
                    string script = udM.LoadScriptFromFile("04_01中转区到坏卡收集.txt");
                    r = FKQRunScript(script, 0, 0, 0);
                    if (r == 0)
                        return 0;
                }
            }
            return ERR_GET_CARD_TO_BAD_CARD_SLOT;
        }

        /// <summary>
        /// 读卡器返回卡到中转区
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCarFromOutput()
        {
            uint r;
            string hexData;
            this.ClearDevCondition();
            //判断卡是否在读卡器里
            if (!this.CardIsInIcReaderOutput())
                return ERR_PUT_IC_READER_EMPTY;
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
                //移动电动读卡器
                r = this.MotoGoToPosition(2, this.cfgICReader_OutCard, false, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 2;
                //X轴移动到出卡口
                r = this.MotoGoToPosition(5, this.cfgX_OutCard, true, true);
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

            string script = udM.LoadScriptFromFile("02_02_电动读卡至中转区.txt");
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
        /// 将电机都松开
        /// </summary>
        /// <returns></returns>
        public override uint MotorUnlock()
        {
            this.ClearDevCondition();

            string filePath = "05_01松开电机.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 将电磁铁收回
        /// </summary>
        /// <returns></returns>
        public uint ElectromagnetTakeBack()
        {
            this.ClearDevCondition();

            string filePath = "05_02继电器收回.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = udM.RunScript(this.mDevIndex, script);
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

        /// <summary>
        /// Y轴二次确认函数
        /// </summary>
        /// <returns></returns>
        public int YPositionAffirmTwice(int cardSlot)
        {
            uint r = 0;
            int slotNum = 0;
            StringBuilder script = new StringBuilder();
            slotNum = GetSensorCheckNum(6);//获取感应步数
            if (slotNum % 2 == 0)//如果跳变次数为双数，则对不齐卡槽需调整位置
            {
                //调整位置代码
                script.Append("A=S(0301 01 03 10 00 0a 01 00 00000000)\r\n");
                script.Append("A=S(0301 00 00 00 11 0a 01 01 C8000000)\r\n");
                script.Append("1=TEST(03 00 02 00 0303 00)\r\n");
                r = this.FKQRunScript(script.ToString(), 0, 0, 0);
            }
            if (cardSlot > slotNum / 2)
            {
                //向下移动
                script.Append("A=S(0301 01 03 10 00 0a 01 00 00000000)\r\n");
                script.Append("A=S(0301 00 00 00 11 0a 01 01 C8000000)\r\n");
                script.Append("1=TEST(03 00 02 00 0303 00)\r\n");
                r = this.FKQRunScript(script.ToString(), 0, 0, 0);
            }
            else if (cardSlot < slotNum / 2)
            {
                //向上移动
                script.Append("A=S(0301 01 03 10 00 0a 01 00 00000000)\r\n");
                script.Append("A=S(0301 00 00 00 11 0a 01 00 C8000000)\r\n");
                script.Append("1=TEST(03 00 02 00 0303 00)\r\n");
                r = this.FKQRunScript(script.ToString(), 0, 0, 0);
            }
            //如果相等则不用动
            return 0;
        }
    }
}
