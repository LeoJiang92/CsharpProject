
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
//==========================================================
//名称：FKJ1000_V3
//说明：这个类适用于带以下功能的1000张发证机：
//              1.使用两个欧姆龙读卡器（入卡出卡各一个）
//              2.出卡口有一个拉卡器；
//              3.中转机构用两个槽式传感器(10，9号传感器)；
//备注：1.此类暂无使用Y轴数卡槽传感器；
//      2.此类为X，Y轴异步移动；  
//      3.发卡机用的是TTCE-D2000的时候，读卡器用的是欧姆龙；
//==========================================================

namespace MotorControlBoard
{
    public class FKJ1000_V3 : IFZJ
    {

        string InitScriptTxt = "";//发卡机初始化脚本
        string CardToInReaderScriptTxt = "";//入卡到读卡器脚本
        string InReaderToTransferScriptTxt = "";//入卡到中转机构
        string OutReaderToTransferScriptTxt = "";//出卡读卡器返回中转机构
        string OuterToOutReaderScriptTxt = "";//出卡口返回出卡读卡器
        string TransferToOutReaderScriptTxt = "";//中转机构到出卡读卡器
        string OutReaderToUserScriptTxt = "";//出卡读卡器到用户
        string OutReaderToCardPocketScriptTxt = "";//出卡读卡器到面板卡兜

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ1000_V3(int PortNum, byte devIndex, string scriptPath)
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

        public FKJ1000_V3(UsbDeviceManager dev, byte devIndex, string scriptPath)
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
            string runStep1, runStep2;

            //功能选择开关
            FunctionSwitchSet();

            ////获取电机10步数
            //runStep1 = GetMotorStep("A");
            ////移动电机10向下移动20步数
            //MotoRun(10, true, 1, 20);
            ////判断电机10步数
            //runStep2 = GetMotorStep("A");
            //if (runStep1.Equals(runStep2))
            //{
            //    this.ClearDevCondition();
            //    return ERR_MOVE_MOTOR | 10;
            //}

            //对射传感检测到有东西，则不允许运行
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁状态是否正常
            if (this.SensorState(15).Equals("00"))
                return ERR_ELECTROMAGNET_STATE;

            this.ClearDevCondition();
            this.SendCommand("0217", "", out hexData);

            string script = udM.LoadScriptFromFile(InitScriptTxt);
            uint r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT;
            }

            if (this.SendCommand("0216", "", out hexData) == 0)
            {
                if (!"00".Equals(hexData.Replace(" ", "")))
                    return ERR_MOVE_MOTOR | 1;
            }
            return r;
        }

        private string GetMotorStep(string motorNum)
        {
            string hexRData;
            string cmd = "0237";
            StringBuilder hexReData = new StringBuilder();
            hexReData.Append(motorNum.ToString().PadLeft(2, '0'));
            this.SendCommand(cmd, hexReData.ToString(), out hexRData);
            return hexRData;
        }

        /// <summary>
        /// 功能开关设置
        /// </summary>
        public void FunctionSwitchSet()
        {
            if ((this.cfgFunctionSwitch & 0x04) == 0x04)//判断第3位是否为1，为1则说明是身份证版
            {
                InitScriptTxt = "01_整机初始化(二合一读卡器版).txt";
                CardToInReaderScriptTxt = "02_01_入卡到读卡器(二合一读卡器版).txt";
                InReaderToTransferScriptTxt = "02_02_入卡到中转机构(二合一读卡器版).txt";
                OutReaderToTransferScriptTxt = "02_05_读卡器返卡到中转机构(二合一读卡器版).txt";
                OuterToOutReaderScriptTxt = "02_06_出卡口返卡到读卡器(二合一读卡器版).txt";
                TransferToOutReaderScriptTxt = "03_02_取卡到读卡器(二合一读卡器版).txt";
                OutReaderToUserScriptTxt = "03_03_取卡到用户(二合一读卡器版).txt";
                OutReaderToCardPocketScriptTxt = "03_04_取卡到卡兜(二合一读卡器版).txt";
            }
            else if ((this.cfgFunctionSwitch & 0x04) == 0x0)
            {
                InitScriptTxt = "01_整机初始化(欧姆龙读卡器版).txt";
                CardToInReaderScriptTxt = "02_01_入卡到读卡器(欧姆龙读卡器版).txt";
                InReaderToTransferScriptTxt = "02_02_入卡到中转机构(欧姆龙读卡器版).txt";
                OutReaderToTransferScriptTxt = "02_05_读卡器返卡到中转机构(欧姆龙读卡器版).txt";
                OuterToOutReaderScriptTxt = "02_06_出卡口返卡到读卡器(欧姆龙读卡器版).txt";
                TransferToOutReaderScriptTxt = "03_02_取卡到读卡器(欧姆龙读卡器版).txt";
                OutReaderToUserScriptTxt = "03_03_取卡到用户(欧姆龙读卡器版).txt";
                OutReaderToCardPocketScriptTxt = "03_04_取卡到卡兜(欧姆龙读卡器版).txt";
            }
        }

        /// <summary>
        /// 检测发卡兜内是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsReady()
        {
            if ((this.cfgFunctionSwitch & 0x04) == 0x04)
            {
                string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",RF)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(7 * 2, 2);
                    if ("34".Equals(cardState))//有卡
                        return true;
                }
            }
            else if ((this.cfgFunctionSwitch & 0x04) == 0x0)
            {
                if (this.cfgTKDeviceType == 4)//TTCE-D1801
                {
                    string script = "CARD1=D2000(" + this.mTKQPort.ToString() + ",AP)";
                    if (0 == this.udM.RunScript(this.mDevIndex, script, false))
                    {
                        string strState = this.udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(4 * 2, 2);
                        if ("31".Equals(cardState) || "30".Equals(cardState))//少卡或者足卡
                            return true;
                    }
                }
                else if (this.cfgTKDeviceType == 6)//TYIM2000-150
                {
                    string script = "CARD1=TYIM2000(" + this.mTKQPort.ToString() + ",h,k,A)";
                    if (this.udM.RunScript(this.mDevIndex, script, false) == 0)
                    {
                        string strState = this.udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(8 * 2, 2);
                        if ("38".Equals(cardState))//无卡
                            return false;
                        else
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检测发卡读卡位置中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool CardIsInIcReader()
        {
            //判断是银行卡读卡还是身份证读卡(1为身份证版，0为银行卡版)
            if ((this.cfgFunctionSwitch & 0x04) == 0x04)
            {
                string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",RF)";
                for (int i = 0; i < 4; i++)
                {
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(7 * 2, 2);
                        if ("31".Equals(cardState) || "33".Equals(cardState))//IC读卡器位置有卡
                            return true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
            else if ((this.cfgFunctionSwitch & 0x04) == 0x0)
            {
                string script = "A=V2CF(" + this.mInCardReaderPort.ToString() + ",11)";
                if (0 == this.udM.RunScript(this.mDevIndex, script, false))
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
            }
            return false;
        }

        /// <summary>
        /// 检测出卡读卡位置中是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInIcReaderOutput()
        {
            if ((this.cfgFunctionSwitch & 0x04) == 0x04)
            {
                if (SensorState(4).Equals("01"))
                {
                    return true;
                }
            }
            else if ((this.cfgFunctionSwitch & 0x04) == 0x0)
            {
                string script = "B=V2CF(" + this.mOutCardReaderPort.ToString() + ",11)";
                if (0 == this.udM.RunScript(this.mDevIndex, script, false))
                {
                    string re = udM.GetParam(0, "B");
                    if (re.Equals("0000303030303030303030303030"))
                    {
                        return false;
                    }
                    else if (re.Equals("0002313030303030303030303030"))
                    {
                        return true;
                    }
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
            if (this.SensorState(9).Equals("01"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测出卡口位置是否有卡函数
        /// </summary>
        /// <returns>返回true为有卡，返回false为无卡</returns>
        public override bool CardIsInOutput()
        {
            if (this.SensorState(5).Equals("01"))
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

        System.Threading.Mutex mutexGlobals = new System.Threading.Mutex();
        /// <summary>
        /// 入卡到发卡读卡位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToICReader()
        {
            uint r;
            string script;
            int position = 0;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                OutputLog("进入PutCardToICReader", false);
                //发卡机内是否有卡
                if (!this.CardIsReady())
                    return ERR_PUT_CONTAINER_EMPTY;
                //判断IC读卡器里是不是已经有卡
                if (this.CardIsInIcReader())
                    return ERR_PUT_IC_READER_BLOCK;

                if ((this.cfgFunctionSwitch & 0x01) == 0x01)//欧姆龙出卡不会被阻挡，就不用做这些处理
                {
                    r = this.MotoGoToPosition(5, this.cfgX_InCard, true, false);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_MOVE_MOTOR | 5;
                    }
                    r = this.MotoGoToPosition(10, this.cfgY_InCard, true, false);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_MOVE_MOTOR | 10;
                    }
                }
                else
                {
                    if (this.MotoGetPosition(10, out position))
                    {
                        if (position < this.cfgY_InCard + 800 && position > this.cfgY_InCard - 1700)
                        {
                            r = this.MotoGoToPosition(10, this.cfgY_InCard - 1700, true, true);
                            if (r != 0)
                            {
                                return ERR_MOVE_MOTOR | 10;
                            }
                            this.MotoGoToPosition(5, this.cfgX_InCard, true, false);
                        }
                        else
                        {
                            r = this.MotoGoToPosition(5, this.cfgX_InCard, true, false);
                            if (position < this.cfgY_InCard - 2000)
                            {
                                this.MotoGoToPosition(10, this.cfgY_InCard - 1700, true, false);
                            }
                            else if (position > this.cfgY_InCard + 800)
                            {
                                this.MotoGoToPosition(10, this.cfgY_InCard + 500, true, false);
                            }
                        }
                    }
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(CardToInReaderScriptTxt);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                int xP, yP;
                int xP2, yP2;
                int i = 0;
                while ((i < 30) && this.MotoGetPosition(5, out xP) && this.MotoGetPosition(10, out yP))
                {
                    System.Threading.Thread.Sleep(100);
                    this.MotoGetPosition(5, out xP2);
                    this.MotoGetPosition(10, out yP2);
                    if ((xP2 == xP) && (yP2 == yP))
                        break;
                    i++;
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 发卡读卡器位置入卡到中转机构函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCar()
        {
            uint r;
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                OutputLog("进入PutCardToTransferCar", false);
                //检测读卡器中是否有卡
                if (!this.CardIsInIcReader())
                    return ERR_PUT_IC_READER_EMPTY;
                //检查中转机构中是否有卡
                if (this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_BLOCK;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测机箱内是否有阻碍物
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;
                r = this.MotoTransferCarGotoPosition(cfgX_InCard, cfgY_InCard, true);
                OutputLog("    进入PutCardToTransferCar_中转定位完成R=" + r, false);
                if (r != 0)
                    return r;

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(InReaderToTransferScriptTxt);
                r = this.FKQRunScript(script, 0, 0, 0);
                OutputLog("    进入PutCardToTransferCar_执行脚本(02_02_入卡到中转机构) R=" + r, false);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_TRANSFER_CAR;
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 出卡器读卡位置（或者出卡口位置）返卡到中转机构函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCarFromOutput()
        {
            uint r;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {

                //判断读卡器或者是出卡口是否有卡
                if (!this.CardIsInIcReaderOutput() && !this.CardIsInOutput())
                    return ERR_GET_IC_READER_EMPTY;
                //中转区中是否有卡
                if (this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_BLOCK;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;
                r = MotoTransferCarGotoPosition(cfgX_OutCard, cfgY_OutCard, true);
                if (r != 0)
                    return r;

                if (this.CardIsInIcReaderOutput())
                {
                    this.ClearDevCondition();
                    string script = udM.LoadScriptFromFile(OutReaderToTransferScriptTxt);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_PUT_CARD_TO_TRANSFER_CAR;
                    }
                    return r;
                }
                else if (this.CardIsInOutput())
                {
                    this.ClearDevCondition();
                    string script = udM.LoadScriptFromFile(OuterToOutReaderScriptTxt);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_PUT_CARD_TO_ICREADER;
                    }

                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(OutReaderToTransferScriptTxt);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_PUT_CARD_TO_TRANSFER_CAR;
                    }
                    return r;
                }

                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
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
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                OutputLog("进入PutCardToCardSlot", false);
                //检测中转区中是否有卡
                if (!this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_EMPTY;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;

                r = MotoGotoCardSlot(false, x, y, false);
                OutputLog("   进入PutCardToCardSlot_进行到位：R=" + r, false);
                if (r != 0)
                    return r;
                this.ClearDevCondition();
                OutputLog("   进入PutCardToCardSlot_清出条件：R=" + r, false);
                script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                OutputLog("   进入PutCardToCardSlot_执行(04_02_抬起中转机构.txt)：R=" + r, false);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 4;
                }

                this.ClearDevCondition();
                int[] b0 = this.GetSensorCheckNum();
                OutputLog("   进入PutCardToCardSlot_清出条件：R=" + r, false);
                script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                r = this.FKQRunScript(script, x, y, 0);
                OutputLog("   进入PutCardToCardSlot_执行(02_03_入卡到卡槽.txt)：脚本长度[" + script.Length + "]R=" + r, false);
                int[] b1 = this.GetSensorCheckNum();
                for (int t = 0; t < b0.Length; t++)
                {
                    if (b0[t] != b1[t])
                        break;
                    else if ((t == b0.Length - 1) && b0[t] == b1[t])
                        MessageBox.Show("入卡到卡槽时，所有传感都没有变化过!");
                }
                if (r != 0)//判断脚本是否出错（电机不转动的情况）
                {
                    if (SensorState(9).Equals("01"))//如果卡还在中转机构，没有往前推
                    {
                        if (SensorState(8).Equals("00"))
                        {
                            System.Threading.Thread.Sleep(500);
                            this.MotoRun(0, true, 1, 20);
                            OutputLog("       进入PutCardToCardSlot_电机移动20步：R=" + r, false);
                            System.Threading.Thread.Sleep(500);
                            this.ClearDevCondition();
                            OutputLog("       进入PutCardToCardSlot_清出条件：R=" + r, false);
                            script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                            r = this.FKQRunScript(script, x, y, 0);
                            OutputLog("       进入PutCardToCardSlot_重试执行(02_03_入卡到卡槽.txt)：R=" + r, false);
                        }
                    }
                }
                if (r != 0)
                {
                    this.ClearDevCondition();
                    if (SensorState(8).Equals("01"))
                    {
                        //回收
                        script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        OutputLog("   进入PutCardToCardSlot_执行(02_04_卡返回到中转机构.txt)：R=" + r, false);
                        this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                        OutputLog("   进入PutCardToCardSlot_执行收电磁铁", false);
                        System.Threading.Thread.Sleep(500);
                        if (SensorState(8).Equals("01"))
                        {
                            this.ClearDevCondition();
                            return ERR_CARD_WORRY_PLACE;
                        }
                    }
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_04_复位中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    OutputLog("   进入PutCardToCardSlot_执行(04_04_复位中转机构.txt)：R=" + r, false);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 5;
                    }
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    OutputLog("   进入PutCardToCardSlot_执行(04_02_抬起中转机构.txt)：R=" + r, false);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 8;
                    }
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                    r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                    OutputLog("   进入PutCardToCardSlot_执行(02_03_入卡到卡槽.txt)：R=" + r, false);
                    if (r != 0)
                    {
                        System.Threading.Thread.Sleep(200);
                        this.ClearDevCondition();
                        script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        OutputLog("   进入PutCardToCardSlot_执行(02_04_卡返回到中转机构.txt)：R=" + r, false);
                        this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                        System.Threading.Thread.Sleep(500);
                        if (SensorState(8).Equals("01"))
                        {
                            this.ClearDevCondition();
                            return ERR_GET_CARD_TO_TRANSFER_CAR;
                        }
                        this.ClearDevCondition();
                        script = udM.LoadScriptFromFile("04_04_复位中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        if (r != 0)
                        {
                            this.ClearDevCondition();
                            return ERR_EXECUTE_SCRIPT | 5;
                        }
                        return ERR_EXECUTE_SCRIPT | 7;
                    }
                }
                //if (SensorState(8).Equals("01") || SensorState(9).Equals("01"))//如果放射传感器有感应，说明卡没有完全放入
                //{
                //    //这里如果插错卡槽很容易把其它卡也拉出来，所以不重试
                //    this.ClearDevCondition();
                //    //以下步骤为重新调整角度并尝试继续放卡：
                //    //1.将卡收回如入中转机构；
                //    //2.中转机构放平；
                //    //3.中转机构抬起；
                //    //4.尝试继续将卡放入卡槽；
                //    string hexOut;
                //    this.SendCommand("0206", "0000", out hexOut);
                //    this.ClearDevCondition();
                //    System.Threading.Thread.Sleep(50);
                //    //this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 01)", x, y, 0);//弹电磁铁
                //    System.Threading.Thread.Sleep(200);
                //    script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                //    r = this.FKQRunScript(script, x, y, 0);
                //    this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                //    System.Threading.Thread.Sleep(500);
                //    if ((r == 0) && (!SensorState(8).Equals("01")))
                //    {
                //        System.Threading.Thread.Sleep(200);
                //        this.ClearDevCondition();
                //        script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                //        r = this.FKQRunScript(script, x, y, 0);
                //        if (r != 0)
                //        {
                //            this.ClearDevCondition();
                //            return ERR_EXECUTE_SCRIPT | 7;
                //        }
                //        System.Threading.Thread.Sleep(200);
                //        this.ClearDevCondition();
                //        script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                //        r = this.FKQRunScript(script, x, y, 0);
                //        if (r != 0)
                //        {
                //            this.ClearDevCondition();
                //            return ERR_EXECUTE_SCRIPT | 8;
                //        }
                //        System.Threading.Thread.Sleep(200);
                //        this.ClearDevCondition();
                //        script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                //        r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                //        if (r != 0)
                //        {
                //            this.ClearDevCondition();
                //            System.Threading.Thread.Sleep(200);
                //            script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                //            r = this.FKQRunScript(script, x, y, 0);
                //            this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                //            System.Threading.Thread.Sleep(500);
                //            if (SensorState(8).Equals("01"))
                //            {
                //                this.ClearDevCondition();
                //                return ERR_GET_CARD_TO_TRANSFER_CAR;
                //            }
                //            this.ClearDevCondition();
                //            script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                //            r = this.FKQRunScript(script, x, y, 0);
                //            if (r != 0)
                //            {
                //                this.ClearDevCondition();
                //                return ERR_EXECUTE_SCRIPT | 5;
                //            }
                //            return ERR_EXECUTE_SCRIPT | 7;
                //        }
                //        System.Threading.Thread.Sleep(200);
                //    }
                //    else if (!SensorState(8).Equals("01"))
                //    {
                //        this.ClearDevCondition();
                //        script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                //        r = this.FKQRunScript(script, x, y, 0);
                //    }
                //}
                if (SensorState(8).Equals("01"))
                {
                    //1.将卡收回中转机构；
                    //2.放平中转机构；                    
                    string hexOut;
                    this.SendCommand("0206", "0000", out hexOut);
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_CARD_SLOT;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 11;
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 中转区入卡到坏卡槽函数
        /// </summary>
        /// <param name="param">0为0号废卡槽，1为1号废卡槽</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r = 0;
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                //检测中转区是否有卡
                if (!this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_EMPTY;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;

                if (param == 0)
                {
                    //移动X,Y轴到废卡兜0位置
                    r = this.MotoTransferCarGotoPosition(cfgX_BadSlot, this.cfgY_BadSlot, true);
                }
                else//(param == 1)
                {
                    //移动X,Y轴到废卡兜1位置
                    r = this.MotoTransferCarGotoPosition(cfgX_BadSlot, this.cfgY_BadSlot - this.cfgY_BadCard_Excursion, true);
                }
                this.ClearDevCondition();
                if (r != 0)
                    return r;
                for (int i = 0; i < 3; i++)
                {
                    script = udM.LoadScriptFromFile("04_01_放卡到废卡槽.txt");
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r == 0)
                    {
                        if (!CardIsInTransfer())
                            break;
                        else
                        {
                            this.ClearDevCondition();
                            System.Threading.Thread.Sleep(400);
                        }
                    }
                    else
                    {
                        if (!CardIsInTransfer())
                            break;
                    }
                }
                this.ClearDevCondition();
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
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
            int changeFrequency1, changeFrequency2, changeFrequency3;//电磁铁传感器跳变频率
            int correlationCF1, correlationCF2;//对射传感器跳变频率
            BoardConfig bc = new BoardConfig();
            //降低转动速度
            //if (ReadBoardConfig(ref bc) != 0)
            //    return ERR_EXECUTE_SCRIPT | 1;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                //r = SendCommand("0202", "00BC020000", out rhexData);//降低转速到700
                //if (r != 0)
                //    return ERR_EXECUTE_SCRIPT | 2;

                //中转区中是否有卡
                if (this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_BLOCK;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
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

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 3;
                }

                changeFrequency1 = this.GetSensorChangeCount(15);//获取电磁铁传感跳变数
                correlationCF1 = this.GetSensorChangeCount(8);//获取对射传感跳变数

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("03_01_取卡到中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);
                System.Threading.Thread.Sleep(150);
                if (this.SensorState(9).Equals("00") && (r != 0))
                {
                    changeFrequency2 = this.GetSensorChangeCount(15);
                    correlationCF2 = this.GetSensorChangeCount(8);
                    if (changeFrequency2 == changeFrequency1)//判断电磁铁是否有问题（相等就是电磁铁没有推卡）
                    {
                        //尝试继续取卡到中转机构
                        this.ClearDevCondition();
                        script = udM.LoadScriptFromFile("03_01_取卡到中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);
                        System.Threading.Thread.Sleep(150);
                        if (r != 0)
                        {
                            changeFrequency3 = this.GetSensorChangeCount(15);
                            string hexOut;
                            this.ClearDevCondition();
                            if (!this.SensorState(8).Equals("01"))//没有卡挡住才放平
                            {
                                this.SendCommand("0206", "0000", out hexOut);
                                script = udM.LoadScriptFromFile("04_04_复位中转机构.txt");
                                r = this.FKQRunScript(script, x, y, 0);
                                if (r != 0)
                                {
                                    this.ClearDevCondition();
                                    return ERR_EXECUTE_SCRIPT | 4;
                                }
                            }
                            if (changeFrequency3 == changeFrequency2)//如果传感器还是相等，则说明电磁铁（或者是传感有问题）
                                return ERR_ELECTROMAGNET_CHANGE;
                            else//如果不是，则是脚本出错
                                return ERR_GET_CARD_TO_TRANSFER_CAR;
                        }
                    }
                    if (correlationCF1 == correlationCF2)//对射传感没有变化
                    {
                        if (!this.SensorState(8).Equals("01"))//没有卡挡住才放平
                        {
                            string hexOut;
                            this.SendCommand("0206", "0000", out hexOut);
                            this.ClearDevCondition();
                            script = udM.LoadScriptFromFile("04_04_复位中转机构.txt");
                            r = this.FKQRunScript(script, x, y, 0);
                            if (r != 0)
                            {
                                this.ClearDevCondition();
                                return ERR_EXECUTE_SCRIPT | 4;
                            }
                        }
                        return ERR_CARD_SLOT_EMPTY;
                    }
                    else//电磁铁没有问题，则是脚本出错
                    {
                        string hexOut;
                        this.ClearDevCondition();
                        this.SendCommand("0206", "0000", out hexOut);
                        if ((!SensorState(8).Equals("01")))
                        {
                            script = udM.LoadScriptFromFile("04_04_复位中转机构.txt");
                            r = this.FKQRunScript(script, x, y, 0);
                            if (r != 0)
                            {
                                this.ClearDevCondition();
                                return ERR_EXECUTE_SCRIPT | 5;
                            }
                        }
                        return ERR_GET_CARD_TO_TRANSFER_CAR;
                    }
                }
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;
                //检测中转区中是否有卡
                if (!this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_EMPTY;

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 6;
                }
                return 0;
            }
            finally
            {
               // byte[] bs = BitConverter.GetBytes(bc.Frequency1);
                mutexGlobals.ReleaseMutex();
                //SendCommand("0202", "00" + UsbDeviceManager.ConvertBytesToHexString(bs, 4, ""), out rhexData);
            }
        }

        /// <summary>
        /// 中转机构取卡片到出卡口读卡位置函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToICReader()
        {
            uint r;
            int motor5Position, motor10Position;
            string hexData, script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                //判断中转区是否有卡
                if (!this.CardIsInTransfer())
                    return ERR_TRANSFER_CAR_EMPTY;
                //判断读卡器是否有卡
                if (this.CardIsInIcReaderOutput())
                    return ERR_GET_IC_READER_BLOCK;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;
                //电磁铁状态是否正常
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
                //检测对射传感是否有感应
                if (this.SensorState(8).Equals("01"))
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

                    //去到出卡位置
                    r = MotoGotoOutCardPosition(0);
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
                        {
                            this.MotoGetPosition(10, out motor10Position);//判断是否走对了步数
                            this.MotoGetPosition(5, out motor5Position);//判断是否走对了步数
                            if ((motor10Position == cfgY_OutCard) && (motor5Position == cfgX_OutCard))
                                break;
                        }
                    }
                }

                this.MotoGetPosition(10, out motor10Position);//判断是否走对了步数
                this.MotoGetPosition(5, out motor5Position);//判断是否走对了步数
                if (motor10Position != cfgY_OutCard)
                    return ERR_CARD_WRONG_POSITION | 10;

                if (motor5Position != cfgX_OutCard)
                    return ERR_CARD_WRONG_POSITION | 5;

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TransferToOutReaderScriptTxt);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_GET_CARD_TO_ICREADER;
                }
                for (int i = 0; i < 5; i++)
                {
                    if (this.CardIsInIcReaderOutput())
                        break;
                    System.Threading.Thread.Sleep(200);
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 出卡读卡位置取卡到出卡口函数
        /// </summary>
        /// <param name="isHoldCard">是否夹卡</param>
        /// <param name="step">步数（暂时不用）</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r;
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                //判断读卡器中是否有卡
                if (!this.CardIsInIcReaderOutput())
                    return ERR_GET_IC_READER_EMPTY;

                if (isHoldCard)
                {
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(OutReaderToUserScriptTxt);
                    r = FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_GET_CARD_TO_USER;
                    }
                }
                else
                {
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(OutReaderToCardPocketScriptTxt);
                    r = FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_GET_CARD_TO_USER;
                    }
                }

                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 从出卡口或者出卡读卡位置取卡到废卡槽函数
        /// </summary>
        /// <param name="param">废卡槽号</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                //将卡从出卡口读卡器放回中转机构
                if (PutCardToTransferCarFromOutput() != 0)
                    return ERR_PUT_CARD_TO_TRANSFER_CAR;

                //将卡放入卡槽
                r = PutCardToBadCardSlot(param);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_GET_CARD_TO_BAD_CARD_SLOT;
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 提示灯开关
        /// </summary>
        /// <param name="OnOrOff">true为开，false为关</param>
        /// <returns>0为成功，其余为失败</returns>
        public uint ReminderLight(bool OnOrOff)
        {
            uint r = 0;
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                this.ClearDevCondition();
                if (OnOrOff)
                {
                    script = udM.LoadScriptFromFile("05_03_提示灯开.txt");
                    r = udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_REMINDERLIGHT;
                    }
                }
                else
                {
                    script = udM.LoadScriptFromFile("05_04_提示灯关.txt");
                    r = udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_REMINDERLIGHT;
                    }
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 门锁开关
        /// </summary>
        /// <param name="OnOrOff">true为开，false为关</param>
        /// <returns>0为成功，其余为失败</returns>
        public uint GateLock(bool OnOrOff)
        {
            uint r = 0;
            string script;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                this.ClearDevCondition();
                if (OnOrOff)
                {
                    script = udM.LoadScriptFromFile("05_05_门锁开.txt");
                    r = udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_GATELOCK;
                    }
                }
                else
                {
                    script = udM.LoadScriptFromFile("05_06_门锁关.txt");
                    r = udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_GATELOCK;
                    }
                }
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        /// <summary>
        /// 将电机松开（伺服电机除外）函数
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint MotorUnlock()
        {
            uint r;
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                this.ClearDevCondition();
                string script = udM.LoadScriptFromFile("05_01_松开电机.txt");
                r = udM.RunScript(this.mDevIndex, script);
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
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
                catch (Exception)
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
            if (!mutexGlobals.WaitOne())
                return 0x0000E510;
            try
            {
                this.ClearDevCondition();
                string script = udM.LoadScriptFromFile("05_02_继电器收回.txt");
                r = udM.RunScript(this.mDevIndex, script);
                return r;
            }
            finally
            {
                mutexGlobals.ReleaseMutex();
            }
        }

        protected override string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = str.Replace("##INREADERPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##JDQ##", this.GetCardJDQ(x));
            str = str.Replace("##OUTREADERPORT##", this.mOutCardReaderPort.ToString());
            return str;
        }

        /// <summary>
        /// 快熟盘库函数，用于判断卡槽是否有卡
        /// </summary>
        /// <param name="x">X轴坐标</param>
        /// <param name="y">Y轴坐标</param>
        /// <returns>0为有卡，1为有无卡，其余为报错</returns>
        public override uint SlotIsEmpty(int x, int y)
        {
            uint r;
            string hexData, script;
            int sensor8State, sensor15State1, sensor15State2;

            //中转区中是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //电磁铁收回
            if (this.ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            //电磁铁状态是否正常
            if (this.SensorState(15).Equals("00"))
                return ERR_ELECTROMAGNET_STATE;
            //检测对射传感是否有感应
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;

            //移动到卡槽位
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

            //抬起中转机构
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 3;
            }

            sensor8State = this.GetSensorChangeCount(8);
            sensor15State1 = this.GetSensorChangeCount(15);

            //取卡到中转机构
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("03_01_取卡到中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);
            System.Threading.Thread.Sleep(150);
            if (r != 0 && this.SensorState(9).Equals("00"))
            {
                //判断是否为空卡槽
                sensor15State2 = this.GetSensorChangeCount(15);
                if (this.GetSensorChangeCount(8) == sensor8State)
                {
                    if (!this.SensorState(8).Equals("01"))//没有卡挡住才放平
                    {
                        string hexOut;
                        this.SendCommand("0206", "0000", out hexOut);
                        this.ClearDevCondition();
                        script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        if (r != 0)
                        {
                            this.ClearDevCondition();
                            return ERR_EXECUTE_SCRIPT | 4;
                        }
                    }
                    return 1;
                }
                //判断电磁铁有没有动作
                if (sensor15State2 == sensor15State1)
                {
                    //尝试继续取卡到中转机构
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("03_01_取卡到中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);
                    System.Threading.Thread.Sleep(150);
                    if (r != 0)
                    {
                        if (sensor15State2 == this.GetSensorChangeCount(15))
                        {
                            if (!this.SensorState(8).Equals("01"))//没有卡挡住才放平
                            {
                                string hexOut;
                                this.SendCommand("0206", "0000", out hexOut);
                                this.ClearDevCondition();
                                script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                                r = this.FKQRunScript(script, x, y, 0);
                                if (r != 0)
                                {
                                    this.ClearDevCondition();
                                    return ERR_EXECUTE_SCRIPT | 4;
                                }
                            }
                            return ERR_ELECTROMAGNET_CHANGE;
                        }
                    }

                }
                if (SensorState(8).Equals("01"))
                {
                    //尝试继续取卡到中转机构
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("03_01_取卡到中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);
                    System.Threading.Thread.Sleep(150);
                    if (r != 0)
                    {
                        if (!this.SensorState(8).Equals("01"))//没有卡挡住才放平
                        {
                            string hexOut;
                            this.SendCommand("0206", "0000", out hexOut);
                            this.ClearDevCondition();
                            script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                            r = this.FKQRunScript(script, x, y, 0);
                            if (r != 0)
                            {
                                this.ClearDevCondition();
                                return ERR_EXECUTE_SCRIPT | 4;
                            }
                        }
                        return ERR_EXECUTE_SCRIPT | 5;
                    }
                }
            }


            //将卡放回卡槽
            if (this.SensorState(9).Equals("00"))
            {
                return ERR_TRANSFER_CAR_EMPTY;
            }

            this.ClearDevCondition();
            OutputLog("   进入PutCardToCardSlot_清出条件：R=" + r, false);
            script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
            r = this.FKQRunScript(script, x, y, 0);
            OutputLog("   进入PutCardToCardSlot_执行(02_03_入卡到卡槽.txt)：脚本长度[" + script.Length + "]R=" + r, false);
            if (r != 0)//判断脚本是否出错（电机不转动的情况）
            {
                if (SensorState(9).Equals("01"))//如果卡还在中转机构，没有往前推
                {
                    if (SensorState(8).Equals("00"))
                    {
                        System.Threading.Thread.Sleep(500);
                        this.MotoRun(0, true, 1, 20);
                        OutputLog("       进入PutCardToCardSlot_电机移动20步：R=" + r, false);
                        System.Threading.Thread.Sleep(500);
                        this.ClearDevCondition();
                        OutputLog("       进入PutCardToCardSlot_清出条件：R=" + r, false);
                        script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        OutputLog("       进入PutCardToCardSlot_重试执行(02_03_入卡到卡槽.txt)：R=" + r, false);
                    }
                }
            }
            if (r != 0)
            {
                this.ClearDevCondition();
                if (SensorState(8).Equals("01"))
                {
                    //回收
                    script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    OutputLog("   进入PutCardToCardSlot_执行(02_04_卡返回到中转机构.txt)：R=" + r, false);
                    this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                    OutputLog("   进入PutCardToCardSlot_执行收电磁铁", false);
                    System.Threading.Thread.Sleep(500);
                    if (SensorState(8).Equals("01"))
                    {
                        return ERR_CARD_WORRY_PLACE;
                    }
                }
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                OutputLog("   进入PutCardToCardSlot_执行(04_03_放平中转机构.txt)：R=" + r, false);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 5;
                }
                System.Threading.Thread.Sleep(200);
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                OutputLog("   进入PutCardToCardSlot_执行(04_02_抬起中转机构.txt)：R=" + r, false);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 8;
                }
                System.Threading.Thread.Sleep(200);
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                OutputLog("   进入PutCardToCardSlot_执行(02_03_入卡到卡槽.txt)：R=" + r, false);
                if (r != 0)
                {
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    OutputLog("   进入PutCardToCardSlot_执行(02_04_卡返回到中转机构.txt)：R=" + r, false);
                    this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                    System.Threading.Thread.Sleep(500);
                    if (SensorState(8).Equals("01"))
                    {
                        this.ClearDevCondition();
                        return ERR_GET_CARD_TO_TRANSFER_CAR;
                    }
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 5;
                    }
                    return ERR_EXECUTE_SCRIPT | 7;
                }
            }
            if (SensorState(8).Equals("01"))//如果放射传感器有感应，说明卡没有完全放入
            {
                //这里如果插错卡槽很容易把其它卡也拉出来，所以不重试
                this.ClearDevCondition();
                //以下步骤为重新调整角度并尝试继续放卡：
                //1.将卡收回如入中转机构；
                //2.中转机构放平；
                //3.中转机构抬起；
                //4.尝试继续将卡放入卡槽；
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(50);
                //this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 01)", x, y, 0);//弹电磁铁
                System.Threading.Thread.Sleep(200);
                script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                r = this.FKQRunScript(script, x, y, 0);
                this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                System.Threading.Thread.Sleep(500);
                if ((r == 0) && (!SensorState(8).Equals("01")))
                {
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 7;
                    }
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_02_抬起中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 8;
                    }
                    System.Threading.Thread.Sleep(200);
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("02_03_入卡到卡槽.txt");
                    r = this.FKQRunScript(script, x, y, 0);//继续尝试放卡
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        System.Threading.Thread.Sleep(200);
                        script = udM.LoadScriptFromFile("02_04_卡返回到中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        this.FKQRunScript("1=S(0211 " + x.ToString().PadLeft(2, '0') + " 00)", x, y, 0);//收电磁铁 
                        System.Threading.Thread.Sleep(500);
                        if (SensorState(8).Equals("01"))
                        {
                            this.ClearDevCondition();
                            return ERR_GET_CARD_TO_TRANSFER_CAR;
                        }
                        this.ClearDevCondition();
                        script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                        r = this.FKQRunScript(script, x, y, 0);
                        if (r != 0)
                        {
                            this.ClearDevCondition();
                            return ERR_EXECUTE_SCRIPT | 5;
                        }
                        return ERR_EXECUTE_SCRIPT | 7;
                    }
                    System.Threading.Thread.Sleep(200);
                }
                else if (!SensorState(8).Equals("01"))
                {
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
                    r = this.FKQRunScript(script, x, y, 0);
                }
            }
            if (SensorState(8).Equals("01"))
            {
                //1.将卡收回中转机构；
                //2.放平中转机构；                    
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_CARD_SLOT;
            }

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile("04_03_放平中转机构.txt");
            r = this.FKQRunScript(script, x, y, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 11;
            }
            return r;
        }
    }

}
