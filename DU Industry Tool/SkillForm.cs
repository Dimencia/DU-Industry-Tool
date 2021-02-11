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
    public partial class SkillForm : Form
    {
        private IndustryManager Manager;
        private FlowLayoutPanel mainPanel;

        public SkillForm(IndustryManager manager)
        {
            InitializeComponent();
            Manager = manager;
            mainPanel = new FlowLayoutPanel();
            mainPanel.FlowDirection = FlowDirection.TopDown;
            mainPanel.WrapContents = false;
            mainPanel.AutoScroll = true;
            mainPanel.AutoSize = true;

            foreach (var talent in manager.Talents.OrderBy(t => t.Name))
            {
                var panel = new FlowLayoutPanel();
                panel.WrapContents = false;
                panel.AutoSize = true;

                var label = new Label();
                label.Text = talent.Name;
                label.AutoSize = true;
                panel.Controls.Add(label);

                var textbox = new TextBox();
                textbox.Text = talent.Value.ToString();
                panel.Controls.Add(textbox);

                mainPanel.Controls.Add(panel);
            }
            this.Controls.Add(mainPanel);
            this.AutoScroll = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(var panel in mainPanel.Controls.OfType<FlowLayoutPanel>())
            {
                var talentName = (panel.Controls[0] as Label).Text;
                var talentValue = (panel.Controls[1] as TextBox).Text;
                if(int.TryParse(talentValue, out int value))
                {
                    Manager.Talents.Where(t => t.Name == talentName).First().Value = value;
                }
            }
            Manager.SaveTalents();
            this.Close();
        }
    }
}
