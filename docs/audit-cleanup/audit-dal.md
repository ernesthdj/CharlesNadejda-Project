# Audit DAL — ArtisaStock
> Date: 2026-06-11 | Agent: DAL Auditor (#1)
> Branche: `feat/refactoring-sprints-p0-p3`
> Fichiers audites: 19 DAL + 21 Models + schema_complet.sql

---

## Resume

**31 findings** (3 critiques, 11 importants, 17 mineurs)

| Severite  | Count |
|-----------|-------|
| CRITIQUE  | 3     |
| IMPORTANT | 11    |
| MINEUR    | 17    |

---

## Findings

---

### [F-DAL-001] schema_complet.sql desynchronise — colonne `nb_par_lot` absente
- **Severite:** CRITIQUE
- **Fichier(s):** `sql/schema_complet.sql:65-86`, `sql/migration_v19_nb_par_lot.sql`
- **Type:** Incoherence
- **Description:** La colonne `nb_par_lot` est utilisee dans `IngredientDAL.cs` (SELECT ligne 23, INSERT ligne 93, UPDATE ligne 115, Bind ligne 169, Map ligne 187) et existe dans `migration_v19_nb_par_lot.sql`, mais elle est absente de `schema_complet.sql`. Ce fichier est donc desynchronise post-migration v19.
- **Impact:** Quiconque recreant la base depuis `schema_complet.sql` aura une erreur SQL immediate. Le fichier de reference du schema est menteur.
- **Suggestion:** Regenerer `schema_complet.sql` depuis la base live ou appliquer manuellement l'ajout `nb_par_lot INT NOT NULL DEFAULT 1 AFTER qte_par_conditionnement` dans la definition de `fiches_ingredients`.

---

### [F-DAL-002] CategorieWebDAL utilise `SELECT c.*` — fragile et non-explicite
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/CategorieWebDAL.cs:19,38`
- **Type:** Pattern manque
- **Description:** `GetAll()` et `GetById()` utilisent `SELECT c.*, COUNT(p.id) AS nb_produits` au lieu de lister explicitement les colonnes. Le `Map()` ne lit pas `date_creation` (la propriete n'existe pas dans le model `CategorieWeb`), ce qui fonctionne car le champ est simplement ignore. Mais si une colonne est ajoutee a la table, le `*` la ramene silencieusement sans la mapper.
- **Impact:** Incompatible avec le pattern utilise dans tous les autres DAL (colonnes explicites). Si une colonne avec un nom ambigu est ajoutee, risque de conflit avec l'alias `nb_produits`. Le model `CategorieWeb` ne mappe pas `date_creation` alors que la table l'a.
- **Suggestion:** Remplacer `c.*` par la liste explicite `c.id, c.nom, c.description, c.ordre_affichage, c.actif` et decider si `date_creation` doit etre mappe dans le model.

---

### [F-DAL-003] StockDAL.Delete — double verification redondante
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/StockDAL.cs:106-128`
- **Type:** Redondance
- **Description:** `Delete()` fait deux checks successifs sur `lots_ingredients WHERE id_stock = @id` :
  1. `COUNT(*)` de tous les lots (ligne 106)
  2. `COUNT(*)` des lots avec `quantite_disponible > 0` (ligne 115)
  Le premier check est un surensemble du second — si le check 1 passe (nb > 0), le delete est deja bloque, rendant le check 2 impossible a atteindre dans un scenario ou il y a des lots consommes mais aucun lot avec du stock restant.
- **Impact:** Code mort partiel. La methode `ContientDonnees()` (ligne 182) fait la meme double verification dans une seule requete. Confusion sur le garde-fou exact.
- **Suggestion:** Unifier en un seul check : soit "il y a des lots" (tout court), soit "il y a des lots avec stock > 0" selon la regle metier voulue. La methode `ContientDonnees()` semble faire double emploi avec `Delete()`.

---

### [F-DAL-004] BomProductionDAL.Executer — lectures hors transaction (GetById sur niveau et fiche)
- **Severite:** CRITIQUE
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs:350-351`
- **Type:** Securite
- **Description:** Dans `Executer()`, les appels `BomNiveauDAL.GetById(idNiveau)` et `BomFicheDAL.GetById(idFiche)` ouvrent chacun leur propre connexion independante (via `DbHelper.GetConnection()`), en dehors de la transaction `tx` deja ouverte. Les donnees de la fiche (notamment ses lignes et `QuantiteOutput`) sont lues hors du scope transactionnel, alors que les operations FIFO qui suivent sont dans la transaction.
- **Impact:** Race condition theorique (TOCTOU) : si la fiche est modifiee (lignes changees) entre la lecture et l'execution FIFO, la production utilise des donnees obsoletes. Le probleme est attenue par le fait que les verifs de stock sont, elles, bien dans la transaction avec FOR UPDATE.
- **Suggestion:** Ajouter des surcharges transactionnelles a `BomNiveauDAL.GetById` et `BomFicheDAL.GetById` acceptant `(conn, tx)`, et les appeler dans le scope de la transaction existante.

---

### [F-DAL-005] IngredientDAL — colonne `dlc_jours_reference` du schema jamais lue ni ecrite
- **Severite:** MINEUR
- **Fichier(s):** `sql/schema_complet.sql:76` (colonne `dlc_jours_reference INT`), `app-csharp/CharlesNadejda/CharlesNadejda/DAL/IngredientDAL.cs`
- **Type:** Orphelin
- **Description:** La table `fiches_ingredients` a une colonne `dlc_jours_reference` (schema ligne 76), mais IngredientDAL ne la SELECT, INSERT ni UPDATE. Le model `Ingredient.cs` n'a pas cette propriete non plus.
- **Impact:** Colonne morte en base. Si elle est prevue pour une fonctionnalite future (calcul DLC auto), elle est pour l'instant inaccessible depuis le C#.
- **Suggestion:** Soit ajouter le champ au model et au DAL, soit supprimer la colonne par migration si la fonctionnalite n'est pas prevue.

---

### [F-DAL-006] IngredientDAL — colonne `qualite_label` du schema jamais lue ni ecrite
- **Severite:** MINEUR
- **Fichier(s):** `sql/schema_complet.sql:77` (colonne `qualite_label VARCHAR(100)`), `app-csharp/CharlesNadejda/CharlesNadejda/DAL/IngredientDAL.cs`
- **Type:** Orphelin
- **Description:** Meme probleme que `dlc_jours_reference`. La colonne `qualite_label` existe en DB mais n'est ni lue ni ecrite par aucun DAL. Aucune propriete correspondante dans le model.
- **Impact:** Colonne morte.
- **Suggestion:** Ajouter au model si necessaire pour la defense, ou supprimer par migration.

---

### [F-DAL-007] IngredientDAL — colonne `date_creation` du schema jamais lue
- **Severite:** MINEUR
- **Fichier(s):** `sql/schema_complet.sql:82`, `app-csharp/CharlesNadejda/CharlesNadejda/DAL/IngredientDAL.cs`
- **Type:** Orphelin
- **Description:** `fiches_ingredients.date_creation` n'est pas dans le SELECT de `GetAll()` ni `GetById()`, ni dans le model `Ingredient`. Contrairement aux autres entites (Activite, Stock, BomFiche...) qui mappent toutes `DateCreation`.
- **Impact:** Inconsistence inter-DAL. Si un formulaire a besoin d'afficher la date de creation, il faudra modifier le DAL.
- **Suggestion:** Ajouter `date_creation` au SELECT et au model pour coherence.

---

### [F-DAL-008] BomReservationDAL.Update — n'utilise pas Bind()
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomReservationDAL.cs:66-79`
- **Type:** Incoherence
- **Description:** `Insert()` utilise `Bind(cmd, res)` pour ajouter les parametres, mais `Update()` ajoute les parametres manuellement sans passer par `Bind()`. De plus, `Update()` ne met a jour que `quantite_reservee` et `notes`, pas `id_lot` ni `id_contexte` — ce qui peut etre voulu (on ne change pas la cible d'une reservation) mais n'est pas documente.
- **Impact:** Si la signature de `Bind()` evolue (ajout de parametres), `Update()` ne beneficiera pas du changement. Risque de divergence.
- **Suggestion:** Documenter que `Update()` ne modifie intentionnellement que qte+notes, ou utiliser `Bind()` avec les champs supplementaires.

---

### [F-DAL-009] BomNiveauDAL.Update — n'utilise pas Bind()
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomNiveauDAL.cs:76-86`
- **Type:** Incoherence
- **Description:** `Insert()` utilise `Bind(cmd, n)` qui ajoute `@idCtx, @nom, @desc`. Mais `Update()` ajoute manuellement `@nom` et `@desc` sans passer par `Bind()`, et n'utilise pas `@idCtx` car il ne met pas a jour `id_contexte` (ce qui est correct — on ne deplace pas un niveau entre contextes). Mais le pattern est inconsistant avec les autres DAL.
- **Impact:** Risque mineur de divergence si `Bind()` evolue.
- **Suggestion:** Soit utiliser `Bind()` + ignorer `@idCtx`, soit documenter l'intentionnalite.

---

### [F-DAL-010] BomContexteDAL.Delete — multi-requetes sans transaction
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomContexteDAL.cs:137-169`
- **Type:** Pattern manque
- **Description:** `Delete()` fait 3 requetes sequentielles (check productions, check stocks, DELETE) sur la meme connexion mais SANS transaction. Entre le check et le DELETE, un autre process pourrait ajouter une production.
- **Impact:** Race condition theorique. L'impact est faible (operation admin peu frequente), mais le pattern est incorrect comparativement a `ActiviteDAL.Desactiver()` qui utilise correctement une transaction pour ses checks similaires.
- **Suggestion:** Envelopper dans `BeginTransaction()`/`Commit()` comme `ActiviteDAL.Desactiver()`.

---

### [F-DAL-011] BomFicheDAL.Delete — multi-requetes sans transaction
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomFicheDAL.cs:188-217`
- **Type:** Pattern manque
- **Description:** Meme probleme que F-DAL-010. `Delete()` fait 3 requetes (check references, check productions, DELETE) sans transaction.
- **Impact:** Race condition theorique entre le check et le DELETE.
- **Suggestion:** Envelopper dans une transaction.

---

### [F-DAL-012] IngredientDAL.Delete — multi-requetes sans transaction
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/IngredientDAL.cs:126-156`
- **Type:** Pattern manque
- **Description:** Meme pattern que F-DAL-010 et F-DAL-011. Trois requetes (check lots actifs, check fiches BOM, DELETE) sans transaction.
- **Impact:** Race condition theorique.
- **Suggestion:** Envelopper dans une transaction.

---

### [F-DAL-013] LotDAL.Delete — multi-requetes sans transaction
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/LotDAL.cs:130-150`
- **Type:** Pattern manque
- **Description:** Deux requetes (check consommation, DELETE) sans transaction.
- **Impact:** Race condition theorique entre le check et le DELETE.
- **Suggestion:** Envelopper dans une transaction.

---

### [F-DAL-014] Requetes SELECT redondantes dans BomProductionDAL (GetByNiveau, GetRecentByActivite, GetDuJourByActivite)
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs:41-52,70-82,103-115`
- **Type:** Redondance
- **Description:** Les trois methodes `GetByNiveau()`, `GetRecentByActivite()`, et `GetDuJourByActivite()` repetent le meme bloc SELECT (14 colonnes, 4 JOINs) en dur. Il n'y a pas de constante `SELECT_BASE` comme dans les autres DAL (BomContexteDAL, BomFicheDAL, BomNiveauDAL, LotDAL, etc.).
- **Impact:** Si une colonne est ajoutee au mapping, il faut modifier 3 endroits. Risque d'oubli eleve.
- **Suggestion:** Extraire une constante `SELECT_BASE` et ne varier que le WHERE/ORDER BY/LIMIT, comme dans les autres DAL.

---

### [F-DAL-015] BomStockDAL.GetByNiveau — sous-requete correlee dans le SELECT
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomStockDAL.cs:23-26`
- **Type:** Redondance
- **Description:** La sous-requete `COALESCE((SELECT SUM(s2.quantite_disponible) FROM bom_stocks s2 WHERE s2.id_fiche = s.id_fiche AND s2.quantite_disponible > 0), 0) AS total_dispo_fiche` est une sous-requete correlee executee pour chaque ligne. Si une fiche a N lots en stock, la sous-requete est executee N fois pour le meme resultat.
- **Impact:** Performance potentiellement degradee sur des volumes importants. Pas critique vu les volumes actuels d'une patisserie.
- **Suggestion:** Utiliser un `LEFT JOIN` sur une sous-requete groupee ou un `OVER(PARTITION BY)` window function.

---

### [F-DAL-016] BomStockDAL.GetLotsDispoFIFO (non-transactionnel) — HAVING sur alias calcule
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomStockDAL.cs:157`
- **Type:** Incoherence
- **Description:** La version non-transactionnelle de `GetLotsDispoFIFO()` utilise `HAVING dispo_nette > 0` pour filtrer en SQL, tandis que la version transactionnelle (ligne 191-197) fait le filtrage cote C# avec `if (dispo > 0)`. Les deux approches fonctionnent mais sont inconsistantes.
- **Impact:** Comportement fonctionnellement identique, mais la difference rend le code plus difficile a comprendre. Le `HAVING` sans `GROUP BY` fonctionne par tolerance MySQL.
- **Suggestion:** Aligner les deux versions sur la meme approche (preferer le filtrage C# car le `HAVING` sans `GROUP BY` est un pattern non-standard).

---

### [F-DAL-017] UtilisateurDAL — mapping incomplet du model
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/UtilisateurDAL.cs:15-16`
- **Type:** Orphelin
- **Description:** Le SELECT ne recupere que `id, nom, prenom, email, role, mot_de_passe`. Le model `Utilisateur` n'a que `Id, Nom, Prenom, Email, Role`. La table `utilisateurs` contient aussi `telephone, adresse, code_postal, ville, date_inscription, actif` — aucune de ces colonnes n'est jamais lue par le DAL ni exposee dans le model.
- **Impact:** C'est acceptable car le DAL est readonly (authentification uniquement). Mais si l'admin devait gerer les utilisateurs depuis l'ERP, il faudrait un CRUD complet.
- **Suggestion:** Rien a faire si le scope reste l'auth. Documenter que c'est intentionnel.

---

### [F-DAL-018] DbHelper.GetConnection — connexion ouverte dans le constructeur
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/DbHelper.cs:8-14`
- **Type:** Pattern manque
- **Description:** `GetConnection()` fait `conn.Open()` avant de retourner. Tous les appelants font `using (var conn = DbHelper.GetConnection())` ce qui est correct. Mais le pattern "open dans le helper" signifie qu'une exception dans `Open()` laisse la connexion non-disposee (le `new MySqlConnection` est cree, `Open()` echoue, et le `using` de l'appelant n'a jamais recu l'objet).
- **Impact:** Fuite de connexion potentielle en cas d'echec de `Open()` (MySQL down, timeout reseau). En pratique, le garbage collector finira par collecter, mais c'est un pattern fragile.
- **Suggestion:** Retourner la connexion non-ouverte et laisser l'appelant faire `conn.Open()`, ou utiliser un try/catch dans `GetConnection()` qui dispose en cas d'erreur.

---

### [F-DAL-019] ProduitWebDAL.Update — ne met pas a jour `id_bom_fiche`
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/ProduitWebDAL.cs:131-137`
- **Type:** Incoherence
- **Description:** `Insert()` inclut `id_bom_fiche` via `@idFiche` (Bind, ligne 191), mais `Update()` ne le SET pas dans le SQL (ligne 133 ne contient pas `id_bom_fiche`). Le `Bind()` ajoute quand meme `@idFiche` en parametre mais il n'est pas utilise dans le UPDATE.
- **Impact:** Un produit web ne peut pas etre rattache a une autre fiche BOM apres creation. C'est probablement voulu (la contrainte UNIQUE `uk_prodweb_fiche` impose un 1:1 fiche-produit), mais le parametre orphelin `@idFiche` est envoye inutilement a MySQL.
- **Suggestion:** Documenter l'intentionnalite, ou separer le `Bind()` en `BindInsert`/`BindUpdate` pour eviter le parametre orphelin.

---

### [F-DAL-020] Nommage parametre inconsistant : @desc vs @description
- **Severite:** MINEUR
- **Fichier(s):** Tous les DAL utilisant `@desc` comme alias pour la colonne `description`
- **Type:** Naming
- **Description:** Tous les DAL utilisent `@desc` comme nom de parametre pour la colonne `description` (ActiviteDAL, BomContexteDAL, BomFicheDAL, BomNiveauDAL, CategorieWebDAL, FournisseurDAL, IngredientDAL, etc.). C'est coherent entre eux mais diverge du nom de colonne reel.
- **Impact:** Negligeable — c'est un choix de convention. La coherence interne est respectee. Mentionne pour exhaustivite.
- **Suggestion:** Aucune action requise. La convention `@desc` est uniformement appliquee.

---

### [F-DAL-021] Nommage parametre inconsistant entre DAL : @tel vs @telephone
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/FournisseurDAL.cs:78`
- **Type:** Naming
- **Description:** FournisseurDAL utilise `@tel` pour la colonne `telephone`. C'est le seul DAL avec ce champ, mais le raccourcissement est inconsistant avec d'autres qui gardent le nom complet (ex: `@adresse` est utilise tel quel).
- **Impact:** Negligeable.
- **Suggestion:** Aucune action urgente.

---

### [F-DAL-022] BomCoutDAL.CalculerLigneFiche — copie du HashSet a chaque appel recursif
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomCoutDAL.cs:174-175`
- **Type:** Incoherence
- **Description:** Ligne 175 : `new HashSet<int>(fichesVisitees)` cree une copie du HashSet pour chaque appel recursif sur une sous-fiche. Cela signifie que le meme ID de fiche peut apparaitre dans deux branches paralleles de l'arbre sans declencher la detection de cycle. C'est probablement voulu (un meme ingredient peut etre utilise dans 2 sous-fiches differentes), MAIS cela casse partiellement la protection TICKET-09 contre les cycles indirects (A->B->C->A ne sera detecte que si le cycle est sur une seule branche lineaire).
- **Impact:** Un cycle indirect traversant plusieurs branches ne sera pas detecte. En pratique, les cycles BOM sont rares et la DB elle-meme les empeche, mais la protection en code est incomplete.
- **Suggestion:** Passer le meme HashSet (pas une copie) pour une detection stricte des cycles, en s'assurant de retirer l'ID apres le retour recursif (`fichesVisitees.Remove(idFiche)` apres l'appel).

---

### [F-DAL-023] VueStockGlobalDAL — pas de SELECT_BASE partage pour GetByContexte
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/VueStockGlobalDAL.cs:54-64`
- **Type:** Redondance
- **Description:** `GetByContexte()` reecrit le SELECT complet au lieu d'utiliser `SELECT_BASE`. C'est parce qu'il ajoute un `JOIN bom_reservations` qui n'est pas dans le SELECT_BASE. Mais le fragment `SELECT vsg.*, a.nom AS nom_activite FROM vue_stock_global vsg LEFT JOIN activites a ON a.id = vsg.id_activite` est duplique.
- **Impact:** Si le mapping de la vue change, il faut mettre a jour deux endroits.
- **Suggestion:** Extraire le fragment commun ou accepter la duplication comme necessaire pour la lisibilite.

---

### [F-DAL-024] ActiviteDAL — pas de methode GetById avec stocks lies
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/ActiviteDAL.cs`
- **Type:** Orphelin
- **Description:** La table `activites_stocks` (M:N) lie activites et stocks. `StockDAL.GetByActivite()` et `StockDAL.GetActivitesLiees()` exploitent cette relation, mais `ActiviteDAL` n'a aucune methode pour charger les stocks lies a une activite. Tout passe par `StockDAL`.
- **Impact:** Aucun impact fonctionnel — l'information est accessible via StockDAL. C'est une observation architecturale.
- **Suggestion:** Aucune action requise. La responsabilite est correctement placee dans StockDAL.

---

### [F-DAL-025] CommandeWebDAL — DAL lecture seule, pas de methode pour changer le statut
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/CommandeWebDAL.cs`
- **Type:** Orphelin
- **Description:** Le DAL est strictement readonly (GetAll, GetById, GetCountByStatut). Aucune methode pour modifier le statut d'une commande (ex: passer de `payee` a `expediee`). Le commentaire precise que les commandes sont creees par Laravel, ce qui est correct. Mais si l'admin ERP doit gerer les expeditions, il faudra un `UpdateStatut()`.
- **Impact:** Limitation fonctionnelle potentielle pour le module d'expedition.
- **Suggestion:** Si le workflow de commandes passe uniquement par Laravel, aucune action. Sinon, ajouter un `UpdateStatut(int id, string statut)`.

---

### [F-DAL-026] LotDAL.Map — cast `(int)r["nb_par_lot"]` sans DBNull check
- **Severite:** CRITIQUE
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/LotDAL.cs:17-19` (SELECT), `LotDAL.cs:187` (Map, le cast `NbParLot` n'est PAS dans LotDAL mais dans IngredientDAL)
- **Type:** Incoherence
- **Description:** `LotDAL` SELECT `fi.qte_par_conditionnement` (ligne 17) mais ne SELECT PAS `fi.nb_par_lot`. Or le model `Lot` n'a pas de champ `NbParLot` — c'est uniquement dans `Ingredient`. Pas de probleme ici.

  MAIS dans `IngredientDAL.cs:23`, le SELECT inclut `fi.nb_par_lot` et le Map (ligne 187) fait `NbParLot = (int)r["nb_par_lot"]`. La colonne `nb_par_lot` est definie avec `NOT NULL DEFAULT 1` en migration v19, mais `schema_complet.sql` ne l'a pas. Si quelqu'un cree la base depuis schema_complet.sql, ce SELECT plantera avec une `IndexOutOfRangeException` car la colonne n'existe pas.
- **Impact:** Erreur runtime si la base est creee sans la migration v19. Lie directement a F-DAL-001.
- **Suggestion:** Corriger schema_complet.sql (cf F-DAL-001). Le code DAL est correct par rapport a la base reelle (post-migration).

---

### [F-DAL-027] BomProductionDAL.MapHeader — DBNull check inconsistant
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs:648-664`
- **Type:** Incoherence
- **Description:** Le `MapHeader()` de BomProductionDAL fait `r["unite_output"] == DBNull.Value ? "" : r["unite_output"].ToString()` (ligne 659) et `r["quantite_output"] == DBNull.Value ? 0 : Convert.ToDecimal(...)` (ligne 660). Mais dans le schema, `bom_fiches.unite_output` est `NOT NULL DEFAULT 'piece'` et `bom_fiches.quantite_output` est `NOT NULL DEFAULT 1.0000`. Ces colonnes ne peuvent JAMAIS etre DBNull. Les checks sont superflus.
- **Impact:** Code defensif inutile. Pas de bug, mais induit en erreur le lecteur sur la nullabilite.
- **Suggestion:** Supprimer les checks DBNull sur les colonnes NOT NULL, ou les garder par securite defensive (choix d'equipe).

---

### [F-DAL-028] BomFicheLigneDAL — pas de methode Insert/Update/Delete independante
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomFicheLigneDAL.cs`
- **Type:** Orphelin
- **Description:** BomFicheLigneDAL n'a que des methodes de lecture (`GetByFiche`, `GetFichesUtilisant`, `GetFichesConsommant`). L'insertion des lignes est faite par `BomFicheDAL.InsertLignes()` (methode `internal static`). Il n'y a pas de CRUD autonome pour les lignes.
- **Impact:** Architecture acceptable — les lignes sont gerees comme partie de la fiche (aggregate root pattern). Mais si on a besoin de modifier une ligne individuellement, il faudra tout re-supprimer/re-inserer (ce qui est le pattern actuel dans `BomFicheDAL.Update()`).
- **Suggestion:** Aucune action requise. Le pattern delete-all + re-insert est correct pour un aggregate root.

---

### [F-DAL-029] BomStockDAL — methodes GetLotsDispoFIFO et GetBomStocksFIFO dupliquees (avec/sans transaction)
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomStockDAL.cs:141-201` et `207-253`
- **Type:** Redondance
- **Description:** Chaque methode FIFO existe en deux versions : une autonome (nouvelle connexion) et une transactionnelle (conn+tx). Le SQL est quasi-identique (seule difference : `FOR UPDATE` et le filtrage). C'est systematique dans BomStockDAL : `GetDisponible`, `GetDisponibleIngredient`, `GetLotsDispoFIFO`, `GetBomStocksFIFO` — 4 methodes x 2 = 8 methodes au total.
- **Impact:** 8 blocs SQL a maintenir en parallele. Si une requete est modifiee, il faut toujours penser a l'autre version.
- **Suggestion:** Extraire une methode privee commune acceptant `(conn, tx)` en optionnel (nullable), et faire la version publique sans transaction appeler la version avec en creant sa propre connexion.

---

### [F-DAL-030] BomProductionDAL.VerifierDisponibiliteLignes — dupliquee (avec/sans transaction)
- **Severite:** IMPORTANT
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs:146-202` et `211-262`
- **Type:** Redondance
- **Description:** Meme pattern que F-DAL-029 mais dans BomProductionDAL. La logique de verification est dupliquee entre la version standalone et la version transactionnelle. 50+ lignes de code quasi-identiques.
- **Impact:** Maintenance double. Si la logique de verification evolue, il faut modifier les deux versions.
- **Suggestion:** Fusionner en une seule methode acceptant `(conn, tx)` optionnels.

---

### [F-DAL-031] BomProductionDAL.GetIdNiveauDeFiche — dupliquee (avec/sans transaction)
- **Severite:** MINEUR
- **Fichier(s):** `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs:617-643`
- **Type:** Redondance
- **Description:** Deux methodes identiques sauf `cmd.Transaction = tx`. Pattern recurrent (cf F-DAL-029, F-DAL-030).
- **Impact:** Mineur — la methode est simple (2 lignes de logique), mais la duplication est systematique.
- **Suggestion:** Unifier avec parametre optionnel.

---

## Synthese par categorie

### Transactions manquantes (4 findings)
| Finding | DAL | Methode |
|---------|-----|---------|
| F-DAL-010 | BomContexteDAL | Delete() |
| F-DAL-011 | BomFicheDAL | Delete() |
| F-DAL-012 | IngredientDAL | Delete() |
| F-DAL-013 | LotDAL | Delete() |

**Pattern commun :** Check-then-act sans transaction = race condition theorique.

### Duplications code (4 findings)
| Finding | Fichier | Nb doublons |
|---------|---------|-------------|
| F-DAL-014 | BomProductionDAL | 3 SELECT identiques |
| F-DAL-029 | BomStockDAL | 4 methodes x2 (8 total) |
| F-DAL-030 | BomProductionDAL | VerifierDisponibiliteLignes x2 |
| F-DAL-031 | BomProductionDAL | GetIdNiveauDeFiche x2 |

**Volume estime :** ~200 lignes de code dupliquees factorisables.

### Colonnes orphelines (3 findings)
| Finding | Colonne | Table |
|---------|---------|-------|
| F-DAL-005 | dlc_jours_reference | fiches_ingredients |
| F-DAL-006 | qualite_label | fiches_ingredients |
| F-DAL-007 | date_creation | fiches_ingredients |

---

## Matrice couverture DAL vs Schema

| Table | SELECT | INSERT | UPDATE | DELETE | Transaction | Notes |
|-------|--------|--------|--------|--------|-------------|-------|
| activites | OK | OK | OK | OK | Desactiver() | Complet |
| stocks | OK | OK | OK | OK | Non (Delete) | F-DAL-003 |
| activites_stocks | OK (via StockDAL) | OK | - | OK | Non | M:N |
| fournisseurs | OK | OK | OK | OK | Non | Complet |
| fiches_ingredients | OK (-3 cols) | OK (-2 cols) | OK (-2 cols) | OK | Non (Delete) | F-DAL-005/006/007 |
| lots_ingredients | OK | OK | OK | OK | Non (Delete) | F-DAL-013 |
| bom_contextes | OK | OK (tx) | OK | OK | InsertAvecNiveaux | F-DAL-010 |
| bom_niveaux | OK | OK | OK | OK | Non | OK |
| bom_fiches | OK | OK (tx) | OK (tx) | OK | Insert/Update | F-DAL-011 |
| bom_fiches_lignes | OK | OK (via BomFicheDAL) | - | - | Via parent | Aggregate |
| bom_productions | OK | OK (tx) | OK (tx) | - | Executer() | OK |
| bom_productions_lignes | - | OK (via Executer) | - | - | Via parent | Tracabilite |
| bom_stocks | OK | OK (via Executer) | OK (via Executer) | - | Via parent | F-DAL-015/029 |
| bom_reservations | OK | OK | OK | - | Non | Soft delete |
| categories_web | OK | OK | OK | OK | Non | F-DAL-002 |
| produits_web | OK | OK | OK | OK | Non | F-DAL-019 |
| commandes_web | OK | - | - | - | - | Readonly |
| commandes_web_lignes | OK (via GetById) | - | - | - | - | Readonly |
| clients | - | - | - | - | - | Laravel only |
| utilisateurs | Auth only | - | - | - | - | F-DAL-017 |
| vue_stock_global | OK | - | - | - | - | VIEW |

---

## Priorites de correction suggerees

### P0 — Critiques (corriger avant defense)
1. **F-DAL-001/F-DAL-026** — Resynchroniser `schema_complet.sql` avec la base reelle (ajouter `nb_par_lot`)
2. **F-DAL-004** — Lire niveau+fiche dans la transaction de `Executer()` (surcharges transactionnelles)

### P1 — Importants (corriger si temps disponible)
3. **F-DAL-010/011/012/013** — Envelopper les Delete() a multi-checks dans des transactions
4. **F-DAL-014** — Extraire `SELECT_BASE` dans BomProductionDAL
5. **F-DAL-029/030/031** — Factoriser les doublons tx/non-tx (BomStockDAL, BomProductionDAL)
6. **F-DAL-002** — Remplacer `SELECT c.*` par colonnes explicites dans CategorieWebDAL
7. **F-DAL-018** — Securiser `DbHelper.GetConnection()` contre les fuites en cas d'echec Open()
8. **F-DAL-022** — Corriger la detection de cycle dans BomCoutDAL

### P2 — Mineurs (nice to have)
9. F-DAL-005/006/007 — Colonnes orphelines (ajouter ou supprimer)
10. Reste des findings mineurs (naming, DBNull defensif, documentation)
