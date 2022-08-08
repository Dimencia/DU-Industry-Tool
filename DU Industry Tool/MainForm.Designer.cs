using System.Windows.Forms;

namespace DU_Industry_Tool
{
    partial class MainForm
    {
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.SplitContainer leftPanel;
        private System.Windows.Forms.FlowLayoutPanel infoPanel;
        private System.Windows.Forms.FlowLayoutPanel searchPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inputOreValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem schematicValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem skillLevelsToolStripMenuItem;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.ComboBox QuantityBox;
        private System.Windows.Forms.ToolStripMenuItem updateMarketValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToMarketToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToSpreadsheetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem factoryBreakdownForSelectedToolStripMenuItem;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView = new System.Windows.Forms.TreeView();
            this.infoPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.leftPanel = new System.Windows.Forms.SplitContainer();
            this.searchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inputOreValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.schematicValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.skillLevelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateMarketValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToMarketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToSpreadsheetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.factoryBreakdownForSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.QuantityBox = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = DockStyle.Top;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(969, 28);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inputOreValuesToolStripMenuItem,
            this.schematicValuesToolStripMenuItem,
            this.skillLevelsToolStripMenuItem,
            this.updateMarketValuesToolStripMenuItem,
            this.filterToMarketToolStripMenuItem,
            this.exportToSpreadsheetToolStripMenuItem,
            this.factoryBreakdownForSelectedToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // inputOreValuesToolStripMenuItem
            // 
            this.inputOreValuesToolStripMenuItem.Name = "inputOreValuesToolStripMenuItem";
            this.inputOreValuesToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.inputOreValuesToolStripMenuItem.Text = "Ore Values";
            this.inputOreValuesToolStripMenuItem.Click += new System.EventHandler(this.inputOreValuesToolStripMenuItem_Click);
            // 
            // skillLevelsToolStripMenuItem
            // 
            this.skillLevelsToolStripMenuItem.Name = "skillLevelsToolStripMenuItem";
            this.skillLevelsToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.skillLevelsToolStripMenuItem.Text = "Talents";
            this.skillLevelsToolStripMenuItem.Click += new System.EventHandler(this.SkillLevelsToolStripMenuItem_Click);
            // 
            // schematicValuesToolStripMenuItem
            // 
            this.schematicValuesToolStripMenuItem.Name = "schematicValuesToolStripMenuItem";
            this.schematicValuesToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.schematicValuesToolStripMenuItem.Text = "Schematics";
            this.schematicValuesToolStripMenuItem.Click += new System.EventHandler(this.SchematicValuesToolStripMenuItem_Click);
            // 
            // updateMarketValuesToolStripMenuItem
            // 
            this.updateMarketValuesToolStripMenuItem.Name = "updateMarketValuesToolStripMenuItem";
            this.updateMarketValuesToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.updateMarketValuesToolStripMenuItem.Text = "Update Market Values";
            this.updateMarketValuesToolStripMenuItem.Click += new System.EventHandler(this.UpdateMarketValuesToolStripMenuItem_Click);
            // 
            // filterToMarketToolStripMenuItem
            // 
            this.filterToMarketToolStripMenuItem.Name = "filterToMarketToolStripMenuItem";
            this.filterToMarketToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.filterToMarketToolStripMenuItem.Text = "Filter to Market";
            this.filterToMarketToolStripMenuItem.Click += new System.EventHandler(this.FilterToMarketToolStripMenuItem_Click);
            // 
            // exportToSpreadsheetToolStripMenuItem
            // 
            this.exportToSpreadsheetToolStripMenuItem.Name = "exportToSpreadsheetToolStripMenuItem";
            this.exportToSpreadsheetToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.exportToSpreadsheetToolStripMenuItem.Text = "Export to Spreadsheet";
            this.exportToSpreadsheetToolStripMenuItem.Click += new System.EventHandler(this.exportToSpreadsheetToolStripMenuItem_Click);
            // 
            // factoryBreakdownForSelectedToolStripMenuItem
            // 
            this.factoryBreakdownForSelectedToolStripMenuItem.Name = "factoryBreakdownForSelectedToolStripMenuItem";
            this.factoryBreakdownForSelectedToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.factoryBreakdownForSelectedToolStripMenuItem.Text = "Factory Breakdown for Selected";
            this.factoryBreakdownForSelectedToolStripMenuItem.Click += new System.EventHandler(this.factoryBreakdownForSelectedToolStripMenuItem_Click);
            // 
            // searchPanel
            // 
            this.searchPanel.AutoSize = true;
            this.searchPanel.Dock = DockStyle.Top;
            this.searchPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Top
                 | System.Windows.Forms.AnchorStyles.Left)));
            this.searchPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.searchPanel.Location = new System.Drawing.Point(0, 26);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(969, 34);
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(12, 20);
            this.SearchBox.MaxLength = 50;
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(290, 28);
            this.SearchBox.TabIndex = 0;
            // 
            // SearchButton
            // 
            this.SearchButton.Parent = searchPanel;
            this.SearchButton.AutoSize = true;
            this.SearchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchButton.Location = new System.Drawing.Point(310, 18);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(80, 28);
            this.SearchButton.TabIndex = 1;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // QuantityBox
            // 
            this.QuantityBox.Parent = searchPanel;
            this.QuantityBox.AutoSize = true;
            this.QuantityBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuantityBox.Location = new System.Drawing.Point(400, 20);
            this.QuantityBox.Name = "QuantityBox";
            this.QuantityBox.Size = new System.Drawing.Size(50, 28);
            this.QuantityBox.MaxLength = 4;
            this.QuantityBox.TabIndex = 2;
            this.QuantityBox.Items.AddRange(new []
            {
                "1",
                "10",
                "50",
                "100",
                "250",
                "500",
                "1000"
            });
            this.QuantityBox.Text = "10";
            this.QuantityBox.SelectedIndex = 1;
            this.QuantityBox.DropDownStyle = ComboBoxStyle.DropDownList;
            // 
            // treeView
            // 
            this.treeView.AutoSize = true;
            this.treeView.Scrollable = true;
            this.treeView.Dock = DockStyle.Fill;
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top
                 | System.Windows.Forms.AnchorStyles.Bottom
                 | System.Windows.Forms.AnchorStyles.Left
                 | System.Windows.Forms.AnchorStyles.Right
                 ));
            this.treeView.Location = new System.Drawing.Point(0, 36);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(444, 650);
            this.treeView.Padding = new Padding(0, 0, 0, 0);
            this.treeView.TabIndex = 3;
            // 
            // leftPanel
            // 
            this.leftPanel.BorderStyle = BorderStyle.None;
            this.leftPanel.AutoSize = false;
            this.leftPanel.Dock = DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(0, 40);
            this.leftPanel.Size = new System.Drawing.Size(486, 680);
            this.leftPanel.Margin = new Padding(10,10,10,10);
            this.leftPanel.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top
                 | System.Windows.Forms.AnchorStyles.Bottom
                 | System.Windows.Forms.AnchorStyles.Left
                 | System.Windows.Forms.AnchorStyles.Right
                 ));
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.IsSplitterFixed = true;
            this.leftPanel.Panel1MinSize = 480;
            this.leftPanel.Panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.leftPanel.Panel1.AutoScrollMinSize = new System.Drawing.Size(600, 800);
            this.leftPanel.Panel1.AutoScroll = false;
            this.leftPanel.Panel2MinSize = 480;
            this.leftPanel.Panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.leftPanel.Panel2.AutoScrollMinSize = new System.Drawing.Size(489, 800);
            this.leftPanel.Panel2.AutoScroll = true;
            this.leftPanel.DataBindings.Add("Height", this, "Height");
            // 
            // infoPanel
            // 
            this.infoPanel.AutoSize = true;
            this.infoPanel.Dock = DockStyle.Fill;
            this.infoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top
                 | System.Windows.Forms.AnchorStyles.Bottom
                 | System.Windows.Forms.AnchorStyles.Left
                 | System.Windows.Forms.AnchorStyles.Right));
            this.infoPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.infoPanel.Location = new System.Drawing.Point(0, 0);
            this.infoPanel.Size = new System.Drawing.Size(700, 800);
            this.infoPanel.AutoScroll = true;
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.TabIndex = 4;

            this.searchPanel.Controls.Add(SearchBox);
            this.searchPanel.Controls.Add(SearchButton);
            this.searchPanel.Controls.Add(QuantityBox);

            this.leftPanel.Panel1.Controls.Add(searchPanel);
            this.leftPanel.Panel1.Controls.Add(treeView);
            this.leftPanel.Panel2.Controls.Add(infoPanel);
            this.Controls.Add(menuStrip1);
            this.Controls.Add(leftPanel);
            // 
            // MainForm
            // 
            this.Dock = DockStyle.None;
            this.AcceptButton = this.SearchButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(969, 700);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(400, 500);
            this.Name = "MainForm";
            this.Text = "DU Industry Tool (Mercury)";
            this.leftPanel.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
            this.ResizeEnd += OnMainformResize;
        }

        #endregion
    }
}