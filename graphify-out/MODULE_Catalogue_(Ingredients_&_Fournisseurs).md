# MODULE: Catalogue (Ingredients & Fournisseurs)
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/DAL/IngredientDAL.cs`
> Type: rationale

## Description
Ingredients avec: unite de base (g/ml/piece), conditionnement, stock_cible, seuil_alerte, fournisseur FK. Proprietes calculees: StockPieces, StockRatio, PrixParUniteBase. Fournisseurs CRUD simple. Filtrage par chips (type, fournisseur). Affichage conditionnel densite/conditionnement.

## Relations
### Connexions sortantes
- --documents--> [[IngredientDAL|IngredientDAL]]
- --documents--> [[FournisseurDAL|FournisseurDAL]]
