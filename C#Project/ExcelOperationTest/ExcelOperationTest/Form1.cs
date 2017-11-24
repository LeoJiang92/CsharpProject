using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft;
using System.IO;
using System.Reflection;
using System.Configuration;

namespace ExcelOperationTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private void btnCreateExcel_Click(object sender, EventArgs e)
        {
            
        }

        private void btnDeleteExcel_Click(object sender, EventArgs e)
        {

        }

        private void btnOpenExcel_Click(object sender, EventArgs e)
        {
        }

        private void btnSetSavePath_Click(object sender, EventArgs e)
        {
            excelPathChangeForm epcf = new excelPathChangeForm();
            epcf.Show();
        }

        private void btnCreateSheet_Click(object sender, EventArgs e)
        {

        }

        private void btnDeleteSheet_Click(object sender, EventArgs e)
        {

        }

        private void btnDeleteText_Click(object sender, EventArgs e)
        {

        }

        private void btnInsertText_Click(object sender, EventArgs e)
        {

        }

    }
}
