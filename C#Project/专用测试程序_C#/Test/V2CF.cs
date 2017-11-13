using System;
using System.Collections.Generic;
using System.Text;

namespace MotorControlBoard
{
    public class V2CF
    {
        
        static PushCardDevice currentDev = null;
        static int currentPort = 0;
        public static int OpenDev(int comPort)
        {
            if (currentDev != null && comPort == currentPort)
                return 0;
            if (currentDev != null)
            {
                currentDev.Close();
                currentDev = null;
                currentPort = 0;
            }
            PushCardDevice pcd = new PushCardDevice(comPort, 19200);
            try
            {
                pcd.Open();
                pcd.RtsEnable = true;
                pcd.DtrEnable = true;
                pcd.Parity = System.IO.Ports.Parity.Even;
                pcd.BreakState = false;
                currentDev = pcd;
                currentPort = comPort;
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public static int PortClose()
        {
            if (currentDev != null)
            {
                currentDev.Close();
                currentDev = null;
                currentPort = 0;
            }
            return 0;
        }

        public static int SendData(string cmd, byte[] sdata, out int rstate, out byte[] rdata)
        {
            byte[] tmpData = currentDev.SendV2CFCmd(cmd, sdata);
            rstate = 0;
            rdata = new byte[0];
            if (tmpData.Length > 0)
            {
                if (tmpData[2] != 0x50)//P
                {
                    rstate |= 0xF00;                    
                }
                rstate |= Convert.ToInt32(((char)tmpData[5]).ToString() + ((char)tmpData[6]).ToString(), 16);
                int dataLen = tmpData.Length-10;
                if (dataLen > 0)
                {
                    for (int i = 7; i < tmpData.Length - 10; i++)
                    {
                        if (tmpData[i] == 0x10)
                        {
                            dataLen--;
                            i++;
                        }
                    }
                    rdata = new byte[dataLen];
                    int tmpIndex = 7;
                    for (int i = 0; i < dataLen; i++)
                    {
                        rdata[i] = tmpData[tmpIndex];
                        if (tmpData[tmpIndex] == 0x10)
                            tmpIndex++;
                        tmpIndex++;
                    }
                }
                return 0;
            }
            return -1;
        }
    }
}
