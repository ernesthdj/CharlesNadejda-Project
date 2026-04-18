using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire liste générique — base commune à tous les formulaires de type liste+CRUD.
    ///
    /// USAGE — créer un formulaire liste :
    ///   1. Hériter de FrmListeBase&lt;TonEntite&gt;
    ///   2. Implémenter les 5 membres abstraits (Titre, ChargerDonnees, ConfigurerColonnes,
    ///      OuvrirFormulaire, Supprimer)
    ///   3. Surcharger NomElement() et/ou AppliquerStylesLignes() si besoin
    ///   4. Supprimer ou vider le Designer.cs — tout le layout est géré ici
    ///
    /// CONTRAINTE : le Designer VS ne peut pas ouvrir les classes héritant d'un Form
    /// générique. Pas de blocage car on écrit le layout entièrement en code.
    /// </summary>
    public abstract class FrmListeBase<T> : Form where T : class
    {
        // ── Contrôles accessibles aux sous-classes ────────────────────────
        protected readonly DataGridView dgv;
        protected readonly Label        lblTitre;
        protected readonly Button       btnAjouter;
        protected readonly Button       btnModifier;
        protected readonly Button       btnSupprimer;
        protected readonly Button       btnFermer;

        /// <summary>Position X de la colonne de boutons — pour ajouter des boutons supplémentaires.</summary>
        protected const int BtnX = 736;

        /// <summary>Y disponible après btnSupprimer pour un bouton supplémentaire.</summary>
        protected const int BtnYExtra = 196;

        // ── Palette artisanale — cohérente avec FrmPrincipal ─────────
        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61,  40, 23);
        private static readonly Color CHOCOLAT_MOYEN = Color.FromArgb(111, 78, 55);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);
        private static readonly Color OR             = Color.FromArgb(212, 175, 55);

        protected FrmListeBase()
        {
            // ── DataGridView — ancré aux 4 bords pour suivre le redimensionnement ──
            dgv = new DataGridView
            {
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.None,
                MultiSelect                 = false,
                ReadOnly                    = true,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                Location                    = new Point(12, 52),
                Size                        = new Size(710, 420),
                Anchor                      = AnchorStyles.Top | AnchorStyles.Bottom
                                            | AnchorStyles.Left | AnchorStyles.Right,
                Font                        = new Font("Segoe UI", 9.5F),
                RowHeadersVisible           = false,
                BackgroundColor             = Color.White,
                BorderStyle                 = BorderStyle.None,
                GridColor                   = Color.FromArgb(230, 220, 210),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 32
            };

            // En-tête chocolat clair (CREME) — cohérence palette
            dgv.ColumnHeadersDefaultCellStyle.BackColor          = CREME;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor          = CHOCOLAT_FONCE;
            dgv.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CREME;

            // Sélection chocolat moyen
            dgv.DefaultCellStyle.SelectionBackColor = CHOCOLAT_MOYEN;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // Lignes alternées gris très léger (loi de Gestalt : continuité)
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 247, 244);

            // Double-clic → Modifier (Nielsen #7 : flexibilité power users)
            dgv.CellDoubleClick += (s, ev) => { if (ev.RowIndex >= 0) OnModifier(); };

            // ── Label titre ───────────────────────────────────────────────
            lblTitre = new Label
            {
                AutoSize  = false,
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = CHOCOLAT_FONCE,
                Location  = new Point(12, 12),
                Size      = new Size(700, 30),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left
            };

            // ── Boutons — palette cohérente ───────────────────────────────
            var ancreHD = AnchorStyles.Top | AnchorStyles.Right;
            var ancreBD = AnchorStyles.Bottom | AnchorStyles.Right;

            btnAjouter   = CreerBouton("+ Ajouter",  BtnX, 52,  ancreHD, CHOCOLAT_FONCE, Color.White);
            btnModifier  = CreerBouton("✎  Modifier", BtnX, 96,  ancreHD, Color.FromArgb(90, 130, 80), Color.White);
            btnSupprimer = CreerBouton("✕  Supprimer", BtnX, 140, ancreHD, Color.FromArgb(180, 50, 40), Color.White);
            btnFermer    = CreerBouton("Fermer",       BtnX, 436, ancreBD, Color.FromArgb(100, 90, 80), Color.White);

            btnAjouter.Click   += (s, e) => OnAjouter();
            btnModifier.Click  += (s, e) => OnModifier();
            btnSupprimer.Click += (s, e) => OnSupprimer();
            btnFermer.Click    += (s, e) => Close();

            // ── Formulaire ────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(882, 490);
            MinimumSize         = new Size(750, 400);
            FormBorderStyle     = FormBorderStyle.Sizable;
            MaximizeBox         = true;
            StartPosition       = FormStartPosition.CenterParent;
            BackColor           = Color.White;

            Controls.AddRange(new Control[]
            {
                lblTitre, dgv,
                btnAjouter, btnModifier, btnSupprimer, btnFermer
            });
        }

        // ── Cycle de vie ──────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text          = Titre;
            lblTitre.Text = Titre;
            Charger();
        }

        /// <summary>Recharge les données depuis le DAL et rafraîchit l'affichage.</summary>
        protected void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = ChargerDonnees();
                ConfigurerColonnes();
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                AppliquerStylesLignes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de chargement : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Actions CRUD ──────────────────────────────────────────────────

        private void OnAjouter()
        {
            using (var frm = OuvrirFormulaire(null))
                if (frm != null && frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void OnModifier()
        {
            var item = Selectionne();
            if (item == null) { MessageBox.Show("Sélectionnez un élément.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = OuvrirFormulaire(item))
                if (frm != null && frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void OnSupprimer()
        {
            var item = Selectionne();
            if (item == null) { MessageBox.Show("Sélectionnez un élément.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            // DefaultButton.Button2 = "Non" par défaut — protection contre clic accidentel (Nielsen #3)
            if (MessageBox.Show(
                    $"Supprimer « {NomElement(item)} » ?\n\nCette action est irréversible.",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2)
                != DialogResult.Yes) return;
            try
            {
                Supprimer(item);
                Charger();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers disponibles dans les sous-classes ─────────────────────

        /// <summary>Retourne l'entité sélectionnée dans le DGV, ou null.</summary>
        protected T Selectionne() => dgv.CurrentRow?.DataBoundItem as T;

        /// <summary>Configure header, largeur initiale et MinimumWidth d'une colonne.</summary>
        protected void ConfigCol(string nom, string header, int largeur, int minimum)
        {
            if (dgv.Columns[nom] == null) return;
            dgv.Columns[nom].HeaderText   = header;
            dgv.Columns[nom].Width        = largeur;
            dgv.Columns[nom].MinimumWidth = minimum;
        }

        /// <summary>Cache une ou plusieurs colonnes par leur nom de propriété.</summary>
        protected void CacherColonnes(params string[] noms)
        {
            foreach (var nom in noms)
                if (dgv.Columns[nom] != null) dgv.Columns[nom].Visible = false;
        }

        private static Button CreerBouton(string texte, int x, int y,
            AnchorStyles anchor,
            Color? backColor = null, Color? foreColor = null)
        {
            var btn = new Button
            {
                Text      = texte,
                Location  = new Point(x, y),
                Size      = new Size(130, 36),
                Font      = new Font("Segoe UI", 9.5F),
                Anchor    = anchor,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                BackColor = backColor ?? SystemColors.Control,
                ForeColor = foreColor ?? SystemColors.ControlText
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ── Membres abstraits — à implémenter dans chaque sous-classe ─────

        /// <summary>Titre affiché dans le label et dans la barre de titre de la fenêtre.</summary>
        protected abstract string    Titre             { get; }

        /// <summary>Appelle le DAL et retourne la liste à afficher dans le DGV.</summary>
        protected abstract List<T>   ChargerDonnees    ();

        /// <summary>
        /// Configure les colonnes du DGV après le binding.
        /// Utilisez CacherColonnes() et ConfigCol() depuis cette méthode.
        /// </summary>
        protected abstract void      ConfigurerColonnes();

        /// <summary>
        /// Retourne le formulaire d'édition.
        /// element == null → création, element != null → modification.
        /// </summary>
        protected abstract Form      OuvrirFormulaire  (T element);

        /// <summary>
        /// Appelle le DAL pour supprimer l'élément.
        /// La confirmation utilisateur est gérée par la classe de base.
        /// </summary>
        protected abstract void      Supprimer         (T element);

        // ── Membres virtuels — surchargeables si besoin ───────────────────

        /// <summary>Retourne le nom affiché dans la boîte de confirmation de suppression.</summary>
        protected virtual string NomElement(T element) => element?.ToString() ?? "?";

        /// <summary>
        /// Applique des styles visuels sur les lignes après chargement.
        /// Exemple : coloration des lignes en alerte de stock.
        /// </summary>
        protected virtual void AppliquerStylesLignes() { }
    }
}
