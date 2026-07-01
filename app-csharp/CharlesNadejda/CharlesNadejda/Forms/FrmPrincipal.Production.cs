using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal.Production — Ecran de production BOM unifie
    //
    //  Responsabilite : selection contexte/niveau, gestion fiches recettes,
    //  visualisation stock compact, simulation de faisabilite (penurie),
    //  lancement FIFO, affichage historique et journal de production.
    //
    //  Layout Kanban 3 colonnes :
    //    Col1 (220px Left)  : combo contexte + cards niveaux + cards fiches
    //    Col2 (Fill)        : stock compact + panel simulation (DGV + inputs)
    //    Col3 (260px Right) : historique productions + mini-journal
    //
    //  Flux principal :
    //    Contexte → Niveaux → [N2+] Fiches → Simuler → Lancer → Historique
    // ════════════════════════════════════════════════════════════════
    partial class FrmPrincipal
    {

        // ── Constantes de layout (Kanban 3 colonnes) ──────────────────────
        // Largeurs fixes des colonnes laterales — la centrale est en Dock.Fill
        private const int PROD_COL_NIVEAUX_W   = 220;  // Colonne gauche (niveaux + fiches)
        private const int PROD_COL_HISTORIQUE_W = 260;  // Colonne droite (historique + journal)
        // Largeur interne des cards dans la colonne gauche (220 - padding 8*2 - marge)
        private const int PROD_CARD_W           = 196;
        // Hauteurs de sections recurrentes
        private const int PROD_HEADER_H         = 52;
        private const int PROD_KPI_BAR_H        = 92;
        private const int PROD_SECTION_HEADER_H = 26;
        // Dimensions des boutons d'action (Simuler, Lancer)
        private const int PROD_BTN_ACTION_W     = 130;
        private const int PROD_BTN_ACTION_H     = 32;

        // ── Etat local de la production ──────────────────────────────────

        // Contrôles de saisie pour la simulation : quantité de batches et délai en jours
        private NumericUpDown _prodNudQuantite, _prodNudDelai;
        // Zone de texte libre pour ajouter des notes à la production
        private TextBox       _prodTxtNotes;
        // Labels d'affichage : info batch (ex: "1 batch = 500g"), résultat simulation, coût estimé
        private Label         _prodLblInfoBatch, _prodLblResultat, _prodLblCoutEstime;
        // Grille qui affiche les lignes de simulation (ingrédient, nécessaire, dispo, manque, jauge %)
        private DataGridView  _prodDgvSimulation;
        // Bouton "Simuler" pour vérifier le stock, bouton "Lancer" pour exécuter la production
        private Button        _prodBtnSimuler, _prodBtnLancer;
        // Panel en bas à droite qui affiche le mini-journal des dernières actions
        private Panel         _prodJournalPanel;
        // Résultat de la simulation : liste des lignes avec les quantités manquantes par input
        private List<BomManque> _prodLignesSimulation;
        // Flag qui indique si la dernière simulation est OK (pas de pénurie) → autorise le lancement
        private bool            _prodSimulationValide;

        // ── Colonne gauche : niveaux + fiches ─────────────────────────

        // Combo pour choisir le contexte BOM (ex: "Pâtisserie fine", "Viennoiserie"...)
        private ComboBox _prodCboContexte;
        // Flow qui contient les cards de niveaux (N0, N1, N2...) empilées verticalement
        private FlowLayoutPanel _prodFlowNiveaux;
        // Flow qui contient les cards de fiches recettes pour le niveau sélectionné
        private FlowLayoutPanel _prodFlowFichesInner;
        // Le SplitContainer divise la colonne gauche en 2 : niveaux en haut, fiches en bas
        private SplitContainer  _prodSplitNivFiches;
        // Dictionnaires pour retrouver rapidement la card Panel associée à chaque niveau/fiche par ID
        private Dictionary<int, Panel> _prodNiveauPanels = new Dictionary<int, Panel>();
        private Dictionary<int, Panel> _prodFichePanels  = new Dictionary<int, Panel>();
        // Le niveau BOM actuellement sélectionné dans la colonne gauche
        private BomNiveau _prodSelectedNiveau;
        // La fiche recette actuellement sélectionnée (visible uniquement pour N2+)
        private BomFiche  _prodSelectedFiche;

        // ── Colonne centrale : stock compact + simulation ─────────────

        // Flow qui affiche les lignes de stock compact (ingrédients ou produits semi-finis)
        private FlowLayoutPanel _prodFlowStock;
        // Panel contenant tout le bloc simulation (DGV, inputs, boutons) — visible seulement en N2+
        private Panel           _prodPnlSimulation;
        // Bouton "Acheter" rapide, visible uniquement quand je suis sur le niveau N1 (ingrédients)
        private Button          _prodBtnAchatRapide;

        // ── Colonne droite : historique + journal ─────────────────────

        // Flow qui affiche les cards d'historique des dernières productions
        private FlowLayoutPanel _prodFlowHistorique;

        // ── KPI labels (pour refresh contextuel) ──────────────────────
        // Je garde des refs vers les labels de la barre KPI pour les mettre à jour dynamiquement
        // Val = la valeur principale (nombre), Sub = le sous-titre descriptif
        private Label _prodKpi1Val, _prodKpi2Val, _prodKpi3Val, _prodKpi4Val;
        private Label _prodKpi1Sub, _prodKpi2Sub, _prodKpi3Sub, _prodKpi4Sub;

        // Point d'entrée de l'écran Production — je construis tout le layout ici
        // et je charge les données initiales via ProdChargerContextes
        private void ShowProductionScreen(NavigationParams p)
        {
            // Si aucune activité n'est active, je redirige vers l'onboarding
            if (_state.ActiveActivite == null) { ShowOnboarding(); return; }
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            // Je reset tout l'état local pour repartir de zéro à chaque affichage
            _prodLignesSimulation = null;
            _prodSimulationValide = false;
            _prodSelectedFiche    = null;
            _prodSelectedNiveau   = null;
            _prodFichePanels.Clear();
            _prodNiveauPanels.Clear();

            // ═══════════════════════════════════════════════════════════
            //  1. HEADER + KPI BAR (Dock Top)
            //  Je construis le bandeau titre + la barre de KPIs en haut
            // ═══════════════════════════════════════════════════════════
            var pnlHdr = BuildProdHeader();
            var pnlKpi = BuildProdKpiBar();

            // ═══════════════════════════════════════════════════════════
            //  2. KANBAN BODY — 3 colonnes
            //  Le corps principal avec les 3 colonnes côte à côte
            // ═══════════════════════════════════════════════════════════
            var pnlKanbanBody = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM };

            var colNiveaux = BuildProdColNiveaux();
            var colCentrale = BuildProdColCentrale();
            var colHist = BuildProdColHistorique();

            // Séparateurs visuels entre les colonnes (petites lignes verticales subtiles)
            var sep1 = MakeColumnSeparator();
            var sep2 = MakeColumnSeparator();
            sep2.Dock = DockStyle.Right;

            // Assemblage Kanban (Fill premier, puis Right, puis Left)
            // Important : en WinForms, l'ordre d'ajout des contrôles Docked compte !
            // Le Fill doit être ajouté en premier pour occuper l'espace restant
            pnlKanbanBody.Controls.Add(colCentrale);    // Fill — en premier
            pnlKanbanBody.Controls.Add(sep2);            // Dock Right
            pnlKanbanBody.Controls.Add(colHist);         // Dock Right
            pnlKanbanBody.Controls.Add(sep1);            // Dock Left
            pnlKanbanBody.Controls.Add(colNiveaux);      // Dock Left

            // ── Assemblage final dans _pnlDroit ───────────────────────
            // Même logique : Fill d'abord, puis les Top (qui s'empilent de bas en haut)
            _pnlDroit.Controls.Add(pnlKanbanBody);     // Fill
            _pnlDroit.Controls.Add(pnlKpi);            // Top
            _pnlDroit.Controls.Add(pnlHdr);            // Top

            _pnlDroit.ResumeLayout();

            // ── Charger le combo contexte puis sélectionner ────────────
            // Ça déclenche toute la cascade : contexte → niveaux → fiches → stock
            ProdChargerContextes();
        }

        // ════════════════════════════════════════════════════════════════
        //  HEADER
        //  Bandeau en haut de l'écran avec le titre "Production" et un sous-titre descriptif
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdHeader()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top, Height = PROD_HEADER_H,
                BackColor = CREME_WARM, Padding = new Padding(20, 0, 20, 0)
            };
            // Je dessine une ligne fine en bas du header pour séparer visuellement
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };
            pnl.Controls.Add(new Label
            {
                Text = "Production", Location = new Point(20, 6),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true
            });
            // Sous-titre qui rappelle le workflow de l'écran
            pnl.Controls.Add(new Label
            {
                Text = "Sélection niveau · Stock · Simulation · Lancement · Historique",
                Location = new Point(20, 30), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCO_MED, AutoSize = true
            });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════
        //  KPI BAR (refresh contextuel via ProdRefreshKpi)
        //  Barre de 4 indicateurs clés en haut : productions 7j, coût 7j, alertes stock, fiches actives
        //  Les valeurs sont mises à jour à chaque changement de sélection
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdKpiBar()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top, Height = PROD_KPI_BAR_H,
                BackColor = CREME_WARM, Padding = new Padding(16, 12, 16, 8)
            };

            // Je crée 4 cartes statistiques avec des valeurs placeholder "—"
            var c1 = MakeStatCard("▶", "Productions 7j", "—", "", "");
            var c2 = MakeStatCard("💰", "Coût 7j", "—", "", "gold");
            var c3 = MakeStatCard("⚠", "Alertes stock", "—", "", "");
            var c4 = MakeStatCard("🧪", "Fiches actives", "—", "", "");

            // Je récupère les refs vers les labels de valeur et sous-titre pour les mettre à jour plus tard
            // Je les identifie par leur taille de font (>15 pour la valeur, <8 pour le sous-titre)
            _prodKpi1Val = c1.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size > 15);
            _prodKpi2Val = c2.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size > 15);
            _prodKpi3Val = c3.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size > 15);
            _prodKpi4Val = c4.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size > 15);
            _prodKpi1Sub = c1.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size < 8);
            _prodKpi2Sub = c2.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size < 8);
            _prodKpi3Sub = c3.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size < 8);
            _prodKpi4Sub = c4.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Size < 8);

            // Au resize, je recalcule la largeur de chaque carte pour qu'elles se répartissent équitablement
            pnl.Resize += (s, ev) =>
            {
                int w = (pnl.ClientSize.Width - 32 - 12 * 3) / 4;
                for (int i = 0; i < pnl.Controls.Count; i++)
                    pnl.Controls[i].SetBounds(16 + i * (w + 12), 12, w, 68);
            };
            pnl.Controls.Add(c1);
            pnl.Controls.Add(c2);
            pnl.Controls.Add(c3);
            pnl.Controls.Add(c4);

            return pnl;
        }

        // Met à jour les 4 KPIs avec les vraies données depuis la DB
        // Appelé à chaque changement de niveau, après une production, etc.
        private void ProdRefreshKpi()
        {
            if (_prodKpi1Val == null) return;

            try
            {
                // Je récupère les 50 dernières productions et les ingrédients de cette activité
                var prods = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 50);
                var ings  = IngredientDAL.GetAllByActivite(_state.ActiveActivite.Id);

                // KPI 1 : nombre de productions des 7 derniers jours
                int prods7j    = prods.Count(pp => pp.DateProduction >= DateTime.Now.AddDays(-7));
                // KPI 2 : coût total des ingrédients consommés sur 7 jours
                decimal cout7j = prods.Where(pp => pp.DateProduction >= DateTime.Now.AddDays(-7))
                                      .Sum(pp => pp.CoutIngredients);
                // KPI 3 : nombre d'ingrédients en alerte (stock bas)
                int alertes    = ings.Count(i => i.EstEnAlerte);

                // KPI 4 : nombre de fiches actives — filtré par niveau si N2+, sinon toutes
                int fichesCount = 0;
                if (_prodSelectedNiveau != null && _prodSelectedNiveau.Ordre >= 2)
                    fichesCount = BomFicheDAL.GetByNiveau(_prodSelectedNiveau.Id).Count;
                else
                    fichesCount = BomFicheDAL.GetAll(idActivite: _state.ActiveActivite.Id).Count;

                // Je mets à jour les labels de valeur
                _prodKpi1Val.Text = prods7j.ToString();
                _prodKpi2Val.Text = $"{cout7j:F2} €";
                _prodKpi3Val.Text = alertes.ToString();
                _prodKpi4Val.Text = fichesCount.ToString();

                // Je mets à jour les sous-titres descriptifs
                if (_prodKpi1Sub != null) _prodKpi1Sub.Text = prods7j > 0 ? "dernières 7 jours" : "Aucune";
                if (_prodKpi2Sub != null) _prodKpi2Sub.Text = cout7j > 0 ? "total ingrédients" : "";
                if (_prodKpi3Sub != null) _prodKpi3Sub.Text = alertes > 0 ? "ingrédients bas" : "Tout OK";
                if (_prodKpi4Sub != null) _prodKpi4Sub.Text = _prodSelectedNiveau != null
                    ? $"N{_prodSelectedNiveau.Ordre} {_prodSelectedNiveau.Nom}" : "toutes";
            }
            catch (Exception ex)
            {
                Trace.TraceError("ProdRefreshKpi : {0}", ex);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  COLONNE 1 — NIVEAUX + FICHES (Dock Left, 220px)
        //  Colonne de gauche : en haut le combo contexte + les cards niveaux,
        //  en bas les cards fiches recettes (visibles uniquement pour N2+)
        // ════════════════════════════════════════════════════════════════

        // Construit toute la colonne gauche avec le SplitContainer niveaux/fiches
        private Panel BuildProdColNiveaux()
        {
            var col = new Panel { Dock = DockStyle.Left, Width = PROD_COL_NIVEAUX_W, BackColor = CREME_WARM };

            var lblHeader = MakeKanbanHeader("NIVEAUX", CHOCO_BRAND);

            // ── Combo contexte (Dock Top) ─────────────────────────────
            // Le combo permet de choisir le contexte BOM (ex: "Pâtisserie", "Boulangerie")
            // Changer de contexte recharge tous les niveaux et fiches
            var pnlCombo = new Panel
            {
                Dock = DockStyle.Top, Height = 44, BackColor = CREME_WARM,
                Padding = new Padding(8, 4, 8, 4)
            };
            var lblCtx = new Label
            {
                Text = "Contexte", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(8, 2), AutoSize = true
            };
            _prodCboContexte = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(8, 18), Size = new Size(196, 22)
            };
            _prodCboContexte.SelectedIndexChanged += ProdContexte_Changed;
            pnlCombo.Controls.Add(lblCtx);
            pnlCombo.Controls.Add(_prodCboContexte);

            // ── SplitContainer 50/50 : Niveaux (haut) | Fiches (bas) ──
            // Je divise la colonne en deux zones scrollables indépendantes
            _prodSplitNivFiches = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = CREME_WARM,
                BorderStyle = BorderStyle.None,
                SplitterWidth = 3,
                SplitterDistance = 200  // sera recalculé au Resize
            };
            _prodSplitNivFiches.SplitterMoved += (s, ev) => { };
            // Au resize de la colonne, je recalcule la position du splitter pour garder 50/50
            col.Resize += (s, ev) =>
            {
                if (_prodSplitNivFiches.Panel2Collapsed) return;
                int dispo = col.ClientSize.Height - lblHeader.Height - pnlCombo.Height;
                if (dispo > 40)
                    _prodSplitNivFiches.SplitterDistance = dispo / 2;
            };

            // ── Panel 1 : Niveaux ─────────────────────────────────────
            _prodSplitNivFiches.Panel1.BackColor = CREME_WARM;

            // Header + boutons CRUD figés (Dock Top)
            // Les 3 boutons (ajouter, modifier, supprimer) sont alignés à droite du header
            var pnlNivHeader = new Panel
            {
                Dock = DockStyle.Top, Height = PROD_SECTION_HEADER_H, BackColor = CREME_WARM
            };
            var lblNivHdr = new Label
            {
                Text = "NIVEAUX", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true,
                Location = new Point(8, 5), BackColor = Color.Transparent
            };
            // Bouton + pour ajouter un nouveau niveau au contexte actif
            var btnNivAdd = MakeNivActionBtn("＋", "Ajouter un niveau", 0);
            btnNivAdd.Click += ProdBtnAjouterNiveau_Click;
            // Bouton ⚙ pour modifier le niveau sélectionné (sauf N0 qui est le stock global)
            var btnNivEdit = MakeNivActionBtn("⚙", "Modifier le niveau sélectionné", 1);
            btnNivEdit.Click += (s2, ev2) =>
            {
                if (_prodSelectedNiveau != null && _prodSelectedNiveau.Ordre > 0)
                    ProdModifierNiveau(_prodSelectedNiveau);
            };
            // Bouton 🗑 pour supprimer — seul le niveau le plus haut peut être supprimé
            // (sinon on casserait la hiérarchie BOM)
            var btnNivDel = MakeNivActionBtn("🗑", "Supprimer le niveau", 2);
            btnNivDel.ForeColor = RED_CRIT;
            btnNivDel.Click += (s2, ev2) =>
            {
                if (_prodSelectedNiveau == null || _prodSelectedNiveau.Ordre == 0) return;
                var niveaux2 = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
                int topOrdre = niveaux2.Count > 0 ? niveaux2.Max(n => n.Ordre) : 0;
                // Je vérifie que c'est bien le niveau le plus haut avant d'autoriser la suppression
                if (_prodSelectedNiveau.Ordre == topOrdre)
                    ProdSupprimerNiveau(_prodSelectedNiveau);
                else
                    MessageBox.Show("Seul le niveau le plus haut peut être supprimé.",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            // Positionner les boutons à droite du header
            int bx = 196 - 3 * 34;
            btnNivAdd.Location  = new Point(bx, 0);
            btnNivEdit.Location = new Point(bx + 34, 0);
            btnNivDel.Location  = new Point(bx + 68, 0);
            pnlNivHeader.Controls.Add(lblNivHdr);
            pnlNivHeader.Controls.Add(btnNivAdd);
            pnlNivHeader.Controls.Add(btnNivEdit);
            pnlNivHeader.Controls.Add(btnNivDel);

            // Flow scrollable (cards uniquement)
            // C'est ici que les cards niveaux s'affichent, empilées de haut en bas
            _prodFlowNiveaux = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(8, 2, 8, 4)
            };
            _prodSplitNivFiches.Panel1.Controls.Add(_prodFlowNiveaux);  // Fill
            _prodSplitNivFiches.Panel1.Controls.Add(pnlNivHeader);      // Top

            // ── Panel 2 : Fiches recettes ─────────────────────────────
            // Visible uniquement quand un niveau N2+ est sélectionné
            _prodSplitNivFiches.Panel2.BackColor = CREME_WARM;
            // Je dessine une ligne de séparation en haut du Panel2
            _prodSplitNivFiches.Panel2.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, _prodSplitNivFiches.Panel2.Width, 0);
            };

            // Header + boutons CRUD figés (Dock Top) — même pattern que les niveaux
            var pnlFicheHeader = new Panel
            {
                Dock = DockStyle.Top, Height = PROD_SECTION_HEADER_H, BackColor = CREME_WARM
            };
            var lblFicheHdr = new Label
            {
                Text = "FICHES RECETTES", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true,
                Location = new Point(8, 5), BackColor = Color.Transparent
            };
            // Boutons CRUD pour les fiches : ajouter, modifier, supprimer
            var btnFicheAdd = MakeNivActionBtn("＋", "Ajouter une fiche", 0);
            btnFicheAdd.Click += (s2, ev2) => ProdAjouterFiche();
            var btnFicheEdit = MakeNivActionBtn("⚙", "Modifier la fiche sélectionnée", 1);
            btnFicheEdit.Click += (s2, ev2) => ProdModifierFiche();
            var btnFicheDel = MakeNivActionBtn("🗑", "Supprimer la fiche sélectionnée", 2);
            btnFicheDel.ForeColor = RED_CRIT;
            btnFicheDel.Click += (s2, ev2) => ProdSupprimerFiche();

            int fbx = 196 - 3 * 34;
            btnFicheAdd.Location  = new Point(fbx, 0);
            btnFicheEdit.Location = new Point(fbx + 34, 0);
            btnFicheDel.Location  = new Point(fbx + 68, 0);
            pnlFicheHeader.Controls.Add(lblFicheHdr);
            pnlFicheHeader.Controls.Add(btnFicheAdd);
            pnlFicheHeader.Controls.Add(btnFicheEdit);
            pnlFicheHeader.Controls.Add(btnFicheDel);

            // Flow scrollable pour les cards de fiches recettes
            _prodFlowFichesInner = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(8, 2, 8, 4)
            };
            _prodSplitNivFiches.Panel2.Controls.Add(_prodFlowFichesInner);  // Fill
            _prodSplitNivFiches.Panel2.Controls.Add(pnlFicheHeader);         // Top

            // Assemblage col — ordre WinForms : Fill d'abord, puis les Top
            col.Controls.Add(_prodSplitNivFiches);  // Fill
            col.Controls.Add(pnlCombo);      // Top
            col.Controls.Add(lblHeader);     // Top

            return col;
        }

        // MakeProdNiveauCard → FrmPrincipal.KanbanDetail.cs

        // ── Sélection niveau — handler maître ─────────────────────────
        // C'est LA méthode centrale : quand je clique sur un niveau,
        // tout l'écran se reconfigure (stock, fiches, simulation, KPIs, historique)
        private void ProdSelectNiveau(BomNiveau niv)
        {
            _prodSelectedNiveau = niv;
            _state.SetNiveau(niv);

            // Repaint toutes les cards niveaux pour mettre à jour le highlight doré
            foreach (var kvp in _prodNiveauPanels)
                kvp.Value.Invalidate();

            // Toujours rafraîchir : stock compact + KPI + historique, quel que soit le niveau
            ProdRefreshStockCompact(niv);
            ProdRefreshKpi();
            ProdRefreshHistorique();

            if (niv.Ordre >= 2)
            {
                // N2+ : j'affiche les fiches recettes, le panel simulation, et le stock compact en haut
                if (_prodSplitNivFiches != null) _prodSplitNivFiches.Panel2Collapsed = false;
                if (_prodPnlSimulation != null) _prodPnlSimulation.Visible = true;
                if (_prodFlowStock != null) { _prodFlowStock.Dock = DockStyle.Top; _prodFlowStock.Height = 160; }
                if (_prodBtnAchatRapide != null) _prodBtnAchatRapide.Visible = false;
                ProdChargerFicheCards();
            }
            else
            {
                // N0/N1 : pas de fiches, pas de simulation
                // Les niveaux prennent tout l'espace gauche, le stock occupe toute la zone centrale
                if (_prodSplitNivFiches != null) _prodSplitNivFiches.Panel2Collapsed = true;
                if (_prodPnlSimulation != null) _prodPnlSimulation.Visible = false;
                if (_prodFlowStock != null) _prodFlowStock.Dock = DockStyle.Fill;
                // Le bouton "Acheter" n'est visible que pour N1 (ingrédients)
                if (_prodBtnAchatRapide != null) _prodBtnAchatRapide.Visible = niv.Ordre == 1;
                _prodSelectedFiche = null;
                ProdResetSimulation();
            }
        }

        // ── Actions niveaux (absorbées de Contexte.cs) ────────────────

        // Handler du bouton "+" pour ajouter un niveau : ouvre le formulaire inline
        // L'ordre est calculé automatiquement (prochain disponible dans le contexte)
        private void ProdBtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            if (_state.ActiveContexte == null) return;
            var n = new BomNiveau
            {
                IdContexte = _state.ActiveContexte.Id,
                Ordre      = BomNiveauDAL.GetProchainOrdre(_state.ActiveContexte.Id)
            };
            ShowFormInline(new FrmBomNiveauEdit(n, false), result =>
            {
                if (result == DialogResult.OK)
                    ProdRefreshNiveauxCards();
            });
        }

        // Ouvre le formulaire de modification d'un niveau existant
        private void ProdModifierNiveau(BomNiveau niv)
        {
            ShowFormInline(new FrmBomNiveauEdit(niv, true), result =>
            {
                if (result == DialogResult.OK)
                    ProdRefreshNiveauxCards();
            });
        }

        // Supprime un niveau après confirmation utilisateur
        // Si le niveau supprimé était sélectionné, je le déselectionne
        private void ProdSupprimerNiveau(BomNiveau niv)
        {
            if (MessageBox.Show($"Supprimer le niveau « {niv.Nom} » ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomNiveauDAL.Delete(niv.Id);
                if (_state.ActiveNiveau?.Id == niv.Id) _state.SetNiveau(null);
                _prodSelectedNiveau = null;
                ProdRefreshNiveauxCards();
            }
            catch (InvalidOperationException ex)
            { MessageBox.Show(ex.Message, "Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (Exception ex)
            { MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // Appelé après toute modification de fiche (ajout, modif, suppression)
        // pour rafraîchir le stock, les fiches et les KPIs
        private void ProdRefreshAfterFicheEdit()
        {
            if (_prodSelectedNiveau != null)
            {
                ProdRefreshStockCompact(_prodSelectedNiveau);
                if (_prodSelectedNiveau.Ordre >= 2)
                    ProdChargerFicheCards();
            }
            ProdRefreshKpi();
        }

        // ── Cascade Contexte → Niveaux cards ──────────────────────────

        // Charge le combo des contextes BOM depuis la DB et sélectionne le bon
        // C'est le point d'entrée de la cascade : contexte → niveaux → fiches → stock
        private void ProdChargerContextes()
        {
            // Je débranche l'event avant de manipuler pour éviter les cascades parasites
            _prodCboContexte.SelectedIndexChanged -= ProdContexte_Changed;
            _prodCboContexte.Items.Clear();

            foreach (var ctx in BomContexteDAL.GetAll(_state.ActiveActivite.Id))
                _prodCboContexte.Items.Add(ctx);

            if (_prodCboContexte.Items.Count == 0)
            {
                // Aucun contexte BOM trouvé — j'affiche un message guide
                if (_prodLblResultat != null)
                {
                    _prodLblResultat.ForeColor = CHOCO_MED;
                    _prodLblResultat.Text = "Aucun contexte BOM — créez-en depuis la sidebar.";
                }
            }
            else
            {
                // Je tente de re-sélectionner le contexte précédemment actif (si on revient sur l'écran)
                bool found = false;
                if (_state.ActiveContexte != null)
                {
                    for (int i = 0; i < _prodCboContexte.Items.Count; i++)
                    {
                        if (((BomContexte)_prodCboContexte.Items[i]).Id == _state.ActiveContexte.Id)
                        {
                            _prodCboContexte.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }
                }
                // Sinon je prends le premier par défaut
                if (!found)
                    _prodCboContexte.SelectedIndex = 0;
            }

            // Je rebranche l'event
            _prodCboContexte.SelectedIndexChanged += ProdContexte_Changed;

            // Trigger initial load — je force le premier chargement même sans changement de sélection
            ProdContexte_Changed(null, EventArgs.Empty);
        }

        // Handler quand le contexte change dans le combo
        // Je reset la simulation et je recharge les niveaux pour ce contexte
        private void ProdContexte_Changed(object sender, EventArgs e)
        {
            ProdResetSimulation();
            if (!(_prodCboContexte.SelectedItem is BomContexte ctx))
            {
                if (_prodFlowNiveaux != null)
                {
                    DisposeAndClear(_prodFlowNiveaux.Controls);
                }
                return;
            }
            _state.SetContexte(ctx);
            ProdRefreshNiveauxCards();
        }

        // Recharge toutes les cards niveaux depuis la DB pour le contexte actif
        // Dispose les anciennes, crée les nouvelles, et auto-sélectionne
        private void ProdRefreshNiveauxCards()
        {
            if (_prodFlowNiveaux == null) return;

            _prodFlowNiveaux.SuspendLayout();
            // Je dispose les anciennes cards pour libérer les ressources GDI+
            DisposeAndClear(_prodFlowNiveaux.Controls);
            _prodNiveauPanels.Clear();

            if (_state.ActiveContexte == null)
            {
                _prodFlowNiveaux.ResumeLayout();
                return;
            }

            var niveaux     = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
            int ordreMax    = niveaux.Count > 0 ? niveaux.Max(n => n.Ordre) : 0;
            // Je récupère le nombre de fiches par niveau en un seul appel DB (optimisation)
            var ficheCounts = BomFicheDAL.GetCountsByContexte(_state.ActiveContexte.Id);

            // J'affiche les niveaux du plus haut au plus bas (produit final en premier)
            foreach (var niv in niveaux.OrderByDescending(n => n.Ordre))
            {
                ficheCounts.TryGetValue(niv.Id, out int fc);
                var card = MakeProdNiveauCard(niv, fc, ordreMax);
                _prodFlowNiveaux.Controls.Add(card);
            }

            _prodFlowNiveaux.ResumeLayout();

            // Auto-sélectionner : je reprends le niveau précédemment actif, sinon le premier
            if (_state.ActiveNiveau != null && _prodNiveauPanels.ContainsKey(_state.ActiveNiveau.Id))
            {
                var match = niveaux.Find(n => n.Id == _state.ActiveNiveau.Id);
                if (match != null) ProdSelectNiveau(match);
            }
            else if (niveaux.Count > 0)
            {
                ProdSelectNiveau(niveaux[0]);
            }
        }

        // ── Fiches cards (N2+) — panel dédié sous les niveaux ──────────

        // Charge et affiche les cards de fiches recettes pour le niveau sélectionné
        // Visible uniquement quand un niveau N2+ est actif
        private void ProdChargerFicheCards()
        {
            if (_prodFlowFichesInner == null) return;

            _prodFlowFichesInner.SuspendLayout();
            DisposeAndClear(_prodFlowFichesInner.Controls);
            _prodFichePanels.Clear();
            _prodSelectedFiche = null;

            // Pas de fiches pour N0 et N1
            if (_prodSelectedNiveau == null || _prodSelectedNiveau.Ordre < 2)
            {
                _prodFlowFichesInner.ResumeLayout();
                return;
            }

            var fiches = BomFicheDAL.GetByNiveau(_prodSelectedNiveau.Id);

            foreach (var fiche in fiches)
            {
                var card = MakeProdFicheCard(fiche);
                _prodFlowFichesInner.Controls.Add(card);
                _prodFichePanels[fiche.Id] = card;
            }

            // Message si aucune fiche n'existe pour ce niveau
            if (fiches.Count == 0)
            {
                _prodFlowFichesInner.Controls.Add(new Label
                {
                    Text = "Aucune fiche", Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    ForeColor = CHOCO_MED, AutoSize = true, Margin = new Padding(4, 4, 0, 0)
                });
            }

            _prodFlowFichesInner.ResumeLayout();

            // Auto-sélectionner la première fiche disponible
            if (fiches.Count > 0)
                ProdSelectFiche(fiches[0]);
        }

        // MakeProdFicheCard → FrmPrincipal.KanbanDetail.cs

        // ── CRUD fiches inline ────────────────────────────────────────

        // Ajouter une fiche — uniquement possible sur un niveau N2+
        // Ouvre le formulaire FrmBomFicheEdit en mode création
        private void ProdAjouterFiche()
        {
            if (_prodSelectedNiveau == null || _prodSelectedNiveau.Ordre < 2)
            {
                MessageBox.Show("Sélectionnez un niveau N2+ pour ajouter une fiche.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ShowFormInline(new FrmBomFicheEdit(null, _prodSelectedNiveau), result =>
            {
                if (result == DialogResult.OK)
                    ProdRefreshAfterFicheEdit();
            });
        }

        // Modifier la fiche sélectionnée — recharge la fiche complète (avec ses lignes BOM)
        // puis ouvre le formulaire en mode édition
        private void ProdModifierFiche()
        {
            if (_prodSelectedFiche == null)
            {
                MessageBox.Show("Sélectionnez une fiche à modifier.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Je recharge la fiche avec ses lignes BOM pour avoir les données complètes
            var ficheComplete = BomFicheDAL.GetById(_prodSelectedFiche.Id, avecLignes: true);
            ShowFormInline(new FrmBomFicheEdit(ficheComplete, _prodSelectedNiveau), result =>
            {
                if (result == DialogResult.OK)
                    ProdRefreshAfterFicheEdit();
            });
        }

        // Supprimer la fiche sélectionnée après confirmation
        // Suppression irréversible — on prévient l'utilisateur
        private void ProdSupprimerFiche()
        {
            if (_prodSelectedFiche == null)
            {
                MessageBox.Show("Sélectionnez une fiche à supprimer.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show($"Supprimer la fiche « {_prodSelectedFiche.Nom} » ?\nCette action est irréversible.",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomFicheDAL.Delete(_prodSelectedFiche.Id);
                _prodSelectedFiche = null;
                ProdRefreshAfterFicheEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Quand je clique sur une card fiche, je la sélectionne (highlight doré)
        // et je mets à jour le label info batch + je reset la simulation
        private void ProdSelectFiche(BomFiche fiche)
        {
            _prodSelectedFiche = fiche;
            // Repaint toutes les cards fiches pour mettre à jour le highlight
            foreach (var kvp in _prodFichePanels)
                kvp.Value.Invalidate();
            // J'affiche l'info du batch dans le panel simulation
            if (_prodLblInfoBatch != null)
                _prodLblInfoBatch.Text = $"1 batch = {fiche.QuantiteOutput} {fiche.UniteOutput}";
            // Nouvelle fiche = il faut re-simuler
            ProdResetSimulation();
        }

        // ════════════════════════════════════════════════════════════════
        //  COLONNE 2 — STOCK COMPACT + SIMULATION (Dock Fill)
        //  La colonne centrale : en haut un aperçu compact du stock,
        //  en dessous le panel de simulation (DGV + inputs + boutons)
        // ════════════════════════════════════════════════════════════════

        // Construit toute la colonne centrale
        private Panel BuildProdColCentrale()
        {
            var col = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM };

            // ── Header stock compact avec boutons ─────────────────────
            // Bandeau "STOCK" avec une barre verte en bas et un bouton "Acheter" (N1 only)
            var pnlStockHeader = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = CREME_WARM };
            pnlStockHeader.Paint += (s, ev) =>
            {
                // Petite barre verte (Success) en bas du header stock
                using (var br = new SolidBrush(AppColors.Success))
                    ev.Graphics.FillRectangle(br, 0, pnlStockHeader.Height - 2, pnlStockHeader.Width, 2);
            };

            var lblStockHdr = new Label
            {
                Text = "STOCK", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 100, 80), Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 2),
                BackColor = Color.Transparent
            };
            // Bouton "Acheter" pour créer un achat rapide — visible seulement en N1 (ingrédients)
            _prodBtnAchatRapide = MakeSmallButton("🛒 Acheter", AppColors.Success, Color.White);
            _prodBtnAchatRapide.Dock = DockStyle.Right;
            _prodBtnAchatRapide.AutoSize = true;
            _prodBtnAchatRapide.Visible = false;
            // Au clic, j'ouvre le formulaire d'achat, et si il retourne Retry c'est qu'il faut
            // d'abord créer un ingrédient (workflow en deux étapes)
            _prodBtnAchatRapide.Click += (s, ev) =>
            {
                if (_prodSelectedNiveau == null || _prodSelectedNiveau.Ordre != 1) return;
                using (var frm = new FrmAchatEdit(null, _state.ActiveActivite?.Id ?? 0, 0))
                {
                    var result = frm.ShowDialog(this);
                    if (result == DialogResult.OK && _prodSelectedNiveau != null)
                        ProdRefreshStockCompact(_prodSelectedNiveau);
                    else if (result == DialogResult.Retry)
                    {
                        // Le formulaire d'achat demande de créer d'abord un ingrédient
                        using (var frmIng = new FrmIngredientEdit(null))
                            frmIng.ShowDialog(this);
                        if (_prodSelectedNiveau != null)
                            ProdRefreshStockCompact(_prodSelectedNiveau);
                    }
                }
            };
            pnlStockHeader.Controls.Add(lblStockHdr);
            pnlStockHeader.Controls.Add(_prodBtnAchatRapide);

            // ── Stock compact flow ────────────────────────────────────
            // Zone scrollable qui affiche les lignes de stock en cards compactes
            _prodFlowStock = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 160,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 4)
            };

            // ── Simulation panel (toggle visible N2+ only) ────────────
            // Ce panel contient toute la logique de simulation : DGV, inputs, boutons
            // Il est masqué pour N0/N1 et visible pour N2+
            _prodPnlSimulation = new Panel
            {
                Dock = DockStyle.Fill, BackColor = CREME_WARM, Visible = false
            };
            var lblSimHeader = MakeKanbanHeader("SIMULATION", AppColors.Info);

            // Corps du panel simulation avec positionnement absolu (Location)
            var simBody = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_WARM, Padding = new Padding(12, 8, 12, 12)
            };

            int y = 0;

            // Label résultat de la simulation (✔ OK ou ✘ pénurie)
            _prodLblResultat = new Label
            {
                Text = "", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(0, y), AutoSize = false, Size = new Size(600, 20)
            };
            simBody.Controls.Add(_prodLblResultat);
            y += 22;

            // Label coût estimé (affiché seulement si la simulation est OK)
            _prodLblCoutEstime = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = AppColors.Success, Location = new Point(0, y),
                AutoSize = false, Size = new Size(600, 16)
            };
            simBody.Controls.Add(_prodLblCoutEstime);
            y += 22;

            // DGV de simulation — affiche les lignes ingrédient/fiche avec nécessaire, dispo, manque, jauge
            _prodDgvSimulation = BuildProdDgv();
            _prodDgvSimulation.Location = new Point(0, y);
            _prodDgvSimulation.Size     = new Size(600, 200);
            _prodDgvSimulation.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ProdInitSimColumns();
            // CellPainting pour dessiner la barre de jauge en custom dans la colonne "Stock %"
            _prodDgvSimulation.CellPainting += ProdDgvSimulation_CellPainting;
            // CellFormatting pour formater les quantités avec les bonnes unités
            _prodDgvSimulation.CellFormatting += ProdDgvSim_CellFormatting;
            simBody.Controls.Add(_prodDgvSimulation);
            y += 208;

            // Inputs : nombre de batches à produire
            simBody.Controls.Add(new Label
            {
                Text = "Nb batches", Font = new Font("Segoe UI", 8F),
                ForeColor = CHOCO_MED, Location = new Point(0, y), AutoSize = true
            });
            _prodNudQuantite = new NumericUpDown
            {
                DecimalPlaces = 0, Minimum = 1, Maximum = 100000, Value = 1,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(0, y + 16), Size = new Size(90, 24)
            };
            simBody.Controls.Add(_prodNudQuantite);

            // Label d'info batch à côté du NUD : rappelle combien fait 1 batch
            _prodLblInfoBatch = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = CHOCO_MED, Location = new Point(98, y + 18), AutoSize = true
            };
            simBody.Controls.Add(_prodLblInfoBatch);

            // Input délai en jours (optionnel, pour planification)
            simBody.Controls.Add(new Label
            {
                Text = "Délai (j)", Font = new Font("Segoe UI", 8F),
                ForeColor = CHOCO_MED, Location = new Point(260, y), AutoSize = true
            });
            _prodNudDelai = new NumericUpDown
            {
                Minimum = 0, Maximum = 3650, Value = 0,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(260, y + 16), Size = new Size(70, 24)
            };
            simBody.Controls.Add(_prodNudDelai);
            y += 46;

            // Zone de notes libres (optionnel, stocké avec la production)
            simBody.Controls.Add(new Label
            {
                Text = "Notes (optionnel)", Font = new Font("Segoe UI", 8F),
                ForeColor = CHOCO_MED, Location = new Point(0, y), AutoSize = true
            });
            _prodTxtNotes = new TextBox
            {
                Font = new Font("Segoe UI", 9F), Location = new Point(0, y + 16),
                Size = new Size(400, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            simBody.Controls.Add(_prodTxtNotes);
            y += 48;

            // Bouton "Simuler" — vérifie si le stock est suffisant pour la production demandée
            _prodBtnSimuler = new Button
            {
                Text = "⚡  Simuler", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = AppColors.Info, ForeColor = Color.White,
                Size = new Size(PROD_BTN_ACTION_W, PROD_BTN_ACTION_H),
                Location = new Point(0, y), Cursor = Cursors.Hand
            };
            _prodBtnSimuler.FlatAppearance.BorderColor = Color.FromArgb(40, 85, 160);
            _prodBtnSimuler.Click += ProdBtnSimuler_Click;
            simBody.Controls.Add(_prodBtnSimuler);

            // Bouton "Lancer" — exécute vraiment la production (consomme le stock)
            // Désactivé tant que la simulation n'est pas validée (pas de pénurie)
            _prodBtnLancer = new Button
            {
                Text = "▶  Lancer",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = AppColors.Success, ForeColor = Color.White,
                Size = new Size(PROD_BTN_ACTION_W, PROD_BTN_ACTION_H),
                Location = new Point(PROD_BTN_ACTION_W + 10, y),
                Enabled = false, Cursor = Cursors.Hand
            };
            _prodBtnLancer.FlatAppearance.BorderColor = Color.FromArgb(20, 100, 50);
            _prodBtnLancer.Click += ProdBtnLancer_Click;
            simBody.Controls.Add(_prodBtnLancer);

            // Légende des couleurs de la DGV
            simBody.Controls.Add(new Label
            {
                Text = "■ Faible impact   ■ Impact moyen   ■ Quasi épuisé",
                Font = new Font("Segoe UI", 7.5F), ForeColor = Color.Gray,
                Location = new Point(280, y + 8), AutoSize = true
            });

            // Au resize, j'adapte la largeur du DGV et des labels résultat
            simBody.Resize += (s, ev) =>
            {
                int w = Math.Max(300, simBody.ClientSize.Width - 24);
                _prodDgvSimulation.Width = w;
                _prodLblResultat.Width   = w;
                _prodLblCoutEstime.Width = w;
            };

            _prodPnlSimulation.Controls.Add(simBody);       // Fill
            _prodPnlSimulation.Controls.Add(lblSimHeader);   // Top

            // Assemblage colonne centrale — simulation en Fill, stock en Top
            col.Controls.Add(_prodPnlSimulation);  // Fill
            col.Controls.Add(_prodFlowStock);       // Top
            col.Controls.Add(pnlStockHeader);       // Top

            return col;
        }

        // ── Stock compact — labels/cards ──────────────────────────────

        // Rafraîchit la zone de stock compact selon le niveau sélectionné :
        // - N0 : message "pas de stock spécifique"
        // - N1 : liste des ingrédients en stock avec quantités
        // - N2+ : stock BOM agrégé par fiche (produits semi-finis)
        private void ProdRefreshStockCompact(BomNiveau niv)
        {
            if (_prodFlowStock == null) return;

            _prodFlowStock.SuspendLayout();
            DisposeAndClear(_prodFlowStock.Controls);

            try
            {
                if (niv.Ordre == 1)
                {
                    // N1 : j'affiche les ingrédients dans les stocks liés à cette activité
                    var ings = IngredientDAL.GetAllByActivite(_state.ActiveActivite.Id);
                    var enStock = ings.Where(i => i.StockActuel > 0).OrderBy(i => i.Nom).ToList();

                    if (enStock.Count == 0)
                    {
                        _prodFlowStock.Controls.Add(new Label
                        {
                            Text = "Aucun ingrédient en stock",
                            Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                            ForeColor = CHOCO_MED, AutoSize = true, Margin = new Padding(6, 8, 0, 0)
                        });
                    }
                    else
                    {
                        foreach (var ing in enStock)
                        {
                            // Pour chaque ingrédient en stock, je crée une card compacte
                            // avec le nom, la quantité formatée, les pièces et le flag d'alerte
                            var card = MakeProdStockCompactCard(
                                ing.Nom,
                                $"{UnitConvertisseur.FormatQte(ing.StockActuel, ing.UniteMesure)}",
                                ing.StockPieces > 0 ? $"{ing.StockPieces:0} pce" : "",
                                ing.EstEnAlerte);
                            _prodFlowStock.Controls.Add(card);
                        }
                    }
                }
                else if (niv.Ordre >= 2)
                {
                    // N2+ : je groupe le stock BOM par fiche pour voir combien de batches sont disponibles
                    var stocks = BomStockDAL.GetByNiveau(niv.Id);
                    var fiches = BomFicheDAL.GetByNiveau(niv.Id);
                    Trace.TraceInformation("ProdRefreshStockCompact N{0} id={1} → {2} lots, {3} fiches",
                        niv.Ordre, niv.Id, stocks.Count, fiches.Count);

                    if (stocks.Count == 0)
                    {
                        _prodFlowStock.Controls.Add(new Label
                        {
                            Text = $"Aucun stock — N{niv.Ordre} {niv.Nom}",
                            Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                            ForeColor = CHOCO_MED, AutoSize = true, Margin = new Padding(6, 8, 0, 0)
                        });
                    }
                    else
                    {
                        // Je regroupe par fiche et je calcule le total disponible + nb de batches complets
                        var parFiche = stocks
                            .GroupBy(s => s.IdFiche)
                            .Select(g =>
                            {
                                var ficheRef = fiches.Find(f => f.Id == g.Key);
                                decimal totalDispo = g.Sum(s => s.QuantiteDisponible);
                                decimal qteOutput = ficheRef?.QuantiteOutput ?? 1;
                                // Nombre de batches complets en stock (division entière)
                                int batchesEnStock = qteOutput > 0 ? (int)(totalDispo / qteOutput) : 0;
                                return new
                                {
                                    NomFiche   = g.First().NomFiche,
                                    Unite      = g.First().UniteOutput,
                                    TotalDispo = totalDispo,
                                    Batches    = batchesEnStock,
                                    IsZero     = totalDispo <= 0
                                };
                            })
                            .OrderBy(x => x.NomFiche)
                            .ToList();

                        foreach (var item in parFiche)
                        {
                            var card = MakeProdStockCompactCard(
                                item.NomFiche,
                                $"{item.Batches} unité{(item.Batches != 1 ? "s" : "")}",
                                UnitConvertisseur.FormatQte(item.TotalDispo, item.Unite),
                                item.IsZero);
                            _prodFlowStock.Controls.Add(card);
                        }
                    }
                }
                else
                {
                    // N0 : c'est le stock global partagé, pas de détail spécifique à afficher
                    _prodFlowStock.Controls.Add(new Label
                    {
                        Text = "Niveau partagé — pas de stock spécifique",
                        Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                        ForeColor = CHOCO_MED, AutoSize = true, Margin = new Padding(6, 8, 0, 0)
                    });
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("ProdRefreshStockCompact : {0}", ex);
                _prodFlowStock.Controls.Add(new Label
                {
                    Text = "Erreur chargement stock",
                    Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    ForeColor = RED_CRIT, AutoSize = true, Margin = new Padding(6, 8, 0, 0)
                });
            }

            _prodFlowStock.ResumeLayout();
        }

        // MakeProdStockCompactCard → FrmPrincipal.KanbanDetail.cs

        // ════════════════════════════════════════════════════════════════
        //  COLONNE 3 — HISTORIQUE + JOURNAL (Dock Right, 260px)
        //  Colonne de droite avec l'historique des dernières productions
        //  et un mini-journal en bas qui trace les actions récentes
        // ════════════════════════════════════════════════════════════════

        // Construit la colonne droite : flow historique en haut, journal en bas
        private Panel BuildProdColHistorique()
        {
            var col = new Panel { Dock = DockStyle.Right, Width = PROD_COL_HISTORIQUE_W, BackColor = CREME_WARM };
            var lblHeader = MakeKanbanHeader("HISTORIQUE", OR);

            // Flow scrollable pour les cards d'historique de production
            _prodFlowHistorique = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 4)
            };

            // Mini-journal en bas — panel fixe de 140px qui affiche les 5 dernières actions
            _prodJournalPanel = new Panel
            {
                Dock = DockStyle.Bottom, Height = 140, BackColor = Color.White
            };
            // Je dessine une ligne de séparation en haut et une barre accent à gauche
            _prodJournalPanel.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, _prodJournalPanel.Width, 0);
                using (var br = new SolidBrush(CHOCO_MED))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, _prodJournalPanel.Height);
            };
            _prodJournalPanel.Controls.Add(new Label
            {
                Text = "JOURNAL", Location = new Point(14, 6),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true
            });

            // Initialisation du journal avec un message d'attente
            _prodJournalEntries.Clear();
            _prodJournalEntries.Add($"[{DateTime.Now:dd/MM HH:mm}] En attente");
            ProdRefreshJournal();

            col.Controls.Add(_prodFlowHistorique);  // Fill
            col.Controls.Add(_prodJournalPanel);     // Bottom
            col.Controls.Add(lblHeader);             // Top

            return col;
        }

        // ── Historique filtré par niveau ──────────────────────────────

        // Rafraîchit l'historique des productions récentes
        // Filtré par niveau si N2+, vide pour N0/N1 (pas de production à ces niveaux)
        private void ProdRefreshHistorique()
        {
            try
            {
                // Je récupère les 20 dernières productions de l'activité
                var prods = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 20);

                // Filtrage selon le niveau sélectionné
                if (_prodSelectedNiveau != null && _prodSelectedNiveau.Ordre > 1)
                    prods = prods.Where(pp => pp.IdNiveau == _prodSelectedNiveau.Id).ToList();
                else if (_prodSelectedNiveau != null && _prodSelectedNiveau.Ordre <= 1)
                    prods = new List<BomProduction>();

                // Je ne garde que les 10 premières pour l'affichage
                prods = prods.Take(10).ToList();
                ProdRemplirHistoriqueCards(prods);

                // Mise à jour du mini-journal avec les 5 dernières productions
                _prodJournalEntries.Clear();
                foreach (var pp in prods.Take(5))
                {
                    int u = pp.QuantiteOutputBatch > 0 ? (int)(pp.QuantiteProduite / pp.QuantiteOutputBatch) : 0;
                    _prodJournalEntries.Add($"[{pp.DateProduction:dd/MM HH:mm}] {pp.NomFiche} × {u}");
                }
                if (_prodJournalEntries.Count == 0)
                    _prodJournalEntries.Add($"[{DateTime.Now:dd/MM HH:mm}] En attente");
                ProdRefreshJournal();
            }
            catch (Exception ex)
            {
                Trace.TraceError("ProdRefreshHistorique : {0}", ex);
            }
        }

        // Remplit le flow historique avec les cards de production
        // Dispose les anciennes cards et en crée de nouvelles
        private void ProdRemplirHistoriqueCards(List<BomProduction> prods)
        {
            if (_prodFlowHistorique == null) return;

            _prodFlowHistorique.SuspendLayout();
            DisposeAndClear(_prodFlowHistorique.Controls);

            foreach (var prod in prods)
            {
                var card = MakeProdHistoriqueCard(prod);
                _prodFlowHistorique.Controls.Add(card);
            }

            // Message si aucune production dans l'historique
            if (prods.Count == 0)
            {
                _prodFlowHistorique.Controls.Add(new Label
                {
                    Text = "Aucune production", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = CHOCO_MED, AutoSize = true, Margin = new Padding(8, 12, 0, 0)
                });
            }

            _prodFlowHistorique.ResumeLayout();
        }

        // DGV Factory, cards, UI helpers → FrmPrincipal.KanbanDetail.cs
        // Simulation + journal           → FrmPrincipal.Simulation.cs
    }
}
