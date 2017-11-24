using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExcelOperationTest
{
    public partial class excelPathChangeForm : Form
    {
        public excelPathChangeForm()
        {
            InitializeComponent();
        }

        Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private void excelPathChangeForm_Load(object sender, EventArgs e)
        {
            string excelPath = config.AppSettings.Settings["excelPath"].Value;
            txtExcelPath.Text = excelPath;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExcelPathChange_Click(object sender, EventArgs e)
        {
            MessageBoxButtons mButton = MessageBoxButtons.YesNo;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtExcelPath.Text = fbd.SelectedPath;
                DialogResult dr = MessageBox.Show("确定修改路径为：\r\n\n" + txtExcelPath.Text, "注意！", mButton, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    config.AppSettings.Settings["excelPath"].Value = txtExcelPath.Text;
                    config.Save();
                    MessageBox.Show("修改成功！");
                    this.Close();
                }
            }
        }
    }
}
