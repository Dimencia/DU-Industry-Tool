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
    public partial class SchematicValueForm : Form
    {
        private readonly IndustryManager _manager;
        public SchematicValueForm(IndustryManager manager)
        {
            InitializeComponent();
            _manager = manager;
            // This is intended to have a DataGrid that they can paste the ore values into
            // So we parse out the names and store them back in after they save
            foreach(var schema in manager.Schematics.OrderBy(o => o.Key))
            {
                schematicsGrid.Rows.Add(schema.Value.Name, schema.Value.Cost);
            }
            schematicsGrid.AutoSize = true;
            this.AutoSize = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_manager != null)
            {
                foreach(var values in schematicsGrid.Rows)
                {
                    if (!(values is DataGridViewRow row)) continue;
                    var oreName = row.Cells[0].Value as string;
                    var oreValueString = row.Cells[1].Value as string;
                    if(double.TryParse(oreValueString, out double schemaPrice))
                    {
                        var schema = _manager.Schematics.FirstOrDefault(o => o.Value.Name.Equals(oreName, StringComparison.InvariantCultureIgnoreCase));
                        if (!string.IsNullOrEmpty(schema.Key))
                            schema.Value.Cost = schemaPrice;
                    }
                }
                _manager.SaveSchematicValues();
            }
            this.Close();
        }
    }
}
