using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TSPTest.Common;
using Summer.System.Log;
using TSPTest.Protocol;
using TSPTest.TestCase;
using Summer.System.Util;

namespace TSPTest.Model
{
    [Serializable]
    public class TestedRacks
    {
        //Key(分别对应二进制文件和日志文件)
        public string Key;
        //上一次的key
        public string LastKey;
        //测试人
        public string Tester;
        //本次测试SN    
        public string SN;
        //待测试机笼和板卡
        public List<Rack> Racks;
        //测试状态
        public TestStatus TestStatus;
        //开始测试时间
        public DateTime StartTime;
        //测试时长(单位：秒)
        public long RunningTime;
        //应用协议中EqID的长度，5位
        public int EqIDLength;

        [field: NonSerialized]
        public event EventHandler OnTestStatusChange;
        [field: NonSerialized]
        public event EventHandler OnTimeout;

        [field: NonSerialized]
        public event EventHandler OnBoardStatusChange;

        [NonSerialized]
        ErrorCodeMsgFile errorCodeMsgFile; //spring初始化完成后后，重载也不覆盖，单态唯一        

        [NonSerialized]
        int preTimeout;                     //spring初始化完成后后，重载也不覆盖，单态唯一
        [NonSerialized]
        int runTimeout;                    //spring初始化完成后后，重载也不覆盖，单态唯一

        DateTime preTestTime;
        [NonSerialized]
        Dictionary<uint, Board> IDBoardDic;
        [NonSerialized]
        Dictionary<uint, bool> IDStatusDic ;
        [NonSerialized]
        Dictionary<uint, bool> IDStopDic ;
        Dictionary<Board, DateTime> runTimeoutDic;//开始测试时自动初始化

        [NonSerialized]
        ShakeHandsCase shakeHandsCase;
        [NonSerialized]
        StartTestCase startTestCase;
        [NonSerialized]
        StopTestCase stopTestCase;
        [NonSerialized]
        TxMsgQueue txMsgQueue;
        [NonSerialized]
        RxMsgQueue rxMsgQueue;

        public void CheckTstConfig()
        {
            int testCount = 0;
            if (this.Tester == "")
            {
                throw new Exception("请输入测试人员姓名。");
            }
            foreach (Rack rack in this.Racks)
            {
                if (rack.IsTested)
                {
                    testCount++;
                }
            }
            if (testCount == 0)
            {
                throw new Exception("未选中一块板卡，无法进行测试！");
            }
            foreach (Rack r in this.Racks)
            {
                if (r.IsTested)
                {
                    if (r.SN.Length == 0)
                    {
                        throw new Exception(string.Format("{0}#{1}未设置SN号", r.No, r.Name));
                    }
                }
            }
        }

        //检查是否能够握手，至少选中一个机笼
        public bool CanShakeHands()
        {
            bool temp = false;
            foreach (Rack rack in Racks)
            {
                if (rack.IsTested)
                {
                    temp = true;
                    continue;
                }
            }
            return temp;
        }


        public void ShakeHands()
        {
            //进入临界状态
            preTestTime = DateTime.Now;
            GenTestStatusChangeEvent(TestStatus, TestStatus.THRESHOLD);
            TestStatus = TestStatus.THRESHOLD;
            IDBoardDic = new Dictionary<uint, Board>();
            IDStatusDic = new Dictionary<uint, bool>();
            IDStopDic = new Dictionary<uint, bool>();
            foreach (Rack rack in Racks)
            {
                foreach (Board board in rack.Boards)
                {
                    byte[] ID = board.EqId;
                    uint uID = BitConverter.ToUInt32(ID, 0);
                    if(!IDBoardDic.ContainsKey(uID))
                    {
                        IDBoardDic.Add(uID, board);
                    }
                    if (board.IPAndPort == "")
                    {
                        continue;
                    }
                    try
                    {
                        if (!IDStopDic.ContainsKey(uID))
                        {
                            IDStopDic.Add(uID, false);
                            IDStatusDic.Add(uID, false);
                            BaseMessage baseMsg = shakeHandsCase.Request(0x01, board.EqId, board.IPAndPort);
                            txMsgQueue.Push(baseMsg);
                        }
                    }
                    catch (System.Exception ex)
                    {

                    }
                }
            }
        }

        //没有消息的时候，界面会周期性发送0xFF消息
        public void AppendLog(BaseMessage msg)
        {
            if (TestStatus == TestStatus.THRESHOLD)
            {
                if (!IsHeartMsg(msg))
                    return;
                //检查握手消息是否收到
                CheckShakeHands(msg);
            }

            //判断心跳并且生成心跳超时
            if (TestStatus == TestStatus.RUNNING)
            {
                List<Board> boards = GetTimeoutBoard(runTimeoutDic, runTimeout);
                foreach (Board b in boards)
                {
                    HeartTimeoutMsg heartTimeoutMsg = HeartTimeoutMsg.CreateNewMsg(b);
                    if (rxMsgQueue != null)
                        rxMsgQueue.Push(heartTimeoutMsg);
                    //下一次再进行超时判断
                    runTimeoutDic[b] = DateTime.Now;
                }

                //心跳消息、错误码消息都算是心跳，更新收到上次心跳的时间
                Board board = GetBoard(msg);
                if (!board.Name.Contains("未知板卡"))
                {
                    runTimeoutDic[board] = DateTime.Now;
                }
            }

            if (TestStatus == TestStatus.RUNNING && IsStopTestMsg(msg))
            {
                CheckStop(msg);
            }

            //只在测试过程中记录日志，并且不记录心跳数据
            if (TestStatus == TestStatus.RUNNING && errorCodeMsgFile != null && !IsHeartMsg(msg))
            {
                Board b = GetBoard(msg);
                if (BoardErrorStatus(msg))//有错误板卡，触发板卡状态错误事件
                {
                    if (b.IsPassed)
                    {
                        b.IsPassed = false;
                        GenBoardStatusChangeEvent(b);
                    }
                }
                errorCodeMsgFile.Append(msg, b.Name, GetTestInfo(msg));
            }
        }

        //根据消息中错误状态获取板卡是否通过，0x01：通过，0x02:失败
        public bool BoardErrorStatus(BaseMessage msg)
        {
            bool result = true;
            switch (msg.ErrorStatus)
            {
                case 0x01:
                    result = false;
                    break;
                case 0x02:
                    result = true;
                    break;
            }
            return result;
        }

        //检查是否都收到握手消息
        public void CheckShakeHands(BaseMessage msg)
        {
            if (IsShakeHandMsg(msg))
            {
                byte[] data = msg.Data;
                uint uData = BitConverter.ToUInt32(data, 0);
                bool temp = true;
                if (IDBoardDic.ContainsKey(uData))
                    IDStatusDic[uData] = true;
                foreach (var kv in IDStatusDic)
                {
                    temp &= kv.Value;
                }
                if (temp)
                {
                    TestStatus = TestStatus.HANDS_OK;
                    GenTestStatusChangeEvent(TestStatus.THRESHOLD, TestStatus.HANDS_OK);
                    return;
                }
            }//如果超时了，还有板卡没有收到消息，进入异常结束

            else if (DateTime.Now.Ticks - preTestTime.Ticks > (uint)preTimeout * 10000000)
            {
                List<Board> boards = new List<Board>();
                foreach (var kv in IDStatusDic)
                {
                    if (kv.Value == false)
                        boards.Add(IDBoardDic[kv.Key]);
                }
                //执行异常结束工作
                GenTimeoutEvent(boards, TestStatus.THRESHOLD);
                FinishHandsNotOK();
            }
        }

        //检查是否收到停止消息
        public void CheckStop(BaseMessage msg)
        {
            byte[] data = msg.Data;
            uint uData = BitConverter.ToUInt32(data, 0);
            bool temp = true;
            if (IDBoardDic.ContainsKey(uData))
                IDStopDic[uData] = true;
            foreach (var kv in IDStopDic)
            {
                temp &= kv.Value;
            }
            if (temp)
            {
                TestStatus = TestStatus.EXPECTED_FINNISH;
                GenTestStatusChangeEvent(TestStatus.RUNNING, TestStatus);
                return;
            }
        }

        public void FinishHandsNotOK()
        {
            TestStatus = TestStatus.HANDS_NOTOK;
            GenTestStatusChangeEvent(TestStatus.THRESHOLD, TestStatus);
        }

        //结束一次非正常测试
        public void FinishUnExpectedTest()
        {
            RunningTime = 0;
            //对每个设备发送停止测试指令
            foreach (Rack rack in Racks)
            {
                if(rack.IsTested)
                {
                    foreach (Board board in rack.Boards)
                    {
                        if (board.IPAndPort == "")
                        {
                            continue;
                        }
                        BaseMessage baseMsg = stopTestCase.Request(rack.subType, board.EqId, board.IPAndPort);
                        txMsgQueue.Push(baseMsg);
                    }
                }

            }

            TestStatus = TestStatus.UNEXPECTED_FINNISH;
            GenTestStatusChangeEvent(TestStatus.THRESHOLD, TestStatus);

            LastKey = Key;
            errorCodeMsgFile.Close();
            Save(Util.GetBasePath() + "//Report//Data//" + Key + ".trs");
        }

        //结束一次正常测试
        public void FinishExpectedTest()
        {
            //先切换状态，然后执行耗时操作
            //TestStatus = TestStatus.EXPECTED_FINNISH;
            //GenTestStatusChangeEvent(TestStatus.RUNNING, TestStatus);
            LastKey = Key;
            RunningTime = (DateTime.Now.Ticks - StartTime.Ticks) / 10000000;
            errorCodeMsgFile.Close();
            Save(Util.GetBasePath() + "//Report//Data//" + Key + ".trs");
        }

        //开始一次正常测试
        public void StartTest()
        {
            CheckTstConfig();
            Key = Util.GenrateKey();
            preTestTime = DateTime.Now;
            StartTime = DateTime.Now;
            errorCodeMsgFile.Open(Key);
            runTimeoutDic = new Dictionary<Board, DateTime>();
            runTimeoutDic.Clear();
            RunningTime = 0;
            //对每个设备发送开始测试指令
            foreach (Rack rack in Racks)
            {
                if (rack.IsTested)
                {
                    foreach (Board board in rack.Boards)
                    {
                        board.IsPassed = true;//全部置为true，因为和上次的待测板卡不一定相同
                        if (board.IPAndPort == "")
                        {
                            continue;
                        }
                        runTimeoutDic.Add(board, DateTime.Now);
                        BaseMessage baseMsg = startTestCase.Request(rack.subType, board.EqId, board.IPAndPort);
                        txMsgQueue.Push(baseMsg);
                    }
                }
            }
            //bool temp = false;
            //foreach (var kv in IDStatusDic)
            //{
            //    temp |= kv.Value;
            //}

            TestStatus = TestStatus.RUNNING;
            GenTestStatusChangeEvent(TestStatus.THRESHOLD, TestStatus);
        }

        //是否是停止测试消息
        public bool IsStopTestMsg(BaseMessage msg)
        {
            return msg is StopTestMsg;
        }

        //是否是握手消息
        private bool IsShakeHandMsg(BaseMessage baseMessage)
        {
            return baseMessage is ShakeHandMsg;
        }

        //是否是心跳或者空闲消息或者握手消息
        private bool IsHeartMsg(BaseMessage msg)
        {
            return (msg is HeartMsg || msg is IdleMsg || msg is ShakeHandMsg || msg is StopTestMsg);
        }

        //获得已测时间
        public long GetRuningDuration()
        {
            return (DateTime.Now.Ticks - StartTime.Ticks) / 10000000;
        }


        //获取消息中的字符串
        public string GetTestInfo(BaseMessage msg)
        {
            string message = "";
            if (msg.Data.Length > 5)
            {
                int strLength = msg.Data.Length - EqIDLength;
                byte[] temp = new byte[strLength];
                Array.Copy(msg.Data, EqIDLength, temp, 0, strLength);
                //message = ByteHelper.Byte2String(temp);
                message = Encoding.Default.GetString(temp);
            }
            return message;
        }

        //获得运行过程中超时的板卡
        private List<Board> GetTimeoutBoard(Dictionary<Board, DateTime> dict, int timeout)
        {
            List<Board> boards = new List<Board>();
            foreach (var b in dict.Keys)
            {
                if (DateTime.Now.Ticks - dict[b].Ticks > (uint)timeout * 10000000)
                {
                    boards.Add(b);
                     byte[] ID = b.EqId;
                    uint uID = BitConverter.ToUInt32(ID, 0);
                    IDStopDic.Remove(uID);
                }
            }
            return boards;
        }

        public string GetSN()
        {
            string sn = "";
            foreach (Rack r in Racks)
            {
                if (r.IsTested)
                {
                    sn = r.SN;
                }
            }
            return sn;
        }

        //清理SN号
        public void ClearSNs()
        {
            //历史SN号清除
            foreach (Rack r in Racks)
            {
                r.SN = "";
            }
        }

        //根据消息的Equip获取板卡
        public Board GetBoard(BaseMessage msg)
        {
            byte[] temp = new byte[5];
            Array.Copy(msg.Data, 0, temp, 0, 5);
            //byte[] equipID = msg.Data;
            uint uID = BitConverter.ToUInt32(temp, 0);
            if (IDBoardDic.ContainsKey(uID))
            {
                return IDBoardDic[uID];
            }
            Board b = new Board();
            b = new Board();
            b.Name = "未知板卡";
            return b;
        }

        //判断此次测试是否通过
        public bool IsPass()
        {
            foreach (Rack r in Racks)
            {
                foreach (Board b in r.Boards)
                {
                    //有一块板卡未通过就失败
                    if (b.IsTested && !b.IsPassed)
                        return false;
                }
            }
            return true;
        }

        //生成状态切换事件
        protected void GenTestStatusChangeEvent(TestStatus last, TestStatus cur)
        {
            TestStatusEventArgs e = new TestStatusEventArgs();
            e.LastStatus = last;
            e.CurStatus = cur;
            EventHandler temp = OnTestStatusChange;
            if (temp != null)
            {
                temp(this, e);
            }
        }
        //生成超时事件
        protected void GenTimeoutEvent(List<Board> boards, TestStatus cur)
        {
            TimeoutEventArgs e = new TimeoutEventArgs();
            e.Boards = boards;
            e.TestStatus = cur;
            EventHandler temp = OnTimeout;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        //生成板卡通过状态切换事件（在Running 状态下）
        protected void GenBoardStatusChangeEvent(Board board)
        {
            BoardStatusEventArgs e = new BoardStatusEventArgs();
            e.Board = board;
            EventHandler temp = OnBoardStatusChange;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        #region 序列化相关
        public void SaveThis()
        {
            //序列化用户当前的配置
            Save(@".\Config\testedRacks.bin");
        }

        private void Save(string pathAndFilename)
        {
            //序列化用户当前的配置
            System.Runtime.Serialization.IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(pathAndFilename, FileMode.OpenOrCreate, FileAccess.Write);
            using (stream)
            {
                formatter.Serialize(stream, this);
            }
        }

        public TestedRacks Load()
        {
            return Load(@".\Config\testedRacks.bin");
        }

        public TestedRacks LoadByKey(string key)
        {
            return Load(Util.GetBasePath() + "//Report//Data//" + key + ".trs");
        }

        private TestedRacks Load(string pathAndFilename)
        {
            try
            {
                System.Runtime.Serialization.IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(pathAndFilename, FileMode.Open, FileAccess.Read);
                TestedRacks tr;
                using (stream)
                {
                    tr = (TestedRacks)formatter.Deserialize(stream);
                }
                return tr;
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<TestedRacks>().Error(ee.Message);
                LogHelper.GetLogger<TestedRacks>().Error(ee.StackTrace);
            }
            return null;
        }

        //执行一次深度复制
        public void CopyFrom(TestedRacks tr)
        {
            this.Key = tr.Key;
            this.Tester = tr.Tester;
            this.SN = tr.SN;
            this.Racks = tr.Racks;
            this.TestStatus = tr.TestStatus;
            this.StartTime = tr.StartTime;
            this.LastKey = tr.LastKey;
            this.preTestTime = tr.preTestTime;
            this.RunningTime = tr.RunningTime;
        }
        #endregion
    }
}
