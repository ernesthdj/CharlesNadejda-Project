# DEFENSE_04 -- Publication Web & Vue Stock Global

> Document de defense orale -- Workflows exacts dans ArtisaStock (C# WinForms)
> Genere depuis le code source reel le 2026-05-21

---

## TABLE DES MATIERES

1. [Publication produit web (etape 9)](#1-publication-produit-web)
2. [Vue Stock Global (etape 15)](#2-vue-stock-global)
3. [Commandes web dans ArtisaStock](#3-commandes-web-dans-artisastock)

---

## 1. PUBLICATION PRODUIT WEB

### 1.1 Acces a l'ecran Boutique Web

L'ecran "Boutique en ligne" est un partial class de `FrmPrincipal`, defini dans :

**Fichier :** `Forms/FrmPrincipal.BoutiqueWeb.cs`

L'utilisateur clique sur l'entree "Boutique Web" dans la sidebar. Cela appelle `ShowBoutiqueWebScreen()` qui construit un `TabControl` avec **3 onglets** :

| Onglet | Index | Construction | Contenu |
|--------|-------|-------------|---------|
| Categories | 0 | Immediate (au chargement) | CRUD categories web |
| Produits | 1 | Lazy (au premier clic) | CRUD produits web + publication |
| Commandes | 2 | Lazy (au premier clic) | Lecture seule des commandes |

Les onglets 1 et 2 utilisent le pattern **lazy init** : ils ne sont construits qu'a la premiere visite (flags `_tabProduitsBuilt`, `_tabCommandesBuilt`).

### 1.2 Onglet Categories

**Formulaire d'edition :** `Forms/FrmCategorieWebEdit.cs` (herite de `FrmEditBase`)
**DAL :** `DAL/CategorieWebDAL.cs`
**Model :** `Models/CategorieWeb.cs`

#### Model CategorieWeb

```csharp
public class CategorieWeb
{
    public int    Id              { get; set; }
    public string Nom             { get; set; }
    public string Description     { get; set; }
    public int    OrdreAffichage  { get; set; }
    public bool   Actif           { get; set; }
    public int    NbProduits      { get; set; }  // calcule par le DAL
}
```

#### DGV Categories -- Colonnes

| Colonne | DataPropertyName | Visible | Description |
|---------|-----------------|---------|-------------|
| Id | Id | Non (masque) | PK |
| Nom | Nom | Oui | Nom de la categorie |
| Description | Description | Oui | Texte libre |
| Ordre | OrdreAffichage | Oui | Tri d'affichage |
| Produits | NbProduits | Oui | Nombre de produits publies dans cette categorie |
| Actif | Actif | Oui | Checkbox -- visible sur le site ou non |

#### Boutons Categories

- **+ Ajouter** : ouvre `FrmCategorieWebEdit(null)` en mode creation
- **Modifier** : ouvre `FrmCategorieWebEdit(catSelected)` en mode edition
- **Supprimer** : confirmation + `CategorieWebDAL.Delete(id)`. Message : "Les produits associes ne seront pas supprimes mais perdront leur categorie." (FK SET NULL en DB)

#### CategorieWebDAL -- Requetes SQL

**GetAll** (avec comptage produits) :
```sql
SELECT c.*,
       COUNT(p.id) AS nb_produits
FROM categories_web c
LEFT JOIN produits_web p ON p.id_categorie = c.id AND p.en_vente = 1
[WHERE c.actif = 1]  -- optionnel selon parametre includeInactifs
GROUP BY c.id
ORDER BY c.ordre_affichage, c.nom
```

**NomExiste** (unicite) :
```sql
SELECT COUNT(*) FROM categories_web WHERE nom = @nom AND id <> @id
```

**Insert** :
```sql
INSERT INTO categories_web (nom, description, ordre_affichage, actif)
VALUES (@nom, @desc, @ordre, @actif)
```

**Update** :
```sql
UPDATE categories_web
SET nom = @nom, description = @desc,
    ordre_affichage = @ordre, actif = @actif
WHERE id = @id
```

**Delete** :
```sql
DELETE FROM categories_web WHERE id = @id
```
> La FK `produits_web.id_categorie` est configuree `SET NULL` en DB.

#### FrmCategorieWebEdit -- Validation

1. Nom obligatoire (non vide)
2. Nom unique (appel `CategorieWebDAL.NomExiste()`)

### 1.3 Onglet Produits -- Publication d'une fiche BOM

**Formulaire d'edition :** `Forms/FrmProduitWebEdit.cs` (herite de `FrmEditBase`)
**DAL :** `DAL/ProduitWebDAL.cs`
**Model :** `Models/ProduitWeb.cs`

#### Model ProduitWeb

```csharp
public class ProduitWeb
{
    public int      Id              { get; set; }
    public int      IdBomFiche      { get; set; }   // FK vers bom_fiches
    public int?     IdCategorie     { get; set; }   // FK vers categories_web (nullable)
    public string   NomCommercial   { get; set; }
    public string   Description     { get; set; }
    public decimal  PrixVente       { get; set; }
    public string   ImagePath       { get; set; }   // chemin relatif dans le storage Laravel
    public bool     EnVente         { get; set; }   // publie ou non
    public int      OrdreAffichage  { get; set; }
    public DateTime DateCreation    { get; set; }

    // Jointures chargees par le DAL
    public string  NomFiche        { get; set; }   // bom_fiches.nom
    public string  NomCategorie    { get; set; }   // categories_web.nom

    // Calcule par le DAL : SUM(bom_stocks.quantite_disponible)
    public decimal StockDisponible { get; set; }

    public bool EstEnStock => StockDisponible > 0;
}
```

#### DGV Produits -- Colonnes

| Colonne | DataPropertyName | Format | Description |
|---------|-----------------|--------|-------------|
| Id | Id | Masque | PK |
| Produit | NomCommercial | Texte | Nom commercial |
| Categorie | NomCategorie | Texte | Nom de la categorie (jointure) |
| Prix (EUR) | PrixVente | N2, aligne droite | Prix de vente TTC |
| Stock | StockDisponible | N2, aligne droite | **Stock calcule depuis bom_stocks** |
| Publie | EnVente | Checkbox | Visible sur le site |
| Fiche BOM | NomFiche | Texte | Nom de la fiche BOM source |

**Coloration du stock :** vert si `> 0`, rouge si `<= 0` (via `ProdDgvFormatting`).

#### Boutons Produits

| Bouton | Action |
|--------|--------|
| **+ Publier fiche** | Ouvre `FrmProduitWebEdit(null)` -- mode creation |
| **Modifier** | Ouvre `FrmProduitWebEdit(prodSelected)` -- mode edition |
| **Publier/Depublier** | Toggle rapide `EnVente` sans ouvrir le formulaire |
| **Supprimer** | Verification referentielle + confirmation |

#### Workflow de publication -- Etape par etape

**Creation (publier une fiche) :**

1. L'utilisateur clique "**+ Publier fiche**"
2. `FrmProduitWebEdit` s'ouvre avec le titre "Publier une fiche en boutique"
3. **ComboBox "Fiche BOM"** : charge uniquement les fiches **non deja publiees** via `ProduitWebDAL.GetFichesNonPubliees()`
4. L'utilisateur selectionne une fiche BOM, choisit une categorie, saisit :
   - Nom commercial (obligatoire)
   - Description (optionnel, multiline)
   - Prix de vente (obligatoire, > 0)
   - Image (optionnel, via FileDialog)
   - Ordre d'affichage
   - Checkbox "Publie (visible sur le site)"
5. Clic "Enregistrer" : `Valider()` puis `Sauvegarder()`

**Edition :**

- La fiche BOM est **non modifiable** en edition (ComboBox desactive)
- Tous les autres champs sont editables

#### GetFichesNonPubliees -- SQL

```sql
SELECT f.id, f.nom, f.unite_output, f.quantite_output
FROM bom_fiches f
WHERE f.actif = 1
  AND f.id NOT IN (SELECT id_bom_fiche FROM produits_web)
ORDER BY f.nom
```

> Cela garantit qu'une fiche BOM ne peut etre publiee qu'une seule fois en boutique.

#### Gestion des images -- Partage avec Laravel

Le formulaire copie l'image selectionnee directement dans le **storage Laravel** :

```
Chemin cible : site-laravel/storage/app/public/produits/{id}_{timestamp}.{ext}
```

1. Le chemin Laravel est lu depuis `App.config` (cle `LaravelStoragePath`)
2. Si absent, fallback par navigation relative depuis `bin/Debug`
3. A la creation, le fichier est d'abord copie avec ID temporaire `0_`, puis renomme avec le vrai ID apres `INSERT`
4. En DB, le chemin est stocke avec des slashes (`produits/42_20260521123000.jpg`) pour compatibilite URL web

#### ProduitWebDAL -- Calcul du stock disponible

La requete `SELECT_BASE` calcule le stock en temps reel :

```sql
SELECT p.*, f.nom AS nom_fiche, c.nom AS nom_categorie,
       COALESCE(SUM(bs.quantite_disponible), 0) AS stock_disponible
FROM produits_web p
INNER JOIN bom_fiches f     ON f.id = p.id_bom_fiche
LEFT  JOIN categories_web c ON c.id = p.id_categorie
LEFT  JOIN bom_stocks bs    ON bs.id_fiche = p.id_bom_fiche
                            AND bs.quantite_disponible > 0
GROUP BY p.id
ORDER BY p.ordre_affichage, p.nom_commercial
```

**Logique :** `stock_disponible = SUM(bom_stocks.quantite_disponible)` ou les `bom_stocks` sont lies a la meme `bom_fiche`. Seuls les lots avec `quantite_disponible > 0` sont pris en compte.

#### ProduitWebDAL -- Insert

```sql
INSERT INTO produits_web
    (id_bom_fiche, id_categorie, nom_commercial, description,
     prix_vente, image_path, en_vente, ordre_affichage)
VALUES
    (@idFiche, @idCat, @nom, @desc,
     @prix, @img, @enVente, @ordre)
```

#### ProduitWebDAL -- Update

```sql
UPDATE produits_web
SET id_categorie = @idCat, nom_commercial = @nom,
    description = @desc, prix_vente = @prix,
    image_path = @img, en_vente = @enVente,
    ordre_affichage = @ordre
WHERE id = @id
```

> Note : `id_bom_fiche` n'est PAS dans le UPDATE -- la liaison fiche BOM est immuable apres creation.

#### ProduitWebDAL -- Toggle EnVente

```sql
UPDATE produits_web SET en_vente = NOT en_vente WHERE id = @id
```

#### ProduitWebDAL -- Suppression securisee

Avant suppression, verification referentielle :

```sql
SELECT COUNT(*) FROM commandes_web_lignes WHERE id_produit_web = @id
```

Si le produit est reference dans des commandes, la suppression est **refusee** avec le message :
> "Ce produit ne peut pas etre supprime car il est reference dans des commandes. Utilisez la depublication a la place."

```sql
DELETE FROM produits_web WHERE id = @id
```

#### FrmProduitWebEdit -- Champs du formulaire

| Champ | Type | Obligatoire | Notes |
|-------|------|-------------|-------|
| Fiche BOM | ComboBox (DropDownList) | Oui (creation) | Desactive en edition |
| Categorie | ComboBox (DropDownList) | Non | "(Aucune)" en premier |
| Nom commercial | TextBox | Oui | Texte libre |
| Description | TextBox multiline | Non | Zone de texte 60px hauteur |
| Prix de vente (EUR) | NumericUpDown | Oui (> 0) | Min 0.01, Max 99999, 2 decimales |
| Image | FileDialog + PictureBox | Non | JPG/JPEG/PNG/WEBP |
| Ordre d'affichage | NumericUpDown | Non | 0-999 |
| Publie | CheckBox | -- | Coche par defaut |

#### FrmProduitWebEdit -- Validation

1. Fiche BOM selectionnee (en creation uniquement)
2. Nom commercial non vide
3. Prix > 0

---

## 2. VUE STOCK GLOBAL

### 2.1 Acces

**Fichier :** `Forms/FrmVueStock.cs`
**DAL :** `DAL/VueStockGlobalDAL.cs`
**Model :** `Models/VueStockGlobal.cs`

L'utilisateur accede a la Vue Stock Global depuis la sidebar. Elle s'ouvre comme une **fenetre modale** (`Form`, pas un ecran integre au panel droit).

### 2.2 Architecture de donnees

La vue lit une **VIEW SQL** nommee `vue_stock_global` qui unifie deux sources :

| type_stock | Source DB | Description |
|-----------|----------|-------------|
| `lot_ingredient` | `lots_ingredients` | Matieres premieres (lots d'achat) |
| `produit_fabrique` | `bom_stocks` | Produits issus de la production BOM |

#### Model VueStockGlobal

```csharp
public class VueStockGlobal
{
    public string    TypeStock           { get; set; } // "lot_ingredient" | "produit_fabrique"
    public int       IdEntree            { get; set; }
    public string    Nom                 { get; set; }
    public string    Unite               { get; set; }
    public decimal   QuantiteTotale      { get; set; }
    public decimal   QuantiteReservee    { get; set; }
    public decimal   QuantiteDispoReelle { get; set; }
    public decimal   CoutUnitaire        { get; set; }
    public decimal?  PrixConditionnement { get; set; }
    public decimal?  QteParConditionnement { get; set; }
    public string    ConditionnementLabel { get; set; }
    public DateTime? DateDlc             { get; set; }

    // Lots uniquement
    public int?      IdStock             { get; set; }
    public string    StockNom            { get; set; }
    public int?      IdFicheIngredient   { get; set; }

    // Produits fabriques uniquement
    public int?      IdActivite          { get; set; }
    public string    NomActivite         { get; set; }
    public int?      IdContexte          { get; set; }
    public int?      IdNiveau            { get; set; }
    public int?      IdFicheBom          { get; set; }

    public bool EstLot           => TypeStock == "lot_ingredient";
    public bool EstEnAlerte      => QuantiteDispoReelle <= 0;
    public bool ADesReservations => QuantiteReservee > 0;
}
```

### 2.3 Interface utilisateur

#### Layout global

```
+----------------------------------------------------------+
| [Bandeau]  VUE STOCK GLOBAL                              |
|  Lots ingredients + produits fabriques -- lecture seule   |
+----------------------------------------------------------+
| [Chips]  [Tous] [Patisserie] [Boulangerie] [Chocolat]    |
+----------------------------------------------------------+
|                                          |  VOLET DETAIL  |
|  [DGV -- grille principale]             |  (320px droit)  |
|                                          |                 |
|  INGREDIENTS (12)                        |  Nom produit    |
|  Farine T55    150.000 kg    ...         |  Type: ...      |
|  Beurre        42.000 kg     ...         |  STOCK          |
|                                          |  Disponible: .. |
|  PRODUITS INTERMEDIAIRES (3)             |  Reserve: ...   |
|  Creme patiss. 8.500 L       ...         |  COMPOSITION    |
|                                          |  - Lait 2L      |
|  PRODUITS FINALS (5)                     |  - Sucre 500g   |
|  Croissant     120.000 pce   ...         |                 |
+----------------------------------------------------------+
| [Exporter CSV]  [Vert] Dispo [Orange] Reserve [Rouge] Pen|
+----------------------------------------------------------+
```

#### Colonnes du DGV

| Colonne | HeaderText | Contenu | MinWidth |
|---------|-----------|---------|----------|
| TypeStock | Type | "Ingredient" ou "Intermediaire" ou "Produit final" | 80 |
| Nom | Nom | Nom de l'ingredient/produit | 180 |
| Dispo | Disponible | Quantite dispo reelle + unite | 110 |
| Reservee | Reserve | Quantite reservee ou "--" | 100 |
| Total | Total | Quantite totale + unite | 100 |
| DLC | DLC | Date limite, format dd/MM/yyyy | 90 |
| StockOuAct | Stock / Activite | Nom du stock (lots) ou activite (produits) | 140 |
| CoutUnit | Cout unit. | Prix par unite de base | 90 |
| CoutTotal | Valeur stock | Cout unitaire x quantite totale | 100 |

### 2.4 Filtrage par activite (Chips)

Le bandeau de chips charge toutes les activites via `ActiviteDAL.GetAll()` et ajoute un chip "Tous" (id = 0).

- **Tous** : `VueStockGlobalDAL.GetAll()` -- tout le stock
- **Activite specifique** : `VueStockGlobalDAL.GetByActivite(idActivite)`

#### SQL GetAll

```sql
SELECT vsg.*, a.nom AS nom_activite
FROM vue_stock_global vsg
LEFT JOIN activites a ON a.id = vsg.id_activite
ORDER BY vsg.type_stock, vsg.date_dlc ASC
```

#### SQL GetByActivite

```sql
SELECT vsg.*, a.nom AS nom_activite
FROM vue_stock_global vsg
LEFT JOIN activites a ON a.id = vsg.id_activite
WHERE (    vsg.type_stock = 'lot_ingredient'
       AND vsg.id_stock IN (
               SELECT id_stock FROM activites_stocks
               WHERE id_activite = @p))
   OR (    vsg.type_stock = 'produit_fabrique'
       AND vsg.id_activite = @p)
ORDER BY vsg.type_stock, vsg.date_dlc ASC
```

> Logique de filtrage :
> - **Lots** : filtre via la table de liaison M:N `activites_stocks` (un stock physique peut servir plusieurs activites)
> - **Produits fabriques** : filtre via `id_activite` direct sur `bom_stocks`

### 2.5 Agregation par fiche ingredient

Apres le chargement, les donnees passent par `VueStockGlobalDAL.AgregerParFiche()` :

**Principe :** Plusieurs lots du meme ingredient (ex: 3 sacs de farine achetes a des dates differentes) sont **agreges en une seule ligne** par `IdFicheIngredient`.

```
Avant agregation :  Farine T55 (lot #1, 50kg) + Farine T55 (lot #2, 30kg) + Farine T55 (lot #3, 70kg)
Apres agregation :  Farine T55 : 150 kg total
```

**Calculs d'agregation :**

| Champ | Calcul |
|-------|--------|
| QuantiteTotale | `SUM(qte)` |
| QuantiteReservee | `SUM(reservee)` |
| QuantiteDispoReelle | `total - reservee` |
| CoutUnitaire | Moyenne ponderee : `SUM(cout * qte) / SUM(qte)` |
| DateDlc | `MIN(dlc)` parmi les lots ayant une DLC (plus proche echeance) |

Les **produits fabriques** passent **sans agregation** (1 bom_stock = 1 ligne).

### 2.6 Les 3 sections

Les lignes sont reparties en 3 sections avec **headers de section** colores (fond `ChocoBrand`, texte `Or`) :

| Section | Critere |
|---------|---------|
| INGREDIENTS | `EstLot == true` (type_stock = 'lot_ingredient') |
| PRODUITS INTERMEDIAIRES | `EstLot == false` ET `Ordre du niveau < OrdreMax du contexte` |
| PRODUITS FINALS | `EstLot == false` ET `Ordre du niveau >= OrdreMax du contexte` |

**Determination intermediaire vs final :**

1. Pour chaque produit fabrique, on recupere `IdContexte` et `IdNiveau`
2. `BomNiveauDAL.GetOrdreMax(idContexte)` retourne l'ordre maximum dans ce contexte
3. `BomNiveauDAL.GetById(idNiveau).Ordre` retourne l'ordre du niveau du produit
4. Si `ordreNiveau < ordreMax` : intermediaire. Si `ordreNiveau >= ordreMax` : final.

Les ordres de niveaux sont mis en cache (`_cacheOrdreNiveau`) pour eviter des requetes repetees.

### 2.7 Coloration des lignes (Color coding)

#### Couleur de fond de la ligne entiere

| Condition | Couleur | Constante |
|-----------|---------|-----------|
| `QuantiteDispoReelle <= 0` | Rouge | `AppColors.RougePenur` |
| `QuantiteReservee > 0` | Orange | `AppColors.OrangeReserv` |
| Sinon | Vert | `AppColors.VertDispo` |

> Priorite : rouge > orange > vert (la premiere condition vraie l'emporte).

#### Colonne DLC -- surcharge independante

| Condition | Couleur cellule DLC |
|-----------|-------------------|
| DLC expiree (`DateDlc < aujourd'hui`) | Rouge vif `(220, 80, 80)` |
| DLC < 7 jours (`< 7 jours restants`) | Orange vif `(255, 165, 0)` |

### 2.8 Volet detail (panneau droit, 320px)

Quand l'utilisateur **selectionne une ligne** dans le DGV, un panneau de detail s'affiche a droite.

#### Detail INGREDIENT (clic sur un lot agrege)

Sections affichees :
1. **En-tete** : nom + badge "Ingredient"
2. **IDENTITE** : marque, type physique, densite, unite de base, conditionnement, stock de rattachement
3. **STOCK** : disponible, reserve (orange si > 0), total, seuil d'alerte, etat (alerte ou OK)
4. **PRIX** : cout moyen/unite, valeur stock totale
5. **LOTS (N)** : liste de chaque lot individuel avec numero, quantite, DLC (coloree si proche/expiree)
6. **UTILISE DANS** : liste des fiches BOM qui utilisent cet ingredient (via `BomFicheLigneDAL.GetFichesUtilisant`)

#### Detail PRODUIT FABRIQUE (clic sur un bom_stock)

Sections affichees :
1. **En-tete** : nom + badge "Produit fabrique"
2. **IDENTITE** : unite, activite, contexte BOM, niveau BOM, temps de preparation
3. **STOCK** : disponible, cout unitaire, valeur stock, DLC (coloree si expiree)
4. **PRODUCTION** : date de production, ID production
5. **COMPOSITION** : liste des lignes de la fiche BOM (ingredient + quantite) -- charge via `BomFicheDAL.GetById(id, avecLignes: true)`
6. **CONSOMME PAR** : liste des fiches BOM qui consomment ce produit (via `BomFicheLigneDAL.GetFichesConsommant`)

### 2.9 Export CSV

Bouton "Exporter CSV" en bas a gauche de la fenetre.

**Comportement :**
1. `SaveFileDialog` s'ouvre, nom par defaut : `stock_global_2026-05-21.csv`
2. Repertoire initial : Mes Documents
3. Encodage : UTF-8 avec BOM
4. Separateur : point-virgule (`;`)
5. En-tete CSV : `Type;Nom;Disponible;Reserve;Total;DLC;Stock / Activite;Cout unit.;Valeur stock`
6. Les champs contenant `;` sont echappes entre guillemets
7. Message de confirmation apres export

**Colonnes exportees :**

| Colonne CSV | Contenu |
|------------|---------|
| Type | "Ingredient" ou "Produit BOM" |
| Nom | Nom de l'element |
| Disponible | Quantite + unite (3 decimales) |
| Reserve | Quantite + unite |
| Total | Quantite + unite |
| DLC | dd/MM/yyyy ou vide |
| Stock / Activite | Nom du stock physique ou nom de l'activite |
| Cout unit. | Prix en F2 ou vide |
| Valeur stock | Cout x qte en F2 ou vide |

### 2.10 Barre du bas -- Legende + Stats

- **Legende** : 3 carres colores avec labels (Disponible / Reserve / Penurie/DLC)
- **Stats** : `{N} entrees -- {M} penuries`
- **Bouton Fermer** : ferme la fenetre (aligne a droite, repositionne au Resize)

---

## 3. COMMANDES WEB DANS ARTISASTOCK

### 3.1 Onglet Commandes dans FrmPrincipal.BoutiqueWeb.cs

**Oui**, il existe un onglet Commandes (index 2) dans l'ecran Boutique Web. Il est construit en lazy init au premier clic.

**DAL :** `DAL/CommandeWebDAL.cs` (lecture seule)
**Models :** `Models/CommandeWeb.cs` + `Models/CommandeWebLigne.cs`

> Le DAL est explicitement documente comme **lecture seule** : "Les commandes sont creees par le site Laravel, consultees par l'admin dans l'ERP."

### 3.2 Model CommandeWeb

```csharp
public class CommandeWeb
{
    public int       Id                 { get; set; }
    public int       IdClient           { get; set; }
    public string    Statut             { get; set; }   // "panier", "payee", "annulee"
    public decimal   TotalTtc           { get; set; }
    public string    AdresseLivraison   { get; set; }
    public DateTime? DateCommande       { get; set; }
    public DateTime  DateCreation       { get; set; }

    // Jointures
    public string NomClient    { get; set; }
    public string PrenomClient { get; set; }
    public string EmailClient  { get; set; }

    public int NbArticles { get; set; }  // sous-requete COUNT
    public List<CommandeWebLigne> Lignes { get; set; }

    public string NomCompletClient => $"{PrenomClient} {NomClient}";
}
```

### 3.3 Model CommandeWebLigne

```csharp
public class CommandeWebLigne
{
    public int     Id             { get; set; }
    public int     IdCommande     { get; set; }
    public int     IdProduitWeb   { get; set; }
    public int     Quantite       { get; set; }
    public decimal PrixUnitaire   { get; set; }
    public decimal SousTotal      { get; set; }   // colonne GENERATED en DB
    public string  NomProduit     { get; set; }   // jointure produits_web.nom_commercial
}
```

### 3.4 Interface de l'onglet Commandes

#### Filtre par statut

En haut de l'onglet, un ComboBox avec 3 options :
- **Toutes** (par defaut) : pas de filtre statut
- **payee** : uniquement les commandes payees
- **annulee** : uniquement les commandes annulees

> Les paniers en cours (`statut = 'panier'`) sont **toujours exclus** de la vue ERP.

#### DGV Commandes -- Colonnes

| Colonne | DataPropertyName | Format | Description |
|---------|-----------------|--------|-------------|
| N | Id | Entier | Numero de commande |
| Client | NomCompletClient | Texte | "Prenom Nom" |
| Date | DateCommande | dd/MM/yyyy HH:mm | Date de validation |
| Articles | NbArticles | Centre | Nombre de lignes |
| Total (EUR) | TotalTtc | N2, droite | Montant total TTC |
| Statut | Statut | Texte | "payee" ou "annulee" |

#### Panneau de detail (bas, 160px)

Quand l'utilisateur **selectionne une commande**, un panneau de detail s'affiche en bas :

1. **En-tete** : `Commande #42 -- Jean Dupont . jean@mail.com`
2. **Adresse** : adresse de livraison (si renseignee)
3. **Lignes** : pour chaque article, `NomProduit x Quantite -- SousTotal EUR`
4. **Total** : montant total en gras, couleur or (`AppColors.Or`)

Le detail est charge via `CommandeWebDAL.GetById(id)` qui execute **deux requetes** :
- Header (commande + client)
- Lignes (articles + jointure produit)

### 3.5 CommandeWebDAL -- Requetes SQL

**GetAll** (liste des commandes) :

```sql
SELECT cmd.id, cmd.id_client, cmd.statut, cmd.total_ttc,
       cmd.adresse_livraison, cmd.date_commande, cmd.date_creation,
       cl.nom       AS nom_client,
       cl.prenom    AS prenom_client,
       cl.email     AS email_client,
       (SELECT COUNT(*) FROM commandes_web_lignes l
        WHERE l.id_commande = cmd.id) AS nb_articles
FROM commandes_web cmd
INNER JOIN clients cl ON cl.id = cmd.id_client
WHERE cmd.statut <> 'panier'
[AND cmd.statut = @statut]  -- optionnel
ORDER BY cmd.date_commande DESC
```

**GetById** (commande + lignes) :

```sql
-- Header
SELECT ... FROM commandes_web cmd
INNER JOIN clients cl ON cl.id = cmd.id_client
WHERE cmd.id = @id

-- Lignes
SELECT l.id, l.id_commande, l.id_produit_web,
       l.quantite, l.prix_unitaire, l.sous_total,
       p.nom_commercial AS nom_produit
FROM commandes_web_lignes l
INNER JOIN produits_web p ON p.id = l.id_produit_web
WHERE l.id_commande = @id
ORDER BY l.id
```

**GetCountByStatut** (statistiques pour le dashboard) :

```sql
SELECT COUNT(*) FROM commandes_web WHERE statut = @statut
```

### 3.6 Ce que l'admin peut faire avec les commandes

| Action | Disponible | Details |
|--------|-----------|---------|
| Voir la liste | Oui | DGV avec filtre par statut |
| Voir le detail | Oui | Panneau bas avec lignes + total |
| Filtrer par statut | Oui | ComboBox : Toutes / payee / annulee |
| Changer le statut | **Non** | Le DAL est lecture seule, pas de methode Update |
| Supprimer une commande | **Non** | Aucune methode Delete dans le DAL |
| Creer une commande | **Non** | Creation exclusivement via le site Laravel |

> Le C# est un **viewer** des commandes web, pas un gestionnaire. Toute la logique de creation/paiement est cote Laravel.

---

## RESUME DES FICHIERS IMPLIQUES

### Publication Web

| Fichier | Role |
|---------|------|
| `Forms/FrmPrincipal.BoutiqueWeb.cs` | Ecran principal avec 3 onglets (partial class de FrmPrincipal) |
| `Forms/FrmProduitWebEdit.cs` | Formulaire creation/edition d'un produit web |
| `Forms/FrmCategorieWebEdit.cs` | Formulaire creation/edition d'une categorie |
| `Models/ProduitWeb.cs` | Model avec jointures et stock calcule |
| `Models/CategorieWeb.cs` | Model avec comptage produits |
| `Models/CommandeWeb.cs` | Model commande avec lignes |
| `Models/CommandeWebLigne.cs` | Model ligne de commande |
| `DAL/ProduitWebDAL.cs` | CRUD + calcul stock via SUM(bom_stocks) |
| `DAL/CategorieWebDAL.cs` | CRUD + unicite nom + FK SET NULL |
| `DAL/CommandeWebDAL.cs` | Lecture seule -- commandes creees par Laravel |

### Vue Stock Global

| Fichier | Role |
|---------|------|
| `Forms/FrmVueStock.cs` | Fenetre modale avec DGV + volet detail + export CSV |
| `DAL/VueStockGlobalDAL.cs` | Lecture VIEW + filtrage activite + agregation par fiche |
| `Models/VueStockGlobal.cs` | Model unifiant lots + bom_stocks |

---

## POINTS CLES POUR LA DEFENSE ORALE

1. **Lien ERP-Web** : la publication lie un `produit_web` a une `bom_fiche` via FK `id_bom_fiche`. Une fiche ne peut etre publiee qu'une fois.

2. **Stock en temps reel** : le stock affiche dans l'onglet Produits est calcule en direct par `SUM(bom_stocks.quantite_disponible)` -- pas de cache, pas de valeur stockee.

3. **Images partagees** : le C# copie les images directement dans le storage Laravel (`storage/app/public/produits/`), ce qui permet au site web de les servir sans synchronisation.

4. **Commandes read-only** : l'ERP C# ne peut que consulter les commandes web, pas les modifier. C'est une decision architecturale deliberee (separation des responsabilites).

5. **Vue Stock Global** : la VIEW SQL `vue_stock_global` est la source de verite unique. L'agregation par fiche se fait cote C# (`AgregerParFiche`), pas en SQL.

6. **3 niveaux de stock** : ingredients (lots d'achat), intermediaires (sous-produits BOM), finals (produits vendables) -- determines dynamiquement par l'ordre des niveaux BOM.

7. **Color coding triple** : rouge (penurie), orange (reservation active), vert (disponible) + DLC independante (rouge expire, orange < 7 jours).
