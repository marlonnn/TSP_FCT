using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Model
{
    [Serializable]
    public class Board
    {
        public int No;

        public byte[] EqId;//每个设备都有唯一的ID

        public bool IsTested;

        public string Name;

        public bool IsPassed;

        public string IPAndPort;
    }
}
