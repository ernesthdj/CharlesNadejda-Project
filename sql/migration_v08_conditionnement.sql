-- ============================================================
-- Migration v8 — Système de conditionnement universel
-- Chaque ingrédient est désormais lié à son conditionnement
-- commercial (ex : Sac 10 kg, Brique 1 L, Boîte 12 pcs).
-- Le stock est TOUJOURS stocké en unité de base (g / ml / piece).
-- Les achats se font en nb × conditionnement.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. fiches_ingredients : ajout colonnes conditionnement
-- ============================================================
ALTER TABLE fiches_ingredients
    ADD COLUMN conditionnement_label    VARCHAR(100)  NOT NULL DEFAULT ''  AFTER densite,
    ADD COLUMN qte_par_conditionnement  DECIMAL(12,4) NOT NULL DEFAULT 1   AFTER conditionnement_label;

-- Normaliser unite_mesure vers les unités de base uniquement
-- Anciens enregistrements avec 'kg' → 'g', 'l'/'cl'/'dl' → 'ml'
UPDATE fiches_ingredients SET unite_mesure = 'g'  WHERE unite_mesure IN ('kg', 'mg');
UPDATE fiches_ingredients SET unite_mesure = 'ml' WHERE unite_mesure IN ('l', 'cl', 'dl');
-- 'g', 'ml', 'piece' restent inchangés

-- ============================================================
-- 2. lots_ingredients : ajout nb_conditionnements
--    quantite_initiale/disponible = désormais TOUJOURS en unité de base
--    prix_unitaire = prix par CONDITIONNEMENT (€/sac, €/brique…)
--    prix_achat_reel = nb_conditionnements × prix_unitaire
-- ============================================================
ALTER TABLE lots_ingredients
    ADD COLUMN nb_conditionnements DECIMAL(10,3) NOT NULL DEFAULT 1 AFTER id_fournisseur;

-- Purge des lots existants (données de développement orphelines
-- depuis la truncation de fiches_ingredients en v7)
TRUNCATE TABLE lots_ingredients;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v8
-- Tables modifiées : fiches_ingredients  (ADD conditionnement_label, qte_par_conditionnement)
--                    lots_ingredients     (ADD nb_conditionnements, TRUNCATE données dev)
-- Règle : unite_mesure = 'g' | 'ml' | 'piece' exclusivement
--         Stock toujours en unité de base
--         Prix/conditionnement → prix/base = prix / qte_par_conditionnement
-- ============================================================
