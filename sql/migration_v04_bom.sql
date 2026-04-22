-- ============================================================
-- Migration v4 — BOM Générique Multi-Niveaux
-- À exécuter sur une DB existante (charlesnadejda déjà peuplée).
-- Pour une DB fraîche, utiliser create_database.sql directement.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. Corrections fiches_ingredients (champs manquants)
-- ============================================================
ALTER TABLE fiches_ingredients
    ADD COLUMN IF NOT EXISTS type_physique ENUM('solide','liquide','poudre','piece')
        NOT NULL DEFAULT 'solide'
        AFTER unite_mesure,
    ADD COLUMN IF NOT EXISTS densite DECIMAL(8,4) DEFAULT NULL
        COMMENT 'g/ml — obligatoire si liquide ou poudre'
        AFTER type_physique,
    ADD COLUMN IF NOT EXISTS activite ENUM('chocolaterie','patisserie','partage')
        NOT NULL DEFAULT 'partage'
        AFTER densite,
    MODIFY COLUMN unite_mesure ENUM('kg','g','l','ml','cl','piece') NOT NULL;

-- ============================================================
-- 2. Corrections lots_ingredients (champ prix_unitaire manquant)
-- ============================================================
ALTER TABLE lots_ingredients
    ADD COLUMN IF NOT EXISTS prix_unitaire DECIMAL(10,4) NOT NULL DEFAULT 0
        AFTER quantite_disponible;

-- ============================================================
-- 3. Nouvelles tables BOM
-- ============================================================

CREATE TABLE IF NOT EXISTS bom_contextes (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(200) NOT NULL,
    description   TEXT,
    activite      ENUM('chocolaterie','patisserie') NOT NULL,
    actif         TINYINT(1) NOT NULL DEFAULT 1,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_niveaux (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    id_contexte   INT NOT NULL,
    ordre         TINYINT UNSIGNED NOT NULL,
    nom           VARCHAR(200) NOT NULL,
    description   TEXT,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_bom_niveau_ordre (id_contexte, ordre),
    CONSTRAINT fk_bn_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_fiches (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    nom               VARCHAR(200) NOT NULL UNIQUE,
    description       TEXT,
    activite          ENUM('chocolaterie','patisserie','partage') NOT NULL DEFAULT 'partage',
    unite_output      ENUM('kg','g','l','ml','cl','piece') NOT NULL DEFAULT 'piece',
    quantite_output   DECIMAL(10,4) NOT NULL DEFAULT 1
                          COMMENT 'Quantité produite par une exécution',
    temps_preparation INT DEFAULT NULL COMMENT 'Minutes estimées',
    actif             TINYINT(1) NOT NULL DEFAULT 1,
    date_creation     DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_fiches_lignes (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche            INT NOT NULL,
    type_input          ENUM('ingredient','fiche') NOT NULL,
    id_input_ingredient INT DEFAULT NULL,
    id_input_fiche      INT DEFAULT NULL,
    quantite            DECIMAL(12,4) NOT NULL,
    unite_mesure        ENUM('kg','g','l','ml','cl','piece') NOT NULL,
    CONSTRAINT fk_bfl_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_ingredient
        FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_fiche_input
        FOREIGN KEY (id_input_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_bfl_input CHECK (
        (type_input = 'ingredient' AND id_input_ingredient IS NOT NULL AND id_input_fiche IS NULL)
        OR
        (type_input = 'fiche' AND id_input_fiche IS NOT NULL AND id_input_ingredient IS NULL)
    )
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_productions (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau         INT NOT NULL,
    id_fiche          INT NOT NULL,
    quantite_produite DECIMAL(10,4) NOT NULL,
    cout_ingredients  DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_unitaire     DECIMAL(10,4) NOT NULL DEFAULT 0.0000,
    date_production   DATETIME DEFAULT CURRENT_TIMESTAMP,
    notes             TEXT,
    CONSTRAINT fk_bp_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bp_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_stocks (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau           INT NOT NULL,
    id_fiche            INT NOT NULL,
    id_production       INT NOT NULL,
    quantite_disponible DECIMAL(12,4) NOT NULL,
    cout_unitaire       DECIMAL(10,4) NOT NULL,
    date_production     DATE NOT NULL,
    date_dlc            DATE DEFAULT NULL,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_bs_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_production
        FOREIGN KEY (id_production) REFERENCES bom_productions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_productions_lignes (
    id                   INT AUTO_INCREMENT PRIMARY KEY,
    id_production        INT NOT NULL,
    type_source          ENUM('lot_ingredient','bom_stock') NOT NULL,
    id_lot_ingredient    INT DEFAULT NULL,
    id_bom_stock         INT DEFAULT NULL,
    quantite_consommee   DECIMAL(12,4) NOT NULL,
    cout_unitaire_moment DECIMAL(10,4) NOT NULL,
    CONSTRAINT fk_bpl_production
        FOREIGN KEY (id_production) REFERENCES bom_productions(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_bpl_lot
        FOREIGN KEY (id_lot_ingredient) REFERENCES lots_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bpl_stock
        FOREIGN KEY (id_bom_stock) REFERENCES bom_stocks(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_bpl_source CHECK (
        (type_source = 'lot_ingredient' AND id_lot_ingredient IS NOT NULL AND id_bom_stock IS NULL)
        OR
        (type_source = 'bom_stock' AND id_bom_stock IS NOT NULL AND id_lot_ingredient IS NULL)
    )
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bom_reservations (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_lot            INT NOT NULL,
    id_contexte       INT NOT NULL,
    quantite_reservee DECIMAL(12,4) NOT NULL,
    date_reservation  DATETIME DEFAULT CURRENT_TIMESTAMP,
    notes             TEXT,
    actif             TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT fk_br_lot
        FOREIGN KEY (id_lot) REFERENCES lots_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_br_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v4
-- Nouvelles tables : bom_contextes, bom_niveaux, bom_fiches,
--   bom_fiches_lignes, bom_productions, bom_stocks,
--   bom_productions_lignes, bom_reservations
-- Colonnes ajoutées : fiches_ingredients.(type_physique, densite, activite)
--                     lots_ingredients.(prix_unitaire)
-- ============================================================
