using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class ErrorCodeMsg:BaseMessage
    {
        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            if (frameData.Length < 10)
            {
                return null;
            }
            else
            {
                ErrorCodeMsg errorCodeMsg = new ErrorCodeMsg();
                errorCodeMsg.IpAndPort = ipEndPort;
                errorCodeMsg.ProtocolVersion = frameData[0];
                byte[] cyclebytes = new byte[4];
                Array.Copy(frameData, 1, cyclebytes, 0, 4);
                int CycleNo = BitConverter.ToInt32(cyclebytes, 0);
                errorCodeMsg.CycleNo = CycleNo;
                errorCodeMsg.Type = frameData[5];
                errorCodeMsg.SubType = frameData[6];
                errorCodeMsg.ErrorStatus = frameData[7];
                byte[] data = new byte[frameData.Length - 10];
                Array.Copy(frameData, 10, data, 0, data.Length);
                errorCodeMsg.DataLen = System.Net.IPAddress.HostToNetworkOrder((short)data.Length);
                errorCodeMsg.Data = data;
                return errorCodeMsg;
            }
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }
    }
}
