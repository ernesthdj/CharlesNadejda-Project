-- ============================================================
-- Migration v10 — Découplage Stocks / Activités
--
-- Architecture :
--   stocks           : contenants physiques/logiques indépendants
--   activites_stocks : jonction M:N (activité ↔ stock)
--   fiches_ingredients : id_activite → id_stock
--
-- Règle : un ingrédient appartient à UN stock physique.
--         Une activité consomme depuis N stocks (via jonction).
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. Nouvelle table stocks
-- ============================================================
CREATE TABLE IF NOT EXISTS stocks (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(200) NOT NULL UNIQUE,
    description   TEXT,
    actif         TINYINT(1)  NOT NULL DEFAULT 1,
    date_creation DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 2. Table de jonction activites_stocks  (M:N)
--    Cascade sur les deux FK : supprimer une activité ou un stock
--    retire automatiquement le lien.
-- ============================================================
CREATE TABLE IF NOT EXISTS activites_stocks (
    id_activite INT NOT NULL,
    id_stock    INT NOT NULL,
    PRIMARY KEY (id_activite, id_stock),
    CONSTRAINT fk_as_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_as_stock
        FOREIGN KEY (id_stock) REFERENCES stocks(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 3. fiches_ingredients : id_activite → id_stock
--    La DB est vide (v9) donc pas de data migration nécessaire.
-- ============================================================
ALTER TABLE fiches_ingredients DROP FOREIGN KEY IF EXISTS fk_fi_activite;
ALTER TABLE fiches_ingredients DROP COLUMN IF EXISTS id_activite;
ALTER TABLE fiches_ingredients ADD COLUMN id_stock INT NOT NULL AFTER densite;

ALTER TABLE fiches_ingredients
    ADD CONSTRAINT fk_fi_stock
        FOREIGN KEY (id_stock) REFERENCES stocks(id)
        ON DELETE CASCADE ON UPDATE CASCADE;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v10
--
-- Tables créées  : stocks, activites_stocks
-- Tables modif.  : fiches_ingredients (DROP id_activite, ADD id_stock)
-- FK supprimée   : fk_fi_activite
-- FKs ajoutées   : fk_fi_stock, fk_as_activite, fk_as_stock
-- ============================================================
