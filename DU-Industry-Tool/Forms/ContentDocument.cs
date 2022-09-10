using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
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

        public FlowLayoutPanel CostDetailsPanel { get; set; }
    }
}
