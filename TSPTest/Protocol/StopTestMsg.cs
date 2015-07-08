using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class StopTestMsg:BaseMessage
    {
        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            if (frameData.Length < 10)
            {
                return null;
            }
            else
            {
                StopTestMsg stopTestMsg = new StopTestMsg();
                stopTestMsg.IpAndPort = ipEndPort;
                stopTestMsg.ProtocolVersion = frameData[0];
                byte[] cyclebytes = new byte[4];
                Array.Copy(frameData, 1, cyclebytes, 0, 4);
                int CycleNo = BitConverter.ToInt32(cyclebytes, 0);
                stopTestMsg.CycleNo = CycleNo;
                stopTestMsg.Type = frameData[5];
                stopTestMsg.SubType = frameData[6];
                stopTestMsg.ErrorStatus = frameData[7];
                byte[] data = new byte[frameData.Length - 10];
                Array.Copy(frameData, 10, data, 0, data.Length);
                stopTestMsg.DataLen = System.Net.IPAddress.HostToNetworkOrder((short)data.Length);
                stopTestMsg.Data = data;
                return stopTestMsg;
            }
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }
    }
}
