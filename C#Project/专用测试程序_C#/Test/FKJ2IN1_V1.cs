using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MotorControlBoard
{
    public class FKJ2IN1_V1 : IFZJ
    {
        public FKJ4000_V2 fkj_D1 = null;//定义fkj_D1为左滚筒
        public FKJ4000_V2 fkj_D2 = null;//定义fkj_D2为右滚筒
        public Boolean IsCard = true;//判断是否是卡片式证件

        private void SetDevInfor(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            mScriptPath = scriptPath;
            udM = dev;
            udM.ScriptPath = scriptPath;
            mDevIndex = devIndex;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="devIndex"></param>
        /// <param name="scriptPath"></param>
        public FKJ2IN1_V1(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            fkj_D1 = new FKJ4000_V2(dev, devIndex, scriptPath);
            if (fkj_D1 == null)
            {
                return;
            }
            fkj_D2 = new FKJ4000_V2(dev, devIndex, scriptPath);
            if (fkj_D2 == null)
            {
                return;
            }
            SetDevInfor(dev, devIndex, scriptPath);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ2IN1_V1(int PortNum, byte devIndex, string scriptPath)
        {
            //打开串口号，实例化udM
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
            //实例化fkj_D1和fkj_D2
            fkj_D1 = new FKJ4000_V2(udM, devIndex, scriptPath);
            if (fkj_D1 == null)
            {
                return;
            }
            fkj_D2 = new FKJ4000_V2(udM, devIndex, scriptPath);
            if (fkj_D2 == null)
            {
                return;
            }
            SetDevInfor(udM, 0, scriptPath);
        }

        /// <summary>
        /// 重写父类中的导入参数函数
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public override bool LoadConfigFile(string filePath)
        {
            //判断文件路径是否存在
            if (!File.Exists(filePath))
                return false;
            this.fkj_D1.LoadConfigFile(filePath);//左滚筒类加载参数
            if ((fkj_D1.cfgFunctionSwitch & 0x2) == 2)//双滚筒
            {
                string filePath2 = filePath.Substring(0, filePath.Length - 4) + "_D2.cfg";
                if (!File.Exists(filePath2))
                    return false;
                this.fkj_D2.LoadConfigFile(filePath2);//右滚筒类加载参数
            }
            return true;
        }

        /// <summary>
        /// 发证机初始化
        /// </summary>
        /// <returns></returns>
        public override uint DevInit()
        {
            uint r;
            //调用功能开关
            this.fkj_D1.FunctionSwitchSet();
            r = this.fkj_D1.DevInit();
            if (r != 0)
            {
                this.ClearDevCondition();
                return r;
            }
            if ((fkj_D1.cfgFunctionSwitch & 0x2) == 2)//双滚筒
            {
                this.fkj_D2.FunctionSwitchSet();
                r = this.fkj_D2.DevInit();
                if (r != 0)
                {
                    this.ClearDevCondition();
                    return r;
                }
            }
            return r;
        }

        public override bool CardIsReady()
        {
            bool r = false;
            //要判断是发护照的类还是发卡片的类
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = false; break;
                    case 1://单左卡片
                        r = fkj_D1.CardIsReady(); break;
                    case 2://左护照右卡片
                        r = fkj_D2.CardIsReady(); break;
                    case 3://双护照
                        r = false; break;
                    case 4://双卡片
                        r = fkj_D1.CardIsReady(); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.CardIsReady(); break;
                    case 1://单左卡片
                        r = false; break;
                    case 2://左护照右卡片
                        r = fkj_D1.CardIsReady(); break;
                    case 3://双护照
                        r = fkj_D1.CardIsReady(); break;
                    case 4://双卡片
                        r = false; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override bool CardIsInIcReader()
        {
            return this.fkj_D1.CardIsInIcReader();
        }

        public override bool CardIsInIcReaderOutput()
        {
            return this.fkj_D1.CardIsInIcReaderOutput();
        }

        public override bool CardIsInTransfer()
        {
            return this.fkj_D1.CardIsInTransfer();
        }

        public override bool CardIsInOutput()
        {
            return this.fkj_D1.CardIsInOutput();
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
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.PutCardToICReader(); break;
                    case 2://左护照右卡片
                        r = fkj_D2.PutCardToICReader(); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.PutCardToICReader(); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.PutCardToICReader(); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.PutCardToICReader(); break;
                    case 3://双护照
                        r = fkj_D1.PutCardToICReader(); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint PutCardToTransferCar()
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.PutCardToTransferCar(); break;
                    case 2://左护照右卡片
                        r = fkj_D2.PutCardToTransferCar(); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.PutCardToTransferCar(); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.PutCardToTransferCar(); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.PutCardToTransferCar(); break;
                    case 3://双护照
                        r = fkj_D1.PutCardToTransferCar(); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint PutCardToTransferCarFromOutput()
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.PutCardToTransferCarFromOutput(); break;
                    case 2://左护照右卡片
                        r = fkj_D2.PutCardToTransferCarFromOutput(); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.PutCardToTransferCarFromOutput(); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.PutCardToTransferCarFromOutput(); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.PutCardToTransferCarFromOutput(); break;
                    case 3://双护照
                        r = fkj_D1.PutCardToTransferCarFromOutput(); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint PutCardToCardSlot(int x, int y)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.PutCardToCardSlot(x, y); break;
                    case 2://左护照右卡片
                        r = fkj_D2.PutCardToCardSlot(x, y); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        if (x > fkj_D1.cfgX_SlotCount)
                            r = fkj_D2.PutCardToCardSlot(x - fkj_D1.cfgX_SlotCount, y);
                        else
                            r = fkj_D1.PutCardToCardSlot(x, y);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.PutCardToCardSlot(x, y); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.PutCardToCardSlot(x, y); break;
                    case 3://双护照
                        if (x > fkj_D1.cfgX_SlotCount)
                            r = fkj_D2.PutCardToCardSlot(x - fkj_D1.cfgX_SlotCount, y);
                        else
                            r = fkj_D1.PutCardToCardSlot(x, y);
                        break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint PutCardToBadCardSlot(uint param)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.PutCardToBadCardSlot(param); break;
                    case 2://左护照右卡片
                        r = fkj_D2.PutCardToBadCardSlot(param); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.PutCardToBadCardSlot(param); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.PutCardToBadCardSlot(param); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.PutCardToBadCardSlot(param); break;
                    case 3://双护照
                        r = fkj_D1.PutCardToBadCardSlot(param); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint GetCardToTransferCar(int x, int y)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.GetCardToTransferCar(x, y); break;
                    case 2://左护照右卡片
                        r = fkj_D2.GetCardToTransferCar(x, y); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        if (x > fkj_D1.cfgX_SlotCount)
                            r = fkj_D2.GetCardToTransferCar(x - fkj_D1.cfgX_SlotCount, y);
                        else
                            r = fkj_D1.GetCardToTransferCar(x, y);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.GetCardToTransferCar(x, y); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.GetCardToTransferCar(x, y); break;
                    case 3://双护照
                        if (x > fkj_D1.cfgX_SlotCount)
                            r = fkj_D2.GetCardToTransferCar(x - fkj_D1.cfgX_SlotCount, y);
                        else
                            r = fkj_D1.GetCardToTransferCar(x, y);
                        break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint GetCardToICReader()
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.GetCardToICReader(); break;
                    case 2://左护照右卡片
                        r = fkj_D2.GetCardToICReader(); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.GetCardToICReader(); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.GetCardToICReader(); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.GetCardToICReader(); break;
                    case 3://双护照
                        r = fkj_D1.GetCardToICReader(); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.GetCardToUser(isHoldCard, step); break;
                    case 2://左护照右卡片
                        r = fkj_D2.GetCardToUser(isHoldCard, step); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.GetCardToUser(isHoldCard, step); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.GetCardToUser(isHoldCard, step); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.GetCardToUser(isHoldCard, step); break;
                    case 3://双护照
                        r = fkj_D1.GetCardToUser(isHoldCard, step); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint GetCardToBadCardSlot(uint param)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1; break;
                    case 1://单左卡片
                        r = fkj_D1.GetCardToBadCardSlot(param); break;
                    case 2://左护照右卡片
                        r = fkj_D2.GetCardToBadCardSlot(param); break;
                    case 3://双护照
                        r = 1; break;
                    case 4://双卡片
                        r = fkj_D1.GetCardToBadCardSlot(param); break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.GetCardToBadCardSlot(param); break;
                    case 1://单左卡片
                        r = 1; break;
                    case 2://左护照右卡片
                        r = fkj_D1.GetCardToBadCardSlot(param); break;
                    case 3://双护照
                        r = fkj_D1.GetCardToBadCardSlot(param); break;
                    case 4://双卡片
                        r = 1; break;
                    default:
                        break;
                }
            }
            return r;
        }

        public override uint MotorUnlock()
        {
            return fkj_D1.MotorUnlock();
        }

        /// <summary>
        /// 获取二合一发证机类型,用于确定用户程序显示界面
        /// </summary>
        /// <returns>
        ///          -2为发证机左滚筒没有实例化
        ///          -1为参数错误；
        ///          0是只有左滚筒，用于装护照;
        ///          1是只有左滚筒，用于装卡片;
        ///          2是左滚筒装护照，右滚筒装卡片;
        ///          3是左、右滚筒都是装护照;
        ///          4是左、右滚筒都是装卡片;
        /// </returns>
        public int Get2IN1FKJType()
        {
            if ((fkj_D1.cfgFunctionSwitch & 0x2) == 0)//是否只有单一的左滚筒
            {
                if ((fkj_D1.cfgFunctionSwitch & 0x1) == 1)
                    return 0;//单一左滚筒装护照
                else
                    return 1;//单一左滚筒装卡片
            }
            else
            {
                if ((fkj_D1.cfgFunctionSwitch & 0x1) == 1)
                {
                    if (fkj_D2 == null)//发证机右滚筒没有实例化
                        return -2;
                    if ((fkj_D2.cfgFunctionSwitch & 0x1) == 1)
                        return 3;//左右滚筒都是装护照
                    else
                        return 2;//左护照,右卡片
                }
                else
                {
                    if (fkj_D2 == null)//发证机右滚筒没有实例化
                        return -2;
                    if ((fkj_D2.cfgFunctionSwitch & 0x1) == 1)
                        return -1;//没有左是卡片，右是护照的二合一发证机
                    else
                        return 5;//左右滚筒都是装卡片
                }
            }
        }

        /// <summary>
        /// 获取发证机成卡片存储数量
        /// </summary>
        /// <returns></returns>
        public int GetFKJStoreQuantity(bool IsCard)
        {
            int quantity = 0;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        quantity = 0; break;
                    case 1://单左卡片
                        quantity = fkj_D1.cfgX_SlotCount * fkj_D1.cfgY_SlotCount; break;
                    case 2://左护照右卡片
                        quantity = fkj_D2.cfgX_SlotCount * fkj_D2.cfgY_SlotCount; break;
                    case 3://双护照
                        quantity = 0; break;
                    case 4://双卡片
                        quantity = fkj_D1.cfgX_SlotCount * fkj_D1.cfgY_SlotCount + fkj_D2.cfgX_SlotCount * fkj_D2.cfgY_SlotCount; break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        quantity = fkj_D1.cfgX_SlotCount * fkj_D1.cfgY_SlotCount; break;
                    case 1://单左卡片
                        quantity = 0; break;
                    case 2://左护照右卡片
                        quantity = fkj_D1.cfgX_SlotCount * fkj_D1.cfgY_SlotCount; break;
                    case 3://双护照
                        quantity = fkj_D1.cfgX_SlotCount * fkj_D1.cfgY_SlotCount + fkj_D2.cfgX_SlotCount * fkj_D2.cfgY_SlotCount; break;
                    case 4://双卡片
                        quantity = 0; break;
                    default:
                        break;
                }
            }

            return quantity;
        }

        public int GetXCount(bool IsCard)
        {
            int XCount = 0;
            switch (Get2IN1FKJType())
            {
                case 0://单左护照
                    XCount = IsCard ? 0 : fkj_D1.cfgX_SlotCount;
                    break;
                case 1://单左卡片
                    XCount = IsCard ? fkj_D1.cfgX_SlotCount : 0;
                    break;
                case 2://左护照右卡片
                    XCount = IsCard ? fkj_D2.cfgX_SlotCount : fkj_D1.cfgX_SlotCount;
                    break;
                case 3://双护照
                    XCount = IsCard ? 0 : fkj_D1.cfgX_SlotCount + fkj_D2.cfgX_SlotCount;
                    break;
                case 4://双卡片
                    XCount = IsCard ? fkj_D1.cfgX_SlotCount + fkj_D2.cfgX_SlotCount : 0;
                    break;
                default:
                    break;
            }
            return XCount;
        }


        public int GetYCount(bool IsCard)
        {
            int YCount = 0;
            switch (Get2IN1FKJType())
            {
                case 0://单左护照
                    YCount = IsCard ? 0 : fkj_D1.cfgY_SlotCount;
                    break;
                case 1://单左卡片
                    YCount = IsCard ? fkj_D1.cfgY_SlotCount : 0;
                    break;
                case 2://左护照右卡片
                    YCount = IsCard ? fkj_D2.cfgY_SlotCount : fkj_D1.cfgY_SlotCount;
                    break;
                case 3://双护照
                    YCount = IsCard ? 0 : fkj_D1.cfgY_SlotCount + fkj_D2.cfgY_SlotCount;
                    break;
                case 4://双卡片
                    YCount = IsCard ? fkj_D1.cfgY_SlotCount + fkj_D2.cfgY_SlotCount : 0;
                    break;
                default:
                    break;
            }
            return YCount;
        }

        /// <summary>
        /// 移动到卡槽位置
        /// </summary>
        /// <param name="isGetCard"></param>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <param name="waitCompleted"></param>
        /// <returns></returns>
        public override uint MotoGotoCardSlot(bool isGetCard, int xIndex, int yIndex, bool waitCompleted)
        {
            uint r = 1;
            if (IsCard)
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = 1;
                        break;
                    case 1://单左卡片
                        r = fkj_D1.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        break;
                    case 2://左护照右卡片
                        r = fkj_D2.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        break;
                    case 3://双护照
                        r = 1;
                        break;
                    case 4://双卡片
                        if (xIndex > fkj_D1.cfgX_SlotCount && (xIndex - fkj_D1.cfgX_SlotCount) < fkj_D2.cfgY_SlotCount)
                        {
                            r = fkj_D2.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        }
                        else if (xIndex < fkj_D1.cfgX_SlotCount)
                        {
                            r = fkj_D1.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        }
                        else
                        {
                            r = 1;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Get2IN1FKJType())
                {
                    case 0://单左护照
                        r = fkj_D1.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        break;
                    case 1://单左卡片
                        r = 1;
                        break;
                    case 2://左护照右卡片
                        r = fkj_D1.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        break;
                    case 3://双护照
                        if (xIndex > fkj_D1.cfgX_SlotCount && (xIndex - fkj_D1.cfgX_SlotCount) < fkj_D2.cfgY_SlotCount)
                        {
                            r = fkj_D2.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        }
                        else if (xIndex < fkj_D1.cfgX_SlotCount)
                        {
                            r = fkj_D1.MotoGotoCardSlot(isGetCard, xIndex, yIndex, waitCompleted);
                        }
                        else
                        {
                            r = 1;
                        }
                        break;
                    case 4://双卡片
                        r = 1;
                        break;
                    default:
                        break;
                }
            }
            return r;
        }

        //获取当前滚筒X轴的坐标
        public int GetCurrentXValue()
        {
            int currentValue = 0;
            currentValue = fkj_D1.currentXCoordinate;
            currentValue = fkj_D2.currentXCoordinate;
            return currentValue;
        }

        /// <summary>
        /// 获取传感状态值
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
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
                    return sensorState[num];
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            throw new Exception("发送命令错误");
        }
    }
}
