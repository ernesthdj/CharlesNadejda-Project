-- ============================================================
-- Migration v11 — Discriminants directs sur bom_stocks
--                + VIEW vue_stock_global
--
-- Objectif : filtrer le stock fabriqué par activité/contexte
--            sans JOIN en cascade.
-- Décision : Option B + VIEW (brainstorm 2026-04-15)
-- Architecture : bom_stocks.id_contexte + id_activite
--   dénormalisés depuis bom_niveaux → bom_contextes → activites
--   pour accès direct O(1) côté C#.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. Ajout id_contexte + id_activite sur bom_stocks
-- ============================================================
ALTER TABLE bom_stocks
    ADD COLUMN id_contexte INT NOT NULL AFTER id_niveau,
    ADD COLUMN id_activite INT NOT NULL AFTER id_contexte;

ALTER TABLE bom_stocks
    ADD CONSTRAINT fk_bs_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    ADD CONSTRAINT fk_bs_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- 2. VIEW vue_stock_global
--    Unifie lots_ingredients (matières premières achetées)
--    et bom_stocks (produits fabriqués) pour consultation.
--
--    Règles :
--    - lots   : une ligne par lot, quantite_dispo_reelle = total - réservations actives
--    - produits : une ligne par lot de production, id_activite/contexte/niveau directs
--    - Lecture seule — modifications via LotDAL / BomStockDAL uniquement
-- ============================================================
CREATE OR REPLACE VIEW vue_stock_global AS

    -- Matières premières (lots achetés)
    SELECT
        'lot_ingredient'        AS type_stock,
        li.id                   AS id_entree,
        fi.nom                  AS nom,
        fi.unite_mesure         AS unite,
        li.quantite_disponible  AS quantite_totale,
        COALESCE(
            SUM(br.quantite_reservee), 0
        )                       AS quantite_reservee,
        li.quantite_disponible
            - COALESCE(SUM(br.quantite_reservee), 0)
                                AS quantite_dispo_reelle,
        li.prix_unitaire / NULLIF(fi.qte_par_conditionnement, 0)
                                AS cout_unitaire,
        li.prix_unitaire        AS prix_conditionnement,
        fi.qte_par_conditionnement AS qte_par_conditionnement,
        fi.conditionnement_label AS conditionnement_label,
        li.date_peremption      AS date_dlc,
        fi.id_stock             AS id_stock,
        s.nom                   AS stock_nom,
        NULL                    AS id_activite,
        NULL                    AS id_contexte,
        NULL                    AS id_niveau,
        NULL                    AS id_fiche_bom
    FROM lots_ingredients li
    JOIN fiches_ingredients fi  ON fi.id  = li.id_fiche_ingredient
    JOIN stocks s               ON s.id   = fi.id_stock
    LEFT JOIN bom_reservations br
        ON  br.id_lot = li.id
        AND br.actif  = 1
    GROUP BY
        li.id, fi.nom, fi.unite_mesure,
        li.quantite_disponible, li.prix_unitaire, fi.qte_par_conditionnement,
        li.date_peremption, fi.id_stock, s.nom

UNION ALL

    -- Produits fabriqués (output BOM)
    SELECT
        'produit_fabrique'      AS type_stock,
        bs.id                   AS id_entree,
        bf.nom                  AS nom,
        bf.unite_output         AS unite,
        bs.quantite_disponible  AS quantite_totale,
        0                       AS quantite_reservee,
        bs.quantite_disponible  AS quantite_dispo_reelle,
        bs.cout_unitaire,
        NULL                    AS prix_conditionnement,
        NULL                    AS qte_par_conditionnement,
        NULL                    AS conditionnement_label,
        bs.date_dlc,
        NULL                    AS id_stock,
        NULL                    AS stock_nom,
        bs.id_activite,
        bs.id_contexte,
        bs.id_niveau,
        bs.id_fiche             AS id_fiche_bom
    FROM bom_stocks bs
    JOIN bom_fiches bf ON bf.id = bs.id_fiche;

-- ============================================================
-- FIN migration v11
--
-- Tables modifiées : bom_stocks (ADD id_contexte, ADD id_activite)
-- FKs ajoutées     : fk_bs_contexte, fk_bs_activite
-- Vues créées      : vue_stock_global
-- ============================================================
