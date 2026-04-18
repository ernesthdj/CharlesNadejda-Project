namespace CharlesNadejda.Forms
{
    partial class FrmBomContexteEdit
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components     = new System.ComponentModel.Container();
            this.errorProvider  = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblNom         = new System.Windows.Forms.Label();
            this.txtNom         = new System.Windows.Forms.TextBox();
            this.lblActivite    = new System.Windows.Forms.Label();
            this.cboActivite    = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler     = new System.Windows.Forms.Button();
            // pnlNiveaux est ajouté programmatiquement en mode création uniquement
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            // lblNom
            this.lblNom.AutoSize = true;
            this.lblNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNom.Location = new System.Drawing.Point(20, 20);
            this.lblNom.Text     = "Nom du contexte *";

            // txtNom
            this.txtNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNom.Location = new System.Drawing.Point(20, 42);
            this.txtNom.Size     = new System.Drawing.Size(340, 26);
            this.txtNom.TabIndex = 0;

            // lblActivite
            this.lblActivite.AutoSize = true;
            this.lblActivite.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblActivite.Location = new System.Drawing.Point(20, 82);
            this.lblActivite.Text     = "Activité *";

            // cboActivite
            this.cboActivite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboActivite.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cboActivite.Location      = new System.Drawing.Point(20, 104);
            this.cboActivite.Size          = new System.Drawing.Size(200, 26);
            this.cboActivite.TabIndex      = 1;

            // lblDescription
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescription.Location = new System.Drawing.Point(20, 144);
            this.lblDescription.Text     = "Description";

            // txtDescription
            this.txtDescription.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location  = new System.Drawing.Point(20, 166);
            this.txtDescription.Multiline = true;
            this.txtDescription.Size      = new System.Drawing.Size(340, 60);
            this.txtDescription.TabIndex  = 2;

            // btnEnregistrer — palette chocolat
            this.btnEnregistrer.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnregistrer.Location  = new System.Drawing.Point(20, 244);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = 10;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            // btnAnnuler — ton neutre
            this.btnAnnuler.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 244);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.TabIndex  = 11;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(390, 300);
            this.Controls.Add(this.lblNom);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.lblActivite);
            this.Controls.Add(this.cboActivite);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.btnAnnuler);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FrmBomContexteEdit";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load           += new System.EventHandler(this.FrmBomContexteEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label         lblNom;
        private System.Windows.Forms.TextBox       txtNom;
        private System.Windows.Forms.Label         lblActivite;
        private System.Windows.Forms.ComboBox      cboActivite;
        private System.Windows.Forms.Label         lblDescription;
        private System.Windows.Forms.TextBox       txtDescription;
        private System.Windows.Forms.Button        btnEnregistrer;
        private System.Windows.Forms.Button        btnAnnuler;
    }
}
