using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MotorControlBoard
{
    public class FKJ2000_V1 : IFZJ
    {
        string TransferSensorSense = "";
        string TransferSensorNoSense = "";
        string SenderToTransferTxt = "";//发卡机到中转机构脚本
        string TransferToCardSlotTxt = "";//中转机构到卡槽脚本
        string CardSlotToTransferTxt = "";//卡槽到中转机构脚本

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ2000_V1(int PortNum, byte devIndex, string scriptPath)
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

        public FKJ2000_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

                /// <summary>
        /// 功能开关设置
        /// </summary>
        public void FunctionSwitchSet()
        {
            if ((this.cfgFunctionSwitch & 0x10) == 0x10)//判断第五位是否为1，为1则说明是旧版的2000张
            {
                TransferSensorSense = "00";
                TransferSensorNoSense = "01";
                SenderToTransferTxt = "";//发卡机到中转机构脚本
                TransferToCardSlotTxt = "";//中转机构到卡槽脚本
                CardSlotToTransferTxt = "";//卡槽到中转机构脚本
            }
            else if ((this.cfgFunctionSwitch & 0x10) == 0x0)
            {
                TransferSensorSense = "01";
                TransferSensorNoSense = "00";
                SenderToTransferTxt = "";//发卡机到中转机构脚本
                TransferToCardSlotTxt = "";//中转机构到卡槽脚本
                CardSlotToTransferTxt = "";//卡槽到中转机构脚本
            }
        }

        /// <summary>
        /// 发证机初始化函数
        /// </summary>
        /// <returns>0为初始化成功，其余为失败</returns>
        public override uint DevInit()
        {
            uint r;
            string hexData, script;
            if (SensorState(8).Equals("01"))//如果对射传感检测到东西，则不允许运行
            {
                return ERR_CARD_WORRY_PLACE;
            }
            this.ClearDevCondition();
            this.SendCommand("0217", "", out hexData);

            script = udM.LoadScriptFromFile("01_整机初始化.txt");
            r = FKQRunScript(script, 0, 0, 0);
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
        /// 判断发卡兜是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsReady()
        {
            if (cfgTKDeviceType == 1)
            {
                string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",RF)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(6, 6);
                    if ("303030".Equals(cardState))//有卡
                        return true;
                    if ("303130".Equals(cardState))//有卡
                        return true;
                }
            }
            else if (cfgTKDeviceType == 2)
            {
                string script = "CARD1=K720(" + this.mTKQPort.ToString() + ",AP)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(10 * 2, 2);
                    if ("34".Equals(cardState))//有卡
                        return true;
                }
                return false;
            }
            else if (cfgTKDeviceType == 3)
            {
                string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",RF)";
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
        /// 判断入卡读卡位置是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInIcReader()
        {
            if (cfgTKDeviceType == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",RF)";
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(6, 6);
                        if ("303032".Equals(cardState))//没有卡
                            return true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                return false;
            }
            else if (cfgTKDeviceType == 2)
            {
                string script = "CARD1=K720(" + this.mTKQPort.ToString() + ",AP)";
                for (int i = 0; i < 4; i++)
                {
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(10 * 2, 2);
                        if ("33".Equals(cardState))//IC读卡器位置有卡
                            return true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                return false;
            }
            else if (cfgTKDeviceType == 3)//TTCE-D1801
            {
                string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",RF)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(7 * 2, 2);
                    if ("31".Equals(cardState) || "33".Equals(cardState))//IC读卡器位置有卡
                        return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 判断出卡口读卡器中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInIcReaderOutput()
        {
            if (SensorState(4).Equals("01"))
                return true;
            return false;
        }

        /// <summary>
        /// 判断中转机构是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInTransfer()
        {
            if (SensorState(9).Equals("00") || SensorState(10).Equals("00"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断出卡口是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInOutput()
        {
            if (SensorState(5).Equals("01"))
                return true;
            return false;
        }

        /// <summary>
        /// 获取传感器函数（2000版本中不需要此函数）
        /// </summary>
        /// <param name="x">卡槽的列号</param>
        /// <returns></returns>
        public override string PutCardSensorInfor(int x)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取电磁铁号码函数
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <returns>返回输入卡槽列数对应的电磁铁号码</returns>
        public override string GetCardJDQ(int x)
        {
            return x.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// 获取中转机构方向函数
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <param name="param">param为0：放平中转机构，param为1：放低中转机构</param>
        /// <returns>返回“01”代表向右方向偏下移动，返回“00”代表向左方向偏下移动，返回“”代表param参数输入错误</returns>
        protected string GetDirectionOfColumn(int x, int param)
        {
            //当param为0时，说明是要放平中转机构
            //当param为1时，说明是要放低中转机构
            if (param == 0)
            {
                if (x > 4)//判断是否是大于第四列的卡槽
                    return "01";
                else
                    return "00";
            }
            else if (param == 1)
            {
                if (x > 4)//判断是否是大于第四列的卡槽
                    return "00";
                else
                    return "01";
            }
            return "";
        }


        /// <summary>
        /// 卡槽列数对应的偏移角度函数
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <returns>返回输入卡槽列数对应的偏移角度</returns>
        protected string GetAngerOfColumn(int x)
        {
            if (x > 4)
                return Common.ConvertStringToHex(this.cfgAnger0_1);
            else
                return Common.ConvertStringToHex(this.cfgAnger0_0);
        }

        /// <summary>
        /// 入卡到发卡读卡位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToICReader()
        {
            if (this.CardIsInIcReader())
                return 0;
            if (this.CardIsReady())
            {
                if (cfgTKDeviceType == 1)
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("CARD1=TCD820M(" + this.mTKQPort.ToString() + ",DC)\r\n");
                    script.Append("1=DELAY(1000)\r\n");
                    if (0 == this.udM.RunScript(this.mDevIndex, script.ToString()))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        if ("06".Equals(strState))//没有卡 
                            return 0;
                    }
                }
                else if (cfgTKDeviceType == 2)
                {
                    //卡传送到读卡器位置
                    string script = "CARD1=K720(" + this.mTKQPort.ToString() + ",FC7)";
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        System.Threading.Thread.Sleep(200);
                        for (int i = 0; i < 5; i++)
                        {
                            Application.DoEvents();
                            if (CardIsInIcReader())
                                return 0;
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
                else if (cfgTKDeviceType == 3)
                {
                    //卡传送到读卡器位置
                    string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",FC7)";
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        System.Threading.Thread.Sleep(200);
                        for (int i = 0; i < 5; i++)
                        {
                            Application.DoEvents();
                            if (CardIsInIcReader())
                                return 0;
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
            }
            return ERR_PUT_CONTAINER_EMPTY;
        }

        /// <summary>
        /// 发卡读卡器位置入卡到中转机构函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCar()
        {
            uint r;
            string hexData;
            int motor5Position, motor10Position;

            //检查中转机构中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //检测读卡器中是否有卡
            if (!this.CardIsInIcReader())
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

                //移动Y轴
                r = this.MotoGoToPosition(10, this.cfgY_InCard, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                //移动X轴
                r = this.MotoGoToPosition(5, this.cfgX_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;

                MotoGetPosition(5, out motor5Position);
                if (motor5Position != cfgX_InCard)  //么有到达指定位置
                    return ERR_CARD_WRONG_POSITION | 5;

                MotoGetPosition(10, out motor10Position);
                if (motor10Position != cfgY_InCard)//么有到达指定位置
                    return ERR_CARD_WRONG_POSITION | 10;

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
            string script = udM.LoadScriptFromFile("02_02_电动读卡至中转机构.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
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

            //判断出卡器读卡位置或者是出卡口是否有卡
            if (!this.CardIsInIcReaderOutput() && !this.CardIsInOutput())
            {
                return ERR_GET_IC_READER_EMPTY;
            }

            //中转机构是否有卡
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
            string script = udM.LoadScriptFromFile("02_05_返卡入中转机构.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            return r;
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

            //检测中转区中是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            //移动Y轴到指定卡槽位置
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

            //放下中转机构到合适的入卡位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
            r = this.FKQRunScript(script, x, 0, 1);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }

            this.ClearDevCondition();
            if (x > 4)
                script = udM.LoadScriptFromFile("02_04_放卡入卡槽下.txt");
            else
                script = udM.LoadScriptFromFile("02_03_放卡入卡槽上.txt");
            r = this.FKQRunScript(script, x, y, 0);
            System.Threading.Thread.Sleep(1000);//延时1秒
            if (SensorState(8).Equals("01"))//如果放射传感器有感应，说明卡没有放正
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(500);
                if (x > 4)
                {
                    script = udM.LoadScriptFromFile("03_07_入卡到中转机构下.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
                    r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                    System.Threading.Thread.Sleep(500);
                    return ERR_PUT_CARD_TO_CARD_SLOT;
                }
                else
                {
                    script = udM.LoadScriptFromFile("03_06_入卡到中转机构上.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
                    r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                    System.Threading.Thread.Sleep(500);
                    return ERR_PUT_CARD_TO_CARD_SLOT;
                }
            }
            if (SensorState(8).Equals("01"))
            {
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
                r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                return ERR_PUT_CARD_TO_CARD_SLOT;
            }

            script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
            this.ClearDevCondition();
            r = this.FKQRunScript(script, x, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }
            return r;
        }

        /// <summary>
        /// 发卡读卡位置入卡到废卡槽函数
        /// </summary>
        /// <param name="param">0为0号废卡槽，1为1号废卡槽</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r;
            string hexData, script;

            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测对射传感是否有感应
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //检测中转区是否有卡
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

                if (param == 0)
                {
                    //移动Y轴到废卡兜位置
                    r = this.MotoGoToPosition(10, this.cfgY_BadSlot, true, false);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;
                }
                else if (param == 1)
                {
                    //移动Y轴到废卡兜位置
                    r = this.MotoGoToPosition(10, this.cfgY_BadSlot - 6100, true, false);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;
                }

                //移动X轴到废卡兜位置
                r = this.MotoGoToPosition(5, this.cfgX_BadSlot, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;
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
            script = udM.LoadScriptFromFile("02_04_放卡入卡槽下.txt");
            r = this.FKQRunScript(script, 5, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
            }

            return 0;
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
            string hexData, script;
            //检测中转机构中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //检测机箱内是否有阻碍物
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            //到指点的卡槽位置
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

                r = MotoGotoCardSlot(false, x, y, false);
                if (r != 0)
                    return r;

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

            //抬起中转机构到合适的出卡位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
            r = this.FKQRunScript(script, x, 0, 1);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }

            //推卡入中转机构
            if (x > 4)
                script = udM.LoadScriptFromFile("03_02_推卡入中转机构下.txt");
            else
                script = udM.LoadScriptFromFile("03_01_推卡入中转机构上.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                //放平中转机构
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
                r = this.FKQRunScript(script, x, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | 4;
                }
                return ERR_GET_CARD_TO_TRANSFER_CAR;
            }

            //放平中转机构
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_02_中转机构角度偏移.txt");
            r = this.FKQRunScript(script, x, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 4;
            }

            return 0;
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

            //移动到出卡口读卡器位置
            for (int i = 0; i < 20; i++)
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

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("03_03_取卡到读卡器.txt");
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_GET_CARD_TO_ICREADER;
            return r;
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

            //判断卡是否在出卡读卡器
            if (!this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_EMPTY;

            string script = udM.LoadScriptFromFile("03_04_取卡到用户.txt");
            this.ClearDevCondition();
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
                return ERR_GET_CARD_TO_USER;
            return r;
        }

        /// <summary>
        /// 从出卡口或者出卡读卡位置取卡到废卡槽函数
        /// </summary>
        /// <param name="param">废卡槽号</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;

            //出卡器返回中转机构
            r = this.PutCardToTransferCarFromOutput();
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_GET_CARD_TO_TRANSFER_CAR;
            }

            //将卡放到废卡槽
            r = PutCardToBadCardSlot(param);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_CARD_SLOT;
            }

            return r;
        }

        /// <summary>
        /// 将电机松开（伺服电机除外）函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint MotorUnlock()
        {
            uint r;

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("05_01_松开电机.txt");
            r = udM.RunScript(this.mDevIndex, script);
            return r;
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
                    this.ClearDevCondition();
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
        /// 将电磁铁复原到初始位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public uint ElectromagnetTakeBack()
        {
            uint r;

            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile("05_02_继电器收回.txt");
            r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        protected override string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = str.Replace("##INREAERPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##ANGER##", this.GetAngerOfColumn(x));
            str = str.Replace("##JDQ##", this.GetCardJDQ(x));
            str = str.Replace("##OUTREAERPORT##", this.mOutCardReaderPort.ToString());
            str = str.Replace("##FX##", this.GetDirectionOfColumn(x, Params));
            if (cfgTKDeviceType == 1)
                str = str.Replace("##TK_CMD##", "TCD820M([TKQPORT],DC)");
            else if (cfgTKDeviceType == 2)
                str = str.Replace("##TK_CMD##", "K720([TKQPORT],FC0)");
            else if (cfgTKDeviceType == 3)
                str = str.Replace("##TK_CMD##", "D1801([TKQPORT],FC0)");
            return str;
        }

        //重写父类中的MotoGotoCardSlot方法
        public override uint MotoGotoCardSlot(bool isGetCard, int xIndex, int yIndex, bool waitCompleted)
        {
            uint r;
            int step;
            int motor5Position = 0;
            int motor10Position = 0;
            int startLinePosition;
            //首先计算要走到的卡槽的坐标
            if (yIndex >= this.cfgY_SlotCount)
                return 0xEE9;
            if (xIndex >= this.cfgX_SlotCount)
                return 0xEEA;
            if (yIndex >= 0)
            {
                step = cfgY_Slot0 + cfgY_Interval * yIndex;
                if (cfgY_AdaptTime != 0)
                {
                    if (yIndex < 0)
                        step -= (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    else
                        if (xIndex > 4)
                        {
                            step += (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM + cfgY_Excursion;
                        }
                        else
                        {
                            step += (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                        }
                }
            }
            else
            {
                step = cfgY_InCard;
                if (isGetCard)
                    step = cfgY_OutCard;
            }
            r = MotoGoToPosition(10, step, true, waitCompleted);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 10;
            }

            if (xIndex <= 4)
            {
                startLinePosition = cfgX_Slot0 + xIndex * cfgX_Interval;
            }
            else
            {
                startLinePosition = cfgX_Slot0 + (xIndex - 4) * cfgX_Interval + cfgX_Excursion;
            }
            
            if (isGetCard)
                startLinePosition = cfgX_OutCard;
            r = this.MotoGoToPosition(5, startLinePosition, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 5;
            }

            MotoGetPosition(10, out motor10Position);//判断是否走对了步数
            if (motor10Position != step)
                return ERR_CARD_WRONG_POSITION;

            MotoGetPosition(5, out motor5Position);//判断是否走对了步数
            if (motor5Position != startLinePosition)
                return ERR_CARD_WRONG_POSITION;
            return 0;
        }
    }
}
