namespace CharlesNadejda.Forms
{
    partial class FrmBomFiches
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
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnFermer    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize = false;
            this.lblTitre.Font     = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.Location = new System.Drawing.Point(12, 12);
            this.lblTitre.Size     = new System.Drawing.Size(640, 30);
            this.lblTitre.Text     = "Fiches recettes BOM";

            // dgv — palette artisanale
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AllowUserToResizeRows = false;
            this.dgv.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.MultiSelect           = false;
            this.dgv.ReadOnly              = true;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.RowHeadersVisible     = false;
            this.dgv.BackgroundColor       = System.Drawing.Color.White;
            this.dgv.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgv.GridColor             = System.Drawing.Color.FromArgb(230, 220, 210);
            this.dgv.ColumnHeadersHeight   = 32;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv.Location              = new System.Drawing.Point(12, 55);
            this.dgv.Size                  = new System.Drawing.Size(640, 380);
            this.dgv.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor          = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor          = System.Drawing.Color.FromArgb(61, 40, 23);
            this.dgv.ColumnHeadersDefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(245, 230, 211);
            this.dgv.DefaultCellStyle.SelectionBackColor              = System.Drawing.Color.FromArgb(111, 78, 55);
            this.dgv.DefaultCellStyle.SelectionForeColor              = System.Drawing.Color.White;
            this.dgv.AlternatingRowsDefaultCellStyle.BackColor        = System.Drawing.Color.FromArgb(250, 247, 244);

            // btnAjouter — palette couleurs sémantiques
            this.btnAjouter.Location  = new System.Drawing.Point(664, 55);
            this.btnAjouter.Size      = new System.Drawing.Size(130, 36);
            this.btnAjouter.Text      = "+ Ajouter";
            this.btnAjouter.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAjouter.BackColor = System.Drawing.Color.FromArgb(61, 40, 23);
            this.btnAjouter.ForeColor = System.Drawing.Color.White;
            this.btnAjouter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouter.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAjouter.FlatAppearance.BorderSize = 0;
            this.btnAjouter.Click    += new System.EventHandler(this.btnAjouter_Click);

            // btnModifier
            this.btnModifier.Location  = new System.Drawing.Point(664, 99);
            this.btnModifier.Size      = new System.Drawing.Size(130, 36);
            this.btnModifier.Text      = "Modifier";
            this.btnModifier.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(90, 130, 80);
            this.btnModifier.ForeColor = System.Drawing.Color.White;
            this.btnModifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModifier.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnModifier.FlatAppearance.BorderSize = 0;
            this.btnModifier.Click    += new System.EventHandler(this.btnModifier_Click);

            // btnSupprimer
            this.btnSupprimer.Location  = new System.Drawing.Point(664, 143);
            this.btnSupprimer.Size      = new System.Drawing.Size(130, 36);
            this.btnSupprimer.Text      = "Supprimer";
            this.btnSupprimer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSupprimer.BackColor = System.Drawing.Color.FromArgb(180, 50, 40);
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupprimer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSupprimer.FlatAppearance.BorderSize = 0;
            this.btnSupprimer.Click    += new System.EventHandler(this.btnSupprimer_Click);

            // btnFermer
            this.btnFermer.Location  = new System.Drawing.Point(664, 399);
            this.btnFermer.Size      = new System.Drawing.Size(130, 36);
            this.btnFermer.Text      = "Fermer";
            this.btnFermer.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnFermer.BackColor = System.Drawing.Color.FromArgb(100, 90, 80);
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFermer.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnFermer.FlatAppearance.BorderSize = 0;
            this.btnFermer.Click    += new System.EventHandler(this.btnFermer_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(810, 460);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btnAjouter);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FrmBomFiches";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Fiches recettes BOM";
            this.Load           += new System.EventHandler(this.FrmBomFiches_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label        lblTitre;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Button       btnAjouter;
        private System.Windows.Forms.Button       btnModifier;
        private System.Windows.Forms.Button       btnSupprimer;
        private System.Windows.Forms.Button       btnFermer;
    }
}
