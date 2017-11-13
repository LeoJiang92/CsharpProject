using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MotorControlBoard
{
    public partial class FrmFKJDemo : Form
    {
        public FrmFKJDemo()
        {
            InitializeComponent();
        }

        IFZJ fkj = null;
        private void btnFKQ_Init_Click(object sender, EventArgs e)
        {
            ofd.Title = "选择参数文件";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (fkj != null)
                {
                    fkj.CloseDev();
                    fkj = null;
                }

                fkj = new FKJXXX_V1(int.Parse(cbBoardPort.Text), 0, Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")) + @"\发卡机\FKJ1000_V1\");
                fkj.LoadConfigFile(ofd.FileName);
                fkj.mInCardReaderPort = int.Parse(cbICReaderPort.Text);
                fkj.mTKQPort = int.Parse(cbTKQPort.Text);
                if (cbInited.Checked)
                    fkj.DevInit();
                MessageBox.Show("打开控制板成功");
            }
        }

        private Boolean OutCardFlag = false;
        private void btnGotoCardSlot_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.MotoGotoCardSlot(false, (int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value, fkj.cfgGotoSlotWaitCompleted == 1) == 0)
            {
                OutCardFlag = true;
            }
        }

        private void btCSGetCardToTemp_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.GetCardToTransferCar((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value) == 0)
                OutCardFlag = true;
            else
                MessageBox.Show("取卡到中转区失败");
        }

        private void btCSGetCardToICReader_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.GetCardToICReader() == 0)
                OutCardFlag = true;
            else
                MessageBox.Show("取卡到IC读卡器失败");
        }

        private void btnCSGetCardToUser_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.GetCardToUser(false, 10) == 0)
            {
                switch (cbAutoChangeCardSlotY.SelectedIndex)
                {
                    case 1: nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % fkj.cfgY_SlotCount; break;
                    case 2: nudCSGotoIndexY.Value = (nudCSGotoIndexY.Value == 0) ? fkj.cfgY_SlotCount : (nudCSGotoIndexY.Value - 1); break;
                    case 3: nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 2) % fkj.cfgY_SlotCount; break;
                    case 4: nudCSGotoIndexY.Value = (nudCSGotoIndexY.Value < 2) ? fkj.cfgY_SlotCount : (nudCSGotoIndexY.Value - 2); break;
                }
                switch (cbAutoChangeCardSlotX.SelectedIndex)
                {
                    case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % fkj.cfgX_SlotCount; break;
                    case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? fkj.cfgX_SlotCount : (nudCSGotoIndexY.Value - 1); break;
                    case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % fkj.cfgX_SlotCount; break;
                    case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? fkj.cfgX_SlotCount : (nudCSGotoIndexY.Value - 2); break;
                }
                if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                {
                    if (nudCSGotoIndexX.Value == 0)
                    {
                        nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % fkj.cfgY_SlotCount;
                        if (nudCSGotoIndexY.Value == 0)
                        {
                            return;
                        }
                    }
                }
                OutCardFlag = true;
            }
        }

        private void btnCSGetCardToBadSlot_Click(object sender, EventArgs e)
        {
            uint r = fkj.GetCardToBadCardSlot(0);
            if (r != 0)
            {
                MessageBox.Show("执行失败R=" + r.ToString());
            }
        }

        private void btCSPutCardToICReader_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.PutCardToICReader() == 0)
                OutCardFlag = true;
            else
                MessageBox.Show("放卡到读卡器失败!");
        }

        private void btCSPutCardToTemp_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.PutCardToTransferCar() == 0)
                OutCardFlag = true;
        }

        private void btCSPutCardToCS_Click(object sender, EventArgs e)
        {
            OutCardFlag = false;
            if (fkj.PutCardToCardSlot((int)nudCSGotoIndexX.Value, (int)nudCSGotoIndexY.Value) == 0)
            {
                switch (cbAutoChangeCardSlotY.SelectedIndex)
                {
                    case 1: nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % fkj.cfgY_SlotCount; break;
                    case 2: nudCSGotoIndexY.Value = (nudCSGotoIndexY.Value == 0) ? fkj.cfgY_SlotCount : (nudCSGotoIndexY.Value - 1); break;
                    case 3: nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 2) % fkj.cfgY_SlotCount; break;
                    case 4: nudCSGotoIndexY.Value = (nudCSGotoIndexY.Value < 2) ? fkj.cfgY_SlotCount : (nudCSGotoIndexY.Value - 2); break;
                }
                switch (cbAutoChangeCardSlotX.SelectedIndex)
                {
                    case 1: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 1) % fkj.cfgX_SlotCount; break;
                    case 2: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value == 0) ? fkj.cfgX_SlotCount : (nudCSGotoIndexY.Value - 1); break;
                    case 3: nudCSGotoIndexX.Value = ((int)(nudCSGotoIndexX.Value) + 2) % fkj.cfgX_SlotCount; break;
                    case 4: nudCSGotoIndexX.Value = (nudCSGotoIndexX.Value < 2) ? fkj.cfgX_SlotCount : (nudCSGotoIndexY.Value - 2); break;
                }
                if (cbAutoChangeCardSlotY.SelectedIndex <= 0 && cbAutoChangeCardSlotX.SelectedIndex > 0)
                {
                    if (nudCSGotoIndexX.Value == 0)
                    {
                        nudCSGotoIndexY.Value = ((int)(nudCSGotoIndexY.Value) + 1) % fkj.cfgY_SlotCount;//取余卡槽数
                        if (nudCSGotoIndexY.Value == 0)
                        {
                            return;
                        }
                    }
                }
                OutCardFlag = true;
            }
            else
                MessageBox.Show("入卡到卡槽失败");
        }

        private void tm_auto_in_card_Tick(object sender, EventArgs e)
        {
            tm_auto_in_card.Enabled = false;
            if (cbAutoChangeCardSlotX.SelectedIndex <= 0 && cbAutoChangeCardSlotY.SelectedIndex <= 0)
            {
                MessageBox.Show("自动执行必须使卡槽自增或自减");
                return;
            }
            btCSPutCardToICReader_Click(btCSPutCardToICReader, null);
            if (!OutCardFlag)
            {
                MessageBox.Show("放卡到IC读卡器失败");
                return;
            }
            btCSPutCardToTemp_Click(btCSPutCardToTemp, null);
            if (!OutCardFlag)
            {
                MessageBox.Show("放卡到中转器失败");
                return;
            }
            btCSPutCardToCS_Click(btCSPutCardToCS, null);
            if (!OutCardFlag)
            {
                MessageBox.Show("放卡到卡槽失败");
                return;
            }
            Application.DoEvents();
            tm_auto_in_card.Enabled = cbAutoPutCard.Checked;
        }

        private void tm_auto_out_card_Tick(object sender, EventArgs e)
        {
            tm_auto_out_card.Enabled = false;
            if (cbAutoChangeCardSlotX.SelectedIndex <= 0 && cbAutoChangeCardSlotY.SelectedIndex <= 0)
            {
                MessageBox.Show("自动执行必须使卡槽自增或自减");
                return;
            }
            Application.DoEvents();
            btCSGetCardToTemp_Click(btCSGetCardToTemp, null);
            if (!this.OutCardFlag)
            {
                MessageBox.Show("取卡到中转机构失败");
                return;
            }
            Application.DoEvents();
            btCSGetCardToICReader_Click(btCSGetCardToICReader, null);
            if (!this.OutCardFlag)
            {
                MessageBox.Show("取卡到读卡器失败");
                return;
            }
            Application.DoEvents();
            btnCSGetCardToUser_Click(btnCSGetCardToUser, null);
            if (!this.OutCardFlag)
            {
                MessageBox.Show("取卡到用户失败");
                return;
            }
            Application.DoEvents();
            tm_auto_out_card.Enabled = cbAutoOutCard.Checked;
        }

        private void cbAutoOutCard_CheckedChanged(object sender, EventArgs e)
        {
            tm_auto_out_card.Enabled = cbAutoOutCard.Checked;
        }

        private void cbAutoPutCard_CheckedChanged(object sender, EventArgs e)
        {
            tm_auto_in_card.Enabled = cbAutoPutCard.Checked;
        }
    }
}
