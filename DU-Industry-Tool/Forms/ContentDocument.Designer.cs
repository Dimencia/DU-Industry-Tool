using System.Windows.Forms;

namespace DU_Industry_Tool
{
    partial class ContentDocument
    {
        private Krypton.Toolkit.KryptonPanel kryptonPanel;
        public System.Windows.Forms.FlowLayoutPanel InfoPanel;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentDocument));
            this.InfoPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.kryptonPanel = new Krypton.Toolkit.KryptonPanel();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel)).BeginInit();
            this.kryptonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoPanel
            // 
            this.InfoPanel.AutoScroll = false;
            this.InfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoPanel.AutoSize = true;
            this.InfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom 
                                                       | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.InfoPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.InfoPanel.Location = new System.Drawing.Point(0, 0);
            this.InfoPanel.Size = new System.Drawing.Size(600, 600);
            this.InfoPanel.Name = "InfoPanel";
            this.InfoPanel.TabIndex = 0;
            // 
            // kryptonPanel
            // 
            this.kryptonPanel.Controls.Add(this.InfoPanel);
            this.kryptonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanel.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanel.Name = "kryptonPanel";
            this.kryptonPanel.Padding = new System.Windows.Forms.Padding(0);
            this.kryptonPanel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            this.kryptonPanel.Size = new System.Drawing.Size(600, 600);
            this.kryptonPanel.TabIndex = 1;
            // 
            // ContentDocument
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanel);
            this.Name = "ContentDocument";
            this.Size = new System.Drawing.Size(610, 600);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel)).EndInit();
            this.kryptonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}
