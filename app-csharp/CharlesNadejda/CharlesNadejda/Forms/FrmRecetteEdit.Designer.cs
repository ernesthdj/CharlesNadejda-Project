namespace CharlesNadejda.Forms
{
    partial class FrmRecetteEdit
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.components       = new System.ComponentModel.Container();
            this.errorProvider    = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtNom           = new System.Windows.Forms.TextBox();
            this.txtDescription   = new System.Windows.Forms.TextBox();
            this.nudRendement     = new System.Windows.Forms.NumericUpDown();
            this.nudTemps         = new System.Windows.Forms.NumericUpDown();
            this.cmbIngredient    = new System.Windows.Forms.ComboBox();
            this.txtQteIngredient = new System.Windows.Forms.TextBox();
            this.btnAjouterIngredient = new System.Windows.Forms.Button();
            this.dgvIngredients   = new System.Windows.Forms.DataGridView();
            this.lblCout          = new System.Windows.Forms.Label();
            this.btnEnregistrer   = new System.Windows.Forms.Button();
            this.btnAnnuler       = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRendement)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTemps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;
            var font = new System.Drawing.Font("Segoe UI", 10F);
            int lx = 16;

            void Lbl(string t, int x, int y) { var l = new System.Windows.Forms.Label { AutoSize = true, Font = font, Location = new System.Drawing.Point(x, y), Text = t }; this.Controls.Add(l); }

            // ── En-tête recette ──
            Lbl("Nom *", lx, 14);
            this.txtNom.Font = font; this.txtNom.Location = new System.Drawing.Point(lx, 34); this.txtNom.Size = new System.Drawing.Size(420, 26); this.txtNom.TabIndex = 0;

            Lbl("Description", lx, 72);
            this.txtDescription.Font = font; this.txtDescription.Location = new System.Drawing.Point(lx, 92); this.txtDescription.Size = new System.Drawing.Size(420, 48); this.txtDescription.Multiline = true; this.txtDescription.TabIndex = 1;

            Lbl("Rendement (pièces) *", lx, 152);
            this.nudRendement.Font = font; this.nudRendement.Location = new System.Drawing.Point(lx, 172); this.nudRendement.Size = new System.Drawing.Size(90, 26); this.nudRendement.Minimum = 1; this.nudRendement.Maximum = 10000; this.nudRendement.Value = 20; this.nudRendement.TabIndex = 2;
            this.nudRendement.ValueChanged += new System.EventHandler(this.nudRendement_ValueChanged);

            Lbl("Temps prépa (min)", 160, 152);
            this.nudTemps.Font = font; this.nudTemps.Location = new System.Drawing.Point(160, 172); this.nudTemps.Size = new System.Drawing.Size(80, 26); this.nudTemps.Maximum = 9999; this.nudTemps.TabIndex = 3;

            // ── Zone ajout ingrédient ──
            var sep = new System.Windows.Forms.Label { AutoSize = false, BackColor = System.Drawing.Color.Silver, Location = new System.Drawing.Point(lx, 212), Size = new System.Drawing.Size(580, 1) };
            this.Controls.Add(sep);
            Lbl("Ajouter un ingrédient :", lx, 220);

            Lbl("Ingrédient", lx, 242);
            this.cmbIngredient.Font = font; this.cmbIngredient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIngredient.Location = new System.Drawing.Point(lx, 262); this.cmbIngredient.Size = new System.Drawing.Size(280, 26); this.cmbIngredient.TabIndex = 4;

            Lbl("Quantité", 310, 242);
            this.txtQteIngredient.Font = font; this.txtQteIngredient.Location = new System.Drawing.Point(310, 262); this.txtQteIngredient.Size = new System.Drawing.Size(90, 26); this.txtQteIngredient.TabIndex = 5;

            this.btnAjouterIngredient.Font = font; this.btnAjouterIngredient.Location = new System.Drawing.Point(414, 260);
            this.btnAjouterIngredient.Size = new System.Drawing.Size(80, 30); this.btnAjouterIngredient.Text = "+ Ajouter"; this.btnAjouterIngredient.TabIndex = 6;
            this.btnAjouterIngredient.Click += new System.EventHandler(this.btnAjouterIngredient_Click);

            // ── Grille ingrédients ──
            Lbl("Ingrédients de la recette (Suppr = retirer la ligne sélectionnée) :", lx, 302);
            this.dgvIngredients.AllowUserToAddRows = false; this.dgvIngredients.AllowUserToDeleteRows = false;
            this.dgvIngredients.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvIngredients.MultiSelect = false; this.dgvIngredients.ReadOnly = true;
            this.dgvIngredients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIngredients.Location = new System.Drawing.Point(lx, 322); this.dgvIngredients.Size = new System.Drawing.Size(580, 160);
            this.dgvIngredients.Font = new System.Drawing.Font("Segoe UI", 9.5F); this.dgvIngredients.RowHeadersVisible = false;
            this.dgvIngredients.BackgroundColor = System.Drawing.Color.White; this.dgvIngredients.TabIndex = 7;
            this.dgvIngredients.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvIngredients_KeyDown);

            // ── Coût calculé ──
            this.lblCout.AutoSize = false; this.lblCout.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCout.Location = new System.Drawing.Point(lx, 492); this.lblCout.Size = new System.Drawing.Size(580, 24);
            this.lblCout.Text = "Coût batch : — €   |   Coût/pièce : — €";

            // ── Boutons — palette chocolat ──
            this.btnEnregistrer.Font      = font;
            this.btnEnregistrer.Location  = new System.Drawing.Point(lx, 526);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = 8;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            this.btnAnnuler.Font      = font;
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 526);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.TabIndex  = 9;
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.txtNom, this.txtDescription, this.nudRendement, this.nudTemps,
                this.cmbIngredient, this.txtQteIngredient, this.btnAjouterIngredient,
                this.dgvIngredients, this.lblCout, this.btnEnregistrer, this.btnAnnuler
            });

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F); this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 578); this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.Name = "FrmRecetteEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FrmRecetteEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRendement)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTemps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider    errorProvider;
        private System.Windows.Forms.TextBox          txtNom, txtDescription, txtQteIngredient;
        private System.Windows.Forms.NumericUpDown    nudRendement, nudTemps;
        private System.Windows.Forms.ComboBox         cmbIngredient;
        private System.Windows.Forms.Button           btnAjouterIngredient, btnEnregistrer, btnAnnuler;
        private System.Windows.Forms.DataGridView     dgvIngredients;
        private System.Windows.Forms.Label            lblCout;
    }
}
