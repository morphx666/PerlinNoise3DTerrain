namespace PerlinNoise3DTerrain {
    partial class FormMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.LabelShortcuts = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelShortcuts
            // 
            this.LabelShortcuts.AutoSize = true;
            this.LabelShortcuts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(11)))), ((int)(((byte)(11)))), ((int)(((byte)(11)))));
            this.LabelShortcuts.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelShortcuts.ForeColor = System.Drawing.Color.Gainsboro;
            this.LabelShortcuts.Location = new System.Drawing.Point(12, 9);
            this.LabelShortcuts.Name = "LabelShortcuts";
            this.LabelShortcuts.Padding = new System.Windows.Forms.Padding(8);
            this.LabelShortcuts.Size = new System.Drawing.Size(376, 88);
            this.LabelShortcuts.TabIndex = 0;
            this.LabelShortcuts.Text = "Arrow Keys = Move\r\n[+] / [-]  = Change terrain height\r\n[Q] / [A]  = Change Perlin" +
    " Noise Z attribute\r\n[R]        = Change Render Mode: Filled";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SkyBlue;
            this.ClientSize = new System.Drawing.Size(1067, 692);
            this.Controls.Add(this.LabelShortcuts);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.DimGray;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Perlin Noise 3D Terrain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelShortcuts;
    }
}

