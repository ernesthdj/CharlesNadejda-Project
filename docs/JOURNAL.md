# Journal de Développement — Charles & Nadejda
> Carnet d'historique des modifications · ordre antéchronologique (plus récent en premier)
> Projet : App gestion artisanale (C# WinForms) + Site vitrine/boutique (Laravel 11)
> BDD partagée : `charlesnadejda` (MySQL 8 via Docker)

---

## Règles apprises
> Section cumulative — chaque erreur corrigée y laisse une trace permanente.

| #   | Règle                                                                                                                                                                                                           | Fichier(s) concerné(s)                                     | Date       |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- | ---------- |
| 1   | `.csproj` format classique .NET 4.8.1 : tout nouveau fichier `.cs` doit être ajouté manuellement dans `<Compile Include="..." />` — sans ça, le compilateur ignore le fichier et lève CS0246 "type introuvable" | `CharlesNadejda.csproj`                                    | 2026-03-30 |
| 2   | Ne jamais utiliser `DB_HOST=localhost` dans le `.env` Laravel sous Docker — utiliser `DB_HOST=mysql` (nom du service docker-compose)                                                                            | `site-laravel/.env`                                        | 2026-03-27 |
| 3   | MySQL 8 n'enforce pas les CHECK constraints sur les FK — toute validation de cohérence doit être faite dans les DAL (C#) ou les Use Cases (Laravel), pas en base                                                | `DAL/*.cs`                                                 | 2026-03-30 |
| 4   | `bom_fiches` liée à un niveau spécifique (pas juste à une activité) — sans `id_niveau`, impossible de filtrer par niveau ni d'imposer que les inputs viennent du N-1                                            | `DAL/BomFicheDAL.cs`, `sql/migration_v5_fiches_niveau.sql` | 2026-04-xx |
| 5   | Quand on ajoute un champ au Model, vérifier systématiquement les 4 endroits du DAL : SELECT · INSERT · UPDATE · Map() — oublier l'un d'eux laisse le champ toujours NULL en base                              | `DAL/IngredientDAL.cs`                                      | 2026-04-09 |
| 6   | Tout formulaire liste (DGV) doit être `Sizable` + `AutoSizeColumnsMode.AllCells` + `MinimumWidth` par colonne + Anchor DGV aux 4 bords. Ne jamais utiliser `Fill` seul sans plancher.                          | `Forms/FrmIngredients.Designer.cs`                          | 2026-04-09 |
| 7   | L'unité d'une ligne de fiche BOM est toujours celle de l'ingrédient/fiche source (N-1). Le ComboBox d'unité doit être verrouillé (`Enabled=false`) après sélection de l'input — toute autre unité = calcul faux | `Forms/FrmBomFicheEdit.cs`                                  | 2026-04-09 |
| 8   | Quand on passe d'un ENUM à une FK, supprimer l'ancienne colonne ET ajouter la nouvelle dans le même ALTER TABLE (pas deux instructions séparées) — sinon la FK est ajoutée sur une colonne non-nullable sans défaut et INSERT échoue | `sql/migration_v7_activites.sql`                            | 2026-04-13 |
| 9   | Les activités ne doivent jamais être codées en dur dans le C# (string "chocolaterie"). Toute référence à une activité passe par l'objet `Activite` (Id + Nom) chargé depuis `ActiviteDAL`. Violation = bug silencieux sur ajout d'une nouvelle activité. | `DAL/ActiviteDAL.cs`, `Models/Activite.cs`                  | 2026-04-13 |
| 10  | Après tout refactor de signature DAL ou Model, grepper tous les call sites avant de clore la tâche — FrmBomSimulation et FrmAchatEdit avaient été oubliés lors du refactor v7, causant CS1503 au build. | `Forms/FrmBomSimulation.cs`, `Forms/FrmAchatEdit.cs`        | 2026-04-13 |
| 11  | Ordre d'ajout WinForms (non-partial, Controls.Add programmatique) : le Fill DOIT être ajouté en index 0 (premier), puis Bottom, puis Top en dernier. Le docking est traité en ordre inverse — si Top est en index 0, le Fill le recouvre partiellement. | `Forms/FrmActivites.cs`                                     | 2026-04-13 |
| 12  | Dans un gestionnaire `(object sender, EventArgs e)`, tout lambda interne doit utiliser `ev` (ou `_`) et non `e` — CS0136 : le compilateur C# interdit de redéclarer un nom déjà présent dans une portée englobante, même si c'est un paramètre de lambda. | `Forms/FrmIngredientEdit.cs`                                | 2026-04-14 |
| 13  | Après tout refactor de signature avec paramètres optionnels reordonnés (ex: `GetAll(int idStock=0, int idActivite=0)` au lieu de `GetAll(int idActivite=0)`), grepper TOUS les call sites et remplacer les appels positionnels par des paramètres nommés — sinon l'argument est silencieusement passé au mauvais paramètre. | `Forms/FrmAchatEdit.cs`, `Forms/FrmBomFicheEdit.cs`         | 2026-04-14 |
| 14  | Dans `lots_ingredients`, la colonne DLC s'appelle `date_peremption` (pas `date_dlc`). Dans les VIEWs qui l'exposent, toujours aliaser : `li.date_peremption AS date_dlc` pour uniformiser avec `bom_stocks.date_dlc`. | `sql/migration_v11_stock_discriminants.sql`, `DAL/VueStockGlobalDAL.cs` | 2026-04-15 |
| 15  | `quantiteCible` dans `BomProductionDAL` = **nombre de batches**, pas une quantité finale. La quantité réellement produite et stockée = `quantiteCible × fiche.QuantiteOutput`. Confondre les deux = stock × 10 trop faible. | `DAL/BomProductionDAL.cs` | 2026-04-15 |
| 16  | Dans la branche `TypeInput == "fiche"` de BomProductionDAL, `ligne.UniteMesureInput` = unité output de la fiche source. La conversion `UnitConvertisseur.Convertir(qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput)` est OBLIGATOIRE avant toute comparaison ou décrémentation de bom_stocks. Omettre = pénurie fantôme (g vs kg). | `DAL/BomProductionDAL.cs` | 2026-04-15 |
| 17  | Une VIEW MySQL ne contient que les colonnes explicitement sélectionnées. Si l'on a besoin d'un label dérivé d'une FK (ex: `nom_activite` depuis `activites`), le JOIN doit être dans le DAL (requête SQL), pas dans le Model C#. Éviter de résoudre un FK manquant côté applicatif avec string concatenation (`"act. " + id`). | `DAL/VueStockGlobalDAL.cs`, `Models/VueStockGlobal.cs` | 2026-04-15 |
| 18  | `Controls.Clear()` sur un conteneur hébergeant des Forms (ex: `_pnlDroit` en SFA) ne Dispose PAS les enfants — chaque navigation laisse une Form orpheline vivante avec tous ses handlers. Implémenter une méthode helper qui itère et dispose chaque enfant avant Clear. Symptôme : latence à la fermeture de l'app (dispose en cascade). | `Forms/FrmPrincipal.cs` (ClearAndDisposePanel) | 2026-04-22 |
| 19  | Les migrations SQL dans `docker-entrypoint-initdb.d` s'exécutent en ordre alphabétique. TOUJOURS zéro-padder les numéros de version (`v01`, `v02`... `v10`) pour garantir l'ordre chronologique. Ne jamais placer de script de reset/test dans le dossier d'init Docker. | `sql/migration_v0X_*.sql` | 2026-04-22 |
| 20  | `DataGridView.AutoResizeColumns(AllCells)` mesure chaque cellule → coût proportionnel au nombre de lignes. Préférer `AllCellsExceptHeader` ou largeurs fixes explicites par colonne. Toujours entourer les modifications DataSource+Columns par `SuspendLayout`/`ResumeLayout` sur le DGV. | `Forms/FrmPrincipal.cs` (ChargerFiches, ChargerStockNiveau) | 2026-04-22 |
| 21  | Les colonnes INT MySQL sont retournées comme `Int32` par MySql.Data, mais `COUNT(*)` en tant que `Int64` — un cast `(int)(long)r["col"]` plante sur une colonne INT simple. Utiliser `Convert.ToInt32(r["col"])` pour une gestion sûre des types numériques hétérogènes. | `DAL/BomFicheDAL.cs` (GetCountsByContexte) | 2026-04-22 |
| 22  | Pour éviter la duplication du cycle validation/sauvegarde dans les formulaires d'édition, toute Form Edit doit hériter de `FrmEditBase` — ne jamais redéclarer `btnEnregistrer`, `btnAnnuler`, `errorProvider`. Implémenter uniquement `Valider()` et `Sauvegarder()`. | `Forms/FrmEditBase.cs`, `Forms/Frm*Edit.cs` | 2026-04-22 |
| 23  | Ne jamais nommer une classe `StatusBarPanel` dans un projet WinForms — `System.Windows.Forms.StatusBarPanel` existe déjà et crée une référence ambiguë même sans `using` explicite. Préférer un nom distinctif comme `AppStatusBar`. | `Forms/Shell/StatusBarPanel.cs` → `AppStatusBar` | 2026-05-13 |
| 24  | Pour afficher des quantités avec unité dans un DGV lié par DataSource, utiliser `CellFormatting` plutôt que modifier le modèle — cacher la colonne Unité séparée et suffixer via `FormatQte()`. Ne jamais manipuler `e.Value` sur une colonne auto-générée en dehors de CellFormatting. | `Forms/FrmIngredients.cs`, `Forms/FrmAchats.cs` et 5 autres | 2026-05-13 |
| 25  | Le tri natif du DGV (`SortMode.Automatic`) est incompatible avec les lignes de section header insérées manuellement — le tri mélange headers et données. Désactiver le tri (`SortMode.NotSortable`) dès qu'on utilise des groupes visuels. | `Forms/FrmVueStock.cs` | 2026-05-13 |

---

## Historique

---

### SESSION 18 — 2026-05-13
> Refactor ERP majeur : nouveau Shell (TitleBar + Sidebar Odoo-style + StatusBar), audit P1/P2, fusion unités/prix dans DGV, auto-conversion intelligente.

---

#### [2026-05-13] FEAT — Shell ERP : TitleBar + Sidebar + StatusBar
**Fichiers :** `Forms/Shell/TitleBarPanel.cs`, `Forms/Shell/SidebarPanel.cs`, `Forms/Shell/StatusBarPanel.cs` (nouveaux), `Forms/FrmPrincipal.cs` (restructuration majeure), `Forms/FrmPrincipal.Designer.cs`
**Résumé :** Remplacement complet du SplitContainer + 3 ListBoxes + 5 boutons ressources par un shell ERP moderne inspiré Odoo/Dynamics. TitleBar 38px avec gradient chocolat, logo monogramme "C" doré, titre document dynamique, indicateur MySQL, avatar utilisateur. Sidebar 224px avec ActivitySwitcher (ComboBox OwnerDraw), 3 groupes de navigation (Workflow / Stock & Achats / Référentiels), items avec hover/active + badges dorés. StatusBar 26px affichant connexion, activité, contexte, niveau courants. 12 NavItemId mappés vers les ScreenId existants + 4 placeholders. MenuStrip conservé invisible comme escape hatch. -409/+171 lignes dans FrmPrincipal.
**Règle retenue :** Pour le layout DockStyle WinForms avec 4 zones (Top/Left/Bottom/Fill), ajouter Fill en premier, puis Bottom, Left, Top — dans cet ordre exact.

---

#### [2026-05-13] FEAT — Fusion unités + prix dans cellules DGV + auto-conversion
**Fichiers :** `Forms/UnitConvertisseur.cs`, `Forms/FrmIngredients.cs`, `Forms/FrmAchats.cs`, `Forms/FrmBomFiches.cs`, `Forms/FrmBomProductionSimulation.cs`, `Forms/FrmPrincipal.cs`, `Forms/FrmVueStock.cs`
**Résumé :** Suppression de toutes les colonnes "Unité" séparées dans les DGV — l'unité est fusionnée directement dans la valeur via CellFormatting (ex: `1,73 l` au lieu de `1.734` + `l`). Auto-conversion intelligente : 652 cl → 6,52 l, 1500 g → 1,50 kg. Valeurs rondes sans décimale (5 l, pas 5,00 l). Prix toujours avec 2 décimales + symbole € fusionné (30,00 €). Vue Stock Global avec 3 sections groupées (Ingrédients / Intermédiaires / Finals).
**Règle retenue :** Utiliser `UnitConvertisseur.FormatQte()` et `FormatPrix()` pour tout affichage de quantité/prix dans les DGV — garantit cohérence et lisibilité sur l'ensemble de l'app.

---

#### [2026-05-13] FIX — Audit P1/P2 post-remote sync
**Fichiers :** `DAL/ActiviteDAL.cs`, `Forms/FrmBomProductionSimulation.cs`, `Forms/FrmPrincipal.cs`, `Forms/AppColors.cs` + 6 autres
**Résumé :** 3 corrections P1 (ActiviteDAL.Desactiver requête obsolète post-v10, async void sans try/catch global, 5× ShowDialog sans using) + centralisation palette AppColors dans 10 fichiers (suppression ~30 constantes Color.FromArgb dupliquées). Ajout AppColors.HintOnDark.

---

### SESSION 17 — 2026-04-22
> Refactoring structurel post-audit : nettoyage orphelins + scope ERP pur + migration massive vers FrmEditBase + fix fuite mémoire + doc d'architecture.

---

#### [2026-04-22] DOCS — ARCHITECTURE.md pour défense orale
**Fichiers :** `docs/ARCHITECTURE.md`
**Résumé :** Documentation d'architecture avec 3 diagrammes Mermaid (couches, SFA, modules métier). Pitch en 3 phrases pour l'oral d'examen, tableau module→Forms→Base héritée, patterns appliqués (Template Method, Generics, SFA, DAL), règles WinForms critiques, arborescence. Prêt comme support de défense.

---

#### [2026-04-22] REFACTOR — Migration Phase 2 : 8 Forms Edit vers FrmEditBase
**Fichiers :** `Forms/FrmBomNiveauEdit.cs`, `Forms/FrmFournisseurEdit.cs`, `Forms/FrmStockEdit.cs`, `Forms/FrmIngredientEdit.cs`, `Forms/FrmActiviteEdit.cs`, `Forms/FrmBomContexteEdit.cs`, `Forms/FrmBomFicheEdit.cs`, `Forms/FrmAchatEdit.cs`, `Forms/StringExtensions.cs` (extrait)
**Résumé :** Les 8 Forms d'édition ERP héritent maintenant de FrmEditBase. Pattern uniforme : champs + `Valider()` + `Sauvegarder()`, cycle bouton Enregistrer/Annuler + ErrorProvider centralisés. 6 fichiers Designer.cs supprimés (FrmFournisseurEdit, FrmBomNiveauEdit, FrmIngredientEdit, FrmBomContexteEdit, FrmBomFicheEdit, FrmAchatEdit — les 2 autres étaient déjà en code-behind). `StringExtensions.NullIfEmpty()` extrait dans son propre fichier. **Total : −1 110 lignes** éliminées.
**Règle retenue :** Pour éviter la duplication du plumbing d'édition, faire hériter toute nouvelle Form Edit de `FrmEditBase` — ne jamais redéclarer `btnEnregistrer`, `btnAnnuler`, `errorProvider`.

---

#### [2026-04-22] FIX — Fuite mémoire navigation SFA (ClearAndDisposePanel)
**Fichiers :** `Forms/FrmPrincipal.cs`
**Résumé :** Remplacement des 4 occurrences de `_pnlDroit.Controls.Clear()` par `ClearAndDisposePanel()` qui dispose les Forms enfants en plus de les retirer du panel. Avant : chaque navigation laissait une Form orpheline vivante en mémoire avec tous ses handlers DAL — visible à la fermeture (dispose en cascade). Après : transitions instantanées, fermeture sans latence.
**Règle retenue :** `Controls.Clear()` sur un conteneur hébergeant des Forms n'appelle PAS `Dispose()` — implémenter systématiquement une méthode helper qui itère et dispose chaque enfant.

---

#### [2026-04-22] PERF — Optimisation écran Contexte (N+1 + cache)
**Fichiers :** `Forms/FrmPrincipal.cs`, `DAL/BomFicheDAL.cs`
**Résumé :** Trois optimisations sur le chargement contexte/niveau/fiches/stock : (1) `BomFicheDAL.GetCountsByContexte()` ajoute GROUP BY pour remplacer N requêtes par niveau par 1 seule — confirmé par Convert.ToInt32 pour compatibilité types MySQL Int32/Int64. (2) `IngredientDAL.GetAll()` chargé 1× dans ChargerFiches et passé en cache à ChargerStockNiveau (était appelé 2× consécutivement en N1). (3) `AutoResizeColumns(AllCells)` remplacé par largeurs explicites par colonne + `SuspendLayout`/`ResumeLayout` sur les 2 DGV. Gain utilisateur : transitions nettes, plus de freeze visible.
**Règle retenue :** `DataGridView.AutoResizeColumns(AllCells)` mesure CHAQUE cellule → coûteux sur gros datasets. Préférer `AllCellsExceptHeader` ou largeurs fixes. Toujours entourer les modifications DataSource+Columns par `SuspendLayout`/`ResumeLayout`.

---

#### [2026-04-22] REFACTOR — Suppression module Catalogue Web (scope ERP pur)
**Fichiers :** `Forms/FrmCategor*`, `Forms/FrmParfum*`, `Forms/FrmProduit*`, `Forms/FrmRecette*`, `DAL/CategorieDAL.cs`, `DAL/ParfumDAL.cs`, `DAL/ProduitDAL.cs`, `DAL/RecetteDAL.cs`, `Models/Categorie.cs`, `Models/Parfum.cs`, `Models/Produit.cs`, `Models/Recette.cs`, `Models/RecetteIngredient.cs`, `Forms/FrmPrincipal.cs`, `Navigation/RessourceType.cs`
**Résumé :** Décision périmètre : l'app C# est un **ERP pur** (calcul production depuis ingrédients + bilan), le catalogue (catégories, parfums, produits, recettes) est **hors scope** — il relèvera d'une future intégration avec le site Laravel. Suppression de 24 fichiers : 15 Forms + 4 DALs + 5 Models. Menus du Catalogue dans FrmPrincipal redirigent vers `PlaceholderWeb()` ("Module Catalogue Web — à venir"). `RessourceType` réduit à 5 entrées ERP (Stocks, Ingredients, Fournisseurs, Achats, VueStock). **Total : −1 500 lignes** supprimées.

---

#### [2026-04-22] CHORE — Nettoyage orphelins Forms
**Fichiers :** suppression de `Form1.cs/.Designer.cs/.resx`, `Forms/FrmArtisaStock.cs/.Designer.cs`, `Forms/FrmBomSimulation.cs/.Designer.cs`
**Résumé :** 3 Forms orphelines confirmées via `grep -r "new FrmX("` → 0 instanciation. Form1 = résidu du template VS par défaut. FrmArtisaStock = ancien dashboard BOM entièrement repris par FrmPrincipal. FrmBomSimulation = remplacée par FrmBomProductionSimulation. **−1 080 lignes** de code mort supprimées du .csproj.

---

#### [2026-04-22] SECURITY — Retirer App.config du git + App.config.example + cn_user/cn_password
**Fichiers :** `.gitignore`, `App.config.example` (nouveau), `App.config` (modifié puis untracked via `git rm --cached`)
**Résumé :** L'audit DevOps avait flaggé SEC-1 critique : `App.config` versionné avec `root/root` en clair. Fix : `App.config` ajouté à `.gitignore`, `App.config.example` créé avec valeurs REMPLACER, connection string modifiée en `cn_user/cn_password` (utilisateur dédié Docker — principe de moindre privilège). Le `.env` Laravel était déjà dans `.gitignore` et non-tracké, pas d'action nécessaire.
**Règle retenue :** Les fichiers de config contenant des credentials doivent être en `.gitignore` et accompagnés d'un fichier `.example` versionné avec valeurs fictives.

---

#### [2026-04-22] INFRA — Ordre migrations Docker : zéro-padding + reset hors init
**Fichiers :** `sql/migration_v4_bom.sql` → `sql/migration_v04_bom.sql` (et v5→v09), `sql/reset_db_for_tests.sql` → `sql/tests/reset_db_for_tests.sql`
**Résumé :** L'audit DevOps avait flaggé INFRA-1 : tri alphabétique des fichiers SQL dans `docker-entrypoint-initdb.d` exécutait v10→v13 AVANT v4→v9 (cassant les ALTER TABLE). Fix : zéro-padding sur v04→v09 + déplacement de `reset_db_for_tests.sql` dans `sql/tests/` pour l'exclure du volume Docker init.
**Règle retenue :** Les migrations SQL dans un volume Docker init s'exécutent en ordre alphabétique → TOUJOURS zéro-padder les numéros de version (`v01`, `v02`... `v10`, `v11`) pour garantir l'ordre chronologique.

---

### SESSION 16 — 2026-04-22

---

#### [2026-04-22 00:00] DOCS — audit multi-agents 2026-04-22
**Fichiers :** `docs/AUDIT_2026-04-22.md` — audit complet pipeline #1→#7
**Résumé :** Audit transversal post-sprints P0→P3. 7 agents, scores : PO ~80% US, Architect 7.2/10, UI/UX 7.4/10, Backend 8.0/10, Frontend 7.0/10, QA 6.0/10, DevOps 4.5/10. Items P0 : App.config/.env en git (SEC-1/2 critiques), #if DEBUG credentials FrmLogin, FrmListeBase<T> encode ShowDialog() standardisant la violation SFA, ordre migrations Docker cassé (tri alphabétique v10 avant v4), btnDel Size(24,22) violation Fitts persistante, palette dupliquée FrmPrincipal vs AppColors, commentaire SFA trompeur ligne 15. Score global estimé 7.2/10 (↑ depuis 6.3/10).

---

### SESSION 15 — 2026-04-21
> Bugfixes volets N1 : filtrage par activité, stock instancié uniquement. NRE SelectionChanged. Navigation singleton centralisée via NavigateTo().

---

#### [2026-04-21] FIX — Volets N1 : filtrage par activité + stock instancié seulement
**Fichiers :** `Forms/FrmPrincipal.cs`
**Résumé :** Les deux volets N1 (fiches gauche + stock droite) appelaient `IngredientDAL.GetAll()` sans filtre → affichaient les ingrédients de TOUTES les activités. Fix : `GetAll(idActivite: _state.ActiveActivite?.Id ?? 0)` sur les deux appels. Volet droit affiné : `.Where(i => i.StockActuel > 0)` — n'affiche que les ingrédients instanciés (au moins un lot acheté disponible). Les ingrédients catalogue sans stock disparaissent du volet droit.

---

#### [2026-04-21] FIX — NRE SelectionChanged (lambda capture champ nullé)
**Fichiers :** `Forms/FrmPrincipal.cs` (ligne ~916)
**Résumé :** Le lambda `_dgvFiches.SelectionChanged` capturait le champ `_dgvFiches` par référence. Lors d'une re-navigation, `_dgvFiches = null` (ligne 762) puis `Controls.Clear()` → l'ancien DGV déclenche `SelectionChanged` → NRE. Fix : variable locale `dgvFichesCapture = _dgvFiches` capturée par instance dans le lambda + guard `btnDupFiche.IsDisposed`.

---

#### [2026-04-21] REFACTOR — Navigation singleton : NavigateTo() + ScreenRouter guard
**Fichiers :** `Navigation/ScreenRouter.cs`, `Forms/FrmPrincipal.cs`
**Résumé :** Trois couches de protection contre les re-navigations inutiles :
1. **ScreenRouter** : mémorise `(_lastScreen, _lastContexteId, _lastRessource)` — `Navigate()` est un no-op si état identique. `Invalidate()` force le prochain appel à passer.
2. **ListBox handlers** : guard ID avant `ChargerContextes/Niveaux` (opérations coûteuses). `LstActivites`, `LstContextes`, `LstNiveaux` retournent immédiatement si même item sélectionné.
3. **`NavigateTo(screen, stateSetup, forceRefresh)`** : méthode unique dans FrmPrincipal — tous les 15 call sites migrent vers elle. Élimine les patterns `{ setup(); _router.Navigate(); }` dispersés. `forceRefresh: true` pour les callbacks post-CRUD (add/edit/delete niveau, contexte, production).

**Règle retenue :** Toute navigation dans FrmPrincipal passe par `NavigateTo`. Jamais `_router.Navigate()` ni `ShowXxxScreen()` directement depuis les handlers.

---

### SESSION 14 — 2026-04-21
> Sprint post-audit suite : T-14 (migration FrmListeBase<T> × 4 formulaires liste).

---

#### [2026-04-21] REFACTOR — T-14 : FrmBomContextes, FrmBomFiches, FrmBomNiveaux, FrmAchats → FrmListeBase<T>
**Fichiers :**
- `Forms/FrmBomContextes.cs` — migration complète + btnNiveaux extra à BtnYExtra=196
- `Forms/FrmBomFiches.cs` — migration + TICKET-19 _lblEtatVide préservé via AppliquerStylesLignes()
- `Forms/FrmBomNiveaux.cs` — migration + logique GetOrdreMax+1 dans OuvrirFormulaire(null)
- `Forms/FrmAchats.cs` — migration standard (NomElement = NomIngredient + DateAchat)
- `Forms/FrmBomContextes.Designer.cs` — supprimé
- `Forms/FrmBomFiches.Designer.cs` — supprimé
- `Forms/FrmBomNiveaux.Designer.cs` — supprimé
- `Forms/FrmAchats.Designer.cs` — supprimé
- `CharlesNadejda.csproj` — 4 entrées SubType+Designer remplacées par `<Compile Include="..." />`

**Résumé :** Les 4 formulaires partial héritant de Form migrés vers FrmListeBase<T>. Particularités préservées : (1) FrmBomContextes ajoute btnNiveaux via OnLoad avant base.OnLoad(e) ; (2) FrmBomFiches crée _lblEtatVide dans OnLoad et le gère via AppliquerStylesLignes() → dgv.Rows.Count==0 ; (3) FrmBomNiveaux calcule GetOrdreMax dans OuvrirFormulaire(null) ; (4) FrmAchats NomElement() = NomIngredient + DateAchat formatée.

---

### SESSION 13 — 2026-04-21
> Sprint post-audit : corrections P0 → P1. Tickets T-01 à T-13 + T-16 à T-21 (P0+P1 complets). T-05, T-06, T-07 également appliqués (UX P0).

---

#### [2026-04-21] UX — cboActivite → Label + btnSuppCtx/btnSuppNiv agrandis (T-05, T-06, T-07)
**Fichiers :** `Forms/FrmBomFicheEdit.cs`, `Forms/FrmArtisaStock.cs`
**Résumé :** T-05 : `cboActivite` + `lblActivite` supprimés de `FrmBomFicheEdit` ; remplacés par `lblActiviteValeur` (lecture seule) alimenté via `_niveau.ActiviteNom` — élimine un champ grisé sans utilité cognitive (Nielsen #8). T-06 : `btnSuppCtx` → `Size(90,30)` + texte "Supprimer" + ToolTip dans `FrmArtisaStock` (Fitts — ex 36px < seuil 44px). T-07 : `btnSuppNiv` dans les cards → `Size(36,28)` + ToolTip "Supprimer ce niveau" (Fitts — ex 28×22 sous le minimum).

---

#### [2026-04-21] FIX — Race condition TOCTOU + réservations (T-01, T-08)
**Fichiers :** `DAL/BomProductionDAL.cs`
**Résumé :** `VerifierDisponibilite()` déplacé DANS la transaction (T-01). Ajout d'un `UPDATE bom_reservations SET actif=0 WHERE id_lot=@id AND actif=1` inline dans ConsumeStock après chaque décrémentation de lot (T-08), dans la même `conn`+`tx`.

#### [2026-04-21] DB — Migration v13 : CHECK quantite_disponible >= 0 (T-02)
**Fichiers :** `sql/migration_v13_check_stock_positif.sql`
**Résumé :** Contraintes CHECK ajoutées sur `lots_ingredients.quantite_disponible` et `bom_stocks.quantite_disponible`. MySQL 8 enforce ces CHECK sur scalaires — protection filet de dernier recours contre stocks négatifs.

#### [2026-04-21] FIX — Null-guards fiche/niveau + guards UnitConvertisseur (T-03, T-04)
**Fichiers :** `DAL/BomProductionDAL.cs`, `Forms/UnitConvertisseur.cs`
**Résumé :** Guards `if (fiche == null)` et `if (niveau == null)` dans Executer() avec `InvalidOperationException` explicite (T-03). Guards `ArgumentNullException` sur `uniteSource`/`uniteCible` et `ArgumentOutOfRangeException` sur valeur négative dans `UnitConvertisseur.Convertir()` (T-04).

#### [2026-04-21] REFACTOR — Anti-cycle BomCoutDAL via HashSet<int> (T-09)
**Fichiers :** `DAL/BomCoutDAL.cs`
**Résumé :** Surcharge privée `CalculerCout(idFiche, nBatches, HashSet<int> fichesVisitees)` — détecte les cycles (fiche A → B → A) et lève `InvalidOperationException` explicite. L'entrée publique crée un `new HashSet<int>()`. `CalculerLigneFiche` propage le HashSet avec une copie (`new HashSet<int>(fichesVisitees)`) pour éviter les faux-positifs sur branches parallèles.

#### [2026-04-21] FIX — Catches silencieux remplacés par Trace.TraceError (T-10, T-11)
**Fichiers :** `Forms/FrmPrincipal.cs`, `Forms/FrmBomProductionSimulation.cs`
**Résumé :** Les deux `catch { }` dans `ShowHubScreen()` (chargement DAL + contextes) remplacés par `catch (Exception ex) { Trace.TraceError(...); }` (T-10). Le `catch { lblCoutEstime.Text = ""; }` dans le calcul de coût remplacé par logging + `"Coût non disponible"` en rouge (T-11). `using System.Diagnostics` ajouté aux deux fichiers.

#### [2026-04-21] FEAT — Palette centralisée AppColors + migration 5 fichiers (T-12, T-13)
**Fichiers :** `Forms/AppColors.cs` (nouveau), `Forms/FrmListeBase.cs`, `Forms/FrmActivites.cs`, `Forms/FrmArtisaStock.cs`, `Forms/FrmVueStock.cs`, `Forms/FrmIngredients.cs`, `CharlesNadejda.csproj`
**Résumé :** Classe statique `AppColors` créée avec palette complète (ChocoBrand, ChocoMed, ChocoAbyss, Creme, CremeWarm, CremeBg, Or, GreyBtn, Border, GreenOk, RedCrit, OrgWarn, VertDispo, OrangeReserv, RougePenur). 4 fichiers migrent leurs constantes privates vers AppColors (T-12). `FrmIngredients` migré de `partial class Form` vers `FrmListeBase<Ingredient>` — Designer.cs supprimé, membres abstraits implémentés, chip panel préservé via `OnLoad` override (T-13).

#### [2026-04-21] REFACTOR — Suppression FrmBomProduction doublon (T-16)
**Fichiers :** `Forms/FrmBomProduction.cs`, `Forms/FrmBomProduction.Designer.cs` (supprimés), `CharlesNadejda.csproj`
**Résumé :** `FrmBomProduction` n'était instancié nulle part (grep confirmé). Fichiers supprimés + entrées .csproj retirées. `FrmBomProductionSimulation` est le formulaire de production unique (US-C01).

#### [2026-04-21] FEAT — Module Catalogue connecté dans FrmPrincipal (T-17)
**Fichiers :** `Navigation/RessourceType.cs`, `Forms/FrmPrincipal.cs`
**Résumé :** Ajout de `Categories`, `Parfums`, `Produits` à l'enum `RessourceType`. Cases correspondantes dans `ShowRessourceScreen()`. Les 3 handlers `menuCatCategories/Parfums/Produits_Click` remplacent `PlaceholderWeb()` par la navigation SFA (SetRessource + _router.Navigate). `PlaceholderWeb()` conservé pour `menuCommandes_Click`.

#### [2026-04-21] UX — Style btnFermer artisanal + hint label production + états vides (T-18, T-19, T-20, T-21)
**Fichiers :** `Forms/FrmBomProductionSimulation.cs`, `Forms/FrmBomFiches.cs`, `Forms/FrmBomFicheEdit.cs`
**Résumé :** T-18 : `btnFermer` styled AppColors.GreyBtn + ChocoBrand + Border dans Load(). T-19 : label `_lblEtatVide` dans `FrmBomFiches`, hint dans `lblResultat` si aucun contexte BOM. T-20 : `btnEnregistrer.Enabled = cboInput.Items.Count > 0` dans `ChargerInputsDisponibles()`. T-21 : label hint sous `btnLancerProduction` géré via `EnabledChanged` event.

---

### SESSION 12 — 2026-04-17
> Agent #3 UI/UX Designer : rework esthétique et ergonomique WinForms — palette chocolat unifiée, DGV artisanaux, boutons sémantiques FlatStyle, protection des suppressions (DefaultButton.Button2), WaitCursor sur opérations longues.

---

#### [2026-04-17] REFACTOR — UI/UX : rework esthétique WinForms complet (Agent #3)

**Fichiers :**
- `Forms/FrmListeBase.cs` — palette CHOCOLAT_FONCE/CREME/OR, DGV styling centralisé (CREME header, alternance, sélection chocolat, BorderStyle=None, GridColor, ColumnHeadersHeight=32, DisabledResizing, AllowUserToResizeRows=false), CellDoubleClick→OnModifier, boutons FlatStyle sémantiques (chocolat/vert/rouge/gris), DefaultButton.Button2 sur confirmation suppression
- `Forms/FrmEditBase.cs` — ErrorProvider.BlinkStyle=NeverBlink, btnEnregistrer CHOCOLAT_FONCE + Cursor=Hand, btnAnnuler ton neutre (220,215,210) + FlatStyle, BackColor=White
- `Forms/FrmLogin.Designer.cs` — refonte complète : pnlHeader CHOCOLAT_FONCE (H=80) avec titre "Charles & Nadejda" (OR) + sous-titre italique, pnlForm White, btnConnexion H=40 FlatStyle, lblErreur rouge, titre fenêtre "Connexion — ArtisaStock"
- `Forms/FrmIngredients.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques avec Cursor=Hand et BorderSize=0
- `Forms/FrmAchats.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques
- `Forms/FrmBomFiches.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques
- `Forms/FrmFournisseurs.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques, BackColor=White, lblTitre ForeColor CHOCOLAT_FONCE
- `Forms/FrmProduits.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques, lblTitre ForeColor CHOCOLAT_FONCE
- `Forms/FrmParfums.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques, BackColor=White, lblTitre ForeColor CHOCOLAT_FONCE
- `Forms/FrmRecettes.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques, BackColor=White, lblTitre ForeColor CHOCOLAT_FONCE
- `Forms/FrmBomNiveaux.Designer.cs` — DGV palette complète + AllowUserToResizeRows=false, 4 boutons FlatStyle sémantiques, BackColor=White, lblTitre ForeColor CHOCOLAT_FONCE
- `Forms/FrmIngredientEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmBomContexteEdit.Designer.cs` — btnEnregistrer Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmBomFicheEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmRecetteEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmBomNiveauEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmProduitEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmParfumEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmFournisseurEdit.Designer.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmAchatEdit.cs` — btnEnregistrer CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand + BorderSize=0, btnAnnuler ton neutre FlatStyle
- `Forms/FrmStockEdit.cs` — btnOK CHOCOLAT_FONCE (était (60,60,60)) + Cursor=Hand, btnAnnuler ton neutre FlatStyle + BorderSize=0
- `Forms/FrmBomProductionSimulation.Designer.cs` — DGV palette complète, lblTitre ForeColor CHOCOLAT_FONCE, BackColor=White
- `Forms/FrmBomProductionSimulation.cs` — WaitCursor + btnSimuler.Enabled=false pendant simulation ; WaitCursor + btnLancerProduction.Enabled=false après confirmation + DefaultButton.Button2
- `Forms/FrmActivites.cs` — DefaultButton.Button2 sur Desactiver() et Supprimer()
- `Forms/FrmPrincipal.cs` — DefaultButton.Button2 sur BtnSupprimerContexte_Click et SupprimerNiveau
- `Forms/FrmAchats.cs` — DefaultButton.Button2 sur btnSupprimer_Click
- `Forms/FrmIngredients.cs` — DefaultButton.Button2 sur btnSupprimer_Click, message explicité
- `Forms/FrmBomFiches.cs` — DefaultButton.Button2 sur btnSupprimer_Click
- `Forms/FrmStocks.cs` — DefaultButton.Button2 sur Supprimer()
- `Forms/FrmBomNiveaux.cs` — DefaultButton.Button2 sur btnSupprimer_Click
- `Forms/FrmFournisseurs.cs` — DefaultButton.Button2 sur btnSupprimer_Click
- `Forms/FrmArtisaStock.cs` — DefaultButton.Button2 sur BtnSupprimerContexte_Click et SupprimerNiveau

**Résumé :** Rework UI/UX complet de l'application WinForms — unification de la palette artisanale chocolat sur tous les formulaires (liste et édition), standardisation DGV avec entête CRÈME et sélection chocolat, remplacement de tous les boutons 3D par FlatStyle.Flat sémantiques (chocolat=ajouter, vert=modifier, rouge=supprimer, gris=fermer), ajout de DefaultButton.Button2 sur toutes les confirmations destructives (protection accidentelle — Nielsen #3), WaitCursor sur les deux opérations longues (simulation + production BOM).

**Selfdoubt appliqué :** Certitude élevée sur les modifications Designer.cs (code direct, vérifié). Hypothèse : FrmCategories, FrmCategorieEdit et les formulaires héritant de FrmListeBase/FrmEditBase bénéficient automatiquement des changements de la classe de base sans modification directe — vérifié par lecture des Designer.cs vides qui confirment l'héritage.

**Alerte session suivante :** FrmBomSimulation.Designer.cs et FrmBomProduction.Designer.cs + FrmBomContextes.Designer.cs existent mais n'ont pas été lus — vérifier s'ils contiennent des contrôles visuels à harmoniser ou s'ils sont vides (héritage).

---

### SESSION 11 — 2026-04-15
> FIX FrmVueStock : colonne "Stock / Activité" affichait "act. 6" → désormais nom réel. FIX coût unitaire tronqué pour valeurs < 0,01 €.

---

#### [2026-04-15] FIX — VueStockGlobalDAL + FrmVueStock : NomActivite manquant + précision coût

**Fichiers :**
- `Models/VueStockGlobal.cs` — ajout propriété `NomActivite` (string, nullable)
- `DAL/VueStockGlobalDAL.cs` — toutes les requêtes `SELECT *` → `SELECT vsg.*, a.nom AS nom_activite` + `LEFT JOIN activites a ON a.id = vsg.id_activite` ; `Map()` : lecture `NomActivite` depuis `nom_activite`
- `Forms/FrmVueStock.cs` — `lieuText` : `l.NomActivite ?? "—"` (au lieu de `"act. " + l.IdActivite`) ; `coutText` : format adaptatif F4 si < 0,01 €, F2 sinon

**Résumé :** La VIEW `vue_stock_global` ne contient pas de colonne `nom_activite`, seulement `id_activite`. Les requêtes DAL jointurent désormais directement sur `activites` pour récupérer le nom. Le coût unitaire s'affichait "0,00 €" pour les valeurs très petites (ex: 0,0047 €/g) — résolu par format adaptatif.

**Erreur corrigée :**
- Symptôme : colonne "Stock / Activité" affiche "act. 6" au lieu du nom de l'activité ; coût = "0,00 €" pour les produits de faible coût unitaire
- Cause 1 : `VueStockGlobal` n'avait pas `NomActivite`, le DAL ne jointurait pas `activites`
- Fix 1 : `LEFT JOIN activites a ON a.id = vsg.id_activite` + propriété + `Map()` + `l.NomActivite ?? "—"` dans FrmVueStock
- Cause 2 : format `F2` arrondit 0.0047 → 0.00
- Fix 2 : `l.CoutUnitaire < 0.01m ? "F4" : "F2"` adaptatif
- Règle retenue : la VIEW n'est qu'une projection de tables — si l'on a besoin d'un label dérivé d'une FK, il faut le JOIN dans le DAL, pas dans le Model

---

### SESSION 10 — 2026-04-15
> Priorité 3 Lot B : FrmVueStock (consultation stock global). FIX : multiplication batches × QuantiteOutput manquante dans BomProductionDAL.

---

#### [2026-04-15] FEAT — FrmVueStock : consultation du stock global (P3 Lot B)

**Fichiers :**
- `Forms/FrmVueStock.cs` — nouveau formulaire lecture seule ; DGV vue_stock_global, chips par activité, coloration par ligne (vert dispo / orange réservé / rouge pénurie), coloration cellule DLC (orange < 7 j, rouge expiré), compteur en bas
- `Forms/FrmPrincipal.cs` — 4ème bouton "📊 Vue stock global" dans RESSOURCES (pnlRessources 120→160px)
- `CharlesNadejda.csproj` — ADD Compile FrmVueStock.cs

**Résumé :** FrmVueStock consomme VueStockGlobalDAL (SESSION 8) pour afficher en un écran les lots ingrédients et produits fabriqués. Le filtre par activité utilise les chips (GetByActivite via activites_stocks M:N pour les lots, id_activite direct pour les bom_stocks). Accessible depuis le rail gauche RESSOURCES.

---

#### [2026-04-15] FEAT — BomCoutDAL : calcul de coût de revient récursif multi-niveaux (règle de 3)

**Fichiers :**
- `Models/RapportCout.cs` — nouveaux modèles `RapportCout` (rapport complet) et `LigneCout` (détail par input, récursif)
- `DAL/BomCoutDAL.cs` — `CalculerCout(idFiche, nBatches)` : calcul récursif sur tous les niveaux BOM ; `GetPrixMoyenIngredient()` : moyenne pondérée lots disponibles (fallback prix_achat_reference) ; conversion UnitConvertisseur systématique
- `Forms/FrmBomProductionSimulation.Designer.cs` — ajout `lblCoutEstime` (label italique vert) sous `lblResultat`, DGV décalé de 12px
- `Forms/FrmBomProductionSimulation.cs` — après simulation réussie : appel `BomCoutDAL.CalculerCout` + affichage `lblCoutEstime` ; `RéinitialiserRésultats` vide `lblCoutEstime`
- `CharlesNadejda.csproj` — ADD Compile RapportCout.cs + BomCoutDAL.cs

**Résumé :** Moteur de calcul de coût de revient BOM. Pour N batches d'une fiche, descend récursivement dans l'arbre jusqu'aux ingrédients N1. Règle de 3 inter-niveaux : `nBatchesSource = qteConsommée / QuantiteOutput(source)`. Prix des ingrédients = moyenne pondérée des lots disponibles (quantité × prix/conditionnement). Résultat : coût total + coût par unité output. Visible dans FrmBomProductionSimulation après toute simulation sans pénurie.

---

#### [2026-04-15] FIX — BomProductionDAL : conversion d'unités manquante pour les inputs de type fiche (inter-niveaux)

**Fichiers :**
- `Forms/UnitConvertisseur.cs` — ajout `cg` (centigram = 0.01g) dans la chaîne masse, documentation complète des chaînes et facteurs
- `DAL/BomProductionDAL.cs` — `VerifierDisponibilite()` + `Simuler()` : convert `qteNecessaire` de `ligne.UniteMesure` vers `ligne.UniteMesureInput` avant comparaison avec `qteDisponible` (bom_stocks) ; `ConsumeStock()` : `restant = UnitConvertisseur.Convertir(aConommer, ligne.UniteMesure, ligne.UniteMesureInput)` dans les deux branches (suppression du ternaire — la conversion s'applique toujours)

**Résumé :** La branche `TypeInput == "fiche"` de BomProductionDAL ne convertissait jamais les unités. bom_stocks stocke la quantité dans l'unité output de la fiche source (ex: kg) ; la ligne de la fiche consommatrice pouvait être en g. 500g vs 20kg → comparaison numérique brute → faux positif pénurie. Fix : utiliser `UnitConvertisseur.Convertir(qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput)` systématiquement dans les 3 méthodes.

**Erreur corrigée :**
- Symptôme : pénurie fantôme — 10 batches de 50g boules bloqués alors que 20kg d'appareil disponibles
- Cause : `qteNecessaire = 500` (g) comparé à `qteDisponible = 20` (kg) sans conversion → 20 < 500 = pénurie
- Fix : Convertir 500g → 0.5kg avant comparaison → 20 ≥ 0.5 = OK
- Règle retenue : pour les fiches de type "fiche", `ligne.UniteMesureInput` = `bf.unite_output` (chargé dans BomFicheLigneDAL). La conversion est TOUJOURS nécessaire sauf si les deux unités sont identiques.

---

#### [2026-04-15] FIX — BomProductionDAL : multiplication batches × QuantiteOutput manquante

**Fichiers :**
- `DAL/BomProductionDAL.cs` — `VerifierDisponibilite` + `Simuler` + `Executer` : `multiplicateur = quantiteCible` (nombre de batches, sans division), `qteProduite = quantiteCible * fiche.QuantiteOutput` stockée dans `bom_productions.quantite_produite` et `bom_stocks.quantite_disponible`, `coutUnitaire = coutTotal / qteProduite`
- `Forms/FrmBomProductionSimulation.Designer.cs` — `lblQuantite.Text` "Quantité" → "Nombre de batches", ajout `lblInfoBatch` (label italique dynamique)
- `Forms/FrmBomProductionSimulation.cs` — `cboFiche_SelectedIndexChanged` : `lblInfoBatch.Text = "1 batch = X [unité]"`, confirmation + succès affichent `nbBatches × QuantiteOutput = qteTotale unité`

**Résumé :** La sémantique de `quantiteCible` était ambiguë — le DAL la traitait comme une quantité finale (divisait par QuantiteOutput pour obtenir un multiplicateur) mais l'utilisateur saisit un nombre de batches. Résultat : 5 batches × 10 kg → stockait 5 kg au lieu de 50 kg. Fix : `multiplicateur = quantiteCible` directement, `qteProduite = batches × QuantiteOutput`.

**Erreur corrigée :**
- Symptôme : bom_stocks.quantite_disponible = nbBatches (ex: 5) au lieu de nbBatches × QuantiteOutput (ex: 50)
- Cause : `multiplicateur = quantiteCible / fiche.QuantiteOutput` interprétait nbBatches comme une quantité finale
- Fix : `multiplicateur = quantiteCible` + `qteProduite = quantiteCible * fiche.QuantiteOutput`
- Règle retenue : tout paramètre de production doit documenter explicitement son unité sémantique (batches vs quantité finale). Les deux sont valides mais ne se calculent pas de la même façon.

---

### SESSION 9 — 2026-04-15
> US-C01 : fusion FrmBomProduction + FrmBomSimulation → FrmBomProductionSimulation. Flux Simuler → résultat coloré → Lancer la production activé si 0 pénuries.

---

#### [2026-04-15] REFACTOR — Activités en liste verticale dans le rail gauche

**Fichiers :**
- `Forms/FrmPrincipal.cs` — suppression FlowLayoutPanel bandeau, ajout _lstActivites (ListBox custom-draw), section ACTIVITÉS dans BuildLeftPanel entre RESSOURCES et CONTEXTES, LstActivites_DrawItem, LstActivites_SelectedIndexChanged, ChargerActivites() (ex-ChargerBoutonsActivites), MettreAJourBoutonsActivite supprimée

**Résumé :** Les activités quittent le bandeau horizontal pour rejoindre le rail gauche sous forme de liste verticale, cohérente visuellement avec la liste des contextes (même custom-draw OR + barre gauche). Bandeau réduit à ArtisaStock + ⚙. Hiérarchie visuelle complète : RESSOURCES → ACTIVITÉS → CONTEXTES → (droite) NIVEAUX.

---

#### [2026-04-15] FEAT — US-UX01/02/03 : refonte navigation principale FrmPrincipal

**Fichiers :**
- `Forms/FrmPrincipal.cs` — BuildHub (retrait 📦 bandeau, ajout [+ Activité]), BuildLeftPanel (refonte rail gauche en 2 sections : RESSOURCES + PRODUCTION), AfficherPanneauVide (onboarding mis à jour)

**Résumé :** Séparation configuration/opérationnel. Le rail gauche expose désormais Stocks, Ingrédients et Fournisseurs de manière permanente et indépendante de toute activité. La section Contextes (Production) reste en-dessous. Le bouton [+ Activité] est intégré dans le bandeau pour créer une activité sans passer par ⚙. L'onboarding pointe vers le workflow correct : ressources d'abord, activités ensuite.

---

#### [2026-04-15] REFACTOR — US-C02 : UnitConvertisseur déplacé hors du namespace Forms

**Fichiers :**
- `Forms/UnitConvertisseur.cs` — namespace `CharlesNadejda.Forms` → `CharlesNadejda`
- `DAL/BomProductionDAL.cs` — suppression `using CharlesNadejda.Forms` (dépendance DAL→Forms éliminée)
- `Forms/FrmBomFicheEdit.cs` — ajout `using CharlesNadejda`

**Résumé :** UnitConvertisseur est une utilitaire pure (pas de WinForms), elle doit être accessible depuis le DAL sans créer de dépendance DAL→Forms. Déplacement de namespace corrige la violation de Clean Architecture. La conversion était déjà fonctionnelle dans VerifierDisponibilite, Simuler et ConsumeStock — US-C02 était uniquement ce correctif structurel.

---

#### [2026-04-15] REFACTOR — US-C01 : mise à jour call sites vers FrmBomProductionSimulation

**Fichiers :**
- `Forms/FrmArtisaStock.cs` — OuvrirProduction + OuvrirSimulation pointent vers FrmBomProductionSimulation
- `Forms/FrmPrincipal.cs` — bouton "Produire" pointe vers FrmBomProductionSimulation ; bouton "Simuler" supprimé (redondant)
- `DAL/BomFicheDAL.cs` — commentaire mis à jour
- `DAL/BomProductionDAL.cs` — commentaire mis à jour

**Résumé :** Tous les appels vers FrmBomProduction et FrmBomSimulation migrés vers FrmBomProductionSimulation. Le bouton "Simuler" de FrmPrincipal supprimé car désormais inclus dans le flux "Produire".

---

#### [2026-04-15] FEAT — US-C01 : FrmBomProductionSimulation (fusion Simulation + Production)

**Fichiers :**
- `Forms/FrmBomProductionSimulation.cs` — nouveau formulaire unifié
- `Forms/FrmBomProductionSimulation.Designer.cs` — layout 780×680px
- `CharlesNadejda.csproj` — ADD Compile FrmBomProductionSimulation.cs + Designer.cs

**Résumé :** Fusion de FrmBomProduction et FrmBomSimulation en un formulaire unique. Flux : Simuler → grille complète colorée vert/rouge → btnLancerProduction activé uniquement si 0 pénuries → confirmation → BomProductionDAL.Executer(). Règles conservées : Ordre > 1 pour les niveaux, BomFicheDAL.GetByNiveau() pour les fiches, constructeur avec pré-sélection (BomContexte, BomNiveau), notes préservées.

---

### SESSION 8 — 2026-04-15
> Brainstorm architecture stock unifié → décision Option B + VIEW. Migration v11 : discriminants directs sur bom_stocks + vue_stock_global. Nouveaux VueStockGlobal.cs + VueStockGlobalDAL.cs.

---

#### [2026-04-15] FEAT — Migration v11 : discriminants bom_stocks + VIEW vue_stock_global

**Fichiers :**
- `sql/migration_v11_stock_discriminants.sql` — ALTER bom_stocks (ADD id_contexte, id_activite + FK), CREATE VIEW vue_stock_global
- `Models/BomStock.cs` — ADD IdContexte, IdActivite, NomActivite
- `DAL/BomStockDAL.cs` — GetByNiveau() : SELECT + JOIN activites mis à jour, Map() : 3 champs ajoutés
- `DAL/BomProductionDAL.cs` — Executer() étape 4 : INSERT bom_stocks inclut @idCtx + @idAct depuis niveau
- `Models/VueStockGlobal.cs` — nouveau modèle lecture seule (lots + produits fabriqués unifiés)
- `DAL/VueStockGlobalDAL.cs` — nouveau DAL : GetAll / GetByActivite / GetByContexte / GetByNiveau
- `CharlesNadejda.csproj` — ADD Compile VueStockGlobal.cs + VueStockGlobalDAL.cs

**Résumé :** Implémentation de l'architecture stock Option B + VIEW décidée en brainstorm. bom_stocks porte désormais id_contexte et id_activite directement (dénormalisé, déduit automatiquement depuis le niveau à la production). vue_stock_global unifie les deux sources pour l'affichage sans casser l'intégrité des DAL dédiés.

---

### SESSION 7 — 2026-04-14
> Refactor architecture stocks : découplage ingrédients ↔ activités via table `stocks` indépendante et jonction M:N `activites_stocks`. Chip filter dans FrmIngredients. Nouveaux formulaires FrmStocks, FrmStockEdit, FrmActiviteStocks.

---

#### [2026-04-14] FEAT — Architecture stocks indépendants (migration v10)
**Fichiers :**
- `sql/migration_v10_stocks.sql` — CREATE stocks, CREATE activites_stocks (M:N), DROP FK + colonne id_activite sur fiches_ingredients, ADD id_stock + FK vers stocks
- `Models/Stock.cs` — nouveau modèle (Id, Nom, Description, Actif, DateCreation)
- `Models/Ingredient.cs` — `IdActivite`/`ActiviteNom` → `IdStock`/`StockNom`
- `DAL/StockDAL.cs` — CRUD complet + `GetByActivite()` + `LierActivite()` / `DelierActivite()`
- `DAL/IngredientDAL.cs` — signature `GetAll(int idStock=0, int idActivite=0)`, JOIN stocks, filtres dual-mode (par stock direct OU par activité via sous-requête)
- `DAL/LotDAL.cs` — filtre `fi.id_activite = @idActivite` → `fi.id_stock IN (SELECT id_stock FROM activites_stocks WHERE id_activite = @idActivite)`

**Résumé :** Les ingrédients sont maintenant liés à un stock physique indépendant, pas à une activité. Une activité peut pointer vers N stocks (M:N). Donne de la liberté d'organisation sans contrainte rigide.

---

#### [2026-04-14] FEAT — FrmStocks + FrmStockEdit (CRUD stocks)
**Fichiers :**
- `Forms/FrmStocks.cs` — DGV + boutons Nouveau / Modifier / Supprimer (pattern FrmActivites)
- `Forms/FrmStockEdit.cs` — formulaire Nom + Description, valide unicité via `StockDAL.NomExiste()`
- `CharlesNadejda.csproj` — ajout `<Compile>` pour FrmStocks, FrmStockEdit, FrmActiviteStocks, Models\Stock, DAL\StockDAL

**Résumé :** CRUD complet pour les stocks physiques/logiques. La suppression est bloquée si des ingrédients y sont rattachés (guard dans StockDAL.Delete).

---

#### [2026-04-14] FEAT — FrmActiviteStocks (liaison M:N activité ↔ stocks)
**Fichiers :**
- `Forms/FrmActiviteStocks.cs` — nouveau dialog : CheckedListBox de tous les stocks, cochés = liés à l'activité. Enregistrement synchronise via LierActivite/DelierActivite.
- `Forms/FrmActivites.cs` — 5ème bouton "📦 Stocks liés" (form élargie 560→620px, boutons 120→104px), méthode `GererStocks()` ouvre FrmActiviteStocks

**Résumé :** L'utilisateur peut lier/délier des stocks à une activité depuis FrmActivites. Interface CheckedListBox — une case = un stock, coche = lié.

---

#### [2026-04-14] FEAT — FrmIngredients : chip filter par stock
**Fichiers :**
- `Forms/FrmIngredients.cs` — refactor complet : chip "Tous" + chips par stock (via `StockDAL.GetByActivite`), filtre dynamique par `_stockFiltre`, colonne `ActiviteNom`→`StockNom`, appel DAL avec paramètre nommé `idActivite:`
- `Forms/FrmIngredientEdit.cs` — constructeur `Activite activiteDefaut` → `Stock stockDefaut`, champs `IdStock`/`StockNom`
- `Forms/FrmIngredientEdit.Designer.cs` — `cmbActivite` → `cmbStock` (déclaration, Field label "Activité *"→"Stock *", DropDownStyle, champ déclaré)

**Résumé :** FrmIngredients affiche une barre de chips "Tous / Stock A / Stock B…" au-dessus du DGV. Le clic sur un chip filtre la liste. "Ajouter" pré-sélectionne le stock du chip actif.

---

#### [2026-04-14] FIX — Paramètres positionnels IngredientDAL.GetAll (3 call sites)
**Fichiers :**
- `Forms/FrmAchatEdit.cs` — `GetAll(_idActivite)` → `GetAll(idActivite: _idActivite)`
- `Forms/FrmBomFicheEdit.cs` — `GetAll(_niveau.IdActivite)` → `GetAll(idActivite: _niveau.IdActivite)`

**Résumé :** Après le refactor de signature `GetAll(int idStock=0, int idActivite=0)`, les appels positionnels passaient l'idActivite dans le paramètre idStock. Correction par paramètre nommé explicite.

**Erreur corrigée :**
- Symptôme : listes d'ingrédients vides dans FrmAchatEdit et FrmBomFicheEdit (filtre appliqué sur idStock au lieu d'idActivite)
- Cause : changement de signature de `GetAll(int idActivite=0)` vers `GetAll(int idStock=0, int idActivite=0)` — l'ancien paramètre positif devient le mauvais paramètre
- Fix : utiliser le paramètre nommé `idActivite:` dans tous les call sites
- Règle retenue : après tout changement de signature avec paramètres optionnels, grepper TOUS les call sites et vérifier les appels positionnels

---

#### [2026-04-14] FEAT — FrmPrincipal : bouton 📦 stocks + onboarding 4 étapes
**Fichiers :**
- `Forms/FrmPrincipal.cs` — ajout bouton "📦" dans le bandeau (x = btnGerer - 38px, ouvre FrmStocks), texte onboarding mis à jour en 4 étapes (stocks → activité → lier stocks → contexte+niveaux), hauteur panel onboarding 220→260px

**Résumé :** Le bouton 📦 est fixé à droite du bandeau, juste à gauche de ⚙, position recalculée au Resize. L'onboarding reflète le nouveau workflow stock-first.

---

### SESSION 6 — 2026-04-14
> Corrections de compilation post-migration v8 : namespace Forms dans DAL, conflit de variable lambda `e`.

#### [2026-04-14] FIX — FrmIngredientEdit : conflit variable lambda `e`
**Fichiers :**
- `Forms/FrmIngredientEdit.cs` — `(s, e)` → `(s, ev)` sur le lambda `cmbUnite.SelectedIndexChanged`

**Résumé :** Le lambda `(s, e)` à l'intérieur de `FrmIngredientEdit_Load(object sender, EventArgs e)` déclarait un paramètre `e` déjà utilisé dans la portée englobante — CS0136. Renommage en `ev`.

**Erreur corrigée :**
- Symptôme : CS0136 "Impossible de déclarer une variable locale ou un paramètre nommé 'e' dans cette portée"
- Cause : le paramètre lambda `e` d'un `SelectedIndexChanged` masquait le `EventArgs e` de la méthode `_Load` englobante
- Fix : renommer le paramètre lambda en `ev`
- Règle retenue : dans tout gestionnaire d'événement `(object sender, EventArgs e)`, les lambdas internes doivent utiliser `ev` (ou `_`) à la place de `e`

---

#### [2026-04-14] FIX — BomProductionDAL : namespace Forms manquant
**Fichiers :**
- `DAL/BomProductionDAL.cs` — ajout `using CharlesNadejda.Forms;`, suppression du préfixe `Forms.` sur les 3 appels `UnitConvertisseur`

**Résumé :** BomProductionDAL référençait `Forms.UnitConvertisseur` sans `using` explicite — résolution de nom ambiguë selon compilateur. Ajout du `using` et simplification des appels.

---

### SESSION 5 — 2026-04-13
> Refactor majeur : activités dynamiques. Suppression des ENUM codés en dur. Architecture ERP générique. Mise à jour CLAUDE.md ergonomie UI. Correction erreurs de compilation post-refactor.

---

#### [2026-04-13] FEAT — Système de conditionnement universel (migration v8)
**Fichiers :**
- `sql/migration_v8_conditionnement.sql` — ADD conditionnement_label + qte_par_conditionnement sur fiches_ingredients, ADD nb_conditionnements sur lots_ingredients
- `Forms/UnitConvertisseur.cs` — ajout `UniteBase()` + `VersUniteBase()`
- `Models/Ingredient.cs` — ajout ConditionnementLabel, QteParConditionnement, PrixParUniteBase; UniteMesure restreint à 'g'/'ml'/'piece'
- `Models/Lot.cs` — ajout NbConditionnements, ConditionnementLabel, QteParConditionnement (jointure)
- `DAL/IngredientDAL.cs` — SELECT + Bind + Map des nouvelles colonnes
- `DAL/LotDAL.cs` — SELECT + INSERT + UPDATE + Bind + Map; QuantiteInitiale = nb × qte_par_cond
- `DAL/BomFicheLigneDAL.cs` — prix_ref = prix_achat_reference / qte_par_conditionnement (€/unité base)
- `DAL/BomStockDAL.cs` — GetLotsDispoFIFO retourne prix_unitaire_base = prix_unit / qte_par_cond
- `DAL/BomProductionDAL.cs` — VerifierDisponibilite + Simuler + ConsumeStock : conversion UnitConvertisseur avant comparaison/déduction stock
- `Forms/FrmIngredientEdit.Designer.cs` — nouvelle section Conditionnement obligatoire (label + qté + unité dynamique); unités de base uniquement (g/ml/piece)
- `Forms/FrmIngredientEdit.cs` — logique conditionnement + validation + MajLabelUniteQteCond()
- `Forms/FrmAchatEdit.cs` — input = nb conditionnements; live preview "× Xg = Yg en stock"; prix = €/cond; QuantiteInitiale = nb × qte_par_cond

**Résumé :** Le conditionnement EST l'identité de l'ingrédient. Deux tailles de sac = deux ingrédients distincts. Stock toujours en unité de base. Conversion automatique à l'achat et à la consommation BOM.

---

#### [2026-04-13] FIX — Suppression multi-statement SQL dans tous les DAL
**Fichiers :** `DAL/BomContexteDAL.cs`, `DAL/BomNiveauDAL.cs`, `DAL/ActiviteDAL.cs`, `DAL/BomFicheDAL.cs`, `DAL/BomReservationDAL.cs`, `DAL/BomProductionDAL.cs`, `DAL/RecetteDAL.cs`
- Remplacement du pattern `INSERT...; SELECT LAST_INSERT_ID();` + `ExecuteScalar()` par `ExecuteNonQuery()` + `cmd.LastInsertedId` dans les 7 méthodes Insert concernées

**Résumé :** Le connecteur MySQL.NET ne garantit pas l'exécution de multi-statements sans `AllowBatch=True` dans la connection string. `MySqlCommand.LastInsertedId` est la propriété officielle — aucune seconde instruction SQL nécessaire.

---

#### [2026-04-13] FIX — FrmPrincipal.BuildLeftPanel() DockStyle ordering
**Fichiers :** `Forms/FrmPrincipal.cs`
- Reordonné les 3 `_split.Panel1.Controls.Add()` : `_lstContextes` (Fill) en index 0, `pnlBas` (Bottom) en index 1, `pnlHeader` (Top) en index 2
- Supprimé `Controls.Add(pnlHeader)` inline juste après sa construction — il est maintenant ajouté en dernier
- Même bug et même fix que FrmActivites (règle #11)

**Résumé :** Le premier contexte créé était masqué sous le header de 40px car ListBox Fill démarrait à y=0. Correction de l'ordre d'ajout dans Controls — règle WinForms DockStyle (inverse de l'index).

---

#### [2026-04-13] FIX — FrmArtisaStock, FrmBomContextes, FrmBomProduction (round 2)
**Fichiers :** `Forms/FrmArtisaStock.cs`, `Forms/FrmBomContextes.cs`, `Forms/FrmBomProduction.cs`
- `FrmArtisaStock` : `string _activite` → `Activite _activite`, titre dynamique, `GetAll(Id)`, `ctx.Activite` → `new Activite { Id=ctx.IdActivite, Nom=ctx.ActiviteNom }`, `FrmBomSimulation(ctx.IdActivite)`
- `FrmBomContextes` : `string _activite` → `Activite _activite`, titre dynamique, `GetAll(Id)`, colonne `Activite` → `ActiviteNom`
- `FrmBomProduction` : `string _activite` → `int _idActivite`, `contexte.Activite` → `contexte.IdActivite`, `GetAll(_idActivite)`
- `FrmBomFicheEdit` : `_niveau.Activite` → `_niveau.IdActivite`, suppression `_fiche.Activite = ...`
- `FrmAchats` : bouton Modifier passait encore `_activite?.Nom` → `_activite?.Id ?? 0`

**Résumé :** 5 formulaires supplémentaires avaient été oubliés lors du refactor v7 — tous les `.Activite` (propriété supprimée) et `GetAll(string)` (signature obsolète) sont éliminés.

**Erreur corrigée :**
- Symptôme : CS1061 "ne contient pas de définition pour 'Activite'" + CS1503 "conversion impossible de 'string' en 'int'"
- Cause : Refactor v7 incomplet — seuls les call sites dans FrmPrincipal avaient été mis à jour
- Fix : grep systématique sur `.Activite` + `string _activite` + `GetAll(_activite)` dans tous les .cs
- Règle retenue : après refactor de champ/signature, toujours lancer un grep global AVANT de considérer la tâche terminée

#### [2026-04-13] FIX — FrmBomSimulation, FrmAchatEdit, FrmAchats
**Fichiers :** `Forms/FrmBomSimulation.cs`, `Forms/FrmAchatEdit.cs`, `Forms/FrmAchats.cs`
- `FrmBomSimulation` : `string activite` → `int idActivite`, `BomContexteDAL.GetAll(_activite)` → `GetAll(_idActivite)`, `BomFicheDAL.GetAll(ctx.Activite)` → `GetAll(ctx.IdActivite)`
- `FrmAchatEdit` : `string activite` → `int idActivite`, `IngredientDAL.GetAll(_activite)` → `GetAll(_idActivite)`
- `FrmAchats` : `new FrmAchatEdit(null, _activite?.Nom)` → `new FrmAchatEdit(null, _activite?.Id ?? 0)`

**Résumé :** Ces 3 formulaires n'avaient pas été mis à jour lors du refactor v7 — ils utilisaient encore les signatures `string activite` obsolètes, causant des erreurs de compilation (type mismatch int/string).

**Erreur corrigée :**
- Symptôme : CS1503 — ne peut pas convertir `int` en `string` (FrmBomSimulation) et `string` en `int` (FrmAchatEdit)
- Cause : FrmPrincipal mis à jour pour passer `_activite.Id` (int) mais les constructeurs cibles n'avaient pas été refactorisés
- Fix : alignement des signatures sur `int idActivite` dans les 3 fichiers
- Règle retenue : après tout refactor de signature DAL, vérifier systématiquement tous les call sites via grep avant de considérer la tâche terminée

#### [2026-04-13] REFACTOR — Catalogue web → placeholders
**Fichiers :** `Forms/FrmPrincipal.cs`, `Forms/FrmPrincipal.Designer.cs`
- `menuCatCategories`, `menuCatParfums`, `menuCatProduits`, `menuCommandes` → tous redirigent vers `PlaceholderCatalogueWeb()` (MessageBox informatif)
- Libellés menu mis à jour : `"Catalogue web  [à venir]"`, `"Commandes web  [à venir]"`, sous-items `[placeholder]`
- `menuFournisseurs` conservé fonctionnel (ERP — chaîne achat/lots)
- Tables DB catalogue conservées sans modification (aucune migration destructive)
- Forms FrmCategories, FrmParfums, FrmProduits : fichiers conservés en place mais plus appelés

**Résumé :** Focus ERP — le catalogue web est mis en veille avec placeholders visibles dans le menu. Fournisseurs reste actif (nécessaire aux achats d'ingrédients).

#### [2026-04-13] FIX — Onboarding "aucune activité" + suppression seed hardcodé
**Fichiers :** `Forms/FrmPrincipal.cs`, `sql/migration_v7_activites.sql`
- Suppression du seed "Chocolaterie"/"Pâtisserie" de la migration et de la DB — l'ERP repart de zéro
- `AfficherPanneauVide()` : nouveau panneau d'onboarding visible quand `_activite == null`
  - Titre "Bienvenue dans ArtisaStock", guide 3 étapes, bouton CTA "Créer ma première activité" (OR + Bold, Fitts)
  - Bouton ouvre `FrmActivites` puis recharge `ChargerBoutonsActivites()` automatiquement
  - Cas `_activite != null` mais pas de contexte : comportement inchangé

**Résumé :** Les activités étaient seedées automatiquement, rendant le flux d'onboarding invisible. La DB est maintenant vide au départ — l'utilisateur crée ses propres activités via l'interface.

#### [2026-04-13] REFACTOR — sql/migration_v7_activites.sql
**Fichiers :** `sql/migration_v7_activites.sql`
- CREATE TABLE `activites` (id, nom, description, actif, date_creation)
- ALTER `fiches_ingredients` : DROP ENUM activite, ADD id_activite FK
- ALTER `bom_contextes` : DROP ENUM activite, ADD id_activite FK
- ALTER `bom_fiches` : DROP ENUM activite (scope implicite via niveau→contexte→activite)
- Seed : Chocolaterie, Pâtisserie

**Résumé :** Migration de l'architecture mono-activité codée en dur vers un système ERP générique où l'utilisateur crée ses propres activités (Glacier, Cocktails, etc.).

#### [2026-04-13] FEAT — Models/Activite.cs (nouveau)
**Fichiers :** `Models/Activite.cs`
- Model : Id, Nom, Description, Actif, DateCreation

#### [2026-04-13] REFACTOR — Models : BomContexte, Ingredient, BomFiche, BomNiveau
**Fichiers :** `Models/BomContexte.cs`, `Models/Ingredient.cs`, `Models/BomFiche.cs`, `Models/BomNiveau.cs`
- `string Activite` → `int IdActivite` + `string ActiviteNom` (jointure)
- `BomFiche` : suppression de `Activite` (scope implicite), ajout `IdActivite` + `ActiviteNom` via JOIN chain

#### [2026-04-13] FEAT — DAL/ActiviteDAL.cs (nouveau)
**Fichiers :** `DAL/ActiviteDAL.cs`
- GetAll, GetById, NomExiste, Insert, Update, Desactiver (soft delete avec vérification des contextes et ingrédients actifs)

#### [2026-04-13] REFACTOR — DAL : BomContexteDAL, IngredientDAL, LotDAL, BomFicheDAL, BomNiveauDAL
**Fichiers :** 5 fichiers DAL
- Tous les filtres `activite = 'chocolaterie'` → `id_activite = @idActivite`
- Suppression de la logique `IN ('X','partage')` (le partage n'existe plus)
- Signatures : `GetAll(string activite)` → `GetAll(int idActivite = 0)` (0 = tous)
- JOIN étendu à la table `activites` pour exposer `nom_activite`

#### [2026-04-13] FEAT — Forms/FrmActivites.cs + FrmActiviteEdit.cs (nouveaux)
**Fichiers :** `Forms/FrmActivites.cs`, `Forms/FrmActiviteEdit.cs`
- CRUD complet des activités
- FrmActivites : DGV + boutons Nouveau/Modifier/Désactiver
- FrmActiviteEdit : formulaire création/modification (Nom + Description)
- Pattern non-partial, UI programmatique (même pattern que FrmAchatEdit)

#### [2026-04-13] REFACTOR — Forms/FrmPrincipal.cs (majeur)
**Fichiers :** `Forms/FrmPrincipal.cs`
- Suppression `_btnChoc` et `_btnPat` (hardcodés)
- Ajout `_flowBoutonsActivite` (FlowLayoutPanel) : boutons générés dynamiquement depuis `ActiviteDAL.GetAll()`
- `_activite` passe de `string` à objet `Activite`
- Bouton ⚙ fixé à droite du bandeau → ouvre `FrmActivites` (Fitts : position constante)
- Gestion état vide si aucune activité en DB
- `FrmIngredients`, `FrmAchats` reçoivent un objet `Activite` (plus une string)
- `FrmBomSimulation` reçoit `int idActivite` (plus une string)

#### [2026-04-13] REFACTOR — Forms : FrmBomContexteEdit, FrmIngredients, FrmIngredientEdit, FrmAchats, FrmBomFicheEdit
**Fichiers :** 5 formulaires
- `FrmBomContexteEdit` : `cboActivite` chargé depuis `ActiviteDAL.GetAll()` ; paramètre `string activiteForce` → `Activite activiteForce`
- `FrmIngredients` / `FrmAchats` : paramètre `string activite` → `Activite activite`
- `FrmIngredientEdit` : `cmbActivite` chargé depuis `ActiviteDAL.GetAll()` ; `IdActivite` au lieu de string
- `FrmBomFicheEdit` : `niveau.Activite` → `niveau.ActiviteNom`

#### [2026-04-13] CONFIG — CharlesNadejda.csproj
**Fichiers :** `CharlesNadejda.csproj`
- Ajout des 4 nouvelles entrées `<Compile>` : FrmActivites, FrmActiviteEdit, Activite (model), ActiviteDAL

#### [2026-04-13] DOCS — ~/.claude/CLAUDE.md
**Fichiers :** `~/.claude/CLAUDE.md`
- Ajout section "Ergonomie UI — Standards & Références" : lois cognitives (Fitts, Hick, Miller, Tesler, Jakob), nombre d'or φ, grille 8px, règle des tiers, Gestalt, 10 heuristiques de Nielsen, ergonomie métier ERP, standards WinForms

---

### SESSION 4 — 2026-04-09
> Lot A — Module Achats : CRUD complet, TVA HTVA/TVAC, unité imposée. UnitConvertisseur. FrmBomFicheEdit : unités compatibles + verrouillage pièce.

#### [2026-04-09] FEAT — sql/migration_v6_lots_tva.sql
**Fichiers :** `sql/migration_v6_lots_tva.sql`
- Ajout colonne `tva_pct DECIMAL(5,2) DEFAULT 0` à `lots_ingredients`
- Commentaire : prix_unitaire reste en HTVA — TVA stockée séparément

#### [2026-04-09] FEAT — Models/Lot.cs
**Fichiers :** `Models/Lot.cs`
- Ajout propriété `TvaPct decimal`
- Commentaires mis à jour : `PrixUnitaire` = HTVA, `PrixAchatReel` = total HTVA

#### [2026-04-09] FEAT — DAL/LotDAL.cs (Update + Delete + tva_pct)
**Fichiers :** `DAL/LotDAL.cs`
- Ajout `Update(Lot)` : UPDATE avec recalcul automatique de `quantite_disponible` via `GREATEST(0, @qteInit - (quantite_initiale - quantite_disponible))`
- Ajout `Delete(int id)`
- `Insert`, `Map`, `GetAll` : intègrent `tva_pct`
- Extraction helper `Bind()` pour éviter duplication INSERT/UPDATE

#### [2026-04-09] FIX+FEAT — Forms/FrmAchats.* (CRUD complet + Sizable)
**Fichiers :** `Forms/FrmAchats.Designer.cs`, `Forms/FrmAchats.cs`
- Designer : form `Sizable` + `MinimumSize`, DGV ancré aux 4 bords, `btnModifier` + `btnSupprimer`, renommage "Enregistrer achat" → "Nouvel achat"
- .cs : handlers `btnModifier_Click` (→ FrmAchatEdit edit mode) + `btnSupprimer_Click` (confirmation + LotDAL.Delete)
- Colonne TvaPct masquée dans la liste ; colonnes Prix libellées HTVA

#### [2026-04-09] FEAT — Forms/FrmAchatEdit.cs (refonte complète)
**Fichiers :** `Forms/FrmAchatEdit.cs`, `Forms/FrmAchatEdit.Designer.cs` (vidé)
- Classe non-partial, UI inline dans le constructeur (même pattern que FrmCategorieEdit)
- Constructeur : `FrmAchatEdit(Lot lot, string activite)` — lot=null → création, lot fourni → édition
- En édition : ingrédient affiché en lecture seule (Label) — unité chargée depuis `_lot.UniteMesure`
- En création : ComboBox ingrédient avec `MajUniteIngredient()` sur `SelectedIndexChanged`
- Radio HTVA / TVAC : détermine si `nudPrix` représente le prix HT ou TTC
- `MajPrix()` : recalcule lblPrixHtva, lblPrixTvac, lblTotalHtva, lblTotalTvac en temps réel
- Sauvegarde : stocke toujours `prix_unitaire` = HTVA, `prix_achat_reel` = qty × HTVA

#### [2026-04-09] FEAT — Forms/UnitConvertisseur.cs (nouveau)
**Fichiers :** `Forms/UnitConvertisseur.cs`, `CharlesNadejda.csproj`
- Classe statique : groupes masse (mg/g/kg), volume (ml/cl/dl/l), pièce
- `UnitesCompatibles(string)` : retourne le groupe de l'unité
- `SontCompatibles(string, string)` : vérifie si même groupe
- `Convertir(decimal, string, string)` : conversion via unité de base (g ou ml), exception si incompatibles

#### [2026-04-09] FEAT — Forms/FrmBomFicheEdit.cs (unités compatibles + pièce)
**Fichiers :** `Forms/FrmBomFicheEdit.cs`
- `SynchroniserUniteInput()` reécrit : peuple `cboUniteLigne` avec `UnitConvertisseur.UnitesCompatibles()` (choix libre dans le groupe)
- Pièce : `cboUniteLigne` verrouillé, `nudQteLigne` forcé à 1 et désactivé
- Autres unités : `nudQteLigne` réactivé (min=0, max=99999)
- `cboUniteOutput` : ajout "mg" et "dl"

---

### SESSION 3 — 2026-04-09
> Corrections post-test ArtisaStock : champs manquants ingrédient, formulaire liste redimensionnable, verrouillage unités BOM.

#### [2026-04-09] FIX — DAL/IngredientDAL.cs + Forms/FrmIngredientEdit.*
**Fichiers :**
- `DAL/IngredientDAL.cs` — ajout `type_physique`, `densite` dans SELECT / INSERT / UPDATE / Map / Bind
- `Forms/FrmIngredientEdit.Designer.cs` — ajout contrôles `cmbTypePhysique`, `nudDensite`, `nudPrix`
- `Forms/FrmIngredientEdit.cs` — load/save nouveaux champs, show/hide densité selon type, validation densité obligatoire si liquide/poudre

**Résumé :** Le formulaire de création/modification d'ingrédient ne permettait pas de saisir TypePhysique, Densité ni PrixAchatReference — ces champs existaient dans le Model et la BDD mais n'étaient ni dans le formulaire ni dans les requêtes SQL du DAL.

**Erreur corrigée :**
- Symptôme : à l'ouverture d'un ingrédient existant, TypePhysique et Densité toujours vides/NULL en base
- Cause : `Bind()` et `Map()` omettaient ces colonnes ; le formulaire n'avait pas les contrôles correspondants
- Fix : ajout des colonnes dans toutes les requêtes + nouveaux contrôles avec layout 2 colonnes
- Règle retenue : quand on ajoute un champ au Model, vérifier systématiquement les 4 endroits : SELECT · INSERT · UPDATE · Map()

#### [2026-04-09] FIX — Forms/FrmIngredients.*
**Fichiers :**
- `Forms/FrmIngredients.Designer.cs` — `FormBorderStyle` → `Sizable`, `MinimumSize`, DGV ancré aux 4 bords, boutons ancrés droite
- `Forms/FrmIngredients.cs` — `AutoSizeColumnsMode = AllCells`, `MinimumWidth` par colonne via helper `ConfigCol()`, ajout colonne `TypePhysique`

**Résumé :** Le formulaire était `FixedSingle` (non redimensionnable), les colonnes en mode `Fill` ne s'adaptaient pas au contenu et pouvaient masquer des données en rétrécissant.

**Erreur corrigée :**
- Symptôme : colonnes trop étroites cachant le contenu, impossible d'agrandir le formulaire
- Cause : `FormBorderStyle.FixedSingle` + `AutoSizeColumnsMode.Fill` sans `MinimumWidth`
- Fix : `Sizable` + `AllCells` + `MinimumWidth` sur chaque colonne + Anchor DGV sur les 4 bords
- Règle retenue : tout formulaire liste DGV doit être `Sizable` + `AllCells` + `MinimumWidth`. Ne jamais utiliser `Fill` seul sans plancher.

#### [2026-04-09] FIX — Forms/FrmBomFicheEdit.cs
**Fichiers :**
- `Forms/FrmBomFicheEdit.cs` — `SynchroniserUniteInput()` : vide + remplit `cboUniteLigne` avec l'unité de l'input uniquement, puis `Enabled = false`

**Résumé :** L'unité d'une ligne de fiche BOM était libre (ComboBox avec toutes les unités). Si l'ingrédient Lait était en L mais qu'on saisissait 1000 ml, la comparaison au stock (en L) donnait "1000 > stock_en_L" → fausse pénurie ou fausse disponibilité.

**Erreur corrigée :**
- Symptôme : saisie d'une quantité dans une unité différente de l'ingrédient → calcul de stock erroné à la production
- Cause : `cboUniteLigne` proposait toutes les unités sans contrainte ; `SynchroniserUniteInput()` ne faisait que `SelectedItem = item.Unite` sans verrouiller
- Fix : vider la liste, n'y mettre que l'unité de l'input, désactiver le combo
- Règle retenue : l'unité d'une ligne de fiche BOM est toujours celle de l'ingrédient/fiche source. Toute conversion doit se faire en interne (backlog : masse↔volume via densité).

#### [2026-04-09] FEAT — Forms/FrmListeBase.cs + Forms/FrmEditBase.cs (Étape 1 Option C)
**Fichiers :**
- `Forms/FrmListeBase.cs` — nouvelle classe de base générique pour formulaires liste CRUD
- `Forms/FrmEditBase.cs` — nouvelle classe de base pour formulaires d'édition
- `Forms/FrmCategories.cs` — migré : 100 lignes → 28 lignes
- `Forms/FrmCategories.Designer.cs` — vidé (placeholder vide)
- `Forms/FrmCategorieEdit.cs` — migré : 120 lignes → 65 lignes
- `Forms/FrmCategorieEdit.Designer.cs` — vidé (placeholder vide)
- `CharlesNadejda.csproj` — 2 nouvelles entrées `<Compile>`

**Résumé :** Architecture Option C — les classes de base `FrmListeBase<T>` et `FrmEditBase` centralisent tout le boilerplate DGV/boutons/layout/validation. Les sous-classes n'implémentent que la logique métier propre à leur entité. Démonstration sur Catégories. Étape 2 = migration des entités simples restantes.

**Règle retenue :** Le Designer VS ne peut pas ouvrir les Forms héritant d'une classe générique. Pas bloquant car on écrit le layout en code. Toujours vider le Designer.cs pour éviter les conflits de noms de contrôles.

#### [2026-04-09] DOCS — docs/JOURNAL.md
**Fichiers :**
- `docs/JOURNAL.md` — création du carnet d'historique

**Résumé :** Création du journal de développement suite à l'ajout du skill `/journal` dans le CLAUDE.md global. Rétro-documentation des sessions 1 et 2 depuis les fichiers `sauvegarde*.md`.

---

### SESSION 2 — 2026-03-30
> BOM (Bill of Materials — nomenclature de fabrication) complet : 7 DAL + 8 Forms + câblage FrmPrincipal + migration v5.

#### [2026-03-30] DB — sql/migration_v5_fiches_niveau.sql
**Fichiers :**
- `sql/migration_v5_fiches_niveau.sql` — nouvelle migration appliquée sur Docker

**Résumé :** Ajout de `id_niveau` (FK → `bom_niveaux`) sur `bom_fiches` pour lier chaque fiche recette à un niveau de contexte précis. UNIQUE index modifié de `(nom)` vers `(nom, id_niveau)`.

**Erreur corrigée :**
- Symptôme : impossible de filtrer les fiches par niveau, ni d'imposer que les inputs d'une fiche viennent du niveau N-1
- Cause : `bom_fiches` n'était liée qu'à une `activite` (champ string), pas à un niveau structurel
- Fix : ajout colonne `id_niveau INT NOT NULL` + FK + UNIQUE composite
- Règle retenue : toute entité "recette" dans le BOM doit être liée à un niveau, pas juste à une activité

#### [2026-03-30] FEAT — DAL/BomProductionDAL.cs
**Fichiers :**
- `DAL/BomProductionDAL.cs` — création

**Résumé :** DAL de production BOM avec trois méthodes clés : `VerifierDisponibilite()` (liste des manques), `Simuler()` (toutes lignes avec flag OK/pénurie), `Executer()` (transaction atomique FIFO : vérif → INSERT production → ConsumeStock FIFO → UPDATE coûts → INSERT stock N).

#### [2026-03-30] FEAT — DAL/BomReservationDAL.cs
**Fichiers :**
- `DAL/BomReservationDAL.cs` — création

**Résumé :** Gestion des réservations de stock pour les simulations. Dispo réelle = `quantite_disponible - SUM(reservations actives)`. Méthodes : GetByContexte, Insert, Liberer, LibererToutContexte.

#### [2026-03-30] FEAT — DAL/BomStockDAL.cs
**Fichiers :**
- `DAL/BomStockDAL.cs` — création

**Résumé :** Lecture des stocks de production par niveau. `GetLotsDispoFIFO` ordonne par `date_production ASC` pour la consommation FIFO (First In, First Out — épuiser les lots les plus anciens en premier).

#### [2026-03-30] FEAT — DAL/BomFicheLigneDAL.cs
**Fichiers :**
- `DAL/BomFicheLigneDAL.cs` — création

**Résumé :** Lecture des lignes d'une fiche BOM avec COALESCE sur `fiches_ingredients` + `bom_fiches` pour unifier les deux types d'input (ingrédient brut ou sous-fiche).

#### [2026-03-30] FEAT — DAL/BomFicheDAL.cs
**Fichiers :**
- `DAL/BomFicheDAL.cs` — création

**Résumé :** CRUD fiche BOM. `Insert` utilise une transaction (header + lignes atomiques). `Update` remplace toutes les lignes. `NomExiste` validé par `(nom, id_niveau)` depuis migration v5.

#### [2026-03-30] FEAT — DAL/BomNiveauDAL.cs
**Fichiers :**
- `DAL/BomNiveauDAL.cs` — création

**Résumé :** CRUD niveaux de contexte. `Insert` calcule `ordre = MAX+1` automatiquement. `Delete` lève `InvalidOperationException` si le niveau n'est pas le dernier (`MAX(ordre)` du contexte) — ordre topologique.

#### [2026-03-30] FEAT — DAL/BomContexteDAL.cs
**Fichiers :**
- `DAL/BomContexteDAL.cs` — création

**Résumé :** CRUD contextes de production. `Insert` crée un premier niveau vide par défaut en transaction. Filtrage par `activite` (Chocolaterie / Pâtisserie).

#### [2026-03-30] FEAT — Forms/FrmBomSimulation.*
**Fichiers :**
- `Forms/FrmBomSimulation.cs` — création
- `Forms/FrmBomSimulation.Designer.cs` — création

**Résumé :** Formulaire lecture seule de simulation BOM. DGV avec code couleur (vert = stock OK, rouge = pénurie), légende, appelle `BomProductionDAL.Simuler()`. Pas de bouton "Exécuter" (simulation uniquement).

#### [2026-03-30] FEAT — Forms/FrmBomProduction.*
**Fichiers :**
- `Forms/FrmBomProduction.cs` — création
- `Forms/FrmBomProduction.Designer.cs` — création

**Résumé :** Formulaire d'exécution de production. Sélecteurs en cascade (contexte → niveau → fiche). Bouton "Vérifier" → affiche manques. Bouton "Exécuter" activé seulement si stock OK.

#### [2026-03-30] FEAT — Forms/FrmBomFicheEdit.* + FrmBomFiches.*
**Fichiers :**
- `Forms/FrmBomFicheEdit.cs` / `.Designer.cs` — création
- `Forms/FrmBomFiches.cs` / `.Designer.cs` — création

**Résumé :** Formulaires gestion des fiches BOM. `FrmBomFicheEdit` : formulaire complexe avec cascade `cboTypeInput → cboInput`, gestion dynamique des lignes, classe interne `InputItem`.

#### [2026-03-30] FEAT — Forms/FrmBomNiveauEdit.* + FrmBomNiveaux.*
**Fichiers :**
- `Forms/FrmBomNiveauEdit.cs` / `.Designer.cs` — création
- `Forms/FrmBomNiveaux.cs` / `.Designer.cs` — création

**Résumé :** Formulaires gestion des niveaux d'un contexte BOM. `FrmBomNiveaux` affiche "Niveau 0 = stock ingrédients" en note. Gère `InvalidOperationException` sur Delete (protection ordre topologique).

#### [2026-03-30] FEAT — Forms/FrmBomContexteEdit.* + FrmBomContextes.*
**Fichiers :**
- `Forms/FrmBomContexteEdit.cs` / `.Designer.cs` — création
- `Forms/FrmBomContextes.cs` / `.Designer.cs` — création

**Résumé :** Formulaires gestion des contextes de production BOM. Filtrage par activité. `FrmBomContexteEdit` : validation `NomExiste` avant insertion.

#### [2026-03-30] CONFIG — CharlesNadejda.csproj
**Fichiers :**
- `CharlesNadejda.csproj` — 26 entrées `<Compile>` ajoutées

**Résumé :** Enregistrement manuel de tous les fichiers BOM dans le `.csproj` (format .NET Framework 4.8.1 classique). 7 DAL + 9 Models + 10 Forms (5 paires cs/Designer).

**Erreur corrigée :**
- Symptôme : CS0246 "Le nom de type ou d'espace de noms 'FrmBomProduction' est introuvable"
- Cause : fichiers `.cs` présents sur disque mais absents du `.csproj` — le compilateur ne les voit pas
- Fix : ajout explicite de chaque fichier dans `<Compile Include="..." />`
- Règle retenue : pour tout nouveau fichier `.cs` dans ce projet, ajouter immédiatement l'entrée dans le `.csproj` (règle #1)

#### [2026-03-30] REFACTOR — Forms/FrmPrincipal.*
**Fichiers :**
- `Forms/FrmPrincipal.cs` — câblage menus BOM
- `Forms/FrmPrincipal.Designer.cs` — ajout sous-menus

**Résumé :** Remplacement du menu `menuChocProductions` (AfficherAVenir) par un sous-menu "Production BOM" complet avec 4 entrées (Contextes, Fiches, Exécuter, Simuler) pour Chocolaterie ET Pâtisserie.

---

### SESSION 1 — 2026-03-27 / 2026-03-28
> Architecture BOM validée. Models BOM (9) créés. Infrastructure Docker + Laravel scaffold.

#### [2026-03-28] FEAT — Models/Bom*.cs (×9)
**Fichiers :**
- `Models/BomContexte.cs`, `BomNiveau.cs`, `BomFiche.cs`, `BomFicheLigne.cs`
- `Models/BomProduction.cs`, `BomProductionLigne.cs`, `BomStock.cs`
- `Models/BomReservation.cs`, `BomManque.cs`

**Résumé :** Création des 9 models du module BOM (Bill of Materials — nomenclature de fabrication multi-niveaux). Architecture générique à profondeur variable validée : pas de niveaux codés en dur, tout passe par `contextes_niveaux`.

#### [2026-03-27] DB — sql/migration_v4_bom.sql
**Fichiers :**
- `sql/migration_v4_bom.sql` — appliquée sur Docker
- `sql/create_database.sql` — v4, 50 tables

**Résumé :** Migration v4 : ajout des tables BOM (`bom_contextes`, `bom_niveaux`, `bom_fiches`, `bom_fiches_lignes`, `bom_stocks`, `bom_productions`, `bom_productions_lignes`, `bom_reservations`).

#### [2026-03-27] CONFIG — docker-compose.yml + site-laravel/
**Fichiers :**
- `docker-compose.yml` — configuration MySQL 8 + Laravel + phpMyAdmin
- `site-laravel/` — scaffold Laravel 11 créé

**Résumé :** Infrastructure Docker opérationnelle. phpMyAdmin accessible sur `http://localhost:8080` (root/root). Laravel scaffoldé, phases applicatives (3–8) non démarrées.

---

### SESSION 0 — 2026-03-20 (approx.)
> Initialisation du projet. Phase 0-2 C# complètes.

#### [2026-03-20] FEAT — Forms/ anciens (×11 paires)
**Fichiers :**
- `Forms/FrmLogin.*`, `FrmPrincipal.*`
- `Forms/FrmCategories.*`, `FrmProduits.*`, `FrmParfums.*`, `FrmFournisseurs.*`
- `Forms/FrmIngredients.*`, `FrmRecettes.*`, `FrmAchats.*`

**Résumé :** Phases 0–2 C# : FrmLogin avec BCrypt interopérable PHP↔C#, FrmPrincipal avec menu complet, catalogue chocolaterie (Catégories, Parfums, Produits, Fournisseurs), gestion Ingrédients/Recettes/Achats.

#### [2026-03-20] FEAT — DAL/ anciens (×9) + Models/ anciens (×9)
**Fichiers :**
- `DAL/DbHelper.cs`, `UtilisateurDAL.cs`, `CategorieDAL.cs`, `ProduitDAL.cs`
- `DAL/ParfumDAL.cs`, `FournisseurDAL.cs`, `IngredientDAL.cs`, `RecetteDAL.cs`, `LotDAL.cs`
- `Models/Utilisateur.cs`, `Categorie.cs`, `Produit.cs`, `Parfum.cs`, `Fournisseur.cs`
- `Models/Ingredient.cs`, `Recette.cs`, `RecetteIngredient.cs`, `Lot.cs`

**Résumé :** DAL (Data Access Layer — couche centralisant tous les accès MySQL) et Models initiaux. Toutes les requêtes utilisent `cmd.Parameters.AddWithValue` — jamais de concaténation SQL.
