using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.IO;

namespace MotorControlBoard
{
    public abstract class IFZJ
    {
        private string LogFileName = null;
        private bool LogInited = false;
        private FileStream LogFileStream = null;
        DateTime dtStart = DateTime.Now;
        DateTime dtEnd = DateTime.Now;
        public void Debug(string msg)
        {
            if (!LogInited)
            {
                if (Directory.Exists("c:\\hblogs\\fkj"))
                {
                    LogFileName = "c:\\hblogs\\fkj\\LOG_" + DateTime.Now.ToString("yyyyMMddHHmmss_fff") + ".txt";
                    new FileStream(LogFileName, FileMode.OpenOrCreate, FileAccess.Write);
                }
                LogInited = true;
            }
            if (LogFileStream != null)
            {
                dtEnd = DateTime.Now;
                int useTime = (int)dtEnd.Subtract(dtStart).TotalMilliseconds;
                string strUseTime = useTime.ToString();
                if (useTime > 60000)
                {
                    strUseTime = "...";
                }
                else if (useTime > 1000)
                {
                    strUseTime = (useTime % 1000).ToString();
                    strUseTime = (useTime / 1000).ToString() + "秒" + strUseTime;
                }
                string str = DateTime.Now.ToString("yyMMddHHmmss_nnn") + "用时:" + strUseTime + "毫秒:" + msg + "\r\n";
                byte[] bs = System.Text.UnicodeEncoding.Default.GetBytes(str);
                LogFileStream.Write(bs, 0, bs.Length);
                LogFileStream.Flush();
                dtStart = DateTime.Now;
            }
        }
        public const uint ERR_PUT_CARD_TO_ICREADER = 0x0000E100;//放卡到IC读卡器错误
        public const uint ERR_PUT_CARD_TO_TRANSFER_CAR = 0x0000E200;//放卡到中转器错误
        public const uint ERR_PUT_CARD_TO_CARD_SLOT = 0x0000E300;//放卡到卡槽错误
        public const uint ERR_PUT_CARD_TO_CARD_SLOT1 = 0x0000E302;//中转区转入卡槽时失败
        public const uint ERR_PUT_CARD_TO_CARD_SLOT2 = 0x0000E303;//卡在卡槽时失败再往里推一下失败
        public const uint ERR_PUT_CARD_TO_BAD_CARD_SLOT = 0x0000E400;//放卡到坏卡槽错误

        public const uint ERR_GET_CARD_TO_TRANSFER_CAR = 0x0000F100;//取卡到中转区错误
        public const uint ERR_GET_CARD_TO_ICREADER = 0x0000F200;//取卡到IC读卡器错误
        public const uint ERR_GET_CARD_TO_BAD_CARD_SLOT = 0x0000F300;//取卡到坏卡槽错误
        public const uint ERR_GET_CARD_TO_USER = 0x0000F400;//取卡到用户错误
        public const uint ERR_GET_CARD_TO_OUTER = 0x0000F500;//取卡到出卡器失败

        public const uint ERR_INIT = 0x0000E001;//初始化失败
        public const uint ERR_INIT_CARD_SLOT_ADJUST = 0x0000E002;//初始化卡位校正失败
        public const uint ERR_INIT_OUTER = 0x0000E003;//初始化出卡器失败

        //0x0000E5xx为通用错误
        public const uint ERR_PUT_CONTAINER_EMPTY = 0x0000E501;//入卡时容器没卡,PutCardToICReader
        public const uint ERR_PUT_IC_READER_BLOCK = 0x0000E502;//入卡处IC卡读写器阻塞,PutCardToICReader
        public const uint ERR_PUT_IC_READER_EMPTY = 0x0000E503;//入卡处IC卡读写器无卡,PutCardToTransferCar
        public const uint ERR_TRANSFER_CAR_BLOCK = 0x0000E504;//中转器阻塞卡,PutCardToCardSlot,
        public const uint ERR_TRANSFER_CAR_EMPTY = 0x0000E505;//中转器无卡,PutCardToCardSlot,

        public const uint ERR_GET_IC_READER_BLOCK = 0x0000E506;//出卡处IC卡读写器阻塞
        public const uint ERR_GET_IC_READER_EMPTY = 0x0000E507;//出卡处IC卡读写器为空

        public const uint ERR_CARD_WORRY_PLACE = 0x0000E508;//卡位置在中转区和卡槽间
        public const uint ERR_CARD_SLOT_EMPTY = 0x0000E509;//取卡时发现卡槽为空

        public const uint ERR_WRONG_PARAMS = 0x0000E50A;//传入的参数不合式，一般是卡槽位置超限
        public const uint ERR_ELECTROMAGNET_TAKEBACK = 0x0000E50B;//电磁铁收回失败
        public const uint ERR_REMINDERLIGHT = 0x0000E50C;//提示灯控制失败
        public const uint ERR_GATELOCK = 0x0000E50D;//门锁控制失败
        public const uint ERR_ELECTROMAGNET_STATE = 0x0000E50E;//电磁铁当前状态错误
        public const uint ERR_ELECTROMAGNET_CHANGE = 0x0000E50F;//电磁铁没有跳变
        public const uint ERR_THREAD_ERROR = 0x0000E510;//线程阻塞

        public const uint ERR_EXECUTE_SCRIPT = 0x00EE0000;//执行相关动作出错
        public const uint ERR_MOVE_MOTOR = 0x00EF0000;//0xE000xx,移动电机时出错,xx表示电机索引
        public const uint ERR_CARD_WRONG_POSITION = 0x00ED0000;//没有运行到指定位置

        public string GetErrDescript(uint code)
        {
            switch (code)
            {
                case ERR_PUT_CARD_TO_ICREADER: return "放卡到IC读卡器错误";
                case ERR_PUT_CARD_TO_TRANSFER_CAR: return "放卡到中转器错误";
                case ERR_PUT_CARD_TO_CARD_SLOT: return "放卡到卡槽错误";
                case ERR_PUT_CARD_TO_CARD_SLOT1: return "中转区转入卡槽时失败";
                case ERR_PUT_CARD_TO_CARD_SLOT2: return "卡在卡槽时失败再往里推一下失败";
                case ERR_PUT_CARD_TO_BAD_CARD_SLOT: return "放卡到坏卡槽错误";

                case ERR_GET_CARD_TO_TRANSFER_CAR: return "取卡到中转区错误";
                case ERR_GET_CARD_TO_ICREADER: return "取卡到IC读卡器错误";
                case ERR_GET_CARD_TO_BAD_CARD_SLOT: return "取卡到坏卡槽错误";
                case ERR_GET_CARD_TO_USER: return "取卡到用户错误";
                case ERR_GET_CARD_TO_OUTER: return "取卡到出卡器失败";

                case ERR_INIT: return "初始化失败";
                case ERR_INIT_CARD_SLOT_ADJUST: return "初始化卡位校正失败";
                case ERR_INIT_OUTER: return "初始化出卡器失败";

                //0x0000E5xx为通用错误
                case ERR_PUT_CONTAINER_EMPTY: return "入卡时容器没卡";
                case ERR_PUT_IC_READER_BLOCK: return "入卡处IC卡读写器阻塞";
                case ERR_PUT_IC_READER_EMPTY: return "入卡处IC卡读写器无卡";
                case ERR_TRANSFER_CAR_BLOCK: return "中转器阻塞卡";
                case ERR_TRANSFER_CAR_EMPTY: return "中转器无卡";

                case ERR_GET_IC_READER_BLOCK: return "出卡处IC卡读写器阻塞";
                case ERR_GET_IC_READER_EMPTY: return "出卡处IC卡读写器为空";
                case ERR_CARD_SLOT_EMPTY: return "取卡时发现卡槽为空";

                case ERR_CARD_WORRY_PLACE: return "卡位置在中转区和卡槽间";
                case ERR_WRONG_PARAMS: return "传入的参数不合式，一般是卡槽位置超限";
                case ERR_ELECTROMAGNET_TAKEBACK: return "电磁铁收回失败";
                case ERR_REMINDERLIGHT: return "提示灯控制失败";
                case ERR_GATELOCK: return "门锁控制失败";
                case ERR_ELECTROMAGNET_STATE: return "电磁铁当前状态错误";
                case ERR_ELECTROMAGNET_CHANGE: return "电磁铁没有跳变";
                case ERR_THREAD_ERROR: return "线程阻塞";
            }
            if ((code & 0x00FF0000) == ERR_MOVE_MOTOR)
                return "移动电机【" + (code & 0xFF) + "】出错";
            if ((code & 0x00FF0000) == ERR_EXECUTE_SCRIPT)
                return "执行脚本段【" + (code & 0xFF) + "】出错";
            if ((code & 0x00FF0000) == ERR_CARD_WRONG_POSITION)
                return "电机【" + (code & 0xFF) + "】没有运行到指定位置";
            return "未知错误";

        }

        //端口参数
        public int mInCardReaderPort = 0;//入卡读卡器端口
        public int mOutCardReaderPort = 0;//出卡读卡器端口
        public int mInCardReaderType = 0;//入卡读卡器类型(1为欧姆龙，2为二合一读卡)
        public int mOutCardReaderType = 0;//出卡读卡器类型(1为欧姆龙，2为二合一读卡)
        public int mTKQPort = 0;//推卡器端口
        public int cfgTKDeviceType = 0;//1:TCD820,2:K720 3:D1802 4:D2000 5:TY900 6:TYIM2000

        //X轴参数
        public int cfgX_InCard = 0;//X轴入卡坐标
        public int cfgX_OutCard = 0;//X轴出卡坐标
        public int cfgX_BadSlot = 0;//X轴坏卡存放区坐标
        public int cfgX_Slot0 = 0;//X轴0号卡槽坐标
        public int cfgX_Interval = 0;//X轴卡槽间距
        public int cfgX_SlotCount = 0;//X轴卡槽的数量

        //Y轴参数
        public int cfgY_InCard = 0;//Y轴入卡坐标
        public int cfgY_OutCard = 0;//Y轴出卡坐标
        public int cfgY_BadSlot = 0;//Y轴坏卡存放区坐标
        public int cfgY_Slot0 = 0;//Y轴0号卡槽坐标
        public int cfgY_Interval = 0;//Y轴间距
        public int cfgY_SlotCount = 0;//Y轴卡槽的数量
        public int cfgY_AdaptTime = 0;//Y轴的调节频率(每cfgY_AdaptTime个卡槽就需要偏移)
        public int cfgY_AdaptPWM = 0;//Y轴的调节步数(偏移的步数)   

        public int cfgAnger0_0 = 0;//X000张的左角度(x000可以为1000或者是2000)
        public int cfgPushCardStep = 0;//推卡装置要走的步数(500张发证机特有)
        public int cfgY_GetCard_Excursion = 0;//新500张Y轴出卡偏移值(500张发证机特有)
        public int cfgY_BadCard_Excursion = 0;//坏卡槽偏移值

        //功能开关，一位代表一个功能。个别型号可能有功能上的区别
        //第一位为设置发卡时中转机构移动的位置（1为直接移动到发卡口位置，0为移动到发卡口位置附近）
        //第二位为500张的新版和旧版（主要用于中转反射传感跳变不同，0为旧版，1为新版）
        //第三位为1000张的银行卡版和身份证版（1为身份证版，0为银行卡版，主要是读卡器的不同）
        //第四位为是否启用电磁铁后的传感器（0为启用，1为不启用，默认为0）
        //第五位为决定的是中转机构的旧版和新版（主要是中转机构的传感器不同，旧版为激光反射传感，新版为槽式传感,1为旧版，0为新版）
        //第六位为决定的是2000张的前排卡槽和后排卡槽（中转机构的运行方向不同，中转机构的抬起方向不同,0为前排，1为后排）
        public int cfgFunctionSwitch = 0;

        //以下参数由于机构修改或者程序上的修改，暂时不用
        public int cfgAnger0_1 = 0;//2000张的右角度(2000张发证机特有)(PS:现暂不用)
        public int cfgY_Excursion = 0;//2000张右卡槽组的偏移值(2000张发证机特有)（PS:现暂不用）
        public int cfgX_Excursion = 0;//2000张左卡槽组的偏移值(2000张发证机特有)（PS:现暂不用）
        public int cfgICReader_InCard = 0;//IC读卡器入卡坐标（PS:现暂不用）
        public int cfgICReader_OutCard = 0;//IC读卡器出卡坐标（PS:现暂不用）
        public int cfgGotoSlotWaitCompleted = 0;//是否等待条件完成后再继续（PS:现暂不用）

        public FKJ_PositionProperty pcfgPositionProperty = new FKJ_PositionProperty();

        public int ReadBoardConfig(ref BoardConfig bc)
        {
            string hexRData;
            uint r = this.SendCommand("0801", "", out hexRData);
            if (r != 0)
            {
                return -1;
            }

            byte[] rdata = UsbDeviceManager.ConvertHexStringToBytes(hexRData);
            bc = (BoardConfig)UsbDeviceManager.ByteToStruct(rdata, typeof(BoardConfig));
            return 0;
        }

        public void SaveToConfigFile(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            Type type = this.GetType();
            FieldInfo[] fis = type.GetFields();//利用反射，查看public字段
            foreach (FieldInfo fi in fis)
            {
                if (fi.Name.StartsWith("cfg"))
                {
                    sb.AppendLine(fi.Name + "=" + fi.GetValue(this).ToString());
                }
                else if ("mInCardReaderPort".Equals(fi.Name) || "mTKQPort".Equals(fi.Name) || "mOutCardReaderPort".Equals(fi.Name) || "mOutCardReaderType".Equals(fi.Name) || "mInCardReaderType".Equals(fi.Name))
                {
                    sb.AppendLine(fi.Name + "=" + fi.GetValue(this).ToString());
                }
            }
            sb.AppendLine("pcfgPositionProperty" + "=" + this.pcfgPositionProperty.ToConfigString());
            if (File.Exists(filePath))
                File.Delete(filePath);
            FileStream fsMyfile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                StreamWriter swMyfile = new StreamWriter(fsMyfile);
                swMyfile.Write(sb.ToString());
                swMyfile.Flush();
                swMyfile.Close();

            }
            finally
            {
                fsMyfile.Close();
            }
        }

        public virtual bool LoadConfigFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            try
            {
                string line;
                Type type = this.GetType();
                while ((line = sr.ReadLine()) != null)
                {
                    string[] pl = line.Split('=');
                    if (pl.Length == 2)
                    {
                        if (pl[0].Equals("pcfgPositionProperty"))
                        {
                            this.pcfgPositionProperty.FromConfigString(pl[1]);
                        }
                        else
                        {
                            FieldInfo fi = type.GetField(pl[0]);
                            if (fi != null)
                            {
                                fi.SetValue(this, int.Parse(pl[1]));
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                sr.Close();
                return false;
            }
            return true;
        }

        protected UsbDeviceManager udM = null;
        protected byte mDevIndex = 0;
        protected string mScriptPath = "";
        public string ScriptPath
        {
            get { return mScriptPath; }
            set { mScriptPath = value; }
        }

        public void CloseDev()
        {
            if (udM != null)
                udM.CloseDev();
        }

        protected uint SendCommand(string hexCMD, string hexData, out string hexRData)
        {
            if (udM == null)
            {
                hexRData = "";
                return 9999;
            }
            uint waitMs = 1500;
            uint r = udM.SendCommandAndWaitAck(mDevIndex, hexCMD, hexData, out hexRData, waitMs);
            return r;

        }

        protected virtual string ReplaceParams(string script, int x, int y, int Params)
        {
            string str = script.Replace("##TKQPORT##", this.mTKQPort.ToString());
            str = str.Replace("##INREADERPORT##", this.mInCardReaderPort.ToString());
            str = str.Replace("##OUTREADERPORT##", this.mOutCardReaderPort.ToString());
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##ANGER0_0##", Common.ConvertStringToHex(this.cfgAnger0_0));
            str = str.Replace("##ICREADER_IN##", this.cfgICReader_InCard.ToString());
            str = str.Replace("##ICREADER_OUT##", this.cfgICReader_OutCard.ToString());
            return str;
        }

        protected void ClearDevCondition()
        {
            string hexRData = "";
            uint r = this.SendCommand("0302", "", out hexRData);
            Debug("ClearDevCondition , R=" + r);
        }

        public void ClearMotorBrakeState()
        {
            string hexData;
            uint r = this.SendCommand("0217", "", out hexData);
            Debug("ClearMotorBrakeState , R=" + r);
        }

        public int GetMotorBrakeState()
        {
            string hexData;
            string msg = "GetMotorBrakeState ,";
            uint r = this.SendCommand("0216", "", out hexData);
            if (r == 0x0B)
            {
                Debug(msg + " R=" + r);
                return 0;
            }
            if (r == 0)
            {
                Debug(msg + " R=" + r + ",hexRData=" + hexData);
                return int.Parse(hexData.Replace(" ", ""));
            }
            return 1;
        }

        public bool MotoGetPosition(int motoIndex, out int position)
        {
            string cmd = "0237";
            position = 0;
            StringBuilder hexData = new StringBuilder();
            hexData.Append(string.Format("{0:X002}", motoIndex));
            String hexRData;
            if (this.SendCommand(cmd, hexData.ToString(), out hexRData) == 0)
            {
                position = int.Parse(Common.ConvertHexToString(hexRData, true));
                Debug("MotoGetPosition ,position=" + position);
                return true;
            }
            Debug("MotoGetPosition ,ERROR 执行失败");
            return false;
        }

        public virtual uint MotoGotoInCardPosition(int param)
        {
            return this.MotoTransferCarGotoPosition(cfgX_InCard, cfgY_InCard, true);
        }

        public virtual uint MotoGotoOutCardPosition(int param)
        {
            return this.MotoTransferCarGotoPosition(cfgX_OutCard, cfgY_OutCard, true);
        }

        public virtual uint MotoTransferCarGotoPosition(int x, int y, bool waitCompleted)
        {
            string hexData;
            this.SendCommand("0217", "", out hexData);
            uint r = this.MotoGoToPosition(5, x, true, false);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 5;
            }

            r = this.MotoGoToPosition(10, y, true, false);
            if (r != 0)
            {
                this.ClearDevCondition();
                return ERR_MOVE_MOTOR | 10;
            }
            if (waitCompleted)
            {
                int xP, yP;
                int xP2, yP2;
                //先判断电机停止了没有
                for (int tryCnt = 0; tryCnt < 10; tryCnt++)
                {
                    int i = 0;
                    while ((i < 180) && this.MotoGetPosition(5, out xP) && this.MotoGetPosition(10, out yP))
                    {
                        System.Threading.Thread.Sleep(100);
                        this.MotoGetPosition(5, out xP2);
                        this.MotoGetPosition(10, out yP2);
                        if ((xP2 == xP) && (yP2 == yP))
                            break;
                        i++;
                    }
                    //停住了
                    this.MotoGetPosition(5, out xP);
                    this.MotoGetPosition(10, out yP);
                    if ((xP == x) && (yP == y))
                        return 0;
                    System.Threading.Thread.Sleep(1000);
                    this.SendCommand("0217", "", out hexData);
                    this.MotoGoToPosition(5, x, true, false);
                    this.MotoGoToPosition(10, y, true, false);
                }
                this.MotoGetPosition(5, out xP);
                this.MotoGetPosition(10, out yP);
                if (xP != x)
                    return ERR_CARD_WRONG_POSITION | 5;
                if (yP != y)
                    return ERR_CARD_WRONG_POSITION | 10;
            }
            return r;

        }

        public virtual uint MotoGotoCardSlot(bool isGetCard, int xIndex, int yIndex, bool waitCompleted)
        {
            uint r;
            int step = 0;
            //首先计算要走到的卡槽的坐标
            if (yIndex >= this.cfgY_SlotCount)
                return 0xEE9;
            if (xIndex >= this.cfgX_SlotCount)
                return 0xEEA;
            this.ClearDevCondition();

            //计算X轴需要走的步数
            int startLinePosition = cfgX_Slot0 + xIndex * cfgX_Interval;
            if (isGetCard)
                startLinePosition = cfgX_OutCard;
            //如果有位置配置信息的话，就使用位置配置信息重新计算
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
                    step = cfgY_Slot0 + cfgY_Interval * yIndex;
                    if (cfgY_AdaptTime != 0)
                    {
                        if (yIndex < 0)
                            step -= (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                        else
                            step += (yIndex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    }
                }
                else
                {
                    step = cfgY_InCard;
                    if (isGetCard)
                        step = cfgY_OutCard;
                }
            }
            else
            {
                int realYindex = yIndex - ppi.Y;//计算卡槽数
                int new0step = cfgY_Slot0 + ppi.Step;//计算新的0号卡槽的步数（相当于上面的0号卡槽）
                step = new0step + realYindex * cfgY_Interval;//计算步数                
                //计算的步数后添加偏移值
                if (cfgY_AdaptTime != 0)
                {
                    if (realYindex < 0)
                        step -= (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                    else
                        step += (realYindex / cfgY_AdaptTime) * cfgY_AdaptPWM;
                }
            }
            r = this.MotoTransferCarGotoPosition(startLinePosition, step, true);
            if (r != 0)
            {
                this.ClearDevCondition();
            }
            return r;
        }

        public void MotoStop(int motoIndex, Boolean lockMotor)
        {
            string hexRData;
            this.SendCommand("0206", (lockMotor ? 1 : 0).ToString().PadLeft(2, '0'), out hexRData);
        }

        public void MotoRun(int motoIndex, Boolean lockMotor, byte direction, int pwm)
        {
            string hexRData;
            this.SendCommand("0205", Convert.ToString(motoIndex, 16).PadLeft(2, '0') + (lockMotor ? "01" : "00") + ((direction == 00) ? "00" : "01") + Common.ConvertStringToHex(pwm), out hexRData);
        }

        public uint MotoGoToPosition(int motoIndex, int position, Boolean lockMotor, Boolean waitForOk)
        {
            StringBuilder hexData = new StringBuilder();
            string hexRData;
            int step = 0;
            int currentPosition = 0;
            BoardConfig bc = new BoardConfig();
            Debug("==>MotoGoToPosition ,motoIndex=" + motoIndex + ",position=" + position + ",lockMotor=" + lockMotor + ",waitForOk" + waitForOk);
            // bc.Frequency3 = 0;
            if (this.MotoGetPosition(motoIndex, out currentPosition))
            {
                StringBuilder script = new StringBuilder();
                if (position != currentPosition)
                {
                    step = position - currentPosition;
                    //如果是适服电机距离太短，就改变速度
                    //if (waitForOk && Math.Abs(step) < 700 && motoIndex == 0x0A)
                    //{
                    //    uint r = this.SendCommand("0801", "", out hexRData);
                    //    byte[] rdata = UsbDeviceManager.ConvertHexStringToBytes(hexRData);
                    //    if (r == 0)
                    //    {
                    //        bc = (BoardConfig)UsbDeviceManager.ByteToStruct(rdata, typeof(BoardConfig));
                    //        string cmd = "08" + Common.ConvertStringToHex((int)(bc.Frequency3 / 2));
                    //        r = this.SendCommand("0202", cmd, out hexRData);
                    //        if (r != 0)
                    //            bc.Frequency3 = 0;
                    //        else
                    //            System.Threading.Thread.Sleep(100);
                    //    }
                    //    else
                    //        bc.Frequency3 = 0;
                    //}

                    //##---          等待 电机【42】 (【停止】) 执行 ---  【 不处理动作,只等待条件完成 】 
                    script.Append("A=S(0301 02" + string.Format("{0:X002}", motoIndex) + " 00 00 ff 00 00 00000000)\r\n");
                    //##---          【向前】 转动电机【3】 100步，停止时【松开】
                    script.Append("A=S(0301 00 00 00 11 " + string.Format("{0:X002}", motoIndex));
                    if (lockMotor)
                        script.Append("01 ");
                    else
                        script.Append("00 ");
                    if (step < 0)
                        script.Append("00 ");
                    else
                        script.Append("01 ");
                    script.Append(Common.ConvertStringToHex(Math.Abs(step)) + ")\r\n");
                }
                //##---          在【0x30秒】内等待所有条件处理完成【超时终止脚本】
                if (waitForOk)
                    script.Append("1=TEST(1E 00 02 00 0303 00)\r\n");
                script.Append("1=DELAY(200)\r\n");
                udM.Logger = new StringBuilder();
                uint r2 = udM.RunScript(0, script.ToString());
                //if (waitForOk && Math.Abs(step) < 3000 && motoIndex == 0x0A && bc.Frequency3 != 0)
                //    this.SendCommand("0202", "08" + Common.ConvertStringToHex((int)bc.Frequency3), out hexRData);
                Debug("<==MotoGoToPosition ,R=" + r2);
                return r2;
            }
            Debug("<==MotoGoToPosition ERROR 执行失败");
            return 9;
        }

        public uint FKQRunScriptFromFile(string filePath, int x, int y, int p)
        {
            string script = udM.LoadScriptFromFile(filePath);
            string str = ReplaceParams(script, x, y, p);
            return udM.RunScript(mDevIndex, str);
        }

        public uint FKQRunScript(string script, int x, int y, int p, Boolean isSynchronized)
        {
            string str = ReplaceParams(script, x, y, p);
            return udM.RunScript(mDevIndex, str, isSynchronized);
        }

        public uint FKQRunScript(string script, int x, int y, int p)
        {
            return FKQRunScript(script, x, y, p, true);
        }

        /// <summary>
        /// 获取传感器跳变次数
        /// </summary>
        /// <param name="num">传感器号</param>
        /// <returns></returns>
        public int GetSensorChangeCount(int num)
        {
            string hexOut;
            this.SendCommand("0401", "", out hexOut);
            string[] sensorCheckNum = hexOut.Split(' ');
            return Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
        }

        public int[] GetSensorChangeCount()
        {
            string hexOut;
            this.SendCommand("0401", "", out hexOut);
            string[] sensorCheckNum = hexOut.Split(' ');
            int[] result = new int[16];
            for (int num = 0; num < 16; num++)
                result[num] = Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
            return result;
        }

        public int GetSensorStateCount(int num, int state)
        {
            string hexOut;
            uint r;
            if (state == 0)
                r = this.SendCommand("0405", "00", out hexOut);
            else
                r = this.SendCommand("0405", "01", out hexOut);
            if (r != 0)
                return 0;
            //return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            string[] sensorCheckNum = hexOut.Split(' ');
            return Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
        }

        public int[] GetSensorStateCount(int state)
        {
            string hexOut;
            uint r;
            if (state == 0)
                r = this.SendCommand("0405", "00", out hexOut);
            else
                r = this.SendCommand("0405", "01", out hexOut);
            if (r != 0)
                return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            string[] sensorCheckNum = hexOut.Split(' ');
            int[] result = new int[16];
            for (int num = 0; num < 16; num++)
                result[num] = Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
            return result;
        }

        public int[] GetSensorCheckNum()
        {
            int[] result = new int[16];
            string hexOut;
            this.SendCommand("0401", "", out hexOut);
            string[] sensorCheckNum = hexOut.Split(' ');
            for (int i = 0; i < 16; i++)
            {
                result[i] = Convert.ToInt32(sensorCheckNum[i * 2 + 1] + sensorCheckNum[i * 2], 16);
            }
            return result;
        }
        int[] sensorCurrentChangeCount = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private string mLogPath = null;
        public string LogPath
        {
            get { return mLogPath; }
            set
            {
                if (value.EndsWith("\\"))
                    mLogPath = value;
                else
                    mLogPath = value + "\\";
            }
        }
        public void OutputLog(string msg, Boolean outputState)
        {
            if (LogPath != null && Directory.Exists(LogPath))
            {
                if (File.Exists(LogPath + "IFZJ_LOG.txt"))
                {
                    FileInfo fi = new FileInfo(LogPath + "IFZJ_LOG.txt");
                    if (fi.Length > 1024 * 1024 * 5)
                    {
                        File.Move(LogPath + "IFZJ_LOG.txt", LogPath + "IFZJ_LOG_" + DateTime.Now.ToString("yyMMddHHmmss") + ".txt");
                    }
                }
                int[] bh0 = GetSensorCheckNum();
                string hexRData;
                this.SendCommand("0204", "", out hexRData);
                string[] v = hexRData.Split(' ');
                int[] scnt0 = GetSensorStateCount(0);
                int[] scnt1 = GetSensorStateCount(1);
                StreamWriter sw = File.AppendText(LogPath + "IFZJ_LOG.txt");
                string str = DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss:fff") + " " + msg;
                if (outputState)
                {
                    str += "\r\n\t传感状态：";
                    for (int i = 0; i < bh0.Length; i++)
                        str += i.ToString().PadLeft(2) + ":" + v[i] + ", ";
                    str += "\r\n\t传感变化：";
                    for (int i = 0; i < bh0.Length; i++)
                        str += i.ToString().PadLeft(2) + ":" + bh0[i].ToString().PadRight(6) + ",";
                    str += "\r\n\t传感差值：";
                    for (int i = 0; i < bh0.Length; i++)
                        str += i.ToString().PadLeft(2) + ":" + (bh0[i] - sensorCurrentChangeCount[i]).ToString().PadRight(6) + ",";
                    str += "\r\n\t传感器低：";
                    for (int i = 0; i < scnt0.Length; i++)
                        str += i.ToString().PadLeft(2) + ":" + scnt0[i].ToString().PadRight(6) + ",";
                    str += "\r\n\t传感器高：";
                    for (int i = 0; i < scnt0.Length; i++)
                        str += i.ToString().PadLeft(2) + ":" + scnt1[i].ToString().PadRight(6) + ",";
                }
                sensorCurrentChangeCount = bh0;
                if (outputState)
                    sw.WriteLine(str + "\r\n=======\r\n\r\n", Encoding.UTF8);
                else
                    sw.WriteLine(str + "\r\n", Encoding.UTF8);
                sw.Close();
            }
        }

        public abstract uint DevInit();//发证机初始化

        public abstract bool CardIsReady();//发卡器是否有卡
        public abstract bool CardIsInIcReader();//入卡读卡器是否有卡
        public abstract bool CardIsInIcReaderOutput();//出卡读卡器是否有卡
        public abstract bool CardIsInTransfer();//中转区是否有卡
        public abstract bool CardIsInOutput();//出卡口是否有卡

        public abstract string PutCardSensorInfor(int x);
        public abstract string GetCardJDQ(int x);

        public abstract uint PutCardToICReader();//入卡到读卡器
        public abstract uint PutCardToTransferCar();//入卡到中转机构
        public abstract uint PutCardToTransferCarFromOutput();//出卡回收到中转机构
        public abstract uint PutCardToCardSlot(int x, int y);//入卡到卡槽
        public abstract uint PutCardToBadCardSlot(uint param);//入卡到坏卡槽

        public abstract uint GetCardToTransferCar(int x, int y);//取卡到中转机构
        public abstract uint GetCardToICReader();//取卡到读卡器
        public abstract uint GetCardToUser(bool isHoldCard, int step);//取卡到用户
        public abstract uint GetCardToBadCardSlot(uint param);//取卡到坏卡槽

        /// <summary>
        /// 快速盘库函数，用于判断卡槽是否有卡
        /// </summary>
        /// <param name="x">X轴坐标</param>
        /// <param name="y">Y轴坐标</param>
        /// <returns>0为无卡，1为有卡，其余为报错</returns>
        public virtual uint SlotIsEmpty(int x, int y)
        {
            return 0;
        }

        public abstract uint MotorUnlock();//电机松开（除伺服电机）
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BoardConfig
    {
        public byte IsSetCfg;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public int[] O_Frequency;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public int[] T_Frequency;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public int[] A_Frequency;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] IsSfmotor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] DutyCycle;
        public int WaitLoopInKeyEvent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorDir;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorSensor0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorSensor1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorControlSensor0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorControlSensor1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorBrake1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] motorBrake2;
        public byte SFMotorAlamAvailableState;
        public byte SFMotorBreakerAvailableState;
        public byte SFMotorPutDownBreakTimeout;
        public byte SFMotorPutDownBreakWaitInterval;
    }
}
