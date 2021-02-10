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
    public partial class MainForm : Form
    {

        private IndustryManager Manager;
        public MainForm(IndustryManager manager)
        {
            InitializeComponent();
            // Setup the trees.  One recipe on each main node
            Manager = manager;
            treeView.AfterSelect += TreeView_AfterSelect;

            // TODO: Freeze updates first
            foreach(var group in manager._recipes.Values.GroupBy(r => r.ParentGroupName))
            {
                var groupNode = new TreeNode(group.Key);
                
                
                foreach(var recipe in group)
                {
                    var recipeNode = new TreeNode(recipe.Name);
                    recipeNode.Tag = recipe;
                    recipe.Node = recipeNode;
                    var inputsNode = new TreeNode("Inputs");
                    var outputsNode = new TreeNode("Outputs");

                    foreach(var ingredient in recipe.Ingredients)
                    {
                        var ingredientNode = new TreeNode(ingredient.Name + " : " + ingredient.Quantity);
                        ingredientNode.Tag = manager._recipes[ingredient.Type];
                        inputsNode.Nodes.Add(ingredientNode);
                    }
                    foreach (var product in recipe.Products)
                    {
                        var productNode = new TreeNode(product.Name + " : " + product.Quantity);
                        productNode.Tag = manager._recipes[product.Type];
                        outputsNode.Nodes.Add(productNode);
                    }
                    recipeNode.Nodes.Add(inputsNode);
                    recipeNode.Nodes.Add(outputsNode);

                    groupNode.Nodes.Add(recipeNode);
                }

                treeView.Nodes.Add(groupNode);
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var recipe = e.Node.Tag as SchematicRecipe;
            if (recipe != null)
            {
                // Display recipe info for the thing they have selected

                infoPanel.Controls.Clear();

                Label header = new Label();
                header.Text = recipe.Name;
                header.Font = new Font(header.Font.FontFamily, 12, FontStyle.Bold);
                header.AutoSize = true;
                header.Padding = new Padding(header.Padding.Left, header.Padding.Top, header.Padding.Right, 20);
                header.TextAlign = ContentAlignment.MiddleCenter;

                Console.WriteLine(recipe.Name);

                infoPanel.Controls.Add(header);

                FlowLayoutPanel costPanel = new FlowLayoutPanel();
                costPanel.FlowDirection = FlowDirection.LeftToRight;
                costPanel.AutoSize = true;
                var costLabel = new Label();
                costLabel.Text = "Cost To Make: ";
                costLabel.AutoSize = true;
                costLabel.Padding = new Padding(0, 0, 0, 10);
                costPanel.Controls.Add(costLabel);
                var totalCostLabel = new Label();
                var cost = Manager.GetTotalCost(recipe.Key);
                totalCostLabel.Text = cost.ToString("0.0");
                totalCostLabel.AutoSize = true;
                costPanel.Controls.Add(totalCostLabel);

                infoPanel.Controls.Add(costPanel);

                var ingredientsLabel = new Label();
                ingredientsLabel.Text = "Ingredients";
                ingredientsLabel.Font = new Font(ingredientsLabel.Font, FontStyle.Bold);
                infoPanel.Controls.Add(ingredientsLabel);


                var grid = new TableLayoutPanel();
                grid.ColumnCount = 2;
                grid.RowCount = recipe.Ingredients.Count;
                grid.AutoSize = true;
                grid.Padding = new Padding(0, 0, 0, 20);

                for(int i = 0; i < recipe.Ingredients.Count; i++)
                {
                    var ingredient = recipe.Ingredients[i];
                    var label = new Label();
                    label.AutoSize = true;
                    label.Text = ingredient.Name;
                    label.ForeColor = Color.CornflowerBlue;
                    label.Font = new Font(label.Font, FontStyle.Underline);
                    label.Tag = ingredient.Type;

                    label.Click += Label_Click;


                    grid.Controls.Add(label);
                    label = new Label();
                    label.AutoSize = true;
                    label.Text = ingredient.Quantity.ToString("0.0");
                    grid.Controls.Add(label);
                }
                infoPanel.Controls.Add(grid);

                ingredientsLabel = new Label();
                ingredientsLabel.Text = "Products";
                ingredientsLabel.Font = new Font(ingredientsLabel.Font, FontStyle.Bold);
                infoPanel.Controls.Add(ingredientsLabel);


                grid = new TableLayoutPanel();
                grid.ColumnCount = 2;
                grid.RowCount = recipe.Products.Count;
                grid.AutoSize = true;

                for (int i = 0; i < recipe.Products.Count; i++)
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
                infoPanel.WrapContents = false;
                infoPanel.AutoScroll = true;
                
            }
        }

        private void Label_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;
            var recipe = Manager._recipes[label.Tag as string];
            var outerNodes = treeView.Nodes.OfType<TreeNode>();
            TreeNode targetNode = null;
            foreach(var outerNode in outerNodes)
            {
                foreach(var innerNode in outerNode.Nodes.OfType<TreeNode>())
                {
                    if (innerNode.Text == recipe.Name) // Yes we have tags and keys but, this is easiest since tags are on input/output ones too
                    {
                        targetNode = innerNode;
                        break;
                    }
                }
            }

            if (targetNode != null)
            {
                treeView.SelectedNode = targetNode;
                treeView.SelectedNode.EnsureVisible();
            }
        }

        private void inputOreValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var oreForm = new OreValueForm(Manager);
            oreForm.ShowDialog(this);
        }

        private void skillLevelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var talentForm = new SkillForm(Manager);
            talentForm.ShowDialog(this);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string searchValue = SearchBox.Text;
            var outerNodes = treeView.Nodes.OfType<TreeNode>();
            TreeNode firstResult = null;
            treeView.CollapseAll();
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                return; // Do nothing
            }
            foreach (var outerNode in outerNodes)
            {
                foreach (var innerNode in outerNode.Nodes.OfType<TreeNode>())
                {
                    if (innerNode.Text.ToLower().Contains(searchValue.ToLower())) // Yes we have tags and keys but, this is easiest since tags are on input/output ones too
                    {
                        innerNode.EnsureVisible();
                        if (firstResult == null)
                            firstResult = innerNode;
                    }
                }
            }

            if (firstResult != null)
            {
                treeView.SelectedNode = firstResult;
                treeView.SelectedNode.EnsureVisible();
            }
            treeView.Focus();
        }
    }
}
