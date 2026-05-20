using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'une fiche BOM (Bill of Materials).
    ///
    /// Reçoit le BomNiveau auquel appartient la fiche. Le niveau détermine
    /// automatiquement quels inputs sont proposés :
    ///   - Toujours disponibles : ingrédients de l'activité (base N1)
    ///   - Si ordre >= 3 : aussi les fiches de TOUS les niveaux inférieurs (N2, ..., N-1)
    ///
    /// Règle : niveau N peut consommer n'importe quel niveau inférieur, jamais supérieur.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmBomFicheEdit : FrmEditBase
    {
        private readonly BomFiche  _fiche;
        private readonly BomNiveau _niveau;
        private readonly bool      _isEdit;
        private readonly List<BomFicheLigne> _lignes = new List<BomFicheLigne>();

        // ── Contrôles en-tête ─────────────────────────────────────────────
        private readonly TextBox       txtNom;
        private readonly TextBox       txtDescription;
        private readonly ComboBox      cboUniteOutput;
        private readonly NumericUpDown nudQuantiteOutput;
        private readonly NumericUpDown nudTemps;
        private readonly CheckBox      chkStockCible;
        private readonly NumericUpDown nudStockCible;
        private readonly Label         lblActiviteValeur;

        // ── Contrôles section lignes ──────────────────────────────────────
        private readonly Label         lblTypeInput;
        private readonly ComboBox      cboInput;
        private readonly NumericUpDown nudQteLigne;
        private readonly ComboBox      cboUniteLigne;
        private readonly DataGridView  dgvLignes;

        public FrmBomFicheEdit(BomFiche fiche, BomNiveau niveau)
        {
            _isEdit = fiche != null;
            _fiche  = fiche ?? new BomFiche { IdNiveau = niveau.Id, ActiviteNom = niveau.ActiviteNom };
            _niveau = niveau;

            if (_isEdit && _fiche.Lignes != null)
                _lignes.AddRange(_fiche.Lignes);

            var font  = new Font("Segoe UI", 10F);
            var fontS = new Font("Segoe UI", 9.5F);
            ClientSize = new Size(800, 100);

            // ── Ligne 1 : Nom + Activité (lecture seule) ──────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(12, 15), Text = "Nom de la fiche *" });
            txtNom = new TextBox { Font = font, Location = new Point(12, 36), Size = new Size(280, 26), TabIndex = 0 };
            Controls.Add(txtNom);

            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(304, 15), Text = "Activité *" });
            lblActiviteValeur = new Label
            {
                AutoSize  = true,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand,
                Location  = new Point(304, 36), Text = ""
            };
            Controls.Add(lblActiviteValeur);

            // ── Ligne 2 : Unité output + Qté output + Temps ───────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(12, 76), Text = "Unité produite" });
            cboUniteOutput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = font, Location = new Point(12, 97), Size = new Size(100, 26), TabIndex = 2
            };
            Controls.Add(cboUniteOutput);

            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(124, 76), Text = "Qté produite/exécution" });
            nudQuantiteOutput = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = new decimal(new[] { 1, 0, 0, 131072 }),
                Maximum = new decimal(new[] { 100000, 0, 0, 0 }),
                Value   = new decimal(new[] { 1, 0, 0, 0 }),
                Font = font, Location = new Point(124, 97), Size = new Size(100, 26), TabIndex = 3
            };
            Controls.Add(nudQuantiteOutput);

            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(236, 76), Text = "Temps (min)" });
            nudTemps = new NumericUpDown
            {
                Maximum = new decimal(new[] { 9999, 0, 0, 0 }),
                Font = font, Location = new Point(236, 97), Size = new Size(80, 26), TabIndex = 4
            };
            Controls.Add(nudTemps);

            chkStockCible = new CheckBox
            {
                AutoSize = true, Font = font, Location = new Point(328, 99),
                Text = "Stock cible", TabIndex = 5
            };
            chkStockCible.CheckedChanged += (s, e) => nudStockCible.Enabled = chkStockCible.Checked;
            Controls.Add(chkStockCible);

            nudStockCible = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0, Maximum = 99999,
                Enabled = false,
                Font = font, Location = new Point(436, 97), Size = new Size(90, 26), TabIndex = 6
            };
            Controls.Add(nudStockCible);

            // ── Ligne 3 : Description ─────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(12, 137), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(12, 158),
                Size = new Size(432, 50), Multiline = true, TabIndex = 5
            };
            Controls.Add(txtDescription);

            // ── GroupBox Lignes ───────────────────────────────────────────
            var grpLignes = new GroupBox
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(12, 220), Size = new Size(770, 300),
                Text = "Composition — inputs de la fiche", TabStop = false
            };
            Controls.Add(grpLignes);

            lblTypeInput = new Label
            {
                AutoSize = true, Font = fontS,
                Location = new Point(10, 28), Text = "Type"
            };
            grpLignes.Controls.Add(lblTypeInput);

            grpLignes.Controls.Add(new Label { AutoSize = true, Font = fontS, Location = new Point(140, 28), Text = "Input" });
            cboInput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = fontS, Location = new Point(140, 47), Size = new Size(220, 24), TabIndex = 7
            };
            cboInput.SelectedIndexChanged += (s, e) => SynchroniserUniteInput();
            grpLignes.Controls.Add(cboInput);

            grpLignes.Controls.Add(new Label { AutoSize = true, Font = fontS, Location = new Point(370, 28), Text = "Quantité" });
            nudQteLigne = new NumericUpDown
            {
                DecimalPlaces = 3,
                Minimum = new decimal(new[] { 1, 0, 0, 196608 }),
                Maximum = new decimal(new[] { 100000, 0, 0, 0 }),
                Value   = new decimal(new[] { 1, 0, 0, 0 }),
                Font = fontS, Location = new Point(370, 47), Size = new Size(90, 24), TabIndex = 8
            };
            grpLignes.Controls.Add(nudQteLigne);

            grpLignes.Controls.Add(new Label { AutoSize = true, Font = fontS, Location = new Point(470, 28), Text = "Unité" });
            cboUniteLigne = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = fontS, Location = new Point(470, 47), Size = new Size(80, 24), TabIndex = 9
            };
            cboUniteLigne.Items.AddRange(new object[] { "piece", "kg", "g", "l", "ml", "cl" });
            cboUniteLigne.SelectedIndex = 0;
            grpLignes.Controls.Add(cboUniteLigne);

            var btnAjouterLigne = new Button
            {
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(560, 44), Size = new Size(90, 28),
                Text = "+ Ajouter", TabIndex = 10,
                BackColor = Color.FromArgb(40, 120, 40), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAjouterLigne.Click += BtnAjouterLigne_Click;
            grpLignes.Controls.Add(btnAjouterLigne);

            var btnRetirerLigne = new Button
            {
                Font = fontS,
                Location = new Point(660, 44), Size = new Size(90, 28),
                Text = "Retirer", TabIndex = 11,
                ForeColor = Color.DarkRed
            };
            btnRetirerLigne.Click += BtnRetirerLigne_Click;
            grpLignes.Controls.Add(btnRetirerLigne);

            dgvLignes = new DataGridView
            {
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect           = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible     = false,
                BackgroundColor       = Color.White,
                Location              = new Point(10, 82), Size = new Size(745, 200),
                Font                  = new Font("Segoe UI", 9F)
            };
            grpLignes.Controls.Add(dgvLignes);

            PositionnerBoutons(535);

            Load += FrmBomFicheEdit_Load;
        }

        private void FrmBomFicheEdit_Load(object sender, EventArgs e)
        {
            Text = _isEdit
                ? $"Modifier la fiche  —  {_niveau.NomContexte} › {_niveau.Nom}"
                : $"Nouvelle fiche  —  {_niveau.NomContexte} › {_niveau.Nom} (N{_niveau.Ordre})";

            lblActiviteValeur.Text = _niveau.ActiviteNom ?? "";

            cboUniteOutput.Items.Clear();
            cboUniteOutput.Items.AddRange(new[] { "piece", "kg", "g", "mg", "l", "dl", "cl", "ml" });
            cboUniteOutput.SelectedItem = _isEdit ? _fiche.UniteOutput : "piece";

            if (_isEdit)
            {
                txtNom.Text             = _fiche.Nom;
                txtDescription.Text     = _fiche.Description;
                nudQuantiteOutput.Value = (decimal)Math.Max(1, (double)_fiche.QuantiteOutput);
                if (_fiche.TempsPreparation.HasValue)
                    nudTemps.Value = _fiche.TempsPreparation.Value;
                if (_fiche.StockCible.HasValue)
                {
                    chkStockCible.Checked = true;
                    nudStockCible.Value   = _fiche.StockCible.Value;
                }
            }

            AfficherInfoInput();
            ChargerInputsDisponibles();
            RafraichirGrilleLignes();
        }

        private void AfficherInfoInput()
        {
            lblTypeInput.Text = _niveau.Ordre <= 2
                ? "Inputs : ingrédients du stock (N1)"
                : "Inputs : ingrédients + fiches de tous les niveaux inférieurs";
            lblTypeInput.ForeColor = AppColors.ChocoMed;
            lblTypeInput.Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblTypeInput.AutoSize  = true;
        }

        private void ChargerInputsDisponibles()
        {
            cboInput.Items.Clear();

            foreach (var ing in IngredientDAL.GetAll(idActivite: _niveau.IdActivite))
                cboInput.Items.Add(new InputItem
                {
                    Id = ing.Id, Nom = "[Ingr.]  " + ing.Nom,
                    Unite = ing.UniteMesure, TypeInput = "ingredient"
                });

            foreach (var niv in BomNiveauDAL.GetByContexte(_niveau.IdContexte))
            {
                if (niv.Ordre >= _niveau.Ordre || niv.Ordre < 2) continue;
                foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
                    cboInput.Items.Add(new InputItem
                    {
                        Id = f.Id, Nom = $"[N{niv.Ordre}]  {f.Nom}",
                        Unite = f.UniteOutput, TypeInput = "fiche"
                    });
            }

            if (cboInput.Items.Count > 0)
            {
                cboInput.SelectedIndex = 0;
                SynchroniserUniteInput();
            }

            // TICKET-20 : désactiver l'enregistrement si aucun input disponible
            btnEnregistrer.Enabled = cboInput.Items.Count > 0;
        }

        private void SynchroniserUniteInput()
        {
            if (!(cboInput.SelectedItem is InputItem item)) return;

            cboUniteLigne.Items.Clear();
            foreach (var u in UnitConvertisseur.UnitesCompatibles(item.Unite))
                cboUniteLigne.Items.Add(u);

            cboUniteLigne.SelectedItem = item.Unite;
            if (cboUniteLigne.SelectedIndex < 0 && cboUniteLigne.Items.Count > 0)
                cboUniteLigne.SelectedIndex = 0;

            cboUniteLigne.Enabled = cboUniteLigne.Items.Count > 1;

            // Quantité toujours libre — un pack peut consommer N pièces
            nudQteLigne.Minimum       = 0.001m;
            nudQteLigne.Maximum       = 99999;
            nudQteLigne.DecimalPlaces = string.Equals(item.Unite, "piece", StringComparison.OrdinalIgnoreCase) ? 0 : 3;
            nudQteLigne.Value         = 1;
            nudQteLigne.Enabled       = true;
        }

        private void BtnAjouterLigne_Click(object sender, EventArgs e)
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

            _lignes.Add(new BomFicheLigne
            {
                TypeInput         = item.TypeInput,
                IdInputIngredient = item.TypeInput == "ingredient" ? (int?)item.Id : null,
                IdInputFiche      = item.TypeInput == "fiche"      ? (int?)item.Id : null,
                Quantite          = nudQteLigne.Value,
                UniteMesure       = cboUniteLigne.SelectedItem?.ToString() ?? item.Unite,
                NomInput          = item.Nom,
                UniteMesureInput  = item.Unite
            });
            RafraichirGrilleLignes();
        }

        private void BtnRetirerLigne_Click(object sender, EventArgs e)
        {
            if (dgvLignes.CurrentRow == null) return;
            int index = dgvLignes.CurrentRow.Index;
            if (index >= 0 && index < _lignes.Count)
            {
                _lignes.RemoveAt(index);
                RafraichirGrilleLignes();
            }
        }

        private void RafraichirGrilleLignes()
        {
            dgvLignes.DataSource = null;
            dgvLignes.DataSource = new BindingList<BomFicheLigne>(_lignes);

            string[] cachees = { "Id", "IdFiche", "IdInputIngredient", "IdInputFiche",
                                 "UniteMesureInput", "PrixUnitaireRef", "SousTotal" };
            foreach (string col in cachees)
                if (dgvLignes.Columns[col] != null) dgvLignes.Columns[col].Visible = false;

            if (dgvLignes.Columns["TypeInput"]   != null) { dgvLignes.Columns["TypeInput"].HeaderText  = "Type";     dgvLignes.Columns["TypeInput"].Width = 90; }
            if (dgvLignes.Columns["NomInput"]    != null)   dgvLignes.Columns["NomInput"].HeaderText   = "Input";
            if (dgvLignes.Columns["Quantite"]    != null) { dgvLignes.Columns["Quantite"].HeaderText   = "Quantité"; dgvLignes.Columns["Quantite"].Width = 80; }
            if (dgvLignes.Columns["UniteMesure"] != null) { dgvLignes.Columns["UniteMesure"].HeaderText= "Unité";    dgvLignes.Columns["UniteMesure"].Width = 70; }
        }

        protected override bool Valider()
        {
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); ok = false; }
            else if (BomFicheDAL.NomExiste(txtNom.Text.Trim(), _niveau.Id, _isEdit ? _fiche.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà dans ce niveau."); ok = false; }

            if (_lignes.Count == 0)
            {
                MessageBox.Show("Ajoutez au moins un input à la fiche.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ok = false;
            }

            return ok;
        }

        protected override void Sauvegarder()
        {
            _fiche.IdNiveau         = _niveau.Id;
            _fiche.Nom              = txtNom.Text.Trim();
            _fiche.Description      = txtDescription.Text.Trim().NullIfEmpty();
            _fiche.UniteOutput      = cboUniteOutput.SelectedItem?.ToString() ?? "piece";
            _fiche.QuantiteOutput   = nudQuantiteOutput.Value;
            _fiche.TempsPreparation = nudTemps.Value > 0 ? (int?)nudTemps.Value : null;
            _fiche.StockCible       = chkStockCible.Checked && nudStockCible.Value > 0 ? nudStockCible.Value : (decimal?)null;
            _fiche.Lignes           = _lignes;

            if (_isEdit) BomFicheDAL.Update(_fiche);
            else         BomFicheDAL.Insert(_fiche);
        }

        private class InputItem
        {
            public int    Id        { get; set; }
            public string Nom       { get; set; }
            public string Unite     { get; set; }
            public string TypeInput { get; set; }
            public override string ToString() => Nom;
        }
    }
}
