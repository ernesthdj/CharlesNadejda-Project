-- =============================================
-- Migration v15 : Boutique Web
-- Ajoute les tables nécessaires au module e-commerce client
-- Partagées entre ArtisaStock (C# WinForms) et Laravel
--
-- Tables créées :
--   1. categories_web    — Catégories produits (gérées par l'admin ERP)
--   2. clients           — Comptes clients web
--   3. produits_web      — Fiches BOM publiées en boutique
--   4. commandes_web     — Commandes passées par les clients
--   5. commandes_web_lignes — Lignes de commande (détail articles)
--
-- Dépendances : bom_fiches (v04+v05)
-- =============================================

-- -------------------------------------------
-- 1. Catégories produits web
-- Gérées par l'admin depuis ArtisaStock (CRUD)
-- FK SET NULL : supprimer une catégorie ne supprime pas les produits
-- -------------------------------------------
CREATE TABLE IF NOT EXISTS categories_web (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(150)    NOT NULL,
    description     TEXT,
    ordre_affichage INT             NOT NULL DEFAULT 0,
    actif           TINYINT(1)      NOT NULL DEFAULT 1,
    date_creation   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uk_catweb_nom UNIQUE (nom)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------------------------
-- 2. Comptes clients web
-- Inscription depuis le site Laravel
-- Hash BCrypt interopérable PHP ↔ C#
-- -------------------------------------------
CREATE TABLE IF NOT EXISTS clients (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    nom               VARCHAR(100)    NOT NULL,
    prenom            VARCHAR(100)    NOT NULL,
    email             VARCHAR(255)    NOT NULL,
    mot_de_passe      VARCHAR(255)    NOT NULL COMMENT 'BCrypt hash — password_hash() PHP / BCrypt.Net C#',
    telephone         VARCHAR(20),
    adresse_rue       VARCHAR(255),
    adresse_cp        VARCHAR(10),
    adresse_ville     VARCHAR(100),
    adresse_pays      VARCHAR(100)    NOT NULL DEFAULT 'Belgique',
    actif             TINYINT(1)      NOT NULL DEFAULT 1,
    date_creation     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT uk_clients_email UNIQUE (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------------------------
-- 3. Produits web (fiches BOM publiées en boutique)
-- L'admin sélectionne une bom_fiche et la publie avec un nom commercial,
-- un prix de vente, une description et une image.
-- Stock = calculé dynamiquement depuis bom_stocks (pas stocké ici).
-- Une fiche BOM = un seul produit web (UNIQUE sur id_bom_fiche).
-- -------------------------------------------
CREATE TABLE IF NOT EXISTS produits_web (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_bom_fiche      INT             NOT NULL COMMENT 'FK vers bom_fiches — le produit fabriqué',
    id_categorie      INT                      COMMENT 'FK vers categories_web — nullable si pas de catégorie',
    nom_commercial    VARCHAR(200)    NOT NULL,
    description       TEXT,
    prix_vente        DECIMAL(10,2)   NOT NULL COMMENT 'Prix de vente TTC en euros',
    image_path        VARCHAR(500)             COMMENT 'Chemin relatif : produits/xxx.jpg (storage local)',
    en_vente          TINYINT(1)      NOT NULL DEFAULT 1 COMMENT '1 = publié, 0 = dépublié (masqué du catalogue)',
    ordre_affichage   INT             NOT NULL DEFAULT 0,
    date_creation     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_prodweb_bomfiche
        FOREIGN KEY (id_bom_fiche) REFERENCES bom_fiches(id) ON DELETE RESTRICT,
    CONSTRAINT fk_prodweb_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories_web(id) ON DELETE SET NULL,
    CONSTRAINT uk_prodweb_fiche
        UNIQUE (id_bom_fiche)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------------------------
-- 4. Commandes web
-- Cycle de vie : panier → payee (→ annulee en v2)
-- Le panier est une commande avec statut 'panier'.
-- Un client a au plus un panier actif.
-- adresse_livraison = snapshot au moment de la validation.
-- -------------------------------------------
CREATE TABLE IF NOT EXISTS commandes_web (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_client           INT             NOT NULL,
    statut              ENUM('panier','payee','annulee')
                                        NOT NULL DEFAULT 'panier',
    total_ttc           DECIMAL(10,2)   NOT NULL DEFAULT 0.00,
    adresse_livraison   TEXT            COMMENT 'Snapshot adresse au moment de la validation',
    date_commande       DATETIME        COMMENT 'NULL tant que panier, renseigné à la validation',
    date_creation       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_cmdweb_client
        FOREIGN KEY (id_client) REFERENCES clients(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------------------------
-- 5. Lignes de commande web
-- Chaque ligne = un produit × quantité avec prix snapshot.
-- sous_total = colonne calculée (GENERATED STORED).
-- CASCADE sur la commande : supprimer une commande supprime ses lignes.
-- RESTRICT sur le produit : on ne peut pas supprimer un produit commandé.
-- -------------------------------------------
CREATE TABLE IF NOT EXISTS commandes_web_lignes (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    id_commande     INT             NOT NULL,
    id_produit_web  INT             NOT NULL,
    quantite        INT             NOT NULL DEFAULT 1,
    prix_unitaire   DECIMAL(10,2)   NOT NULL COMMENT 'Snapshot du prix au moment de l ajout au panier',
    sous_total      DECIMAL(10,2)   GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,

    CONSTRAINT fk_cmdligne_cmd
        FOREIGN KEY (id_commande) REFERENCES commandes_web(id) ON DELETE CASCADE,
    CONSTRAINT fk_cmdligne_prodweb
        FOREIGN KEY (id_produit_web) REFERENCES produits_web(id) ON DELETE RESTRICT,

    CONSTRAINT chk_cmdligne_qte_positive
        CHECK (quantite >= 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------------------------
-- Index de performance
-- -------------------------------------------
CREATE INDEX idx_prodweb_envente ON produits_web (en_vente, ordre_affichage);
CREATE INDEX idx_prodweb_categorie ON produits_web (id_categorie);
CREATE INDEX idx_cmdweb_client_statut ON commandes_web (id_client, statut);
CREATE INDEX idx_cmdweb_statut ON commandes_web (statut);
CREATE INDEX idx_cmdligne_commande ON commandes_web_lignes (id_commande);
