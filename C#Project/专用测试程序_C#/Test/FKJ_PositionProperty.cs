using System;
using System.Collections.Generic;
using System.Text;

namespace MotorControlBoard
{
    /// <summary>
    /// 保存位置和步数信息
    /// </summary>
    public class FKJ_PositionPropertyItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Step { get; set; }

        public FKJ_PositionPropertyItem(int x, int y, int step)
        {
            X = x;
            Y = y;
            Step = step;
        }
        public FKJ_PositionPropertyItem()
            : this(0, 0, 0)
        {

        }

        public override string ToString()
        {
            return X.ToString().PadLeft(5, '0') + " 行 " + Y.ToString().PadLeft(5, '0') + " 列 位置:" + Step;
        }

    }

    /// <summary>
    /// 用于增加或者删除位置信息列表
    /// </summary>
    public class FKJ_PositionProperty
    {
        List<FKJ_PositionPropertyItem> Items = new List<FKJ_PositionPropertyItem>();

        public void Clear()
        {
            Items.Clear();
        }

        //插入时要先按X排列，然后按Y排列 
        public void AddItem(int x, int y, int step)
        {
            int index = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].X > x)
                {
                    index = i;
                    break;
                }
                else if (Items[i].X == x && Items[i].Y > y)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
                Items.Add(new FKJ_PositionPropertyItem(x, y, step));
            else
                Items.Insert(index, new FKJ_PositionPropertyItem(x, y, step));

        }

        public void RemoveItem(int x, int y)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if ((Items[i].X == x) && (Items[i].Y == y))
                {
                    Items.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveItemAt(int index)
        {
            Items.RemoveAt(index);
        }

        //打完全一置的配置
        public FKJ_PositionPropertyItem FindPositionProperty(int x, int y)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if ((Items[i].X == x) && (Items[i].Y == y))
                {
                    FKJ_PositionPropertyItem result = new FKJ_PositionPropertyItem(Items[i].X, Items[i].Y, Items[i].Step);
                    return result;
                }
            }
            return null;
        }

        //找到比自己小而且最接近自己的配置
        public FKJ_PositionPropertyItem FindNearPositionProperty(int x, int y)
        {
            FKJ_PositionPropertyItem result = null;
            int index = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                //因为插入时进行过排序，所以如果X比当前要大的话就可以退出了
                if ((Items[i].X > x))
                    break;
                if (Items[i].X != x)//不是同一列的，不比较
                    continue;
                if ((Items[i].X==x)&&(Items[i].Y>y))
                    break;
                index = i;
            }
            if (index >= 0)
                result = new FKJ_PositionPropertyItem(Items[index].X, Items[index].Y, Items[index].Step);
            return result;
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public FKJ_PositionPropertyItem GetItem(int index)
        {
            return new FKJ_PositionPropertyItem(Items[index].X, Items[index].Y, Items[index].Step);
        }

        /// <summary>
        /// 将信息列表中所有的的X,Y和Step生成字符串输出
        /// </summary>
        /// <returns></returns>
        public string ToConfigString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (FKJ_PositionPropertyItem ppi in Items)
            {
                sb.Append(ppi.X.ToString() + "," + ppi.Y.ToString() + "," + ppi.Step.ToString() + ";");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);//删除最后一个“;”号
            return sb.ToString();
        }

        /// <summary>
        /// 将参数文件中的导入到信息列表中
        /// </summary>
        /// <param name="str"></param>
        public void FromConfigString(string str)
        {
            this.Clear();//将信息列表清空
            if (String.IsNullOrEmpty(str))
                return;
            string[] items = str.Split(';');
            foreach (string s in items)
            {
                string[] c = s.Split(',');
                this.AddItem(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
            }
        }
    }
}
