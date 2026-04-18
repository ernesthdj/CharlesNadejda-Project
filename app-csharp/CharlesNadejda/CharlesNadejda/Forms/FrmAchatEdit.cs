using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'un achat (lot d'ingrédient).
    /// - Création : lot == null  → sélection libre de l'ingrédient.
    /// - Modification : lot fourni → ingrédient affiché en lecture seule.
    /// - Prix saisissable en HTVA ou TVAC (radio button) ; stocké toujours en HTVA.
    /// - L'unité de mesure est imposée par la fiche ingrédient (non modifiable).
    /// </summary>
    public class FrmAchatEdit : Form
    {
        private readonly Lot        _lot;
        private readonly bool       _isEdit;
        private readonly int        _idActivite;
        private          Ingredient _ingredientSelectionne;

        // Contrôles
        private readonly ComboBox       cmbIngredient;
        private readonly Label          lblIngredientRO;
        private readonly Label          lblUnite;
        private readonly ComboBox       cmbFournisseur;
        private readonly TextBox        txtNumeroLot;
        private readonly NumericUpDown  nudQuantite;
        private readonly RadioButton    rbHtva;
        private readonly RadioButton    rbTvac;
        private readonly NumericUpDown  nudPrix;
        private readonly NumericUpDown  nudTvaPct;
        private readonly Label          lblPrixHtva;
        private readonly Label          lblPrixTvac;
        private readonly Label          lblTotalHtva;
        private readonly Label          lblTotalTvac;
        private readonly TextBox        txtRefFacture;
        private readonly DateTimePicker dtpDateAchat;
        private readonly CheckBox       chkPeremption;
        private readonly DateTimePicker dtpPeremption;
        private readonly TextBox        txtNotes;
        private readonly Button         btnEnregistrer;
        private readonly Button         btnAnnuler;
        private readonly ErrorProvider  errorProvider;

        public FrmAchatEdit(Lot lot, int idActivite = 0)
        {
            _isEdit     = lot != null;
            _lot        = lot ?? new Lot();
            _idActivite = idActivite;

            Text            = _isEdit ? "Modifier un achat" : "Nouvel achat";
            ClientSize      = new Size(415, 735);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            var font     = new Font("Segoe UI", 10F);
            var fontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
            errorProvider = new ErrorProvider { ContainerControl = this };

            // Helper : ajoute un Label descriptif
            Label Lbl(string text, int x, int y)
            {
                var l = new Label { AutoSize = true, Font = font, Location = new Point(x, y), Text = text };
                Controls.Add(l);
                return l;
            }

            int lx = 15, w = 370, tab = 0;

            // ── Ingrédient ─────────────────────────────────────────────────
            Lbl(_isEdit ? "Ingrédient" : "Ingrédient *", lx, 12);
            if (_isEdit)
            {
                lblIngredientRO = new Label
                {
                    AutoSize    = false,
                    Font        = font,
                    Location    = new Point(lx, 34),
                    Size        = new Size(w, 26),
                    Text        = _lot.NomIngredient,
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor   = Color.FromArgb(80, 80, 80)
                };
                Controls.Add(lblIngredientRO);
                cmbIngredient = null;
            }
            else
            {
                cmbIngredient = new ComboBox
                {
                    Font          = font,
                    Location      = new Point(lx, 34),
                    Size          = new Size(w, 26),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    TabIndex      = tab++
                };
                cmbIngredient.SelectedIndexChanged += (s, e) => MajUniteIngredient();
                Controls.Add(cmbIngredient);
                lblIngredientRO = null;
            }

            // ── Fournisseur ────────────────────────────────────────────────
            Lbl("Fournisseur", lx, 70);
            cmbFournisseur = new ComboBox
            {
                Font          = font,
                Location      = new Point(lx, 92),
                Size          = new Size(w, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabIndex      = tab++
            };
            Controls.Add(cmbFournisseur);

            // ── N° lot ─────────────────────────────────────────────────────
            Lbl("N° lot (facultatif)", lx, 128);
            txtNumeroLot = new TextBox { Font = font, Location = new Point(lx, 150), Size = new Size(200, 26), TabIndex = tab++ };
            Controls.Add(txtNumeroLot);

            // ── Quantité (= nb de conditionnements) ───────────────────────
            Lbl("Nombre de conditionnements *", lx, 186);
            nudQuantite = new NumericUpDown
            {
                Font          = font,
                Location      = new Point(lx, 208),
                Size          = new Size(120, 26),
                DecimalPlaces = 3,
                Minimum       = 0,
                Maximum       = 99999,
                TabIndex      = tab++
            };
            nudQuantite.ValueChanged += (s, e) => MajPrix();
            Controls.Add(nudQuantite);

            // Label "× 10 000 g = 50 000 g en stock" — mis à jour dynamiquement
            lblUnite = new Label
            {
                AutoSize  = false,
                Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(60, 110, 60),
                Location  = new Point(lx, 236),
                Size      = new Size(w, 20),
                Text      = "—"
            };
            Controls.Add(lblUnite);

            // ── GroupBox saisie du prix ────────────────────────────────────
            var grp = new GroupBox { Font = font, Location = new Point(12, 262), Size = new Size(390, 120), Text = "Saisie du prix (€ par conditionnement)" };
            Controls.Add(grp);

            rbHtva = new RadioButton { Font = font, Location = new Point(10, 22), AutoSize = true, Text = "HTVA (hors taxe)", Checked = true, TabIndex = tab++ };
            rbTvac = new RadioButton { Font = font, Location = new Point(195, 22), AutoSize = true, Text = "TVAC (TVA incluse)", TabIndex = tab++ };
            rbHtva.CheckedChanged += (s, e) => { if (rbHtva.Checked) MajPrix(); };
            rbTvac.CheckedChanged += (s, e) => { if (rbTvac.Checked) MajPrix(); };
            grp.Controls.Add(rbHtva);
            grp.Controls.Add(rbTvac);

            grp.Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(10, 52), Text = "Prix saisi (€/unité) *" });
            nudPrix = new NumericUpDown
            {
                Font          = font,
                Location      = new Point(10, 72),
                Size          = new Size(130, 26),
                DecimalPlaces = 4,
                Minimum       = 0,
                Maximum       = 999999,
                TabIndex      = tab++
            };
            nudPrix.ValueChanged += (s, e) => MajPrix();
            grp.Controls.Add(nudPrix);

            grp.Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(155, 52), Text = "TVA %" });
            nudTvaPct = new NumericUpDown
            {
                Font          = font,
                Location      = new Point(155, 72),
                Size          = new Size(72, 26),
                DecimalPlaces = 2,
                Minimum       = 0,
                Maximum       = 100,
                TabIndex      = tab++
            };
            nudTvaPct.ValueChanged += (s, e) => MajPrix();
            grp.Controls.Add(nudTvaPct);

            // Résultats HTVA / TVAC par unité
            lblPrixHtva = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(30, 110, 30), Location = new Point(10, 100), Text = "HTVA : —" };
            lblPrixTvac = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(180, 80, 10), Location = new Point(205, 100), Text = "TVAC : —" };
            grp.Controls.Add(lblPrixHtva);
            grp.Controls.Add(lblPrixTvac);

            // ── Totaux (hors GroupBox) ─────────────────────────────────────
            lblTotalHtva = new Label { AutoSize = true, Font = fontBold, ForeColor = Color.FromArgb(30, 110, 30), Location = new Point(lx, 395), Text = "Total HTVA : — €" };
            lblTotalTvac = new Label { AutoSize = true, Font = fontBold, ForeColor = Color.FromArgb(180, 80, 10), Location = new Point(215, 395), Text = "Total TVAC : — €" };
            Controls.Add(lblTotalHtva);
            Controls.Add(lblTotalTvac);

            // ── Réf. facture ───────────────────────────────────────────────
            Lbl("Réf. facture", lx, 425);
            txtRefFacture = new TextBox { Font = font, Location = new Point(lx, 447), Size = new Size(200, 26), TabIndex = tab++ };
            Controls.Add(txtRefFacture);

            // ── Date achat ─────────────────────────────────────────────────
            Lbl("Date achat *", lx, 483);
            dtpDateAchat = new DateTimePicker
            {
                Font     = font,
                Location = new Point(lx, 505),
                Size     = new Size(180, 26),
                Format   = DateTimePickerFormat.Short,
                TabIndex = tab++
            };
            Controls.Add(dtpDateAchat);

            // ── Péremption ─────────────────────────────────────────────────
            chkPeremption = new CheckBox { Font = font, Location = new Point(lx, 542), AutoSize = true, Text = "Date de péremption", TabIndex = tab++ };
            chkPeremption.CheckedChanged += (s, e) =>
            {
                dtpPeremption.Enabled = chkPeremption.Checked;
                if (chkPeremption.Checked && dtpPeremption.Value < DateTime.Today)
                    dtpPeremption.Value = DateTime.Today.AddMonths(6);
            };
            Controls.Add(chkPeremption);
            dtpPeremption = new DateTimePicker
            {
                Font     = font,
                Location = new Point(lx, 566),
                Size     = new Size(180, 26),
                Format   = DateTimePickerFormat.Short,
                Enabled  = false,
                TabIndex = tab++
            };
            Controls.Add(dtpPeremption);

            // ── Notes ──────────────────────────────────────────────────────
            Lbl("Notes", lx, 602);
            txtNotes = new TextBox
            {
                Font      = font,
                Location  = new Point(lx, 624),
                Size      = new Size(w, 50),
                Multiline = true,
                TabIndex  = tab++
            };
            Controls.Add(txtNotes);

            // ── Boutons ────────────────────────────────────────────────────
            btnEnregistrer = new Button
            {
                Font      = font,
                Location  = new Point(lx, 685),
                Size      = new Size(160, 36),
                Text      = "Enregistrer",
                BackColor = Color.FromArgb(61, 40, 23),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TabIndex  = tab++
            };
            btnEnregistrer.FlatAppearance.BorderSize = 0;
            btnEnregistrer.Click += BtnEnregistrer_Click;

            btnAnnuler = new Button
            {
                Font      = font,
                Location  = new Point(230, 685),
                Size      = new Size(155, 36),
                Text      = "Annuler",
                BackColor = Color.FromArgb(220, 215, 210),
                ForeColor = Color.FromArgb(61, 40, 23),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TabIndex  = tab++
            };
            btnAnnuler.FlatAppearance.BorderSize = 0;
            btnAnnuler.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnEnregistrer);
            Controls.Add(btnAnnuler);

            Load += FrmAchatEdit_Load;
        }

        // ── Chargement ─────────────────────────────────────────────────────

        private void FrmAchatEdit_Load(object sender, EventArgs e)
        {
            dtpDateAchat.Value    = DateTime.Today;
            dtpPeremption.Value   = DateTime.Today.AddMonths(6);
            chkPeremption.Checked = false;

            try
            {
                // Fournisseurs
                cmbFournisseur.Items.Add("— Aucun —");
                foreach (var f in FournisseurDAL.GetAll()) cmbFournisseur.Items.Add(f);
                cmbFournisseur.DisplayMember = "Nom";
                cmbFournisseur.SelectedIndex = 0;

                if (_isEdit)
                    PreremplirEdition();
                else
                    ChargerIngredients();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void ChargerIngredients()
        {
            var ingredients = IngredientDAL.GetAll(idActivite: _idActivite);
            cmbIngredient.DataSource    = ingredients;
            cmbIngredient.DisplayMember = "Nom";
            cmbIngredient.ValueMember   = "Id";
            if (ingredients.Count > 0) MajUniteIngredient();
        }

        private void PreremplirEdition()
        {
            // Initialiser _ingredientSelectionne depuis les données du lot pour les calculs live
            _ingredientSelectionne = new Ingredient
            {
                UniteMesure           = _lot.UniteMesure,
                ConditionnementLabel  = _lot.ConditionnementLabel,
                QteParConditionnement = _lot.QteParConditionnement
            };

            txtNumeroLot.Text    = _lot.NumeroLot ?? "";
            nudQuantite.Value    = _lot.NbConditionnements > 0 ? _lot.NbConditionnements : 1m;
            nudTvaPct.Value      = _lot.TvaPct;
            nudPrix.Value        = _lot.PrixUnitaire;   // HTVA par conditionnement
            rbHtva.Checked       = true;
            dtpDateAchat.Value   = _lot.DateAchat;
            txtRefFacture.Text   = _lot.ReferenceFacture ?? "";
            txtNotes.Text        = _lot.Notes ?? "";

            if (_lot.DatePeremption.HasValue)
            {
                chkPeremption.Checked = true;
                dtpPeremption.Value   = _lot.DatePeremption.Value;
            }

            // Sélectionner le fournisseur correspondant
            if (_lot.IdFournisseur.HasValue)
            {
                for (int i = 0; i < cmbFournisseur.Items.Count; i++)
                {
                    if (cmbFournisseur.Items[i] is Fournisseur f && f.Id == _lot.IdFournisseur.Value)
                    { cmbFournisseur.SelectedIndex = i; break; }
                }
            }

            MajPrix();
        }

        // ── Conditionnement de l'ingrédient sélectionné ───────────────────

        private void MajUniteIngredient()
        {
            if (cmbIngredient?.SelectedItem is Ingredient ing)
            {
                _ingredientSelectionne = ing;
                MajInfoConditionnement();
            }
            else
            {
                _ingredientSelectionne = null;
                lblUnite.Text = "—";
            }
            MajPrix();
        }

        private void MajInfoConditionnement()
        {
            if (_ingredientSelectionne == null) { lblUnite.Text = "—"; return; }
            decimal nbCond  = nudQuantite.Value;
            decimal qteBase = nbCond * _ingredientSelectionne.QteParConditionnement;
            string  unite   = _ingredientSelectionne.UniteMesure;
            lblUnite.Text   = nbCond > 0
                ? $"× {_ingredientSelectionne.QteParConditionnement:G} {unite}" +
                  $" = {qteBase:G} {unite} en stock"
                : $"Conditionnement : {_ingredientSelectionne.ConditionnementLabel}" +
                  $" ({_ingredientSelectionne.QteParConditionnement:G} {unite})";
        }

        // ── Calcul HTVA / TVAC en temps réel ──────────────────────────────

        private void MajPrix()
        {
            MajInfoConditionnement();

            decimal prixSaisi = nudPrix.Value;
            decimal tvaPct    = nudTvaPct.Value;
            decimal nbCond    = nudQuantite.Value;
            decimal facteur   = 1 + tvaPct / 100m;

            decimal prixHtva, prixTvac;

            if (rbHtva.Checked)
            {
                prixHtva = prixSaisi;
                prixTvac = prixSaisi * facteur;
            }
            else
            {
                prixTvac = prixSaisi;
                prixHtva = facteur != 0 ? prixSaisi / facteur : 0;
            }

            string labelCond = _ingredientSelectionne?.ConditionnementLabel ?? "cond.";

            if (prixSaisi > 0)
            {
                lblPrixHtva.Text  = $"HTVA : {prixHtva:F4} €/{labelCond}";
                lblPrixTvac.Text  = $"TVAC : {prixTvac:F4} €/{labelCond}";
                lblTotalHtva.Text = nbCond > 0 ? $"Total HTVA : {nbCond * prixHtva:F2} €" : "Total HTVA : — €";
                lblTotalTvac.Text = nbCond > 0 ? $"Total TVAC : {nbCond * prixTvac:F2} €" : "Total TVAC : — €";
            }
            else
            {
                lblPrixHtva.Text  = "HTVA : —";
                lblPrixTvac.Text  = "TVAC : —";
                lblTotalHtva.Text = "Total HTVA : — €";
                lblTotalTvac.Text = "Total TVAC : — €";
            }
        }

        // ── Validation et enregistrement ──────────────────────────────────

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool ok = true;

            if (!_isEdit && cmbIngredient?.SelectedItem == null)
            { errorProvider.SetError(cmbIngredient, "Choisissez un ingrédient."); ok = false; }

            if (nudQuantite.Value <= 0)
            { errorProvider.SetError(nudQuantite, "Quantité invalide."); ok = false; }

            if (nudPrix.Value <= 0)
            { errorProvider.SetError(nudPrix, "Prix invalide."); ok = false; }

            if (!ok) return;

            try
            {
                // Calcul HTVA final
                decimal tvaPct  = nudTvaPct.Value;
                decimal facteur = 1 + tvaPct / 100m;
                decimal prixHtva;

                if (rbHtva.Checked)
                    prixHtva = nudPrix.Value;
                else
                    prixHtva = facteur != 0 ? nudPrix.Value / facteur : nudPrix.Value;

                decimal nbCond = nudQuantite.Value;

                // Ingrédient : depuis la sélection (création) ou depuis le lot (édition)
                Ingredient ing = _isEdit ? null : (cmbIngredient?.SelectedItem as Ingredient);
                decimal qteParCond = ing?.QteParConditionnement ?? _lot.QteParConditionnement;

                _lot.NumeroLot         = txtNumeroLot.Text.Trim().NullIfEmpty();
                _lot.IdFournisseur     = cmbFournisseur.SelectedItem is Fournisseur f ? (int?)f.Id : null;
                _lot.DateAchat         = dtpDateAchat.Value;
                _lot.DatePeremption    = chkPeremption.Checked ? (DateTime?)dtpPeremption.Value : null;
                _lot.NbConditionnements = nbCond;
                _lot.QuantiteInitiale  = Math.Round(nbCond * qteParCond, 4);  // en unité de base (g/ml/pce)
                _lot.PrixUnitaire      = Math.Round(prixHtva, 4);              // €/conditionnement
                _lot.PrixAchatReel     = Math.Round(nbCond * prixHtva, 4);     // total HTVA
                _lot.TvaPct            = tvaPct;
                _lot.ReferenceFacture  = txtRefFacture.Text.Trim().NullIfEmpty();
                _lot.Notes             = txtNotes.Text.Trim().NullIfEmpty();

                if (!_isEdit)
                {
                    _lot.IdFicheIngredient = ing.Id;
                    LotDAL.Insert(_lot);
                }
                else
                {
                    LotDAL.Update(_lot);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
