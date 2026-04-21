namespace CharlesNadejda.Forms
{
    partial class FrmBomFicheEdit
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components        = new System.ComponentModel.Container();
            this.errorProvider     = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblNom            = new System.Windows.Forms.Label();
            this.txtNom            = new System.Windows.Forms.TextBox();
            this.lblActivite        = new System.Windows.Forms.Label();
            this.lblActiviteValeur  = new System.Windows.Forms.Label();
            this.lblUniteOutput    = new System.Windows.Forms.Label();
            this.cboUniteOutput    = new System.Windows.Forms.ComboBox();
            this.lblQuantiteOutput = new System.Windows.Forms.Label();
            this.nudQuantiteOutput = new System.Windows.Forms.NumericUpDown();
            this.lblTemps          = new System.Windows.Forms.Label();
            this.nudTemps          = new System.Windows.Forms.NumericUpDown();
            this.lblDescription    = new System.Windows.Forms.Label();
            this.txtDescription    = new System.Windows.Forms.TextBox();
            this.grpLignes         = new System.Windows.Forms.GroupBox();
            this.lblTypeInput      = new System.Windows.Forms.Label();
            this.cboTypeInput      = new System.Windows.Forms.ComboBox();
            this.lblInput          = new System.Windows.Forms.Label();
            this.cboInput          = new System.Windows.Forms.ComboBox();
            this.lblQteLigne       = new System.Windows.Forms.Label();
            this.nudQteLigne       = new System.Windows.Forms.NumericUpDown();
            this.lblUniteLigne     = new System.Windows.Forms.Label();
            this.cboUniteLigne     = new System.Windows.Forms.ComboBox();
            this.btnAjouterLigne   = new System.Windows.Forms.Button();
            this.btnRetirerLigne   = new System.Windows.Forms.Button();
            this.dgvLignes         = new System.Windows.Forms.DataGridView();
            this.btnEnregistrer    = new System.Windows.Forms.Button();
            this.btnAnnuler        = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantiteOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTemps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQteLigne)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLignes)).BeginInit();
            this.grpLignes.SuspendLayout();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            // ── Colonne gauche — en-tête de la fiche ────────────────────

            // lblNom
            this.lblNom.AutoSize = true;
            this.lblNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNom.Location = new System.Drawing.Point(12, 15);
            this.lblNom.Text     = "Nom de la fiche *";

            // txtNom
            this.txtNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNom.Location = new System.Drawing.Point(12, 36);
            this.txtNom.Size     = new System.Drawing.Size(280, 26);
            this.txtNom.TabIndex = 0;

            // lblActivite
            this.lblActivite.AutoSize = true;
            this.lblActivite.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblActivite.Location = new System.Drawing.Point(304, 15);
            this.lblActivite.Text     = "Activité *";

            // lblActiviteValeur — lecture seule, remplace le combobox grisé (TICKET-05)
            this.lblActiviteValeur.AutoSize  = true;
            this.lblActiviteValeur.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblActiviteValeur.Location  = new System.Drawing.Point(304, 36);
            this.lblActiviteValeur.Name      = "lblActiviteValeur";
            this.lblActiviteValeur.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.lblActiviteValeur.Text      = "";

            // lblUniteOutput
            this.lblUniteOutput.AutoSize = true;
            this.lblUniteOutput.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUniteOutput.Location = new System.Drawing.Point(12, 76);
            this.lblUniteOutput.Text     = "Unité produite";

            // cboUniteOutput
            this.cboUniteOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUniteOutput.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cboUniteOutput.Location      = new System.Drawing.Point(12, 97);
            this.cboUniteOutput.Size          = new System.Drawing.Size(100, 26);
            this.cboUniteOutput.TabIndex      = 2;

            // lblQuantiteOutput
            this.lblQuantiteOutput.AutoSize = true;
            this.lblQuantiteOutput.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblQuantiteOutput.Location = new System.Drawing.Point(124, 76);
            this.lblQuantiteOutput.Text     = "Qté produite/exécution";

            // nudQuantiteOutput
            this.nudQuantiteOutput.DecimalPlaces = 2;
            this.nudQuantiteOutput.Minimum       = new decimal(new int[] { 1, 0, 0, 131072 }); // 0.01
            this.nudQuantiteOutput.Maximum       = new decimal(new int[] { 100000, 0, 0, 0 });
            this.nudQuantiteOutput.Value         = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudQuantiteOutput.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.nudQuantiteOutput.Location      = new System.Drawing.Point(124, 97);
            this.nudQuantiteOutput.Size          = new System.Drawing.Size(100, 26);
            this.nudQuantiteOutput.TabIndex      = 3;

            // lblTemps
            this.lblTemps.AutoSize = true;
            this.lblTemps.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblTemps.Location = new System.Drawing.Point(236, 76);
            this.lblTemps.Text     = "Temps (min)";

            // nudTemps
            this.nudTemps.Maximum  = new decimal(new int[] { 9999, 0, 0, 0 });
            this.nudTemps.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.nudTemps.Location = new System.Drawing.Point(236, 97);
            this.nudTemps.Size     = new System.Drawing.Size(80, 26);
            this.nudTemps.TabIndex = 4;

            // lblDescription
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescription.Location = new System.Drawing.Point(12, 137);
            this.lblDescription.Text     = "Description";

            // txtDescription
            this.txtDescription.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location  = new System.Drawing.Point(12, 158);
            this.txtDescription.Multiline = true;
            this.txtDescription.Size      = new System.Drawing.Size(432, 50);
            this.txtDescription.TabIndex  = 5;

            // ── GroupBox Lignes ──────────────────────────────────────────

            this.grpLignes.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpLignes.Location = new System.Drawing.Point(12, 220);
            this.grpLignes.Size     = new System.Drawing.Size(770, 300);
            this.grpLignes.Text     = "Composition — inputs de la fiche";
            this.grpLignes.TabStop  = false;

            // lblTypeInput
            this.lblTypeInput.AutoSize = true;
            this.lblTypeInput.Font     = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTypeInput.Location = new System.Drawing.Point(10, 28);
            this.lblTypeInput.Text     = "Type";

            // cboTypeInput
            this.cboTypeInput.DropDownStyle          = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTypeInput.Font                   = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboTypeInput.Location               = new System.Drawing.Point(10, 47);
            this.cboTypeInput.Size                   = new System.Drawing.Size(120, 24);
            this.cboTypeInput.TabIndex               = 6;
            // cboTypeInput_SelectedIndexChanged supprimé — le type d'input est auto-déterminé par le niveau

            // lblInput
            this.lblInput.AutoSize = true;
            this.lblInput.Font     = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblInput.Location = new System.Drawing.Point(140, 28);
            this.lblInput.Text     = "Input";

            // cboInput
            this.cboInput.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInput.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboInput.Location              = new System.Drawing.Point(140, 47);
            this.cboInput.Size                  = new System.Drawing.Size(220, 24);
            this.cboInput.TabIndex              = 7;
            this.cboInput.SelectedIndexChanged += new System.EventHandler(this.cboInput_SelectedIndexChanged);

            // lblQteLigne
            this.lblQteLigne.AutoSize = true;
            this.lblQteLigne.Font     = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblQteLigne.Location = new System.Drawing.Point(370, 28);
            this.lblQteLigne.Text     = "Quantité";

            // nudQteLigne
            this.nudQteLigne.DecimalPlaces = 3;
            this.nudQteLigne.Minimum       = new decimal(new int[] { 1, 0, 0, 196608 }); // 0.001
            this.nudQteLigne.Maximum       = new decimal(new int[] { 100000, 0, 0, 0 });
            this.nudQteLigne.Value         = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudQteLigne.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.nudQteLigne.Location      = new System.Drawing.Point(370, 47);
            this.nudQteLigne.Size          = new System.Drawing.Size(90, 24);
            this.nudQteLigne.TabIndex      = 8;

            // lblUniteLigne
            this.lblUniteLigne.AutoSize = true;
            this.lblUniteLigne.Font     = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblUniteLigne.Location = new System.Drawing.Point(470, 28);
            this.lblUniteLigne.Text     = "Unité";

            // cboUniteLigne
            this.cboUniteLigne.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUniteLigne.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboUniteLigne.Location      = new System.Drawing.Point(470, 47);
            this.cboUniteLigne.Size          = new System.Drawing.Size(80, 24);
            this.cboUniteLigne.Items.AddRange(new object[] { "piece", "kg", "g", "l", "ml", "cl" });
            this.cboUniteLigne.SelectedIndex = 0;
            this.cboUniteLigne.TabIndex      = 9;

            // btnAjouterLigne
            this.btnAjouterLigne.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAjouterLigne.Location  = new System.Drawing.Point(560, 44);
            this.btnAjouterLigne.Size      = new System.Drawing.Size(90, 28);
            this.btnAjouterLigne.Text      = "+ Ajouter";
            this.btnAjouterLigne.TabIndex  = 10;
            this.btnAjouterLigne.BackColor = System.Drawing.Color.FromArgb(40, 120, 40);
            this.btnAjouterLigne.ForeColor = System.Drawing.Color.White;
            this.btnAjouterLigne.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouterLigne.Click    += new System.EventHandler(this.btnAjouterLigne_Click);

            // btnRetirerLigne
            this.btnRetirerLigne.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnRetirerLigne.Location  = new System.Drawing.Point(660, 44);
            this.btnRetirerLigne.Size      = new System.Drawing.Size(90, 28);
            this.btnRetirerLigne.Text      = "Retirer";
            this.btnRetirerLigne.TabIndex  = 11;
            this.btnRetirerLigne.ForeColor = System.Drawing.Color.DarkRed;
            this.btnRetirerLigne.Click    += new System.EventHandler(this.btnRetirerLigne_Click);

            // dgvLignes
            this.dgvLignes.AllowUserToAddRows    = false;
            this.dgvLignes.AllowUserToDeleteRows = false;
            this.dgvLignes.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLignes.MultiSelect           = false;
            this.dgvLignes.ReadOnly              = true;
            this.dgvLignes.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLignes.RowHeadersVisible     = false;
            this.dgvLignes.BackgroundColor       = System.Drawing.Color.White;
            this.dgvLignes.Location              = new System.Drawing.Point(10, 82);
            this.dgvLignes.Size                  = new System.Drawing.Size(745, 200);
            this.dgvLignes.Font                  = new System.Drawing.Font("Segoe UI", 9F);

            this.grpLignes.Controls.Add(this.lblTypeInput);
            this.grpLignes.Controls.Add(this.cboTypeInput);
            this.grpLignes.Controls.Add(this.lblInput);
            this.grpLignes.Controls.Add(this.cboInput);
            this.grpLignes.Controls.Add(this.lblQteLigne);
            this.grpLignes.Controls.Add(this.nudQteLigne);
            this.grpLignes.Controls.Add(this.lblUniteLigne);
            this.grpLignes.Controls.Add(this.cboUniteLigne);
            this.grpLignes.Controls.Add(this.btnAjouterLigne);
            this.grpLignes.Controls.Add(this.btnRetirerLigne);
            this.grpLignes.Controls.Add(this.dgvLignes);

            // btnEnregistrer — palette chocolat
            this.btnEnregistrer.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnregistrer.Location  = new System.Drawing.Point(12, 535);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = 12;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            // btnAnnuler — ton neutre
            this.btnAnnuler.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAnnuler.Location  = new System.Drawing.Point(190, 535);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.TabIndex  = 13;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(800, 590);
            this.Controls.Add(this.lblNom);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.lblActivite);
            this.Controls.Add(this.lblActiviteValeur);
            this.Controls.Add(this.lblUniteOutput);
            this.Controls.Add(this.cboUniteOutput);
            this.Controls.Add(this.lblQuantiteOutput);
            this.Controls.Add(this.nudQuantiteOutput);
            this.Controls.Add(this.lblTemps);
            this.Controls.Add(this.nudTemps);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.grpLignes);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.btnAnnuler);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FrmBomFicheEdit";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load           += new System.EventHandler(this.FrmBomFicheEdit_Load);

            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantiteOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTemps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQteLigne)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLignes)).EndInit();
            this.grpLignes.ResumeLayout(false);
            this.grpLignes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider     errorProvider;
        private System.Windows.Forms.Label             lblNom;
        private System.Windows.Forms.TextBox           txtNom;
        private System.Windows.Forms.Label             lblActivite;
        private System.Windows.Forms.Label             lblActiviteValeur;
        private System.Windows.Forms.Label             lblUniteOutput;
        private System.Windows.Forms.ComboBox          cboUniteOutput;
        private System.Windows.Forms.Label             lblQuantiteOutput;
        private System.Windows.Forms.NumericUpDown     nudQuantiteOutput;
        private System.Windows.Forms.Label             lblTemps;
        private System.Windows.Forms.NumericUpDown     nudTemps;
        private System.Windows.Forms.Label             lblDescription;
        private System.Windows.Forms.TextBox           txtDescription;
        private System.Windows.Forms.GroupBox          grpLignes;
        private System.Windows.Forms.Label             lblTypeInput;
        private System.Windows.Forms.ComboBox          cboTypeInput;
        private System.Windows.Forms.Label             lblInput;
        private System.Windows.Forms.ComboBox          cboInput;
        private System.Windows.Forms.Label             lblQteLigne;
        private System.Windows.Forms.NumericUpDown     nudQteLigne;
        private System.Windows.Forms.Label             lblUniteLigne;
        private System.Windows.Forms.ComboBox          cboUniteLigne;
        private System.Windows.Forms.Button            btnAjouterLigne;
        private System.Windows.Forms.Button            btnRetirerLigne;
        private System.Windows.Forms.DataGridView      dgvLignes;
        private System.Windows.Forms.Button            btnEnregistrer;
        private System.Windows.Forms.Button            btnAnnuler;
    }
}
