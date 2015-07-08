using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    /// <summary>
    /// 上位机发送的停止消息
    /// </summary>
    public class Cmd2StopMsg : BaseMessage
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
