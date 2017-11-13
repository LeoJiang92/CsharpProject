using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace MotorControlBoard
{
    public class SerialDeviceManager : UsbDeviceManager
    {
        public SerialDeviceManager()
            : base(0, 0)
        {

        }

        public override uint OpenDev(byte portNum)
        {
            CloseDev();
            return open_device(portNum, ref mDevHandle);
        }

        public void OpenDebug()
        {
            enable_device_debug(mDevHandle, 1);
        }

        public override void CloseDev()
        {
            if (mDevHandle != 0)
                close_device(mDevHandle);
            mDevHandle = 0;
        }

        private UInt32 mDevHandle = 0;
        [DllImport(UsbDeviceManager.dllPath)]
        private static extern byte open_device(byte portNum, ref UInt32 handle);

        [DllImport(UsbDeviceManager.dllPath)]
        private static extern void close_device(uint handle);

        [DllImport(UsbDeviceManager.dllPath)]
        private static extern UInt32 send_command_wait_ack_16(UInt32 handle, ushort packageNum, ushort scmd, byte state, byte[] psdata, ushort slen, ref ushort prpackageNum, byte[] prdata, ref ushort prlen, UInt32 WaitTime);

        [DllImport(UsbDeviceManager.dllPath)]
        private static extern void enable_device_debug(UInt32 handle, byte isDebug);

        byte count = 0;
        public override UInt32 SendCommandAndWaitAck(byte devIndex, string hexCMD, string hexData, out string hexRData, UInt32 waitMs)
        {
            UInt32 result = 1;
            UInt16 cmdCode = (UInt16)Convert.ToInt16(hexCMD, 16);
            UInt16 packageNum = 0, rdataLen = 0;
            byte[] pdata = ConvertHexStringToBytes(hexData);
            byte[] prdata = new byte[0x200];
            UInt32 rDataLen;
            DateTime st = DateTime.Now;
            int whileCount = 0;
            //count++;
            //if (count == 0xaa)
            //    count++;
            //result = send_command_wait_ack_16(mDevHandle, (ushort)(count << 8), cmdCode, 0, pdata, (ushort)pdata.Length, ref packageNum, prdata, ref rdataLen, (uint)waitMs);
            //if (result == 2 || result == 0x0B)
            //{
            //    WriteLog("    出错命令：" + hexCMD + "\r\n    出错数据：" + (hexData.Equals("") ? "无" : hexData) + "\r\n    命执行失败：返回值为0x"
            //            + String.Format("{0:X0004}", result) + "(" + result + ")"
            //            + "\r\n    第" + (whileCount - 1).ToString() + "次重发该数据");
            //}
            do//重发机制，当result等于2时，为没有接收到数据，重新发一次(返回0x0b为数据出错)
            {
                whileCount++;
                count++;
                if (count == 0xaa)
                    count++;
                result = send_command_wait_ack_16(mDevHandle, (ushort)(count << 8), cmdCode, 0, pdata, (ushort)pdata.Length, ref packageNum, prdata, ref rdataLen, (uint)waitMs);
                if (result == 2 || result == 0x0B)
                {
                    WriteLog("    出错命令：" + hexCMD + "\r\n    出错数据：" + (hexData.Equals("") ? "无" : hexData) + "\r\n    命执行失败：返回值为0x"
                            + String.Format("{0:X0004}", result) + "(" + result + ")"
                            + "\r\n    第" + (whileCount - 1).ToString() + "次重发该数据");
                }
                if (whileCount == 3)
                    break;
            } while (result == 2 || result == 0x0B);
            DateTime et = DateTime.Now;
            rDataLen = rdataLen;
            if (Logger != null)
            {
                Logger.Append(devIndex.ToString() + " SEND:[0x" + hexCMD + "] (" + hexData + ")\r\n");
            }
            hexRData = "";
            if (result == 0)
            {
                if (rDataLen != 0)
                    hexRData = ConvertBytesToHexString(prdata, rDataLen, " ");
                //result = state;
            }
            else
            {
                if (rDataLen != 0)
                    hexRData = ConvertBytesToHexString(prdata, rDataLen, " ");
            }
            if (Logger != null)
                Logger.Append(devIndex.ToString() + " REC :[0x" + String.Format("{0:X004}", result) + "," + result + "] (" + hexRData + ")\r\n---------用时：" + (et.Ticks - st.Ticks).ToString() + "毫秒----------\r\n\r\n");
            return result;
        }

        /// <summary>
        ///日志记录
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            StreamWriter streamWriter = null;
            string logPath = "";

            try
            {
                logPath = "C:\\FKJLog";
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                logPath += string.Format(@"\{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
                if (streamWriter == null)
                {
                    streamWriter = !File.Exists(logPath) ? File.CreateText(logPath) : File.AppendText(logPath);
                }
                streamWriter.WriteLine("--------------------------------输出信息--------------------------------");
                streamWriter.WriteLine("    输出时间：" + DateTime.Now.ToString("HH:mm:ss"));
                if (msg != null)
                {
                    streamWriter.WriteLine(msg);
                }
                streamWriter.WriteLine("    ---------------------------------------------------------------");
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
    }
}
