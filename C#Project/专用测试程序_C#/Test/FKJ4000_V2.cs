using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/*
 * 1.此类专用于二合一发证机第二版本；
 * 2.用于参展；
 * 3.   3号电机更换成9号电机（中转机构夹卡电机）；
        0号传感和4号传感对换（夹卡限位和发护照复位）；
        查询0号传感不能用（发护照口）；
 */

namespace MotorControlBoard
{
    public class FKJ4000_V2 : IFZJ
    {
        string FKQLeftInitScript = "0_1_左发卡机初始化.txt";//左发卡器初始化脚本
        string FKQRightInitScript = "0_2_右发卡机初始化.txt";//右发卡器初始化脚本
        string FKQPositionInitScript = "0_3_发卡机位置初始化.txt";//发卡机位置初始化脚本
        string JKQInitScript = "0_4_中转夹卡机构初始化.txt";//夹卡器初始化脚本
        string TKQInitScript = "0_5_中转推卡机构初始化.txt";//推卡器初始化脚本
        string TransferInitScript = "0_6_中转旋转机构初始化.txt";//中转机构初始化脚本
        string LeftMotorInitScript = "0_7_左滚筒机构初始化.txt";//左滚筒初始化脚本
        string RightMotorInitScript = "0_8_右滚筒机构初始化.txt";//右滚筒初始化脚本
        string YLineInitScript = "0_9_Y轴初始化.txt";//Y轴初始化脚本
        string JKQTakePassportScript = "1_1_夹卡机构夹本.txt";//夹卡器夹护照脚本
        string JKQTakeCardScript = "1_2_夹卡机构夹卡.txt";//夹卡器夹卡脚本
        string JKQMoveToMiddleScript = "1_3_夹卡机构半复位.txt";//夹卡器半打开脚本
        string JKQAllOpenScript = "1_4_夹卡机构全开.txt";//夹卡器全打开脚本
        string TKQMoveToTakeCardScript = "2_1_推卡机构到接卡位.txt";//推卡器移动到去取卡位脚本 
        string TKQMoveToTakePassportScript = "2_2_推卡机构到接本位.txt";//推卡器移动到取证位脚本
        string TKQMoveToEdgeScript = "2_3_推卡机构到边缘位置.txt";//推卡器移动到中转机构边缘脚本
        string TKQMoveToSensorScript = "2_4_推卡机构到传感位置.txt";//推卡器移动到中转机构边缘传感脚本
        string TKQMoveToMiddlePositionScript = "2_5_推卡机构到中间位置.txt";//推卡器从边缘移动到中间脚本
        string FKQSendPassportScript = "3_1_发卡机构发本.txt";//发卡器发证脚本
        string FKQSendCardScript = "3_2_发卡机构发卡.txt";//发卡器发卡脚本      

        int ROLLERMOTORNUM = 6;//滚筒电机号
        int SPINMOTORNUM = 5;//中转旋转电机号
        int YLINEMOTORNUM = 8;//Y轴电机号
        int SENDERMOVEMOTORNUM = 2;//发卡机移动电机号

        public int currentXCoordinate = 0;//X轴当前坐标，每次移动X轴都需要修改

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ4000_V2(int PortNum, byte devIndex, string scriptPath)
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

        public FKJ4000_V2(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            SetDevInfor(dev, devIndex, scriptPath);
        }

        /*
         * cfgFunctionSwitch位数         代表功能
         * 第一位                        0为身份证，1为护照；
         * 第二位                        0为单一滚筒机构的发证机，1为添加了右滚筒机构的二合一发证机；
         * 第三位                        0为发证机的左滚筒机箱，1为发证机的右滚筒机箱；
         * 第四位                        0为初始化时初始化中转机构、发卡机构和滚筒机构，1为初始化时只转滚筒机构；
         * 第五位                        0为不使用自动微调节位置功能，1为使用自动微调功能
         */
        public void FunctionSwitchSet()
        {
            if ((this.cfgFunctionSwitch & 0x04) == 4)//如果第三位为0，则是发证机左滚筒运行，为1是右滚筒运行
            {
                //主要是滚筒电机运行不一样，左边是6号电机，右边是7号电机
                this.ROLLERMOTORNUM = 7;
            }
            else
            {
                this.ROLLERMOTORNUM = 6;
            }
        }

        /// <summary>
        /// 二合一发证机初始化函数
        /// </summary>
        /// <returns></returns>
        public override uint DevInit()
        {
            uint r;
            string script;
            this.ClearDevCondition();

            FunctionSwitchSet();
            if (this.CardIsInEdge())
            {
                //推卡器复位
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }
            }

            //发卡机构初始化
            if ((this.cfgFunctionSwitch & 0x08) == 0)//判断是不是只转滚筒
            {
                //发卡机构位置复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQPositionInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0003;
                }
            }

            //滚筒机构初始化
            if ((this.cfgFunctionSwitch & 0x04) == 4)
            {
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(RightMotorInitScript);//右滚筒初始化
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0008;
                }
            }
            else
            {
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(LeftMotorInitScript);//左滚筒初始化
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0007;
                }
            }
            r = MotoGoToPosition(ROLLERMOTORNUM, this.cfgX_Slot0, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)ROLLERMOTORNUM;
            }

            if ((this.cfgFunctionSwitch & 0x08) == 0)
            {
                //发卡电机复位
                script = udM.LoadScriptFromFile(FKQRightInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0002;
                }

                //发证机电机复位
                script = udM.LoadScriptFromFile(FKQLeftInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0001;
                }

                ////中转机构初始化
                //推卡器复位
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }

                //夹卡器复位
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0004;
                }

                //旋转机构初始化
                script = udM.LoadScriptFromFile(TransferInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0006;
                }

                r = MotoGoToPosition(SPINMOTORNUM, this.cfgX_InCard, true, false);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
                }

                //Y轴初始化
                script = udM.LoadScriptFromFile(YLineInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0009;
                }

            }

            return 0;
        }

        public override bool CardIsReady()
        {
            if ((this.cfgFunctionSwitch & 0x01) == 1)//护照
            {
                if (InquireSensorState(1).Equals("01"))
                    return true;
                else
                    return false;
            }
            else
            {
                if (InquireSensorState(3).Equals("01"))
                    return true;
                else
                    return false;
            }
        }

        //在出卡口是否有卡
        public bool CardIsInSenderOutside()
        {
            if ((this.cfgFunctionSwitch & 0x01) == 1)
            {
                if (InquireSensorState(0).Equals("00"))
                    return true;
                else
                    return false;
            }
            else
            {
                if (InquireSensorState(2).Equals("00"))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 检测边缘是否有遮挡
        /// </summary>
        /// <returns></returns>
        public bool CardIsInEdge()
        {
            if (this.InquireSensorState(12).Equals("01") || this.InquireSensorState(13).Equals("01"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断中转夹卡是否复位
        /// </summary>
        /// <returns></returns>
        public bool TransferIsReady()
        {
            if (this.SensorState(7).Equals("00") || this.SensorState(14).Equals("00"))
                return true;
            return false;
        }

        public override bool CardIsInIcReader()
        {
            return false;
        }

        public override bool CardIsInIcReaderOutput()
        {
            return false;
        }

        public override bool CardIsInTransfer()
        {
            if (this.InquireSensorState(14).Equals("01"))
            {
                return true;
            }
            return false;
        }

        public override bool CardIsInOutput()
        {
            if (this.InquireSensorState(6).Equals("01"))
            {
                return true;
            }
            return false;
        }

        public override string PutCardSensorInfor(int x)
        {
            return "";
        }

        public override string GetCardJDQ(int x)
        {
            return "";
        }

        /// <summary>
        /// 入卡到入卡读卡器，由于发证机的机构不同，不需要使用该方法
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 查询传感器状态
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string InquireSensorState(int num)
        {
            string rHexData;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    udM.SendCommandAndWaitAck(this.mDevIndex, "0248", "", out rHexData, 500);
                    string[] sensorState = rHexData.Split(' ');
                    return sensorState[num + 16];
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

            //判断边缘是否有遮挡
            if (this.CardIsInEdge())
            {
                this.ClearDevCondition();
                return ERR_CARD_WORRY_PLACE | 0x40;
            }

            //判断发卡器有没有卡
            if (!this.CardIsReady())
            {
                this.ClearDevCondition();
                return ERR_PUT_CONTAINER_EMPTY;
            }

            //判断中转机构是否有卡
            if (this.CardIsInTransfer())
            {
                this.ClearDevCondition();
                return ERR_TRANSFER_CAR_BLOCK;
            }

            //判断中转推卡机构是否复位传感器上
            if (!this.SensorState(7).Equals("00"))
            {
                //推卡器复位
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }
            }

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

            //移动发卡机对应的发卡位置
            this.ClearDevCondition();
            r = MotoGoToPosition(SENDERMOVEMOTORNUM, this.cfgX_Excursion, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SENDERMOVEMOTORNUM;
            }

            if ((this.cfgFunctionSwitch & 0x01) == 1)//1为护照
            {
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQAllOpenScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0104;
                }

                //推卡器推出
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToTakePassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0202;
                }

                //发证机发证
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQSendPassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0301;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakePassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0101;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }

                //发证机复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQLeftInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0001;
                }
            }
            else//0为身份证
            {
                //夹卡器打开
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQMoveToMiddleScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0103;
                }

                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToTakeCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0201;
                }

                //发卡机发卡
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQSendCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0302;
                }

                //夹卡器夹卡
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakeCardScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0102;
                }

                //推卡器复位到中间
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToMiddlePositionScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0205;
                }

                //发卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(FKQRightInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0002;
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

            if (!this.CardIsInTransfer())
            {
                this.ClearDevCondition();
                return ERR_TRANSFER_CAR_EMPTY;
            }

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
                script = udM.LoadScriptFromFile(TKQMoveToSensorScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0203;
                }
                //判断推卡器是否在中转机构的出卡位置上
                if (this.SensorState(6).Equals("01"))
                {
                    //如果不在位置上，就直接回收
                    r = RunTKQInitScript();
                    if (r != 0) return r;
                    if ((this.cfgFunctionSwitch & 0x10) == 0x10)//是否使用自动微调节功能
                    {
                        r = PositionAutoAdjust(x, y);
                        if (r != 0)
                        {
                            r = RunTKQInitScript();
                            if (r != 0)
                                return r;
                        }
                    }
                    else
                        return ERR_PUT_CARD_TO_CARD_SLOT;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQAllOpenScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0104;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0004;
                }
            }
            else
            {
                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToEdgeScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0203;
                }
                //判断推卡器是否在中转机构的出卡位置上
                if (this.SensorState(6).Equals("01"))
                {
                    //如果不在位置上，就直接回收
                    r = RunTKQMoveToMiddlePositionScript();
                    if (r != 0) return r;
                    if ((this.cfgFunctionSwitch & 0x10) == 0x10)//是否使用自动微调节功能
                    {
                        r = PositionAutoAdjust(x, y);
                        if (r != 0)
                        {
                            r = RunTKQMoveToMiddlePositionScript();
                            if (r != 0)
                                return r;
                        }
                    }
                    else
                        return ERR_PUT_CARD_TO_CARD_SLOT;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQMoveToMiddleScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0103;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0004;
                }
            }

            if (this.CardIsInEdge())
            {
                this.ClearDevCondition();
                return ERR_CARD_WORRY_PLACE | 0x20; ;
            }

            return r;
        }

        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r;
            string script;
            //X轴移动到废卡位置
            r = MotoGoToPosition(this.SPINMOTORNUM, this.cfgX_BadSlot, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            }
            //Y轴移动到废卡位置(判断param参数)
            r = MotoGoToPosition(this.YLINEMOTORNUM, ((param == 1) ? this.cfgY_BadSlot : this.cfgY_BadSlot + this.cfgY_BadCard_Excursion), true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            }
            //移动推卡机构到边缘位置
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQMoveToSensorScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0203;
            }

            //Y轴下降一定的距离
            r = MotoGoToPosition(this.YLINEMOTORNUM, ((param == 1) ? this.cfgY_BadSlot : this.cfgY_BadSlot + this.cfgY_BadCard_Excursion) + 100, true, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)YLINEMOTORNUM;
            }

            //夹卡器松开
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(JKQAllOpenScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0104;
            }

            //推卡器复位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0005;
            }

            //夹卡器复位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(JKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0004;
            }
            return 0;
        }

        //推卡机构移动边缘位置
        private uint RunTKQMoveToSensorScript()
        {
            string script;
            uint r;
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQMoveToSensorScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0203;
            }
            return r;
        }

        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r = 1;
            string script;

            if (this.CardIsInTransfer())
            {
                this.ClearDevCondition();
                return ERR_TRANSFER_CAR_BLOCK;
            }

            if (!this.SensorState(7).Equals("00"))
            {
                //推卡器复位s
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }
            }

            r = this.MotoGotoCardSlot(false, x, y, true);
            if (r != 0)
            {
                this.ClearDevCondition();
                return r;
            }

            if ((this.cfgFunctionSwitch & 0x01) == 1)//护照
            {
                //夹卡器全开
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQAllOpenScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0104;
                }

                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToSensorScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0203;
                }
                //判断推卡器是否在中转机构的出卡位置上
                if (this.SensorState(6).Equals("01"))
                {
                    //如果不在位置上，就直接回收
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(TKQInitScript);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 0x0005;
                    }
                    //夹卡机构复位
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(JKQInitScript);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 0x0004;
                    }

                    return ERR_PUT_CARD_TO_CARD_SLOT;

                }

                //夹卡器夹本
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQTakePassportScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0101;
                }

                //推卡器复位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0005;
                }
            }
            else
            {
                //夹卡器移动到夹卡位
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQMoveToMiddleScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0103;
                }

                //推卡器移动
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToEdgeScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0203;
                }
                //判断推卡器是否在中转机构的出卡位置上
                if (this.SensorState(6).Equals("01"))
                {
                    //如果不在位置上，就直接回收
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(TKQInitScript);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 0x0005;
                    }
                    //夹卡机构复位
                    this.ClearDevCondition();
                    script = udM.LoadScriptFromFile(JKQInitScript);
                    r = this.FKQRunScript(script, 0, 0, 0);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return ERR_EXECUTE_SCRIPT | 0x0004;
                    }
                    return ERR_PUT_CARD_TO_CARD_SLOT;
                }

                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(JKQInitScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0004;
                }

                //推卡器移动到中间
                this.ClearDevCondition();
                script = udM.LoadScriptFromFile(TKQMoveToMiddlePositionScript);
                r = this.FKQRunScript(script, 0, 0, 0);
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return ERR_EXECUTE_SCRIPT | 0x0205;
                }
            }

            if (this.CardIsInEdge())
            {
                this.ClearDevCondition();
                return ERR_CARD_WORRY_PLACE | 0x30;
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

            //判断中转机构是否有证件
            if (!this.CardIsInTransfer())
            {
                this.ClearDevCondition();
                return ERR_TRANSFER_CAR_EMPTY;
            }

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
            string script = udM.LoadScriptFromFile(TKQMoveToEdgeScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0202;
            }

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(JKQAllOpenScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0104;
            }

            //推卡器复位
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0005;
            }

            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(JKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0004;
            }
            return r;
        }

        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r;
            r = this.PutCardToBadCardSlot(param);
            return r;
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

            //判断参数是否正确
            if (yIndex >= this.cfgY_SlotCount)
                return 0xEE9;
            if (xIndex >= this.cfgX_SlotCount)
                return 0xEEA;

            //判断中转夹卡是否出于复位状态
            if (!this.TransferIsReady())
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | (uint)SPINMOTORNUM;
            }

            //判断是间隙中是否有东西
            if (this.CardIsInEdge())
            {
                this.ClearDevCondition();
                return ERR_CARD_WORRY_PLACE | 0x10;
            }

            //判断发卡机出卡口是否有卡
            //if (this.CardIsInSenderOutside())
            //{
            //    this.ClearDevCondition();
            //    return ERR_CARD_WORRY_PLACE | 0x10;
            //}

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


        /// <summary>
        /// 移动电机到指定的X轴的值
        /// </summary>
        /// <param name="currentX">当前X值</param>
        /// <param name="moveToX">移动到的X值</param>
        /// <param name="?"></param>
        public void MoveRollerToAssignX(int currentX, int moveToX)
        {

        }

        /// <summary>
        /// 推卡机构复位
        /// </summary>
        /// <returns></returns>
        private uint RunTKQInitScript()
        {
            string script;
            uint r;
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQInitScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0005;
            }
            return r;
        }

        private uint RunTKQMoveToMiddlePositionScript()
        {
            string script;
            uint r;
            //推卡器复位到中间
            this.ClearDevCondition();
            script = udM.LoadScriptFromFile(TKQMoveToMiddlePositionScript);
            r = this.FKQRunScript(script, 0, 0, 0);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_EXECUTE_SCRIPT | 0x0205;
            }
            return r;
        }

        /// <summary>
        /// 自动为调节功能
        /// </summary>
        /// <returns></returns>
        private uint PositionAutoAdjust(int x, int y)
        {
            uint r = 1;
            int excursionStep = 0;
            //获取当前位置
            int currentPosition;
            if (this.MotoGetPosition(this.YLINEMOTORNUM, out currentPosition))
            {
                for (int i = 0; i < 6; i++)
                {
                    excursionStep = this.cfgY_Excursion * ((i / 2 == 0) ? i / 2 : -(i / 2));
                    //修改配置项
                    this.pcfgPositionProperty.RemoveItem(x, y);
                    this.pcfgPositionProperty.AddItem(x, y, currentPosition + excursionStep - this.cfgY_Slot0);
                    //移动到入卡位置
                    r = this.MotoGotoCardSlot(false, x, y, true);
                    if (r != 0)
                    {
                        this.ClearDevCondition();
                        return r;
                    }
                    //尝试移动推卡器
                    r = RunTKQMoveToSensorScript();
                    if (r == 0 && this.SensorState(6).Equals("00"))
                    {
                        //如果成功，则写入配置
                        break;//退出当前循环
                    }
                }
            }
            else
            {
                return ERR_PUT_CARD_TO_CARD_SLOT1;
            }
            return r;
        }
    }
}
