using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Model
{
    [Serializable]
    public class Rack
    {

        public int No;

        public byte[] EqId;//每个设备都有唯一的ID

        public byte subType;//对应相应机笼的测试子类型

        public string Name;

        public bool IsTested;

        public string SN;

        public List<Board> Boards;
    }
}
