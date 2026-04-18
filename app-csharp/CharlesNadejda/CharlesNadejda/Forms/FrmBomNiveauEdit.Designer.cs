namespace CharlesNadejda.Forms
{
    partial class FrmBomNiveauEdit
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
            this.lblOrdre       = new System.Windows.Forms.Label();
            this.lblNom         = new System.Windows.Forms.Label();
            this.txtNom         = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler     = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            // lblOrdre
            this.lblOrdre.AutoSize  = false;
            this.lblOrdre.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblOrdre.ForeColor = System.Drawing.Color.Gray;
            this.lblOrdre.Location  = new System.Drawing.Point(20, 15);
            this.lblOrdre.Size      = new System.Drawing.Size(340, 20);
            this.lblOrdre.Text      = "Ordre :";

            // lblNom
            this.lblNom.AutoSize = true;
            this.lblNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNom.Location = new System.Drawing.Point(20, 45);
            this.lblNom.Text     = "Nom du niveau *";

            // txtNom
            this.txtNom.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNom.Location = new System.Drawing.Point(20, 67);
            this.txtNom.Size     = new System.Drawing.Size(340, 26);
            this.txtNom.TabIndex = 0;

            // lblDescription
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescription.Location = new System.Drawing.Point(20, 107);
            this.lblDescription.Text     = "Description";

            // txtDescription
            this.txtDescription.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location  = new System.Drawing.Point(20, 129);
            this.txtDescription.Multiline = true;
            this.txtDescription.Size      = new System.Drawing.Size(340, 60);
            this.txtDescription.TabIndex  = 1;

            // btnEnregistrer — palette chocolat
            this.btnEnregistrer.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnregistrer.Location  = new System.Drawing.Point(20, 210);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.TabIndex  = 2;
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            // btnAnnuler — ton neutre
            this.btnAnnuler.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 210);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.TabIndex  = 3;
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(390, 268);
            this.Controls.Add(this.lblOrdre);
            this.Controls.Add(this.lblNom);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.btnAnnuler);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "FrmBomNiveauEdit";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load           += new System.EventHandler(this.FrmBomNiveauEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label         lblOrdre;
        private System.Windows.Forms.Label         lblNom;
        private System.Windows.Forms.TextBox       txtNom;
        private System.Windows.Forms.Label         lblDescription;
        private System.Windows.Forms.TextBox       txtDescription;
        private System.Windows.Forms.Button        btnEnregistrer;
        private System.Windows.Forms.Button        btnAnnuler;
    }
}
