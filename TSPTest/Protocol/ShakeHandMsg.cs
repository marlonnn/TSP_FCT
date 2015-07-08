using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    public class ShakeHandMsg : BaseMessage
    {
        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            if (frameData.Length < 10)
            {
                return null;
            }
            else
            {
                ShakeHandMsg shakeHandMsg = new ShakeHandMsg();
                shakeHandMsg.IpAndPort = ipEndPort;
                shakeHandMsg.ProtocolVersion = frameData[0];
                byte[] cyclebytes = new byte[4];
                Array.Copy(frameData, 1, cyclebytes, 0, 4);
                int CycleNo = BitConverter.ToInt32(cyclebytes, 0);
                shakeHandMsg.CycleNo = CycleNo;
                shakeHandMsg.Type = frameData[5];
                shakeHandMsg.SubType = frameData[6];
                shakeHandMsg.ErrorStatus = frameData[7];
                byte[] data = new byte[frameData.Length - 10];
                Array.Copy(frameData, 10, data, 0, data.Length);
                shakeHandMsg.DataLen = System.Net.IPAddress.HostToNetworkOrder((short)data.Length);
                shakeHandMsg.Data = data;
                return shakeHandMsg;
            }
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }
    }
}
