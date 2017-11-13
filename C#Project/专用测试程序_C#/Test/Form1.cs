using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data;
using System.Drawing;
using System.IO;

using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace MotorControlBoard
{
    public partial class Form1 : Form
    {

        UsbDeviceManager udM;

        #region 常量



        private ushort vid
        {
            get
            {
                string s = this.textBox_vid.Text;
                if (String.IsNullOrEmpty(s))
                {
                    s = "0";
                }
                return ushort.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
        }

        private ushort pid
        {
            get
            {
                string s = this.textBox_pid.Text;
                if (String.IsNullOrEmpty(s))
                {
                    s = "0";
                }
                return ushort.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
        }


        /// <summary>
        /// 设备索引
        /// </summary>
        private string devIndex
        {
            get
            {
                string s = this.textBox_devIndex.Text;
                if (string.IsNullOrEmpty(s))
                {
                    s = "0";
                }
                return s;
            }

        }


        /// <summary>
        /// 为load状态（在load里设置）
        /// </summary>
        private bool IsLoad = false;


        /// <summary>
        /// 方案目录
        /// </summary>
        private string FangAn_MuLu = Common.Get_FangAn_MuLu();

        #endregion


        #region 自定义方法

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="info"></param>
        private void ShowInfo(string info)
        {
            info = DateTime.Now.ToString("hh:mm:ss") + "：\r\n" + info;
            this.textBox1.Text += info;
            //this.textBox1.Focus();//获取焦点
            this.textBox1.Select(this.textBox1.TextLength, 0);//光标定位到文本最后
            this.textBox1.ScrollToCaret();
        }

        /// <summary>
        /// 发送执行命令
        /// （成功返回0，否则返回错误代码）
        /// </summary>
        /// <param name="hexCMD">命令</param>
        /// <param name="hexData">数据</param>
        /// <param name="hexRData">返回接收数据</param>
        private uint SendCommand(string hexCMD, string hexData, out string hexRData)
        {
            if (udM == null)
            {
                MessageBox.Show("设备未打开");
                hexRData = "";
                return 9999;
            }

            udM.Logger = new StringBuilder();

            byte devIndex = Convert.ToByte(this.devIndex);

            uint waitMs = 2000;
            uint r = udM.SendCommandAndWaitAck(devIndex, hexCMD, hexData, out hexRData, waitMs);
            ShowInfo(udM.Logger.ToString());
            return r;

        }


        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="script"></param>
        private uint ExecScript(string script)
        {

            if (udM == null)
            {
                MessageBox.Show("设备未打开");
                return 0xFF;
            }
            if (String.IsNullOrEmpty(script))
            {
                MessageBox.Show("脚本为空。");
                return 0xFF;
            }

            udM.Logger = new StringBuilder();

            int devIndex = int.Parse(this.textBox_devIndex.Text.Trim());


            uint r = udM.RunScript(devIndex, script);
            ShowInfo(udM.Logger.ToString());
            return r;
            /// 变量值=SEND:命令 数据
            /// 变量值=DELAY:xxx毫秒
            /// 变量值=WAIT:命令 数据
        }




        /// <summary>
        /// 获取条件脚本的选项卡的当前活动listview
        /// </summary>
        /// <returns></returns>
        private ListView Get_TiaoJianScript_Tab_Action_ListView()
        {
            ListView lv = tabControl_tj_script.SelectedTab.Controls[0] as ListView;   //因为只有一个控件
            return lv;
        }

        /// <summary>
        /// 添加条件到 listView
        /// </summary>
        /// <param name="tiaojian"></param>
        private void Add_TiaoJian_To_ListView()
        {



        }

        /// <summary>
        /// 从listview里获取 条件脚本的命令
        /// </summary>
        /// <param name="getCurrentRow">获取当前行的数据？反之获取全部</param>
        /// <returns></returns>
        private List<string> Get_TiaoJian_Script_Cmd_FormListView()
        {
            List<string> cmdList = new List<string>();
            ListView lv = tabControl_tj_script.SelectedTab.Controls[0] as ListView;   //因为只有一个控件

            //string fengefu = "- ";  //有空格

            for (int i = 0; i < lv.Items.Count; i++)
            {
                //string lvText = lv.Items[i].Text;  //获取文本
                //int fengefu_index = lvText.IndexOf(fengefu) + 1; //分隔符的位置，这里+1主要是去掉分隔符
                //cmdList.Add(lvText.Substring(fengefu_index, lvText.Length - fengefu_index));
                ////MessageBox.Show(hexData);
                cmdList.Add(Get_TiaoJian_Script_Cmd_FormListView_Index(lv, i));
            }

            return cmdList;

        }

        /// <summary>
        /// 获取条件脚本命令从 ListView指定的索引
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="currentItemIndex"></param>
        /// <returns></returns>
        private string Get_TiaoJian_Script_Cmd_FormListView_Index(ListView lv, int currentItemIndex)
        {
            string fengefu = "- ";  //有空格
            string lvText = lv.Items[currentItemIndex].Text;  //获取文本
            int fengefu_index = lvText.IndexOf(fengefu) + 1; //分隔符的位置，这里+1主要是去掉分隔符
            return lvText.Substring(fengefu_index, lvText.Length - fengefu_index).Trim();
        }

        #endregion


        #region 下拉列表框绑定


        /// <summary>
        /// 下拉列表框绑定
        /// </summary>
        private void DropDownBind()
        {

            List<string> sourceList = new List<string>();

            #region 条件类别

            sourceList.Clear();  //清除
            sourceList.Add("传感状态:0" + (int)Common.UintCmd.ChuanGanQi_TiaoJian_Cmd);
            sourceList.Add("电机:0" + (int)Common.UintCmd.DianJi_TiaoJian_Cmd);
            sourceList.Add("传感次数:0" + (int)Common.UintCmd.ChuanGanQiTimes_TiaoJian_Cmd);

            Common.ComboBoxBind(this.comboBox_tiaojian_type, sourceList);

            TiaoJian_Status_DropBind();  //绑定条件状态

            #endregion

            #region 注释
            //#region 传感状态

            //sourceList.Clear();  //清除
            //sourceList.Add("变高状态:10");
            //sourceList.Add("变低状态:11");
            //sourceList.Add("升降状态:01");

            //Common.ComboBoxBind(this.comboBox_tiaojian_status, sourceList);

            //#endregion

            //#region 电机状态

            //sourceList.Clear();  //清除
            //sourceList.Add("停止状态:00");
            //sourceList.Add("启动状态:01");

            //Common.ComboBoxBind(this.comboBox_tj_dianji_stauts, sourceList);

            //#endregion
            #endregion

            #region 执行动作

            sourceList.Clear();  //清除
            sourceList.Add("执行停止:00");
            sourceList.Add("执行启动:01");
            sourceList.Add("直接停止:" + (int)Common.UintCmd.ZhiJi_TingZhi_Cmd);
            sourceList.Add("直接启动:" + (int)Common.UintCmd.ZhiJie_QiDong_Cmd);

            Common.ComboBoxBind(this.comboBox_dianji_dz, sourceList);


            #endregion

            #region 转动方向

            sourceList.Clear();  //清除
            sourceList.Add("向前:00");
            sourceList.Add("向后:01");

            Common.ComboBoxBind(this.comboBox_dz_dianji_fangxiang, sourceList);

            #endregion

            #region 是否锁住

            sourceList.Clear();  //清除
            sourceList.Add("松开:00");
            sourceList.Add("锁死:01");

            Common.ComboBoxBind(this.comboBox_dz_stop_status, sourceList);

            #endregion


        }


        /// <summary>
        /// 条件状态绑定
        /// </summary>
        private void TiaoJian_Status_DropBind()
        {
            List<string> sourceList = new List<string>();

            if (this.comboBox_tiaojian_type.SelectedValue == null)
            {
                return;
            }

            if (int.Parse(this.comboBox_tiaojian_type.SelectedValue.ToString())
                == (int)Common.UintCmd.ChuanGanQi_TiaoJian_Cmd)  //传感器
            {
                sourceList.Clear();  //清除
                sourceList.Add("变高状态:10");
                sourceList.Add("变低状态:01");
                sourceList.Add("升降状态:11");
            }
            else if (int.Parse(this.comboBox_tiaojian_type.SelectedValue.ToString())
                == (int)Common.UintCmd.DianJi_TiaoJian_Cmd)  //电机
            {
                sourceList.Clear();
                sourceList.Add("停止状态:00");
                sourceList.Add("启动状态:01");
            }
            else
            {
                sourceList.Clear();
                for (int i = 0; i < 200; i++)
                {
                    sourceList.Add(String.Format("{0:d}:{0:X002}", i));
                }
            }

            Common.ComboBoxBind(this.comboBox_tiaojian_status, sourceList);

        }


        #endregion

        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
       
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            this.IsLoad = true;  //设置标志
            DropDownBind();
            tabControl2.SelectedIndex = 4;
            dianji_controls11.SetLockOnStop = true;
            dianji_controls12.SetLockOnStop = true;
            cbTKDeviceType.SelectedIndex = 0;
            cbSFMotorAlamAvailableState.SelectedIndex = 0;
            cbAutoChangeCardSlotY.SelectedIndex = 1;
            cbPutCardType.SelectedIndex = 0;
            cbGetCardType.SelectedIndex = 2;
            cbDevType.SelectedIndex = 0;
            cbAutoCheckJDQ_List.SelectedIndex = 0;
            this.IsLoad = false;  //设置标志，放在最后    

        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_open_Click(object sender, EventArgs e)
        {
            if (udM == null)
            {
                udM = new UsbDeviceManager(vid, pid);
            }
            else
            {
                udM.CloseDev();
                udM = new UsbDeviceManager(vid, pid);
                udM.ReflashDeviceList(vid, pid);
            }

            this.label_device_count.Text = udM.GetDevicesCount().ToString();
        }


        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_close_Click(object sender, EventArgs e)
        {
            if (udM != null)
                udM.CloseDev();
            udM = new UsbDeviceManager(0, 0);  //给错误的端口，让其执行释放
            udM = null;
            this.label_device_count.Text = "0";
            lbSerialOpenState.Text = "未打开";
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_exec_cmd_Click(object sender, EventArgs e)
        {

            byte devIndex = Convert.ToByte(this.devIndex);

            string hexCMD = this.textBox_cmd.Text;
            string hexData = this.textBox_data.Text;
            string hexRData = "";

            this.SendCommand(hexCMD, hexData, out hexRData);

        }


        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_exec_script_Click(object sender, EventArgs e)
        {
            ExecScript(this.textBox_script.Text);
        }


        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            ((TextBox)sender).Text = "";
        }


        /// <summary>
        /// 获取传感器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_get_chuanganqi_Click(object sender, EventArgs e)
        {
            string hexCMD = "0204";
            string hexData = "";
            string hexRData = null;
            string hexRDataCnt = null;

            this.SendCommand("0401", hexData, out hexRDataCnt);
            uint r = this.SendCommand(hexCMD, hexData, out hexRData);
            if (r != 0)
            {
                checkBox_auto_chuanganqi.Checked = false;
                return;
            }
            if (String.IsNullOrEmpty(hexRData) == false)
            {
                string[] cgqS = hexRData.Trim().Split(' ');  //去掉空格
                string[] cgqCnt = hexRDataCnt.Trim().Split(' ');
                btnRemoveScript.Items.Clear();
                string str = "";
                for (int i = 0; i < cgqS.Length; i++)
                {
                    if (i % 4 == 0 && i != 0)
                        str += "\r\n";
                    btnRemoveScript.Items.Add(i, Convert.ToBoolean(int.Parse(cgqS[i])));
                    Int16 cnt = Convert.ToInt16(cgqCnt[i * 2 + 1], 16);
                    cnt <<= 8;
                    cnt |= Convert.ToInt16(cgqCnt[i * 2], 16);
                    str += "【" + i.ToString().PadLeft(2) + "】传感:" + (cgqS[i].Equals("01") ? "1" : "0") + ",变化:" + cnt.ToString().PadRight(10) + "    ";
                }
                ShowInfo(str);
                //textBox1.Text += str;
            }
        }



        /// <summary>
        /// 选择条件类型时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_tiaojian_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoad == false) //不是加载时
            {
                TiaoJian_Status_DropBind();  //绑定条件状态
            }

        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_add_tiaojian_Click(object sender, EventArgs e)
        {
            //描述
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            StringBuilder tiaojian_miaoshu = new StringBuilder();  //条件描述
            StringBuilder tiaojian_cmd = new StringBuilder();  //条件命令
            int dianji_dongzuo = int.Parse(this.comboBox_dianji_dz.SelectedValue.ToString());  //电机动作

            if (dianji_dongzuo == (int)Common.UintCmd.ZhiJi_TingZhi_Cmd
                || dianji_dongzuo == (int)Common.UintCmd.ZhiJie_QiDong_Cmd)   //直接停止/启动
            {
                tiaojian_cmd.Append("00 00 00 ");
            }
            else
            {
                if (tcConditionType.SelectedIndex == 0)//普通条件
                {
                    if (this.comboBox_tiaojian_type.SelectedIndex < 2)
                    {
                        tiaojian_cmd.Append(this.comboBox_tiaojian_type.SelectedValue.ToString().PadLeft(2, '0') + " ");  //条件类型
                        tiaojian_cmd.Append(textBox_tiaojian_index.Text.Trim() + " ");   //条件索引
                        tiaojian_cmd.Append(this.comboBox_tiaojian_status.SelectedValue.ToString() + " ");  //条件状态
                    }
                    else//次数条件
                    {
                        tiaojian_cmd.Append(this.comboBox_tiaojian_type.SelectedValue.ToString().PadLeft(2, '0') + " ");  //条件类型
                        tiaojian_cmd.Append(textBox_tiaojian_index.Text.Trim() + " ");   //条件索引
                        tiaojian_cmd.Append(this.comboBox_tiaojian_status.SelectedValue.ToString() + "00 ");  //条件状态
                    }
                }
                else//蒙板条件
                {
                    tiaojian_cmd.Append("03 ");
                    if (cbCdtIsNot.Checked)
                        tiaojian_cmd.Append("01 ");
                    else
                        tiaojian_cmd.Append("00 ");
                    if (cbCgqMaskCdtJustOnChange.Checked)
                        tiaojian_cmd.Append("01 ");
                    else
                        tiaojian_cmd.Append("00 ");
                    ulong mask = 0;
                    ulong v = 0;
                    for (int i = 0; i < 48; i++)
                    {
                        if (clbCgqMask.GetItemCheckState(i) == CheckState.Checked)
                        {
                            mask |= ((ulong)1 << i);
                            if (clbCgqValue.GetItemCheckState(i) == CheckState.Checked)
                                v |= ((ulong)1 << i);
                        }
                    }
                    tiaojian_cmd.Append(String.Format("{0:X}", mask & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 8) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 16) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 24) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 32) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 40) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 48) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (mask >> 56) & 0xFF).PadLeft(2, '0') + " ");

                    tiaojian_cmd.Append(String.Format("{0:X}", v & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 8) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 16) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 24) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 32) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 40) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 48) & 0xFF).PadLeft(2, '0'));
                    tiaojian_cmd.Append(String.Format("{0:X}", (v >> 56) & 0xFF).PadLeft(2, '0') + " ");
                }
            }

            //条件命令
            if (edSendCmd.Text != "")
            {
                tiaojian_cmd.Append("02");
                tiaojian_cmd.Append(edSendCmd.Text.Substring(2, 2));
                tiaojian_cmd.Append(edSendCmd.Text.Substring(0, 2));
                String strData = edSendCmd_Data.Text.Replace(" ", "");
                tiaojian_cmd.Append(String.Format("{0:X}", strData.Length / 2).PadLeft(2, '0'));
                tiaojian_cmd.Append(strData);
                edSendCmd.Text = "";
                edSendCmd_Data.Text = "";
            }
            else
            {
                tiaojian_cmd.Append(this.comboBox_dianji_dz.SelectedValue.ToString() + " ");
                tiaojian_cmd.Append(this.textBox_dz_dianji_index.Text.Trim() + " ");
                tiaojian_cmd.Append(this.comboBox_dz_stop_status.SelectedValue.ToString() + " ");
                tiaojian_cmd.Append(this.comboBox_dz_dianji_fangxiang.SelectedValue.ToString() + " ");
                tiaojian_cmd.Append(Common.ConvertStringToHex(int.Parse(this.textBox_dz_dianji_bushu.Text.Trim())));  //步数
            }

            sp.AddScriptLine("A=S(0301 " + tiaojian_cmd.ToString() + ")");
        }


        /// <summary>
        /// 运行条件脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_exec_tj_script_Click(object sender, EventArgs e)
        {
            //描述
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            ExecScript(sp.GetScripts());
        }

        /// <summary>
        /// 清除下位机脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_clear_tj_script_Click(object sender, EventArgs e)
        {
            string hexRData = "";
            this.SendCommand("0302", "", out hexRData);
        }


        /// <summary>
        /// 查看动作条件的状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ck_dz_status_Click(object sender, EventArgs e)
        {
            string hexCmd = "0303";
            string hexData = Common.BuCongStr_To_String(this.textBox_ck_dz_tiaojian_index.Text.Trim(),
                2, "0", true);
            if (String.IsNullOrEmpty(hexData))
            {
                MessageBox.Show("请输入" + label15.Text);
                return;
            }
            string hexRData = "";
            this.SendCommand(hexCmd, hexData, out hexRData);
        }


        /// <summary>
        /// 启动/关闭 自动运行条件脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_auto_exec_tj_script_CheckedChanged(object sender, EventArgs e)
        {
            string auto_exec_tj_script_sec = this.textBox_auto_exec_tj_script_sec.Text;
            if (String.IsNullOrEmpty(auto_exec_tj_script_sec))
            {
                auto_exec_tj_script_sec = "5";  //若为空，则默认5
                this.textBox_auto_exec_tj_script_sec.Text = auto_exec_tj_script_sec;
            }
            timer_auto_exec_tj_script.Interval = int.Parse(auto_exec_tj_script_sec) * 1000;
            timer_auto_exec_tj_script.Enabled = checkBox_auto_exec_tj_script.Checked;  //启动定时器

        }

        /// <summary>
        /// 自动运行条件脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_auto_exec_tj_script_Tick(object sender, EventArgs e)
        {
            //描述
            timer_auto_exec_tj_script.Enabled = false;
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            uint r = ExecScript(sp.GetScripts());
            if (checkBox_error_is_go.Checked == false)  //出错继续为false
            {
                if (r != 0)  //执行不成功
                {
                    this.checkBox_auto_exec_tj_script.Checked = false;  //关闭自动运行
                    checkBox_auto_exec_tj_script_CheckedChanged(sender, e);
                }
            }
            timer_auto_exec_tj_script.Enabled = this.checkBox_auto_exec_tj_script.Checked;
        }

        /// <summary>
        /// 保存 设置的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_set_config_save_Click(object sender, EventArgs e)
        {


        }

        private void dianji_controls1_MotorActionEvent(object sender, EventArgs ee)
        {
            MotorEventArgs e = (MotorEventArgs)ee;
            Control ctl = (Control)sender;
            String hexRData;
            if (e.Tag == 1)//点了RESET
            {
                // if ("0A".Equals(ctl.Tag.ToString()))
                this.SendCommand("0218", "00", out hexRData);
                //else if ("0B".Equals(ctl.Tag.ToString()))
                //     this.SendCommand("0218", "01", out hexRData);
                return;
            }
            else if (e.JustQueryMotorStep)
            {
                string cmd = "0237";
                StringBuilder hexData = new StringBuilder();
                hexData.Append(ctl.Tag.ToString().PadLeft(2, '0'));
                this.SendCommand(cmd, hexData.ToString(), out hexRData);
                MessageBox.Show(hexRData + "【" + Common.ConvertHexToString(hexRData, true) + "】");
            }
            else
            {
                string cmd = "0206";  //转动命令
                StringBuilder hexData = new StringBuilder();
                hexData.Append(ctl.Tag.ToString().PadLeft(2, '0'));
                if (e.LockOnStop)  //锁定
                {
                    hexData.Append("01");
                }
                else
                {
                    hexData.Append("00");
                }

                if (e.IsRun)
                {
                    cmd = "0205";
                    if (e.IsLeft)  //向左转
                    {
                        hexData.Append("00");
                    }
                    else  //向右转
                    {
                        hexData.Append("01");
                    }
                    hexData.Append(Common.ConvertStringToHex(e.PwmCnt));
                    this.SendCommand("0217", "", out hexRData);
                }
                this.SendCommand(cmd, hexData.ToString(), out hexRData);
            }
        }

        private void button_clear_tiaojian_Click(object sender, EventArgs e)
        {
            //描述
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            if (DialogResult.Yes == MessageBox.Show("是否清空脚本", "询问", MessageBoxButtons.YesNo))
                sp.ClearScript();
        }

        private int mScriptIndex = 2;
        private void btnAddScript_Click(object sender, EventArgs e)
        {
            tabControl_tj_script.TabPages.Add("脚本" + (mScriptIndex++).ToString());
            ScriptPanel sp = new ScriptPanel();
            TabPage tp = tabControl_tj_script.TabPages[tabControl_tj_script.TabPages.Count - 1];
            tp.Controls.Add(sp);
            sp.Left = 0;
            sp.Top = 0;
            sp.Width = tp.Width;
            sp.Height = tp.Height;
            sp.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            tabControl_tj_script.SelectedIndex = tabControl_tj_script.TabPages.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((tabControl_tj_script.TabCount > 1) && (tabControl_tj_script.TabIndex < tabControl_tj_script.TabCount))
            {
                tabControl_tj_script.TabPages.RemoveAt(tabControl_tj_script.TabIndex);
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void btnAddCdtAllCdtComplete_Click(object sender, EventArgs e)
        {
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            string act = "TEST";
            if (cbTestContinue.Checked)
            {
                act += "_CONTINUE";
                if (cbClearScript.Checked)
                    act += "_CLEAR";
            }
            sp.AddScriptLine("1=" + act + "(" + String.Format("{0:X}", (int)nudWaitTimeOut.Value).PadLeft(2, '0') + " 00 02 00 0303 00)");

        }

        private void btnAddIfStatement_Click(object sender, EventArgs e)
        {
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            ushort mask = 0;
            ushort v = 0;
            for (int i = 0; i < 16; i++)
            {
                if (clbCgqMask.GetItemCheckState(i) == CheckState.Checked)
                {
                    mask |= (ushort)(1 << i);
                    if (clbCgqValue.GetItemCheckState(i) == CheckState.Checked)
                        v |= (ushort)(1 << i);
                }
            }
            string strMask = String.Format("{0:X}", mask & 0xFF).PadLeft(2, '0') + String.Format("{0:X}", mask >> 8).PadLeft(2, '0');
            string strV = String.Format("{0:X}", v & 0xFF).PadLeft(2, '0') + String.Format("{0:X}", v >> 8).PadLeft(2, '0');
            //00 04 XXXX 0208 XXXX
            sp.AddScriptLine("1=IF(00 04 " + strV + " 0208 " + strMask + ")");
            sp.AddScriptLine("1=ELSE()");
            sp.AddScriptLine("1=ENDIF()");
        }

        private void tm_auto_cgq_Tick(object sender, EventArgs e)
        {
            tm_auto_cgq.Enabled = false;
            button_get_chuanganqi_Click(sender, e);
            tm_auto_cgq.Enabled = checkBox_auto_chuanganqi.Checked;
        }

        private void checkBox_auto_chuanganqi_CheckStateChanged(object sender, EventArgs e)
        {
            tm_auto_cgq.Enabled = checkBox_auto_chuanganqi.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ScriptPanel sp = (ScriptPanel)tabControl_tj_script.SelectedTab.Controls[0];
            if (tbParamName.Text.Replace(" ", "").Equals(""))
            {
                MessageBox.Show("请输入变量名称");
                tbParamName.Focus();
                return;
            }
            sp.AddScriptLine(tbParamName.Text.Replace(" ", "") + "=SET(" + tbParamValue.Text + ")");
        }

        private bool ReadedConfig = false;
        private void btnReadSetting_Click(object sender, EventArgs e)
        {
            string hexRData;
            uint r = this.SendCommand("0801", "", out hexRData);
            if (r != 0)
            {
                MessageBox.Show("读取配置失败R=0x" + String.Format("{0:X}", 2).PadLeft(2, '0'));
                return;
            }

            byte[] rdata = UsbDeviceManager.ConvertHexStringToBytes(hexRData);
            BoardConfig bc = (BoardConfig)UsbDeviceManager.ByteToStruct(rdata, typeof(BoardConfig));
            this.nudCfgFrequence1.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence2.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence3.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence4.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence5.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence6.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence7.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence8.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence9.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence10.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence11.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence12.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

            this.nudCfgAcceleration1.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration2.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration3.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration4.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration5.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration6.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration7.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration8.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration9.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration10.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration11.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration12.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);

            this.nudCfgTargetFre1.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre2.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre3.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre4.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre5.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre6.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre7.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre8.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre9.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre10.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre11.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre12.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);

            this.IsSf1.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf2.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf3.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf4.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf5.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf6.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf7.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf8.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf9.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf10.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf11.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf12.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);

            this.nudutycycle1.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle2.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle3.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle4.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle5.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle6.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle7.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle8.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle9.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle10.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle11.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle12.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);



            this.nudCfgwaitLoopInKeyEvent.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

            if (bc.SFMotorAlamAvailableState < 2)
                cbSFMotorAlamAvailableState.SelectedIndex = bc.SFMotorAlamAvailableState;
            else
                cbSFMotorAlamAvailableState.SelectedIndex = 2;

            if (bc.SFMotorBreakerAvailableState < 2)
                cbSFMotorBreakerAvailableState.SelectedIndex = bc.SFMotorBreakerAvailableState;
            else
                cbSFMotorBreakerAvailableState.SelectedIndex = 1;
            nudSFMotorPutDownBreakTimeout.Value = (byte)bc.SFMotorPutDownBreakTimeout;
            nudSFMotorPutDownBreakWaitInterval.Value = (byte)bc.SFMotorPutDownBreakWaitInterval;

            this.nudCfgFrequence1.Value = bc.O_Frequency[0];
            this.nudCfgFrequence2.Value = bc.O_Frequency[1];
            this.nudCfgFrequence3.Value = bc.O_Frequency[2];
            this.nudCfgFrequence4.Value = bc.O_Frequency[3];
            this.nudCfgFrequence5.Value = bc.O_Frequency[4];
            this.nudCfgFrequence6.Value = bc.O_Frequency[5];
            this.nudCfgFrequence7.Value = bc.O_Frequency[6];
            this.nudCfgFrequence8.Value = bc.O_Frequency[7];
            this.nudCfgFrequence9.Value = bc.O_Frequency[8];
            this.nudCfgFrequence10.Value = bc.O_Frequency[9];
            this.nudCfgFrequence11.Value = bc.O_Frequency[10];
            this.nudCfgFrequence12.Value = bc.O_Frequency[11];

            this.nudCfgAcceleration1.Value = bc.A_Frequency[0];
            this.nudCfgAcceleration2.Value = bc.A_Frequency[1];
            this.nudCfgAcceleration3.Value = bc.A_Frequency[2];
            this.nudCfgAcceleration4.Value = bc.A_Frequency[3];
            this.nudCfgAcceleration5.Value = bc.A_Frequency[4];
            this.nudCfgAcceleration6.Value = bc.A_Frequency[5];
            this.nudCfgAcceleration7.Value = bc.A_Frequency[6];
            this.nudCfgAcceleration8.Value = bc.A_Frequency[7];
            this.nudCfgAcceleration9.Value = bc.A_Frequency[8];
            this.nudCfgAcceleration10.Value = bc.A_Frequency[9];
            this.nudCfgAcceleration11.Value = bc.A_Frequency[10];
            this.nudCfgAcceleration12.Value = bc.A_Frequency[11];

            this.nudCfgTargetFre1.Value = bc.T_Frequency[0];
            this.nudCfgTargetFre2.Value = bc.T_Frequency[1];
            this.nudCfgTargetFre3.Value = bc.T_Frequency[2];
            this.nudCfgTargetFre4.Value = bc.T_Frequency[3];
            this.nudCfgTargetFre5.Value = bc.T_Frequency[4];
            this.nudCfgTargetFre6.Value = bc.T_Frequency[5];
            this.nudCfgTargetFre7.Value = bc.T_Frequency[6];
            this.nudCfgTargetFre8.Value = bc.T_Frequency[7];
            this.nudCfgTargetFre9.Value = bc.T_Frequency[8];
            this.nudCfgTargetFre10.Value = bc.T_Frequency[9];
            this.nudCfgTargetFre11.Value = bc.T_Frequency[10];
            this.nudCfgTargetFre12.Value = bc.T_Frequency[11];

            this.IsSf1.Checked = (bc.IsSfmotor[0] > 0);
            this.IsSf2.Checked = (bc.IsSfmotor[1] > 0);
            this.IsSf3.Checked = (bc.IsSfmotor[2] > 0);
            this.IsSf4.Checked = (bc.IsSfmotor[3] > 0);
            this.IsSf5.Checked = (bc.IsSfmotor[4] > 0);
            this.IsSf6.Checked = (bc.IsSfmotor[5] > 0);
            this.IsSf7.Checked = (bc.IsSfmotor[6] > 0);
            this.IsSf8.Checked = (bc.IsSfmotor[7] > 0);
            this.IsSf9.Checked = (bc.IsSfmotor[8] > 0);
            this.IsSf10.Checked = (bc.IsSfmotor[9] > 0);
            this.IsSf11.Checked = (bc.IsSfmotor[10] > 0);
            this.IsSf12.Checked = (bc.IsSfmotor[11] > 0);

            this.nudutycycle1.Value = bc.DutyCycle[0];
            this.nudutycycle2.Value = bc.DutyCycle[1];
            this.nudutycycle3.Value = bc.DutyCycle[2];
            this.nudutycycle4.Value = bc.DutyCycle[3];
            this.nudutycycle5.Value = bc.DutyCycle[4];
            this.nudutycycle6.Value = bc.DutyCycle[5];
            this.nudutycycle7.Value = bc.DutyCycle[6];
            this.nudutycycle8.Value = bc.DutyCycle[7];
            this.nudutycycle9.Value = bc.DutyCycle[8];
            this.nudutycycle10.Value = bc.DutyCycle[9];
            this.nudutycycle11.Value = bc.DutyCycle[10];
            this.nudutycycle12.Value = bc.DutyCycle[11];

            this.nudCfgwaitLoopInKeyEvent.Value = Math.Min(20000, bc.WaitLoopInKeyEvent);
            this.nudCfgFrequence1.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence2.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence3.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence4.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence5.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence6.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence7.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence8.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence9.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence10.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence11.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            this.nudCfgFrequence12.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

            this.nudCfgAcceleration1.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration2.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration3.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration4.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration5.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration6.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration7.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration8.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration9.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration10.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration11.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
            this.nudCfgAcceleration12.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);

            this.nudCfgTargetFre1.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre2.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre3.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre4.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre5.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre6.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre7.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre8.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre9.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre10.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre11.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
            this.nudCfgTargetFre12.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);

            this.IsSf1.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf2.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf3.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf4.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf5.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf6.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf7.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf8.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf9.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf10.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf11.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
            this.IsSf12.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);

            this.nudutycycle1.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle2.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle3.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle4.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle5.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle6.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle7.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle8.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle9.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle10.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle11.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
            this.nudutycycle12.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);

            this.nudCfgwaitLoopInKeyEvent.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
            for (int i = 0; i < 12; i++)
            {
                this.clbCfgMotorFX.SetItemChecked(i, bc.motorDir[i] != 0);
            }
            nudCfgXWF0.Value = bc.motorSensor1[0]; nudCfgXWF1.Value = bc.motorSensor1[1];
            nudCfgXWF2.Value = bc.motorSensor1[2]; nudCfgXWF3.Value = bc.motorSensor1[3];
            nudCfgXWF4.Value = bc.motorSensor1[4]; nudCfgXWF5.Value = bc.motorSensor1[5];
            nudCfgXWF6.Value = bc.motorSensor1[6]; nudCfgXWF7.Value = bc.motorSensor1[7];
            nudCfgXWF8.Value = bc.motorSensor1[8]; nudCfgXWF9.Value = bc.motorSensor1[9];
            nudCfgXWF10.Value = bc.motorSensor1[10]; nudCfgXWF11.Value = bc.motorSensor1[11];

            nudCfgXW0.Value = bc.motorSensor0[0]; nudCfgXW1.Value = bc.motorSensor0[1];
            nudCfgXW2.Value = bc.motorSensor0[2]; nudCfgXW3.Value = bc.motorSensor0[3];
            nudCfgXW4.Value = bc.motorSensor0[4]; nudCfgXW5.Value = bc.motorSensor0[5];
            nudCfgXW6.Value = bc.motorSensor0[6]; nudCfgXW7.Value = bc.motorSensor0[7];
            nudCfgXW8.Value = bc.motorSensor0[8]; nudCfgXW9.Value = bc.motorSensor0[9];
            nudCfgXW10.Value = bc.motorSensor0[10]; nudCfgXW11.Value = bc.motorSensor0[11];

            //新的配置项
            nudCfgCtrl0_0.Value = bc.motorControlSensor0[0]; nudCfgCtrl0_1.Value = bc.motorControlSensor0[1];
            nudCfgCtrl0_2.Value = bc.motorControlSensor0[2]; nudCfgCtrl0_3.Value = bc.motorControlSensor0[3];
            nudCfgCtrl0_4.Value = bc.motorControlSensor0[4]; nudCfgCtrl0_5.Value = bc.motorControlSensor0[5];
            nudCfgCtrl0_6.Value = bc.motorControlSensor0[6]; nudCfgCtrl0_7.Value = bc.motorControlSensor0[7];
            nudCfgCtrl0_8.Value = bc.motorControlSensor0[8]; nudCfgCtrl0_9.Value = bc.motorControlSensor0[9];
            nudCfgCtrl0_10.Value = bc.motorControlSensor0[10]; nudCfgCtrl0_11.Value = bc.motorControlSensor0[11];

            nudCfgCtrl1_0.Value = bc.motorControlSensor1[0]; nudCfgCtrl1_1.Value = bc.motorControlSensor1[1];
            nudCfgCtrl1_2.Value = bc.motorControlSensor1[2]; nudCfgCtrl1_3.Value = bc.motorControlSensor1[3];
            nudCfgCtrl1_4.Value = bc.motorControlSensor1[4]; nudCfgCtrl1_5.Value = bc.motorControlSensor1[5];
            nudCfgCtrl1_6.Value = bc.motorControlSensor1[6]; nudCfgCtrl1_7.Value = bc.motorControlSensor1[7];
            nudCfgCtrl1_8.Value = bc.motorControlSensor1[8]; nudCfgCtrl1_9.Value = bc.motorControlSensor1[9];
            nudCfgCtrl1_10.Value = bc.motorControlSensor1[10]; nudCfgCtrl1_11.Value = bc.motorControlSensor1[11];

            nudCfgMotorBrake0.Value = bc.motorBrake1[0]; nudCfgMotorBrake1.Value = bc.motorBrake1[1];
            nudCfgMotorBrake2.Value = bc.motorBrake1[2]; nudCfgMotorBrake3.Value = bc.motorBrake1[3];
            nudCfgMotorBrake4.Value = bc.motorBrake1[4]; nudCfgMotorBrake5.Value = bc.motorBrake1[5];
            nudCfgMotorBrake6.Value = bc.motorBrake1[6]; nudCfgMotorBrake7.Value = bc.motorBrake1[7];
            nudCfgMotorBrake8.Value = bc.motorBrake1[8]; nudCfgMotorBrake9.Value = bc.motorBrake1[9];
            nudCfgMotorBrake10.Value = bc.motorBrake1[10]; nudCfgMotorBrake11.Value = bc.motorBrake1[11];

            nudCfgMotorBrake2_0.Value = bc.motorBrake2[0]; nudCfgMotorBrake2_1.Value = bc.motorBrake2[1];
            nudCfgMotorBrake2_2.Value = bc.motorBrake2[2]; nudCfgMotorBrake2_3.Value = bc.motorBrake2[3];
            nudCfgMotorBrake2_4.Value = bc.motorBrake2[4]; nudCfgMotorBrake2_5.Value = bc.motorBrake2[5];
            nudCfgMotorBrake2_6.Value = bc.motorBrake2[6]; nudCfgMotorBrake2_7.Value = bc.motorBrake2[7];
            nudCfgMotorBrake2_8.Value = bc.motorBrake2[8]; nudCfgMotorBrake2_9.Value = bc.motorBrake2[9];
            nudCfgMotorBrake2_10.Value = bc.motorBrake2[10]; nudCfgMotorBrake2_11.Value = bc.motorBrake2[11];


            ReadedConfig = true;
        }

        private void btnWriteSetting_Click(object sender, EventArgs e)
        {
            byte[] tmpdata = new byte[300];
            if (!ReadedConfig)
            {
                MessageBox.Show("还没有读取当前配置，不能写入配置信息");
                return;
            }
            BoardConfig bc = (BoardConfig)UsbDeviceManager.ByteToStruct(tmpdata, typeof(BoardConfig));
            bc.IsSetCfg = 0x33;
            bc.SFMotorAlamAvailableState = (byte)cbSFMotorAlamAvailableState.SelectedIndex;
            bc.SFMotorBreakerAvailableState = (byte)cbSFMotorBreakerAvailableState.SelectedIndex;
            bc.SFMotorPutDownBreakTimeout = (byte)nudSFMotorPutDownBreakTimeout.Value;
            bc.SFMotorPutDownBreakWaitInterval = (byte)nudSFMotorPutDownBreakWaitInterval.Value;

            bc.O_Frequency[0] = (int)this.nudCfgFrequence1.Value; bc.O_Frequency[1] = (int)this.nudCfgFrequence2.Value;
            bc.O_Frequency[2] = (int)this.nudCfgFrequence3.Value; bc.O_Frequency[3] = (int)this.nudCfgFrequence4.Value;
            bc.O_Frequency[4] = (int)this.nudCfgFrequence5.Value; bc.O_Frequency[5] = (int)this.nudCfgFrequence6.Value;
            bc.O_Frequency[6] = (int)this.nudCfgFrequence7.Value; bc.O_Frequency[7] = (int)this.nudCfgFrequence8.Value;
            bc.O_Frequency[8] = (int)this.nudCfgFrequence9.Value; bc.O_Frequency[9] = (int)this.nudCfgFrequence10.Value;
            bc.O_Frequency[10] = (int)this.nudCfgFrequence11.Value; bc.O_Frequency[11] = (int)this.nudCfgFrequence12.Value;


            bc.T_Frequency[0] = (int)this.nudCfgTargetFre1.Value; bc.T_Frequency[1] = (int)this.nudCfgTargetFre2.Value;
            bc.T_Frequency[2] = (int)this.nudCfgTargetFre3.Value; bc.T_Frequency[3] = (int)this.nudCfgTargetFre4.Value;
            bc.T_Frequency[4] = (int)this.nudCfgTargetFre5.Value; bc.T_Frequency[5] = (int)this.nudCfgTargetFre6.Value;
            bc.T_Frequency[6] = (int)this.nudCfgTargetFre7.Value; bc.T_Frequency[7] = (int)this.nudCfgTargetFre8.Value;
            bc.T_Frequency[8] = (int)this.nudCfgTargetFre9.Value; bc.T_Frequency[9] = (int)this.nudCfgTargetFre10.Value;
            bc.T_Frequency[10] = (int)this.nudCfgTargetFre11.Value; bc.T_Frequency[11] = (int)this.nudCfgTargetFre12.Value;

            bc.A_Frequency[0] = (int)this.nudCfgAcceleration1.Value; bc.A_Frequency[1] = (int)this.nudCfgAcceleration2.Value;
            bc.A_Frequency[2] = (int)this.nudCfgAcceleration3.Value; bc.A_Frequency[3] = (int)this.nudCfgAcceleration4.Value;
            bc.A_Frequency[4] = (int)this.nudCfgAcceleration5.Value; bc.A_Frequency[5] = (int)this.nudCfgAcceleration6.Value;
            bc.A_Frequency[6] = (int)this.nudCfgAcceleration7.Value; bc.A_Frequency[7] = (int)this.nudCfgAcceleration8.Value;
            bc.A_Frequency[8] = (int)this.nudCfgAcceleration9.Value; bc.A_Frequency[9] = (int)this.nudCfgAcceleration10.Value;
            bc.A_Frequency[10] = (int)this.nudCfgAcceleration11.Value; bc.A_Frequency[11] = (int)this.nudCfgAcceleration12.Value;

            bc.IsSfmotor[0] = Convert.ToByte(IsSf1.Checked); bc.IsSfmotor[1] = Convert.ToByte(IsSf2.Checked);
            bc.IsSfmotor[2] = Convert.ToByte(IsSf3.Checked); bc.IsSfmotor[3] = Convert.ToByte(IsSf4.Checked);
            bc.IsSfmotor[4] = Convert.ToByte(IsSf5.Checked); bc.IsSfmotor[5] = Convert.ToByte(IsSf6.Checked);
            bc.IsSfmotor[6] = Convert.ToByte(IsSf7.Checked); bc.IsSfmotor[7] = Convert.ToByte(IsSf8.Checked);
            bc.IsSfmotor[8] = Convert.ToByte(IsSf9.Checked); bc.IsSfmotor[9] = Convert.ToByte(IsSf10.Checked);
            bc.IsSfmotor[10] = Convert.ToByte(IsSf11.Checked); bc.IsSfmotor[11] = Convert.ToByte(IsSf12.Checked);

            bc.DutyCycle[0] = (byte)this.nudutycycle1.Value; bc.DutyCycle[1] = (byte)this.nudutycycle2.Value;
            bc.DutyCycle[2] = (byte)this.nudutycycle3.Value; bc.DutyCycle[3] = (byte)this.nudutycycle4.Value;
            bc.DutyCycle[4] = (byte)this.nudutycycle5.Value; bc.DutyCycle[5] = (byte)this.nudutycycle6.Value;
            bc.DutyCycle[6] = (byte)this.nudutycycle7.Value; bc.DutyCycle[7] = (byte)this.nudutycycle8.Value;
            bc.DutyCycle[8] = (byte)this.nudutycycle9.Value; bc.DutyCycle[9] = (byte)this.nudutycycle10.Value;
            bc.DutyCycle[10] = (byte)this.nudutycycle11.Value; bc.DutyCycle[11] = (byte)this.nudutycycle12.Value;

            bc.WaitLoopInKeyEvent = (int)this.nudCfgwaitLoopInKeyEvent.Value;
            for (int i = 0; i < 12; i++)
            {
                if (this.clbCfgMotorFX.GetItemChecked(i))
                    bc.motorDir[i] = 1;
                else
                    bc.motorDir[i] = 0;
            }
            bc.motorSensor1[0] = (byte)nudCfgXWF0.Value; bc.motorSensor1[1] = (byte)nudCfgXWF1.Value;
            bc.motorSensor1[2] = (byte)nudCfgXWF2.Value; bc.motorSensor1[3] = (byte)nudCfgXWF3.Value;
            bc.motorSensor1[4] = (byte)nudCfgXWF4.Value; bc.motorSensor1[5] = (byte)nudCfgXWF5.Value;
            bc.motorSensor1[6] = (byte)nudCfgXWF6.Value; bc.motorSensor1[7] = (byte)nudCfgXWF7.Value;
            bc.motorSensor1[8] = (byte)nudCfgXWF8.Value; bc.motorSensor1[9] = (byte)nudCfgXWF9.Value;
            bc.motorSensor1[10] = (byte)nudCfgXWF10.Value; bc.motorSensor1[11] = (byte)nudCfgXWF11.Value;

            bc.motorSensor0[0] = (byte)nudCfgXW0.Value; bc.motorSensor0[1] = (byte)nudCfgXW1.Value;
            bc.motorSensor0[2] = (byte)nudCfgXW2.Value; bc.motorSensor0[3] = (byte)nudCfgXW3.Value;
            bc.motorSensor0[4] = (byte)nudCfgXW4.Value; bc.motorSensor0[5] = (byte)nudCfgXW5.Value;
            bc.motorSensor0[6] = (byte)nudCfgXW6.Value; bc.motorSensor0[7] = (byte)nudCfgXW7.Value;
            bc.motorSensor0[8] = (byte)nudCfgXW8.Value; bc.motorSensor0[9] = (byte)nudCfgXW9.Value;
            bc.motorSensor0[10] = (byte)nudCfgXW10.Value; bc.motorSensor0[11] = (byte)nudCfgXW11.Value;

            //新配置项

            bc.motorControlSensor0[0] = (byte)nudCfgCtrl0_0.Value; bc.motorControlSensor0[1] = (byte)nudCfgCtrl0_1.Value;
            bc.motorControlSensor0[2] = (byte)nudCfgCtrl0_2.Value; bc.motorControlSensor0[3] = (byte)nudCfgCtrl0_3.Value;
            bc.motorControlSensor0[4] = (byte)nudCfgCtrl0_4.Value; bc.motorControlSensor0[5] = (byte)nudCfgCtrl0_5.Value;
            bc.motorControlSensor0[6] = (byte)nudCfgCtrl0_6.Value; bc.motorControlSensor0[7] = (byte)nudCfgCtrl0_7.Value;
            bc.motorControlSensor0[8] = (byte)nudCfgCtrl0_8.Value; bc.motorControlSensor0[9] = (byte)nudCfgCtrl0_9.Value;
            bc.motorControlSensor0[10] = (byte)nudCfgCtrl0_10.Value; bc.motorControlSensor0[11] = (byte)nudCfgCtrl0_11.Value;

            bc.motorControlSensor1[0] = (byte)nudCfgCtrl1_0.Value; bc.motorControlSensor1[1] = (byte)nudCfgCtrl1_1.Value;
            bc.motorControlSensor1[2] = (byte)nudCfgCtrl1_2.Value; bc.motorControlSensor1[3] = (byte)nudCfgCtrl1_3.Value;
            bc.motorControlSensor1[4] = (byte)nudCfgCtrl1_4.Value; bc.motorControlSensor1[5] = (byte)nudCfgCtrl1_5.Value;
            bc.motorControlSensor1[6] = (byte)nudCfgCtrl1_6.Value; bc.motorControlSensor1[7] = (byte)nudCfgCtrl1_7.Value;
            bc.motorControlSensor1[8] = (byte)nudCfgCtrl1_8.Value; bc.motorControlSensor1[9] = (byte)nudCfgCtrl1_9.Value;
            bc.motorControlSensor1[10] = (byte)nudCfgCtrl1_10.Value; bc.motorControlSensor1[11] = (byte)nudCfgCtrl1_11.Value;

            bc.motorBrake1[0] = (byte)nudCfgMotorBrake0.Value; bc.motorBrake1[1] = (byte)nudCfgMotorBrake1.Value;
            bc.motorBrake1[2] = (byte)nudCfgMotorBrake2.Value; bc.motorBrake1[3] = (byte)nudCfgMotorBrake3.Value;
            bc.motorBrake1[4] = (byte)nudCfgMotorBrake4.Value; bc.motorBrake1[5] = (byte)nudCfgMotorBrake5.Value;
            bc.motorBrake1[6] = (byte)nudCfgMotorBrake6.Value; bc.motorBrake1[7] = (byte)nudCfgMotorBrake7.Value;
            bc.motorBrake1[8] = (byte)nudCfgMotorBrake8.Value; bc.motorBrake1[9] = (byte)nudCfgMotorBrake9.Value;
            bc.motorBrake1[10] = (byte)nudCfgMotorBrake10.Value; bc.motorBrake1[11] = (byte)nudCfgMotorBrake11.Value;

            bc.motorBrake2[0] = (byte)nudCfgMotorBrake2_0.Value; bc.motorBrake2[1] = (byte)nudCfgMotorBrake2_1.Value;
            bc.motorBrake2[2] = (byte)nudCfgMotorBrake2_2.Value; bc.motorBrake2[3] = (byte)nudCfgMotorBrake2_3.Value;
            bc.motorBrake2[4] = (byte)nudCfgMotorBrake2_4.Value; bc.motorBrake2[5] = (byte)nudCfgMotorBrake2_5.Value;
            bc.motorBrake2[6] = (byte)nudCfgMotorBrake2_6.Value; bc.motorBrake2[7] = (byte)nudCfgMotorBrake2_7.Value;
            bc.motorBrake2[8] = (byte)nudCfgMotorBrake2_8.Value; bc.motorBrake2[9] = (byte)nudCfgMotorBrake2_9.Value;
            bc.motorBrake2[10] = (byte)nudCfgMotorBrake2_10.Value; bc.motorBrake2[11] = (byte)nudCfgMotorBrake2_11.Value;
            int size = 0;
            unsafe
            {
                size = size = Marshal.SizeOf(bc);
            }
            byte[] sdata = UsbDeviceManager.StructToBytes(bc, size);
            string rdata;
            string hexSData = UsbDeviceManager.ConvertBytesToHexString(sdata, (uint)sdata.Length, "");
            uint r = this.SendCommand("0802", hexSData, out rdata);
            if (r != 0)
                MessageBox.Show("写入配置失败R=0x" + String.Format("{0:X}", r).PadLeft(2, '0'));
            else
                MessageBox.Show("执行成功");
        }

        private void checkBox_auto_chuanganqi_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbTestContinue_CheckedChanged(object sender, EventArgs e)
        {
            cbClearScript.Visible = cbTestContinue.Checked;
        }

        private void nudCfgFrequence1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown cmp = (NumericUpDown)sender;
            int freq1 = Math.Max(100, (int)cmp.Value);
            int freq2 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                    if ((freq1 & (1 << (8 * i + j))) != 0)
                        freq2 |= (1 << (8 * (3 - i) + j));
            }

            string hexData = String.Format("{0:X}", freq2).PadLeft(8, '0');
            string hexRData;
            if (cmp == nudCfgFrequence1)
                hexData = "00" + hexData;
            else if (cmp == nudCfgFrequence2)
                hexData = "01" + hexData;
            else if (cmp == nudCfgFrequence3)
                hexData = "02" + hexData;
            else if (cmp == nudCfgFrequence4)
                hexData = "03" + hexData;
            else if (cmp == nudCfgFrequence5)
                hexData = "04" + hexData;
            else if (cmp == nudCfgFrequence6)
                hexData = "05" + hexData;
            else if (cmp == nudCfgFrequence7)
                hexData = "06" + hexData;
            else if (cmp == nudCfgFrequence8)
                hexData = "07" + hexData;
            else if (cmp == nudCfgFrequence9)
                hexData = "08" + hexData;
            else if (cmp == nudCfgFrequence10)
                hexData = "09" + hexData;
            else if (cmp == nudCfgFrequence11)
                hexData = "0A" + hexData;
            else if (cmp == nudCfgFrequence12)
                hexData = "0B" + hexData;

            uint r = this.SendCommand("0202", hexData, out hexRData);
            if (r != 0)
                MessageBox.Show("设置频率失败!");
        }

        private void nudCfgAcceleration1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown cmp = (NumericUpDown)sender;
            int freq1 = Math.Max(100, (int)cmp.Value);
            int freq2 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                    if ((freq1 & (1 << (8 * i + j))) != 0)
                        freq2 |= (1 << (8 * (3 - i) + j));
            }

            string hexData = String.Format("{0:X}", freq2).PadLeft(8, '0');
            string hexRData;
            if (cmp == nudCfgAcceleration1)
                hexData = "00" + hexData;
            else if (cmp == nudCfgAcceleration2)
                hexData = "01" + hexData;
            else if (cmp == nudCfgAcceleration3)
                hexData = "02" + hexData;
            else if (cmp == nudCfgAcceleration4)
                hexData = "03" + hexData;
            else if (cmp == nudCfgAcceleration5)
                hexData = "04" + hexData;
            else if (cmp == nudCfgAcceleration6)
                hexData = "05" + hexData;
            else if (cmp == nudCfgAcceleration7)
                hexData = "06" + hexData;
            else if (cmp == nudCfgAcceleration8)
                hexData = "07" + hexData;
            else if (cmp == nudCfgAcceleration9)
                hexData = "08" + hexData;
            else if (cmp == nudCfgAcceleration10)
                hexData = "09" + hexData;
            else if (cmp == nudCfgAcceleration11)
                hexData = "0A" + hexData;
            else if (cmp == nudCfgAcceleration12)
                hexData = "0B" + hexData;

            uint r = this.SendCommand("0245", hexData, out hexRData);
            if (r != 0)
                MessageBox.Show("设置加速度失败!");
        }



        private void nudCfgTargetFre1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown cmp = (NumericUpDown)sender;
            int freq1 = Math.Max(100, (int)cmp.Value);
            int freq2 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                    if ((freq1 & (1 << (8 * i + j))) != 0)
                        freq2 |= (1 << (8 * (3 - i) + j));
            }

            string hexData = String.Format("{0:X}", freq2).PadLeft(8, '0');
            string hexRData;
            if (cmp == nudCfgTargetFre1)
                hexData = "00" + hexData;
            else if (cmp == nudCfgTargetFre2)
                hexData = "01" + hexData;
            else if (cmp == nudCfgTargetFre3)
                hexData = "02" + hexData;
            else if (cmp == nudCfgTargetFre4)
                hexData = "03" + hexData;
            else if (cmp == nudCfgTargetFre5)
                hexData = "04" + hexData;
            else if (cmp == nudCfgTargetFre6)
                hexData = "05" + hexData;
            else if (cmp == nudCfgTargetFre7)
                hexData = "06" + hexData;
            else if (cmp == nudCfgTargetFre8)
                hexData = "07" + hexData;
            else if (cmp == nudCfgTargetFre9)
                hexData = "08" + hexData;
            else if (cmp == nudCfgTargetFre10)
                hexData = "09" + hexData;
            else if (cmp == nudCfgTargetFre11)
                hexData = "0A" + hexData;
            else if (cmp == nudCfgTargetFre12)
                hexData = "0B" + hexData;

            uint r = this.SendCommand("0246", hexData, out hexRData);
            if (r != 0)
                MessageBox.Show("设置目标速度失败!");
        }
        private void IsSf1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cmp = (CheckBox)sender;

            string hexData, hexRData;
            if (cmp.Checked)
                hexData = "01";
            else
                hexData = "00";

            if (cmp == IsSf1)
                hexData = "00" + hexData;
            else if (cmp == IsSf2)
                hexData = "01" + hexData;
            else if (cmp == IsSf3)
                hexData = "02" + hexData;
            else if (cmp == IsSf4)
                hexData = "03" + hexData;
            else if (cmp == IsSf5)
                hexData = "04" + hexData;
            else if (cmp == IsSf6)
                hexData = "05" + hexData;
            else if (cmp == IsSf7)
                hexData = "06" + hexData;
            else if (cmp == IsSf8)
                hexData = "07" + hexData;
            else if (cmp == IsSf9)
                hexData = "08" + hexData;
            else if (cmp == IsSf10)
                hexData = "09" + hexData;
            else if (cmp == IsSf11)
                hexData = "0A" + hexData;
            else if (cmp == IsSf12)
                hexData = "0B" + hexData;

            uint r = this.SendCommand("0247", hexData, out hexRData);
            if (r != 0)
                MessageBox.Show("设置为伺服电机失败!");
        }


        private void btnClearMotorStep_Click(object sender, EventArgs e)
        {
            string cmd = "0231";
            String hexRData;
            int aa = (int)numMotorClean.Value;
            String hexData = aa.ToString("x").PadLeft(2, '0');
            this.SendCommand(cmd, hexData.ToString(), out hexRData);
        }

        private void btnJDQ0_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string hexRData;
            if (cbJustToggle.Checked)
                this.SendCommand("0212", btn.Tag.ToString(), out hexRData);
            else
                this.SendCommand("0213", btn.Tag.ToString(), out hexRData);
        }

        private bool MotoGetPosition(int motoIndex, out int position)
        {
            string cmd = "0237";
            position = 0;
            StringBuilder hexData = new StringBuilder();
            hexData.Append(string.Format("{0:X002}", motoIndex));
            String hexRData;
            if (this.SendCommand(cmd, hexData.ToString(), out hexRData) == 0)
            {
                position = int.Parse(Common.ConvertHexToString(hexRData, true));
                return true;
            }
            return false;
        }
        private uint MotoGoToPosition(int motoIndex, int position, Boolean lockMotor)
        {
            StringBuilder hexData = new StringBuilder();
            int currentPosition;

            if (this.MotoGetPosition(motoIndex, out currentPosition))
            {
                if (position != currentPosition)
                {
                    int step = position - currentPosition;
                    StringBuilder script = new StringBuilder();
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

                    //##---          在【0x14秒】内等待所有条件处理完成【超时终止脚本】
                    script.Append("1=TEST(14 00 02 00 0303 00)\r\n");
                    script.Append("1=DELAY(200)\r\n");
                    udM.Logger = new StringBuilder();
                    this.ShowInfo(udM.Logger.ToString());
                    return udM.RunScript(0, script.ToString());


                }
                return 0;
            }
            return 9;
        }

        private void btnGoToInLine0_Click(object sender, EventArgs e)
        {

        }

        private Boolean OutCardFlag = false;
        private void btnGotoCardSlot_Click(object sender, EventArgs e)
        {
            uint r;
            OutCardFlag = false;
            Control ctl = (Control)sender;
            r = fkj.MotoGotoCardSlot("in".Equals(ctl.Tag), (int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value, fkj.cfgGotoSlotWaitCompleted == 1);
            if (r == 0)
            {
                OutCardFlag = true;
                MessageBox.Show("移动到卡槽成功！");
            }
            MessageBox.Show("移动到卡槽失败，出错原因是R = " + r.ToString());
        }

        private void btCSGetCardToTemp_Click(object sender, EventArgs e)
        {
            udM.Logger = new StringBuilder();
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                int cgIndex = Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString());
                if (cbAutoOutCard.Checked)//如果是盘库,达到循环盘库
                {
                    if ((nudX_SlotCount.Value <= nudCSGotoIndexX.Value) || (nudY_SlotCount.Value <= nudCSGotoIndexY.Value))
                    {
                        nudCSGotoIndexX.Value = 0;
                        nudCSGotoIndexY.Value = 0;
                    }
                }
                OutputSensorState("取卡到中转【前】");
                uint r = fkj.GetCardToTransferCar((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value);
                OutputSensorState("取卡到中转【后】R=" + String.Format("{0:x}", r));
                if (r == 0)
                    OutCardFlag = true;
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("取卡到中转区【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("取卡到中转区【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);      
                }
                textBox1.Text = udM.Logger.ToString();
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        private void tm_auto_out_card_Tick(object sender, EventArgs e)
        {
            tm_auto_out_card.Enabled = false;
            if (cbAutoChangeCardSlotX.SelectedIndex <= 0 && cbAutoChangeCardSlotY.SelectedIndex <= 0)
            {
                cbAutoOutCard.Checked = false;
                MessageBox.Show("自动执行必须使卡槽自增或自减");
                return;
            }
            Application.DoEvents();
            if (cbGetCardType.SelectedIndex == 5)//快速盘库
            {
                btnFastCheckCardSlot_Click(btnFastCheckCardSlot, null);
                if (!this.OutCardFlag)
                {
                    cbAutoOutCard.Checked = false;
                    return;
                }
            }
            else//不是快速盘库就先将卡从卡槽推进中转机构
            {
                btCSGetCardToTemp_Click(btCSGetCardToTemp, null);
                if (!this.OutCardFlag)
                {
                    cbAutoOutCard.Checked = false;
                    return;
                }
            }
            Application.DoEvents();
            if (cbGetCardType.SelectedIndex == 4)//盘库
            {
                //判断有没有卡
                btCSPutCardToCS_Click(btCSPutCardToCS, null);
                if (!this.OutCardFlag)
                {
                    cbAutoOutCard.Checked = false;
                    return;
                }
            }
            else if (cbGetCardType.SelectedIndex == 3)//直接到废卡
            {
                btCSPutCardToBadSlot_Click(btCSPutCardToBadSlot, null);
                if (!this.OutCardFlag)
                {
                    cbAutoOutCard.Checked = false;
                    return;
                }
            }
            else if ((cbGetCardType.SelectedIndex == 2) || (cbGetCardType.SelectedIndex == 1) || (cbGetCardType.SelectedIndex == 0))
            {
                btCSGetCardToICReader_Click(btCSGetCardToICReader, null);
                if (!this.OutCardFlag)
                {
                    cbAutoOutCard.Checked = false;
                    return;
                }
                if (cbGetCardType.SelectedIndex == 0)
                    cbHoldCard.Checked = false;
                else
                    cbHoldCard.Checked = true;
                Thread.Sleep(300);
                Application.DoEvents();
                switch (cbGetCardType.SelectedIndex)
                {
                    case 2://返回卡槽
                        if (fkj.GetType() != typeof(FKJ500_V2))
                        {
                            btnCSGetCardToUser_Click(btnCSGetCardToUser, null);
                            if (!this.OutCardFlag)
                            {
                                cbAutoOutCard.Checked = false;
                                return;
                            }
                        }
                        else
                        {
                            ((FKJ500_V2)fkj).X = (int)nudCSGotoIndexX.Value;
                        }
                        btCSReturnCardToTemp_Click(btCSReturnCardToTemp, null);
                        if (!this.OutCardFlag)
                        {
                            cbAutoOutCard.Checked = false;
                            return;
                        }
                        btCSPutCardToCS_Click(btCSPutCardToCS, null);
                        break;
                    case 1://进坏卡兜
                        btnCSGetCardToUser_Click(btnCSGetCardToUser, null);
                        if (!this.OutCardFlag)
                        {
                            cbAutoOutCard.Checked = false;
                            return;
                        }
                        btCSReturnCardToTemp_Click(btCSReturnCardToTemp, null);
                        if (!this.OutCardFlag)
                        {
                            cbAutoOutCard.Checked = false;
                            return;
                        }
                        btCSPutCardToBadSlot_Click(btCSPutCardToBadSlot, null);
                        break;
                    case 0:
                        btnCSGetCardToUser_Click(btnCSGetCardToUser, null);
                        break;
                }
            }
            if (!this.OutCardFlag)
            {
                cbAutoOutCard.Checked = false;
                return;
            }
            Application.DoEvents();
            tm_auto_out_card.Enabled = cbAutoOutCard.Checked;
            cbAutoOutCard.Text = (actionCount + int.Parse(cbAutoOutCard.Tag.ToString())).ToString();

            endTime = DateTime.Now;
            double s = endTime.Subtract(startTime).TotalSeconds;
            int uh = (int)(s / 3600);
            int um = (int)(s % 3600 / 60);
            int us = (int)s % 60;
            string str = "用时：" + uh + ":" + um + ":" + us + ",处理:" + actionCount + "次，平均:" + (int)(s / actionCount) + "秒/次";
            txtMsg.Text = "\r\n" + str + "\r\n\r\n";
        }

        private void cbAutoOutCard_CheckedChanged(object sender, EventArgs e)
        {
            cbAutoOutCard.Tag = cbAutoOutCard.Text;
            if (cbAutoOutCard.Checked)
            {
                startTime = DateTime.Now;
                endTime = startTime;
                actionCount = 0;
                if (cbGetCardType.SelectedIndex < 0)
                {
                    MessageBox.Show("必须选择出卡类型");
                    return;
                }
            }
            tm_auto_out_card.Enabled = cbAutoOutCard.Checked;
            if (!cbAutoOutCard.Checked)
            {
                if (endTime.Equals(startTime))
                {
                    endTime = DateTime.Now;
                }
                if (actionCount == 0)
                    actionCount = 1;
                //MessageBox.Show(str);
            }
        }

        private void btCSPutCardToTemp_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            udM.Logger = new StringBuilder();
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                if (chkRecordChange.Checked)
                {
                    StreamWriter sw = File.AppendText("传感跳变次数.txt");
                    sw.WriteLine("\r\n" + DateTime.Now.ToString() + "\t【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】\t" + "入卡到中转机构[前]".PadRight(20) + "\t" + "传感:" + cmbGetSensorName.SelectedItem.ToString() + "变化:" + GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString())).ToString().PadRight(6) + "传感0变化:" + GetSensorCheckNum(0).ToString().PadRight(6) + "传感1变化:" + GetSensorCheckNum(1).ToString().PadRight(6), Encoding.UTF8);
                    sw.Close();
                }
                uint r = fkj.PutCardToTransferCar();
                if (chkRecordChange.Checked)
                {
                    StreamWriter sw = File.AppendText("传感跳变次数.txt");
                    sw.WriteLine("\r\n" + DateTime.Now.ToString() + "\t【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】\t" + "入卡到中转机构[后]".PadRight(20) + "\t" + "传感:" + cmbGetSensorName.SelectedItem.ToString() + "变化:" + GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString())).ToString().PadRight(6) + "传感0变化:" + GetSensorCheckNum(0).ToString().PadRight(6) + "传感1变化:" + GetSensorCheckNum(1).ToString().PadRight(6), Encoding.UTF8);
                    sw.Close();
                }
                if (r == 0)
                {
                    OutCardFlag = true;
                }
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("入卡到中转区失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("入卡到中转区失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
                textBox1.Text = udM.Logger.ToString();
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        DateTime startTime = DateTime.Now;
        DateTime endTime = DateTime.Now;
        int actionCount = 0;
        int actionCount2 = 0;
        private void cbAutoPutCard_CheckedChanged(object sender, EventArgs e)
        {
            cbAutoPutCard.Tag = cbAutoPutCard.Text;
            if (cbAutoPutCard.Checked)
            {
                startTime = DateTime.Now;
                endTime = startTime;
                actionCount = 0;
            }
            tm_auto_in_card.Enabled = cbAutoPutCard.Checked;
            if (!cbAutoPutCard.Checked)
            {

                if (actionCount == 0)
                    actionCount = 1;
                //MessageBox.Show(str);
            }
        }

        private void tm_auto_in_card_Tick(object sender, EventArgs e)
        {
            tm_auto_in_card.Enabled = false;
            if (cbAutoChangeCardSlotX.SelectedIndex <= 0 && cbAutoChangeCardSlotY.SelectedIndex <= 0)
            {
                cbAutoPutCard.Checked = false;
                MessageBox.Show("自动执行必须使卡槽自增或自减");
                return;
            }
            if (cbPutCardType.SelectedIndex == 2)
            {
                btCSPutCardToCS_Click(btCSPutCardToCS, null);
                if (!OutCardFlag)
                {
                    cbAutoPutCard.Checked = false;
                    return;
                }
                nudCSGotoIndexY.Value--;//取消自加
                btCSGetCardToTemp_Click(btCSGetCardToTemp, null);
                if (!OutCardFlag)
                {
                    cbAutoPutCard.Checked = false;
                    return;
                }
                nudCSGotoIndexY.Value++;//现在自加
            }
            else
            {
                btCSPutCardToICReader_Click(btCSPutCardToICReader, null);
                if (!OutCardFlag)
                {
                    cbAutoPutCard.Checked = false;
                    return;
                }
                btCSPutCardToTemp_Click(btCSPutCardToTemp, null);
                if (!OutCardFlag)
                {
                    cbAutoPutCard.Checked = false;
                    return;
                }
                if (cbPutCardType.SelectedIndex == 0)
                    btCSPutCardToCS_Click(btCSPutCardToCS, null);
                else if (cbPutCardType.SelectedIndex == 1)
                    btCSPutCardToBadSlot_Click(btCSPutCardToBadSlot, null);
                if (!OutCardFlag)
                {
                    cbAutoPutCard.Checked = false;
                    return;
                }
            }

            Application.DoEvents();
            tm_auto_in_card.Enabled = cbAutoPutCard.Checked;
            cbAutoPutCard.Text = (actionCount + int.Parse(cbAutoPutCard.Tag.ToString())).ToString();
            endTime = DateTime.Now;
            double s = endTime.Subtract(startTime).TotalSeconds;
            int uh = (int)(s / 3600);
            int um = (int)(s % 3600 / 60);
            int us = (int)s % 60;
            string str = "用时：" + uh + ":" + um + ":" + us + ",处理:" + actionCount + "次，平均:" + (int)(s / actionCount) + "秒/次";
            txtMsg.Text = "\r\n" + str + "\r\n\r\n";
        }

        IFZJ fkj = null;
        private void btnFKQ_Init_Click(object sender, EventArgs e)
        {
            tabControl2.Enabled = false;
            try
            {
                if (udM == null)
                {
                    MessageBox.Show("没有可用的设备");
                    return;
                }
                //if (fkj != null)
                //{
                //    fkj = null;
                //}
                if (fkj == null)
                {
                    if (cbDevType.Text.Equals("FKJ500_V1"))
                    {
                        fkj = new FKJ500_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ500_V2"))
                    {
                        fkj = new FKJ500_V2(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                        //nud_500ParamEnable();
                    }
                    else if (cbDevType.Text.Equals("FKJ500_V3"))
                    {
                        fkj = new FKJ500_V3(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ1000_V1"))
                    {
                        fkj = new FKJ1000_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ1000_V2"))
                    {
                        fkj = new FKJXXX_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                        // nud_1000ParamEnable();
                    }
                    else if (cbDevType.Text.Equals("FKJ1000_V3"))
                    {
                        fkj = new FKJ1000_V3(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ1000_V4"))
                    {
                        fkj = new FKJ1000_V4(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ1000_V2K"))
                    {
                        fkj = new FKJ1000_V2K(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ4000_V1"))
                    {
                        fkj = new FKJ4000_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                    }
                    else if (cbDevType.Text.Equals("FKJ2000_V2"))
                    {
                        fkj = new FKJ2000_V2(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\FKJ1000_V2K\");
                    }
                    else if (cbDevType.Text.Equals("FKJ4000_V2"))
                    {
                        fkj = new FKJ4000_V2(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\FKJ4000_V2\");
                    }
                    else if (cbDevType.Text.Equals("FKJ2IN1_V1"))
                    {
                        fkj = new FKJ2IN1_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\FKJ4000_V2\");
                    }
                    else
                    {
                        fkj = new FKJ2000_V1(udM, 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\" + cbDevType.Text + "\\");
                        //nud_2000ParamEnable();
                    }

                }
                FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                string FilePath = fi2.DirectoryName + "\\Log\\";
                fkj.LogPath = FilePath;
                btnLoadParam_Click(btnLoadParam, null);//发卡机加载参数
                if (cbInited.Checked)
                {
                    uint r = fkj.DevInit();
                    if (r != 0)
                    {
                        // textBox1.Text = udM.Logger.ToString();
                        MessageBox.Show("初始化失败 R=" + r.ToString() + ",0x" + Convert.ToString(r, 16) + "," + fkj.GetErrDescript(r));
                    }
                    else
                    {
                        cbAutoPutCard.Text = "0";
                        cbAutoPutCard.Tag = "0";
                        cbAutoOutCard.Text = "0";
                        cbAutoOutCard.Tag = "0";
                    }
                }
                if (fkj.GetType() == typeof(FKJ2000_V2))
                {
                    cmbDisplayFKJ.Enabled = true;
                    btnReadParam.Enabled = true;
                }
                else
                {
                    cmbDisplayFKJ.Enabled = false;
                    btnReadParam.Enabled = false;
                }
            }
            finally
            {
                tabControl2.Enabled = true;
            }
        }

        private void button_open_serial_Click(object sender, EventArgs e)
        {
            if (udM != null)
            {
                udM.CloseDev();
            }
            udM = new SerialDeviceManager();
            uint r = udM.OpenDev(byte.Parse(textBox_SerialNum.Text));

            if (r == 0)
            {
                ((SerialDeviceManager)udM).OpenDebug();
                lbSerialOpenState.Text = "打开成功";

                string deviceNum;
                string hexRData;
                this.SendCommand("0104", "", out hexRData);
                this.textBox2.Text = "";
                deviceNum = HexToASCII(hexRData);
                this.textBox2.Text = deviceNum;
                MessageBox.Show("设备号：" + deviceNum);
            }
            else
                lbSerialOpenState.Text = "打开失败:R=0x" + String.Format("{0:X}", r) + "," + r.ToString();
        }

        private void btCSPutCardToCS_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            udM.Logger = new StringBuilder();
            int cgIndex = Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString());
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                OutputSensorState("入卡到卡槽【前】");
                uint r = fkj.PutCardToCardSlot((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value);
                OutputSensorState("入卡到卡槽【后】R=" + String.Format("{0:x}", r));
                if (chkAutoGetSensorChange.Checked)
                {
                    txtSensorCheckNum.Text = GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString())).ToString();
                }
                if (r == 0)
                {
                    if (cbAutoChangeCardSlotY.SelectedIndex > 0)
                    {
                        nudCSGotoIndexY.Value = ((int)((nudCSGotoIndexY.Value) + int.Parse(cbAutoChangeCardSlotY.SelectedItem.ToString())));
                        if (cbAutoChangeCardSlotX.SelectedIndex < 0)
                        {
                            if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                            else if (0 > nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                        }
                        if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = 0;
                        }
                        else if (0 > nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = nudY_SlotCount.Value - 1;
                        }
                    }
                    switch (cbAutoChangeCardSlotX.SelectedIndex)
                    {
                        case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % nudX_SlotCount.Value; break;
                        case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 1); break;
                        case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % nudX_SlotCount.Value; break;
                        case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 2); break;
                    }
                    if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                    {
                        if (nudCSGotoIndexX.Value == 0)
                        {
                            nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % nudY_SlotCount.Value;
                            if (nudCSGotoIndexY.Value == 0)
                            {
                                //这里本来要防止入卡放满时重复插入的，这里先不判断
                                //textBox1.Text = udM.Logger.ToString();
                                //return;
                            }
                        }
                    }

                    OutCardFlag = true;
                    actionCount++;
                    actionCount2++;
                }
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("入卡到卡槽【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("入卡到卡槽【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
                textBox1.Text = udM.Logger.ToString();
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }
        int[] sensorCurrentChangeCount = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private void OutputSensorState(string title)
        {
            FileInfo fi2 = new FileInfo(Application.ExecutablePath);
            string FilePath = fi2.DirectoryName + "\\";
            if (chkRecordChange.Checked)
            {
                if (File.Exists(FilePath + "传感跳变次数.txt"))
                {
                    FileInfo fi = new FileInfo(FilePath + "传感跳变次数.txt");
                    if (fi.Length > 1024 * 1024 * 5)
                    {
                        File.Move(FilePath + "传感跳变次数.txt", FilePath + "传感跳变次数" + DateTime.Now.ToString("yyMMddHHmmss") + ".txt");
                    }
                }
                int[] bh0 = GetSensorCheckNum();
                string hexRData;
                this.SendCommand("0204", "", out hexRData);
                string[] v = hexRData.Split(' ');
                int[] scnt0 = fkj.GetSensorStateCount(0);
                int[] scnt1 = fkj.GetSensorStateCount(1);
                StreamWriter sw = File.AppendText(FilePath + "传感跳变次数.txt");
                string str = "\r\n" + DateTime.Now.ToString().PadRight(20) + "【" + nudCSGotoIndexX.Value.ToString().PadRight(2) + "," + nudCSGotoIndexY.Value.ToString().PadRight(2) + "】 " + title;
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
                sensorCurrentChangeCount = bh0;
                sw.WriteLine(str + "\r\n=======\r\n\r\n", Encoding.UTF8);
                sw.Close();
            }
        }
        private void btCSPutCardToICReader_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                udM.Logger = new StringBuilder();
                uint r = fkj.PutCardToICReader();
                if (chkRecordChange.Checked)
                {
                    StreamWriter sw = File.AppendText("传感跳变次数.txt");
                    sw.WriteLine("\r\n" + DateTime.Now.ToString() + "          " + nudCSGotoIndexX.Value.ToString() + "/" + nudCSGotoIndexY.Value.ToString() + "               " + "入卡到读卡器" + "                     " + cmbGetSensorName.SelectedItem.ToString() + "                     " + GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString())).ToString(), Encoding.UTF8);
                    sw.Close();
                }
                if (r == 0)
                    OutCardFlag = true;
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("放卡到IC读卡器失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("放卡到IC读卡器失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
                textBox1.Text = udM.Logger.ToString();
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        private void btCSGetCardToICReader_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                OutputSensorState("取卡到ICReader【前】");
                uint r = fkj.GetCardToICReader();
                OutputSensorState("取卡到ICReader【后】R=" + String.Format("{0:x}", r));
                if (r == 0)
                    OutCardFlag = true;
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("取卡到IC读卡器失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("取卡到IC读卡器失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);               
                }
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        private void btnCSGetCardToUser_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                OutputSensorState("取卡到用户【前】");
                uint r = fkj.GetCardToUser(cbHoldCard.Checked, 10);
                OutputSensorState("取卡到用户【后】R=" + String.Format("{0:x}", r));
                if (r == 0)
                {
                    if (!cbHoldCard.Checked)//夹卡就不能改变卡槽位了
                    {
                        if (cbAutoChangeCardSlotY.SelectedIndex > 0)
                        {
                            nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + int.Parse(cbAutoChangeCardSlotY.SelectedItem.ToString()));
                            if (cbAutoChangeCardSlotX.SelectedIndex < 0)
                            {
                                if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                                {
                                    nudCSGotoIndexX.Value++;
                                }
                                else if (0 > nudCSGotoIndexY.Value)
                                {
                                    nudCSGotoIndexX.Value++;
                                }
                            }
                            if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexY.Value = 0;
                            }
                            else if (0 > nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexY.Value = nudY_SlotCount.Value - 1;
                            }
                        }
                        switch (cbAutoChangeCardSlotX.SelectedIndex)
                        {
                            case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % nudX_SlotCount.Value; break;
                            case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 1); break;
                            case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % nudX_SlotCount.Value; break;
                            case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 2); break;
                        }
                        if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                        {
                            if (nudCSGotoIndexX.Value == 0)
                            {
                                nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % nudY_SlotCount.Value;
                                if (nudCSGotoIndexY.Value == 0)
                                {
                                    textBox1.Text = udM.Logger.ToString();
                                    return;
                                }
                            }
                        }
                        actionCount++;
                    }
                    OutCardFlag = true;
                }
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("取卡到用户失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("取卡到用户失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        private void btnCSGetCardToBadSlot_Click(object sender, EventArgs e)
        {
            OutCardFlag = true;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                uint r = fkj.GetCardToBadCardSlot((uint)nudGetCardToBadSlotParams.Value);
                if (chkRecordChange.Checked)
                {
                    StreamWriter sw = File.AppendText("传感跳变次数.txt");
                    sw.WriteLine("\r\n" + DateTime.Now.ToString() + "          " + nudCSGotoIndexX.Value.ToString() + "/" + nudCSGotoIndexY.Value.ToString() + "                " + "取卡到废卡槽" + "                     " + cmbGetSensorName.SelectedItem.ToString() + "                     " + GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString())).ToString(), Encoding.UTF8);
                    sw.Close();
                }
                if (r != 0)
                {
                    endTime = DateTime.Now;
                    OutCardFlag = false;
                    MessageBox.Show("取卡到坏卡槽失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("取卡到坏卡槽失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);            
                    return;
                }
                actionCount++;
                if ((cbAutoOutCard.Checked || cbAutoPutCard.Checked) && ((actionCount + 1) % 60 == 0))
                    nudGetCardToBadSlotParams.Value = (nudGetCardToBadSlotParams.Value + 1) % 2;
                if (cbAutoChangeCardSlotY.SelectedIndex > 0)
                {
                    nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + int.Parse(cbAutoChangeCardSlotY.SelectedItem.ToString()));
                    if (cbAutoChangeCardSlotX.SelectedIndex < 0)
                    {
                        if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexX.Value++;
                        }
                        else if (0 > nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexX.Value++;
                        }
                    }
                    if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                    {
                        nudCSGotoIndexY.Value = 0;
                    }
                    else if (0 > nudCSGotoIndexY.Value)
                    {
                        nudCSGotoIndexY.Value = nudY_SlotCount.Value - 1;
                    }
                }
                switch (cbAutoChangeCardSlotX.SelectedIndex)
                {
                    case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % nudX_SlotCount.Value; break;
                    case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 1); break;
                    case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % nudX_SlotCount.Value; break;
                    case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 2); break;
                }
                if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                {
                    if (nudCSGotoIndexX.Value == 0)
                    {
                        nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % nudY_SlotCount.Value;
                        if (nudCSGotoIndexY.Value == 0)
                        {
                            textBox1.Text = udM.Logger.ToString();
                            return;
                        }
                    }
                }
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }

        }

        private void btnExportConfig_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
                fkj.SaveToConfigFile(sfd.FileName);
        }

        private void btnImportConfig_Click(object sender, EventArgs e)
        {
            if (fkj == null)
            {
                MessageBox.Show("请先将发证机初始化（选好发证机类型，点击“发证机初始化”按钮即可）");
                return;
            }
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fkj.LoadConfigFile(ofd.FileName);
                DisplayFKJParam(fkj);      //各种串口定义
                //cbICReaderPort.Text = fkj.mInCardReaderPort.ToString();
                //cbTKQPort.Text = fkj.mTKQPort.ToString();
                //cbV2CPort.Text = fkj.mOutCardReaderPort.ToString();

                ////外设类型
                //cbTKDeviceType.SelectedIndex = fkj.cfgTKDeviceType - 1;
                //cmbInReaderType.SelectedIndex = fkj.mInCardReaderType - 1;
                //cmbOutReaderType.SelectedIndex = fkj.mOutCardReaderType - 1;

                ////参数初始化
                //nudX_InCard.Value = fkj.cfgX_InCard;
                //nudX_OutCard.Value = fkj.cfgX_OutCard;
                //nudX_BadSlot.Value = fkj.cfgX_BadSlot;
                //nudX_Slot0.Value = fkj.cfgX_Slot0;
                //nudX_Interval.Value = fkj.cfgX_Interval;
                //nudX_SlotCount.Value = fkj.cfgX_SlotCount;

                //nudICReader_InCard.Value = fkj.cfgICReader_InCard;
                //nudICReader_OutCard.Value = fkj.cfgICReader_OutCard;

                //nudY_InCard.Value = fkj.cfgY_InCard;
                //nudY_OutCard.Value = fkj.cfgY_OutCard;
                //nudY_BadSlot.Value = fkj.cfgY_BadSlot;
                //nudY_Slot0.Value = fkj.cfgY_Slot0;
                //nudY_Interval.Value = fkj.cfgY_Interval;
                //nudY_SlotCount.Value = fkj.cfgY_SlotCount;

                //nudY_AdaptTime.Value = fkj.cfgY_AdaptTime;
                //nudY_AdaptPWM.Value = fkj.cfgY_AdaptPWM;

                //nudAnger0_0.Value = fkj.cfgAnger0_0;//左角度
                //nudAnger0_1.Value = fkj.cfgAnger0_1;//右角度
                //nudPushCardStep.Value = fkj.cfgPushCardStep;
                //nudY_Excursion.Value = fkj.cfgY_Excursion;
                //nudX_Excursion.Value = fkj.cfgX_Excursion;
                //nudY_GetCard_Excursion.Value = fkj.cfgY_GetCard_Excursion;
                //nudY_BadCard_Excursion.Value = fkj.cfgY_BadCard_Excursion;//坏卡槽偏移

                //nudFunctionSwitch.Value = fkj.cfgFunctionSwitch;

                //ShowPositionProperty(fkj);
            }
        }

        private void ShowPositionProperty(IFZJ fkjArg)
        {
            //清空lbPositionConfig控件中的内容
            lbPositionConfig.Items.Clear();
            //将内存中的显示到lbPositionConfig控件上
            for (int i = 0; i < fkjArg.pcfgPositionProperty.Count; i++)
            {
                FKJ_PositionPropertyItem ppi = fkjArg.pcfgPositionProperty.GetItem(i);
                lbPositionConfig.Items.Add(ppi.ToString());
            }
        }

        private void btnLoadParam_Click(object sender, EventArgs e)
        {
            //各种串口定义
            fkj.mInCardReaderPort = int.Parse(cbICReaderPort.Text);
            fkj.mInCardReaderType = (int)cmbInReaderType.SelectedIndex + 1;
            fkj.mTKQPort = int.Parse(cbTKQPort.Text);
            fkj.cfgTKDeviceType = (int)cbTKDeviceType.SelectedIndex + 1;
            fkj.mOutCardReaderPort = int.Parse(cbV2CPort.Text);
            fkj.mOutCardReaderType = (int)cmbOutReaderType.SelectedIndex + 1;

            //参数初始化
            fkj.cfgX_InCard = (int)nudX_InCard.Value;
            fkj.cfgX_OutCard = (int)nudX_OutCard.Value;
            fkj.cfgX_BadSlot = (int)nudX_BadSlot.Value;
            fkj.cfgX_Slot0 = (int)nudX_Slot0.Value;
            fkj.cfgX_Interval = (int)nudX_Interval.Value;
            fkj.cfgX_SlotCount = (int)nudX_SlotCount.Value;

            fkj.cfgICReader_InCard = (int)nudICReader_InCard.Value;
            fkj.cfgICReader_OutCard = (int)nudICReader_OutCard.Value;

            fkj.cfgY_InCard = (int)nudY_InCard.Value;
            fkj.cfgY_OutCard = (int)nudY_OutCard.Value;
            fkj.cfgY_BadSlot = (int)nudY_BadSlot.Value;
            fkj.cfgY_Slot0 = (int)nudY_Slot0.Value;
            fkj.cfgY_Interval = (int)nudY_Interval.Value;
            fkj.cfgY_SlotCount = (int)nudY_SlotCount.Value;


            fkj.cfgY_AdaptTime = (int)nudY_AdaptTime.Value;
            fkj.cfgY_AdaptPWM = (int)nudY_AdaptPWM.Value;

            fkj.cfgAnger0_0 = (int)nudAnger0_0.Value;//左角度
            fkj.cfgAnger0_1 = (int)nudAnger0_1.Value;//右角度
            fkj.cfgPushCardStep = (int)nudPushCardStep.Value;

            fkj.cfgX_Excursion = (int)nudX_Excursion.Value;
            fkj.cfgY_Excursion = (int)nudY_Excursion.Value;
            fkj.cfgY_GetCard_Excursion = (int)nudY_GetCard_Excursion.Value;
            fkj.cfgY_BadCard_Excursion = (int)nudY_BadCard_Excursion.Value;

            fkj.cfgFunctionSwitch = (int)nudFunctionSwitch.Value;

            //清除位置配置列表
            fkj.pcfgPositionProperty.Clear();
            //将位置调节也加载
            for (int i = 0; i < lbPositionConfig.Items.Count; i++)
            {
                //字符串分解
                string[] str = lbPositionConfig.Items[i].ToString().Split(' ');
                int x = Convert.ToInt32(str[0]);
                int y = Convert.ToInt32(str[2]);
                int step = Convert.ToInt32(str[4].Substring(3));
                fkj.pcfgPositionProperty.AddItem(x, y, step);//添加
                //int d = fkj.pcfgPositionProperty.Count;
            }
        }

        private void btCSPutCardToBadSlot_Click(object sender, EventArgs e)
        {
            OutCardFlag = true;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                OutputSensorState("入卡到废卡兜【前】");
                uint r = fkj.PutCardToBadCardSlot((uint)nudGetCardToBadSlotParams.Value);
                OutputSensorState("入卡到废卡兜【前】");
                if (r == 0)
                {
                    if (cbAutoChangeCardSlotY.SelectedIndex > 0)
                    {
                        nudCSGotoIndexY.Value = ((int)((nudCSGotoIndexY.Value) + int.Parse(cbAutoChangeCardSlotY.SelectedItem.ToString())));
                        if (cbAutoChangeCardSlotX.SelectedIndex < 0)
                        {
                            if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                            else if (0 > nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                        }
                        if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = 0;
                        }
                        else if (0 > nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = nudY_SlotCount.Value - 1;
                        }
                    }
                    switch (cbAutoChangeCardSlotX.SelectedIndex)
                    {
                        case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % nudX_SlotCount.Value; break;
                        case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 1); break;
                        case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % nudX_SlotCount.Value; break;
                        case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 2); break;
                    }
                    if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                    {
                        if (nudCSGotoIndexX.Value == 0)
                        {
                            nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % nudY_SlotCount.Value;
                            if (nudCSGotoIndexY.Value == 0)
                            {
                                textBox1.Text = udM.Logger.ToString();
                                return;
                            }
                        }
                    }

                    OutCardFlag = true;
                    actionCount++;
                    actionCount2++;
                    if ((cbAutoOutCard.Checked || cbAutoPutCard.Checked) && ((actionCount + 1) % 60 == 0))
                        nudGetCardToBadSlotParams.Value = (nudGetCardToBadSlotParams.Value + 1) % 2;
                }
                else
                {
                    endTime = DateTime.Now;
                    OutCardFlag = false;
                    MessageBox.Show("取卡到坏卡槽失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("取卡到坏卡槽失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    return;
                }
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }

        private void btCSReturnCardToTemp_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            ((Control)sender).BackColor = Color.Red;
            try
            {
                OutputSensorState("返回中转【前】");
                uint r = fkj.PutCardToTransferCarFromOutput();
                OutputSensorState("返回中转【后】R=" + String.Format("{0:x}", r));
                if (r != 0)
                {
                    MessageBox.Show("返回中转机构失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                    if (chkUseTcp.Checked)
                        ClientSendMsg("返回中转机构失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
                else
                    OutCardFlag = true;
            }
            finally
            {
                ((Control)sender).Enabled = true;
                ((Control)sender).BackColor = Color.Green;
            }
        }


        Thread t1 = null;
        Thread t2 = null;
        private void cbAutoPutCardThread_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAutoPutCardThread.Checked == true)
            {
                if (cbAutoChangeCardSlotX.SelectedIndex <= 0 && cbAutoChangeCardSlotY.SelectedIndex <= 0)
                {
                    cbAutoPutCard.Checked = false;
                    MessageBox.Show("自动执行必须使卡槽自增或自减");
                    return;
                }

                startTime = DateTime.Now;
                actionCount = 0;
                if (t1 == null)
                    t1 = new Thread(autoSendCard);
                if (t2 == null)
                    t2 = new Thread(autoPutCardToCS);
                t1.Start();
                t2.Start();
            }
            else
            {
                if (t1.IsAlive)
                    t1.Abort();
                if (t2.IsAlive)
                    t2.Abort();
                if (endTime.Equals(startTime))
                {
                    endTime = DateTime.Now;
                }
                double s = endTime.Subtract(startTime).TotalSeconds;
                if (actionCount == 0)
                    actionCount = 1;
                string str = "用时：" + (int)s / 60 + "分钟,处理:" + actionCount + "次，平均:" + (int)(s / actionCount) + "秒/次";
                txtMsg.Text = "\r\n" + str + "\r\n\r\n";
                //MessageBox.Show(str);
            }
        }
        private void autoSendCard()
        {
            uint r;
            while (true)
            {
                if (!fkj.CardIsInIcReader())//如果读卡器位置检测不到卡片，发卡入读卡器
                {
                    System.Threading.Thread.Sleep(1000);//等卡完全离开再处理
                    r = fkj.PutCardToICReader();
                    if (r != 0)
                    {
                        MessageBox.Show("autoSendCard:入卡到读卡器执行失败 R=" + r + ",0x" + String.Format("{0:x}", r));
                        return;
                    }
                }
                System.Threading.Thread.Sleep(100);//延时100毫秒
            }
        }

        private void autoPutCardToCS()
        {
            uint r;
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                if ((!fkj.CardIsInTransfer()) && (fkj.CardIsInIcReader()))
                {
                    r = fkj.PutCardToTransferCar();
                    if (r != 0)
                    {
                        MessageBox.Show("autoPutCardToCS:入卡到中转机构执行失败 R=" + r + ",0x" + String.Format("{0:x}", r));
                        return;
                    }
                    else
                    {
                        this.btCSPutCardToCS_Click(btCSPutCardToCS, null);
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }

        private void chbAutoMoveMotor10_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = chbAutoMoveMotor10.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int step = 0;
            timer1.Enabled = false;
            MotoGetPosition(10, out step);
            for (int i = 0; i < 5; i++)
            {
                step += (int)nudMotor10UpStep.Value;
                if (step >= 183 * 250)
                {
                    MotoGoToPosition(10, fkj.cfgY_Slot0, true);
                    MotoGetPosition(10, out step);
                    txtMotor10CurrentStep.Text = step.ToString();
                    i = 0;
                }
                MotoGoToPosition(10, step, true);
                txtMotor10CurrentStep.Text = step.ToString();
                System.Threading.Thread.Sleep(1000);
            }
            MotoGoToPosition(10, step - (int)nudMotor10DownStep.Value, true);
            MotoGetPosition(10, out step);
            txtMotor10CurrentStep.Text = step.ToString();
            System.Threading.Thread.Sleep(1000);
            timer1.Enabled = chbAutoMoveMotor10.Checked;
        }

        public int GetSensorCheckNum(int num)
        {
            string hexOut;
            this.SendCommand("0401", "", out hexOut);
            string[] sensorCheckNum = hexOut.Split(' ');
            return Convert.ToInt32(sensorCheckNum[num * 2 + 1] + sensorCheckNum[num * 2], 16);
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

        public void CleanSensorCheckNum()
        {
            string hexOut;
            this.SendCommand("0402", "", out hexOut);
            this.SendCommand("0406", "", out hexOut);
        }

        private void btnGetSensorCheckNum_Click(object sender, EventArgs e)
        {
            int currentCS = GetSensorCheckNum(Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString()));
            txtSensorCheckNum.Text = currentCS.ToString();
        }

        private void btnCleanSensorCheckNum_Click(object sender, EventArgs e)
        {
            if (btnCleanSensorCheckNum.Equals(sender))
                System.IO.File.Delete("传感跳变次数.txt");
            CleanSensorCheckNum();
        }

        private void btnMotorLoosen_Click(object sender, EventArgs e)
        {
            uint r = fkj.MotorUnlock();
            if (r != 0)
            {
                MessageBox.Show("执行失败 R=" + r);
            }
        }

        //判断各个位置是否有卡
        private void btnGetCardPostion_Click(object sender, EventArgs e)
        {
            //计算当前座标
            string hexRData;
            string cmd = "0237";
            this.SendCommand(cmd, "05", out hexRData);
            byte[] bs = UsbDeviceManager.ConvertHexStringToBytes(hexRData);
            int xStep = BitConverter.ToInt32(bs, 0);

            this.SendCommand(cmd, "0A", out hexRData);
            bs = UsbDeviceManager.ConvertHexStringToBytes(hexRData);
            int yStep = BitConverter.ToInt32(bs, 0);

            if ((xStep >= fkj.cfgX_Slot0) && (yStep >= fkj.cfgY_Slot0) && (fkj.cfgX_Interval != 0) && (fkj.cfgY_Interval != 0))
            {
                int x = (xStep - fkj.cfgX_Slot0) / fkj.cfgX_Interval;
                int y = (yStep - fkj.cfgY_Slot0) / fkj.cfgY_Interval;
                lbPosition.Text = "当前坐标:" + x + "," + y;
            }
            else
                lbPosition.Text = "___";

            chkfkd.Checked = fkj.CardIsReady();
            chkfkdkq.Checked = fkj.CardIsInIcReader();
            chkzzq.Checked = fkj.CardIsInTransfer();
            chkckdkq.Checked = fkj.CardIsInIcReaderOutput();
            chkckk.Checked = fkj.CardIsInOutput();
            this.SendCommand("0204", "", out hexRData);
            string[] v = hexRData.Split(' ');
            chkdscgdk.Checked = "01".Equals(v[8]);

        }

        private void nud_500ParamEnable()
        {
            //X轴
            nudX_InCard.Enabled = true;
            nudX_OutCard.Enabled = true;
            nudX_BadSlot.Enabled = false;
            nudX_Slot0.Enabled = false;
            nudX_Interval.Enabled = true;
            nudX_SlotCount.Enabled = true;

            //Y轴
            nudY_InCard.Enabled = true;
            nudY_OutCard.Enabled = true;
            nudY_BadSlot.Enabled = false;
            nudY_Slot0.Enabled = true;
            nudY_Interval.Enabled = true;
            nudY_SlotCount.Enabled = true;
            nudY_AdaptTime.Enabled = true;
            nudY_AdaptPWM.Enabled = true;

            //角度参数
            nudAnger0_0.Enabled = false;
            nudAnger0_1.Enabled = false;
            nudPushCardStep.Enabled = true;

            //电动读卡器
            nudICReader_InCard.Enabled = false;
            nudICReader_OutCard.Enabled = false;

            //偏移位置
            nudX_Excursion.Enabled = false;
            nudY_Excursion.Enabled = false;
            nudY_GetCard_Excursion.Enabled = true;
        }

        private void nud_1000ParamEnable()
        {
            //X轴
            nudX_InCard.Enabled = true;
            nudX_OutCard.Enabled = true;
            nudX_BadSlot.Enabled = true;
            nudX_Slot0.Enabled = true;
            nudX_Interval.Enabled = true;
            nudX_SlotCount.Enabled = true;

            //Y轴
            nudY_InCard.Enabled = true;
            nudY_OutCard.Enabled = true;
            nudY_BadSlot.Enabled = true;
            nudY_Slot0.Enabled = true;
            nudY_Interval.Enabled = true;
            nudY_SlotCount.Enabled = true;
            nudY_AdaptTime.Enabled = true;
            nudY_AdaptPWM.Enabled = true;

            //角度参数
            nudAnger0_0.Enabled = true;
            nudAnger0_1.Enabled = false;
            nudPushCardStep.Enabled = false;

            //电动读卡器
            nudICReader_InCard.Enabled = false;
            nudICReader_OutCard.Enabled = false;

            //偏移位置
            nudX_Excursion.Enabled = false;
            nudY_Excursion.Enabled = false;
            nudY_GetCard_Excursion.Enabled = false;
        }

        private void nud_2000ParamEnable()
        {
            //X轴
            nudX_InCard.Enabled = true;
            nudX_OutCard.Enabled = true;
            nudX_BadSlot.Enabled = true;
            nudX_Slot0.Enabled = true;
            nudX_Interval.Enabled = true;
            nudX_SlotCount.Enabled = true;

            //Y轴
            nudY_InCard.Enabled = true;
            nudY_OutCard.Enabled = true;
            nudY_BadSlot.Enabled = true;
            nudY_Slot0.Enabled = true;
            nudY_Interval.Enabled = true;
            nudY_SlotCount.Enabled = true;
            nudY_AdaptTime.Enabled = true;
            nudY_AdaptPWM.Enabled = true;

            //角度参数
            nudAnger0_0.Enabled = true;
            nudAnger0_1.Enabled = true;
            nudPushCardStep.Enabled = false;

            //电动读卡器
            nudICReader_InCard.Enabled = false;
            nudICReader_OutCard.Enabled = false;

            //偏移位置
            nudX_Excursion.Enabled = true;
            nudY_Excursion.Enabled = true;
            nudY_GetCard_Excursion.Enabled = false;
        }

        private void cbAutoCheckJDQ_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAutoCheckJDQ.Checked)
            {
                JDQ_CheckList = cbAutoCheckJDQ_List.Text.Split(',');
                if (!bgCheckJDQ.IsBusy)
                {
                    bgCheckJDQ.RunWorkerAsync();
                }
                else
                    cbAutoCheckJDQ.Checked = false;
            }
        }
        string[] JDQ_CheckList = null;
        private void bgCheckJDQ_DoWork(object sender, DoWorkEventArgs e)
        {
            int cnt = 1;
            string hexRData;
            int[] oldCnt = fkj.GetSensorChangeCount();
            while (cbAutoCheckJDQ.Checked)
            {
                //模式一，全部弹出再收回去
                for (int i = 0; ((i < JDQ_CheckList.Length) && cbAutoCheckJDQ.Checked); i++)
                {
                    this.SendCommand("0212", JDQ_CheckList[i], out hexRData);
                    Thread.Sleep(100);
                }
                Thread.Sleep(500);
                for (int i = 0; ((i < JDQ_CheckList.Length) && cbAutoCheckJDQ.Checked); i++)
                {
                    this.SendCommand("0212", JDQ_CheckList[i], out hexRData);
                    Thread.Sleep(100);
                }
                //模式二，逐个开关
                for (int i = 0; ((i < JDQ_CheckList.Length) && cbAutoCheckJDQ.Checked); i++)
                {
                    this.SendCommand("0212", JDQ_CheckList[i], out hexRData);
                    Thread.Sleep(100);
                    this.SendCommand("0212", JDQ_CheckList[i], out hexRData);
                    Thread.Sleep(300);
                }
                int[] newCnt = fkj.GetSensorChangeCount();
                for (int i = 0; i < 15; i++)//15用来处理继电器变化，所以肯定会变
                {
                    if (newCnt[i] != oldCnt[i])
                    {
                        cbAutoCheckJDQ.Checked = false;
                    }
                }
                cbAutoCheckJDQ.Invoke(new EventHandler(delegate
                {
                    cbAutoCheckJDQ.Text = cbAutoCheckJDQ.Tag.ToString() + "," + (cnt++).ToString() + "次";
                }));
                if (cbAutoCheckJDQ.Checked)
                    Thread.Sleep(1000);
            }
            for (int i = 0; (i < JDQ_CheckList.Length); i++)
            {
                this.SendCommand("0211", JDQ_CheckList[i] + "00", out hexRData);
            }
        }

        private void btnAllMotorReset_Click(object sender, EventArgs e)
        {
            string hexOut;
            this.SendCommand("0898", "", out hexOut);
        }

        private void btnMainBoardReset_Click(object sender, EventArgs e)
        {
            string hexOut;
            this.SendCommand("0899", "", out hexOut);
        }

        private void btnFastCheckCardSlot_Click(object sender, EventArgs e)
        {
            udM.Logger = new StringBuilder();
            OutCardFlag = false;
            ((Control)sender).Enabled = false;
            try
            {
                //int cgIndex = Convert.ToInt32(cmbGetSensorName.SelectedItem.ToString());
                if (cbAutoOutCard.Checked)//如果是盘库,达到循环盘库
                {
                    if ((nudX_SlotCount.Value <= nudCSGotoIndexX.Value) || (nudY_SlotCount.Value <= nudCSGotoIndexY.Value))
                    {
                        nudCSGotoIndexX.Value = 0;
                        nudCSGotoIndexY.Value = 0;
                    }
                }
                OutputSensorState("快速盘库【前】");
                uint r = fkj.SlotIsEmpty((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value);
                OutputSensorState("快速盘库【后】R=" + String.Format("{0:x}", r));
                if (r == 0)
                {
                    if (cbAutoChangeCardSlotY.SelectedIndex > 0)
                    {
                        nudCSGotoIndexY.Value = ((int)((nudCSGotoIndexY.Value) + int.Parse(cbAutoChangeCardSlotY.SelectedItem.ToString())));
                        if (cbAutoChangeCardSlotX.SelectedIndex < 0)
                        {
                            if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                            else if (0 > nudCSGotoIndexY.Value)
                            {
                                nudCSGotoIndexX.Value++;
                            }
                        }
                        if (nudY_SlotCount.Value <= nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = 0;
                        }
                        else if (0 > nudCSGotoIndexY.Value)
                        {
                            nudCSGotoIndexY.Value = nudY_SlotCount.Value - 1;
                        }
                    }
                    switch (cbAutoChangeCardSlotX.SelectedIndex)
                    {
                        case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % nudX_SlotCount.Value; break;
                        case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 1); break;
                        case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % nudX_SlotCount.Value; break;
                        case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? nudX_SlotCount.Value : (nudCSGotoIndexY.Value - 2); break;
                    }
                    if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                    {
                        if (nudCSGotoIndexX.Value == 0)
                        {
                            nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % nudY_SlotCount.Value;
                            if (nudCSGotoIndexY.Value == 0)
                            {
                                //这里本来要防止入卡放满时重复插入的，这里先不判断
                                //textBox1.Text = udM.Logger.ToString();
                                //return;
                            }
                        }
                    }

                    OutCardFlag = true;
                    actionCount++;
                    actionCount2++;
                }
                else
                {
                    endTime = DateTime.Now;
                    MessageBox.Show("快速盘库失败【" + nudCSGotoIndexX.Value.ToString() + "," + nudCSGotoIndexY.Value.ToString() + "】失败 R=" + r + ",0x" + String.Format("{0:x}", r) + "," + fkj.GetErrDescript(r) + ",停止时间：" + endTime);
                }
                textBox1.Text = udM.Logger.ToString();
            }
            finally
            {
                ((Control)sender).Enabled = true;
            }
        }

        /// <summary>
        /// 去入卡位置按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToInPosition_Click(object sender, EventArgs e)
        {
            uint r;
            r = this.MotoGoToPosition(10, fkj.cfgY_InCard, false);
            if (r != 0)
            {
                MessageBox.Show("Y轴移动到入卡口失败");
            }
            r = this.MotoGoToPosition(5, fkj.cfgX_InCard, true);
            if (r != 0)
            {
                MessageBox.Show("X轴移动到入卡口失败");
            }
        }

        /// <summary>
        /// 去出卡位置按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToOutPosition_Click(object sender, EventArgs e)
        {
            uint r;
            r = this.MotoGoToPosition(10, fkj.cfgY_OutCard, true);
            if (r != 0)
            {
                MessageBox.Show("Y轴移动到出卡口失败");
            }
            r = this.MotoGoToPosition(5, fkj.cfgX_OutCard, true);
            if (r != 0)
            {
                MessageBox.Show("X轴移动到出卡口失败");
            }
        }

        /// <summary>
        /// 去废卡存放区按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToBreakPosition_Click(object sender, EventArgs e)
        {
            uint r;
            r = this.MotoGoToPosition(10, fkj.cfgY_BadSlot, true);
            if (r != 0)
            {
                MessageBox.Show("Y轴移动到废卡存放区失败");
            }
            r = this.MotoGoToPosition(5, fkj.cfgX_BadSlot, true);
            if (r != 0)
            {
                MessageBox.Show("X轴移动到废卡存放区失败");
            }
        }

        /// <summary>
        /// 500张的推卡角度和1000的角度移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAngleUp_Click(object sender, EventArgs e)
        {
            Type type = typeof(FKJ500_V2);
            if (fkj.GetType() == type)
            {

            }
        }

        private void btnRemovePPI_Click(object sender, EventArgs e)
        {
            if (lbPositionConfig.SelectedIndex >= 0)
            {
                fkj.pcfgPositionProperty.RemoveItemAt(lbPositionConfig.SelectedIndex);
                ShowPositionProperty(fkj);
            }
        }

        /// <summary>
        /// 获取电机步数
        /// </summary>
        /// <param name="motorNum"></param>
        /// <returns>返回十进制的步数</returns>
        private string GetMotorStep(string motorNum)
        {
            string hexRData;
            string cmd = "0237";
            StringBuilder hexReData = new StringBuilder();
            hexReData.Append(motorNum.ToString().PadLeft(2, '0'));
            this.SendCommand(cmd, hexReData.ToString(), out hexRData);
            return Common.ConvertHexToString(hexRData, true);
        }

        private void btnAddPPI_Click(object sender, EventArgs e)
        {
            //最近一个参数要用当前电机的位置到0号卡槽的距离
            if (fkj.GetType() == typeof(FKJ4000_V2))
            {
                fkj.pcfgPositionProperty.AddItem((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value,
                                                           Convert.ToInt32(GetMotorStep("8")) - fkj.cfgY_Slot0);
            }
            else
            {
                fkj.pcfgPositionProperty.AddItem((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value,
                                                            Convert.ToInt32(GetMotorStep("A")) - fkj.cfgY_Slot0);
            }
            ShowPositionProperty(fkj);
        }

        /// <summary>
        /// 获取主板设备号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetDeviceNum_Click(object sender, EventArgs e)
        {
            string deviceNum;
            string hexRData;
            this.SendCommand("0104", "", out hexRData);
            this.textBox2.Text = "";
            deviceNum = HexToASCII(hexRData);
            this.textBox2.Text = deviceNum;
            MessageBox.Show("设备号：" + deviceNum);
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static string HexToASCII(string Msg)
        {
            byte[] buff = new byte[Msg.Length / 2];
            string Message = "";
            System.Text.Encoding chs = System.Text.Encoding.ASCII;
            Message = chs.GetString(strToToHexByte(Msg));
            return Message;
        }

        /// <summary>
        /// 显示配置按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadParam_Click(object sender, EventArgs e)
        {
            if (cmbDisplayFKJ.SelectedItem.ToString().Equals("fkj.cfg"))
            {
                DisplayFKJParam(fkj);
            }
            else if (cmbDisplayFKJ.SelectedItem.ToString().Equals("fkj_D1(2000).cfg"))
            {
                DisplayFKJParam(((FKJ2000_V2)fkj).fkj_D1);
            }
            else if (cmbDisplayFKJ.SelectedItem.ToString().Equals("fkj_D2(2000).cfg"))
            {
                DisplayFKJParam(((FKJ2000_V2)fkj).fkj_D2);
            }
            else
            {
                MessageBox.Show("选择正确的类");
            }
        }

        /// <summary>
        /// 面板显示FKJ配置
        /// </summary>
        /// <param name="fkjArg"></param>
        private void DisplayFKJParam(IFZJ fkjArg)
        {
            cbICReaderPort.Text = fkjArg.mInCardReaderPort.ToString();
            cbTKQPort.Text = fkjArg.mTKQPort.ToString();
            cbV2CPort.Text = fkjArg.mOutCardReaderPort.ToString();

            //外设类型
            cbTKDeviceType.SelectedIndex = fkjArg.cfgTKDeviceType - 1;
            cmbInReaderType.SelectedIndex = fkjArg.mInCardReaderType - 1;
            cmbOutReaderType.SelectedIndex = fkjArg.mOutCardReaderType - 1;

            //参数初始化
            nudX_InCard.Value = fkjArg.cfgX_InCard;
            nudX_OutCard.Value = fkjArg.cfgX_OutCard;
            nudX_BadSlot.Value = fkjArg.cfgX_BadSlot;
            nudX_Slot0.Value = fkjArg.cfgX_Slot0;
            nudX_Interval.Value = fkjArg.cfgX_Interval;
            nudX_SlotCount.Value = fkjArg.cfgX_SlotCount;

            nudICReader_InCard.Value = fkjArg.cfgICReader_InCard;
            nudICReader_OutCard.Value = fkjArg.cfgICReader_OutCard;

            nudY_InCard.Value = fkjArg.cfgY_InCard;
            nudY_OutCard.Value = fkjArg.cfgY_OutCard;
            nudY_BadSlot.Value = fkjArg.cfgY_BadSlot;
            nudY_Slot0.Value = fkjArg.cfgY_Slot0;
            nudY_Interval.Value = fkjArg.cfgY_Interval;
            nudY_SlotCount.Value = fkjArg.cfgY_SlotCount;

            nudY_AdaptTime.Value = fkjArg.cfgY_AdaptTime;
            nudY_AdaptPWM.Value = fkjArg.cfgY_AdaptPWM;

            nudAnger0_0.Value = fkjArg.cfgAnger0_0;//左角度
            nudAnger0_1.Value = fkjArg.cfgAnger0_1;//右角度
            nudPushCardStep.Value = fkjArg.cfgPushCardStep;
            nudY_Excursion.Value = fkjArg.cfgY_Excursion;
            nudX_Excursion.Value = fkjArg.cfgX_Excursion;
            nudY_GetCard_Excursion.Value = fkjArg.cfgY_GetCard_Excursion;
            nudY_BadCard_Excursion.Value = fkjArg.cfgY_BadCard_Excursion;//坏卡槽偏移

            nudFunctionSwitch.Value = fkjArg.cfgFunctionSwitch;

            ShowPositionProperty(fkjArg);
        }

        private void dianji_controls2_Load(object sender, EventArgs e)
        {

        }



        private void nudutycycle1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown cmp = (NumericUpDown)sender;
            int cycle = Math.Min(100, (int)cmp.Value);
            string hexData = String.Format("{0:X}", (byte)cycle).PadLeft(2, '0'); ;
            string hexRData;
            if (cmp == nudutycycle1)
                hexData = "00" + hexData;
            else if (cmp == nudutycycle2)
                hexData = "01" + hexData;
            else if (cmp == nudutycycle3)
                hexData = "02" + hexData;
            else if (cmp == nudutycycle4)
                hexData = "03" + hexData;
            else if (cmp == nudutycycle5)
                hexData = "04" + hexData;
            else if (cmp == nudutycycle6)
                hexData = "05" + hexData;
            else if (cmp == nudutycycle7)
                hexData = "06" + hexData;
            else if (cmp == nudutycycle8)
                hexData = "07" + hexData;
            else if (cmp == nudutycycle9)
                hexData = "08" + hexData;
            else if (cmp == nudutycycle10)
                hexData = "09" + hexData;
            else if (cmp == nudutycycle11)
                hexData = "0A" + hexData;
            else if (cmp == nudutycycle12)
                hexData = "0B" + hexData;

            uint r = this.SendCommand("0249", hexData, out hexRData);
            if (r != 0)
                MessageBox.Show("设置占空比失败!");
        }

        private void btnExportSetting_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
                BoardSettingSaveToDisk(sfd.FileName);
        }

        /// <summary>
        /// 读取芯片信息，导出到文本
        /// </summary>
        /// <param name="fileName"></param>
        private void BoardSettingSaveToDisk(string filePath)
        {
            string hexRData;
            uint r = this.SendCommand("0801", "", out hexRData);
            if (r != 0)
            {
                MessageBox.Show("读取配置失败R=0x" + String.Format("{0:X}", 2).PadLeft(2, '0'));
                return;
            }
            if (File.Exists(filePath))
                File.Delete(filePath);
            FileStream fsMyfile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                StreamWriter swMyfile = new StreamWriter(fsMyfile);
                swMyfile.Write(hexRData.ToString());
                swMyfile.Flush();
                swMyfile.Close();
            }
            finally
            {
                fsMyfile.Close();
            }
        }

        /// <summary>
        /// 将文本中的信息导入到控件中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportSetting_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(ofd.FileName))
                    MessageBox.Show("File Is Not Exit!");
                StreamReader sr = new StreamReader(ofd.FileName, Encoding.Default);
                string restOfStream = sr.ReadToEnd();

                byte[] rdata = UsbDeviceManager.ConvertHexStringToBytes(restOfStream);
                BoardConfig bc = (BoardConfig)UsbDeviceManager.ByteToStruct(rdata, typeof(BoardConfig));
                this.nudCfgFrequence1.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence2.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence3.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence4.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence5.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence6.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence7.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence8.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence9.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence10.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence11.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence12.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

                this.nudCfgAcceleration1.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration2.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration3.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration4.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration5.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration6.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration7.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration8.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration9.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration10.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration11.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration12.ValueChanged -= new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);

                this.nudCfgTargetFre1.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre2.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre3.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre4.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre5.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre6.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre7.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre8.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre9.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre10.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre11.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre12.ValueChanged -= new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);

                this.IsSf1.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf2.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf3.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf4.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf5.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf6.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf7.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf8.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf9.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf10.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf11.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf12.CheckedChanged -= new System.EventHandler(this.IsSf1_CheckedChanged);

                this.nudutycycle1.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle2.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle3.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle4.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle5.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle6.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle7.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle8.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle9.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle10.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle11.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle12.ValueChanged -= new System.EventHandler(this.nudutycycle1_ValueChanged);



                this.nudCfgwaitLoopInKeyEvent.ValueChanged -= new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

                if (bc.SFMotorAlamAvailableState < 2)
                    cbSFMotorAlamAvailableState.SelectedIndex = bc.SFMotorAlamAvailableState;
                else
                    cbSFMotorAlamAvailableState.SelectedIndex = 2;

                if (bc.SFMotorBreakerAvailableState < 2)
                    cbSFMotorBreakerAvailableState.SelectedIndex = bc.SFMotorBreakerAvailableState;
                else
                    cbSFMotorBreakerAvailableState.SelectedIndex = 1;
                nudSFMotorPutDownBreakTimeout.Value = (byte)bc.SFMotorPutDownBreakTimeout;
                nudSFMotorPutDownBreakWaitInterval.Value = (byte)bc.SFMotorPutDownBreakWaitInterval;

                this.nudCfgFrequence1.Value = bc.O_Frequency[0];
                this.nudCfgFrequence2.Value = bc.O_Frequency[1];
                this.nudCfgFrequence3.Value = bc.O_Frequency[2];
                this.nudCfgFrequence4.Value = bc.O_Frequency[3];
                this.nudCfgFrequence5.Value = bc.O_Frequency[4];
                this.nudCfgFrequence6.Value = bc.O_Frequency[5];
                this.nudCfgFrequence7.Value = bc.O_Frequency[6];
                this.nudCfgFrequence8.Value = bc.O_Frequency[7];
                this.nudCfgFrequence9.Value = bc.O_Frequency[8];
                this.nudCfgFrequence10.Value = bc.O_Frequency[9];
                this.nudCfgFrequence11.Value = bc.O_Frequency[10];
                this.nudCfgFrequence12.Value = bc.O_Frequency[11];

                this.nudCfgAcceleration1.Value = bc.A_Frequency[0];
                this.nudCfgAcceleration2.Value = bc.A_Frequency[1];
                this.nudCfgAcceleration3.Value = bc.A_Frequency[2];
                this.nudCfgAcceleration4.Value = bc.A_Frequency[3];
                this.nudCfgAcceleration5.Value = bc.A_Frequency[4];
                this.nudCfgAcceleration6.Value = bc.A_Frequency[5];
                this.nudCfgAcceleration7.Value = bc.A_Frequency[6];
                this.nudCfgAcceleration8.Value = bc.A_Frequency[7];
                this.nudCfgAcceleration9.Value = bc.A_Frequency[8];
                this.nudCfgAcceleration10.Value = bc.A_Frequency[9];
                this.nudCfgAcceleration11.Value = bc.A_Frequency[10];
                this.nudCfgAcceleration12.Value = bc.A_Frequency[11];

                this.nudCfgTargetFre1.Value = bc.T_Frequency[0];
                this.nudCfgTargetFre2.Value = bc.T_Frequency[1];
                this.nudCfgTargetFre3.Value = bc.T_Frequency[2];
                this.nudCfgTargetFre4.Value = bc.T_Frequency[3];
                this.nudCfgTargetFre5.Value = bc.T_Frequency[4];
                this.nudCfgTargetFre6.Value = bc.T_Frequency[5];
                this.nudCfgTargetFre7.Value = bc.T_Frequency[6];
                this.nudCfgTargetFre8.Value = bc.T_Frequency[7];
                this.nudCfgTargetFre9.Value = bc.T_Frequency[8];
                this.nudCfgTargetFre10.Value = bc.T_Frequency[9];
                this.nudCfgTargetFre11.Value = bc.T_Frequency[10];
                this.nudCfgTargetFre12.Value = bc.T_Frequency[11];

                this.IsSf1.Checked = (bc.IsSfmotor[0] > 0);
                this.IsSf2.Checked = (bc.IsSfmotor[1] > 0);
                this.IsSf3.Checked = (bc.IsSfmotor[2] > 0);
                this.IsSf4.Checked = (bc.IsSfmotor[3] > 0);
                this.IsSf5.Checked = (bc.IsSfmotor[4] > 0);
                this.IsSf6.Checked = (bc.IsSfmotor[5] > 0);
                this.IsSf7.Checked = (bc.IsSfmotor[6] > 0);
                this.IsSf8.Checked = (bc.IsSfmotor[7] > 0);
                this.IsSf9.Checked = (bc.IsSfmotor[8] > 0);
                this.IsSf10.Checked = (bc.IsSfmotor[9] > 0);
                this.IsSf11.Checked = (bc.IsSfmotor[10] > 0);
                this.IsSf12.Checked = (bc.IsSfmotor[11] > 0);

                this.nudutycycle1.Value = bc.DutyCycle[0];
                this.nudutycycle2.Value = bc.DutyCycle[1];
                this.nudutycycle3.Value = bc.DutyCycle[2];
                this.nudutycycle4.Value = bc.DutyCycle[3];
                this.nudutycycle5.Value = bc.DutyCycle[4];
                this.nudutycycle6.Value = bc.DutyCycle[5];
                this.nudutycycle7.Value = bc.DutyCycle[6];
                this.nudutycycle8.Value = bc.DutyCycle[7];
                this.nudutycycle9.Value = bc.DutyCycle[8];
                this.nudutycycle10.Value = bc.DutyCycle[9];
                this.nudutycycle11.Value = bc.DutyCycle[10];
                this.nudutycycle12.Value = bc.DutyCycle[11];

                this.nudCfgwaitLoopInKeyEvent.Value = Math.Min(20000, bc.WaitLoopInKeyEvent);
                this.nudCfgFrequence1.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence2.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence3.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence4.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence5.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence6.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence7.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence8.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence9.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence10.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence11.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                this.nudCfgFrequence12.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);

                this.nudCfgAcceleration1.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration2.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration3.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration4.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration5.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration6.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration7.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration8.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration9.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration10.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration11.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);
                this.nudCfgAcceleration12.ValueChanged += new System.EventHandler(this.nudCfgAcceleration1_ValueChanged);

                this.nudCfgTargetFre1.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre2.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre3.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre4.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre5.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre6.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre7.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre8.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre9.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre10.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre11.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);
                this.nudCfgTargetFre12.ValueChanged += new System.EventHandler(this.nudCfgTargetFre1_ValueChanged);

                this.IsSf1.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf2.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf3.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf4.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf5.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf6.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf7.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf8.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf9.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf10.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf11.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);
                this.IsSf12.CheckedChanged += new System.EventHandler(this.IsSf1_CheckedChanged);

                this.nudutycycle1.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle2.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle3.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle4.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle5.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle6.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle7.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle8.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle9.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle10.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle11.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);
                this.nudutycycle12.ValueChanged += new System.EventHandler(this.nudutycycle1_ValueChanged);

                this.nudCfgwaitLoopInKeyEvent.ValueChanged += new System.EventHandler(this.nudCfgFrequence1_ValueChanged);
                for (int i = 0; i < 12; i++)
                {
                    this.clbCfgMotorFX.SetItemChecked(i, bc.motorDir[i] != 0);
                }
                nudCfgXWF0.Value = bc.motorSensor1[0]; nudCfgXWF1.Value = bc.motorSensor1[1];
                nudCfgXWF2.Value = bc.motorSensor1[2]; nudCfgXWF3.Value = bc.motorSensor1[3];
                nudCfgXWF4.Value = bc.motorSensor1[4]; nudCfgXWF5.Value = bc.motorSensor1[5];
                nudCfgXWF6.Value = bc.motorSensor1[6]; nudCfgXWF7.Value = bc.motorSensor1[7];
                nudCfgXWF8.Value = bc.motorSensor1[8]; nudCfgXWF9.Value = bc.motorSensor1[9];
                nudCfgXWF10.Value = bc.motorSensor1[10]; nudCfgXWF11.Value = bc.motorSensor1[11];

                nudCfgXW0.Value = bc.motorSensor0[0]; nudCfgXW1.Value = bc.motorSensor0[1];
                nudCfgXW2.Value = bc.motorSensor0[2]; nudCfgXW3.Value = bc.motorSensor0[3];
                nudCfgXW4.Value = bc.motorSensor0[4]; nudCfgXW5.Value = bc.motorSensor0[5];
                nudCfgXW6.Value = bc.motorSensor0[6]; nudCfgXW7.Value = bc.motorSensor0[7];
                nudCfgXW8.Value = bc.motorSensor0[8]; nudCfgXW9.Value = bc.motorSensor0[9];
                nudCfgXW10.Value = bc.motorSensor0[10]; nudCfgXW11.Value = bc.motorSensor0[11];

                //新的配置项
                nudCfgCtrl0_0.Value = bc.motorControlSensor0[0]; nudCfgCtrl0_1.Value = bc.motorControlSensor0[1];
                nudCfgCtrl0_2.Value = bc.motorControlSensor0[2]; nudCfgCtrl0_3.Value = bc.motorControlSensor0[3];
                nudCfgCtrl0_4.Value = bc.motorControlSensor0[4]; nudCfgCtrl0_5.Value = bc.motorControlSensor0[5];
                nudCfgCtrl0_6.Value = bc.motorControlSensor0[6]; nudCfgCtrl0_7.Value = bc.motorControlSensor0[7];
                nudCfgCtrl0_8.Value = bc.motorControlSensor0[8]; nudCfgCtrl0_9.Value = bc.motorControlSensor0[9];
                nudCfgCtrl0_10.Value = bc.motorControlSensor0[10]; nudCfgCtrl0_11.Value = bc.motorControlSensor0[11];

                nudCfgCtrl1_0.Value = bc.motorControlSensor1[0]; nudCfgCtrl1_1.Value = bc.motorControlSensor1[1];
                nudCfgCtrl1_2.Value = bc.motorControlSensor1[2]; nudCfgCtrl1_3.Value = bc.motorControlSensor1[3];
                nudCfgCtrl1_4.Value = bc.motorControlSensor1[4]; nudCfgCtrl1_5.Value = bc.motorControlSensor1[5];
                nudCfgCtrl1_6.Value = bc.motorControlSensor1[6]; nudCfgCtrl1_7.Value = bc.motorControlSensor1[7];
                nudCfgCtrl1_8.Value = bc.motorControlSensor1[8]; nudCfgCtrl1_9.Value = bc.motorControlSensor1[9];
                nudCfgCtrl1_10.Value = bc.motorControlSensor1[10]; nudCfgCtrl1_11.Value = bc.motorControlSensor1[11];

                nudCfgMotorBrake0.Value = bc.motorBrake1[0]; nudCfgMotorBrake1.Value = bc.motorBrake1[1];
                nudCfgMotorBrake2.Value = bc.motorBrake1[2]; nudCfgMotorBrake3.Value = bc.motorBrake1[3];
                nudCfgMotorBrake4.Value = bc.motorBrake1[4]; nudCfgMotorBrake5.Value = bc.motorBrake1[5];
                nudCfgMotorBrake6.Value = bc.motorBrake1[6]; nudCfgMotorBrake7.Value = bc.motorBrake1[7];
                nudCfgMotorBrake8.Value = bc.motorBrake1[8]; nudCfgMotorBrake9.Value = bc.motorBrake1[9];
                nudCfgMotorBrake10.Value = bc.motorBrake1[10]; nudCfgMotorBrake11.Value = bc.motorBrake1[11];

                nudCfgMotorBrake2_0.Value = bc.motorBrake2[0]; nudCfgMotorBrake2_1.Value = bc.motorBrake2[1];
                nudCfgMotorBrake2_2.Value = bc.motorBrake2[2]; nudCfgMotorBrake2_3.Value = bc.motorBrake2[3];
                nudCfgMotorBrake2_4.Value = bc.motorBrake2[4]; nudCfgMotorBrake2_5.Value = bc.motorBrake2[5];
                nudCfgMotorBrake2_6.Value = bc.motorBrake2[6]; nudCfgMotorBrake2_7.Value = bc.motorBrake2[7];
                nudCfgMotorBrake2_8.Value = bc.motorBrake2[8]; nudCfgMotorBrake2_9.Value = bc.motorBrake2[9];
                nudCfgMotorBrake2_10.Value = bc.motorBrake2[10]; nudCfgMotorBrake2_11.Value = bc.motorBrake2[11];

            }
        }

        private void btnGetLocalIPAddress_Click(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipadrList = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipa in ipadrList)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    txtLocalIPAddress.Text = ipa.ToString() + "\r\n";
            }
        }

        private void lbPositionConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbPositionConfig.SelectedIndex >= 0)
            {
                FKJ_PositionPropertyItem fppi = fkj.pcfgPositionProperty.GetItem(lbPositionConfig.SelectedIndex);
                txtExcursionPositionX.Text = fppi.X.ToString();
                txtExcursionPositionY.Text = fppi.Y.ToString();
                txtExcursionPositionStep.Text = fppi.Step.ToString();
            }
        }

        private void btnAlterExcursion_Click(object sender, EventArgs e)
        {
            fkj.pcfgPositionProperty.RemoveItem(Int32.Parse(txtExcursionPositionX.Text), Int32.Parse(txtExcursionPositionY.Text));//删除
            fkj.pcfgPositionProperty.AddItem(Int32.Parse(txtExcursionPositionX.Text), Int32.Parse(txtExcursionPositionY.Text), Int32.Parse(txtExcursionPositionStep.Text));//添加
            //更新列表
            ShowPositionProperty(fkj);
        }

        /*
                #region 局域网通信
                private IPEndPoint serverIPEndPoint1;
                private int listenPort;
                private Socket listenSocket;
                private Socket clientSocket;
                private Thread threadAccept;
                private Thread threadReceive;
                private byte[] receiveByte1 = new byte[1024];
                private static ManualResetEvent lockSocket1 = new ManualResetEvent(false);

                private void btnStartListen_Click(object sender, EventArgs e)
                {
                    listenPort = Int32.Parse(txtSocketPort.Text);
                    serverIPEndPoint1 = new IPEndPoint(IPAddress.Any, listenPort);
                    listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listenSocket.Bind(serverIPEndPoint1);
                    listenSocket.Listen(50);
                    threadAccept = new Thread(new ThreadStart(AcceptThread));
                    threadAccept.Start();
                }

                private void AcceptThread()
                {
                    while (true)
                    {
                        lockSocket1.Reset();
                        listenSocket.BeginAccept(new AsyncCallback(AcceptCall), listenSocket);
                        lockSocket.WaitOne();
                    }
                }

                private void AcceptCall(IAsyncResult asyncResult)
                {
                    lockSocket.Set();
                    clientSocket = listenSocket.EndAccept(asyncResult);
                    threadReceive = new Thread(new ThreadStart(ReceiveThread1));
                    threadReceive.Start();
                }

                private void ReceiveThread1()
                {
                    try
                    {
                        clientSocket.BeginReceive(receiveByte1, 0, receiveByte.Length, 0, new AsyncCallback(AsyncReceiveCall1), clientSocket);
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show(Ex.Message);
                    }
                }

                private void AsyncReceiveCall1(IAsyncResult asyncResult)
                {
                    int bytesRead = clientSocket.EndReceive(asyncResult);
                    string receiveRead = Encoding.Default.GetString(receiveByte1, 0, bytesRead);
                    ShowMessage1(receiveRead);
                    clientSocket.BeginReceive(receiveByte, 0, receiveByte.Length, 0, new AsyncCallback(AsyncReceiveCall1), clientSocket);
                }

                delegate void ShowMessageCallBack(string message);

                void ShowMessage1(string message)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new ShowMessageCallBack(ShowMessage1), new object[] { message });
                    }
                    else
                    {
                        txtServerReceiveMsg.AppendText(message + "\n");
                    }
                }

                private void btnServerSendMsg_Click(object sender, EventArgs e)
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        Byte[] sendByte = Encoding.Default.GetBytes(txtServerSendMsg.Text);
                        clientSocket.BeginSend(sendByte, 0, sendByte.Length, 0, new AsyncCallback(SendCallback1), clientSocket);
                    }
                }

                private void SendCallback1(IAsyncResult asyncResult)
                {
                    Socket socket = (Socket)asyncResult.AsyncState;
                    int bytesSent = socket.EndSend(asyncResult);
                }

                private IPAddress serverIP;
                private int serverPort;
                private IPEndPoint serverIPEndPoint;
                private Socket clitenSocket;
                static ManualResetEvent lockSocket = new ManualResetEvent(false);
                byte[] receiveByte = new byte[1024];

                private void btnSocketConnect_Click(object sender, EventArgs e)
                {
                    serverIP = IPAddress.Parse(txtServerIPAddress.Text);
                    serverPort = Int32.Parse(txtServerSocketPort.Text);
                    serverIPEndPoint = new IPEndPoint(serverIP, serverPort);
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.BeginConnect(serverIPEndPoint, new AsyncCallback(ConnectCallback), clientSocket);
                    lockSocket.WaitOne();
                }

                private void ConnectCallback(IAsyncResult asyncResult)
                {
                    try
                    {
                        Socket client = (Socket)asyncResult.AsyncState;
                        client.EndConnect(asyncResult);
                        Thread thread = new Thread(new ThreadStart(ReceiveThread));
                        thread.Start();
                        lockSocket.Set();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }

                private void ReceiveThread()
                {
                    try
                    {
                        clientSocket.BeginReceive(receiveByte, 0, receiveByte.Length, 0, new AsyncCallback(AsyncReceiveCall), clientSocket);

                    }
                    catch (Exception EX)
                    {
                        MessageBox.Show(EX.Message);
                    }
                }

                private void AsyncReceiveCall(IAsyncResult asyncResult)
                {
                    int bytesRead = clientSocket.EndReceive(asyncResult);
                    string receiveString = Encoding.Default.GetString(receiveByte, 0, bytesRead);
                    ShowMessage(receiveString);
                    clientSocket.BeginReceive(receiveByte, 0, receiveByte.Length, 0, new AsyncCallback(AsyncReceiveCall), clientSocket);
                }

                delegate void ShowMessageCallback(string message);

                void ShowMessage(string message)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new ShowMessageCallback(ShowMessage), new object[] { message });
                    }
                    else
                    {
                        txtClientReceiveMsg.AppendText(message + "\n");
                    }
                }

                private void btnClientSendMsg_Click(object sender, EventArgs e)
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        Byte[] sendByte = Encoding.Default.GetBytes(txtClientSendMsg.Text);
                        clientSocket.BeginSend(sendByte, 0, sendByte.Length, 0, new AsyncCallback(SendCallback), clientSocket);
                    }
                }

                private void SendCallback(IAsyncResult asyncResult)
                {
                    Socket socket = (Socket)asyncResult.AsyncState;
                    int byteSent = socket.EndSend(asyncResult);
                }
                #endregion
                */

        #region Tcp通信
        Thread threadWatch = null;
        Socket socketWatch = null;
        Socket socketClient = null;
        Thread threadClient = null;

        private void btnSocketConnect_Click(object sender, EventArgs e)
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(txtServerIPAddress.Text);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(txtServerSocketPort.Text));
            socketClient.Connect(endpoint);
            threadClient = new Thread(RecMsg);
            threadClient.IsBackground = true;
            threadClient.Start();
        }

        private void RecMsg()
        {
            while (true)
            {
                byte[] arrRecMsg = new byte[1024 * 1024];
                int length = socketClient.Receive(arrRecMsg);
                string strRecMsg = Encoding.UTF8.GetString(arrRecMsg,0,length);
                txtClientReceiveMsg.AppendText(GetCurrentTime() + "\r\n" + strRecMsg + "\r\n");
            }
        }

        private void ClientSendMsg(string sendMsg)
        {
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            if (socketClient == null)
            {
                MessageBox.Show("socketClient为空");
                return;
            }
            socketClient.Send(arrClientSendMsg);
            txtClientReceiveMsg.AppendText("客户端：" + GetCurrentTime() + "\r\n" + sendMsg + "\r\n");
        }

        private void btnClientSendMsg_Click(object sender, EventArgs e)
        {
            ClientSendMsg(txtClientSendMsg.Text);
        }

        private void btnServerSendMsg_Click(object sender, EventArgs e)
        {
            ServerSendMsg(txtServerSendMsg.Text);
        }

        private void btnStartListen_Click(object sender, EventArgs e)
        {
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, int.Parse(txtSocketPort.Text));
            socketWatch.Bind(endpoint);
            socketWatch.Listen(50);
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            txtServerReceiveMsg.AppendText("开始监听......\r\n");
        }

        Socket socConnection = null;

        private void WatchConnecting()
        {
            while (true)
            {
                socConnection = socketWatch.Accept();//阻塞
                txtServerReceiveMsg.AppendText("客户端连接成功！\r\n");
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ServerRecMsg);
                Thread thr = new Thread(pts);
                thr.IsBackground = true;
                thr.Start(socConnection);
            }
        }

        private void ServerSendMsg(string sendMsg)
        {
            byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            socConnection.Send(arrSendMsg);
            txtServerReceiveMsg.AppendText(GetCurrentTime() + "\r\n" + sendMsg + "\r\n");
        }

        private void ServerRecMsg(object socketClientPara)
        {
            Socket socketServer = socketClientPara as Socket;
            while (true)
            {
                byte[] arrServerRecMsg = new byte[1024 * 1024];
                int length = socketServer.Receive(arrServerRecMsg);
                string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                txtServerReceiveMsg.AppendText("客户端：" + GetCurrentTime() +"\r\n" + strSRecMsg + "\r\n");
                MessageBox.Show("客户端：" + "\r\n" + GetCurrentTime() +"\r\n" + strSRecMsg + "\r\n");
            }
        }

        private DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }
        #endregion
    }
}
