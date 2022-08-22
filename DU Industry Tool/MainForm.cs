﻿using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Workspace;
using ComponentFactory.Krypton.Docking;

namespace DU_Industry_Tool
{
    public partial class MainForm : KryptonForm
    {
        private IndustryManager Manager;
        private MarketManager Market;
        private bool MarketFiltered = false;
        private FlowLayoutPanel _costDetailsPanel;
        private TextBox _costDetailsLabel;
        private Label _costDetailsTitleLabel;
        private int _costDetailsLineCount = 0;
        private List<string> _breadcrumbs = new List<string>();
        private FlowLayoutPanel infoPanel;

        public MainForm(IndustryManager manager)
        {
            InitializeComponent();

            CultureInfo.CurrentCulture = new CultureInfo("en-us");
            QuantityBox.SelectedIndex = 0;

            // Setup the trees. One recipe on each main node
            Manager = manager;

            Market = new MarketManager();
            kryptonPage1.Flags = 0;
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowDocked);
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowClose);

            treeView.AfterSelect += treeview_AfterSelect;
            treeView.NodeMouseClick += treeview_NodeClick;
            treeView.BeginUpdate();
            foreach(var group in manager.Groupnames)
            {
                var groupNode = new TreeNode(group);
                foreach(var recipe in manager._recipes.Where(x => x.Value.ParentGroupName.Equals(group, StringComparison.CurrentCultureIgnoreCase)).
                            OrderBy(r => r.Value.Name).Select(x => x.Value))
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

        private void treeview_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView.SelectedNode == e.Node)
            {
                SelectRecipe(sender, e.Node);
            }
        }

        private void treeview_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectRecipe(sender, e.Node);
        }

        private void SelectRecipe(object sender, TreeNode e)
        {
            if (!(e.Tag is SchematicRecipe recipe))
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
            if (NewDocument(recipe.Name) == null)
                return;
            if (infoPanel == null) return;
            infoPanel.Controls.Clear();

            var header = new Label
            {
                Text = $"{recipe.Name} (T{recipe.Level})",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Padding = new Padding(header.Padding.Left, header.Padding.Top, header.Padding.Right, 20);
            header.Font = new Font(header.Font.FontFamily, 12, FontStyle.Bold);
            infoPanel.Controls.Add(header);

            var costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            Manager.ProductQuantity = int.Parse(QuantityBox.Text);
            if (!double.TryParse(QuantityBox.Text, out var cnt)) cnt = 1d;
            var costToMake = Manager.GetTotalCost(recipe.Key, cnt, silent: false);
            var totalCostLabel = new Label
            {
                Text = "Cost To Make " + costToMake.ToString("N02") + "q",
                AutoSize = true
            };
            costPanel.Controls.Add(totalCostLabel);
            infoPanel.Controls.Add(costPanel);

            costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            var cost = Manager.GetBaseCost(recipe.Key);
            totalCostLabel = new Label
            {
                Text = "Untalented (without schematics) " + cost.ToString("N02") + "q",
                AutoSize = true
            };
            costPanel.Controls.Add(totalCostLabel);
            infoPanel.Controls.Add(costPanel);
            costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };

            // IDK why sometimes prices are listed as 0
            var orders = Market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId &&
                                                               o.BuyQuantity < 0 &&
                                                               DateTime.Now < o.ExpirationDate &&
                                                               o.Price > 0);
            var mostRecentOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
            cost = mostRecentOrder?.Price ?? 0;

            totalCostLabel = new Label
            {
                Text = "Market " + cost.ToString("N02") + "q",
                AutoSize = true
            };
            costPanel.Controls.Add(totalCostLabel);

            if (mostRecentOrder == null)
            {
                infoPanel.Controls.Add(costPanel);
            }
            else
            {
                var costLabela = new Label
                {
                    Text = "Until " + mostRecentOrder.ExpirationDate ?? "No expiration"
                };
                costPanel.Controls.Add(costLabela);
                infoPanel.Controls.Add(costPanel);

                var costPanelm = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                var costLabelb = new Label
                {
                    Text = "Profit Margin ",
                    AutoSize = true
                };
                costPanelm.Controls.Add(costLabelb);
                cost = ((mostRecentOrder.Price-costToMake)/mostRecentOrder.Price);
                var totalCostLabelm = new Label
                {
                    Text = cost.ToString("0%"),
                    AutoSize = true
                };
                costPanelm.Controls.Add(totalCostLabelm);
                infoPanel.Controls.Add(costPanelm);

                costPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                cost = (mostRecentOrder.Price - costToMake)*(86400/recipe.Time);
                totalCostLabel = new Label
                {
                    Text = "Profit/Day/Industry " + cost.ToString("N02") + "q",
                    AutoSize = true
                };
                costPanel.Controls.Add(totalCostLabel);
                infoPanel.Controls.Add(costPanel);
            }

            var costPanel2 = new FlowLayoutPanel();
            costPanel2.FlowDirection = FlowDirection.LeftToRight;
            costPanel2.AutoSize = false;
            costPanel2.Size = new System.Drawing.Size(400, 40);

            cost = 86400/recipe.Time;
            var totalCostLabel2 = new Label
            {
                Text = "Per Industry " + cost.ToString("0.0") + "/Day",
                AutoSize = true
            };
            costPanel2.Controls.Add(totalCostLabel2);
            infoPanel.Controls.Add(costPanel2);

            // ----- Ingredients -----
            costPanel = new FlowLayoutPanel();
            costPanel.FlowDirection = FlowDirection.TopDown;
            costPanel.AutoSize = true;
            var ingredientsLabel = new Label();
            ingredientsLabel.AutoSize = true;
            ingredientsLabel.Text = "Ingredients";
            ingredientsLabel.Font = new Font(ingredientsLabel.Font, FontStyle.Bold);
            infoPanel.Controls.Add(ingredientsLabel);

            var grid = new TableLayoutPanel();
            grid.ColumnCount = 2;
            grid.RowCount = recipe.Ingredients.Count;
            grid.AutoSize = true;
            grid.Padding = new Padding(0, 0, 0, 10);
            for (var i = 0; i < recipe.Ingredients.Count; i++)
            {
                var ingredient = recipe.Ingredients[i];
                var label = new Label
                {
                    AutoSize = true,
                    Text = ingredient.Name,
                    ForeColor = Color.CornflowerBlue,
                    Tag = ingredient.Type
                };
                label.Font = new Font(label.Font, FontStyle.Underline);
                label.Click += Label_Click;

                grid.Controls.Add(label);
                label = new Label
                {
                    AutoSize = true,
                    Text = ingredient.Quantity.ToString("0.0")
                };
                grid.Controls.Add(label);
            }
            infoPanel.Controls.Add(grid);

            // ----- Products -----
            var prodPanel = new FlowLayoutPanel();
            prodPanel.FlowDirection = FlowDirection.TopDown;
            prodPanel.AutoSize = true;
            var prodLabel = new Label();
            prodLabel.AutoSize = true;
            prodLabel.Text = "Products";
            prodLabel.Font = new Font(prodLabel.Font, FontStyle.Bold);
            prodPanel.Controls.Add(prodLabel);
            infoPanel.Controls.Add(prodPanel);

            grid = new TableLayoutPanel();
            grid.ColumnCount = 2;
            grid.RowCount = recipe.Products.Count;
            grid.AutoSize = true;
            grid.Padding = new Padding(0, 0, 0, 10);
            for (var i = 0; i < recipe.Products.Count; i++)
            {
                var ingredient = recipe.Products[i];
                var label = new Label();
                label.AutoSize = true;
                label.Text = ingredient.Name;
                grid.Controls.Add(label);
                label = new Label();
                label.AutoSize = true;
                label.Text = ingredient.Quantity.ToString("0.0");
                grid.Controls.Add(label);
            }
            infoPanel.Controls.Add(grid);
            _costDetailsTitleLabel = new Label();
            _costDetailsTitleLabel.AutoSize = true;
            _costDetailsTitleLabel.Text = "Cost Details";
            _costDetailsTitleLabel.Font = new Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _costDetailsTitleLabel.Size = new System.Drawing.Size(kryptonPage2.Width, 24);
            infoPanel.Controls.Add(_costDetailsTitleLabel);

            _costDetailsPanel = new FlowLayoutPanel();
            try
            {
                _costDetailsPanel.SuspendLayout();
                _costDetailsPanel.FlowDirection = FlowDirection.TopDown;
                _costDetailsPanel.Dock = DockStyle.None;
                _costDetailsPanel.Size = new System.Drawing.Size(400, 400);
                _costDetailsPanel.AutoSize = false;
                _costDetailsPanel.AutoScroll = true;
                _costDetailsPanel.Font = new Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular,
                    System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                _costDetailsLabel = new TextBox();
                _costDetailsLabel.AutoSize = false;
                _costDetailsLabel.Size = new System.Drawing.Size(kryptonPage2.Width, kryptonPage2.Height-10);
                _costDetailsLabel.Text = Manager.CostResults.ToString();
                _costDetailsLabel.Multiline = true;
                _costDetailsLabel.WordWrap = false;

                _costDetailsLabel.ReadOnly = true;
                _costDetailsPanel.Controls.Add(_costDetailsLabel);
                infoPanel.Controls.Add(_costDetailsPanel);
                _costDetailsLineCount = _costDetailsLabel.Text.Count(c => c == '\n');
            }
            finally
            {
                _costDetailsPanel.ResumeLayout();
            }

            infoPanel.AutoScroll = true;
            OnMainformResize(null, null);
        }

        private void Label_Click(object sender, EventArgs e)
        {
            var label = sender as Label;
            if (!Manager._recipes.ContainsKey(label.Tag as string))
            {
                return;
            }
            var recipe = Manager._recipes[label.Tag as string];

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
                    if (innerNode.Text.Equals(recipe.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        targetNode = innerNode;
                        break;
                    }
                }
            }
            if (targetNode == null) return;
            treeView.SelectedNode = targetNode;
            treeView.SelectedNode.EnsureVisible();
            SearchBox.Text = targetNode.Text;
        }

        private void inputOreValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new OreValueForm(Manager);
            form.ShowDialog(this);
        }

        private void SkillLevelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new SkillForm(Manager);
            form.ShowDialog(this);
        }

        private void SchematicValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new SchematicValueForm(Manager);
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
            treeView.CollapseAll();
            foreach (var outerNode in outerNodes)
            {
                foreach (var innerNode in outerNode.Nodes.OfType<TreeNode>())
                {
                    if (!innerNode.Text.ToLower().Contains(searchValue.ToLower()))
                        continue;
                    innerNode.EnsureVisible();
                    if (firstResult == null)
                    {
                        firstResult = innerNode;
                    }
                }
            }

            if (firstResult != null)
            {
                treeView.SelectedNode = firstResult;
                treeView.SelectedNode.EnsureVisible();
            }
            treeView.EndUpdate();
            treeView.Focus();
        }

        private void UpdateMarketValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var loadForm = new LoadingForm(Market);
            loadForm.ShowDialog(this);
            if (loadForm.DiscardOres)
            {
                // Get rid of them
                List<ulong> toRemove = new List<ulong>();
                foreach(var order in Market.MarketOrders)
                {
                    var recipe = Manager._recipes.Values.Where(r => r.NqId == order.Value.ItemType).FirstOrDefault();
                    if (recipe != null && recipe.ParentGroupName == "Ore")
                        toRemove.Add(order.Key);

                }
                foreach (var key in toRemove)
                    Market.MarketOrders.Remove(key);
                Market.SaveData();
            }
            else
            {
                // Process them and leave them so they show in exports
                foreach (var order in Market.MarketOrders)
                {
                    var recipe = Manager._recipes.Values.Where(r => r.NqId == order.Value.ItemType).FirstOrDefault();
                    if (recipe != null && recipe.ParentGroupName == "Ore")
                    {
                        var ore = Manager.Ores.Where(o => o.Key.ToLower() == recipe.Key.ToLower()).FirstOrDefault();
                        if (ore != null)
                        {
                            var orders = Market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId && o.BuyQuantity < 0 && DateTime.Now < o.ExpirationDate && o.Price > 0);

                            var bestOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
                            if (bestOrder != null)
                                ore.Value = bestOrder.Price;
                        }
                    }

                }
                Manager.SaveOreValues();
            }
            loadForm.Dispose();
        }

        private void FilterToMarketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MarketFiltered)
            {
                MarketFiltered = false;
                if (sender is ToolStripMenuItem tsItem) tsItem.Text = "Filter to Market";
                else
                if (sender is KryptonContextMenuItem kBtn) kBtn.Text = "Filter to Market";
                treeView.Nodes.Clear();
                foreach (var group in Manager._recipes.Values.GroupBy(r => r.ParentGroupName))
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
                MarketFiltered = true;
                if (sender is ToolStripMenuItem tsItem) tsItem.Text = "Unfilter Market";
                    else
                if (sender is KryptonContextMenuItem kBtn) kBtn.Text = "Unfilter Market";
                treeView.Nodes.Clear();
                foreach (var group in Manager._recipes.Values.Where(r => Market.MarketOrders.Values.Any(v => v.ItemType == r.NqId)).GroupBy(r => r.ParentGroupName))
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

        private void exportToSpreadsheetToolStripMenuItem_Click(object sender, EventArgs e)
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

                var recipes = Manager._recipes.Values.ToList();
                if (MarketFiltered)
                {
                    recipes = Manager._recipes.Values
                        .Where(r => Market.MarketOrders.Values.Any(v => v.ItemType == r.NqId)).ToList();
                }

                foreach(var recipe in recipes)
                {
                    worksheet.Cell(row, 1).Value = recipe.Name;
                    var costToMake = Manager.GetTotalCost(recipe.Key, silent: true);
                    worksheet.Cell(row, 2).Value = Math.Round(costToMake,2);

                    var orders = Market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId && o.BuyQuantity < 0 && DateTime.Now < o.ExpirationDate && o.Price > 0);

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

        private void factoryBreakdownForSelectedToolStripMenuItem_Click(object sender, EventArgs e)
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
                var ingredients = Manager.GetIngredientRecipes(recipe.Key).OrderByDescending(i => i.Level).GroupBy(i => i.Name);
                if (!ingredients?.Any() == true) return;
                foreach(var group in ingredients)
                {
                    worksheet.Cell(row, 3).Value = group.First().Name;
                    worksheet.Cell(row, 4).FormulaA1 = $"=B2*{group.Sum(g => g.Quantity)}";
                    double outputMult = 1;
                    var talents = Manager.Talents.Where(t => t.InputTalent == false && t.ApplicableRecipes.Contains(group.First().Name));
                    if (talents?.Any() == true)
                        outputMult += talents.Sum(t => t.Multiplier);
                    if (group.First().ParentGroupName != "Ore")
                        worksheet.Cell(row, 5).Value = (86400 / group.First().Time)*group.First().Products.First().Quantity*outputMult;
                    worksheet.Cell(row, 6).FormulaR1C1 = "=R[0]C[-2]/R[0]C[-1]";
                    worksheet.Cell(row, 7).FormulaR1C1 = "=ROUNDUP(R[0]C[-1])";

                    row++;
                }

                worksheet.ColumnsUsed().AdjustToContents();
                workbook.SaveAs($"Factory Plan {recipe.Name} {DateTime.Now.ToString("yyyy-MM-dd")}.xlsx");
                MessageBox.Show($"Exported to 'Factory Plan {recipe.Name} { DateTime.Now.ToString("yyyy-MM-dd")}.xlsx' in the same folder as the exe!");
            }
        }

        private void OnMainformResize(object sender, EventArgs e)
        {
            //leftPanel.Height = this.ClientSize.Height-46;
            treeView.Height = kryptonPage1.Height-treeView.Top-10;
            treeView.Width = kryptonPage1.Width-4;
            if (_costDetailsPanel == null) return;
            _costDetailsPanel.SuspendLayout();
            try
            {
                _costDetailsPanel.Height = kryptonPage1.Height - _costDetailsTitleLabel.Top - 30;
                _costDetailsPanel.Width = kryptonPage2.Width - 40;
                _costDetailsTitleLabel.Width = _costDetailsPanel.Width - 4;
                _costDetailsLabel.Width = _costDetailsPanel.Width - 4;
                _costDetailsLabel.Height = _costDetailsLineCount * (_costDetailsTitleLabel.Height+1);
            }
            finally
            {
                _costDetailsPanel.ResumeLayout();
            }
        }

        // Krypton related stuff

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Setup docking functionality
            var w = kryptonDockingManager.ManageWorkspace(kryptonDockableWorkspace);
            kryptonDockingManager.ManageControl(kryptonPage1, w);
            kryptonDockingManager.ManageFloating(this);

            // Do not allow the left-side page to be closed or made auto hidden/docked
            kryptonPage1.ClearFlags(KryptonPageFlags.DockingAllowAutoHidden |
                            KryptonPageFlags.DockingAllowDocked |
                            KryptonPageFlags.DockingAllowClose);
        }

        private KryptonPage NewPage(string name, int image, Control content)
        {
            var p = new KryptonPage(name)
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Flags = 0
            };
            p.SetFlags(KryptonPageFlags.DockingAllowDocked | KryptonPageFlags.DockingAllowClose);
            content.Dock = DockStyle.Fill;
            p.Controls.Add(content);
            return p;
        }

        private KryptonPage NewDocument(string title = null)
        {
            infoPanel = null;
            if (kryptonWorkspaceCell2 == null) return null;
            var oldPage = kryptonWorkspaceCell2.Pages.FirstOrDefault(x => x.Text == title);
            if (oldPage != null)
            {
                kryptonWorkspaceCell2.SelectedPage = oldPage;
                if (oldPage.Controls.Count > 0 && oldPage.Controls[0] is ContentDocument xDoc)
                {
                    infoPanel = xDoc.InfoPanel;
                }
                return oldPage;
            }
            var tmp = new ContentDocument();
            infoPanel = tmp.InfoPanel;
            var page = NewPage(title ?? "Cost", 0, tmp);
            kryptonWorkspaceCell2.Pages.Add(page);
            kryptonWorkspaceCell2.SelectedPage = page;
            return page;
        }

        private void kryptonWorkspaceCell2_OnCellShowContextMenu(object sender, ShowContextMenuArgs e)
        {
            e.Cancel = true;
        }

        private void ribbonAppButtonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void kryptonDockableWorkspace_WorkspaceCellAdding(object sender, WorkspaceCellEventArgs e)
        {
            e.Cell.Button.CloseButtonAction = CloseButtonAction.RemovePageAndDispose;
            // Remove the context menu from the tabs bar, as it is not relevant to this sample
            e.Cell.Button.ContextButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.NextButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.PreviousButtonDisplay = ButtonDisplay.Hide;
        }
    } // Mainform
}
