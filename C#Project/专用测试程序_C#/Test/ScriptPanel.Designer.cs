namespace MotorControlBoard
{
    partial class ScriptPanel
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
            this.listScript = new System.Windows.Forms.ListBox();
            this.tbScriptLine = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnAddLine = new System.Windows.Forms.Button();
            this.btnUP = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnDeleteLine = new System.Windows.Forms.Button();
            this.tbScript = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listScript
            // 
            this.listScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listScript.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listScript.FormattingEnabled = true;
            this.listScript.Location = new System.Drawing.Point(0, 0);
            this.listScript.Name = "listScript";
            this.listScript.Size = new System.Drawing.Size(653, 303);
            this.listScript.TabIndex = 0;
            this.listScript.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listScript_DrawItem);
            this.listScript.SelectedIndexChanged += new System.EventHandler(this.listScript_SelectedIndexChanged);
            this.listScript.DoubleClick += new System.EventHandler(this.listScript_DoubleClick);
            // 
            // tbScriptLine
            // 
            this.tbScriptLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScriptLine.Location = new System.Drawing.Point(3, 310);
            this.tbScriptLine.Name = "tbScriptLine";
            this.tbScriptLine.Size = new System.Drawing.Size(376, 21);
            this.tbScriptLine.TabIndex = 1;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(384, 308);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(43, 23);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "修改";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddLine.Location = new System.Drawing.Point(433, 308);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(44, 23);
            this.btnAddLine.TabIndex = 3;
            this.btnAddLine.Text = "增加";
            this.btnAddLine.UseVisualStyleBackColor = true;
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // btnUP
            // 
            this.btnUP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUP.Location = new System.Drawing.Point(556, 308);
            this.btnUP.Name = "btnUP";
            this.btnUP.Size = new System.Drawing.Size(44, 23);
            this.btnUP.TabIndex = 4;
            this.btnUP.Text = "上移";
            this.btnUP.UseVisualStyleBackColor = true;
            this.btnUP.Click += new System.EventHandler(this.btnUP_Click);
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDown.Location = new System.Drawing.Point(606, 308);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(44, 23);
            this.btnDown.TabIndex = 5;
            this.btnDown.Text = "下移";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteLine.Location = new System.Drawing.Point(483, 308);
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.Size = new System.Drawing.Size(44, 23);
            this.btnDeleteLine.TabIndex = 6;
            this.btnDeleteLine.Text = "删除";
            this.btnDeleteLine.UseVisualStyleBackColor = true;
            this.btnDeleteLine.Click += new System.EventHandler(this.btnDeleteLine_Click);
            // 
            // tbScript
            // 
            this.tbScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScript.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.tbScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbScript.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbScript.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.tbScript.Location = new System.Drawing.Point(0, 0);
            this.tbScript.Multiline = true;
            this.tbScript.Name = "tbScript";
            this.tbScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbScript.Size = new System.Drawing.Size(653, 50);
            this.tbScript.TabIndex = 7;
            this.tbScript.Visible = false;
            this.tbScript.DoubleClick += new System.EventHandler(this.tbScript_DoubleClick);
            // 
            // ScriptPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbScript);
            this.Controls.Add(this.btnDeleteLine);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUP);
            this.Controls.Add(this.btnAddLine);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.tbScriptLine);
            this.Controls.Add(this.listScript);
            this.Name = "ScriptPanel";
            this.Size = new System.Drawing.Size(653, 341);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listScript;
        private System.Windows.Forms.TextBox tbScriptLine;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnAddLine;
        private System.Windows.Forms.Button btnUP;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnDeleteLine;
        private System.Windows.Forms.TextBox tbScript;
    }
}
