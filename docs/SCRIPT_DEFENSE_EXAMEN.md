# SCRIPT DE DEFENSE -- EXAMEN ORAL PDSGBD + PDWEB

> **Projet** : ArtisaStock (C# WinForms) + Boutique Web (Laravel 11)
> **Etudiant** : mentalyas -- 2e annee Bachelier Informatique
> **Format** : Discord screen share, demo live end-to-end
> **Deux examens en une session** : PDSGBD (C#/SQL) et PDWEB (Laravel/PHP)

---

## INTRODUCTION (30 secondes)

**CE QUE TU DIS :**

> "Mon projet s'appelle ArtisaStock. C'est un systeme de gestion pour un artisan chocolatier -- mes parents.
> Il y a deux parties : une application desktop en C# WinForms pour gerer les stocks, les recettes et la production,
> et un site web en Laravel pour que les clients puissent acheter en ligne.
> Les deux partagent la meme base de donnees MySQL, ce qui fait que quand on produit dans le C#,
> le stock est mis a jour en temps reel sur le site web. Je vais montrer le workflow complet de A a Z."

---

## ARCHITECTURE GLOBALE (1 minute)

### Ce que tu montres

Ouvre un schema (dessine-le a l'avance ou montre le texte ci-dessous) :

```
   ArtisaStock (C# WinForms)                    Boutique Web (Laravel 11)
  +----------------------------+                +----------------------------+
  | Forms (UI WinForms)        |                | Controllers (PHP)          |
  | Models (POCO classes)      |                | Models Eloquent            |
  | DAL (static, SQL param.)   |                | Blade Views + Tailwind     |
  | Helpers (UnitConvertisseur)|                | JS (AJAX panier)           |
  +----------------------------+                +----------------------------+
              |                                             |
              +----------------- MySQL 8 ------------------+
                          (base charlesnadejda)
                    meme DB, meme tables, temps reel
```

### Ce que tu dis

> "L'architecture est simple mais efficace. Cote C#, j'ai une separation en trois couches :
> les Forms pour l'interface, les Models qui representent les entites metier, et la DAL --
> Data Access Layer, c'est-a-dire la couche qui centralise tous les acces a la base de donnees.
> Toutes les methodes de la DAL sont statiques et utilisent des requetes parametrees,
> ce qui protege contre l'injection SQL.
>
> Cote Laravel, j'utilise l'ORM Eloquent qui genere automatiquement des requetes parametrees via PDO.
> Les deux applications partagent la meme base MySQL, donc quand je produis un lot dans le C#,
> le stock est immediatement visible sur le site web."

### Questions probables

**Q : Pourquoi la DAL est statique ?**
> "C'est un choix de simplicite pour une application mono-utilisateur. Chaque methode ouvre sa propre connexion,
> execute la requete, et ferme la connexion. Pas besoin de gerer l'etat d'un objet DAL.
> Dans une vraie application multi-utilisateurs, j'aurais plutot utilise le pattern Repository avec injection de dependances."

**Q : Comment les deux apps communiquent ?**
> "Elles ne communiquent pas directement entre elles. Elles partagent simplement la meme base de donnees MySQL.
> Le C# ecrit dans les tables `bom_stocks`, `produits_web`, etc., et Laravel les lit en temps reel via Eloquent.
> C'est le pattern 'shared database', simple et suffisant pour ce cas d'usage."

---

## ETAPE 1 -- Creation de l'infrastructure (Stock, Activite, Contexte, Niveaux)

> **Criteres d'examen couverts** : C# syntaxe, POO (classes, encapsulation, heritage), formulaires modaux, DialogResult, INSERT, validation, unicite

---

### 1.1 Creer un Stock physique

#### Ce que tu montres

1. Dans ArtisaStock, clique sur **"Stock & Liaisons"** dans la sidebar gauche
2. L'ecran `FrmStocks` s'affiche avec un DataGridView (DGV) et un panel de liaison a droite
3. Clique sur **"+ Nouveau stock"**
4. Le formulaire modal `FrmStockEdit` s'ouvre
5. Tape "Frigo Atelier" dans le champ Nom
6. Tape "Refrigerateur principal de l'atelier" dans Description
7. Clique **Enregistrer**
8. Montre que la ligne apparait dans le DGV

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/FrmStockEdit.cs` -- montre la classe
- `DAL/StockDAL.cs` -- montre la requete INSERT

#### Ce que tu dis

> "Je commence par creer un stock physique -- c'est un lieu de stockage, comme un frigo ou une armoire.
>
> Le formulaire `FrmStockEdit` herite de `FrmEditBase`. C'est une classe abstraite que j'ai creee
> pour factoriser le comportement commun a tous mes formulaires d'edition.
> Elle fournit un `ErrorProvider` -- c'est le petit icone rouge qui apparait a cote du champ si la validation echoue --
> et deux boutons standard : Enregistrer et Annuler.
>
> Le cycle est toujours le meme : quand je clique Enregistrer, ca appelle `Valider()`, qui verifie
> que le nom n'est pas vide et qu'il n'existe pas deja en base. Si tout est bon, ca appelle `Sauvegarder()`
> qui envoie les donnees a la DAL.
>
> [Ouvre StockDAL.cs]
>
> Ici vous voyez la requete INSERT. Le `@nom` et `@desc` sont des parametres -- pas de la concatenation de chaines.
> C'est ce qui protege contre l'injection SQL. Meme si l'utilisateur tape du SQL malveillant dans le champ Nom,
> ca sera traite comme du texte, pas comme du code SQL."

```csharp
// StockDAL.cs - Insert
cmd.CommandText = "INSERT INTO stocks (nom, description, actif) VALUES (@nom, @desc, 1)";
cmd.Parameters.AddWithValue("@nom", s.Nom);
cmd.Parameters.AddWithValue("@desc", (object)s.Description ?? DBNull.Value);
```

#### Questions probables

**Q : C'est quoi FrmEditBase exactement ?**
> "C'est une classe abstraite -- on ne peut pas l'instancier directement, on doit en heriter.
> Elle fournit le squelette commun : un `ErrorProvider` pour afficher les erreurs de validation,
> les boutons Enregistrer/Annuler, et le cycle `Valider()` puis `Sauvegarder()`.
> Chaque formulaire concret comme `FrmStockEdit` ou `FrmIngredientEdit` override ces methodes
> avec ses propres regles. Ca evite de dupliquer le code dans chaque formulaire."

**Q : Comment tu verifies l'unicite du nom ?**
> "Avant de faire l'INSERT, j'appelle `StockDAL.NomExiste()` qui fait un
> `SELECT COUNT(*) FROM stocks WHERE nom = @nom AND id <> @id`.
> Le `AND id <> @id` est important : en mode edition, je dois exclure l'enregistrement en cours
> de la verification, sinon il detecterait son propre nom comme doublon."

---

### 1.2 Creer une Activite artisanale

#### Ce que tu montres

1. Dans la sidebar, clique sur l'icone engrenage **"Gerer les activites"**
2. Le formulaire modal `FrmActivites` s'ouvre (c'est une liste modale)
3. Clique **"+ Nouvelle activite"**
4. `FrmActiviteEdit` s'ouvre -- tape "Chocolaterie" comme nom
5. Clique Enregistrer -- retour a la liste, la ligne apparait
6. Montre le bouton **"Stocks lies"** qui ouvre `FrmActiviteStocks`
7. Coche le stock "Frigo Atelier" qu'on vient de creer -- Enregistrer

**Fichier a ouvrir :**
- `DAL/StockDAL.cs` -- montre `LierActivite()` avec `INSERT IGNORE`

#### Ce que tu dis

> "Les activites representent les differents metiers de l'artisan -- Chocolaterie, Patisserie, etc.
> La relation entre Stock et Activite est une relation many-to-many -- plusieurs stocks peuvent servir
> plusieurs activites. C'est gere par une table pivot `activites_stocks`.
>
> Quand je coche un stock dans la CheckedListBox, ca execute un `INSERT IGNORE` dans la table pivot.
> Le `INSERT IGNORE` est pratique : si la liaison existe deja, il ne fait rien au lieu de planter avec une erreur de doublon.
>
> [Montre le code]
>
> Ce qui est interessant ici, c'est que cette liaison many-to-many est accessible depuis deux endroits :
> depuis l'ecran Stocks avec le panel a droite, et depuis l'ecran Activites avec le bouton Stocks lies.
> Les deux ecrivent dans la meme table pivot -- c'est juste deux points d'entree differents pour l'utilisateur."

```sql
-- INSERT IGNORE evite l'erreur si la liaison existe deja
INSERT IGNORE INTO activites_stocks (id_activite, id_stock) VALUES (@idActivite, @idStock)
```

#### Questions probables

**Q : C'est quoi INSERT IGNORE ?**
> "C'est une variante MySQL de INSERT. Si l'insertion violerait une contrainte d'unicite --
> ici la cle primaire composite (id_activite, id_stock) -- au lieu de lever une erreur,
> MySQL ignore silencieusement l'insertion. C'est plus propre que de faire un SELECT avant pour verifier."

---

### 1.3 Creer un Contexte de production avec ses Niveaux

#### Ce que tu montres

1. Dans la sidebar, avec l'activite "Chocolaterie" selectionnee, clique **"+ Nouveau contexte"**
2. Le formulaire `FrmBomContexteEdit` s'ouvre en mode creation
3. Tape "Chocolats" comme nom de contexte
4. Le niveau N1 "Ingredients" est deja pre-rempli automatiquement
5. Clique le bouton **"+"** pour ajouter un niveau N2 -- tape "Recettes"
6. Clique encore **"+"** pour ajouter un niveau N3 -- tape "Assemblages"
7. Montre la liste des niveaux : N1 Ingredients, N2 Recettes, N3 Assemblages
8. Clique **Enregistrer**

**Fichier a ouvrir :**
- `DAL/BomContexteDAL.cs` -- montre `InsertAvecNiveaux()` avec la transaction

#### Ce que tu dis

> "Le contexte BOM -- Bill of Materials, nomenclature en francais -- c'est un regroupement
> de niveaux de transformation. Le N1 est toujours le niveau ingredients, les matieres premieres.
> Le N2, N3 et au-dela sont les niveaux de production.
>
> Ce qui est important ici, c'est que la creation du contexte et de tous ses niveaux est **transactionnelle**.
>
> [Ouvre BomContexteDAL.cs, montre InsertAvecNiveaux]
>
> On voit le `BeginTransaction()` : d'abord j'insere le contexte, ensuite je boucle sur les noms de niveaux
> pour inserer chaque niveau. Si quoi que ce soit echoue, le `catch` fait un `Rollback()` -- tout est annule.
> Soit tout passe, soit rien ne passe. C'est le principe ACID des transactions.
>
> L'alternative serait d'inserer sans transaction, mais la on risquerait d'avoir un contexte sans niveaux
> si le programme plante au milieu. La transaction garantit la coherence."

```csharp
// BomContexteDAL.InsertAvecNiveaux -- Transaction atomique
using (var tx = conn.BeginTransaction())
{
    try {
        // 1. INSERT bom_contextes
        // 2. Pour chaque niveau : INSERT bom_niveaux
        tx.Commit();
    }
    catch { tx.Rollback(); throw; }
}
```

#### Questions probables

**Q : Pourquoi tu n'utilises pas des FK CASCADE pour creer les niveaux automatiquement ?**
> "CASCADE c'est pour la suppression ou la mise a jour en cascade, pas pour la creation.
> Les niveaux doivent etre crees explicitement avec un nom et un ordre choisis par l'utilisateur.
> C'est pour ca que j'utilise une transaction : pour m'assurer que le contexte et ses niveaux
> sont crees ensemble de maniere atomique."

**Q : C'est quoi l'ordre des niveaux et pourquoi c'est important ?**
> "L'ordre determine la hierarchie de production. Le N1 contient les matieres premieres,
> le N2 les premieres transformations, le N3 les assemblages, etc.
> La regle fondamentale, c'est qu'un niveau N peut consommer n'importe quel niveau inferieur,
> mais jamais superieur. Ca empeche les references circulaires dans les recettes."

---

## ETAPE 2 -- Ajouter des ingredients et acheter des lots

> **Criteres d'examen couverts** : controles d'edition (TextBox, NumericUpDown, ComboBox, ErrorProvider), controles de selection (DGV, ComboBox), jointures SQL, WHERE, validation, proprietes calculees

---

### 2.1 Creer une fiche ingredient

#### Ce que tu montres

1. Dans la sidebar, clique sur **"Ingredients"**
2. L'ecran `FrmIngredients` s'affiche avec le DGV et les chips de filtrage par stock
3. Clique **"+ Ajouter"**
4. Le formulaire `FrmIngredientEdit` s'ouvre -- montre tous les champs :
   - Tape "Chocolat noir 70%" dans Nom
   - Laisse Marque vide
   - Selectionne "g" dans Unite de base (ComboBox)
   - Selectionne "solide" dans Type physique (ComboBox) -- **montre que la densite disparait**
   - Change en "liquide" -- **montre que la densite reapparait** -- remet "solide"
   - Tape 1000 dans Conditionnement (NumericUpDown)
   - Tape 15.00 dans Prix reference
   - Selectionne "Frigo Atelier" dans Stock (ComboBox)
5. **Fais une erreur expres** : laisse le nom vide et clique Enregistrer
6. Montre le `ErrorProvider` avec le message "Obligatoire."
7. Remets le nom, clique Enregistrer -- succes

**Fichier a ouvrir :**
- `Forms/FrmIngredientEdit.cs` -- montre la methode `Valider()`
- `DAL/IngredientDAL.cs` -- montre le `GetAll()` avec ses jointures

#### Ce que tu dis

> "Le formulaire d'ingredient montre bien la variete des controles d'edition :
> des TextBox pour le nom et la marque, des NumericUpDown pour le prix et le conditionnement --
> qui forcent l'utilisateur a entrer un nombre, pas du texte -- et des ComboBox en mode DropDownList
> pour l'unite, le type physique, le fournisseur et le stock.
>
> Le comportement dynamique de la densite, c'est un bon exemple de logique conditionnelle dans un formulaire :
> quand le type physique change, l'evenement `SelectedIndexChanged` du ComboBox se declenche,
> et je masque ou affiche le champ densite selon que c'est liquide/poudre ou solide/piece.
>
> [Montre la validation -- erreur expres]
>
> L'ErrorProvider, c'est un composant WinForms qui affiche une icone d'erreur a cote du controle fautif.
> J'ai 7 regles de validation : nom obligatoire, nom unique, unite selectionnee, conditionnement > 0,
> type physique selectionne, stock selectionne, et densite obligatoire si liquide ou poudre.
>
> [Ouvre IngredientDAL.cs, montre GetAll]
>
> La requete `GetAll()` est interessante parce qu'elle utilise plusieurs jointures.
> Le `LEFT JOIN fournisseurs` recupere le nom du fournisseur -- LEFT JOIN parce que le fournisseur est optionnel.
> Le `INNER JOIN stocks` recupere le nom du stock -- INNER JOIN parce que le stock est obligatoire.
> Et le `LEFT JOIN lots_ingredients` avec un `SUM(quantite_disponible)` calcule le stock actuel en temps reel
> en additionnant tous les lots disponibles. Le `GROUP BY fi.id` est necessaire a cause de l'aggregation."

```sql
-- IngredientDAL.GetAll() -- jointures + stock agrege
SELECT fi.*, f.nom AS nom_fournisseur, s.nom AS nom_stock,
       COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
FROM fiches_ingredients fi
LEFT  JOIN fournisseurs      f ON f.id = fi.id_fournisseur_defaut
INNER JOIN stocks            s ON s.id = fi.id_stock
LEFT  JOIN lots_ingredients  l ON l.id_fiche_ingredient = fi.id
WHERE fi.actif = 1
GROUP BY fi.id
ORDER BY fi.nom
```

#### Questions probables

**Q : C'est quoi la difference entre LEFT JOIN et INNER JOIN ?**
> "INNER JOIN ne garde que les lignes qui ont une correspondance des deux cotes.
> Si un ingredient n'a pas de fournisseur, avec un INNER JOIN il disparaitrait de la liste.
> LEFT JOIN garde toutes les lignes de la table de gauche, meme sans correspondance a droite --
> dans ce cas, les colonnes du fournisseur seront NULL. J'utilise LEFT JOIN pour le fournisseur
> et les lots parce qu'ils sont optionnels, et INNER JOIN pour le stock parce qu'il est obligatoire."

**Q : Pourquoi COALESCE dans le SUM ?**
> "Si un ingredient n'a aucun lot -- aucun achat encore -- le `SUM()` retourne NULL, pas zero.
> `COALESCE(SUM(...), 0)` transforme ce NULL en 0, ce qui est plus logique pour l'affichage
> et evite des erreurs quand on fait des calculs dessus en C#."

**Q : Comment tu geres le `DBNull.Value` en C# ?**
> "Pour les champs nullables comme le fournisseur ou la marque, quand je lis le resultat de la requete,
> je verifie `reader.IsDBNull(index)` avant de lire la valeur. Et quand j'ecris en base,
> j'utilise `(object)valeur ?? DBNull.Value` pour passer NULL a MySQL si la valeur C# est null."

---

### 2.2 Enregistrer un achat (lot d'ingredient)

#### Ce que tu montres

1. Dans la sidebar, clique sur **"Achats & Lots"**
2. L'ecran `FrmAchats` s'affiche
3. Clique **"+ Ajouter"**
4. `FrmAchatEdit` s'ouvre -- montre :
   - Selectionne "Chocolat noir 70%" dans le ComboBox Ingredient
   - Le label info s'affiche : "x 1000 g = ..."
   - Le prix se pre-remplit avec le prix de reference de l'ingredient
   - Tape 5 dans Nombre de conditionnements
   - Le label montre : "x 1000 g = 5000 g en stock"
   - Les totaux HTVA/TVAC se mettent a jour en temps reel
   - Selectionne une date d'achat
   - Coche la date de peremption, selectionne une date
5. Clique **Enregistrer**
6. Montre que la ligne apparait dans la liste des achats
7. **Reviens sur les Ingredients** -- montre que le "Stock actuel" a augmente

**Fichier a ouvrir :**
- `DAL/LotDAL.cs` -- montre l'INSERT et surtout l'UPDATE

#### Ce que tu dis

> "L'ecran d'achat montre un calcul temps reel interessant. A chaque changement de quantite ou de prix,
> la methode `MajPrix()` recalcule les totaux HTVA et TVAC. L'utilisateur peut saisir son prix en HTVA ou TVAC
> grace aux RadioButtons, et la conversion se fait automatiquement.
>
> Ce qui est cle, c'est la formule a l'insertion :
> `quantite_initiale = nb_conditionnements x qte_par_conditionnement`.
> Si j'achete 5 sacs de 1000g, ca fait 5000g en stock.
> Et `quantite_disponible` est egale a `quantite_initiale` a la creation.
> C'est cette `quantite_disponible` qui va diminuer au fur et a mesure de la production.
>
> [Montre le code UPDATE dans LotDAL.cs]
>
> L'UPDATE est plus subtil. Si je modifie un lot qui a deja ete partiellement consomme,
> je ne peux pas juste ecraser la quantite disponible. La formule est :
> `nouvelle_disponible = GREATEST(0, nouvelle_initiale - (ancienne_initiale - ancienne_disponible))`.
> Le `(ancienne_initiale - ancienne_disponible)` c'est ce qui a deja ete consomme.
> Le GREATEST(0,...) empeche d'avoir un stock negatif si la nouvelle quantite est inferieure a ce qui a ete consomme."

```sql
-- LotDAL.Update -- conservation de la consommation
UPDATE lots_ingredients SET
    quantite_disponible = GREATEST(0, @qteInit - (quantite_initiale - quantite_disponible)),
    ...
WHERE id = @id
```

#### Questions probables

**Q : C'est quoi un DateTimePicker ?**
> "C'est un controle WinForms qui affiche un calendrier. L'utilisateur ne peut selectionner qu'une date valide,
> pas taper n'importe quoi. Ca evite les erreurs de format. J'en utilise un pour la date d'achat
> et un autre pour la date de peremption, qui est optionnel -- active par un CheckBox."

**Q : Pourquoi stocker quantite_initiale ET quantite_disponible ?**
> "La quantite_initiale, c'est l'historique : combien j'ai achete. Elle ne change jamais.
> La quantite_disponible, c'est ce qui reste apres les productions. Ca me permet de calculer
> combien a ete consomme : `consomme = initiale - disponible`. C'est important pour la tracabilite."

---

## ETAPE 3 -- Creer une fiche recette (BOM)

> **Criteres d'examen couverts** : POO (heritage, encapsulation), transaction SQL, INSERT transactionnel, validation, jointures COALESCE/LEFT JOIN, interaction formulaire parent-enfant

---

#### Ce que tu montres

1. Depuis le Kanban de production ou la vue contexte, accede au niveau N2 "Recettes"
2. Clique **"+ Ajouter"** -- le formulaire `FrmBomFicheEdit` s'ouvre
3. Montre les champs de l'en-tete :
   - Tape "Ganache chocolat noir" comme nom
   - L'activite est affichee en lecture seule (label bold)
   - Selectionne "g" comme unite output, 500 comme quantite output
   - "Ca veut dire que chaque batch produit 500g de ganache"
4. **Montre la section Composition (lignes)** :
   - Dans le ComboBox `cboInput`, montre les ingredients prefixes `[Ingr.]`
   - Selectionne "Chocolat noir 70%"
   - Le ComboBox d'unite s'adapte automatiquement (g, kg, mg...)
   - Tape 300 g comme quantite
   - Clique **"+ Ajouter"** -- la ligne apparait dans le DGV des lignes
   - Ajoute un 2e ingredient (ex: "Creme liquide", 200 ml)
5. Clique **Enregistrer**
6. Montre le message de confirmation et la fiche dans la liste

**Fichiers a ouvrir :**
- `Forms/FrmBomFicheEdit.cs` -- la methode `ChargerInputsDisponibles()`
- `DAL/BomFicheDAL.cs` -- la methode `Insert()` transactionnelle
- `DAL/BomFicheLigneDAL.cs` -- la jointure `GetByFiche()` avec COALESCE

#### Ce que tu dis

> "La fiche BOM, c'est la recette. Elle definit ce qu'on fabrique et avec quoi.
> L'en-tete donne le nom, l'unite de sortie et la quantite par batch.
> Les lignes definissent la composition -- quels ingredients ou quelles sous-fiches on consomme.
>
> Ce qui est interessant ici, c'est le chargement dynamique des inputs.
> La methode `ChargerInputsDisponibles()` remplit le ComboBox avec deux types de sources :
> les ingredients de l'activite (toujours disponibles), et les fiches de niveaux inferieurs
> (seulement si on est en N3 ou plus). Un niveau N3 peut consommer du N2,
> un N4 peut consommer du N2 et du N3. Mais jamais un niveau egal ou superieur.
> Ca garantit qu'on n'a pas de references circulaires.
>
> [Ouvre BomFicheDAL.cs, montre Insert()]
>
> L'insertion est transactionnelle, comme la creation du contexte.
> D'abord j'insere la fiche, je recupere l'ID auto-genere avec `LastInsertedId`,
> puis j'insere toutes les lignes dans une boucle. Si quoi que ce soit echoue, rollback.
>
> [Ouvre BomFicheLigneDAL.cs, montre GetByFiche()]
>
> La requete de lecture des lignes utilise un double LEFT JOIN : un vers `fiches_ingredients`
> et un vers `bom_fiches`. Parce qu'une ligne peut etre soit un ingredient, soit une fiche.
> Le `COALESCE(fi.nom, bf.nom)` choisit le bon nom selon le type.
> C'est comme un IF en SQL : prends le premier qui n'est pas NULL."

```sql
-- BomFicheLigneDAL.GetByFiche -- COALESCE resout le polymorphisme
SELECT l.*,
       COALESCE(fi.nom, bf.nom) AS nom_input,
       COALESCE(fi.unite_mesure, bf.unite_output) AS unite_input
FROM bom_fiches_lignes l
LEFT JOIN fiches_ingredients fi ON fi.id = l.id_input_ingredient
LEFT JOIN bom_fiches         bf ON bf.id = l.id_input_fiche
WHERE l.id_fiche = @idFiche
```

#### Questions probables

**Q : Pourquoi deux FK mutuellement exclusives (id_input_ingredient et id_input_fiche) au lieu d'une seule FK ?**
> "C'est le pattern 'polymorphic FK'. Une ligne peut pointer soit vers un ingredient,
> soit vers une fiche de production. Si je mettais une seule FK, elle pointerait vers
> une seule table, et il faudrait une table intermediaire abstraite. Avec deux FK mutuellement exclusives,
> c'est plus simple et les jointures sont directes. Le champ `type_input` ('ingredient' ou 'fiche')
> indique laquelle des deux FK est remplie."

**Q : Comment tu geres la conversion d'unites ?**
> "J'ai une classe statique `UnitConvertisseur` dans le dossier Helpers.
> Elle definit des groupes d'unites -- masse (mg, g, kg), volume (ml, cl, dl, l) et piece.
> La methode `Convertir(valeur, source, cible)` passe d'abord en unite de base
> (grammes pour la masse, millilitres pour le volume), puis convertit vers l'unite cible.
> C'est utilise partout : dans les formulaires pour filtrer les unites compatibles,
> et dans la production pour comparer les stocks avec les besoins."

---

## ETAPE 4 -- Lancer une production (consommation FIFO)

> **Criteres d'examen couverts** : transaction SQL complexe, UPDATE, DELETE implicite par decrementation, algorithme FIFO, boucles, try/catch, jointures multiples

---

#### Ce que tu montres

1. Va sur l'ecran **Production** (sidebar ou Kanban)
2. Montre les KPI en haut : Productions 7j, Cout 7j, Alertes stock, Fiches actives
3. Selectionne le **Contexte** "Chocolats" dans la cascade
4. Selectionne le **Niveau** "Recettes (N2)" -- note : N1 n'est pas disponible (ingredients)
5. Selectionne la **Fiche** "Ganache chocolat noir"
6. Le label montre : "1 batch = 500 g"
7. Tape **2** dans Nombre de batches (= 1000g de ganache)
8. Clique **Simuler**
9. Le DGV s'affiche avec des jauges visuelles :
   - Chocolat noir : barre verte 100% -- stock suffisant
   - Creme liquide : barre verte ou rouge selon le stock
10. Si tout est vert, clique **"Lancer la production"**
11. MessageBox de confirmation avec resume
12. Clique Oui -- la production s'execute
13. Message "Production terminee avec succes"
14. Montre que le stock d'ingredients a diminue (retour sur les ingredients)

**Fichiers a ouvrir :**
- `DAL/BomProductionDAL.cs` -- methodes `Simuler()`, `Executer()`, `ConsumeStock()`

#### Ce que tu dis

> "La production est le coeur du systeme. C'est ici que tout se connecte.
>
> D'abord, la **simulation** : pour chaque ligne de la recette, je calcule la quantite necessaire
> -- ici 300g de chocolat x 2 batches = 600g -- et je compare avec le stock disponible.
> La jauge visuelle est un custom painting dans le DGV : je dessine une barre de progression
> directement dans la cellule avec l'evenement `CellPainting`.
>
> Si tout est vert, on peut lancer la production. Et la, c'est une **transaction atomique** :
>
> [Ouvre BomProductionDAL.cs, montre Executer()]
>
> 1. D'abord je re-verifie la disponibilite DANS la transaction -- c'est une double verification
>    pour eviter les problemes de concurrence. Quelqu'un pourrait avoir consomme du stock entre
>    le moment de la simulation et le lancement.
> 2. J'insere la production avec un cout a zero provisoirement.
> 3. Pour chaque ligne, j'appelle `ConsumeStock()` qui fait le FIFO.
> 4. Apres la consommation, je mets a jour le cout reel de la production.
> 5. Je cree le stock produit dans `bom_stocks`.
> 6. COMMIT. Si quoi que ce soit echoue, ROLLBACK complet.
>
> **Le FIFO** -- First In First Out -- c'est-a-dire que les lots les plus anciens sont consommes en premier.
>
> [Montre ConsumeStock()]
>
> La requete charge les lots tries par `date_achat ASC` -- le plus ancien d'abord.
> Ensuite je boucle : je prends le minimum entre ce qu'il me reste a consommer et ce qui est disponible
> dans le lot. Je decremente le lot avec un UPDATE, j'insere une ligne de tracabilite,
> et je passe au lot suivant si necessaire.
>
> C'est exactement comme dans un vrai entrepot : on utilise d'abord ce qui est arrive en premier,
> pour eviter le gaspillage."

```sql
-- FIFO : lots tries par date d'achat croissante
SELECT l.id, l.quantite_disponible, ...
FROM lots_ingredients l
WHERE l.id_fiche_ingredient = @idFi
HAVING dispo_nette > 0
ORDER BY l.date_achat ASC    -- FIFO : le plus ancien d'abord
```

```csharp
// Boucle FIFO dans ConsumeStock()
foreach (var lot in lotsFIFO)
{
    if (restant <= 0) break;
    decimal pris = Math.Min(restant, lot.DispoNette);

    // UPDATE lots_ingredients SET quantite_disponible = quantite_disponible - @pris
    // INSERT INTO bom_productions_lignes (tracabilite)

    restant -= pris;
    coutLigne += pris * lot.PrixUnitaireBase;
}
```

#### Questions probables

**Q : Pourquoi tu re-verifies la disponibilite dans la transaction ?**
> "C'est pour la securite des donnees. Imaginons que deux productions se lancent en meme temps.
> La premiere simulation montre que le stock est OK, mais entre la simulation et l'execution,
> l'autre production a deja consomme une partie du stock. La double verification dans la transaction
> detecte ca et fait un rollback si le stock n'est plus suffisant.
> C'est un pattern classique en programmation concurrente : 'check-then-act' dans une transaction."

**Q : C'est quoi la tracabilite dont tu parles ?**
> "Chaque production genere des lignes dans `bom_productions_lignes`. Chaque ligne dit :
> 'pour cette production, j'ai pris X grammes du lot numero Y, au prix de Z euros par gramme'.
> Ca permet de remonter exactement quel lot d'ingredient a ete utilise dans quel produit fini.
> C'est important pour la securite alimentaire -- si un lot est rappele, on sait quels produits sont concernes."

**Q : Comment tu calcules le cout unitaire ?**
> "Le cout n'est pas base sur les prix de reference, mais sur les prix reels des lots consommes.
> Pendant la boucle FIFO, j'accumule le cout de chaque lot consomme :
> `pris * prixUnitaireBase` (prix du lot divise par la quantite par conditionnement).
> A la fin, le cout unitaire = cout total / quantite produite.
> C'est un cout reel, pas une estimation."

---

## ETAPE 5 -- Publier un produit sur la boutique web

> **Criteres d'examen couverts** : interaction formulaires, ComboBox avec filtre NOT IN, gestion images, FK cross-app, stock calcule en temps reel

---

#### Ce que tu montres

1. Dans la sidebar, clique **"Boutique Web"**
2. Montre le **TabControl** avec 3 onglets : Categories, Produits, Commandes
3. Va d'abord sur **Categories** -- montre la liste, ajoute une categorie "Ganaches" si besoin
4. Va sur **Produits** -- clique **"+ Publier fiche"**
5. Le formulaire `FrmProduitWebEdit` s'ouvre
6. Montre le ComboBox **"Fiche BOM"** -- seules les fiches NON deja publiees apparaissent
7. Selectionne "Ganache chocolat noir"
8. Selectionne la categorie "Ganaches"
9. Tape "Ganache Chocolat Noir Artisanale" comme nom commercial
10. Tape le prix de vente : 12.50
11. Optionnel : montre la selection d'image
12. Coche "Publie"
13. Clique Enregistrer
14. De retour dans la liste, montre la colonne **Stock** calculee en temps reel depuis `bom_stocks`

**Fichiers a ouvrir :**
- `DAL/ProduitWebDAL.cs` -- montre `GetFichesNonPubliees()` et le calcul de stock
- `Forms/FrmPrincipal.BoutiqueWeb.cs` -- montre le `TabControl` avec lazy init

#### Ce que tu dis

> "L'ecran Boutique Web est un partial class de FrmPrincipal -- c'est-a-dire que c'est
> toujours la meme fenetre principale, mais le code est organise dans un fichier separe
> pour la lisibilite. C'est un pattern C# qui permet de decouper une grande classe en plusieurs fichiers.
>
> Le ComboBox des fiches BOM utilise un filtre SQL interessant :
> `WHERE f.id NOT IN (SELECT id_bom_fiche FROM produits_web)`.
> Ca garantit qu'une fiche ne peut etre publiee qu'une seule fois. Si je veux modifier le produit web,
> je modifie la publication, pas la fiche BOM elle-meme.
>
> Le stock affiche dans la liste n'est pas stocke dans la table `produits_web`.
> Il est calcule en direct par la requete :
> `COALESCE(SUM(bs.quantite_disponible), 0)` sur la table `bom_stocks`.
> Donc quand la production cree du stock ou qu'une commande web en consomme,
> le chiffre se met a jour automatiquement.
>
> Pour les images, le C# copie le fichier directement dans le dossier storage de Laravel :
> `site-laravel/storage/app/public/produits/`. Comme ca le site web peut servir les images
> sans avoir besoin d'une synchronisation separee."

```sql
-- GetFichesNonPubliees : seules les fiches pas encore publiees
SELECT f.id, f.nom, f.unite_output
FROM bom_fiches f
WHERE f.actif = 1
  AND f.id NOT IN (SELECT id_bom_fiche FROM produits_web)
ORDER BY f.nom

-- Calcul stock temps reel dans la liste produits
SELECT p.*, COALESCE(SUM(bs.quantite_disponible), 0) AS stock_disponible
FROM produits_web p
LEFT JOIN bom_stocks bs ON bs.id_fiche = p.id_bom_fiche AND bs.quantite_disponible > 0
GROUP BY p.id
```

#### Questions probables

**Q : Comment tu geres la suppression d'un produit web qui a des commandes ?**
> "Avant de supprimer, je verifie s'il y a des lignes de commande qui referencent ce produit :
> `SELECT COUNT(*) FROM commandes_web_lignes WHERE id_produit_web = @id`.
> Si oui, je refuse la suppression et je propose la depublication a la place --
> mettre `en_vente = false`, ce qui le retire du site sans perdre l'historique des commandes."

---

## ETAPE 6 -- Parcours client sur le site Laravel

> **Criteres d'examen couverts (PDWEB)** : PHP, formulaires avec validation, sessions (panier, auth, flash), contenu par niveau d'auth, securisation middleware, MySQL via ORM, AJAX

---

### 6.1 Le catalogue (visiteur non connecte)

#### Ce que tu montres

1. Ouvre le navigateur sur `http://localhost:8000`
2. Montre la page d'accueil / catalogue avec la grille de produits
3. Montre les **pills de categories** en haut (filtrage)
4. Clique sur une categorie -- les produits se filtrent
5. Montre le **select de tri** (prix croissant, decroissant, defaut)
6. Clique sur un produit -- page detail
7. Montre le badge de stock (vert/rouge)
8. Montre que le bouton dit **"Connectez-vous pour commander"** (pas connecte)

**Fichiers a ouvrir dans VS Code :**
- `site-laravel/app/Http/Controllers/CatalogueController.php`
- `site-laravel/app/Models/ProduitWeb.php` -- l'accessor `getStockDisponibleAttribute()`
- `site-laravel/resources/views/catalogue/index.blade.php`

#### Ce que tu dis

> "Le catalogue utilise Eloquent, l'ORM de Laravel. ORM -- Object-Relational Mapping --
> c'est une couche qui permet de manipuler la base de donnees avec des objets PHP au lieu d'ecrire du SQL.
>
> [Ouvre CatalogueController.php]
>
> La requete `ProduitWeb::where('en_vente', 1)->with('categorie')` fait deux choses :
> elle filtre les produits publies, et le `with('categorie')` c'est du eager loading --
> ca evite le probleme N+1. Sans `with`, chaque produit ferait une requete separee pour charger
> sa categorie. Avec `with`, Laravel fait une seule requete supplementaire pour toutes les categories.
>
> Le tri utilise `match` de PHP 8 -- c'est comme un switch mais en plus propre.
>
> [Ouvre ProduitWeb.php, montre l'accessor]
>
> Le stock est calcule via un accessor Laravel. C'est une methode `getStockDisponibleAttribute()`
> qui est appelee automatiquement quand on accede a `$produit->stock_disponible` dans le Blade.
> Elle fait un `SUM(quantite_disponible)` sur la table `bom_stocks`.
> C'est le meme stock que dans le C# -- la meme table, la meme base.
>
> [Montre la vue Blade]
>
> Blade, c'est le moteur de template de Laravel. Le `{{ $produit->nom_commercial }}`
> echappe automatiquement le HTML pour eviter les attaques XSS.
> Si quelqu'un mettait du JavaScript dans un nom de produit, Blade le transformerait en texte visible, pas en code executable."

```php
// ProduitWeb.php -- accessor qui calcule le stock en temps reel
public function getStockDisponibleAttribute(): float
{
    return (float) BomStock::where('id_fiche', $this->id_bom_fiche)
        ->where('quantite_disponible', '>', 0)
        ->sum('quantite_disponible');
}
```

#### Questions probables

**Q : C'est quoi le eager loading / probleme N+1 ?**
> "Sans eager loading, si j'ai 20 produits, Laravel ferait 1 requete pour les produits
> + 20 requetes pour charger la categorie de chacun = 21 requetes.
> Avec `with('categorie')`, ca fait 1 requete pour les produits + 1 requete pour TOUTES les categories = 2 requetes.
> C'est beaucoup plus performant."

**Q : Pourquoi le stock n'est pas une colonne dans la table produits_web ?**
> "Parce que le stock change en permanence -- les productions ajoutent, les commandes consomment.
> Si je stockais la valeur, elle serait vite desynchronisee. En la calculant en temps reel
> depuis la table `bom_stocks`, j'ai toujours la valeur exacte. Le cout en performance est negligeable
> pour un catalogue de taille artisanale."

---

### 6.2 Inscription du client

#### Ce que tu montres

1. Clique sur **"S'inscrire"** dans le header
2. Le formulaire d'inscription s'affiche avec les champs obligatoires marques *
3. **Fais des erreurs expres** :
   - Laisse l'email vide -- soumets -- montre le message d'erreur en rouge sous le champ
   - Tape un email invalide ("abc") -- soumets -- montre "L'adresse email n'est pas valide."
   - Tape un mot de passe de 3 caracteres -- montre "au moins 8 caracteres"
   - Montre que les champs deja remplis sont **conserves** grace a `old()`
4. Remplis correctement tous les champs
5. Clique **S'inscrire** -- redirection vers le catalogue avec message flash "Bienvenue !"
6. Montre que le header affiche maintenant le prenom du client

**Fichiers a ouvrir :**
- `site-laravel/app/Http/Requests/RegisterRequest.php` -- regles de validation
- `site-laravel/app/Http/Controllers/Auth/RegisterController.php`

#### Ce que tu dis

> "Le formulaire d'inscription utilise un FormRequest de Laravel. C'est une classe separee
> qui centralise toutes les regles de validation. Ca separe la validation du controleur --
> c'est plus propre et reutilisable.
>
> [Ouvre RegisterRequest.php]
>
> Les regles sont expressives : `required|email|unique:clients,email` verifie que le champ est present,
> que c'est un email valide, et qu'il n'existe pas deja dans la table `clients`.
> Le `confirmed` sur le mot de passe verifie qu'il y a un champ `password_confirmation` identique.
>
> Les messages sont personnalises en francais. Sans les messages custom,
> Laravel afficherait les messages par defaut en anglais.
>
> [Ouvre RegisterController.php]
>
> Le mot de passe est hache avec `PASSWORD_BCRYPT`. C'est un hachage irreversible --
> meme moi en tant qu'admin, je ne peux pas voir le mot de passe en clair dans la base.
> Seul `password_verify()` peut comparer un mot de passe entre avec le hash stocke.
>
> Apres la creation du client, je stocke son ID dans la session et j'appelle `session()->regenerate()`.
> Le `regenerate()` est une mesure de securite contre les attaques de session fixation :
> ca genere un nouveau session ID pour que personne ne puisse reutiliser l'ancien."

```php
// RegisterController -- securite
$client = Client::create([
    'mot_de_passe' => password_hash($request->password, PASSWORD_BCRYPT),
    // ...
]);
session(['client_id' => $client->id, 'client_prenom' => $client->prenom]);
session()->regenerate();  // Anti session fixation
```

#### Questions probables

**Q : C'est quoi la session fixation ?**
> "C'est une attaque ou un attaquant fixe l'ID de session avant que la victime se connecte.
> Si la session n'est pas regeneree apres la connexion, l'attaquant connait l'ID et peut
> acceder au compte. Le `regenerate()` cree un nouvel ID, ce qui rend l'ancien inutile."

**Q : Pourquoi bcrypt et pas md5 ou sha256 ?**
> "MD5 et SHA256 sont rapides -- c'est bien pour des checksums, mais pas pour des mots de passe.
> Bcrypt est volontairement lent et inclut un 'salt' aleatoire.
> Ca rend les attaques par force brute et les rainbow tables pratiquement impossibles.
> En plus, bcrypt a un facteur de cout qu'on peut augmenter si les ordinateurs deviennent plus rapides."

**Q : Comment tu affiches les erreurs dans le formulaire ?**
> "Blade a la directive `@error('champ')`. Si la validation echoue, Laravel redirige automatiquement
> vers le formulaire avec les erreurs. Le `@error` affiche le message sous le champ concerne.
> Et `old('prenom')` remet la valeur que l'utilisateur avait tape pour pas qu'il doive tout resaisir."

---

### 6.3 Connexion

#### Ce que tu montres

1. Deconnecte-toi d'abord (bouton Deconnexion)
2. Montre le message flash "Vous avez ete deconnecte."
3. Clique **"Se connecter"**
4. Tape un mauvais mot de passe -- montre le message "Email ou mot de passe incorrect."
5. Tape les bons identifiants -- connexion reussie
6. Montre le message flash "Bon retour, [prenom] !"
7. Montre que le header a change (prenom + badge panier)

**Fichier a ouvrir :**
- `site-laravel/app/Http/Controllers/Auth/LoginController.php`

#### Ce que tu dis

> "La connexion est simple mais securisee.
> Le message d'erreur est volontairement generique : 'Email ou mot de passe incorrect.'
> Je ne dis jamais 'cet email n'existe pas' ou 'mot de passe incorrect' -- ca revelerait
> a un attaquant si un email est enregistre ou non.
>
> Le `password_verify()` compare le mot de passe saisi avec le hash bcrypt stocke en base.
> C'est une comparaison en temps constant -- elle prend toujours le meme temps que le mot de passe
> soit bon ou mauvais, ce qui empeche les attaques par timing.
>
> J'ai aussi un `throttle:5,1` sur la route POST login. Ca limite a 5 tentatives par minute par IP.
> Apres 5 echecs, l'utilisateur doit attendre avant de reessayer.
> C'est une protection contre le brute force."

#### Questions probables

**Q : Pourquoi tu n'utilises pas Laravel Auth / Breeze / Jetstream ?**
> "J'ai choisi de gerer l'authentification manuellement avec les sessions PHP pour deux raisons :
> d'abord pour comprendre le mecanisme -- c'est plus formateur que d'utiliser un package tout fait.
> Ensuite, mon modele Client est specifique (pas le User standard de Laravel),
> avec des champs comme telephone, adresse_rue, etc.
> Le systeme de sessions Laravel fonctionne tres bien pour ca."

---

### 6.4 Panier AJAX

#### Ce que tu montres

1. Retourne sur le catalogue, clique sur un produit
2. Montre l'input de quantite et le bouton **"Ajouter au panier"**
3. Clique -- montre le **toast** (notification verte en haut a droite)
4. Montre que le **badge du panier** dans le header s'est mis a jour **sans rechargement de page**
5. Ajoute un 2e produit
6. Va sur la page **Panier**
7. Modifie une quantite -- la page se rafraichit
8. Supprime un article -- montre l'animation de disparition

**Ouvre les DevTools du navigateur** (F12) > onglet Network pour montrer les requetes AJAX

**Fichiers a ouvrir :**
- `site-laravel/public/js/panier.js` -- le code JavaScript
- `site-laravel/app/Http/Controllers/PanierController.php` -- `ajouter()`

#### Ce que tu dis

> "Le panier utilise AJAX pour l'ajout et la suppression.
> AJAX -- Asynchronous JavaScript And XML, meme si on utilise JSON maintenant, pas XML --
> ca permet de communiquer avec le serveur sans recharger la page.
>
> [Ouvre les DevTools, montre le Network]
>
> Quand je clique 'Ajouter au panier', le JavaScript envoie une requete POST avec `fetch()`.
> On voit la requete dans l'onglet Network : POST /panier/ajouter, avec le body en JSON.
> Le header `X-CSRF-TOKEN` est obligatoire -- sans lui, Laravel refuse la requete avec un 419.
> Le CSRF -- Cross-Site Request Forgery -- c'est une attaque ou un site malveillant
> envoie une requete a ta place. Le token prouve que la requete vient bien de notre site.
>
> [Ouvre panier.js]
>
> Le token est lu depuis une balise `<meta>` dans le layout HTML.
> La reponse JSON contient `success`, `message` et `panier_count`.
> Si success est true, je mets a jour le badge dans le header et j'affiche un toast.
> Si c'est false -- par exemple stock insuffisant -- j'affiche le message d'erreur.
>
> [Ouvre PanierController.php, montre ajouter()]
>
> Cote serveur, le controleur fait plusieurs verifications :
> 1. Validation des entrees avec `$request->validate()`
> 2. Verification du stock : `$produit->stock_disponible < $request->quantite`
> 3. Si le produit est deja en panier, on incremente au lieu d'ajouter une nouvelle ligne
> 4. On re-verifie le stock avec la nouvelle quantite totale
>
> Ce qui est cle, c'est que le panier EST une commande avec `statut = 'panier'`.
> C'est le pattern Shopping Cart as Order Draft -- pas de table separee pour le panier.
> Quand le client valide, on change juste le statut de 'panier' a 'payee'."

```javascript
// panier.js -- requete AJAX avec token CSRF
const res = await fetch('/panier/ajouter', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': CSRF,        // Protection CSRF
        'Accept': 'application/json',
    },
    body: JSON.stringify({ id_produit: idProduit, quantite }),
});
```

#### Questions probables

**Q : Pourquoi AJAX plutot qu'un formulaire classique ?**
> "Pour l'experience utilisateur. Avec un formulaire classique, ajouter un produit au panier
> rechargerait toute la page -- l'utilisateur perdrait sa position dans le catalogue.
> Avec AJAX, la page reste en place, seul le badge se met a jour et un toast confirme l'action.
> C'est plus fluide et plus moderne."

**Q : Comment tu securises les routes du panier ?**
> "Avec un middleware `client.auth` qui verifie deux choses : que la session contient un `client_id`,
> et que le client existe toujours en base et est actif. Si un admin desactive un client dans l'ERP,
> la prochaine requete du client le deconnecte automatiquement.
> En plus, chaque action sur une ligne de panier verifie l'ownership --
> `$ligne->id_commande !== $panier->id` retourne 403 si la ligne n'appartient pas au client."

**Q : C'est quoi le `sous_total` GENERATED en MySQL ?**
> "C'est une colonne calculee automatiquement par MySQL :
> `sous_total = quantite * prix_unitaire`. C'est defini avec `GENERATED ALWAYS AS ... STORED`
> dans la table. Quand je modifie la quantite, MySQL recalcule le sous-total tout seul.
> Pas besoin de le gerer en PHP, c'est le SGBD qui s'en charge."

---

### 6.5 Validation de commande (checkout FIFO)

#### Ce que tu montres

1. Depuis le panier, clique **"Passer commande"** ou **"Commander"**
2. L'ecran recapitulatif s'affiche avec :
   - Liste des articles + sous-totaux
   - Formulaire d'adresse pre-rempli depuis le profil du client
   - Bouton "Simuler le paiement"
3. Clique **Simuler le paiement**
4. Montre la page de confirmation avec le numero de commande
5. Montre l'historique des commandes ("Mes commandes")

**Fichier a ouvrir :**
- `site-laravel/app/Http/Controllers/CommandeController.php` -- methode `valider()`

#### Ce que tu dis

> "Le checkout est la partie la plus critique du site. C'est ici qu'on decremente le stock.
>
> [Ouvre CommandeController.php, montre valider()]
>
> D'abord, `DB::beginTransaction()` -- tout est dans une transaction.
> Ensuite, `lockForUpdate()` sur le panier et sur les stocks. C'est un verrou pessimiste :
> ca bloque les autres requetes qui voudraient modifier ces memes lignes pendant que la transaction est en cours.
> Ca empeche deux clients de commander le dernier produit en stock en meme temps.
>
> Puis pour chaque article du panier, je fais la decrementation FIFO.
> Les lots sont charges avec `orderBy('date_production', 'asc')` -- FIFO, le plus ancien d'abord.
> Je verifie que le stock total est suffisant, puis je boucle lot par lot pour decrementer.
>
> C'est le meme algorithme FIFO que dans le C# -- mais adapte a Eloquent au lieu de SQL brut.
> La logique est identique : `aConsommer = min(restant, stock.quantite_disponible)`,
> on decremente, on passe au lot suivant.
>
> Enfin, le statut du panier passe de 'panier' a 'payee', la date de commande est enregistree,
> et le total TTC est calcule.
>
> Si quoi que ce soit echoue -- stock insuffisant, erreur base de donnees -- `DB::rollBack()` annule tout.
> Le client voit un message d'erreur et son panier est intact."

```php
// CommandeController@valider -- FIFO dans la transaction
DB::beginTransaction();
try {
    $panier = CommandeWeb::where(...)->lockForUpdate()->first();

    foreach ($panier->lignes as $ligne) {
        $stocks = BomStock::where('id_fiche', $idFiche)
            ->where('quantite_disponible', '>', 0)
            ->orderBy('date_production', 'asc')   // FIFO
            ->lockForUpdate()                      // Lock pessimiste
            ->get();

        foreach ($stocks as $stock) {
            if ($restant <= 0) break;
            $aConsommer = min($restant, $stock->quantite_disponible);
            $stock->quantite_disponible -= $aConsommer;
            $stock->save();
            $restant -= $aConsommer;
        }
    }

    $panier->update(['statut' => 'payee', 'date_commande' => now()]);
    DB::commit();
} catch (\Exception $e) {
    DB::rollBack();
}
```

#### Questions probables

**Q : C'est quoi `lockForUpdate()` ?**
> "C'est l'equivalent SQL de `SELECT ... FOR UPDATE`. Ca pose un verrou en ecriture sur les lignes selectionnees.
> Les autres transactions qui voudraient lire ces memes lignes avec `lockForUpdate` sont bloquees
> jusqu'a ce que ma transaction fasse COMMIT ou ROLLBACK.
> C'est necessaire pour eviter les conditions de course -- deux achats simultanement sur le meme stock."

**Q : Qu'est-ce qui se passe si le stock est insuffisant au moment du checkout ?**
> "La transaction fait un rollback et le client est redirige vers son panier avec un message d'erreur.
> Son panier reste intact -- il n'a pas perdu ses articles. Il peut ajuster les quantites ou attendre
> que le stock soit reapprovisionne."

**Q : Pourquoi le total est calcule cote serveur et pas cote client ?**
> "Jamais faire confiance au client pour les montants. Un utilisateur pourrait modifier le JavaScript
> pour envoyer un total de 0 euros. Le total est toujours calcule cote serveur :
> `$panier->lignes->sum('sous_total')`. Les prix unitaires sont fixes au moment de l'ajout au panier."

---

## ETAPE 7 -- Retour dans ArtisaStock : verification du stock et commandes

> **Criteres d'examen couverts** : lecture seule cross-app, vues SQL, jointures, controles de selection (DGV, ComboBox, filtrage)

---

#### Ce que tu montres

1. Retourne dans ArtisaStock (C# WinForms)
2. Va sur la **Vue Stock Global** (sidebar ou menu)
3. Montre que le stock du produit a **diminue** -- la quantite reflete la commande web
4. Montre les 3 sections : INGREDIENTS, PRODUITS INTERMEDIAIRES, PRODUITS FINALS
5. Montre le color coding : vert (dispo), orange (reserve), rouge (penurie)
6. Clique sur un produit -- montre le volet detail a droite avec la composition
7. Retourne sur **"Boutique Web" > Commandes**
8. Montre la commande qu'on vient de passer depuis le site web
9. Selectionne la commande -- montre le detail en bas (client, articles, total)

#### Ce que tu dis

> "Et voila la boucle complete. On a produit dans le C#, publie sur le web,
> le client a achete sur le site Laravel, et maintenant dans le C# on voit que le stock a diminue
> et la commande est visible dans l'onglet Commandes.
>
> La Vue Stock Global utilise une VIEW SQL -- c'est une requete pre-definie dans la base de donnees
> qui unifie les lots d'ingredients et les produits fabriques dans une seule vue.
> C'est pratique parce que ca evite de faire l'union dans chaque requete.
>
> L'onglet Commandes est en lecture seule. Le DAL C# n'a que des methodes Get, pas de Insert ou Update.
> C'est une decision architecturale : les commandes sont creees exclusivement par Laravel,
> et consultees dans l'ERP. Ca respecte le principe de responsabilite unique --
> chaque application a son perimetre d'action clair."

```sql
-- CommandeWebDAL.GetAll -- commandes creees par Laravel, lues par C#
SELECT cmd.*, cl.nom AS nom_client, cl.prenom AS prenom_client,
       (SELECT COUNT(*) FROM commandes_web_lignes l WHERE l.id_commande = cmd.id) AS nb_articles
FROM commandes_web cmd
INNER JOIN clients cl ON cl.id = cmd.id_client
WHERE cmd.statut <> 'panier'
ORDER BY cmd.date_commande DESC
```

#### Questions probables

**Q : Pourquoi les commandes sont en lecture seule dans le C# ?**
> "C'est la separation des responsabilites. Laravel gere le workflow client : panier, paiement, confirmation.
> Le C# gere le workflow de production : stocks, recettes, fabrication.
> Si les deux pouvaient modifier les commandes, on aurait des conflits de donnees.
> En gardant le C# en lecture seule sur les commandes, chaque application a une responsabilite claire."

**Q : Comment tu filtres les commandes par statut ?**
> "Il y a un ComboBox en haut de l'onglet avec trois options : Toutes, Payee, Annulee.
> Quand l'utilisateur change la selection, la requete ajoute un `AND cmd.statut = @statut`.
> Les paniers en cours sont toujours exclus de la vue ERP avec `WHERE cmd.statut <> 'panier'`."

---

## CONCLUSION (1 minute)

### Ce que tu dis

> "Pour resumer, le projet montre un workflow complet de bout en bout :
> de la creation de l'infrastructure jusqu'a la commande client.
>
> Cote C#, j'ai utilise des patterns solides : FrmEditBase pour eviter la duplication,
> des DAL statiques avec des requetes parametrees, des transactions pour la coherence des donnees,
> et un algorithme FIFO pour la consommation de stock.
>
> Cote Laravel, j'ai l'authentification avec bcrypt et sessions securisees,
> un panier AJAX avec protection CSRF et verification de stock,
> et un checkout transactionnel avec locks pessimistes.
>
> Ce qui pourrait etre ameliore :
> - Ajouter des tests unitaires -- je n'en ai pas assez
> - Utiliser une API REST entre le C# et Laravel au lieu de la base partagee, ca serait plus propre
> - Ajouter un vrai systeme de paiement (Stripe/Bancontact) au lieu de la simulation
> - Mettre en place un systeme de notifications en temps reel (WebSocket) pour que le C# soit averti
>   quand une commande arrive sur le site web
>
> Merci pour votre attention, je suis pret pour vos questions."

---

## REFERENCE RAPIDE -- "Si le prof demande..."

| Le prof demande... | Voir etape | Code cle |
|---------------------|-----------|----------|
| **Injection SQL / requetes parametrees** | Etape 1 (Stock) | `StockDAL.cs` -- `@nom`, `AddWithValue` |
| **Heritage / FrmEditBase** | Etape 1 | `FrmEditBase.cs`, n'importe quel FrmXxxEdit |
| **ErrorProvider / validation** | Etape 2.1 (Ingredients) | `FrmIngredientEdit.Valider()` |
| **ComboBox / NumericUpDown** | Etape 2.1 | `FrmIngredientEdit.cs` -- 11 controles |
| **DateTimePicker** | Etape 2.2 (Achats) | `FrmAchatEdit.cs` -- `dtpDateAchat`, `dtpPeremption` |
| **DGV (DataGridView)** | Etape 2 | `FrmIngredients.cs`, `FrmAchats.cs` |
| **Jointure SQL (JOIN)** | Etape 2.1 | `IngredientDAL.GetAll()` -- LEFT/INNER JOIN |
| **WHERE / critere de selection** | Etape 2.1 | `GetAll(idStock: X)` -- filtre dynamique |
| **Transaction SQL** | Etape 1.3 ou 4 | `BomContexteDAL.InsertAvecNiveaux()` ou `BomProductionDAL.Executer()` |
| **INSERT / UPDATE / DELETE** | Toutes etapes | Chaque DAL a ses methodes CRUD |
| **Unicite a l'ajout et modification** | Etape 1 | `NomExiste(nom, excludeId)` |
| **Suppression + verification** | Etape 5 | `ProduitWebDAL.Delete()` -- FK check |
| **Cascade** | Etape 1.2 | FK CASCADE sur activites |
| **DialogResult / formulaire modal** | Etape 1 | `FrmStockEdit.ShowDialog()` |
| **Formulaire Laravel + validation** | Etape 6.2 | `RegisterRequest.php` |
| **Sessions** | Etape 6.2-6.4 | `session('client_id')`, `session()->regenerate()` |
| **Contenu par auth level** | Etape 6.1 | Bouton panier conditionnel dans show.blade.php |
| **Middleware / securisation** | Etape 6.4 | `ClientAuth.php` |
| **ORM Eloquent** | Etape 6.1 | `ProduitWeb::where()->with()->get()` |
| **AJAX** | Etape 6.4 | `panier.js` + `PanierController.php` |
| **CSRF** | Etape 6.4 | `<meta name="csrf-token">` + header `X-CSRF-TOKEN` |
| **FIFO** | Etape 4 ou 6.5 | C# `ConsumeStock()` ou Laravel `CommandeController@valider` |

---

## CHEMINS DES FICHIERS CLES

### C# (Visual Studio)
```
app-csharp/CharlesNadejda/CharlesNadejda/
  Forms/
    FrmEditBase.cs              -- classe abstraite (cycle Valider/Sauvegarder)
    FrmListeBase.cs             -- classe generique (DGV + CRUD)
    FrmStockEdit.cs             -- CRUD stock
    FrmActiviteEdit.cs          -- CRUD activite
    FrmBomContexteEdit.cs       -- creation contexte + niveaux
    FrmIngredientEdit.cs        -- 11 controles d'edition
    FrmAchatEdit.cs             -- achat lot (prix HTVA/TVAC, DateTimePicker)
    FrmBomFicheEdit.cs          -- fiche recette BOM (composition)
    FrmIngredients.cs           -- liste ingredients (chips, alertes)
    FrmPrincipal.BoutiqueWeb.cs -- TabControl 3 onglets (partial class)
    FrmPrincipal.Production.cs  -- ecran production inline
    FrmBomProductionSimulation.cs -- production modale
    FrmVueStock.cs              -- vue stock global (3 sections, export CSV)
  DAL/
    StockDAL.cs                 -- INSERT/UPDATE + liaison M:N
    ActiviteDAL.cs              -- CRUD + desactivation transactionnelle
    BomContexteDAL.cs           -- InsertAvecNiveaux (transaction)
    IngredientDAL.cs            -- GetAll avec jointures + stock agrege
    LotDAL.cs                   -- GREATEST pour update preservant consommation
    BomFicheDAL.cs              -- Insert transactionnel (fiche + lignes)
    BomFicheLigneDAL.cs         -- COALESCE pour polymorphisme ingredient/fiche
    BomProductionDAL.cs         -- Simuler + Executer + ConsumeStock FIFO
    BomStockDAL.cs              -- stock produit (FIFO bom_stocks)
    ProduitWebDAL.cs            -- stock temps reel SUM(bom_stocks)
    CommandeWebDAL.cs           -- lecture seule (commandes web)
    VueStockGlobalDAL.cs        -- VIEW SQL + agregation
  Models/
    Ingredient.cs               -- proprietes calculees (StockRatio, EstEnAlerte)
    Lot.cs                      -- PrixUnitaireBase calcule
    BomFiche.cs                 -- CoutBatch, CoutUnitaire
    BomFicheLigne.cs            -- FK polymorphiques (ingredient/fiche)
    ProduitWeb.cs               -- StockDisponible, EstEnStock
    CommandeWeb.cs              -- NomCompletClient
  Helpers/
    UnitConvertisseur.cs        -- conversion masse/volume/piece
```

### Laravel (VS Code)
```
site-laravel/
  app/Http/Controllers/
    CatalogueController.php     -- index (eager loading) + show (404)
    Auth/RegisterController.php -- inscription + bcrypt + session
    Auth/LoginController.php    -- connexion + password_verify + throttle
    PanierController.php        -- 5 methodes AJAX + ownership check
    CommandeController.php      -- recap + valider (FIFO transactionnel)
  app/Http/Requests/
    RegisterRequest.php         -- regles validation + messages FR
  app/Http/Middleware/
    ClientAuth.php              -- session + compte actif
  app/Models/
    ProduitWeb.php              -- accessor stock_disponible
    Client.php                  -- fillable + hidden
    CommandeWeb.php             -- relation lignes
    BomStock.php                -- table bom_stocks
  resources/views/
    catalogue/index.blade.php   -- grille produits + pills categories
    catalogue/show.blade.php    -- detail produit + bouton conditionnel
    auth/register.blade.php     -- formulaire inscription + @error
    auth/login.blade.php        -- formulaire connexion
    panier/index.blade.php      -- panier avec actions AJAX
    commandes/recap.blade.php   -- recapitulatif avant paiement
    commandes/confirmation.blade.php -- confirmation commande
  public/js/
    panier.js                   -- fetch AJAX + CSRF + toast
```

---

## AIDE-MEMOIRE VOCABULAIRE TECHNIQUE

A utiliser dans les phrases pour montrer la maitrise du vocabulaire :

| Terme | Definition simple |
|-------|------------------|
| **DAL** | Data Access Layer -- couche qui centralise les acces base de donnees |
| **ORM** | Object-Relational Mapping -- manipuler la DB avec des objets au lieu de SQL |
| **FIFO** | First In First Out -- consommer les lots les plus anciens en premier |
| **CRUD** | Create, Read, Update, Delete -- les 4 operations de base |
| **FK** | Foreign Key -- cle etrangere, lien entre deux tables |
| **BOM** | Bill of Materials -- nomenclature, liste des composants d'un produit |
| **ACID** | Atomicity, Consistency, Isolation, Durability -- proprietes des transactions |
| **CSRF** | Cross-Site Request Forgery -- attaque par requete forgee depuis un autre site |
| **XSS** | Cross-Site Scripting -- injection de code JavaScript malveillant |
| **N+1** | Probleme de performance : N requetes supplementaires pour charger des relations |
| **Eager loading** | Charger les relations en avance avec `with()` pour eviter N+1 |
| **Bcrypt** | Algorithme de hachage lent, ideal pour les mots de passe |
| **Lock pessimiste** | Verrouiller les lignes pendant la transaction pour eviter les conflits |
| **Middleware** | Code execute avant le controleur, pour verifier l'authentification par exemple |
| **Partial class** | Classe C# repartie sur plusieurs fichiers (meme classe, fichiers differents) |
| **ErrorProvider** | Composant WinForms qui affiche des icones d'erreur a cote des champs |
| **Accessor** | Methode Laravel qui ajoute une propriete calculee a un modele |
| **Session fixation** | Attaque ou l'attaquant impose un session ID avant la connexion |
| **Soft delete** | Marquer comme inactif au lieu de supprimer physiquement (champ `actif`) |
