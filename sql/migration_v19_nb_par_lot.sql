-- ============================================================
-- Migration v19 — Ajout nb_par_lot sur fiches_ingredients
-- Permet de persister le nombre de conditionnements par lot
-- d'achat (ex : lot de 6 briques, lot de 4 sacs).
-- Défaut = 1 (lot = 1 conditionnement).
-- ============================================================

USE charlesnadejda;

ALTER TABLE fiches_ingredients
    ADD COLUMN nb_par_lot INT NOT NULL DEFAULT 1 AFTER qte_par_conditionnement;

-- ============================================================
-- FIN migration v19
-- Table modifiée : fiches_ingredients (ADD nb_par_lot)
-- ============================================================
