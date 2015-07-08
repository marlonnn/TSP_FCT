using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSPTest.Common;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Summer.System.Log;
using System.IO;
using System.Windows.Forms;

namespace TSPTest.Model
{
    //文件存储结构：
    //Report根目录放pdf报告
    //Report目录下的Data放TestRacks二进制文件和日志文件
    //这三种文件使用同一个文件名，后缀分别是pdf trs log
    public class Report
    {
        TestedRacks testedRacks;

        TSPTest.Common.Version version;

        string endString;

        string templetFile;

        string reportTitle;
        string fontFile;
        float fontSizeHead;
        float fontSizeBody;
        public void GeneratePdf()
        {
            GeneratePdf(testedRacks);
        }

        public void ExploreReportFolder()
        {
            //System.Diagnostics.Process.Start("explorer.exe", Util.GetBasePath() + "//Report");
            System.Diagnostics.Process.Start(Util.GetBasePath() + "//Report");
        }

        public void GeneratePdf(TestedRacks tr)
        {
            PdfReader rdr;
            PdfStamper stamper;
            BaseFont baseFont;

            string pdfFile = Util.GetBasePath() + "//Report//" + tr.Key + ".pdf";
            string dataFile = Util.GetBasePath() + "//Report//Data//" + tr.Key + ".trs";
            string logFile = Util.GetBasePath() + "//Report//Data//" + tr.Key + ".log";

            LogHelper.GetLogger<Report>().Debug(pdfFile);
            LogHelper.GetLogger<Report>().Debug(dataFile);
            LogHelper.GetLogger<Report>().Debug(logFile);

            try
            {
                baseFont = BaseFont.CreateFont(fontFile, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
                MessageBox.Show("报告系统的资源文件载入失败，请与开发人员联系。");
                return;
            }

            try
            {
                rdr = new PdfReader(Util.GetBasePath() + templetFile);
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
                MessageBox.Show(string.Format("模板文件载入失败，请检查'{0}'是否存在。",
                    Util.GetBasePath() + templetFile));
                return;
            }

            try
            {
                stamper = new PdfStamper(rdr, new System.IO.FileStream(pdfFile, System.IO.FileMode.Create));
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
                MessageBox.Show(string.Format("报告文件'{0}'创建失败，请关闭软件重新生成报告。", pdfFile));
                rdr.Close();
                return;
            }

            try
            {
                stamper.AcroFields.AddSubstitutionFont(baseFont);

                SetFieldValue(stamper, "pageHead", reportTitle);
                SetFieldValue(stamper, "Head", reportTitle);
                stamper.AcroFields.SetFieldProperty("Head", "textsize", 20.0f, null);

                SetHeadFieldValue(stamper, "ver", string.Format("{0} Build:{1}", version.Ver, version.Build));
                SetHeadFieldValue(stamper, "data", tr.Key + ".trs");
                SetHeadFieldValue(stamper, "gDate", Util.FormateDateTime2(DateTime.Now));

                SetFieldValue(stamper, "tester", tr.Tester);
                SetFieldValue(stamper, "startTime", Util.FormateDateTime(tr.StartTime));
                SetFieldValue(stamper, "SN", tr.GetSN());
                SetFieldValue(stamper, "runningTime", Util.FormateDurationSecondsMaxHour(tr.RunningTime));
                if (tr.IsPass())
                {
                    SetFieldValue(stamper, "IsPass", "PASS");
                    stamper.AcroFields.SetFieldProperty("IsPass", "textsize", 38.0f, null);
                    stamper.AcroFields.SetFieldProperty("IsPass", "textcolor", BaseColor.BLUE, null);
                }
                else
                {
                    SetFieldValue(stamper, "IsPass", "FAIL");
                    stamper.AcroFields.SetFieldProperty("IsPass", "textsize", 38.0f, null);
                    stamper.AcroFields.SetFieldProperty("IsPass", "textcolor", BaseColor.RED, null);
                }
                string testType = "";
                string boardName = "";
                string isBoardPass = "";
                foreach (Rack r in tr.Racks)
                {
                    if(r.IsTested)
                    {
                        testType += r.Name + "\n";
                        foreach (Board b in r.Boards)
                        {
                            if (!b.IsTested)
                                continue;
                            boardName += b.Name + "\n";
                            isBoardPass += b.IsPassed ? "PASS\n" : "FAIL\n";
                        }
                    }

                }

                testType += endString;
                boardName += endString;
                isBoardPass += endString;
                SetFieldValue(stamper, "testType", testType);
                SetFieldValue(stamper, "boardName", boardName);
                SetFieldValue(stamper, "isBoardPass", isBoardPass);

                if (File.Exists(logFile))
                {
                    stamper.AddFileAttachment("Diagnostics logs", null, logFile, "log.txt");
                }

                stamper.FormFlattening = true;//不允许编辑                
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
            }

            try
            {
                stamper.Close();
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
            }

            try
            {
                rdr.Close();
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
            }

            try
            {
                System.Diagnostics.Process.Start(pdfFile);
            }
            catch (Exception ee)
            {
                LogHelper.GetLogger<Report>().Error(ee.Message);
                LogHelper.GetLogger<Report>().Error(ee.StackTrace);
            }
        }

        private void SetHeadFieldValue(PdfStamper stamper, string name, string value)
        {
            stamper.AcroFields.SetField(name, value);
            stamper.AcroFields.SetFieldProperty(name, "textsize", fontSizeHead, null);
            stamper.AcroFields.SetFieldProperty(name, "textcolor", BaseColor.GRAY, null);
        }

        private void SetFieldValue(PdfStamper stamper, string name, string value)
        {
            stamper.AcroFields.SetField(name, value);
            stamper.AcroFields.SetFieldProperty(name, "textsize", fontSizeBody, null);
        }
    }
}
