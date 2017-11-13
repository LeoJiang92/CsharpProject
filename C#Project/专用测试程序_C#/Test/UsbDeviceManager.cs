using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Runtime.CompilerServices;

namespace MotorControlBoard
{
    
    public class UsbDeviceManager
    {
        protected const string dllPath = "HBUsbDev.dll";
        protected string mScriptPath = "";
        public string ScriptPath
        {
            get { return mScriptPath; }
            set {
                if (value.LastIndexOf("\\") != value.Length - 1)
                    mScriptPath = value + "\\";
                else
                    mScriptPath = value;
            }
        }
        /// <summary>
        /// 根据VID,PID创建设备通讯实例
        /// </summary>
        /// <param name="vid">VID</param>
        /// <param name="pid">PID</param>
        /// <returns>返回设备通讯实例的句柄，如果创建失败，返回0</returns>
        [DllImport(dllPath)]
        private static extern UInt32 hid_create_device_class(UInt16 vid, UInt16 pid);

        /// <summary>
        /// 释放设备通讯实例
        /// </summary>
        /// <param name="handle">设备通讯实例</param>
        [DllImport(dllPath)]
        private static extern void hid_free_device_class(UInt32 handle);

        /// <summary>
        /// 获取可使用的设备数量
        /// </summary>
        /// <param name="handle">设备通讯实例句柄</param>
        /// <returns>实际可用设备的数量</returns>
        [DllImport(dllPath)]
        private static extern UInt32 hid_get_devices_count(UInt32 handle);

        /// <summary>
        /// 发送使命令
        /// </summary>
        /// <param name="handle">设备通讯实例句柄</param>
        /// <param name="devIndex">设备索引，从0开始</param>
        /// <param name="packageNum">命令包号（可选)，用于调试时输出句号的日志，一般搞个全局变量自增即可</param>
        /// <param name="cmdCode">命令代码</param>
        /// <param name="state">固定取:0</param>
        /// <param name="pdata">命令的可选数据</param>
        /// <param name="datalen">可选数据的长度</param>
        /// <returns>返回错误代码，执行成功返回0x00</returns>
        [DllImport(dllPath)]
        private static extern UInt32 hid_send_command(UInt32 handle, byte devIndex, UInt16 packageNum, UInt16 cmdCode, byte state, byte[] pdata, UInt32 datalen);

        /// <summary>
        /// 发送命令包，并且等待数据返馈包。
        /// </summary>
        /// <param name="handle">设备通讯实例句柄</param>
        /// <param name="devIndex">设备索引，从0开始</param>
        /// <param name="packageNum">命令包号（可选)，用于调试时输出句号的日志，一般搞个全局变量自增即可</param>
        /// <param name="cmdCode">命令代码</param>
        /// <param name="state">固定取:0</param>
        /// <param name="pdata">命令的可选数据</param>
        /// <param name="datalen">可选数据的长度</param>
        /// <param name="rstate">命令处理结果</param>
        /// <param name="prdata">返回相应的数据</param>
        /// <param name="rdatalen">返回相应数据长度</param>
        /// <param name="waitMs">接收数据时的等待超时时间</param>
        /// <returns>返回错误代码，执行成功返回0x00</returns>
        [DllImport(dllPath)]
        private static extern UInt32 hid_send_cmd_and_rec(UInt32 handle, byte devIndex, UInt16 packageNum, UInt16 cmdCode, byte state, byte[] pdata, UInt32 datalen, out byte rstate, byte[] prdata, out UInt32 rdatalen, UInt32 waitMs);


        /// <summary>
        /// 读取设备的Flash信息，Flash大小0xFF
        /// </summary>
        /// <param name="handle">设备通讯实例句柄</param>
        /// <param name="devIndex">设备索引，从0开始</param>
        /// <param name="addr">Flash的地址索引,0x00开始</param>
        /// <param name="len">要读取的长度</param>
        /// <param name="pdata">读取到的数据</param>
        /// <param name="rlen">读取到的数据实际长度</param>
        /// <returns>返回错误代码，执行成功返回0x00</returns>
        [DllImport(dllPath)]
        private static extern UInt32 hid_read_flash(UInt32 handle, byte devIndex,UInt32 addr,UInt32 len,byte[] pdata, out UInt32 rlen);
        
        /// <summary>
        /// 写取设备的Flash信息，Flash大小0xFF
        /// </summary>
        /// <param name="handle">设备通讯实例句柄</param>
        /// <param name="devIndex">设备索引，从0开始</param>
        /// <param name="addr">Flash的地址索引,0x00开始</param>
        /// <param name="pdata">写入的数据</param>
        /// <param name="len">待写入的数据的长度</param>
        /// <returns>返回错误代码，执行成功返回0x00</returns>                              
        [DllImport(dllPath)]
        private static extern UInt32 hid_write_flash(UInt32 handle, byte devIndex, UInt32 addr, byte[] pdata, UInt32 len);

        
        [DllImport(dllPath)]
        private static extern UInt32 hid_set_flash_value(UInt32 handle, byte devIndex, byte flashValue);

        [DllImport(dllPath)]
        private static extern UInt32 hid_find_index_by_flash(UInt32 handle, byte flashValue);

        public const UInt32 USER_EXIT_FROM_SCRIPT = 0x0000EE001;
        public const UInt32 USER_SCRIPT_ERROR= 0x0000EE002;

        private UInt32 mDevHandle = 0;
        private UInt16 mVID = 0;
        private UInt16 mPID = 0;
        private UInt16 mPackageIndex = 1;

        private StringBuilder mLogger = null;

        private Dictionary<string,string>[] mParams = null;
        private bool[] mExitFromScript = null;
        private SerialPort[] SerialPorts = new SerialPort[200];

        public StringBuilder Logger
        {
            get { return mLogger; }
            set { mLogger = value; }
        }
        public virtual uint OpenDev(byte portNum)
        {
            return 0;
        }

        public virtual void CloseDev() { }
        public UsbDeviceManager(UInt16 vid, UInt16 pid)
        {
            ReflashDeviceList(vid,pid);
        }

        ~UsbDeviceManager()
        {
            if (mDevHandle != 0)
                hid_free_device_class(mDevHandle);
            mDevHandle = 0;
        }

        /// <summary>
        /// 根据VID和PID刷新可用备列表
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        public void ReflashDeviceList(UInt16 vid, UInt16 pid)
        {
            if (mDevHandle != 0)
                hid_free_device_class(mDevHandle);
            mDevHandle = hid_create_device_class(vid, pid);
            mVID = vid;
            mVID = pid;
            mParams = new Dictionary<string,string>[hid_get_devices_count(mDevHandle)+1];
            mExitFromScript = new bool[hid_get_devices_count(mDevHandle) + 1];
            for (int i = 0; i < hid_get_devices_count(mDevHandle) + 1; i++)
            {
                mParams[i] = new Dictionary<string, string>();
                mExitFromScript[i] = false;
            }
            for (int i = 0; i < SerialPorts.Length; i++)
            {
                if (SerialPorts[i] != null)
                    SerialPorts[i].Close();
                SerialPorts[i] = null;
            }
        }

        public void ReflashDeviceList()
        {
            ReflashDeviceList(mVID, mPID);
            mPackageIndex = 1;
        }

        public UInt32 GetDevicesCount()
        {
            return hid_get_devices_count(mDevHandle);
        }

        /// <summary>
        /// 把16进制的字符串转换为相应的数组
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToBytes(string hexStr)
        {
            string realHexStr = hexStr.Replace(" ", "").Replace("0x", "");
            if (realHexStr.Length<2)
                return new byte[0];
            byte[] result = new byte[realHexStr.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(realHexStr.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static string ConvertBytesToHexString(byte[] bytes, UInt32 len, string spanChar)
        {
            StringBuilder sb = new StringBuilder();
            if (len > bytes.Length)
                len = (UInt32)bytes.Length;
            string format="{0:X002}"+spanChar;
            for (int i = 0; i < len; i++)
            {
                sb.AppendFormat(format, bytes[i]);
            } 
            return sb.ToString();
        }

        public static byte[] StructToBytes(object structObj,int size)
        {
            int num = 2;
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间 
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷贝到byte 数组 
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间 
            Marshal.FreeHGlobal(structPtr);
            return bytes;

        } 
        public static object ByteToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            //分配结构体内存空间 
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷贝到分配好的内存空间 
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体 
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间 
            Marshal.FreeHGlobal(structPtr);
            return obj; 
        }

        public UInt32 ReadFlash(int devIndex, int addr, int len,out string hexStr)
        {
            byte[] pdata = new byte[0xFF + 1];
            uint rlen = 0;
            if (len > 0xFF)
                len = 0xFF;
            UInt32 result = hid_read_flash(mDevHandle, (byte)devIndex, (uint)addr, (uint)len, pdata, out rlen);
            if (result == 0)
                hexStr = ConvertBytesToHexString(pdata, rlen, " ");
            else
                hexStr = "";
            return result;
        }

        public UInt32 WriteFlash(int devIndex, int addr, string hexStr)
        {
            byte[] pdata = ConvertHexStringToBytes(hexStr);
            int len = pdata.Length;
            if (addr + len > 255)
                len = 255 - addr;
            return hid_write_flash(mDevHandle, (byte)devIndex, (uint)addr, pdata, (uint)len);
        }

        /// <summary>
        /// 发送单条指令
        /// （成功返回0，反之返回错误代码）
        /// </summary>
        /// <param name="devIndex"></param>
        /// <param name="hexCMD"></param>
        /// <param name="hexData"></param>
        /// <param name="hexRData"></param>
        /// <param name="waitMs"></param>
        /// <returns>成功返回0，反之返回错误代码</returns>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        public virtual UInt32 SendCommandAndWaitAck(byte devIndex, string hexCMD, string hexData, out string hexRData,UInt32 waitMs)
        {
            UInt16 cmdCode = (UInt16)Convert.ToInt16(hexCMD, 16);
            byte[] pdata = ConvertHexStringToBytes(hexData);     
            byte[] prdata = new byte[0x1FF];
            byte state;
            UInt32 rDataLen;
            DateTime st = DateTime.Now;
            UInt32 result = hid_send_cmd_and_rec(mDevHandle, devIndex, mPackageIndex++, cmdCode, 0, pdata, (UInt32)pdata.Length, out state, prdata, out rDataLen, waitMs);
            if (result==0)
                result = state;
            DateTime et = DateTime.Now;
            if (Logger != null)
            {
                Logger.Append(devIndex.ToString() + " SEND:[0x" + hexCMD + "] (" + hexData + ")\r\n");
            }
            hexRData = "";
            if (result == 0)
            {
                if (rDataLen != 0)
                    hexRData = ConvertBytesToHexString(prdata, rDataLen, " ");
                result = state;
            }
            else
            {
                WriteLog("\r\n出错命令：" + hexCMD + "\r\n出错数据：" + hexData +"\r\n命执行失败:返回0x" + String.Format("{0:X0004}", result) + "(" + result + ")");                                  
            }
            if (Logger!=null)
                Logger.Append(devIndex.ToString() + " REC :[0x" + String.Format("{0:X004}",result)+","+result + "] (" + hexRData + ")\r\n---------用时："+(et.Ticks-st.Ticks).ToString()+"毫秒----------\r\n\r\n");
            return result;
        }

        public string ReadLine(string str,ref int startIndex)
        {
            int nextPos = str.IndexOf("\n",startIndex);
            string result = "";
            if (nextPos>0)
                result = str.Substring(startIndex, nextPos - startIndex);
            startIndex = nextPos + 1;
            return result;
        }

        public void SetParam(int devIndex, string paramName,string value)
        {
            this.mParams[devIndex][paramName] = value;
        }

        public string GetParam(int devIndex, string paramName)
        {
            return this.mParams[devIndex][paramName];
        }

        
        public void ExitFromScript(int devIndex)
        {
            mExitFromScript[devIndex] = true;
        }

        public String LoadScriptFromFile(string filePath)
        {
            string realFileString = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\scripts\";
            if (!string.IsNullOrEmpty(mScriptPath))
            {
                realFileString = mScriptPath + filePath;
            }
            FileStream fs = new FileStream(realFileString, FileMode.Open, FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs,0,bs.Length);
            fs.Close();
            return System.Text.ASCIIEncoding.Default.GetString(bs);
        }


        private uint CreateUsartPortInstance(int portIndex, int bautrate)
        {
            if (this.SerialPorts[portIndex] != null)
            {
                if (!(this.SerialPorts[portIndex] is PushCardDevice))
                {
                    this.SerialPorts[portIndex].Close();
                    this.SerialPorts[portIndex] = null;
                }
            }
            if (this.SerialPorts[portIndex] == null)
            {
                this.SerialPorts[portIndex] = new PushCardDevice(portIndex, bautrate);
                this.SerialPorts[portIndex].Open();
            }
            if (!this.SerialPorts[portIndex].IsOpen)
            {
                this.SerialPorts[portIndex] = null;
                Error("打开串口[" + portIndex + "]失败");
                return 0xE8;
            }
            return 0;
        }

        System.Threading.Mutex mutexScript = new System.Threading.Mutex();
        public UInt32 RunScript(int devIndex, string strScript)
        {
            return RunScript(devIndex, strScript, true);
        }

        /// <summary>
        /// 格式： 
        /// 变量值=SEND:命令 数据
        /// 变量值=DELAY:xxx毫秒
        /// 变量值=WAIT:命令 数据
        /// </summary>
        /// <param name="devIndex"></param>
        /// <param name="strScript"></param>
        /// <returns></returns>                
        public UInt32 RunScript(int devIndex,string strScript,Boolean isSynchronized)
        {
            int curPos = 0;
            string line;            
            mExitFromScript[devIndex] = false;
            strScript = strScript.Replace("\r","") + "\n";
            //Regex r = new Regex(@"([0-9,A-F,a-f]{1})\=([^:]*)\:(.*)");
            Regex r = new Regex(@"([^\=]*)\=([^\(]*)\((.*)\)");
            //替换变量
            Regex rParams0 = new Regex(@"\[([^\,,^\]]*)\]");
            Regex rParams1 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\]");
            Regex rParams2 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\,([0-9]{1,5})\]");
            if (isSynchronized)
                if (!mutexScript.WaitOne())
                    return 0x0000E510;
            try
            {
                line = ReadLine(strScript, ref curPos);
                Boolean isInIFStatement = false;//是否在条件语句里
                Boolean ifStatementIsTrue = false;//条件为真
                try
                {
                    while (curPos > 0)
                    {
                        Application.DoEvents();
                        if (mExitFromScript[devIndex])
                        {
                            Error("强制退出脚本");
                            return USER_EXIT_FROM_SCRIPT;
                        }
                        if ((line.Replace(" ", "").Length < 4) || (line.Replace(" ", "")[0] == '#'))
                        {
                            line = ReadLine(strScript, ref curPos);
                            continue;
                        }
                        Match m = r.Match(line);
                        if (m.Success)
                        {
                            string paramName = m.Groups[1].Value.ToUpper();
                            string cmd = m.Groups[2].Value.Replace(" ", "").ToUpper();
                            string data = m.Groups[3].Value;
                            string realData = data;
                            //替换变量
                            Match mP0 = rParams0.Match(data);
                            while (mP0.Success)
                            {
                                string pName = mP0.Groups[1].Value.ToUpper();
                                realData = realData.Replace(mP0.Groups[0].Value, this.mParams[devIndex][pName].ToString());
                                mP0 = mP0.NextMatch();
                            }
                            data = realData;
                            Match mP1 = rParams1.Match(data);
                            while (mP1.Success)
                            {
                                string pName = mP1.Groups[1].Value.ToUpper();
                                int pLen = int.Parse(mP1.Groups[2].Value);
                                pLen = Math.Min(pLen, this.mParams[devIndex][pName].Length);
                                realData = realData.Replace(mP1.Groups[0].Value, this.mParams[devIndex][pName].Substring(0, pLen));
                                mP1 = mP1.NextMatch();
                            }
                            data = realData;
                            Match mP2 = rParams2.Match(data);
                            while (mP2.Success)
                            {
                                string pName = mP2.Groups[1].Value.ToUpper();
                                int pStartIndex = int.Parse(mP2.Groups[2].Value);
                                int pLen = int.Parse(mP2.Groups[3].Value);
                                pStartIndex = Math.Min(pStartIndex, this.mParams[devIndex][pName].Length);
                                pLen = Math.Min(pLen, this.mParams[devIndex][pName].Length - pStartIndex);
                                realData = realData.Replace(mP2.Groups[0].Value, this.mParams[devIndex][pName].Substring(pStartIndex, pLen));
                                mP2 = mP2.NextMatch();
                            }
                            if (mExitFromScript[devIndex])
                            {
                                Error("强制退出脚本");
                                return USER_EXIT_FROM_SCRIPT;
                            }
                            Application.DoEvents();
                            //先处理条件语句
                            if (isInIFStatement)
                            {
                                if (cmd.Equals("ENDIF"))
                                    isInIFStatement = false;
                                else if (cmd.Equals("ELSE"))
                                {
                                    ifStatementIsTrue = !ifStatementIsTrue;
                                }
                                else
                                {
                                    if (!ifStatementIsTrue)
                                    {
                                        line = ReadLine(strScript, ref curPos);
                                        continue;
                                    }
                                }
                            }
                            if (cmd.Equals("DELAY"))
                            {
                                int waitTimeMS = int.Parse(realData);
                                for (int i = 0; i < waitTimeMS; i += 50)
                                {
                                    System.Threading.Thread.Sleep(50);
                                    if (mExitFromScript[devIndex])
                                    {
                                        Error("强制退出脚本");
                                        return USER_EXIT_FROM_SCRIPT;
                                    }
                                    Application.DoEvents();
                                }
                            }
                            else if (cmd.Equals("INCLUDE"))
                            {
                                string fileName = ScriptPath + realData;
                                if (realData.IndexOf(@":") == 1)
                                    fileName = realData;
                                if (!System.IO.File.Exists(fileName))
                                {
                                    MessageBox.Show("脚本文件不存在[" + fileName + "]");
                                    return USER_SCRIPT_ERROR;
                                }
                                else
                                {
                                    string includeScript = this.LoadScriptFromFile(realData);
                                    if (RunScript(devIndex, includeScript) != 0)
                                        return USER_SCRIPT_ERROR;
                                }
                            }
                            else if (cmd.Equals("EXIST"))
                            {
                                return UInt32.Parse(realData);
                            }
                            else if (cmd.Equals("SET"))
                            {
                                mParams[devIndex][paramName] = realData;
                            }
                            else if (cmd.Equals("OUTPUT"))
                            {
                                if (Logger != null)
                                    Logger.Append("\r\n\r\nOUTPUT:" + realData + "\r\n\r\n");
                            }
                            else if (cmd.Equals("S") || cmd.Equals("SEND"))
                            {
                                realData = realData.Replace(" ", "");
                                string strAck;
                                UInt32 sendResult = SendCommandAndWaitAck((byte)devIndex, realData.Substring(0, 4), realData.Substring(4), out strAck, 2000);
                                mParams[devIndex][paramName] = strAck;
                                if (sendResult != 0)
                                {
                                    Error("命执行失败:返回0x" + String.Format("{0:X0004}", sendResult) + "(" + sendResult + ")");
                                   // WriteLog("+/r/n出错命令：" + realData + "/r/n" + "命执行失败:返回0x" + String.Format("{0:X0004}", sendResult) + "(" + sendResult + ")");
                                    return sendResult;
                                }
                            }
                            else if (cmd.Equals("TYRM3000"))
                            {
                                Debug(line);
                                byte[] tyrmData = new byte[100];
                                int dwRead = 0;
                                string[] tyrmCmd = realData.Split(',');
                                int r1 = TYRM3000.OpenDev(int.Parse(tyrmCmd[0]));
                                if (r1 != 0)
                                {
                                    Error("TYRM3000 打开端口 失败:" + tyrmCmd[0] + "R=" + r1.ToString());
                                    return 0xE8;
                                }
                                r1 = TYRM3000.SendData(tyrmCmd[1][0], tyrmCmd[2][0], tyrmCmd[3][0], tyrmData, 0, ref dwRead, 3000);
                                if (r1 == 0)
                                {
                                    string Param = ConvertBytesToHexString(tyrmData, (uint)dwRead, "");
                                    mParams[devIndex][paramName] = Param;
                                    Debug("接收" + dwRead + "字节:" + Param);
                                }
                                else
                                {
                                    Error("TYRM3000 失败" + r1.ToString());
                                    return 0xE8;
                                }
                            }
                            else if (cmd.Equals("TYRMClose"))
                            {
                                Debug(line);
                                TYRM3000.PortClose();
                            }
                            else if (cmd.Equals("TCD820M"))
                            {
                                Debug(line);
                                mParams[devIndex][paramName] = "";
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                uint result = CreateUsartPortInstance(portIndex, 9600);
                                if (result != 0)
                                    return result;
                                byte[] rdata = ((PushCardDevice)this.SerialPorts[portIndex]).SendTCD820M(realData.Substring(realData.IndexOf(",") + 1));
                                if (rdata.Length == 0)
                                {
                                    Error("推卡器发执行命令" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)rdata.Length, "");
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("K720"))
                            {
                                Debug(line);
                                mParams[devIndex][paramName] = "";
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                uint result = CreateUsartPortInstance(portIndex, 9600);
                                if (result != 0)
                                    return result;
                                byte[] rdata = ((PushCardDevice)this.SerialPorts[portIndex]).SendK720(realData.Substring(realData.IndexOf(",") + 1));
                                if (rdata.Length == 0)
                                {
                                    Error("推卡器发执行命令" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)rdata.Length, "");
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("D1801"))
                            {
                                Debug(line);
                                mParams[devIndex][paramName] = "";
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                uint result = CreateUsartPortInstance(portIndex, 9600);
                                if (result != 0)
                                    return result;
                                byte[] rdata = ((PushCardDevice)this.SerialPorts[portIndex]).SendD1801(realData.Substring(realData.IndexOf(",") + 1));
                                if (rdata.Length == 0)
                                {
                                    Error("推卡器发执行命令" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)rdata.Length, "");
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("D2000"))
                            {
                                Debug(line);
                                mParams[devIndex][paramName] = "";
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                uint result = CreateUsartPortInstance(portIndex, 9600);
                                if (result != 0)
                                    return result;
                                byte[] rdata = ((PushCardDevice)this.SerialPorts[portIndex]).SendD2000(realData.Substring(realData.IndexOf(",") + 1));
                                if (rdata.Length == 0)
                                {
                                    Error("推卡器发执行命令" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)rdata.Length, "");
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("TY900"))
                            {
                                Debug(line);
                                mParams[devIndex][paramName] = "";
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                uint result = CreateUsartPortInstance(portIndex, 9600);
                                if (result != 0)
                                    return result;
                                byte[] rdata = ((PushCardDevice)this.SerialPorts[portIndex]).SendTy900Cmd(realData.Substring(realData.IndexOf(",") + 1));
                                if (rdata.Length == 0)
                                {
                                    Error("推卡器发执行命令" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)rdata.Length, "");
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("V2CF"))
                            {
                                Debug(line);
                                realData = realData.Replace(" ", "");
                                string[] ps = realData.Split(',');
                                int portIndex = int.Parse(ps[0]);
                                byte[] pdata = null;
                                if (ps.Length == 3)
                                    pdata = ConvertHexStringToBytes(ps[2]);

                                if (V2CF.OpenDev(portIndex) != 0)
                                {
                                    Error("打开端口[" + portIndex + "]失败!");
                                    return 0xE6;
                                }
                                int rstate;
                                byte[] rdata;
                                int result = V2CF.SendData(ps[1], pdata, out rstate, out rdata);
                                if (result != 0)
                                {
                                    Error("读卡器发执行命令失败R=" + result + ":" + realData.Substring(realData.IndexOf(",") + 1));
                                    return 0xE6;
                                }
                                else
                                {
                                    mParams[devIndex][paramName] = (Convert.ToString(rstate, 16).PadLeft(4, '0') + ConvertBytesToHexString(rdata, (uint)rdata.Length, "")).ToUpper();
                                    Debug("接收:" + mParams[devIndex][paramName]);
                                }
                            }
                            else if (cmd.Equals("TYIM2000"))
                            {
                                Debug(line);
                                byte[] tyrmData = new byte[100];
                                int dwRead = 0;
                                string[] tyrmCmd = realData.Split(',');
                                int r1 = TYRM3000.OpenDev(int.Parse(tyrmCmd[0]));
                                if (r1 != 0)
                                {
                                    Error("TYRM2000 打开端口 失败:" + tyrmCmd[0] + "R=" + r1.ToString());
                                    return 0xE8;
                                }
                                r1 = TYRM3000.SendData(tyrmCmd[1][0], tyrmCmd[2][0], tyrmCmd[3][0], tyrmData, 0, ref dwRead, 3000);
                                if (r1 == 0)
                                {
                                    string Param = ConvertBytesToHexString(tyrmData, (uint)dwRead, "");
                                    mParams[devIndex][paramName] = Param;
                                    Debug("接收" + dwRead + "字节:" + Param);
                                }
                                else
                                {
                                    Error("TYIM2000 失败" + r1.ToString());
                                    return 0xE8;
                                }
                            }
                            else if (cmd.Equals("TYRIMClose"))
                            {
                                Debug(line);
                                TYRM3000.PortClose();
                            }
                            else if (cmd.StartsWith("USART_CLOSE"))
                            {
                                Debug(line);
                                int portIndex = int.Parse(realData.Replace(" ", ""));
                                if (this.SerialPorts[portIndex] != null)
                                {
                                    this.SerialPorts[portIndex].Close();
                                    this.SerialPorts[portIndex] = null;
                                }
                            }
                            else if (cmd.StartsWith("USART_W"))
                            {
                                Debug(line);
                                realData = realData.Replace(" ", "");
                                int portIndex = int.Parse(realData.Substring(0, realData.IndexOf(",")));
                                byte[] usartData = ConvertHexStringToBytes(realData.Substring(realData.IndexOf(",") + 1));
                                if (this.SerialPorts[portIndex] == null)
                                    this.SerialPorts[portIndex] = new PushCardDevice(portIndex, 9600);
                                this.SerialPorts[portIndex].Write(usartData, 0, usartData.Length);
                            }
                            else if (cmd.StartsWith("USART_R"))
                            {
                                Debug(line);
                                int portIndex = int.Parse(realData.Replace(" ", ""));
                                if (this.SerialPorts[portIndex] == null)
                                    this.SerialPorts[portIndex] = new PushCardDevice(portIndex, 9600);
                                Application.DoEvents();
                                int oldByteToRead = 0;
                                int byteToRead = this.SerialPorts[portIndex].BytesToRead;
                                int cnt = 0;
                                while (cnt++ < 10)
                                {
                                    if (oldByteToRead != byteToRead)
                                    {
                                        cnt = 0;
                                        oldByteToRead = byteToRead;
                                    }
                                    System.Threading.Thread.Sleep(10);
                                    Application.DoEvents();
                                    byteToRead = this.SerialPorts[portIndex].BytesToRead;
                                }
                                if (byteToRead > 0)
                                {
                                    byte[] rdata = new byte[byteToRead];
                                    this.SerialPorts[portIndex].Read(rdata, 0, byteToRead);
                                    mParams[devIndex][paramName] = ConvertBytesToHexString(rdata, (uint)byteToRead, "");
                                }
                                else
                                    mParams[devIndex][paramName] = "";
                            }
                            else if (cmd.StartsWith("TEST"))//第一字节是多少个200毫秒,第一字节为开始，第二字节为长度，第三字节开始为值
                            //[1:测试的超时时间多少秒] [1:比较字符串的开始位置] [1:字符串长度] [N:比较的值] [2:命令] [N:数据]
                            {
                                uint ErrCode = USER_SCRIPT_ERROR;
                                //这里再检查一下                            
                                realData = realData.Replace(" ", "");
                                int vTestSec = Convert.ToInt32(realData.Substring(0, 2), 16);//第一字节，时间
                                int vStartIndex = Convert.ToInt32(realData.Substring(2, 2), 16);//第二字节，源字符串的起始位置
                                int vLen = Convert.ToInt32(realData.Substring(4, 2), 16);//第三字节，源字符串的长度
                                //第四节字暂未获取
                                string vValue = realData.Substring(6, vLen);//第六字节到vLen,是比对目标 
                                string strAck;
                                UInt32 sendResult;
                                bool testSuccess = false;
                                int n = 1;
                                if (vTestSec == 0)
                                    vTestSec = 1;
                                while ((!testSuccess))
                                {
                                    sendResult = SendCommandAndWaitAck((byte)devIndex, "0216", "", out strAck, 2000);
                                    if (sendResult != 0x0B)
                                    {
                                        if (sendResult != 0)
                                        {
                                            Error("获取电机急停状态失败！R=" + sendResult + ",strAck=" + strAck);
                                            break;
                                        }
                                        else if (!"00".Equals(strAck.Replace(" ", "")))
                                        {
                                            Error("电机急停中无法操作！");
                                            testSuccess = true;
                                            break;
                                        }
                                    }
                                    if ((n++) % 5 == 0)     
                                    {
                                        if (--vTestSec == 0)
                                            break;
                                        n = 1;
                                    }
                                    Application.DoEvents();
                                    if (mExitFromScript[devIndex])
                                    {
                                        Error("强制退出脚本");
                                        return USER_EXIT_FROM_SCRIPT;
                                    }
                                    
                                    if (cmd.IndexOf("TEST_RESULT") == 0)
                                    {
                                        sendResult = SendCommandAndWaitAck((byte)devIndex, realData.Substring(8, 4), realData.Substring(10), out strAck, 2000);
                                        //mParams[devIndex][paramName] = String.Format("{0:X0004}", sendResult) + strAck;
                                        mParams[devIndex][paramName] = strAck;
                                        if (sendResult == Convert.ToInt32(realData.Substring(0, 8)))
                                        {
                                            testSuccess = true;
                                            break;
                                        }
                                    }
                                    else if (cmd.IndexOf("TEST_ERR_CODE") == 0)
                                    {
                                        //第一字节，时间
                                        //第二字节，源字符串的起始位置
                                        //第三字节，vLen:源字符串的长度(是字符串，不是HEX字节数)
                                        //第四节开始，以vLen个字符串长度作为比较
                                        //vLen后的两个字节是发送的命令
                                        //后面的字节是命令的参数
                                        //....
                                        ErrCode = uint.Parse(realData.Substring(realData.Length-4));
                                        int hexDataLen = realData.Length - (6 + vLen + 4) - 4;
                                        string cmd_params = realData.Substring(6 + vLen + 4, hexDataLen);
                                        sendResult = SendCommandAndWaitAck((byte)devIndex, realData.Substring(6 + vLen, 4), cmd_params, out strAck, 2000);
                                        if (sendResult == 0)
                                        {
                                            mParams[devIndex][paramName] = strAck.Replace(" ", "");
                                            if (mParams[devIndex][paramName].Length >= vStartIndex + vLen)
                                            {
                                                if (mParams[devIndex][paramName].Substring(vStartIndex, vLen).Equals(vValue))
                                                {
                                                    testSuccess = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (cmd.IndexOf("TEST") == 0)//TEST,TEST_TIMEOUT
                                    {
                                        //第一字节，时间
                                        //第二字节，源字符串的起始位置
                                        //第三字节，vLen:源字符串的长度(是字符串，不是HEX字节数)
                                        //第四节开始，以vLen个字符串长度作为比较
                                        //vLen后的两个字节是发送的命令
                                        //后面的字节是命令的参数
                                        sendResult = SendCommandAndWaitAck((byte)devIndex, realData.Substring(6 + vLen, 4), realData.Substring(6 + vLen + 4), out strAck, 2000);
                                        if (sendResult == 0)
                                        {
                                            mParams[devIndex][paramName] = strAck.Replace(" ", "");
                                            if (mParams[devIndex][paramName].Length >= vStartIndex + vLen)
                                            {
                                                if (mParams[devIndex][paramName].Substring(vStartIndex, vLen).Equals(vValue))
                                                {
                                                    testSuccess = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    System.Threading.Thread.Sleep(200);
                                }
                                if ((!testSuccess))
                                {
                                    if (cmd.IndexOf("CONTINUE") > 1)
                                    {
                                        Error("命执行失败(但脚本继续):TEST命令:\r\n" + realData + "\r\n没有期望结果:\r\n" + mParams[devIndex][paramName] + " <=> " + vValue);
                                        if (cmd.IndexOf("CLEAR") > 1)
                                            SendCommandAndWaitAck((byte)devIndex, "0302", "", out strAck, 2000);
                                    }
                                    else
                                    {
                                        Error("命执行失败(脚要终止):TEST命令:\r\n" + realData + "\r\n没有期望结果:\r\n" + mParams[devIndex][paramName] + " <=> " + vValue + "\r\n返回的错误值为：" + ErrCode.ToString());
                                        if (cmd.IndexOf("CLEAR") > 1)
                                            SendCommandAndWaitAck((byte)devIndex, "0302", "", out strAck, 2000);
                                        return ErrCode;
                                    }
                                }
                            }
                            else if (cmd.Equals("IF"))//处理方式基本与TEST一样
                            //[1:比较字符串的开始位置] [1:字符串长度] [N:比较的值] [2:命令] [N:数据]
                            {
                                isInIFStatement = true;
                                realData = realData.Replace(" ", "");
                                int vStartIndex = Convert.ToInt32(realData.Substring(0, 2), 16);
                                int vLen = Convert.ToInt32(realData.Substring(2, 2), 16);
                                string vValue = realData.Substring(4, vLen);
                                string strAck;
                                UInt32 sendResult;
                                bool testSuccess = false;
                                sendResult = SendCommandAndWaitAck((byte)devIndex, realData.Substring(4 + vLen, 4), realData.Substring(4 + vLen + 4), out strAck, 2000);
                                if (sendResult == 0)
                                {
                                    mParams[devIndex][paramName] = strAck.Replace(" ", "");
                                    if (mParams[devIndex][paramName].Length >= vStartIndex + vLen)
                                    {
                                        if (mParams[devIndex][paramName].Substring(vStartIndex, vLen).Equals(vValue))
                                            testSuccess = true;
                                    }
                                }
                                if (!testSuccess)
                                {
                                    if (Logger != null)
                                        Logger.Append("\r\n\r\n条件【不成立】\r\n\r\n");
                                    ifStatementIsTrue = false;
                                }
                                else
                                {
                                    if (Logger != null)
                                        Logger.Append("\r\n\r\n条件【成立】\r\n\r\n");
                                    ifStatementIsTrue = true;
                                }
                            }
                            else if (cmd.Equals("ELSE") || cmd.Equals("ENDIF"))
                            {
                            }
                        }
                        else
                        {
                            Error("脚本未能识别格式:\r\n" + line + "\r\n", null);
                            return USER_SCRIPT_ERROR;
                        }

                        line = ReadLine(strScript, ref curPos);
                    }
                }
                catch (Exception e)
                {
                    Error("处理指令:\r\n" + line + "\r\n时出错", e);
                    String strDicInfor = "";

                    foreach (String k in this.mParams[devIndex].Keys)
                        strDicInfor = k + "=" + this.mParams[devIndex][k];
                    Error(strDicInfor);

                    return USER_SCRIPT_ERROR;
                }
                return 0;
            }
            finally
            {
                if (isSynchronized)
                    mutexScript.ReleaseMutex();
            }
        }

        private void Error(string msg,Exception e)
        {
            if ((Logger == null)||((msg==null)&&(e==null)))
                return;
            Logger.Append("======ERROR======   ");
            Logger.Append(DateTime.Now.ToString());            
            if ((msg != null) && (msg != ""))
            {
                Logger.Append("\r\n\tMessage:");
                Logger.Append(msg);
            }
            if (e != null)
            {
                Logger.Append("\r\n--------------------\r\n");
                Logger.Append(e.ToString());
                Logger.Append("\r\n--------------------\r\n\r\n");
            }
            Logger.Append("\r\n======ERROR======\r\n");
        }

        protected bool MotoGetPosition(byte devIndex,int motoIndex, out int position)
        {
            string cmd = "0237";
            position = 0;
            StringBuilder hexData = new StringBuilder();
            hexData.Append(string.Format("{0:X002}", motoIndex));
            String hexRData;
            if (SendCommandAndWaitAck(devIndex, cmd, hexData.ToString(), out hexRData, 800) == 0)
            {
                position = int.Parse(Common.ConvertHexToString(hexRData, true));
                return true;
            }
            return false;
        }

        protected uint MotoGoToPosition(byte devIndex,int motoIndex, int position, Boolean lockMotor, Boolean waitForOk)
        {
            StringBuilder hexData = new StringBuilder();
            int currentPosition;

            if (this.MotoGetPosition(devIndex,motoIndex, out currentPosition))
            {
                StringBuilder script = new StringBuilder();
                if (position != currentPosition)
                {
                    int step = position - currentPosition;
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
                //##---          在【0x14秒】内等待所有条件处理完成【超时终止脚本】
                if (waitForOk)
                    script.Append("1=TEST(1E 00 02 00 0303 00)\r\n");
                script.Append("1=DELAY(200)\r\n");
                uint r = RunScript(0, script.ToString());
                return 0;
            }
            return 9;
        }

        private void Error(Exception e)
        {
            Error(null, e);
        }

        private void Error(String msg)
        {
            Error(msg, null);
        }

        private void Debug(string msg)
        {
            if (Logger!=null)
                Logger.Append(DateTime.Now+":"+msg+"\r\n");
        }

        public static void WriteLog(string msg)
        {
            StreamWriter streamWriter = null;
            string logPath = "";

            try
            {
                logPath  = "C:\\FKJLog";
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                logPath += string.Format(@"\{0}.log", DateTime.Now.ToString("yyyy-mm-dd"));
                if (streamWriter == null)
                {
                    streamWriter = !File.Exists(logPath) ? File.CreateText(logPath) : File.AppendText(logPath);
                }
                streamWriter.WriteLine("***************************************************************");
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
                streamWriter.WriteLine("输出错误信息");
                if (msg != null)
                {
                    streamWriter.WriteLine("异常信息：\r\n" + msg);
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }

        public static string ConfigurationManager { get; set; }
    }
}
