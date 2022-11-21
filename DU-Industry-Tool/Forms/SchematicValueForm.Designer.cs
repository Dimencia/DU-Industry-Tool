namespace DU_Industry_Tool
{
    partial class SchematicValueForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.schematicsGrid = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.SchematicName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Quanta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.schematicsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // schematicsGrid
            // 
            this.schematicsGrid.AllowUserToAddRows = false;
            this.schematicsGrid.AllowUserToDeleteRows = false;
            this.schematicsGrid.AllowUserToResizeColumns = false;
            this.schematicsGrid.AllowUserToResizeRows = false;
            this.schematicsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.schematicsGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.schematicsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.schematicsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SchematicName,
            this.Quanta});
            this.schematicsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.schematicsGrid.GridColor = System.Drawing.SystemColors.Window;
            this.schematicsGrid.Location = new System.Drawing.Point(14, 14);
            this.schematicsGrid.MultiSelect = false;
            this.schematicsGrid.Name = "schematicsGrid";
            this.schematicsGrid.RowHeadersVisible = false;
            this.schematicsGrid.RowHeadersWidth = 40;
            this.schematicsGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.schematicsGrid.RowTemplate.Height = 24;
            this.schematicsGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.schematicsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.schematicsGrid.ShowEditingIcon = false;
            this.schematicsGrid.Size = new System.Drawing.Size(561, 664);
            this.schematicsGrid.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(256, 684);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "&Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // SchematicName
            // 
            this.SchematicName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SchematicName.DefaultCellStyle = dataGridViewCellStyle1;
            this.SchematicName.HeaderText = "Schematic Name";
            this.SchematicName.MinimumWidth = 60;
            this.SchematicName.Name = "SchematicName";
            this.SchematicName.ReadOnly = true;
            // 
            // Quanta
            // 
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Quanta.DefaultCellStyle = dataGridViewCellStyle2;
            this.Quanta.HeaderText = "Quanta";
            this.Quanta.MinimumWidth = 125;
            this.Quanta.Name = "Quanta";
            this.Quanta.ReadOnly = true;
            this.Quanta.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Quanta.Width = 125;
            // 
            // SchematicValueForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(590, 722);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.schematicsGrid);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SchematicValueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Schematic prices (per 1 item!)";
            ((System.ComponentModel.ISupportInitialize)(this.schematicsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView schematicsGrid;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn SchematicName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Quanta;
    }
}