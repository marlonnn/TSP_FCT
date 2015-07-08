using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class HeartMsg : BaseMessage
    {
        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            if (frameData.Length < 10)
            {
                return null;
            }
            else
            {
                HeartMsg heartMsg = new HeartMsg();
                heartMsg.IpAndPort = ipEndPort;
                heartMsg.ProtocolVersion = frameData[0];
                byte[] cyclebytes = new byte[4];
                Array.Copy(frameData, 1, cyclebytes, 0, 4);
                int CycleNo = BitConverter.ToInt32(cyclebytes, 0);
                heartMsg.CycleNo = CycleNo;
                heartMsg.Type = frameData[5];
                heartMsg.SubType = frameData[6];
                heartMsg.ErrorStatus = frameData[7];
                byte[] data = new byte[frameData.Length - 10];
                Array.Copy(frameData, 10, data, 0, data.Length);
                heartMsg.DataLen = System.Net.IPAddress.HostToNetworkOrder((short)data.Length);
                heartMsg.Data = data;
                return heartMsg;
            }
        }

        public override BaseMessage Encode(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
