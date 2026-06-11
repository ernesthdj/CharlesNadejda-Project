# Audit SQL/Schema -- ArtisaStock
> Date: 2026-06-11 | Agent: SQL/Schema Auditor (#6)
> Scope: sql/*.sql, DAL C# (19 fichiers), Models Laravel (8 fichiers)

---

## Resume

**21 findings** (3 critiques, 8 importants, 10 mineurs)

| Severite  | Nombre |
|-----------|--------|
| CRITIQUE  | 3      |
| IMPORTANT | 8      |
| MINEUR    | 10     |

---

## Findings

---

### [F-SQL-001] schema_complet.sql obsolete (pre-v19, pre-v18)
- **Severite:** CRITIQUE
- **Fichier(s):** `sql/schema_complet.sql:1-393`
- **Type:** Incoherence
- **Description:** Le fichier `schema_complet.sql` se declare "post migration v18" (ligne 3) mais ne contient pas la colonne `nb_par_lot` (v19) sur `fiches_ingredients`. De plus, la table `fiches_ingredients` y contient encore la colonne `id_fournisseur_defaut` referencee correctement, mais `id_stock` n'est pas present (deja supprime par v18), ce qui est correct. Cependant, la VIEW `vue_stock_global` utilise encore `fi.id_stock` dans le GROUP BY au lieu de `li.id_stock` (incoherent avec la v18 qui a deplace id_stock vers lots_ingredients). La colonne `nb_par_lot` de la v19 est absente de la table fiches_ingredients dans ce fichier.
- **Impact:** Un deploiement from-scratch via `schema_complet.sql` cree un schema incomplet. La VIEW echoue car `fi.id_stock` n'existe plus. Le schema ne match plus la production.
- **Suggestion:** Regenerer `schema_complet.sql` a partir de `create_database.sql` qui est deja a jour (post-v19). Marquer `schema_complet.sql` comme obsolete ou le supprimer, car `create_database.sql` est le fichier canonique.

---

### [F-SQL-002] schema_complet.sql - VIEW vue_stock_global utilise fi.id_stock (colonne supprimee en v18)
- **Severite:** CRITIQUE
- **Fichier(s):** `sql/schema_complet.sql:353` et `sql/schema_complet.sql:362-366`
- **Type:** Incoherence
- **Description:** Dans le `schema_complet.sql`, la VIEW `vue_stock_global` contient `li.id_stock` dans le SELECT (ligne 353, correct) mais le JOIN est `JOIN stocks s ON s.id = li.id_stock` (correct). Toutefois, la vue referencie `fi.id` dans le GROUP BY (ligne 364) sans alias explicite, ce qui est acceptable. Le vrai probleme : la table `fiches_ingredients` dans `schema_complet.sql` ne contient pas `nb_par_lot` (ajoutee en v19), rendant ce fichier non deployable sans la v19.
- **Impact:** Le fichier `schema_complet.sql` ne peut pas etre utilise seul pour un deploiement propre.
- **Suggestion:** Ce finding est un duplicata partiel de F-SQL-001. Le fichier `create_database.sql` est la reference correcte et doit etre utilise a la place.

---

### [F-SQL-003] seed_data.sql reference des tables supprimees en v12
- **Severite:** CRITIQUE
- **Fichier(s):** `sql/seed_data.sql:17-20` (frais_generaux_config), `sql/seed_data.sql:61-70` (fiches_recettes), `sql/seed_data.sql:73-90` (recettes_ingredients), `sql/seed_data.sql:95-100` (categories), `sql/seed_data.sql:106-121` (parfums), `sql/seed_data.sql:126-143` (fiches_compositions), `sql/seed_data.sql:154-191` (produits), `sql/seed_data.sql:190-191` (produits_parfums), `sql/seed_data.sql:196-198` (zones_livraison), `sql/seed_data.sql:216-234` (commandes, lignes_commandes, selections_parfums, factures), `sql/seed_data.sql:242-260` (productions_recettes, mouvements_lots_ingredients, stock_produits), `sql/seed_data.sql:265-269` (contacts), `sql/seed_data.sql:281-362` (pat_*)
- **Type:** Orphelin / Dead code
- **Description:** Le fichier `seed_data.sql` reference massivement des tables qui ont ete supprimees par la migration v12 (cleanup). Tables inexistantes : `frais_generaux_config`, `fiches_recettes`, `recettes_ingredients`, `categories`, `parfums`, `fiches_compositions`, `compositions_recettes`, `produits`, `produits_parfums`, `zones_livraison`, `commandes`, `lignes_commandes`, `selections_parfums`, `factures`, `productions_recettes`, `mouvements_lots_ingredients`, `stock_produits`, `contacts`, `pat_formes`, `pat_gabarits`, `pat_types_couche`, `pat_allergenes`, `pat_options_deco`, `pat_parametres`. Seuls les INSERT dans `fournisseurs`, `fiches_ingredients`, `lots_ingredients` et `utilisateurs` sont potentiellement valides, mais les colonnes de `fiches_ingredients` ne matchent pas le schema actuel (manque `conditionnement_label`, `qte_par_conditionnement`, `nb_par_lot` ; `id_stock` manquant sur lots_ingredients).
- **Impact:** L'execution de `seed_data.sql` sur la DB actuelle echoue a 100% (erreurs SQL massives). Aucun jeu de donnees de test utilisable.
- **Suggestion:** Reecrire `seed_data.sql` pour le schema post-v19 (20 tables actuelles). En attendant, le fichier `sql/tests/injection_glacier_stress_test.sql` est un bon modele de seed coherent avec le schema actuel.

---

### [F-SQL-004] create_database.sql diverge de schema_complet.sql sur bom_contextes FK
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql:181` vs `sql/schema_complet.sql:124`
- **Type:** Incoherence
- **Description:** Dans `create_database.sql`, la FK `fk_bc_activite` sur `bom_contextes` est definie comme `ON DELETE RESTRICT` (ligne 181). Dans `schema_complet.sql`, elle est `ON DELETE CASCADE` (ligne 124). Le code DAL C# (`BomContexteDAL.Delete`) verifie manuellement les productions et stocks avant suppression, ce qui implique que RESTRICT est le comportement correct.
- **Impact:** `schema_complet.sql` permettrait une suppression en cascade non voulue d'un contexte avec productions, perdant la tracabilite.
- **Suggestion:** Aligner `schema_complet.sql` sur `create_database.sql` (RESTRICT). Le fichier `schema_complet.sql` devrait idealement etre regenere ou supprime (cf. F-SQL-001).

---

### [F-SQL-005] Index manquant sur lots_ingredients.id_fiche_ingredient
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql` (lots_ingredients, lignes 121-149)
- **Type:** Pattern manque
- **Description:** La colonne `lots_ingredients.id_fiche_ingredient` est utilisee dans de nombreuses jointures et WHERE :
  - `IngredientDAL.GetAll` : `LEFT JOIN lots_ingredients l ON l.id_fiche_ingredient = fi.id`
  - `LotDAL.GetByFicheIngredient` : `WHERE l.id_fiche_ingredient = @idFiche`
  - `BomStockDAL.GetDisponibleIngredient` : `WHERE l.id_fiche_ingredient = @idFi`
  - `BomStockDAL.GetLotsDispoFIFO` : `WHERE l.id_fiche_ingredient = @idFi`
  - `BomCoutDAL.GetPrixMoyenIngredient` : `LEFT JOIN lots_ingredients l ON l.id_fiche_ingredient = fi.id`
  - `vue_stock_global` : `JOIN fiches_ingredients fi ON fi.id = li.id_fiche_ingredient`

  Aucun index explicite n'est cree. MySQL cree automatiquement un index sur les colonnes FK (`fk_lot_fiche`), mais il est bon de le documenter et de s'assurer qu'il couvre aussi les requetes composees.
- **Impact:** Performance sur les requetes de stock et de production, surtout avec un volume de lots croissant (le stress test injecte 32+ lots).
- **Suggestion:** L'index implicite de la FK `fk_lot_fiche` couvre deja cette colonne. Verifier avec `SHOW INDEX FROM lots_ingredients` en production. Si besoin d'un index composite pour les requetes FIFO : `CREATE INDEX idx_lot_fiche_achat ON lots_ingredients (id_fiche_ingredient, date_achat)`.

---

### [F-SQL-006] Index manquant sur lots_ingredients.id_stock
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql` (lots_ingredients, ligne 143)
- **Type:** Pattern manque
- **Description:** La colonne `lots_ingredients.id_stock` est utilisee dans :
  - `LotDAL.GetAll` : sous-requete `SELECT id_stock FROM activites_stocks WHERE id_activite = @idActivite`
  - `StockDAL.Delete` : `SELECT COUNT(*) FROM lots_ingredients WHERE id_stock = @id`
  - `StockDAL.ContientDonnees` : `WHERE id_stock = @id`
  - `vue_stock_global` : `JOIN stocks s ON s.id = li.id_stock`

  La FK `fk_lots_stock` est definie sans clause ON DELETE/ON UPDATE (ligne 143 de create_database.sql), alors que le schema_complet.sql ne specifie pas non plus de cascade. L'index implicite de la FK devrait exister, mais l'absence de ON DELETE/ON UPDATE sur la FK est notable.
- **Impact:** Si un stock est supprime, les lots orphelins ne seront pas nettoyes automatiquement (pas de CASCADE). Le code C# (`StockDAL.Delete`) verifie manuellement, mais une erreur FK silencieuse est possible.
- **Suggestion:** Ajouter `ON DELETE RESTRICT ON UPDATE CASCADE` explicitement a la FK `fk_lots_stock` pour coherence avec le reste du schema.

---

### [F-SQL-007] Index manquant sur bom_reservations pour les requetes frequentes
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql` (bom_reservations, lignes 332-346)
- **Type:** Pattern manque
- **Description:** La table `bom_reservations` est jointe dans la VIEW `vue_stock_global` avec `ON br.id_lot = li.id AND br.actif = 1`. Elle est aussi utilisee dans `BomStockDAL.GetLotsDispoFIFO` (sous-requete par id_lot + actif) et `BomReservationDAL.GetByContexte` (WHERE id_contexte + actif). Aucun index composite n'est defini.
- **Impact:** Avec beaucoup de reservations, les sous-requetes de la VIEW et du FIFO deviennent couteuses.
- **Suggestion:** `CREATE INDEX idx_bomres_lot_actif ON bom_reservations (id_lot, actif)` et `CREATE INDEX idx_bomres_ctx_actif ON bom_reservations (id_contexte, actif)`.

---

### [F-SQL-008] FK fk_lots_stock sans clause ON DELETE/ON UPDATE
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql:143`, `sql/schema_complet.sql:108`
- **Type:** Incoherence
- **Description:** Dans les deux fichiers, la FK `fk_lots_stock` est definie comme `FOREIGN KEY (id_stock) REFERENCES stocks(id)` sans clause ON DELETE ni ON UPDATE. Toutes les autres FK du schema ont des clauses explicites (CASCADE, RESTRICT ou SET NULL). Le comportement par defaut de MySQL est RESTRICT, ce qui est probablement voulu, mais l'absence de declaration explicite est inconsistante.
- **Impact:** Ambiguite sur le comportement attendu. Un developpeur pourrait supposer un comportement CASCADE par erreur.
- **Suggestion:** Ajouter `ON DELETE RESTRICT ON UPDATE CASCADE` explicitement a la FK `fk_lots_stock` dans `create_database.sql`.

---

### [F-SQL-009] Colonne fiches_ingredients.dlc_jours_reference -- jamais utilisee dans le DAL C#
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql:106` (colonne `dlc_jours_reference`)
- **Type:** Colonne inutilisee
- **Description:** La colonne `fiches_ingredients.dlc_jours_reference` est declaree dans le schema (INT DEFAULT NULL) mais n'est jamais referencee dans :
  - `IngredientDAL.cs` : pas dans le SELECT, INSERT, UPDATE ni Map()
  - Aucun Model C# ne la mappe
  - Aucun Model Laravel ne l'utilise

  Elle semble etre un vestige de l'ancien systeme de recettes (supprime en v12).
- **Impact:** Colonne orpheline occupant de l'espace. Pas de risque fonctionnel mais source de confusion.
- **Suggestion:** Soit ajouter le support dans le DAL (valeur pertinente pour la DLC auto des lots), soit documenter comme "reserve v2" et considerer sa suppression si non prevue.

---

### [F-SQL-010] Colonne fiches_ingredients.qualite_label -- jamais utilisee dans le DAL C#
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql:107` (colonne `qualite_label`)
- **Type:** Colonne inutilisee
- **Description:** La colonne `fiches_ingredients.qualite_label` (VARCHAR(100) DEFAULT NULL) n'est jamais referencee dans `IngredientDAL.cs` (pas dans SELECT, INSERT, UPDATE, Map). Elle est absente de tous les models C# et Laravel.
- **Impact:** Colonne orpheline. Les donnees du seed d'origine ("Grand Cru", "AOP", "Bio") ne sont plus injectees.
- **Suggestion:** Soit integrer dans l'UI/DAL pour enrichir les fiches ingredients, soit supprimer si non prevue.

---

### [F-SQL-011] Colonnes utilisateurs jamais lues par le DAL : telephone, adresse, code_postal, ville, date_inscription
- **Severite:** IMPORTANT
- **Fichier(s):** `sql/create_database.sql:161-166` (colonnes telephone, adresse, code_postal, ville, date_inscription)
- **Type:** Colonne inutilisee
- **Description:** `UtilisateurDAL.Authenticate` ne selectionne que `id, nom, prenom, email, role, mot_de_passe`. Les colonnes `telephone`, `adresse`, `code_postal`, `ville`, `date_inscription`, `actif` (partiellement -- actif est dans le WHERE mais pas dans le Map) ne sont jamais lues ni exposees au C#. Cote Laravel, le model `User.php` existe mais n'est pas utilise (la table `users` a ete supprimee en v12 et `clients` est la table web).
- **Impact:** 5 colonnes inutilisees sur une table de 11 colonnes. Le profil utilisateur n'est pas exploite cote ERP.
- **Suggestion:** Documenter comme "reserve" pour un futur ecran de profil admin, ou simplifier la table si non prevu.

---

### [F-SQL-012] ENUM commandes_web.statut : valeur 'en_preparation' absente mais potentiellement attendue
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:409-410`
- **Type:** Pattern manque
- **Description:** L'ENUM de `commandes_web.statut` contient `'panier','payee','annulee'`. Le DAL C# (`CommandeWebDAL.GetAll`) filtre `WHERE cmd.statut <> 'panier'` et `GetCountByStatut` accepte n'importe quel statut en parametre. Pas de valeur intermediaire (ex: 'en_preparation', 'expediee', 'livree') pour le workflow de traitement des commandes.
- **Impact:** Le workflow de commande est rudimentaire (panier -> payee ou annulee). Pour un ERP, des etats intermediaires seraient utiles.
- **Suggestion:** Prevoir l'extension de l'ENUM pour la v2 : `ENUM('panier','payee','en_preparation','expediee','livree','annulee')`. Pas bloquant pour le MVP.

---

### [F-SQL-013] CHECK manquant sur bom_reservations.quantite_reservee
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:336`
- **Type:** Pattern manque
- **Description:** La colonne `bom_reservations.quantite_reservee` est un DECIMAL(12,4) NOT NULL sans contrainte CHECK. Les tables `lots_ingredients` et `bom_stocks` ont des CHECK `>= 0` sur `quantite_disponible` (v13), mais `bom_reservations` n'a aucun CHECK. Une reservation negative n'a pas de sens metier.
- **Impact:** Une reservation negative pourrait etre inseree par erreur, faussant les calculs de disponibilite dans la VIEW et le FIFO.
- **Suggestion:** `ALTER TABLE bom_reservations ADD CONSTRAINT chk_bomres_qte_positive CHECK (quantite_reservee > 0)`.

---

### [F-SQL-014] CHECK manquant sur bom_productions.quantite_produite
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:255`
- **Type:** Pattern manque
- **Description:** La colonne `bom_productions.quantite_produite` est un DECIMAL(10,4) NOT NULL sans CHECK. Le DAL C# (`BomProductionDAL.Executer`) calcule `qteProduite = quantiteCible * fiche.QuantiteOutput` qui devrait toujours etre positif, mais aucune contrainte DB ne l'impose.
- **Impact:** Faible -- le code C# valide cote application. Mais une insertion directe via phpMyAdmin pourrait introduire une valeur negative.
- **Suggestion:** `ALTER TABLE bom_productions ADD CONSTRAINT chk_bomprod_qte_positive CHECK (quantite_produite > 0)`.

---

### [F-SQL-015] CHECK manquant sur bom_fiches.quantite_output
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:209`
- **Type:** Pattern manque
- **Description:** `bom_fiches.quantite_output` (DECIMAL(10,4) NOT NULL DEFAULT 1) est utilise comme diviseur dans le calcul de cout unitaire (`coutUnitaire = qteProduite > 0 ? coutTotalIngredients / qteProduite : 0`). La valeur 0 est techniquement permise par le schema.
- **Impact:** Division par zero dans `BomCoutDAL.CalculerCout` si `QuantiteOutput = 0` (le code C# a un guard `> 0` mais la DB ne l'empeche pas).
- **Suggestion:** `ALTER TABLE bom_fiches ADD CONSTRAINT chk_bf_output_positive CHECK (quantite_output > 0)`.

---

### [F-SQL-016] reset_db_for_tests.sql reference des tables supprimees en v12
- **Severite:** MINEUR
- **Fichier(s):** `sql/tests/reset_db_for_tests.sql:25-84`
- **Type:** Dead code / Orphelin
- **Description:** Le script de reset reference des tables supprimees par la migration v12 : `mouvements_lots_ingredients`, `produits_parfums`, `produits`, `parfums`, `categories`, `pat_*` (16 tables), `selections_parfums`, `lignes_commandes`, `factures`, `commandes`, `zones_livraison`, `contacts`, `mouvements_stock_produits`, `stock_produits`, `productions_compositions`, `stock_compositions`, `frais_production_variables`, `productions_recettes`, `compositions_recettes`, `fiches_compositions`, `recettes_ingredients`, `fiches_recettes`, `frais_generaux_config`. L'execution de ce script echoue a cause de TRUNCATE sur des tables inexistantes.
- **Impact:** Le script de test n'est pas fonctionnel.
- **Suggestion:** Reecrire pour ne TRUNCATE que les 20 tables existantes. L'option `SET FOREIGN_KEY_CHECKS = 0` est deja presente, ce qui est correct.

---

### [F-SQL-017] injection_glacier_stress_test.sql -- fichier de test non-production
- **Severite:** MINEUR
- **Fichier(s):** `sql/tests/injection_glacier_stress_test.sql`
- **Type:** Documentation
- **Description:** Ce fichier est un jeu de donnees de stress test pour le scenario "Gelato di Marco". Il injecte 1 activite, 4 stocks, 7 fournisseurs, 25 fiches ingredients, 32 lots, 3 contextes, 12 niveaux et 18 fiches BOM. Il est coherent avec le schema post-v19 (utilise `nb_par_lot`, `id_stock` sur lots_ingredients). C'est le seul fichier de seed fonctionnel.
- **Impact:** Aucun -- fichier de test correctement place dans `sql/tests/`.
- **Suggestion:** Marquer clairement comme "test only" (deja fait via le commentaire d'en-tete). Ce fichier pourrait servir de base pour regenerer un `seed_data.sql` fonctionnel.

---

### [F-SQL-018] Migrations v01-v03 absentes du repertoire
- **Severite:** MINEUR
- **Fichier(s):** `sql/` (repertoire)
- **Type:** Documentation
- **Description:** Les fichiers de migration commencent a v04 (`migration_v04_bom.sql`). Les migrations v01, v02 et v03 sont absentes du repertoire. Le fichier `create_database.sql` mentionne "Etat final apres migrations v01..v19", impliquant que v01-v03 existaient mais ne sont plus presentes.
- **Impact:** Perte de tracabilite historique. Pour un deploiement from-scratch, `create_database.sql` couvre tout, mais l'historique incrementale est incomplete.
- **Suggestion:** Documenter dans un fichier CHANGELOG.md ou dans le JOURNAL que v01-v03 sont integrees dans le schema de base initial et ne sont plus necessaires incrementalement.

---

### [F-SQL-019] Pas de contrainte UNIQUE sur fournisseurs.nom
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:82-89`
- **Type:** Pattern manque
- **Description:** La table `fournisseurs` n'a pas de contrainte UNIQUE sur la colonne `nom`. Le DAL C# (`FournisseurDAL.NomExiste`) verifie l'unicite cote application (`SELECT COUNT(*) FROM fournisseurs WHERE nom = @nom AND id <> @id`), mais rien n'empeche un INSERT concurrent avec le meme nom via phpMyAdmin ou Laravel.
- **Impact:** Doublons possibles en cas d'insertion concurrente ou manuelle.
- **Suggestion:** `ALTER TABLE fournisseurs ADD UNIQUE KEY uk_fournisseur_nom (nom)`.

---

### [F-SQL-020] Pas de contrainte UNIQUE sur bom_contextes.nom (scope activite)
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:172-182`
- **Type:** Pattern manque
- **Description:** La table `bom_contextes` n'a pas de contrainte UNIQUE composite `(nom, id_activite)`. Le DAL C# (`BomContexteDAL.NomExiste`) verifie `WHERE nom = @nom AND id_activite = @idActivite AND id <> @id`, mais la DB ne l'impose pas.
- **Impact:** Deux contextes avec le meme nom dans la meme activite sont possibles par insertion directe.
- **Suggestion:** `ALTER TABLE bom_contextes ADD UNIQUE KEY uq_bomctx_nom_activite (nom, id_activite)`.

---

### [F-SQL-021] Colonne fiches_ingredients.date_creation jamais lue par le DAL C#
- **Severite:** MINEUR
- **Fichier(s):** `sql/create_database.sql:112`
- **Type:** Colonne inutilisee (partiel)
- **Description:** La colonne `fiches_ingredients.date_creation` est definie dans le schema mais jamais selectionnee ni mappee dans `IngredientDAL.cs`. Le model `Ingredient` en C# ne contient pas de propriete `DateCreation`.
- **Impact:** Perte d'information d'audit. La date de creation de chaque fiche ingredient est stockee mais jamais exploitee cote ERP.
- **Suggestion:** Ajouter au SELECT de `IngredientDAL` et au model si un affichage/tri par date est prevu.

---

## Resume par categorie

### Schema vs Code
| Aspect | Statut |
|--------|--------|
| create_database.sql vs DAL C# | OK -- coherent post-v19 |
| create_database.sql vs Models Laravel | OK -- les 8 models referent les bonnes tables/colonnes |
| schema_complet.sql vs create_database.sql | DIVERGENT (F-SQL-001, F-SQL-002, F-SQL-004) |
| vue_stock_global vs VueStockGlobalDAL | OK -- toutes les colonnes mappees |
| ENUM unite_mesure vs code C# | OK -- 8 valeurs coherentes |
| ENUM statut commandes_web vs code | OK -- 3 valeurs matchent |
| ENUM type_input bom_fiches_lignes vs code | OK -- 'ingredient'/'fiche' matchent |
| ENUM type_source bom_productions_lignes vs code | OK -- 'lot_ingredient'/'bom_stock' matchent |

### Index
| Table | Index explicites | Manquants recommandes |
|-------|------------------|----------------------|
| produits_web | idx_prodweb_envente, idx_prodweb_categorie | -- |
| commandes_web | idx_cmdweb_client_statut, idx_cmdweb_statut | -- |
| commandes_web_lignes | idx_cmdligne_commande | -- |
| bom_reservations | (FK implicites) | idx_bomres_lot_actif, idx_bomres_ctx_actif |
| lots_ingredients | (FK implicites) | idx_lot_fiche_achat (composite FIFO) |

### FK Coherence
| FK | Strategie | Correcte |
|----|-----------|----------|
| fk_lot_fiche (lots->fiches_ingredients) | CASCADE | Oui (suppression ingredient = suppression lots) |
| fk_lots_stock (lots->stocks) | DEFAULT (RESTRICT implicite) | A expliciter (F-SQL-008) |
| fk_lot_fournisseur (lots->fournisseurs) | SET NULL | Oui |
| fk_fi_fournisseur (fiches_ingredients->fournisseurs) | SET NULL | Oui |
| fk_bc_activite (bom_contextes->activites) | RESTRICT (create_db) / CASCADE (schema_complet) | Divergence (F-SQL-004) |
| fk_bn_contexte (bom_niveaux->bom_contextes) | CASCADE | Oui |
| fk_bf_niveau (bom_fiches->bom_niveaux) | RESTRICT | Oui (protege les fiches si niveau supprime) |
| fk_bp_niveau, fk_bp_fiche (productions) | RESTRICT | Oui (tracabilite) |
| fk_bs_* (bom_stocks) | RESTRICT | Oui (tracabilite) |
| fk_bpl_* (bom_productions_lignes) | CASCADE/RESTRICT | Oui |
| fk_br_lot (reservations->lots) | CASCADE | Oui |
| fk_br_contexte (reservations->contextes) | CASCADE | Oui |
| fk_cmdweb_client | RESTRICT | Oui |
| fk_cmdligne_cmd | CASCADE | Oui |
| fk_cmdligne_prodweb | RESTRICT | Oui |
| fk_prodweb_bomfiche | RESTRICT | Oui |
| fk_prodweb_categorie | SET NULL | Oui |

### CHECK Constraints
| Table | CHECK existant | CHECK manquant |
|-------|---------------|----------------|
| lots_ingredients | chk_lot_qte_positive (>= 0) | -- |
| bom_stocks | chk_bomstock_qte_positive (>= 0) | -- |
| commandes_web_lignes | chk_cmdligne_qte_positive (>= 1) | -- |
| bom_fiches_lignes | chk_bfl_input (XOR polymorphique) | -- |
| bom_productions_lignes | chk_bpl_source (XOR polymorphique) | -- |
| bom_reservations | -- | chk_bomres_qte_positive (> 0) |
| bom_productions | -- | chk_bomprod_qte_positive (> 0) |
| bom_fiches | -- | chk_bf_output_positive (> 0) |

### Fichiers tests
| Fichier | Statut | Fonctionnel |
|---------|--------|-------------|
| sql/tests/reset_db_for_tests.sql | NON-PRODUCTION | NON -- reference tables v12 supprimees |
| sql/tests/injection_glacier_stress_test.sql | NON-PRODUCTION | OUI -- coherent post-v19 |

### Colonnes inutilisees (presentes en DB, jamais SELECT dans aucun DAL)
| Table | Colonne | Commentaire |
|-------|---------|-------------|
| fiches_ingredients | dlc_jours_reference | Jamais lue/ecrite par C# ni Laravel |
| fiches_ingredients | qualite_label | Jamais lue/ecrite par C# ni Laravel |
| fiches_ingredients | date_creation | Jamais lue par IngredientDAL |
| utilisateurs | telephone | Jamais lu par UtilisateurDAL |
| utilisateurs | adresse | Jamais lu par UtilisateurDAL |
| utilisateurs | code_postal | Jamais lu par UtilisateurDAL |
| utilisateurs | ville | Jamais lu par UtilisateurDAL |
| utilisateurs | date_inscription | Jamais lu par UtilisateurDAL |

### Migrations
| Migration | Sequentielle | Conflit |
|-----------|-------------|---------|
| v01-v03 | Absentes (integrees au schema initial) | -- |
| v04 | OK | -- |
| v05 | OK | -- |
| v06 | OK | -- |
| v07 | OK (purge + ENUM->FK) | -- |
| v08 | OK | -- |
| v09 | OK (CASCADE + ENUM) | -- |
| v10 | OK (stocks decouplage) | -- |
| v11 | OK (discriminants + VIEW) | -- |
| v12 | OK (cleanup massif) | -- |
| v13 | OK (CHECK constraints) | -- |
| v14 | OK (stock_cible ingredients) | -- |
| v15 | OK (boutique web 5 tables) | -- |
| v16 | OK (stock_cible bom_fiches) | -- |
| v17 | OK (VIEW id_fiche_ingredient) | -- |
| v18 | OK (id_stock move fiche->lot) | -- |
| v19 | OK (nb_par_lot) | -- |

**Pas de conflit de numerotation.** Sequence v04-v19 continue et correcte.

---

## Priorites de correction

### P0 -- Immediat (avant defense)
1. **F-SQL-001/002** : Soit supprimer `schema_complet.sql`, soit le regenerer depuis `create_database.sql`
2. **F-SQL-003** : Reecrire `seed_data.sql` pour le schema actuel

### P1 -- Important (sprint courant)
3. **F-SQL-008** : Expliciter `ON DELETE RESTRICT ON UPDATE CASCADE` sur `fk_lots_stock`
4. **F-SQL-004** : Aligner la FK `fk_bc_activite` entre les fichiers
5. **F-SQL-016** : Reecrire `reset_db_for_tests.sql` pour les 20 tables actuelles

### P2 -- Amelioration (backlog)
6. **F-SQL-007** : Ajouter les index sur bom_reservations
7. **F-SQL-005** : Index composite FIFO sur lots_ingredients
8. **F-SQL-013/014/015** : Ajouter les CHECK manquants
9. **F-SQL-019/020** : UNIQUE manquants sur fournisseurs et bom_contextes
10. **F-SQL-009/010/011/021** : Decider du sort des colonnes inutilisees
