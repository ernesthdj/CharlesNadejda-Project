-- Migration v6 : TVA sur les lots d'achats
-- Ajoute tva_pct (pourcentage de TVA) à lots_ingredients
-- prix_unitaire reste en HTVA (hors taxe) — cohérence avec le BOM

ALTER TABLE lots_ingredients
    ADD COLUMN tva_pct DECIMAL(5,2) NOT NULL DEFAULT 0
    COMMENT 'Taux de TVA en % appliqué à cet achat (0 = exonéré). Prix stocké toujours en HTVA.';
