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
            this.menuStrip      = new System.Windows.Forms.MenuStrip();
            this.statusStrip    = new System.Windows.Forms.StatusStrip();
            this.lblUtilisateur = new System.Windows.Forms.ToolStripStatusLabel();

            // Menu items
            this.menuFournisseurs    = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCatalogueWeb    = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCatCategories   = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCatParfums      = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCatProduits     = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCommandes       = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSession         = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDeconnexion     = new System.Windows.Forms.ToolStripMenuItem();

            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // ── menuStrip ────────────────────────────────────────────
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.menuStrip.ForeColor = System.Drawing.Color.White;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuCatalogueWeb,
                this.menuFournisseurs,
                this.menuCommandes,
                this.menuSession
            });
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Size     = new System.Drawing.Size(1200, 24);
            this.menuStrip.Renderer = new CharlesNadejda.Forms.FrmPrincipal.DarkMenuRenderer();

            // Catalogue web — placeholder (à développer après ERP)
            this.menuCatalogueWeb.Text      = "Catalogue web  [à venir]";
            this.menuCatalogueWeb.ForeColor = System.Drawing.Color.FromArgb(160, 140, 110);
            this.menuCatalogueWeb.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuCatCategories,
                this.menuCatParfums,
                this.menuCatProduits
            });
            this.menuCatCategories.Text    = "Catégories  [placeholder]";
            this.menuCatCategories.Click  += new System.EventHandler(this.menuCatCategories_Click);
            this.menuCatParfums.Text       = "Parfums  [placeholder]";
            this.menuCatParfums.Click     += new System.EventHandler(this.menuCatParfums_Click);
            this.menuCatProduits.Text      = "Produits  [placeholder]";
            this.menuCatProduits.Click    += new System.EventHandler(this.menuCatProduits_Click);

            // Fournisseurs — ERP (actif, utilisé par le module Achats)
            this.menuFournisseurs.Text   = "Fournisseurs";
            this.menuFournisseurs.Click += new System.EventHandler(this.menuFournisseurs_Click);

            // Commandes — placeholder
            this.menuCommandes.Text      = "Commandes web  [à venir]";
            this.menuCommandes.ForeColor = System.Drawing.Color.FromArgb(160, 140, 110);
            this.menuCommandes.Click    += new System.EventHandler(this.menuCommandes_Click);

            // Session (à droite)
            this.menuSession.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.menuSession.Text      = "⚙ Session";
            this.menuSession.DropDownItems.Add(this.menuDeconnexion);
            this.menuDeconnexion.Text   = "Se déconnecter";
            this.menuDeconnexion.Click += new System.EventHandler(this.menuDeconnexion_Click);

            // ── statusStrip ───────────────────────────────────────────
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(50, 32, 18);
            this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(200, 180, 160);
            this.statusStrip.Items.Add(this.lblUtilisateur);
            this.lblUtilisateur.Name = "lblUtilisateur";
            this.lblUtilisateur.Text = "";

            // ── FrmPrincipal ──────────────────────────────────────────
            // Le contenu (split + cards) est construit programmatiquement dans Load
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1200, 700);
            this.MinimumSize         = new System.Drawing.Size(900, 580);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip   = this.menuStrip;
            this.Name            = "FrmPrincipal";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "Charles & Nadejda — ArtisaStock";
            this.Load           += new System.EventHandler(this.FrmPrincipal_Load);
            this.Resize         += new System.EventHandler(this.FrmPrincipal_Resize);

            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip            menuStrip;
        private System.Windows.Forms.StatusStrip          statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblUtilisateur;
        private System.Windows.Forms.ToolStripMenuItem    menuCatalogueWeb;
        private System.Windows.Forms.ToolStripMenuItem    menuCatCategories;
        private System.Windows.Forms.ToolStripMenuItem    menuCatParfums;
        private System.Windows.Forms.ToolStripMenuItem    menuCatProduits;
        private System.Windows.Forms.ToolStripMenuItem    menuFournisseurs;
        private System.Windows.Forms.ToolStripMenuItem    menuCommandes;
        private System.Windows.Forms.ToolStripMenuItem    menuSession;
        private System.Windows.Forms.ToolStripMenuItem    menuDeconnexion;
    }
}
