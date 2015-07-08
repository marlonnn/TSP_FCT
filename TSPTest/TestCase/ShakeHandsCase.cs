using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Protocol;

namespace TSPTest.TestCase
{
    public class ShakeHandsCase : TestCasebase
    {
        public ShakeHandsCase()
        {
        }

        public override BaseMessage Request(byte subType, byte[] data, string IpAndPort)
        {
            ShakeHandMsg shakeHandMsg = new ShakeHandMsg();
            shakeHandMsg.IpAndPort = IpAndPort;
            shakeHandMsg.ProtocolVersion = 0x01;
            shakeHandMsg.CycleNo = 0x01;
            shakeHandMsg.Type = 0x01;
            shakeHandMsg.SubType = 0x01;
            shakeHandMsg.ErrorStatus = 0x00;
            shakeHandMsg.DataLen = 1;
            shakeHandMsg.Encode(data);
            return shakeHandMsg;
        }
    }
}
