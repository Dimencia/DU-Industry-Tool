using ClosedXML.Excel;
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
using DocumentFormat.OpenXml.Drawing.ChartDrawing;

// ReSharper disable LocalizableElement

namespace DU_Industry_Tool
{
    public partial class MainForm : KryptonForm
    {
        private IndustryManager Manager;
        private MarketManager Market;
        private bool MarketFiltered = false;
        private FlowLayoutPanel _costDetailsPanel;
        private TextBox _costDetailsLabel;
        //private Label _costDetailsTitleLabel;
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

            treeView.AfterSelect += Treeview_AfterSelect;
            treeView.NodeMouseClick += Treeview_NodeClick;
            treeView.BeginUpdate();
            foreach(var group in manager.Groupnames)
            {
                var groupNode = new TreeNode(group);
                foreach(var recipe in manager._recipes.Where(x => x.Value?.ParentGroupName?.Equals(group, StringComparison.CurrentCultureIgnoreCase) == true)
                            .OrderBy(r => r.Value.Name).Select(x => x.Value))
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
            if (treeView.SelectedNode == e.Node)
            {
                SelectRecipe(sender, e.Node);
            }
        }

        private void Treeview_AfterSelect(object sender, TreeViewEventArgs e)
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
            infoPanel.BorderStyle = BorderStyle.None;// BorderStyle.FixedSingle;

            //if (recipe.ParentGroupName.EndsWith(" Parts", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var containedIn = Manager._recipes.Values.Any(x =>
            //        true == x.Ingredients?.Any(y => y.Name.Equals(recipe.Name, StringComparison.InvariantCultureIgnoreCase)));
            //    if (containedIn)
            //    {
            //        var btn = new Button
            //        {
            //            Left = 4,
            //            Top = 4,
            //            Text = "Part of...",
            //            Height = 24,
            //            Width = 80,
            //            Tag = recipe.Name
            //        };
            //        btn.Click += BtnPartOfClick;
            //        infoPanel.Controls.Add(btn);
            //    }
            //}

            var header = new Label
            {
                AutoSize = true,
                Font = new Font(infoPanel.Font.FontFamily, 12, FontStyle.Bold),
                Padding = new Padding(4, 10, 4, 10),
                Text = $"{recipe.Name} (T{recipe.Level})",
                TextAlign = ContentAlignment.MiddleCenter
            };
            infoPanel.Controls.Add(header);

            var costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            Manager.ProductQuantity = int.Parse(QuantityBox.Text);
            if (!double.TryParse(QuantityBox.Text, out var cnt)) cnt = 1d;
            var costToMake = Manager.GetTotalCost(recipe.Key, cnt, silent: false);
            costPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Text = "Cost To Make " + costToMake.ToString("N02") + "q"
            });
            infoPanel.Controls.Add(costPanel);

            costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            var cost = Manager.GetBaseCost(recipe.Key);
            costPanel.Controls.Add(new Label
            {
                Text = "Untalented (without schematics) " + cost.ToString("N02") + "q",
                AutoSize = true
            });

            // IDK why sometimes prices are listed as 0
            var orders = Market.MarketOrders.Values.Where(o => o.ItemType == recipe.NqId &&
                                                               o.BuyQuantity < 0 &&
                                                               DateTime.Now < o.ExpirationDate &&
                                                               o.Price > 0);
            var mostRecentOrder = orders.OrderBy(o => o.Price).FirstOrDefault();
            if (mostRecentOrder == null)
            {
                infoPanel.Controls.Add(costPanel);
            }
            else
            {
                costPanel.Controls.Add(new Label
                {
                    Text = "Market " + mostRecentOrder.Price.ToString("N02") + "q",
                    AutoSize = true
                });
                costPanel.Controls.Add(new Label
                {
                    Text = "Until " + mostRecentOrder.ExpirationDate
                });
                infoPanel.Controls.Add(costPanel);

                var costPanelm = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                costPanelm.Controls.Add(new Label
                {
                    Text = "Profit Margin ",
                    AutoSize = true
                });
                cost = ((mostRecentOrder.Price-costToMake)/mostRecentOrder.Price);
                costPanelm.Controls.Add(new Label
                {
                    Text = cost.ToString("0%"),
                    AutoSize = true
                });
                infoPanel.Controls.Add(costPanelm);

                cost = (mostRecentOrder.Price - costToMake)*(86400/recipe.Time);
                costPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                costPanel.Controls.Add(new Label
                {
                    Text = "Profit/Day/Industry " + cost.ToString("N02") + "q",
                    AutoSize = true
                });
                infoPanel.Controls.Add(costPanel);
            }

            var costPanel2 = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                Size = new Size(400, 40)
            };

            cost = recipe.Time > 0 ? 86400/recipe.Time : 0;
            costPanel2.Controls.Add(new Label
            {
                Text = "Per Industry " + cost.ToString("0.0") + "/Day",
                AutoSize = true
            });
            infoPanel.Controls.Add(costPanel2);

            // ----- Ingredients -----
            costPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            infoPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Text = "Ingredients",
                Font = new Font(costPanel.Font, FontStyle.Bold)
            });

            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = recipe.Ingredients.Count,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 10)
            };
            foreach (var ingredient in recipe.Ingredients)
            {
                var label = new Label
                {
                    AutoSize = true,
                    Font = new Font(infoPanel.Font, FontStyle.Underline),
                    Text = ingredient.Name,
                    ForeColor = Color.CornflowerBlue,
                    Tag = ingredient.Type
                };
                label.Click += Label_Click;
                grid.Controls.Add(label);
                grid.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = ingredient.Quantity.ToString("0.0")
                });
            }
            infoPanel.Controls.Add(grid);

            // ----- Products -----
            var prodPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            var prodLabel = new Label
            {
                AutoSize = true,
                Text = "Products"
            };
            prodLabel.Font = new Font(prodLabel.Font, FontStyle.Bold);
            prodPanel.Controls.Add(prodLabel);
            infoPanel.Controls.Add(prodPanel);

            grid = new TableLayoutPanel();
            grid.ColumnCount = 2;
            grid.RowCount = recipe.Products.Count;
            grid.AutoSize = true;
            grid.Padding = new Padding(0, 0, 0, 10);
            foreach (var ingredient in recipe.Products)
            {
                grid.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = ingredient.Name
                });
                grid.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = ingredient.Quantity.ToString("0.0")
                });
            }
            infoPanel.Controls.Add(grid);

            if (recipe.ParentGroupName.EndsWith(" Parts", StringComparison.InvariantCultureIgnoreCase))
            {
                var containedIn = Manager._recipes.Values.Where(x =>
                    true == x.Ingredients?.Any(y => y.Name.Equals(recipe.Name, StringComparison.InvariantCultureIgnoreCase)));
                if (containedIn?.Any() == true)
                {
                    prodPanel = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.TopDown,
                        AutoSize = true
                    };
                    prodPanel.Controls.Add(new Label
                    {
                        AutoSize = true,
                        Font = new Font(infoPanel.Font, FontStyle.Bold),
                        Text = "Part of recipes:"
                    });
                    grid = new TableLayoutPanel
                    {
                        AutoScroll = true,
                        AutoSize = false,
                        ColumnCount = 1,
                        RowCount = containedIn.Count(),
                        Padding = new Padding(0, 0, 0, 10),
                        Width = 400,
                        Height = 400,
                        VerticalScroll = { Visible = true }
                    };
                    foreach (var master in containedIn)
                    {
                        var label = new Label
                        {
                            AutoSize = true,
                            Font = new Font(infoPanel.Font, FontStyle.Underline),
                            Text = master.Name,
                            ForeColor = Color.CornflowerBlue,
                            Tag = master.Key
                        };
                        label.Click += Label_Click;
                        grid.Controls.Add(label);
                    }
                    prodPanel.Controls.Add(grid);
                    infoPanel.Controls.Add(prodPanel);
                }
            }

            _costDetailsPanel = new FlowLayoutPanel();
            _costDetailsPanel.SuspendLayout();
            try
            {
                _costDetailsPanel.FlowDirection = FlowDirection.TopDown;
                _costDetailsPanel.BorderStyle = BorderStyle.None;// FixedSingle;
                _costDetailsPanel.Dock = DockStyle.None;
                _costDetailsPanel.AutoSize = true;
                _costDetailsPanel.AutoScroll = true;
                _costDetailsPanel.Font = new Font("Lucida Console", 10F, FontStyle.Regular, GraphicsUnit.Point, (0));
                _costDetailsPanel.Size = new Size(600, 500);
                _costDetailsLabel = new TextBox
                {
                    AutoSize = true,
                    ScrollBars = ScrollBars.Both,
                    Size = new Size(600, 500),
                    Text = Manager.CostResults.ToString(),
                    Multiline = true,
                    WordWrap = false,
                    ReadOnly = true
                };
                _costDetailsPanel.Controls.Add(_costDetailsLabel);
                infoPanel.Controls.Add(_costDetailsPanel);
                _costDetailsLineCount = _costDetailsLabel.Text.Count(c => c == '\n');
            }
            finally
            {
                _costDetailsPanel.ResumeLayout();
            }

            infoPanel.AutoScroll = false;
            OnMainformResize(null, null);
        }

        private void BtnPartOfClick(object sender, EventArgs e)
        {
            var recipeName = (string)(sender as Button).Tag;
            var containedIn = Manager._recipes.Where(x =>
                x.Value?.Ingredients != null &&
                x.Value.Ingredients.Any(y => y.Name.Equals(recipeName, StringComparison.InvariantCultureIgnoreCase)));
            if (containedIn?.Any() != true)
                return;
            var msg = containedIn.ToList().Aggregate("", (current, kvp) => current + ("- "+kvp.Value.Name + "\r\n"));
            MessageBox.Show(recipeName+" is ingredient in the following recipes:\r\n"+msg, "Ingredient");
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

        private void InputOreValuesToolStripMenuItem_Click(object sender, EventArgs e)
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
            treeView.Height = kryptonPage1.Height - treeView.Top;
            treeView.Width = kryptonPage1.Width-4;

            if (kryptonNavigator1.SelectedPage == null) return;

            if (kryptonNavigator1.SelectedPage.Controls.Count > 0 &&
                kryptonNavigator1.SelectedPage.Controls[0] is ContentDocument xDoc)
            {
                _costDetailsPanel = xDoc.InfoPanel;
            }
            if (_costDetailsPanel == null) return;
            _costDetailsPanel.SuspendLayout();
            try
            {
                _costDetailsPanel.Height = Math.Max(500, kryptonNavigator1.SelectedPage.Height - 10);
                _costDetailsPanel.Width = kryptonNavigator1.SelectedPage.Width - 4;
                _costDetailsLabel.Width = kryptonNavigator1.SelectedPage.Width - 430;
                _costDetailsLabel.Height = _costDetailsPanel.Height - 10;
                //_costDetailsTitleLabel.Width = _costDetailsPanel.Width - 10;
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

        private static KryptonPage NewPage(string name, int image, Control content)
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
            if (kryptonNavigator1 == null) return null;
            var oldPage = kryptonNavigator1.Pages.FirstOrDefault(x => x.Text == title);
            if (oldPage != null)
            {
                kryptonNavigator1.SelectedPage = oldPage;
                if (oldPage.Controls.Count > 0 && oldPage.Controls[0] is ContentDocument xDoc)
                {
                    infoPanel = xDoc.InfoPanel;
                }
                return oldPage;
            }
            var tmp = new ContentDocument();
            //tmp.Resize += OnMainformResize;
            infoPanel = tmp.InfoPanel;
            var page = NewPage(title ?? "Cost", 0, tmp);
            kryptonNavigator1.Pages.Add(page);
            kryptonNavigator1.SelectedPage = page;
            return page;
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

    } // Mainform
}
