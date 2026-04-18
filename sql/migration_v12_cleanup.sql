-- ============================================================
-- Migration v12 — Nettoyage DB
--
-- Objectif : supprimer toutes les tables orphelines ou
--            appartenant à des modules non encore démarrés.
-- Tables conservées : les 15 tables du workflow ERP actuel.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ── Module pâtisserie (non démarré) ──────────────────────────
DROP TABLE IF EXISTS pat_recettes_couche_ingr;
DROP TABLE IF EXISTS pat_recettes_couche;
DROP TABLE IF EXISTS pat_fiches_gateau_couches;
DROP TABLE IF EXISTS pat_etages_couches;
DROP TABLE IF EXISTS pat_ingredients_allergenes;
DROP TABLE IF EXISTS pat_gabarits;
DROP TABLE IF EXISTS pat_options_deco;
DROP TABLE IF EXISTS pat_fiches_gateau;
DROP TABLE IF EXISTS pat_etages;
DROP TABLE IF EXISTS pat_commandes;
DROP TABLE IF EXISTS pat_devis;
DROP TABLE IF EXISTS pat_couches_dispo_config;
DROP TABLE IF EXISTS pat_types_couche;
DROP TABLE IF EXISTS pat_formes;
DROP TABLE IF EXISTS pat_allergenes;
DROP TABLE IF EXISTS pat_parametres;

-- ── Ancien système recettes (orphelines) ─────────────────────
DROP TABLE IF EXISTS productions_compositions;
DROP TABLE IF EXISTS productions_recettes;
DROP TABLE IF EXISTS compositions_recettes;
DROP TABLE IF EXISTS recettes_ingredients;
DROP TABLE IF EXISTS fiches_compositions;
DROP TABLE IF EXISTS fiches_recettes;

-- ── Mouvements stock (non travaillé) ─────────────────────────
DROP TABLE IF EXISTS mouvements_stock_produits;
DROP TABLE IF EXISTS mouvements_lots_ingredients;
DROP TABLE IF EXISTS stock_compositions;
DROP TABLE IF EXISTS stock_produits;

-- ── Modules non démarrés ──────────────────────────────────────
DROP TABLE IF EXISTS lignes_commandes;
DROP TABLE IF EXISTS commandes;
DROP TABLE IF EXISTS factures;
DROP TABLE IF EXISTS contacts;
DROP TABLE IF EXISTS produits_parfums;
DROP TABLE IF EXISTS selections_parfums;
DROP TABLE IF EXISTS produits;
DROP TABLE IF EXISTS parfums;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS zones_livraison;

-- ── Coûts / config (non travaillé) ───────────────────────────
DROP TABLE IF EXISTS frais_production_variables;
DROP TABLE IF EXISTS frais_generaux_config;

-- ── Infrastructure Laravel (non utilisée côté C#) ────────────
DROP TABLE IF EXISTS failed_jobs;
DROP TABLE IF EXISTS job_batches;
DROP TABLE IF EXISTS jobs;
DROP TABLE IF EXISTS cache_locks;
DROP TABLE IF EXISTS cache;
DROP TABLE IF EXISTS sessions;
DROP TABLE IF EXISTS password_reset_tokens;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS migrations;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- Tables conservées (15) :
--   utilisateurs, fournisseurs,
--   fiches_ingredients, lots_ingredients,
--   stocks, activites, activites_stocks,
--   bom_contextes, bom_niveaux,
--   bom_fiches, bom_fiches_lignes,
--   bom_stocks, bom_productions, bom_productions_lignes,
--   bom_reservations
-- ============================================================
