using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Model;

namespace TSPTest.Protocol
{
    public class HeartTimeoutMsg :BaseMessage
    {
        public static HeartTimeoutMsg CreateNewMsg(Board board)
        {
            HeartTimeoutMsg heartTimeoutMsg = new HeartTimeoutMsg();
            heartTimeoutMsg.IpAndPort = " ";
            heartTimeoutMsg.ProtocolVersion = 0x01;
            heartTimeoutMsg.CycleNo = 0;
            heartTimeoutMsg.Type = 0xEE;
            heartTimeoutMsg.SubType = 0xEE;
            heartTimeoutMsg.ErrorStatus = 0x02;
            heartTimeoutMsg.DataLen = 1;
            byte[] temp = new byte[] { 0x4E,0x6F,0x20,0x68,0x65,0x61,0x72,0x74,0x20,0x6D,0x65,0x73,0x73,0x61,0x67, 0x65, 0x20,0x72,0x65, 0x63, 0x65, 0x69,0x76,0x65,0x64};
            byte[] heart = new byte[5 + temp.Length];
            Array.Copy(board.EqId, 0, heart, 0, 5);
            Array.Copy(temp, 0, heart, 5, temp.Length);
            heartTimeoutMsg.Data = heart;
            return heartTimeoutMsg;
        }

        public override BaseMessage Decode(byte[] frameData, string ipEndPort)
        {
            if (frameData.Length < 10)
            {
                return null;
            }
            else
            {
                HeartTimeoutMsg heartTimeoutMsg = new HeartTimeoutMsg();
                heartTimeoutMsg.IpAndPort = ipEndPort;
                heartTimeoutMsg.ProtocolVersion = frameData[0];
                byte[] cyclebytes = new byte[4];
                Array.Copy(frameData, 1, cyclebytes, 0, 4);
                int CycleNo = BitConverter.ToInt32(cyclebytes, 0);
                heartTimeoutMsg.CycleNo = CycleNo;
                heartTimeoutMsg.Type = frameData[5];
                heartTimeoutMsg.SubType = frameData[6];
                heartTimeoutMsg.ErrorStatus = frameData[7];
                byte[] data = new byte[frameData.Length - 10];
                Array.Copy(frameData, 10, data, 0, data.Length);
                heartTimeoutMsg.DataLen = System.Net.IPAddress.HostToNetworkOrder((short)data.Length);
                heartTimeoutMsg.Data = data;
                return heartTimeoutMsg;
            }
        }

        public override BaseMessage Encode(byte[] data)
        {
            this.Data = data;
            return this;
        }
    }
}
