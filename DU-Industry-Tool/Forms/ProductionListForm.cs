using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Krypton.Toolkit;

namespace DU_Industry_Tool
{
    public partial class ProductionListForm : Form
    {
        private string LoadedFile;
        private IndustryManager Manager { get; }
        public KryptonComboBox RecipeNames => ComboRecipeNames;

        public ProductionListForm(IndustryManager manager)
        {
            InitializeComponent();
            Manager = manager;
            Manager.ProductionBindingList = new BindingList<ProductionItem>
            {
                AllowEdit = true,
                AllowNew = true,
                AllowRemove = true,
                RaiseListChangedEvents = true
            };
            dgvProductionList.DataSource = Manager.ProductionBindingList;
            Column1.Items.AddRange(Manager.RecipeNames.ToArray());
            // wth! AutoComplete popup stays empty no matter what source :(
            //Column1.AutoCompleteCustomSource.AddRange(Manager.RecipeNames.ToArray());
            //Column1.AutoCompleteSource = AutoCompleteSource.ListItems;

            // manually assign click events due to designer occasionally loosing them :(
            BtnAdd.Click += BtnAddOnClick;
            BtnCalculate.Click += BtnCalculateOnClick;
            BtnLoad.Click += BtnLoad_Click;
            BtnSave.Click += BtnSave_Click;
            BtnClear.Click += BtnClear_Click;
        }

        private void BtnAddOnClick(object sender, EventArgs e)
        {
            // make sure the entered text is actually existing in the recipes list
            if (!Manager.RecipeNames.Any(x => x.Equals(ComboRecipeNames.Text, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }
            // Add new recipe, otherwise increase quantity of existing recipe
            try
            {
                var item = Manager.ProductionBindingList.FirstOrDefault(x => x.Name.Equals(ComboRecipeNames.Text, StringComparison.CurrentCultureIgnoreCase));
                if (item == null)
                {
                    Manager.ProductionBindingList.Add(new ProductionItem
                    {
                        Name = ComboRecipeNames.Text,
                        Quantity = (int)NumUpDownQuantity.Value
                    });
                    return;
                }
                item.Name = ComboRecipeNames.Text;
                item.Quantity += (int)NumUpDownQuantity.Value;
            }
            finally
            {
                dgvProductionList.Invalidate(true);
            }
        }

        private void BtnCalculateOnClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            var lastDir = Properties.Settings.Default.ProdListDirectory;
            if (string.IsNullOrEmpty(lastDir) || !Directory.Exists(lastDir))
            {
                lastDir = Application.StartupPath;
            }
            Properties.Settings.Default.Save();

            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = ".json",
                FileName = LoadedFile,
                Filter = @"JSON|*.json|All files|*.*",
                FilterIndex = 1,
                Title = @"Load Production List",
                InitialDirectory = lastDir,
                ShowHelp = false,
                SupportMultiDottedExtensions = false,
            };
            if(dlg.ShowDialog() != DialogResult.OK) return;
            if (!File.Exists(dlg.FileName))
            {
                KryptonMessageBox.Show(@"File could not be loaded!", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Properties.Settings.Default.ProdListDirectory = Path.GetDirectoryName(dlg.FileName);
            Properties.Settings.Default.Save();

            try
            {
                var tmp = JsonConvert.DeserializeObject<List<ProductionItem>>(File.ReadAllText(dlg.FileName));
                if (tmp == null) return;
                Manager.ProductionBindingList.Clear();
                foreach (var entry in tmp)
                {
                    Manager.ProductionBindingList.Add(entry);
                }
                LoadedFile = Path.GetFileName(dlg.FileName);
                LblLoaded.Text = "Loaded: " + LoadedFile;
                LblLoaded.Visible = true;
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(@"Could not load file!\r\n" + ex.Message, @"ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            dgvProductionList.Invalidate(true);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var lastDir = Properties.Settings.Default.ProdListDirectory;
            if (string.IsNullOrEmpty(lastDir) || !Directory.Exists(lastDir))
            {
                lastDir = Application.StartupPath;
            }
            Properties.Settings.Default.Save();

            var dlg = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                FileName = LoadedFile,
                DefaultExt = ".json",
                Filter = @"JSON|*.json|All files|*.*",
                FilterIndex = 1,
                Title = @"Save Production List",
                InitialDirectory = lastDir,
                ShowHelp = false,
                SupportMultiDottedExtensions = false
            };
            if(dlg.ShowDialog() != DialogResult.OK) return;

            if (File.Exists(dlg.FileName))
            {
                if (KryptonMessageBox.Show(@"Overwrite existing file?", @"Overwrite", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }

            Properties.Settings.Default.ProdListDirectory = Path.GetDirectoryName(dlg.FileName);
            Properties.Settings.Default.Save();

            try
            {
                File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(Manager.ProductionBindingList));
                LoadedFile = Path.GetFileName(dlg.FileName);
                LblLoaded.Text = "Loaded: " + LoadedFile;
                LblLoaded.Visible = true;
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(@"Could not write file!\r\n" + ex.Message, @"ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (KryptonMessageBox.Show(@"Really clear list now?", @"Clear List", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            Manager.ProductionBindingList.Clear();
            dgvProductionList.Invalidate(true);
            LblLoaded.Text = "";
            LblLoaded.Visible = false;
        }
    }
}
