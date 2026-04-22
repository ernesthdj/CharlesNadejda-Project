-- ============================================================
-- RESET DB POUR TESTS MANUELS — ArtisaStock
-- Date : 2026-04-21
-- ⚠️  DESTRUCTIF — supprime TOUTES les données applicatives.
--     Conserver la structure (tables, vues, procédures).
-- Exécuter dans MySQL Workbench ou via CLI :
--   mysql -u root -p charlesnadejda < reset_db_for_tests.sql
-- ============================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ── BOM : traçabilité de production (enfants en premier) ────
TRUNCATE TABLE bom_productions_lignes;
TRUNCATE TABLE bom_productions;
TRUNCATE TABLE bom_stocks;
TRUNCATE TABLE bom_reservations;

-- ── BOM : recettes & niveaux ─────────────────────────────────
TRUNCATE TABLE bom_fiches_lignes;
TRUNCATE TABLE bom_fiches;
TRUNCATE TABLE bom_niveaux;
TRUNCATE TABLE bom_contextes;

-- ── Ingrédients & achats ─────────────────────────────────────
TRUNCATE TABLE mouvements_lots_ingredients;
TRUNCATE TABLE lots_ingredients;
TRUNCATE TABLE fiches_ingredients;

-- ── Stocks & activités ───────────────────────────────────────
TRUNCATE TABLE activites_stocks;
TRUNCATE TABLE stocks;
TRUNCATE TABLE fournisseurs;
TRUNCATE TABLE activites;

-- ── Catalogue produits ───────────────────────────────────────
TRUNCATE TABLE produits_parfums;
TRUNCATE TABLE produits;
TRUNCATE TABLE parfums;
TRUNCATE TABLE categories;

-- ── Module Pâtisserie ────────────────────────────────────────
TRUNCATE TABLE pat_etages_couches;
TRUNCATE TABLE pat_etages;
TRUNCATE TABLE pat_commandes;
TRUNCATE TABLE pat_devis;
TRUNCATE TABLE pat_fiches_gateau_couches;
TRUNCATE TABLE pat_fiches_gateau;
TRUNCATE TABLE pat_couches_dispo_config;
TRUNCATE TABLE pat_recettes_couche_ingr;
TRUNCATE TABLE pat_recettes_couche;
TRUNCATE TABLE pat_ingredients_allergenes;
TRUNCATE TABLE pat_options_deco;
TRUNCATE TABLE pat_allergenes;
TRUNCATE TABLE pat_types_couche;
TRUNCATE TABLE pat_gabarits;
TRUNCATE TABLE pat_formes;
TRUNCATE TABLE pat_parametres;

-- ── Commandes & facturation ──────────────────────────────────
TRUNCATE TABLE selections_parfums;
TRUNCATE TABLE lignes_commandes;
TRUNCATE TABLE factures;
TRUNCATE TABLE commandes;
TRUNCATE TABLE zones_livraison;
TRUNCATE TABLE contacts;

-- ── Productions & stocks produits ────────────────────────────
TRUNCATE TABLE mouvements_stock_produits;
TRUNCATE TABLE stock_produits;
TRUNCATE TABLE productions_compositions;
TRUNCATE TABLE stock_compositions;
TRUNCATE TABLE frais_production_variables;
TRUNCATE TABLE productions_recettes;
TRUNCATE TABLE compositions_recettes;
TRUNCATE TABLE fiches_compositions;
TRUNCATE TABLE recettes_ingredients;
TRUNCATE TABLE fiches_recettes;

-- ── Config ───────────────────────────────────────────────────
TRUNCATE TABLE frais_generaux_config;

-- ── Utilisateurs — NE PAS supprimer (garder les comptes) ────
-- TRUNCATE TABLE utilisateurs;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'DB vidée avec succès. Prête pour les tests manuels.' AS statut;
