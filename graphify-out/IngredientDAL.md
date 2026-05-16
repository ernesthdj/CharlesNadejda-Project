# IngredientDAL
> Source: `CharlesNadejda/DAL/IngredientDAL.cs`
> Type: code

## Description
CRUD des ingredients avec agregat SUM stock actuel depuis lots_ingredients. Proprietes calculees: PrixParUniteBase, StockRatio (actuel/cible), StockPieces (conditionnements entiers).

## Methodes
- `GetAll(idActivite) -> List<Ingredient>`
- `GetById(id) -> Ingredient`
- `Insert(ingr) -> int`
- `Update(ingr)`
- `Delete(id)`
- `Bind(ingr,cmd)`
- `Map(reader) -> Ingredient`

## Regles (JOURNAL.md)
- #5 ajout champ = 4 endroits (SELECT+INSERT+UPDATE+Map)
- #27 migration SQL AVANT lancement app
- #28 conversion pieces<->unite de base pour seuils

## Relations
Appele par: FrmIngredients, FrmIngredientEdit, FrmAchatEdit, BomFicheEdit (liste inputs)

### Connexions sortantes
- --method--> .GetAll()
- --method--> .NomExiste()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- IngredientDAL.cs --contains-->
- [[MODULE_Catalogue_(Ingredients_&_Fournisseurs)|MODULE: Catalogue (Ingredients & Fournisseurs)]] --documents-->
