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
    public partial class OreValueForm : Form
    {
        private IndustryManager Manager;
        public OreValueForm(IndustryManager manager)
        {
            InitializeComponent();
            Manager = manager;
            // This is intended to have a DataGrid that they can paste the ore values into
            // So we parse out the names and store them back in after they save
            foreach(var ore in manager.Ores.OrderBy(o => o.Name))
            {
                oreGrid.Rows.Add(ore.Name, ore.Value);
            }
            oreGrid.AutoSize = true;
            this.AutoSize = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(var values in oreGrid.Rows)
            {
                var row = values as DataGridViewRow;
                var oreName = row.Cells[0].Value as string;
                var oreValueString = row.Cells[1].Value as string;
                if(double.TryParse(oreValueString, out double oreValue))
                {
                    var oreRecipe = Manager.Ores.FirstOrDefault(o => o.Name.Equals(oreName, StringComparison.InvariantCultureIgnoreCase));
                    if (oreRecipe != null)
                        oreRecipe.Value = oreValue;
                }
            }
            Manager.SaveOreValues();
            this.Close();
        }
    }
}
