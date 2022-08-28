using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using Krypton.Toolkit;
using Krypton.Navigator;
using Krypton.Workspace;
using Krypton.Docking;

namespace DU_Industry_Tool
{
    public partial class ContentDocument : UserControl
    {
        public ContentDocument()
        {
            InitializeComponent();
        }

        public FlowLayoutPanel InfoPanel { get; private set; }
        public FlowLayoutPanel CostDetailsPanel { get; set; }

        private void ContentDocument_Load(object sender, EventArgs e)
        {
            //Console.WriteLine("ContentDocument_Load");
        }
    }
}
