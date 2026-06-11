-- ============================================================
-- CharlesNadejda — Schema complet de la base de donnees
-- GENERE automatiquement depuis create_database.sql
-- Date de regeneration : 2026-06-11 (post migration v21)
-- ============================================================
--
-- IMPORTANT : Ce fichier est un MIROIR de create_database.sql.
-- La source de verite est create_database.sql.
-- Si une modification schema est necessaire, modifier create_database.sql
-- et regenerer ce fichier.
--
-- 20 tables, 1 VIEW, 8 CHECK constraints, 2 UNIQUE keys supplementaires
-- Modules : Referentiels, Ingredients/Lots, BOM, Production/Stock, Boutique Web, Utilisateurs
-- ============================================================

-- ============================================================
-- CONTENU IDENTIQUE A create_database.sql (commentaires inclus)
-- ============================================================

CREATE DATABASE IF NOT EXISTS charlesnadejda
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- MODULE : REFERENTIELS (tables de base partagees par tout l'ERP)
-- Activites = branches metier du patissier (ex: "Pralines", "Boulangerie").
-- Stocks = emplacements physiques de rangement (ex: "Frigo 1", "Reserve seche").
-- La table de jonction activites_stocks lie les deux en M:N.
-- ============================================================

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
-- Lie une activite a un ou plusieurs stocks physiques.
-- Permet de filtrer les lots visibles par activite dans l'UI.
CREATE TABLE IF NOT EXISTS activites_stocks (
    id_activite INT NOT NULL,
    id_stock    INT NOT NULL,
    PRIMARY KEY (id_activite, id_stock),
    -- Une liaison disparait si l'activite est supprimee
    CONSTRAINT fk_as_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Une liaison disparait si le stock est supprime
    CONSTRAINT fk_as_stock
        FOREIGN KEY (id_stock) REFERENCES stocks(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- MODULE : INGREDIENTS & LOTS (gestion des matieres premieres)
-- Fournisseurs → Fiches ingredients (catalogue) → Lots (achats physiques).
-- Les lots suivent un modele FIFO : quantite_disponible diminue
-- au fur et a mesure des consommations en production.
-- ============================================================

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
    notes     TEXT,
    -- Unicite metier : un seul fournisseur par nom
    UNIQUE KEY uk_fournisseur_nom (nom)
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
    -- Fournisseur par defaut pour les reapprovisionnements ; mis a NULL si le fournisseur est supprime
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
    -- Un lot est lie a UNE fiche ingredient ; suppression en cascade si la fiche disparait
    CONSTRAINT fk_lot_fiche
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Emplacement physique ou le lot est range ; RESTRICT empeche la suppression d'un stock utilise
    CONSTRAINT fk_lots_stock
        FOREIGN KEY (id_stock) REFERENCES stocks(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Fournisseur effectif de cet achat (peut differer du fournisseur par defaut de la fiche)
    CONSTRAINT fk_lot_fournisseur
        FOREIGN KEY (id_fournisseur) REFERENCES fournisseurs(id)
        ON DELETE SET NULL ON UPDATE CASCADE,
    -- Empeche un stock negatif : la quantite disponible ne peut jamais descendre sous zero
    CONSTRAINT chk_lot_qte_positive
        CHECK (quantite_disponible >= 0)
) ENGINE=InnoDB;

-- ============================================================
-- MODULE : UTILISATEURS (authentification app C# WinForms)
-- Comptes admin/client pour l'application desktop.
-- Mots de passe hashes en BCrypt (compatible PHP <-> C#).
-- ============================================================

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
-- MODULE : BOM (Bill of Materials — nomenclature de fabrication)
-- Gere la structure hierarchique des recettes :
--   Contexte (ex: "Pralines") → Niveaux (ex: "Ganaches", "Enrobage")
--     → Fiches (ex: "Ganache Praline") → Lignes (ingredients ou fiches)
-- Un contexte appartient a une activite. Les niveaux ordonnent les
-- etapes de fabrication. Les fiches sont les recettes detaillees.
-- ============================================================

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
    -- Unicite metier : un seul contexte BOM par (nom, activite)
    UNIQUE KEY uq_bomctx_nom_activite (nom, id_activite),
    -- Un contexte appartient a une activite ; RESTRICT empeche la suppression d'une activite utilisee
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
    -- Unicite : un seul niveau par position dans un contexte donne
    UNIQUE KEY uq_bom_niveau_ordre (id_contexte, ordre),
    -- Un niveau appartient a un contexte ; CASCADE car supprimer un contexte supprime ses niveaux
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
    -- Unicite : un seul nom de fiche par niveau (ex: pas deux "Ganache Praline" dans le meme niveau)
    UNIQUE KEY uq_fiche_nom_niveau (nom, id_niveau),
    -- Une fiche appartient a un niveau ; RESTRICT empeche la suppression d'un niveau qui a des fiches
    CONSTRAINT fk_bf_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Empeche un output a zero (evite division par zero dans le calcul de stock vendable)
    CONSTRAINT chk_bf_output_positive
        CHECK (quantite_output > 0)
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
    -- Fiche parente (la recette qui contient cette ligne)
    CONSTRAINT fk_bfl_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Si la ligne est de type 'ingredient' : reference vers la fiche ingredient (matiere premiere)
    CONSTRAINT fk_bfl_ingredient
        FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Si la ligne est de type 'fiche' : reference vers une autre fiche BOM (sous-recette)
    CONSTRAINT fk_bfl_fiche_input
        FOREIGN KEY (id_input_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Exclusion mutuelle : une ligne est SOIT un ingredient SOIT une sous-fiche, jamais les deux
    CONSTRAINT chk_bfl_input CHECK (
        (type_input = 'ingredient' AND id_input_ingredient IS NOT NULL AND id_input_fiche IS NULL)
        OR
        (type_input = 'fiche' AND id_input_fiche IS NOT NULL AND id_input_ingredient IS NULL)
    )
) ENGINE=InnoDB;

-- ============================================================
-- MODULE : PRODUCTION & STOCK BOM (execution des recettes et suivi des produits fabriques)
-- bom_productions      = un acte de fabrication d'une fiche (quantite, cout, date)
-- bom_productions_lignes = detail des matieres consommees (lots ou stocks BOM)
-- bom_stocks           = stock des produits fabriques (output d'une production)
-- bom_reservations     = quantites reservees sur un lot pour un contexte de production
-- ============================================================

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
    -- Niveau dans lequel cette production a eu lieu (tracabilite)
    CONSTRAINT fk_bp_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Fiche (recette) qui a ete executee ; RESTRICT car un historique de production ne doit pas etre orphelin
    CONSTRAINT fk_bp_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Empeche une production a zero ou negative
    CONSTRAINT chk_bomprod_qte_positive
        CHECK (quantite_produite > 0)
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
    -- Denormalisation volontaire : niveau, contexte et activite sont stockes
    -- pour permettre des filtres rapides sans jointures dans la vue stock global.
    CONSTRAINT fk_bs_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bs_activite
        FOREIGN KEY (id_activite) REFERENCES activites(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Fiche qui a produit ce stock (permet de connaitre le nom du produit fabrique)
    CONSTRAINT fk_bs_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Production d'origine (tracabilite : quel acte de fabrication a cree ce lot)
    CONSTRAINT fk_bs_production
        FOREIGN KEY (id_production) REFERENCES bom_productions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Empeche un stock BOM negatif
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
    -- Production parente ; CASCADE car les lignes n'ont pas de sens sans la production
    CONSTRAINT fk_bpl_production
        FOREIGN KEY (id_production) REFERENCES bom_productions(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Si source = 'lot_ingredient' : lot de matiere premiere consomme (FIFO)
    CONSTRAINT fk_bpl_lot
        FOREIGN KEY (id_lot_ingredient) REFERENCES lots_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Si source = 'bom_stock' : stock de produit fabrique consomme (sous-recette)
    CONSTRAINT fk_bpl_stock
        FOREIGN KEY (id_bom_stock) REFERENCES bom_stocks(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Exclusion mutuelle : une ligne consomme SOIT un lot ingredient SOIT un stock BOM
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
    -- Lot dont une quantite est reservee pour une production future
    CONSTRAINT fk_br_lot
        FOREIGN KEY (id_lot) REFERENCES lots_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Contexte de production qui a pose la reservation
    CONSTRAINT fk_br_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Empeche une reservation a zero ou negative
    CONSTRAINT chk_bomres_qte_positive
        CHECK (quantite_reservee > 0)
) ENGINE=InnoDB;

-- ============================================================
-- MODULE : BOUTIQUE WEB (e-commerce Laravel)
-- Categories → Produits (lies a une fiche BOM) → Commandes → Lignes.
-- Les clients web ont leur propre table (separee de utilisateurs)
-- car le site Laravel gere son authentification independamment.
-- Les prix sont TTC cote boutique (contrairement aux prix HTVA des lots).
-- ============================================================

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
    -- Lien vers la fiche BOM dont ce produit est la version commerciale (1:1)
    CONSTRAINT fk_prodweb_bomfiche
        FOREIGN KEY (id_bom_fiche) REFERENCES bom_fiches(id) ON DELETE RESTRICT,
    -- Categorie d'affichage sur le site ; mise a NULL si la categorie est supprimee
    CONSTRAINT fk_prodweb_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories_web(id) ON DELETE SET NULL,
    -- Un produit web correspond a exactement une fiche BOM (pas de doublons)
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
    -- Un client ne peut pas etre supprime s'il a des commandes (RESTRICT = protection historique)
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
    -- Les lignes suivent le cycle de vie de la commande (CASCADE)
    CONSTRAINT fk_cmdligne_cmd
        FOREIGN KEY (id_commande) REFERENCES commandes_web(id) ON DELETE CASCADE,
    -- Un produit ne peut pas etre supprime s'il est reference dans une commande
    CONSTRAINT fk_cmdligne_prodweb
        FOREIGN KEY (id_produit_web) REFERENCES produits_web(id) ON DELETE RESTRICT,
    -- Quantite minimale = 1 (pas de ligne a zero article)
    CONSTRAINT chk_cmdligne_qte_positive
        CHECK (quantite >= 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Index de performance (v15)
-- Optimisent les requetes les plus frequentes du site Laravel :
-- catalogue (en_vente + tri), filtrage par categorie, panier client.
-- ============================================================
CREATE INDEX idx_prodweb_envente ON produits_web (en_vente, ordre_affichage);
CREATE INDEX idx_prodweb_categorie ON produits_web (id_categorie);
CREATE INDEX idx_cmdweb_client_statut ON commandes_web (id_client, statut);
CREATE INDEX idx_cmdweb_statut ON commandes_web (statut);
CREATE INDEX idx_cmdligne_commande ON commandes_web_lignes (id_commande);

-- Index v20 — Durcissement schema (optimisation requetes frequentes)
CREATE INDEX idx_lot_fiche_achat ON lots_ingredients (id_fiche_ingredient, date_achat);
CREATE INDEX idx_bomres_lot_actif ON bom_reservations (id_lot, actif);
CREATE INDEX idx_bomres_ctx_actif ON bom_reservations (id_contexte, actif);
CREATE INDEX idx_bomstock_fiche_dispo ON bom_stocks (id_fiche, quantite_disponible, date_production);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- VIEW : vue_stock_global (v11, maj v17, maj v18)
-- ============================================================
-- Vue unifiee de TOUT le stock de l'atelier, qu'il s'agisse
-- de matieres premieres achetees ou de produits fabriques en interne.
--
-- Structure UNION ALL en deux parties :
--   PARTIE 1 — Lots ingredients (matieres premieres) :
--     Source = lots_ingredients + fiches_ingredients + stocks.
--     Inclut le calcul des reservations actives (LEFT JOIN bom_reservations)
--     pour obtenir la quantite_dispo_reelle = disponible - reservee.
--
--   PARTIE 2 — Produits fabriques (output BOM) :
--     Source = bom_stocks + bom_fiches.
--     Pas de reservations (quantite_reservee = 0 en dur).
--
-- Les colonnes sont alignees pour que les deux parties aient le meme schema.
-- Les colonnes non applicables sont remplies par NULL (ex: id_stock pour les produits fabriques).
-- ============================================================
CREATE OR REPLACE VIEW vue_stock_global AS

    -- PARTIE 1 : Matieres premieres (lots achetes)
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

    -- PARTIE 2 : Produits fabriques (output des productions BOM)
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
-- FIN — 20 tables + 1 VIEW + 8 CHECK constraints + 2 UNIQUE keys supplementaires
-- ============================================================
