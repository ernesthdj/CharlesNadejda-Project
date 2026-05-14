-- Migration v14 : Ajout du champ stock_cible sur fiches_ingredients
-- Stock cible = capacité nominale définie par l'utilisateur (100% de la jauge)
-- Nullable : si non défini, pas de jauge affichée

ALTER TABLE fiches_ingredients
    ADD COLUMN stock_cible DECIMAL(10,4) DEFAULT NULL
    AFTER seuil_alerte_stock;
