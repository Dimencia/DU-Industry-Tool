using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using Krypton.Toolkit;
using Krypton.Navigator;
using Krypton.Workspace;
using Krypton.Docking;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DU_Industry_Tool.Properties;

// ReSharper disable LocalizableElement

namespace DU_Industry_Tool
{
    public partial class MainForm : KryptonForm
    {
        private readonly IndustryManager _manager;
        private readonly MarketManager _market;
        private bool _marketFiltered;
        private TextBox _costDetailsLabel;
        private int _costDetailsLineCount;
        private readonly List<string> _breadcrumbs = new List<string>();
        private FlowLayoutPanel _costDetailsPanel;
        private FlowLayoutPanel _infoPanel;
        private bool _navUpdating;

        public MainForm(IndustryManager manager)
        {
            InitializeComponent();

            CultureInfo.CurrentCulture = new CultureInfo("en-us");
            QuantityBox.SelectedIndex = 0;

            // Setup the trees. One recipe on each main node
            _manager = manager;

            _market = new MarketManager();
            kryptonPage1.Flags = 0;
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowDocked);
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowClose);

            treeView.AfterSelect += Treeview_AfterSelect;
            treeView.NodeMouseClick += Treeview_NodeClick;
            treeView.BeginUpdate();
            foreach(var group in manager.Groupnames)
            {
                var groupNode = new TreeNode(group);
                foreach(var recipe in manager.Recipes.Where(x => x.Value?.ParentGroupName?.Equals(group, StringComparison.CurrentCultureIgnoreCase) == true)
                            .OrderBy(r => r.Value.Level).ThenBy(r => r.Value.Name)
                            .Select(x => x.Value))
                {
                    var recipeNode = new TreeNode(recipe.Name)
                    {
                        Tag = recipe
                    };
                    recipe.Node = recipeNode;
                    groupNode.Nodes.Add(recipeNode);
                }
                treeView.Nodes.Add(groupNode);
            }
            treeView.EndUpdate();
            OnMainformResize(null, null);
        }

        private void Treeview_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && treeView.SelectedNode == e.Node)
            {
                SelectRecipe(e.Node);
            }
        }

        private void Treeview_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectRecipe(e?.Node);
        }

        private void SelectRecipe(TreeNode e)
        {
            if (!(e?.Tag is SchematicRecipe recipe))
            {
                return;
            }

            if (_breadcrumbs.Count == 0 || _breadcrumbs.LastOrDefault() != recipe.Name)
            {
                _breadcrumbs.Add(recipe.Name);
            }
            PreviousButton.Enabled = _breadcrumbs.Count > 0;

            // Display recipe info for the thing they have selected
            Console.WriteLine(recipe.Name);
            SearchBox.Text = recipe.Name;
            _navUpdating = true;
            ContentDocument newDoc = null;
            try
            {
                newDoc = NewDocument(recipe.Name);
                if (newDoc == null || _infoPanel == null) return;
            }
            finally
            {
                _navUpdating = false;
            }
            _infoPanel.Controls.Clear();

            var lbl = AddLabel(_infoPanel.Controls, $"{recipe.Name} (T{recipe.Level})");
            lbl.Font = new Font(_infoPanel.Font.FontFamily, 12f, FontStyle.Bold);
            lbl.Padding = new Padding(2, 5, 4, 2);
            lbl.Height = 30;
            lbl.Width = 370;
            lbl = AddLabel(_infoPanel.Controls, $"Unit mass: {recipe.UnitMass:N1} volume: {recipe.UnitVolume:N1}"+(recipe.Nanocraftable ? "  *nanocraftable*" : ""));
            lbl.Padding = new Padding(4, 0, 4, 5);

            _manager.ProductQuantity = int.Parse(QuantityBox.Text);
            if (!double.TryParse(QuantityBox.Text, out var cnt)) cnt = 1d;
            var costToMake = _manager.GetTotalCost(recipe.Key, cnt, silent: true);
            AddFlowLabel(_infoPanel.Controls, "Cost To Make " + costToMake.ToString("N02") + "q");

            var cost = _manager.GetBaseCost(recipe.Key);
            AddFlowLabel(_infoPanel.Controls, $"Untalented (without schematics) {cost:N1}q");

            cost = recipe.Time > 0 ? 86400/recipe.Time : 0;
            var pnl = AddFlowLabel(_infoPanel.Controls, $"Per Industry {cost:N1} / Day");
            pnl.Padding = new Padding(0, 0, 0, 10);

            // IDK why sometimes prices are listed as 0
            var orders = _market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId &&
                                                                o.BuyQuantity < 0 &&
                                                                DateTime.Now < o.ExpirationDate &&
                                                                o.Price > 0);
            var mostRecentOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
            if (mostRecentOrder?.Price > 0.00d)
            {
                var costPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Padding = new Padding(0)
                };
                AddLabel(costPanel.Controls, "Market " + mostRecentOrder.Price.ToString("N02") + "q");
                AddLabel(costPanel.Controls, "Until " + mostRecentOrder.ExpirationDate);
                AddLabel(costPanel.Controls, "Profit Margin ");
                cost = ((mostRecentOrder.Price-costToMake) / mostRecentOrder.Price);
                AddLabel(costPanel.Controls, cost.ToString("0%"));
                cost = (mostRecentOrder.Price - costToMake)*(86400/recipe.Time);
                AddLabel(costPanel.Controls, "Profit/Day/Industry " + cost.ToString("N02") + "q");
                _infoPanel.Controls.Add(costPanel);
            }

            // ----- Ingredients -----
            var newPanel = AddFlowLabel(_infoPanel.Controls, "Ingredients", FontStyle.Bold);
            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = recipe.Ingredients.Count,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0,0,0,10)
            };
            newPanel.SuspendLayout();
            grid.SuspendLayout();
            foreach (var ingredient in recipe.Ingredients)
            {
                AddLinkedLabel(grid.Controls, ingredient.Name, ingredient.Type).Click += Label_Click;
                AddLabel(grid.Controls, ingredient.Quantity.ToString("0.0"));
            }
            grid.ResumeLayout();
            newPanel.Controls.Add(grid);
            newPanel.ResumeLayout();

            // ----- Products -----
            newPanel = AddFlowLabel(_infoPanel.Controls, "Products", FontStyle.Bold);
            grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = recipe.Products.Count,
                AutoSize = true,
                Padding = new Padding(0,4,0,10)
            };
            newPanel.SuspendLayout();
            grid.SuspendLayout();
            foreach (var prod in recipe.Products)
            {
                if (prod.Type == recipe.Key)
                    AddLabel(grid.Controls, prod.Name);
                else
                    AddLinkedLabel(grid.Controls, prod.Name, prod.Type).Click += Label_Click;
                AddLabel(grid.Controls, prod.Quantity.ToString("0.0"));
            }
            grid.ResumeLayout();
            newPanel.Controls.Add(grid);
            newPanel.ResumeLayout();

            if (recipe.ParentGroupName.EndsWith("Ore", StringComparison.InvariantCultureIgnoreCase) ||
                recipe.ParentGroupName.EndsWith("Parts", StringComparison.InvariantCultureIgnoreCase) ||
                recipe.ParentGroupName.EndsWith("Product", StringComparison.InvariantCultureIgnoreCase) ||
                recipe.ParentGroupName.EndsWith("Pure", StringComparison.InvariantCultureIgnoreCase) ||
                recipe.Name?.StartsWith("Relic Plasma", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var containedIn = _manager.Recipes.Values.Where(x =>
                    true == x.Ingredients?.Any(y => y.Name.Equals(recipe.Name, StringComparison.InvariantCultureIgnoreCase))).ToList();
                if (containedIn?.Any() == true)
                {
                    newPanel = AddFlowLabel(_infoPanel.Controls, "Part of recipes:", FontStyle.Bold);
                    grid = new TableLayoutPanel
                    {
                        AutoScroll = true,
                        ColumnCount = 1,
                        Padding = new Padding(0),
                        RowCount = containedIn.Count(),
                        Height = 400,
                        Width = 450,
                        VerticalScroll = { Visible = true }
                    };
                    newPanel.SuspendLayout();
                    grid.SuspendLayout();
                    foreach (var entry in containedIn)
                    {
                        AddLinkedLabel(grid.Controls, entry.Name, entry.Key).Click += Label_Click;
                    }
                    grid.ResumeLayout();
                    newPanel.Controls.Add(grid);
                    newPanel.ResumeLayout();
                }
            }

            _costDetailsPanel = new FlowLayoutPanel();
            _costDetailsPanel.SuspendLayout();
            try
            {
                _costDetailsPanel.FlowDirection = FlowDirection.TopDown;
                _costDetailsPanel.BorderStyle = BorderStyle.None;
                _costDetailsPanel.Dock = DockStyle.None;
                _costDetailsPanel.AutoSize = true;
                _costDetailsPanel.AutoScroll = true;
                _costDetailsPanel.Font = new Font("Lucida Console", 10F, FontStyle.Regular, GraphicsUnit.Point, (0));
                _costDetailsPanel.Size = new Size(600, 500);
                _costDetailsLabel = new TextBox
                {
                    AutoSize = true,
                    BorderStyle = BorderStyle.None,
                    ScrollBars = ScrollBars.Both,
                    Size = new Size(600, 500),
                    Text = _manager.CostResults.ToString(),
                    Multiline = true,
                    WordWrap = false,
                    ReadOnly = true
                };
                _costDetailsPanel.Controls.Add(_costDetailsLabel);
                _infoPanel.Controls.Add(_costDetailsPanel);
                _costDetailsLineCount = _costDetailsLabel.Text.Count(c => c == '\n');
            }
            finally
            {
                _costDetailsPanel.ResumeLayout();
                newDoc.CostDetailsPanel = _costDetailsPanel;
            }

            _infoPanel.AutoScroll = false;
            OnMainformResize(null, null);
        }

        private FlowLayoutPanel AddFlowLabel(System.Windows.Forms.Control.ControlCollection cc, string lblText, FontStyle fstyle = FontStyle.Regular)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0)
            };
            var lbl = new Label
            {
                AutoSize = true,
                Font = new Font(_infoPanel.Font, fstyle),
                Padding = new Padding(0),
                Text = lblText
            };
            panel.Controls.Add(lbl);
            cc.Add(panel);
            return panel;
        }

        private Label AddLabel(System.Windows.Forms.Control.ControlCollection cc, string lblText, FontStyle fstyle = FontStyle.Regular)
        {
            var lbl = new Label
            {
                AutoSize = true,
                Font = new Font(_infoPanel.Font, fstyle),
                Padding = new Padding(0),
                Text = lblText
            };
            cc.Add(lbl);
            return lbl;
        }

        private Label AddLinkedLabel(System.Windows.Forms.Control.ControlCollection cc, string lblText, string lblKey)
        {
            var lbl = AddLabel(cc, lblText, FontStyle.Underline);
            lbl.ForeColor = Color.CornflowerBlue;
            lbl.Text = lblText;
            lbl.Tag = lblKey;
            return lbl;
        }

        private void Label_Click(object sender, EventArgs e)
        {
            if (!(sender is Label label)) return;
            if (!(label.Tag is string tag)) return;
            if (!_manager.Recipes.ContainsKey(tag))
            {
                return;
            }
            var recipe = _manager.Recipes[tag];

            if (_breadcrumbs.Count == 0 || _breadcrumbs.LastOrDefault() != recipe.Name)
            {
                _breadcrumbs.Add(recipe.Name);
            }
            PreviousButton.Enabled = _breadcrumbs.Count > 0;

            var outerNodes = treeView.Nodes.OfType<TreeNode>();
            TreeNode targetNode = null;
            foreach(var outerNode in outerNodes)
            {
                foreach(var innerNode in outerNode.Nodes.OfType<TreeNode>())
                {
                    if (!innerNode.Text.Equals(recipe.Name, StringComparison.CurrentCultureIgnoreCase)) continue;
                    targetNode = innerNode;
                    break;
                }
                if (targetNode != null) break;
            }
            if (targetNode == null) return;
            treeView.SelectedNode = targetNode;
            treeView.SelectedNode.EnsureVisible();
            SearchBox.Text = targetNode.Text;
        }

        private void InputOreValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new OreValueForm(_manager);
            form.ShowDialog(this);
        }

        private void SkillLevelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new SkillForm(_manager);
            form.ShowDialog(this);
        }

        private void SchematicValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new SchematicValueForm(_manager);
            form.ShowDialog(this);
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_breadcrumbs.Count == 0) return;
                var entry = _breadcrumbs.LastOrDefault();
                if (entry == SearchBox.Text)
                {
                    _breadcrumbs.Remove(entry);
                    entry = _breadcrumbs.LastOrDefault();
                }
                if (string.IsNullOrEmpty(entry)) return;
                SearchBox.Text = entry;
                _breadcrumbs.Remove(entry);
                SearchButton_Click(sender, null);
            }
            finally
            {
                PreviousButton.Enabled = _breadcrumbs.Count > 0;
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            var searchValue = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                PreviousButton.Enabled = _breadcrumbs.Count > 0;
                return; // Do nothing
            }

            var outerNodes = treeView.Nodes.OfType<TreeNode>();
            TreeNode firstResult = null;
            treeView.BeginUpdate();
            try
            {
                treeView.CollapseAll();
                foreach (var outerNode in outerNodes)
                {
                    foreach (var innerNode in outerNode.Nodes.OfType<TreeNode>())
                    {
                        if (innerNode.Text.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) < 0)
                            continue;
                        innerNode.EnsureVisible();
                        var isExact = innerNode.Text.Equals(searchValue, StringComparison.InvariantCultureIgnoreCase);
                        if (firstResult == null || isExact)
                        {
                            firstResult = innerNode;
                            if (isExact) break;
                        }
                    }
                }
                if (firstResult != null)
                {
                    treeView.SelectedNode = firstResult;
                    treeView.SelectedNode.EnsureVisible();
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
            treeView.Focus();
        }

        private void QuantityBoxOnSelectionChangeCommitted(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                SearchButton_Click(sender, e);
            }
            else
            {
                SelectRecipe(treeView.SelectedNode);
            }
        }

        private void UpdateMarketValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var loadForm = new LoadingForm(_market);
            loadForm.ShowDialog(this);
            if (loadForm.DiscardOres)
            {
                // Get rid of them
                List<ulong> toRemove = new List<ulong>();
                foreach(var order in _market.MarketOrders)
                {
                    var recipe = _manager.Recipes.Values.Where(r => r.NqId == order.Value.ItemType).FirstOrDefault();
                    if (recipe != null && recipe.ParentGroupName == "Ore")
                        toRemove.Add(order.Key);

                }
                foreach (var key in toRemove)
                    _market.MarketOrders.Remove(key);
                _market.SaveData();
            }
            else
            {
                // Process them and leave them so they show in exports
                foreach (var order in _market.MarketOrders)
                {
                    var recipe = _manager.Recipes.Values.Where(r => r.NqId == order.Value.ItemType).FirstOrDefault();
                    if (recipe != null && recipe.ParentGroupName == "Ore")
                    {
                        var ore = _manager.Ores.Where(o => o.Key.ToLower() == recipe.Key.ToLower()).FirstOrDefault();
                        if (ore != null)
                        {
                            var orders = _market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId && o.BuyQuantity < 0 && DateTime.Now < o.ExpirationDate && o.Price > 0);

                            var bestOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
                            if (bestOrder != null)
                                ore.Value = bestOrder.Price;
                        }
                    }

                }
                _manager.SaveOreValues();
            }
            loadForm.Dispose();
        }

        private void FilterToMarketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_marketFiltered)
            {
                _marketFiltered = false;
                if (sender is ToolStripMenuItem tsItem) tsItem.Text = "Filter to Market";
                else
                if (sender is KryptonContextMenuItem kBtn) kBtn.Text = "Filter to Market";
                treeView.Nodes.Clear();
                foreach (var group in _manager.Recipes.Values.GroupBy(r => r.ParentGroupName))
                {
                    var groupNode = new TreeNode(group.Key);
                    foreach (var recipe in group)
                    {
                        var recipeNode = new TreeNode(recipe.Name);
                        recipeNode.Tag = recipe;
                        recipe.Node = recipeNode;

                        groupNode.Nodes.Add(recipeNode);
                    }
                    treeView.Nodes.Add(groupNode);
                }
            }
            else
            {
                _marketFiltered = true;
                if (sender is ToolStripMenuItem tsItem) tsItem.Text = "Unfilter Market";
                    else
                if (sender is KryptonContextMenuItem kBtn) kBtn.Text = "Unfilter Market";
                treeView.Nodes.Clear();
                foreach (var group in _manager.Recipes.Values.Where(r => _market.MarketOrders.Values.Any(v => v.ItemType == r.NqId)).GroupBy(r => r.ParentGroupName))
                {
                    var groupNode = new TreeNode(group.Key);
                    foreach (var recipe in group)
                    {
                        var recipeNode = new TreeNode(recipe.Name);
                        recipeNode.Tag = recipe;
                        recipe.Node = recipeNode;

                        groupNode.Nodes.Add(recipeNode);
                    }
                    treeView.Nodes.Add(groupNode);
                }
            }
        }

        private void ExportToSpreadsheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If market filtered, only exports items with market values.
            // Exports the following:
            // Name, Cost To Make, Market Cost, Time To Make, Profit Margin (with formula),
            // Profit Per Day (with formula), Units Per Day with formula
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Price Data " + DateTime.Now.ToString("yyyy-MM-dd"));

                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Cost To Make";
                worksheet.Cell(1, 3).Value = "Market Cost";
                worksheet.Cell(1, 4).Value = "Time To Make";
                worksheet.Cell(1, 5).Value = "Profit Margin";
                worksheet.Cell(1, 6).Value = "Profit Per Day";
                worksheet.Cell(1, 7).Value = "Units Per Day";

                worksheet.Row(1).CellsUsed().Style.Font.SetBold();

                int row = 2;

                var recipes = _manager.Recipes.Values.ToList();
                if (_marketFiltered)
                {
                    recipes = _manager.Recipes.Values
                        .Where(r => _market.MarketOrders.Values.Any(v => v.ItemType == r.NqId)).ToList();
                }

                foreach(var recipe in recipes)
                {
                    worksheet.Cell(row, 1).Value = recipe.Name;
                    var costToMake = _manager.GetTotalCost(recipe.Key, silent: true);
                    worksheet.Cell(row, 2).Value = Math.Round(costToMake,2);

                    var orders = _market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId && o.BuyQuantity < 0 && DateTime.Now < o.ExpirationDate && o.Price > 0);

                    var mostRecentOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
                    var cost = mostRecentOrder?.Price ?? 0d;
                    worksheet.Cell(row, 3).Value = cost;
                    worksheet.Cell(row, 4).Value = recipe.Time;
                    worksheet.Cell(row, 5).FormulaR1C1 = "=((R[0]C[-2]-R[0]C[-3])/R[0]C[-2])";
                    //worksheet.Cell(row, 5).Value = cost = ((mostRecentOrder.Price - costToMake) / mostRecentOrder.Price);
                    //worksheet.Cell(row, 5).FormulaR1C1 = "=IF((R[0]C[-2]<>0);(R[0]C[-2]-R[0]C[-3])/R[0]C[-2];0)";
                    //cost = (mostRecentOrder.Price - costToMake)*(86400/recipe.Time);
                    worksheet.Cell(row, 6).FormulaR1C1 = "=(R[0]C[-3]-R[0]C[-4])*(86400/R[0]C[-2])";
                    worksheet.Cell(row, 7).FormulaR1C1 = "=86400/R[0]C[-3]";
                    row++;
                }

                worksheet.ColumnsUsed().AdjustToContents(1, 50);
                workbook.SaveAs("Item Export " + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx");
                MessageBox.Show("Exported to Item Export " + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx in the same folder as the exe!");
            }
        }

        private void FactoryBreakdownForSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exports an excel sheet with info about how to setup the factory for the selected recipe (aborts if no recipe selected)
            if (!(treeView.SelectedNode?.Tag is SchematicRecipe recipe)) return;
            // Shows the amount of required components, amount per day required, amount per day per industry, and the number of industries you need of that component to provide for 1 of the parent
            // The number of parent parts can be put in as a value
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Factory");
                worksheet.Cell(1, 1).Value = "Number of industries producing " + recipe.Name;
                worksheet.Cell(1, 2).Value = "Produced/day";
                worksheet.Cell(2, 1).Value = 1;
                worksheet.Cell(2, 2).FormulaR1C1 = $"=R[0]C[-1]*(86400/{recipe.Time})";

                worksheet.Cell(1, 3).Value = "Product";
                worksheet.Cell(1, 4).Value = "Required/day";
                worksheet.Cell(1, 5).Value = "Produced/day/industry";
                worksheet.Cell(1, 6).Value = "Num industries required";
                worksheet.Cell(1, 7).Value = "Actual";

                worksheet.Row(1).Style.Font.SetBold();

                int row = 2;
                var ingredients = _manager.GetIngredientRecipes(recipe.Key).OrderByDescending(i => i.Level).GroupBy(i => i.Name);
                if (!ingredients?.Any() == true) return;
                try
                {
                    foreach(var group in ingredients)
                    {
                        var groupSum = group.Sum(g => g.Quantity);
                        worksheet.Cell(row, 3).Value = group.First().Name;
                        worksheet.Cell(row, 4).FormulaA1 = $"=B2*{groupSum}";
                        double outputMult = 1;
                        var talents = _manager.Talents.Where(t => t.InputTalent == false && t.ApplicableRecipes.Contains(group.First().Name));
                        if (talents?.Any() == true)
                            outputMult += talents.Sum(t => t.Multiplier);
                        if (group.First().ParentGroupName != "Ore")
                        {
                            worksheet.Cell(row, 5).Value = (86400 / group.First().Time) * group.First().Products.First().Quantity * outputMult;
                            worksheet.Cell(row, 6).FormulaR1C1 = "=R[0]C[-2]/R[0]C[-1]";
                            worksheet.Cell(row, 7).FormulaR1C1 = "=ROUNDUP(R[0]C[-1])";
                        }
                        row++;
                    }

                    worksheet.ColumnsUsed().AdjustToContents();
                    workbook.SaveAs($"Factory Plan {recipe.Name} {DateTime.Now.ToString("yyyy-MM-dd")}.xlsx");
                    MessageBox.Show($"Exported to 'Factory Plan {recipe.Name} { DateTime.Now.ToString("yyyy-MM-dd")}.xlsx' in the same folder as the exe!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sorry, an error occured during calculation!", "ERROR", MessageBoxButtons.OK);
                    Console.WriteLine(ex);
                }
            }
        }

        private void ConvertLua2JsonFile_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon... ;)");
        }

        private void OnMainformResize(object sender, EventArgs e)
        {
            if (kryptonNavigator1.SelectedPage == null) return;
            _costDetailsLabel = null;
            if (kryptonNavigator1.SelectedPage.Controls.Count > 0 &&
                kryptonNavigator1.SelectedPage.Controls[0] is ContentDocument xDoc)
            {
                _costDetailsPanel = xDoc.CostDetailsPanel;
                if (_costDetailsPanel?.Controls.Count > 0)
                {
                    _costDetailsLabel = _costDetailsPanel.Controls[0] as TextBox;
                }
            }
            if (_costDetailsPanel == null) return;
            _costDetailsPanel.SuspendLayout();
            try
            {
                _costDetailsPanel.AutoSize = false;
                _costDetailsPanel.Height = kryptonNavigator1.SelectedPage.Height - 6;
                _costDetailsPanel.Width = kryptonNavigator1.SelectedPage.Width - 380;
                if (_costDetailsLabel == null) return;
                _costDetailsLabel.AutoSize = false;
                _costDetailsLabel.Width  = _costDetailsPanel.Width  - 8;
                _costDetailsLabel.Height = _costDetailsPanel.Height - 8;
            }
            finally
            {
                _costDetailsPanel.ResumeLayout();
            }
        }

        // Krypton related stuff

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.buttonConvertLua2JsonFile.Visible = false;
            // Setup docking functionality
            var w = kryptonDockingManager.ManageWorkspace(kryptonDockableWorkspace);
            if (w != null)
            {
                kryptonDockingManager.ManageControl(kryptonPage1, w);
            }
            kryptonDockingManager.ManageFloating(this);

            // Do not allow the left-side page to be closed or made auto hidden/docked
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowAutoHidden |
                            KryptonPageFlags.DockingAllowDocked |
                            KryptonPageFlags.DockingAllowClose);
        }

        private static KryptonPage NewPage(string name, Control content)
        {
            var p = new KryptonPage(name)
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Flags = 0
            };
            p.SetFlags(KryptonPageFlags.DockingAllowDocked | KryptonPageFlags.DockingAllowClose);
            if (content != null)
            {
                content.Dock = DockStyle.Fill;
                p.Controls.Add(content);
            }
            return p;
        }

        private ContentDocument NewDocument(string title = null)
        {
            _infoPanel = null;
            _costDetailsPanel = null;
            if (kryptonNavigator1 == null) return null;
            var oldPage = kryptonNavigator1.Pages.FirstOrDefault(x => x.Text == title);
            if (oldPage != null)
            {
                if (oldPage.Controls.Count > 0 && oldPage.Controls[0] is ContentDocument xDoc)
                {
                    _infoPanel = xDoc.InfoPanel;
                    kryptonNavigator1.SelectedPage = oldPage;
                    return xDoc;
                }
            }
            var newDoc = new ContentDocument();
            _infoPanel = newDoc.InfoPanel;
            var page = NewPage(title ?? "Cost", newDoc);
            kryptonNavigator1.Pages.Add(page);
            kryptonNavigator1.SelectedPage = page;
            return newDoc;
        }

        private void RibbonAppButtonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void KryptonDockableWorkspace_WorkspaceCellAdding(object sender, WorkspaceCellEventArgs e)
        {
            e.Cell.Button.CloseButtonAction = CloseButtonAction.RemovePageAndDispose;
            // Remove the context menu from the tabs bar, as it is not relevant to this sample
            e.Cell.Button.ContextButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.NextButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.PreviousButtonDisplay = ButtonDisplay.Hide;
        }

        private void KryptonNavigator1OnSelectedPageChanged(object sender, EventArgs e)
        {
            if(_navUpdating || !(sender is KryptonNavigator nav && nav.SelectedPage != null)) return;
            if (nav.SelectedPage.Controls.Count == 0) return;
            SearchBox.Text = nav.SelectedPage.Text;
            SearchButton_Click(SearchButton, e);
        }

        private void RibbonButtonAboutClick(object sender, EventArgs e)
        {
            var form = new AboutForm();
            form.ShowDialog(this);
        }

    } // Mainform
}
