using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TSPTest.Common
{
    public class OriginalRxBytes : Original
    {
        public byte[] Data { get; set; }

        public OriginalRxBytes()
        {
        }

        public OriginalRxBytes(DateTime dt, IPEndPoint ip, byte[] msg)
        {
            RxTime = dt;
            Data = msg;
            RemoteIpEndPoint = ip;
        }
    }
}
