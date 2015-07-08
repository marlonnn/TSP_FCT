using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Common;
using Summer.System.Log;
using System.Net;

namespace TSPTest.Protocol
{
    public class ProtocolFactory
    {
        RxQueue rxQueue;
        TxQueue txQueue;
        RxMsgQueue rxMsgQueue;
        TxMsgQueue txMsgQueue;
        FrameProtocol frameProtocol;

        Dictionary<byte, BaseMessage> decoders;
        Dictionary<byte, BaseMessage> encoders;

        public void ExecuteDecoderInternal()
        {
            LogHelper.GetLogger("job").Debug("Begin ProtocolFactory.ExecuteDecoderInternal()");
            List<Original> originalData = rxQueue.PopAll();
            if (originalData.Count != 0)
            {
                //先通信层解析，再应用层解析
                foreach (Original original in originalData)
                {
                    OriginalRxBytes oBytes = original as OriginalRxBytes;
                    IPEndPoint ipEndPoint = oBytes.RemoteIpEndPoint;
                    if (oBytes != null && oBytes.Data.Length > 9)
                    {
                        byte[] realData = frameProtocol.DePackage(oBytes.Data);

                        if (realData != null && realData.Length > 10)
                        {
                            BaseMessage msg = null;
                            byte type = realData[5];

                            if (decoders.ContainsKey(type))
                            {
                                msg = decoders[type].Decode(realData, oBytes.RemoteIpEndPoint.ToString());
                            }
                            else
                            {
                                //default

                            }
                            if (msg != null)
                                rxMsgQueue.Push(msg);
                        }
                    }
                }
            }
            else
            {
                rxMsgQueue.Push(IdleMsg.CreateNewMsg());
            }

            LogHelper.GetLogger("job").Debug("Finish ProtocolFactory.ExecuteDecoderInternal()");
        }

        public void ExecuteEncoderInternal()
        {
            LogHelper.GetLogger("job").Debug("Begin ProtocolFactory.ExecuteEncoderInternal()");
            List<BaseMessage> listMsg = txMsgQueue.PopAll();
            if (listMsg.Count == 0)
            {
                return;
            }
            else
            {
                foreach (BaseMessage msg in listMsg)
                {
                    //组包放到发送数据原始队列TxQueue中
                    byte[] framData = frameProtocol.EnPackage(msg, 0);
                    OriginalTxBytes originalTxbytes = new OriginalTxBytes();
                    originalTxbytes.RemoteIpEndPoint = String2IPEndPoint(msg.IpAndPort);
                    originalTxbytes.Data = framData;
                    txQueue.Push(originalTxbytes);
                }
            }

            LogHelper.GetLogger("job").Debug("Finish ProtocolFactory.ExecuteEncoderInternal()");
        }

        public IPEndPoint String2IPEndPoint(string ip)
        {
            string[] addrPort = ip.Split(':');
            var endPoint = new IPEndPoint(IPAddress.Parse(addrPort[0]), Int32.Parse(addrPort[1]));
            return endPoint;
        }
    }
}
