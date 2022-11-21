using System.Windows.Forms;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.oreGrid = new System.Windows.Forms.DataGridView();
            this.BtnSave = new System.Windows.Forms.Button();
            this.OreName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrePrice = new Krypton.Toolkit.KryptonDataGridViewMaskedTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.oreGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // oreGrid
            // 
            this.oreGrid.AllowUserToAddRows = false;
            this.oreGrid.AllowUserToDeleteRows = false;
            this.oreGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.oreGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.oreGrid.ColumnHeadersHeight = 32;
            this.oreGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.oreGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OreName,
            this.OrePrice});
            this.oreGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.oreGrid.Location = new System.Drawing.Point(14, 14);
            this.oreGrid.Name = "oreGrid";
            this.oreGrid.RowHeadersWidth = 51;
            this.oreGrid.RowTemplate.Height = 24;
            this.oreGrid.Size = new System.Drawing.Size(561, 664);
            this.oreGrid.TabIndex = 0;
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSave.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnSave.Location = new System.Drawing.Point(242, 684);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(100, 32);
            this.BtnSave.TabIndex = 1;
            this.BtnSave.Text = "&Save";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.Button1_Click);
            // 
            // OreName
            // 
            this.OreName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OreName.DefaultCellStyle = dataGridViewCellStyle3;
            this.OreName.HeaderText = "Name";
            this.OreName.MinimumWidth = 60;
            this.OreName.Name = "OreName";
            this.OreName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // OrePrice
            // 
            this.OrePrice.AllowPromptAsInput = false;
            this.OrePrice.AsciiOnly = true;
            this.OrePrice.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OrePrice.DefaultCellStyle = dataGridViewCellStyle4;
            this.OrePrice.HeaderText = "Quanta/L";
            this.OrePrice.Mask = "########";
            this.OrePrice.MinimumWidth = 150;
            this.OrePrice.Name = "OrePrice";
            this.OrePrice.RejectInputOnFirstFailure = true;
            this.OrePrice.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OrePrice.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            this.OrePrice.Width = 150;
            // 
            // OreValueForm
            // 
            this.AcceptButton = this.BtnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(590, 722);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.oreGrid);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "OreValueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ore && Plasma Prices";
            ((System.ComponentModel.ISupportInitialize)(this.oreGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView oreGrid;
        private System.Windows.Forms.Button BtnSave;
        private DataGridViewTextBoxColumn OreName;
        private Krypton.Toolkit.KryptonDataGridViewMaskedTextBoxColumn OrePrice;
    }
}