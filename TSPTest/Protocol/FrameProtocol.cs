using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Summer.System.Log;

namespace TSPTest.Protocol
{
    public class FrameProtocol
    {
        /// <summary>
        /// 应用标志位
        /// </summary>
        private byte AppMarkHead;//0X5A
        private byte AppMarkTail;//0XA5
        private byte[] cycleNo;

        /// <summary>
        /// 将数据解包（第一个字节是标识符，最后第二个字节是奇偶校验，最后一个字节是结尾标识符）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] DePackage(byte[] data)
        {
            if (data == null || data.Length <= 9)
            {
                LogHelper.GetLogger<FrameProtocol>().Error("通信层接收到数据包为空或者数据长度不足，丢弃。");
                return null;
            }
            if (data[0] != AppMarkHead && data[data.Length - 1] != AppMarkTail)
            {
                LogHelper.GetLogger<FrameProtocol>().Error("通信层接收到数据包不是本应用需要接受的数据包，丢弃。");
                return null;
            }

            //数据正常，去掉头尾返回，此处未考虑下位机发过来的数据分成N（N>1）包的情况，后续补充
            byte[] realData = new byte[data.Length - 9];
            Array.Copy(data, 7, realData, 0, data.Length - 9);
            return realData;
        }

        /// <summary>
        ///  将数据加包，包首增加应用标志，包尾增加奇偶校验
        /// </summary>
        /// <param name="baseMessage"></param>
        /// <param name="cycle"></param>
        /// <returns></returns>
        public byte[] EnPackage(BaseMessage baseMessage, int cycle)
        {
            if (baseMessage.Data == null)
            {
                LogHelper.GetLogger<FrameProtocol>().Error("通信层待编码数据为空，丢弃。");
                return null;
            }
            byte[] temp = BaseMessageToBytes(baseMessage);
            byte[] data = new byte[baseMessage.Data.Length + 10 + 9];
            data[0] = AppMarkHead;//frame head
            byte[] cyclebytes = new byte[4];
            cyclebytes = BitConverter.GetBytes(cycle);
            //cyclebytes.CopyTo(data, 1);//周期号
            Array.Copy(cyclebytes, 0, data, 1, 4);
            data[5] = 1;//分包总数
            data[6] = 1;//序列号
            Array.Copy(temp, 0, data, 7, temp.Length);
            //计算奇偶校验和，最后2位不参与奇偶校验
            byte oddCheck = data[0];
            for (int i = 1; i < data.Length - 2; i++)
            {
                oddCheck ^= data[i];
            }
            data[data.Length - 2] = oddCheck;//奇偶校验位
            data[data.Length - 1] = AppMarkTail;//frame tail

            return data;
        }

        /// <summary>
        /// 将应用消息转换为byte[]
        /// </summary>
        /// <param name="baseMessage"></param>
        /// <returns></returns>
        private byte[] BaseMessageToBytes(BaseMessage baseMessage)
        {
            byte[] temp = new byte[baseMessage.Data.Length + 10];
            temp[0] = baseMessage.ProtocolVersion;
            byte[] cycles = new byte[4];
            cycles = BitConverter.GetBytes(baseMessage.CycleNo);
            cycles.CopyTo(temp, 1);
            temp[5] = baseMessage.Type;
            temp[6] = baseMessage.SubType;
            temp[7] = baseMessage.ErrorStatus;
            BitConverter.GetBytes(baseMessage.DataLen).CopyTo(temp, 8);
            baseMessage.Data.CopyTo(temp, 10);
            Array.Copy(baseMessage.Data, 0, temp, 10, baseMessage.Data.Length);
            return temp;
        }
    }
}
