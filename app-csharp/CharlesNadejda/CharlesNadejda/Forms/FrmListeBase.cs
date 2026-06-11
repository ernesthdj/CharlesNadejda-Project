using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// FrmListeBase&lt;T&gt; — formulaire liste générique, base commune à tous les écrans CRUD.
    /// C'est mon pattern "Template Method" : la classe de base gère tout le layout et le workflow
    /// CRUD (Ajouter/Modifier/Supprimer/Charger), et les sous-classes ne fournissent que la
    /// logique spécifique (quel DAL appeler, quelles colonnes afficher, quel formulaire ouvrir).
    ///
    /// USAGE — créer un nouveau formulaire liste :
    ///   1. Hériter de FrmListeBase&lt;TonEntite&gt;
    ///   2. Implémenter les 5 membres abstraits (Titre, ChargerDonnees, ConfigurerColonnes,
    ///      OuvrirFormulaire, Supprimer)
    ///   3. Surcharger NomElement() et/ou AppliquerStylesLignes() si besoin
    ///   4. Supprimer ou vider le Designer.cs — tout le layout est géré ici en code
    ///
    /// CONTRAINTE : le Designer VS ne peut pas ouvrir les classes héritant d'un Form
    /// générique (limitation de Visual Studio). Pas de blocage car on écrit le layout
    /// entièrement en code — pas besoin du Designer.
    /// </summary>
    public abstract class FrmListeBase<T> : Form where T : class
    {
        // ── Contrôles accessibles aux sous-classes ────────────────────────
        // protected pour que FrmAchats, FrmFournisseurs, etc. puissent y accéder

        // Le DataGridView central — affiche la liste des entités
        protected readonly DataGridView dgv;

        // Label titre en haut du formulaire (ex: "Achats — Chocolaterie")
        protected readonly Label        lblTitre;

        // Boutons CRUD alignés à droite
        protected readonly Button       btnAjouter;    // Crée une nouvelle entité
        protected readonly Button       btnModifier;   // Modifie l'entité sélectionnée
        protected readonly Button       btnSupprimer;  // Supprime l'entité sélectionnée
        protected readonly Button       btnFermer;     // Ferme le formulaire

        /// <summary>Position X de la colonne de boutons — pour ajouter des boutons supplémentaires dans les sous-classes.</summary>
        protected const int BtnX = 736;

        /// <summary>Y disponible après btnSupprimer pour un bouton supplémentaire (ex: bouton "Dupliquer").</summary>
        protected const int BtnYExtra = 196;

        // ── Palette — les couleurs viennent de AppColors (TICKET-12) ─────

        // Constructeur — je construis tout le layout en code, pas de Designer.cs
        // Tout est configuré ici : DGV, boutons, titre, styles, ancres, etc.
        protected FrmListeBase()
        {
            // ── DataGridView — ancré aux 4 bords pour suivre le redimensionnement ──
            // Configuration stricte : lecture seule, sélection ligne entière, pas d'ajout/suppression
            dgv = new DataGridView
            {
                AllowUserToAddRows          = false,   // Pas de ligne vide en bas
                AllowUserToDeleteRows       = false,   // Suppression uniquement via le bouton
                AllowUserToResizeRows       = false,   // Les hauteurs de ligne sont fixes
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.None, // Géré manuellement dans ConfigurerColonnes
                MultiSelect                 = false,   // Une seule ligne sélectionnable à la fois
                ReadOnly                    = true,    // Pas de modification inline
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect, // Click = sélection de la ligne entière
                Location                    = new Point(12, 52),
                Size                        = new Size(710, 420),
                // Ancre 4 bords — le DGV grandit/rétrécit avec la fenêtre
                Anchor                      = AnchorStyles.Top | AnchorStyles.Bottom
                                            | AnchorStyles.Left | AnchorStyles.Right,
                Font                        = new Font("Segoe UI", 9.5F),
                RowHeadersVisible           = false,   // Pas de colonne "header" à gauche
                BackgroundColor             = AppColors.CremeWarm,
                BorderStyle                 = BorderStyle.None,
                GridColor                   = AppColors.Border,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 32
            };

            // En-tête chocolat clair — cohérence avec la palette Charles & Nadejda
            // SelectionBackColor = même couleur pour éviter le bleu système moche
            dgv.ColumnHeadersDefaultCellStyle.BackColor          = AppColors.Creme;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor          = AppColors.ChocoBrand;
            dgv.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = AppColors.Creme;

            // Sélection chocolat foncé — contraste renforcé pour bien voir la ligne active
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.ChocoBrand;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // Lignes alternées crème très léger — loi de Gestalt "continuité"
            // Aide l'oeil à suivre une ligne horizontalement dans un grand tableau
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 246, 238);

            // Double-clic → Modifier — raccourci power user (Nielsen #7 : flexibilité)
            dgv.CellDoubleClick += (s, ev) => { if (ev.RowIndex >= 0) OnModifier(); };

            // ── Label titre ───────────────────────────────────────────────
            // Grand label en haut à gauche avec le titre du formulaire
            lblTitre = new Label
            {
                AutoSize  = false,
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand,
                Location  = new Point(12, 12),
                Size      = new Size(700, 30),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left
            };

            // ── Boutons — palette cohérente Charles & Nadejda ───────────
            // ancreHD = Haut-Droite (les boutons restent collés en haut à droite au resize)
            // ancreBD = Bas-Droite (le bouton Fermer reste en bas à droite)
            var ancreHD = AnchorStyles.Top | AnchorStyles.Right;
            var ancreBD = AnchorStyles.Bottom | AnchorStyles.Right;

            // Chaque bouton a sa couleur pour distinguer les actions
            // Chocolat = Ajouter, Vert = Modifier, Rouge = Supprimer, Gris = Fermer
            btnAjouter   = CreerBouton("＋  Ajouter",   BtnX, 52,  ancreHD, AppColors.ChocoBrand, Color.White);
            btnModifier  = CreerBouton("✎  Modifier",  BtnX, 96,  ancreHD, AppColors.GreenOk,  Color.White);
            btnSupprimer = CreerBouton("✕  Supprimer", BtnX, 140, ancreHD, AppColors.RedCrit,  Color.White);
            btnFermer    = CreerBouton("Fermer",        BtnX, 436, ancreBD, AppColors.GreyBtn,        AppColors.ChocoBrand);

            // FlatAppearance — bordures et couleurs de hover personnalisées
            // Chaque bouton a sa propre teinte de hover pour rester dans la palette
            btnAjouter.FlatAppearance.BorderColor        = AppColors.ChocoAbyss;
            btnAjouter.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 60, 36);

            btnModifier.FlatAppearance.BorderColor        = Color.FromArgb(46, 125, 50);
            btnModifier.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 125, 50);

            btnSupprimer.FlatAppearance.BorderColor        = Color.FromArgb(141, 31, 51);
            btnSupprimer.FlatAppearance.MouseOverBackColor = Color.FromArgb(158, 31, 55);

            btnFermer.FlatAppearance.BorderColor        = AppColors.Border;
            btnFermer.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 213, 202);

            // Branchement des clicks — chaque bouton appelle sa méthode privée
            btnAjouter.Click   += (s, e) => OnAjouter();
            btnModifier.Click  += (s, e) => OnModifier();
            btnSupprimer.Click += (s, e) => OnSupprimer();
            btnFermer.Click    += (s, e) => Close();

            // ── Formulaire — configuration globale ─────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(882, 490);
            MinimumSize         = new Size(750, 400);   // Taille minimale pour ne pas écraser le DGV
            FormBorderStyle     = FormBorderStyle.Sizable;  // Redimensionnable
            MaximizeBox         = true;
            StartPosition       = FormStartPosition.CenterParent;
            BackColor           = AppColors.CremeWarm;

            // Ajout de tous les contrôles au formulaire d'un coup
            Controls.AddRange(new Control[]
            {
                lblTitre, dgv,
                btnAjouter, btnModifier, btnSupprimer, btnFermer
            });
        }

        // ── Cycle de vie ──────────────────────────────────────────────────

        // OnLoad() — appelé quand le formulaire s'affiche pour la première fois
        // Je mets à jour le titre et je lance le chargement des données
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text          = Titre;     // Titre dans la barre Windows
            lblTitre.Text = Titre;     // Titre dans le label du formulaire
            Charger();                 // Premier chargement des données
        }

        /// <summary>
        /// Charger() — recharge les données depuis le DAL et rafraîchit l'affichage.
        /// Appelé au OnLoad, après un Ajouter/Modifier/Supprimer, et depuis les sous-classes.
        /// Le pattern : DataSource = null → DataSource = données → ConfigurerColonnes → styles.
        /// Le null intermédiaire force le DGV à tout reconstruire (sinon les colonnes restent).
        /// </summary>
        protected void Charger()
        {
            try
            {
                dgv.DataSource = null;            // Force le reset du binding
                dgv.DataSource = ChargerDonnees(); // Appelle le DAL de la sous-classe
                ConfigurerColonnes();              // La sous-classe configure les colonnes
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                AppliquerStylesLignes();           // Hook optionnel pour la coloration
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de chargement : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Actions CRUD ──────────────────────────────────────────────────

        // OnAjouter() — ouvre le formulaire d'édition en mode création (null = nouvel élément)
        // Si le formulaire retourne OK → je recharge la liste
        // Si le formulaire retourne Retry → j'appelle OnFormRetry() (hook pour les sous-classes)
        private void OnAjouter()
        {
            using (var frm = OuvrirFormulaire(null))
            {
                if (frm == null) return;
                var result = frm.ShowDialog();
                if (result == DialogResult.OK) Charger();
                else if (result == DialogResult.Retry) OnFormRetry();
            }
        }

        /// <summary>
        /// OnFormRetry() — hook appelé quand un formulaire retourne DialogResult.Retry.
        /// Par défaut ne fait rien. Les sous-classes le surchargent pour gérer les cas spéciaux.
        /// Ex: FrmAchats l'utilise pour ouvrir FrmIngredientEdit quand l'utilisateur
        /// veut créer un ingrédient qui n'existe pas encore.
        /// </summary>
        protected virtual void OnFormRetry() { }

        // OnModifier() — ouvre le formulaire d'édition avec l'élément sélectionné
        // Si aucun élément sélectionné → message d'info (pas d'erreur)
        private void OnModifier()
        {
            var item = Selectionne();
            if (item == null) { MessageBox.Show("Sélectionnez un élément.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = OuvrirFormulaire(item))
                if (frm != null && frm.ShowDialog() == DialogResult.OK) Charger();
        }

        // OnSupprimer() — supprime l'élément sélectionné après confirmation
        // Nielsen #3 : le bouton "Non" est sélectionné par défaut (DefaultButton.Button2)
        // pour éviter les suppressions accidentelles par Enter rapide
        private void OnSupprimer()
        {
            var item = Selectionne();
            if (item == null) { MessageBox.Show("Sélectionnez un élément.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            // MessageBox avec "Non" par défaut — protection contre le clic accidentel
            if (MessageBox.Show(
                    $"Supprimer « {NomElement(item)} » ?\n\nCette action est irréversible.",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2)
                != DialogResult.Yes) return;
            try
            {
                Supprimer(item);  // Appelle le DAL de la sous-classe
                Charger();        // Recharge la liste pour refléter la suppression
            }
            catch (Exception ex)
            {
                // L'erreur peut venir d'une FK contrainte (ex: lot utilisé dans une production)
                MessageBox.Show("Impossible de supprimer : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers disponibles dans les sous-classes ─────────────────────

        /// <summary>
        /// Selectionne() — retourne l'entité T de la ligne sélectionnée dans le DGV, ou null.
        /// Utilise DataBoundItem qui est le vrai objet C# bindé à la ligne (pas un DataRow).
        /// </summary>
        protected T Selectionne() => dgv.CurrentRow?.DataBoundItem as T;

        /// <summary>
        /// ConfigCol() — configure le header, la largeur initiale et la MinimumWidth d'une colonne.
        /// J'appelle ça dans ConfigurerColonnes() des sous-classes pour personnaliser l'affichage.
        /// Le guard "if null" évite le crash si la propriété n'existe pas dans le modèle.
        /// </summary>
        protected void ConfigCol(string nom, string header, int largeur, int minimum)
        {
            if (dgv.Columns[nom] == null) return;
            dgv.Columns[nom].HeaderText   = header;
            dgv.Columns[nom].Width        = largeur;
            dgv.Columns[nom].MinimumWidth = minimum;
        }

        /// <summary>
        /// CacherColonnes() — cache une ou plusieurs colonnes par leur nom de propriété C#.
        /// Pratique pour masquer les FK et les champs techniques en un seul appel.
        /// Ex: CacherColonnes("Id", "IdFournisseur", "Notes")
        /// </summary>
        protected void CacherColonnes(params string[] noms)
        {
            foreach (var nom in noms)
                if (dgv.Columns[nom] != null) dgv.Columns[nom].Visible = false;
        }

        // CreerBouton() — factory method pour créer un bouton flat stylé
        // Tous les boutons de l'app ont le même look : FlatStyle.Flat, Segoe UI 9.5, curseur Hand
        // Les couleurs sont passées en paramètre pour différencier les actions
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
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        // ── Membres abstraits — à implémenter dans chaque sous-classe ─────

        /// <summary>Titre affiché dans le label et dans la barre de titre de la fenêtre.</summary>
        protected abstract string    Titre             { get; }

        /// <summary>Appelle le DAL et retourne la liste des entités à afficher dans le DGV.</summary>
        protected abstract List<T>   ChargerDonnees    ();

        /// <summary>
        /// Configure les colonnes du DGV après le data-binding.
        /// C'est ici que les sous-classes appellent CacherColonnes() et ConfigCol()
        /// pour masquer les colonnes inutiles et renommer les headers.
        /// </summary>
        protected abstract void      ConfigurerColonnes();

        /// <summary>
        /// Retourne le formulaire d'édition approprié.
        /// element == null → mode création (nouveau), element != null → mode modification.
        /// Le formulaire retourné sera affiché en ShowDialog() par la classe de base.
        /// </summary>
        protected abstract Form      OuvrirFormulaire  (T element);

        /// <summary>
        /// Appelle le DAL pour supprimer l'élément.
        /// La confirmation utilisateur (MessageBox Oui/Non) est gérée par la classe de base,
        /// pas besoin de la refaire ici.
        /// </summary>
        protected abstract void      Supprimer         (T element);

        // ── Membres virtuels — surchargeables si besoin ───────────────────

        /// <summary>
        /// NomElement() — retourne le nom lisible de l'élément pour la boîte de confirmation.
        /// Ex: "Beurre de cacao du 10/06/2026" pour un lot, "Callebaut" pour un fournisseur.
        /// Par défaut utilise ToString(), mais les sous-classes le surchargent pour un affichage métier.
        /// </summary>
        protected virtual string NomElement(T element) => element?.ToString() ?? "?";

        /// <summary>
        /// AppliquerStylesLignes() — hook pour appliquer des styles visuels sur les lignes du DGV.
        /// Appelé après chaque Charger(). Par défaut ne fait rien.
        /// Ex: FrmStocks l'utilise pour colorer les lignes en alerte de stock bas en rouge.
        /// </summary>
        protected virtual void AppliquerStylesLignes() { }

        // ── Raccourcis clavier ──────────────────────────────────────────────
        // Ctrl+N : Nouveau | Ctrl+E : Modifier | Suppr : Supprimer | Échap : Fermer

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    OnAjouter();
                    return true;
                case Keys.Control | Keys.E:
                    OnModifier();
                    return true;
                case Keys.Delete:
                    OnSupprimer();
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
