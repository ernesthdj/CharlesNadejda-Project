-- ============================================================
-- Migration v20 — Durcissement schema (index, FK explicite)
-- Date : 2026-06-11
-- ============================================================

USE charlesnadejda;

-- Index composite FIFO pour lots_ingredients (optimise BomStockDAL.GetLotsDispoFIFO)
CREATE INDEX idx_lot_fiche_achat ON lots_ingredients (id_fiche_ingredient, date_achat);

-- Index pour filtres frequents sur bom_reservations
CREATE INDEX idx_bomres_lot_actif ON bom_reservations (id_lot, actif);
CREATE INDEX idx_bomres_ctx_actif ON bom_reservations (id_contexte, actif);

-- Index composite pour le scope withStockDisponible (ProduitWeb Laravel)
CREATE INDEX idx_bomstock_fiche_dispo ON bom_stocks (id_fiche, quantite_disponible, date_production);

-- FK lots_ingredients.id_stock : ajouter ON DELETE RESTRICT ON UPDATE CASCADE explicite
-- (actuellement sans clause ON DELETE/ON UPDATE)
ALTER TABLE lots_ingredients DROP FOREIGN KEY fk_lots_stock;
ALTER TABLE lots_ingredients ADD CONSTRAINT fk_lots_stock
    FOREIGN KEY (id_stock) REFERENCES stocks(id)
    ON DELETE RESTRICT ON UPDATE CASCADE;
