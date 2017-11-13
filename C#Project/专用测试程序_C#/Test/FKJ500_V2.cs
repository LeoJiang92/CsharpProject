using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJ500_V2
//说明：这个类适用于带以下功能的500张发证机：
//              1.有对射传感，用于检测卡是否在中转区和卡槽中间；
//              2.出卡机的读卡器在里面，可以实现将卡返回中转区（没有X轴左边，不能实现）；    
//最后修改时间：2016/6/22
//==========================================================

namespace MotorControlBoard
{
    public class FKJ500_V2 : IFZJ
    {
        //下列参数是依据cfgFunctionSwitch参数的不同而不同，在初始化的时候修改
        string LaserSensorSense = "00";//激光传感感应到的状态
        string LaserSensorNoSense = "01";//激光传感没有感应到的状态
        string TransferCheckTxt = "";//中转机构卡位校正脚本
        string SenderToTransferTxt = "";//发卡机到中转机构脚本
        string TransferToCardSlotTxt = "";//中转机构到卡槽脚本
        string CardSlotToTransferTxt = "";//卡槽到中转机构脚本
        string TranferTakeCardBack = "";//中转机构收卡脚本
        string OuterToTransferTxt = "";//出卡器到中转区脚本

        public int X = -1;
        public int Y = -1;

        /// <summary>
        /// 父类(IFZJ)中的udm没有实例化的时候通过此构造函数实例化,并同时为父类(IFZJ)中mScriptPath，udM和mDevIndex赋值
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备索引（大于100用USB口，小于100用串口）</param>
        /// <param name="scriptPath">脚本文件路径</param>
        public FKJ500_V2(int PortNum, byte devIndex, String scriptPath)
        {
            //devIndex大于100用USB接口，小于100用串口
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
            SetDevInfor(udM, 0, scriptPath);//赋值给mScriptPath，udM和mDevIndex
        }

        /// <summary>
        /// 父类(IFZJ)中的udm已经实例化可使用此构造函数为父类(IFZJ)中mScriptPath，udM和mDevIndex赋值
        /// </summary>
        /// <param name="dev">已经实例化的udm</param>
        /// <param name="devIndex">设备索引</param>
        /// <param name="scriptPath">脚本文件路径</param>
        public FKJ500_V2(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

        /// <summary>
        /// 为父类(IFZJ)中mScriptPath，udM和mDevIndex赋值
        /// </summary>
        /// <param name="dev">已经实例化的udm</param>
        /// <param name="devIndex">设备索引</param>
        /// <param name="scriptPath">脚本文件路径</param>
        private void SetDevInfor(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            mScriptPath = scriptPath;
            udM = dev;
            if (udM != null)
                udM.ScriptPath = scriptPath;
            mDevIndex = devIndex;
        }

        /// <summary>
        /// 获取传感器号
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <returns>返回输入卡槽列数的的传感器号</returns>
        public override string PutCardSensorInfor(int x)
        {
            if ((this.cfgFunctionSwitch & 0x10) == 0x0)
            {
                if (x == 0)
                {
                    return "0A";
                }
                else
                {
                    return "09";
                }
            }
            else
            {
                if (x == 0)
                {
                    return "0C";
                }
                else
                {
                    return "0A";
                }
            }
        }

        /// <summary>
        /// 获取继电器号
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <returns>返回输入卡槽列数的继电器号</returns>
        public override string GetCardJDQ(int x)
        {
            if (x == 0)
            {
                return "01";
            }
            else
            {
                return "00";
            }
        }

        /// <summary>
        /// 功能开关设置
        /// </summary>
        public void FunctionSwitchSet()
        {
            if ((this.cfgFunctionSwitch & 0x10) == 0x0)//判断中转是不是为槽式传感器
            {
                LaserSensorSense = "01";
                LaserSensorNoSense = "00";
                TransferCheckTxt = "01_01中转区卡位校正(槽式传感版).txt";//卡位校正脚本
                SenderToTransferTxt = "02_01发卡机到中转区(槽式传感版).txt";//发卡机到中转机构脚本
                TransferToCardSlotTxt = "02_02中转区到卡槽(槽式传感版).txt";//中转机构到卡槽脚本
                CardSlotToTransferTxt = "03_01卡槽到中转区(槽式传感版).txt";//卡槽到中转机构脚本
                TranferTakeCardBack = "03_05中转区收卡(槽式传感版).txt";//中转机构收卡脚本
                OuterToTransferTxt = "03_07出卡器到中转区(槽式传感版).txt";
            }
            else if ((this.cfgFunctionSwitch & 0x02) == 0x02)//判断第二位是否为1，为1则说明是新的扩展板
            {
                LaserSensorSense = "01";
                LaserSensorNoSense = "00";
                TransferCheckTxt = "01_01中转区卡位校正(新板).txt";//卡位校正脚本
                SenderToTransferTxt = "02_01发卡机到中转区(新板).txt";//发卡机到中转机构脚本
                TransferToCardSlotTxt = "02_02中转区到卡槽(新板).txt";//中转机构到卡槽脚本
                CardSlotToTransferTxt = "03_01卡槽到中转区(新板).txt";//卡槽到中转机构脚本
                TranferTakeCardBack = "03_05中转区收卡(新板).txt";//中转机构收卡脚本
                OuterToTransferTxt = "03_07出卡器到中转区(新板).txt";
            }
            else if ((this.cfgFunctionSwitch & 0x02) == 0)
            {
                LaserSensorSense = "00";
                LaserSensorNoSense = "01";
                TransferCheckTxt = "01_01中转区卡位校正.txt";//卡位校正脚本
                SenderToTransferTxt = "02_01发卡机到中转区.txt";//发卡机到中转机构脚本
                TransferToCardSlotTxt = "02_02中转区到卡槽.txt";//中转机构到卡槽脚本
                CardSlotToTransferTxt = "03_01卡槽到中转区.txt";//卡槽到中转机构脚本
                TranferTakeCardBack = "03_05中转区收卡.txt";//中转机构收卡脚本
            }
        }

        /// <summary>
        /// 用于检测传感器状态并返回
        /// </summary>
        /// <param name="num">传感器编号</param>
        /// <returns>"01"为高电平，"00"为低电平</returns>
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
        /// 检测出卡器位置中是否有卡，有卡则将卡片放入废卡兜
        /// </summary>
        /// <returns>返回脚本“01_02出卡器卡片检测”执行后的值</returns>
        public uint OuterCheck()
        {
            uint r;
            string filePath = "01_02出卡器卡片检测.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = this.udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 检测中转区中是否有卡，有卡则将卡调整到中转区的正中位置
        /// </summary>
        /// <returns>"03"为出错或者失败，"02"为无卡，"01"为有1卡槽有卡，"00"为0卡槽有卡</returns>
        public string TransferCheck()
        {
            uint r = 2;
            string script = udM.LoadScriptFromFile(TransferCheckTxt);
            this.ClearDevCondition();
            r = this.udM.RunScript(this.mDevIndex, script);
            if (r != 0)//出错返回"03"
                return "03";

            if ((this.cfgFunctionSwitch & 0x10) == 0x0)
            {
                if (SensorState(9).Equals(LaserSensorSense))
                    return "01";
                else if (SensorState(10).Equals(LaserSensorSense))
                    return "00";
                else
                    return "02";
            }
            else
            {
                if (SensorState(9).Equals(LaserSensorSense) && SensorState(10).Equals(LaserSensorSense))//左中转机构返回"01"
                    return "01";
                else if (SensorState(11).Equals(LaserSensorSense) && SensorState(12).Equals(LaserSensorSense))//右中转机构返回"00"
                    return "00";
                else if (!CardIsInTransfer())//无卡返回"02"
                    return "02";
            }

            return "03";
        }

        /// <summary>
        /// 判断中转机构是否有卡
        /// </summary>
        /// <returns>true为有卡，false为无卡</returns>
        public override bool CardIsInTransfer()
        {
            if ((this.cfgFunctionSwitch & 0x10) == 0x0)
            {
                if (SensorState(9).Equals(LaserSensorSense) || SensorState(10).Equals(LaserSensorSense))
                    return true;
            }
            else
            {
                if (SensorState(9).Equals(LaserSensorSense) || SensorState(10).Equals(LaserSensorSense) || SensorState(11).Equals(LaserSensorSense) || SensorState(12).Equals(LaserSensorSense))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断出卡器和读卡器位置是否有卡
        /// </summary>
        /// <returns>true为有卡，false为无卡</returns>
        public override bool CardIsInIcReaderOutput()
        {
            if (SensorState(5).Equals("01") || SensorState(6).Equals("01"))
            //因为惯性不能把卡带出传感6，所以如果这里也判断卡是不是传感6的话，会影响连续测试
            //if (SensorState(5).Equals("01"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测出卡口位置是否有卡
        /// </summary>
        /// <returns>true为有卡，false为无卡</returns>
        public override bool CardIsInOutput()
        {
            if (SensorState(6).Equals("01"))
                return true;
            return false;
        }

        /// <summary>
        /// 判断发卡器中是否有卡
        /// </summary>
        /// <returns>true为有卡，false为无卡</returns>
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
        /// 判断发卡器中的读卡器是否有卡
        /// </summary>
        /// <returns>true为有卡，false为无卡</returns>
        public override bool CardIsInIcReader()
        {
            if (this.cfgTKDeviceType == 1)//TCD-820M
            {
                for (int i = 0; i < 4; i++)
                {
                    string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",RF)";
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(6, 6);
                        if ("303032".Equals(cardState))//有卡
                            return true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                return false;
            }
            else if (this.cfgTKDeviceType == 2)//TTCE-K720
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
            else if (this.cfgTKDeviceType == 3)//TTCE-D1801
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
                return false;
            }
            return false;
        }

        /// <summary>
        /// Y轴和X轴移动到指定卡槽
        /// </summary>
        /// <param name="isGetCard">是放卡还是出卡</param>
        /// <param name="xIndex">X轴卡槽号</param>
        /// <param name="yIndex">Y轴卡槽号</param>
        /// <param name="waitCompleted">是否等待完成后再执行下一条指令</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint MotoGotoCardSlot(bool isGetCard, int xIndex, int yIndex, bool waitCompleted)
        {
            if (yIndex >= this.cfgY_SlotCount)
                return 0xEE9;
            if (xIndex >= this.cfgX_SlotCount)
                return 0xEEA;
            //X轴
            int step = cfgX_InCard + xIndex * cfgX_Interval;
            if (isGetCard)
                step = cfgX_OutCard + xIndex * cfgX_Interval;
            uint r = this.MotoGoToPosition(5, step, true, waitCompleted);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            //Y轴
            if (cfgY_AdaptTime == 0)
            {
                r = this.MotoGoToPosition(10, cfgY_Slot0 + yIndex * cfgY_Interval, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
            }
            else
            {
                this.MotoGoToPosition(10, cfgY_Slot0 + yIndex * cfgY_Interval + (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
            }
            return 0;
        }

        /// <summary>
        /// 初始化设备，将X轴，Y轴和推卡器初始化
        /// </summary>
        /// <returns>0为初始化正确，1为0卡槽有卡，2为1卡槽有卡，其余为初始化失败</returns>
        public override uint DevInit()
        {
            uint r;
            string hexData;

            //功能选择开关
            FunctionSwitchSet();

            //初始化前先检测中转区中是否有卡,有则调整卡在中转区中的位置
            string cardInTheTransfer = TransferCheck();
            if (cardInTheTransfer.Equals("03"))
                return ERR_INIT_CARD_SLOT_ADJUST;

            //检测出卡器是否有卡，有则将卡放入废卡兜
            r = OuterCheck();
            if (r != 0)
                return ERR_INIT_OUTER;

            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
            }

            //卡槽和中转区中有卡则停止初始化
            if (SensorState(8).Equals("01"))
            {
                return ERR_CARD_WORRY_PLACE;
            }
            this.SendCommand("0217", "", out hexData);

            //运行“01_整机初始化”脚本
            string filePath = "01_整机初始化.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(0, script);
            if (r != 0)
                return ERR_INIT;

            //初始化时如果急停会报错
            if (this.SendCommand("0216", "", out hexData) == 0)
            {
                if (!"00".Equals(hexData.Replace(" ", "")))
                    return ERR_MOVE_MOTOR | 10;
            }

            if (cardInTheTransfer.Equals("00"))//如果中转区中有卡，则提示有卡并停止运行
                return 1;
            else if (cardInTheTransfer.Equals("01"))
                return 2;

            return 0;
        }

        /// <summary>
        /// 中转区移动到出卡位置
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <returns>0为成功，其余为失败</returns>
        public uint TransferCardToOuter(int x)
        {
            uint r = 0;
            string hexData;
            r = MotoGoToPosition(5, cfgX_Interval * x + cfgX_OutCard, true, true);
            if (r != 0)
            {
                return ERR_MOVE_MOTOR | 5;
            }
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
                r = MotoGoToPosition(10, cfgY_OutCard, true, true);
                if (r != 0)
                {
                    return ERR_MOVE_MOTOR | 10;
                }
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 20)
                            return ERR_MOVE_MOTOR | 13;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                    else
                        break;
                }
            }
            return r;
        }

        /// <summary>
        /// 入卡到发卡器读卡位置
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToICReader()
        {
            uint r;
            int position = 0;
            //判断发卡机是否有卡
            if (!this.CardIsReady())
                return ERR_PUT_CONTAINER_EMPTY;
            //判断入卡读卡器是否有卡
            if (this.CardIsInIcReader())
                return 0;

            //移动Y轴和X轴到入卡位置
            if ((this.cfgFunctionSwitch & 0x01) == 0x01)//发卡机发卡时不会被阻挡，就不用做这些处理
            {
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
                    }
                    else
                    {
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

            if (this.cfgTKDeviceType == 1)
            {
                //因为发卡器出卡会向前顶一下，所以要把中转机构移开
                int currentY = 0;
                if (this.MotoGetPosition(10, out currentY))
                {
                    int currentSlotIndex = currentY / cfgY_Interval;
                    if (currentSlotIndex > 110 && currentSlotIndex < 160)
                    {
                        if (currentSlotIndex < 140)
                            r = this.MotoGoToPosition(10, cfgY_Interval * 110, true, true);
                        else
                            r = this.MotoGoToPosition(10, cfgY_Interval * 160, true, true);
                        if (r != 0)
                            return ERR_MOVE_MOTOR | 10;
                    }
                }

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
            else if (this.cfgTKDeviceType == 2)
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
            else if (this.cfgTKDeviceType == 3)
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
            return 0;
        }

        /// <summary>
        /// 发卡机读卡位置入卡到中转机构（500张不用此函数）
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToTransferCar()
        {
            return 0;
        }

        /// <summary>
        /// 中转机构入卡到卡槽
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <param name="y">卡槽行数</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToCardSlot(int x, int y)
        {
            string hexData;
            uint r;
            string filePath, script;
            int Xstep, Ystep;

            //判断输入参数是否合适
            if (x >= this.cfgX_SlotCount || y >= this.cfgY_SlotCount)
                return ERR_WRONG_PARAMS;
            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
            }
            //判断中转机构是否有卡,无卡就发卡到中转机构(用于在盘库时使用)
            if (!this.CardIsInTransfer())
            {
                //判断入卡读卡器是否有卡
                if (!this.CardIsInIcReader())
                    return ERR_PUT_IC_READER_EMPTY;
                //判断对射传感是否感应
                if (this.SensorState(8).Equals("01"))
                    return ERR_CARD_WORRY_PLACE;
                //电磁铁收回
                if (this.ElectromagnetTakeBack() != 0)
                    return ERR_ELECTROMAGNET_TAKEBACK;

                //X轴移动到入卡位置
                Xstep = cfgX_InCard + x * cfgX_Interval;
                r = this.MotoGoToPosition(5, Xstep, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;

                //Y轴移动到入卡位置
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

                    r = this.MotoGoToPosition(10, cfgY_InCard, true, true);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;

                    if (this.SendCommand("0216", "", out hexData) == 0)
                    {
                        if (!"00".Equals(hexData.Replace(" ", "")))
                        {
                            if (i == 20)
                                return ERR_MOVE_MOTOR | 13;
                            else
                                System.Threading.Thread.Sleep(500);
                        }
                        else
                            break;
                    }
                }

                //运行入卡到中转机构脚本
                script = udM.LoadScriptFromFile(SenderToTransferTxt);
                if (cfgTKDeviceType == 1)
                    script = script.Replace("##TK_CMD##", "TCD820M([TKQPORT],DC)");
                else if (cfgTKDeviceType == 2)
                    script = script.Replace("##TK_CMD##", "K720([TKQPORT],FC0)");
                else if (cfgTKDeviceType == 3)
                    script = script.Replace("##TK_CMD##", "D1801([TKQPORT],FC0)");
                script = script.Replace("##TKQPORT##", mTKQPort.ToString());
                if ((this.cfgFunctionSwitch & 0x10) == 0x0)//判断中转传感是不是为槽式传感
                    script = script.Replace("##CGQ##", (x == 0) ? "0A" : "09");
                else
                    script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
                this.ClearDevCondition();
                r = udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_TRANSFER_CAR;
                }
            }

            //判断中转机构中是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))//判断电磁铁状态是否正确
                    return ERR_ELECTROMAGNET_STATE;
            }

            //Y轴移动到指定的卡槽位
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

                //如果有位置配置信息的话，就使用位置配置信息重新计算
                FKJ_PositionPropertyItem ppi = this.pcfgPositionProperty.FindNearPositionProperty(x, y);

                if (ppi == null)
                {
                    Ystep = cfgY_Slot0 + cfgY_Interval * y;
                    if (cfgY_AdaptTime != 0)
                    {
                        if (y < 0)
                            Ystep -= (y / cfgY_AdaptTime) * cfgY_AdaptPWM;
                        else
                            Ystep += (y / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    }
                }
                else
                {
                    //计算卡槽数
                    int realYindex = y - ppi.Y;
                    //计算步数（0号卡槽Y轴 + 0号卡槽到修改卡槽的间距 + 修改卡槽下面卡槽的数量*卡槽间距）
                    Ystep = cfgY_Slot0 + ppi.Step + realYindex * cfgY_Interval;
                    //计算的步数后添加偏移值
                    if (cfgY_AdaptTime != 0)
                    {
                        if (realYindex < 0)
                            Ystep -= (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                        else
                            Ystep += (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    }
                }

                r = this.MotoGoToPosition(10, Ystep, true, true);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | 10;
                }

                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)
                {
                    if (!"00".Equals(hexData.Replace(" ", "")))
                    {
                        if (i == 20)
                            return ERR_MOVE_MOTOR | 13;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                    else
                        break;
                }
            }

            //运行入卡到卡槽脚本
            script = udM.LoadScriptFromFile(TransferToCardSlotTxt);
            if ((this.cfgFunctionSwitch & 0x10) == 0)
                script = script.Replace("##CGQ##", (x == 0) ? "0A" : "09");
            else
                script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_CARD_SLOT1;
            }

            filePath = "02_03推卡入卡槽.txt";
            script = udM.LoadScriptFromFile(filePath);
            script = script.Replace("##PUSH_STEP##", Common.ConvertStringToHex(cfgPushCardStep).PadRight(8, '0'));
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_CARD_SLOT2;
            }

            if (SensorState(8).Equals("01"))
            {

                if ((this.cfgFunctionSwitch & 0x10) == 0x0)
                {
                    if (SensorState(9).Equals(LaserSensorSense) || SensorState(10).Equals(LaserSensorSense))
                    {
                        //运卡到卡槽
                        script = udM.LoadScriptFromFile(TransferToCardSlotTxt);
                        script = script.Replace("##CGQ##", (x == 0) ? "0A" : "09");
                        r = udM.RunScript(this.mDevIndex, script);
                        if (r != 0)
                        {
                            this.ClearDevCondition();
                            return ERR_PUT_CARD_TO_CARD_SLOT1;
                        }
                    }
                }
                else
                {
                    if (SensorState(11).Equals(LaserSensorSense) || SensorState(9).Equals(LaserSensorSense))
                    {
                        //运卡到卡槽
                        script = udM.LoadScriptFromFile(TransferToCardSlotTxt);
                        script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
                        r = udM.RunScript(this.mDevIndex, script);
                        if (r != 0)
                        {
                            this.ClearDevCondition();
                            return ERR_PUT_CARD_TO_CARD_SLOT1;
                        }
                    }
                }
                //推卡入卡槽
                filePath = "02_03推卡入卡槽.txt";
                script = udM.LoadScriptFromFile(filePath);
                script = script.Replace("##PUSH_STEP##", Common.ConvertStringToHex(cfgPushCardStep).PadRight(8, '0'));
                r = udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_CARD_SLOT2;
                }
            }
            if (SensorState(8).Equals("01"))
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                return ERR_CARD_WORRY_PLACE;
            }
            return 0;
        }

        /// <summary>
        /// 发卡机读卡位置入卡到废卡槽
        /// </summary>
        /// <param name="param">用于扩展，暂为0</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r;
            string script;
            //判读入卡读卡器位置是否有卡
            if (!this.CardIsInIcReader())
                return ERR_PUT_IC_READER_EMPTY;
            if (this.cfgTKDeviceType == 1)
            {
                script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",CP)";
                r = this.udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                    return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
            }
            else if (this.cfgTKDeviceType == 2)
            {
                script = "CARD1=K720(" + this.mTKQPort.ToString() + ",CP)";
                r = this.udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                    return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
            }
            else if (this.cfgTKDeviceType == 3)
            {
                script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",CP)";
                r = this.udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                    return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    if (CardIsReady())
            //        break;
            //    System.Threading.Thread.Sleep(200);
            //}
            return 0;
        }

        /// <summary>
        /// 卡槽取卡到中转机构
        /// </summary>
        /// <param name="x">卡槽列数</param>
        /// <param name="y">卡槽行数</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r;
            string hexData, script, filePath;
            int Ystep, Xstep;
            int SensorChangeNum, JDQSensorChangeNum;//SensorChangeNum用于获取激光对射传感的变化次数，JDQSensorChangeNum用于获取电磁铁传感器变化次数

            //判断输入参数是否正确
            if (x >= this.cfgX_SlotCount || y >= this.cfgY_SlotCount)
                return ERR_WRONG_PARAMS;
            //判断中转机构是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //对射传感是否有感应
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (this.ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
            }

            //移动Y轴到指定卡槽位置
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

                //如果有位置配置信息的话，就使用位置配置信息重新计算
                FKJ_PositionPropertyItem ppi = this.pcfgPositionProperty.FindNearPositionProperty(x, y);

                if (ppi == null)
                {
                    Ystep = cfgY_Slot0 + cfgY_Interval * y;
                    if (cfgY_AdaptTime != 0)
                    {
                        if (y < 0)
                            Ystep -= (y / cfgY_AdaptTime) * cfgY_AdaptPWM + cfgY_GetCard_Excursion;
                        else
                            Ystep += (y / cfgY_AdaptTime) * cfgY_AdaptPWM + cfgY_GetCard_Excursion;
                    }
                }
                else
                {
                    //计算卡槽数
                    int realYindex = y - ppi.Y;
                    //计算步数（0号卡槽Y轴 + 0号卡槽到修改卡槽的间距 + 修改卡槽下面卡槽的数量*卡槽间距）
                    Ystep = cfgY_Slot0 + ppi.Step + realYindex * cfgY_Interval;
                    //计算的步数后添加偏移值
                    if (cfgY_AdaptTime != 0)
                    {
                        if (realYindex < 0)
                            Ystep -= (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM + cfgY_GetCard_Excursion;
                        else
                            Ystep += (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM + cfgY_GetCard_Excursion;
                    }
                }

                r = this.MotoGoToPosition(10, Ystep, true, true);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | 10;
                }

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

            SensorChangeNum = this.GetSensorChangeCount(8);//获取对射传感跳变数
            JDQSensorChangeNum = this.GetSensorChangeCount(15);//获取电磁铁传感跳变次数

            //运行取卡脚本
            script = udM.LoadScriptFromFile(CardSlotToTransferTxt);
            script = script.Replace("##JDQ##", GetCardJDQ(x));
            script = script.Replace("##CGQ##", PutCardSensorInfor(x));
            r = this.udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                //如果为空卡槽（1.中转机构为空；2.激光对射传感没有变化；3，电磁铁传感器有变化）
                if ((SensorChangeNum == this.GetSensorChangeCount(8)) && !this.CardIsInTransfer() && JDQSensorChangeNum != this.GetSensorChangeCount(15))
                {
                    this.ClearDevCondition();
                    return ERR_CARD_SLOT_EMPTY;
                }

                //脚本运行失败，并且电磁铁传感没有变化，有可能电磁铁不够力，再尝试第一次
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);//停止电机运行
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(500);
                r = this.udM.RunScript(this.mDevIndex, script);
                if (r != 0)//有可能继电器不够力，再尝试第二次
                {
                    this.SendCommand("0206", "0000", out hexOut);
                    this.ClearDevCondition();
                    System.Threading.Thread.Sleep(500);
                    r = this.udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.SendCommand("0206", "0000", out hexOut);
                        this.ClearDevCondition();
                        if (JDQSensorChangeNum == this.GetSensorChangeCount(15))//电磁铁传感没有变化，电磁铁有问题
                            return ERR_ELECTROMAGNET_CHANGE;
                        return ERR_GET_CARD_TO_TRANSFER_CAR;
                    }
                }
            }

            //移动X轴到出卡口位置
            Xstep = cfgX_OutCard + x * cfgX_Interval;
            r = this.MotoGoToPosition(5, Xstep, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            return r;
        }

        /// <summary>
        /// 中转机构取卡到出卡器读卡位置
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToICReader()
        {
            uint r;
            int x = 0;
            string hexData, filePath, script;

            //判断中转区是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;
            //判断出卡读卡器位置是否有卡
            if (this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_BLOCK;
            //对射传感是否有感应
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (this.ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
            }

            //移动Y轴到出卡器位置
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

                //移动Y轴到出卡口
                r = this.MotoGoToPosition(10, cfgY_OutCard, true, true);
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

            filePath = "03_02中转区到出卡器.txt";
            script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                if ((this.cfgFunctionSwitch & 0x10) == 0)
                {
                    if (SensorState(9).Equals(LaserSensorSense))//判断卡片在中转机构的位置（左卡槽还是右卡槽）
                        x = 1;
                    else if (SensorState(10).Equals(LaserSensorSense))
                        x = 0;
                    else
                        return ERR_GET_CARD_TO_ICREADER;
                }
                else
                {
                    if (SensorState(10).Equals(LaserSensorSense))//判断卡片在中转机构的位置（左卡槽还是右卡槽）
                        x = 1;
                    else if (SensorState(12).Equals(LaserSensorSense))
                        x = 0;
                    else
                        return ERR_GET_CARD_TO_ICREADER;
                }
                script = udM.LoadScriptFromFile(TranferTakeCardBack);
                if ((this.cfgFunctionSwitch & 0x10) == 0)
                    script = script.Replace("##CGQ##", (x == 0) ? "0A" : "09");
                else
                    script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
                this.ClearDevCondition();
                r = udM.RunScript(this.mDevIndex, script);
                return ERR_GET_CARD_TO_ICREADER;
            }
            return 0;
        }

        /// <summary>
        /// 出卡器取卡到用户
        /// </summary>
        /// <param name="isHoldCard">是否将卡夹住</param>
        /// <param name="step">默认为0，用于扩展</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r;
            string filePath, script;
            //判读出卡读卡器位置是否有卡
            if (!this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_EMPTY;

            //X轴到用户出卡口
            r = this.MotoGoToPosition(5, cfgX_OutCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            filePath = "03_03出卡机出卡.txt";
            //因为惯性不能把卡带出传感，所以这个脚本没有意义
            if (!isHoldCard)
                filePath = "03_03出卡机出卡_2.txt";
            script = udM.LoadScriptFromFile(filePath);
            r = udM.RunScript(this.mDevIndex, script);
            System.Threading.Thread.Sleep(300);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_GET_CARD_TO_USER;
            }
            return r;
        }

        /// <summary>
        /// 出卡器取卡到废卡槽
        /// </summary>
        /// <param name="param">用于扩展，默认为0</param>
        /// <returns>0为成功，其余为失败</returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            //如果卡在中转机构，直接移动到废卡位置
            if (this.CardIsInTransfer())
            {
                //移动Y轴到靠近底部位置
                uint r;
                r = this.MotoGoToPosition(10, this.cfgY_BadSlot, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
                string filePath = "03_06中转区废卡.txt";
                string script = udM.LoadScriptFromFile(filePath);
                this.ClearDevCondition();
                r = udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_GET_CARD_TO_BAD_CARD_SLOT;
                }
                return r;

            }
            else if (this.CardIsInIcReaderOutput())
            {
                string filePath = "03_04出卡机废卡.txt";
                string script = udM.LoadScriptFromFile(filePath);
                this.ClearDevCondition();
                uint r = udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_GET_CARD_TO_BAD_CARD_SLOT;
                }
                return r;
            }
            return ERR_GET_CARD_TO_BAD_CARD_SLOT;
        }

        /// <summary>
        /// 将出卡器中的卡放回中转区,正常流程不会调用，用于测试
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCarFromOutput()
        {
            string hexData, filePath, script;
            uint r;
            int Xstep;

            //判断中转区是否有卡
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //判断出卡读卡器位置是否有卡
            if (!this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_EMPTY;
            //对射传感是否有感应
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (this.ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
            if ((this.cfgFunctionSwitch & 0x08) != 0x08)//判断是否启用电磁铁传感
            {
                if (this.SensorState(15).Equals("00"))
                    return ERR_ELECTROMAGNET_STATE;
            }
            //判断输入参数是否正确
            if (X == -1)
                return ERR_WRONG_PARAMS;

            //移动Y轴到出卡器位置
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

                //移动Y轴到出卡口
                r = this.MotoGoToPosition(10, cfgY_OutCard, true, true);
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

            //移动X轴到出卡口位置
            Xstep = cfgX_OutCard + X * cfgX_Interval;
            r = this.MotoGoToPosition(5, Xstep, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            //运行卡从出卡器返回到中转脚本
            script = udM.LoadScriptFromFile(OuterToTransferTxt);
            if ((this.cfgFunctionSwitch & 0x10) == 0x0)//判断中转传感是不是为槽式传感
                script = script.Replace("##CGQ##", (X == 0) ? "0A" : "09");
            else
                script = script.Replace("##CGQ##", (X == 0) ? "0B" : "09");
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 1;
            }

            return 0;
        }

        /// <summary>
        /// 将电机松开(除伺服电机外)
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public override uint MotorUnlock()
        {
            string filePath = "05_01松开电机.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            uint r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 将所用电机都停止运行
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public uint StopForError()
        {
            string filePath = "04_出错停止.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            uint r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

        /// <summary>
        /// 将电磁铁恢复到原始状态
        /// </summary>
        /// <returns>0为成功，其余为失败</returns>
        public uint ElectromagnetTakeBack()
        {
            string filePath = "05_02继电器收回.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            uint r = udM.RunScript(this.mDevIndex, script);
            return r;
        }


        System.Threading.Mutex mutexGlobals = new System.Threading.Mutex();
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

        protected override string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = script.Replace("##CGQ##", this.mTKQPort.ToString());
            str = str.Replace("##INREADERPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##JDQ##", this.GetCardJDQ(x));
            str = str.Replace("##OUTREADERPORT##", this.mOutCardReaderPort.ToString());
            return str;
        }
    }
}