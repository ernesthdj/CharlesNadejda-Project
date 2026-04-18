namespace CharlesNadejda.Forms
{
    partial class FrmBomContextes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre     = new System.Windows.Forms.Label();
            this.dgv          = new System.Windows.Forms.DataGridView();
            this.btnAjouter   = new System.Windows.Forms.Button();
            this.btnModifier  = new System.Windows.Forms.Button();
            this.btnNiveaux   = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize = false;
            this.lblTitre.Font     = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.Location = new System.Drawing.Point(12, 12);
            this.lblTitre.Size     = new System.Drawing.Size(620, 30);
            this.lblTitre.Text     = "Contextes de production";

            // dgv
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.MultiSelect           = false;
            this.dgv.ReadOnly              = true;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.RowHeadersVisible     = false;
            this.dgv.BackgroundColor       = System.Drawing.Color.White;
            this.dgv.Location              = new System.Drawing.Point(12, 55);
            this.dgv.Size                  = new System.Drawing.Size(620, 380);
            this.dgv.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);

            // btnAjouter
            this.btnAjouter.Location = new System.Drawing.Point(644, 55);
            this.btnAjouter.Size     = new System.Drawing.Size(140, 36);
            this.btnAjouter.Text     = "Ajouter";
            this.btnAjouter.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAjouter.Click   += new System.EventHandler(this.btnAjouter_Click);

            // btnModifier
            this.btnModifier.Location = new System.Drawing.Point(644, 101);
            this.btnModifier.Size     = new System.Drawing.Size(140, 36);
            this.btnModifier.Text     = "Modifier";
            this.btnModifier.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.btnModifier.Click   += new System.EventHandler(this.btnModifier_Click);

            // btnNiveaux
            this.btnNiveaux.Location  = new System.Drawing.Point(644, 147);
            this.btnNiveaux.Size      = new System.Drawing.Size(140, 36);
            this.btnNiveaux.Text      = "Gérer niveaux";
            this.btnNiveaux.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnNiveaux.BackColor = System.Drawing.Color.FromArgb(63, 81, 181);
            this.btnNiveaux.ForeColor = System.Drawing.Color.White;
            this.btnNiveaux.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNiveaux.Click    += new System.EventHandler(this.btnNiveaux_Click);

            // btnSupprimer
            this.btnSupprimer.Location  = new System.Drawing.Point(644, 203);
            this.btnSupprimer.Size      = new System.Drawing.Size(140, 36);
            this.btnSupprimer.Text      = "Supprimer";
            this.btnSupprimer.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSupprimer.ForeColor = System.Drawing.Color.DarkRed;
            this.btnSupprimer.Click    += new System.EventHandler(this.btnSupprimer_Click);

            // btnFermer
            this.btnFermer.Location = new System.Drawing.Point(644, 399);
            this.btnFermer.Size     = new System.Drawing.Size(140, 36);
            this.btnFermer.Text     = "Fermer";
            this.btnFermer.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFermer.Click   += new System.EventHandler(this.btnFermer_Click);

            // FrmBomContextes
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(800, 460);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btnAjouter);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.btnNiveaux);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmBomContextes";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Contextes de production";
            this.Load           += new System.EventHandler(this.FrmBomContextes_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label         lblTitre;
        private System.Windows.Forms.DataGridView  dgv;
        private System.Windows.Forms.Button        btnAjouter;
        private System.Windows.Forms.Button        btnModifier;
        private System.Windows.Forms.Button        btnNiveaux;
        private System.Windows.Forms.Button        btnSupprimer;
        private System.Windows.Forms.Button        btnFermer;
    }
}
