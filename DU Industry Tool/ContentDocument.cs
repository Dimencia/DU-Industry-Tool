using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Workspace;
using ComponentFactory.Krypton.Docking;

namespace DU_Industry_Tool
{
    public partial class ContentDocument : UserControl
    {
        public ContentDocument()
        {
            InitializeComponent();
        }

        public FlowLayoutPanel InfoPanel => this.infoPanel;

        private void ContentDocument_Load(object sender, EventArgs e)
        {
            Console.WriteLine("ContentDocument_Load");
        }
    }
}
