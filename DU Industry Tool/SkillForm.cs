using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace DU_Industry_Tool
{
    public sealed partial class SkillForm : Form
    {
        private readonly IndustryManager _manager;
        private readonly FlowLayoutPanel _mainPanel;

        public SkillForm(IndustryManager manager)
        {
            InitializeComponent();
            this.SuspendLayout();
            _manager = manager;
            _mainPanel = new FlowLayoutPanel();
            _mainPanel.FlowDirection = FlowDirection.TopDown;
            _mainPanel.WrapContents = false;
            _mainPanel.AutoScroll = true;
            _mainPanel.AutoSize = true;
            _mainPanel.SuspendLayout();
            foreach (var talent in manager.Talents.OrderBy(t => t.Name))
            {
                var panel = new FlowLayoutPanel();
                panel.WrapContents = false;
                panel.AutoSize = true;
                panel.Margin = new System.Windows.Forms.Padding(0);

                var label = new Label();
                label.Text = talent.Name;
                label.AutoSize = false;
                label.Location = new System.Drawing.Point(4, 12);
                label.Size = new System.Drawing.Size(250, 30);
                label.Margin = new System.Windows.Forms.Padding(4,4,4,0);
                label.TabStop = false;
                panel.Controls.Add(label);

                var textbox = new TextBox();
                textbox.Text = talent.Value.ToString();
                textbox.Location = new System.Drawing.Point(258, 0);
                textbox.Size = new System.Drawing.Size(30, 32);
                textbox.MaxLength = 1;
                panel.Controls.Add(textbox);
                _mainPanel.Controls.Add(panel);
                _mainPanel.ScrollControlIntoView(panel);
            }
            _mainPanel.ResumeLayout();
            this.Controls.Add(_mainPanel);
            this.ResumeLayout();
            this.AutoScroll = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(var panel in _mainPanel.Controls.OfType<FlowLayoutPanel>())
            {
                var talentName = (panel.Controls[0] as Label).Text;
                var talentValue = (panel.Controls[1] as TextBox).Text;
                if(int.TryParse(talentValue, out int value))
                {
                    _manager.Talents.First(t => t.Name == talentName).Value = value;
                }
            }
            _manager.SaveTalents();
            this.Close();
        }
    }
}
