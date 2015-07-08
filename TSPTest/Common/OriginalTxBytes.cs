using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TSPTest.Common
{
    /// <summary>
    /// 包含Udp发送的组包之后的原始数据，时间，IP和端口号
    /// Data是帧数据，包含了应用协议中的应用数据
    /// </summary>
    public class OriginalTxBytes : Original
    {
        public DateTime RxTime { get; set; }

        public byte[] Data { get; set; }

        public IPEndPoint RemoteIpEndPoint { get; set; }

        public OriginalTxBytes()
        {
        }

        public OriginalTxBytes(DateTime dt, IPEndPoint ip, byte[] msg)
        {
            RxTime = dt;
            Data = msg;
            RemoteIpEndPoint = ip;
        }
    }
}
