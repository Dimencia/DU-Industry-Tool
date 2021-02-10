namespace DU_Industry_Tool
{
    partial class OreValueForm
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
            this.oreGrid = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.OreName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.oreGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // oreGrid
            // 
            this.oreGrid.AllowUserToAddRows = false;
            this.oreGrid.AllowUserToDeleteRows = false;
            this.oreGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.oreGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OreName,
            this.Value});
            this.oreGrid.Location = new System.Drawing.Point(12, 12);
            this.oreGrid.Name = "oreGrid";
            this.oreGrid.RowHeadersWidth = 51;
            this.oreGrid.RowTemplate.Height = 24;
            this.oreGrid.Size = new System.Drawing.Size(499, 590);
            this.oreGrid.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 608);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(524, 34);
            this.button1.TabIndex = 1;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OreName
            // 
            this.OreName.HeaderText = "Ore Name";
            this.OreName.MinimumWidth = 6;
            this.OreName.Name = "OreName";
            this.OreName.Width = 125;
            // 
            // Value
            // 
            this.Value.HeaderText = "Value";
            this.Value.MinimumWidth = 6;
            this.Value.Name = "Value";
            this.Value.Width = 125;
            // 
            // OreValueForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 642);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.oreGrid);
            this.Name = "OreValueForm";
            this.Text = "Ore Values";
            ((System.ComponentModel.ISupportInitialize)(this.oreGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView oreGrid;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn OreName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
    }
}