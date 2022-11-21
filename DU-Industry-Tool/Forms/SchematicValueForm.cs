using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace DU_Industry_Tool
{
    public partial class SchematicValueForm : KryptonForm
    {
        public SchematicValueForm()
        {
            InitializeComponent();
            // This is intended to have a DataGrid that they can paste the ore values into
            // So we parse out the names and store them back in after they save
            foreach(var schema in DUData.Schematics.OrderBy(o => o.Key))
            {
                schematicsGrid.Rows.Add(schema.Value.Name, schema.Value.Cost);
            }
            schematicsGrid.AutoSize = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
