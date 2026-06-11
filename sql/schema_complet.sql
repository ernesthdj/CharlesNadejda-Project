-- ============================================================
-- CharlesNadejda — Schema complet de la base de donnees
-- GENERE automatiquement depuis create_database.sql
-- Date de regeneration : 2026-06-11 (post migration v19)
-- ============================================================
--
-- IMPORTANT : Ce fichier est un MIROIR de create_database.sql.
-- La source de verite est create_database.sql.
-- Si une modification schema est necessaire, modifier create_database.sql
-- et regenerer ce fichier.
--
-- 20 tables, 1 VIEW, 5 CHECK constraints
-- Modules : Referentiels, Ingredients/Lots, BOM, Production/Stock, Boutique Web, Utilisateurs
-- ============================================================

-- ============================================================
-- CONTENU IDENTIQUE A create_database.sql
-- ============================================================

CREATE DATABASE IF NOT EXISTS charlesnadejda
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. activites (v07)
-- ============================================================
CREATE TABLE IF NOT EXISTS activites (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(100) NOT NULL UNIQUE,
    description   TEXT,
    actif         TINYINT(1)  NOT NULL DEFAULT 1,
    date_creation DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 2. stocks (v10)
-- ============================================================
CREATE TABLE IF NOT EXISTS stocks (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(200) NOT NULL UNIQUE,
    description   TEXT,
    actif         TINYINT(1)  NOT NULL DEFAULT 1,
    date_creation DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 3. activites_stocks (v10) — jonction M:N
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
-- 4. fournisseurs
-- ============================================================
CREATE TABLE IF NOT EXISTS fournisseurs (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    nom       VARCHAR(200) NOT NULL,
    contact   VARCHAR(200),
    email     VARCHAR(255),
    telephone VARCHAR(20),
    adresse   VARCHAR(255),
    notes     TEXT
) ENGINE=InnoDB;

-- ============================================================
-- 5. fiches_ingredients
-- ============================================================
CREATE TABLE IF NOT EXISTS fiches_ingredients (
    id                      INT AUTO_INCREMENT PRIMARY KEY,
    nom                     VARCHAR(200) NOT NULL UNIQUE,
    marque                  VARCHAR(200),
    description             TEXT,
    unite_mesure            ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL,
    type_physique           ENUM('solide','liquide','poudre','piece') NOT NULL DEFAULT 'solide',
    densite                 DECIMAL(8,4) DEFAULT NULL COMMENT 'g/ml — obligatoire si liquide ou poudre',
    conditionnement_label   VARCHAR(100) NOT NULL DEFAULT '',
    qte_par_conditionnement DECIMAL(12,4) NOT NULL DEFAULT 1,
    nb_par_lot              INT NOT NULL DEFAULT 1,
    prix_achat_reference    DECIMAL(10,4) NOT NULL DEFAULT 0,
    dlc_jours_reference     INT DEFAULT NULL,
    qualite_label           VARCHAR(100) DEFAULT NULL,
    id_fournisseur_defaut   INT DEFAULT NULL,
    seuil_alerte_stock      DECIMAL(10,4) DEFAULT NULL,
    stock_cible             DECIMAL(10,4) DEFAULT NULL,
    actif                   TINYINT(1) NOT NULL DEFAULT 1,
    date_creation           DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_fi_fournisseur
        FOREIGN KEY (id_fournisseur_defaut) REFERENCES fournisseurs(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 6. lots_ingredients
-- ============================================================
CREATE TABLE IF NOT EXISTS lots_ingredients (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche_ingredient INT NOT NULL,
    id_stock            INT NOT NULL,
    numero_lot          VARCHAR(100) DEFAULT NULL,
    id_fournisseur      INT DEFAULT NULL,
    nb_conditionnements DECIMAL(10,3) NOT NULL DEFAULT 1,
    date_achat          DATE NOT NULL,
    date_peremption     DATE DEFAULT NULL,
    quantite_initiale   DECIMAL(10,4) NOT NULL,
    quantite_disponible DECIMAL(10,4) NOT NULL,
    prix_unitaire       DECIMAL(10,4) NOT NULL DEFAULT 0,
    prix_achat_reel     DECIMAL(10,4) NOT NULL DEFAULT 0,
    reference_facture   VARCHAR(100) DEFAULT NULL,
    notes               TEXT,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    tva_pct             DECIMAL(5,2) NOT NULL DEFAULT 0
                        COMMENT 'Taux de TVA en % (0 = exonere). Prix stocke toujours en HTVA.',
    CONSTRAINT fk_lot_fiche
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_lots_stock
        FOREIGN KEY (id_stock) REFERENCES stocks(id),
    CONSTRAINT fk_lot_fournisseur
        FOREIGN KEY (id_fournisseur) REFERENCES fournisseurs(id)
        ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT chk_lot_qte_positive
        CHECK (quantite_disponible >= 0)
) ENGINE=InnoDB;

-- ============================================================
-- 7. utilisateurs
-- ============================================================
CREATE TABLE IF NOT EXISTS utilisateurs (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    nom              VARCHAR(100) NOT NULL,
    prenom           VARCHAR(100) NOT NULL,
    email            VARCHAR(255) NOT NULL UNIQUE,
    mot_de_passe     VARCHAR(255) NOT NULL,
    role             ENUM('client','admin') NOT NULL DEFAULT 'client',
    telephone        VARCHAR(20),
    adresse          VARCHAR(255),
    code_postal      VARCHAR(10),
    ville            VARCHAR(100),
    date_inscription DATETIME DEFAULT CURRENT_TIMESTAMP,
    actif            TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ============================================================
-- 8. bom_contextes (FK -> activites)
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_contextes (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(200) NOT NULL,
    description   TEXT,
    id_activite   INT NOT NULL,
    actif         TINYINT(1) NOT NULL DEFAULT 1,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_bc_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 9. bom_niveaux
-- ============================================================
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
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 10. bom_fiches
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_fiches (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau         INT NOT NULL,
    nom               VARCHAR(200) NOT NULL,
    description       TEXT,
    unite_output      ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL DEFAULT 'piece',
    quantite_output   DECIMAL(10,4) NOT NULL DEFAULT 1
                      COMMENT 'Quantite produite par une execution',
    temps_preparation INT DEFAULT NULL COMMENT 'Minutes estimees',
    stock_cible       DECIMAL(10,4) DEFAULT NULL,
    actif             TINYINT(1) NOT NULL DEFAULT 1,
    date_creation     DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_fiche_nom_niveau (nom, id_niveau),
    CONSTRAINT fk_bf_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 11. bom_fiches_lignes
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_fiches_lignes (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche            INT NOT NULL,
    type_input          ENUM('ingredient','fiche') NOT NULL,
    id_input_ingredient INT DEFAULT NULL,
    id_input_fiche      INT DEFAULT NULL,
    quantite            DECIMAL(12,4) NOT NULL,
    unite_mesure        ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL,
    CONSTRAINT fk_bfl_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_ingredient
        FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_fiche_input
        FOREIGN KEY (id_input_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_bfl_input CHECK (
        (type_input = 'ingredient' AND id_input_ingredient IS NOT NULL AND id_input_fiche IS NULL)
        OR
        (type_input = 'fiche' AND id_input_fiche IS NOT NULL AND id_input_ingredient IS NULL)
    )
) ENGINE=InnoDB;

-- ============================================================
-- 12. bom_productions
-- ============================================================
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

-- ============================================================
-- 13. bom_stocks
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_stocks (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau           INT NOT NULL,
    id_contexte         INT NOT NULL,
    id_activite         INT NOT NULL,
    id_fiche            INT NOT NULL,
    id_production       INT NOT NULL COMMENT 'Production qui a cree ce stock',
    quantite_disponible DECIMAL(12,4) NOT NULL,
    cout_unitaire       DECIMAL(10,4) NOT NULL,
    date_production     DATE NOT NULL,
    date_dlc            DATE DEFAULT NULL,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_bs_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_production
        FOREIGN KEY (id_production) REFERENCES bom_productions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_bomstock_qte_positive
        CHECK (quantite_disponible >= 0)
) ENGINE=InnoDB;

-- ============================================================
-- 14. bom_productions_lignes
-- ============================================================
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

-- ============================================================
-- 15. bom_reservations
-- ============================================================
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
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_br_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 16. categories_web (v15)
-- ============================================================
CREATE TABLE IF NOT EXISTS categories_web (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(150) NOT NULL,
    description     TEXT,
    ordre_affichage INT          NOT NULL DEFAULT 0,
    actif           TINYINT(1)   NOT NULL DEFAULT 1,
    date_creation   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_catweb_nom UNIQUE (nom)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 17. clients (v15)
-- ============================================================
CREATE TABLE IF NOT EXISTS clients (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    nom               VARCHAR(100) NOT NULL,
    prenom            VARCHAR(100) NOT NULL,
    email             VARCHAR(255) NOT NULL,
    mot_de_passe      VARCHAR(255) NOT NULL COMMENT 'BCrypt hash',
    telephone         VARCHAR(20),
    adresse_rue       VARCHAR(255),
    adresse_cp        VARCHAR(10),
    adresse_ville     VARCHAR(100),
    adresse_pays      VARCHAR(100) NOT NULL DEFAULT 'Belgique',
    actif             TINYINT(1)   NOT NULL DEFAULT 1,
    date_creation     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uk_clients_email UNIQUE (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 18. produits_web (v15)
-- ============================================================
CREATE TABLE IF NOT EXISTS produits_web (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_bom_fiche      INT          NOT NULL,
    id_categorie      INT                  ,
    nom_commercial    VARCHAR(200) NOT NULL,
    description       TEXT,
    prix_vente        DECIMAL(10,2) NOT NULL,
    image_path        VARCHAR(500),
    en_vente          TINYINT(1)   NOT NULL DEFAULT 1,
    ordre_affichage   INT          NOT NULL DEFAULT 0,
    date_creation     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_prodweb_bomfiche
        FOREIGN KEY (id_bom_fiche) REFERENCES bom_fiches(id) ON DELETE RESTRICT,
    CONSTRAINT fk_prodweb_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories_web(id) ON DELETE SET NULL,
    CONSTRAINT uk_prodweb_fiche UNIQUE (id_bom_fiche)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 19. commandes_web (v15)
-- ============================================================
CREATE TABLE IF NOT EXISTS commandes_web (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_client           INT          NOT NULL,
    statut              ENUM('panier','payee','annulee')
                                     NOT NULL DEFAULT 'panier',
    total_ttc           DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    adresse_livraison   TEXT,
    date_commande       DATETIME,
    date_creation       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_cmdweb_client
        FOREIGN KEY (id_client) REFERENCES clients(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 20. commandes_web_lignes (v15)
-- ============================================================
CREATE TABLE IF NOT EXISTS commandes_web_lignes (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    id_commande     INT          NOT NULL,
    id_produit_web  INT          NOT NULL,
    quantite        INT          NOT NULL DEFAULT 1,
    prix_unitaire   DECIMAL(10,2) NOT NULL,
    sous_total      DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
    CONSTRAINT fk_cmdligne_cmd
        FOREIGN KEY (id_commande) REFERENCES commandes_web(id) ON DELETE CASCADE,
    CONSTRAINT fk_cmdligne_prodweb
        FOREIGN KEY (id_produit_web) REFERENCES produits_web(id) ON DELETE RESTRICT,
    CONSTRAINT chk_cmdligne_qte_positive
        CHECK (quantite >= 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Index de performance (v15)
-- ============================================================
CREATE INDEX idx_prodweb_envente ON produits_web (en_vente, ordre_affichage);
CREATE INDEX idx_prodweb_categorie ON produits_web (id_categorie);
CREATE INDEX idx_cmdweb_client_statut ON commandes_web (id_client, statut);
CREATE INDEX idx_cmdweb_statut ON commandes_web (statut);
CREATE INDEX idx_cmdligne_commande ON commandes_web_lignes (id_commande);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- VIEW : vue_stock_global (v11, maj v17, maj v18)
-- ============================================================
CREATE OR REPLACE VIEW vue_stock_global AS

    -- Matieres premieres (lots achetes)
    SELECT
        'lot_ingredient'        AS type_stock,
        li.id                   AS id_entree,
        fi.nom                  AS nom,
        fi.unite_mesure         AS unite,
        li.quantite_disponible  AS quantite_totale,
        COALESCE(SUM(br.quantite_reservee), 0) AS quantite_reservee,
        li.quantite_disponible
            - COALESCE(SUM(br.quantite_reservee), 0) AS quantite_dispo_reelle,
        li.prix_unitaire / NULLIF(fi.qte_par_conditionnement, 0) AS cout_unitaire,
        li.prix_unitaire        AS prix_conditionnement,
        fi.qte_par_conditionnement,
        fi.conditionnement_label,
        li.date_peremption      AS date_dlc,
        li.id_stock,
        s.nom                   AS stock_nom,
        NULL                    AS id_activite,
        NULL                    AS id_contexte,
        NULL                    AS id_niveau,
        NULL                    AS id_fiche_bom,
        fi.id                   AS id_fiche_ingredient
    FROM lots_ingredients li
    JOIN fiches_ingredients fi  ON fi.id = li.id_fiche_ingredient
    JOIN stocks s               ON s.id  = li.id_stock
    LEFT JOIN bom_reservations br
        ON br.id_lot = li.id AND br.actif = 1
    GROUP BY
        li.id, fi.id, fi.nom, fi.unite_mesure,
        li.quantite_disponible, li.prix_unitaire, fi.qte_par_conditionnement,
        fi.conditionnement_label,
        li.date_peremption, li.id_stock, s.nom

UNION ALL

    -- Produits fabriques (output BOM)
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
-- FIN — 20 tables + 1 VIEW + 5 CHECK constraints
-- ============================================================
