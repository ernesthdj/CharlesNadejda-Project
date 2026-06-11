-- ============================================================
-- Migration v21 — Contraintes CHECK et UNIQUE supplementaires
-- Date : 2026-06-11
-- ============================================================

USE charlesnadejda;

-- Quantites positives sur les tables de production/reservation
ALTER TABLE bom_reservations ADD CONSTRAINT chk_bomres_qte_positive CHECK (quantite_reservee > 0);
ALTER TABLE bom_productions ADD CONSTRAINT chk_bomprod_qte_positive CHECK (quantite_produite > 0);

-- Quantite output > 0 (evite division par zero dans le calcul de stock vendable)
ALTER TABLE bom_fiches ADD CONSTRAINT chk_bf_output_positive CHECK (quantite_output > 0);

-- Unicite metier : un seul fournisseur par nom
ALTER TABLE fournisseurs ADD UNIQUE KEY uk_fournisseur_nom (nom);

-- Unicite metier : un seul contexte BOM par (nom, activite)
ALTER TABLE bom_contextes ADD UNIQUE KEY uq_bomctx_nom_activite (nom, id_activite);
