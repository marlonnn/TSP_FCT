using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Common;
using TSPTest.Model;
using TSPTest.Protocol;
using Summer.System.NET;
using System.Threading;
using System.Net;
using Summer.System.Log;

namespace TSPTest.Net
{
    public class Udp
    {
        public volatile bool NeedRunning = true;
        RxQueue rxQueue;
        TxQueue txQueue;
        TestedRacks testedRacks;
        FrameProtocol frameProtocol;
        private int port;
        private UdpNetServer udpNetServer;

        private Dictionary<string, UdpNetClient> udpNetClients = new Dictionary<string, UdpNetClient>();

        public Udp()
        {

        }

        public void UdpTxPrepare()
        {
            foreach (Rack rack in testedRacks.Racks)
            {
                foreach (Board board in rack.Boards)
                {
                    if (string.IsNullOrEmpty(board.IPAndPort))
                        continue;
                    string[] ipAndPort = board.IPAndPort.Split(':');
                    if (ipAndPort.Length != 2)
                        continue;
                    UdpNetClient client = new UdpNetClient(ipAndPort[0], Int32.Parse(ipAndPort[1]));
                    udpNetClients[board.IPAndPort] = client;
                    try
                    {
                        client.Connect();
                    }
                    catch (System.Exception e)
                    {
                        LogHelper.GetLogger<Udp>().Error(e.Message);
                        LogHelper.GetLogger<Udp>().Error(e.StackTrace);
                    }
                }
            }
        }

        public void UdpTxStart()
        {
            while (NeedRunning)
            {
                try
                {
                    List<Original> originalData = txQueue.PopAll();
                    if (originalData != null)
                    {
                        if (originalData.Count != 0)
                        {
                            foreach (Original original in originalData)
                            {
                                OriginalTxBytes oBytes = original as OriginalTxBytes;
                                UdpNetClient udpClient = udpNetClients[oBytes.RemoteIpEndPoint.ToString()];
                                udpClient.Send(oBytes.Data);
                            }
                        }

                    }

                    Thread.Sleep(20);
                }
                catch (System.Exception ex)
                {

                }
            }
        }


        //启动侦听端口并接收数据
        public void UpdRxStart()
        {
            try
            {
                udpNetServer.AsyncRxProcessCallBack += new NetAsyncRxDataCallBack(this.ReceiveBytes);
                udpNetServer.Open();
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
                udpNetServer.ReceiveAsync(remoteIpEndPoint);
            }
            catch (System.Exception e)
            {
                LogHelper.GetLogger<Udp>().Error(e.Message);
                LogHelper.GetLogger<Udp>().Error(e.StackTrace);
            }
        }

        //关闭Udp
        public void UdpClose()
        {
            try
            {
                NeedRunning = false;
                udpNetServer.AsyncRxProcessCallBack -= new NetAsyncRxDataCallBack(this.ReceiveBytes);
            }
            catch (System.Exception e)
            {
                LogHelper.GetLogger<Udp>().Error(e.Message);
                LogHelper.GetLogger<Udp>().Error(e.StackTrace);
            }
            finally
            {
                udpNetServer.Close();
            }
        }

        private void ReceiveBytes(byte[] receiveBytes, IPEndPoint remoteIpEndPoint)
        {
            try
            {
                rxQueue.Push(new OriginalRxBytes(DateTime.Now, remoteIpEndPoint, receiveBytes));
                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, port);
                udpNetServer.ReceiveAsync(remoteIp);
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Udp>().Error(ee.Message);
                LogHelper.GetLogger<Udp>().Error(ee.StackTrace);
            }
        }
    }
}
