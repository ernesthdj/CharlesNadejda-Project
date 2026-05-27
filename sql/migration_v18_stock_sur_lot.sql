-- ============================================================
-- Migration v18 — Déplacer id_stock de fiches_ingredients vers lots_ingredients
--
-- Objectif : le stock physique est assigné au moment de l'achat (lot),
--            pas à la fiche ingrédient. Un même ingrédient peut exister
--            dans plusieurs stocks (frigo + armoire).
-- ============================================================

USE charlesnadejda;

-- 1. Ajouter id_stock nullable sur lots_ingredients
ALTER TABLE lots_ingredients ADD COLUMN id_stock INT NULL AFTER id_fiche_ingredient;

-- 2. Migrer les données existantes depuis fiches_ingredients
UPDATE lots_ingredients l
JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
SET l.id_stock = fi.id_stock;

-- 3. Rendre NOT NULL + FK
ALTER TABLE lots_ingredients
    MODIFY COLUMN id_stock INT NOT NULL,
    ADD CONSTRAINT fk_lots_stock FOREIGN KEY (id_stock) REFERENCES stocks(id);

-- 4. Supprimer la FK + colonne de fiches_ingredients
ALTER TABLE fiches_ingredients DROP FOREIGN KEY fk_fi_stock, DROP COLUMN id_stock;

-- 5. Recréer la VIEW vue_stock_global (join via li.id_stock au lieu de fi.id_stock)
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
        li.id_stock             AS id_stock,
        s.nom                   AS stock_nom,
        NULL                    AS id_activite,
        NULL                    AS id_contexte,
        NULL                    AS id_niveau,
        NULL                    AS id_fiche_bom,
        fi.id                   AS id_fiche_ingredient
    FROM lots_ingredients li
    JOIN fiches_ingredients fi  ON fi.id  = li.id_fiche_ingredient
    JOIN stocks s               ON s.id   = li.id_stock
    LEFT JOIN bom_reservations br
        ON  br.id_lot = li.id
        AND br.actif  = 1
    GROUP BY
        li.id, fi.id, fi.nom, fi.unite_mesure,
        li.quantite_disponible, li.prix_unitaire, fi.qte_par_conditionnement,
        fi.conditionnement_label,
        li.date_peremption, li.id_stock, s.nom

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
        bs.id_fiche             AS id_fiche_bom,
        NULL                    AS id_fiche_ingredient
    FROM bom_stocks bs
    JOIN bom_fiches bf ON bf.id = bs.id_fiche;

-- ============================================================
-- FIN migration v18
--
-- Tables modifiées : lots_ingredients (ADD id_stock + FK)
--                    fiches_ingredients (DROP id_stock)
-- Vues modifiées   : vue_stock_global (JOIN via li.id_stock)
-- ============================================================
