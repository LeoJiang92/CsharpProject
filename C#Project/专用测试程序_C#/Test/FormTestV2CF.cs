using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MotorControlBoard
{
    public partial class FormTestV2CF : Form
    {
        public FormTestV2CF()
        {
            InitializeComponent();
        }

        private void cbTestCard_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTestCard.Checked)
            {
                V2CF.OpenDev(int.Parse(cbICReaderPort.Text));
                int rstate;
                byte[] rdata1;
                int r = V2CF.SendData("01", null, out rstate, out rdata1);
                Thread.Sleep(1000);
                if (MessageBox.Show("是否清空原有统计?", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    totalCount = 0;
                    successCount = 0;
                }
            }
            else
            {
                Loger("停止测试\r\n");
            }
            timer2.Enabled = cbTestCard.Checked;
        }

        int totalCount = 0;
        int successCount = 0;
        PushCardDevice pcd1 = null;
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            if (pcd1 == null)
            {
                pcd1 = new PushCardDevice(int.Parse(cbTKQPort.Text), 9600);
                pcd1.Open();
            }
            int rstate;
            byte[] rdata1;
            totalCount++;
            int r = V2CF.SendData(":0", null, out rstate, out rdata1);
            if (r == 0 && rstate == 0)
            {
                byte[] rdata = pcd1.Send1801("DC");
                if (rdata.Length == 1 && rdata[0] == 0x06)
                {
                    Thread.Sleep(1000);
                    r = V2CF.SendData("61", null, out rstate, out rdata1);
                    if (r == 0)
                    {
                        if (rstate == 2)
                        {
                            successCount++;
                            textBox1.Text += successCount.ToString().PadLeft(5) + "/" + totalCount.ToString().PadRight(5) + "读磁:R=" + rstate + "," + UsbDeviceManager.ConvertBytesToHexString(rdata1, (uint)rdata1.Length, "") + "\r\n";
                        }
                        else
                        {
                            textBox1.Text += successCount.ToString().PadLeft(5) + "/" + totalCount.ToString().PadRight(5) + "-----读磁失败 state=" + rstate + "\r\n";
                            int tryTimes = 0;
                            for (tryTimes = 0; tryTimes < 3; tryTimes++)
                            {
                                r = V2CF.SendData("61", null, out rstate, out rdata1);
                                if (r == 0 && rstate == 2)
                                {
                                    Loger("*重试第" + tryTimes + "次成功\r\n");
                                }
                            }
                        }
                        V2CF.SendData("01", null, out rstate, out rdata1);
                    }
                    else
                    {
                        Loger("-----读磁失败\r\n");
                        V2CF.SendData("01", null, out rstate, out rdata1);
                        if (cbStopWhenFailure.Checked)
                            cbTestCard.Checked = false;
                    }
                }
                else
                {
                    Loger("*推卡失败" + UsbDeviceManager.ConvertBytesToHexString(rdata, (uint)rdata.Length, " ") + "\r\n");
                    cbTestCard.Checked = false;
                    return;
                }
            }
            else
            {
                Loger("*准备入卡失败R=" + r.ToString() + ",RSTATE=" + rstate.ToString() + "\r\n");
                cbTestCard.Checked = false;
                return;
            }

            timer2.Enabled = cbTestCard.Checked;
        }

        public void Loger(string str)
        {
            textBox1.Text += str;
            textBox1.SelectionStart = textBox1.Text.Length-1;
            textBox1.SelectionStart = textBox1.Text.Length - 1;
            textBox1.SelectionLength = 0;
            textBox1.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
