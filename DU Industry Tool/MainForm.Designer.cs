using System.ComponentModel;
using System.Windows.Forms;
using ComponentFactory.Krypton.Navigator;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.kryptonRibbon = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.buttonOreValues = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonSchematicValues = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonSkillLevels = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonUpdateMarketValues = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonFilterToMarket = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonExportToSpreadsheet = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.buttonFactoryBreakdownForSelected = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.ribbonAppButtonExit = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.kryptonDockableWorkspace = new ComponentFactory.Krypton.Docking.KryptonDockableWorkspace();
            this.kryptonPage1 = new ComponentFactory.Krypton.Navigator.KryptonPage();
            this.searchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.QuantityBox = new System.Windows.Forms.ComboBox();
            this.PreviousButton = new System.Windows.Forms.Button();
            this.treeView = new System.Windows.Forms.TreeView();
            this.kryptonWorkspaceCell1 = new ComponentFactory.Krypton.Workspace.KryptonWorkspaceCell();
            this.kryptonWorkspaceCell2 = new ComponentFactory.Krypton.Workspace.KryptonWorkspaceCell();
            this.kryptonPage2 = new ComponentFactory.Krypton.Navigator.KryptonPage();
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.kryptonManager = new ComponentFactory.Krypton.Toolkit.KryptonManager(this.components);
            this.kryptonDockingManager = new ComponentFactory.Krypton.Docking.KryptonDockingManager();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonDockableWorkspace)).BeginInit();
            this.kryptonDockableWorkspace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPage1)).BeginInit();
            this.kryptonPage1.SuspendLayout();
            this.searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonWorkspaceCell1)).BeginInit();
            this.kryptonWorkspaceCell1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonWorkspaceCell2)).BeginInit();
            this.kryptonWorkspaceCell2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPage2)).BeginInit();
            this.SuspendLayout();
            // 
            // kryptonRibbon
            // 
            this.kryptonRibbon.InDesignHelperMode = true;
            this.kryptonRibbon.Name = "kryptonRibbon";
            this.kryptonRibbon.RibbonAppButton.AppButtonMenuItems.AddRange(new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItemBase[] {
                this.buttonOreValues,
                this.buttonSchematicValues,
                this.buttonSkillLevels,
                this.buttonUpdateMarketValues,
                this.buttonFilterToMarket,
                this.buttonExportToSpreadsheet,
                this.buttonFactoryBreakdownForSelected,
                this.ribbonAppButtonExit});
            this.kryptonRibbon.RibbonAppButton.AppButtonShowRecentDocs = false;
            this.kryptonRibbon.Size = new System.Drawing.Size(764, 5);
            this.kryptonRibbon.TabIndex = 0;
            this.kryptonRibbon.AllowMinimizedChange = false;
            this.kryptonRibbon.ShowMinimizeButton = false;
            this.kryptonRibbon.MinimizedMode = true;
            // 
            // buttonOreValues
            // 
            this.buttonOreValues.Text = "&Ore/Plasma Values";
            this.buttonOreValues.Click += new System.EventHandler(this.inputOreValuesToolStripMenuItem_Click);
            // 
            // buttonSchematicValues
            // 
            this.buttonSchematicValues.Text = "&Schematics";
            this.buttonSchematicValues.Click += new System.EventHandler(this.SchematicValuesToolStripMenuItem_Click);
            // 
            // buttonSkillLevels
            // 
            this.buttonSkillLevels.Text = "&Talents";
            this.buttonSkillLevels.Click += new System.EventHandler(this.SkillLevelsToolStripMenuItem_Click);
            // 
            // buttonUpdateMarketValues
            // 
            this.buttonUpdateMarketValues.Text = "&Update Market Values";
            this.buttonUpdateMarketValues.Click += new System.EventHandler(this.UpdateMarketValuesToolStripMenuItem_Click);
            // 
            // buttonFilterToMarket
            // 
            this.buttonFilterToMarket.Text = "&Filter to Market";
            this.buttonFilterToMarket.Click += new System.EventHandler(this.FilterToMarketToolStripMenuItem_Click);
            // 
            // buttonExportToSpreadsheet
            // 
            this.buttonExportToSpreadsheet.Text = "&Export to CSV";
            this.buttonExportToSpreadsheet.Click += new System.EventHandler(this.exportToSpreadsheetToolStripMenuItem_Click);
            // 
            // buttonFactoryBreakdownForSelected
            // 
            this.buttonFactoryBreakdownForSelected.Text = "&Factory Breakdown for Selected";
            this.buttonFactoryBreakdownForSelected.Click += new System.EventHandler(this.factoryBreakdownForSelectedToolStripMenuItem_Click);
            // 
            // ribbonAppButtonExit
            // 
            this.ribbonAppButtonExit.Image = ((System.Drawing.Image)(resources.GetObject("ribbonAppButtonExit.Image")));
            this.ribbonAppButtonExit.Text = "E&xit";
            this.ribbonAppButtonExit.Click += new System.EventHandler(this.ribbonAppButtonExit_Click);
            // 
            // kryptonDockableWorkspace
            // 
            this.kryptonDockableWorkspace.AutoHiddenHost = false;
            this.kryptonDockableWorkspace.CompactFlags = ComponentFactory.Krypton.Workspace.CompactFlags.AtLeastOneVisibleCell;
            this.kryptonDockableWorkspace.ContainerBackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.TabDock;
            this.kryptonDockableWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonDockableWorkspace.Location = new System.Drawing.Point(0, 30);
            this.kryptonDockableWorkspace.Name = "kryptonDockableWorkspace";
            this.kryptonDockableWorkspace.Root.Children.AddRange(new System.ComponentModel.Component[] {
            this.kryptonWorkspaceCell1,
            this.kryptonWorkspaceCell2});
            this.kryptonDockableWorkspace.Root.UniqueName = "D51970B3EA2C496AD51970B3EA2C496A";
            this.kryptonDockableWorkspace.Root.WorkspaceControl = this.kryptonDockableWorkspace;
            this.kryptonDockableWorkspace.SeparatorStyle = ComponentFactory.Krypton.Toolkit.SeparatorStyle.HighProfile;
            this.kryptonDockableWorkspace.ShowMaximizeButton = false;
            this.kryptonDockableWorkspace.Size = new System.Drawing.Size(1000, 950);
            this.kryptonDockableWorkspace.SplitterWidth = 8;
            this.kryptonDockableWorkspace.TabIndex = 0;
            this.kryptonDockableWorkspace.TabStop = true;
            this.kryptonDockableWorkspace.WorkspaceCellAdding += new System.EventHandler<ComponentFactory.Krypton.Workspace.WorkspaceCellEventArgs>(this.kryptonDockableWorkspace_WorkspaceCellAdding);
            // 
            // searchPanel
            // 
            this.searchPanel.AutoSize = true;
            this.searchPanel.Controls.Add(this.SearchBox);
            this.searchPanel.Controls.Add(this.SearchButton);
            this.searchPanel.Controls.Add(this.QuantityBox);
            this.searchPanel.Controls.Add(this.PreviousButton);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(0, 0);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(541, 36);
            this.searchPanel.TabIndex = 0;
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(3, 3);
            this.SearchBox.MaxLength = 50;
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(200, 28);
            this.SearchBox.TabIndex = 0;
            // 
            // SearchButton
            // 
            this.SearchButton.AutoSize = true;
            this.SearchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchButton.Location = new System.Drawing.Point(209, 3);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(80, 30);
            this.SearchButton.TabIndex = 1;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // QuantityBox
            // 
            this.QuantityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QuantityBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuantityBox.Items.AddRange(new object[] {
            "1",
            "2",
            "5",
            "10",
            "20",
            "50",
            "100",
            "200",
            "500",
            "1000"});
            this.QuantityBox.Location = new System.Drawing.Point(295, 3);
            this.QuantityBox.MaxDropDownItems = 10;
            this.QuantityBox.MaxLength = 4;
            this.QuantityBox.Name = "QuantityBox";
            this.QuantityBox.Size = new System.Drawing.Size(50, 28);
            this.QuantityBox.TabIndex = 2;
            // 
            // PreviousButton
            // 
            this.PreviousButton.AutoSize = true;
            this.PreviousButton.Enabled = false;
            this.PreviousButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PreviousButton.Location = new System.Drawing.Point(351, 3);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(39, 30);
            this.PreviousButton.TabIndex = 3;
            this.PreviousButton.Text = "<<";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(0, 40);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(535, 916);
            this.treeView.TabIndex = 3;
            // 
            // kryptonPage1
            // 
            this.kryptonPage1.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
            this.kryptonPage1.Controls.Add(this.searchPanel);
            this.kryptonPage1.Controls.Add(this.treeView);
            this.kryptonPage1.Flags = 0;
            this.kryptonPage1.SetFlags(KryptonPageFlags.DockingAllowDocked);
            this.kryptonPage1.LastVisibleSet = true;
            this.kryptonPage1.MinimumSize = new System.Drawing.Size(450, 450);
            this.kryptonPage1.Name = "kryptonPage1";
            this.kryptonPage1.Size = new System.Drawing.Size(541, 955);
            this.kryptonPage1.Text = "Recipes Explorer";
            this.kryptonPage1.TextDescription = "";
            this.kryptonPage1.TextTitle = "";
            this.kryptonPage1.ToolTipTitle = "";
            this.kryptonPage1.UniqueName = "38D886AD20CD402D38D886AD20CD402D";
            this.kryptonPage1.CausesValidation = false;
            // 
            // kryptonWorkspaceCell1
            // 
            this.kryptonWorkspaceCell1.AllowPageReorder = false;
            this.kryptonWorkspaceCell1.AllowPageDrag = false;
            this.kryptonWorkspaceCell1.AllowTabFocus = false;
            this.kryptonWorkspaceCell1.Button.ButtonDisplayLogic = ComponentFactory.Krypton.Navigator.ButtonDisplayLogic.None;
            this.kryptonWorkspaceCell1.Button.CloseButtonAction = ComponentFactory.Krypton.Navigator.CloseButtonAction.None;
            this.kryptonWorkspaceCell1.Button.CloseButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell1.Button.ContextButtonAction = ComponentFactory.Krypton.Navigator.ContextButtonAction.SelectPage;
            this.kryptonWorkspaceCell1.Button.ContextButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell1.Button.ContextMenuMapImage = ComponentFactory.Krypton.Navigator.MapKryptonPageImage.Small;
            this.kryptonWorkspaceCell1.Button.ContextMenuMapText = ComponentFactory.Krypton.Navigator.MapKryptonPageText.TextTitle;
            this.kryptonWorkspaceCell1.Button.NextButtonAction = ComponentFactory.Krypton.Navigator.DirectionButtonAction.None;
            this.kryptonWorkspaceCell1.Button.NextButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell1.Button.PreviousButtonAction = ComponentFactory.Krypton.Navigator.DirectionButtonAction.None;
            this.kryptonWorkspaceCell1.Button.PreviousButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell1.Name = "kryptonWorkspaceCell1";
            this.kryptonWorkspaceCell1.NavigatorMode = ComponentFactory.Krypton.Navigator.NavigatorMode.HeaderGroupTab;// BarTabGroup;
            this.kryptonWorkspaceCell1.Pages.AddRange(new ComponentFactory.Krypton.Navigator.KryptonPage[] {
            this.kryptonPage1});
            this.kryptonWorkspaceCell1.SelectedIndex = 0;
            this.kryptonWorkspaceCell2.CausesValidation = false;
            this.kryptonWorkspaceCell1.UniqueName = "B46823ED744B4A87B46823ED744B4A87";
            // 
            // kryptonWorkspaceCell2
            // 
            this.kryptonWorkspaceCell2.AllowPageDrag = true;
            this.kryptonWorkspaceCell1.AllowPageReorder = true;
            this.kryptonWorkspaceCell2.AllowTabFocus = false;
            this.kryptonWorkspaceCell2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.kryptonWorkspaceCell2.AutoSize = false;
            this.kryptonWorkspaceCell2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kryptonWorkspaceCell2.Bar.BarMapExtraText = ComponentFactory.Krypton.Navigator.MapKryptonPageText.None;
            this.kryptonWorkspaceCell2.Bar.BarMapImage = ComponentFactory.Krypton.Navigator.MapKryptonPageImage.Small;
            this.kryptonWorkspaceCell2.Bar.BarMapText = ComponentFactory.Krypton.Navigator.MapKryptonPageText.TextTitle;
            this.kryptonWorkspaceCell2.Bar.ItemSizing = ComponentFactory.Krypton.Navigator.BarItemSizing.SameHeight;
            this.kryptonWorkspaceCell2.Bar.TabStyle = ComponentFactory.Krypton.Toolkit.TabStyle.Dock;
            this.kryptonWorkspaceCell2.Button.ButtonDisplayLogic = ComponentFactory.Krypton.Navigator.ButtonDisplayLogic.Context;
            this.kryptonWorkspaceCell2.Button.CloseButtonAction = ComponentFactory.Krypton.Navigator.CloseButtonAction.RemovePageAndDispose;
            this.kryptonWorkspaceCell2.Button.CloseButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Logic;
            this.kryptonWorkspaceCell2.Button.ContextButtonAction = ComponentFactory.Krypton.Navigator.ContextButtonAction.None;
            this.kryptonWorkspaceCell2.Button.ContextButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell2.Button.ContextMenuMapImage = ComponentFactory.Krypton.Navigator.MapKryptonPageImage.Small;
            this.kryptonWorkspaceCell2.Button.ContextMenuMapText = ComponentFactory.Krypton.Navigator.MapKryptonPageText.TextTitle;
            this.kryptonWorkspaceCell2.Button.NextButtonAction = ComponentFactory.Krypton.Navigator.DirectionButtonAction.None;
            this.kryptonWorkspaceCell2.Button.NextButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell2.Button.PreviousButtonAction = ComponentFactory.Krypton.Navigator.DirectionButtonAction.None;
            this.kryptonWorkspaceCell2.Button.PreviousButtonDisplay = ComponentFactory.Krypton.Navigator.ButtonDisplay.Hide;
            this.kryptonWorkspaceCell2.CausesValidation = false;
            this.kryptonWorkspaceCell2.Name = "kryptonWorkspaceCell2";
            this.kryptonWorkspaceCell2.NavigatorMode =
                ComponentFactory.Krypton.Navigator.NavigatorMode.BarCheckButtonGroupOutside;
            this.kryptonWorkspaceCell2.UniqueName = "B46823ED744B4A87B46823ED744B4A88";
            this.kryptonWorkspaceCell2.ShowContextMenu += kryptonWorkspaceCell2_OnCellShowContextMenu;
            // 
            // kryptonPage2
            // 
            this.kryptonPage2.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
            this.kryptonPage2.Flags = 32;
            this.kryptonPage2.LastVisibleSet = true;
            this.kryptonPage2.Margin = new System.Windows.Forms.Padding(5);
            this.kryptonPage2.MinimumSize = new System.Drawing.Size(450, 450);
            this.kryptonPage2.Name = "kryptonPage2";
            this.kryptonPage2.Size = new System.Drawing.Size(542, 955);
            this.kryptonPage2.Text = "Calculations";
            this.kryptonPage2.TextDescription = "";
            this.kryptonPage2.TextTitle = "Calculation";
            this.kryptonPage2.ToolTipTitle = "Page ToolTip";
            this.kryptonPage2.UniqueName = "38D886AD20CD402D38D886AD20CD402E";
            this.kryptonPage2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.kryptonPage2.AllowDrop = false;
            this.kryptonPage2.Dock = DockStyle.Fill;
            // 
            // imageListSmall
            // 
            this.imageListSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmall.ImageStream")));
            this.imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSmall.Images.SetKeyName(0, "document_plain.png");
            this.imageListSmall.Images.SetKeyName(1, "preferences.png");
            this.imageListSmall.Images.SetKeyName(2, "information2.png");
            // 
            // kryptonManager
            // 
            this.kryptonManager.GlobalPaletteMode =
                ComponentFactory.Krypton.Toolkit.PaletteModeManager.Office2010Silver;
            // 
            // MainForm
            // 
            this.AcceptButton = this.SearchButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1095, 986);
            this.Controls.Add(this.kryptonDockableWorkspace);
            this.Controls.Add(this.kryptonRibbon);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "MainForm";
            this.Text = "DU Industry Tool (Mercury)";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonDockableWorkspace)).EndInit();
            this.kryptonDockableWorkspace.ResumeLayout(false);
            this.kryptonDockableWorkspace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPage1)).EndInit();
            this.kryptonPage1.ResumeLayout(false);
            this.kryptonPage1.PerformLayout();
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonWorkspaceCell1)).EndInit();
            this.kryptonWorkspaceCell1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonWorkspaceCell2)).EndInit();
            this.kryptonWorkspaceCell2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPage2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
            this.ResizeEnd += OnMainformResize;
        }

        #endregion

        private System.Windows.Forms.ImageList imageListSmall;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbon kryptonRibbon;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem ribbonAppButtonExit;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonOreValues;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonSchematicValues;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonSkillLevels;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonUpdateMarketValues;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonFilterToMarket;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonExportToSpreadsheet;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem buttonFactoryBreakdownForSelected;
        private ComponentFactory.Krypton.Toolkit.KryptonManager kryptonManager;
        private ComponentFactory.Krypton.Docking.KryptonDockingManager kryptonDockingManager;
        private ComponentFactory.Krypton.Docking.KryptonDockableWorkspace kryptonDockableWorkspace;
        private ComponentFactory.Krypton.Workspace.KryptonWorkspaceCell kryptonWorkspaceCell1;
        private ComponentFactory.Krypton.Workspace.KryptonWorkspaceCell kryptonWorkspaceCell2;
        private ComponentFactory.Krypton.Navigator.KryptonPage kryptonPage1;
        private ComponentFactory.Krypton.Navigator.KryptonPage kryptonPage2;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.FlowLayoutPanel searchPanel;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Button PreviousButton;
        private System.Windows.Forms.ComboBox QuantityBox;
    }
}