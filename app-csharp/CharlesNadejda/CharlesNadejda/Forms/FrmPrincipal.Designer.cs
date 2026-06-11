namespace CharlesNadejda.Forms
{
    partial class FrmPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // ── FrmPrincipal ──────────────────────────────────────────
            // Le contenu (shell + panels) est construit programmatiquement dans Load
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1200, 700);
            this.MinimumSize         = new System.Drawing.Size(900, 580);
            this.WindowState         = System.Windows.Forms.FormWindowState.Maximized;
            this.Name            = "FrmPrincipal";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "Charles & Nadejda — ArtisaStock";
            this.Load           += new System.EventHandler(this.FrmPrincipal_Load);
            this.Resize         += new System.EventHandler(this.FrmPrincipal_Resize);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
