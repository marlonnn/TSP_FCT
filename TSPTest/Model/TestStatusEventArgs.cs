using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Common;

namespace TSPTest.Model
{
    public class TestStatusEventArgs : EventArgs
    {
        public TestStatus LastStatus;
        public TestStatus CurStatus;
    }
}
