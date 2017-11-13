using System;
using System.Collections.Generic;
using System.Text;

namespace MotorControlBoard
{

    class FKJ4000_V1 : IFZJ
    {
        string FKQLeftResetScript = "0_1_左发卡机初始化.txt";//左发卡器复位
        string FKQRightResetScript = "0_2_右发卡机初始化.txt";//右发卡器复位脚本
        string FKQPositionResetScript = "0_3_发卡机位置初始化.txt";//发卡机位置复位脚本
        string JKQInitScript = "0_4_中转夹卡机构初始化.txt";//夹卡器复位脚本
        string TKQInitScript = "0_5_中转推卡机构初始化.txt";//推卡器复位脚本
        string TransferResetScript = "0_6_中转旋转机构初始化.txt";//中转机构复位脚本
        string LeftMotorResetScript = "0_7_左滚筒机构初始化.txt";//左滚筒初始化
        string RightMotorResetScript = "0_8_右滚筒机构初始化.txt";//右滚筒初始化
        string YLineResetScript = "0_9_Y轴初始化.txt";//Y轴初始化
        string TKQMoveScript = "2_1_推卡机构到接卡位.txt";//推卡器移动脚本 
        string TKQResetScript = "2_2_推卡机构复位.txt";
        string JKQTakeCardScript = "1_1_夹卡机构夹卡.txt";//夹卡器夹卡脚本
        string JKQTakePassportScript = "1_1_夹卡机构夹本.txt";//夹卡器夹护照脚本
        string FKQSendCardScript = "3_1_发卡机发卡.txt";//发卡器发卡脚本      
        string FKQSendPassportScript = "3_1_发卡机发本.txt";//发卡器发证脚本
        string JKQHalfResetScript = "1_2_夹卡机构半复位.txt";//夹卡器半复位
        string JKQHalfActionScript = "1_3_夹卡机构到半复位.txt";//夹卡器半移动
        string JKQHalfActionToCardScript = "1_1_夹卡机构预夹卡位夹卡.txt";//夹卡器半移动后夹卡

        int ROLLERMOTORNUM = 6;//滚筒电机号
        int SPINMOTORNUM = 5;//中转旋转电机号
        int YLINEMOTORNUM = 8;//Y轴电机号
        int SENDERMOVEMOTORNUM = 2;//发卡机移动电机号
        int JKQMOTORNUM = 3;//夹卡电机号

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ4000_V1(int PortNum, byte devIndex, string scriptPath)
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

        public FKJ4000_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

        /*
         * cfgFunctionSwitch位数         代表功能
         * 第一位                        0为身份证，1为护照
         * 第二位                        0为原本滚筒机构，1为扩展滚筒机构
         * 第三位                        0为左滚筒机箱，1为右滚筒机箱
         */
        public void FunctionSwitchSet()
        {
            if ((this.cfgFunctionSwitch & 0x04) == 4)//如果第三位为0，则是发证机左滚筒运行，为1是右滚筒运行
            {
                //主要是滚筒电机运行不一样，左边是6号电机，右边是7号电机
                this.ROLLERMOTORNUM = 7;
            }
        }

        /// <summary>
        /// 二合一发证机初始化函数
        /// </summary>
        /// <returns></returns>
        public override uint DevInit()
        {
            this.ClearDevCondition();

            FunctionSwitchSet();

            //发卡电机初始化
            string script = udM.LoadScriptFromFile(FKQRightResetScript);
            uint r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x00;
            }

            //发证机电机复位
            script = udM.LoadScriptFromFile(FKQLeftResetScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x10;
            }

            //发卡机位置复位
            script = udM.LoadScriptFromFile(FKQPositionResetScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x20;
            }

            ////夹卡器半复位
            //script = udM.LoadScriptFromFile(JKQHalfResetScript);
            //r = FKQRunScript(script, 0, 0, 0);
            //if (r != 0)
            //{
            //    this.ClearDevCondition();
            //    return ERR_INIT | 0x20;
            //}

            //推卡器复位
            script = udM.LoadScriptFromFile(TKQInitScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x50;
            }

            //夹卡器复位
            script = udM.LoadScriptFromFile(JKQInitScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x30;
            }

            //旋转机构初始化
            script = udM.LoadScriptFromFile(TransferResetScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x50;
            }

            //Y轴初始化
            script = udM.LoadScriptFromFile(YLineResetScript);
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x80;
            }

            //左滚筒初始化
            script = udM.LoadScriptFromFile(LeftMotorResetScript);//左滚筒初始化
            r = FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_INIT | 0x60;
            }

            //添加了右滚筒，需要将右滚筒初始化
            if ((cfgFunctionSwitch & 0x02) == 2)
            {
                script = udM.LoadScriptFromFile(RightMotorResetScript);//右滚筒初始化
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_INIT | 0x70;
                }
            }
            return 0;
        }

        public override bool CardIsReady()
        {
            throw new NotImplementedException();
        }

        public override bool CardIsInIcReader()
        {
            throw new NotImplementedException();
        }

        public override bool CardIsInIcReaderOutput()
        {
            throw new NotImplementedException();
        }

        public override bool CardIsInTransfer()
        {
            throw new NotImplementedException();
        }

        public override bool CardIsInOutput()
        {
            throw new NotImplementedException();
        }

        public override string PutCardSensorInfor(int x)
        {
            throw new NotImplementedException();
        }

        public override string GetCardJDQ(int x)
        {
            throw new NotImplementedException();
        }

        public override uint PutCardToICReader()
        {
            return 0;
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

        public override uint PutCardToTransferCar()
        {
            uint r;
            string script;

            //移动Y轴到入卡位置
            r = MotoGoToPosition(YLINEMOTORNUM, this.cfgY_InCard, true, false);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            }

            //移动中转机构到入卡位置
            r = MotoGoToPosition(SPINMOTORNUM, this.cfgX_OutCard, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            }

            if ((this.cfgFunctionSwitch & 0x01) == 1)//1为护照
            {
                //移动发卡机到发身份证位置
                this.ClearDevCondition();
                r = MotoGoToPosition(SENDERMOVEMOTORNUM, 0, true, true);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | (uint)SENDERMOVEMOTORNUM;
                }

                //推卡器推出
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //发证机发证
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQSendPassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器夹证
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakePassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //发证机复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQLeftResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }
            else//0为身份证
            {
                //移动发卡机到发身份证位置
                this.ClearDevCondition();
                r = MotoGoToPosition(SENDERMOVEMOTORNUM, this.cfgX_Excursion, true, true);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | (uint)SENDERMOVEMOTORNUM;
                }

                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //发卡机发卡
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQSendCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器夹卡
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakeCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //发卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQRightResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }
            return r;
        }

        public override uint PutCardToTransferCarFromOutput()
        {
            return 0;
        }

        public override uint PutCardToCardSlot(int x, int y)
        {
            uint r = 1;
            string script;

            r = this.MotoGotoCardSlot(false, x, y, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return r;
            }

            if ((this.cfgFunctionSwitch & 0x01) == 1)//护照
            {
                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }
            else
            {
                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器复位到一半
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQHalfResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }

            return r;
        }

        public override uint PutCardToBadCardSlot(uint param)
        {
            return 0;
        }

        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r = 1;
            string script;

            //if (y >= this.cfgY_SlotCount)
            //    return 0xEE9;
            //if (x >= this.cfgX_SlotCount)
            //    return 0xEEA;

            ////移动Y轴到卡槽位置，X轴移动到放卡位置
            //r = MotoGoToPosition(YLINEMOTORNUM, this.cfgY_Slot0 + y * this.cfgY_Interval + (y / this.cfgY_AdaptTime) * this.cfgY_AdaptPWM, true, true);
            //if (r != 0)
            //{
            //    this.ClearDevCondition();
            //    return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            //}
            ////移动滚筒到取卡位置
            //r = MotoGoToPosition(ROLLERMOTORNUM, this.cfgX_Slot0 + x * this.cfgX_Interval, true, true);
            //if (r != 0)
            //{
            //    this.ClearDevCondition();
            //    return ERR_MOVE_MOTOR | (uint)ROLLERMOTORNUM;
            //}
            ////转动中转机构对准卡槽位置
            //r = MotoGoToPosition(SPINMOTORNUM, this.cfgX_InCard, true, true);
            //if (r != 0)
            //{
            //    this.ClearDevCondition();
            //    return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            //}

            r = this.MotoGotoCardSlot(false,x,y,true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return r;
            }

            if ((this.cfgFunctionSwitch & 0x01) == 1)//护照
            {
                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器夹本
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakePassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }
            else
            {
                //夹卡器收到一半
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQHalfActionScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //夹卡器夹卡
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQHalfActionToCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQResetScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_PUT_CARD_TO_ICREADER;
                }
            }
            return r;
        }

        public override uint GetCardToICReader()
        {
            return 0;
        }

        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            //检测对射传感是否有感应
            uint r = 1;

            r = MotoGoToPosition(YLINEMOTORNUM, this.cfgY_OutCard, true, false);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            }

            r = MotoGoToPosition(SPINMOTORNUM, this.cfgX_OutCard, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            }

            //推卡器移动
            this.ClearDevCondition();
            string script = udM.LoadScriptFromFile(TKQMoveScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_ICREADER;
            }

            //夹卡器复位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(JKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_ICREADER;
            }

            //推卡器复位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQResetScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_PUT_CARD_TO_ICREADER;
            }
            return r;
        }

        public override uint GetCardToBadCardSlot(uint param)
        {
            return 0;
        }

        public override uint MotorUnlock()
        {
            return 0;
        }

        public override uint MotoGotoCardSlot(bool isGetCard, int xIndex, int yIndex, bool waitCompleted)
        {
            uint r = 1;
            int YLineStep = 0;
            int XLineStep = 0;

            if (yIndex >= this.cfgY_SlotCount)
                return 0xEE9;
            if (xIndex >= this.cfgX_SlotCount)
                return 0xEEA;

            this.ClearDevCondition();

            XLineStep = this.cfgX_Slot0 + xIndex * this.cfgX_Interval;
            FKJ_PositionPropertyItem ppi = this.pcfgPositionProperty.FindNearPositionProperty(xIndex, yIndex);
            /*
             1.没有个别位置调节的方法：
                a.移动到卡槽步数 = 0号卡槽位置 + 需要走到的卡槽数 * 卡槽间距
	                step1 = cfgY_Slot0 + x * cfgY_Interval
                b.偏移位置调节步数 = 移动到卡槽需要的步数 + （需要走到的卡槽数 / 调节频率）* 调节步数
	                step2 = step1 + (x / cfgY_AdaptTime) * cfgY_AdaptPWM

             2.有个别位置调节的方法：
                a.移动到卡槽步数 = 0号卡槽位置 + 个别位置调节步数
	                step1 = cfgY_Slot0 + ppi.Step
                b.计算卡槽数量 = 需要到的卡槽数 - 个别位置调节步数的Y轴参数
	                y1 = y - ppi.Y	
                c.移动到卡槽步数 = 移动到卡槽步数 + 计算卡槽数量 * 卡槽间距
	                step2 = step1  + y1 * cfgY_Interval	
                d.偏移位置调节步数 = 移动到卡槽需要的步数 + （需要走到的卡槽数 / 调节频率）* 调节步数
	                step3 = step2 + (y1 / cfgY_AdaptTime) *　cfgY_AdaptPWM
             */
            //计算Y轴需要走的步数
            if (ppi == null)
            {
                if (yIndex >= 0)
                {
                    YLineStep = cfgY_Slot0 + cfgY_Interval * yIndex;
                    if (cfgY_AdaptTime != 0)
                    {
                        if (yIndex < 0)
                            YLineStep -= (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                        else
                            YLineStep += (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    }
                }
                else
                {
                    YLineStep = cfgY_InCard;
                    if (isGetCard)
                        YLineStep = cfgY_OutCard;
                }
            }
            else
            {
                int realYindex = yIndex - ppi.Y;//计算卡槽数
                int new0step = cfgY_Slot0 + ppi.Step;//计算新的0号卡槽的步数（相当于上面的0号卡槽）
                YLineStep = new0step + realYindex * cfgY_Interval;//计算步数                
                //计算的步数后添加偏移值
                if (cfgY_AdaptTime != 0)
                {
                    if (realYindex < 0)
                        YLineStep -= (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    else
                        YLineStep += (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                }
            }

            //移动Y轴到卡槽位置，X轴移动到放卡位置
            r = MotoGoToPosition(YLINEMOTORNUM, YLineStep, true, false);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            }

            //转动中转机构对准卡槽位置
            r = MotoGoToPosition(SPINMOTORNUM, this.cfgX_InCard, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            }

            //移动滚筒到取卡位置
            r = MotoGoToPosition(ROLLERMOTORNUM, XLineStep, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)ROLLERMOTORNUM;
            }

            return r;
        }
    }
}
