using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJ500_V3
//说明：这个类适用于带以下功能的500张发证机：
//              1.将入卡读卡器更换成IC读卡器的500张机型可以使用
//              2.此版本为最最最无聊和最最最烦人的版本，多此一举的加了一个读卡器，无理的要求。靠靠靠！！！！
//最后修改时间：2016/8/25
//==========================================================

namespace MotorControlBoard
{
    public class FKJ500_V3 : IFZJ
    {
        /// <summary>
        /// 父类(IFZJ)中的udm没有实例化的时候通过此构造函数实例化,并同时为父类(IFZJ)中mScriptPath，udM和mDevIndex赋值
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备索引（大于100用USB口，小于100用串口）</param>
        /// <param name="scriptPath">脚本文件路径</param>
        public FKJ500_V3(int PortNum, byte devIndex, String scriptPath)
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
        public FKJ500_V3(UsbDeviceManager dev, byte devIndex, string scriptPath)
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
        /// 用于获取传感器号
        /// </summary>
        /// <param name="x">卡槽列</param>
        /// <returns></returns>
        public override string PutCardSensorInfor(int x)
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

        /// <summary>
        /// 用于获取继电器号
        /// </summary>
        /// <param name="x">卡槽列</param>
        /// <returns></returns>
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
        /// 检测中转区中是否有卡，有卡则将卡放到中转区的正中
        /// </summary>
        /// <returns>"03"为出错或者失败，"02"为无卡，"01"为有1卡槽有卡，"00"为0卡槽有卡</returns>
        public string TransferCheck()
        {
            uint r = 2;

            this.ClearDevCondition();
            string filePath = "01_01中转区卡位校正.txt";
            string script = udM.LoadScriptFromFile(filePath);
            r = this.udM.RunScript(this.mDevIndex, script);
            if (r != 0)//出错返回“03”
                return "03";

            if (SensorState(9).Equals("00") && SensorState(10).Equals("00"))
            {
                return "01";
            }
            else if (SensorState(11).Equals("00") && SensorState(12).Equals("00"))
            {
                return "00";
            }
            else if (!CardIsInTransfer())
            {
                return "02";
            }
            return "03";
        }

        /// <summary>
        /// 判断中转区是否有卡
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInTransfer()
        {
            if (SensorState(9).Equals("00") || SensorState(10).Equals("00") || SensorState(11).Equals("00") || SensorState(12).Equals("00"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断出卡器和读卡器位置是否有卡
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInIcReaderOutput()
        {
            if (SensorState(5).Equals("01"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测出卡口位置是否有卡
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInOutput()
        {
            if (SensorState(6).Equals("01"))
                return true;
            return false;
        }
        /// <summary>
        /// 判断发卡器中是否有卡
        /// </summary>
        /// <returns></returns>
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
            return false;
        }

        /// <summary>
        /// 判断发卡器中的IC读卡器是否有卡
        /// </summary>
        /// <returns></returns>
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
        /// Y轴和X轴移动到指定卡槽
        /// </summary>
        /// <param name="isGetCard">是放卡还是出卡</param>
        /// <param name="xIndex">X轴卡槽号</param>
        /// <param name="yIndex">Y轴卡槽号</param>
        /// <param name="waitCompleted">是否等待完成后再执行下一条指令</param>
        /// <returns></returns>
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
            string hexData;

            //初始化前先检测中转区中是否有卡,有则调整卡在中转区中的位置
            string cardInTheTransfer = TransferCheck();
            if (cardInTheTransfer.Equals("03"))
                return ERR_INIT_CARD_SLOT_ADJUST;

            if (SensorState(8).Equals("01"))//卡槽和中转区中有卡则停止初始化
            {
                return ERR_CARD_WORRY_PLACE;
            }

            this.SendCommand("0217", "", out hexData);

            //运行“01_整机初始化”脚本
            string filePath = "01_整机初始化.txt";
            string script = udM.LoadScriptFromFile(filePath);
            script = script.Replace("##ICPORT##", this.mInCardReaderPort.ToString());
            this.ClearDevCondition();
            uint r = udM.RunScript(0, script);
            if (r != 0)
                return ERR_INIT;

            //初始化时如果急停会报错
            if (this.SendCommand("0216", "", out hexData) == 0)
            {
                if (!"00".Equals(hexData.Replace(" ", "")))
                    return ERR_MOVE_MOTOR | 10;
            }

            if (cardInTheTransfer.Equals("00"))//如果中转区中有卡，则提示有卡并停止运行
            {
                return 1;
            }
            else if (cardInTheTransfer.Equals("01"))
            {
                return 2;
            }
            return 0;
        }

        /// <summary>
        /// 中转区移动到出卡位置
        /// </summary>
        /// <param name="x">卡槽列</param>
        /// <returns></returns>
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
        /// 将卡发送到发卡器上读卡器
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToICReader()
        {
            uint r;
            string hexData, script;
            //判断IC读卡器中是否有卡，有卡则直接成功
            if (this.CardIsInIcReader())
                return 0;
            //判断中转机构是否有卡，有卡则报错
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            //判断发卡器是否有卡，无卡则报错
            if (!this.CardIsReady())
                return ERR_PUT_CONTAINER_EMPTY;
            //判断中转机构和卡槽间是否有东西
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;

            if (cfgTKDeviceType == 1)
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

                StringBuilder script1 = new StringBuilder();
                script1.Append("CARD1=TCD820M(" + this.mTKQPort.ToString() + ",DC)\r\n");
                script1.Append("1=DELAY(1000)\r\n");
                if (0 == this.udM.RunScript(this.mDevIndex, script1.ToString()))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    if (!"06".Equals(strState))//没有卡 
                        return ERR_EXECUTE_SCRIPT | 1;
                }
            }
            else if (cfgTKDeviceType == 2)
            {
                //卡传送到读卡器位置
                script = "CARD1=K720(" + this.mTKQPort.ToString() + ",FC7)";
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

            //移动X,Y轴到入卡读卡器位置（入卡到0号卡槽列）
            r = this.MotoGoToPosition(5, cfgX_InCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

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

            //入卡到中转机构
            string filePath = "02_01发卡机到中转区.txt";
            script = udM.LoadScriptFromFile(filePath);
            if (cfgTKDeviceType == 1)
                script = script.Replace("##TK_CMD##", "TCD820M([TKQPORT],DC)");
            else
                script = script.Replace("##TK_CMD##", "K720([TKQPORT],FC0)");
            script = script.Replace("##TKQPORT##", mTKQPort.ToString());
            script = script.Replace("##CGQ##", "0B");
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            }

            //判断中转机构是否有卡，无卡则报错
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            //移动Y轴到IC读卡器位置
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
                //Y轴移动到电动读卡器坐标
                r = this.MotoGoToPosition(10, cfgICReader_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)

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

            filePath = "02_05中转机构到读卡器.txt";
            script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
                return ERR_PUT_CARD_TO_ICREADER;
         
            return 0;
        }

        /// <summary>
        /// 卡从IC读卡器送到中转区（500张不用此函数）
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCar()
        {
            return 0;
        }

        /// <summary>
        /// 中转区放卡到卡槽
        /// </summary>
        /// <param name="x">X轴卡槽号</param>
        /// <param name="y">Y轴卡槽号</param>
        /// <returns></returns>
        public override uint PutCardToCardSlot(int x, int y)
        {
            string hexData;
            uint r;
            string filePath, script;

            if (x >= cfgX_SlotCount || y >= cfgY_SlotCount)
                return ERR_WRONG_PARAMS;

            //如果中转区有卡则报错
            if (CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            if (!CardIsInIcReader())
                return ERR_PUT_IC_READER_EMPTY;
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;

            //X轴移动
            int step = cfgX_InCard + x * cfgX_Interval;
            r = this.MotoGoToPosition(5, step, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

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

                r = this.MotoGoToPosition(10, cfgICReader_InCard, true, true);
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

            //入卡到中转机构
            filePath = "02_06读卡器到中转机构.txt";
            script = udM.LoadScriptFromFile(filePath);
            script = script.Replace("##ICPORT##", this.mInCardReaderPort.ToString());
            script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            }

            //判断中转机构是否有卡，无则报错
            if (!CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            //Y轴移动到卡槽位置
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
                //Y轴移动
                if (cfgY_AdaptTime == 0)
                {
                    this.MotoGoToPosition(10, cfgY_Slot0 + y * cfgY_Interval, true, true);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;
                }
                else
                {
                    this.MotoGoToPosition(10, cfgY_Slot0 + y * cfgY_Interval + (y / cfgY_AdaptTime) * cfgY_AdaptPWM, true, true);
                    if (r != 0)
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

            //运卡到卡槽
            filePath = "02_02中转区到卡槽.txt";
            script = udM.LoadScriptFromFile(filePath);
            script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_CARD_SLOT1;
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

            if (SensorState(8).Equals("01"))
            {
                if (SensorState(11).Equals("00") || SensorState(9).Equals("00"))
                {
                    //运卡到卡槽
                    filePath = "02_02中转区到卡槽.txt";
                    script = udM.LoadScriptFromFile(filePath);
                    script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
                    r = udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_PUT_CARD_TO_CARD_SLOT1;
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
        /// 读卡器将发卡到废卡槽
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r;
            r = GetCardToBadCardSlot(param);
            return r;
        }

        /// <summary>
        /// 从卡槽取卡到中转区
        /// </summary>
        /// <param name="x">X轴卡槽号</param>
        /// <param name="y">Y轴卡槽号</param>
        /// <returns></returns>
        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r;
            string hexData;

            if (x >= cfgX_SlotCount || y >= cfgY_SlotCount)
                return ERR_WRONG_PARAMS;
            if (this.CardIsInTransfer())//如果中转区有卡则报错
                return ERR_TRANSFER_CAR_BLOCK;
            if (this.SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (this.ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;

            //Y轴移动到指定的卡槽位置
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
                //Y轴移动
                if (cfgY_AdaptTime == 0)
                {
                    r = this.MotoGoToPosition(10, cfgY_Slot0 + y * cfgY_Interval, true, true);
                    if (r != 0)
                        return ERR_MOVE_MOTOR | 10;
                }
                else
                {
                    r = this.MotoGoToPosition(10, cfgY_Slot0 + y * cfgY_Interval + (y / cfgY_AdaptTime) * cfgY_AdaptPWM, true, true);
                    if (r != 0)
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

            //装备放卡
            string script = udM.LoadScriptFromFile("03_01卡槽到中转区.txt");
            script = script.Replace("##JDQ##", GetCardJDQ(x));
            script = script.Replace("##CGQ##", PutCardSensorInfor(x));
            this.ClearDevCondition();
            r = this.udM.RunScript(this.mDevIndex, script);
            if (r != 0)//有可能继电器不够力，再尝试第一次
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(500);
                r = this.udM.RunScript(this.mDevIndex, script);
            }
            if (r != 0)//有可能继电器不够力，再尝试第二次
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                System.Threading.Thread.Sleep(500);
                r = this.udM.RunScript(this.mDevIndex, script);
            }
            if (r != 0)
            {
                string hexOut;
                this.SendCommand("0206", "0000", out hexOut);
                this.ClearDevCondition();
                return ERR_GET_CARD_TO_TRANSFER_CAR;
            }

            //移动到读卡器位置
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

                //移动Y轴到读卡器位置
                r = this.MotoGoToPosition(10, cfgICReader_InCard, true, true);
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

            //移动X轴
            int step = cfgX_InCard + x * cfgX_Interval;
            r = this.MotoGoToPosition(5, step, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            return r;
        }

        /// <summary>
        /// 中转区出卡到读卡器
        /// </summary>
        /// <returns></returns>
        public override uint GetCardToICReader()
        {
            uint r;
            string hexData;
            int step;

            //判断中转区是否有卡
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            //通过判断传感来移动X轴到合适的位置
            if (this.SensorState(12).Equals("00"))
            {
                step = cfgX_InCard;
            }
            else if (this.SensorState(10).Equals("00"))
            {
                step = cfgX_InCard + cfgX_Interval;
            }
            else
            {
                return ERR_TRANSFER_CAR_EMPTY;
            }
            r = this.MotoGoToPosition(5, step, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            //移动到读卡器位置
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

                //移动Y轴到读卡器位置
                r = this.MotoGoToPosition(10, cfgICReader_InCard, true, true);
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

            string filePath = "02_05中转机构到读卡器.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                return ERR_GET_CARD_TO_ICREADER;
            }
            return 0;
        }

        /// <summary>
        /// 将卡从IC读卡器运送到出卡器
        /// </summary>
        /// <returns></returns>
        private uint TakeCardFormICReaderToOuter()
        {
            uint r;
            string hexData, filePath, script;
            //判断中转机构是否有卡，有则报错
            if (this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_BLOCK;
            if (SensorState(8).Equals("01"))
                return ERR_CARD_WORRY_PLACE;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;

            //移动Y轴到IC读卡器位置
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
                //Y轴移动到电动读卡器坐标
                r = this.MotoGoToPosition(10, cfgICReader_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;
                //判断有没Brake
                if (this.SendCommand("0216", "", out hexData) == 0)

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
            //移动X轴到0号卡槽位置
            r = this.MotoGoToPosition(5, cfgX_InCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;
            //入卡到中转机构
            filePath = "02_06读卡器到中转机构.txt";
            script = udM.LoadScriptFromFile(filePath);
            script = script.Replace("##ICPORT##", this.mInCardReaderPort.ToString());
            script = script.Replace("##CGQ##", "0B");
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_TRANSFER_CAR;
            }

            //判断中转机构是否有卡，无则报错
            if (!this.CardIsInTransfer())
                return ERR_TRANSFER_CAR_EMPTY;

            //移动Y轴到出卡口位置
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

                r = this.MotoGoToPosition(10, cfgY_OutCard, true, true);
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

            //移动X轴到出卡口
            r = this.MotoGoToPosition(5, cfgX_OutCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            //出卡到出卡器
            filePath = "03_02中转区到出卡器.txt";
            script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
            {
                string hexOut;
                this.ClearDevCondition();
                this.SendCommand("0206", "0000", out hexOut);
                return ERR_GET_CARD_TO_ICREADER;
            }
            return 0;
        }

        /// <summary>
        /// 出卡器出卡到用户
        /// </summary>
        /// <param name="isHoldCard">是否将卡夹住</param>
        /// <param name="step"></param>
        /// <returns></returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r;
            //IC读卡器到出卡器
            r = TakeCardFormICReaderToOuter();
            if (r != 0)
                return ERR_GET_CARD_TO_OUTER;
            
            //判断出卡读卡器是否有卡，无则报错
            if (!this.CardIsInIcReaderOutput())
                return ERR_GET_IC_READER_EMPTY;

            //X轴到用户出卡口
            r = this.MotoGoToPosition(5, cfgX_OutCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            string filePath = "03_03出卡机出卡.txt";
            //因为惯性不能把卡带出传感，所以这个脚本没有意义
            if (!isHoldCard)
                filePath = "03_03出卡机出卡_2.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            r = udM.RunScript(this.mDevIndex, script);
            System.Threading.Thread.Sleep(300);
            if (r != 0)
            {
                StopForError();
                this.ClearDevCondition();
                return ERR_GET_CARD_TO_USER;
            }
            return r;
        }

        /// <summary>
        /// 出卡器出卡到废卡槽
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;
            if (!this.CardIsInOutput())
            {
                //IC读卡器到出卡器
                r = TakeCardFormICReaderToOuter();
                if (r != 0)
                    return ERR_GET_CARD_TO_OUTER;
            }
               
            string filePath = "03_04出卡机废卡.txt";
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

        //将出卡器中的卡放回中转区
        public override uint PutCardToTransferCarFromOutput()
        {
            //不能实现，无X轴坐标，不知道是那个卡槽
            return 0;
        }

        /// <summary>
        /// 将电机都松开(除伺服电机外)
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

        //出错停止
        public uint StopForError()
        {
            this.ClearDevCondition();

            //停止所有电机运行
            string filePath = "04_出错停止.txt";
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

    }
}