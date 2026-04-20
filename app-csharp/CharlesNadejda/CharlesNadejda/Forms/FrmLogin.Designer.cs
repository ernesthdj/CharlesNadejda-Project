namespace CharlesNadejda.Forms
{
    partial class FrmLogin
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
            this.pnlHeader     = new System.Windows.Forms.Panel();
            this.lblTitre      = new System.Windows.Forms.Label();
            this.lblSoustitre  = new System.Windows.Forms.Label();
            this.pnlForm       = new System.Windows.Forms.Panel();
            this.lblEmailLabel = new System.Windows.Forms.Label();
            this.txtEmail      = new System.Windows.Forms.TextBox();
            this.lblMdpLabel   = new System.Windows.Forms.Label();
            this.txtMotDePasse = new System.Windows.Forms.TextBox();
            this.btnConnexion  = new System.Windows.Forms.Button();
            this.lblErreur     = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // ── Palette Charles & Nadejda — design system v2 ────────────────
            System.Drawing.Color chocolatFonce = System.Drawing.Color.FromArgb(61,  40,  23);  // #3D2817
            System.Drawing.Color chocolatMoyen = System.Drawing.Color.FromArgb(111, 78,  55);  // #6F4E37
            System.Drawing.Color chocoAbyss    = System.Drawing.Color.FromArgb(30,  15,   8);  // #1E0F08
            System.Drawing.Color creameWarm    = System.Drawing.Color.FromArgb(236, 233, 216); // #ECE9D8
            System.Drawing.Color or            = System.Drawing.Color.FromArgb(212, 175,  55); // #D4AF37

            // ── Bandeau supérieur (gradient chocolat foncé → moyen, identité marque) ──
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 80;
            this.pnlHeader.BackColor = chocolatFonce;
            this.pnlHeader.Paint    += new System.Windows.Forms.PaintEventHandler(this.PnlHeader_Paint);

            this.lblTitre.AutoSize  = false;
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitre.ForeColor = or;
            this.lblTitre.Location  = new System.Drawing.Point(0, 14);
            this.lblTitre.Size      = new System.Drawing.Size(400, 32);
            this.lblTitre.Text      = "Charles & Nadejda";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblSoustitre.AutoSize  = false;
            this.lblSoustitre.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblSoustitre.ForeColor = System.Drawing.Color.FromArgb(200, 175, 140);
            this.lblSoustitre.Location  = new System.Drawing.Point(0, 50);
            this.lblSoustitre.Size      = new System.Drawing.Size(400, 20);
            this.lblSoustitre.Text      = "ArtisaStock — Gestion de production artisanale";
            this.lblSoustitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.pnlHeader.Controls.Add(this.lblTitre);
            this.pnlHeader.Controls.Add(this.lblSoustitre);

            // ── Panneau de formulaire (fond crème warm) ─────────────────────
            this.pnlForm.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlForm.BackColor = creameWarm;
            this.pnlForm.Padding   = new System.Windows.Forms.Padding(40, 24, 40, 24);

            // lblEmailLabel
            this.lblEmailLabel.AutoSize  = false;
            this.lblEmailLabel.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEmailLabel.ForeColor = chocolatFonce;
            this.lblEmailLabel.BackColor = System.Drawing.Color.Transparent;
            this.lblEmailLabel.Location  = new System.Drawing.Point(40, 24);
            this.lblEmailLabel.Size      = new System.Drawing.Size(320, 18);
            this.lblEmailLabel.Text      = "Adresse email";

            // txtEmail
            this.txtEmail.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEmail.Location    = new System.Drawing.Point(40, 44);
            this.txtEmail.Size        = new System.Drawing.Size(320, 26);
            this.txtEmail.TabIndex    = 0;
            this.txtEmail.BackColor   = System.Drawing.Color.White;
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // lblMdpLabel
            this.lblMdpLabel.AutoSize  = false;
            this.lblMdpLabel.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblMdpLabel.ForeColor = chocolatFonce;
            this.lblMdpLabel.BackColor = System.Drawing.Color.Transparent;
            this.lblMdpLabel.Location  = new System.Drawing.Point(40, 86);
            this.lblMdpLabel.Size      = new System.Drawing.Size(320, 18);
            this.lblMdpLabel.Text      = "Mot de passe";

            // txtMotDePasse
            this.txtMotDePasse.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMotDePasse.Location     = new System.Drawing.Point(40, 106);
            this.txtMotDePasse.Size         = new System.Drawing.Size(320, 26);
            this.txtMotDePasse.PasswordChar = '•';
            this.txtMotDePasse.TabIndex     = 1;
            this.txtMotDePasse.BackColor    = System.Drawing.Color.White;
            this.txtMotDePasse.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMotDePasse.KeyDown     += new System.Windows.Forms.KeyEventHandler(this.txtMotDePasse_KeyDown);

            // btnConnexion — CTA principal (Fitts : large, centré)
            this.btnConnexion.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnConnexion.Location  = new System.Drawing.Point(40, 152);
            this.btnConnexion.Size      = new System.Drawing.Size(320, 40);
            this.btnConnexion.TabIndex  = 2;
            this.btnConnexion.Text      = "Se connecter";
            this.btnConnexion.BackColor = chocolatFonce;
            this.btnConnexion.ForeColor = System.Drawing.Color.White;
            this.btnConnexion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnexion.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnConnexion.FlatAppearance.BorderSize         = 1;
            this.btnConnexion.FlatAppearance.BorderColor        = chocoAbyss;
            this.btnConnexion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(88, 60, 36);
            this.btnConnexion.Click    += new System.EventHandler(this.btnConnexion_Click);

            // lblErreur — rouge design system (#C72C48), pas de popup
            this.lblErreur.AutoSize  = false;
            this.lblErreur.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblErreur.ForeColor = System.Drawing.Color.FromArgb(199, 44, 72); // #C72C48
            this.lblErreur.BackColor = System.Drawing.Color.Transparent;
            this.lblErreur.Location  = new System.Drawing.Point(40, 200);
            this.lblErreur.Size      = new System.Drawing.Size(320, 20);
            this.lblErreur.Text      = "";
            this.lblErreur.Visible   = false;
            this.lblErreur.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.pnlForm.Controls.Add(this.lblEmailLabel);
            this.pnlForm.Controls.Add(this.txtEmail);
            this.pnlForm.Controls.Add(this.lblMdpLabel);
            this.pnlForm.Controls.Add(this.txtMotDePasse);
            this.pnlForm.Controls.Add(this.btnConnexion);
            this.pnlForm.Controls.Add(this.lblErreur);

            // FrmLogin
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(400, 320);
            this.Controls.Add(this.pnlForm);   // Fill en 1er (index 0)
            this.Controls.Add(this.pnlHeader); // Top en 2e (traité en 1er visuellement)
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Name            = "FrmLogin";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "Connexion — ArtisaStock";
            this.BackColor       = creameWarm;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel  pnlHeader;
        private System.Windows.Forms.Panel  pnlForm;
        private System.Windows.Forms.Label  lblTitre;
        private System.Windows.Forms.Label  lblSoustitre;
        private System.Windows.Forms.Label  lblEmailLabel;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label  lblMdpLabel;
        private System.Windows.Forms.TextBox txtMotDePasse;
        private System.Windows.Forms.Button btnConnexion;
        private System.Windows.Forms.Label  lblErreur;
    }
}
