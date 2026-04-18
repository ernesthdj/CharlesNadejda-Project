namespace CharlesNadejda.Forms
{
    partial class FrmParfumEdit
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.components     = new System.ComponentModel.Container();
            this.errorProvider  = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtNom         = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtType        = new System.Windows.Forms.TextBox();
            this.txtCouleur     = new System.Windows.Forms.TextBox();
            this.picCouleur     = new System.Windows.Forms.PictureBox();
            this.cmbRecette     = new System.Windows.Forms.ComboBox();
            this.chkDisponible  = new System.Windows.Forms.CheckBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler     = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCouleur)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;
            var font = new System.Drawing.Font("Segoe UI", 10F);
            int lx = 20, w = 340, tab = 0;

            void Field(string label, System.Windows.Forms.Control ctrl, int y, int h = 26)
            {
                var lbl = new System.Windows.Forms.Label { AutoSize = true, Font = font, Location = new System.Drawing.Point(lx, y), Text = label };
                ctrl.Font = font; ctrl.Location = new System.Drawing.Point(lx, y + 22); ctrl.Size = new System.Drawing.Size(w, h); ctrl.TabIndex = tab++;
                this.Controls.Add(lbl); this.Controls.Add(ctrl);
            }

            Field("Nom *",        txtNom,         20);
            Field("Description",  txtDescription, 65);
            Field("Type",         txtType,        110);

            // Couleur + aperçu
            var lblCouleur = new System.Windows.Forms.Label { AutoSize = true, Font = font, Location = new System.Drawing.Point(lx, 155), Text = "Couleur hex (ex: #6F4E37)" };
            this.txtCouleur.Font = font; this.txtCouleur.Location = new System.Drawing.Point(lx, 177); this.txtCouleur.Size = new System.Drawing.Size(200, 26); this.txtCouleur.TabIndex = tab++; this.txtCouleur.Text = "#6F4E37";
            this.txtCouleur.Leave += new System.EventHandler(this.txtCouleur_Leave);
            this.picCouleur.Location = new System.Drawing.Point(230, 175); this.picCouleur.Size = new System.Drawing.Size(40, 28); this.picCouleur.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; this.picCouleur.BackColor = System.Drawing.ColorTranslator.FromHtml("#6F4E37");
            this.Controls.Add(lblCouleur); this.Controls.Add(this.txtCouleur); this.Controls.Add(this.picCouleur);

            Field("Recette liée", cmbRecette, 215);
            this.cmbRecette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.chkDisponible.AutoSize = true; this.chkDisponible.Font = font; this.chkDisponible.Location = new System.Drawing.Point(lx, 265); this.chkDisponible.Text = "Disponible"; this.chkDisponible.Checked = true; this.chkDisponible.TabIndex = tab++;
            this.Controls.Add(this.chkDisponible);

            this.btnEnregistrer.Font      = font;
            this.btnEnregistrer.Location  = new System.Drawing.Point(lx, 300);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = tab++;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            this.btnAnnuler.Font      = font;
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 300);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.TabIndex  = tab++;
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            this.Controls.Add(this.btnEnregistrer); this.Controls.Add(this.btnAnnuler);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F); this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 352); this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.Name = "FrmParfumEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FrmParfumEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCouleur)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider  errorProvider;
        private System.Windows.Forms.TextBox        txtNom, txtDescription, txtType, txtCouleur;
        private System.Windows.Forms.PictureBox     picCouleur;
        private System.Windows.Forms.ComboBox       cmbRecette;
        private System.Windows.Forms.CheckBox       chkDisponible;
        private System.Windows.Forms.Button         btnEnregistrer, btnAnnuler;
    }
}
