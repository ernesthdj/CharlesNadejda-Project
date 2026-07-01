# AGENT #2 -- ARCHITECT : ANALYSE ARCHITECTURALE
> **Date :** 2026-05-20
> **Codebase :** CharlesNadejda -- ERP artisanal patisserie/chocolaterie
> **Perimetre :** Couplage inter-couches, decomposition fichiers lourds, patterns structurels, flux navigation

---

## 1. CARTE DES DEPENDANCES : Forms --> DAL

### 1.1 Matrice de couplage (chaque Form et ses DAL directs)

| Form | DALs appeles | Nb DALs | Verdict |
|------|-------------|---------|---------|
| **FrmPrincipal.cs** (1961L) | ActiviteDAL, BomContexteDAL, BomNiveauDAL, IngredientDAL, BomFicheDAL, BomProductionDAL, BomStockDAL, BomFicheLigneDAL | **8** | FORT -- hub central, justifie |
| **FrmPrincipal.Production.cs** (959L) | BomProductionDAL, IngredientDAL, BomFicheDAL, BomContexteDAL, BomNiveauDAL, BomCoutDAL | **6** | FORT -- workflow complet inline |
| **FrmPrincipal.BoutiqueWeb.cs** (494L) | CategorieWebDAL, ProduitWebDAL, CommandeWebDAL | **3** | OK -- scope isole |
| **FrmVueStock.cs** (867L) | ActiviteDAL, VueStockGlobalDAL, BomNiveauDAL, IngredientDAL, LotDAL, BomFicheDAL, BomStockDAL, BomFicheLigneDAL | **8** | FORT -- vue transversale |
| **FrmBomProductionSimulation.cs** | BomContexteDAL, BomNiveauDAL, BomFicheDAL, BomProductionDAL, BomCoutDAL | **5** | MODERE -- workflow unifie |
| **FrmBomFicheEdit.cs** | IngredientDAL, BomNiveauDAL, BomFicheDAL | **3** | OK |
| **FrmIngredients.cs** | IngredientDAL, StockDAL | **2** | OK |
| **FrmIngredientEdit.cs** | StockDAL, FournisseurDAL, IngredientDAL | **3** | OK |
| **FrmAchatEdit.cs** | FournisseurDAL, IngredientDAL, LotDAL | **3** | OK |
| **FrmStocks.cs** | StockDAL, ActiviteDAL | **2** | OK |
| **FrmActivites.cs** | ActiviteDAL | **1** | OK |
| **FrmActiviteStocks.cs** | StockDAL | **1** | OK |
| **FrmBomContextes.cs** | BomContexteDAL | **1** | OK |
| **FrmBomContexteEdit.cs** | ActiviteDAL, BomContexteDAL | **2** | OK |
| **FrmBomFiches.cs** | BomFicheDAL | **1** | OK |
| **FrmBomNiveaux.cs** | BomNiveauDAL | **1** | OK |
| **FrmBomNiveauEdit.cs** | BomNiveauDAL | **1** | OK |
| **FrmFournisseurs.cs** | FournisseurDAL | **1** | OK |
| **FrmFournisseurEdit.cs** | FournisseurDAL | **1** | OK |
| **FrmStockEdit.cs** | StockDAL | **1** | OK |
| **FrmActiviteEdit.cs** | ActiviteDAL | **1** | OK |
| **FrmCategorieWebEdit.cs** | CategorieWebDAL | **1** | OK |
| **FrmProduitWebEdit.cs** | ProduitWebDAL, CategorieWebDAL | **2** | OK |
| **FrmLogin.cs** | UtilisateurDAL | **1** | OK |
| **FrmAchats.cs** | LotDAL | **1** | OK |

### 1.2 Synthese couplage

| Seuil | Forms | Commentaire |
|-------|-------|-------------|
| 5+ DALs | FrmPrincipal (8), FrmPrincipal.Production (6), FrmVueStock (8), FrmBomProductionSimulation (5) | 4 Forms a couplage fort |
| 2-4 DALs | FrmBomFicheEdit (3), FrmIngredientEdit (3), FrmAchatEdit (3), FrmBomContexteEdit (2), FrmIngredients (2), FrmStocks (2), FrmProduitWebEdit (2), FrmPrincipal.BoutiqueWeb (3) | 8 Forms -- couplage modere normal |
| 1 DAL | 12 Forms | Couplage faible -- bon |

**Verdict global :** Le couplage est concentre sur 4 fichiers. C'est structurellement attendu pour un hub ERP SFA (Single-Form Application). FrmPrincipal est le point d'orchestration -- il est **normal** qu'il touche a tout. Le vrai probleme n'est pas le nombre de DALs, mais le **volume de code UI** dans ces 4 fichiers.

### 1.3 Couplage inter-DAL (DAL --> DAL)

| DAL | Appelle | Commentaire |
|-----|---------|-------------|
| **BomProductionDAL** | BomNiveauDAL, BomFicheDAL, BomStockDAL | Moteur de production -- couplage justifie par la transaction atomique |
| **BomCoutDAL** | BomFicheDAL | Calcul recursif -- couplage minimal |
| **BomFicheDAL** | BomFicheLigneDAL | Composition fiche+lignes -- pattern normal |

Pas de couplage circulaire entre DALs. Le couplage est unidirectionnel et suit la hierarchie metier.

---

## 2. DECOMPOSITION DES 3 FICHIERS LOURDS

### 2.1 FrmPrincipal.cs -- 1961 lignes (+ 2 partials = 3414 lignes logiques)

**Etat actuel :** Deja partiellement decomporse via `partial class` :
- `FrmPrincipal.cs` (1961L) -- Shell + Hub + Contextes/Niveaux/Kanban + Rapport
- `FrmPrincipal.Production.cs` (959L) -- Ecran production inline
- `FrmPrincipal.BoutiqueWeb.cs` (494L) -- Ecran boutique web
- `FrmPrincipal.Designer.cs` (107L) -- Auto-genere

**Analyse par bloc fonctionnel dans FrmPrincipal.cs :**

| Bloc | Lignes approx. | Responsabilite |
|------|---------------|----------------|
| Shell + Router + Navigation | 1-200 | Construction shell, routage -- **OK, reste ici** |
| Chargement donnees | 241-288 | ChargerActivites/Contextes/Niveaux -- **OK** |
| ShowOnboarding | 294-355 | Ecran bienvenue -- extractible |
| ShowHubScreen | 357-634 | Dashboard avec KPIs -- **LOURD : 277 lignes** |
| ShowContexteScreen | 636-851 | Kanban 3 colonnes -- **LOURD : 215 lignes** |
| Detail Kanban (Render*) | 883-1042 | 3 methodes Render* -- extractible |
| Helpers rendu (KDetail*) | 1044-1121 | 6 helpers -- extractible |
| DGV CellFormatting/Painting | 1123-1246 | Custom drawing jauge -- extractible |
| ChargerStockNiveau | 1248-1338 | Config DGV stock -- OK |
| Actions Contextes/Niveaux | 1340-1414 | CRUD handlers -- OK |
| GenererRapport | 1422-1548 | Rapport PrintDocument -- **extractible** |
| Builder helpers (MakeStat*, etc.) | 1550-1780 | Factory de controles -- **extractible** |
| Ecrans Ressources + EmbedForm | 1782-1843 | Navigation SFA -- OK |
| Placeholder + Menu | 1845-1961 | Divers -- OK |

**Strategie de decomposition proposee :**

| Refactoring | Cible | Gain | Effort |
|-------------|-------|------|--------|
| R-01 : Extraire `FrmPrincipal.Hub.cs` (partial) | ShowHubScreen + MakeStatCard + MakeAlertRow | -340L dans .cs principal | `MEDIUM` |
| R-02 : Extraire `FrmPrincipal.Contexte.cs` (partial) | ShowContexteScreen + MakeNiveauCard + Kanban helpers + DGV Cell* | -500L | `MEDIUM` |
| R-03 : Extraire `FrmPrincipal.KanbanDetail.cs` (partial) | RenderIngredient/Fiche/StockDetail + tous KDetail* helpers | -200L | `QUICK` |
| R-04 : Extraire GenererRapport dans `RapportDuJourService.cs` | GenererRapport() -> service qui retourne un PrintDocument | -130L + separation logique/UI | `MEDIUM` |

**Resultat apres extraction :** FrmPrincipal.cs passerait de 1961L a environ **790L** (Shell + Navigation + Chargement + Actions + EmbedForm). Chaque partial resterait sous 500L.

### 2.2 BomProductionDAL.cs -- 526 lignes

**Analyse structurelle :**

| Methode | Lignes | Responsabilite |
|---------|--------|----------------|
| GetByNiveau | 15-38 | Query lecture -- **pure DAL** |
| GetRecentByActivite | 40-65 | Query lecture -- **pure DAL** |
| GetDuJourByActivite | 73-97 | Query lecture -- **pure DAL** |
| VerifierDisponibilite | 107-166 | **Logique metier** : compare stock vs besoin avec conversion d'unites |
| Simuler | 175-223 | **Logique metier** : idem VerifierDisponibilite mais retourne tout |
| Executer | 235-344 | **Mixte** : transaction DB + logique metier (calcul cout, FIFO) |
| ConsumeStock | 353-448 | **Logique metier + DB** : FIFO + decrementation transactionnelle |
| InsertLigne | 450-471 | Pure DAL -- tracabilite |
| GetIdNiveauDeFiche | 479-489 | Pure DAL -- query scalaire |
| GetIdNiveauPrecedent | 495-508 | Pure DAL -- query scalaire (note : non utilisee?) |
| MapHeader | 510-524 | Pure mapping |

**Verdict :** BomProductionDAL melange 3 responsabilites :
1. **Acces donnees** (queries, inserts, updates) -- OK dans un DAL
2. **Logique metier** (verification dispo, calcul multiplicateur, FIFO) -- devrait etre dans un Service
3. **Conversion d'unites** (appels UnitConvertisseur) -- logique applicative

**Proposition :** `HEAVY` -- extraire vers `ProductionService.cs`

Toutefois, la contrainte pragmatique est forte :
- La methode `Executer()` **doit** rester transactionnelle et atomique
- Separer la logique metier du code transactionnel DB ajouterait de la complexite sans gain fonctionnel immediat
- Le fichier fait 526 lignes -- c'est au-dessus de la norme mais pas critique

**Recommandation pragmatique :** Garder BomProductionDAL tel quel, mais :
1. Factoriser la duplication entre `VerifierDisponibilite()` et `Simuler()` (meme boucle, resultat different) -- `QUICK`
2. Documenter clairement les 3 zones dans le fichier avec des bannières de section
3. Si un jour un `ProductionService` est cree, il appellerait `BomProductionDAL.Executer()` tel quel

### 2.3 FrmVueStock.cs -- 867 lignes

**Analyse structurelle :**

| Bloc | Lignes | Responsabilite |
|------|--------|----------------|
| BuildUI | 54-238 | Construction UI -- **185 lignes, extractible** |
| Charger/Filtre/Chips | 265-376 | Logique de filtrage + agregation ingredients |
| RemplirGrille | 378-464 | Mise en forme DGV + categorisation |
| ExporterCsv | 470-513 | Export CSV -- **extractible dans un service** |
| Volet detail (Render*) | 519-739 | 2 methodes de rendu detail -- **extractible** |
| Helpers detail (AddDetail*) | 741-831 | Helpers rendu -- quasi-identiques a FrmPrincipal.KDetail* |
| CellFormatting | 835-865 | Coloration lignes |

**Problemes identifies :**

| # | Probleme | Localisation | Impact |
|---|----------|-------------|--------|
| V-01 | **Duplication RenderDetail** : les helpers `AddDetailHeader/Section/Row/Tag` sont des copies quasi-identiques de `KDetailHeader/Section/Row/Tag` dans FrmPrincipal | FrmVueStock L741-831 vs FrmPrincipal L1046-1121 | Maintenabilite |
| V-02 | **Logique metier dans Form** : `AgregerIngredients()` effectue une agregation comptable (moyenne ponderee des couts) -- c'est de la logique metier | FrmVueStock L321-376 | Architecture |
| V-03 | **N+1 potentiel** : `GetOrdreNiveau()` appelle `BomNiveauDAL.GetById()` par ID dans une boucle, malgre un cache local | FrmVueStock L456-464 | Performance |
| V-04 | **IngredientDAL.GetAll() sans filtre** : dans `RenderDetailIngredient()`, charge TOUS les ingredients pour en trouver un seul par ID | FrmVueStock L556 | Performance |

**Propositions :**

| Refactoring | Description | Effort | Impact |
|-------------|-------------|--------|--------|
| R-05 : Extraire `DetailPanelHelper.cs` | Factoriser les helpers AddDetail*/KDetail* en une classe utilitaire partagee entre FrmPrincipal et FrmVueStock | `MEDIUM` | Elimine ~160L de duplication |
| R-06 : Deplacer `AgregerIngredients()` dans `VueStockGlobalDAL` ou un `StockAggregationService` | La logique d'agregation est comptable, pas UI | `QUICK` |
| R-07 : Ajouter `IngredientDAL.GetById(int id)` | Evite de charger GetAll() pour un seul ingredient | `QUICK` |

---

## 3. PATTERN STATIC DAL VS INTERFACE + DI

### 3.1 Etat actuel

Toutes les 19 DALs sont `public static class` avec des methodes statiques. Chaque methode ouvre/ferme sa propre connexion via `DbHelper.GetConnection()`.

### 3.2 Avantages du pattern actuel

| Avantage | Detail |
|----------|--------|
| Simplicite d'appel | `IngredientDAL.GetAll()` -- zero ceremony, lisible |
| Zero configuration | Pas de container DI, pas de registration, pas de scope |
| Coherent dans tout le projet | 100% des DALs suivent le meme pattern |
| Adapte au contexte | Projet etudiant WinForms -- pas d'API, pas de multi-tenant |
| Testabilite suffisante | Les DALs sont des facades fines vers MySQL -- les tests d'integration suffisent |

### 3.3 Inconvenients identifies

| Inconvenient | Gravite | Mitigation actuelle |
|-------------|---------|---------------------|
| Non-mockable pour tests unitaires purs | FAIBLE | Pas de tests unitaires prevus pour ce projet |
| Couplage fort Forms --> DAL concret | FAIBLE | Le nombre de DALs par Form est raisonnable (cf. section 1) |
| Pas de polymorphisme (ex: SQLite en test, MySQL en prod) | FAIBLE | Un seul environnement cible |

### 3.4 Verdict

**Garder le pattern static DAL tel quel.**

Justification :
- Le gain d'une DI (Dependency Injection -- injection de dependances via un conteneur) serait **nul** dans ce contexte : pas de tests unitaires, pas de multi-provider, pas d'API.
- Introduire une DI dans un projet WinForms .NET Framework 4.x ajouterait de la complexite (NuGet `Microsoft.Extensions.DependencyInjection`, registration manuelle, propagation du scope dans chaque Form) pour un gain theorique.
- Le pattern actuel est **propre, coherent et maintenable**. Ne pas changer ce qui fonctionne.

> Si le projet evoluait vers une API ASP.NET Core, la migration vers des interfaces + DI serait naturelle mais ce n'est pas le cas aujourd'hui.

---

## 4. FLUX NAVIGATION : AppState --> ScreenRouter --> FrmPrincipal

### 4.1 Architecture du flux

```
FrmLogin
  |
  v (ShowDialog OK)
FrmPrincipal(Utilisateur)
  |
  |-- BuildShell() -> TitleBarPanel, SidebarPanel, AppStatusBar, _pnlDroit
  |-- InitRouter() -> câble ScreenRouter.On* vers Show*Screen()
  |
  [SidebarPanel]
    |-- NavigationRequested(NavItemId) --> OnSidebarNavigation()
    |       |
    |       v
    |   NavigateTo(ScreenId, stateSetup?, forceRefresh?)
    |       |
    |       v
    |   ScreenRouter.Navigate(ScreenId)
    |       |-- Guard singleton (evite rebuild inutile)
    |       |-- Dispatch vers On* callback
    |       v
    |   Show*Screen() --> construit l'UI dans _pnlDroit
    |
    |-- ActivityChanged(Activite) --> OnActivityChanged()
    |-- NewContextRequested --> BtnNouveauContexte_Click()
    |-- ManageActivitiesRequested --> FrmActivites (ShowDialog)
```

### 4.2 Evaluation

| Critere | Verdict | Detail |
|---------|---------|--------|
| Point d'entree unique | OK | `NavigateTo()` est le seul point d'entree -- bien documente |
| Guard de re-navigation | OK | `ScreenRouter.Navigate()` evite les rebuilds inutiles via `_lastScreen` |
| Separation etat/navigation | OK | `AppState` gere l'etat, `ScreenRouter` gere le dispatch |
| Invalidation | OK | `forceRefresh` + `router.Invalidate()` pour forcer le rebuild apres CRUD |
| Mapping NavItemId --> ScreenId | OK | Explicite dans `OnSidebarNavigation()` switch/case |

### 4.3 Points d'attention mineurs

| # | Observation | Fichier:Ligne | Severite |
|---|-------------|---------------|----------|
| N-01 | `NavItemId.FichesBom` redirige vers `ScreenId.ContexteNiveaux` (commentaire "retrocompat") -- est-ce voulu a terme ? | FrmPrincipal.cs:168-169 | INFO |
| N-02 | `ScreenRouter.OnPlaceholder` est utilise pour 4 ScreenIds differents (Planning, Devis, Mouvements, Parametres) -- OK pour des placeholders, mais a refactorer quand ces modules seront implementes | ScreenRouter.cs:67 | INFO |
| N-03 | `AppState.ActiveScreen` a un setter `internal set` -- seul `ScreenRouter` peut le modifier. Bien. | AppState.cs:12 | OK |
| N-04 | `GetIdNiveauPrecedent()` dans BomProductionDAL (L495-508) semble **non utilise** -- methode orpheline potentielle | BomProductionDAL.cs:495 | QUICK |

**Verdict global navigation : SOLIDE.** Le flux est clair, documente, et le guard singleton evite les problemes de performance. Pas de refactoring necessaire sur cette couche.

---

## 5. RESPONSABILITES MAL PLACEES

### 5.1 Logique metier dans les Forms

| # | Probleme | Localisation | Devrait etre dans | Effort |
|---|----------|-------------|-------------------|--------|
| M-01 | Calcul `cout7j` (somme des couts 7 derniers jours) | FrmPrincipal.cs:380-381 | Service ou DAL (agreg SQL) | `QUICK` |
| M-02 | Calcul `prods7j` (count productions 7j) | FrmPrincipal.cs:379 | Idem | `QUICK` |
| M-03 | `AgregerIngredients()` -- agregation comptable (moyenne ponderee couts, somme quantites) | FrmVueStock.cs:321-376 | VueStockGlobalDAL ou Service | `QUICK` |
| M-04 | Categorisation ingredients/intermediaires/finaux dans `RemplirGrille()` | FrmVueStock.cs:383-401 | Pourrait etre une propriete calculee dans VueStockGlobal ou un helper | `MEDIUM` |
| M-05 | Logique de production complete (simulation + confirmation + journal) inline dans FrmPrincipal.Production.cs | FrmPrincipal.Production.cs | Deja partiellement dans BomProductionDAL -- acceptable | INFO |
| M-06 | `GenererRapport()` -- generation PrintDocument avec logique de mise en page | FrmPrincipal.cs:1429-1548 | `RapportService` ou classe dediee | `MEDIUM` |

### 5.2 Logique metier dans les DALs

| # | Probleme | Localisation | Devrait etre dans | Effort |
|---|----------|-------------|-------------------|--------|
| D-01 | `BomProductionDAL.VerifierDisponibilite()` -- logique de comparaison stock vs besoin + conversion d'unites | BomProductionDAL.cs:107-166 | `ProductionService` (idealement) | `HEAVY` |
| D-02 | `BomProductionDAL.Simuler()` -- quasi-identique a VerifierDisponibilite avec resultat different | BomProductionDAL.cs:175-223 | Factoriser avec D-01 | `QUICK` |
| D-03 | `BomProductionDAL.ConsumeStock()` -- logique FIFO avec conversion d'unites | BomProductionDAL.cs:353-448 | Lie a la transaction -- **doit rester dans le DAL** | AUCUN |
| D-04 | `BomCoutDAL.CalculerCout()` -- calcul recursif avec regle de 3 + conversion d'unites | BomCoutDAL.cs:46-110 | `CoutService` (idealement), mais la recursion + acces DB rend la separation couteuse | `HEAVY` |

### 5.3 UnitConvertisseur -- localisation

`UnitConvertisseur` est dans `Forms/` mais est utilise par :
- `BomProductionDAL` (6 appels)
- `BomCoutDAL` (1 appel)
- Plusieurs Forms (formatage affichage)

C'est un utilitaire metier, pas un helper UI. Il devrait etre dans un namespace neutre (`Services/` ou `Utils/`).

| Refactoring | Description | Effort |
|-------------|-------------|--------|
| R-08 : Deplacer `UnitConvertisseur.cs` vers `Services/` | Changement de namespace uniquement (+ using dans les DALs/Forms) | `QUICK` |

---

## 6. DUPLICATION DE CODE IDENTIFIEE

| # | Code duplique | Fichier A | Fichier B | Lignes | Effort dedup |
|---|--------------|-----------|-----------|--------|-------------|
| DUP-01 | Helpers detail panel (Header/Section/Row/Tag) | FrmPrincipal.cs (KDetail*) L1046-1121 | FrmVueStock.cs (AddDetail*) L741-831 | ~160L | `MEDIUM` |
| DUP-02 | DGV config boilerplate (colonnes, styles, headers) | FrmPrincipal.cs L773-791 | Repete dans chaque Form avec DGV | ~30L par instance | `QUICK` (via FormHelper) |
| DUP-03 | VerifierDisponibilite / Simuler | BomProductionDAL.cs L107-223 | Meme boucle, resultat different | ~60L | `QUICK` |

---

## 7. SYNTHESE DES REFACTORINGS PROPOSES

### Par priorite (effort vs impact)

| # | Refactoring | Effort | Impact | Priorite |
|---|-------------|--------|--------|----------|
| R-03 | Extraire `FrmPrincipal.KanbanDetail.cs` (partial) | `QUICK` | Lisibilite FrmPrincipal (-200L) | P1 |
| R-08 | Deplacer `UnitConvertisseur.cs` vers `Services/` | `QUICK` | Architecture (placement correct) | P1 |
| R-07 | Ajouter `IngredientDAL.GetById(int id)` | `QUICK` | Performance (evite GetAll inutile) | P1 |
| N-04 | Supprimer `GetIdNiveauPrecedent()` si confirme orphelin | `QUICK` | Nettoyage code mort | P1 |
| DUP-03 | Factoriser VerifierDispo/Simuler en methode privee commune | `QUICK` | Maintenabilite (-60L duplication) | P2 |
| R-06 | Deplacer `AgregerIngredients()` hors de FrmVueStock | `QUICK` | Architecture (separation logique/UI) | P2 |
| R-01 | Extraire `FrmPrincipal.Hub.cs` (partial) | `MEDIUM` | Lisibilite FrmPrincipal (-340L) | P2 |
| R-02 | Extraire `FrmPrincipal.Contexte.cs` (partial) | `MEDIUM` | Lisibilite FrmPrincipal (-500L) | P2 |
| R-05 | Extraire `DetailPanelHelper.cs` (helpers partages) | `MEDIUM` | Elimine 160L de duplication | P3 |
| R-04 | Extraire `RapportDuJourService.cs` | `MEDIUM` | Separation logique/UI (-130L) | P3 |

### Ce qui ne devrait PAS etre refactore

| Element | Raison de ne pas toucher |
|---------|--------------------------|
| Pattern static DAL | Fonctionne, coherent, adapte au contexte projet |
| Flux Navigation (AppState/ScreenRouter) | Solide, bien documente, pas de probleme |
| FrmListeBase / FrmEditBase | Excellente abstraction, bien utilisee partout |
| BomProductionDAL.Executer() | Transaction atomique -- la separation ajouterait de la complexite sans gain |
| BomCoutDAL | Recursion + DB -- separation couteuse pour un gain theorique |
| SimulationService | Existe deja, fait son travail (projection BomManque -> SimulationResultat) |

---

## 8. AVIS FINAL

### Score architecture : 7.5/10

**Points forts :**
- Navigation SFA bien architecturee (AppState/ScreenRouter/partials)
- Abstraction FrmListeBase/FrmEditBase solide et coherente
- Pattern DAL statique uniforme et simple
- Separation Shell (TitleBarPanel, SidebarPanel, StatusBarPanel) propre
- Aucun couplage circulaire
- Transactions atomiques dans BomProductionDAL

**Points a ameliorer :**
- FrmPrincipal.cs reste trop gros malgre les partials -- 3 extractions de partial supplementaires le rendraient maintenable
- Duplication des helpers de rendu detail entre FrmPrincipal et FrmVueStock
- UnitConvertisseur mal place (dans Forms/ alors qu'utilise par les DALs)
- Quelques logiques metier dans les Forms (agregation, calculs KPI)

**Aucun probleme bloquant.** Tous les refactorings sont des ameliorations incrementales, pas des corrections d'erreurs. Le codebase est sain pour un projet etudiant.
