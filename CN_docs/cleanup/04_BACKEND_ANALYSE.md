# AGENT #4 — BACKEND DEVELOPER : ANALYSE COMPLÈTE
> **Date :** 2026-05-20
> **Codebase :** CharlesNadejda — ERP artisanal patisserie/chocolaterie
> **Perimetre :** Models (21 fichiers), DAL (19 fichiers), Services (2 fichiers), DbHelper, UnitConvertisseur
> **Regle :** Documentation-only. Aucun code modifie.

---

## TABLE DES MATIERES

1. [Synthese globale](#1-synthese-globale)
2. [Audit des Models](#2-audit-des-models)
3. [Audit des DAL — par fichier](#3-audit-des-dal--par-fichier)
4. [Deep dive BomProductionDAL.cs](#4-deep-dive-bomproductiondal)
5. [BomCoutDAL.cs — recursion et cycles](#5-bomcoutdal--recursion-et-cycles)
6. [SimulationService.cs](#6-simulationservice)
7. [Problemes transversaux](#7-problemes-transversaux)
8. [Propositions de refactoring](#8-propositions-de-refactoring)

---

## 1. Synthese globale

| Metrique | Valeur |
|----------|--------|
| Models audites | 21 / 21 |
| DAL audites | 19 / 19 (18 DAL + DbHelper) |
| Services audites | 2 / 2 |
| Utilitaires audites | UnitConvertisseur (1) |
| Problemes critiques (P0) | 2 |
| Problemes majeurs (P1) | 5 |
| Problemes mineurs (P2) | 9 |
| Ameliorations (P3) | 6 |

**Verdict global : 7.5/10** — Architecture backend solide et coherente. Les patterns Bind/Map sont appliques uniformement. Les transactions sont presentes la ou necessaire. Les deux problemes critiques (TOCTOU dans Executer, dead code) meritent attention.

---

## 2. Audit des Models

### 2.1 POCO Compliance

Tous les Models sont des POCO (Plain Old CLR Object — classe C# sans logique metier, uniquement des proprietes).

| Model | Pur POCO ? | Proprietes calculees | Observation |
|-------|-----------|---------------------|-------------|
| Activite | OUI | 0 | Propre |
| BomContexte | OUI | 0 | Liste `Niveaux` initialisee = OK |
| BomFiche | **PRESQUE** | 2 (`CoutBatch`, `CoutUnitaire`) | Calcul via `Lignes.Sum()` — acceptable car derivation triviale |
| BomFicheLigne | **PRESQUE** | 1 (`SousTotal`) | `Quantite * PrixUnitaireRef` — derivation triviale |
| BomManque | **PRESQUE** | 1 (`Manque`) | Calcule a partir de 2 props — DTO de lecture seule |
| BomNiveau | OUI | 0 | Propre |
| BomProduction | OUI | 0 | Propre |
| BomProductionLigne | **PRESQUE** | 1 (`SousTotal`) | Derivation triviale |
| BomReservation | OUI | 0 | Propre |
| BomStock | **PRESQUE** | 3 (`StockRatio`, `EstPerime`, `CoutTotal`) | Derivations triviales, lecture seule |
| CategorieWeb | OUI | 0 | `NbProduits` est un computed DAL, pas POCO-breaking |
| CommandeWeb | **PRESQUE** | 1 (`NomCompletClient`) | Concatenation — trivial |
| CommandeWebLigne | OUI | 0 | `SousTotal` est GENERATED en DB |
| Fournisseur | OUI | 0 | Propre |
| Ingredient | **PRESQUE** | 4 (`PrixParUniteBase`, `EstEnAlerte`, `StockPieces`, `StockRatio`) | Derivations de lecture, toutes utilisees dans l'UI |
| Lot | **PRESQUE** | 1 (`PrixUnitaireBase`) | Derivation triviale |
| ProduitWeb | **PRESQUE** | 1 (`EstEnStock`) | Boolean trivial |
| RapportCout + LigneCout | OUI | 0 | DTO de lecture seule, jamais persiste |
| Stock | OUI | 0 | Propre |
| Utilisateur | OUI | 0 | Propre |
| VueStockGlobal | **PRESQUE** | 3 (`EstLot`, `EstEnAlerte`, `ADesReservations`) | DTO lecture seule sur VIEW SQL |

**Conclusion Models :** Les proprietes calculees sont toutes des derivations triviales (multiplications, comparaisons, concatenations). Aucune logique metier complexe n'est dans les Models. C'est un choix pragmatique acceptable pour un projet etudiant.

### 2.2 Nullable Consistency

| Model | Probleme | Fichier:ligne | Priorite |
|-------|----------|---------------|----------|
| Ingredient | `Description` est declare dans le Model mais **jamais selectionne ni mappe dans IngredientDAL** | `Models/Ingredient.cs:10` / `DAL/IngredientDAL.cs:20-46` | P2 |
| BomFiche | `TempsPreparation` (int?) et `StockCible` (decimal?) — correctement nullable dans Model et DAL | — | OK |
| BomStock | `DateDlc` (DateTime?) — correctement nullable | — | OK |
| Lot | `DatePeremption` (DateTime?), `IdFournisseur` (int?), `NumeroLot` (string) — tous correctement geres | — | OK |
| CommandeWeb | `DateCommande` (DateTime?), `AdresseLivraison` (string) — nullable correct | — | OK |

**Detail Ingredient.Description :**
Le Model `Ingredient` a une propriete `Description` (heritee par convention) mais elle n'est ni dans le SELECT du DAL ni dans le Map(). Cela signifie que `Description` sera toujours `null`. Si la colonne `description` n'existe pas dans la table `fiches_ingredients`, la propriete est orpheline. Si elle existe, c'est un oubli de mapping.

### 2.3 Proprietes non mappees / orphelines

| Model | Propriete | Present dans DAL Map() ? | Observation |
|-------|-----------|--------------------------|-------------|
| Ingredient | `Description` | **NON** | Propriete existe mais jamais alimentee |
| Activite | Toutes | OUI | OK |
| Fournisseur | Toutes | OUI | OK |
| Stock | Toutes | OUI | OK |

---

## 3. Audit des DAL — par fichier

### 3.1 DbHelper.cs (17 lignes)

| # | Observation | Priorite |
|---|-------------|----------|
| 1 | Connection string lue depuis `ConfigurationManager` — correct | OK |
| 2 | Aucun pool de connexion explicite — MySqlConnector gere le pool en interne, acceptable | OK |
| 3 | Pas de `try/catch` autour de `conn.Open()` — si la DB est down, exception non-wrappee. Acceptable pour ERP desktop. | P3 |

### 3.2 ActiviteDAL.cs (145 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `Desactiver()` reuse le meme `cmd` avec `cmd.Parameters.Clear()` — correct mais fragile si refactore | L97-128 | P3 |
| 2 | Pattern Bind/Map coherent | — | OK |
| 3 | Delete sans verification prealable — cascade FK en DB | L83-92 | OK |

### 3.3 BomContexteDAL.cs (165 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `InsertAvecNiveaux()` : transaction correcte avec `Commit/Rollback` | L68-117 | OK |
| 2 | Boucle `for` avec creation de `MySqlCommand` par iteration — N inserts dans la transaction. Acceptable car le nombre de niveaux est tres petit (2-5 max) | L97-109 | P3 |

### 3.4 BomCoutDAL.cs (237 lignes)

Voir [section 5 dediee](#5-bomcoutdal--recursion-et-cycles).

### 3.5 BomFicheDAL.cs (294 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `SELECT_HEADER` : 4 JOINs (bom_fiches -> niveaux -> contextes -> activites) — correct et necessaire | L10-19 | OK |
| 2 | `Update()` : delete-then-insert des lignes dans une transaction — pattern replace correct | L129-161 | OK |
| 3 | `Duplicate()` : appelle `NomExiste()` en boucle `while` pour generer un nom unique — acceptable car rarissime en pratique | L206-231 | P3 |
| 4 | `InsertLignes()` : marque `internal` — seul appelant = BomFicheDAL. Pourrait etre `private` | L235 | P3 |
| 5 | Transaction dans `Insert()` et `Update()` — correct | — | OK |

### 3.6 BomFicheLigneDAL.cs (103 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `GetByFiche()` : COALESCE pour resoudre ingredient vs fiche en une seule requete — pattern elegant | L22-37 | OK |
| 2 | `GetFichesUtilisant()` et `GetFichesConsommant()` : requetes simples, correctes | — | OK |
| 3 | Pas de methode Insert/Update/Delete autonome — la gestion des lignes passe toujours par BomFicheDAL | — | OK (conception volontaire) |

### 3.7 BomNiveauDAL.cs (148 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `Insert()` : sous-select pour calculer `MAX(ordre)+1` — correct pour l'auto-increment de l'ordre | L62-66 | OK |
| 2 | `Delete()` : verification que c'est le dernier niveau (pas de suppression intermediaire) — logique metier correcte | L90-112 | OK |
| 3 | `Delete()` : reuse du meme `cmd` avec `Parameters.Clear()` — meme pattern que ActiviteDAL | L107-110 | P3 |

### 3.8 BomProductionDAL.cs (526 lignes)

Voir [section 4 dediee](#4-deep-dive-bomproductiondal).

### 3.9 BomReservationDAL.cs (128 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | CRUD simple et correct | — | OK |
| 2 | `Liberer()` et `LibererToutContexte()` : soft delete (`actif = 0`) — correct | L83-104 | OK |
| 3 | Pas de verification de coherence lors de `Insert()` (peut reserver plus que le stock dispo). La validation est faite cote Form | L52-64 | P2 |

### 3.10 BomStockDAL.cs (170 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `GetByNiveau()` : sous-requete correlees pour `total_dispo_fiche` — potentiellement lent sur beaucoup de lignes bom_stocks | L23-26 | P2 |
| 2 | `GetDisponibleIngredient()` : sous-requete pour deduire les reservations — correct | L67-85 | OK |
| 3 | `GetLotsDispoFIFO()` : filtre `dispo > 0` cote C# apres SELECT — pourrait etre un `HAVING dispo_nette > 0` cote SQL pour reduire le transfert | L114-115 | P2 |
| 4 | Pas de methode Insert/Update/Delete directe — les insertions passent par BomProductionDAL.Executer() | — | OK (conception volontaire) |

### 3.11 CategorieWebDAL.cs (132 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `GetAll()` : `SELECT c.*` au lieu de colonnes explicites — fragile si la table change | L19 | P2 |
| 2 | Pattern Bind/Map coherent | — | OK |
| 3 | `Delete()` : s'appuie sur FK SET NULL en DB pour les produits lies — correct mais non documente dans le code (commentaire present) | L99-100 | OK |

### 3.12 CommandeWebDAL.cs (132 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | DAL en **lecture seule** (pas d'Insert/Update/Delete) — correct, les commandes viennent de Laravel | — | OK |
| 2 | `GetById()` : charge header + lignes en 2 requetes sur la meme connexion — correct, evite le N+1 | L54-88 | OK |
| 3 | Sous-requete correlees dans `SELECT_BASE` pour `nb_articles` — acceptable car rarement plus de 50 commandes | L20 | P3 |

### 3.13 FournisseurDAL.cs (92 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | CRUD minimal et correct | — | OK |
| 2 | Pas de `GetById()` — si un form en a besoin, il devra faire un GetAll() + filtre. Acceptable si jamais besoin unitaire | — | P3 |
| 3 | `Insert()` ne retourne pas l'id insere (void) — incoherent avec les autres DAL qui retournent `int` | L35-45 | P2 |

### 3.14 IngredientDAL.cs (153 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `GetAll()` : pas de `GetById()` — meme observation que FournisseurDAL | — | P3 |
| 2 | `GetAll()` : `GROUP BY fi.id` sans colonnes explicites — MySQL le tolere en mode non-strict, mais pas standard SQL. Fonctionne car fi.id est la PK | L46 | P2 |
| 3 | `Insert()` ne retourne pas l'id insere | L66-83 | P2 |
| 4 | Model Ingredient a une propriete `Description` jamais mappee (voir section 2.2) | — | P2 |

### 3.15 LotDAL.cs (197 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | 3 methodes de SELECT partagent le meme SELECT SQL — non factorise en constante comme BomFicheDAL fait | L17-63, L77-88 | P2 |
| 2 | `Update()` : `GREATEST(0, @qteInit - (quantite_initiale - quantite_disponible))` — formule intelligente pour recalculer le disponible. Correcte | L131 | OK |
| 3 | `Bind()` : `DateAchat.ToString("yyyy-MM-dd")` passe une string au lieu de DateTime pour MySQL — fonctionne mais pourrait casser sur un format de date systeme different | L164 | P1 |
| 4 | INSERT met `quantite_disponible = @qteInit` (meme valeur que `quantite_initiale`) — correct pour un nouveau lot | L108 | OK |

### 3.16 ProduitWebDAL.cs (213 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `SELECT_BASE` avec `LEFT JOIN bom_stocks` pour stock_disponible — correct | L10-21 | OK |
| 2 | `GetFichesNonPubliees()` : retourne un `BomFiche` partiellement initialise (4 props sur 15+) — pourrait utiliser un DTO leger au lieu de remplir un BomFiche incomplet | L74-97 | P2 |
| 3 | `PeutSupprimer()` avant `Delete()` — bonne pratique, verification cote DAL | L158-169 | OK |
| 4 | `Update()` ne met pas a jour `id_bom_fiche` — delibere (une fois lie, pas de changement de fiche) | L125-137 | OK |

### 3.17 StockDAL.cs (191 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `Delete()` : 2 verifications prealables (fiches + lots) dans le meme `cmd` avec `Parameters.Clear()` | L97-127 | OK |
| 2 | `LierActivite()` : `INSERT IGNORE` — correct pour la jonction M:N | L135 | OK |
| 3 | `GetActivitesLiees()` : methode utilitaire clean | L162-174 | OK |

### 3.18 UtilisateurDAL.cs (42 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | `Authenticate()` : utilise BCrypt pour la comparaison de mot de passe — correct et securise | L27 | OK |
| 2 | Filtre `role = 'admin'` hardcode dans le WHERE — si d'autres roles doivent se connecter a l'ERP, a revoir | L16 | P3 |
| 3 | Pas de `GetById()`, `GetAll()`, ni de CRUD — DAL minimal pour l'authentification uniquement | — | OK |

### 3.19 VueStockGlobalDAL.cs (124 lignes)

| # | Observation | Fichier:ligne | Priorite |
|---|-------------|---------------|----------|
| 1 | Methode `Execute()` privee avec params tuples — pattern factorisation elegant | L72-86 | OK |
| 2 | Lecture seule sur VIEW — correct, les modifications passent par LotDAL et BomStockDAL | — | OK |
| 3 | `GetByContexte()` : JOIN `bom_reservations` pour filtrer les lots reserves — correct | L50-61 | OK |

---

## 4. Deep dive BomProductionDAL

### 4.1 Structure et responsabilites

| Methode | Lignes | Responsabilite |
|---------|--------|----------------|
| `GetByNiveau()` | 15-38 | SELECT productions par niveau |
| `GetRecentByActivite()` | 40-65 | SELECT recentes par activite |
| `GetDuJourByActivite()` | 73-97 | SELECT du jour par activite |
| `VerifierDisponibilite()` | 107-166 | Verification stock avant production |
| `Simuler()` | 175-223 | Simulation complete (toutes lignes) |
| `Executer()` | 235-344 | Production atomique (transaction) |
| `ConsumeStock()` | 353-448 | Consommation FIFO |
| `InsertLigne()` | 450-471 | Insertion tracabilite |
| `GetIdNiveauDeFiche()` | 479-489 | Lookup id_niveau d'une fiche |
| `GetIdNiveauPrecedent()` | 495-508 | Lookup niveau d'ordre N-1 |
| `MapHeader()` | 510-524 | Mapping reader -> Model |

### 4.2 Problemes identifies

| # | Probleme | Fichier:ligne | Severite | Detail |
|---|---------|---------------|----------|--------|
| **C1** | **TOCTOU (Time-Of-Check Time-Of-Use)** | L244 | **P0** | `VerifierDisponibilite()` a la L244 ouvre sa **propre connexion** (via DbHelper.GetConnection()) au lieu d'utiliser la connexion+transaction de `Executer()`. La verification et la consommation ne sont donc **pas atomiques**. Malgre le commentaire TICKET-01, la race condition existe toujours : entre le check (connexion A, hors transaction) et la consommation (connexion B, dans la transaction), un autre processus pourrait consommer le stock. |
| **C2** | **Code mort : `GetIdNiveauPrecedent()`** | L495-508 | **P1** | Cette methode n'est appelee **nulle part** dans le codebase. Elle semble etre un vestige de l'ancienne logique N-1 stricte, remplacee par `GetIdNiveauDeFiche()`. |
| C3 | Duplication VerifierDisponibilite / Simuler | L107-223 | P1 | Les deux methodes ont une logique presque identique (boucle sur fiche.Lignes, conversion d'unites, appel GetIdNiveauDeFiche). La seule difference : VerifierDisponibilite retourne uniquement les penuries, Simuler retourne tout. Extraction possible. |
| C4 | `Executer()` appelle `BomNiveauDAL.GetById()` et `BomFicheDAL.GetById()` **3 fois chacun** | L110-111, L178-179, L252-253 | P1 | Pour une meme execution de `Executer()`, `VerifierDisponibilite()` charge niveau+fiche (L110-111), puis `Executer()` les recharge apres la verification (L252-253). C'est 2 aller-retours DB inutiles (4 SELECT de trop). |
| C5 | `GetIdNiveauDeFiche()` ouvre une connexion separee a chaque appel | L479-489 | P2 | Appele 1 fois par ligne de fiche de type "fiche" dans `VerifierDisponibilite()`, `Simuler()` et `ConsumeStock()`. Pour une fiche avec 3 sous-fiches, c'est 3 connexions supplementaires. |
| C6 | Guard `restant > 0.0001m` au lieu de `restant > 0m` | L408, L443 | P2 | Tolerance d'arrondi intentionnelle. Correct pour eviter les erreurs de precision decimale, mais la valeur 0.0001 est un magic number — devrait etre une constante nommee. |

### 4.3 Complexite cyclomatique estimee

| Methode | CC estimee | Observation |
|---------|-----------|-------------|
| `VerifierDisponibilite()` | 8 | Boucle foreach + 2 branches (ingredient/fiche) + conditions |
| `Simuler()` | 7 | Quasi-identique a VerifierDisponibilite |
| `ConsumeStock()` | 10 | 2 branches principales + 2 boucles foreach + guards |
| `Executer()` | 6 | Lineaire avec transaction |
| `GetByNiveau()` | 2 | Simple |

Aucune methode ne depasse un seuil critique (>15). La complexite est raisonnable.

### 4.4 Logique FIFO — Correctness

La logique FIFO est **correcte** :

1. `BomStockDAL.GetLotsDispoFIFO()` trie par `date_achat ASC` — le plus ancien lot est consomme en premier
2. `BomStockDAL.GetBomStocksFIFO()` trie par `date_production ASC` — idem pour les produits intermediaires
3. `ConsumeStock()` itere dans l'ordre FIFO et prend `Math.Min(restant, dispo)` — correct
4. Les reservations sont deduites du stock disponible avant la consommation FIFO
5. Le guard final (`restant > 0.0001m`) protege contre l'epuisement total

**Risque identifie :** En environnement multi-utilisateur, si deux productions concurrentes consomment le meme lot, le `UPDATE quantite_disponible = quantite_disponible - @pris` peut passer sous zero si la verification hors-transaction a valide pour les deux. C'est le bug TOCTOU (C1 ci-dessus).

### 4.5 Avis sur extraction en ProductionService

**Recommandation : OUI, extraction partielle justifiee.**

| Ce qui devrait rester dans BomProductionDAL | Ce qui devrait etre extrait |
|---------------------------------------------|----------------------------|
| `GetByNiveau()`, `GetRecentByActivite()`, `GetDuJourByActivite()` (lecture pure) | `VerifierDisponibilite()` → `ProductionService` |
| `MapHeader()` | `Simuler()` → `ProductionService` (deja un wrapper dans SimulationService) |
| `InsertLigne()`, `GetIdNiveauDeFiche()` (helpers DB) | `Executer()` → `ProductionService` (orchestration) |
| | `ConsumeStock()` → `ProductionService` (logique metier FIFO) |

**Argument :** BomProductionDAL melange actuellement 2 responsabilites : acces aux donnees (SELECT/INSERT) et logique metier (FIFO, verification de stock, regles de production). Le principe SRP (Single Responsibility Principle — une classe = une responsabilite) suggere de separer les deux.

**Effort :** MEDIUM (1-2h). Les methodes sont bien decoupees, l'extraction est mecanique.

---

## 5. BomCoutDAL — recursion et cycles

### 5.1 Detection de cycles

```csharp
// Ligne 60-63
if (!fichesVisitees.Add(idFiche))
    throw new InvalidOperationException(
        $"Cycle detecte dans les fiches BOM : la fiche (id={idFiche}) reference " +
        "l'une de ses propres dependances.");
```

| # | Observation | Severite |
|---|-------------|----------|
| 1 | `HashSet<int>` pour la detection — **correct et suffisant** | OK |
| 2 | `fichesVisitees.Add()` retourne `false` si deja present — utilisation elegante du retour de `Add()` | OK |
| 3 | Exception claire avec l'id de la fiche fautive | OK |

### 5.2 Probleme : copie du HashSet a chaque branche

```csharp
// Ligne 162-163
RapportCout detail = CalculerCout(ligne.IdInputFiche.Value, nBatchesSrc,
                                   new HashSet<int>(fichesVisitees));
```

| # | Probleme | Fichier:ligne | Severite |
|---|---------|---------------|----------|
| **R1** | **Copie du HashSet a chaque appel recursif** | L163 | **P1** |

**Detail :** A chaque branche "fiche", un **nouveau HashSet** est cree par copie. Cela signifie que les cycles ne sont detectes que sur le **chemin actuel**, pas sur l'ensemble de l'arbre. C'est semantiquement correct (un meme sous-composant peut etre utilise dans 2 branches differentes sans que ce soit un cycle), mais cela cree `O(N)` copies ou N = nombre de fiches dans l'arbre.

**Performance sur arbres profonds :**

| Profondeur | Nb fiches | Nb copies HashSet | Impact |
|------------|-----------|-------------------|--------|
| 3 niveaux | ~10 | ~10 | Negligeable |
| 5 niveaux | ~30 | ~30 | Negligeable |
| 10 niveaux | ~100+ | ~100+ | Commence a sentir, mais reste OK |

**Conclusion :** Pour un ERP artisanal, les arbres de fiches ont rarement plus de 5 niveaux. La copie du HashSet est un non-probleme en pratique. Si le systeme devait scaler (>100 fiches imbriquees), passer le meme HashSet avec `Remove()` apres le retour serait plus efficace, mais au prix de la clarte.

### 5.3 Appels DB dans la recursion

| Appel | Par iteration | Connexion |
|-------|---------------|-----------|
| `BomFicheDAL.GetById(avecLignes: true)` | 1 par fiche | Nouvelle connexion |
| `BomFicheDAL.GetById(avecLignes: false)` | 1 par ligne "fiche" | Nouvelle connexion |
| `GetPrixMoyenIngredient()` | 1 par ligne "ingredient" | Nouvelle connexion |

Pour un arbre de 10 fiches avec 5 lignes chacune : ~25 connexions DB ouvertes/fermees. Acceptable pour un calcul ponctuel declenche par l'utilisateur, mais pourrait etre optimise avec un cache in-memory si besoin.

---

## 6. SimulationService

### 6.1 Usage

| Question | Reponse |
|----------|---------|
| SimulationService est-il utilise ? | **NON** — aucun appel a `SimulationService.SimulerAsync()` dans le codebase |
| Les Forms appellent-ils directement BomProductionDAL.Simuler() ? | **OUI** — `FrmBomProductionSimulation.cs:188` et `FrmPrincipal.Production.cs:533` |
| Le Task.Run() est-il justifie ? | **EN THEORIE OUI** — Simuler() fait des appels DB multiples qui bloquent le thread UI. MAIS puisque personne n'appelle SimulationService, c'est du code mort. |

### 6.2 Problemes

| # | Probleme | Severite |
|---|---------|----------|
| **S1** | **Code mort** — SimulationService n'est utilise par aucun Form | **P0** |
| S2 | SimulationResultat duplique BomManque (memes proprietes + 2 extras `EnPenurie`/`Resultat`) | P2 |
| S3 | Les 2 Forms qui appellent BomProductionDAL.Simuler() le font de facon **synchrone** sur le thread UI. Si la DB est lente, l'UI gele. | P1 |

### 6.3 Recommandation

**Option A (pragmatique) :** Supprimer SimulationService et SimulationResultat. Les Forms appellent deja BomProductionDAL.Simuler() directement. Si le besoin d'async revient, ajouter `Task.Run()` directement dans le Form.

**Option B (propre) :** Faire utiliser SimulationService par les 2 Forms concernes. Ajouter `EnPenurie` et `Resultat` directement dans BomManque (proprietes calculees). Supprimer SimulationResultat.

**Effort :** QUICK (<30 min) pour les deux options.

---

## 7. Problemes transversaux

### 7.1 Pattern Bind/Map — Coherence

| DAL | Bind() | Map() | Coherent ? |
|-----|--------|-------|-----------|
| ActiviteDAL | OUI | OUI | OUI |
| BomContexteDAL | OUI | OUI | OUI |
| BomFicheDAL | OUI (`BindHeader`) | OUI (`MapHeader`) | OUI |
| BomFicheLigneDAL | NON (pas de Bind, pas de CRUD propre) | OUI | OUI (by design) |
| BomNiveauDAL | OUI | OUI | OUI |
| BomProductionDAL | NON (inserts inline) | OUI (`MapHeader`) | Acceptable — les inserts sont dans une transaction complexe |
| BomReservationDAL | OUI | OUI | OUI |
| BomStockDAL | NON (pas de CRUD direct) | OUI | OUI (by design) |
| CategorieWebDAL | OUI | OUI | OUI |
| CommandeWebDAL | NON (lecture seule) | OUI (2 Map) | OUI |
| FournisseurDAL | OUI | OUI | OUI |
| IngredientDAL | OUI | OUI | OUI |
| LotDAL | OUI | OUI | OUI |
| ProduitWebDAL | OUI | OUI | OUI |
| StockDAL | OUI | OUI | OUI |
| UtilisateurDAL | NON (1 seule methode) | Inline | Acceptable |
| VueStockGlobalDAL | NON (lecture seule) | OUI | OUI |

**Conclusion :** Le pattern Bind/Map est applique de maniere tres coherente dans tout le codebase. Les exceptions sont justifiees par la nature du DAL (lecture seule, pas de CRUD propre, etc.).

### 7.2 Gestion de DBNull.Value

Toutes les DAL gerent correctement les valeurs NULL avec le pattern :
```csharp
// Bind
cmd.Parameters.AddWithValue("@param", value ?? (object)DBNull.Value);
// Map
r["col"] == DBNull.Value ? null : r["col"].ToString()
```

Aucune omission detectee.

### 7.3 Transactions

| DAL | Operations multi-tables ? | Transaction ? | Verdict |
|-----|--------------------------|---------------|---------|
| BomContexteDAL.InsertAvecNiveaux | OUI | OUI | OK |
| BomFicheDAL.Insert | OUI | OUI | OK |
| BomFicheDAL.Update | OUI | OUI | OK |
| BomProductionDAL.Executer | OUI | OUI | OK |
| StockDAL.Delete (verif + delete) | Verif + action | NON mais atomique (meme cmd, 1 seule table finale) | Acceptable |
| ActiviteDAL.Desactiver (verif + update) | Verif + action | NON | P2 — risque theorique si suppression concurrente entre la verif et l'update |

### 7.4 Index implicites recommandes

| Table | Colonne(s) | Requete concernee | Justification |
|-------|------------|-------------------|---------------|
| `bom_stocks` | `(id_fiche, quantite_disponible)` | `BomStockDAL.GetDisponible()`, sous-requete `total_dispo_fiche` | Frequemment utilise pour calcul stock |
| `lots_ingredients` | `(id_fiche_ingredient, date_achat)` | `BomStockDAL.GetLotsDispoFIFO()` | Tri FIFO |
| `bom_reservations` | `(id_lot, actif)` | `BomReservationDAL.GetTotalReservePourLot()`, sous-requetes dans BomStockDAL | Deduction des reservations |
| `bom_productions` | `(id_niveau, date_production)` | `BomProductionDAL.GetByNiveau()` | Tri chronologique |

**Note :** Les PK et FK sont normalement indexes par MySQL. Verifier avec `SHOW INDEX FROM <table>` si les index composites ci-dessus existent.

### 7.5 Securite

| # | Observation | Verdict |
|---|-------------|---------|
| 1 | Toutes les requetes utilisent des parametres (`@param`) — **aucune concatenation SQL** | OK |
| 2 | BCrypt pour les mots de passe (UtilisateurDAL) | OK |
| 3 | Connection string dans `App.config` — acceptable pour un ERP desktop | OK |
| 4 | Pas de validation d'entrees dans les DAL (faite cote Forms) | Acceptable pour cette architecture |

---

## 8. Propositions de refactoring

### 8.1 Tableau consolide par priorite

| # | Probleme | Fichier(s) | Priorite | Effort | Proposition |
|---|---------|------------|----------|--------|-------------|
| 1 | TOCTOU dans BomProductionDAL.Executer() | BomProductionDAL.cs:244 | **P0** | MEDIUM | Passer la connexion+transaction a VerifierDisponibilite() au lieu de la laisser ouvrir sa propre connexion |
| 2 | SimulationService + SimulationResultat = code mort | Services/ | **P0** | QUICK | Supprimer ou integrer dans les Forms existants |
| 3 | GetIdNiveauPrecedent() = dead code | BomProductionDAL.cs:495-508 | P1 | QUICK | Supprimer |
| 4 | LotDAL.Bind() passe DateTime comme string | LotDAL.cs:164-165 | P1 | QUICK | Passer directement l'objet DateTime au lieu de `.ToString()` |
| 5 | Duplication VerifierDisponibilite/Simuler | BomProductionDAL.cs:107-223 | P1 | MEDIUM | Extraire une methode commune parametree |
| 6 | Forms appellent Simuler() de maniere synchrone | FrmBomProductionSimulation, FrmPrincipal.Production | P1 | QUICK | Wrapper avec `await Task.Run()` |
| 7 | Executer() charge niveau+fiche 2x (dont 1 dans VerifierDisponibilite) | BomProductionDAL.cs:110-111,252-253 | P1 | MEDIUM | Passer les objets charges en parametre au lieu de les recharger |
| 8 | IngredientDAL.Insert() ne retourne pas l'id | IngredientDAL.cs:66 | P2 | QUICK | Changer `void` en `int`, retourner `cmd.LastInsertedId` |
| 9 | FournisseurDAL.Insert() ne retourne pas l'id | FournisseurDAL.cs:35 | P2 | QUICK | Idem |
| 10 | Ingredient.Description jamais mappee | Models/Ingredient.cs:10, DAL/IngredientDAL.cs | P2 | QUICK | Verifier si la colonne existe en DB. Si oui, mapper. Si non, supprimer la propriete. |
| 11 | CategorieWebDAL.GetAll() utilise SELECT * | CategorieWebDAL.cs:19 | P2 | QUICK | Lister les colonnes explicitement |
| 12 | BomStockDAL.GetLotsDispoFIFO() filtre dispo > 0 cote C# | BomStockDAL.cs:114-115 | P2 | QUICK | Ajouter `HAVING dispo_nette > 0` cote SQL |
| 13 | LotDAL : 3 SELECT quasi-identiques non factorises | LotDAL.cs:17-88 | P2 | QUICK | Extraire une constante `SELECT_BASE` comme BomFicheDAL |
| 14 | BomReservationDAL.Insert() sans validation de stock | BomReservationDAL.cs:52-64 | P2 | MEDIUM | Ajouter une verification que `quantite_reservee <= dispo` |
| 15 | BomStockDAL.GetByNiveau() : sous-requete correlee potentiellement lente | BomStockDAL.cs:23-26 | P2 | MEDIUM | Utiliser une window function ou GROUP BY + JOIN |
| 16 | ProduitWebDAL.GetFichesNonPubliees() retourne BomFiche partiellement rempli | ProduitWebDAL.cs:74-97 | P2 | QUICK | Creer un DTO leger ou documenter le contrat |
| 17 | ActiviteDAL.Desactiver() : verif + update sans transaction | ActiviteDAL.cs:97-128 | P2 | QUICK | Encapsuler dans une transaction |
| 18 | Magic number 0.0001m dans ConsumeStock | BomProductionDAL.cs:408,443 | P2 | QUICK | Extraire en constante `const decimal TOLERANCE_ARRONDI = 0.0001m` |
| 19 | UnitConvertisseur.cs dans le dossier Forms/ au lieu de Helpers/ ou Utils/ | Forms/UnitConvertisseur.cs | P3 | QUICK | Deplacer dans un dossier plus logique |
| 20 | Extraction BomProductionDAL -> ProductionService | BomProductionDAL.cs | P3 | MEDIUM | Separer lecture (DAL) et logique metier (Service) |
| 21 | FournisseurDAL : pas de GetById() | FournisseurDAL.cs | P3 | QUICK | Ajouter si besoin |
| 22 | IngredientDAL : pas de GetById() | IngredientDAL.cs | P3 | QUICK | Ajouter si besoin |

### 8.2 Plan d'action recommande (par sprint)

**Sprint 1 — Corrections critiques (P0 + P1 rapides) :**
1. Corriger le TOCTOU dans Executer() — passer connexion+transaction a VerifierDisponibilite
2. Supprimer SimulationService + SimulationResultat (code mort)
3. Supprimer GetIdNiveauPrecedent() (dead code)
4. Corriger LotDAL.Bind() : passer DateTime au lieu de string
5. Ajouter magic number comme constante

**Sprint 2 — Ameliorations de qualite (P1 + P2) :**
1. Factoriser VerifierDisponibilite/Simuler en methode commune
2. Eviter double chargement niveau/fiche dans Executer
3. Ajouter retour `int` sur Insert() de IngredientDAL et FournisseurDAL
4. Clarifier Ingredient.Description (verifier DB)
5. Factoriser SELECT_BASE dans LotDAL

**Sprint 3 — Polish (P2 + P3) :**
1. HAVING dans GetLotsDispoFIFO
2. Transaction dans ActiviteDAL.Desactiver
3. Deplacer UnitConvertisseur
4. Ajouter GetById manquants si necessaire

---

## Annexe — Inventaire des methodes par DAL

| DAL | GetAll | GetById | Insert | Update | Delete | Autres |
|-----|--------|---------|--------|--------|--------|--------|
| ActiviteDAL | OUI | OUI | OUI (int) | OUI | OUI | NomExiste, Desactiver |
| BomContexteDAL | OUI | OUI | OUI (int) | OUI | OUI | NomExiste, InsertAvecNiveaux |
| BomCoutDAL | — | — | — | — | — | CalculerCout (recursif) |
| BomFicheDAL | OUI | OUI | OUI (int) | OUI | OUI | NomExiste, Duplicate, GetCountsByContexte, GetByNiveau |
| BomFicheLigneDAL | — | — | — | — | — | GetByFiche, GetFichesUtilisant, GetFichesConsommant |
| BomNiveauDAL | — | OUI | OUI (int) | OUI | OUI | GetByContexte, GetOrdreMax, GetByContexteEtOrdre |
| BomProductionDAL | — | — | — | — | — | GetByNiveau, GetRecentByActivite, GetDuJourByActivite, VerifierDisponibilite, Simuler, Executer |
| BomReservationDAL | — | — | OUI (int) | OUI | — | GetByContexte, GetTotalReservePourLot, Liberer, LibererToutContexte |
| BomStockDAL | — | — | — | — | — | GetByNiveau, GetDisponible, GetDisponibleIngredient, GetLotsDispoFIFO, GetBomStocksFIFO |
| CategorieWebDAL | OUI | OUI | OUI (int) | OUI | OUI | NomExiste |
| CommandeWebDAL | OUI | OUI | — | — | — | GetCountByStatut (lecture seule) |
| FournisseurDAL | OUI | **NON** | OUI (**void**) | OUI | OUI | NomExiste |
| IngredientDAL | OUI | **NON** | OUI (**void**) | OUI | OUI | NomExiste |
| LotDAL | OUI | OUI | OUI (void) | OUI | OUI | GetByFicheIngredient |
| ProduitWebDAL | OUI | OUI | OUI (int) | OUI | OUI | GetEnVente, GetFichesNonPubliees, ToggleEnVente, PeutSupprimer |
| StockDAL | OUI | OUI | OUI (int) | OUI | OUI | NomExiste, GetByActivite, LierActivite, DelierActivite, GetActivitesLiees |
| UtilisateurDAL | — | — | — | — | — | Authenticate |
| VueStockGlobalDAL | OUI | — | — | — | — | GetByActivite, GetByContexte, GetByNiveau (lecture VIEW) |
