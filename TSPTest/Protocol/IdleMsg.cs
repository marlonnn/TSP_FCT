using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class IdleMsg : BaseMessage
    {
        public static IdleMsg CreateNewMsg()
        {
            byte[] idleBytes = new byte[] { 0xFF,0xFF,0XFF,0XFF,0XFF};
            IdleMsg msg = new IdleMsg();
            msg.IpAndPort = "";
            msg.ProtocolVersion = 0x01;
            msg.CycleNo = 0xFF;
            msg.Type = 0xFF;
            msg.SubType = 0xFF;
            msg.ErrorStatus = 0xFF;
            msg.DataLen = 1;
            msg.Encode(idleBytes);
            return msg;
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }

        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            throw new NotImplementedException();
        }
    }
}
