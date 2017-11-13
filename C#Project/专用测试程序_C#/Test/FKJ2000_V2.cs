using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

//==========================================================
//名称：FKJ2000_V2
//说明：这个类适用于带以下功能的2000张发证机： 
//备注：该类用于的是2000张发证机；
//      2000张发证机主要相当于两个1000张的发证机组合，只是参数和脚本不同
//      2000张发证机实例化的时候将此类实例化两次，加载两个不同的参数和脚本
//      fkj_D1为前排卡槽的实例化类，fkj_D2为后排卡槽实例化的类
//      该类除了GetCardToTransferCar（）方法和PutCardToCardSlot（）方法外，其他的卡片运行方法都是调用fkj_D1类中同样的方法
//==========================================================

namespace MotorControlBoard
{
    public class FKJ2000_V2 : IFZJ
    {
        public FKJ1000_V2K fkj_D1 = null;//2000张前
        public FKJ1000_V2K fkj_D2 = null;//2000张后

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="devIndex"></param>
        /// <param name="scriptPath"></param>
        public FKJ2000_V2(UsbDeviceManager dev, byte devIndex, string scriptPath)
        {
            fkj_D1 = new FKJ1000_V2K(dev, devIndex, scriptPath);
            if (fkj_D1 == null)
            {
                return;
            }
            fkj_D2 = new FKJ1000_V2K(dev, devIndex, scriptPath);
            if (fkj_D2 == null)
            {
                return;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PortNum">串口号</param>
        /// <param name="devIndex">设备号</param>
        /// <param name="scriptPath">脚本路径</param>
        public FKJ2000_V2(int PortNum, byte devIndex, string scriptPath)
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
            fkj_D1 = new FKJ1000_V2K(udM, devIndex, scriptPath);
            if (fkj_D1 == null)
            {
                return;
            }
            fkj_D2 = new FKJ1000_V2K(udM, devIndex, scriptPath);
            if (fkj_D2 == null)
            {
                return;
            }
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
            string filePath2 = filePath.Substring(0, filePath.Length - 4) + "_D2.cfg";
            if (!File.Exists(filePath2))
                return false;
            this.fkj_D1.LoadConfigFile(filePath);
            this.fkj_D2.LoadConfigFile(filePath2);

            //这几个参数外部程序需要调用，需要将其赋值到2000_V2这个类中
            this.cfgY_SlotCount = fkj_D1.cfgY_SlotCount;
            this.cfgX_SlotCount = fkj_D1.cfgX_SlotCount + fkj_D2.cfgX_SlotCount;
            this.mInCardReaderPort = fkj_D1.mInCardReaderPort;
            this.mOutCardReaderPort = fkj_D1.mOutCardReaderPort;

            return true;
        }

        /// <summary>
        /// 初始化发证机函数 
        /// </summary>
        /// <returns></returns>
        public override uint DevInit()
        {
            //初始化发证机
            this.fkj_D1.FunctionSwitchSet();
            this.fkj_D2.FunctionSwitchSet();//这里需要加载fkj_D2脚本
            //2000张初始化的前卡槽和后卡槽都是一样的，都可以初始化
            return this.fkj_D1.DevInit();
        }

        /// <summary>
        /// 检测发卡兜函数
        /// </summary>
        /// <returns></returns>
        public override bool CardIsReady()
        {
            return this.fkj_D1.CardIsReady();
        }

        /// <summary>
        /// 检测读卡器函数
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInIcReader()
        {
            return this.fkj_D1.CardIsInIcReader();
        }

        /// <summary>
        /// 检测出卡口读卡器是否有卡函数
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInIcReaderOutput()
        {
            return this.fkj_D1.CardIsInIcReaderOutput();
        }

        /// <summary>
        /// 检测中转机构是否有卡函数
        /// </summary>
        /// <returns></returns>
        public override bool CardIsInTransfer()
        {
            return this.fkj_D1.CardIsInTransfer();
        }

        /// <summary>
        /// 检测出卡口是否有卡函数
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 入卡到读卡器函数
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToICReader()
        {
            return this.fkj_D1.PutCardToICReader();
        }

        /// <summary>
        /// 入卡到中转机构函数
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCar()
        {
            return this.fkj_D1.PutCardToTransferCar();
        }

        /// <summary>
        /// 出卡读卡器返卡到中转机构函数
        /// </summary>
        /// <returns></returns>
        public override uint PutCardToTransferCarFromOutput()
        {
            return this.fkj_D1.PutCardToTransferCarFromOutput();
        }

        /// <summary>
        /// 入卡到卡槽函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override uint PutCardToCardSlot(int x, int y)
        {
            if (x > 4)
            {
                return this.fkj_D2.PutCardToCardSlot((x - 5), y);
            }
            else
            {
                return this.fkj_D1.PutCardToCardSlot(x, y);
            }
        }

        /// <summary>
        /// 入卡到废卡存放区函数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint PutCardToBadCardSlot(uint param)
        {
            return this.fkj_D1.PutCardToBadCardSlot(param);
        }

        /// <summary>
        /// 取卡到中转机构函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override uint GetCardToTransferCar(int x, int y)
        {
            if (x > 4)
            {
                return this.fkj_D2.GetCardToTransferCar((x - 5), y);
            }
            else
            {
                return this.fkj_D1.GetCardToTransferCar(x, y);
            }
        }

        /// <summary>
        /// 取卡到读卡器函数
        /// </summary>
        /// <returns></returns>
        public override uint GetCardToICReader()
        {
            return this.fkj_D1.GetCardToICReader();
        }

        /// <summary>
        /// 取卡到用户函数
        /// </summary>
        /// <param name="isHoldCard"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public override uint GetCardToUser(bool isHoldCard, int step)
        {
            return this.fkj_D1.GetCardToUser(isHoldCard, step);
        }

        /// <summary>
        /// 取卡到废卡存放区函数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override uint GetCardToBadCardSlot(uint param)
        {
            return this.fkj_D1.GetCardToBadCardSlot(param);
        }

        /// <summary>
        /// 电机不锁死函数
        /// </summary>
        /// <returns></returns>
        public override uint MotorUnlock()
        {
            return this.fkj_D1.MotorUnlock();
        }

        /// </summary>
        /// <param name="OnOrOff">true为开，false为关</param>
        /// <returns>0为成功，其余为失败</returns>
        public uint ReminderLight(bool OnOrOff)
        {
            return fkj_D1.ReminderLight(OnOrOff);
        }

        /// <summary>
        /// 门锁开关
        /// </summary>
        /// <param name="OnOrOff">true为开，false为关</param>
        /// <returns>0为成功，其余为失败</returns>
        public uint GateLock(bool OnOrOff)
        {
            return fkj_D1.GateLock(OnOrOff);
        }
    }
}
