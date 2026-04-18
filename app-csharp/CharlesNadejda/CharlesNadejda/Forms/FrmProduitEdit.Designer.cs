namespace CharlesNadejda.Forms
{
    partial class FrmProduitEdit
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
            this.components     = new System.ComponentModel.Container();
            this.errorProvider  = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblNom         = new System.Windows.Forms.Label();
            this.txtNom         = new System.Windows.Forms.TextBox();
            this.lblCategorie   = new System.Windows.Forms.Label();
            this.cmbCategorie   = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblPrixTTC     = new System.Windows.Forms.Label();
            this.txtPrixTTC     = new System.Windows.Forms.TextBox();
            this.lblPrixPromo   = new System.Windows.Forms.Label();
            this.txtPrixPromo   = new System.Windows.Forms.TextBox();
            this.lblStock       = new System.Windows.Forms.Label();
            this.nudStock       = new System.Windows.Forms.NumericUpDown();
            this.chkDisponible  = new System.Windows.Forms.CheckBox();
            this.lblImageUrl    = new System.Windows.Forms.Label();
            this.txtImageUrl    = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler     = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStock)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            int left = 20, w = 360, tab = 0;

            // Nom
            this.lblNom.AutoSize = true;
            this.lblNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNom.Location = new System.Drawing.Point(left, 20);
            this.lblNom.Text     = "Nom *";
            this.txtNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNom.Location = new System.Drawing.Point(left, 42);
            this.txtNom.Size     = new System.Drawing.Size(w, 26);
            this.txtNom.TabIndex = tab++;

            // Catégorie
            this.lblCategorie.AutoSize = true;
            this.lblCategorie.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblCategorie.Location = new System.Drawing.Point(left, 82);
            this.lblCategorie.Text     = "Catégorie *";
            this.cmbCategorie.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCategorie.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategorie.Location      = new System.Drawing.Point(left, 104);
            this.cmbCategorie.Size          = new System.Drawing.Size(w, 26);
            this.cmbCategorie.TabIndex      = tab++;

            // Description
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescription.Location = new System.Drawing.Point(left, 144);
            this.lblDescription.Text     = "Description";
            this.txtDescription.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location  = new System.Drawing.Point(left, 166);
            this.txtDescription.Multiline = true;
            this.txtDescription.Size      = new System.Drawing.Size(w, 60);
            this.txtDescription.TabIndex  = tab++;

            // Prix TTC
            this.lblPrixTTC.AutoSize = true;
            this.lblPrixTTC.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPrixTTC.Location = new System.Drawing.Point(left, 240);
            this.lblPrixTTC.Text     = "Prix TTC (€) *";
            this.txtPrixTTC.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPrixTTC.Location = new System.Drawing.Point(left, 262);
            this.txtPrixTTC.Size     = new System.Drawing.Size(100, 26);
            this.txtPrixTTC.TabIndex = tab++;

            // Prix promo
            this.lblPrixPromo.AutoSize = true;
            this.lblPrixPromo.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPrixPromo.Location = new System.Drawing.Point(160, 240);
            this.lblPrixPromo.Text     = "Prix promo (€)";
            this.txtPrixPromo.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPrixPromo.Location = new System.Drawing.Point(160, 262);
            this.txtPrixPromo.Size     = new System.Drawing.Size(100, 26);
            this.txtPrixPromo.TabIndex = tab++;

            // Stock
            this.lblStock.AutoSize = true;
            this.lblStock.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStock.Location = new System.Drawing.Point(left, 302);
            this.lblStock.Text     = "Stock";
            this.nudStock.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.nudStock.Location = new System.Drawing.Point(left, 324);
            this.nudStock.Maximum  = new decimal(new int[] { 99999, 0, 0, 0 });
            this.nudStock.Size     = new System.Drawing.Size(80, 26);
            this.nudStock.TabIndex = tab++;

            // Disponible
            this.chkDisponible.AutoSize  = true;
            this.chkDisponible.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDisponible.Location  = new System.Drawing.Point(160, 326);
            this.chkDisponible.Text      = "Disponible";
            this.chkDisponible.TabIndex  = tab++;

            // Image URL
            this.lblImageUrl.AutoSize = true;
            this.lblImageUrl.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblImageUrl.Location = new System.Drawing.Point(left, 364);
            this.lblImageUrl.Text     = "URL image";
            this.txtImageUrl.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtImageUrl.Location = new System.Drawing.Point(left, 386);
            this.txtImageUrl.Size     = new System.Drawing.Size(w, 26);
            this.txtImageUrl.TabIndex = tab++;

            // Boutons — palette chocolat
            this.btnEnregistrer.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnregistrer.Location  = new System.Drawing.Point(left, 430);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = tab++;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            this.btnAnnuler.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAnnuler.Location  = new System.Drawing.Point(220, 430);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.TabIndex  = tab++;
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(410, 485);
            this.Controls.Add(this.lblNom);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.lblCategorie);
            this.Controls.Add(this.cmbCategorie);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblPrixTTC);
            this.Controls.Add(this.txtPrixTTC);
            this.Controls.Add(this.lblPrixPromo);
            this.Controls.Add(this.txtPrixPromo);
            this.Controls.Add(this.lblStock);
            this.Controls.Add(this.nudStock);
            this.Controls.Add(this.chkDisponible);
            this.Controls.Add(this.lblImageUrl);
            this.Controls.Add(this.txtImageUrl);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.btnAnnuler);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FrmProduitEdit";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load           += new System.EventHandler(this.FrmProduitEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStock)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider  errorProvider;
        private System.Windows.Forms.Label          lblNom;
        private System.Windows.Forms.TextBox        txtNom;
        private System.Windows.Forms.Label          lblCategorie;
        private System.Windows.Forms.ComboBox       cmbCategorie;
        private System.Windows.Forms.Label          lblDescription;
        private System.Windows.Forms.TextBox        txtDescription;
        private System.Windows.Forms.Label          lblPrixTTC;
        private System.Windows.Forms.TextBox        txtPrixTTC;
        private System.Windows.Forms.Label          lblPrixPromo;
        private System.Windows.Forms.TextBox        txtPrixPromo;
        private System.Windows.Forms.Label          lblStock;
        private System.Windows.Forms.NumericUpDown  nudStock;
        private System.Windows.Forms.CheckBox       chkDisponible;
        private System.Windows.Forms.Label          lblImageUrl;
        private System.Windows.Forms.TextBox        txtImageUrl;
        private System.Windows.Forms.Button         btnEnregistrer;
        private System.Windows.Forms.Button         btnAnnuler;
    }
}
