using System;
using System.Collections.Generic;
using System.Text;

namespace MotorControlBoard
{
    public interface IUsartDataChecker
    {
        int CheckData(byte[] data,int len);
    }
}
