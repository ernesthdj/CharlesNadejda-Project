namespace CharlesNadejda.Forms
{
    partial class FrmFournisseurEdit
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.components    = new System.ComponentModel.Container();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtNom = new System.Windows.Forms.TextBox(); this.txtContact = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox(); this.txtTel = new System.Windows.Forms.TextBox();
            this.txtAdresse = new System.Windows.Forms.TextBox(); this.txtNotes = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button(); this.btnAnnuler = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();

            this.errorProvider.ContainerControl = this;

            var font = new System.Drawing.Font("Segoe UI", 10F);
            int lx = 20, lw = 360, tab = 0;

            void Row(System.Windows.Forms.Label lbl, string txt, System.Windows.Forms.TextBox tb, int y, bool multi = false)
            {
                lbl.AutoSize = true; lbl.Font = font; lbl.Location = new System.Drawing.Point(lx, y); lbl.Text = txt;
                tb.Font = font; tb.Location = new System.Drawing.Point(lx, y + 22); tb.Size = new System.Drawing.Size(lw, multi ? 60 : 26); tb.TabIndex = tab++;
                if (multi) tb.Multiline = true;
                this.Controls.Add(lbl); this.Controls.Add(tb);
            }

            var lblNom = new System.Windows.Forms.Label(); Row(lblNom, "Nom *", txtNom, 20);
            var lblContact = new System.Windows.Forms.Label(); Row(lblContact, "Contact", txtContact, 65);
            var lblEmail = new System.Windows.Forms.Label(); Row(lblEmail, "Email", txtEmail, 110);
            var lblTel = new System.Windows.Forms.Label(); Row(lblTel, "Téléphone", txtTel, 155);
            var lblAdresse = new System.Windows.Forms.Label(); Row(lblAdresse, "Adresse", txtAdresse, 200);
            var lblNotes = new System.Windows.Forms.Label(); Row(lblNotes, "Notes", txtNotes, 245, true);

            this.btnEnregistrer.Font      = font;
            this.btnEnregistrer.Location  = new System.Drawing.Point(lx, 325);
            this.btnEnregistrer.Size      = new System.Drawing.Size(160, 36);
            this.btnEnregistrer.Text      = "Enregistrer";
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEnregistrer.TabIndex  = tab++;
            this.btnEnregistrer.FlatAppearance.BorderSize = 0;
            this.btnEnregistrer.Click    += new System.EventHandler(this.btnEnregistrer_Click);

            this.btnAnnuler.Font      = font;
            this.btnAnnuler.Location  = new System.Drawing.Point(200, 325);
            this.btnAnnuler.Size      = new System.Drawing.Size(160, 36);
            this.btnAnnuler.Text      = "Annuler";
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(220, 215, 210);
            this.btnAnnuler.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAnnuler.TabIndex  = tab++;
            this.btnAnnuler.FlatAppearance.BorderSize = 0;
            this.btnAnnuler.Click    += new System.EventHandler(this.btnAnnuler_Click);

            this.Controls.Add(this.btnEnregistrer); this.Controls.Add(this.btnAnnuler);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F); this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 378); this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false; this.Name = "FrmFournisseurEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FrmFournisseurEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }

        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.TextBox txtNom, txtContact, txtEmail, txtTel, txtAdresse, txtNotes;
        private System.Windows.Forms.Button btnEnregistrer, btnAnnuler;
    }
}
