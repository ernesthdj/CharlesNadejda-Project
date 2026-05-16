# FrmIngredientEdit
> Source: `CharlesNadejda/Forms/FrmIngredientEdit.cs`
> Type: code

## Description
Editeur ingredient (herite FrmEditBase). Champs : nom, marque, unite base, type physique, fournisseur, stock lie, densite (si liquide), prix, qte conditionnement, seuil alerte, stock cible (en pieces). Affichage conditionnel densite/conditionnement selon type physique. Stock cible saisi en pieces, converti en unite de base a la sauvegarde.

## Methodes
- `FrmIngredientEdit_Load() Valider()->bool Sauvegarder()`

## Regles (JOURNAL.md)
- #28 conversion pieces<->unite de base pour stock_cible
- #27 migration SQL AVANT lancement app si nouveau champ
- #5 nouveau champ = verifier 4 endroits DAL

## Relations
Appelle: IngredientDAL.Insert/Update, FournisseurDAL.GetAll, StockDAL.GetAll. Herite: FrmEditBase.

### Connexions sortantes
- --inherits--> FrmEditBase
- --references--> bool
- --references--> Ingredient
- --references--> ComboBox
- --references--> Label
- --references--> TextBox
- --references--> NumericUpDown
- --method--> .FrmIngredientEdit_Load()
- --method--> .MettreAJourVisibiliteDensite()
- --method--> .MajLabelUniteQteCond()
- --method--> .Valider()
- --method--> .Sauvegarder()

### Connexions entrantes
- FrmIngredientEdit.cs --contains-->
