using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MotorControlBoard
{
    public partial class ArrayValueEditor : UserControl
    {
        public ArrayValueEditor()
        {
            InitializeComponent();
        }

        string mText = "值";
        public string Text
        {
            get { return mText; }
            set { mText = value; }
        }

        int mArraySize = 4;
        public int ArraySize
        {
            get { return mArraySize; }
            set { mArraySize = value; }
        }

        int mColumnSize = 1;
        public int ColumnSize
        {
            get { return mArraySize; }
            set { mArraySize = value; }
        }
    }
}
