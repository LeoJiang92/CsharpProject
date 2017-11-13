namespace MotorControlBoard
{
    partial class FormTestV2CF
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cbTestCard = new System.Windows.Forms.CheckBox();
            this.cbTKQPort = new System.Windows.Forms.ComboBox();
            this.label75 = new System.Windows.Forms.Label();
            this.cbICReaderPort = new System.Windows.Forms.ComboBox();
            this.label76 = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cbStopWhenFailure = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbTestCard
            // 
            this.cbTestCard.AutoSize = true;
            this.cbTestCard.Location = new System.Drawing.Point(396, 7);
            this.cbTestCard.Name = "cbTestCard";
            this.cbTestCard.Size = new System.Drawing.Size(60, 16);
            this.cbTestCard.TabIndex = 25;
            this.cbTestCard.Text = "测试卡";
            this.cbTestCard.UseVisualStyleBackColor = true;
            this.cbTestCard.CheckedChanged += new System.EventHandler(this.cbTestCard_CheckedChanged);
            // 
            // cbTKQPort
            // 
            this.cbTKQPort.FormattingEnabled = true;
            this.cbTKQPort.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.cbTKQPort.Location = new System.Drawing.Point(152, 6);
            this.cbTKQPort.Name = "cbTKQPort";
            this.cbTKQPort.Size = new System.Drawing.Size(73, 20);
            this.cbTKQPort.TabIndex = 48;
            this.cbTKQPort.Text = "1";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Location = new System.Drawing.Point(69, 9);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(77, 12);
            this.label75.TabIndex = 49;
            this.label75.Text = "推卡装置串口";
            // 
            // cbICReaderPort
            // 
            this.cbICReaderPort.FormattingEnabled = true;
            this.cbICReaderPort.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.cbICReaderPort.Location = new System.Drawing.Point(339, 5);
            this.cbICReaderPort.Name = "cbICReaderPort";
            this.cbICReaderPort.Size = new System.Drawing.Size(38, 20);
            this.cbICReaderPort.TabIndex = 50;
            this.cbICReaderPort.Text = "2";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Location = new System.Drawing.Point(244, 8);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(89, 12);
            this.label76.TabIndex = 51;
            this.label76.Text = "电动读卡器串口";
            // 
            // timer2
            // 
            this.timer2.Interval = 1000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(865, 342);
            this.textBox1.TabIndex = 52;
            // 
            // cbStopWhenFailure
            // 
            this.cbStopWhenFailure.AutoSize = true;
            this.cbStopWhenFailure.Location = new System.Drawing.Point(462, 5);
            this.cbStopWhenFailure.Name = "cbStopWhenFailure";
            this.cbStopWhenFailure.Size = new System.Drawing.Size(120, 16);
            this.cbStopWhenFailure.TabIndex = 53;
            this.cbStopWhenFailure.Text = "读磁失败立即停止";
            this.cbStopWhenFailure.UseVisualStyleBackColor = true;
            // 
            // FormTestV2CF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 386);
            this.Controls.Add(this.cbStopWhenFailure);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cbTKQPort);
            this.Controls.Add(this.label75);
            this.Controls.Add(this.cbICReaderPort);
            this.Controls.Add(this.label76);
            this.Controls.Add(this.cbTestCard);
            this.Name = "FormTestV2CF";
            this.Text = "欧姆龙测试";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbTestCard;
        private System.Windows.Forms.ComboBox cbTKQPort;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.ComboBox cbICReaderPort;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox cbStopWhenFailure;
    }
}