namespace CharlesNadejda.Forms
{
    partial class FrmBomProductionSimulation
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre               = new System.Windows.Forms.Label();
            this.grpSélection           = new System.Windows.Forms.GroupBox();
            this.lblContexte            = new System.Windows.Forms.Label();
            this.cboContexte            = new System.Windows.Forms.ComboBox();
            this.lblNiveau              = new System.Windows.Forms.Label();
            this.cboNiveau              = new System.Windows.Forms.ComboBox();
            this.lblFiche               = new System.Windows.Forms.Label();
            this.cboFiche               = new System.Windows.Forms.ComboBox();
            this.lblQuantite            = new System.Windows.Forms.Label();
            this.nudQuantite            = new System.Windows.Forms.NumericUpDown();
            this.lblInfoBatch           = new System.Windows.Forms.Label();
            this.btnSimuler             = new System.Windows.Forms.Button();
            this.lblDelaiConservation   = new System.Windows.Forms.Label();
            this.nudDelaiConservation   = new System.Windows.Forms.NumericUpDown();
            this.lblNotes               = new System.Windows.Forms.Label();
            this.txtNotes               = new System.Windows.Forms.TextBox();
            this.grpResultat        = new System.Windows.Forms.GroupBox();
            this.lblResultat        = new System.Windows.Forms.Label();
            this.lblCoutEstime      = new System.Windows.Forms.Label();
            this.dgvSimulation      = new System.Windows.Forms.DataGridView();
            this.lblLegende         = new System.Windows.Forms.Label();
            this.btnLancerProduction = new System.Windows.Forms.Button();
            this.btnFermer          = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelaiConservation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimulation)).BeginInit();
            this.grpSélection.SuspendLayout();
            this.grpResultat.SuspendLayout();
            this.SuspendLayout();

            // lblTitre — palette chocolat
            this.lblTitre.AutoSize  = false;
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.lblTitre.Location  = new System.Drawing.Point(12, 12);
            this.lblTitre.Size      = new System.Drawing.Size(700, 30);
            this.lblTitre.Text      = "Production BOM";

            // ── grpSélection ─────────────────────────────────────────────

            this.grpSélection.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpSélection.Location = new System.Drawing.Point(12, 52);
            this.grpSélection.Size     = new System.Drawing.Size(750, 210);
            this.grpSélection.Text     = "Paramètres";
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
            this.lblQuantite.Text     = "Nombre de batches";

            // nudQuantite
            this.nudQuantite.DecimalPlaces = 2;
            this.nudQuantite.Minimum       = new decimal(new int[] { 1, 0, 0, 131072 }); // 0.01
            this.nudQuantite.Maximum       = new decimal(new int[] { 100000, 0, 0, 0 });
            this.nudQuantite.Value         = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudQuantite.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.nudQuantite.Location      = new System.Drawing.Point(10, 108);
            this.nudQuantite.Size          = new System.Drawing.Size(120, 26);
            this.nudQuantite.TabIndex      = 3;

            // lblInfoBatch — affiche "1 batch = X [unité]" dynamiquement selon la fiche sélectionnée
            this.lblInfoBatch.AutoSize  = false;
            this.lblInfoBatch.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblInfoBatch.ForeColor = System.Drawing.Color.FromArgb(111, 78, 55);
            this.lblInfoBatch.Location  = new System.Drawing.Point(10, 136);
            this.lblInfoBatch.Size      = new System.Drawing.Size(400, 16);
            this.lblInfoBatch.Text      = "";

            // btnSimuler
            this.btnSimuler.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSimuler.Location  = new System.Drawing.Point(145, 105);
            this.btnSimuler.Size      = new System.Drawing.Size(200, 32);
            this.btnSimuler.Text      = "Simuler la production";
            this.btnSimuler.TabIndex  = 4;
            this.btnSimuler.BackColor = System.Drawing.Color.FromArgb(63, 81, 181);
            this.btnSimuler.ForeColor = System.Drawing.Color.White;
            this.btnSimuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSimuler.Click    += new System.EventHandler(this.btnSimuler_Click);

            // lblDelaiConservation
            this.lblDelaiConservation.AutoSize = true;
            this.lblDelaiConservation.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDelaiConservation.Location = new System.Drawing.Point(370, 88);
            this.lblDelaiConservation.Text     = "Délai conservation (jours)";

            // nudDelaiConservation — 0 = pas de DLC
            this.nudDelaiConservation.DecimalPlaces = 0;
            this.nudDelaiConservation.Minimum       = 0;
            this.nudDelaiConservation.Maximum       = 3650;
            this.nudDelaiConservation.Value         = 0;
            this.nudDelaiConservation.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.nudDelaiConservation.Location      = new System.Drawing.Point(370, 108);
            this.nudDelaiConservation.Size          = new System.Drawing.Size(100, 26);
            this.nudDelaiConservation.TabIndex      = 4;

            // lblNotes
            this.lblNotes.AutoSize = true;
            this.lblNotes.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNotes.Location = new System.Drawing.Point(10, 152);
            this.lblNotes.Text     = "Notes de production (optionnel)";

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
            this.grpSélection.Controls.Add(this.lblInfoBatch);
            this.grpSélection.Controls.Add(this.btnSimuler);
            this.grpSélection.Controls.Add(this.lblDelaiConservation);
            this.grpSélection.Controls.Add(this.nudDelaiConservation);
            this.grpSélection.Controls.Add(this.lblNotes);
            this.grpSélection.Controls.Add(this.txtNotes);

            // ── grpResultat ───────────────────────────────────────────────

            this.grpResultat.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpResultat.Location = new System.Drawing.Point(12, 274);
            this.grpResultat.Size     = new System.Drawing.Size(750, 340);
            this.grpResultat.Text     = "Résultat de la simulation";
            this.grpResultat.TabStop  = false;

            // lblResultat
            this.lblResultat.AutoSize  = false;
            this.lblResultat.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResultat.Location  = new System.Drawing.Point(10, 28);
            this.lblResultat.Size      = new System.Drawing.Size(720, 20);
            this.lblResultat.Text      = "";

            // lblCoutEstime — coût estimé, affiché après une simulation réussie
            this.lblCoutEstime.AutoSize  = false;
            this.lblCoutEstime.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblCoutEstime.ForeColor = System.Drawing.Color.FromArgb(60, 100, 60);
            this.lblCoutEstime.Location  = new System.Drawing.Point(10, 50);
            this.lblCoutEstime.Size      = new System.Drawing.Size(720, 16);
            this.lblCoutEstime.Text      = "";

            // dgvSimulation — palette artisanale, header CREME, sélection chocolat moyen
            this.dgvSimulation.AllowUserToAddRows    = false;
            this.dgvSimulation.AllowUserToDeleteRows = false;
            this.dgvSimulation.AllowUserToResizeRows = false;
            this.dgvSimulation.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSimulation.MultiSelect           = false;
            this.dgvSimulation.ReadOnly              = true;
            this.dgvSimulation.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSimulation.RowHeadersVisible     = false;
            this.dgvSimulation.BackgroundColor       = System.Drawing.Color.White;
            this.dgvSimulation.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgvSimulation.GridColor             = System.Drawing.Color.FromArgb(230, 220, 210);
            this.dgvSimulation.ColumnHeadersHeight   = 32;
            this.dgvSimulation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSimulation.ColumnHeadersDefaultCellStyle.BackColor          = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgvSimulation.ColumnHeadersDefaultCellStyle.ForeColor          = System.Drawing.Color.FromArgb(61, 40, 23);
            this.dgvSimulation.ColumnHeadersDefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgvSimulation.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgvSimulation.DefaultCellStyle.SelectionBackColor              = System.Drawing.Color.FromArgb(111, 78, 55);
            this.dgvSimulation.DefaultCellStyle.SelectionForeColor              = System.Drawing.Color.White;
            this.dgvSimulation.AlternatingRowsDefaultCellStyle.BackColor        = System.Drawing.Color.FromArgb(250, 247, 244);
            this.dgvSimulation.Location              = new System.Drawing.Point(10, 70);
            this.dgvSimulation.Size                  = new System.Drawing.Size(725, 244);
            this.dgvSimulation.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);

            // lblLegende (dans le groupe résultat)
            this.lblLegende.AutoSize  = false;
            this.lblLegende.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLegende.ForeColor = System.Drawing.Color.Gray;
            this.lblLegende.Location  = new System.Drawing.Point(10, 322);
            this.lblLegende.Size      = new System.Drawing.Size(600, 16);
            this.lblLegende.Text      = "Vert = stock suffisant    Rouge = stock insuffisant (production bloquée)";

            this.grpResultat.Controls.Add(this.lblResultat);
            this.grpResultat.Controls.Add(this.lblCoutEstime);
            this.grpResultat.Controls.Add(this.dgvSimulation);
            this.grpResultat.Controls.Add(this.lblLegende);

            // btnLancerProduction
            this.btnLancerProduction.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnLancerProduction.Location  = new System.Drawing.Point(12, 628);
            this.btnLancerProduction.Size      = new System.Drawing.Size(240, 40);
            this.btnLancerProduction.Text      = "Lancer la production";
            this.btnLancerProduction.BackColor = System.Drawing.Color.FromArgb(27, 94, 32);
            this.btnLancerProduction.ForeColor = System.Drawing.Color.White;
            this.btnLancerProduction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLancerProduction.Enabled   = false;
            this.btnLancerProduction.Click    += new System.EventHandler(this.btnLancerProduction_Click);

            // btnFermer
            this.btnFermer.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFermer.Location = new System.Drawing.Point(644, 628);
            this.btnFermer.Size     = new System.Drawing.Size(120, 40);
            this.btnFermer.Text     = "Fermer";
            this.btnFermer.Click   += new System.EventHandler(this.btnFermer_Click);

            // Form — fond blanc, palette cohérente
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(780, 680);
            this.BackColor           = System.Drawing.Color.White;
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.grpSélection);
            this.Controls.Add(this.grpResultat);
            this.Controls.Add(this.btnLancerProduction);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmBomProductionSimulation";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Production BOM";
            this.Load           += new System.EventHandler(this.FrmBomProductionSimulation_Load);

            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelaiConservation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSimulation)).EndInit();
            this.grpSélection.ResumeLayout(false);
            this.grpSélection.PerformLayout();
            this.grpResultat.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label         lblTitre;
        private System.Windows.Forms.GroupBox      grpSélection;
        private System.Windows.Forms.Label         lblContexte;
        private System.Windows.Forms.ComboBox      cboContexte;
        private System.Windows.Forms.Label         lblNiveau;
        private System.Windows.Forms.ComboBox      cboNiveau;
        private System.Windows.Forms.Label         lblFiche;
        private System.Windows.Forms.ComboBox      cboFiche;
        private System.Windows.Forms.Label         lblQuantite;
        private System.Windows.Forms.NumericUpDown nudQuantite;
        private System.Windows.Forms.Label         lblInfoBatch;
        private System.Windows.Forms.Button        btnSimuler;
        private System.Windows.Forms.Label         lblDelaiConservation;
        private System.Windows.Forms.NumericUpDown nudDelaiConservation;
        private System.Windows.Forms.Label         lblNotes;
        private System.Windows.Forms.TextBox       txtNotes;
        private System.Windows.Forms.GroupBox      grpResultat;
        private System.Windows.Forms.Label         lblResultat;
        private System.Windows.Forms.Label         lblCoutEstime;
        private System.Windows.Forms.DataGridView  dgvSimulation;
        private System.Windows.Forms.Label         lblLegende;
        private System.Windows.Forms.Button        btnLancerProduction;
        private System.Windows.Forms.Button        btnFermer;
    }
}
