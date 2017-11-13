using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MotorControlBoard
{
    public class Common
    {

        #region 获取


        /// <summary>
        /// 获取应用程序 的开始目录
        /// （ X:\xxx\xxx (.exe文件所在的目录)）
        /// </summary>
        public static string Get_Application_StartupPath
        {
            get
            {
                return System.Windows.Forms.Application.StartupPath;
            }
        }

        #region 方案

        /// <summary>
        /// 获取方案的所在目录
        /// </summary>
        /// <returns></returns>
        public static string Get_FangAn_MuLu()
        {
            return Get_Application_StartupPath + "\\FangAn";
        }


        /// <summary>
        /// 获取指定方案名的所在目录
        /// </summary>
        /// <param name="fangan_name"></param>
        /// <returns></returns>
        public static string Get_FangAn_MuLu(string fangan_name)
        {
            return Get_FangAn_MuLu() + "\\" + fangan_name;
        }


        /// <summary>
        /// 获取 方案的脚本所在目录
        /// </summary>
        /// <param name="fangan_name">方案名</param>
        /// <returns></returns>
        public static string Get_FangAn_Scripts_MuLu(string fangan_name)
        {
            return Get_FangAn_MuLu(fangan_name) + "\\" + FangAn_Scripts_MuLuName;  //Scripts
        }

        /// <summary>
        /// 获取 方案配置文件的路径
        /// </summary>
        /// <param name="fangan_name"></param>
        /// <returns></returns>
        public static string Get_FangAn_Config_Path(string fangan_name)
        {
            return Get_FangAn_MuLu(fangan_name) + "\\" + FangAn_Config_FileName;
        }

        /// <summary>
        /// 方案脚本所在目录的名称（Scripts）
        /// </summary>
        public const string FangAn_Scripts_MuLuName = "Scripts";

        /// <summary>
        /// 方案配置文件名
        /// </summary>
        public const string FangAn_Config_FileName = "config.xml";

        #endregion 

        #endregion

        #region 枚举


        public enum UintCmd
        {

            /// <summary>
            /// 直接停止的命令
            /// </summary>
            ZhiJi_TingZhi_Cmd = 10,

            /// <summary>
            /// 直接启动的命令
            /// </summary>
            ZhiJie_QiDong_Cmd = 11,


            /// <summary>
            /// 传感器 条件命令
            /// </summary>
            ChuanGanQi_TiaoJian_Cmd = 1,

            /// <summary>
            /// 电机 条件命令
            /// </summary>
            DianJi_TiaoJian_Cmd = 2,

            /// <summary>
            /// 传感器变化次数 条件命令
            /// </summary>
            ChuanGanQiTimes_TiaoJian_Cmd = 4,

        }

        #endregion

        #region 转换/操作

        /// <summary>
        /// 补充字符串 到 指定字符串（如补0等）
        /// </summary>
        /// <param name="str">被补充的字符串</param>
        /// <param name="targetLeng">目标长度</param>
        /// <param name="buchongStr">补充的字符串，如0，为空默认为0</param>
        /// <param name="AddToQian">补充到前面？否则补充到后面</param>
        /// <returns></returns>
        public static string BuCongStr_To_String(string str, int targetLeng, string buchongStr, bool AddToQian)
        {
            if (str.Length < targetLeng)  //小于目标长度
            {
                if (String.IsNullOrEmpty(buchongStr))  //若为空，则为0
                {
                    buchongStr = "0";
                }


                for (int i = 0; i < targetLeng - str.Length; i++)
                {
                    if (AddToQian)
                    {
                        str = buchongStr + str;
                    }
                    else
                    {
                        str += buchongStr;
                    }
                }


            }

            return str;
        }

        /// <summary>
        /// 把字符串变成 十六进制，且低位在前，高位在后
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ConvertStringToHex(int str)
        {

            byte[] bData = BitConverter.GetBytes(str);
            Array.Reverse(bData);
            string bs2 = BitConverter.ToString(bData).Replace("-", "");
            string bs = "";
            for (int i = 2; i < bs2.Length + 2; i = i + 2)  //把低位在前，高位在后（如00-BC-61-4E 变成 4E-61-BC-00）
            {
                bs += bs2.Substring(bs2.Length - i, 2);
            }

            /* String.Format("十进制数(Int32): {0} 用二进制表示为(高字节在前):\r\n{1}", bushu,
               BitConverter.ToString(bData));*/
            /* 结果:
             * 十进制数(Int32): 12345678 用二进制表示为(高字节在前):
             * 00-BC-61-4E
             * 
             */

            return bs;

        }


        /// <summary>
        /// 把十六进制转换成 是十进制字符串
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <param name="is_digao">十六进制是从低到高？若是这样，则重新拍列为高到低，false不变化</param>
        /// <returns></returns>
        public static string ConvertHexToString(string hex, bool is_digao)
        {
            if (hex.Equals(""))
            {
                return "";
            }
            string strHex = "";
            hex = hex.Replace(" ", "");
            if (is_digao)  //若为从低到高
            {
                for (int i = 2; i < hex.Length + 2; i = i + 2)
                {
                    strHex += hex.Substring(hex.Length - i, 2);
                }
            }
            else
            {
                strHex = hex;
            }

            return Convert.ToInt32(strHex, 16).ToString();

        }

        #endregion

        #region 绑定

        /// <summary>
        /// 下拉列表框绑定
        /// </summary>
        /// <param name="cmb">下拉列表框</param>
        /// <param name="source">数据源</param>
        /// <param name="valueMember">value字段名</param>
        /// <param name="textMember">text字段名</param>
        public static void ComboBoxBind(ComboBox cmb, object source, string valueMember, string textMember)
        {
            cmb.DataSource = source;
            cmb.DisplayMember = textMember;
            cmb.ValueMember = valueMember;
        }

        /// <summary>
        /// 下拉列表框绑定
        /// </summary>
        /// <param name="cmb">下拉列表框</param>
        /// <param name="sourceList">list《string》里的string的格式：text值:value值，即用英文冒号分开，text在前面，value在后面，如：状态1:1</param>
        public static void ComboBoxBind(ComboBox cmb, List<string> sourceList)
        {

            DataTable dt = new DataTable();
            string _text = "_text";
            string _value = "_value";
            dt.Columns.Add(_text);
            dt.Columns.Add(_value);

            DataRow dr = null;

            for (int i = 0; i < sourceList.Count; i++)
            {

                string[] sourceS = sourceList[i].Split(':');  //根据冒号分割成数组
                dr = dt.NewRow();
                dr[_text] = sourceS[0];
                dr[_value] = sourceS[1];
                dt.Rows.Add(dr);

            }

            ComboBoxBind(cmb, dt, _value, _text);


        }


        #endregion

    }
}
