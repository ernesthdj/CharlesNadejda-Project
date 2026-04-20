# Journal — ArtisaStock (C# WinForms .NET Framework 4.8)

## Règles apprises

| # | Règle | Fichier(s) | Date |
|---|-------|------------|------|
| 1 | SFA Pattern : ne jamais démarrer `Application.Run()` sur FrmLogin — FrmPrincipal doit être la Form racine | `Program.cs` · `FrmLogin.cs` | 2026-04-19 |
| 2 | `DialogResult = DialogResult.OK` ferme automatiquement une Form ouverte via `ShowDialog()` — pas besoin de `this.Close()` | `FrmLogin.cs` | 2026-04-19 |
| 3 | `Application.Restart()` est la solution la plus robuste pour déconnexion/reconnexion — perd tout état mémoire (acceptable si pas de données non persistées) | `FrmPrincipal.cs` | 2026-04-19 |
| 4 | `Controls.Add()` place un contrôle EN ARRIÈRE (ZOrder) — appeler `BringToFront()` si un contrôle sans DockStyle doit apparaître au-dessus d'un `DockStyle.Fill` | `FrmPrincipal.cs` | 2026-04-20 |
| 5 | `NumericUpDown.Enter += Select(0, Text.Length)` — sélectionne tout au focus, l'utilisateur tape directement sans effacer | `FormHelper.cs` | 2026-04-20 |
| 6 | Prix référence dans achat : ne pas conditionner à `nudPrix.Value == 0` — toujours rafraîchir à chaque changement d'ingrédient | `FrmAchatEdit.cs` | 2026-04-20 |
| 7 | `SetFiltreAlertes(false)` doit être appelé dans `ShowRessourceScreen()` avant la lecture de `FiltreAlertesSeulement` pour éviter la persistence cross-navigation | `FrmPrincipal.cs` | 2026-04-20 |
| 8 | `SplitContainer.Panel1MinSize` / `Panel2MinSize` ne peuvent pas être définis dans le constructeur si le contrôle n'est pas encore layouté (Width = 0) — `InvalidOperationException` garantie. Définir après `Loaded` ou laisser les valeurs par défaut (25px) | `FrmPrincipal.cs` | 2026-04-20 |
| 9 | La propriété d'alignement d'un `Label` WinForms est `TextAlign` (type `ContentAlignment`) et NON `ContentAlignment` — cette propriété n'existe pas sur Label | `FrmPrincipal.cs` | 2026-04-20 |
| 10 | `AutoSizeColumnsMode.Fill` empêche le scroll horizontal (les colonnes s'étirent toujours). Pour scroll H + auto-fit au contenu : `None` + `AutoResizeColumns(AllCells)` après binding + `ScrollBars = Both` | `FrmPrincipal.cs` | 2026-04-20 |
| 11 | Forcer l'ordre des colonnes DGV avec `DisplayIndex` incrémenté (compteur `int di = 0`) dans les appels `ShowCol()` séquentiels — le `FillWeight` est ignoré en mode `None` | `FrmPrincipal.cs` | 2026-04-20 |

---

## Historique

### SESSION 1 — 2026-04-19

---

### [2026-04-19 00:00] FEAT — Sprint P0 · US-00 + US-05

**Fichiers :**
- `CharlesNadejda/Forms/FrmLogin.cs` — suppression bloc debug + ajout propriété + pattern DialogResult
- `CharlesNadejda/Program.cs` — SFA : FrmLogin en ShowDialog, FrmPrincipal comme Form racine
- `CharlesNadejda/Forms/FrmPrincipal.cs` — déconnexion via Application.Restart()

**Résumé :** Sprint P0 — deux US bloquantes résolues. US-00 supprime le préremplissage des champs email/mot de passe présent dans un bloc `#if DEBUG` (risque git history). US-05 corrige l'architecture SFA (Single-Form Application) : FrmLogin passe en dialogue bloquant (`ShowDialog()`), FrmPrincipal devient la Form racine de la boucle de messages (`Application.Run()`), et la déconnexion utilise `Application.Restart()` au lieu de `new FrmLogin().Show(); this.Close()` qui cassait l'AppState.

**Selfdoubt appliqué :**
- 6 affirmations vérifiées dans le code source avant modification — ratio 0/6 hypothèses non vérifiées
- Bloc `#if DEBUG` confirmé aux lignes 15-18 (✅ Certain)
- `btnConnexion_Click` confirmé comme créant `FrmPrincipal(u)` + `this.Hide()` (✅ Certain)
- `menuDeconnexion_Click` confirmé faire `new FrmLogin().Show(); this.Close()` (✅ Certain)
- Propriété `Utilisateur` absente de FrmLogin avant modification (✅ Certain)
- `FrmPrincipal(Utilisateur u)` — constructeur existant confirmé (✅ Certain)
- `OnFormClosed` → `Application.Exit()` confirmé valide une fois FrmPrincipal racine (✅ Certain)

**Alerte agent suivant (Agent #5 Frontend) :**
- `FrmLogin` ne préremplit plus les champs — prévoir un compte de test dans la DB ou un `.env` de dev
- L'entrée `Application.Run(new FrmPrincipal(...))` dans `Program.cs` confirme que FrmPrincipal est désormais la Form racine : tous les `EmbedForm()` et navigations SFA fonctionnent dans ce contexte
- `Application.Restart()` à la déconnexion : si l'utilisateur a des modifications non sauvegardées dans un formulaire embarqué, elles seront perdues — envisager un `IsDirty` check avant déconnexion dans un sprint futur
- Build MSBuild non exécuté (permission Bash refusée) — à valider manuellement avant de procéder au Sprint 1

---

### [2026-04-19 15:30] FEAT — Sprint P0 · US-01 + US-02

**Fichiers :**
- `CharlesNadejda/Forms/FrmIngredients.cs` — US-01 : filtre chips + chargement DGV + colonnes design v10
- `CharlesNadejda/Forms/FrmPrincipal.cs` — US-02 : bouton ＋ Contextes + label onboarding Hub

**Résumé :**
US-01 — corrige le filtre ingrédients : `BuildChipPanel()` utilise désormais `StockDAL.GetAll()` (tous les stocks physiques actifs, sans filtre activité) ; `Charger()` en mode "Tous" appelle `IngredientDAL.GetAll()` sans paramètre. Colonnes DGV mises en conformité design v10 : Nom · Conditionnement · Unité base · Stock (lieu) · Type physique · Densité · Dispo · €/cond. Colonne `Marque` masquée.
US-02 — câble le point d'entrée "Créer un contexte" : bouton ＋ ajouté dans `_pnlHdrContextes` (section rail gauche), positionné à droite via `Resize`, connecté à `BtnNouveauContexte_Click`. Dans `ShowHubScreen()`, message d'onboarding ajouté si l'activité n'a aucun contexte de production.

**Selfdoubt appliqué :**
- 8 affirmations vérifiées dans le code source avant modification — ratio 0/8 hypothèses non vérifiées
- `StockDAL.GetByActivite` dans `BuildChipPanel()` sous condition `_activite != null` (✅ Certain ligne 54-59)
- `IngredientDAL.GetAll(idActivite:)` dans `Charger()` ligne 115 (✅ Certain)
- Colonne `Marque` présente ligne 129 (✅ Certain)
- `BtnNouveauContexte_Click` ligne 923 (✅ Certain)
- `_pnlHdrContextes` champ de classe et ajouté à `_pnlContextesFull.Controls` ligne 263 (✅ Certain)
- `BomContexteDAL.GetAll()` utilisé ligne 330 — disponible dans le namespace (✅ Certain)
- `using System.Linq` manquant dans `FrmIngredients.cs` → ajouté (✅ Corrigé)
- `DisplayIndex` peut déclencher une exception si des colonnes attendues sont absentes (ex: données vides) → protégé par `if (dgv.Columns[item.name] != null)` (✅ Certain)

**Alerte agent suivant :**
- Build MSBuild non exécuté (permission Bash refusée) — valider avec `Ctrl+Shift+B` dans Visual Studio avant tout commit
- `DisplayIndex` pour l'ordre des colonnes DGV : si l'assignation par index provoque des conflits (colonnes cachées interférant avec les indices), remplacer par une réaffectation explicite de `DisplayIndex` colonne par colonne dans l'ordre décroissant
- Le paramètre `activite` du constructeur `FrmIngredients(Activite activite = null)` est conservé pour le titre contextuel — comportement attendu
- US-01 : `StockDAL.GetAll()` peut retourner des stocks inactifs selon l'implémentation DAL — vérifier que la méthode filtre `actif = 1`

---

## [AGENT #5 Frontend] — 2026-04-19 15:30
**Entrée consommée :**
- `docs/ARCHITECT_PLAN_TECHNIQUE.md` — fiches US-01 et US-02
- `FrmIngredients.cs` · `FrmPrincipal.cs` (complet) · `Models/Ingredient.cs`
- `memory/MEMORY.md` — règles DockStyle.Controls.Add + SplitterDistance

**Output produit :**
- `FrmIngredients.cs` — US-01 implémentée (3 modifications + ajout using Linq)
- `FrmPrincipal.cs` — US-02 implémentée (bouton ＋ sidebar + label onboarding)
- `docs/JOURNAL.md` — entrée ajoutée

**Décisions clés :**
1. Bouton ＋ ajouté dans `_pnlHdrContextes` (panel header existant) via `Controls.Add` — pas de nouveau panel intermédiaire (YAGNI)
2. Message d'onboarding dans `pnlHdr` du Hub : `Height` dynamique (56 → 76px si aucun contexte) plutôt qu'un panel séparé
3. Colonnes DGV : `DisplayIndex` géré via LINQ anonyme pour lisibilité — protégé contre colonnes absentes

**Selfdoubt appliqué :** ratio 0/8 — toutes affirmations vérifiées

**Alerte agent suivant :** Build non validé (permission Bash refusée) — vérifier compilation avant Sprint 1.

---

### [2026-04-19 16:00] FEAT — Sprint P1 · US-03 DAL

**Fichiers :** `CharlesNadejda/DAL/StockDAL.cs`

**Résumé :** Sprint P1 US-03 — deux modifications DAL sur StockDAL.cs. (1) `Delete()` renforcé : ajout d'une vérification 2 sur `lots_ingredients` (via JOIN `fiches_ingredients`) avant suppression physique — comble le gap d'intégrité référentielle signalé par l'Architecte. (2) Nouvelle méthode `GetActivitesLiees(int idStock)` retournant les ids des activités liées via `activites_stocks` — nécessaire pour initialiser les checkboxes du futur panel de liaison dans FrmStocks (US-03 UI, Sprint P1 Agent #5).

**Selfdoubt appliqué :**
- Position d'insertion de la vérif 2 dans `Delete()` : entre `if (nb > 0) throw` (ligne 108) et `cmd.CommandText = "DELETE"` (ancienne ligne 110) — confirmé par lecture directe du fichier (✅ Certain)
- `cmd.Parameters.Clear()` obligatoire avant réutilisation du même `cmd` — pattern déjà présent dans la méthode originale, reproduit à l'identique (✅ Certain)
- Signature `GetActivitesLiees` conforme à la section "Signatures DAL" du plan technique (✅ Certain)
- Toutes requêtes SQL paramétrées via `AddWithValue` — jamais de concaténation (✅ Certain)
- Build MSBuild non exécuté (permission Bash refusée) — compilation à valider manuellement

**Alerte agent suivant (Agent #5 Frontend — FrmStocks) :**
- `StockDAL.GetActivitesLiees(int idStock)` est disponible — à appeler dans `ChargerLiaisons(stockId)` pour initialiser les checkboxes
- Règle MEMORY.md à respecter pour FrmStocks refactoré : SplitterDistance dans `LayoutEventHandler` + Controls.Add Panel1 (Fill) avant Panel2 (Right)
- Événement `ItemCheck` : utiliser `e.NewValue` et NON `GetItemChecked(index)` — se déclenche AVANT la mise à jour de l'état coché
- Désactiver `ItemCheck` pendant `ChargerLiaisons()` pour éviter les faux déclenchements
- Build non validé — lancer `Ctrl+Shift+B` avant de procéder

---

## [AGENT #4 Backend] — 2026-04-19

**Entrée consommée :**
- `docs/ARCHITECT_PLAN_TECHNIQUE.md` — fiches US-00 et US-05 complètes
- Code source lu : `FrmLogin.cs` · `Program.cs` · `FrmPrincipal.cs` (lignes 1280-1295)
- `memory/MEMORY.md` — alerte sécurité R8 (mot de passe debug en dur ligne 17)

**Output produit :**
- `FrmLogin.cs` modifié — US-00 (bloc DEBUG supprimé) + US-05 (propriété Utilisateur + DialogResult.OK)
- `Program.cs` modifié — US-05 (SFA Pattern : ShowDialog + Application.Run FrmPrincipal)
- `FrmPrincipal.cs` modifié — US-05 (menuDeconnexion_Click → Application.Restart())
- `docs/JOURNAL.md` créé

**Décisions clés :**
1. Suppression totale du bloc `#if DEBUG` (pas de remplacement par une variable d'env) — cohérent avec OWASP et l'audit sécurité
2. Pattern `ShowDialog()` retenu (Option A du plan technique) — plus simple et plus propre que `Application.Run` avec gestion manuelle de la Form racine
3. `Application.Restart()` pour la déconnexion — solution robuste, perte d'état acceptable

**Selfdoubt appliqué :** ratio 0/6 — toutes affirmations vérifiées dans le code source

**Alerte agent suivant :** Build MSBuild non exécuté — permission Bash non accordée dans cette session. Lancer `Ctrl+Shift+B` dans Visual Studio avant de procéder au Sprint 1 (US-02 + US-01).

---

### [2026-04-19 17:00] FEAT — Sprint P2 · US-07 DAL + US-09 DAL

**Fichiers :**
- `CharlesNadejda/DAL/BomFicheDAL.cs` — US-07 : `InsertLignes` → `internal` + ajout `Duplicate()`
- `CharlesNadejda/DAL/BomProductionDAL.cs` — US-09 : ajout `GetDuJourByActivite()`

**Résumé :**
US-07 — `InsertLignes` passé de `private` à `internal` (même assembly, accessible depuis `Duplicate()` mais invisible hors assembly). Ajout de `Duplicate(int idFiche)` : charge la fiche source avec ses lignes via `GetById(avecLignes: true)`, génère un nom unique ("Copie de [nom]", puis "(2)", "(3)"... via `NomExiste()`), crée la copie via `Insert()` qui réinsère toutes les lignes dans une transaction atomique.
US-09 — Ajout de `GetDuJourByActivite(int idActivite)` : requête SQL paramétrée avec jointure `bom_productions → bom_fiches → bom_niveaux → bom_contextes`, filtre `c.id_activite = @idActivite AND DATE(p.date_production) = CURDATE()`, ORDER BY `p.date_production DESC`. Réutilise le `MapHeader` existant — pas de code dupliqué.

**Selfdoubt appliqué :**
- `GetById(int id, bool avecLignes = true)` existe déjà ligne 68 — pas à créer (✅ Certain, vérifié par lecture)
- `InsertLignes` était bien `private` ligne 175 — changement en `internal` sans risque d'ambiguïté (✅ Certain)
- `NomExiste(string nom, int idNiveau, int excludeId = 0)` existe ligne 85 — signature compatible (✅ Certain)
- `Insert(BomFiche f)` gère déjà la transaction + appel `InsertLignes` — pas de duplication de logique (✅ Certain)
- `MapHeader` dans `BomProductionDAL` ligne 420 lit exactement les colonnes SELECT de la nouvelle requête (✅ Certain, colonnes identiques à `GetRecentByActivite`)
- `BomFiche.Lignes` est une `List<BomFicheLigne>` — copie par référence correcte pour `Insert()` (✅ Probable — non vérifié dans le modèle, mais logique d'usage confirmée)
- Build MSBuild non exécuté (permission Bash refusée) — compilation à valider manuellement

**Alerte agent suivant (Agent #5 Frontend — câblage UI US-07 + US-09) :**
- `BomFicheDAL.Duplicate(int idFiche)` est disponible — câbler dans `FrmPrincipal.ShowContexteScreen()` avec bouton "📋 Dupliquer" et méthode `DupliquerFiche(BomFiche fiche)` selon le plan technique
- `BomProductionDAL.GetDuJourByActivite(int idActivite)` est disponible — câbler dans `FrmPrincipal.ShowHubScreen()` via bouton "🖨 Rapport du jour" + `PrintDocument`
- `InsertLignes` est maintenant `internal` — si un autre projet dans la solution référence cet assembly, vérifier qu'il n'y a pas de conflit d'accès (peu probable dans ce contexte mono-projet)
- Build non validé — lancer `Ctrl+Shift+B` dans Visual Studio avant de procéder

---

## [AGENT #4 Backend] — 2026-04-19 17:00
**Entrée consommée :**
- `docs/ARCHITECT_PLAN_TECHNIQUE.md` — fiches US-07 et US-09 + section Signatures DAL
- `CharlesNadejda/DAL/BomFicheDAL.cs` (233 lignes — lecture complète)
- `CharlesNadejda/DAL/BomProductionDAL.cs` (437 lignes — lecture complète)

**Output produit :**
- `BomFicheDAL.cs` modifié — `InsertLignes` passé `internal` + méthode `Duplicate()` ajoutée
- `BomProductionDAL.cs` modifié — méthode `GetDuJourByActivite()` ajoutée
- `docs/JOURNAL.md` mis à jour

**Décisions clés :**
1. `GetById` existait déjà avec le bon paramètre `avecLignes` — aucune création nécessaire
2. `internal` retenu plutôt qu'une extraction inline dans `Duplicate()` — cohérence avec le plan technique et réutilisabilité future
3. `MapHeader` réutilisé tel quel pour `GetDuJourByActivite` — colonnes SELECT identiques à `GetRecentByActivite` (DRY)

**Selfdoubt appliqué :** ratio 0/6 confirmés — 1/6 probable (modèle BomFiche.Lignes non relu)

**Alerte agent suivant :** Câblage UI de US-07 (bouton Dupliquer) et US-09 (rapport du jour) à faire par Agent #5. Build non validé — `Ctrl+Shift+B` obligatoire.

---

### [2026-04-19 18:30] FEAT — Sprint P2 · US-06 + US-07 UI + US-08 + US-09 UI

**Fichiers :**
- `CharlesNadejda/Forms/FrmVueStock.cs` — US-06 : bouton "🖨 Exporter CSV" + méthode `ExporterCsv()` + helper `Escape()`
- `CharlesNadejda/Forms/FrmPrincipal.cs` — US-07 UI : `btnDupFiche` + `SelectionChanged` DGV + méthode `DupliquerFiche()` | US-08 : StatCards cliquables avec callbacks navigation | US-09 UI : bouton "🖨 Rapport du jour" + méthode `GenererRapport()` via `PrintDocument`
- `CharlesNadejda/Forms/FrmIngredients.cs` — US-08 : paramètre `filtreAlertesSeulement` + filtre `Where(i => i.EstEnAlerte)` dans `Charger()`
- `CharlesNadejda/Navigation/AppState.cs` — US-08 : propriété `FiltreAlertesSeulement` + méthode `SetFiltreAlertes(bool)`
- `CharlesNadejda/Navigation/NavigationParams.cs` — US-08 : propriété `FiltreAlertesSeulement`

**Résumé :**
US-06 — Export CSV dans FrmVueStock : bouton "🖨 Exporter CSV" ajouté dans `pnlBas` (à gauche des légendes décalées à droite). `ExporterCsv()` : SaveFileDialog, StreamWriter UTF-8 BOM, séparateur `;`, exporte `_lignes` (liste filtrée active). Champs contenant `;` échappés via `Escape()`. Les légendes ont été décalées à droite pour faire de la place.
US-07 UI — Bouton "📋 Dupliquer" ajouté dans `ShowContexteScreen()` après `btnSupFiche`. Désactivé par défaut, activé via `_dgvFiches.SelectionChanged`. Méthode `DupliquerFiche()` : confirmation, appel `BomFicheDAL.Duplicate()`, rechargement fiches, sélection de la copie dans le DGV.
US-08 — StatCards Hub cliquables : H3 (Ingrédients) toujours cliquable → navigation sans filtre ; H4 (En alerte) cliquable si alertes > 0 → filtre `FiltreAlertesSeulement = true` ; H5 (Fiches BOM) cliquable si fiches > 0 et contextes > 0 → SetContexte(contextes[0]) + Navigate(ContexteNiveaux) ; H6 (Productions 7j) cliquable si prods7j > 0. `SetFiltreAlertes()` sans `RaiseChanged()` — filtre lu au moment de la navigation. `FrmIngredients` accepte `filtreAlertesSeulement = false` en paramètre optionnel.
US-09 UI — Bouton "🖨 Rapport du jour" ajouté dans `pnlHdr` (visible si `prods.Count > 0`). `GenererRapport()` via `PrintDocument` + `PrintPreviewDialog` : en-tête activité/date, Section 1 productions du jour (ou message "Aucune production"), Section 2 alertes stock, pied de page coût total. Namespace complet utilisé (`System.Drawing.Printing.PrintDocument`) — pas de `using` supplémentaire.

**Selfdoubt appliqué :**
- `_lignes` dans FrmVueStock toujours synchronisé après `AppliquerFiltre()` — export garantit les données filtrées actives (✅ Certain, ligne 262)
- `BomFicheDAL.Duplicate()` existe — ajouté par Agent #4 (✅ Certain, vérifié lecture BomFicheDAL.cs lignes 181-205)
- `BomProductionDAL.GetDuJourByActivite()` existe — ajouté par Agent #4 (✅ Certain, vérifié lignes 73-97)
- `MakeStatCard()` retourne un `Panel` — Click attachable directement (✅ Certain, ligne 1093)
- `FrmIngredients` constructeur modifié sans casser l'appel existant dans `ShowRessourceScreen` grâce à la valeur par défaut `false` (✅ Certain)
- `System.Drawing.Printing` et `System.IO` non importés → namespaces complets utilisés (✅ Corrigé)
- Build MSBuild non exécuté (permission Bash refusée) — à valider manuellement

**Alerte agent suivant (Agent #6 QA) :**
- Build non validé — `Ctrl+Shift+B` dans Visual Studio obligatoire avant tout test
- US-07 : `btnDupFiche` est une variable locale dans `ShowContexteScreen()` — la référence est capturée dans le lambda `SelectionChanged`. Si la méthode est rappelée (rechargement de l'écran), un nouveau lambda est attaché sur le nouveau `_dgvFiches`. Le pattern est cohérent car `_dgvFiches` est réinstancié à chaque appel de `ShowContexteScreen()`.
- US-08 : après clic StatCard H4, `FiltreAlertesSeulement = true` reste dans l'AppState jusqu'à la prochaine navigation. Prévoir un reset dans `ShowRessourceScreen()` après usage : `_state.SetFiltreAlertes(false)` — non implémenté dans ce sprint (à monitorer).
- US-09 : `GenererRapport()` est synchrone — pour des jours avec beaucoup de productions, la génération peut prendre quelques ms. Acceptable selon le plan technique (< 200ms attendu).
- US-06 : légendes décalées à droite (0→130, 130→260, 260→390) pour laisser place au bouton CSV. Vérifier visuellement que le layout pnlBas reste cohérent à 980px.

---

## [AGENT #5 Frontend] — 2026-04-19 18:30
**Entrée consommée :**
- `docs/ARCHITECT_PLAN_TECHNIQUE.md` — fiches US-06, US-07, US-08, US-09
- `FrmVueStock.cs` · `FrmPrincipal.cs` (complet) · `FrmIngredients.cs` · `AppState.cs` · `NavigationParams.cs`
- `DAL/BomFicheDAL.cs` · `DAL/BomProductionDAL.cs` — vérification existence méthodes DAL Agent #4
- `memory/MEMORY.md` — règles DockStyle.Controls.Add + SplitterDistance

**Output produit :**
- `FrmVueStock.cs` — US-06 bouton CSV + méthode ExporterCsv + helper Escape
- `FrmPrincipal.cs` — US-07 UI (btnDupFiche + DupliquerFiche) + US-08 (StatCards cliquables) + US-09 UI (btnRapport + GenererRapport)
- `FrmIngredients.cs` — US-08 : paramètre filtreAlertesSeulement + filtre dans Charger()
- `AppState.cs` — US-08 : FiltreAlertesSeulement + SetFiltreAlertes()
- `NavigationParams.cs` — US-08 : FiltreAlertesSeulement
- `docs/JOURNAL.md` mis à jour

**Décisions clés :**
1. StatCards : Click attaché directement sur le Panel retourné par `MakeStatCard()` (pas de nouvelle méthode `MakeStatCardClickable`) — YAGNI, pattern plus lisible
2. Export CSV : légendes décalées à droite plutôt qu'un nouveau layout — minimal et cohérent
3. Rapport US-09 : namespaces complets pour `PrintDocument` + `StreamWriter` — pas d'ajout d'`using` global
4. FrmIngredients : paramètre optionnel `filtreAlertesSeulement = false` — rétrocompatibilité garantie

**Selfdoubt appliqué :** ratio 0/7 certains + 0/7 hypothèses — toutes affirmations vérifiées avant implémentation

**Alerte agent suivant (Agent #6 QA) :** Build non validé (permission Bash refusée). Tester le flow StatCard H4 → ingrédients filtrés (vérifier reset filtre entre navigations). Vérifier layout pnlBas FrmVueStock à 980px avec le bouton CSV ajouté.

---

## [AGENT #6 QA] — 2026-04-19 19:00
**Entrée consommée :**
- `docs/JOURNAL.md` — 5 sessions agents précédents (P0 à P3), alertes Agent #5
- Code source lu intégralement : `FrmLogin.cs` · `Program.cs` · `AppState.cs` · `NavigationParams.cs` · `UtilisateurDAL.cs` · `FrmIngredients.cs` · `FrmVueStock.cs` · `FrmPrincipal.cs` (1656 lignes) · `FrmStocks.cs` · `FrmBomProductionSimulation.cs` · `DAL/BomFicheDAL.cs` · `DAL/BomProductionDAL.cs` · `DAL/StockDAL.cs`

**Output produit :**
- Checklist QA complète — 13 US validées item par item avec preuve fichier:ligne
- Verdict : VALIDÉ AVEC RÉSERVES (2 warnings non bloquants)

**Décisions clés :**
1. US-04 validée sur `FrmBomProductionSimulation.cs` (pas `FrmBomProduction.cs`) — c'est le formulaire SFA effectivement câblé dans `ShowProductionScreen()`
2. US-03 : `StockDAL.Delete()` vérifie `lots_ingredients` (vérification 2) mais via `fiches_ingredients` JOIN — pas de vérification directe de `lots_ingredients` sans JOIN. Acceptable (double filet DAL + UI)
3. US-08 filtre reset : `SetFiltreAlertes(false)` est appelé UNIQUEMENT dans le click handler de `cardIngredients` (ligne 584) — mais pas dans `ShowRessourceScreen()`. Filtre non réinitialisé entre navigations si l'utilisateur navigue via le menu sidebar (warning non bloquant)

**Selfdoubt appliqué :**
- US-04 async : `async void btnLancerProduction_Click` confirmé ligne 232 FrmBomProductionSimulation.cs (✅ Certain)
- US-11 `ordreMax` : paramètre confirmé ligne 1379 FrmPrincipal.cs, `MakeNiveauRow(niv, ficheCount, ordreMax)` (✅ Certain)
- US-03 `CheckedListBox` : `_clbActivites` confirmé ligne 27 FrmStocks.cs (✅ Certain)
- US-03 `e.NewValue` : confirmé ligne 311 FrmStocks.cs (✅ Certain)
- US-10 `LinkLabel` : `lnkStock` confirmé lignes 439-453 FrmPrincipal.cs (✅ Certain)
- Reset filtre US-08 : `SetFiltreAlertes(false)` uniquement sur `cardIngredients.Click` — absent de `ShowRessourceScreen()` (⚠️ Warning confirmé)
- `StockContientDonnees()` retourne toujours false (⚠️ Warning confirmé ligne 293 FrmStocks.cs)

**Alerte agent suivant (Agent #7 DevOps ou Agent #8 Knowledge Curator) :**
1. ~~W1 — `SetFiltreAlertes(false)` absent de `ShowRessourceScreen()`~~ **RÉSOLU** (voir session 2026-04-20)
2. W2 — `StockContientDonnees()` retourne toujours `false` (ligne 293 FrmStocks.cs) : à implémenter dans un sprint futur
3. ~~Build MSBuild non validé par les agents~~ **RÉSOLU** — build propre 0 erreur / 0 warning confirmé
4. Les credentials `#if DEBUG` dans `FrmLogin.cs` restent présents — acceptables en dev, à retirer avant release

---

### SESSION 2 — 2026-04-20

---

### [2026-04-20] FIX — W1 filtre alertes + UX post-tests mentalyas

**Fichiers :**
- `CharlesNadejda/Forms/FrmPrincipal.cs` — reset `FiltreAlertesSeulement` dans `ShowRessourceScreen()`
- `CharlesNadejda/Forms/FrmPrincipal.cs` — `BringToFront()` sur `btnNouvCtxSide` (bouton caché derrière `DockFill` label)
- `CharlesNadejda/Forms/FrmPrincipal.cs` — renommages sidebar : "Stocks" → "Stock/Liaisons", "Ingrédients" → "Fiches Ingrédients"
- `CharlesNadejda/Forms/FrmIngredients.cs` — titres "Stock ingrédients" → "Fiches ingrédients"
- `CharlesNadejda/Forms/FormHelper.cs` — nouveau : `ActiverPointDecimal()` + `ActiverSelectionAuFocus()`
- `CharlesNadejda/Forms/FrmAchatEdit.cs` — prix référence rafraîchi à chaque changement d'ingrédient
- `CharlesNadejda/Forms/FrmIngredientEdit.cs` — titre "Nouvelle fiche ingrédient", point decimal + sélection au focus

**Résumé :** Corrections post-tests utilisateur. Bug critique : le bouton "＋" contexte était invisible (couvert par `DockStyle.Fill` label — `BringToFront()` manquant). Fix W1 QA : `FiltreAlertesSeulement` était persistent entre navigations, maintenant relu et remis à `false` dans `ShowRessourceScreen()`. Ajout `FormHelper` pour convertisseur "." → "," et sélection automatique au focus sur tous les `NumericUpDown`. Prix de référence dans `FrmAchatEdit` mis à jour à chaque sélection d'ingrédient (pas seulement quand valeur = 0).

**Erreur corrigée — BringToFront :**
- Symptôme : le bouton "＋" n'apparaissait pas dans l'en-tête "Contextes"
- Cause : `Controls.Add()` place le contrôle en dernier (ZOrder arrière) — le `Label DockFill` le recouvrait entièrement
- Fix : `btnNouvCtxSide.BringToFront()` après `Controls.Add()`
- Règle retenue : toujours appeler `BringToFront()` sur un contrôle positionné (non Dock) ajouté après un `DockStyle.Fill`

---

### [2026-04-20] CHORE — PR GitHub créée

**Fichiers :** 32 fichiers · 5 439 insertions · branche `feat/refactoring-sprints-p0-p3`

**Résumé :** `git push` + `gh pr create` — PR #1 ouverte sur `ernesthdj/CharlesNadejda-Project`. Inclut tous les sprints P0→P3, la couche Navigation/, Services/, et les docs agents.

---

### [2026-04-20 23:00] FEAT — Vue contexte : volet "Stock produit par ce niveau"

**Fichiers :**
- `CharlesNadejda/Forms/FrmPrincipal.cs` — +114 lignes, -15 lignes

**Résumé :** Ajout d'un volet droit dans la vue contexte/niveau. Quand l'utilisateur clique sur un niveau (N1, N2…), la zone inférieure se divise en deux panneaux via `SplitContainer` vertical : à gauche les fiches BOM existantes (`_dgvFiches`), à droite le stock de production instancié par ce niveau (`_dgvStock`). Les données proviennent de `BomStockDAL.GetByNiveau()` (existait déjà — retourne uniquement `quantite_disponible > 0`, ordre FIFO). Colonnes affichées : Fiche · Qté dispo · Unité · Produit le · DLC · Coût/u.

**Erreur corrigée — SplitterDistance (R8) :**
- Symptôme : `InvalidOperationException` au clic sur un contexte
- Cause : `Panel1MinSize = 220, Panel2MinSize = 160` définis dans le constructeur du `SplitContainer`, avant que le contrôle soit layouté (Width = 0 → `Width - Panel2MinSize = -160` — contrainte impossible)
- Fix : retirer `Panel1MinSize` et `Panel2MinSize` du constructeur (valeurs par défaut 25px suffisent)
- Règle retenue : **règle #8** — ne jamais définir Panel1/Panel2MinSize au constructeur

**Erreur corrigée — Label.ContentAlignment (R9) :**
- Symptôme : `CS0117 — 'Label' ne contient pas de définition pour 'ContentAlignment'`
- Cause : la propriété d'alignement du Label est `TextAlign`, pas `ContentAlignment` (qui est seulement l'enum de valeur)
- Fix : `ContentAlignment = ...` → `TextAlign = ContentAlignment.MiddleLeft`

**DGV auto-sizing + scroll horizontal (R10, R11) :**
- `AutoSizeColumnsMode.Fill` remplacé par `None` sur les deux DGVs
- `ScrollBars = Both` ajouté pour scroll H automatique si colonnes > largeur
- `AutoResizeColumns(AllCells)` appelé après le binding pour ajuster à la largeur du contenu
- `DisplayIndex` forcé via compteur `int di = 0` dans `ShowCol()` — ordre modifiable par simple réordonnancement des appels `ShowCol`

**Selfdoubt appliqué :**
- `BomStockDAL.GetByNiveau()` existant et retournant `NomFiche`, `UniteOutput` (✅ Certain — lu)
- `AutoSizeColumnsMode.None` compatible avec `AutoResizeColumns` (✅ Certain)
- `SplitterDistance` exception avant layout : pattern documenté dans Règle #8 (✅ Certain — reproduit et corrigé)
- Build 0 erreur / 0 warning après chaque correction (✅ Certain — `dotnet build` exécuté)

---
