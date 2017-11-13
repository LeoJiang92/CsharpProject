using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace MotorControlBoard
{
    public class TYRM3000
    {
        #region dll函数

        const string DLL_NAME = "TYRM3000DLL.dll";

        //打开端口
        //pszPort:如"COM1"
        //pszBuildCommDCB: 如"COM1:9600,N,8,1"
        [DllImport(DLL_NAME)]
        private static extern int PortOpen(byte[] pszPort, byte[] pszBuildCommDCB);
        
        public static int OpenDev(int comPort)
        {
            string name = "COM" + comPort;
            return PortOpen(Encoding.Default.GetBytes(name), Encoding.Default.GetBytes(name + ":9600,N,8,1"));
        }

        //关闭串口
        [DllImport(DLL_NAME)]
        public static extern int PortClose();

        //**************************************发送指令通用函数(磁卡都用此进行操作,除了命令参数，没有其它能数)
        //  无数据包指令接口，主要包括磁卡指令
        //  addr_mac: 读卡器的地址
        //  command: 命令字
        //  para: 命令参数
        //          (命令字和命令参数具体见<<TYRM-3000通讯协议>>)
        //  pszReadData: 从串口收到的数据存放的地址指针(接收数据缓冲区)
        //  nReadLen: 数组pszReadData的长度(接收数据缓冲区大小)
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长，一般设置为0
        [DllImport(DLL_NAME)]
        public static extern int SendData(char addr_mac, char command, char para, byte[] pszReadData, int nReadLen, ref int dwRead, int nTimeWaite);

        //设置设备地址
        //  addr_mac: 读卡器的地址
        //  command:命令字
        //  para：命令参数
        //  addr: 待待设置的设备地址
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int SetCardReaderAddr(char addr_mac, char command, char para, char addr, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  允许进卡
        //  addr_mac: 读卡器的地址
        //  iTime：允许进卡时间，以S为单位
        //  nTimeWaite: 串口接收数据等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int InsertCardEnable(char addr_mac, int iTime, int nTimeWaite);

        //****************************************CPU卡SAM卡******************************************************
        //IC卡上下电
        //  addr_mac: 读卡器的地址
        //  command:命令字
        //  para：命令参数
        //  mid：IC卡座编号
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_CPUSAMPowerOnOrDown(char addr_mac, char command, char para, char mid, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //IC卡发送APDU指令
        //  addr_mac: 读卡器的地址
        //  command:命令字
        //  para：命令参数
        //  mid：IC卡座编号
        //  pszSendData:发送apdu指令数据区；
        //  nSendLen:发送apdu指令数据字节数；
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_CPUSAMSendAPDU(char addr_mac, char command, char para, char mid, byte[] pszSendData, int nSendLen, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  非接触式IC卡上电复位
        //  addr_mac: 读卡器的地址
        //  cType:卡类型选择， ='A'为：TYPE-A Mifare pro，='B'为：TYPE-B
        //  mid：IC卡座编号
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFCPUSAMPowerOn(char addr_mac, char cType, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  非接触式IC卡发送APDU指令
        //  addr_mac: 读卡器的地址
        //  pszSendData:发送apdu指令数据区；
        //  nSendLen:发送apdu指令数据字节数；
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFCPUSAMSendAPDU(char addr_mac, byte[] pszSendData, int nSendLen, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //读SIM卡卡号
        //  addr_mac: 读卡器的地址
        //  command:命令字
        //  para：命令参数
        //  mid：IC卡座编号
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_CPUSAMReadICCID(char addr_mac, char command, char para, char mid, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //*****************************************MF1卡**********************************************************
        //  MF1卡读卡模式
        //nMode=0,内置读卡器读卡(默认); /nMode=1,外置读卡器读卡
        [DllImport(DLL_NAME)]
        public static extern int TY_MFReadMode(int nMode);

        //  读卡类型设置
        //  addr_mac: 读卡器的地址
        //  cType:读卡类型；cType='A'： 读ISO14443-A类卡(具体参照：TYRM-3000通讯协议_V22.doc)
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFSelectCardType(char addr_mac, char cType, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  选卡并读卡
        //  addr_mac: 读卡器的地址
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFSelectAndRead(char addr_mac, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  验证密码
        //  addr_mac: 读卡器的地址
        //  blockID：块号
        //  keyType:密码类型
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFAuthentication(char addr_mac, char blockID, char keyType, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  读卡的指定块
        //  addr_mac: 读卡器的地址
        //  blockID：块号
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFReadBlockData(char addr_mac, char blockID, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  写数据到指定块
        //  addr_mac: 读卡器的地址
        //  blockID：块号
        //  pszSendData:待写数据，一般为16字节
        //  nSendLen：待写数据长度，一般为16
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFWriteBlockData(char addr_mac, char blockID, byte[] pszSendData, int nSendLen, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  复位读卡器
        //  addr_mac: 读卡器的地址
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFReset(char addr_mac, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  对卡的指定的值块扣值
        //  addr_mac: 读卡器的地址
        //  blockID：块号
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFDeduValue(char addr_mac, char blockID, byte[] pszValue, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        //  对卡的指定的值块充值
        //  addr_mac: 读卡器的地址
        //  blockID：块号
        //  pszValue: 待充值数据，4字节
        //  pszReadData: 从串口收到的数据存放的地址指针
        //  nReadLen: 数组pszReadData的长度
        //  dwRead: 从串口收到的数据的长度
        //  nTimeWaite: 等待接收的预计超时时长
        [DllImport(DLL_NAME)]
        public static extern int TY_MFAddValue(char addr_mac, char blockID, byte[] pszValue, byte[] pszReadData, int nReadLen, ref IntPtr dwRead, int nTimeWaite);

        #endregion
    }
}
