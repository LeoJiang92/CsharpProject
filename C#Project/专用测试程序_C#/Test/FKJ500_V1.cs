using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJ500_V1
//说明：这个类适用于带以下功能的500张发证机：
//              1.无对射传感器检测；
//              2.出卡机的读卡器在外，不能实现将卡返回中转区；    
//最后修改时间：2016/6/16
//==========================================================

namespace MotorControlBoard
{
    public class FKJ500_V1 : IFZJ
    {
        public FKJ500_V1(int PortNum, byte devIndex, String scriptPath)
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

        public FKJ500_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
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

        //初始化设备的所有初始位置
        public override uint DevInit()
        {
            //初始化前先检测中转区中是否有卡
            int cardInTheTransfer = TransferCheck();

            //运行“01_整机初始化”脚本
            string filePath = "01_整机初始化.txt";
            string script = udM.LoadScriptFromFile(filePath);
            this.ClearDevCondition();
            uint r = udM.RunScript(0, script);

            if (cardInTheTransfer != -1)     //如果中转区中有卡，则提示有卡并停止运行
            {
                return ERR_TRANSFER_CAR_BLOCK;
            }
            return r;
        }

        //检测传感器状态并返回(返回"01"为高电平，"00"为低电平)
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

        //检测中转区中是否有卡，并将卡放到中转区的正中
        //返回-1为无卡，1为有1卡槽有卡，0为0卡槽有卡
        public int TransferCheck()
        {
            uint r = 2;

            //电机直接向后转动200步
            StringBuilder run = new StringBuilder();
            //##---          【向后】 转动电机【0】 200步，停止时【松开】
            run.Append("A=S(0301 00 00 00 11 00 00 01 64000000)\r\n");
            //##---          延时【200】毫秒
            run.Append("1=DELAY(200)\r\n");
            r = udM.RunScript(0, run.ToString());

            string filePath = "01_01中转区卡位校正.txt";
            string script = udM.LoadScriptFromFile(filePath);

            if (SensorState(10).Equals("00"))
            {
                if (SensorState(9).Equals("00"))
                {
                    return 1;
                }
                else if (SensorState(9).Equals("01"))
                {
                    script = script.Replace("##DIRECTION##", "00");//向前转动
                    script = script.Replace("##CGQ##", "09");
                    r = this.udM.RunScript(this.mDevIndex, script);
                    return 1;
                }
            }
            else if (SensorState(12).Equals("00"))
            {
                if (SensorState(11).Equals("00"))//两个传感器都有感应，则说明卡大概在中转区中间
                {
                    return 0;
                }
                else if (SensorState(11).Equals("01"))//只有一个传感器有感应，则需要移动到中转区中间
                {
                    script = script.Replace("##DIRECTION##", "00");//向前转动
                    script = script.Replace("##CGQ##", "0B");
                    r = this.udM.RunScript(this.mDevIndex, script);
                    return 0;
                }
            }
            else if (SensorState(9).Equals("00"))
            {
                if (SensorState(10).Equals("00"))
                {
                    return 1;
                }
                else if (SensorState(10).Equals("01"))
                {
                    script = script.Replace("##DIRECTION##", "01");//向后转动
                    script = script.Replace("##CGQ##", "0A");
                    r = this.udM.RunScript(this.mDevIndex, script);
                    return 1;
                }
            }
            else if (SensorState(11).Equals("00"))
            {
                if (SensorState(12).Equals("00"))
                {
                    return 0;
                }
                else if (SensorState(12).Equals("01"))
                {
                    script = script.Replace("##DIRECTION##", "01");//向后转动
                    script = script.Replace("##CGQ##", "0C");
                    r = this.udM.RunScript(this.mDevIndex, script);
                    return 0;
                }
            }
            return -1;
        }

        //判断中转区是否有卡
        public override bool CardIsInTransfer()
        {
            if (SensorState(9).Equals("00") || SensorState(10).Equals("00") || SensorState(11).Equals("00") || SensorState(12).Equals("00"))
            {
                return true;
            }
            return false;
        }

        //判断出卡器读卡器是否有卡
        public override bool CardIsInIcReaderOutput()
        {
            if (SensorState(5).Equals("01") || SensorState(6).Equals("01"))
            {
                return true;
            }
            return false;
        }

        //判断发卡器卡的位置是否正确
        public override bool CardIsReady()
        {
            if (cfgTKDeviceType == 1)
            {
                string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",RF)";
                for (int i = 0; i < 4; i++)
                {
                    if (0 == this.udM.RunScript(this.mDevIndex, script))
                    {
                        string strState = udM.GetParam(this.mDevIndex, "CARD1");
                        string cardState = strState.Substring(6, 6);
                        if ("303030".Equals(cardState))//没有卡
                            return true;
                        if ("303130".Equals(cardState))//没有卡
                            return true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
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

        //是否还有卡没有发
        public override bool CardIsInIcReader()
        {
            if (cfgTKDeviceType == 1)
            {
                string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",RF)";
                if (0 == this.udM.RunScript(this.mDevIndex, script))
                {
                    string strState = udM.GetParam(this.mDevIndex, "CARD1");
                    string cardState = strState.Substring(6, 6);
                    if ("303032".Equals(cardState))//没有卡
                        return true;
                }
                return false;
            }
            else if (cfgTKDeviceType == 3)
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
        /// 检测出卡口位置是否有卡
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInOutput()
        {
            if (SensorState(6).Equals("01"))
                return true;
            return false;
        }

        public override uint PutCardToTransferCarFromOutput()
        {
            return 0;
        }

        //将卡放入到IC卡读卡器
        public override uint PutCardToICReader()
        {
            uint r;
            if (this.CardIsInIcReader())
            {
                return 0;
            }
            if (this.CardIsReady())
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
            uint r = this.MotoGoToPosition(5, step, true, true);
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

        //放卡
        public override uint PutCardToCardSlot(int x, int y)
        {
            uint r;
            int step;
            string filePath, script;

            if (x >= cfgX_SlotCount || y >= cfgY_SlotCount)
                return ERR_WRONG_PARAMS;
            //if (CardIsInTransfer())//如果中转区有卡则报错
            //    return ERR_TRANSFER_CAR_BLOCK;
            //if (!CardIsInIcReader())
            //    return ERR_PUT_IC_READER_EMPTY;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;

            if (!this.CardIsInTransfer())
            {
                if (!this.CardIsInIcReader())
                    return ERR_PUT_IC_READER_EMPTY;

                //X轴移动到卡槽位置
                step = cfgX_InCard + x * cfgX_Interval;
                r = this.MotoGoToPosition(5, step, true, false);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 5;

                //Y轴移动到入卡位置
                r = this.MotoGoToPosition(10, cfgY_InCard, true, true);
                if (r != 0)
                    return ERR_MOVE_MOTOR | 10;

                //入卡到中转机构
                filePath = "02_01发卡机到中转区.txt";
                script = udM.LoadScriptFromFile(filePath);
                if (cfgTKDeviceType == 1)
                    script = script.Replace("##TK_CMD##", "TCD820M([TKQPORT],DC)");
                else if (cfgTKDeviceType == 3)
                    script = script.Replace("##TK_CMD##", "D1801([TKQPORT],FC0)"); 
                script = script.Replace("##TKQPORT##", mTKQPort.ToString());
                script = script.Replace("##CGQ##", (x == 0) ? "0B" : "09");
                r = udM.RunScript(this.mDevIndex, script);
                if (r != 0)
                    return ERR_PUT_CARD_TO_TRANSFER_CAR;

            }

            //Y轴移动到卡槽位置
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
            return 0;
        }

        //发卡器将发卡到废卡槽
        public override uint PutCardToBadCardSlot(uint param)
        {
            if (CardIsInIcReader())
            {
                if (cfgTKDeviceType == 1)
                {
                    string script = "CARD1=TCD820M(" + this.mTKQPort.ToString() + ",CP)";
                    uint r = this.udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                        return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
                }
                else if (cfgTKDeviceType == 3)
                {
                    string script = "CARD1=D1801(" + this.mTKQPort.ToString() + ",CP)";
                    uint r = this.udM.RunScript(this.mDevIndex, script);
                    if (r != 0)
                        return ERR_PUT_CARD_TO_BAD_CARD_SLOT;
                }
            }
            System.Threading.Thread.Sleep(800);
            return 0;
        }

        //取卡
        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r;
            this.ClearDevCondition();
            if (CardIsInTransfer())//如果中转区有卡则报错
                return ERR_TRANSFER_CAR_BLOCK;
            //电磁铁收回
            if (ElectromagnetTakeBack() != 0)
                return ERR_ELECTROMAGNET_TAKEBACK;
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

            //装备放卡
            string script = udM.LoadScriptFromFile("03_01卡槽到中转区.txt");
            script = script.Replace("##JDQ##", GetCardJDQ(x));
            script = script.Replace("##CGQ##", PutCardSensorInfor(x));
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

            //移动X轴到出卡口
            int step = cfgX_OutCard + x * cfgX_Interval;
            r = this.MotoGoToPosition(5, step, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            return r;
        }

        //出卡
        public override uint GetCardToICReader()
        {
            uint r;

            //移动Y轴到出卡口
            r = this.MotoGoToPosition(10, cfgY_OutCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 10;

            string filePath = "03_02中转区到出卡器.txt";
            string script = udM.LoadScriptFromFile(filePath);
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

        public override uint PutCardToTransferCar()
        {
            return 0;
        }

        //出卡到用户
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            this.ClearDevCondition();

            //X轴到用户出卡口
            uint r = this.MotoGoToPosition(5, cfgX_OutCard, true, true);
            if (r != 0)
                return ERR_MOVE_MOTOR | 5;

            string filePath = "03_03出卡机出卡.txt";
            string script = udM.LoadScriptFromFile(filePath);
            r = udM.RunScript(this.mDevIndex, script);
            System.Threading.Thread.Sleep(800);
            if (r != 0)
            {
                StopForError();
                return ERR_GET_CARD_TO_USER;
            }
            return r;
        }

        //出卡到废卡槽
        public override uint GetCardToBadCardSlot(uint param)
        {
            this.ClearDevCondition();
            string filePath = "03_04出卡机废卡.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = udM.RunScript(this.mDevIndex, script);
            if (r != 0)
                return ERR_GET_CARD_TO_BAD_CARD_SLOT;
            return r;
        }

        //中转区到出卡器
        public uint TransferCardToOuter(int x)
        {
            uint r = 0;
            r = MotoGoToPosition(5, cfgX_Interval * x + cfgX_OutCard, true, true);
            if (r != 0)
            {
                return ERR_MOVE_MOTOR | 5;
            }
            r = MotoGoToPosition(10, cfgY_OutCard, true, true);
            if (r != 0)
            {
                return ERR_MOVE_MOTOR | 10;
            }
            return r;
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

        //出错停止
        public uint StopForError()
        {
            //停止所有电机运行
            string filePath = "04_出错停止.txt";
            string script = udM.LoadScriptFromFile(filePath);
            uint r = udM.RunScript(this.mDevIndex, script);
            return r;
        }

    }
}