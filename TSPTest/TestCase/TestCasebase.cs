using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Protocol;

namespace TSPTest.TestCase
{
    public abstract class TestCasebase
    {
        public abstract BaseMessage Request(byte subType,byte[] data, string IpAndPort);
    }
}
