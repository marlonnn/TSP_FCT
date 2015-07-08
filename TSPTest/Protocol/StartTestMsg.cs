using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class StartTestMsg : BaseMessage
    {
        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            throw new NotImplementedException();
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }
    }
}
