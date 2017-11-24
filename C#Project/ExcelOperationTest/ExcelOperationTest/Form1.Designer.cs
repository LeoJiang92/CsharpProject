namespace ExcelOperationTest
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnInsertText = new System.Windows.Forms.Button();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.txtExcelYLine = new System.Windows.Forms.TextBox();
            this.txtExcelXLine = new System.Windows.Forms.TextBox();
            this.btnDeleteText = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCreateSheet = new System.Windows.Forms.Button();
            this.btnDeleteSheet = new System.Windows.Forms.Button();
            this.txtSheetName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCreateExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDeleteExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOpenExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSetSavePath = new System.Windows.Forms.ToolStripMenuItem();
            this.配置文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddConfigItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDeleteConfigItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInsertText
            // 
            this.btnInsertText.Location = new System.Drawing.Point(372, 61);
            this.btnInsertText.Name = "btnInsertText";
            this.btnInsertText.Size = new System.Drawing.Size(75, 23);
            this.btnInsertText.TabIndex = 0;
            this.btnInsertText.Text = "添加内容";
            this.btnInsertText.UseVisualStyleBackColor = true;
            this.btnInsertText.Click += new System.EventHandler(this.btnInsertText_Click);
            // 
            // txtContent
            // 
            this.txtContent.Location = new System.Drawing.Point(12, 90);
            this.txtContent.Multiline = true;
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(433, 194);
            this.txtContent.TabIndex = 1;
            // 
            // txtExcelYLine
            // 
            this.txtExcelYLine.Location = new System.Drawing.Point(35, 61);
            this.txtExcelYLine.Name = "txtExcelYLine";
            this.txtExcelYLine.Size = new System.Drawing.Size(70, 21);
            this.txtExcelYLine.TabIndex = 2;
            // 
            // txtExcelXLine
            // 
            this.txtExcelXLine.Location = new System.Drawing.Point(134, 61);
            this.txtExcelXLine.Name = "txtExcelXLine";
            this.txtExcelXLine.Size = new System.Drawing.Size(70, 21);
            this.txtExcelXLine.TabIndex = 3;
            // 
            // btnDeleteText
            // 
            this.btnDeleteText.Location = new System.Drawing.Point(291, 61);
            this.btnDeleteText.Name = "btnDeleteText";
            this.btnDeleteText.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteText.TabIndex = 4;
            this.btnDeleteText.Text = "删除内容";
            this.btnDeleteText.UseVisualStyleBackColor = true;
            this.btnDeleteText.Click += new System.EventHandler(this.btnDeleteText_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "行";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "列";
            // 
            // btnCreateSheet
            // 
            this.btnCreateSheet.Location = new System.Drawing.Point(291, 32);
            this.btnCreateSheet.Name = "btnCreateSheet";
            this.btnCreateSheet.Size = new System.Drawing.Size(75, 23);
            this.btnCreateSheet.TabIndex = 9;
            this.btnCreateSheet.Text = "创建工作表";
            this.btnCreateSheet.UseVisualStyleBackColor = true;
            this.btnCreateSheet.Click += new System.EventHandler(this.btnCreateSheet_Click);
            // 
            // btnDeleteSheet
            // 
            this.btnDeleteSheet.Location = new System.Drawing.Point(372, 32);
            this.btnDeleteSheet.Name = "btnDeleteSheet";
            this.btnDeleteSheet.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteSheet.TabIndex = 10;
            this.btnDeleteSheet.Text = "删除工作表";
            this.btnDeleteSheet.UseVisualStyleBackColor = true;
            this.btnDeleteSheet.Click += new System.EventHandler(this.btnDeleteSheet_Click);
            // 
            // txtSheetName
            // 
            this.txtSheetName.Location = new System.Drawing.Point(83, 30);
            this.txtSheetName.Name = "txtSheetName";
            this.txtSheetName.Size = new System.Drawing.Size(202, 21);
            this.txtSheetName.TabIndex = 12;
            this.txtSheetName.Text = "Test1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "工作表名称";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.配置文件ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(455, 25);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCreateExcel,
            this.btnDeleteExcel,
            this.btnOpenExcel,
            this.btnSetSavePath});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(49, 21);
            this.toolStripMenuItem1.Text = "Excel";
            // 
            // btnCreateExcel
            // 
            this.btnCreateExcel.Name = "btnCreateExcel";
            this.btnCreateExcel.Size = new System.Drawing.Size(152, 22);
            this.btnCreateExcel.Text = "创建Excel";
            this.btnCreateExcel.Click += new System.EventHandler(this.btnCreateExcel_Click);
            // 
            // btnDeleteExcel
            // 
            this.btnDeleteExcel.Name = "btnDeleteExcel";
            this.btnDeleteExcel.Size = new System.Drawing.Size(152, 22);
            this.btnDeleteExcel.Text = "删除Excel";
            this.btnDeleteExcel.Click += new System.EventHandler(this.btnDeleteExcel_Click);
            // 
            // btnOpenExcel
            // 
            this.btnOpenExcel.Name = "btnOpenExcel";
            this.btnOpenExcel.Size = new System.Drawing.Size(152, 22);
            this.btnOpenExcel.Text = "打开Excel";
            this.btnOpenExcel.Click += new System.EventHandler(this.btnOpenExcel_Click);
            // 
            // btnSetSavePath
            // 
            this.btnSetSavePath.Name = "btnSetSavePath";
            this.btnSetSavePath.Size = new System.Drawing.Size(152, 22);
            this.btnSetSavePath.Text = "设置保存路径";
            this.btnSetSavePath.Click += new System.EventHandler(this.btnSetSavePath_Click);
            // 
            // 配置文件ToolStripMenuItem
            // 
            this.配置文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddConfigItem,
            this.btnDeleteConfigItem});
            this.配置文件ToolStripMenuItem.Name = "配置文件ToolStripMenuItem";
            this.配置文件ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.配置文件ToolStripMenuItem.Text = "配置文件";
            // 
            // btnAddConfigItem
            // 
            this.btnAddConfigItem.Name = "btnAddConfigItem";
            this.btnAddConfigItem.Size = new System.Drawing.Size(136, 22);
            this.btnAddConfigItem.Text = "添加配置项";
            // 
            // btnDeleteConfigItem
            // 
            this.btnDeleteConfigItem.Name = "btnDeleteConfigItem";
            this.btnDeleteConfigItem.Size = new System.Drawing.Size(136, 22);
            this.btnDeleteConfigItem.Text = "删除配置项";
            this.btnDeleteConfigItem.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(210, 61);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 313);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSheetName);
            this.Controls.Add(this.btnDeleteSheet);
            this.Controls.Add(this.btnCreateSheet);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDeleteText);
            this.Controls.Add(this.txtExcelXLine);
            this.Controls.Add(this.txtExcelYLine);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.btnInsertText);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInsertText;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.TextBox txtExcelYLine;
        private System.Windows.Forms.TextBox txtExcelXLine;
        private System.Windows.Forms.Button btnDeleteText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCreateSheet;
        private System.Windows.Forms.Button btnDeleteSheet;
        private System.Windows.Forms.TextBox txtSheetName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem btnCreateExcel;
        private System.Windows.Forms.ToolStripMenuItem btnDeleteExcel;
        private System.Windows.Forms.ToolStripMenuItem btnOpenExcel;
        private System.Windows.Forms.ToolStripMenuItem btnSetSavePath;
        private System.Windows.Forms.ToolStripMenuItem 配置文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem btnAddConfigItem;
        private System.Windows.Forms.ToolStripMenuItem btnDeleteConfigItem;
        private System.Windows.Forms.Button button1;
    }
}

