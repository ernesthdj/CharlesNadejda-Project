# DEFENSE_02 -- Pipeline Production : Ingredients & Achats

> Document de defense d'examen -- workflow utilisateur exact, etapes 5 et 6.
> Source : code C# WinForms lu directement. Aucune approximation.

---

## Table des matieres

1. [Etape 5 -- Fiches Ingredients](#etape-5--fiches-ingredients)
2. [Etape 6 -- Achats (Lots d'ingredients)](#etape-6--achats-lots-dingrédients)

---

## Etape 5 -- Fiches Ingredients

### 5.1 Navigation vers la liste

**Point d'entree :** Sidebar > `NavItemId.Ingredients`

```
FrmPrincipal.cs ligne 162 :
  NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));

FrmPrincipal.cs ligne 593 :
  case RessourceType.Ingredients:
      frm = new FrmIngredients(_state.ActiveActivite, filtreAlertes);
```

- Si une activite est selectionnee dans le sidebar, `_state.ActiveActivite` filtre les ingredients par les stocks lies a cette activite.
- Le parametre `filtreAlertes` (bool) est pilote par `_state.SetFiltreAlertes()`, active depuis le Hub via la carte "Alertes".
- Deux chemins Hub :
  - **Carte "Ingredients"** : `filtreAlertes = false` -- affiche tout.
  - **Carte "Alertes"** : `filtreAlertes = true` -- uniquement les ingredients en alerte.

### 5.2 FrmIngredients -- Liste des fiches

**Classe :** `FrmIngredients : FrmListeBase<Ingredient>`
**Fichier :** `Forms/FrmIngredients.cs`

#### Chargement des donnees

```csharp
protected override List<Ingredient> ChargerDonnees()
{
    List<Ingredient> liste = _stockFiltre != null
        ? IngredientDAL.GetAll(idStock: _stockFiltre.Id)
        : IngredientDAL.GetAll();

    if (_filtreAlertes)
        liste = liste.Where(i => i.EstEnAlerte).ToList();
    return liste;
}
```

#### Colonnes visibles dans le DGV (DataGridView)

| Propriete             | Header affiche    | Largeur | Min |
|----------------------|-------------------|---------|-----|
| `Nom`                | Ingredient        | 180 px  | 120 |
| `ConditionnementLabel` | Conditionnement | 140 px  | 90  |
| `TypePhysique`       | Type physique     | 90 px   | 65  |
| `Densite`            | Densite           | 70 px   | 55  |
| `NomFournisseur`     | Fournisseur       | 140 px  | 90  |
| `Description`        | Description       | 180 px  | 100 |
| `StockNom`           | Stock (lieu)      | 110 px  | 75  |

**Colonnes cachees :** `Id`, `IdFournisseurDefaut`, `IdStock`, `Actif`, `EstEnAlerte`, `Marque`, `SeuilAlerteStock`, `QteParConditionnement`, `PrixParUniteBase`, `StockActuel`, `PrixAchatReference`, `UniteMesure`.

#### Indicateur d'alerte stock

```csharp
protected override void AppliquerStylesLignes()
{
    foreach (DataGridViewRow row in dgv.Rows)
        if (row.DataBoundItem is Ingredient ing && ing.EstEnAlerte)
            row.DefaultCellStyle.BackColor = Color.MistyRose;
}
```

Les lignes dont `EstEnAlerte == true` sont colorees en **rose pale (MistyRose)**.

#### Filtre par stock -- Chip Panel

Un `FlowLayoutPanel` de "chips" est construit au `OnLoad`. Chaque chip represente un stock physique (issu de `StockDAL.GetAll()`), plus un chip "Tous" (pas de filtre).

- **Chip actif** : fond `AppColors.ChocoMed`, texte blanc, gras.
- **Chip inactif** : fond gris-beige `(220,210,200)`, texte brun `(60,45,30)`.
- **Clic sur un chip** : met a jour `_stockFiltre` et appelle `Charger()` qui relance `ChargerDonnees()` avec le nouveau filtre.

#### Boutons CRUD (herites de FrmListeBase)

| Bouton       | Action                                            |
|-------------|---------------------------------------------------|
| + Ajouter   | `new FrmIngredientEdit(null, _stockFiltre)` -- creation, pre-selectionne le stock filtre courant |
| Modifier    | `new FrmIngredientEdit(element)` -- edition         |
| Supprimer   | Confirmation YesNo (defaut sur Non) puis `IngredientDAL.Delete(id)` |
| Fermer      | Ferme le formulaire                                |

Double-clic sur une ligne = Modifier (Nielsen #7).

---

### 5.3 FrmIngredientEdit -- Formulaire de creation/modification

**Classe :** `FrmIngredientEdit : FrmEditBase`
**Fichier :** `Forms/FrmIngredientEdit.cs`

#### Tous les champs du formulaire

| # | Label affiche                   | Controle             | Type            | Obligatoire | Position |
|---|--------------------------------|---------------------|-----------------|-------------|----------|
| 1 | **Nom \***                     | `txtNom`            | TextBox         | OUI         | Ligne 1  |
| 2 | Marque                         | `txtMarque`         | TextBox         | non         | Ligne 2  |
| 3 | **Unite de base \***           | `cmbUnite`          | ComboBox (DDL)  | OUI         | Ligne 3L |
| 4 | **Type physique \***           | `cmbTypePhysique`   | ComboBox (DDL)  | OUI         | Ligne 3R |
| 5 | Prix ref. (EUR/conditionnement)   | `nudPrix`           | NumericUpDown   | non         | Ligne 4L |
| 6 | **Densite (g/ml) \***          | `nudDensite`        | NumericUpDown   | CONDITIONNEL| Ligne 4R |
| 7 | **Conditionnement \***         | `nudQteConditionnement` | NumericUpDown | OUI (>0)   | Ligne 5  |
| 8 | Seuil alerte stock             | `txtSeuil`          | TextBox         | non         | Ligne 6L |
| 9 | Stock cible (pieces)           | `nudStockCible`     | NumericUpDown   | non         | Ligne 6R |
| 10| Fournisseur par defaut         | `cmbFournisseur`    | ComboBox (DDL)  | non         | Ligne 7  |
| 11| **Stock \***                   | `cmbStock`          | ComboBox (DDL)  | OUI         | Ligne 8  |

**Valeurs des ComboBox :**
- `cmbUnite` : `"mg"`, `"g"`, `"kg"`, `"ml"`, `"cl"`, `"dl"`, `"l"`, `"piece"`
- `cmbTypePhysique` : `"solide"`, `"liquide"`, `"poudre"`, `"piece"`
- `cmbFournisseur` : `"-- Aucun --"` + tous les fournisseurs (`FournisseurDAL.GetAll()`)
- `cmbStock` : tous les stocks physiques (`StockDAL.GetAll()`)

#### Comportements dynamiques

1. **Visibilite densite :** Quand `cmbTypePhysique` change, la densite n'est visible QUE si `"liquide"` ou `"poudre"`. Pour `"solide"` et `"piece"`, le champ est masque.
2. **Label unite dynamique :** `lblUniteQteCond` affiche l'unite selectionnee dans `cmbUnite` a cote du champ conditionnement (ex: "1000 g").
3. **Stock cible en pieces :** A l'affichage en edition, `nudStockCible.Value = StockCible / QteParConditionnement`. A la sauvegarde, `StockCible = nudStockCible.Value * nudQteConditionnement.Value` (conversion pieces -> unite de base).

#### Regles de validation (methode `Valider()`)

```csharp
1. txtNom vide                    -> "Obligatoire."
2. IngredientDAL.NomExiste(nom)   -> "Ce nom existe deja."
   (exclut l'ID en cours en mode edition)
3. cmbUnite == null               -> "Choisissez une unite de base."
4. nudQteConditionnement <= 0     -> "Doit etre > 0."
5. cmbTypePhysique == null        -> "Choisissez un type physique."
6. cmbStock == null               -> "Choisissez un stock."
7. (liquide|poudre) && densite<=0 -> "La densite est obligatoire pour ce type."
```

Toutes les erreurs sont affichees via `ErrorProvider` (icone d'erreur a cote du controle, sans blink).

#### Sauvegarde (methode `Sauvegarder()`)

Logique de conversion avant persistance :
```
seuil         = decimal.TryParse(txtSeuil) ou null
stockCible    = nudStockCible * nudQteConditionnement  (pieces -> unite base)
densite       = affectee uniquement si liquide/poudre, sinon null
fournisseur   = ID du Fournisseur selectionne ou null si "-- Aucun --"
```

Puis :
- Mode creation : `IngredientDAL.Insert(_ing)` -- retourne l'ID genere.
- Mode edition : `IngredientDAL.Update(_ing)`.

---

### 5.4 IngredientDAL -- Couche d'acces aux donnees

**Fichier :** `DAL/IngredientDAL.cs`

#### NomExiste -- Verification d'unicite

```sql
SELECT COUNT(*) FROM fiches_ingredients WHERE nom = @nom AND id <> @id
```

Retourne `true` si le nom existe deja (en excluant l'enregistrement courant en edition).

#### Insert -- SQL exact

```sql
INSERT INTO fiches_ingredients
    (nom, marque, description, unite_mesure, type_physique, densite,
     conditionnement_label, qte_par_conditionnement,
     prix_achat_reference, seuil_alerte_stock, stock_cible,
     id_fournisseur_defaut, id_stock, actif)
VALUES (@nom, @marque, @desc, @unite, @type_physique, @densite,
        @condLabel, @condQte,
        @prix, @seuil, @stockCible, @fournisseur, @idStock, 1)
```

- `actif` est toujours insere a `1` (soft delete pattern).
- Retourne `cmd.LastInsertedId` (ID auto-increment MySQL).

#### Update -- SQL exact

```sql
UPDATE fiches_ingredients
SET nom=@nom, marque=@marque, description=@desc, unite_mesure=@unite,
    type_physique=@type_physique, densite=@densite,
    conditionnement_label=@condLabel, qte_par_conditionnement=@condQte,
    prix_achat_reference=@prix, seuil_alerte_stock=@seuil,
    stock_cible=@stockCible,
    id_fournisseur_defaut=@fournisseur, id_stock=@idStock
WHERE id=@id
```

#### GetAll -- SQL avec jointures et stock agrege

```sql
SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure,
       fi.type_physique, fi.densite,
       fi.conditionnement_label, fi.qte_par_conditionnement,
       fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
       fi.id_fournisseur_defaut, fi.id_stock, fi.actif,
       f.nom  AS nom_fournisseur,
       s.nom  AS nom_stock,
       COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
FROM fiches_ingredients fi
LEFT  JOIN fournisseurs      f ON f.id = fi.id_fournisseur_defaut
INNER JOIN stocks            s ON s.id = fi.id_stock
LEFT  JOIN lots_ingredients  l ON l.id_fiche_ingredient = fi.id
WHERE fi.actif = 1
  [AND fi.id_stock = @idStock]                           -- filtre par stock
  [AND fi.id_stock IN (SELECT id_stock FROM activites_stocks
                       WHERE id_activite = @idActivite)]  -- filtre par activite
GROUP BY fi.id
ORDER BY fi.nom
```

Points cles :
- Le `stock_actuel` est calcule par `SUM(lots_ingredients.quantite_disponible)` -- aggregation directe en SQL.
- Filtre `actif = 1` : soft delete, les ingredients desactives n'apparaissent jamais.
- Requetes parametrees (`@param`) : securite injection SQL.

#### Bind -- Parametres MySqlCommand

Gere les `DBNull.Value` pour tous les champs nullables : `marque`, `description`, `densite`, `seuil_alerte_stock`, `stock_cible`, `id_fournisseur_defaut`.

---

### 5.5 Modele Ingredient -- Proprietes calculees

**Fichier :** `Models/Ingredient.cs`

#### Proprietes stockees en DB

| Propriete              | Type      | Description                                    |
|-----------------------|-----------|------------------------------------------------|
| `Id`                  | int       | PK auto-increment                              |
| `Nom`                 | string    | Nom unique de l'ingredient                     |
| `Marque`              | string?   | Marque commerciale (nullable)                  |
| `Description`         | string?   | Description libre (nullable)                   |
| `UniteMesure`         | string    | Unite atomique : g, ml, piece                  |
| `TypePhysique`        | string    | solide, liquide, poudre, piece                 |
| `Densite`             | decimal?  | g/ml -- obligatoire si liquide/poudre          |
| `ConditionnementLabel`| string    | Label commercial (ex: "Sac 10 kg")             |
| `QteParConditionnement`| decimal  | Quantite en unite de base par conditionnement  |
| `PrixAchatReference`  | decimal   | Prix de reference par conditionnement (EUR)       |
| `SeuilAlerteStock`    | decimal?  | Seuil en dessous duquel l'alerte se declenche  |
| `StockCible`          | decimal?  | Stock cible (100% de la jauge) en unite de base|
| `IdFournisseurDefaut` | int?      | FK vers fournisseurs (nullable)                |
| `IdStock`             | int       | FK vers stocks (obligatoire)                   |
| `Actif`               | bool      | Soft delete flag                               |

#### Proprietes calculees (computed en C#)

```csharp
// Cout par unite de base (EUR/g, EUR/ml, EUR/pce)
PrixParUniteBase = PrixAchatReference / QteParConditionnement

// Alerte stock : true si StockActuel <= SeuilAlerteStock
EstEnAlerte = SeuilAlerteStock.HasValue && StockActuel <= SeuilAlerteStock.Value

// Nombre de pieces (conditionnements) en stock
StockPieces = Math.Floor(StockActuel / QteParConditionnement)

// Ratio jauge : 0.0 a N (>1 = surstock)
StockRatio = StockActuel / StockCible    // null si pas de cible
```

#### Jauge de stock -- Comment ca fonctionne

La jauge de stock repose sur trois valeurs :

1. **`StockActuel`** -- rempli par le DAL via `COALESCE(SUM(lots.quantite_disponible), 0)`. Represente la somme de toutes les quantites disponibles dans les lots.
2. **`StockCible`** -- parametre par l'utilisateur (en nombre de pieces dans le formulaire, converti en unite de base a la sauvegarde). Represente le 100% de la jauge.
3. **`StockRatio`** -- `StockActuel / StockCible`. Un ratio de 0.5 = 50% rempli. Un ratio > 1.0 = surstock.

L'alerte (`EstEnAlerte`) est un mecanisme independant de la jauge, base sur `SeuilAlerteStock` (un seuil fixe en unite de base). Quand `StockActuel <= SeuilAlerteStock`, la fiche est marquee en alerte (ligne rose dans la liste).

---

## Etape 6 -- Achats (Lots d'ingredients)

### 6.1 Navigation vers la liste

**Point d'entree :** Sidebar > `NavItemId.AchatsLots`

```
FrmPrincipal.cs ligne 156 :
  NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Achats));

FrmPrincipal.cs ligne 594 :
  case RessourceType.Achats:
      frm = new FrmAchats(_state.ActiveActivite);
```

Si une activite est active, les achats sont filtres aux ingredients des stocks lies a cette activite.

### 6.2 FrmAchats -- Liste des achats

**Classe :** `FrmAchats : FrmListeBase<Lot>`
**Fichier :** `Forms/FrmAchats.cs`

#### Chargement

```csharp
protected override List<Lot> ChargerDonnees()
    => LotDAL.GetAll(_activite?.Id ?? 0);
```

#### Colonnes visibles dans le DGV

| Propriete            | Header affiche   | Largeur | Min |
|---------------------|------------------|---------|-----|
| `NomIngredient`     | Ingredient       | 180 px  | 120 |
| `NumeroLot`         | N deg. lot        | 90 px   | 70  |
| `NomFournisseur`    | Fournisseur      | 140 px  | 90  |
| `DateAchat`         | Date achat       | 95 px   | 80  |
| `DatePeremption`    | Peremption       | 95 px   | 80  |
| `QuantiteInitiale`  | Qte achetee      | 110 px  | 80  |
| `PrixUnitaire`      | Prix unit. HTVA  | 100 px  | 80  |
| `PrixAchatReel`     | Total HTVA       | 95 px   | 75  |
| `ReferenceFacture`  | Ref. facture     | 100 px  | 80  |

**Colonnes cachees :** `Id`, `IdFicheIngredient`, `IdFournisseur`, `QuantiteDisponible`, `Notes`, `TvaPct`, `UniteMesure`, `ConditionnementLabel`, `QteParConditionnement`, `NbConditionnements`, `PrixUnitaireBase`.

#### Formatage des cellules (CellFormatting)

```csharp
NomIngredient    -> "{NomIngredient} {QteParConditionnement formatee} {unite}"
QuantiteInitiale -> FormatQte(quantite, unite)  -- ex: "5.000 kg"
PrixUnitaire     -> FormatPrix(prix)            -- ex: "12,50 EUR"
PrixAchatReel    -> FormatPrix(prix)
```

Le nom de l'ingredient dans la colonne inclut le conditionnement (ex: "Farine 10000 g").

#### Boutons CRUD (herites de FrmListeBase)

| Bouton       | Action                                          |
|-------------|--------------------------------------------------|
| + Ajouter   | `new FrmAchatEdit(null, _activite?.Id ?? 0)`     |
| Modifier    | `new FrmAchatEdit(element, _activite?.Id ?? 0)`  |
| Supprimer   | Confirmation YesNo puis `LotDAL.Delete(id)`      |
| Fermer      | Ferme le formulaire                               |

Label de suppression : `"{NomIngredient} du {DateAchat:dd/MM/yyyy}"`.

---

### 6.3 FrmAchatEdit -- Formulaire de creation/modification

**Classe :** `FrmAchatEdit : FrmEditBase`
**Fichier :** `Forms/FrmAchatEdit.cs`

#### Tous les champs du formulaire

| # | Label affiche                        | Controle           | Type             | Obligatoire |
|---|-------------------------------------|--------------------|------------------|-------------|
| 1 | **Ingredient \***                   | `cmbIngredient` (creation) / `lblIngredientRO` (edition) | ComboBox DDL / Label RO | OUI (creation) |
| 2 | Fournisseur                         | `cmbFournisseur`   | ComboBox DDL     | non         |
| 3 | N deg. lot (facultatif)              | `txtNumeroLot`     | TextBox          | non         |
| 4 | **Nombre de conditionnements \***   | `nudQuantite`      | NumericUpDown    | OUI (>0)    |
| 5 | _(info conditionnement)_            | `lblUnite`         | Label (calcule)  | --          |
| 6 | HTVA / TVAC                         | `rbHtva`, `rbTvac` | RadioButton      | defaut HTVA |
| 7 | **Prix saisi (EUR/unite) \***          | `nudPrix`          | NumericUpDown    | OUI (>0)    |
| 8 | TVA %                               | `nudTvaPct`        | NumericUpDown    | non (defaut 0) |
| 9 | _(HTVA calcule)_                    | `lblPrixHtva`      | Label (calcule)  | --          |
| 10| _(TVAC calcule)_                    | `lblPrixTvac`      | Label (calcule)  | --          |
| 11| _(Total HTVA)_                      | `lblTotalHtva`     | Label (calcule)  | --          |
| 12| _(Total TVAC)_                      | `lblTotalTvac`     | Label (calcule)  | --          |
| 13| Ref. facture                        | `txtRefFacture`    | TextBox          | non         |
| 14| **Date achat \***                   | `dtpDateAchat`     | DateTimePicker   | OUI (defaut aujourd'hui) |
| 15| Date de peremption                  | `chkPeremption` + `dtpPeremption` | CheckBox + DTP | non (desactive par defaut) |
| 16| Notes                               | `txtNotes`         | TextBox multiline| non         |

#### Comportement du champ Ingredient

- **Mode creation** : ComboBox alimentee par `IngredientDAL.GetAll(idActivite: _idActivite)`. Quand l'utilisateur selectionne un ingredient, le formulaire :
  - Met a jour `_ingredientSelectionne`
  - Affiche le label conditionnement (`lblUnite`)
  - Pre-remplit `nudPrix` avec `PrixAchatReference` de l'ingredient si > 0
- **Mode edition** : Label en lecture seule affichant `_lot.NomIngredient`. L'ingredient ne peut pas etre change.

#### Calcul dynamique du prix (methode `MajPrix()`)

Appele a chaque changement de `nudQuantite`, `nudPrix`, `nudTvaPct`, ou du mode HTVA/TVAC.

```
facteur = 1 + tvaPct / 100

Si HTVA selectionne :
    prixHtva = prixSaisi
    prixTvac = prixSaisi * facteur

Si TVAC selectionne :
    prixTvac = prixSaisi
    prixHtva = prixSaisi / facteur
```

Affichage temps reel :
```
lblPrixHtva  = "HTVA : {prixHtva:F4} EUR/{conditionnementLabel}"
lblPrixTvac  = "TVAC : {prixTvac:F4} EUR/{conditionnementLabel}"
lblTotalHtva = "Total HTVA : {nbCond * prixHtva:F2} EUR"
lblTotalTvac = "Total TVAC : {nbCond * prixTvac:F2} EUR"
```

#### Label info conditionnement (methode `MajInfoConditionnement()`)

Affiche sous le champ quantite :
```
"x {QteParConditionnement} {unite} = {total} {unite} en stock"
```
Exemple : `"x 10000 g = 50000 g en stock"` pour 5 conditionnements de 10 kg.

#### Regles de validation (methode `Valider()`)

```csharp
1. cmbIngredient == null (creation)  -> "Choisissez un ingredient."
2. nudQuantite <= 0                  -> "Quantite invalide."
3. nudPrix <= 0                      -> "Prix invalide."
```

#### Sauvegarde (methode `Sauvegarder()`)

Calculs avant persistance :

```csharp
// 1. Conversion TVAC -> HTVA si necessaire
decimal prixHtva = rbHtva.Checked
    ? nudPrix.Value
    : nudPrix.Value / (1 + nudTvaPct.Value / 100);

// 2. Quantite initiale = nb conditionnements x qte par conditionnement
decimal nbCond    = nudQuantite.Value;
decimal qteParCond = ing.QteParConditionnement;  // depuis l'ingredient
_lot.QuantiteInitiale = Math.Round(nbCond * qteParCond, 4);

// 3. Prix total = nb conditionnements x prix HTVA unitaire
_lot.PrixAchatReel = Math.Round(nbCond * prixHtva, 4);

// 4. Stocke le prix HTVA par conditionnement
_lot.PrixUnitaire = Math.Round(prixHtva, 4);
```

Puis :
- Mode creation : `LotDAL.Insert(_lot)`.
- Mode edition : `LotDAL.Update(_lot)`.

---

### 6.4 LotDAL -- Couche d'acces aux donnees

**Fichier :** `DAL/LotDAL.cs`

#### Insert -- SQL exact

```sql
INSERT INTO lots_ingredients
    (id_fiche_ingredient, nb_conditionnements,
     numero_lot, id_fournisseur, date_achat,
     date_peremption, quantite_initiale, quantite_disponible,
     prix_unitaire, prix_achat_reel, tva_pct, reference_facture, notes)
VALUES (@idFi, @nbCond,
        @numeroLot, @idFourn, @dateAchat, @datePer,
        @qteInit, @qteInit, @prixUnit, @prixTotal, @tvaPct, @refFact, @notes)
```

**Point cle : `quantite_disponible = @qteInit`** -- a l'insertion, la quantite disponible est egale a la quantite initiale. Elle diminuera ensuite au fur et a mesure de la consommation par les productions.

#### Update -- SQL exact avec conservation de la consommation

```sql
UPDATE lots_ingredients SET
    id_fiche_ingredient = @idFi,
    nb_conditionnements = @nbCond,
    numero_lot          = @numeroLot,
    id_fournisseur      = @idFourn,
    date_achat          = @dateAchat,
    date_peremption     = @datePer,
    quantite_initiale   = @qteInit,
    quantite_disponible = GREATEST(0, @qteInit - (quantite_initiale - quantite_disponible)),
    prix_unitaire       = @prixUnit,
    prix_achat_reel     = @prixTotal,
    tva_pct             = @tvaPct,
    reference_facture   = @refFact,
    notes               = @notes
WHERE id = @id
```

**Formule cle du Update :**
```
consomme            = ancienne_initiale - ancienne_disponible
nouvelle_disponible = GREATEST(0, nouvelle_initiale - consomme)
```

Cela permet de modifier la quantite achetee sans perdre le suivi de consommation. Si la nouvelle quantite initiale est inferieure a ce qui a deja ete consomme, `GREATEST(0, ...)` empeche une quantite disponible negative.

#### GetAll -- SQL avec jointures

```sql
SELECT l.id, l.id_fiche_ingredient, fi.nom AS nom_ingredient,
       fi.unite_mesure, fi.conditionnement_label, fi.qte_par_conditionnement,
       l.nb_conditionnements,
       l.numero_lot, l.id_fournisseur, f.nom AS nom_fournisseur,
       l.date_achat, l.date_peremption, l.quantite_initiale,
       l.quantite_disponible, l.prix_unitaire, l.prix_achat_reel,
       l.tva_pct, l.reference_facture, l.notes
FROM lots_ingredients l
INNER JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
LEFT JOIN fournisseurs f ON f.id = l.id_fournisseur
[WHERE fi.id_stock IN (SELECT id_stock FROM activites_stocks
                       WHERE id_activite = @idActivite)]
ORDER BY l.date_achat DESC
```

#### GetByFicheIngredient -- Lots d'un ingredient

```sql
WHERE l.id_fiche_ingredient = @idFiche
ORDER BY l.date_peremption ASC, l.date_achat DESC
```

Tri par date de peremption ascendante = **FIFO** (First Expired, First Out) pour la consommation en production.

---

### 6.5 Modele Lot -- Proprietes calculees

**Fichier :** `Models/Lot.cs`

#### Proprietes stockees en DB

| Propriete              | Type       | Description                                         |
|-----------------------|------------|-----------------------------------------------------|
| `Id`                  | int        | PK auto-increment                                   |
| `IdFicheIngredient`   | int        | FK vers fiches_ingredients                           |
| `NbConditionnements`  | decimal    | Nombre de colis achetes (ex: 5 sacs)                |
| `NumeroLot`           | string?    | Numero de lot fabricant (tracabilite)                |
| `IdFournisseur`       | int?       | FK vers fournisseurs (nullable)                     |
| `DateAchat`           | DateTime   | Date de l'achat                                     |
| `DatePeremption`      | DateTime?  | Date de peremption (nullable)                       |
| `QuantiteInitiale`    | decimal    | Qte totale en unite de base = NbCond x QteParCond   |
| `QuantiteDisponible`  | decimal    | Qte restante (diminue avec les productions)         |
| `PrixUnitaire`        | decimal    | Prix HTVA par conditionnement                       |
| `PrixAchatReel`       | decimal    | Total HTVA = NbCond x PrixUnitaire                  |
| `TvaPct`              | decimal    | Taux TVA en % (0 = exonere)                         |
| `ReferenceFacture`    | string?    | Reference de la facture                              |
| `Notes`               | string?    | Notes libres                                        |

#### Proprietes par jointure (remplies par le DAL)

| Propriete              | Source                                |
|-----------------------|---------------------------------------|
| `NomIngredient`       | `fiches_ingredients.nom`              |
| `UniteMesure`         | `fiches_ingredients.unite_mesure`     |
| `ConditionnementLabel`| `fiches_ingredients.conditionnement_label` |
| `QteParConditionnement`| `fiches_ingredients.qte_par_conditionnement` |
| `NomFournisseur`      | `fournisseurs.nom`                    |

#### Propriete calculee

```csharp
// Prix par unite de base (EUR/g, EUR/ml, EUR/pce)
PrixUnitaireBase = PrixUnitaire / QteParConditionnement
```

Ce prix par unite de base est utilise pour le calcul des couts de revient dans les fiches de production BOM.

---

## Resume du flux de donnees

```
Ingredient (fiche)                     Lot (achat)
+---------------------------+          +-------------------------------+
| nom, unite, conditionnement|          | nb_conditionnements           |
| qte_par_conditionnement   |---1:N--->| quantite_initiale (calcule)   |
| prix_achat_reference      |          | quantite_disponible           |
| seuil_alerte_stock        |          | prix_unitaire (HTVA/cond.)    |
| stock_cible               |          | prix_achat_reel (total HTVA)  |
+---------------------------+          +-------------------------------+

Formules cles :
  quantite_initiale   = nb_conditionnements x qte_par_conditionnement
  prix_achat_reel     = nb_conditionnements x prix_unitaire
  prix_unitaire_base  = prix_unitaire / qte_par_conditionnement
  stock_actuel        = SUM(lots.quantite_disponible)  -- SQL agrege
  stock_ratio         = stock_actuel / stock_cible
  est_en_alerte       = stock_actuel <= seuil_alerte_stock
```

---

## Heritage et architecture des formulaires

```
Form (WinForms)
  |
  +-- FrmListeBase<T>          -- DGV + boutons CRUD generiques
  |     |-- FrmIngredients      -- liste ingredients + chip filter + alerte
  |     |-- FrmAchats           -- liste achats + formatage prix/qte
  |
  +-- FrmEditBase              -- ErrorProvider + Enregistrer/Annuler
        |-- FrmIngredientEdit   -- 11 champs, densite conditionnelle
        |-- FrmAchatEdit        -- prix HTVA/TVAC, calcul temps reel
```
