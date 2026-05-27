-- ============================================================
-- CharlesNadejda — Schéma complet de la base de données
-- Exporté le 2026-05-27 (post migration v18)
-- MySQL 8.0 · InnoDB · utf8mb4
-- ============================================================
--
-- 📌 SCRIPT DEFENSE — Étape 8 : Defense DB
--   8.1 Vue d'ensemble : 19 tables, 1 VIEW, 5 modules
--   8.2 Normalisation 3NF + dénormalisation volontaire (prix_unitaire snapshot)
--   8.3 Types : ENUM, DECIMAL, TINYINT(1), GENERATED, CHECK
--   8.4 Clés : PK auto/composite, FK CASCADE/RESTRICT/SET NULL, UNIQUE composites
--   8.5 Relations : 1:N, M:N (activites_stocks), FK polymorphique (bom_fiches_lignes)
--   8.6 VIEW vue_stock_global : UNION ALL, COALESCE, NULLIF
--   8.8 Migrations : v01..v18 (ALTER TABLE incrémentales)
-- ============================================================

-- ════════════════════════════════════════════════════════════
--  MODULE : Référentiels de base
-- ════════════════════════════════════════════════════════════

CREATE TABLE activites (
  id              INT          NOT NULL AUTO_INCREMENT,
  nom             VARCHAR(100) NOT NULL,
  description     TEXT,
  actif           TINYINT(1)   NOT NULL DEFAULT 1,
  date_creation   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY (nom)
);

CREATE TABLE stocks (
  id              INT          NOT NULL AUTO_INCREMENT,
  nom             VARCHAR(200) NOT NULL,
  description     TEXT,
  actif           TINYINT(1)   NOT NULL DEFAULT 1,
  date_creation   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY (nom)
);

-- Jonction M:N — une activité peut utiliser plusieurs stocks et vice-versa
CREATE TABLE activites_stocks (
  id_activite     INT NOT NULL,
  id_stock        INT NOT NULL,
  PRIMARY KEY (id_activite, id_stock),
  CONSTRAINT fk_as_activite FOREIGN KEY (id_activite) REFERENCES activites(id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_as_stock    FOREIGN KEY (id_stock)    REFERENCES stocks(id)     ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE fournisseurs (
  id              INT          NOT NULL AUTO_INCREMENT,
  nom             VARCHAR(200) NOT NULL,
  contact         VARCHAR(200),
  email           VARCHAR(255),
  telephone       VARCHAR(20),
  adresse         VARCHAR(255),
  notes           TEXT,
  PRIMARY KEY (id)
);

-- ════════════════════════════════════════════════════════════
--  MODULE : Catalogue ingrédients & lots
-- ════════════════════════════════════════════════════════════

CREATE TABLE fiches_ingredients (
  id                      INT          NOT NULL AUTO_INCREMENT,
  nom                     VARCHAR(200) NOT NULL,
  marque                  VARCHAR(200),
  description             TEXT,
  unite_mesure            ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL,
  type_physique           ENUM('solide','liquide','poudre','piece') NOT NULL DEFAULT 'solide',
  densite                 DECIMAL(8,4)  COMMENT 'g/ml — obligatoire si liquide ou poudre',
  conditionnement_label   VARCHAR(100)  NOT NULL DEFAULT '',
  qte_par_conditionnement DECIMAL(12,4) NOT NULL DEFAULT 1.0000,
  prix_achat_reference    DECIMAL(10,4) NOT NULL,
  dlc_jours_reference     INT,
  qualite_label           VARCHAR(100),
  id_fournisseur_defaut   INT,
  seuil_alerte_stock      DECIMAL(10,4),
  stock_cible             DECIMAL(10,4),
  actif                   TINYINT(1)    NOT NULL DEFAULT 1,
  date_creation           DATETIME      DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY (nom),
  CONSTRAINT fk_fi_fournisseur FOREIGN KEY (id_fournisseur_defaut) REFERENCES fournisseurs(id) ON DELETE SET NULL ON UPDATE CASCADE
);

-- Le stock physique est assigné au lot (moment de l'achat), pas à la fiche
CREATE TABLE lots_ingredients (
  id                  INT          NOT NULL AUTO_INCREMENT,
  id_fiche_ingredient INT          NOT NULL,
  id_stock            INT          NOT NULL,
  numero_lot          VARCHAR(100),
  id_fournisseur      INT,
  nb_conditionnements DECIMAL(10,3) NOT NULL DEFAULT 1.000,
  date_achat          DATE          NOT NULL,
  date_peremption     DATE,
  quantite_initiale   DECIMAL(10,4) NOT NULL,
  quantite_disponible DECIMAL(10,4) NOT NULL,
  prix_unitaire       DECIMAL(10,4) NOT NULL DEFAULT 0.0000,
  prix_achat_reel     DECIMAL(10,4) NOT NULL,
  reference_facture   VARCHAR(100),
  notes               TEXT,
  date_creation       DATETIME      DEFAULT CURRENT_TIMESTAMP,
  tva_pct             DECIMAL(5,2)  NOT NULL DEFAULT 0.00 COMMENT 'Taux TVA % (0 = exonéré). Prix stocké toujours HTVA.',
  PRIMARY KEY (id),
  CONSTRAINT fk_lot_fiche       FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_lots_stock      FOREIGN KEY (id_stock)            REFERENCES stocks(id),
  CONSTRAINT fk_lot_fournisseur FOREIGN KEY (id_fournisseur)      REFERENCES fournisseurs(id)       ON DELETE SET NULL ON UPDATE CASCADE
);

-- ════════════════════════════════════════════════════════════
--  MODULE : BOM (Bill of Materials) — Contextes, Niveaux, Fiches
-- ════════════════════════════════════════════════════════════

CREATE TABLE bom_contextes (
  id              INT          NOT NULL AUTO_INCREMENT,
  nom             VARCHAR(200) NOT NULL,
  description     TEXT,
  id_activite     INT          NOT NULL,
  actif           TINYINT(1)   NOT NULL DEFAULT 1,
  date_creation   DATETIME     DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_bc_activite FOREIGN KEY (id_activite) REFERENCES activites(id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE bom_niveaux (
  id              INT              NOT NULL AUTO_INCREMENT,
  id_contexte     INT              NOT NULL,
  ordre           TINYINT UNSIGNED NOT NULL,
  nom             VARCHAR(200)     NOT NULL,
  description     TEXT,
  date_creation   DATETIME         DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_bom_niveau_ordre (id_contexte, ordre),
  CONSTRAINT fk_bn_contexte FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE bom_fiches (
  id                INT          NOT NULL AUTO_INCREMENT,
  id_niveau         INT          NOT NULL,
  nom               VARCHAR(200) NOT NULL,
  description       TEXT,
  unite_output      ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL DEFAULT 'piece',
  quantite_output   DECIMAL(10,4) NOT NULL DEFAULT 1.0000,
  temps_preparation INT,
  stock_cible       DECIMAL(10,4),
  actif             TINYINT(1)    NOT NULL DEFAULT 1,
  date_creation     DATETIME      DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_fiche_nom_niveau (nom, id_niveau),
  CONSTRAINT fk_bf_niveau FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE bom_fiches_lignes (
  id                  INT NOT NULL AUTO_INCREMENT,
  id_fiche            INT NOT NULL,
  type_input          ENUM('ingredient','fiche') NOT NULL,
  id_input_ingredient INT,
  id_input_fiche      INT,
  quantite            DECIMAL(12,4) NOT NULL,
  unite_mesure        ENUM('mg','g','kg','ml','cl','dl','l','piece') NOT NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_bfl_fiche       FOREIGN KEY (id_fiche)            REFERENCES bom_fiches(id)           ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_bfl_ingredient  FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)   ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_bfl_fiche_input FOREIGN KEY (id_input_fiche)      REFERENCES bom_fiches(id)           ON DELETE CASCADE ON UPDATE CASCADE
);

-- ════════════════════════════════════════════════════════════
--  MODULE : Production & Stock BOM
-- ════════════════════════════════════════════════════════════

CREATE TABLE bom_productions (
  id                  INT           NOT NULL AUTO_INCREMENT,
  id_niveau           INT           NOT NULL,
  id_fiche            INT           NOT NULL,
  quantite_produite   DECIMAL(10,4) NOT NULL,
  cout_ingredients    DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  cout_unitaire       DECIMAL(10,4) NOT NULL DEFAULT 0.0000,
  date_production     DATETIME      DEFAULT CURRENT_TIMESTAMP,
  notes               TEXT,
  PRIMARY KEY (id),
  CONSTRAINT fk_bp_niveau FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bp_fiche  FOREIGN KEY (id_fiche)  REFERENCES bom_fiches(id)  ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE bom_productions_lignes (
  id                    INT           NOT NULL AUTO_INCREMENT,
  id_production         INT           NOT NULL,
  type_source           ENUM('lot_ingredient','bom_stock') NOT NULL,
  id_lot_ingredient     INT,
  id_bom_stock          INT,
  quantite_consommee    DECIMAL(12,4) NOT NULL,
  cout_unitaire_moment  DECIMAL(10,4) NOT NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_bpl_production FOREIGN KEY (id_production)     REFERENCES bom_productions(id)  ON DELETE CASCADE  ON UPDATE CASCADE,
  CONSTRAINT fk_bpl_lot        FOREIGN KEY (id_lot_ingredient) REFERENCES lots_ingredients(id)  ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bpl_stock      FOREIGN KEY (id_bom_stock)      REFERENCES bom_stocks(id)        ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE bom_stocks (
  id                  INT           NOT NULL AUTO_INCREMENT,
  id_niveau           INT           NOT NULL,
  id_contexte         INT           NOT NULL,
  id_activite         INT           NOT NULL,
  id_fiche            INT           NOT NULL,
  id_production       INT           NOT NULL,
  quantite_disponible DECIMAL(12,4) NOT NULL,
  cout_unitaire       DECIMAL(10,4) NOT NULL,
  date_production     DATE          NOT NULL,
  date_dlc            DATE,
  date_creation       DATETIME      DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_bs_niveau     FOREIGN KEY (id_niveau)     REFERENCES bom_niveaux(id)     ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bs_contexte   FOREIGN KEY (id_contexte)   REFERENCES bom_contextes(id)   ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bs_activite   FOREIGN KEY (id_activite)   REFERENCES activites(id)       ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bs_fiche      FOREIGN KEY (id_fiche)      REFERENCES bom_fiches(id)      ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_bs_production FOREIGN KEY (id_production) REFERENCES bom_productions(id) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE bom_reservations (
  id                  INT           NOT NULL AUTO_INCREMENT,
  id_lot              INT           NOT NULL,
  id_contexte         INT           NOT NULL,
  quantite_reservee   DECIMAL(12,4) NOT NULL,
  date_reservation    DATETIME      DEFAULT CURRENT_TIMESTAMP,
  notes               TEXT,
  actif               TINYINT(1)    NOT NULL DEFAULT 1,
  PRIMARY KEY (id),
  CONSTRAINT fk_br_lot      FOREIGN KEY (id_lot)      REFERENCES lots_ingredients(id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_br_contexte FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)    ON DELETE CASCADE ON UPDATE CASCADE
);

-- ════════════════════════════════════════════════════════════
--  MODULE : Boutique web (e-commerce)
-- ════════════════════════════════════════════════════════════

CREATE TABLE clients (
  id                  INT          NOT NULL AUTO_INCREMENT,
  nom                 VARCHAR(100) NOT NULL,
  prenom              VARCHAR(100) NOT NULL,
  email               VARCHAR(255) NOT NULL,
  mot_de_passe        VARCHAR(255) NOT NULL COMMENT 'BCrypt hash',
  telephone           VARCHAR(20),
  adresse_rue         VARCHAR(255),
  adresse_cp          VARCHAR(10),
  adresse_ville       VARCHAR(100),
  adresse_pays        VARCHAR(100) NOT NULL DEFAULT 'Belgique',
  actif               TINYINT(1)   NOT NULL DEFAULT 1,
  date_creation       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  date_modification   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_clients_email (email)
);

CREATE TABLE categories_web (
  id                INT          NOT NULL AUTO_INCREMENT,
  nom               VARCHAR(150) NOT NULL,
  description       TEXT,
  ordre_affichage   INT          NOT NULL DEFAULT 0,
  actif             TINYINT(1)   NOT NULL DEFAULT 1,
  date_creation     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_catweb_nom (nom)
);

CREATE TABLE produits_web (
  id                INT          NOT NULL AUTO_INCREMENT,
  id_bom_fiche      INT          NOT NULL COMMENT 'FK vers bom_fiches — le produit fabriqué',
  id_categorie      INT                   COMMENT 'FK vers categories_web',
  nom_commercial    VARCHAR(200) NOT NULL,
  description       TEXT,
  prix_vente        DECIMAL(10,2) NOT NULL COMMENT 'Prix TTC €',
  image_path        VARCHAR(500)           COMMENT 'Chemin relatif : produits/xxx.jpg',
  en_vente          TINYINT(1)    NOT NULL DEFAULT 1,
  ordre_affichage   INT           NOT NULL DEFAULT 0,
  date_creation     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  date_modification DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_prodweb_fiche (id_bom_fiche),
  CONSTRAINT fk_prodweb_bomfiche  FOREIGN KEY (id_bom_fiche) REFERENCES bom_fiches(id)      ON DELETE RESTRICT,
  CONSTRAINT fk_prodweb_categorie FOREIGN KEY (id_categorie) REFERENCES categories_web(id)  ON DELETE SET NULL
);

CREATE TABLE commandes_web (
  id                  INT  NOT NULL AUTO_INCREMENT,
  id_client           INT  NOT NULL,
  statut              ENUM('panier','payee','annulee') NOT NULL DEFAULT 'panier',
  total_ttc           DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  adresse_livraison   TEXT          COMMENT 'Snapshot adresse au moment de la validation',
  date_commande       DATETIME      COMMENT 'NULL tant que panier',
  date_creation       DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_cmdweb_client_statut (id_client, statut),
  KEY idx_cmdweb_statut (statut),
  CONSTRAINT fk_cmdweb_client FOREIGN KEY (id_client) REFERENCES clients(id) ON DELETE RESTRICT
);

CREATE TABLE commandes_web_lignes (
  id              INT           NOT NULL AUTO_INCREMENT,
  id_commande     INT           NOT NULL,
  id_produit_web  INT           NOT NULL,
  quantite        INT           NOT NULL DEFAULT 1,
  prix_unitaire   DECIMAL(10,2) NOT NULL COMMENT 'Snapshot du prix au moment de l ajout',
  sous_total      DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
  PRIMARY KEY (id),
  CONSTRAINT fk_cmdligne_cmd     FOREIGN KEY (id_commande)    REFERENCES commandes_web(id) ON DELETE CASCADE,
  CONSTRAINT fk_cmdligne_prodweb FOREIGN KEY (id_produit_web) REFERENCES produits_web(id)   ON DELETE RESTRICT,
  CONSTRAINT chk_cmdligne_qte_positive CHECK (quantite >= 1)
);

-- ════════════════════════════════════════════════════════════
--  MODULE : Utilisateurs admin (ERP)
-- ════════════════════════════════════════════════════════════

CREATE TABLE utilisateurs (
  id                INT          NOT NULL AUTO_INCREMENT,
  nom               VARCHAR(100) NOT NULL,
  prenom            VARCHAR(100) NOT NULL,
  email             VARCHAR(255) NOT NULL,
  mot_de_passe      VARCHAR(255) NOT NULL,
  role              ENUM('client','admin') NOT NULL DEFAULT 'client',
  telephone         VARCHAR(20),
  adresse           VARCHAR(255),
  code_postal       VARCHAR(10),
  ville             VARCHAR(100),
  date_inscription  DATETIME     DEFAULT CURRENT_TIMESTAMP,
  actif             TINYINT(1)   NOT NULL DEFAULT 1,
  PRIMARY KEY (id),
  UNIQUE KEY (email)
);

-- ════════════════════════════════════════════════════════════
--  VIEW : Vue stock global (agrège lots + produits BOM)
-- ════════════════════════════════════════════════════════════

CREATE OR REPLACE VIEW vue_stock_global AS

    -- Matières premières (lots achetés)
    SELECT
        'lot_ingredient'        AS type_stock,
        li.id                   AS id_entree,
        fi.nom                  AS nom,
        fi.unite_mesure         AS unite,
        li.quantite_disponible  AS quantite_totale,
        COALESCE(SUM(br.quantite_reservee), 0) AS quantite_reservee,
        li.quantite_disponible - COALESCE(SUM(br.quantite_reservee), 0) AS quantite_dispo_reelle,
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
    JOIN fiches_ingredients fi ON fi.id = li.id_fiche_ingredient
    JOIN stocks s              ON s.id  = li.id_stock
    LEFT JOIN bom_reservations br ON br.id_lot = li.id AND br.actif = 1
    GROUP BY li.id, fi.id, fi.nom, fi.unite_mesure,
             li.quantite_disponible, li.prix_unitaire, fi.qte_par_conditionnement,
             fi.conditionnement_label, li.date_peremption, li.id_stock, s.nom

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
