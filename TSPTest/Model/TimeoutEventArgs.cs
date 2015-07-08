using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Common;

namespace TSPTest.Model
{
    public class TimeoutEventArgs : EventArgs
    {
        public List<Board> Boards;
        public TestStatus TestStatus;
    }
}
