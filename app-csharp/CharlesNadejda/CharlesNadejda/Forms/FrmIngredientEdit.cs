using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'une fiche ingrédient.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmIngredientEdit : FrmEditBase
    {
        private readonly Ingredient _ing;
        private readonly bool       _isEdit;

        private readonly TextBox       txtNom;
        private readonly TextBox       txtMarque;
        private readonly TextBox       txtSeuil;
        private readonly NumericUpDown nudStockCible;
        private readonly ComboBox      cmbUnite;
        private readonly ComboBox      cmbTypePhysique;
        private readonly ComboBox      cmbFournisseur;
        private readonly ComboBox      cmbStock;
        private readonly NumericUpDown nudDensite;
        private readonly NumericUpDown nudPrix;
        private readonly NumericUpDown nudQteConditionnement;
        private readonly Label         lblDensite;
        private readonly Label         lblUniteQteCond;

        public FrmIngredientEdit(Ingredient ing, Stock stockDefaut = null)
        {
            _isEdit = ing != null;
            _ing    = ing ?? new Ingredient
            {
                IdStock  = stockDefaut?.Id  ?? 0,
                StockNom = stockDefaut?.Nom ?? "",
                Actif    = true
            };

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

            // ── Ligne 4 — Prix référence | Densité ────────────────────────
            nudPrix = new NumericUpDown
            {
                DecimalPlaces = 4, Minimum = 0,
                Maximum = new decimal(new[] { 999999, 0, 0, 0 }), Value = 0
            };
            AddField("Prix réf. (€/conditionnement)", nudPrix, lx, 164, wL);

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
            Controls.Add(AddLabel("Conditionnement *", font, lx, 214));
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

            // ── Ligne 6 — Seuil alerte ────────────────────────────────────
            txtSeuil = new TextBox();
            AddField("Seuil alerte stock", txtSeuil, lx, 262, wL);
            nudStockCible = new NumericUpDown
            {
                DecimalPlaces = 0, Minimum = 0,
                Maximum = new decimal(new[] { 99999, 0, 0, 0 }),
                Increment = 1m, Value = 0
            };
            AddField("Stock cible (pièces)", nudStockCible, lx2, 262, wR);

            // ── Ligne 7 — Fournisseur ─────────────────────────────────────
            cmbFournisseur = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddField("Fournisseur par défaut", cmbFournisseur, lx, 310, 360);

            // ── Ligne 8 — Stock ───────────────────────────────────────────
            cmbStock = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddField("Stock *", cmbStock, lx, 358, 360);

            PositionnerBoutons(414);

            Load += FrmIngredientEdit_Load;
        }

        private void FrmIngredientEdit_Load(object sender, EventArgs e)
        {
            FormHelper.ActiverPointDecimal(nudPrix, nudQteConditionnement, nudDensite, nudStockCible);
            FormHelper.ActiverSelectionAuFocus(nudPrix, nudQteConditionnement, nudDensite, nudStockCible);
            Text = _isEdit ? "Modifier la fiche ingrédient" : "Nouvelle fiche ingrédient";

            cmbTypePhysique.Items.AddRange(new object[] { "solide", "liquide", "poudre", "piece" });
            cmbUnite.Items.AddRange(new object[] { "mg", "g", "kg", "ml", "cl", "dl", "l", "piece" });

            cmbTypePhysique.SelectedIndexChanged += (s, ev) => MettreAJourVisibiliteDensite();
            cmbUnite.SelectedIndexChanged        += (s, ev) => MajLabelUniteQteCond();

            var stocks = StockDAL.GetAll();
            cmbStock.Items.Clear();
            foreach (var s in stocks) cmbStock.Items.Add(s);
            cmbStock.DisplayMember = "Nom";

            try
            {
                var fournisseurs = FournisseurDAL.GetAll();
                cmbFournisseur.Items.Add("— Aucun —");
                foreach (var f in fournisseurs) cmbFournisseur.Items.Add(f);
                cmbFournisseur.DisplayMember = "Nom";
            }
            catch { }

            if (_isEdit)
            {
                txtNom.Text                  = _ing.Nom;
                txtMarque.Text               = _ing.Marque ?? "";
                cmbUnite.SelectedItem        = _ing.UniteMesure;
                cmbTypePhysique.SelectedItem = _ing.TypePhysique ?? "solide";
                nudPrix.Value                = _ing.PrixAchatReference;
                nudQteConditionnement.Value  = _ing.QteParConditionnement > 0 ? _ing.QteParConditionnement : 1m;
                txtSeuil.Text = _ing.SeuilAlerteStock.HasValue
                    ? _ing.SeuilAlerteStock.Value.ToString("F4") : "";
                if (_ing.StockCible.HasValue && _ing.QteParConditionnement > 0)
                    nudStockCible.Value = Math.Round(_ing.StockCible.Value / _ing.QteParConditionnement);
                else
                    nudStockCible.Value = 0;

                foreach (var item in cmbStock.Items)
                    if (((Stock)item).Id == _ing.IdStock) { cmbStock.SelectedItem = item; break; }

                if (_ing.Densite.HasValue) nudDensite.Value = _ing.Densite.Value;

                if (_ing.IdFournisseurDefaut.HasValue)
                    foreach (var item in cmbFournisseur.Items)
                        if (item is Fournisseur f && f.Id == _ing.IdFournisseurDefaut.Value)
                        { cmbFournisseur.SelectedItem = f; break; }
                else cmbFournisseur.SelectedIndex = 0;
            }
            else
            {
                cmbTypePhysique.SelectedIndex = 0;
                cmbUnite.SelectedIndex        = 0;
                cmbFournisseur.SelectedIndex  = 0;
                nudQteConditionnement.Value   = 1m;

                if (_ing.IdStock > 0)
                    foreach (var item in cmbStock.Items)
                        if (((Stock)item).Id == _ing.IdStock) { cmbStock.SelectedItem = item; break; }
                else if (cmbStock.Items.Count > 0)
                    cmbStock.SelectedIndex = 0;
            }

            MettreAJourVisibiliteDensite();
            MajLabelUniteQteCond();
        }

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

            if (cmbStock.SelectedItem == null)
            { errorProvider.SetError(cmbStock, "Choisissez un stock."); ok = false; }

            string typeSel = cmbTypePhysique.SelectedItem?.ToString() ?? "solide";
            if ((typeSel == "liquide" || typeSel == "poudre") && nudDensite.Value <= 0)
            { errorProvider.SetError(nudDensite, "La densité est obligatoire pour ce type."); ok = false; }

            return ok;
        }

        protected override void Sauvegarder()
        {
            decimal? seuil = null;
            if (!string.IsNullOrWhiteSpace(txtSeuil.Text)
                && decimal.TryParse(txtSeuil.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal s))
                seuil = s;

            decimal? stockCible = null;
            if (nudStockCible.Value > 0)
                stockCible = nudStockCible.Value * nudQteConditionnement.Value;

            string typeSel = cmbTypePhysique.SelectedItem.ToString();
            var stockSel   = (Stock)cmbStock.SelectedItem;

            _ing.Nom                   = txtNom.Text.Trim();
            _ing.Marque                = txtMarque.Text.Trim().NullIfEmpty();
            _ing.UniteMesure           = cmbUnite.SelectedItem.ToString();
            _ing.TypePhysique          = typeSel;
            _ing.Densite               = (typeSel == "liquide" || typeSel == "poudre")
                                         ? (decimal?)nudDensite.Value : null;
            _ing.ConditionnementLabel  = null;
            _ing.QteParConditionnement = nudQteConditionnement.Value;
            _ing.PrixAchatReference    = nudPrix.Value;
            _ing.SeuilAlerteStock      = seuil;
            _ing.StockCible            = stockCible;
            _ing.IdStock               = stockSel.Id;
            _ing.StockNom              = stockSel.Nom;
            _ing.IdFournisseurDefaut   = cmbFournisseur.SelectedItem is Fournisseur f ? (int?)f.Id : null;

            if (_isEdit) IngredientDAL.Update(_ing);
            else         IngredientDAL.Insert(_ing);
        }
    }
}
