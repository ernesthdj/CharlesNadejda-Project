-- Migration v13 : contrainte CHECK stock positif
-- Empêche les quantités négatives en base suite à une race condition de production
-- MySQL 8 enforce les CHECK sur colonnes scalaires (pas sur FK)

ALTER TABLE lots_ingredients
  ADD CONSTRAINT chk_lot_qte_positive CHECK (quantite_disponible >= 0);

ALTER TABLE bom_stocks
  ADD CONSTRAINT chk_bomstock_qte_positive CHECK (quantite_disponible >= 0);
