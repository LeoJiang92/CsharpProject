namespace MotorControlBoard
{
    partial class FrmFKJDemo
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
            this.cbAutoChangeCardSlotX = new System.Windows.Forms.ComboBox();
            this.btCSPutCardToBadSlot = new System.Windows.Forms.Button();
            this.btnCSGetCardToBadSlot = new System.Windows.Forms.Button();
            this.btnCSGetCardToUser = new System.Windows.Forms.Button();
            this.btCSGetCardToICReader = new System.Windows.Forms.Button();
            this.btCSPutCardToICReader = new System.Windows.Forms.Button();
            this.btCSPutCardToCS = new System.Windows.Forms.Button();
            this.nudCSGotoIndexX = new System.Windows.Forms.NumericUpDown();
            this.cbAutoPutCard = new System.Windows.Forms.CheckBox();
            this.btCSPutCardToTemp = new System.Windows.Forms.Button();
            this.cbAutoOutCard = new System.Windows.Forms.CheckBox();
            this.btCSGetCardToTemp = new System.Windows.Forms.Button();
            this.cbAutoChangeCardSlotY = new System.Windows.Forms.ComboBox();
            this.nudCSGotoIndexY = new System.Windows.Forms.NumericUpDown();
            this.btnGotoCardSlot = new System.Windows.Forms.Button();
            this.cbInited = new System.Windows.Forms.CheckBox();
            this.label76 = new System.Windows.Forms.Label();
            this.cbICReaderPort = new System.Windows.Forms.ComboBox();
            this.label75 = new System.Windows.Forms.Label();
            this.cbTKQPort = new System.Windows.Forms.ComboBox();
            this.btnFKQ_Init = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbBoardPort = new System.Windows.Forms.ComboBox();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tm_auto_out_card = new System.Windows.Forms.Timer(this.components);
            this.tm_auto_in_card = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudCSGotoIndexX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCSGotoIndexY)).BeginInit();
            this.SuspendLayout();
            // 
            // cbAutoChangeCardSlotX
            // 
            this.cbAutoChangeCardSlotX.FormattingEnabled = true;
            this.cbAutoChangeCardSlotX.Items.AddRange(new object[] {
            "不处理",
            "自动加1",
            "自动减1",
            "自动加2",
            "自动减2"});
            this.cbAutoChangeCardSlotX.Location = new System.Drawing.Point(56, 191);
            this.cbAutoChangeCardSlotX.Name = "cbAutoChangeCardSlotX";
            this.cbAutoChangeCardSlotX.Size = new System.Drawing.Size(52, 20);
            this.cbAutoChangeCardSlotX.TabIndex = 75;
            // 
            // btCSPutCardToBadSlot
            // 
            this.btCSPutCardToBadSlot.Location = new System.Drawing.Point(521, 212);
            this.btCSPutCardToBadSlot.Name = "btCSPutCardToBadSlot";
            this.btCSPutCardToBadSlot.Size = new System.Drawing.Size(98, 21);
            this.btCSPutCardToBadSlot.TabIndex = 74;
            this.btCSPutCardToBadSlot.Text = "入卡到坏卡槽";
            this.btCSPutCardToBadSlot.UseVisualStyleBackColor = true;
            // 
            // btnCSGetCardToBadSlot
            // 
            this.btnCSGetCardToBadSlot.Location = new System.Drawing.Point(521, 141);
            this.btnCSGetCardToBadSlot.Name = "btnCSGetCardToBadSlot";
            this.btnCSGetCardToBadSlot.Size = new System.Drawing.Size(98, 21);
            this.btnCSGetCardToBadSlot.TabIndex = 73;
            this.btnCSGetCardToBadSlot.Text = "取卡到废卡槽";
            this.btnCSGetCardToBadSlot.UseVisualStyleBackColor = true;
            this.btnCSGetCardToBadSlot.Click += new System.EventHandler(this.btnCSGetCardToBadSlot_Click);
            // 
            // btnCSGetCardToUser
            // 
            this.btnCSGetCardToUser.Location = new System.Drawing.Point(521, 120);
            this.btnCSGetCardToUser.Name = "btnCSGetCardToUser";
            this.btnCSGetCardToUser.Size = new System.Drawing.Size(98, 21);
            this.btnCSGetCardToUser.TabIndex = 72;
            this.btnCSGetCardToUser.Text = "取卡到用户";
            this.btnCSGetCardToUser.UseVisualStyleBackColor = true;
            this.btnCSGetCardToUser.Click += new System.EventHandler(this.btnCSGetCardToUser_Click);
            // 
            // btCSGetCardToICReader
            // 
            this.btCSGetCardToICReader.Location = new System.Drawing.Point(398, 130);
            this.btCSGetCardToICReader.Name = "btCSGetCardToICReader";
            this.btCSGetCardToICReader.Size = new System.Drawing.Size(117, 23);
            this.btCSGetCardToICReader.TabIndex = 71;
            this.btCSGetCardToICReader.Text = "取卡到读卡器";
            this.btCSGetCardToICReader.UseVisualStyleBackColor = true;
            this.btCSGetCardToICReader.Click += new System.EventHandler(this.btCSGetCardToICReader_Click);
            // 
            // btCSPutCardToICReader
            // 
            this.btCSPutCardToICReader.Location = new System.Drawing.Point(294, 202);
            this.btCSPutCardToICReader.Name = "btCSPutCardToICReader";
            this.btCSPutCardToICReader.Size = new System.Drawing.Size(98, 23);
            this.btCSPutCardToICReader.TabIndex = 70;
            this.btCSPutCardToICReader.Text = "入卡到读卡器";
            this.btCSPutCardToICReader.UseVisualStyleBackColor = true;
            this.btCSPutCardToICReader.Click += new System.EventHandler(this.btCSPutCardToICReader_Click);
            // 
            // btCSPutCardToCS
            // 
            this.btCSPutCardToCS.Location = new System.Drawing.Point(521, 191);
            this.btCSPutCardToCS.Name = "btCSPutCardToCS";
            this.btCSPutCardToCS.Size = new System.Drawing.Size(98, 21);
            this.btCSPutCardToCS.TabIndex = 69;
            this.btCSPutCardToCS.Text = "入卡到卡槽";
            this.btCSPutCardToCS.UseVisualStyleBackColor = true;
            this.btCSPutCardToCS.Click += new System.EventHandler(this.btCSPutCardToCS_Click);
            // 
            // nudCSGotoIndexX
            // 
            this.nudCSGotoIndexX.Location = new System.Drawing.Point(56, 157);
            this.nudCSGotoIndexX.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudCSGotoIndexX.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.nudCSGotoIndexX.Name = "nudCSGotoIndexX";
            this.nudCSGotoIndexX.Size = new System.Drawing.Size(52, 21);
            this.nudCSGotoIndexX.TabIndex = 68;
            this.nudCSGotoIndexX.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cbAutoPutCard
            // 
            this.cbAutoPutCard.AutoSize = true;
            this.cbAutoPutCard.Location = new System.Drawing.Point(686, 202);
            this.cbAutoPutCard.Name = "cbAutoPutCard";
            this.cbAutoPutCard.Size = new System.Drawing.Size(72, 16);
            this.cbAutoPutCard.TabIndex = 67;
            this.cbAutoPutCard.Text = "自动放卡";
            this.cbAutoPutCard.UseVisualStyleBackColor = true;
            this.cbAutoPutCard.CheckedChanged += new System.EventHandler(this.cbAutoPutCard_CheckedChanged);
            // 
            // btCSPutCardToTemp
            // 
            this.btCSPutCardToTemp.Location = new System.Drawing.Point(398, 202);
            this.btCSPutCardToTemp.Name = "btCSPutCardToTemp";
            this.btCSPutCardToTemp.Size = new System.Drawing.Size(117, 23);
            this.btCSPutCardToTemp.TabIndex = 66;
            this.btCSPutCardToTemp.Text = "入卡到中转机构";
            this.btCSPutCardToTemp.UseVisualStyleBackColor = true;
            this.btCSPutCardToTemp.Click += new System.EventHandler(this.btCSPutCardToTemp_Click);
            // 
            // cbAutoOutCard
            // 
            this.cbAutoOutCard.AutoSize = true;
            this.cbAutoOutCard.Location = new System.Drawing.Point(686, 134);
            this.cbAutoOutCard.Name = "cbAutoOutCard";
            this.cbAutoOutCard.Size = new System.Drawing.Size(72, 16);
            this.cbAutoOutCard.TabIndex = 65;
            this.cbAutoOutCard.Text = "自动出卡";
            this.cbAutoOutCard.UseVisualStyleBackColor = true;
            this.cbAutoOutCard.CheckedChanged += new System.EventHandler(this.cbAutoOutCard_CheckedChanged);
            // 
            // btCSGetCardToTemp
            // 
            this.btCSGetCardToTemp.Location = new System.Drawing.Point(292, 130);
            this.btCSGetCardToTemp.Name = "btCSGetCardToTemp";
            this.btCSGetCardToTemp.Size = new System.Drawing.Size(98, 23);
            this.btCSGetCardToTemp.TabIndex = 64;
            this.btCSGetCardToTemp.Text = "取卡到中转机构";
            this.btCSGetCardToTemp.UseVisualStyleBackColor = true;
            this.btCSGetCardToTemp.Click += new System.EventHandler(this.btCSGetCardToTemp_Click);
            // 
            // cbAutoChangeCardSlotY
            // 
            this.cbAutoChangeCardSlotY.FormattingEnabled = true;
            this.cbAutoChangeCardSlotY.Items.AddRange(new object[] {
            "不处理",
            "自动加1",
            "自动减1",
            "自动加2",
            "自动减2"});
            this.cbAutoChangeCardSlotY.Location = new System.Drawing.Point(114, 191);
            this.cbAutoChangeCardSlotY.Name = "cbAutoChangeCardSlotY";
            this.cbAutoChangeCardSlotY.Size = new System.Drawing.Size(62, 20);
            this.cbAutoChangeCardSlotY.TabIndex = 63;
            // 
            // nudCSGotoIndexY
            // 
            this.nudCSGotoIndexY.Location = new System.Drawing.Point(114, 157);
            this.nudCSGotoIndexY.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudCSGotoIndexY.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.nudCSGotoIndexY.Name = "nudCSGotoIndexY";
            this.nudCSGotoIndexY.Size = new System.Drawing.Size(62, 21);
            this.nudCSGotoIndexY.TabIndex = 62;
            this.nudCSGotoIndexY.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // btnGotoCardSlot
            // 
            this.btnGotoCardSlot.Location = new System.Drawing.Point(206, 130);
            this.btnGotoCardSlot.Name = "btnGotoCardSlot";
            this.btnGotoCardSlot.Size = new System.Drawing.Size(75, 23);
            this.btnGotoCardSlot.TabIndex = 61;
            this.btnGotoCardSlot.Text = "去卡槽";
            this.btnGotoCardSlot.UseVisualStyleBackColor = true;
            this.btnGotoCardSlot.Click += new System.EventHandler(this.btnGotoCardSlot_Click);
            // 
            // cbInited
            // 
            this.cbInited.AutoSize = true;
            this.cbInited.Checked = true;
            this.cbInited.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbInited.Location = new System.Drawing.Point(631, 19);
            this.cbInited.Name = "cbInited";
            this.cbInited.Size = new System.Drawing.Size(60, 16);
            this.cbInited.TabIndex = 81;
            this.cbInited.Text = "初始化";
            this.cbInited.UseVisualStyleBackColor = true;
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Location = new System.Drawing.Point(369, 20);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(89, 12);
            this.label76.TabIndex = 80;
            this.label76.Text = "电动读卡器串口";
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
            this.cbICReaderPort.Location = new System.Drawing.Point(464, 15);
            this.cbICReaderPort.Name = "cbICReaderPort";
            this.cbICReaderPort.Size = new System.Drawing.Size(51, 20);
            this.cbICReaderPort.TabIndex = 79;
            this.cbICReaderPort.Text = "5";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Location = new System.Drawing.Point(230, 20);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(77, 12);
            this.label75.TabIndex = 78;
            this.label75.Text = "推卡装置串口";
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
            this.cbTKQPort.Location = new System.Drawing.Point(313, 17);
            this.cbTKQPort.Name = "cbTKQPort";
            this.cbTKQPort.Size = new System.Drawing.Size(38, 20);
            this.cbTKQPort.TabIndex = 77;
            this.cbTKQPort.Text = "6";
            // 
            // btnFKQ_Init
            // 
            this.btnFKQ_Init.Location = new System.Drawing.Point(521, 12);
            this.btnFKQ_Init.Name = "btnFKQ_Init";
            this.btnFKQ_Init.Size = new System.Drawing.Size(98, 23);
            this.btnFKQ_Init.TabIndex = 76;
            this.btnFKQ_Init.Text = "打开设备";
            this.btnFKQ_Init.UseVisualStyleBackColor = true;
            this.btnFKQ_Init.Click += new System.EventHandler(this.btnFKQ_Init_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 83;
            this.label1.Text = "控制板串口";
            // 
            // cbBoardPort
            // 
            this.cbBoardPort.FormattingEnabled = true;
            this.cbBoardPort.Items.AddRange(new object[] {
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
            this.cbBoardPort.Location = new System.Drawing.Point(138, 17);
            this.cbBoardPort.Name = "cbBoardPort";
            this.cbBoardPort.Size = new System.Drawing.Size(38, 20);
            this.cbBoardPort.TabIndex = 82;
            this.cbBoardPort.Text = "4";
            // 
            // ofd
            // 
            this.ofd.FileName = "openFileDialog1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(54, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 84;
            this.label2.Text = "X座标";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(112, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 85;
            this.label3.Text = "Y座标";
            // 
            // tm_auto_out_card
            // 
            this.tm_auto_out_card.Interval = 500;
            this.tm_auto_out_card.Tick += new System.EventHandler(this.tm_auto_out_card_Tick);
            // 
            // tm_auto_in_card
            // 
            this.tm_auto_in_card.Interval = 500;
            this.tm_auto_in_card.Tick += new System.EventHandler(this.tm_auto_in_card_Tick);
            // 
            // FrmFKJDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 281);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbBoardPort);
            this.Controls.Add(this.cbInited);
            this.Controls.Add(this.label76);
            this.Controls.Add(this.cbICReaderPort);
            this.Controls.Add(this.label75);
            this.Controls.Add(this.cbTKQPort);
            this.Controls.Add(this.btnFKQ_Init);
            this.Controls.Add(this.cbAutoChangeCardSlotX);
            this.Controls.Add(this.btCSPutCardToBadSlot);
            this.Controls.Add(this.btnCSGetCardToBadSlot);
            this.Controls.Add(this.btnCSGetCardToUser);
            this.Controls.Add(this.btCSGetCardToICReader);
            this.Controls.Add(this.btCSPutCardToICReader);
            this.Controls.Add(this.btCSPutCardToCS);
            this.Controls.Add(this.nudCSGotoIndexX);
            this.Controls.Add(this.cbAutoPutCard);
            this.Controls.Add(this.btCSPutCardToTemp);
            this.Controls.Add(this.cbAutoOutCard);
            this.Controls.Add(this.btCSGetCardToTemp);
            this.Controls.Add(this.cbAutoChangeCardSlotY);
            this.Controls.Add(this.nudCSGotoIndexY);
            this.Controls.Add(this.btnGotoCardSlot);
            this.Name = "FrmFKJDemo";
            this.Text = "发卡器演示程序";
            ((System.ComponentModel.ISupportInitialize)(this.nudCSGotoIndexX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCSGotoIndexY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbAutoChangeCardSlotX;
        private System.Windows.Forms.Button btCSPutCardToBadSlot;
        private System.Windows.Forms.Button btnCSGetCardToBadSlot;
        private System.Windows.Forms.Button btnCSGetCardToUser;
        private System.Windows.Forms.Button btCSGetCardToICReader;
        private System.Windows.Forms.Button btCSPutCardToICReader;
        private System.Windows.Forms.Button btCSPutCardToCS;
        private System.Windows.Forms.NumericUpDown nudCSGotoIndexX;
        private System.Windows.Forms.CheckBox cbAutoPutCard;
        private System.Windows.Forms.Button btCSPutCardToTemp;
        private System.Windows.Forms.CheckBox cbAutoOutCard;
        private System.Windows.Forms.Button btCSGetCardToTemp;
        private System.Windows.Forms.ComboBox cbAutoChangeCardSlotY;
        private System.Windows.Forms.NumericUpDown nudCSGotoIndexY;
        private System.Windows.Forms.Button btnGotoCardSlot;
        private System.Windows.Forms.CheckBox cbInited;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.ComboBox cbICReaderPort;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.ComboBox cbTKQPort;
        private System.Windows.Forms.Button btnFKQ_Init;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbBoardPort;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer tm_auto_out_card;
        private System.Windows.Forms.Timer tm_auto_in_card;
    }
}