using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'une fiche BOM (Bill of Materials).
    ///
    /// Reçoit le BomNiveau auquel appartient la fiche. Le niveau détermine
    /// automatiquement quels inputs sont proposés :
    ///   - Toujours disponibles : ingrédients de l'activité (base N1)
    ///   - Si ordre >= 3 : aussi les fiches de TOUS les niveaux inférieurs (N2, N3, …, N-1)
    ///
    /// Règle : niveau N peut consommer n'importe quel niveau inférieur, jamais supérieur.
    /// </summary>
    public partial class FrmBomFicheEdit : Form
    {
        private readonly BomFiche    _fiche;
        private readonly BomNiveau   _niveau;
        private readonly bool        _isEdit;
        private readonly List<BomFicheLigne> _lignes = new List<BomFicheLigne>();

        // (plus de _typeInputAuto : une fiche peut mixer ingrédients ET fiches de niveaux inférieurs)

        public FrmBomFicheEdit(BomFiche fiche, BomNiveau niveau)
        {
            InitializeComponent();
            _isEdit = fiche != null;
            _fiche  = fiche ?? new BomFiche { IdNiveau = niveau.Id, ActiviteNom = niveau.ActiviteNom };
            _niveau = niveau;

            if (_isEdit && _fiche.Lignes != null)
                _lignes.AddRange(_fiche.Lignes);
        }

        private void FrmBomFicheEdit_Load(object sender, EventArgs e)
        {
            this.Text = _isEdit
                ? $"Modifier la fiche  —  {_niveau.NomContexte} › {_niveau.Nom}"
                : $"Nouvelle fiche  —  {_niveau.NomContexte} › {_niveau.Nom} (N{_niveau.Ordre})";

            // Masquer le sélecteur de type legacy (type déterminé par l'item choisi dans le combo)
            if (cboTypeInput != null) cboTypeInput.Visible = false;

            // Afficher le nom de l'activité en lecture seule — hérité du niveau (TICKET-05)
            if (lblActiviteValeur != null)
                lblActiviteValeur.Text = _niveau.ActiviteNom ?? "";

            // Unités output
            cboUniteOutput.Items.Clear();
            cboUniteOutput.Items.AddRange(new[] { "piece", "kg", "g", "mg", "l", "dl", "cl", "ml" });
            cboUniteOutput.SelectedItem = _isEdit ? _fiche.UniteOutput : "piece";

            if (_isEdit)
            {
                txtNom.Text              = _fiche.Nom;
                txtDescription.Text      = _fiche.Description;
                nudQuantiteOutput.Value  = (decimal)Math.Max(1, (double)_fiche.QuantiteOutput);
                if (_fiche.TempsPreparation.HasValue)
                    nudTemps.Value = _fiche.TempsPreparation.Value;
            }

            // Label d'info sur les inputs attendus
            AfficherInfoInput();

            ChargerInputsDisponibles();
            RafraîchirGrilleLignes();
        }

        // ── Informations sur le type d'input ────────────────────────────

        private void AfficherInfoInput()
        {
            string msg = _niveau.Ordre <= 2
                ? "Inputs : ingrédients du stock (N1)"
                : "Inputs : ingrédients + fiches de tous les niveaux inférieurs";

            if (lblTypeInput != null)
            {
                lblTypeInput.Text      = msg;
                lblTypeInput.ForeColor = System.Drawing.Color.FromArgb(111, 78, 55);
                lblTypeInput.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
                lblTypeInput.AutoSize  = true;
            }
        }

        // ── Chargement des inputs disponibles ───────────────────────────

        private void ChargerInputsDisponibles()
        {
            cboInput.Items.Clear();

            // Ingrédients du stock (base, toujours disponibles quel que soit le niveau)
            foreach (var ing in IngredientDAL.GetAll(idActivite: _niveau.IdActivite))
                cboInput.Items.Add(new InputItem
                {
                    Id        = ing.Id,
                    Nom       = "[Ingr.]  " + ing.Nom,
                    Unite     = ing.UniteMesure,
                    TypeInput = "ingredient"
                });

            // Fiches de TOUS les niveaux inférieurs (ordre >= 2 et ordre < niveau actuel)
            // Règle : N peut consommer n'importe quel niveau < N, jamais >= N.
            foreach (var niv in BomNiveauDAL.GetByContexte(_niveau.IdContexte))
            {
                if (niv.Ordre >= _niveau.Ordre || niv.Ordre < 2) continue;
                foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
                    cboInput.Items.Add(new InputItem
                    {
                        Id        = f.Id,
                        Nom       = $"[N{niv.Ordre}]  {f.Nom}",
                        Unite     = f.UniteOutput,
                        TypeInput = "fiche"
                    });
            }

            if (cboInput.Items.Count > 0)
            {
                cboInput.SelectedIndex = 0;
                SynchroniserUniteInput();
            }

            // TICKET-20 : désactiver l'enregistrement si aucun input disponible
            if (btnEnregistrer != null)
                btnEnregistrer.Enabled = cboInput.Items.Count > 0;
        }

        private void cboInput_SelectedIndexChanged(object sender, EventArgs e)
            => SynchroniserUniteInput();

        /// <summary>
        /// Peuple cboUniteLigne avec toutes les unités compatibles avec l'input sélectionné.
        /// - Masse (mg/g/kg) et Volume (ml/cl/dl/l) : sélection libre dans le groupe.
        /// - Pièce : unité unique, verrouillée ET quantité forcée à 1.
        /// La valeur choisie ici (UniteMesure) est stockée en base et sert à la conversion
        /// lors de la vérification de stock (BomProductionDAL.Convertir).
        /// </summary>
        private void SynchroniserUniteInput()
        {
            if (!(cboInput.SelectedItem is InputItem item)) return;

            cboUniteLigne.Items.Clear();
            foreach (var u in UnitConvertisseur.UnitesCompatibles(item.Unite))
                cboUniteLigne.Items.Add(u);

            // Sélectionner l'unité de l'ingrédient par défaut
            cboUniteLigne.SelectedItem = item.Unite;
            if (cboUniteLigne.SelectedIndex < 0 && cboUniteLigne.Items.Count > 0)
                cboUniteLigne.SelectedIndex = 0;

            bool estPiece = string.Equals(item.Unite, "piece", StringComparison.OrdinalIgnoreCase);

            // Une seule unité disponible (pièce) → verrouillé
            cboUniteLigne.Enabled = cboUniteLigne.Items.Count > 1;

            // Règle métier : pièce = toujours 1 unité consommée
            if (estPiece)
            {
                nudQteLigne.Minimum = 1;
                nudQteLigne.Maximum = 1;
                nudQteLigne.Value   = 1;
                nudQteLigne.Enabled = false;
            }
            else
            {
                nudQteLigne.Minimum = 0;
                nudQteLigne.Maximum = 99999;
                nudQteLigne.Enabled = true;
            }
        }

        // ── Gestion des lignes ───────────────────────────────────────────

        private void btnAjouterLigne_Click(object sender, EventArgs e)
        {
            if (!(cboInput.SelectedItem is InputItem item))
            {
                MessageBox.Show("Sélectionnez un input.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (nudQteLigne.Value <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à 0.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ligne = new BomFicheLigne
            {
                TypeInput         = item.TypeInput,
                IdInputIngredient = item.TypeInput == "ingredient" ? (int?)item.Id : null,
                IdInputFiche      = item.TypeInput == "fiche"      ? (int?)item.Id : null,
                Quantite          = nudQteLigne.Value,
                UniteMesure       = cboUniteLigne.SelectedItem?.ToString() ?? item.Unite,
                NomInput          = item.Nom,
                UniteMesureInput  = item.Unite
            };

            _lignes.Add(ligne);
            RafraîchirGrilleLignes();
        }

        private void btnRetirerLigne_Click(object sender, EventArgs e)
        {
            if (dgvLignes.CurrentRow == null) return;
            int index = dgvLignes.CurrentRow.Index;
            if (index >= 0 && index < _lignes.Count)
            {
                _lignes.RemoveAt(index);
                RafraîchirGrilleLignes();
            }
        }

        private void RafraîchirGrilleLignes()
        {
            dgvLignes.DataSource = null;
            dgvLignes.DataSource = new System.ComponentModel.BindingList<BomFicheLigne>(_lignes);

            string[] cachées = { "Id", "IdFiche", "IdInputIngredient", "IdInputFiche",
                                  "UniteMesureInput", "PrixUnitaireRef", "SousTotal" };
            foreach (string col in cachées)
                if (dgvLignes.Columns[col] != null) dgvLignes.Columns[col].Visible = false;

            if (dgvLignes.Columns["TypeInput"]   != null) { dgvLignes.Columns["TypeInput"].HeaderText  = "Type";     dgvLignes.Columns["TypeInput"].Width = 90; }
            if (dgvLignes.Columns["NomInput"]    != null)   dgvLignes.Columns["NomInput"].HeaderText   = "Input";
            if (dgvLignes.Columns["Quantite"]    != null) { dgvLignes.Columns["Quantite"].HeaderText   = "Quantité"; dgvLignes.Columns["Quantite"].Width = 80; }
            if (dgvLignes.Columns["UniteMesure"] != null) { dgvLignes.Columns["UniteMesure"].HeaderText= "Unité";    dgvLignes.Columns["UniteMesure"].Width = 70; }
        }

        // ── Enregistrement ───────────────────────────────────────────────

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom est obligatoire.");
                valid = false;
            }
            else if (BomFicheDAL.NomExiste(txtNom.Text.Trim(), _niveau.Id, _isEdit ? _fiche.Id : 0))
            {
                errorProvider.SetError(txtNom, "Ce nom existe déjà dans ce niveau.");
                valid = false;
            }
            if (_lignes.Count == 0)
            {
                MessageBox.Show("Ajoutez au moins un input à la fiche.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                valid = false;
            }
            if (!valid) return;

            try
            {
                _fiche.IdNiveau         = _niveau.Id;
                _fiche.Nom              = txtNom.Text.Trim();
                _fiche.Description      = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                _fiche.UniteOutput      = cboUniteOutput.SelectedItem?.ToString() ?? "piece";
                _fiche.QuantiteOutput   = nudQuantiteOutput.Value;
                _fiche.TempsPreparation = nudTemps.Value > 0 ? (int?)nudTemps.Value : null;
                _fiche.Lignes           = _lignes;

                if (_isEdit)
                    BomFicheDAL.Update(_fiche);
                else
                    BomFicheDAL.Insert(_fiche);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── Classe interne — item affiché dans le ComboBox d'inputs ─────

        private class InputItem
        {
            public int    Id        { get; set; }
            public string Nom       { get; set; }
            public string Unite     { get; set; }
            public string TypeInput { get; set; }   // ingredient | fiche
            public override string ToString() => Nom;
        }
    }
}
