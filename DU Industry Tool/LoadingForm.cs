using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Industry_Tool
{
    public partial class LoadingForm : Form
    {
        private readonly MarketManager Market;

        public bool DiscardOres { get; set; } = true;
        public LoadingForm(MarketManager market)
        {
            InitializeComponent();
            Market = market;
            textBox1.Text = market._logFolderPath;
        }

        public void UpdateProgressBar(int progress)
        {
            Invoke((MethodInvoker)delegate {
                progressBar1.Value = progress;
                if (progress >= 100)
                {
                    this.Close();
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Market._logFolderPath = textBox1.Text;
            button1.Enabled = false;
            textBox1.Enabled = false;
            checkBox1.Enabled = false;
            DiscardOres = !checkBox1.Checked;
            Task.Run(() => Market.UpdateMarketData(this));
        }
    }
}
