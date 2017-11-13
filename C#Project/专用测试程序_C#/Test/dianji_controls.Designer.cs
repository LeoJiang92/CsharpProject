namespace MotorControlBoard
{
    partial class dianji_controls
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox_dianji = new System.Windows.Forms.GroupBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnQueryPWM_Cnt = new System.Windows.Forms.Button();
            this.checkBox_dianji_stop_lock = new System.Windows.Forms.CheckBox();
            this.checkBox_dianji_right_changzhuan = new System.Windows.Forms.CheckBox();
            this.checkBox_dianji_left_changzhuan = new System.Windows.Forms.CheckBox();
            this.textBox_dianji_bushu = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_dianji_right = new System.Windows.Forms.Button();
            this.button_dianji_left = new System.Windows.Forms.Button();
            this.groupBox_dianji.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_dianji
            // 
            this.groupBox_dianji.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_dianji.Controls.Add(this.btnReset);
            this.groupBox_dianji.Controls.Add(this.btnQueryPWM_Cnt);
            this.groupBox_dianji.Controls.Add(this.checkBox_dianji_stop_lock);
            this.groupBox_dianji.Controls.Add(this.checkBox_dianji_right_changzhuan);
            this.groupBox_dianji.Controls.Add(this.checkBox_dianji_left_changzhuan);
            this.groupBox_dianji.Controls.Add(this.textBox_dianji_bushu);
            this.groupBox_dianji.Controls.Add(this.label7);
            this.groupBox_dianji.Controls.Add(this.button_dianji_right);
            this.groupBox_dianji.Controls.Add(this.button_dianji_left);
            this.groupBox_dianji.Location = new System.Drawing.Point(3, 3);
            this.groupBox_dianji.Name = "groupBox_dianji";
            this.groupBox_dianji.Size = new System.Drawing.Size(253, 73);
            this.groupBox_dianji.TabIndex = 1;
            this.groupBox_dianji.TabStop = false;
            this.groupBox_dianji.Text = "电机1";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(145, 33);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(44, 20);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "重启";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnQueryPWM_Cnt
            // 
            this.btnQueryPWM_Cnt.Location = new System.Drawing.Point(59, 33);
            this.btnQueryPWM_Cnt.Name = "btnQueryPWM_Cnt";
            this.btnQueryPWM_Cnt.Size = new System.Drawing.Size(86, 20);
            this.btnQueryPWM_Cnt.TabIndex = 7;
            this.btnQueryPWM_Cnt.Text = "获取当前步数";
            this.btnQueryPWM_Cnt.UseVisualStyleBackColor = true;
            this.btnQueryPWM_Cnt.Click += new System.EventHandler(this.btnQueryPWM_Cnt_Click);
            // 
            // checkBox_dianji_stop_lock
            // 
            this.checkBox_dianji_stop_lock.AutoSize = true;
            this.checkBox_dianji_stop_lock.Location = new System.Drawing.Point(96, 55);
            this.checkBox_dianji_stop_lock.Name = "checkBox_dianji_stop_lock";
            this.checkBox_dianji_stop_lock.Size = new System.Drawing.Size(84, 16);
            this.checkBox_dianji_stop_lock.TabIndex = 6;
            this.checkBox_dianji_stop_lock.Text = "停止时锁死";
            this.checkBox_dianji_stop_lock.UseVisualStyleBackColor = true;
            // 
            // checkBox_dianji_right_changzhuan
            // 
            this.checkBox_dianji_right_changzhuan.AutoSize = true;
            this.checkBox_dianji_right_changzhuan.Location = new System.Drawing.Point(195, 52);
            this.checkBox_dianji_right_changzhuan.Name = "checkBox_dianji_right_changzhuan";
            this.checkBox_dianji_right_changzhuan.Size = new System.Drawing.Size(48, 16);
            this.checkBox_dianji_right_changzhuan.TabIndex = 5;
            this.checkBox_dianji_right_changzhuan.Text = "长转";
            this.checkBox_dianji_right_changzhuan.UseVisualStyleBackColor = true;
            this.checkBox_dianji_right_changzhuan.CheckedChanged += new System.EventHandler(this.checkBox_dianji_right_changzhuan_CheckedChanged);
            // 
            // checkBox_dianji_left_changzhuan
            // 
            this.checkBox_dianji_left_changzhuan.AutoSize = true;
            this.checkBox_dianji_left_changzhuan.Location = new System.Drawing.Point(9, 52);
            this.checkBox_dianji_left_changzhuan.Name = "checkBox_dianji_left_changzhuan";
            this.checkBox_dianji_left_changzhuan.Size = new System.Drawing.Size(48, 16);
            this.checkBox_dianji_left_changzhuan.TabIndex = 4;
            this.checkBox_dianji_left_changzhuan.Text = "长转";
            this.checkBox_dianji_left_changzhuan.UseVisualStyleBackColor = true;
            this.checkBox_dianji_left_changzhuan.CheckedChanged += new System.EventHandler(this.checkBox_dianji_right_changzhuan_CheckedChanged);
            // 
            // textBox_dianji_bushu
            // 
            this.textBox_dianji_bushu.Location = new System.Drawing.Point(96, 11);
            this.textBox_dianji_bushu.Name = "textBox_dianji_bushu";
            this.textBox_dianji_bushu.Size = new System.Drawing.Size(93, 21);
            this.textBox_dianji_bushu.TabIndex = 3;
            this.textBox_dianji_bushu.Text = "10000";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(60, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "步数：";
            // 
            // button_dianji_right
            // 
            this.button_dianji_right.Location = new System.Drawing.Point(199, 14);
            this.button_dianji_right.Name = "button_dianji_right";
            this.button_dianji_right.Size = new System.Drawing.Size(44, 21);
            this.button_dianji_right.TabIndex = 1;
            this.button_dianji_right.Text = "后>>>";
            this.button_dianji_right.UseVisualStyleBackColor = true;
            this.button_dianji_right.Click += new System.EventHandler(this.button_dianji_right_Click);
            this.button_dianji_right.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_dianji_right_MouseDown);
            this.button_dianji_right.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_dianji_right_MouseUp);
            // 
            // button_dianji_left
            // 
            this.button_dianji_left.Location = new System.Drawing.Point(9, 14);
            this.button_dianji_left.Name = "button_dianji_left";
            this.button_dianji_left.Size = new System.Drawing.Size(44, 21);
            this.button_dianji_left.TabIndex = 0;
            this.button_dianji_left.Text = "<<<前";
            this.button_dianji_left.UseVisualStyleBackColor = true;
            this.button_dianji_left.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_dianji_left_MouseDown);
            this.button_dianji_left.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_dianji_left_MouseUp);
            // 
            // dianji_controls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.groupBox_dianji);
            this.Name = "dianji_controls";
            this.Size = new System.Drawing.Size(259, 82);
            this.Load += new System.EventHandler(this.dianji_controls_Load);
            this.groupBox_dianji.ResumeLayout(false);
            this.groupBox_dianji.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_dianji;
        private System.Windows.Forms.CheckBox checkBox_dianji_stop_lock;
        private System.Windows.Forms.CheckBox checkBox_dianji_right_changzhuan;
        private System.Windows.Forms.CheckBox checkBox_dianji_left_changzhuan;
        private System.Windows.Forms.TextBox textBox_dianji_bushu;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_dianji_right;
        private System.Windows.Forms.Button button_dianji_left;
        private System.Windows.Forms.Button btnQueryPWM_Cnt;
        private System.Windows.Forms.Button btnReset;
    }
}
