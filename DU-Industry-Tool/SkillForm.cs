using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//using Timer = System.Threading.Timer;

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
            _mainPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                AutoSize = true
            };
            _mainPanel.SuspendLayout();
            foreach (var talent in manager.Talents.OrderBy(t => t.Name))
            {
                var panel = new FlowLayoutPanel
                {
                    WrapContents = false,
                    AutoSize = true,
                    Margin = new System.Windows.Forms.Padding(0)
                };
                if (manager.ApplicableTalents?.Any() == true && manager.ApplicableTalents.Contains(talent.Name))
                {
                    panel.BackColor = Color.Aquamarine;
                }
                var label = new Label
                {
                    Text = talent.Name,
                    AutoSize = false,
                    Location = new System.Drawing.Point(4, 12),
                    Size = new System.Drawing.Size(250, 30),
                    Margin = new System.Windows.Forms.Padding(4, 4, 4, 0),
                    TabStop = false
                };
                panel.Controls.Add(label);

                var textbox = new TextBox
                {
                    Text = talent.Value.ToString(),
                    Location = new System.Drawing.Point(258, 0),
                    Size = new System.Drawing.Size(30, 32),
                    MaxLength = 1
                };
                panel.Controls.Add(textbox);
                _mainPanel.Controls.Add(panel);
                _mainPanel.ScrollControlIntoView(panel);
            }
            _mainPanel.ResumeLayout();
            this.Controls.Add(_mainPanel);
            this.ResumeLayout();
            this.AutoScroll = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            foreach(var panel in _mainPanel.Controls.OfType<FlowLayoutPanel>())
            {
                var talentName = (panel.Controls[0] as Label).Text;
                var talentValue = (panel.Controls[1] as TextBox).Text;
                if (int.TryParse(talentValue, out var value))
                {
                    _manager.Talents.First(t => t.Name == talentName).Value = value;
                }
            }
            _manager.SaveTalents();
            this.Close();
        }
    }
}
