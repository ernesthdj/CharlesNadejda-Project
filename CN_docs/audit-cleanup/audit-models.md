# Audit Models -- ArtisaStock
> Date: 2026-06-11 | Agent: Models Auditor (#2)
> Branche: `feat/refactoring-sprints-p0-p3`
> Reference schema: `sql/schema_complet.sql` (post migration v18) + `sql/create_database.sql` (post v19)

---

## Resume

**21 fichiers audites** -- 21 findings (3 critique, 9 important, 9 mineur)

| Severite | Count |
|----------|-------|
| CRITIQUE | 3 |
| IMPORTANT | 9 |
| MINEUR | 9 |

---

## Inventaire des fichiers

### Entites persistees (mappent une table DB)
| Fichier | Table DB |
|---------|----------|
| `Activite.cs` | `activites` |
| `Stock.cs` | `stocks` |
| `Fournisseur.cs` | `fournisseurs` |
| `Ingredient.cs` | `fiches_ingredients` |
| `Lot.cs` | `lots_ingredients` |
| `BomContexte.cs` | `bom_contextes` |
| `BomNiveau.cs` | `bom_niveaux` |
| `BomFiche.cs` | `bom_fiches` |
| `BomFicheLigne.cs` | `bom_fiches_lignes` |
| `BomProduction.cs` | `bom_productions` |
| `BomProductionLigne.cs` | `bom_productions_lignes` |
| `BomStock.cs` | `bom_stocks` |
| `BomReservation.cs` | `bom_reservations` |
| `Utilisateur.cs` | `utilisateurs` |
| `CategorieWeb.cs` | `categories_web` |
| `ProduitWeb.cs` | `produits_web` |
| `CommandeWeb.cs` | `commandes_web` |
| `CommandeWebLigne.cs` | `commandes_web_lignes` |

### Vue SQL (lecture seule)
| Fichier | Source |
|---------|--------|
| `VueStockGlobal.cs` | VIEW `vue_stock_global` |

### DTOs purs (non persistes)
| Fichier | Usage |
|---------|-------|
| `RapportCout.cs` + `LigneCout` | Retour de `BomCoutDAL.CalculerCout()` -- calcul en memoire |
| `BomManque.cs` | Retour de `BomProductionDAL.VerifierDisponibilite()` -- calcul en memoire |

---

## Findings

### [F-MOD-001] Ingredient: schema_complet.sql desynchronise avec create_database.sql (nb_par_lot absent)
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/schema_complet.sql:65-86`
- **Type:** Incoherence
- **Description:** Le fichier `schema_complet.sql` (exporte le 2026-05-27, post migration v18) ne contient pas la colonne `nb_par_lot` ajoutee en migration v19. Le fichier `create_database.sql` (consolide v4.0) la contient bien. Le Model `Ingredient.cs` et le DAL la referencent correctement. La reference de schema est obsolete.
- **Impact:** Risque de confusion pour tout auditeur se basant sur `schema_complet.sql`. Aucun bug runtime car le DAL et le create_database.sql sont synchronises.
- **Suggestion:** Regenerer `schema_complet.sql` apres chaque migration. Ajouter un commentaire de version dans le header.

---

### [F-MOD-002] Ingredient: colonnes DB `dlc_jours_reference` et `qualite_label` absentes du Model
- **Severite:** CRITIQUE
- **Fichier(s):** `app-csharp/.../Models/Ingredient.cs`
- **Type:** Incoherence
- **Description:** La table `fiches_ingredients` contient deux colonnes presentes dans le schema mais absentes du Model C# :
  - `dlc_jours_reference INT DEFAULT NULL` -- nombre de jours de DLC reference
  - `qualite_label VARCHAR(100) DEFAULT NULL` -- label qualite (Bio, AOP, etc.)

  Le Model ne declare pas `DlcJoursReference` ni `QualiteLabel`. Le DAL (IngredientDAL) ne les lit pas en SELECT, ne les ecrit pas en INSERT/UPDATE. Ces colonnes ne sont ni lues ni ecrites cote C#.
- **Impact:** Donnees DB inaccessibles depuis l'ERP. Si un utilisateur entre une valeur via phpMyAdmin ou Laravel, elle ne sera jamais visible dans WinForms. Perte de fonctionnalite metier (la DLC reference est utile pour pre-calculer les dates de peremption a la creation de lots).
- **Suggestion:** Ajouter `public int? DlcJoursReference { get; set; }` et `public string QualiteLabel { get; set; }` au Model. Mettre a jour les 4 points du DAL (SELECT, INSERT, UPDATE, Map).

---

### [F-MOD-003] Ingredient: colonne `date_creation` absente du Model
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/Ingredient.cs`
- **Type:** Incoherence
- **Description:** La table `fiches_ingredients` a une colonne `date_creation DATETIME DEFAULT CURRENT_TIMESTAMP`. Le Model `Ingredient` ne declare pas cette propriete. Le DAL ne la lit pas non plus.
- **Impact:** Mineur -- la date de creation est rarement affichee pour les ingredients. Mais si un jour on veut trier ou filtrer par anciennete, il faudra l'ajouter.
- **Suggestion:** Ajouter `public DateTime DateCreation { get; set; }` pour coherence avec les autres Models (Activite, Stock, BomContexte qui l'ont tous).

---

### [F-MOD-004] Ingredient: `NbParLot` est `int` mais la DB n'a pas ce type exact dans schema_complet.sql
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/Ingredient.cs:20`
- **Type:** Incoherence documentation
- **Description:** `NbParLot` est declare `int` dans le Model et `INT NOT NULL DEFAULT 1` dans `create_database.sql`. C'est correct. Mais `schema_complet.sql` ne connait pas cette colonne (cf. F-MOD-001). La correspondance de type est valide dans la source de verite (`create_database.sql`).
- **Impact:** Aucun bug. Finding lie a F-MOD-001.
- **Suggestion:** Resolu par F-MOD-001.

---

### [F-MOD-005] Lot: colonne `date_creation` absente du Model
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/Lot.cs`
- **Type:** Incoherence
- **Description:** La table `lots_ingredients` a `date_creation DATETIME DEFAULT CURRENT_TIMESTAMP`. Le Model `Lot` ne la declare pas. Le DAL ne la lit pas.
- **Impact:** Impossible de savoir quand une entree de lot a ete creee dans le systeme (distinct de `DateAchat` qui est la date de l'achat reel).
- **Suggestion:** Ajouter `public DateTime DateCreation { get; set; }` si besoin d'audit trail.

---

### [F-MOD-006] Lot: pas de ToString() override
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/Lot.cs`
- **Type:** Pattern manque
- **Description:** `Lot` est le seul Model entite persiste sans override `ToString()`. Les 17 autres entites ont toutes un `ToString()`. Un `Lot` affiche dans un ComboBox ou un debug afficherait `CharlesNadejda.Models.Lot`.
- **Impact:** Mauvais affichage si un `Lot` est bind dans un ComboBox ou affiche en debug. Inconsistance du pattern.
- **Suggestion:** Ajouter par exemple `public override string ToString() => $"Lot {NumeroLot ?? Id.ToString()} -- {NomIngredient}";`

---

### [F-MOD-007] CommandeWebLigne: pas de ToString() override
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/CommandeWebLigne.cs`
- **Type:** Pattern manque
- **Description:** Meme probleme que F-MOD-006. Pas de `ToString()` sur `CommandeWebLigne`.
- **Impact:** Mineur -- les lignes de commande sont rarement affichees hors d'un DataGridView.
- **Suggestion:** Ajouter `public override string ToString() => $"{Quantite}x {NomProduit} ({SousTotal:C})";`

---

### [F-MOD-008] VueStockGlobal: pas de ToString() override
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/VueStockGlobal.cs`
- **Type:** Pattern manque
- **Description:** Pas de `ToString()` sur `VueStockGlobal`.
- **Impact:** Mineur -- la vue stock est affichee dans un DGV, pas un ComboBox.
- **Suggestion:** Ajouter `public override string ToString() => $"{Nom} -- {QuantiteDispoReelle} {Unite}";`

---

### [F-MOD-009] Utilisateur: colonnes DB manquantes dans le Model (7 champs)
- **Severite:** CRITIQUE
- **Fichier(s):** `app-csharp/.../Models/Utilisateur.cs`
- **Type:** Incoherence
- **Description:** Le Model `Utilisateur` ne contient que 5 proprietes (`Id`, `Nom`, `Prenom`, `Email`, `Role`). La table `utilisateurs` contient 11 colonnes. Colonnes manquantes :
  - `mot_de_passe VARCHAR(255)` -- le hash est lu par le DAL mais jamais stocke dans le Model (correct pour la securite)
  - `telephone VARCHAR(20)` -- absent du Model
  - `adresse VARCHAR(255)` -- absent du Model
  - `code_postal VARCHAR(10)` -- absent du Model
  - `ville VARCHAR(100)` -- absent du Model
  - `date_inscription DATETIME` -- absent du Model
  - `actif TINYINT(1)` -- absent du Model

  Le DAL `UtilisateurDAL.Authenticate()` ne lit que `id, nom, prenom, email, role, mot_de_passe` et filtre `actif = 1` en WHERE. Le mot de passe n'est correctement PAS stocke dans le Model (bonne pratique securite).
- **Impact:** Si l'ERP doit un jour gerer les utilisateurs (CRUD admin), il faudra completer le Model. Actuellement, `Utilisateur` est uniquement utilise pour l'authentification et l'affichage en StatusBar, donc le sous-ensemble est suffisant fonctionnellement.
- **Suggestion:** Accepter le sous-ensemble actuel comme volontaire (login-only DTO). Documenter le choix par un commentaire XML. Si un CRUD admin est ajoute, creer un `UtilisateurComplet` ou enrichir ce Model.

---

### [F-MOD-010] ProduitWeb: colonne `date_modification` absente du Model
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/ProduitWeb.cs`
- **Type:** Incoherence
- **Description:** La table `produits_web` contient `date_modification DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP`. Le Model n'a que `DateCreation`, pas `DateModification`. Le DAL ne la lit pas.
- **Impact:** Impossible de savoir quand un produit a ete modifie pour la derniere fois.
- **Suggestion:** Ajouter `public DateTime DateModification { get; set; }` si l'info est pertinente pour l'admin.

---

### [F-MOD-011] CategorieWeb: colonne `date_creation` absente du Model
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/.../Models/CategorieWeb.cs`
- **Type:** Incoherence
- **Description:** La table `categories_web` a `date_creation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP`. Le Model ne la declare pas. Le DAL utilise `SELECT c.*` mais ne mappe pas `date_creation`.
- **Impact:** Mineur -- rarement besoin d'afficher la date de creation d'une categorie.
- **Suggestion:** Pour coherence, ajouter la propriete ou ne pas utiliser `SELECT *` dans le DAL.

---

### [F-MOD-012] BomNiveau: `Ordre` est `int` mais la DB est `TINYINT UNSIGNED`
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/BomNiveau.cs:9`
- **Type:** Incoherence type
- **Description:** La colonne `ordre` dans `bom_niveaux` est `TINYINT UNSIGNED NOT NULL` (valeurs 0-255). Le Model declare `public int Ordre`. Le MySQL Connector mappe TINYINT en `sbyte` ou `byte` selon le mode. Le cast en `int` fonctionne par promotion implicite, mais la semantique est differente : un `int` autorise des valeurs negatives et jusqu'a 2^31, alors que la DB refuse > 255.
- **Impact:** Pas de bug runtime grace a la promotion implicite. Mais aucune validation cote C# n'empeche de tenter un `Ordre = 300` qui echouera en DB avec une erreur cryptique.
- **Suggestion:** Ajouter une validation metier `if (ordre < 0 || ordre > 255)` avant INSERT/UPDATE. Le type `int` reste acceptable cote C# (pas de `byte` en WinForms standard).

---

### [F-MOD-013] BomFiche.CoutBatch / CoutUnitaire: dependance implicite a Lignes chargees
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/BomFiche.cs:32-33`
- **Type:** Pattern manque
- **Description:** Les proprietes calculees `CoutBatch` et `CoutUnitaire` utilisent `Lignes.Sum(l => l.SousTotal)`. Si `Lignes` n'a pas ete charge par le DAL (chargement optionnel), elles retournent 0 silencieusement (la liste est vide, pas null). Ce n'est pas un crash mais une valeur trompeuse.
- **Impact:** Un developpeur affichant `CoutUnitaire` sans avoir charge les lignes verra `0.00` et pourrait croire que la fiche est gratuite. Aucune indication que les lignes n'ont pas ete chargees.
- **Suggestion:** Documenter explicitement par un commentaire XML que ces proprietes requierent un chargement prealable des lignes. Alternativement, retourner `null` ou `-1` si `Lignes.Count == 0` pour signaler l'absence de donnees.

---

### [F-MOD-014] BomFicheLigne.PrixUnitaireRef: propriete de jointure non-DB potentiellement non peuplee
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/BomFicheLigne.cs:16`
- **Type:** Pattern manque
- **Description:** `PrixUnitaireRef` est une propriete de jointure remplie par le DAL lors du chargement des lignes. Sa valeur par defaut est `0m` (decimal default). Si elle n'est pas peuplee, `SousTotal` retourne `0` silencieusement au lieu de signaler l'absence de prix.
- **Impact:** Meme risque que F-MOD-013 : cout affiche = 0 sans alerte.
- **Suggestion:** Acceptable pour un projet academique. Documenter dans le Model.

---

### [F-MOD-015] Ingredient.EstEnAlerte: utilise StockActuel qui est optionnel
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/Ingredient.cs:34-35`
- **Type:** Pattern manque
- **Description:** `EstEnAlerte` compare `StockActuel <= SeuilAlerteStock.Value`. `StockActuel` est un `decimal` (default 0) peuple par le DAL via `COALESCE(SUM(...), 0)`. Si l'ingredient est charge sans le SUM (hypothetique), `StockActuel = 0` et `EstEnAlerte` retournera `true` pour tout ingredient ayant un seuil > 0, meme s'il a du stock.
- **Impact:** Risque faible car le DAL actuel peuple toujours cette valeur. Mais le contrat implicite n'est pas documente.
- **Suggestion:** Ajouter un commentaire `/// Requiert un chargement via IngredientDAL.GetAll() ou GetById() qui peuple StockActuel.`

---

### [F-MOD-016] BomProduction: propriete `QuantiteOutputBatch` n'est pas une colonne de bom_productions
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/BomProduction.cs:62`
- **Type:** Documentation
- **Description:** `QuantiteOutputBatch` est une propriete de jointure (issue de `bom_fiches.quantite_output`). Elle est correctement documentee par un commentaire dans le fichier. Cependant, la section "Champs issus des jointures" du Model melange des jointures simples (NomFiche, NomNiveau) et des proprietes de calcul (QuantiteOutputBatch). Aucune separation structurelle.
- **Impact:** Lisibilite. Un nouveau developpeur pourrait croire que `QuantiteOutputBatch` est dans la table `bom_productions`.
- **Suggestion:** Grouper clairement les proprietes par categorie avec des regions ou des commentaires de section.

---

### [F-MOD-017] BomStock: proprietes `StockCible` et `TotalDispoFiche` ne sont pas des colonnes DB
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/BomStock.cs:30-31`
- **Type:** Documentation
- **Description:** `StockCible` vient de `bom_fiches.stock_cible` (jointure). `TotalDispoFiche` vient d'une sous-requete SQL (`SUM(s2.quantite_disponible)`). Ces deux proprietes sont correctement peuplees par le DAL Map(), mais elles ne sont pas dans la section "Jointures" du Model.
- **Impact:** Lisibilite. `TotalDispoFiche` en particulier est une valeur agregee calculee a la volee, pas une simple jointure.
- **Suggestion:** Separer en section `// Valeurs calculees/agregees` distincte de `// Jointures`.

---

### [F-MOD-018] CommandeWebLigne.SousTotal: lecture d'une colonne GENERATED
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/CommandeWebLigne.cs:10`
- **Type:** Incoherence
- **Description:** La colonne `sous_total` dans `commandes_web_lignes` est `DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED`. Le Model la declare comme propriete simple `public decimal SousTotal { get; set; }` avec un setter. Le DAL la lit correctement. Cependant, si quelqu'un ecrit `ligne.SousTotal = valeur` cote C# et tente un INSERT, MySQL ignorera la valeur (GENERATED column) sans erreur.
- **Impact:** Pas de bug car le DAL n'insere pas `sous_total`. Mais le setter ouvert est trompeur.
- **Suggestion:** Considerer un pattern read-only : propriete calculee `public decimal SousTotal => Quantite * PrixUnitaire;` qui reflete le calcul DB. Ou ajouter un commentaire `// colonne GENERATED -- lecture seule`.

---

### [F-MOD-019] VueStockGlobal.EstEnAlerte: logique simpliste
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/.../Models/VueStockGlobal.cs:40`
- **Type:** Incoherence
- **Description:** `EstEnAlerte => QuantiteDispoReelle <= 0` retourne `true` uniquement si le stock est a zero ou negatif. Cela ne prend pas en compte le `seuil_alerte_stock` de la fiche ingredient. Pour les lots, le vrai seuil est defini dans `fiches_ingredients.seuil_alerte_stock`. Pour les produits fabriques, le seuil est `bom_fiches.stock_cible`.
- **Impact:** Un ingredient a 5g avec un seuil d'alerte a 100g ne sera PAS signale comme "en alerte" par cette propriete. Le nom `EstEnAlerte` est trompeur -- il signifie en realite "est en rupture".
- **Suggestion:** Renommer en `EstEnRupture` pour refleter la semantique reelle, ou enrichir la logique en injectant le seuil d'alerte.

---

### [F-MOD-020] Pas de Model pour `activites_stocks` (table de jonction M:N)
- **Severite:** CRITIQUE
- **Fichier(s):** N/A (absent)
- **Type:** Orphelin
- **Description:** La table `activites_stocks` (jonction M:N entre activites et stocks) n'a pas de Model C# correspondant. Le DAL la manipule directement via SQL brut dans plusieurs DAL (StockDAL, ActiviteDAL, VueStockGlobalDAL). C'est un choix acceptable pour une table de jonction sans attributs propres, mais cela signifie que la relation M:N n'est pas modelisee en objet.
- **Impact:** Pas de bug. Mais si des attributs sont ajoutes a la table de jonction (ex: `date_association`), il faudra creer un Model.
- **Suggestion:** Acceptable en l'etat. Documenter le choix. Creer un Model si la table evolue.

---

### [F-MOD-021] Pas de Model pour `clients` (table e-commerce)
- **Severite:** MINEUR
- **Fichier(s):** N/A (absent)
- **Type:** Orphelin
- **Description:** La table `clients` (19 colonnes, module e-commerce) n'a pas de Model C# correspondant. Les informations client sont accedees via des proprietes de jointure dans `CommandeWeb` (`NomClient`, `PrenomClient`, `EmailClient`). Le CRUD des clients est gere exclusivement par Laravel.
- **Impact:** Aucun bug. L'ERP ne gere pas les clients directement -- il les consulte uniquement via les commandes.
- **Suggestion:** Acceptable. Creer un Model `Client` uniquement si un ecran de gestion des clients est ajoute a l'ERP.

---

## Synthese des proprietes de jointure

Liste des proprietes dans les Models qui ne correspondent PAS a des colonnes de leur table DB.
Toutes sont peuplees par le DAL correspondant.

| Model | Propriete | Source (jointure/calcul) | Peuplee par DAL |
|-------|-----------|--------------------------|-----------------|
| `Ingredient` | `NomFournisseur` | `fournisseurs.nom` | Oui (GetAll, GetById) |
| `Ingredient` | `StockActuel` | `SUM(lots.quantite_disponible)` | Oui (GetAll, GetById) |
| `Lot` | `StockNom` | `stocks.nom` | Oui |
| `Lot` | `NomIngredient` | `fiches_ingredients.nom` | Oui |
| `Lot` | `UniteMesure` | `fiches_ingredients.unite_mesure` | Oui |
| `Lot` | `ConditionnementLabel` | `fiches_ingredients.conditionnement_label` | Oui |
| `Lot` | `QteParConditionnement` | `fiches_ingredients.qte_par_conditionnement` | Oui |
| `Lot` | `NomFournisseur` | `fournisseurs.nom` | Oui |
| `BomContexte` | `ActiviteNom` | `activites.nom` | Oui |
| `BomNiveau` | `NomContexte` | `bom_contextes.nom` | Oui |
| `BomNiveau` | `IdActivite` | `bom_contextes.id_activite` | Oui |
| `BomNiveau` | `ActiviteNom` | `activites.nom` | Oui |
| `BomFiche` | `NomNiveau` | `bom_niveaux.nom` | Oui |
| `BomFiche` | `OrdreNiveau` | `bom_niveaux.ordre` | Oui |
| `BomFiche` | `IdContexte` | `bom_niveaux.id_contexte` | Oui |
| `BomFiche` | `NomContexte` | `bom_contextes.nom` | Oui |
| `BomFiche` | `IdActivite` | (via contexte) | Oui |
| `BomFiche` | `ActiviteNom` | `activites.nom` | Oui |
| `BomFicheLigne` | `NomInput` | ingredient.nom ou fiche.nom | Oui |
| `BomFicheLigne` | `UniteMesureInput` | ingredient.unite ou fiche.unite | Oui |
| `BomFicheLigne` | `PrixUnitaireRef` | calcul prix reference | Oui |
| `BomProduction` | `NomFiche` | `bom_fiches.nom` | Oui |
| `BomProduction` | `NomNiveau` | `bom_niveaux.nom` | Oui |
| `BomProduction` | `OrdreNiveau` | `bom_niveaux.ordre` | Oui |
| `BomProduction` | `NomContexte` | `bom_contextes.nom` | Oui |
| `BomProduction` | `UniteOutput` | `bom_fiches.unite_output` | Oui |
| `BomProduction` | `QuantiteOutputBatch` | `bom_fiches.quantite_output` | Oui |
| `BomProductionLigne` | `NomSource` | ingredient.nom ou fiche.nom | Oui |
| `BomProductionLigne` | `UniteSource` | unite de la source | Oui |
| `BomStock` | `NomFiche` | `bom_fiches.nom` | Oui |
| `BomStock` | `UniteOutput` | `bom_fiches.unite_output` | Oui |
| `BomStock` | `NomNiveau` | `bom_niveaux.nom` | Oui |
| `BomStock` | `OrdreNiveau` | `bom_niveaux.ordre` | Oui |
| `BomStock` | `NomContexte` | `bom_contextes.nom` | Oui |
| `BomStock` | `NomActivite` | `activites.nom` | Oui |
| `BomStock` | `StockCible` | `bom_fiches.stock_cible` | Oui |
| `BomStock` | `TotalDispoFiche` | sous-requete SUM | Oui |
| `BomReservation` | `NomIngredient` | jointure ingredient | Oui |
| `BomReservation` | `UniteMesure` | jointure ingredient | Oui |
| `BomReservation` | `NomContexte` | `bom_contextes.nom` | Oui |
| `CategorieWeb` | `NbProduits` | `COUNT(produits_web)` | Oui |
| `ProduitWeb` | `NomFiche` | `bom_fiches.nom` | Oui |
| `ProduitWeb` | `NomCategorie` | `categories_web.nom` | Oui |
| `ProduitWeb` | `QuantiteOutputBatch` | `bom_fiches.quantite_output` | Oui |
| `ProduitWeb` | `UniteOutput` | `bom_fiches.unite_output` | Oui |
| `ProduitWeb` | `StockDisponible` | `SUM(bom_stocks.quantite_disponible)` | Oui |
| `CommandeWeb` | `NomClient` | `clients.nom` | Oui |
| `CommandeWeb` | `PrenomClient` | `clients.prenom` | Oui |
| `CommandeWeb` | `EmailClient` | `clients.email` | Oui |
| `CommandeWeb` | `NbArticles` | sous-requete COUNT | Oui |
| `CommandeWebLigne` | `NomProduit` | `produits_web.nom_commercial` | Oui |
| `VueStockGlobal` | `NomActivite` | `activites.nom` (via LEFT JOIN DAL) | Oui |

**Verdict:** Toutes les proprietes de jointure sont correctement peuplees par le DAL correspondant. Aucune propriete orpheline non peuplee detectee.

---

## Synthese des proprietes calculees

| Model | Propriete | Formule | Correcte? |
|-------|-----------|---------|-----------|
| `Ingredient` | `PrixParUniteBase` | `PrixAchatReference / QteParConditionnement` | Oui (division protegee) |
| `Ingredient` | `EstEnAlerte` | `SeuilAlerteStock.HasValue && StockActuel <= SeuilAlerteStock.Value` | Oui |
| `Ingredient` | `StockPieces` | `Floor(StockActuel / QteParConditionnement)` | Oui (division protegee) |
| `Ingredient` | `StockRatio` | `StockActuel / StockCible.Value` | Oui (null-safe) |
| `Lot` | `PrixUnitaireBase` | `PrixUnitaire / QteParConditionnement` | Oui (division protegee) |
| `BomFiche` | `CoutBatch` | `Lignes.Sum(l => l.SousTotal)` | Oui (si lignes chargees, cf F-MOD-013) |
| `BomFiche` | `CoutUnitaire` | `CoutBatch / QuantiteOutput` | Oui (division protegee) |
| `BomFicheLigne` | `SousTotal` | `Quantite * PrixUnitaireRef` | Oui |
| `BomProductionLigne` | `SousTotal` | `QuantiteConsommee * CoutUnitaireMoment` | Oui |
| `BomStock` | `StockRatio` | `TotalDispoFiche / StockCible.Value` | Oui (null-safe) |
| `BomStock` | `EstPerime` | `DateDlc.HasValue && DateDlc.Value < DateTime.Today` | Oui |
| `BomStock` | `CoutTotal` | `QuantiteDisponible * CoutUnitaire` | Oui |
| `BomManque` | `Manque` | `QuantiteNecessaire - QuantiteDisponible` (avec guard >= 0) | Oui |
| `ProduitWeb` | `StockUnites` | `(int)(StockDisponible / QuantiteOutputBatch)` | Oui (division protegee) |
| `ProduitWeb` | `EstEnStock` | `StockDisponible > 0` | Oui |
| `CommandeWeb` | `NomCompletClient` | `$"{PrenomClient} {NomClient}"` | Oui |
| `VueStockGlobal` | `EstLot` | `TypeStock == "lot_ingredient"` | Oui |
| `VueStockGlobal` | `EstEnAlerte` | `QuantiteDispoReelle <= 0` | Semantiquement trompeur (cf F-MOD-019) |
| `VueStockGlobal` | `ADesReservations` | `QuantiteReservee > 0` | Oui |

**Verdict:** Toutes les formules sont mathematiquement correctes. Les divisions sont protegees contre le zero. Les nullables sont correctement geres. Le seul probleme semantique est `VueStockGlobal.EstEnAlerte` (F-MOD-019).

---

## Synthese des collections List<T>

| Model | Propriete | Initialisee? |
|-------|-----------|-------------|
| `BomContexte` | `List<BomNiveau> Niveaux` | Oui (`= new List<BomNiveau>()`) |
| `BomFiche` | `List<BomFicheLigne> Lignes` | Oui (`= new List<BomFicheLigne>()`) |
| `BomProduction` | `List<BomProductionLigne> Lignes` | Oui (`= new List<BomProductionLigne>()`) |
| `CommandeWeb` | `List<CommandeWebLigne> Lignes` | Oui (`= new List<CommandeWebLigne>()`) |
| `RapportCout` | `List<LigneCout> Lignes` | Oui (`= new List<LigneCout>()`) |

**Verdict:** Toutes les collections sont initialisees. Aucun risque de `NullReferenceException`.

---

## Synthese des types C# vs SQL

| Colonne DB | Type SQL | Propriete C# | Type C# | Correspondance |
|------------|----------|--------------|---------|----------------|
| `*.id` | `INT AUTO_INCREMENT` | `Id` | `int` | OK |
| `*.nom` | `VARCHAR(100-200)` | `Nom` | `string` | OK |
| `*.description` | `TEXT` | `Description` | `string` | OK |
| `*.actif` | `TINYINT(1)` | `Actif` | `bool` | OK |
| `*.date_creation` | `DATETIME` | `DateCreation` | `DateTime` | OK |
| `fi.unite_mesure` | `ENUM(...)` | `UniteMesure` | `string` | OK (pas d'enum C#, acceptable) |
| `fi.type_physique` | `ENUM(...)` | `TypePhysique` | `string` | OK |
| `fi.densite` | `DECIMAL(8,4)` | `Densite` | `decimal?` | OK (nullable) |
| `fi.qte_par_cond` | `DECIMAL(12,4)` | `QteParConditionnement` | `decimal` | OK |
| `fi.prix_achat_ref` | `DECIMAL(10,4)` | `PrixAchatReference` | `decimal` | OK |
| `li.nb_cond` | `DECIMAL(10,3)` | `NbConditionnements` | `decimal` | OK |
| `li.prix_unitaire` | `DECIMAL(10,4)` | `PrixUnitaire` | `decimal` | OK |
| `li.tva_pct` | `DECIMAL(5,2)` | `TvaPct` | `decimal` | OK |
| `bn.ordre` | `TINYINT UNSIGNED` | `Ordre` | `int` | Cf. F-MOD-012 |
| `bfl.quantite` | `DECIMAL(12,4)` | `Quantite` | `decimal` | OK |
| `bp.cout_ingredients` | `DECIMAL(10,2)` | `CoutIngredients` | `decimal` | OK |
| `cmd.statut` | `ENUM('panier','payee','annulee')` | `Statut` | `string` | OK |
| `cmd.total_ttc` | `DECIMAL(10,2)` | `TotalTtc` | `decimal` | OK |
| `cwl.quantite` | `INT` | `Quantite` | `int` | OK |
| `cwl.sous_total` | `DECIMAL(10,2) GENERATED` | `SousTotal` | `decimal` | Cf. F-MOD-018 |
| `u.role` | `ENUM('client','admin')` | `Role` | `string` | OK |

**Verdict:** Les correspondances de types sont globalement correctes. Un seul cas de mismatch semantique (TINYINT UNSIGNED vs int, F-MOD-012). Les ENUM SQL sont mappes en `string` cote C#, ce qui est un choix pragmatique acceptable pour un projet WinForms.

---

## Priorites de remediation

### A traiter en priorite (CRITIQUE)
1. **F-MOD-002** -- Ajouter `DlcJoursReference` et `QualiteLabel` au Model Ingredient + DAL (fonctionnalite metier manquante)
2. **F-MOD-009** -- Documenter le choix volontaire de sous-ensemble pour Utilisateur
3. **F-MOD-020** -- Documenter le choix de ne pas modeliser `activites_stocks`

### A planifier (IMPORTANT)
4. **F-MOD-019** -- Renommer `VueStockGlobal.EstEnAlerte` en `EstEnRupture`
5. **F-MOD-018** -- Ajouter un commentaire `// GENERATED` sur `CommandeWebLigne.SousTotal`
6. **F-MOD-013** -- Documenter la dependance aux lignes chargees sur `BomFiche.CoutBatch`
7. **F-MOD-012** -- Ajouter validation `Ordre` dans le range 0-255
8. **F-MOD-001** -- Regenerer `schema_complet.sql`
9. **F-MOD-014/015/016/017** -- Enrichir la documentation XML des proprietes

### Optionnel (MINEUR)
10. **F-MOD-006/007/008** -- Ajouter `ToString()` manquants
11. **F-MOD-003/005/010/011** -- Ajouter proprietes `DateCreation`/`DateModification` manquantes
