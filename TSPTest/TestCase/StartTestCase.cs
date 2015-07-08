using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Protocol;

namespace TSPTest.TestCase
{
    public class StartTestCase : TestCasebase
    {
        public StartTestCase()
        {

        }

        public override Protocol.BaseMessage Request(byte subType, byte[] data, string IpAndPort)
        {
            StartTestMsg startTestMsg = new StartTestMsg();
            startTestMsg.IpAndPort = IpAndPort;
            startTestMsg.ProtocolVersion = 0x01;
            startTestMsg.CycleNo = 0x01;
            startTestMsg.Type = 0xC0;
            startTestMsg.SubType = subType;
            startTestMsg.ErrorStatus = 0x00;
            startTestMsg.DataLen = 1;
            startTestMsg.Encode(data);
            return startTestMsg;
        }
    }
}
