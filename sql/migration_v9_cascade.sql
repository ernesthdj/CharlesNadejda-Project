-- ============================================================
-- Migration v9 — Suppressions en cascade + ENUM unités complet
--
-- 1. FK RESTRICT → CASCADE pour permettre les suppressions depuis l'ERP
-- 2. ENUM unite_mesure étendu à mg / g / kg / ml / cl / dl / l / piece
--
-- NOTE MySQL : DROP FOREIGN KEY + ADD CONSTRAINT avec le même nom
-- dans un seul ALTER TABLE génère "Duplicate FK name".
-- Solution : deux ALTER TABLE séparés par contrainte.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. bom_niveaux → bom_contextes  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE bom_niveaux DROP FOREIGN KEY fk_bn_contexte;
ALTER TABLE bom_niveaux
    ADD CONSTRAINT fk_bn_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 2. recettes_ingredients → fiches_ingredients  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE recettes_ingredients DROP FOREIGN KEY fk_ri_ingredient;
ALTER TABLE recettes_ingredients
    ADD CONSTRAINT fk_ri_ingredient
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 3. bom_fiches_lignes → fiches_ingredients  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE bom_fiches_lignes DROP FOREIGN KEY fk_bfl_ingredient;
ALTER TABLE bom_fiches_lignes
    ADD CONSTRAINT fk_bfl_ingredient
        FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 4. bom_fiches_lignes → bom_fiches (input sous-fiche)  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE bom_fiches_lignes DROP FOREIGN KEY fk_bfl_fiche_input;
ALTER TABLE bom_fiches_lignes
    ADD CONSTRAINT fk_bfl_fiche_input
        FOREIGN KEY (id_input_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 5. lots_ingredients → fiches_ingredients  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE lots_ingredients DROP FOREIGN KEY fk_lot_fiche;
ALTER TABLE lots_ingredients
    ADD CONSTRAINT fk_lot_fiche
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 6. bom_reservations → lots_ingredients  (RESTRICT → CASCADE)
-- ============================================================
ALTER TABLE bom_reservations DROP FOREIGN KEY fk_br_lot;
ALTER TABLE bom_reservations
    ADD CONSTRAINT fk_br_lot
        FOREIGN KEY (id_lot) REFERENCES lots_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

-- ============================================================
-- 7. ENUM unite_mesure — fiches_ingredients  (ajout mg + dl)
-- ============================================================
ALTER TABLE fiches_ingredients
    MODIFY COLUMN unite_mesure
        ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL;

-- ============================================================
-- 8. ENUM unite_mesure — bom_fiches_lignes  (ajout mg + dl)
-- ============================================================
ALTER TABLE bom_fiches_lignes
    MODIFY COLUMN unite_mesure
        ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL;

-- ============================================================
-- 9. ENUM unite_output — bom_fiches  (ajout mg + dl)
-- ============================================================
ALTER TABLE bom_fiches
    MODIFY COLUMN unite_output
        ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL DEFAULT 'piece';

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v9
--
-- FK RESTRICT → CASCADE :
--   bom_niveaux.fk_bn_contexte
--   recettes_ingredients.fk_ri_ingredient
--   bom_fiches_lignes.fk_bfl_ingredient
--   bom_fiches_lignes.fk_bfl_fiche_input
--   lots_ingredients.fk_lot_fiche
--   bom_reservations.fk_br_lot
--
-- FK gardées RESTRICT (historique) :
--   bom_productions.fk_bp_niveau / fk_bp_fiche
--   bom_stocks.fk_bs_*
--   bom_productions_lignes.fk_bpl_*
--   commandes.fk_commande_client
--   factures.fk_facture_commande
--
-- ENUM étendu (mg + dl) :
--   fiches_ingredients.unite_mesure
--   bom_fiches_lignes.unite_mesure
--   bom_fiches.unite_output
-- ============================================================
