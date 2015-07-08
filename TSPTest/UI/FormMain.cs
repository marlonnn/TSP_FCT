using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.AdvTree;
using Summer.System.Core;
using Summer.System.Log;
using System.Threading;
using TSPTest.Model;
using System.Runtime.InteropServices;
using TSPTest.Net;
using TSPTest.Common;
using TSPTest.Protocol;

namespace TSPTest.UI
{
    public partial class FormMain : Office2007RibbonForm
    {
        TestedRacks testedRacks;
        FormTestConfig formTestConfig;
        FormAbout formAbout;
        Udp udp;
        RxMsgQueue rxMsgQueue;
        DataGridViewRowSorter dataGridViewRowSorter;
        Report report;

        public FormMain()
        {
            InitializeComponent();
        }

        public void DisplayTree(TestedRacks testedRacks)
        {
            rackAdvTree.Nodes.Clear();
            Node baseroot = new Node();
            baseroot.Tag = "TSP";
            baseroot.Text = "TSP";
            baseroot.Expand();
            Node root = null;
            Node teed = null;
            foreach (Rack rack in testedRacks.Racks)
            {
                root = new Node();
                root.Text = rack.Name;
                root.Tag = rack;
                root.CheckBoxVisible = true;
                root.Checked = rack.IsTested;
                baseroot.Nodes.Add(root);
                if (rack.Name.Contains("部件功能测试"))
                {
                    root.Expand();
                }
                foreach (Board board in rack.Boards)
                {
                    teed = new Node();
                    teed.Text = board.Name;
                    teed.Tag = board;
                    if (!board.Name.Contains("空"))
                    {
                        if (board.IsTested)
                        {
                            teed.Checked = true;
                        }
                        teed.Expand();
                        root.Nodes.Add(teed);
                    }
                }
            }
            rackAdvTree.Nodes.Add(baseroot);
            rackAdvTree.Refresh();
        }

        private void SetNodeColor(TestedRacks testedRacks, TestStatus testStatus)
        {
            foreach (Node basenode in rackAdvTree.Nodes)
            {
                foreach (Node rack in basenode.Nodes)
                {
                    Rack r = (Rack)rack.Tag;
                    if(r.IsTested)
                    {
                        foreach (Node board in rack.Nodes)
                        {
                            Board b = (Board)board.Tag;
                            switch (testStatus)
                            {
                                case TestStatus.THRESHOLD:
                                    break;
                                case TestStatus.HANDS_OK:
                                    break;
                                case TestStatus.RUNNING:
                                    if (!b.IsPassed)
                                    {
                                        SetAdvTreeNodeColor(Color.Red, board);
                                    }
                                    break;
                                case TestStatus.UNEXPECTED_FINNISH:
                                    break;
                                case TestStatus.EXPECTED_FINNISH:
                                    if (b.IsTested)
                                    {
                                        if (b.IsPassed)
                                        {
                                            SetAdvTreeNodeColor(Color.Green, board);
                                        }
                                        else
                                        {
                                            SetAdvTreeNodeColor(Color.Red, board);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void SetAdvTreeNodeColor(Color color, Node node)
        {
            ElementStyle style = new ElementStyle();
            style.BackColor = color;
            node.Style = style;
        }

        private void ReSetNodeColor()
        {
            foreach (Node basenode in rackAdvTree.Nodes)
            {
                foreach (Node rack in basenode.Nodes)
                {
                    foreach (Node board in rack.Nodes)
                    {
                        SetAdvTreeNodeColor(Color.Transparent, board);
                    }
                }
            }
        }

        private void SetLabelResult(TestStatus testStatus)
        {
            switch (testStatus)
            {
                case TestStatus.THRESHOLD:
                    lblResult.ForeColor = Color.Maroon;
                    lblResult.Text = "正在握手...";
                    break;
                case TestStatus.HANDS_NOTOK:
                    lblResult.ForeColor = Color.Red;
                    lblResult.Text = "握手失败";
                    break;
                case TestStatus.HANDS_OK:
                    lblResult.ForeColor = Color.Green;
                    lblResult.Text = "握手成功";
                    break;
                case TestStatus.RUNNING:
                    lblResult.ForeColor = Color.Gray;
                    lblResult.Text = "测试中...";
                    break;
                case TestStatus.UNEXPECTED_FINNISH:
                    lblResult.ForeColor = Color.Red;
                    lblResult.Text = "异常结束";
                    break;
                case TestStatus.EXPECTED_FINNISH:
                    if (testedRacks.IsPass())
                    {
                        lblResult.ForeColor = Color.Green;
                        lblResult.Text = "测试通过";
                    }
                    else
                    {
                        lblResult.ForeColor = Color.Red;
                        lblResult.Text = "测试失败";
                    }
                    break;
            }
        }

        private void ReSetAllUIStatus()
        {
            ReSetNodeColor();
            logList.Rows.Clear();
            lblResult.Text = "";
        }

        private void btnShakeHand_Click(object sender, EventArgs e)
        {
            try
            {
                ReSetAllUIStatus();
                mainTimer.Enabled = true;
                testedRacks.ShakeHands();
            }
            catch (System.Exception ex)
            {
                setUIEnabled(false);
                MessageBox.Show((ex.Message));
            }
        }

        private void btnRack_Click(object sender, EventArgs e)
        {
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (formTestConfig.ShowDialog() == DialogResult.OK)
            {
                ReSetAllUIStatus();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (testedRacks.TestStatus == TestStatus.HANDS_OK)
                {
                    ReSetAllUIStatus();
                    mainTimer.Enabled = true;
                    setUIEnabled(true);
                    ReSetNodeColor();
                    testedRacks.StartTest();
                }
                else
                {
                    DialogResult result = MessageBox.Show("开始测试前请先保证所有设备握手成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show((ex.Message));
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            if(testedRacks.TestStatus == TestStatus.RUNNING)
            {
                testedRacks.FinishUnExpectedTest();
                testedRacks.ClearSNs();
                testedRacks.SaveThis();
            }
            else if (testedRacks.TestStatus == TestStatus.EXPECTED_FINNISH)
            {
                testedRacks.FinishExpectedTest();
                report.GeneratePdf();
                testedRacks.ClearSNs();
                testedRacks.SaveThis();
            }
            mainTimer.Enabled = false;
            setUIEnabled(true);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
            	report.ExploreReportFolder();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("未开始一次测试，没有任何测试日志！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void setUIEnabled(bool b)
        {
            //btnStart.Enabled = b;
            //btnFinish.Enabled = b;
        }

        private void LoadingCircle(bool flag)
        {
            loadCircle.OuterCircleRadius = 45;
            loadCircle.InnerCircleRadius = 25;
            loadCircle.Visible = flag;
            loadCircle.Active = flag;
        }

        private void UpdateGridData(Board board)
        {
            int index = this.logList.Rows.Add();//添加一行
            this.logList.Rows[index].Cells[0].Value = DateTime.Now.ToString("HH:mm:ss:fff");//第一列是时间
            this.logList.Rows[index].Cells[1].Value = board.Name;//第二列是板卡
            this.logList.Rows[index].Cells[2].Value = "握手失败";//第三列是状态
            this.logList.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
        }

        private void UpdateGridData(BaseMessage msg)
        {
            Board board = testedRacks.GetBoard(msg);
            string logStr = testedRacks.GetTestInfo(msg);
            if(board.Name.Contains("STBY"))
            {
                int index = this.logList.Rows.Add();//添加一行
                this.logList.Rows[index].Cells[0].Value = DateTime.Now.ToString("HH:mm:ss:fff");//第一列是时间
                this.logList.Rows[index].Cells[1].Value = board.Name;//第二列是板卡
                this.logList.Rows[index].Cells[2].Value = logStr;//第三列是状态
                 if (testedRacks.BoardErrorStatus(msg))
                 {
                     this.logList.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                 }
                 else
                 {
                     this.logList.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Green;
                 }
            }
            else
            {
                if (testedRacks.BoardErrorStatus(msg))
                {
                    int index = this.logList.Rows.Add();//添加一行
                    this.logList.Rows[index].Cells[0].Value = DateTime.Now.ToString("HH:mm:ss:fff");//第一列是时间
                    this.logList.Rows[index].Cells[1].Value = board.Name;//第二列是板卡
                    this.logList.Rows[index].Cells[2].Value = logStr;//第三列是状态
                    this.logList.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        //更新日志区列表
        private void mainTimer_Tick(object sender, EventArgs e)
        {
            List<BaseMessage> list = rxMsgQueue.PopAll();
            //转发所有消息
            foreach (BaseMessage em in list)
            {
                testedRacks.AppendLog(em);
            }

            //只有running状态才更新日志列表(除0x7F外所有消息均显示)
            if (testedRacks.TestStatus == TestStatus.RUNNING)
            {
                foreach (var msg in list)
                {
                    if (msg is HeartMsg || msg is IdleMsg || msg is HeartTimeoutMsg || msg is StopTestMsg)
                        continue;
                    //if(testedRacks.IsStopTestMsg(msg))
                    //{
                    //    //是停止测试消息，停止测试
                    //    //testedRacks.FinishExpectedTest();
                    //    //report.GeneratePdf();
                    //    //testedRacks.ClearSNs();
                    //    //mainTimer.Enabled = false;
                    //    testedRacks.CheckStop(msg);
                    //}
                    UpdateGridData(msg);
                }
                lblTime.Text = Util.FormateDurationSecondsMaxHour2(testedRacks.GetRuningDuration());
            }

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
	            TestedRacks tc = testedRacks.Load();
	            if (tc != null)
	            {
	                testedRacks.CopyFrom(tc);
	            }
                else
                {
                    testedRacks.SaveThis();
                }
            }
            catch (System.Exception ex)
            {
            	
            }

            testedRacks.TestStatus = TestStatus.EXPECTED_FINNISH;
            testedRacks.OnTestStatusChange += new EventHandler(onChangeStatus);
            testedRacks.OnTimeout += new EventHandler(onTimeout);
            testedRacks.OnBoardStatusChange += new EventHandler(onBoardStatus);

            setUIEnabled(false);
            DisplayTree(testedRacks);
            udp.UdpTxPrepare();
            udp.UpdRxStart();
            Thread udpTxStartThread = new Thread(new ThreadStart(udp.UdpTxStart));
            udpTxStartThread.Start();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            formAbout.ShowDialog();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {

        }

        private void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (testedRacks.TestStatus == TestStatus.RUNNING)
            {
                DialogResult result = MessageBox.Show("软件关闭前请先点击 停止测试 按钮！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }
            else
            {
                try
                {
                    Quartz.Impl.StdScheduler scheduler = (Quartz.Impl.StdScheduler)SpringHelper.GetContext().GetObject("scheduler");
                    scheduler.Shutdown();

                    udp.UdpClose();
                }
                catch (Exception ee)
                {
                    LogHelper.GetLogger<FormMain>().Error(ee.Message);
                    LogHelper.GetLogger<FormMain>().Error(ee.StackTrace);
                }
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
        }

        private void onChangeStatus(object sender, EventArgs e)
        {
            TestedRacks tr = sender as TestedRacks;
            TestStatusEventArgs args = e as TestStatusEventArgs;
            switch (args.CurStatus)
            {
                case TestStatus.THRESHOLD:
                    SetNodeColor(testedRacks, TestStatus.THRESHOLD);
                    SetLabelResult(TestStatus.THRESHOLD);
                    LoadingCircle(true);
                    break;
                case TestStatus.HANDS_OK:
                    SetNodeColor(testedRacks, TestStatus.HANDS_OK);
                    SetLabelResult(TestStatus.HANDS_OK);
                    setUIEnabled(true);
                    LoadingCircle(false);
                    break;
                case TestStatus.HANDS_NOTOK:
                    SetLabelResult(TestStatus.HANDS_NOTOK);
                    LoadingCircle(false);
                    break;
                case TestStatus.RUNNING:
                    SetNodeColor(testedRacks, TestStatus.RUNNING);
                    SetLabelResult(TestStatus.RUNNING);
                    break;
                case TestStatus.UNEXPECTED_FINNISH:
                    SetNodeColor(testedRacks, TestStatus.UNEXPECTED_FINNISH);
                    SetLabelResult(TestStatus.UNEXPECTED_FINNISH);
                    LoadingCircle(false);
                    setUIEnabled(false);
                    break;
                case TestStatus.EXPECTED_FINNISH:
                    this.btnFinish_Click(null, null);
                    SetNodeColor(testedRacks, TestStatus.EXPECTED_FINNISH);
                    SetLabelResult(TestStatus.EXPECTED_FINNISH);
                    break;
            }
        }

        private void onTimeout(object sender, EventArgs e)
        {
            TestedRacks tr = sender as TestedRacks;
            TimeoutEventArgs args = e as TimeoutEventArgs;
            List<Board> Boards = args.Boards;
            foreach (Board b in Boards)
            {
                UpdateGridData(b);
            }
        }

        private void onBoardStatus(object sender, EventArgs e)
        {
            TestedRacks tr = sender as TestedRacks;
            BoardStatusEventArgs args = e as BoardStatusEventArgs;
            if (!args.Board.IsPassed)
            {
                SetNodeColor(testedRacks, TestStatus.RUNNING);
            }
        }

        private void rackAdvTree_AfterCheck(object sender, AdvTreeCellEventArgs e)
        {
            foreach(Node baseNode in rackAdvTree.Nodes)
            {
                foreach(Node rootNode in baseNode.Nodes)
                {
                    Rack rack = (Rack)rootNode.Tag;
                    rack.IsTested = rootNode.Checked;
                }
            }
            testedRacks.SaveThis();
        }

        private void rackAdvTree_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            Node node = e.Node;
            if(e.Node.Tag is Board)
            {
                Board board = (Board)e.Node.Tag;
                string selectString = board.Name;
                Thread thread = new Thread(new ParameterizedThreadStart(SortDataGridView));
                thread.Start((object)selectString);
            }
            else if (e.Node.Tag is Rack)
            {
                //
            }
        }

        private void SortDataGridView(object name)
        {
            if (InvokeRequired)
            {
                var action = new Action<object>(SortDataGridView);
                this.Invoke(action, name);
            }
            else
            {
                dataGridViewRowSorter.SortColumn = 1;
                dataGridViewRowSorter.OrderOfSort = SortOrder.Ascending;
                logList.Sort(dataGridViewRowSorter);
                int count = logList.Rows.Count;
                string boardName = (string)name;
                for (int i = 0; i < count; i++)
                {
                    if (logList.Rows[i].Cells[1].Value.ToString().Contains(boardName))
                    {
                        this.logList.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.Yellow;
                    }
                    else
                    {
                        this.logList.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.Empty;
                    }
                    this.logList.Update();
                }
            }
        }
    }
}
