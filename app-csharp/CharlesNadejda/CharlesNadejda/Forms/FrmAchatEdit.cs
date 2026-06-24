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
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmAchatEdit : FrmEditBase
    {
        // --- Champs privés ---

        // Le lot qu'on édite (existant en modif, vide en création)
        private readonly Lot        _lot;
        // true = on modifie un lot existant, false = on en crée un nouveau
        private readonly bool       _isEdit;
        // ID de l'activité liée (pas toujours utilisé, mais gardé pour le contexte métier)
        private readonly int        _idActivite;
        // Si on vient d'une autre vue avec un ingrédient déjà choisi, je le préselectionne dans la combo
        private readonly int        _idIngredientPreselect;
        // L'ingrédient actuellement sélectionné dans la combo (ou reconstitué en mode édition)
        private          Ingredient _ingredientSelectionne;

        // --- Contrôles du formulaire ---

        // Combo de sélection d'ingrédient (visible uniquement en création)
        private readonly ComboBox       cmbIngredient;
        // Label lecture seule pour afficher le nom de l'ingrédient en mode édition
        private readonly Label          lblIngredientRO;
        // Label d'info qui affiche l'unité / le détail du conditionnement sous la quantité
        private readonly Label          lblUnite;
        // Combo fournisseur (optionnel, "— Aucun —" par défaut)
        private readonly ComboBox       cmbFournisseur;
        // Champ texte pour le numéro de lot (facultatif, traçabilité)
        private readonly TextBox        txtNumeroLot;
        // Mode d'achat : je laisse le choix entre acheter par conditionnement ou par lot
        private readonly RadioButton    rbModeCond;
        private readonly RadioButton    rbModeLot;
        // Petit label italique qui explique ce que représente le mode choisi (ex: "1 lot = 6 × bouteille 1L")
        private readonly Label          lblModeInfo;
        // Quantité saisie (nombre de conditionnements ou de lots selon le mode)
        private readonly NumericUpDown  nudQuantite;
        // Choix de saisie du prix : soit HTVA (hors taxe), soit TVAC (toutes taxes comprises)
        private readonly RadioButton    rbHtva;
        private readonly RadioButton    rbTvac;
        // Champ prix unitaire saisi par l'utilisateur
        private readonly NumericUpDown  nudPrix;
        // Pourcentage de TVA applicable
        private readonly NumericUpDown  nudTvaPct;
        // Labels d'affichage des prix calculés (unitaire HTVA/TVAC + totaux)
        private readonly Label          lblPrixHtva;
        private readonly Label          lblPrixTvac;
        private readonly Label          lblTotalHtva;
        private readonly Label          lblTotalTvac;
        // Référence facture (optionnel, pour la comptabilité)
        private readonly TextBox        txtRefFacture;
        // Date d'achat du lot
        private readonly DateTimePicker dtpDateAchat;
        // Checkbox + DatePicker pour la date de péremption (optionnelle)
        private readonly CheckBox       chkPeremption;
        private readonly DateTimePicker dtpPeremption;
        // Combo pour choisir dans quel stock physique ranger le lot
        private readonly ComboBox       cmbStock;
        // Zone de notes libres
        private readonly TextBox        txtNotes;

        // ====================================================================
        // Constructeur : je construis toute l'interface programmatiquement ici.
        // lot = null → mode création, lot fourni → mode édition.
        // idIngredientPreselect permet de présélectionner un ingrédient si on vient
        // d'une autre vue (ex: on cliquait sur un ingrédient avant d'ouvrir ce form).
        // ====================================================================
        public FrmAchatEdit(Lot lot, int idActivite = 0, int idIngredientPreselect = 0)
        {
            _isEdit                = lot != null;
            _lot                   = lot ?? new Lot();
            _idActivite            = idActivite;
            _idIngredientPreselect = idIngredientPreselect;

            // Polices de base pour tout le formulaire
            var font     = new Font("Segoe UI", 10F);
            var fontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
            ClientSize = new Size(415, 100);

            // lx = marge gauche, w = largeur standard des contrôles, tab = compteur TabIndex
            int lx = 15, w = 370, tab = 0;

            // Petite fonction locale pour éviter de répéter le code de création de label
            Label AddLabel(string text, int x, int y)
            {
                var l = new Label { AutoSize = true, Font = font, Location = new Point(x, y), Text = text };
                Controls.Add(l);
                return l;
            }

            // ── Ingrédient ────────────────────────────────────────────────
            // En édition : on affiche juste le nom en lecture seule (pas question de changer l'ingrédient d'un lot existant)
            // En création : combo déroulante pour choisir l'ingrédient
            AddLabel(_isEdit ? "Ingrédient" : "Ingrédient *", lx, 12);
            if (_isEdit)
            {
                lblIngredientRO = new Label
                {
                    AutoSize = false, Font = font,
                    Location = new Point(lx, 34), Size = new Size(w, 26),
                    Text = _lot.NomIngredient, BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.FromArgb(80, 80, 80)
                };
                Controls.Add(lblIngredientRO);
                cmbIngredient = null;
            }
            else
            {
                cmbIngredient = new ComboBox
                {
                    Font = font, Location = new Point(lx, 34), Size = new Size(w, 26),
                    DropDownStyle = ComboBoxStyle.DropDownList, TabIndex = tab++
                };
                // Dès qu'on change d'ingrédient, je mets à jour l'unité affichée et le prix de référence
                cmbIngredient.SelectedIndexChanged += (s, e) => MajUniteIngredient();
                Controls.Add(cmbIngredient);
                lblIngredientRO = null;
            }

            // ── Fournisseur ───────────────────────────────────────────────
            // Optionnel : on peut enregistrer un achat sans fournisseur
            AddLabel("Fournisseur", lx, 70);
            cmbFournisseur = new ComboBox
            {
                Font = font, Location = new Point(lx, 92), Size = new Size(w, 26),
                DropDownStyle = ComboBoxStyle.DropDownList, TabIndex = tab++
            };
            Controls.Add(cmbFournisseur);

            // ── Stock (lieu de rangement) ────────────────────────────────
            // Obligatoire : chaque lot doit être rangé quelque part (frigo, réserve, etc.)
            AddLabel("Stock (lieu) *", lx, 128);
            cmbStock = new ComboBox
            {
                Font = font, Location = new Point(lx, 150), Size = new Size(w, 26),
                DropDownStyle = ComboBoxStyle.DropDownList, TabIndex = tab++
            };
            Controls.Add(cmbStock);

            // ── N° lot ────────────────────────────────────────────────────
            // Facultatif : numéro de lot du fournisseur, utile pour la traçabilité
            AddLabel("N° lot (facultatif)", lx, 186);
            txtNumeroLot = new TextBox { Font = font, Location = new Point(lx, 208), Size = new Size(200, 26), TabIndex = tab++ };
            Controls.Add(txtNumeroLot);

            // ── Mode d'achat : Conditionnement ou Lot ────────────────────
            // Deux modes possibles :
            // - "Conditionnement" : j'achète X unités de conditionnement (ex: 3 bouteilles)
            // - "Lot" : j'achète X lots, chaque lot contenant N conditionnements (ex: 2 cartons de 6 bouteilles)
            // Le label en-dessous s'adapte pour montrer ce que ça représente concrètement
            AddLabel("Acheter par *", lx, 244);
            rbModeCond = new RadioButton
            {
                Text = "Conditionnement", Font = new Font("Segoe UI", 9F),
                Location = new Point(lx, 264), AutoSize = true, Checked = true, TabIndex = tab++
            };
            rbModeLot = new RadioButton
            {
                Text = "Lot", Font = new Font("Segoe UI", 9F),
                Location = new Point(lx + 160, 264), AutoSize = true, TabIndex = tab++
            };
            lblModeInfo = new Label
            {
                AutoSize = false, Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 80, 60),
                Location = new Point(lx, 286), Size = new Size(w, 16), Text = ""
            };
            // Quand on change de mode, je recalcule le label explicatif et les prix
            rbModeCond.CheckedChanged += (s, e) => { MajModeLabel(); MajPrix(); };
            rbModeLot.CheckedChanged  += (s, e) => { MajModeLabel(); MajPrix(); };
            Controls.Add(rbModeCond);
            Controls.Add(rbModeLot);
            Controls.Add(lblModeInfo);

            // ── Quantité ──────────────────────────────────────────────────
            // Nombre de conditionnements (ou de lots si mode lot) à acheter
            AddLabel("Quantité *", lx, 306);
            nudQuantite = new NumericUpDown
            {
                Font = font, Location = new Point(lx, 328), Size = new Size(120, 26),
                DecimalPlaces = 0, Minimum = 1, Maximum = 99999, TabIndex = tab++
            };
            // Chaque changement de quantité déclenche le recalcul des prix
            nudQuantite.ValueChanged += (s, e) => MajPrix();
            Controls.Add(nudQuantite);

            // Label sous la quantité : résumé en texte de ce que ça donne (ex: "3 cond. × 1 kg = 3 kg en stock")
            lblUnite = new Label
            {
                AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(60, 110, 60),
                Location = new Point(lx, 356), Size = new Size(w, 20), Text = "—"
            };
            Controls.Add(lblUnite);

            // ── GroupBox prix ─────────────────────────────────────────────
            // Toute la zone de saisie du prix est regroupée dans un GroupBox
            // L'utilisateur choisit s'il saisit en HTVA ou TVAC, et je calcule l'autre automatiquement
            var grp = new GroupBox
            {
                Font = font, Location = new Point(12, 382), Size = new Size(390, 120),
                Text = "Saisie du prix (€ par conditionnement)"
            };
            Controls.Add(grp);

            // Radio HTVA / TVAC : détermine comment interpréter le prix saisi
            rbHtva = new RadioButton { Font = font, Location = new Point(10, 22), AutoSize = true, Text = "HTVA (hors taxe)", Checked = true, TabIndex = tab++ };
            rbTvac = new RadioButton { Font = font, Location = new Point(195, 22), AutoSize = true, Text = "TVAC (TVA incluse)", TabIndex = tab++ };
            rbHtva.CheckedChanged += (s, e) => { if (rbHtva.Checked) MajPrix(); };
            rbTvac.CheckedChanged += (s, e) => { if (rbTvac.Checked) MajPrix(); };
            grp.Controls.Add(rbHtva);
            grp.Controls.Add(rbTvac);

            // Champ prix unitaire : 4 décimales pour être précis (important en pâtisserie, les ingrédients peuvent coûter des centimes)
            grp.Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(10, 52), Text = "Prix saisi (€/unité) *" });
            nudPrix = new NumericUpDown
            {
                Font = font, Location = new Point(10, 72), Size = new Size(130, 26),
                DecimalPlaces = 4, Minimum = 0, Maximum = 999999, TabIndex = tab++
            };
            nudPrix.ValueChanged += (s, e) => MajPrix();
            grp.Controls.Add(nudPrix);

            // Champ TVA % : le taux de TVA applicable (6%, 21%, etc.)
            grp.Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(155, 52), Text = "TVA %" });
            nudTvaPct = new NumericUpDown
            {
                Font = font, Location = new Point(155, 72), Size = new Size(72, 26),
                DecimalPlaces = 2, Minimum = 0, Maximum = 100, TabIndex = tab++
            };
            nudTvaPct.ValueChanged += (s, e) => MajPrix();
            grp.Controls.Add(nudTvaPct);

            // Labels de résultat : affichent le prix calculé HTVA et TVAC par conditionnement
            lblPrixHtva = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(30, 110, 30), Location = new Point(10, 100), Text = "HTVA : —" };
            lblPrixTvac = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(180, 80, 10), Location = new Point(205, 100), Text = "TVAC : —" };
            grp.Controls.Add(lblPrixHtva);
            grp.Controls.Add(lblPrixTvac);

            // ── Totaux ────────────────────────────────────────────────────
            // Les totaux en gras : prix unitaire × nombre de conditionnements, en HTVA et TVAC
            lblTotalHtva = new Label { AutoSize = true, Font = fontBold, ForeColor = Color.FromArgb(30, 110, 30), Location = new Point(lx, 515), Text = "Total HTVA : — €" };
            lblTotalTvac = new Label { AutoSize = true, Font = fontBold, ForeColor = Color.FromArgb(180, 80, 10), Location = new Point(215, 515), Text = "Total TVAC : — €" };
            Controls.Add(lblTotalHtva);
            Controls.Add(lblTotalTvac);

            // ── Réf. facture ──────────────────────────────────────────────
            // Optionnel : numéro ou référence de la facture d'achat
            AddLabel("Réf. facture", lx, 545);
            txtRefFacture = new TextBox { Font = font, Location = new Point(lx, 567), Size = new Size(200, 26), TabIndex = tab++ };
            Controls.Add(txtRefFacture);

            // ── Date achat ────────────────────────────────────────────────
            // Obligatoire : la date à laquelle l'achat a été fait
            AddLabel("Date achat *", lx, 603);
            dtpDateAchat = new DateTimePicker
            {
                Font = font, Location = new Point(lx, 625), Size = new Size(180, 26),
                Format = DateTimePickerFormat.Short, TabIndex = tab++
            };
            Controls.Add(dtpDateAchat);

            // ── Péremption ────────────────────────────────────────────────
            // Optionnel : si la checkbox est cochée, le DatePicker s'active et on peut saisir la date de péremption
            chkPeremption = new CheckBox { Font = font, Location = new Point(lx, 662), AutoSize = true, Text = "Date de péremption", TabIndex = tab++ };
            dtpPeremption = new DateTimePicker
            {
                Font = font, Location = new Point(lx, 686), Size = new Size(180, 26),
                Format = DateTimePickerFormat.Short, Enabled = false, TabIndex = tab++
            };
            // Quand je coche la péremption, j'active le champ date ; si la date est dans le passé, je mets +6 mois par défaut
            chkPeremption.CheckedChanged += (s, e) =>
            {
                dtpPeremption.Enabled = chkPeremption.Checked;
                if (chkPeremption.Checked && dtpPeremption.Value < DateTime.Today)
                    dtpPeremption.Value = DateTime.Today.AddMonths(6);
            };
            Controls.Add(chkPeremption);
            Controls.Add(dtpPeremption);

            // ── Notes ─────────────────────────────────────────────────────
            // Zone de texte libre pour des remarques sur l'achat
            AddLabel("Notes", lx, 722);
            txtNotes = new TextBox
            {
                Font = font, Location = new Point(lx, 744), Size = new Size(w, 50),
                Multiline = true, TabIndex = tab++
            };
            Controls.Add(txtNotes);

            // Place les boutons OK/Annuler de FrmEditBase à la bonne hauteur
            PositionnerBoutons(805);

            // ── Bouton raccourci "＋ Nouvelle Fiche" (haut droite) ────
            // Le bouton Nouvelle Fiche ferme ce form et signale à l'appelant d'ouvrir FrmIngredientEdit
            // via DialogResult.Retry (convention maison). Visible uniquement en mode création.
            if (!_isEdit)
            {
                var btnNouvFiche = new Button
                {
                    Text = "＋ Nouvelle Fiche",
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(70, 130, 180),
                    ForeColor = Color.White,
                    Size = new Size(110, 22),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnNouvFiche.FlatAppearance.BorderColor = Color.FromArgb(50, 100, 150);
                btnNouvFiche.Location = new Point(ClientSize.Width - 145, 8);
                var tt = new ToolTip();
                tt.SetToolTip(btnNouvFiche, "Créer une fiche ingrédient puis revenir");
                btnNouvFiche.Click += (s, ev) =>
                {
                    DialogResult = DialogResult.Retry;  // signal pour l'appelant
                    Close();
                };
                Controls.Add(btnNouvFiche);
                btnNouvFiche.BringToFront();
            }

            // Helpers UX : le point décimal fonctionne sur le pavé numérique, et cliquer dans un champ sélectionne tout
            FormHelper.ActiverPointDecimal(nudQuantite, nudPrix, nudTvaPct);
            FormHelper.ActiverSelectionAuFocus(nudQuantite, nudPrix, nudTvaPct);

            // Titre du formulaire adapté au mode
            Text = _isEdit ? "Modifier un achat" : "Nouvel achat";
            Load += FrmAchatEdit_Load;
        }

        // ====================================================================
        // FrmAchatEdit_Load : appelé au chargement du form.
        // J'initialise les dates par défaut, je charge les combos (fournisseurs, stocks, ingrédients)
        // et je préremplis les champs si on est en mode édition.
        // ====================================================================
        private void FrmAchatEdit_Load(object sender, EventArgs e)
        {
            // Valeurs par défaut pour les dates
            dtpDateAchat.Value    = DateTime.Today;
            dtpPeremption.Value   = DateTime.Today.AddMonths(6);
            chkPeremption.Checked = false;

            try
            {
                // Remplir la combo fournisseur avec "— Aucun —" en premier choix
                cmbFournisseur.Items.Add("— Aucun —");
                foreach (var f in FournisseurDAL.GetAll()) cmbFournisseur.Items.Add(f);
                cmbFournisseur.DisplayMember = "Nom";
                cmbFournisseur.SelectedIndex = 0;

                // Charger uniquement les stocks liés à l'activité courante
                var stocks = _idActivite > 0
                    ? StockDAL.GetByActivite(_idActivite)
                    : StockDAL.GetAll();
                foreach (var s in stocks) cmbStock.Items.Add(s);
                cmbStock.DisplayMember = "Nom";
                if (cmbStock.Items.Count > 0) cmbStock.SelectedIndex = 0;

                // En édition : je préremplis tout avec les données du lot existant
                // En création : je charge la liste des ingrédients dans la combo
                if (_isEdit) PreremplirEdition();
                else         ChargerIngredients();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        // ====================================================================
        // ChargerIngredients : remplit la combo ingrédient avec tous les ingrédients de la DB.
        // Si on a un idIngredientPreselect (venant de l'appelant), je le sélectionne directement.
        // Puis je mets à jour l'unité affichée.
        // ====================================================================
        private void ChargerIngredients()
        {
            // Ingrédients éligibles à l'achat dans ce contexte d'activité :
            //   - fiches sans aucun lot (nouveau : Vodka jamais achetée) → incluses
            //   - fiches avec lots dans les stocks de cette activité → incluses
            //   - fiches avec lots uniquement dans d'autres activités → exclues
            var ingredients = _idActivite > 0
                ? IngredientDAL.GetAllForAchat(_idActivite)
                : IngredientDAL.GetAll();
            cmbIngredient.DataSource    = ingredients;
            cmbIngredient.DisplayMember = "Nom";
            cmbIngredient.ValueMember   = "Id";
            // Si un ingrédient est présélectionné, je le pointe dans la combo
            if (_idIngredientPreselect > 0)
                cmbIngredient.SelectedValue = _idIngredientPreselect;
            if (ingredients.Count > 0) MajUniteIngredient();
        }

        // ====================================================================
        // PreremplirEdition : en mode édition, je reconstitue les infos de l'ingrédient
        // depuis le lot (on n'a pas la fiche complète, juste les champs utiles),
        // puis je remplis tous les champs du form avec les valeurs existantes du lot.
        // ====================================================================
        private void PreremplirEdition()
        {
            // Je reconstitue un objet Ingredient minimal à partir des données du lot
            // (le lot stocke l'unité et le conditionnement, pas besoin de recharger la fiche complète)
            _ingredientSelectionne = new Ingredient
            {
                UniteMesure           = _lot.UniteMesure,
                ConditionnementLabel  = _lot.ConditionnementLabel,
                QteParConditionnement = _lot.QteParConditionnement
            };

            // Remplissage de tous les champs avec les valeurs du lot existant
            txtNumeroLot.Text    = _lot.NumeroLot ?? "";
            nudQuantite.Value    = _lot.NbConditionnements > 0 ? _lot.NbConditionnements : 1m;
            nudTvaPct.Value      = _lot.TvaPct;
            nudPrix.Value        = _lot.PrixUnitaire;
            rbHtva.Checked       = true;
            dtpDateAchat.Value   = _lot.DateAchat;
            txtRefFacture.Text   = _lot.ReferenceFacture ?? "";
            txtNotes.Text        = _lot.Notes ?? "";

            // Si le lot a une date de péremption, j'active la checkbox et je la remplis
            if (_lot.DatePeremption.HasValue)
            {
                chkPeremption.Checked = true;
                dtpPeremption.Value   = _lot.DatePeremption.Value;
            }

            // TICKET-27 : pré-sélection du fournisseur et du stock via helper générique
            // (évite de boucler manuellement sur les items de la combo)
            if (_lot.IdFournisseur.HasValue)
                FormHelper.SelectionnerParId<Fournisseur>(cmbFournisseur, f => f.Id, _lot.IdFournisseur.Value);

            if (_lot.IdStock > 0)
                FormHelper.SelectionnerParId<Stock>(cmbStock, s => s.Id, _lot.IdStock);

            // Recalcul de l'affichage des prix avec les valeurs chargées
            MajPrix();
        }

        // ====================================================================
        // MajUniteIngredient : appelé quand on change d'ingrédient dans la combo.
        // Je récupère l'ingrédient sélectionné, je mets à jour le label du mode,
        // l'info conditionnement, et si l'ingrédient a un prix de référence, je le préremplis.
        // ====================================================================
        private void MajUniteIngredient()
        {
            if (cmbIngredient?.SelectedItem is Ingredient ing)
            {
                _ingredientSelectionne = ing;
                MajModeLabel();
                MajInfoConditionnement();
                // Si l'ingrédient a un prix d'achat de référence, je le propose comme valeur par défaut
                if (ing.PrixAchatReference > 0)
                    nudPrix.Value = ing.PrixAchatReference;
            }
            else
            {
                _ingredientSelectionne = null;
                lblUnite.Text = "—";
                lblModeInfo.Text = "";
            }
            MajPrix();
        }

        // ====================================================================
        // MajModeLabel : met à jour le petit texte explicatif sous les radios Mode.
        // Explique concrètement ce que représente "1 unité" ou "1 lot" pour l'ingrédient sélectionné.
        // Ex: "1 lot = 6 × bouteille 1L" ou "1 unité = sac 25 kg"
        // ====================================================================
        private void MajModeLabel()
        {
            if (_ingredientSelectionne == null) { lblModeInfo.Text = ""; return; }
            var ing = _ingredientSelectionne;
            // Label du conditionnement : soit le label custom, soit "Qté Unité" par défaut
            string condLabel = ing.ConditionnementLabel ?? $"{ing.QteParConditionnement:G} {ing.UniteMesure}";
            if (rbModeLot.Checked && ing.NbParLot > 1)
                lblModeInfo.Text = $"1 lot = {ing.NbParLot} × {condLabel}";
            else if (rbModeLot.Checked)
                lblModeInfo.Text = $"1 lot = 1 × {condLabel} (pas de lot configuré)";
            else
                lblModeInfo.Text = $"1 unité = {condLabel}";
        }

        // ====================================================================
        // GetNbConditionnements : calcule le vrai nombre de conditionnements selon le mode choisi.
        // En mode "Conditionnement" : c'est directement la valeur saisie.
        // En mode "Lot" : je multiplie par NbParLot (ex: 2 lots × 6 = 12 conditionnements).
        // ====================================================================
        /// <summary>Nombre réel de conditionnements selon le mode (cond. ou lot).</summary>
        private decimal GetNbConditionnements()
        {
            decimal nbSaisi = nudQuantite.Value;
            if (rbModeLot.Checked && _ingredientSelectionne != null && _ingredientSelectionne.NbParLot > 1)
                return nbSaisi * _ingredientSelectionne.NbParLot;
            return nbSaisi;
        }

        // ====================================================================
        // MajInfoConditionnement : met à jour le label vert sous la quantité.
        // Affiche le détail du calcul : "3 cond. × 1 kg = 3 kg en stock"
        // Utilise UnitConvertisseur pour formater proprement les quantités selon l'unité.
        // ====================================================================
        private void MajInfoConditionnement()
        {
            if (_ingredientSelectionne == null) { lblUnite.Text = "—"; return; }
            decimal nbCond  = GetNbConditionnements();
            decimal qteBase = nbCond * _ingredientSelectionne.QteParConditionnement;
            string  unite   = _ingredientSelectionne.UniteMesure;
            string  qteFormatee = UnitConvertisseur.FormatQte(qteBase, unite);
            lblUnite.Text = nbCond > 0
                ? $"{nbCond:F0} cond. × {UnitConvertisseur.FormatQte(_ingredientSelectionne.QteParConditionnement, unite)} = {qteFormatee} en stock"
                : $"Conditionnement : {_ingredientSelectionne.ConditionnementLabel}";
        }

        // ====================================================================
        // MajPrix : le gros recalcul central, appelé dès qu'un champ prix/quantité/tva/mode change.
        // Selon que l'utilisateur saisit en HTVA ou TVAC, je calcule l'autre sens,
        // puis j'affiche les prix unitaires ET les totaux (unitaire × nb conditionnements).
        // ====================================================================
        private void MajPrix()
        {
            // Je mets aussi à jour l'info conditionnement (qui dépend de la quantité)
            MajInfoConditionnement();

            decimal prixSaisi = nudPrix.Value;
            decimal tvaPct    = nudTvaPct.Value;
            decimal nbCond    = GetNbConditionnements();
            // Facteur multiplicateur TVA (ex: 1.21 pour 21%)
            decimal facteur   = 1 + tvaPct / 100m;

            // Calcul dans les deux sens selon le mode de saisie choisi
            decimal prixHtva, prixTvac;
            if (rbHtva.Checked)
            {
                // L'utilisateur saisit en HTVA, je calcule le TVAC
                prixHtva = prixSaisi;
                prixTvac = prixSaisi * facteur;
            }
            else
            {
                // L'utilisateur saisit en TVAC, je retrouve le HTVA en divisant
                prixTvac = prixSaisi;
                prixHtva = facteur != 0 ? prixSaisi / facteur : 0;
            }

            string labelCond = _ingredientSelectionne?.ConditionnementLabel ?? "cond.";

            // Mise à jour de l'affichage : prix unitaires + totaux
            if (prixSaisi > 0)
            {
                lblPrixHtva.Text  = $"HTVA : {prixHtva:F4} €/{labelCond}";
                lblPrixTvac.Text  = $"TVAC : {prixTvac:F4} €/{labelCond}";
                lblTotalHtva.Text = nbCond > 0 ? $"Total HTVA : {nbCond * prixHtva:F2} €" : "Total HTVA : — €";
                lblTotalTvac.Text = nbCond > 0 ? $"Total TVAC : {nbCond * prixTvac:F2} €" : "Total TVAC : — €";
            }
            else
            {
                // Pas de prix saisi → on affiche des tirets
                lblPrixHtva.Text  = "HTVA : —";
                lblPrixTvac.Text  = "TVAC : —";
                lblTotalHtva.Text = "Total HTVA : — €";
                lblTotalTvac.Text = "Total TVAC : — €";
            }
        }

        // ====================================================================
        // Valider : override de FrmEditBase, appelé quand l'utilisateur clique OK.
        // Je vérifie que les champs obligatoires sont remplis correctement.
        // Si un champ est invalide, j'affiche l'icône d'erreur à côté via errorProvider (hérité de FrmEditBase).
        // Retourne true si tout est OK, false sinon (le form ne se ferme pas en cas d'erreur).
        // ====================================================================
        protected override bool Valider()
        {
            bool ok = true;

            // En création, un ingrédient doit être sélectionné
            if (!_isEdit && cmbIngredient?.SelectedItem == null)
            { errorProvider.SetError(cmbIngredient, "Choisissez un ingrédient."); ok = false; }

            if (nudQuantite.Value <= 0)
            { errorProvider.SetError(nudQuantite, "Quantité invalide."); ok = false; }

            if (nudPrix.Value <= 0)
            { errorProvider.SetError(nudPrix, "Prix invalide."); ok = false; }

            // Un stock est obligatoire : on doit savoir où ranger le lot
            if (cmbStock.SelectedItem == null)
            { errorProvider.SetError(cmbStock, "Choisissez un stock."); ok = false; }

            return ok;
        }

        // ====================================================================
        // Sauvegarder : override de FrmEditBase, appelé après validation réussie.
        // Je reconvertis le prix saisi en HTVA (c'est le format de stockage en DB),
        // je calcule le vrai nombre de conditionnements, la quantité totale initiale,
        // puis j'alimente l'objet _lot et j'appelle le DAL pour INSERT ou UPDATE.
        // ====================================================================
        protected override void Sauvegarder()
        {
            // Reconversion du prix en HTVA si l'utilisateur a saisi en TVAC
            decimal tvaPct  = nudTvaPct.Value;
            decimal facteur = 1 + tvaPct / 100m;
            decimal prixHtva = rbHtva.Checked
                ? nudPrix.Value
                : (facteur != 0 ? nudPrix.Value / facteur : nudPrix.Value);

            // Nombre réel de conditionnements (tient compte du mode lot)
            decimal nbCond = GetNbConditionnements();

            // En création je prends l'ingrédient sélectionné, en édition je garde celui du lot
            Ingredient ing = _isEdit ? null : (cmbIngredient?.SelectedItem as Ingredient);
            decimal qteParCond = ing?.QteParConditionnement ?? _lot.QteParConditionnement;

            // Alimentation de l'objet Lot avec toutes les valeurs du formulaire
            var stockSel = (Stock)cmbStock.SelectedItem;
            _lot.IdStock            = stockSel.Id;
            _lot.NumeroLot          = txtNumeroLot.Text.Trim().NullIfEmpty();
            _lot.IdFournisseur      = cmbFournisseur.SelectedItem is Fournisseur f ? (int?)f.Id : null;
            _lot.DateAchat          = dtpDateAchat.Value;
            _lot.DatePeremption     = chkPeremption.Checked ? (DateTime?)dtpPeremption.Value : null;
            _lot.NbConditionnements = nbCond;
            // QuantiteInitiale = nb conditionnements × qté par conditionnement (c'est le stock total qu'on reçoit)
            _lot.QuantiteInitiale   = Math.Round(nbCond * qteParCond, 4);
            // PrixUnitaire = prix HTVA par conditionnement (arrondi à 4 décimales)
            _lot.PrixUnitaire       = Math.Round(prixHtva, 4);
            // PrixAchatReel = le montant total HTVA de l'achat
            _lot.PrixAchatReel      = Math.Round(nbCond * prixHtva, 4);
            _lot.TvaPct             = tvaPct;
            _lot.ReferenceFacture   = txtRefFacture.Text.Trim().NullIfEmpty();
            _lot.Notes              = txtNotes.Text.Trim().NullIfEmpty();

            // En création : je lie le lot à la fiche ingrédient et j'insère en DB
            // En édition : je mets à jour le lot existant
            if (!_isEdit)
            {
                _lot.IdFicheIngredient = ing.Id;
                LotDAL.Insert(_lot);
            }
            else
            {
                LotDAL.Update(_lot);
            }
        }
    }
}
