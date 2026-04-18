-- ============================================================
-- Migration v7 — Activités dynamiques (ENUM → table FK)
-- Remplace les valeurs ENUM codées en dur ('chocolaterie',
-- 'patisserie', 'partage') par des FK vers une table `activites`
-- gérée par l'utilisateur — architecture ERP générique.
--
-- IMPORTANT : purge préalable des données de test.
--             La DB ne contient que des données de développement.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. Nouvelle table activites
-- ============================================================
CREATE TABLE IF NOT EXISTS activites (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(100) NOT NULL UNIQUE,
    description   TEXT,
    actif         TINYINT(1)  NOT NULL DEFAULT 1,
    date_creation DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 2. Purge des données de test (ordre inverse des FK)
-- ============================================================
TRUNCATE TABLE bom_reservations;
TRUNCATE TABLE bom_productions_lignes;
TRUNCATE TABLE bom_stocks;
TRUNCATE TABLE bom_productions;
TRUNCATE TABLE bom_fiches_lignes;
TRUNCATE TABLE bom_fiches;
TRUNCATE TABLE bom_niveaux;
TRUNCATE TABLE bom_contextes;
TRUNCATE TABLE lots_ingredients;
TRUNCATE TABLE fiches_ingredients;

-- ============================================================
-- 3. fiches_ingredients : ENUM activite → id_activite FK
-- ============================================================
ALTER TABLE fiches_ingredients
    DROP COLUMN IF EXISTS activite,
    ADD COLUMN id_activite INT NOT NULL AFTER densite;

ALTER TABLE fiches_ingredients
    ADD CONSTRAINT fk_fi_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE;

-- ============================================================
-- 4. bom_contextes : ENUM activite → id_activite FK
-- ============================================================
ALTER TABLE bom_contextes
    DROP COLUMN IF EXISTS activite,
    ADD COLUMN id_activite INT NOT NULL AFTER description;

ALTER TABLE bom_contextes
    ADD CONSTRAINT fk_bc_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE;

-- ============================================================
-- 5. bom_fiches : DROP ENUM activite
--    Scope désormais implicite via la chaîne :
--    bom_fiches → bom_niveaux → bom_contextes → activites
-- ============================================================
ALTER TABLE bom_fiches
    DROP COLUMN IF EXISTS activite;

-- ============================================================
-- 6. Aucun seed — les activités sont créées par l'utilisateur via l'interface
--    (FrmPrincipal → bouton ⚙ → FrmActivites → FrmActiviteEdit)

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v7
-- Tables créées    : activites
-- Tables modifiées : fiches_ingredients  (DROP activite, ADD id_activite FK)
--                    bom_contextes       (DROP activite, ADD id_activite FK)
--                    bom_fiches          (DROP activite)
-- Seed             : aucun — ERP générique, activités créées par l'utilisateur
-- ============================================================
