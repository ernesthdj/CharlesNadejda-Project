-- ============================================================
-- Charles & Nadejda — Script de création BDD
-- Auteur  : Ernest
-- Version : 3.0 — refonte ArtisaStock (lots AFSCA, DLC, compositions)
-- ============================================================
-- Ordre de création (respecter les FK) :
--  1.  fournisseurs
--  2.  frais_generaux_config
--  3.  fiches_ingredients        (FK → fournisseurs)
--  4.  lots_ingredients          (FK → fiches_ingredients, fournisseurs)
--  5.  fiches_recettes
--  6.  recettes_ingredients      (FK → fiches_recettes, fiches_ingredients)
--  7.  categories
--  8.  parfums                   (FK → fiches_recettes nullable)
--  9.  fiches_compositions       (FK → categories)
-- 10.  compositions_recettes     (FK → fiches_compositions, fiches_recettes)
-- 11.  produits                  (FK → categories, fiches_compositions nullable)
-- 12.  produits_parfums          (FK → produits, parfums)
-- 13.  utilisateurs
-- 14.  commandes                 (FK → utilisateurs)
-- 15.  factures                  (FK → commandes)
-- 16.  zones_livraison
-- 17.  lignes_commandes          (FK → commandes, produits)
-- 18.  selections_parfums        (FK → lignes_commandes, parfums)
-- 19.  contacts
-- 20.  productions_recettes      (FK → fiches_recettes)
-- 21.  mouvements_lots_ingredients (FK → lots_ingredients, productions_recettes)
-- 22.  stock_produits            (FK → fiches_recettes, productions_recettes)
-- 23.  productions_compositions  (FK → fiches_compositions)
-- 24.  frais_production_variables (FK → productions_recettes, productions_compositions)
-- 25.  stock_compositions        (FK → fiches_compositions, productions_compositions)
-- 26.  mouvements_stock_produits (FK → stock_produits, productions_compositions, commandes)
-- ============================================================

CREATE DATABASE IF NOT EXISTS charlesnadejda
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. fournisseurs
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
-- 2. frais_generaux_config
-- Paramétrage mensuel des coûts fixes (élec, eau, usure matériel)
-- Alloués proportionnellement aux productions du mois dans l'app C#
-- ============================================================
CREATE TABLE IF NOT EXISTS frais_generaux_config (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    type             ENUM('electricite','eau','usure_materiel','autre') NOT NULL,
    libelle          VARCHAR(200) NOT NULL,
    valeur_mensuelle DECIMAL(10,2) NOT NULL,
    actif            TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ============================================================
-- 3. fiches_ingredients — "classe" ingrédient (master data)
-- ============================================================
CREATE TABLE IF NOT EXISTS fiches_ingredients (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    nom                   VARCHAR(200) NOT NULL UNIQUE,
    marque                VARCHAR(200),
    description           TEXT,
    unite_mesure          ENUM('kg','g','l','ml','cl','piece') NOT NULL,
    type_physique         ENUM('solide','liquide','poudre','piece') NOT NULL DEFAULT 'solide',
    densite               DECIMAL(8,4) DEFAULT NULL COMMENT 'g/ml — obligatoire si liquide ou poudre',
    activite              ENUM('chocolaterie','patisserie','partage') NOT NULL DEFAULT 'partage',
    prix_achat_reference  DECIMAL(10,4) NOT NULL DEFAULT 0,
    dlc_jours_reference   INT DEFAULT NULL,
    qualite_label         VARCHAR(100) DEFAULT NULL,
    id_fournisseur_defaut INT DEFAULT NULL,
    seuil_alerte_stock    DECIMAL(10,4) DEFAULT NULL,
    actif                 TINYINT(1) NOT NULL DEFAULT 1,
    date_creation         DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_fi_fournisseur
        FOREIGN KEY (id_fournisseur_defaut) REFERENCES fournisseurs(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 4. lots_ingredients — "objet" lot acheté (traçabilité AFSCA)
-- ============================================================
CREATE TABLE IF NOT EXISTS lots_ingredients (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche_ingredient INT NOT NULL,
    numero_lot          VARCHAR(100) DEFAULT NULL,   -- n° lot AFSCA fournisseur
    id_fournisseur      INT DEFAULT NULL,
    date_achat          DATE NOT NULL,
    date_peremption     DATE DEFAULT NULL,
    quantite_initiale   DECIMAL(10,4) NOT NULL,
    quantite_disponible DECIMAL(10,4) NOT NULL,
    prix_unitaire       DECIMAL(10,4) NOT NULL DEFAULT 0,
    prix_achat_reel     DECIMAL(10,4) NOT NULL DEFAULT 0,
    reference_facture   VARCHAR(100) DEFAULT NULL,
    notes               TEXT,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_lot_fiche
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_lot_fournisseur
        FOREIGN KEY (id_fournisseur) REFERENCES fournisseurs(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 5. fiches_recettes — "classe" recette
-- ============================================================
CREATE TABLE IF NOT EXISTS fiches_recettes (
    id                       INT AUTO_INCREMENT PRIMARY KEY,
    nom                      VARCHAR(200) NOT NULL UNIQUE,
    description              TEXT,
    type_rendement           ENUM('par_lot','par_unite') NOT NULL DEFAULT 'par_lot',
    rendement_quantite       DECIMAL(10,4) NOT NULL DEFAULT 1,
    poids_unitaire_indicatif DECIMAL(8,4) DEFAULT NULL, -- calculé auto par l'app
    poids_unitaire_reel      DECIMAL(8,4) DEFAULT NULL, -- validé manuellement
    conservation_jours       INT NOT NULL DEFAULT 30,   -- base calcul DLC pralines
    temps_preparation        INT DEFAULT NULL,           -- minutes
    actif                    TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ============================================================
-- 6. recettes_ingredients
-- ============================================================
CREATE TABLE IF NOT EXISTS recettes_ingredients (
    id_recette          INT NOT NULL,
    id_fiche_ingredient INT NOT NULL,
    quantite            DECIMAL(10,4) NOT NULL,
    PRIMARY KEY (id_recette, id_fiche_ingredient),
    CONSTRAINT fk_ri_recette
        FOREIGN KEY (id_recette) REFERENCES fiches_recettes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_ri_ingredient
        FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 7. categories
-- ============================================================
CREATE TABLE IF NOT EXISTS categories (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    nom              VARCHAR(100) NOT NULL UNIQUE,
    description      TEXT,
    ordre_affichage  INT NOT NULL DEFAULT 0
) ENGINE=InnoDB;

-- ============================================================
-- 8. parfums
-- id_recette : lien vers la fiche_recette qui produit ce type de praline
-- Permet au bon de commande de savoir quoi déduire du stock_produits
-- ============================================================
CREATE TABLE IF NOT EXISTS parfums (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255),
    type_parfum VARCHAR(50) DEFAULT NULL,
    couleur_hex VARCHAR(7) DEFAULT '#6F4E37',
    id_recette  INT DEFAULT NULL,
    disponible  TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT fk_parfum_recette
        FOREIGN KEY (id_recette) REFERENCES fiches_recettes(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 9. fiches_compositions — "classe" ballotin/sachet
-- ============================================================
CREATE TABLE IF NOT EXISTS fiches_compositions (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_categorie        INT NOT NULL,
    nom                 VARCHAR(200) NOT NULL,
    description         TEXT,
    type_composition    ENUM('fixe','configurable') NOT NULL,
    poids_cible_grammes DECIMAL(8,2) DEFAULT NULL,
    capacite_max        INT DEFAULT NULL,
    frais_emballage     DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    image_url           VARCHAR(500) DEFAULT NULL,
    afficher_stock_web  TINYINT(1) NOT NULL DEFAULT 0,
    mode_prix           ENUM('manuel','coefficient') NOT NULL DEFAULT 'manuel',
    coefficient_marge   DECIMAL(5,2) DEFAULT 1.00,
    prix_vente_ttc      DECIMAL(10,2) DEFAULT NULL,
    prix_calcule        DECIMAL(10,2) DEFAULT NULL,
    saisonnier          TINYINT(1) NOT NULL DEFAULT 0,
    saison_debut        VARCHAR(20) DEFAULT NULL,
    saison_fin          VARCHAR(20) DEFAULT NULL,
    actif               TINYINT(1) NOT NULL DEFAULT 1,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_fcomp_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 10. compositions_recettes — pour les compositions FIXES uniquement
-- ============================================================
CREATE TABLE IF NOT EXISTS compositions_recettes (
    id_composition INT NOT NULL,
    id_recette     INT NOT NULL,
    quantite       INT NOT NULL,
    PRIMARY KEY (id_composition, id_recette),
    CONSTRAINT fk_cr_composition
        FOREIGN KEY (id_composition) REFERENCES fiches_compositions(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_cr_recette
        FOREIGN KEY (id_recette) REFERENCES fiches_recettes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 11. produits — vitrine web (synchronisé depuis fiches_compositions)
-- ============================================================
CREATE TABLE IF NOT EXISTS produits (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    id_categorie   INT NOT NULL,
    id_composition INT DEFAULT NULL,
    nom            VARCHAR(200) NOT NULL,
    description    TEXT,
    prix_ttc       DECIMAL(10,2) NOT NULL,
    prix_htva      DECIMAL(10,2) GENERATED ALWAYS AS (ROUND(prix_ttc / 1.21, 2)) STORED,
    prix_promo     DECIMAL(10,2) DEFAULT NULL,
    stock          INT NOT NULL DEFAULT 0,
    afficher_stock TINYINT(1) NOT NULL DEFAULT 0,
    image_url      VARCHAR(500) DEFAULT NULL,
    configurable   TINYINT(1) NOT NULL DEFAULT 0,
    capacite_max   INT DEFAULT NULL,
    saisonnier     TINYINT(1) NOT NULL DEFAULT 0,
    saison_debut   VARCHAR(20) DEFAULT NULL,
    saison_fin     VARCHAR(20) DEFAULT NULL,
    disponible     TINYINT(1) NOT NULL DEFAULT 1,
    date_creation  DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_produit_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_produit_composition
        FOREIGN KEY (id_composition) REFERENCES fiches_compositions(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 12. produits_parfums
-- ============================================================
CREATE TABLE IF NOT EXISTS produits_parfums (
    id_produit INT NOT NULL,
    id_parfum  INT NOT NULL,
    PRIMARY KEY (id_produit, id_parfum),
    CONSTRAINT fk_pp_produit
        FOREIGN KEY (id_produit) REFERENCES produits(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_pp_parfum
        FOREIGN KEY (id_parfum) REFERENCES parfums(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 13. utilisateurs
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
-- 14. commandes
-- ============================================================
CREATE TABLE IF NOT EXISTS commandes (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    id_client             INT NOT NULL,
    date_commande         DATETIME DEFAULT CURRENT_TIMESTAMP,
    date_souhaitee        DATE NOT NULL,
    type_reception        ENUM('livraison','retrait') NOT NULL DEFAULT 'retrait',
    statut                ENUM('en_attente','confirmee','en_preparation','prete','livree','annulee') NOT NULL DEFAULT 'en_attente',
    statut_paiement       ENUM('en_attente','paye','rembourse','echec') NOT NULL DEFAULT 'en_attente',
    methode_paiement      ENUM('bancontact','carte','virement','especes') DEFAULT NULL,
    stripe_session_id     VARCHAR(255) DEFAULT NULL,
    stripe_payment_intent VARCHAR(255) DEFAULT NULL,
    frais_livraison       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    total_ttc             DECIMAL(10,2) NOT NULL,
    total_avec_livraison  DECIMAL(10,2) GENERATED ALWAYS AS (total_ttc + frais_livraison) STORED,
    adresse_livraison     VARCHAR(255),
    code_postal_livraison VARCHAR(10),
    ville_livraison       VARCHAR(100),
    pays_livraison        VARCHAR(50) DEFAULT 'Belgique',
    notes                 TEXT,
    notes_internes        TEXT,
    CONSTRAINT fk_commande_client
        FOREIGN KEY (id_client) REFERENCES utilisateurs(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 15. factures
-- ============================================================
CREATE TABLE IF NOT EXISTS factures (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    id_commande    INT NOT NULL,
    numero_facture VARCHAR(30) NOT NULL UNIQUE,
    date_emission  DATETIME DEFAULT CURRENT_TIMESTAMP,
    total_htva     DECIMAL(10,2) NOT NULL,
    montant_tva    DECIMAL(10,2) NOT NULL,
    total_ttc      DECIMAL(10,2) NOT NULL,
    pdf_path       VARCHAR(255) DEFAULT NULL,
    CONSTRAINT fk_facture_commande
        FOREIGN KEY (id_commande) REFERENCES commandes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 16. zones_livraison
-- ============================================================
CREATE TABLE IF NOT EXISTS zones_livraison (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(100) NOT NULL,
    pays_code       VARCHAR(5) NOT NULL,
    code_postal_min VARCHAR(10) DEFAULT NULL,
    code_postal_max VARCHAR(10) DEFAULT NULL,
    frais           DECIMAL(10,2) NOT NULL,
    delai_jours     INT NOT NULL DEFAULT 3,
    actif           TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ============================================================
-- 17. lignes_commandes
-- ============================================================
CREATE TABLE IF NOT EXISTS lignes_commandes (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    id_commande   INT NOT NULL,
    id_produit    INT NOT NULL,
    quantite      INT NOT NULL DEFAULT 1,
    prix_unitaire DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_ligne_commande
        FOREIGN KEY (id_commande) REFERENCES commandes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_ligne_produit
        FOREIGN KEY (id_produit) REFERENCES produits(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 18. selections_parfums
-- SUM(quantite) par id_ligne_commande doit = produits.capacite_max
-- ============================================================
CREATE TABLE IF NOT EXISTS selections_parfums (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_ligne_commande INT NOT NULL,
    id_parfum         INT NOT NULL,
    quantite          INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_selection_ligne
        FOREIGN KEY (id_ligne_commande) REFERENCES lignes_commandes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_selection_parfum
        FOREIGN KEY (id_parfum) REFERENCES parfums(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 19. contacts
-- ============================================================
CREATE TABLE IF NOT EXISTS contacts (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    nom        VARCHAR(100) NOT NULL,
    email      VARCHAR(255) NOT NULL,
    sujet      VARCHAR(200),
    message    TEXT NOT NULL,
    date_envoi DATETIME DEFAULT CURRENT_TIMESTAMP,
    lu         TINYINT(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB;

-- ============================================================
-- 20. productions_recettes — log d'exécution d'une recette
-- ============================================================
CREATE TABLE IF NOT EXISTS productions_recettes (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_recette          INT NOT NULL,
    date_production     DATETIME DEFAULT CURRENT_TIMESTAMP,
    quantite_produite   DECIMAL(10,4) NOT NULL,
    poids_lot_reel      DECIMAL(8,2) DEFAULT NULL,
    poids_unitaire_reel DECIMAL(8,4) DEFAULT NULL,
    cout_ingredients    DECIMAL(10,2) NOT NULL,
    frais_fixes_alloues DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_total          DECIMAL(10,2) NOT NULL,
    cout_unitaire       DECIMAL(10,4) NOT NULL,
    notes               TEXT,
    CONSTRAINT fk_prod_recette
        FOREIGN KEY (id_recette) REFERENCES fiches_recettes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 21. mouvements_lots_ingredients — traçabilité AFSCA modérée
-- ============================================================
CREATE TABLE IF NOT EXISTS mouvements_lots_ingredients (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    id_lot                INT NOT NULL,
    id_production_recette INT DEFAULT NULL,
    type_mouvement        ENUM('entree','sortie','ajustement') NOT NULL,
    quantite              DECIMAL(10,4) NOT NULL,
    prix_unitaire_moment  DECIMAL(10,4) NOT NULL,
    motif                 VARCHAR(255),
    reference             VARCHAR(255),
    date_mouvement        DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_mli_lot
        FOREIGN KEY (id_lot) REFERENCES lots_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_mli_production
        FOREIGN KEY (id_production_recette) REFERENCES productions_recettes(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 22. stock_produits — pralines disponibles (par lot de production)
-- ============================================================
CREATE TABLE IF NOT EXISTS stock_produits (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    id_recette            INT NOT NULL,
    id_production_recette INT NOT NULL,
    quantite_disponible   DECIMAL(10,4) NOT NULL,
    cout_unitaire         DECIMAL(10,4) NOT NULL,
    date_production       DATE NOT NULL,
    date_dlc              DATE NOT NULL,
    statut                ENUM('normal','alerte_dlc','critique','perime','promo') NOT NULL DEFAULT 'normal',
    prix_promo            DECIMAL(10,2) DEFAULT NULL,
    CONSTRAINT fk_sp_recette
        FOREIGN KEY (id_recette) REFERENCES fiches_recettes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_sp_production
        FOREIGN KEY (id_production_recette) REFERENCES productions_recettes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 23. productions_compositions — log d'assemblage d'une composition FIXE
-- ============================================================
CREATE TABLE IF NOT EXISTS productions_compositions (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_composition      INT NOT NULL,
    date_production     DATETIME DEFAULT CURRENT_TIMESTAMP,
    quantite_produite   INT NOT NULL,
    cout_produits       DECIMAL(10,2) NOT NULL,
    cout_emballage      DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    frais_fixes_alloues DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_total          DECIMAL(10,2) NOT NULL,
    cout_unitaire       DECIMAL(10,2) NOT NULL,
    notes               TEXT,
    CONSTRAINT fk_pcomp_composition
        FOREIGN KEY (id_composition) REFERENCES fiches_compositions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 24. frais_production_variables — coûts variables manuels par production
-- Un seul des deux FKs doit être renseigné
-- ============================================================
CREATE TABLE IF NOT EXISTS frais_production_variables (
    id                         INT AUTO_INCREMENT PRIMARY KEY,
    id_production_recette      INT DEFAULT NULL,
    id_production_composition  INT DEFAULT NULL,
    libelle                    VARCHAR(255) NOT NULL,
    montant                    DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_fpv_prod_recette
        FOREIGN KEY (id_production_recette) REFERENCES productions_recettes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_fpv_prod_composition
        FOREIGN KEY (id_production_composition) REFERENCES productions_compositions(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 25. stock_compositions — compositions FIXES disponibles
-- ============================================================
CREATE TABLE IF NOT EXISTS stock_compositions (
    id                        INT AUTO_INCREMENT PRIMARY KEY,
    id_composition            INT NOT NULL,
    id_production_composition INT NOT NULL,
    quantite_disponible       INT NOT NULL,
    cout_unitaire             DECIMAL(10,2) NOT NULL,
    date_production           DATE NOT NULL,
    date_dlc                  DATE DEFAULT NULL,
    statut                    ENUM('normal','alerte_dlc','critique','perime','promo') NOT NULL DEFAULT 'normal',
    prix_promo                DECIMAL(10,2) DEFAULT NULL,
    CONSTRAINT fk_sc_composition
        FOREIGN KEY (id_composition) REFERENCES fiches_compositions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_sc_production
        FOREIGN KEY (id_production_composition) REFERENCES productions_compositions(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 26. mouvements_stock_produits — sorties pralines
-- ============================================================
CREATE TABLE IF NOT EXISTS mouvements_stock_produits (
    id                        INT AUTO_INCREMENT PRIMARY KEY,
    id_stock_produit          INT NOT NULL,
    id_production_composition INT DEFAULT NULL,
    id_commande               INT DEFAULT NULL,
    type_mouvement            ENUM('entree','sortie','ajustement') NOT NULL,
    quantite                  DECIMAL(10,4) NOT NULL,
    motif                     VARCHAR(255),
    date_mouvement            DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_msp_stock
        FOREIGN KEY (id_stock_produit) REFERENCES stock_produits(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_msp_prod_comp
        FOREIGN KEY (id_production_composition) REFERENCES productions_compositions(id)
        ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT fk_msp_commande
        FOREIGN KEY (id_commande) REFERENCES commandes(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- MODULE PÂTISSERIE — 16 tables (préfixe pat_)
-- Ordre de création :
--  27. pat_formes
--  28. pat_gabarits            (FK → pat_formes)
--  29. pat_types_couche
--  30. pat_allergenes
--  31. pat_options_deco
--  32. pat_parametres
--  33. pat_ingredients_allergenes (FK → fiches_ingredients, pat_allergenes)
--  34. pat_recettes_couche     (FK → pat_types_couche, pat_gabarits)
--  35. pat_recettes_couche_ingr (FK → pat_recettes_couche, fiches_ingredients)
--  36. pat_fiches_gateau       (FK → pat_formes, pat_gabarits)
--  37. pat_fiches_gateau_couches (FK → pat_fiches_gateau, pat_recettes_couche)
--  38. pat_couches_dispo_config (FK → pat_fiches_gateau, pat_types_couche, pat_recettes_couche)
--  39. pat_commandes           (FK → commandes)
--  40. pat_etages              (FK → pat_commandes, pat_fiches_gateau, pat_gabarits, pat_options_deco)
--  41. pat_etages_couches      (FK → pat_etages, pat_recettes_couche)
--  42. pat_devis               (FK → pat_commandes)
-- ============================================================

-- ============================================================
-- 27. pat_formes — Formes de gâteaux configurables
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_formes (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    nom           VARCHAR(100) NOT NULL,
    type_calcul   ENUM('circulaire','rectangulaire','libre') NOT NULL DEFAULT 'circulaire',
    description   VARCHAR(255),
    actif         TINYINT(1) NOT NULL DEFAULT 1,
    created_at    DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 28. pat_gabarits — Tailles/dimensions par forme
-- volumes et surfaces calculés et stockés par ArtisaStock au save
-- circulaire : dimension_1_cm = rayon, dimension_2_cm = NULL
-- rectangulaire: dimension_1_cm = largeur, dimension_2_cm = longueur
-- libre       : valeurs saisies manuellement
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_gabarits (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    id_forme              INT NOT NULL,
    nom                   VARCHAR(100) NOT NULL,
    dimension_1_cm        DECIMAL(8,2) NOT NULL COMMENT 'Rayon (circulaire) ou Largeur (rect)',
    dimension_2_cm        DECIMAL(8,2) DEFAULT NULL COMMENT 'NULL pour circulaire, Longueur pour rect',
    hauteur_cm            DECIMAL(8,2) NOT NULL,
    volume_cm3            DECIMAL(12,4) DEFAULT NULL COMMENT 'Calculé par ArtisaStock',
    surface_dessus_cm2    DECIMAL(12,4) DEFAULT NULL COMMENT 'Calculé par ArtisaStock',
    surface_laterale_cm2  DECIMAL(12,4) DEFAULT NULL COMMENT 'Calculé par ArtisaStock',
    actif                 TINYINT(1) NOT NULL DEFAULT 1,
    created_at            DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_gab_forme
        FOREIGN KEY (id_forme) REFERENCES pat_formes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 29. pat_types_couche — Types de couches configurables
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_types_couche (
    id                   INT AUTO_INCREMENT PRIMARY KEY,
    nom                  VARCHAR(100) NOT NULL,
    mode_scaling_defaut  ENUM('volume','surface_dessus','surface_laterale','fixe','manuel')
                             NOT NULL DEFAULT 'volume',
    description          VARCHAR(255),
    actif                TINYINT(1) NOT NULL DEFAULT 1,
    created_at           DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 30. pat_allergenes — 14 allergènes EU + extensibles
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_allergenes (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    nom        VARCHAR(100) NOT NULL,
    code_eu    VARCHAR(10) DEFAULT NULL COMMENT 'Code règlement EU 1169/2011',
    actif      TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ============================================================
-- 31. pat_options_deco — Catalogue de décorations à thème
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_options_deco (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(150) NOT NULL,
    description TEXT,
    prix        DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    image_url   VARCHAR(500) DEFAULT NULL,
    actif       TINYINT(1) NOT NULL DEFAULT 1,
    created_at  DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 32. pat_parametres — Table clé/valeur pour paramètres artisan
-- Clés : taux_horaire, taux_charges, marge_cible,
--        supplement_mixte, tva_taux
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_parametres (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    cle         VARCHAR(100) NOT NULL UNIQUE,
    valeur      VARCHAR(500) DEFAULT NULL,
    description VARCHAR(255),
    updated_at  DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 33. pat_ingredients_allergenes — Liaison ingrédients ↔ allergènes
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_ingredients_allergenes (
    id_ingredient  INT NOT NULL,
    id_allergene   INT NOT NULL,
    PRIMARY KEY (id_ingredient, id_allergene),
    CONSTRAINT fk_ia_ingredient
        FOREIGN KEY (id_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_ia_allergene
        FOREIGN KEY (id_allergene) REFERENCES pat_allergenes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 34. pat_recettes_couche — Recettes de couches réutilisables
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_recettes_couche (
    id                   INT AUTO_INCREMENT PRIMARY KEY,
    nom                  VARCHAR(200) NOT NULL,
    id_type_couche       INT NOT NULL,
    id_gabarit_ref       INT NOT NULL COMMENT 'Gabarit pour lequel les quantités sont écrites',
    hauteur_ref_cm       DECIMAL(8,2) NOT NULL COMMENT 'Épaisseur de cette couche au gabarit de référence',
    mode_scaling         ENUM('volume','surface_dessus','surface_laterale','fixe','manuel')
                             DEFAULT NULL COMMENT 'NULL = utilise le défaut du type',
    temps_preparation_min INT NOT NULL DEFAULT 0,
    notes                TEXT,
    actif                TINYINT(1) NOT NULL DEFAULT 1,
    created_at           DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_rc_type
        FOREIGN KEY (id_type_couche) REFERENCES pat_types_couche(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_rc_gabarit
        FOREIGN KEY (id_gabarit_ref) REFERENCES pat_gabarits(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 35. pat_recettes_couche_ingr — Ingrédients par recette de couche
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_recettes_couche_ingr (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_recette_couche INT NOT NULL,
    id_ingredient     INT NOT NULL COMMENT 'FK → fiches_ingredients (partagé choco+patisserie)',
    quantite_ref      DECIMAL(10,4) NOT NULL,
    unite             VARCHAR(20) NOT NULL,
    CONSTRAINT fk_rci_recette
        FOREIGN KEY (id_recette_couche) REFERENCES pat_recettes_couche(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_rci_ingredient
        FOREIGN KEY (id_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 36. pat_fiches_gateau — Templates de gâteaux
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_fiches_gateau (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(200) NOT NULL,
    description     TEXT,
    image_url       VARCHAR(500) DEFAULT NULL,
    id_forme        INT NOT NULL,
    id_gabarit_ref  INT NOT NULL COMMENT 'Gabarit de référence pour ce gâteau',
    mode            ENUM('classique','configurable') NOT NULL DEFAULT 'classique',
    disponible_site TINYINT(1) NOT NULL DEFAULT 0,
    actif           TINYINT(1) NOT NULL DEFAULT 1,
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_fg_forme
        FOREIGN KEY (id_forme) REFERENCES pat_formes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_fg_gabarit
        FOREIGN KEY (id_gabarit_ref) REFERENCES pat_gabarits(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 37. pat_fiches_gateau_couches — Couches d'un gâteau classique (ordonnées)
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_fiches_gateau_couches (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche_gateau   INT NOT NULL,
    id_recette_couche INT NOT NULL,
    ordre             INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_fgc_fiche
        FOREIGN KEY (id_fiche_gateau) REFERENCES pat_fiches_gateau(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_fgc_recette
        FOREIGN KEY (id_recette_couche) REFERENCES pat_recettes_couche(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 38. pat_couches_dispo_config — Couches disponibles pour le configurateur
-- (mode configurable uniquement : quelles recettes le client peut choisir)
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_couches_dispo_config (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche_gateau   INT NOT NULL,
    id_type_couche    INT NOT NULL,
    id_recette_couche INT NOT NULL,
    actif             TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT fk_cdc_fiche
        FOREIGN KEY (id_fiche_gateau) REFERENCES pat_fiches_gateau(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_cdc_type
        FOREIGN KEY (id_type_couche) REFERENCES pat_types_couche(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_cdc_recette
        FOREIGN KEY (id_recette_couche) REFERENCES pat_recettes_couche(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 39. pat_commandes — Commandes de gâteaux (liées à commandes)
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_commandes (
    id                        INT AUTO_INCREMENT PRIMARY KEY,
    id_commande               INT NOT NULL COMMENT 'FK → commandes (table boutique)',
    type                      ENUM('simple','piece_montee') NOT NULL DEFAULT 'simple',
    supplement_mixte_applique TINYINT(1) NOT NULL DEFAULT 0,
    notes_artisan             TEXT,
    statut_production         ENUM('en_attente','en_preparation','pret','livre')
                                  NOT NULL DEFAULT 'en_attente',
    date_souhaitee            DATE DEFAULT NULL,
    created_at                DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_pc_commande
        FOREIGN KEY (id_commande) REFERENCES commandes(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 40. pat_etages — Étages d'une commande (1 pour simple, N pour pièce montée)
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_etages (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    id_pat_commande  INT NOT NULL,
    ordre            INT NOT NULL DEFAULT 1,
    id_fiche_gateau  INT NOT NULL,
    id_gabarit       INT NOT NULL COMMENT 'Gabarit choisi par le client',
    id_option_deco   INT DEFAULT NULL COMMENT 'NULL = décoration basique incluse',
    notes            TEXT,
    CONSTRAINT fk_et_commande
        FOREIGN KEY (id_pat_commande) REFERENCES pat_commandes(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_et_fiche
        FOREIGN KEY (id_fiche_gateau) REFERENCES pat_fiches_gateau(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_et_gabarit
        FOREIGN KEY (id_gabarit) REFERENCES pat_gabarits(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_et_deco
        FOREIGN KEY (id_option_deco) REFERENCES pat_options_deco(id)
        ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 41. pat_etages_couches — Couches choisies par le client (mode configurable)
-- (vide pour les gâteaux classiques — couches lues depuis fiches_gateau_couches)
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_etages_couches (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_etage          INT NOT NULL,
    id_recette_couche INT NOT NULL,
    ordre             INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_ec_etage
        FOREIGN KEY (id_etage) REFERENCES pat_etages(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_ec_recette
        FOREIGN KEY (id_recette_couche) REFERENCES pat_recettes_couche(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 42. pat_devis — Devis calculé par ArtisaStock pour chaque commande
-- ============================================================
CREATE TABLE IF NOT EXISTS pat_devis (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_pat_commande     INT NOT NULL UNIQUE,
    cout_matieres       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_main_oeuvre    DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_charges        DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_options_deco   DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    supplement_mixte    DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    total_cout          DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    prix_suggere        DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    prix_final          DECIMAL(10,2) DEFAULT NULL COMMENT 'Ajusté par artisane avant validation',
    notes_ajustement    TEXT,
    date_creation       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_dv_commande
        FOREIGN KEY (id_pat_commande) REFERENCES pat_commandes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- MODULE BOM — Bill of Materials Générique Multi-Niveaux
-- Applicable chocolaterie ET pâtisserie via le même moteur.
-- Ordre de création :
--  43. bom_contextes
--  44. bom_niveaux           (FK → bom_contextes)
--  45. bom_fiches
--  46. bom_fiches_lignes     (FK → bom_fiches, fiches_ingredients)
--  47. bom_productions       (FK → bom_niveaux, bom_fiches)
--  48. bom_stocks            (FK → bom_niveaux, bom_fiches, bom_productions)
--  49. bom_productions_lignes (FK → bom_productions, lots_ingredients, bom_stocks)
--  50. bom_reservations      (FK → lots_ingredients, bom_contextes)
-- ============================================================

-- ============================================================
-- 43. bom_contextes — Configurations de production nommées
-- Chaque contexte possède ses propres niveaux et son propre stock.
-- Il puise dans le stock global d'ingrédients (lots_ingredients).
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_contextes (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    nom          VARCHAR(200) NOT NULL,
    description  TEXT,
    activite     ENUM('chocolaterie','patisserie') NOT NULL,
    actif        TINYINT(1) NOT NULL DEFAULT 1,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 44. bom_niveaux — Niveaux de transformation dans un contexte
-- ordre : 1, 2, 3... (croissant)
-- Suppression uniquement si MAX(ordre) du contexte = cet ordre.
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
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 45. bom_fiches — Templates de recettes globaux et réutilisables
-- Une fiche produit `quantite_output` unités par exécution.
-- Niveau d'appartenance = déterminé par le contexte à l'usage,
-- pas stocké dans la fiche elle-même (fiche globale).
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_fiches (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    nom              VARCHAR(200) NOT NULL UNIQUE,
    description      TEXT,
    activite         ENUM('chocolaterie','patisserie','partage') NOT NULL DEFAULT 'partage',
    unite_output     ENUM('kg','g','l','ml','cl','piece') NOT NULL DEFAULT 'piece',
    quantite_output  DECIMAL(10,4) NOT NULL DEFAULT 1
                         COMMENT 'Quantité produite par une exécution (ex: 1 lot = 24 pièces)',
    temps_preparation INT DEFAULT NULL COMMENT 'Minutes estimées',
    actif            TINYINT(1) NOT NULL DEFAULT 1,
    date_creation    DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================
-- 46. bom_fiches_lignes — Composition d'une fiche (inputs)
-- type_input='ingredient' → id_input_ingredient pointe fiches_ingredients
-- type_input='fiche'      → id_input_fiche      pointe bom_fiches (niveau N-1)
-- La règle stricte (N consomme N-1 uniquement) est enforced par l'app.
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_fiches_lignes (
    id                    INT AUTO_INCREMENT PRIMARY KEY,
    id_fiche              INT NOT NULL,
    type_input            ENUM('ingredient','fiche') NOT NULL,
    id_input_ingredient   INT DEFAULT NULL,
    id_input_fiche        INT DEFAULT NULL,
    quantite              DECIMAL(12,4) NOT NULL,
    unite_mesure          ENUM('kg','g','l','ml','cl','piece') NOT NULL,
    CONSTRAINT fk_bfl_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_ingredient
        FOREIGN KEY (id_input_ingredient) REFERENCES fiches_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bfl_fiche_input
        FOREIGN KEY (id_input_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
    -- NOTE: la cohérence type_input ↔ id_input_* est enforced dans l'app C# (MySQL 8 interdit CHECK sur colonnes FK)
) ENGINE=InnoDB;

-- ============================================================
-- 47. bom_productions — Log d'exécution d'une fiche dans un contexte
-- Créé AVANT bom_stocks (bom_stocks.id_production FK → ici).
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_productions (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau        INT NOT NULL,
    id_fiche         INT NOT NULL,
    quantite_produite DECIMAL(10,4) NOT NULL,
    cout_ingredients DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    cout_unitaire    DECIMAL(10,4) NOT NULL DEFAULT 0.0000,
    date_production  DATETIME DEFAULT CURRENT_TIMESTAMP,
    notes            TEXT,
    CONSTRAINT fk_bp_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_bp_fiche
        FOREIGN KEY (id_fiche) REFERENCES bom_fiches(id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 48. bom_stocks — Stock par contexte × niveau × fiche
-- Un enregistrement = un lot de production (traçabilité).
-- quantite_disponible décrémentée à chaque consommation (niveau N+1).
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_stocks (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_niveau           INT NOT NULL,
    id_fiche            INT NOT NULL,
    id_production       INT NOT NULL COMMENT 'Production qui a créé ce stock',
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

-- ============================================================
-- 49. bom_productions_lignes — Traçabilité des consommations
-- Pour chaque production, détaille ce qui a été consommé et d'où.
-- type_source='lot_ingredient' → consommation depuis lots_ingredients (niveau 0)
-- type_source='bom_stock'      → consommation depuis bom_stocks (niveau N-1)
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
        ON DELETE RESTRICT ON UPDATE CASCADE
    -- NOTE: la cohérence type_source ↔ id_* est enforced dans l'app C# (MySQL 8 interdit CHECK sur colonnes FK)
) ENGINE=InnoDB;

-- ============================================================
-- 50. bom_reservations — Réservations sur le stock d'ingrédients
-- Permet à un contexte de réserver des ingrédients sans les consommer.
-- Disponible réel = lot.quantite_disponible - SUM(bom_reservations.quantite_reservee)
--                   WHERE id_lot = lot.id AND actif = 1
-- ============================================================
CREATE TABLE IF NOT EXISTS bom_reservations (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    id_lot           INT NOT NULL,
    id_contexte      INT NOT NULL,
    quantite_reservee DECIMAL(12,4) NOT NULL,
    date_reservation DATETIME DEFAULT CURRENT_TIMESTAMP,
    notes            TEXT,
    actif            TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT fk_br_lot
        FOREIGN KEY (id_lot) REFERENCES lots_ingredients(id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_br_contexte
        FOREIGN KEY (id_contexte) REFERENCES bom_contextes(id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN — Base prête, exécuter seed_data.sql ensuite
-- Tables totales : 50
--   26  chocolaterie/boutique
--   16  pâtisserie (préfixe pat_)
--    8  BOM générique (préfixe bom_)
-- ============================================================
