namespace DU_Industry_Tool
{
    partial class MainForm
    {
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inputOreValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.skillLevelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateMarketValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToMarketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.exportToSpreadsheetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(12, 55);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(539, 430);
            this.treeView.TabIndex = 0;
            // 
            // infoPanel
            // 
            this.infoPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.infoPanel.Location = new System.Drawing.Point(557, 55);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(400, 430);
            this.infoPanel.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(969, 28);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inputOreValuesToolStripMenuItem,
            this.skillLevelsToolStripMenuItem,
            this.updateMarketValuesToolStripMenuItem,
            this.filterToMarketToolStripMenuItem,
            this.exportToSpreadsheetToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 24);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // inputOreValuesToolStripMenuItem
            // 
            this.inputOreValuesToolStripMenuItem.Name = "inputOreValuesToolStripMenuItem";
            this.inputOreValuesToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.inputOreValuesToolStripMenuItem.Text = "Ore Values";
            this.inputOreValuesToolStripMenuItem.Click += new System.EventHandler(this.inputOreValuesToolStripMenuItem_Click);
            // 
            // skillLevelsToolStripMenuItem
            // 
            this.skillLevelsToolStripMenuItem.Name = "skillLevelsToolStripMenuItem";
            this.skillLevelsToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.skillLevelsToolStripMenuItem.Text = "Skill Levels";
            this.skillLevelsToolStripMenuItem.Click += new System.EventHandler(this.skillLevelsToolStripMenuItem_Click);
            // 
            // updateMarketValuesToolStripMenuItem
            // 
            this.updateMarketValuesToolStripMenuItem.Name = "updateMarketValuesToolStripMenuItem";
            this.updateMarketValuesToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.updateMarketValuesToolStripMenuItem.Text = "Update Market Values";
            this.updateMarketValuesToolStripMenuItem.Click += new System.EventHandler(this.updateMarketValuesToolStripMenuItem_Click);
            // 
            // filterToMarketToolStripMenuItem
            // 
            this.filterToMarketToolStripMenuItem.Name = "filterToMarketToolStripMenuItem";
            this.filterToMarketToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.filterToMarketToolStripMenuItem.Text = "Filter To Market";
            this.filterToMarketToolStripMenuItem.Click += new System.EventHandler(this.filterToMarketToolStripMenuItem_Click);
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(12, 31);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(458, 22);
            this.SearchBox.TabIndex = 3;
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(476, 31);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(75, 23);
            this.SearchButton.TabIndex = 4;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // exportToSpreadsheetToolStripMenuItem
            // 
            this.exportToSpreadsheetToolStripMenuItem.Name = "exportToSpreadsheetToolStripMenuItem";
            this.exportToSpreadsheetToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.exportToSpreadsheetToolStripMenuItem.Text = "Export to Spreadsheet";
            this.exportToSpreadsheetToolStripMenuItem.Click += new System.EventHandler(this.exportToSpreadsheetToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.SearchButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(969, 497);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.SearchBox);
            this.Controls.Add(this.infoPanel);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "DU Industry Tool";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.FlowLayoutPanel infoPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inputOreValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem skillLevelsToolStripMenuItem;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.ToolStripMenuItem updateMarketValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToMarketToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToSpreadsheetToolStripMenuItem;
    }
}