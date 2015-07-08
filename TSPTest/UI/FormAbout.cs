using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace TSPTest.UI
{
    public partial class FormAbout : Office2007RibbonForm
    {
        TSPTest.Common.Version version;
        public FormAbout()
        {
            InitializeComponent();
            //InitVersion();
        }

        public void InitVersion()
        {
            lblVersion.Text = String.Format("版本：{0}_{1}", version.Ver, version.Build);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formAbout_Load(object sender, EventArgs e)
        {
            InitVersion();
        }
    }
}
