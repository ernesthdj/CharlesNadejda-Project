namespace CharlesNadejda.Forms
{
    partial class FrmBomProduction
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre      = new System.Windows.Forms.Label();
            this.grpSélection  = new System.Windows.Forms.GroupBox();
            this.lblContexte   = new System.Windows.Forms.Label();
            this.cboContexte   = new System.Windows.Forms.ComboBox();
            this.lblNiveau     = new System.Windows.Forms.Label();
            this.cboNiveau     = new System.Windows.Forms.ComboBox();
            this.lblFiche      = new System.Windows.Forms.Label();
            this.cboFiche      = new System.Windows.Forms.ComboBox();
            this.lblQuantite   = new System.Windows.Forms.Label();
            this.nudQuantite   = new System.Windows.Forms.NumericUpDown();
            this.lblNotes      = new System.Windows.Forms.Label();
            this.txtNotes      = new System.Windows.Forms.TextBox();
            this.btnVérifier   = new System.Windows.Forms.Button();
            this.grpResultat   = new System.Windows.Forms.GroupBox();
            this.lblResultat   = new System.Windows.Forms.Label();
            this.dgvManques    = new System.Windows.Forms.DataGridView();
            this.btnExécuter   = new System.Windows.Forms.Button();
            this.btnFermer     = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManques)).BeginInit();
            this.grpSélection.SuspendLayout();
            this.grpResultat.SuspendLayout();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize = false;
            this.lblTitre.Font     = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.Location = new System.Drawing.Point(12, 12);
            this.lblTitre.Size     = new System.Drawing.Size(700, 30);
            this.lblTitre.Text     = "Exécution de production";

            // ── grpSélection ─────────────────────────────────────────────

            this.grpSélection.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpSélection.Location = new System.Drawing.Point(12, 52);
            this.grpSélection.Size     = new System.Drawing.Size(750, 200);
            this.grpSélection.Text     = "Paramètres de production";
            this.grpSélection.TabStop  = false;

            // lblContexte
            this.lblContexte.AutoSize = true;
            this.lblContexte.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblContexte.Location = new System.Drawing.Point(10, 28);
            this.lblContexte.Text     = "Contexte";

            // cboContexte
            this.cboContexte.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboContexte.Font                  = new System.Drawing.Font("Segoe UI", 10F);
            this.cboContexte.Location              = new System.Drawing.Point(10, 48);
            this.cboContexte.Size                  = new System.Drawing.Size(200, 26);
            this.cboContexte.TabIndex              = 0;
            this.cboContexte.SelectedIndexChanged += new System.EventHandler(this.cboContexte_SelectedIndexChanged);

            // lblNiveau
            this.lblNiveau.AutoSize = true;
            this.lblNiveau.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNiveau.Location = new System.Drawing.Point(220, 28);
            this.lblNiveau.Text     = "Niveau";

            // cboNiveau
            this.cboNiveau.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNiveau.Font                  = new System.Drawing.Font("Segoe UI", 10F);
            this.cboNiveau.Location              = new System.Drawing.Point(220, 48);
            this.cboNiveau.Size                  = new System.Drawing.Size(180, 26);
            this.cboNiveau.TabIndex              = 1;
            this.cboNiveau.SelectedIndexChanged += new System.EventHandler(this.cboNiveau_SelectedIndexChanged);

            // lblFiche
            this.lblFiche.AutoSize = true;
            this.lblFiche.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFiche.Location = new System.Drawing.Point(410, 28);
            this.lblFiche.Text     = "Fiche recette";

            // cboFiche
            this.cboFiche.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFiche.Font                  = new System.Drawing.Font("Segoe UI", 10F);
            this.cboFiche.Location              = new System.Drawing.Point(410, 48);
            this.cboFiche.Size                  = new System.Drawing.Size(240, 26);
            this.cboFiche.TabIndex              = 2;
            this.cboFiche.SelectedIndexChanged += new System.EventHandler(this.cboFiche_SelectedIndexChanged);

            // lblQuantite
            this.lblQuantite.AutoSize = true;
            this.lblQuantite.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblQuantite.Location = new System.Drawing.Point(10, 88);
            this.lblQuantite.Text     = "Quantité à produire";

            // nudQuantite
            this.nudQuantite.DecimalPlaces = 2;
            this.nudQuantite.Minimum       = new decimal(new int[] { 1, 0, 0, 131072 }); // 0.01
            this.nudQuantite.Maximum       = new decimal(new int[] { 100000, 0, 0, 0 });
            this.nudQuantite.Value         = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudQuantite.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.nudQuantite.Location      = new System.Drawing.Point(10, 110);
            this.nudQuantite.Size          = new System.Drawing.Size(120, 26);
            this.nudQuantite.TabIndex      = 3;

            // btnVérifier
            this.btnVérifier.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVérifier.Location  = new System.Drawing.Point(145, 107);
            this.btnVérifier.Size      = new System.Drawing.Size(160, 32);
            this.btnVérifier.Text      = "Vérifier le stock";
            this.btnVérifier.TabIndex  = 4;
            this.btnVérifier.BackColor = System.Drawing.Color.FromArgb(63, 81, 181);
            this.btnVérifier.ForeColor = System.Drawing.Color.White;
            this.btnVérifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVérifier.Click    += new System.EventHandler(this.btnVérifier_Click);

            // lblNotes
            this.lblNotes.AutoSize = true;
            this.lblNotes.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNotes.Location = new System.Drawing.Point(10, 152);
            this.lblNotes.Text     = "Notes (optionnel)";

            // txtNotes
            this.txtNotes.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNotes.Location = new System.Drawing.Point(10, 172);
            this.txtNotes.Size     = new System.Drawing.Size(720, 22);
            this.txtNotes.TabIndex = 5;

            this.grpSélection.Controls.Add(this.lblContexte);
            this.grpSélection.Controls.Add(this.cboContexte);
            this.grpSélection.Controls.Add(this.lblNiveau);
            this.grpSélection.Controls.Add(this.cboNiveau);
            this.grpSélection.Controls.Add(this.lblFiche);
            this.grpSélection.Controls.Add(this.cboFiche);
            this.grpSélection.Controls.Add(this.lblQuantite);
            this.grpSélection.Controls.Add(this.nudQuantite);
            this.grpSélection.Controls.Add(this.btnVérifier);
            this.grpSélection.Controls.Add(this.lblNotes);
            this.grpSélection.Controls.Add(this.txtNotes);

            // ── grpResultat ───────────────────────────────────────────────

            this.grpResultat.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpResultat.Location = new System.Drawing.Point(12, 264);
            this.grpResultat.Size     = new System.Drawing.Size(750, 250);
            this.grpResultat.Text     = "Résultat de la vérification";
            this.grpResultat.TabStop  = false;

            // lblResultat
            this.lblResultat.AutoSize  = false;
            this.lblResultat.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResultat.Location  = new System.Drawing.Point(10, 28);
            this.lblResultat.Size      = new System.Drawing.Size(720, 24);
            this.lblResultat.Text      = "";

            // dgvManques
            this.dgvManques.AllowUserToAddRows    = false;
            this.dgvManques.AllowUserToDeleteRows = false;
            this.dgvManques.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvManques.MultiSelect           = false;
            this.dgvManques.ReadOnly              = true;
            this.dgvManques.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvManques.RowHeadersVisible     = false;
            this.dgvManques.BackgroundColor       = System.Drawing.Color.White;
            this.dgvManques.Location              = new System.Drawing.Point(10, 58);
            this.dgvManques.Size                  = new System.Drawing.Size(725, 180);
            this.dgvManques.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);

            this.grpResultat.Controls.Add(this.lblResultat);
            this.grpResultat.Controls.Add(this.dgvManques);

            // btnExécuter
            this.btnExécuter.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnExécuter.Location  = new System.Drawing.Point(12, 528);
            this.btnExécuter.Size      = new System.Drawing.Size(220, 40);
            this.btnExécuter.Text      = "Exécuter la production";
            this.btnExécuter.BackColor = System.Drawing.Color.FromArgb(27, 94, 32);
            this.btnExécuter.ForeColor = System.Drawing.Color.White;
            this.btnExécuter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExécuter.Enabled   = false;
            this.btnExécuter.Click    += new System.EventHandler(this.btnExécuter_Click);

            // btnFermer
            this.btnFermer.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFermer.Location = new System.Drawing.Point(644, 528);
            this.btnFermer.Size     = new System.Drawing.Size(120, 40);
            this.btnFermer.Text     = "Fermer";
            this.btnFermer.Click   += new System.EventHandler(this.btnFermer_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(780, 586);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.grpSélection);
            this.Controls.Add(this.grpResultat);
            this.Controls.Add(this.btnExécuter);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmBomProduction";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Exécution de production";
            this.Load           += new System.EventHandler(this.FrmBomProduction_Load);

            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManques)).EndInit();
            this.grpSélection.ResumeLayout(false);
            this.grpSélection.PerformLayout();
            this.grpResultat.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label        lblTitre;
        private System.Windows.Forms.GroupBox     grpSélection;
        private System.Windows.Forms.Label        lblContexte;
        private System.Windows.Forms.ComboBox     cboContexte;
        private System.Windows.Forms.Label        lblNiveau;
        private System.Windows.Forms.ComboBox     cboNiveau;
        private System.Windows.Forms.Label        lblFiche;
        private System.Windows.Forms.ComboBox     cboFiche;
        private System.Windows.Forms.Label        lblQuantite;
        private System.Windows.Forms.NumericUpDown nudQuantite;
        private System.Windows.Forms.Label        lblNotes;
        private System.Windows.Forms.TextBox      txtNotes;
        private System.Windows.Forms.Button       btnVérifier;
        private System.Windows.Forms.GroupBox     grpResultat;
        private System.Windows.Forms.Label        lblResultat;
        private System.Windows.Forms.DataGridView dgvManques;
        private System.Windows.Forms.Button       btnExécuter;
        private System.Windows.Forms.Button       btnFermer;
    }
}
