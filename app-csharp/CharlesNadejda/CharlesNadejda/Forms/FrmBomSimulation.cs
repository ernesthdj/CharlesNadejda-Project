using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Écran de simulation de production BOM (Bill of Materials).
    /// Affiche la liste complète des inputs requis avec comparaison stock disponible.
    /// Mode lecture seule — aucune production n'est exécutée.
    /// </summary>
    public partial class FrmBomSimulation : Form
    {
        private readonly int _idActivite;
        private List<BomManque> _lignesSimulation;

        public FrmBomSimulation(int idActivite = 0)
        {
            InitializeComponent();
            _idActivite = idActivite;
        }

        private void FrmBomSimulation_Load(object sender, EventArgs e)
        {
            ChargerContextes();
        }

        // ── Chargement en cascade Contexte → Niveau → Fiche ─────────────

        private void ChargerContextes()
        {
            cboContexte.Items.Clear();
            foreach (var ctx in BomContexteDAL.GetAll(_idActivite))
                cboContexte.Items.Add(ctx);
            if (cboContexte.Items.Count > 0) cboContexte.SelectedIndex = 0;
        }

        private void cboContexte_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboNiveau.Items.Clear();
            cboFiche.Items.Clear();
            RéinitialiserRésultats();

            if (!(cboContexte.SelectedItem is BomContexte ctx)) return;

            foreach (var niv in BomNiveauDAL.GetByContexte(ctx.Id))
                cboNiveau.Items.Add(niv);
            if (cboNiveau.Items.Count > 0) cboNiveau.SelectedIndex = 0;
        }

        private void cboNiveau_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboFiche.Items.Clear();
            RéinitialiserRésultats();

            if (!(cboContexte.SelectedItem is BomContexte ctx)) return;

            foreach (var f in BomFicheDAL.GetAll(ctx.IdActivite))
                cboFiche.Items.Add(f);
            if (cboFiche.Items.Count > 0) cboFiche.SelectedIndex = 0;
        }

        private void cboFiche_SelectedIndexChanged(object sender, EventArgs e)
            => RéinitialiserRésultats();

        private void RéinitialiserRésultats()
        {
            lblResultat.Text      = "";
            dgvSimulation.DataSource = null;
            _lignesSimulation     = null;
        }

        // ── Simulation ───────────────────────────────────────────────────

        private void btnSimuler_Click(object sender, EventArgs e)
        {
            if (!SélectionValide()) return;

            var niveau = cboNiveau.SelectedItem as BomNiveau;
            var fiche  = cboFiche.SelectedItem  as BomFiche;

            try
            {
                _lignesSimulation = BomProductionDAL.Simuler(
                    niveau.Id, fiche.Id, nudQuantite.Value);

                dgvSimulation.DataSource = null;
                dgvSimulation.DataSource = _lignesSimulation;

                ConfigurerGrille();
                ColoriserLignes();

                int pénuries = _lignesSimulation.FindAll(l => l.Manque > 0).Count;

                if (pénuries == 0)
                {
                    lblResultat.ForeColor = Color.DarkGreen;
                    lblResultat.Text = $"✔ Tous les stocks sont suffisants — production possible ({_lignesSimulation.Count} input(s) vérifié(s)).";
                }
                else
                {
                    lblResultat.ForeColor = Color.DarkRed;
                    lblResultat.Text = $"✘ {pénuries} pénurie(s) sur {_lignesSimulation.Count} input(s) — production bloquée.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la simulation : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurerGrille()
        {
            if (dgvSimulation.Columns["NomInput"]           != null) dgvSimulation.Columns["NomInput"].HeaderText           = "Ingrédient / Fiche";
            if (dgvSimulation.Columns["Unite"]              != null) { dgvSimulation.Columns["Unite"].HeaderText             = "Unité";      dgvSimulation.Columns["Unite"].Width = 70; }
            if (dgvSimulation.Columns["QuantiteNecessaire"] != null) { dgvSimulation.Columns["QuantiteNecessaire"].HeaderText = "Nécessaire"; dgvSimulation.Columns["QuantiteNecessaire"].Width = 100; }
            if (dgvSimulation.Columns["QuantiteDisponible"] != null) { dgvSimulation.Columns["QuantiteDisponible"].HeaderText = "Disponible"; dgvSimulation.Columns["QuantiteDisponible"].Width = 100; }
            if (dgvSimulation.Columns["Manque"]             != null) { dgvSimulation.Columns["Manque"].HeaderText             = "Manque";     dgvSimulation.Columns["Manque"].Width = 100; }
        }

        private void ColoriserLignes()
        {
            if (_lignesSimulation == null) return;
            for (int i = 0; i < dgvSimulation.Rows.Count && i < _lignesSimulation.Count; i++)
            {
                bool ok = _lignesSimulation[i].Manque <= 0;
                dgvSimulation.Rows[i].DefaultCellStyle.ForeColor = ok
                    ? Color.DarkGreen
                    : Color.DarkRed;
                dgvSimulation.Rows[i].DefaultCellStyle.BackColor = ok
                    ? Color.FromArgb(240, 255, 240)
                    : Color.FromArgb(255, 240, 240);
            }
        }

        // ── Validation ───────────────────────────────────────────────────

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
