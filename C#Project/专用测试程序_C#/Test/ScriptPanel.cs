using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MotorControlBoard
{
    public partial class ScriptPanel : UserControl
    {
        const int ST_LINE = 1;
        const int ST_NO_CONDITION = 2;//直接执行
        const int ST_IF_STATEMENT = 3;
        const int ST_SET_STATEMENT = 4;
        const int ST_TEST_STATEMENT = 5;
        const int ST_ERROR = 9;
        public ScriptPanel()
        {
            InitializeComponent();
        }
        private Dictionary<String, String> mParams = new Dictionary<String, String>();

        bool mIsTextMode = false;
        public Boolean IsTextMode
        {
            get{
                return mIsTextMode;
            }
        }

        //判断是否为赋值语句
        private bool IsSetStatement(string line)
        {
            Regex r = new Regex(@"([^\=]*)\=([^\(]*)\((.*)\)");
            Match m = r.Match(line);
            if (m.Success)
            {
                string paramName = m.Groups[1].Value.ToUpper();
                string cmd = m.Groups[2].Value.Replace(" ", "").ToUpper();
                return cmd.Equals("SET");
            }
            return false;
        }
        private void InitParams(string strScript)
        {
            int curPos = 0;
            string line;
            strScript = strScript.Replace("\r", "") + "\n";
            //Regex r = new Regex(@"([0-9,A-F,a-f]{1})\=([^:]*)\:(.*)");
            Regex r = new Regex(@"([^\=]*)\=([^\(]*)\((.*)\)");
            //替换变量
            Regex rParams0 = new Regex(@"\[([^\,,^\]]*)\]");
            Regex rParams1 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\]");
            Regex rParams2 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\,([0-9]{1,5})\]");
            line = ReadLine(strScript, ref curPos);
            try
            {
                while (curPos > 0)
                {
                    Application.DoEvents();
                    if ((line.Replace(" ", "").Length < 4) || (line.Replace(" ", "")[0] == '#'))
                    {
                        line = ReadLine(strScript, ref curPos);
                        continue;
                    }
                    Match m = r.Match(line);
                    if (m.Success)
                    {
                        string paramName = m.Groups[1].Value.ToUpper();
                        string cmd = m.Groups[2].Value.Replace(" ", "").ToUpper();
                        string data = m.Groups[3].Value;
                        string realData = data;
                        //替换变量
                        Match mP0 = rParams0.Match(data);
                        while (mP0.Success)
                        {
                            string pName = mP0.Groups[1].Value.ToUpper();
                            realData = realData.Replace(mP0.Groups[0].Value, this.mParams[pName].ToString());
                            mP0 = mP0.NextMatch();
                        }
                        data = realData;
                        Match mP1 = rParams1.Match(data);
                        while (mP1.Success)
                        {
                            string pName = mP1.Groups[1].Value.ToUpper();
                            int pLen = int.Parse(mP1.Groups[2].Value);
                            pLen = Math.Min(pLen, this.mParams[pName].Length);
                            realData = realData.Replace(mP1.Groups[0].Value, this.mParams[pName].Substring(0, pLen));
                            mP1 = mP1.NextMatch();
                        }
                        data = realData;
                        Match mP2 = rParams2.Match(data);
                        while (mP2.Success)
                        {
                            string pName = mP2.Groups[1].Value.ToUpper();
                            int pStartIndex = int.Parse(mP2.Groups[2].Value);
                            int pLen = int.Parse(mP2.Groups[3].Value);
                            pStartIndex = Math.Min(pStartIndex, this.mParams[pName].Length);
                            pLen = Math.Min(pLen, this.mParams[pName].Length - pStartIndex);
                            realData = realData.Replace(mP2.Groups[0].Value, this.mParams[pName].Substring(pStartIndex, pLen));
                            mP2 = mP2.NextMatch();
                        }
                        
                        Application.DoEvents();
                        if (cmd.Equals("SET"))
                        {
                            mParams[paramName] = realData;
                        }                        
                    }
                    line = ReadLine(strScript, ref curPos);
                }
            }
            catch (Exception e)
            {
                line = ReadLine(strScript, ref curPos);
            }
        }

        public string ReadLine(string str, ref int startIndex)
        {
            int nextPos = str.IndexOf("\n", startIndex);
            string result = "";
            if (nextPos > 0)
                result = str.Substring(startIndex, nextPos - startIndex);
            startIndex = nextPos + 1;
            return result;
        }

        public string ConvertScriptDescript(string str,out int scriptType)
        {
            String des = str;
            Regex r = new Regex(@"([^\=]*)\=([^\(]*)\((.*)\)");
            //替换变量
            Regex rParams0 = new Regex(@"\[([^\,,^\]]*)\]");
            Regex rParams1 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\]");
            Regex rParams2 = new Regex(@"\[([^\,,^\]]*)\,([0-9]{1,5})\,([0-9]{1,5})\]");
            Match m = r.Match(str);
            scriptType = 0;
            try
            {
                if (m.Success)
                {
                    scriptType = ScriptPanel.ST_LINE;
                    string paramName = m.Groups[1].Value.ToUpper();
                    string cmd = m.Groups[2].Value.Replace(" ", "").ToUpper();
                    string data = m.Groups[3].Value;
                    string realData = data;
                    //替换变量
                    Match mP0 = rParams0.Match(data);
                    while (mP0.Success)
                    {
                        string pName = mP0.Groups[1].Value.ToUpper();
                        realData = realData.Replace(mP0.Groups[0].Value, this.mParams[pName].ToString());
                        mP0 = mP0.NextMatch();
                    }
                    data = realData;
                    Match mP1 = rParams1.Match(data);
                    while (mP1.Success)
                    {
                        string pName = mP1.Groups[1].Value.ToUpper();
                        int pLen = int.Parse(mP1.Groups[2].Value);
                        pLen = Math.Min(pLen, this.mParams[pName].Length);
                        realData = realData.Replace(mP1.Groups[0].Value, this.mParams[pName].Substring(0, pLen));
                        mP1 = mP1.NextMatch();
                    }
                    data = realData;
                    Match mP2 = rParams2.Match(data);
                    while (mP2.Success)
                    {
                        string pName = mP2.Groups[1].Value.ToUpper();
                        int pStartIndex = int.Parse(mP2.Groups[2].Value);
                        int pLen = int.Parse(mP2.Groups[3].Value);
                        pStartIndex = Math.Min(pStartIndex, this.mParams[pName].Length);
                        pLen = Math.Min(pLen, this.mParams[pName].Length - pStartIndex);
                        realData = realData.Replace(mP2.Groups[0].Value, this.mParams[pName].Substring(pStartIndex, pLen));
                        mP2 = mP2.NextMatch();
                    }
                    if ((!cmd.Equals("TEST_RESULT")) && (cmd.IndexOf("TEST")==0))
                    {
                        string strTmp = realData.Replace(" ", "");
                        if ("000200030300".Equals(strTmp.Substring(2)))
                        {
                            des = "在【0x" + strTmp.Substring(0, 2) + "秒】内等待所有条件处理完成";
                            if (cmd.IndexOf("CONTINUE") > 0)
                            {
                                des += "【超时继续执行脚本】";
                                if (cmd.IndexOf("CLEAR") > 0)
                                    des += "【超时清空脚本】";
                            }
                            else
                            {
                                des += "【超时终止脚本】";
                            }
                            scriptType = ScriptPanel.ST_TEST_STATEMENT;
                        }
                        else
                        {
                            des = "等待条件满足(" + realData + ")";
                            if (cmd.IndexOf("CONTINUE") > 0)
                            {
                                des += "【超时继续执行脚本】";
                                if (cmd.IndexOf("CLEAR") > 0)
                                    des += "【超时清空脚本】";
                            }
                            else
                            {
                                des += "【超时终止脚本】";
                            }
                        }
                    }
                    else if (cmd.Equals("SET"))
                    {
                        des = "赋值:" + paramName + "=\"" + realData + "\"";
                        scriptType = ScriptPanel.ST_SET_STATEMENT;
                    }
                    else if (cmd.Equals("DELAY"))
                    {
                        des = "延时【" + realData + "】毫秒";
                    }
                    else if (cmd.Equals("ELSE"))
                    {
                        des = ("否则");
                        scriptType = ScriptPanel.ST_IF_STATEMENT;
                    }
                    else if (cmd.Equals("ENDIF"))
                    {
                        des = ("条件结束");
                        scriptType = ScriptPanel.ST_IF_STATEMENT;
                    }
                    else if (cmd.Equals("IF"))
                    {
                        scriptType = ScriptPanel.ST_IF_STATEMENT;
                        string strTmp = realData.Replace(" ", "");
                        if (strTmp.Substring(0, 4).Equals("0004") && strTmp.Substring(8, 4).Equals("0208"))
                        {
                            des = "如果(";
                            //查询传感器状态
                            byte[] d = UsbDeviceManager.ConvertHexStringToBytes(strTmp);
                            ushort mask = d[7];
                            mask = (ushort)((mask << 8) | d[6]);
                            ushort v = d[3];
                            v = (ushort)((v << 8) | d[2]);
                            Boolean isFisrt = true;
                            for (int j = 0; j < 16; j++)
                            {
                                if ((mask & (1 << j)) != 0)
                                {
                                    if (!isFisrt)
                                        des += ",";
                                    des += "【传感" + j.ToString();
                                    if ((v & (1 << j)) != 0)
                                        des += " 高】";
                                    else
                                        des += " 低】";
                                }
                            }
                            des += ")";
                        }
                        else
                        {
                            des = "如果(" + realData + ")";
                        }
                    }
                    else if (cmd.Equals("S") || cmd.Equals("SEND"))
                    {
                        realData = realData.Replace(" ", "");
                        String cmdCode = realData.Substring(0, 4);
                        String cmdData = realData.Substring(4);
                        //A=S(0301 00 00 00 11 00 00 01 00000000)
                        if (cmdCode == "0301")//需工翻译的指令
                        {
                            int actByteOffset = 0;

                            string strTJ = "";//条件描述
                            string strDZ = "";//动作描述
                            byte[] bsCmdData = UsbDeviceManager.ConvertHexStringToBytes(cmdData);
                            if (bsCmdData[0] == 0x03)//传感器蒙板条件
                                actByteOffset = 16;
                            else if (bsCmdData[0] == 0x04)//传感变化次数条件
                                actByteOffset = 1;
                            if ((bsCmdData[3 + actByteOffset] & 0x0F) == 0x00)
                            {
                                strDZ += "停止电机【0x" + String.Format("{0:X}", bsCmdData[4 + actByteOffset]) + "】 ";
                                if (bsCmdData[5 + actByteOffset] == 0x01)
                                    strDZ += "【锁死】";
                                else
                                    strDZ += "【松开】";
                            }
                            else if ((bsCmdData[3 + actByteOffset] & 0x0F) == 0x01)
                            {
                                if (bsCmdData[6 + actByteOffset] == 0x00)
                                    strDZ += "【向前】 ";
                                else
                                    strDZ += "【向后】 ";
                                strDZ += "转动电机【" + bsCmdData[4 + actByteOffset].ToString() + "】 ";
                                int pwmCnt = bsCmdData[10 + actByteOffset];
                                pwmCnt = (pwmCnt << 8) + bsCmdData[9 + actByteOffset];
                                pwmCnt = (pwmCnt << 8) + bsCmdData[8 + actByteOffset];
                                pwmCnt = (pwmCnt << 8) + bsCmdData[7 + actByteOffset];
                                if (pwmCnt != 0)
                                {
                                    strDZ += pwmCnt.ToString() + "步，停止时";
                                    if (bsCmdData[5 + actByteOffset] == 0x01)
                                        strDZ += "【锁死】";
                                    else
                                        strDZ += "【松开】";
                                }
                            }
                            else if ((bsCmdData[3 + actByteOffset] & 0x0F) == 0x02)
                            {
                                strDZ += "【发送命令】 ";
                                strDZ += "【0x" + String.Format("{0:X}", bsCmdData[5 + actByteOffset]).PadLeft(2, '0') + String.Format("{0:X}", bsCmdData[4 + actByteOffset]).PadLeft(2, '0') + "】 ";
                                if (bsCmdData[6 + actByteOffset] > 0)
                                {
                                    strDZ += "【数据】: ";
                                    for (int i = 0; i < bsCmdData[6 + actByteOffset]; i++)
                                    {
                                        strDZ += String.Format("{0:X}", bsCmdData[7 + actByteOffset + i]).PadLeft(2, '0');
                                    }
                                }
                            }
                            if (bsCmdData[4 + actByteOffset] >= 12)
                            {
                                strDZ = " 【 不处理动作,只等待条件完成 】 ";
                                scriptType = ScriptPanel.ST_NO_CONDITION;
                            }
                            if ((bsCmdData[3 + actByteOffset] & 0xF0) == 0)//需要判断条件
                            {
                                if (bsCmdData[0] == 0x01)//传感条件
                                {
                                    int bg = bsCmdData[2] >> 4;
                                    int bd = bsCmdData[2] & 0x0F;
                                    if ((bsCmdData[1] & 0x80) == 0)
                                    {
                                        strTJ += "等待 传感器【" + (bsCmdData[1] & 0x7F).ToString() + "】 (";
                                        if (bg != 0)
                                        {
                                            strTJ += "上升【" + bg.ToString() + "】次";
                                        }
                                        if ((bg > 0) && (bd > 0))
                                            strTJ += "  并且  ";
                                        if (bd != 0)
                                        {
                                            strTJ += "下降【" + bd.ToString() + "】次";
                                        }
                                    }
                                    else
                                    {
                                        strTJ += "马上判断 传感器【" + (bsCmdData[1] & 0x7F).ToString() + "】 (";
                                        if (bg > 0)
                                            strTJ += "为 【高】 电平";
                                        else if (bd > 0)
                                            strTJ += "为 【低】 电平";
                                    }

                                    strTJ += ") 执行 --- ";
                                }
                                
                                else if (bsCmdData[0] == 0x02)//电机条件
                                {
                                    strTJ += "等待 电机【" + (bsCmdData[1] & 0x7F).ToString() + "】 (";
                                    if (bsCmdData[2] == 0x00)
                                    {
                                        strTJ += "【停止】";
                                    }
                                    else if (bsCmdData[2] == 0x01)
                                    {
                                        strTJ += "【启动】";
                                    }
                                    strTJ += ") 执行 --- ";
                                }
                                else if (bsCmdData[0] == 0x03)//传感器蒙板条件
                                {
                                    if (bsCmdData[1] == 1)
                                        strTJ += "【NOT】";
                                    if (bsCmdData[2] == 0)
                                        strTJ += "【查询或等待】传感状态变为:";
                                    else
                                        strTJ += "【等待】传感状态变为:";
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[3] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + j.ToString() + " ";
                                            if ((bsCmdData[11] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[4] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 8).ToString() + " ";
                                            if ((bsCmdData[12] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[5] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 16).ToString() + " ";
                                            if ((bsCmdData[13] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[6] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 24).ToString() + " ";
                                            if ((bsCmdData[14] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[7] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 32).ToString() + " ";
                                            if ((bsCmdData[15] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[8] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 40).ToString() + " ";
                                            if ((bsCmdData[16] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[9] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 48).ToString() + " ";
                                            if ((bsCmdData[17] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    for (int j = 0; j < 8; j++)
                                    {
                                        if ((bsCmdData[10] & (1 << j)) != 0)
                                        {
                                            strTJ += "【传感" + (j + 56).ToString() + " ";
                                            if ((bsCmdData[18] & (1 << j)) != 0)
                                                strTJ += "高】 ";
                                            else
                                                strTJ += "低】 ";
                                        }
                                    }
                                    strTJ += " 执行 --- ";
                                }
                                else if (bsCmdData[0] == 0x04)
                                {
                                    int bg = bsCmdData[3];
                                    int bd = bsCmdData[2] | (bg<<8);
                                    strTJ += "等待 传感器【" + (bsCmdData[1] & 0x7F).ToString() + "】 (";
                                    strTJ += "状态变化【" + bd.ToString() + "】次)";
                                    strTJ += " 执行 --- ";
                                }
                            }
                            else
                                scriptType = ScriptPanel.ST_NO_CONDITION;

                            des = strTJ + strDZ;
                        }

                    }
                }
            }
            catch (Exception e2)
            {
                des = e2.ToString();
            }
            return des;
        }

        private void listScript_DrawItem(object sender, DrawItemEventArgs e)
        {            
            if (e.Index == -1)
            {
                tbScriptLine.Text = "";
                return;
            }
            Color bgColor = e.BackColor;
            Color fontColor = e.ForeColor;
            Font vFont = new Font("宋体",9);
            String str = ((ListBox)sender).Items[e.Index].ToString();
            int scriptType = 0;
            String des = this.ConvertScriptDescript(str, out scriptType) ;
            if (scriptType>=1)
                vFont = new Font("宋体", 9 ,FontStyle.Bold);
            if (scriptType == ScriptPanel.ST_SET_STATEMENT)
                bgColor = Color.LemonChiffon;
            else if (scriptType == ScriptPanel.ST_NO_CONDITION)
                fontColor = Color.Blue;
            else if (scriptType == ScriptPanel.ST_TEST_STATEMENT)
                bgColor = Color.LightCyan;
            else if (scriptType == ScriptPanel.ST_IF_STATEMENT)
            {
                bgColor = Color.SlateBlue;
                fontColor = Color.Yellow;
            }
            else if (scriptType == ScriptPanel.ST_ERROR)
            {
                bgColor = Color.Red;
                fontColor = Color.Wheat;
            }
            if (e.BackColor != listScript.BackColor)//选中的行
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
                e.Graphics.DrawString(des, vFont, new SolidBrush(e.ForeColor), e.Bounds);
                e.DrawFocusRectangle();
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);
                e.Graphics.DrawString(des, vFont, new SolidBrush(fontColor), e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void btnAddLine_Click(object sender, EventArgs e)
        {
            AddScriptLine(tbScriptLine.Text);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (listScript.SelectedIndex >= 0)
                listScript.Items[listScript.SelectedIndex] = tbScriptLine.Text;
            else
                listScript.Items.Add(tbScriptLine.Text);
        }

        private String lastScriptText = "";
        public string GetScripts()
        {
            string result = "";
            int scriptType;
            for (int i = 0; i < listScript.Items.Count; i++)
            {
                string d = this.ConvertScriptDescript(listScript.Items[i].ToString(), out scriptType);
                if (!listScript.Items[i].Equals(d))
                    result += "##---          "+d+"\r\n";
                result += listScript.Items[i] + "\r\n";
            }
            return result;           
        }
        private void listScript_DoubleClick(object sender, EventArgs e)
        {
            lastScriptText = GetScripts();
            tbScript.Text = lastScriptText;
            tbScript.Visible = true;
            tbScript.Width = this.Width;
            tbScript.Height = this.Height;
            this.mIsTextMode = true;
        }
        private void tbScript_DoubleClick(object sender, EventArgs e)
        {
            this.mIsTextMode = false;
            if (!tbScript.Text.Equals(lastScriptText))
            {                
                if (DialogResult.Yes == MessageBox.Show("脚本有修改，是否应用修改后的内容！", "询问", MessageBoxButtons.YesNo))
                {
                    int lastSelectItem = listScript.SelectedIndex;
                    listScript.Items.Clear();
                    string[] lines = tbScript.Text.Split(new string[]{"\r\n"},StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("##---"))
                            continue;
                        this.AddScriptLine(lines[i]);
                    }
                }
            }
            
            tbScript.Visible = false;
        }  
        private void listScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listScript.SelectedIndex>=0)
                tbScriptLine.Text = listScript.Items[listScript.SelectedIndex].ToString();
        }

        private void btnDeleteLine_Click(object sender, EventArgs e)
        {
            int oldSelectedIndex = listScript.SelectedIndex;
            if (listScript.SelectedIndex >= 0)
            {
                listScript.Items.RemoveAt(listScript.SelectedIndex);
                if (oldSelectedIndex < listScript.Items.Count)
                    listScript.SelectedIndex = oldSelectedIndex;
                else if (listScript.Items.Count > 0)
                    listScript.SelectedIndex = oldSelectedIndex - 1;
            }
        }


        public void ClearScript()
        {
            listScript.Items.Clear();
            tbScript.Text = "";
        }

        public void AddScriptLine(string line)
        {
            if (this.IsTextMode)
            {
                tbScript.Text += "\r\n"+line + "\r\n";
            }
            else
            {                
                if (listScript.SelectedIndex >= 0)
                {
                    listScript.Items.Insert(listScript.SelectedIndex + 1, line);
                    listScript.SelectedIndex++;
                }
                else
                {
                    listScript.Items.Add(line);
                    listScript.SelectedIndex = listScript.Items.Count - 1;
                }
            }
            if (IsSetStatement(line))
                this.InitParams(this.GetScripts());
        }

        private void btnUP_Click(object sender, EventArgs e)
        {
            if (listScript.SelectedIndex > 0)
            {
                string str = listScript.Items[listScript.SelectedIndex - 1].ToString();
                listScript.Items[listScript.SelectedIndex - 1] = listScript.Items[listScript.SelectedIndex];
                listScript.Items[listScript.SelectedIndex]=str;
                listScript.SelectedIndex--;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if ((listScript.SelectedIndex!=-1)&&(listScript.SelectedIndex < listScript.Items.Count - 1))
            {
                string str = listScript.Items[listScript.SelectedIndex + 1].ToString();
                listScript.Items[listScript.SelectedIndex + 1] = listScript.Items[listScript.SelectedIndex];
                listScript.Items[listScript.SelectedIndex] = str;
                listScript.SelectedIndex++;
            }
        }
    }
}
