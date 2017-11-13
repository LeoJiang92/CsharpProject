using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MotorControlBoard
{    
    public partial class dianji_controls : UserControl
    {
        public dianji_controls()
        {
            InitializeComponent();
        }


        #region 属性

        private string _groupTitle;
        /// <summary>
        /// 组标题
        /// </summary>
        public string GroupTitle
        {
            get { return _groupTitle; }
            set { _groupTitle = value; }
        }

        /// <summary>
        /// 步数
        /// </summary>
        public int BuShu
        {
            get
            {
                string bs = this.textBox_dianji_bushu.Text.Trim();
                if (String.IsNullOrEmpty(bs))
                {
                    bs = "0";
                }
                return int.Parse(bs);
            }
            set
            {
                this.textBox_dianji_bushu.Text = value.ToString();
            }
        }


        /// <summary>
        /// 左长转？
        /// </summary>
        public bool Left_ChangZhuan
        {
            get
            {
                return this.checkBox_dianji_left_changzhuan.Checked;
            }
            set
            {
                this.checkBox_dianji_left_changzhuan.Checked = value;
            }
        }

        /// <summary>
        /// 右长转？
        /// </summary>
        public bool Right_ChangZhuan
        {
            get
            {
                return this.checkBox_dianji_right_changzhuan.Checked;
            }
            set
            {
                this.checkBox_dianji_right_changzhuan.Checked = value;
            }
        }


        /// <summary>
        /// 停止时锁住？
        /// </summary>
        public bool Stop_Lock
        {
            get
            {
                return this.checkBox_dianji_stop_lock.Checked;
            }
            set
            {
                this.checkBox_dianji_stop_lock.Checked = value;
            }
        }

        #endregion


        #region 事件

        /// <summary>
        /// 点击电机左转 事件
        /// </summary>
        public event EventHandler MotorActionEvent;

        #endregion


        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dianji_controls_Load(object sender, EventArgs e)
        {
            this.groupBox_dianji.Text = GroupTitle;
        }

        public bool SetLockOnStop
        {
            get { return checkBox_dianji_stop_lock.Checked; }
            set { checkBox_dianji_stop_lock.Checked = value; }
        }
        /// <summary>
        /// 电机长右转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_dianji_right_changzhuan_CheckedChanged(object sender, EventArgs e)
        {
            checkBox_dianji_left_changzhuan.CheckedChanged -= checkBox_dianji_right_changzhuan_CheckedChanged;
            checkBox_dianji_right_changzhuan.CheckedChanged -= checkBox_dianji_right_changzhuan_CheckedChanged;
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                if (cb.Equals(checkBox_dianji_right_changzhuan))
                    checkBox_dianji_left_changzhuan.Checked = false;
                else
                    checkBox_dianji_right_changzhuan.Checked = false;
            }
            if (MotorActionEvent != null)
            {
                MotorEventArgs args = new MotorEventArgs();
                args.LockOnStop = checkBox_dianji_stop_lock.Checked;
                args.PwmCnt = int.Parse(textBox_dianji_bushu.Text);
                args.IsLeft = !cb.Equals(checkBox_dianji_right_changzhuan);
                args.IsRun = cb.Checked;
                MotorActionEvent(this, args);
            }
            checkBox_dianji_left_changzhuan.CheckedChanged += checkBox_dianji_right_changzhuan_CheckedChanged;
            checkBox_dianji_right_changzhuan.CheckedChanged += checkBox_dianji_right_changzhuan_CheckedChanged;
        }

        private void button_dianji_right_MouseDown(object sender, MouseEventArgs e)
        {
            checkBox_dianji_right_changzhuan.Checked = true;
        }

        private void button_dianji_right_MouseUp(object sender, MouseEventArgs e)
        {
            checkBox_dianji_right_changzhuan.Checked = false;           
        }

        private void button_dianji_left_MouseDown(object sender, MouseEventArgs e)
        {
            checkBox_dianji_left_changzhuan.Checked = true;
        }

        private void button_dianji_left_MouseUp(object sender, MouseEventArgs e)
        {
            checkBox_dianji_left_changzhuan.Checked = false;
        }

        private void button_dianji_right_Click(object sender, EventArgs e)
        {

        }

        private void btnQueryPWM_Cnt_Click(object sender, EventArgs e)
        {            
            if (MotorActionEvent != null)
            {
                MotorEventArgs args = new MotorEventArgs();
                args.JustQueryMotorStep = true; 
                MotorActionEvent(this, args);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MotorActionEvent != null)
            {
                MotorEventArgs args = new MotorEventArgs();
                args.JustQueryMotorStep = false;
                args.Tag = 1;
                MotorActionEvent(this, args);
            }
        }

    }
    public class MotorEventArgs : EventArgs
    {
        public bool LockOnStop = false;
        public bool IsLeft = false;
        public int PwmCnt = 0;
        public bool IsRun = true;
        public bool JustQueryMotorStep = false;
        public int Tag = 0;
    }
}
