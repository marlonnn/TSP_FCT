using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Summer.System.Log;
using Spring.Scheduling.Quartz;
using Quartz;
using TSPTest.Common;
using TSPTest.Protocol;

namespace TSPTest.Model
{
    [Serializable]
    public class ErrorCodeMsgFile
    {
        static StreamWriter sw;
        static object lockFile = new object();
        public void Open(string key)
        {
            try
            {
                lock (lockFile)
                {
                    sw = new StreamWriter(Util.GetBasePath() + "\\Report\\Data\\" + key + ".log");
                }
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.Message);
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.StackTrace);
            }
        }

        public void Close()
        {
            try
            {
                lock (lockFile)
                {
                    if (sw != null)
                    {
                        sw.Close();
                        sw = null;
                    }
                }
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.Message);
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.StackTrace);
            }
        }

        public void Append(BaseMessage msg,string board,string testInfo)
        {
            try
            {
                lock (lockFile)
                {
                    if (sw != null)
                    {
                        sw.WriteLine("{0},{1},{2}",
                            Util.FormateDateTime3(DateTime.Now), board,testInfo);
                    }
                }
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.Message);
                LogHelper.GetLogger<ErrorCodeMsgFile>().Error(ee.StackTrace);
            }
        }

        public void Flush()
        {
            LogHelper.GetLogger("job").Debug("Flush Job Start.");
            lock (lockFile)
            {
                if (sw != null)
                {
                    sw.Flush();
                }
            }
            LogHelper.GetLogger("job").Debug("Flush Job Finish.");
        }
    }
}
