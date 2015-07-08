using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Protocol
{
    /// <summary>
    /// 应用协议
    /// </summary>
    public abstract class BaseMessage
    {
        public string IpAndPort;
        // 协议版本
        public byte ProtocolVersion;
        // 上位机发送的序列号
        public int CycleNo;
        // 类型
        public byte Type;
        // 子类型
        public byte SubType;
        // 错误标识符
        public byte ErrorStatus;
        // 应用实际数据长度
        public short DataLen;
        //实际数据
        public byte[] Data;

        public abstract BaseMessage Decode(byte[] frameData, string ipEndPort);

        public abstract BaseMessage Encode(byte[] data);

    }
}
