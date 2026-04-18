using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Écran d'exécution d'une production BOM (Bill of Materials).
    /// Peut être appelé depuis FrmArtisaStock avec un contexte et niveau
    /// pré-sélectionnés, ou ouvert librement avec une activité.
    /// Les fiches proposées sont filtrées par le niveau sélectionné.
    /// </summary>
    public partial class FrmBomProduction : Form
    {
        private readonly int         _idActivite;
        private readonly BomContexte _contexteInitial;
        private readonly BomNiveau   _niveauInitial;

        public FrmBomProduction(int idActivite = 0)
        {
            InitializeComponent();
            _idActivite = idActivite;
        }

        /// <summary>Appelé depuis FrmArtisaStock avec contexte + niveau pré-sélectionnés.</summary>
        public FrmBomProduction(BomContexte contexte, BomNiveau niveau)
        {
            InitializeComponent();
            _idActivite      = contexte.IdActivite;
            _contexteInitial = contexte;
            _niveauInitial   = niveau;
        }

        private void FrmBomProduction_Load(object sender, EventArgs e)
        {
            ChargerContextes();
            btnExécuter.Enabled = false;

            // Pré-sélectionner le contexte et le niveau si fournis
            if (_contexteInitial != null)
            {
                for (int i = 0; i < cboContexte.Items.Count; i++)
                {
                    if (((BomContexte)cboContexte.Items[i]).Id == _contexteInitial.Id)
                    {
                        cboContexte.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (_niveauInitial != null)
            {
                for (int i = 0; i < cboNiveau.Items.Count; i++)
                {
                    if (((BomNiveau)cboNiveau.Items[i]).Id == _niveauInitial.Id)
                    {
                        cboNiveau.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        // ── Chargement en cascade Contexte → Niveau → Fiche ─────────────

        private void ChargerContextes()
        {
            cboContexte.Items.Clear();
            foreach (var ctx in BomContexteDAL.GetAll(_idActivite))
                cboContexte.Items.Add(ctx);
            if (cboContexte.Items.Count > 0 && _contexteInitial == null)
                cboContexte.SelectedIndex = 0;
        }

        private void cboContexte_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboNiveau.Items.Clear();
            cboFiche.Items.Clear();
            btnExécuter.Enabled = false;
            lblResultat.Text    = "";

            if (!(cboContexte.SelectedItem is BomContexte ctx)) return;

            // N1 (Ingrédients) ne peut pas être "produit" via ce formulaire
            foreach (var niv in BomNiveauDAL.GetByContexte(ctx.Id))
                if (niv.Ordre > 1) cboNiveau.Items.Add(niv);

            if (cboNiveau.Items.Count > 0 && _niveauInitial == null)
                cboNiveau.SelectedIndex = 0;
        }

        private void cboNiveau_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboFiche.Items.Clear();
            btnExécuter.Enabled = false;
            lblResultat.Text    = "";

            if (!(cboNiveau.SelectedItem is BomNiveau niv)) return;

            // Fiches filtrées par le niveau sélectionné (plus toutes les fiches de l'activité)
            foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
                cboFiche.Items.Add(f);

            if (cboFiche.Items.Count > 0)
                cboFiche.SelectedIndex = 0;
        }

        private void cboFiche_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnExécuter.Enabled = false;
            lblResultat.Text    = "";
            dgvManques.DataSource = null;
        }

        // ── Vérification de stock ────────────────────────────────────────

        private void btnVérifier_Click(object sender, EventArgs e)
        {
            if (!SélectionValide()) return;

            var niveau = cboNiveau.SelectedItem as BomNiveau;
            var fiche  = cboFiche.SelectedItem  as BomFiche;

            try
            {
                List<BomManque> manques = BomProductionDAL.VerifierDisponibilite(
                    niveau.Id, fiche.Id, nudQuantite.Value);

                dgvManques.DataSource = null;

                if (manques.Count == 0)
                {
                    lblResultat.ForeColor = System.Drawing.Color.DarkGreen;
                    lblResultat.Text      = "✔ Stock suffisant — production possible.";
                    btnExécuter.Enabled   = true;
                }
                else
                {
                    lblResultat.ForeColor = System.Drawing.Color.DarkRed;
                    lblResultat.Text      = $"✘ Stock insuffisant — {manques.Count} pénurie(s) détectée(s).";
                    btnExécuter.Enabled   = false;

                    dgvManques.DataSource = manques;
                    ConfigurerGrilleManques();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la vérification : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurerGrilleManques()
        {
            if (dgvManques.Columns["NomInput"]           != null) dgvManques.Columns["NomInput"].HeaderText           = "Ingrédient / Fiche";
            if (dgvManques.Columns["Unite"]              != null) { dgvManques.Columns["Unite"].HeaderText             = "Unité";       dgvManques.Columns["Unite"].Width = 70; }
            if (dgvManques.Columns["QuantiteNecessaire"] != null) { dgvManques.Columns["QuantiteNecessaire"].HeaderText = "Nécessaire"; dgvManques.Columns["QuantiteNecessaire"].Width = 100; }
            if (dgvManques.Columns["QuantiteDisponible"] != null) { dgvManques.Columns["QuantiteDisponible"].HeaderText = "Disponible"; dgvManques.Columns["QuantiteDisponible"].Width = 100; }
            if (dgvManques.Columns["Manque"]             != null)
            {
                dgvManques.Columns["Manque"].HeaderText = "Manque";
                dgvManques.Columns["Manque"].Width      = 100;
                dgvManques.Columns["Manque"].DefaultCellStyle.ForeColor = System.Drawing.Color.DarkRed;
            }
        }

        // ── Exécution de la production ───────────────────────────────────

        private void btnExécuter_Click(object sender, EventArgs e)
        {
            if (!SélectionValide()) return;

            var niveau = cboNiveau.SelectedItem as BomNiveau;
            var fiche  = cboFiche.SelectedItem  as BomFiche;

            var confirm = MessageBox.Show(
                $"Lancer la production ?\n\n" +
                $"Fiche    : {fiche.Nom}\n" +
                $"Niveau   : {niveau.Nom} (N{niveau.Ordre})\n" +
                $"Contexte : {niveau.NomContexte}\n" +
                $"Quantité : {nudQuantite.Value} {fiche.UniteOutput}\n\n" +
                $"Le stock du niveau N{niveau.Ordre - 1} sera consommé.",
                "Confirmation de production",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                string notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                BomProductionDAL.Executer(niveau.Id, fiche.Id, nudQuantite.Value, notes);

                MessageBox.Show(
                    $"Production exécutée avec succès.\n" +
                    $"{nudQuantite.Value} {fiche.UniteOutput} de « {fiche.Nom} » ajoutés au stock du niveau « {niveau.Nom} ».",
                    "Production réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnExécuter.Enabled   = false;
                lblResultat.Text      = "";
                dgvManques.DataSource = null;
                txtNotes.Clear();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Stock insuffisant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnExécuter.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la production : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool SélectionValide()
        {
            if (cboContexte.SelectedItem == null || cboNiveau.SelectedItem == null || cboFiche.SelectedItem == null)
            {
                MessageBox.Show("Sélectionnez un contexte, un niveau et une fiche.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (nudQuantite.Value <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à 0.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
