-- migration_v16_bom_stock_cible.sql
-- Ajoute stock_cible aux fiches BOM (produits semi-finis N2+)
-- Pattern identique à migration_v14_stock_cible pour les ingrédients

ALTER TABLE bom_fiches
    ADD COLUMN stock_cible DECIMAL(10,4) DEFAULT NULL
    AFTER temps_preparation;
