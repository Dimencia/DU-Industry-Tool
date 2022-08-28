using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace DU_Industry_Tool
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            discordLink.Click += LinkOnClick;
            DimenciaGithubLink.Click += LinkOnClick;
            TobiReleasesLink.Click += LinkOnClick;
        }

        private static void LinkOnClick(object sender, EventArgs e)
        {
            if (sender is KryptonLinkLabel klb)
            {
                try
                {
                    System.Diagnostics.Process.Start(klb.Text);
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    if (noBrowser.ErrorCode==-2147467259)
                        KryptonMessageBox.Show(noBrowser.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                catch (System.Exception)
                {
                    KryptonMessageBox.Show("Sorry, could not open the URL!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}
