namespace CharlesNadejda.Forms
{
    partial class FrmIngredientEdit
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.components            = new System.ComponentModel.Container();
            this.errorProvider         = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtNom                = new System.Windows.Forms.TextBox();
            this.txtMarque             = new System.Windows.Forms.TextBox();
            this.cmbUnite              = new System.Windows.Forms.ComboBox();
            this.cmbTypePhysique       = new System.Windows.Forms.ComboBox();
            this.nudDensite            = new System.Windows.Forms.NumericUpDown();
            this.lblDensite            = new System.Windows.Forms.Label();
            this.nudPrix               = new System.Windows.Forms.NumericUpDown();
            this.nudQteConditionnement = new System.Windows.Forms.NumericUpDown();
            this.lblUniteQteCond       = new System.Windows.Forms.Label();
            this.txtSeuil              = new System.Windows.Forms.TextBox();
            this.cmbFournisseur        = new System.Windows.Forms.ComboBox();
            this.cmbStock              = new System.Windows.Forms.ComboBox();
            this.btnEnregistrer        = new System.Windows.Forms.Button();
            this.btnAnnuler            = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQteConditionnement)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            var font  = new System.Drawing.Font("Segoe UI", 10F);
            var fontS = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            int lx = 20, lx2 = 215, wL = 175, wR = 170;
            int tab = 0;

            // Helper : ajoute label + contrôle dans la même colonne
            void Field(string label, System.Windows.Forms.Control ctrl, int x, int y, int w, int h = 26)
            {
                var lbl = new System.Windows.Forms.Label
                {
                    AutoSize = true, Font = font,
                    Location = new System.Drawing.Point(x, y), Text = label
                };
                ctrl.Font     = font;
                ctrl.Location = new System.Drawing.Point(x, y + 22);
                ctrl.Size     = new System.Drawing.Size(w, h);
                if (ctrl is System.Windows.Forms.TextBox || ctrl is System.Windows.Forms.ComboBox ||
                    ctrl is System.Windows.Forms.NumericUpDown)
                    ctrl.TabIndex = tab++;
                this.Controls.Add(lbl);
                this.Controls.Add(ctrl);
            }

            // ── Ligne 1 : Nom (pleine largeur) ──────────────────────────
            Field("Nom *",  txtNom, lx, 20, 360);

            // ── Ligne 2 : Marque (pleine largeur) ───────────────────────
            Field("Marque", txtMarque, lx, 68, 360);

            // ── Ligne 3 : Unité de base | Type Physique ─────────────────
            Field("Unité de base *",    cmbUnite,        lx,  116, wL);
            Field("Type physique *",    cmbTypePhysique, lx2, 116, wR);

            // ── Ligne 4 : Prix réf. | Densité ───────────────────────────
            Field("Prix réf. (€/conditionnement)", nudPrix, lx, 164, wL, 26);

            // Densité — label + contrôle gérés manuellement (show/hide)
            this.lblDensite.AutoSize = true;
            this.lblDensite.Font     = font;
            this.lblDensite.Location = new System.Drawing.Point(lx2, 164);
            this.lblDensite.Text     = "Densité (g/ml) *";
            this.nudDensite.Font     = font;
            this.nudDensite.Location = new System.Drawing.Point(lx2, 186);
            this.nudDensite.Size     = new System.Drawing.Size(wR, 26);
            this.nudDensite.TabIndex = tab++;
            this.Controls.Add(this.lblDensite);
            this.Controls.Add(this.nudDensite);

            // ── Ligne 5 : Conditionnement (quantité entière + unité dynamique) ──
            var lblQteCond = new System.Windows.Forms.Label
            {
                AutoSize = true, Font = font,
                Location = new System.Drawing.Point(lx, 214),
                Text     = "Conditionnement *"
            };
            this.nudQteConditionnement.Font     = font;
            this.nudQteConditionnement.Location = new System.Drawing.Point(lx, 236);
            this.nudQteConditionnement.Size     = new System.Drawing.Size(110, 26);
            this.nudQteConditionnement.TabIndex = tab++;

            this.lblUniteQteCond.AutoSize  = true;
            this.lblUniteQteCond.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUniteQteCond.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblUniteQteCond.Location  = new System.Drawing.Point(lx + 115, 240);
            this.lblUniteQteCond.Text      = "g";
            this.Controls.Add(lblQteCond);
            this.Controls.Add(this.nudQteConditionnement);
            this.Controls.Add(this.lblUniteQteCond);

            // ── Ligne 6 : Seuil alerte (pleine largeur) ─────────────────
            Field("Seuil alerte stock", txtSeuil, lx, 262, 175);

            // ── Ligne 7 : Fournisseur (pleine largeur) ───────────────────
            Field("Fournisseur par défaut", cmbFournisseur, lx, 310, 360);

            // ── Ligne 8 : Stock (pleine largeur) ─────────────────────────
            Field("Stock *", cmbStock, lx, 358, 360);

            // ── ComboBox styles ──────────────────────────────────────────
            this.cmbUnite.DropDownStyle        = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypePhysique.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFournisseur.DropDownStyle  = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStock.DropDownStyle        = System.Windows.Forms.ComboBoxStyle.DropDownList;

            // ── NumericUpDown : Prix ─────────────────────────────────────
            this.nudPrix.DecimalPlaces = 4;
            this.nudPrix.Minimum       = 0;
            this.nudPrix.Maximum       = new decimal(new int[] { 999999, 0, 0, 0 });
            this.nudPrix.Value         = 0;

            // ── NumericUpDown : Densité ──────────────────────────────────
            this.nudDensite.DecimalPlaces = 4;
            this.nudDensite.Minimum       = new decimal(new int[] { 1, 0, 0, 196608 }); // 0.001
            this.nudDensite.Maximum       = new decimal(new int[] { 99, 0, 0, 0 });
            this.nudDensite.Value         = new decimal(new int[] { 1, 0, 0, 0 });      // 1.0000

            // Note d'aide densité
            var lblNote = new System.Windows.Forms.Label
            {
                AutoSize  = true,
                Font      = fontS,
                ForeColor = System.Drawing.Color.Gray,
                Location  = new System.Drawing.Point(lx2, 214),
                Text      = "Obligatoire si liquide ou poudre"
            };
            this.Controls.Add(lblNote);

            // ── Boutons ──────────────────────────────────────────────────
            this.btnEnregistrer.Font      = font;
            this.btnEnregistrer.Location  = new System.Drawing.Point(lx, 414);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.TabIndex  = tab++;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            this.btnAnnuler.Font      = font;
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 414);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.TabIndex  = tab++;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.btnAnnuler);

            // ── nudQteConditionnement ────────────────────────────────────
            this.nudQteConditionnement.DecimalPlaces = 0;
            this.nudQteConditionnement.Minimum       = 1m;
            this.nudQteConditionnement.Maximum       = new decimal(new int[] { 999999999, 0, 0, 0 });
            this.nudQteConditionnement.Increment     = 1m;
            this.nudQteConditionnement.Value         = 1m;

            // ── ComboBox cmbUnite : unités de base uniquement ────────────
            // (remplace les anciennes options kg/g/l/ml/cl/piece)

            // ── Formulaire ───────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(415, 468);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "FrmIngredientEdit";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load               += new System.EventHandler(this.FrmIngredientEdit_Load);

            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQteConditionnement)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider  errorProvider;
        private System.Windows.Forms.TextBox        txtNom, txtMarque, txtSeuil;
        private System.Windows.Forms.ComboBox       cmbUnite, cmbTypePhysique, cmbFournisseur, cmbStock;
        private System.Windows.Forms.NumericUpDown  nudDensite, nudPrix, nudQteConditionnement;
        private System.Windows.Forms.Label          lblDensite, lblUniteQteCond;
        private System.Windows.Forms.Button         btnEnregistrer, btnAnnuler;
    }
}
