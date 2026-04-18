namespace CharlesNadejda.Forms
{
    partial class FrmAchats
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.dgv          = new System.Windows.Forms.DataGridView();
            this.lblTitre     = new System.Windows.Forms.Label();
            this.btnAjouter   = new System.Windows.Forms.Button();
            this.btnModifier  = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            // ── Titre ──────────────────────────────────────────────────────
            this.lblTitre.AutoSize  = false;
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.Location  = new System.Drawing.Point(12, 12);
            this.lblTitre.Size      = new System.Drawing.Size(730, 30);
            this.lblTitre.Text      = "Achats";
            this.lblTitre.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // ── DataGridView — palette artisanale ─────────────────────────
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AllowUserToResizeRows = false;
            this.dgv.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgv.MultiSelect           = false;
            this.dgv.ReadOnly              = true;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Location              = new System.Drawing.Point(12, 55);
            this.dgv.Size                  = new System.Drawing.Size(760, 400);
            this.dgv.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgv.RowHeadersVisible     = false;
            this.dgv.BackgroundColor       = System.Drawing.Color.White;
            this.dgv.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgv.GridColor             = System.Drawing.Color.FromArgb(230, 220, 210);
            this.dgv.ColumnHeadersHeight   = 32;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor          = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor          = System.Drawing.Color.FromArgb(61, 40, 23);
            this.dgv.ColumnHeadersDefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgv.DefaultCellStyle.SelectionBackColor              = System.Drawing.Color.FromArgb(111, 78, 55);
            this.dgv.DefaultCellStyle.SelectionForeColor              = System.Drawing.Color.White;
            this.dgv.AlternatingRowsDefaultCellStyle.BackColor        = System.Drawing.Color.FromArgb(250, 247, 244);
            this.dgv.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                            | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            // ── Boutons — palette artisanale, couleurs sémantiques ────────
            int bx = 786, bw = 140, bh = 36;

            this.btnAjouter.Location  = new System.Drawing.Point(bx, 55);
            this.btnAjouter.Size      = new System.Drawing.Size(bw, bh);
            this.btnAjouter.Text      = "+ Nouvel achat";
            this.btnAjouter.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAjouter.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAjouter.ForeColor = System.Drawing.Color.White;
            this.btnAjouter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouter.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAjouter.FlatAppearance.BorderSize = 0;
            this.btnAjouter.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnAjouter.Click    += new System.EventHandler(this.btnAjouter_Click);

            this.btnModifier.Location  = new System.Drawing.Point(bx, 99);
            this.btnModifier.Size      = new System.Drawing.Size(bw, bh);
            this.btnModifier.Text      = "Modifier";
            this.btnModifier.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(90, 130, 80);
            this.btnModifier.ForeColor = System.Drawing.Color.White;
            this.btnModifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModifier.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnModifier.FlatAppearance.BorderSize = 0;
            this.btnModifier.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnModifier.Click    += new System.EventHandler(this.btnModifier_Click);

            this.btnSupprimer.Location  = new System.Drawing.Point(bx, 143);
            this.btnSupprimer.Size      = new System.Drawing.Size(bw, bh);
            this.btnSupprimer.Text      = "Supprimer";
            this.btnSupprimer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSupprimer.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupprimer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSupprimer.FlatAppearance.BorderSize = 0;
            this.btnSupprimer.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnSupprimer.Click    += new System.EventHandler(this.btnSupprimer_Click);

            this.btnFermer.Location  = new System.Drawing.Point(bx, 419);
            this.btnFermer.Size      = new System.Drawing.Size(bw, bh);
            this.btnFermer.Text      = "Fermer";
            this.btnFermer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnFermer.BackColor = System.Drawing.Color.FromArgb(100, 90, 80);
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFermer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnFermer.FlatAppearance.BorderSize = 0;
            this.btnFermer.Anchor    = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnFermer.Click    += new System.EventHandler(this.btnFermer_Click);

            // ── Form ───────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(940, 470);
            this.MinimumSize         = new System.Drawing.Size(800, 420);
            this.Controls.AddRange(new System.Windows.Forms.Control[]
                { this.lblTitre, this.dgv, this.btnAjouter, this.btnModifier, this.btnSupprimer, this.btnFermer });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox     = true;
            this.Name            = "FrmAchats";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Achats";
            this.Load           += new System.EventHandler(this.FrmAchats_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Label        lblTitre;
        private System.Windows.Forms.Button       btnAjouter, btnModifier, btnSupprimer, btnFermer;
    }
}
