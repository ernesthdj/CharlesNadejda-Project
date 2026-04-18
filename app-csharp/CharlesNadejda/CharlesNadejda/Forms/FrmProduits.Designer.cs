namespace CharlesNadejda.Forms
{
    partial class FrmProduits
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
            this.dgvProduits  = new System.Windows.Forms.DataGridView();
            this.btnAjouter   = new System.Windows.Forms.Button();
            this.btnModifier  = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer    = new System.Windows.Forms.Button();
            this.lblTitre     = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduits)).BeginInit();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize  = false;
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.lblTitre.Location  = new System.Drawing.Point(12, 12);
            this.lblTitre.Size      = new System.Drawing.Size(760, 30);
            this.lblTitre.Text      = "Gestion des produits";

            // dgvProduits — palette artisanale
            this.dgvProduits.AllowUserToAddRows    = false;
            this.dgvProduits.AllowUserToDeleteRows = false;
            this.dgvProduits.AllowUserToResizeRows = false;
            this.dgvProduits.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProduits.MultiSelect           = false;
            this.dgvProduits.ReadOnly              = true;
            this.dgvProduits.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProduits.Location              = new System.Drawing.Point(12, 55);
            this.dgvProduits.Size                  = new System.Drawing.Size(760, 420);
            this.dgvProduits.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvProduits.RowHeadersVisible     = false;
            this.dgvProduits.BackgroundColor       = System.Drawing.Color.White;
            this.dgvProduits.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgvProduits.GridColor             = System.Drawing.Color.FromArgb(230, 220, 210);
            this.dgvProduits.ColumnHeadersHeight   = 32;
            this.dgvProduits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProduits.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.dgvProduits.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgvProduits.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.dgvProduits.DefaultCellStyle.SelectionBackColor     = System.Drawing.Color.FromArgb(111, 78, 55);
            this.dgvProduits.DefaultCellStyle.SelectionForeColor     = System.Drawing.Color.White;
            this.dgvProduits.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 247, 244);

            // btnAjouter — chocolat
            this.btnAjouter.Location  = new System.Drawing.Point(784, 55);
            this.btnAjouter.Size      = new System.Drawing.Size(130, 36);
            this.btnAjouter.Text      = "+ Ajouter";
            this.btnAjouter.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAjouter.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAjouter.ForeColor = System.Drawing.Color.White;
            this.btnAjouter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouter.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAjouter.FlatAppearance.BorderSize = 0;
            this.btnAjouter.Click    += new System.EventHandler(this.btnAjouter_Click);

            // btnModifier — vert
            this.btnModifier.Location  = new System.Drawing.Point(784, 101);
            this.btnModifier.Size      = new System.Drawing.Size(130, 36);
            this.btnModifier.Text      = "✎  Modifier";
            this.btnModifier.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(90, 130, 80);
            this.btnModifier.ForeColor = System.Drawing.Color.White;
            this.btnModifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModifier.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnModifier.FlatAppearance.BorderSize = 0;
            this.btnModifier.Click    += new System.EventHandler(this.btnModifier_Click);

            // btnSupprimer — rouge
            this.btnSupprimer.Location  = new System.Drawing.Point(784, 147);
            this.btnSupprimer.Size      = new System.Drawing.Size(130, 36);
            this.btnSupprimer.Text      = "✕  Supprimer";
            this.btnSupprimer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSupprimer.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupprimer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSupprimer.FlatAppearance.BorderSize = 0;
            this.btnSupprimer.Click    += new System.EventHandler(this.btnSupprimer_Click);

            // btnFermer — gris-brun neutre
            this.btnFermer.Location  = new System.Drawing.Point(784, 439);
            this.btnFermer.Size      = new System.Drawing.Size(130, 36);
            this.btnFermer.Text      = "Fermer";
            this.btnFermer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnFermer.BackColor = System.Drawing.Color.FromArgb(100, 90, 80);
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFermer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnFermer.FlatAppearance.BorderSize = 0;
            this.btnFermer.Click    += new System.EventHandler(this.btnFermer_Click);

            // FrmProduits
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(930, 490);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.dgvProduits);
            this.Controls.Add(this.btnAjouter);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmProduits";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Produits";
            this.Load           += new System.EventHandler(this.FrmProduits_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduits)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvProduits;
        private System.Windows.Forms.Button       btnAjouter;
        private System.Windows.Forms.Button       btnModifier;
        private System.Windows.Forms.Button       btnSupprimer;
        private System.Windows.Forms.Button       btnFermer;
        private System.Windows.Forms.Label        lblTitre;
    }
}
