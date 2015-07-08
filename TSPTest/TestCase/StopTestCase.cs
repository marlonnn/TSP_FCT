using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Protocol;

namespace TSPTest.TestCase
{
    public class StopTestCase : TestCasebase
    {
        public StopTestCase()
        {

        }

        public override Protocol.BaseMessage Request(byte subType, byte[] data, string IpAndPort)
        {
            Cmd2StopMsg cmd2StopMsg = new Cmd2StopMsg();
            cmd2StopMsg.IpAndPort = IpAndPort;
            cmd2StopMsg.ProtocolVersion = 0x01;
            cmd2StopMsg.CycleNo = 0x01;
            cmd2StopMsg.Type = 0xC1;
            cmd2StopMsg.SubType = subType;
            cmd2StopMsg.ErrorStatus = 0x00;
            cmd2StopMsg.DataLen = 5;
            cmd2StopMsg.Encode(data);
            return cmd2StopMsg;
        }
    }
}
