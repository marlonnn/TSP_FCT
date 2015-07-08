using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Summer.System.Core;
using TSPTest.UI;
using Summer.System.Log;

namespace TSPTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
	            Application.EnableVisualStyles();
	            Application.SetCompatibleTextRenderingDefault(false);
	            Form formMain = SpringHelper.GetObject<FormMain>("formMain");
	            Application.Run(formMain);
            }
            catch (System.Exception ee)
            {
                LogHelper.GetLogger<FormMain>().Error(ee.InnerException.Message);
                LogHelper.GetLogger<FormMain>().Error(ee.InnerException.StackTrace);
            }
        }
    }
}
