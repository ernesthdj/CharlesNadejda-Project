# MODULE: Stock & Achats
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/DAL/StockDAL.cs`
> Type: rationale

## Description
Deux types de stock: lots_ingredients (achats matieres) + bom_stocks (produits par production). Vue unifiee via VIEW MySQL. FIFO sur les deux. Reservations soft-delete. Liaison M:N stock<->activite. Jauge stock_cible avec ratio couleur. 3 sections DGV (Ingredients/Intermediaires/Finals).

## Relations
### Connexions sortantes
- --documents--> [[StockDAL|StockDAL]]
- --documents--> [[VueStockGlobalDAL|VueStockGlobalDAL]]
- --documents--> [[BomReservationDAL|BomReservationDAL]]
