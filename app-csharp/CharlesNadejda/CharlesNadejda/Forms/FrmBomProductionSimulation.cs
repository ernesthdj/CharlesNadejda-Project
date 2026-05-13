using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Écran unifié Simulation + Production BOM (Bill of Materials).
    ///
    /// Flux :
    ///   1. Sélection Contexte → Niveau (Ordre > 1) → Fiche → Quantité → Notes (opt.)
    ///   2. "Simuler" → liste complète des inputs avec coloration vert/rouge
    ///   3. "Lancer la production" devient actif uniquement si 0 pénurie
    ///   4. Confirmation → BomProductionDAL.Executer() → stock mis à jour
    ///
    /// US-C01 — remplace FrmBomProduction + FrmBomSimulation.
    /// Peut être ouvert avec un contexte + niveau pré-sélectionnés (depuis FrmArtisaStock).
    /// </summary>
    public partial class FrmBomProductionSimulation : Form
    {
        private readonly int         _idActivite;
        private readonly BomContexte _contexteInitial;
        private readonly BomNiveau   _niveauInitial;

        private List<BomManque> _lignesSimulation;
        private bool            _simulationValide;

        public FrmBomProductionSimulation(int idActivite = 0)
        {
            InitializeComponent();
            _idActivite = idActivite;
        }

        /// <summary>Appelé depuis FrmArtisaStock avec contexte + niveau pré-sélectionnés.</summary>
        public FrmBomProductionSimulation(BomContexte contexte, BomNiveau niveau)
        {
            InitializeComponent();
            _idActivite      = contexte.IdActivite;
            _contexteInitial = contexte;
            _niveauInitial   = niveau;
        }

        private void FrmBomProductionSimulation_Load(object sender, EventArgs e)
        {
            // TICKET-18 : unifier le style de btnFermer avec la palette artisanale
            btnFermer.BackColor = AppColors.GreyBtn;
            btnFermer.ForeColor = AppColors.ChocoBrand;
            btnFermer.FlatStyle = FlatStyle.Flat;
            btnFermer.FlatAppearance.BorderColor = AppColors.Border;
            btnFermer.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 213, 202);

            // TICKET-21 : hint dynamique affiché quand btnLancerProduction est grisé
            var lblHint = new Label
            {
                Text      = "Simulez d'abord — aucune pénurie requise pour activer la production.",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = AppColors.ChocoMed,
                AutoSize  = false,
                Size      = new Size(240, 32),
                Location  = new Point(btnLancerProduction.Left, btnLancerProduction.Bottom + 4),
                Visible   = true
            };
            this.Controls.Add(lblHint);
            lblHint.BringToFront();

            btnLancerProduction.EnabledChanged += (s, ev) =>
                lblHint.Visible = !btnLancerProduction.Enabled;

            ChargerContextes();
            btnLancerProduction.Enabled = false;

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

            // TICKET-19 : état vide si aucun contexte disponible pour cette activité
            if (cboContexte.Items.Count == 0)
            {
                lblResultat.ForeColor = System.Drawing.Color.FromArgb(140, 110, 80);
                lblResultat.Text      = "Aucun contexte BOM disponible pour cette activité. " +
                                        "Créez d'abord un contexte et ses niveaux depuis la vue stock.";
                btnSimuler.Enabled = false;
            }
            else if (cboContexte.Items.Count > 0 && _contexteInitial == null)
            {
                cboContexte.SelectedIndex = 0;
            }
        }

        private void cboContexte_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboNiveau.Items.Clear();
            cboFiche.Items.Clear();
            RéinitialiserRésultats();

            if (!(cboContexte.SelectedItem is BomContexte ctx)) return;

            // N1 (Ingrédients) ne peut pas être "produit"
            foreach (var niv in BomNiveauDAL.GetByContexte(ctx.Id))
                if (niv.Ordre > 1) cboNiveau.Items.Add(niv);

            if (cboNiveau.Items.Count > 0 && _niveauInitial == null)
                cboNiveau.SelectedIndex = 0;
        }

        private void cboNiveau_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboFiche.Items.Clear();
            RéinitialiserRésultats();

            if (!(cboNiveau.SelectedItem is BomNiveau niv)) return;

            // Fiches filtrées par le niveau sélectionné
            foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
                cboFiche.Items.Add(f);

            if (cboFiche.Items.Count > 0)
                cboFiche.SelectedIndex = 0;
        }

        private void cboFiche_SelectedIndexChanged(object sender, EventArgs e)
        {
            RéinitialiserRésultats();

            var fiche = cboFiche.SelectedItem as BomFiche;
            lblInfoBatch.Text = fiche != null
                ? $"1 batch = {fiche.QuantiteOutput} {fiche.UniteOutput}   →   saisir le nombre de batches à produire"
                : "";
        }

        private void RéinitialiserRésultats()
        {
            lblResultat.Text            = "";
            lblCoutEstime.Text          = "";
            dgvSimulation.DataSource    = null;
            _lignesSimulation           = null;
            _simulationValide           = false;
            btnLancerProduction.Enabled = false;
        }

        // ── Simulation ───────────────────────────────────────────────────

        private void btnSimuler_Click(object sender, EventArgs e)
        {
            if (!SélectionValide()) return;

            var niveau = cboNiveau.SelectedItem as BomNiveau;
            var fiche  = cboFiche.SelectedItem  as BomFiche;

            // Feedback visuel pendant le traitement (Nielsen #1 : visibilité de l'état)
            btnSimuler.Enabled = false;
            Cursor = Cursors.WaitCursor;
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
                    lblResultat.ForeColor       = Color.DarkGreen;
                    lblResultat.Text            = $"✔ Tous les stocks sont suffisants — {_lignesSimulation.Count} input(s) vérifié(s).";
                    _simulationValide           = true;
                    btnLancerProduction.Enabled = true;

                    // Calcul du coût estimé (règle de 3 récursive multi-niveaux)
                    try
                    {
                        var rapport = BomCoutDAL.CalculerCout(fiche.Id, nudQuantite.Value);
                        lblCoutEstime.ForeColor = Color.FromArgb(60, 100, 60);
                        lblCoutEstime.Text = rapport.QuantiteOutput > 0
                            ? $"Coût estimé : {rapport.CoutTotal:F2} €  " +
                              $"({rapport.CoutUnitaire:F4} €/{rapport.UniteOutput})  " +
                              $"→  {nudQuantite.Value} batch(es) × {fiche.QuantiteOutput} {fiche.UniteOutput}"
                            : "Coût estimé : données de prix manquantes";
                    }
                    catch (Exception exCout)
                    {
                        // TICKET-11 : logguer au lieu d'avaler silencieusement — afficher un message explicite
                        Trace.TraceError("FrmBomProductionSimulation — calcul coût : {0}", exCout);
                        lblCoutEstime.ForeColor = System.Drawing.Color.DarkRed;
                        lblCoutEstime.Text      = "Coût non disponible";
                    }
                }
                else
                {
                    lblResultat.ForeColor       = Color.DarkRed;
                    lblResultat.Text            = $"✘ {pénuries} pénurie(s) sur {_lignesSimulation.Count} input(s) — production bloquée.";
                    lblCoutEstime.Text          = "";
                    _simulationValide           = false;
                    btnLancerProduction.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la simulation : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Toujours réactiver le bouton, même en cas d'erreur
                btnSimuler.Enabled = true;
                Cursor = Cursors.Default;
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
                dgvSimulation.Rows[i].DefaultCellStyle.ForeColor = ok ? Color.DarkGreen : Color.DarkRed;
                dgvSimulation.Rows[i].DefaultCellStyle.BackColor = ok
                    ? Color.FromArgb(240, 255, 240)
                    : Color.FromArgb(255, 240, 240);
            }
        }

        // ── Exécution de la production ───────────────────────────────────

        private async void btnLancerProduction_Click(object sender, EventArgs e)
        {
            try
            {
                if (!SélectionValide() || !_simulationValide) return;

                var niveau = cboNiveau.SelectedItem as BomNiveau;
                var fiche  = cboFiche.SelectedItem  as BomFiche;

                decimal qteTotale = nudQuantite.Value * fiche.QuantiteOutput;

                // DefaultButton.Button2 = "Non" par défaut — protection contre lancement accidentel
                var confirm = MessageBox.Show(
                    $"Lancer la production ?\n\n" +
                    $"Fiche    : {fiche.Nom}\n" +
                    $"Niveau   : {niveau.Nom} (N{niveau.Ordre})\n" +
                    $"Contexte : {niveau.NomContexte}\n" +
                    $"Batches  : {nudQuantite.Value} × {fiche.QuantiteOutput} {fiche.UniteOutput} = {qteTotale} {fiche.UniteOutput}\n\n" +
                    $"Le stock du niveau N{niveau.Ordre - 1} sera consommé.",
                    "Confirmation de production",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes) return;

                // Protection double-clic + feedback visuel pendant l'écriture en base
                btnLancerProduction.Enabled = false;
                btnSimuler.Enabled          = false;
                Cursor = Cursors.WaitCursor;

                try
                {
                    string notes    = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                    int    delaiJ   = (int)nudDelaiConservation.Value;   // 0 = pas de DLC

                    int idProd = await System.Threading.Tasks.Task.Run(() =>
                        BomProductionDAL.Executer(niveau.Id, fiche.Id, nudQuantite.Value, notes, delaiJ));

                    MessageBox.Show(
                        $"Production enregistrée (ID #{idProd}).\n" +
                        $"{nudQuantite.Value} batch(es) → {qteTotale} {fiche.UniteOutput} de « {fiche.Nom} » ajoutés au stock du niveau « {niveau.Nom} ».",
                        "Production réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtNotes.Clear();

                    // Rechargement de la simulation après succès
                    btnSimuler_Click(sender, e);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Stock insuffisant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RéinitialiserRésultats();
                }
                finally
                {
                    // Toujours réactiver les boutons et rétablir le curseur quoi qu'il arrive
                    btnLancerProduction.Enabled = _simulationValide;
                    btnSimuler.Enabled          = true;
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("FrmBomProductionSimulation.btnLancerProduction_Click : {0}", ex);
                MessageBox.Show("Erreur inattendue : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
