# AUDIT-SYNTHESE -- ArtisaStock
> Date: 2026-06-11 | Agent: QA Synthese (#7)
> Sources: 6 rapports d'audit (120 findings bruts)
> Branche: `feat/refactoring-sprints-p0-p3`

---

## Vue d'ensemble

| Metrique | Valeur |
|----------|--------|
| Findings bruts | 120 |
| Findings informatifs (RAS) | 3 (F-NAV-007, F-NAV-011, F-SQL-017) |
| Findings retrocedes (OK apres verification) | 1 (F-LAR-011) |
| Findings reels | 116 |
| Doublons inter-agents | 16 findings impliques dans 8 groupes de doublons |
| **Findings uniques apres deduplication** | **104** |
| Repartition | 15 critiques, 49 importants, 52 mineurs (brut) |
| Repartition dedupliquee | **12 critiques, 43 importants, 49 mineurs** |

---

## Doublons identifies

Les findings suivants decrivent le meme probleme observe par 2 ou 3 agents differents.

| Doublon # | Findings | Agents | Probleme commun | Finding retenu |
|-----------|----------|--------|-----------------|----------------|
| D-01 | F-DAL-001, F-DAL-026, F-MOD-001, F-MOD-004, F-SQL-001, F-SQL-002 | DAL + Models + SQL | `schema_complet.sql` desynchronise (colonne `nb_par_lot` absente, VIEW cassee) | F-SQL-001 (le plus complet) |
| D-02 | F-DAL-005, F-MOD-002, F-SQL-009 | DAL + Models + SQL | Colonne `dlc_jours_reference` jamais lue ni mappee | F-MOD-002 (inclut l'action Model+DAL) |
| D-03 | F-DAL-006, F-MOD-002, F-SQL-010 | DAL + Models + SQL | Colonne `qualite_label` jamais lue ni mappee | F-MOD-002 (meme finding, 2 colonnes) |
| D-04 | F-DAL-007, F-MOD-003, F-SQL-021 | DAL + Models + SQL | Colonne `fiches_ingredients.date_creation` jamais lue | F-MOD-003 |
| D-05 | F-DAL-017, F-MOD-009, F-SQL-011 | DAL + Models + SQL | Colonnes utilisateurs (telephone, adresse, etc.) non mappees | F-MOD-009 |
| D-06 | F-DAL-002, F-MOD-011 | DAL + Models | `CategorieWeb.date_creation` absente du Model + SELECT c.* | F-DAL-002 (couvre les 2 aspects) |
| D-07 | F-NAV-002, F-NAV-003 | Navigation x2 | NavItemId orphelins (FichesBom, NiveauxContextes) dans la sidebar | F-NAV-002+003 (groupes) |
| D-08 | F-LAR-003, F-LAR-004 | Laravel x2 | Logique panier dupliquee (View Composer + Controller) | F-LAR-004 (inclut le N+1) |

**Apres elimination des doublons : 104 findings uniques.**

---

## Theme A : Coherence DAL / Model / Schema

### Contexte
Desynchronisation entre les colonnes declarees en base, les champs mappes dans le DAL C#, et les proprietes des Models. Ce theme regroupe les colonnes orphelines (presentes en DB mais jamais lues), les Models incomplets, et les fichiers SQL obsoletes.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-SQL-001 | CRITIQUE | `schema_complet.sql` obsolete (pre-v19), non deployable | SQL |
| F-SQL-003 | CRITIQUE | `seed_data.sql` reference des tables supprimees en v12, 100% casse | SQL |
| F-SQL-004 | IMPORTANT | FK `fk_bc_activite` : CASCADE dans schema_complet vs RESTRICT dans create_database | SQL |
| F-MOD-002 | CRITIQUE | Colonnes `dlc_jours_reference` + `qualite_label` absentes du Model Ingredient et du DAL | Models |
| F-MOD-003 | MINEUR | Colonne `fiches_ingredients.date_creation` absente du Model | Models |
| F-MOD-005 | MINEUR | Colonne `lots_ingredients.date_creation` absente du Model Lot | Models |
| F-MOD-009 | CRITIQUE | 7 colonnes utilisateurs manquantes dans le Model (acceptable si scope auth-only) | Models |
| F-MOD-010 | MINEUR | Colonne `produits_web.date_modification` absente du Model ProduitWeb | Models |
| F-MOD-011 | MINEUR | Colonne `categories_web.date_creation` absente du Model CategorieWeb | Models |
| F-MOD-012 | IMPORTANT | `BomNiveau.Ordre` est `int` mais DB est `TINYINT UNSIGNED` (0-255) | Models |
| F-DAL-002 | IMPORTANT | `CategorieWebDAL` utilise `SELECT c.*` au lieu de colonnes explicites | DAL |
| F-DAL-019 | MINEUR | `ProduitWebDAL.Update` ne met pas a jour `id_bom_fiche` (parametre orphelin `@idFiche`) | DAL |
| F-SQL-011 | IMPORTANT | 5 colonnes utilisateurs jamais lues par le DAL | SQL |

### Plan de correction

1. **Supprimer ou regenerer `schema_complet.sql`** depuis `create_database.sql`
   - Fichier : `sql/schema_complet.sql`
   - Le fichier `create_database.sql` est deja la reference post-v19. Supprimer `schema_complet.sql` ou le regener.
   - Elimine F-SQL-001, F-SQL-002, F-SQL-004 d'un coup.

2. **Reecrire `seed_data.sql`** pour le schema post-v19 (20 tables actuelles)
   - Fichier : `sql/seed_data.sql`
   - S'inspirer de `sql/tests/injection_glacier_stress_test.sql` qui est fonctionnel.

3. **Ajouter `DlcJoursReference` et `QualiteLabel` au Model Ingredient + DAL**
   - Fichiers : `Models/Ingredient.cs`, `DAL/IngredientDAL.cs` (SELECT, INSERT, UPDATE, Map -- 4 points chacun)
   - Eventuellement un champ dans `FrmIngredientEdit.cs`

4. **Ajouter les `DateCreation` / `DateModification` manquantes** (si utile pour defense)
   - Fichiers : `Models/Ingredient.cs`, `Models/Lot.cs`, `Models/ProduitWeb.cs`, `Models/CategorieWeb.cs`
   - DAL correspondants pour les SELECT

5. **Remplacer `SELECT c.*` par colonnes explicites dans CategorieWebDAL**
   - Fichier : `DAL/CategorieWebDAL.cs`

6. **Documenter le scope auth-only de Utilisateur** via commentaire XML
   - Fichier : `Models/Utilisateur.cs`

### Fichiers impactes (12)
`sql/schema_complet.sql`, `sql/seed_data.sql`, `Models/Ingredient.cs`, `DAL/IngredientDAL.cs`, `Models/Lot.cs`, `Models/ProduitWeb.cs`, `Models/CategorieWeb.cs`, `DAL/CategorieWebDAL.cs`, `Models/Utilisateur.cs`, `Models/BomNiveau.cs`, `DAL/ProduitWebDAL.cs`, `Forms/FrmIngredientEdit.cs`

### Risque de regression
- Moyen pour l'ajout de colonnes Model/DAL (modifier 4 points par colonne, risque d'oubli)
- Faible pour la suppression de `schema_complet.sql` (aucun code ne le reference)
- Faible pour `seed_data.sql` (fichier de donnees de test uniquement)

### Priorite: P0 (schema_complet + seed) / P1 (colonnes Model/DAL)

---

## Theme B : Patterns Forms (heritage, DGV, dispose)

### Contexte
Trois formulaires liste n'heritent pas de `FrmListeBase<T>` et reconstruisent manuellement le pattern CRUD. Des helpers UI (boutons, bandeaux, MakeRow) sont dupliques dans 3-4 fichiers. Des appels `Controls.Clear()` sans `Dispose` prealable violent la regle JOURNAL #18.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-FRM-001 | CRITIQUE | `FrmActivites` n'herite pas de `FrmListeBase` (~300 lignes dupliquees) | Forms |
| F-FRM-002 | CRITIQUE | `FrmStocks` n'herite pas de `FrmListeBase` | Forms |
| F-FRM-003 | CRITIQUE | `FrmFournisseurs` utilise Designer au lieu de `FrmListeBase` | Forms |
| F-FRM-004 | IMPORTANT | `CreerBouton`/`CreerBtn` duplique dans 3 fichiers | Forms |
| F-FRM-005 | IMPORTANT | `MakeRow` duplique dans 3 formulaires Edit | Forms |
| F-FRM-006 | IMPORTANT | Style DGV header duplique dans FrmActivites + FrmStocks | Forms |
| F-FRM-007 | IMPORTANT | Bandeau header chocolat duplique dans 4 formulaires | Forms |
| F-FRM-008 | IMPORTANT | `Controls.Clear()` sans Dispose dans FrmBoutiqueWeb + FrmVueStock | Forms |
| F-FRM-009 | IMPORTANT | `Controls.Clear()` sans Dispose dans FrmPrincipal.Production ligne 724 | Forms |
| F-FRM-019 | MINEUR | Fonts inline sans Dispose dans FrmActivites + FrmStocks | Forms |
| F-FRM-022 | MINEUR | SidebarPanel `CboActivite_DrawItem` cree des Brushes sans using | Forms |

### Plan de correction

1. **Migrer FrmActivites vers `FrmListeBase<Activite>`** -- elimine F-FRM-001, F-FRM-004 (partiel), F-FRM-006 (partiel), F-FRM-007 (partiel), F-FRM-019 (partiel)
   - Fichier : `Forms/FrmActivites.cs` (rewrite)
   - Bouton Desactiver/Reactiver via hook `BtnYExtra`

2. **Migrer FrmStocks vers `FrmListeBase<Stock>`** -- elimine F-FRM-002
   - Fichier : `Forms/FrmStocks.cs` (rewrite)
   - SplitContainer liaison en surcharge du layout

3. **Migrer FrmFournisseurs vers `FrmListeBase<Fournisseur>`** + supprimer Designer.cs
   - Fichiers : `Forms/FrmFournisseurs.cs` (rewrite), `Forms/FrmFournisseurs.Designer.cs` (supprimer)

4. **Corriger les `Controls.Clear()` sans Dispose** (3 occurrences)
   - Fichiers : `Forms/FrmPrincipal.BoutiqueWeb.cs:389`, `Forms/FrmVueStock.cs:274,471`, `Forms/FrmPrincipal.Production.cs:724`
   - Pattern : foreach(Control c in panel.Controls) c.Dispose(); panel.Controls.Clear();

5. **Extraire helpers communs** (si non absorbes par les migrations)
   - `FormHelper.CreerBandeauHeader(titre, hint)` -- pour FrmActiviteStocks + FrmVueStock
   - `FormHelper.AddLabeledTextBox(...)` -- pour les FrmEdit restants
   - Fonts et Brushes en `static readonly`

### Ordre d'execution
- Etape 4 (Dispose) d'abord -- correction rapide, zero dependance
- Etapes 1-3 (migrations heritage) ensuite -- chaque form est independant
- Etape 5 (helpers) en dernier -- benefice marginal si les forms sont deja migres

### Fichiers impactes (9)
`Forms/FrmActivites.cs`, `Forms/FrmStocks.cs`, `Forms/FrmFournisseurs.cs`, `Forms/FrmFournisseurs.Designer.cs`, `Forms/FrmPrincipal.BoutiqueWeb.cs`, `Forms/FrmVueStock.cs`, `Forms/FrmPrincipal.Production.cs`, `Forms/Shell/SidebarPanel.cs`, `Forms/FrmListeBase.cs` (eventuellement pour ajout helpers)

### Risque de regression
- **Eleve pour les migrations heritage** (F-FRM-001/002/003) -- rewrite complet, tester chaque CRUD
- Faible pour les Dispose (F-FRM-008/009) -- correction locale
- Faible pour les helpers (F-FRM-004/005/006/007)

### Priorite: P0 (Dispose) / P1 (heritage FrmActivites, FrmStocks, FrmFournisseurs)

---

## Theme C : Dead code et orphelins

### Contexte
Enums, callbacks, classes et fichiers qui ne sont jamais utilises dans le code applicatif. Ils alourdissent le codebase et creent de la confusion.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-NAV-008 | CRITIQUE | `AppState.StateChanged` -- zero abonne dans tout le codebase | Navigation |
| F-NAV-001 | IMPORTANT | `ScreenId.ContexteNiveaux` alias mort + callback `OnContexteNiveaux` jamais appele | Navigation |
| F-NAV-002 | IMPORTANT | `NavItemId.FichesBom` declare mais absent de la sidebar | Navigation |
| F-NAV-003 | MINEUR | `NavItemId.NiveauxContextes` declare mais absent de la sidebar | Navigation |
| F-NAV-004 | IMPORTANT | `NavigationParams` -- 3 champs sur 5 jamais lus (ScrollToId, Entity, IsEdit) | Navigation |
| F-NAV-009 | IMPORTANT | Dualite `FiltreAlertesSeulement` dans AppState ET NavigationParams | Navigation |
| F-FRM-018 | MINEUR | MenuStrip visible=false dans FrmPrincipal.Designer.cs (~60 lignes mortes) | Forms |
| F-LAR-012 | MINEUR | Model `User.php` inutilise (scaffold Laravel par defaut) | Laravel |
| F-SQL-016 | MINEUR | `reset_db_for_tests.sql` reference des tables supprimees en v12 | SQL |

### Plan de correction

1. **Decider du sort de `AppState.StateChanged`**
   - Option A : Le supprimer (recommande si la navigation reste pilotee par `NavigateTo()`)
   - Option B : L'utiliser pour la StatusBar/TitleBar (reactif au lieu de mise a jour manuelle)
   - Fichier : `Navigation/AppState.cs`

2. **Nettoyer NavigationParams**
   - Supprimer `ScrollToId`, `Entity`, `IsEdit`, `FiltreAlertesSeulement`
   - Si toutes les proprietes supprimees : supprimer la classe et changer les callbacks en `Action`
   - Fichiers : `Navigation/NavigationParams.cs`, `Navigation/ScreenRouter.cs`, `Forms/FrmPrincipal.cs`

3. **Supprimer les ScreenId/NavItemId orphelins**
   - `ScreenId.ContexteNiveaux`, `NavItemId.FichesBom`, `NavItemId.NiveauxContextes`
   - Callback `OnContexteNiveaux` dans ScreenRouter
   - Fichiers : `Navigation/ScreenId.cs`, `Navigation/NavItemId.cs`, `Navigation/ScreenRouter.cs`, `Forms/FrmPrincipal.cs`

4. **Supprimer le MenuStrip mort** dans FrmPrincipal.Designer.cs
   - Fichiers : `Forms/FrmPrincipal.Designer.cs`, `Forms/FrmPrincipal.cs` (event handlers correspondants)

5. **Supprimer `User.php`** ou commenter
   - Fichier : `site-laravel/app/Models/User.php`

6. **Reecrire `reset_db_for_tests.sql`** pour les 20 tables actuelles
   - Fichier : `sql/tests/reset_db_for_tests.sql`

### Fichiers impactes (10)
`Navigation/AppState.cs`, `Navigation/NavigationParams.cs`, `Navigation/ScreenRouter.cs`, `Navigation/ScreenId.cs`, `Navigation/NavItemId.cs`, `Forms/FrmPrincipal.cs`, `Forms/FrmPrincipal.Designer.cs`, `site-laravel/app/Models/User.php`, `sql/tests/reset_db_for_tests.sql`, `Navigation/RessourceType.cs` (verification)

### Risque de regression
- Faible pour la suppression de dead code (par definition, rien ne l'utilise)
- Moyen pour `NavigationParams` si un futur ecran avait prevu de lire les proprietes supprimees
- Nul pour User.php et reset_db_for_tests.sql

### Priorite: P1

---

## Theme D : Securite et transactions

### Contexte
Transactions manquantes dans les operations Delete multi-requetes cote C#, lectures hors transaction dans l'execution de production FIFO, bug de precedence operateur dans Laravel, et pattern `DbHelper.GetConnection` fragile.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-DAL-004 | CRITIQUE | `BomProductionDAL.Executer` -- lectures hors transaction (race condition TOCTOU) | DAL |
| F-LAR-001 | CRITIQUE | Bug operator precedence dans accessor `stock_calc` (ProduitWeb.php) | Laravel |
| F-LAR-002 | CRITIQUE | Champ `actif` non documente comme exclu de $fillable (fragile en maintenance) | Laravel |
| F-DAL-010 | IMPORTANT | `BomContexteDAL.Delete` -- multi-requetes sans transaction | DAL |
| F-DAL-011 | IMPORTANT | `BomFicheDAL.Delete` -- multi-requetes sans transaction | DAL |
| F-DAL-012 | IMPORTANT | `IngredientDAL.Delete` -- multi-requetes sans transaction | DAL |
| F-DAL-013 | IMPORTANT | `LotDAL.Delete` -- multi-requetes sans transaction | DAL |
| F-DAL-018 | IMPORTANT | `DbHelper.GetConnection` -- connexion non-disposee si `Open()` echoue | DAL |
| F-DAL-022 | IMPORTANT | `BomCoutDAL` -- detection de cycle incomplete (copie du HashSet par branche) | DAL |
| F-LAR-008 | IMPORTANT | Route logout hors middleware `client.auth` | Laravel |

### Plan de correction

1. **Corriger le bug operator precedence** dans `ProduitWeb.php`
   - Remplacer `$this->attributes['stock_calc'] ?? null !== null` par `array_key_exists('stock_calc', $this->attributes)`
   - Fichier : `site-laravel/app/Models/ProduitWeb.php:47`
   - **5 minutes, zero dependance**

2. **Ajouter des surcharges transactionnelles** a `BomNiveauDAL.GetById` et `BomFicheDAL.GetById`
   - Fichiers : `DAL/BomNiveauDAL.cs`, `DAL/BomFicheDAL.cs`, `DAL/BomProductionDAL.cs`
   - Les appeler dans le scope de la transaction de `Executer()`

3. **Envelopper les Delete() multi-check dans des transactions** (4 DAL)
   - Fichiers : `DAL/BomContexteDAL.cs`, `DAL/BomFicheDAL.cs`, `DAL/IngredientDAL.cs`, `DAL/LotDAL.cs`
   - Pattern : `conn.BeginTransaction()` avant le premier check, `tx.Commit()` apres le DELETE

4. **Securiser `DbHelper.GetConnection`**
   - Option A : try/catch dans GetConnection, dispose en cas d'erreur
   - Option B : retourner la connexion fermee, laisser l'appelant Open()
   - Fichier : `DAL/DbHelper.cs`

5. **Corriger la detection de cycle dans `BomCoutDAL`**
   - Passer le meme HashSet (pas une copie) + Remove apres retour recursif
   - Fichier : `DAL/BomCoutDAL.cs`

6. **Documenter `$fillable` / `$guarded`** dans Client.php et BomFiche.php
   - Fichiers : `site-laravel/app/Models/Client.php`, `site-laravel/app/Models/BomFiche.php`

7. **Deplacer route logout dans le middleware** `client.auth`
   - Fichier : `site-laravel/routes/web.php`

### Ordre d'execution
- Etape 1 (bug Laravel) d'abord -- 5 min, impact immediat
- Etapes 2-3 (transactions) ensuite -- meme pattern repete
- Etape 4-5 (DbHelper, cycles) -- corrections independantes
- Etapes 6-7 (documentation, route) -- faible effort

### Fichiers impactes (12)
`site-laravel/app/Models/ProduitWeb.php`, `DAL/BomNiveauDAL.cs`, `DAL/BomFicheDAL.cs`, `DAL/BomProductionDAL.cs`, `DAL/BomContexteDAL.cs`, `DAL/IngredientDAL.cs`, `DAL/LotDAL.cs`, `DAL/DbHelper.cs`, `DAL/BomCoutDAL.cs`, `site-laravel/app/Models/Client.php`, `site-laravel/app/Models/BomFiche.php`, `site-laravel/routes/web.php`

### Risque de regression
- **Eleve pour F-DAL-004** (modification du flux transactionnel de production FIFO -- tester integralement)
- Moyen pour les Delete() transactionnels (verifier que le rollback fonctionne si check echoue)
- Faible pour le bug Laravel (correction chirurgicale)
- Faible pour DbHelper (pattern defensif)

### Priorite: P0 (bug Laravel + lectures hors tx) / P1 (Delete transactions + DbHelper)

---

## Theme E : Laravel cleanup (N+1, pages erreur, images, panier)

### Contexte
Performances N+1 dans le flow panier, absence de pages d'erreur personnalisees, incoherence Cloudinary vs storage local, et logique panier dupliquee.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-LAR-007 | IMPORTANT | Pas de pages d'erreur personnalisees (404/500) | Laravel |
| F-LAR-004 | IMPORTANT | Logique panier dupliquee (View Composer + Controller) | Laravel |
| F-LAR-005 | IMPORTANT | N+1 sur `stock_disponible` dans PanierController (fallback requete unitaire) | Laravel |
| F-LAR-006 | IMPORTANT | Images `asset('storage/')` mais Cloudinary declare dans .env | Laravel |
| F-LAR-009 | MINEUR | Validation inline dans CommandeController au lieu de FormRequest | Laravel |
| F-LAR-010 | MINEUR | Validation inline dans PanierController (3 methodes) | Laravel |
| F-LAR-013 | MINEUR | BomFiche et BomStock sans `$guarded` explicite | Laravel |
| F-LAR-014 | MINEUR | Pas de `getSousTotalAttribute` dans CommandeWebLigne | Laravel |

### Plan de correction

1. **Creer les pages d'erreur** `404.blade.php` et `500.blade.php`
   - Fichier nouveau : `site-laravel/resources/views/errors/404.blade.php`
   - Fichier nouveau : `site-laravel/resources/views/errors/500.blade.php`
   - Etendre `layouts.app` pour coherence visuelle

2. **Extraire un `PanierService`** centralisant `getPanierActif()` + `getPanierCount()`
   - Fichiers : nouveau `app/Services/PanierService.php`, `app/Http/Controllers/PanierController.php`, `app/Providers/AppServiceProvider.php`
   - Remplacer la logique dupliquee dans View Composer et Controller
   - Envisager un cache session `session('panier_count')`

3. **Corriger les N+1 stock dans PanierController**
   - Utiliser `ProduitWeb::withStockDisponible()->findOrFail()` dans `ajouter()`
   - Charger `lignes.produit` avec callback scope dans `getPanierActif()`
   - Fichier : `app/Http/Controllers/PanierController.php`

4. **Clarifier la strategie images** (Cloudinary vs local)
   - Decision architecturale : garder local et supprimer les variables Cloudinary du `.env.example`, OU implementer Cloudinary
   - Fichiers : `site-laravel/.env.example`, eventuellement les vues Blade

5. **Creer FormRequests manquants** (optionnel, faible priorite)
   - `CheckoutRequest`, `LoginRequest`, eventuellement `PanierAjouterRequest`
   - Fichiers : nouveaux dans `app/Http/Requests/`

### Fichiers impactes (8+)
`site-laravel/resources/views/errors/404.blade.php` (nouveau), `site-laravel/resources/views/errors/500.blade.php` (nouveau), `site-laravel/app/Services/PanierService.php` (nouveau), `site-laravel/app/Http/Controllers/PanierController.php`, `site-laravel/app/Providers/AppServiceProvider.php`, `site-laravel/.env.example`, `site-laravel/app/Models/BomFiche.php`, `site-laravel/app/Models/CommandeWebLigne.php`

### Risque de regression
- Faible pour les pages d'erreur (ajout pur)
- Moyen pour le PanierService (refactoring de logique existante, tester le flow complet panier)
- Faible pour la strategie images (decision documentaire)

### Priorite: P0 (pages erreur) / P1 (PanierService + N+1) / P2 (FormRequests, Cloudinary)

---

## Theme F : Naming, conventions et documentation Model

### Contexte
Incoherences mineures dans le nommage des parametres SQL, ToString() manquants sur certains Models, proprietes calculees non documentees, et ENUM SQL mappes en string sans validation.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-DAL-020 | MINEUR | Nommage `@desc` vs colonne `description` (coherent entre DAL, mineur) | DAL |
| F-DAL-021 | MINEUR | Nommage `@tel` vs colonne `telephone` dans FournisseurDAL | DAL |
| F-MOD-006 | MINEUR | `Lot` sans override `ToString()` | Models |
| F-MOD-007 | MINEUR | `CommandeWebLigne` sans override `ToString()` | Models |
| F-MOD-008 | MINEUR | `VueStockGlobal` sans override `ToString()` | Models |
| F-MOD-013 | IMPORTANT | `BomFiche.CoutBatch` / `CoutUnitaire` dependance implicite a Lignes chargees | Models |
| F-MOD-014 | IMPORTANT | `BomFicheLigne.PrixUnitaireRef` potentiellement non peuplee | Models |
| F-MOD-015 | IMPORTANT | `Ingredient.EstEnAlerte` utilise `StockActuel` qui est optionnel | Models |
| F-MOD-016 | IMPORTANT | `BomProduction.QuantiteOutputBatch` -- propriete jointure non separee | Models |
| F-MOD-017 | IMPORTANT | `BomStock.StockCible` / `TotalDispoFiche` -- proprietes non-DB non documentees | Models |
| F-MOD-018 | IMPORTANT | `CommandeWebLigne.SousTotal` -- setter ouvert sur colonne GENERATED | Models |
| F-MOD-019 | IMPORTANT | `VueStockGlobal.EstEnAlerte` devrait s'appeler `EstEnRupture` | Models |
| F-MOD-020 | CRITIQUE | Pas de Model pour `activites_stocks` (table jonction M:N) -- acceptable | Models |
| F-MOD-021 | MINEUR | Pas de Model pour `clients` -- acceptable (Laravel-only) | Models |
| F-DAL-008 | MINEUR | `BomReservationDAL.Update` n'utilise pas `Bind()` | DAL |
| F-DAL-009 | MINEUR | `BomNiveauDAL.Update` n'utilise pas `Bind()` | DAL |
| F-DAL-027 | MINEUR | `BomProductionDAL.MapHeader` -- DBNull check superflu sur colonnes NOT NULL | DAL |

### Plan de correction

1. **Renommer `VueStockGlobal.EstEnAlerte` en `EstEnRupture`**
   - Fichier : `Models/VueStockGlobal.cs`
   - Impact : rechercher tous les usages dans les Forms et DAL

2. **Ajouter `ToString()` manquants** sur Lot, CommandeWebLigne, VueStockGlobal
   - Fichiers : `Models/Lot.cs`, `Models/CommandeWebLigne.cs`, `Models/VueStockGlobal.cs`

3. **Ajouter commentaires XML** sur les proprietes calculees et de jointure
   - Documenter la dependance au chargement des lignes (`BomFiche.CoutBatch`)
   - Documenter les colonnes GENERATED (`CommandeWebLigne.SousTotal`)
   - Separer les sections "jointures" / "valeurs calculees" dans les Models
   - Fichiers : `Models/BomFiche.cs`, `Models/BomFicheLigne.cs`, `Models/Ingredient.cs`, `Models/BomProduction.cs`, `Models/BomStock.cs`, `Models/CommandeWebLigne.cs`

4. **Documenter `activites_stocks` et `clients` comme volontairement sans Model**
   - Fichiers : commentaires dans `DAL/StockDAL.cs` et `DAL/CommandeWebDAL.cs`

### Fichiers impactes (12)
`Models/VueStockGlobal.cs`, `Models/Lot.cs`, `Models/CommandeWebLigne.cs`, `Models/BomFiche.cs`, `Models/BomFicheLigne.cs`, `Models/Ingredient.cs`, `Models/BomProduction.cs`, `Models/BomStock.cs`, `DAL/StockDAL.cs`, `DAL/CommandeWebDAL.cs`, `DAL/BomReservationDAL.cs`, `DAL/BomNiveauDAL.cs`

### Risque de regression
- Moyen pour le rename `EstEnAlerte` -> `EstEnRupture` (rechercher-remplacer dans les Forms)
- Nul pour les ToString() et commentaires

### Priorite: P2

---

## Theme G : Schema et migrations (index, FK, CHECK, UNIQUE)

### Contexte
Index manquants pour les requetes frequentes (FIFO, reservations), FK sans clause explicite ON DELETE, CHECK constraints absentes sur certaines colonnes numeriques, et contraintes UNIQUE manquantes sur les noms.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-SQL-005 | IMPORTANT | Index manquant sur `lots_ingredients.id_fiche_ingredient` (composite FIFO) | SQL |
| F-SQL-006 | IMPORTANT | Index manquant sur `lots_ingredients.id_stock` + FK sans ON DELETE explicite | SQL |
| F-SQL-007 | IMPORTANT | Index manquants sur `bom_reservations` (lot+actif, ctx+actif) | SQL |
| F-SQL-008 | IMPORTANT | FK `fk_lots_stock` sans clause ON DELETE/ON UPDATE | SQL |
| F-SQL-012 | MINEUR | ENUM `commandes_web.statut` : pas d'etats intermediaires (en_preparation, expediee) | SQL |
| F-SQL-013 | MINEUR | CHECK manquant sur `bom_reservations.quantite_reservee` | SQL |
| F-SQL-014 | MINEUR | CHECK manquant sur `bom_productions.quantite_produite` | SQL |
| F-SQL-015 | MINEUR | CHECK manquant sur `bom_fiches.quantite_output` (division par zero possible) | SQL |
| F-SQL-018 | MINEUR | Migrations v01-v03 absentes du repertoire (tracabilite) | SQL |
| F-SQL-019 | MINEUR | Pas de contrainte UNIQUE sur `fournisseurs.nom` | SQL |
| F-SQL-020 | MINEUR | Pas de contrainte UNIQUE composite `(nom, id_activite)` sur `bom_contextes` | SQL |

### Plan de correction

1. **Creer une migration v20** regroupant les corrections schema
   - Fichier nouveau : `sql/migration_v20_schema_hardening.sql`
   - Contenu :
     ```sql
     -- Index composites FIFO
     CREATE INDEX idx_lot_fiche_achat ON lots_ingredients (id_fiche_ingredient, date_achat);
     -- Index reservations
     CREATE INDEX idx_bomres_lot_actif ON bom_reservations (id_lot, actif);
     CREATE INDEX idx_bomres_ctx_actif ON bom_reservations (id_contexte, actif);
     -- FK explicite
     ALTER TABLE lots_ingredients DROP FOREIGN KEY fk_lots_stock;
     ALTER TABLE lots_ingredients ADD CONSTRAINT fk_lots_stock
       FOREIGN KEY (id_stock) REFERENCES stocks(id) ON DELETE RESTRICT ON UPDATE CASCADE;
     -- CHECK constraints
     ALTER TABLE bom_reservations ADD CONSTRAINT chk_bomres_qte_positive CHECK (quantite_reservee > 0);
     ALTER TABLE bom_productions ADD CONSTRAINT chk_bomprod_qte_positive CHECK (quantite_produite > 0);
     ALTER TABLE bom_fiches ADD CONSTRAINT chk_bf_output_positive CHECK (quantite_output > 0);
     -- UNIQUE constraints
     ALTER TABLE fournisseurs ADD UNIQUE KEY uk_fournisseur_nom (nom);
     ALTER TABLE bom_contextes ADD UNIQUE KEY uq_bomctx_nom_activite (nom, id_activite);
     ```

2. **Mettre a jour `create_database.sql`** avec les memes index/CHECK/UNIQUE
   - Fichier : `sql/create_database.sql`

### Fichiers impactes (2)
`sql/migration_v20_schema_hardening.sql` (nouveau), `sql/create_database.sql`

### Risque de regression
- Moyen pour les UNIQUE (verifier qu'il n'y a pas de doublons existants en base avant d'appliquer)
- Faible pour les index (ajout pur, pas de changement de comportement)
- Faible pour les CHECK (les donnees existantes devraient deja etre positives)
- Attention a la FK drop/recreate (verifier que les donnees FK sont coherentes)

### Priorite: P1 (FK + index) / P2 (CHECK + UNIQUE)

---

## Theme H : Duplication code DAL (methodes tx/non-tx)

### Contexte
Plusieurs methodes DAL existent en double : une version autonome (nouvelle connexion) et une version transactionnelle (conn+tx). Le SQL est quasi-identique. Ce pattern se retrouve dans BomStockDAL, BomProductionDAL et BomFicheDAL.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-DAL-014 | IMPORTANT | 3 SELECT identiques dans BomProductionDAL (pas de SELECT_BASE) | DAL |
| F-DAL-029 | IMPORTANT | BomStockDAL -- 4 methodes x2 (8 methodes au total) tx/non-tx | DAL |
| F-DAL-030 | IMPORTANT | BomProductionDAL.VerifierDisponibiliteLignes -- dupliquee tx/non-tx | DAL |
| F-DAL-031 | MINEUR | BomProductionDAL.GetIdNiveauDeFiche -- dupliquee tx/non-tx | DAL |
| F-DAL-003 | MINEUR | StockDAL.Delete -- double verification redondante | DAL |
| F-DAL-015 | MINEUR | BomStockDAL.GetByNiveau -- sous-requete correlee dans le SELECT | DAL |
| F-DAL-016 | MINEUR | BomStockDAL.GetLotsDispoFIFO -- HAVING vs filtrage C# (inconsistant) | DAL |
| F-DAL-023 | MINEUR | VueStockGlobalDAL -- pas de SELECT_BASE partage pour GetByContexte | DAL |

### Plan de correction

1. **Extraire `SELECT_BASE` dans BomProductionDAL**
   - Fichier : `DAL/BomProductionDAL.cs`
   - Reduire 3 blocs SELECT en 1 constante + WHERE variable

2. **Factoriser les methodes tx/non-tx dans BomStockDAL**
   - Pattern : methode privee `GetLotsDispoFIFOInternal(conn, tx, idFi)` avec conn/tx nullable
   - La version publique sans tx cree sa propre connexion et appelle l'interne
   - Fichier : `DAL/BomStockDAL.cs`
   - Estimation : ~100 lignes supprimees

3. **Factoriser les methodes tx/non-tx dans BomProductionDAL**
   - Meme pattern pour `VerifierDisponibiliteLignes` et `GetIdNiveauDeFiche`
   - Fichier : `DAL/BomProductionDAL.cs`
   - Estimation : ~80 lignes supprimees

4. **Unifier le check de StockDAL.Delete** (1 seul check au lieu de 2)
   - Fichier : `DAL/StockDAL.cs`

### Fichiers impactes (4)
`DAL/BomProductionDAL.cs`, `DAL/BomStockDAL.cs`, `DAL/StockDAL.cs`, `DAL/VueStockGlobalDAL.cs`

### Risque de regression
- Moyen (refactoring de methodes utilisees dans le flux de production FIFO -- tester integralement)
- Les tests doivent couvrir : production normale, production insuffisante, verification dispo, annulation

### Priorite: P1

---

## Theme I : UX et accessibilite Forms

### Contexte
Raccourcis clavier manquants ou menteurs, focus initial absent, TabIndex incomplet, couleurs inline non centralisees.

### Findings inclus

| ID | Severite | Description courte | Source |
|----|----------|-------------------|--------|
| F-FRM-010 | IMPORTANT | FrmEditBase ne definit pas AcceptButton/CancelButton (Enter/Escape) | Forms |
| F-FRM-011 | IMPORTANT | FrmListeBase ne gere aucun raccourci clavier (Ctrl+N menteur dans StatusBar) | Forms |
| F-FRM-012 | IMPORTANT | 162 Color.FromArgb inline non centralisees dans AppColors | Forms |
| F-FRM-013 | MINEUR | CboActivite dans FrmBomContexteEdit n'utilise pas FormHelper.SelectionnerParId | Forms |
| F-FRM-014 | MINEUR | FrmProduitWebEdit.Sauvegarder fait I/O sans try-catch adequat + return manquant | Forms |
| F-FRM-015 | MINEUR | FrmCategorieWebEdit utilise Size au lieu de ClientSize | Forms |
| F-FRM-016 | MINEUR | Pas de TabIndex dans les formulaires complexes | Forms |
| F-FRM-017 | MINEUR | FrmIngredients.CreerChip -- largeur calculee approximative (magic number) | Forms |
| F-FRM-020 | MINEUR | Absence de focus initial dans 5 formulaires Edit | Forms |
| F-FRM-021 | MINEUR | FrmBomNiveauEdit pre-remplit dans le constructeur au lieu du Load | Forms |
| F-NAV-005 | MINEUR | Guard anti-re-render absent pour BoutiqueWeb et Parametres | Navigation |
| F-NAV-006 | MINEUR | Guard anti-re-render absent pour les placeholders | Navigation |
| F-NAV-010 | MINEUR | BoutiqueWeb absent du dictionnaire de titres dans UpdateTitleBar | Navigation |

### Plan de correction

1. **Ajouter AcceptButton/CancelButton dans FrmEditBase** -- 2 lignes
   - Fichier : `Forms/FrmEditBase.cs`
   - `AcceptButton = btnEnregistrer; CancelButton = btnAnnuler;`

2. **Implementer les raccourcis dans FrmListeBase** via `ProcessCmdKey`
   - Fichier : `Forms/FrmListeBase.cs`
   - Ctrl+N, Ctrl+E, Delete, Escape

3. **Centraliser les couleurs restantes dans AppColors**
   - Ajouter `AppColors.InfoBluePale`, `AppColors.HintText`, etc.
   - Fichiers : `Forms/Shell/AppColors.cs` + ~5 fichiers Forms avec des Color.FromArgb inline

4. **Corriger FrmProduitWebEdit.Sauvegarder** -- ajouter `return` apres le warning chemin
   - Fichier : `Forms/FrmProduitWebEdit.cs`

5. **Ajouter focus initial + TabIndex** dans les formulaires Edit
   - Fichiers : `Forms/FrmBomFicheEdit.cs`, `Forms/FrmBomContexteEdit.cs`, `Forms/FrmIngredientEdit.cs`, `Forms/FrmCategorieWebEdit.cs`, `Forms/FrmProduitWebEdit.cs`

6. **Completer le guard anti-re-render** et le dictionnaire de titres
   - Fichiers : `Navigation/ScreenRouter.cs`, `Forms/FrmPrincipal.cs`

### Fichiers impactes (12+)
`Forms/FrmEditBase.cs`, `Forms/FrmListeBase.cs`, `Forms/Shell/AppColors.cs`, `Forms/FrmProduitWebEdit.cs`, `Forms/FrmBomFicheEdit.cs`, `Forms/FrmBomContexteEdit.cs`, `Forms/FrmIngredientEdit.cs`, `Forms/FrmCategorieWebEdit.cs`, `Forms/FrmProduitWebEdit.cs`, `Navigation/ScreenRouter.cs`, `Forms/FrmPrincipal.cs`, `Forms/FrmBomNiveauEdit.cs`

### Risque de regression
- Faible pour AcceptButton (ajout pur, pas de logique changee)
- Faible pour les raccourcis (ProcessCmdKey nouveau, pas de conflit)
- Faible pour les couleurs (remplacement visuel, meme rendu)

### Priorite: P0 (AcceptButton) / P2 (raccourcis, couleurs, focus)

---

## Ordre d'execution recommande

### P0 -- Corriger avant defense (effort estime : 3-4h)

| # | Theme | Action | Effort | Justification |
|---|-------|--------|--------|---------------|
| 1 | D | **F-LAR-001** : Corriger bug operator precedence `stock_calc` | 5 min | Bug logique actif en production |
| 2 | A | **F-SQL-001** : Supprimer ou regenerer `schema_complet.sql` | 15 min | Schema de reference menteur |
| 3 | A | **F-SQL-003** : Reecrire `seed_data.sql` pour schema actuel | 45 min | Fichier de seed 100% casse |
| 4 | B | **F-FRM-008/009** : Corriger les `Controls.Clear()` sans Dispose | 15 min | Fuites memoire actives |
| 5 | I | **F-FRM-010** : Ajouter AcceptButton/CancelButton dans FrmEditBase | 2 min | Enter/Escape dans tous les Edit |
| 6 | E | **F-LAR-007** : Creer pages erreur 404/500 | 30 min | UX critique pour la defense |
| 7 | D | **F-DAL-004** : Lectures hors transaction dans `Executer()` | 1h | Race condition sur production FIFO |

### P1 -- Sprint courant (effort estime : 12-16h)

| # | Theme | Action | Effort | Justification |
|---|-------|--------|--------|---------------|
| 8 | D | **F-DAL-010/011/012/013** : Transactions dans les Delete() | 2h | 4 race conditions theoriques |
| 9 | B | **F-FRM-001/002/003** : Migrer 3 forms vers FrmListeBase | 5h | ~900 lignes dupliquees eliminees |
| 10 | H | **F-DAL-014/029/030/031** : Factoriser tx/non-tx | 3h | ~200 lignes dupliquees eliminees |
| 11 | C | **F-NAV-004/008/009** : Nettoyer NavigationParams + StateChanged | 1h | Dead code dans la navigation |
| 12 | E | **F-LAR-004/005** : PanierService + fix N+1 | 1.5h | Performance Laravel |
| 13 | G | **F-SQL-005/006/007/008** : Migration v20 (index + FK explicite) | 1h | Performance et coherence schema |
| 14 | A | **F-MOD-002** : Ajouter DlcJoursReference + QualiteLabel | 1h | Colonnes DB inaccessibles |
| 15 | D | **F-DAL-018** : Securiser DbHelper.GetConnection | 30 min | Fuite connexion potentielle |

### P2 -- Backlog (effort estime : 8-10h)

| # | Theme | Action | Effort |
|---|-------|--------|--------|
| 16 | F | Renommer EstEnAlerte -> EstEnRupture + ToString manquants | 1h |
| 17 | F | Documentation XML des proprietes calculees/jointures | 2h |
| 18 | I | Raccourcis clavier FrmListeBase + centralisation couleurs | 2h |
| 19 | G | CHECK constraints + UNIQUE constraints (migration v20 suite) | 1h |
| 20 | C | Nettoyer dead code restant (MenuStrip, NavItemId, User.php) | 1h |
| 21 | E | FormRequests Laravel + strategie Cloudinary | 1.5h |
| 22 | I | Focus initial + TabIndex + guard re-render | 1h |

---

## Statistiques finales

| Metrique | Valeur |
|----------|--------|
| Findings bruts tous agents | 120 |
| Doublons identifies | 16 findings dans 8 groupes |
| Findings uniques | 104 |
| Critiques | 12 |
| Importants | 43 |
| Mineurs | 49 |
| Themes de correction | 9 (A-I) |
| Fichiers impactes (estimation totale) | ~50 fichiers uniques |
| Effort P0 estime | 3-4h |
| Effort P1 estime | 12-16h |
| Effort P2 estime | 8-10h |
| Effort total estime | 23-30h |

### Repartition par composant

| Composant | Findings uniques | Critiques | Importants | Mineurs |
|-----------|-----------------|-----------|------------|---------|
| DAL C# | 28 | 2 | 10 | 16 |
| Models C# | 19 | 3 | 8 | 8 |
| Forms C# | 22 | 3 | 10 | 9 |
| Navigation C# | 9 | 1 | 4 | 4 |
| Laravel | 13 | 2 | 6 | 5 |
| SQL/Schema | 19 | 3 | 7 | 9 |

### Ratio par type de probleme

| Type | Count |
|------|-------|
| Incoherence (DAL/Model/Schema) | 22 |
| Dead code / Orphelins | 14 |
| Redondance / Duplication | 16 |
| Pattern manque (transaction, dispose, etc.) | 18 |
| Securite | 5 |
| Performance (N+1, index) | 7 |
| Naming / Documentation | 14 |
| Accessibilite / UX | 8 |
