using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'une fiche ingrédient.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// Synchronisation tripartite : €/unité ↔ €/conditionnement ↔ €/lot.
    /// </summary>
    public class FrmIngredientEdit : FrmEditBase
    {
        private readonly Ingredient _ing;
        private readonly bool       _isEdit;

        private readonly TextBox       txtNom;
        private readonly TextBox       txtMarque;
        private readonly TextBox       txtConditionnementLabel;
        private readonly NumericUpDown nudSeuil;
        private readonly NumericUpDown nudStockCible;
        private readonly ComboBox      cmbUnite;
        private readonly ComboBox      cmbTypePhysique;
        private readonly ComboBox      cmbFournisseur;
        private readonly ComboBox      cmbStockDefaut;
        private readonly NumericUpDown nudDensite;
        private readonly NumericUpDown nudPrixBase;
        private readonly NumericUpDown nudPrixCond;
        private readonly NumericUpDown nudNbLot;
        private readonly NumericUpDown nudPrixLot;
        private readonly NumericUpDown nudQteConditionnement;
        private readonly Label         lblDensite;
        private readonly Label         lblUniteQteCond;
        private readonly Label         lblPrixBaseLabel;
        private readonly Label         lblPrixRelation;

        private bool _syncing;

        public FrmIngredientEdit(Ingredient ing)
        {
            _isEdit = ing != null;
            _ing    = ing ?? new Ingredient { Actif = true };

            var font  = new Font("Segoe UI", 10F);
            var fontS = new Font("Segoe UI", 9F, FontStyle.Italic);
            ClientSize = new Size(415, 100);

            int lx = 20, lx2 = 215, wL = 175, wR = 170;
            int tab = 0;

            Label AddLabel(string text, Font f, int x, int y) =>
                new Label { AutoSize = true, Font = f, Location = new Point(x, y), Text = text };

            void AddField(string label, Control ctrl, int x, int y, int w, int h = 26)
            {
                Controls.Add(AddLabel(label, font, x, y));
                ctrl.Font     = font;
                ctrl.Location = new Point(x, y + 22);
                ctrl.Size     = new Size(w, h);
                if (ctrl is TextBox || ctrl is ComboBox || ctrl is NumericUpDown)
                    ctrl.TabIndex = tab++;
                Controls.Add(ctrl);
            }

            // ── Ligne 1 — Nom ─────────────────────────────────────────────
            txtNom = new TextBox();
            AddField("Nom *", txtNom, lx, 20, 360);

            // ── Ligne 2 — Marque ──────────────────────────────────────────
            txtMarque = new TextBox();
            AddField("Marque", txtMarque, lx, 68, 360);

            // ── Ligne 3 — Unité | Type physique ───────────────────────────
            cmbUnite        = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTypePhysique = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddField("Unité de base *",  cmbUnite,        lx,  116, wL);
            AddField("Type physique *",  cmbTypePhysique, lx2, 116, wR);

            // ── Ligne 4 — Label conditionnement | Densité ────────────────
            txtConditionnementLabel = new TextBox();
            AddField("Label conditionnement", txtConditionnementLabel, lx, 164, wL);

            lblDensite = AddLabel("Densité (g/ml) *", font, lx2, 164);
            nudDensite = new NumericUpDown
            {
                Font = font, Location = new Point(lx2, 186), Size = new Size(wR, 26),
                DecimalPlaces = 4,
                Minimum = new decimal(new[] { 1, 0, 0, 196608 }),  // 0.001
                Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                Value   = new decimal(new[] { 1, 0, 0, 0 }),
                TabIndex = tab++
            };
            Controls.Add(lblDensite);
            Controls.Add(nudDensite);

            // ── Ligne 5 — Conditionnement (NUD + label unité dynamique) ──
            Controls.Add(AddLabel("Qté / conditionnement *", font, lx, 214));
            nudQteConditionnement = new NumericUpDown
            {
                Font = font, Location = new Point(lx, 236), Size = new Size(110, 26),
                DecimalPlaces = 0, Minimum = 1m,
                Maximum = new decimal(new[] { 999999999, 0, 0, 0 }),
                Increment = 1m, Value = 1m,
                TabIndex = tab++
            };
            Controls.Add(nudQteConditionnement);

            lblUniteQteCond = new Label
            {
                AutoSize  = true,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location  = new Point(lx + 115, 240),
                Text      = "g"
            };
            Controls.Add(lblUniteQteCond);

            // Note d'aide densité
            Controls.Add(new Label
            {
                AutoSize  = true,
                Font      = fontS,
                ForeColor = Color.Gray,
                Location  = new Point(lx2, 214),
                Text      = "Obligatoire si liquide ou poudre"
            });

            // ── Ligne 6 — Prix / unité de base | Prix / conditionnement ──
            lblPrixBaseLabel = AddLabel("Prix / g (€)", font, lx, 262);
            Controls.Add(lblPrixBaseLabel);
            nudPrixBase = new NumericUpDown
            {
                Font = font, Location = new Point(lx, 284), Size = new Size(wL, 26),
                DecimalPlaces = 4, Minimum = 0,
                Maximum = new decimal(new[] { 999999, 0, 0, 0 }), Value = 0,
                TabIndex = tab++
            };
            Controls.Add(nudPrixBase);

            nudPrixCond = new NumericUpDown
            {
                DecimalPlaces = 4, Minimum = 0,
                Maximum = new decimal(new[] { 999999, 0, 0, 0 }), Value = 0
            };
            AddField("Prix / cond. (€)", nudPrixCond, lx2, 262, wR);

            // ── Ligne 7 — Nb cond. / lot | Prix du lot ──────────────────
            nudNbLot = new NumericUpDown
            {
                DecimalPlaces = 0, Minimum = 1m,
                Maximum = new decimal(new[] { 9999, 0, 0, 0 }),
                Increment = 1m, Value = 1m
            };
            AddField("Nb cond. / lot", nudNbLot, lx, 310, wL);

            nudPrixLot = new NumericUpDown
            {
                DecimalPlaces = 4, Minimum = 0,
                Maximum = new decimal(new[] { 999999, 0, 0, 0 }), Value = 0
            };
            AddField("Prix du lot (€)", nudPrixLot, lx2, 310, wR);

            // Label relation prix
            lblPrixRelation = new Label
            {
                AutoSize  = true,
                Font      = fontS,
                ForeColor = Color.FromArgb(100, 100, 100),
                Location  = new Point(lx, 354),
                Text      = ""
            };
            Controls.Add(lblPrixRelation);

            // ── Ligne 8 — Seuil alerte | Stock cible ────────────────────
            nudSeuil = new NumericUpDown
            {
                DecimalPlaces = 4, Minimum = 0,
                Maximum = new decimal(new[] { 999999, 0, 0, 0 }), Value = 0
            };
            AddField("Seuil alerte stock", nudSeuil, lx, 374, wL);
            nudStockCible = new NumericUpDown
            {
                DecimalPlaces = 0, Minimum = 0,
                Maximum = new decimal(new[] { 99999, 0, 0, 0 }),
                Increment = 1m, Value = 0
            };
            AddField("Stock cible (pièces)", nudStockCible, lx2, 374, wR);

            // ── Ligne 9 — Fournisseur ────────────────────────────────────
            cmbFournisseur = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddField("Fournisseur par défaut", cmbFournisseur, lx, 422, 360);

            // ── Ligne 10 — Stock de rangement par défaut ─────────────────
            cmbStockDefaut = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddField("Stock de rangement par défaut", cmbStockDefaut, lx, 470, 360);

            PositionnerBoutons(518);

            Load += FrmIngredientEdit_Load;
            Load += (s, e) => txtNom.Focus();
        }

        private void FrmIngredientEdit_Load(object sender, EventArgs e)
        {
            FormHelper.ActiverPointDecimal(nudPrixBase, nudPrixCond, nudPrixLot, nudQteConditionnement, nudDensite, nudStockCible, nudSeuil);
            FormHelper.ActiverSelectionAuFocus(nudPrixBase, nudPrixCond, nudPrixLot, nudQteConditionnement, nudDensite, nudStockCible, nudSeuil);
            Text = _isEdit ? "Modifier la fiche ingrédient" : "Nouvelle fiche ingrédient";

            cmbTypePhysique.Items.AddRange(new object[] { "solide", "liquide", "poudre", "piece" });
            cmbUnite.Items.AddRange(new object[] { "mg", "g", "kg", "ml", "cl", "dl", "l", "piece" });

            cmbTypePhysique.SelectedIndexChanged += (s, ev) => MettreAJourVisibiliteDensite();
            cmbUnite.SelectedIndexChanged        += (s, ev) =>
            {
                MajLabelUniteQteCond();
                MajLabelPrixBase();
                MajLabelPrixRelation();
            };

            // Synchronisation tripartite des prix
            nudPrixBase.ValueChanged           += (s, ev) => SyncFromBase();
            nudPrixCond.ValueChanged           += (s, ev) => SyncFromCond();
            nudPrixLot.ValueChanged            += (s, ev) => SyncFromLot();
            nudNbLot.ValueChanged              += (s, ev) => SyncFromCond();
            nudQteConditionnement.ValueChanged += (s, ev) => SyncFromBase();

            try
            {
                var fournisseurs = FournisseurDAL.GetAll();
                cmbFournisseur.Items.Add("— Aucun —");
                foreach (var f in fournisseurs) cmbFournisseur.Items.Add(f);
                cmbFournisseur.DisplayMember = "Nom";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chargement fournisseurs : " + ex.Message);
            }

            try
            {
                var stocks = StockDAL.GetAll();
                cmbStockDefaut.Items.Add("— Aucun —");
                foreach (var st in stocks) cmbStockDefaut.Items.Add(st);
                cmbStockDefaut.DisplayMember = "Nom";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chargement stocks : " + ex.Message);
            }

            if (_isEdit)
            {
                _syncing = true;
                try
                {
                    txtNom.Text                    = _ing.Nom;
                    txtMarque.Text                 = _ing.Marque ?? "";
                    txtConditionnementLabel.Text   = _ing.ConditionnementLabel ?? "";
                    cmbUnite.SelectedItem          = _ing.UniteMesure;
                    cmbTypePhysique.SelectedItem   = _ing.TypePhysique ?? "solide";
                    nudQteConditionnement.Value    = _ing.QteParConditionnement > 0 ? _ing.QteParConditionnement : 1m;
                    nudPrixCond.Value              = _ing.PrixAchatReference;
                    nudNbLot.Value                 = _ing.NbParLot > 0 ? _ing.NbParLot : 1m;
                    nudPrixLot.Value               = _ing.PrixAchatReference * nudNbLot.Value;

                    if (nudQteConditionnement.Value > 0)
                        nudPrixBase.Value = Math.Round(_ing.PrixAchatReference / nudQteConditionnement.Value, 4);

                    if (_ing.SeuilAlerteStock.HasValue)
                        nudSeuil.Value = _ing.SeuilAlerteStock.Value;
                    if (_ing.StockCible.HasValue && _ing.QteParConditionnement > 0)
                        nudStockCible.Value = Math.Round(_ing.StockCible.Value / _ing.QteParConditionnement);
                    else
                        nudStockCible.Value = 0;

                    if (_ing.Densite.HasValue) nudDensite.Value = _ing.Densite.Value;

                    if (_ing.IdFournisseurDefaut.HasValue)
                        foreach (var item in cmbFournisseur.Items)
                            if (item is Fournisseur f && f.Id == _ing.IdFournisseurDefaut.Value)
                            { cmbFournisseur.SelectedItem = f; break; }
                    else cmbFournisseur.SelectedIndex = 0;

                    if (_ing.IdStockDefaut.HasValue)
                        foreach (var item in cmbStockDefaut.Items)
                            if (item is Stock st && st.Id == _ing.IdStockDefaut.Value)
                            { cmbStockDefaut.SelectedItem = st; break; }
                    else cmbStockDefaut.SelectedIndex = 0;
                }
                finally
                {
                    _syncing = false;
                }
            }
            else
            {
                cmbTypePhysique.SelectedIndex = 0;
                cmbUnite.SelectedIndex        = 0;
                cmbFournisseur.SelectedIndex  = 0;
                cmbStockDefaut.SelectedIndex  = 0;
                nudQteConditionnement.Value   = 1m;
                nudNbLot.Value                = 1m;
            }

            MettreAJourVisibiliteDensite();
            MajLabelUniteQteCond();
            MajLabelPrixBase();
            MajLabelPrixRelation();
        }

        // ── Synchronisation tripartite des prix ─────────────────────────
        //
        //   prixBase × qtéCond = prixCond × nbLot = prixLot
        //
        //   Modifier prixBase → recalcule prixCond + prixLot
        //   Modifier prixCond → recalcule prixBase + prixLot
        //   Modifier prixLot  → recalcule prixCond + prixBase
        //   Modifier nbLot    → recalcule prixLot (prixCond reste)
        //   Modifier qtéCond  → recalcule prixCond + prixLot (prixBase reste)

        /// <summary>€/unité modifié → recalcule €/cond et €/lot.</summary>
        private void SyncFromBase()
        {
            if (_syncing) return;
            _syncing = true;
            try
            {
                nudPrixCond.Value = nudPrixBase.Value * nudQteConditionnement.Value;
                nudPrixLot.Value  = nudPrixCond.Value * nudNbLot.Value;
                MajLabelPrixRelation();
            }
            finally { _syncing = false; }
        }

        /// <summary>€/cond modifié (ou nbLot changé) → recalcule €/base et €/lot.</summary>
        private void SyncFromCond()
        {
            if (_syncing) return;
            _syncing = true;
            try
            {
                if (nudQteConditionnement.Value > 0)
                    nudPrixBase.Value = Math.Round(nudPrixCond.Value / nudQteConditionnement.Value, 4);
                nudPrixLot.Value = nudPrixCond.Value * nudNbLot.Value;
                MajLabelPrixRelation();
            }
            finally { _syncing = false; }
        }

        /// <summary>€/lot modifié → recalcule €/cond et €/base.</summary>
        private void SyncFromLot()
        {
            if (_syncing) return;
            _syncing = true;
            try
            {
                if (nudNbLot.Value > 0)
                    nudPrixCond.Value = Math.Round(nudPrixLot.Value / nudNbLot.Value, 4);
                if (nudQteConditionnement.Value > 0)
                    nudPrixBase.Value = Math.Round(nudPrixCond.Value / nudQteConditionnement.Value, 4);
                MajLabelPrixRelation();
            }
            finally { _syncing = false; }
        }

        /// <summary>Met à jour le label dynamique "Prix / kg (€)".</summary>
        private void MajLabelPrixBase()
        {
            string unite = cmbUnite.SelectedItem?.ToString() ?? "g";
            lblPrixBaseLabel.Text = $"Prix / {unite} (€)";
        }

        private void MajLabelPrixRelation()
        {
            string unite = cmbUnite.SelectedItem?.ToString() ?? "g";
            decimal prixBase = nudPrixBase.Value;
            decimal qteCond  = nudQteConditionnement.Value;
            decimal prixCond = nudPrixCond.Value;
            decimal nbLot    = nudNbLot.Value;
            decimal prixLot  = nudPrixLot.Value;

            if (prixCond <= 0) { lblPrixRelation.Text = ""; return; }

            string txt = $"{prixBase:0.00##} €/{unite}";
            txt += $"  ×  {qteCond:0.##} = {prixCond:0.00##} €/cond.";
            if (nbLot > 1)
                txt += $"  ×  {nbLot:0.##} = {prixLot:0.00##} €/lot";

            lblPrixRelation.Text = txt;
        }

        // ── Visibilité et labels dynamiques ──────────────────────────────

        private void MettreAJourVisibiliteDensite()
        {
            string type = cmbTypePhysique.SelectedItem?.ToString() ?? "solide";
            bool necessite = type == "liquide" || type == "poudre";
            lblDensite.Visible = necessite;
            nudDensite.Visible = necessite;
        }

        private void MajLabelUniteQteCond()
        {
            lblUniteQteCond.Text = cmbUnite.SelectedItem?.ToString() ?? "g";
        }

        protected override bool Valider()
        {
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Obligatoire."); ok = false; }
            else if (IngredientDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _ing.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); ok = false; }

            if (cmbUnite.SelectedItem == null)
            { errorProvider.SetError(cmbUnite, "Choisissez une unité de base."); ok = false; }

            if (nudQteConditionnement.Value <= 0)
            { errorProvider.SetError(nudQteConditionnement, "Doit être > 0."); ok = false; }

            if (cmbTypePhysique.SelectedItem == null)
            { errorProvider.SetError(cmbTypePhysique, "Choisissez un type physique."); ok = false; }

            string typeSel = cmbTypePhysique.SelectedItem?.ToString() ?? "solide";
            if ((typeSel == "liquide" || typeSel == "poudre") && nudDensite.Value <= 0)
            { errorProvider.SetError(nudDensite, "La densité est obligatoire pour ce type."); ok = false; }

            return ok;
        }

        protected override void Sauvegarder()
        {
            decimal? seuil = nudSeuil.Value > 0 ? (decimal?)nudSeuil.Value : null;

            decimal? stockCible = null;
            if (nudStockCible.Value > 0)
                stockCible = nudStockCible.Value * nudQteConditionnement.Value;

            string typeSel = cmbTypePhysique.SelectedItem.ToString();

            _ing.Nom                   = txtNom.Text.Trim();
            _ing.Marque                = txtMarque.Text.Trim().NullIfEmpty();
            _ing.UniteMesure           = cmbUnite.SelectedItem.ToString();
            _ing.TypePhysique          = typeSel;
            _ing.Densite               = (typeSel == "liquide" || typeSel == "poudre")
                                         ? (decimal?)nudDensite.Value : null;
            _ing.ConditionnementLabel  = txtConditionnementLabel.Text.Trim().NullIfEmpty();
            _ing.QteParConditionnement = nudQteConditionnement.Value;
            _ing.NbParLot              = (int)nudNbLot.Value;
            _ing.PrixAchatReference    = nudPrixCond.Value;
            _ing.SeuilAlerteStock      = seuil;
            _ing.StockCible            = stockCible;
            _ing.IdFournisseurDefaut   = cmbFournisseur.SelectedItem is Fournisseur f  ? (int?)f.Id  : null;
            _ing.IdStockDefaut        = cmbStockDefaut.SelectedItem  is Stock      st ? (int?)st.Id : null;

            if (_isEdit) IngredientDAL.Update(_ing);
            else         IngredientDAL.Insert(_ing);
        }
    }
}
