namespace ExcelOperationTest
{
    partial class excelPathChangeForm
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
            this.txtExcelPath = new System.Windows.Forms.TextBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnExcelPathChange = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtExcelPath
            // 
            this.txtExcelPath.Location = new System.Drawing.Point(12, 12);
            this.txtExcelPath.Name = "txtExcelPath";
            this.txtExcelPath.Size = new System.Drawing.Size(387, 21);
            this.txtExcelPath.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(325, 39);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "取消";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnExcelPathChange
            // 
            this.btnExcelPathChange.Location = new System.Drawing.Point(244, 39);
            this.btnExcelPathChange.Name = "btnExcelPathChange";
            this.btnExcelPathChange.Size = new System.Drawing.Size(75, 23);
            this.btnExcelPathChange.TabIndex = 2;
            this.btnExcelPathChange.Text = "修改";
            this.btnExcelPathChange.UseVisualStyleBackColor = true;
            this.btnExcelPathChange.Click += new System.EventHandler(this.btnExcelPathChange_Click);
            // 
            // excelPathChangeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 72);
            this.Controls.Add(this.btnExcelPathChange);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.txtExcelPath);
            this.Name = "excelPathChangeForm";
            this.Text = "excelPathChangeForm";
            this.Load += new System.EventHandler(this.excelPathChangeForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtExcelPath;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnExcelPathChange;
    }
}