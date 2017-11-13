using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;

namespace MotorControlBoard
{
    public class PushCardDevice : SerialPort
    {
        public class V2CFChecker : IUsartDataChecker
        {
            public int CheckData(byte[] data, int len)
            {
                if (len < 7)
                    return -1;
                if (data[0] == 0x10 && data[1] == 0x02 && data[len - 1 - 2] == 0x10 && data[len - 1 - 1] == 0x03)
                    return 0;
                return -2;
            }
        }
        public PushCardDevice(int index, int baudRate)
            : base("COM" + index.ToString(), baudRate, Parity.None, 8, StopBits.One)
        {

        }

        ~PushCardDevice()
        {
            this.Close();
        }

        public byte[] ReadData(int timeoutMS, int maxLen)
        {
            return ReadData(timeoutMS, 50, maxLen, null);
        }

        public byte[] ReadData(int timeoutMS, int intervalMS, int maxLen, IUsartDataChecker checker)
        {
            int oldByteToRead = -1;
            int byteToReaded = 0;
            byte[] rdata = new byte[256];
            int cnt = 0;
            int interval = -15;
            while (cnt < timeoutMS)
            {
                if (BytesToRead != 0)
                {
                    int len = BytesToRead;
                    this.Read(rdata, byteToReaded, len);
                    byteToReaded += len;
                    interval = -15;
                }
                if (oldByteToRead != byteToReaded)
                {
                    oldByteToRead = byteToReaded;
                    if (checker != null && checker.CheckData(rdata, byteToReaded) == 0)
                        break;
                }
                else
                {
                    interval += 15;
                    if (interval > intervalMS && byteToReaded > 0)
                        break;
                }
                if (byteToReaded >= maxLen)
                    break;
                System.Threading.Thread.Sleep(15);
                cnt += 15;
                Application.DoEvents();
            }
            if (byteToReaded > 0 && checker != null)
            {
                if (checker.CheckData(rdata, byteToReaded) != 0)
                    return new byte[0];
            }
            if (byteToReaded > 0)
            {
                byte[] result = new byte[byteToReaded];
                for (int i = 0; i < byteToReaded; i++)
                    result[i] = rdata[i];
                return result;
            }
            return new byte[0];
        }

        byte sendPackageIndex = 1;

        public byte[] SendTy900Cmd(string cmd)
        {
            if (cmd.Length < 2)
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            byte[] sendData = new byte[8];
            byte[] recData = new byte[10];
            sendData[0] = 0x02;
            sendData[1] = (byte)cmd[0];
            sendData[2] = (byte)cmd[1];
            sendData[3] = 0x30;
            sendData[4] = 0x30;
            sendData[5] = sendPackageIndex++;
            sendData[6] = 0x03;
            sendData[7] = (byte)(sendData[0] ^ sendData[1] ^ sendData[2] ^ sendData[3] ^ sendData[4] ^ sendData[5] ^ sendData[6] ^ sendData[7]);
            Write(sendData, 0, 8);
            try
            {
                byte[] rdata = ReadData(250, 100);
                return rdata;
            }
            catch (TimeoutException e)
            {
                return new byte[0];
            }
        }

        V2CFChecker v2cfChecker = new V2CFChecker();
        public byte[] SendV2CFCmd(string cmd, byte[] optionData)
        {
            if (cmd.Length < 2)
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            int addLen = 0;
            if (optionData != null)
            {
                addLen = optionData.Length;
                for (int i = 0; i < optionData.Length; i++)
                    if (optionData[i] == 0x10)
                        addLen++;
            }
            byte[] sendData = new byte[addLen + 6 + 2];
            sendData[0] = 0x10;
            sendData[1] = 0x02;
            sendData[2] = 0x43;//"C"
            sendData[3] = (byte)cmd[0];
            sendData[4] = (byte)cmd[1];
            byte crc = (byte)(0x03 ^ sendData[2] ^ sendData[3] ^ sendData[4]);
            int currentIndex = 5;
            if (optionData != null)
            {
                for (int i = 0; i < optionData.Length; i++)
                {
                    sendData[currentIndex++] = optionData[i];
                    crc ^= optionData[i];
                    if (optionData[i] == 0x10)
                    {
                        sendData[currentIndex++] = 0x10;
                    }
                }
            }
            sendData[currentIndex++] = 0x10;
            sendData[currentIndex++] = 0x03;
            sendData[currentIndex++] = crc;
            Write(sendData, 0, currentIndex);
            try
            {
                byte[] rdata = ReadData(250, 2);
                if (rdata.Length == 2 && rdata[0] == 0x10 && rdata[1] == 0x06)
                {
                    Write(new byte[] { 0x10, 0x05 }, 0, 2);
                    rdata = ReadData(4000, 50, 255, v2cfChecker);
                    return rdata;
                }
                return new byte[0];
            }
            catch (TimeoutException e)
            {
                return new byte[0];
            }
        }


        //TCD-820M
        public byte[] SendTCD820M(string cmd)
        {
            if (cmd.Length < 2)
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            byte[] sendData = new byte[5];
            byte[] recData = new byte[10];
            sendData[0] = 0x02;
            sendData[1] = (byte)cmd[0];
            sendData[2] = (byte)cmd[1];
            sendData[3] = 0x03;
            sendData[4] = (byte)(sendData[0] ^ sendData[1] ^ sendData[2] ^ sendData[3]);
            Write(sendData, 0, 5);
            try
            {
                byte[] rdata = ReadData(1000, 250, 100, null);
                if ((rdata.Length > 0) && (rdata[0] == 0x06))
                {
                    Write(new byte[] { 0x05 }, 0, 1);
                    if ("RF".Equals(cmd))
                    {
                        rdata = ReadData(25, 8);
                        return rdata;
                    }
                    else
                        return new byte[1] { 0x06 };
                }
                else
                    return new byte[0];
            }
            catch (TimeoutException e)
            {
                return new byte[0];
            }
        }

        public byte[] SendK720(string cmd)
        {
            if (cmd.Length < 2)
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            byte[] sendData = new byte[cmd.Length + 7];
            byte[] recData = new byte[10];
            int dataIndex = 0;
            sendData[dataIndex++] = 0x02;
            sendData[dataIndex++] = 0x30;
            sendData[dataIndex++] = 0x30;
            sendData[dataIndex++] = 0x00;
            sendData[dataIndex++] = (byte)cmd.Length;
            for (int i = 0; i < cmd.Length; i++)
                sendData[dataIndex++] = (byte)cmd[i];
            sendData[dataIndex++] = 03;
            byte bcc = 0;
            for (int i = 0; i < dataIndex; i++)
            {
                bcc ^= sendData[i];
            }
            sendData[dataIndex++] = bcc;
            Write(sendData, 0, dataIndex);
            try
            {
                byte[] rdata = ReadData(1000, 250, 100, null);
                if ((rdata.Length == 3) && (rdata[0] == 0x06))
                {
                    Write(new byte[] { 0x05, 0x30, 0x30 }, 0, 3);
                    if ("RF".Equals(cmd) || "AP".Equals(cmd))
                    {
                        rdata = ReadData(2000, 100, 20, null);
                    }
                    else
                        return new byte[1] { 0x06 };
                    return rdata;
                }
                else
                    return new byte[0];
            }
            catch (TimeoutException e)
            {
                return new byte[0];
            }
        }

        //TTCE-D2000
        public byte[] SendD2000(string cmd)
        {
            if (cmd.Length < 2)//cmd的最小长度为2
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            byte[] sendData = new byte[cmd.Length + 5];
            byte[] recData = new byte[10];
            int dataIndex = 0;
            sendData[dataIndex++] = 0x02;//STX
            sendData[dataIndex++] = 0x30;//卡机地址高字节
            sendData[dataIndex++] = 0x30;//卡机地址低字节
            //命令
            for (int i = 0; i < cmd.Length; i++)
                sendData[dataIndex++] = (byte)cmd[i];
            sendData[dataIndex++] = 0x03;//ETX
            //BCC
            byte bcc = 0;
            for (int i = 0; i < dataIndex; i++)
                bcc ^= sendData[i];
            sendData[dataIndex++] = bcc;
            Write(sendData, 0, dataIndex);//主机发送COMMAND
            try
            {
                byte[] rdata = ReadData(1000, 250, 100, null);
                byte[] rdata1 = new byte[7];
                if (rdata[0] == 0x06)//ACK
                {
                    if ("AP".Equals(cmd))//如果是查询，函数返回接收到的数据
                    {
                        Array.Copy(rdata, 3, rdata1, 0, rdata.Length - 3);
                        return rdata1;//返回接收到的数据
                    }
                    else
                        return new byte[1] { 0x06 };            
                }
                else
                    return new byte[0];//ACK回应不对返回0
            }
            catch (Exception ee)
            {
                return new byte[0];
            }
        }


        //TTCE-D1801
        public byte[] SendD1801(string cmd)
        {
            if (cmd.Length < 2)//cmd的最小长度为2
            {
                return new byte[0];
            }
            this.DiscardInBuffer();
            byte[] sendData = new byte[cmd.Length + 5];
            byte[] recData = new byte[10];
            int dataIndex = 0;
            sendData[dataIndex++] = 0x02;//STX
            sendData[dataIndex++] = 0x30;//卡机地址高字节
            sendData[dataIndex++] = 0x30;//卡机地址低字节
            //命令
            for (int i = 0; i < cmd.Length; i++)
                sendData[dataIndex++] = (byte)cmd[i];
            sendData[dataIndex++] = 0x03;//ETX
            //BCC
            byte bcc = 0;
            for (int i = 0; i < dataIndex; i++)
                bcc ^= sendData[i];
            sendData[dataIndex++] = bcc;
            Write(sendData, 0, dataIndex);//主机发送COMMAND
            try
            {
                byte[] rdata = ReadData(1000, 250, 100, null);
                if ((rdata.Length == 3) && rdata[0] == 0x06)//ACK
                {
                    Write(new byte[] { 0x05, 0x30, 0x30 }, 0, 3);//ENQ
                    if ("RF".Equals(cmd) || "AP".Equals(cmd))//如果是查询，函数返回接收到的数据
                        rdata = ReadData(2000, 100, 20, null);
                    else
                        return new byte[1] { 0x06 };
                    return rdata;//返回接收到的数据
                }
                else
                    return new byte[0];//ACK回应不对返回0
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
    }
}
