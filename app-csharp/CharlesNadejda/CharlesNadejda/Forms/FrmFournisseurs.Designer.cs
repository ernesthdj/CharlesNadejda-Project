namespace CharlesNadejda.Forms
{
    partial class FrmFournisseurs
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.dgv          = new System.Windows.Forms.DataGridView();
            this.btnAjouter   = new System.Windows.Forms.Button();
            this.btnModifier  = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer    = new System.Windows.Forms.Button();
            this.lblTitre     = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            this.lblTitre.AutoSize  = false;
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.ForeColor = CharlesNadejda.Forms.AppColors.ChocoBrand;
            this.lblTitre.Location  = new System.Drawing.Point(12, 12);
            this.lblTitre.Size      = new System.Drawing.Size(560, 30);
            this.lblTitre.Text      = "Fournisseurs";

            // DGV — palette artisanale
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AllowUserToResizeRows = false;
            this.dgv.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.MultiSelect           = false;
            this.dgv.ReadOnly              = true;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Location              = new System.Drawing.Point(12, 55);
            this.dgv.Size                  = new System.Drawing.Size(600, 380);
            this.dgv.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgv.RowHeadersVisible     = false;
            this.dgv.BackgroundColor       = System.Drawing.Color.White;
            this.dgv.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgv.GridColor             = CharlesNadejda.Forms.AppColors.GridLine;
            this.dgv.ColumnHeadersHeight   = 32;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = CharlesNadejda.Forms.AppColors.Creme;
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = CharlesNadejda.Forms.AppColors.ChocoBrand;
            this.dgv.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.dgv.DefaultCellStyle.SelectionBackColor     = CharlesNadejda.Forms.AppColors.ChocoMed;
            this.dgv.DefaultCellStyle.SelectionForeColor     = System.Drawing.Color.White;
            this.dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 247, 244);

            // Boutons — palette sémantique
            int bx = 624, bw = 130, bh = 36;
            this.btnAjouter.Location  = new System.Drawing.Point(bx, 55);
            this.btnAjouter.Size      = new System.Drawing.Size(bw, bh);
            this.btnAjouter.Text      = "+ Ajouter";
            this.btnAjouter.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAjouter.BackColor = CharlesNadejda.Forms.AppColors.ChocoBrand;
            this.btnAjouter.ForeColor = System.Drawing.Color.White;
            this.btnAjouter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouter.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAjouter.FlatAppearance.BorderSize = 0;
            this.btnAjouter.Click    += new System.EventHandler(this.btnAjouter_Click);

            this.btnModifier.Location  = new System.Drawing.Point(bx, 101);
            this.btnModifier.Size      = new System.Drawing.Size(bw, bh);
            this.btnModifier.Text      = "✎  Modifier";
            this.btnModifier.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(90, 130, 80);
            this.btnModifier.ForeColor = System.Drawing.Color.White;
            this.btnModifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModifier.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnModifier.FlatAppearance.BorderSize = 0;
            this.btnModifier.Click    += new System.EventHandler(this.btnModifier_Click);

            this.btnSupprimer.Location  = new System.Drawing.Point(bx, 147);
            this.btnSupprimer.Size      = new System.Drawing.Size(bw, bh);
            this.btnSupprimer.Text      = "✕  Supprimer";
            this.btnSupprimer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSupprimer.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupprimer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSupprimer.FlatAppearance.BorderSize = 0;
            this.btnSupprimer.Click    += new System.EventHandler(this.btnSupprimer_Click);

            this.btnFermer.Location  = new System.Drawing.Point(bx, 399);
            this.btnFermer.Size      = new System.Drawing.Size(bw, bh);
            this.btnFermer.Text      = "Fermer";
            this.btnFermer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnFermer.BackColor = System.Drawing.Color.FromArgb(100, 90, 80);
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFermer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnFermer.FlatAppearance.BorderSize = 0;
            this.btnFermer.Click    += new System.EventHandler(this.btnFermer_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(772, 460);
            this.Controls.AddRange(new System.Windows.Forms.Control[] { this.lblTitre, this.dgv, this.btnAjouter, this.btnModifier, this.btnSupprimer, this.btnFermer });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmFournisseurs";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Fournisseurs";
            this.Load           += new System.EventHandler(this.FrmFournisseurs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit(); this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Button btnAjouter, btnModifier, btnSupprimer, btnFermer;
        private System.Windows.Forms.Label lblTitre;
    }
}
