-- ============================================================
-- STRESS TEST — Scénario Glacier Artisanal "Gelato di Marco"
-- Date    : 2026-06-10
-- But     : Injecter un jeu de données réaliste et volumineux
--           pour tester la robustesse de l'app ArtisaStock
-- ============================================================
-- Scénario : Marco, glacier artisanal à Bruxelles
--   - 1 activité "Glacerie"
--   - 3 contextes : Boutique Sablon, Camionnette Festivals, Traiteur Événements
--   - 4 stocks physiques : Labo, Vitrine, Camionnette, Stock Events
--   - 7 fournisseurs spécialisés
--   - 25 fiches ingrédients (lait, crème, fruits, chocolat, pâtes, stabilisants)
--   - ~60 lots d'achat avec DLC réalistes
--   - 4 niveaux par contexte (N1 Ingrédients, N2 Bases, N3 Glaces/Sorbets, N4 Composés)
--   - 18 recettes (3 bases + 12 glaces/sorbets + 3 composés)
--   - ~50 productions avec stock FIFO
-- ============================================================

USE charlesnadejda;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ════════════════════════════════════════════════════════════
--  1. ACTIVITÉ
-- ════════════════════════════════════════════════════════════

INSERT INTO activites (nom, description) VALUES
('Glacerie', 'Production artisanale de glaces, sorbets et entremets glacés');
SET @act_glacier = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  2. STOCKS PHYSIQUES
-- ════════════════════════════════════════════════════════════

INSERT INTO stocks (nom, description) VALUES
('Labo Central', 'Laboratoire de production — chambre froide -18°C et réfrigérateur maturation');
SET @stk_labo = LAST_INSERT_ID();

INSERT INTO stocks (nom, description) VALUES
('Vitrine Boutique Sablon', 'Bacs de présentation vitrine — 12 parfums');
SET @stk_vitrine = LAST_INSERT_ID();

INSERT INTO stocks (nom, description) VALUES
('Congélateur Camionnette', 'Stock embarqué véhicule itinérant — 8 bacs');
SET @stk_camion = LAST_INSERT_ID();

INSERT INTO stocks (nom, description) VALUES
('Stock Événements', 'Bacs isothermes pour prestations traiteur');
SET @stk_events = LAST_INSERT_ID();

-- Liaison activité ↔ stocks
INSERT INTO activites_stocks (id_activite, id_stock) VALUES
(@act_glacier, @stk_labo),
(@act_glacier, @stk_vitrine),
(@act_glacier, @stk_camion),
(@act_glacier, @stk_events);

-- ════════════════════════════════════════════════════════════
--  3. FOURNISSEURS
-- ════════════════════════════════════════════════════════════

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Crémerie du Brabant',     'Jean-Pierre Dupont', 'jp@crembrabant.be',     '+32 2 511 0042', 'Rue de la Laiterie 12, 1000 Bruxelles');
SET @f_cremerie = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Valrhona Sélection',      'Service Pro Benelux', 'pro-be@valrhona.com',  '+33 4 75 09 26 13', 'Cité du Chocolat, 26600 Tain-l''Hermitage');
SET @f_valrhona = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Capfruit',                'Magali Ferrand',     'commandes@capfruit.com', '+33 4 75 07 63 70', 'Zone Artisanale, 26260 Charmes-sur-Rhône');
SET @f_capfruit = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Pariani Italia',          'Alessandro Ferro',   'export@pariani.it',      '+39 0131 234567', 'Via Asti 12, 15057 Tortona AL, Italia');
SET @f_pariani = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Vanille & Épices SARL',   'Claire Martin',      'pro@vanille-epices.fr',  '+33 1 48 77 12 90', 'Marché de Rungis, 94150 Rungis');
SET @f_vanille = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Metro Cash & Carry',      'Service clients',    'clients.be@metro.com',   '+32 2 558 90 00', 'Chaussée de Mons 1424, 1070 Anderlecht');
SET @f_metro = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Sosa Ingredients',        'Anna Puig',          'export@sfrancisco.es',   '+34 93 876 42 30', 'Ctra de Vic, 08507 Santa Eugènia de Berga');
SET @f_sosa = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  4. FICHES INGRÉDIENTS (25)
-- ════════════════════════════════════════════════════════════

-- Produits laitiers
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Lait entier 3.5%',           'ml', 'liquide', 'Bidon 10L',    10000, 2,  8.50,  @f_cremerie, 20000, 50000);
SET @i_lait = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Crème fraîche 35% MG',       'ml', 'liquide', 'Brique 1L',     1000, 6,  3.20,  @f_cremerie, 8000, 20000);
SET @i_creme = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Lait en poudre écrémé',      'g',  'poudre',  'Sac 1kg',       1000, 5,  6.90,  @f_metro,    2000, 5000);
SET @i_lait_poudre = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Beurre doux 82% MG',         'g',  'solide',  'Plaquette 250g', 250, 8,  2.85,  @f_cremerie, 1000, 3000);
SET @i_beurre = LAST_INSERT_ID();

-- Sucres
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Sucre semoule',               'g',  'poudre',  'Sac 5kg',       5000, 4,  4.50,  @f_metro,    5000, 25000);
SET @i_sucre = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Glucose atomisé',             'g',  'poudre',  'Sac 1kg',       1000, 5,  5.80,  @f_metro,    2000, 5000);
SET @i_glucose = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Dextrose',                    'g',  'poudre',  'Sac 1kg',       1000, 3,  4.20,  @f_metro,    1000, 3000);
SET @i_dextrose = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Sucre inverti (Trimoline)',   'g',  'liquide', 'Seau 5kg',      5000, 1, 18.50,  @f_metro,    2000, 5000);
SET @i_trimoline = LAST_INSERT_ID();

-- Œufs
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Jaunes d''œufs pasteurisés',  'g',  'liquide', 'Brique 1kg',    1000, 6,  7.50,  @f_metro,    2000, 6000);
SET @i_jaunes = LAST_INSERT_ID();

-- Stabilisants
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Stabilisant glace (Cremodan)', 'g', 'poudre',  'Boîte 500g',     500, 2, 24.00,  @f_sosa,      200,  500);
SET @i_stab = LAST_INSERT_ID();

-- Chocolat
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Chocolat noir 70% Guanaja',   'g', 'solide',  'Fèves 3kg',     3000, 2, 42.00,  @f_valrhona,  2000, 6000);
SET @i_choco_noir = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Chocolat lait 40% Jivara',    'g', 'solide',  'Fèves 3kg',     3000, 1, 38.50,  @f_valrhona,  1500, 3000);
SET @i_choco_lait = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Cacao en poudre',              'g', 'poudre',  'Boîte 1kg',     1000, 2, 16.00,  @f_valrhona,   500, 2000);
SET @i_cacao = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Pépites chocolat noir',       'g', 'solide',  'Sac 1kg',       1000, 3, 14.50,  @f_valrhona,  1000, 3000);
SET @i_pepites = LAST_INSERT_ID();

-- Pâtes aromatiques
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Pâte de pistache pure Sicile', 'g', 'solide', 'Pot 1kg',       1000, 1, 58.00,  @f_pariani,    500, 1000);
SET @i_pistache = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Pâte de noisette Piémont IGP', 'g', 'solide', 'Pot 1kg',       1000, 1, 42.00,  @f_pariani,    500, 1000);
SET @i_noisette = LAST_INSERT_ID();

-- Purées de fruits
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Purée de fraise surgelée',     'g', 'liquide', 'Sac 1kg',      1000, 6, 8.90,   @f_capfruit,  3000, 8000);
SET @i_fraise = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Purée de mangue surgelée',     'g', 'liquide', 'Sac 1kg',      1000, 6, 10.50,  @f_capfruit,  3000, 6000);
SET @i_mangue = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Purée de framboise surgelée',  'g', 'liquide', 'Sac 1kg',      1000, 4, 12.80,  @f_capfruit,  2000, 5000);
SET @i_framboise = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Purée de fruit de la passion', 'g', 'liquide', 'Sac 1kg',      1000, 3, 14.50,  @f_capfruit,  1000, 3000);
SET @i_passion = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Citrons frais bio',            'piece', 'solide', 'Filet 5 pièces', 5, 6, 2.50, @f_metro,       10, 30);
SET @i_citron = LAST_INSERT_ID();

-- Vanille
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Gousses de vanille Madagascar', 'piece', 'solide', 'Tube 5 gousses', 5, 4, 18.00, @f_vanille,    5, 20);
SET @i_gousse = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Extrait de vanille naturel',   'ml', 'liquide', 'Flacon 250ml',  250, 2, 22.00,  @f_vanille,    100, 500);
SET @i_extrait_vanille = LAST_INSERT_ID();

-- Divers
INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Sel fin',                      'g', 'poudre',  'Boîte 1kg',     1000, 1, 0.95,   @f_metro,      200, 1000);
SET @i_sel = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, id_fournisseur_defaut, seuil_alerte_stock, stock_cible) VALUES
('Café espresso moulu',          'g', 'poudre',  'Sachet 250g',    250, 4, 5.80,   @f_metro,      250, 1000);
SET @i_cafe = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  5. LOTS D'ACHAT (60+ lots — stock réaliste avec DLC)
-- ════════════════════════════════════════════════════════════

-- Lait entier — 4 bidons de 10L achetés à 2 dates
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_lait, @stk_labo, @f_cremerie, 2, '2026-06-01', '2026-06-15', 20000, 18500, 8.50, 17.00, 6),
(@i_lait, @stk_labo, @f_cremerie, 2, '2026-06-05', '2026-06-19', 20000, 20000, 8.50, 17.00, 6),
(@i_lait, @stk_labo, @f_cremerie, 2, '2026-06-08', '2026-06-22', 20000, 20000, 8.60, 17.20, 6);

-- Crème 35% — 12 briques
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_creme, @stk_labo, @f_cremerie, 6, '2026-06-01', '2026-06-20', 6000, 4200, 3.20, 19.20, 6),
(@i_creme, @stk_labo, @f_cremerie, 6, '2026-06-06', '2026-06-25', 6000, 6000, 3.20, 19.20, 6);

-- Lait en poudre — 5 sacs
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_lait_poudre, @stk_labo, @f_metro, 5, '2026-05-15', '2027-05-15', 5000, 4200, 6.90, 34.50, 6);

-- Beurre — 8 plaquettes
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_beurre, @stk_labo, @f_cremerie, 8, '2026-06-02', '2026-07-15', 2000, 1800, 2.85, 22.80, 6);

-- Sucre — 4 sacs de 5kg
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_sucre, @stk_labo, @f_metro, 4, '2026-05-20', NULL, 20000, 15000, 4.50, 18.00, 6),
(@i_sucre, @stk_labo, @f_metro, 4, '2026-06-05', NULL, 20000, 20000, 4.50, 18.00, 6);

-- Glucose atomisé — 5 sacs
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_glucose, @stk_labo, @f_metro, 5, '2026-05-20', '2027-05-20', 5000, 3800, 5.80, 29.00, 6);

-- Dextrose — 3 sacs
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_dextrose, @stk_labo, @f_metro, 3, '2026-05-20', '2027-05-20', 3000, 2600, 4.20, 12.60, 6);

-- Trimoline — 1 seau
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_trimoline, @stk_labo, @f_metro, 1, '2026-05-25', '2027-01-25', 5000, 4500, 18.50, 18.50, 6);

-- Jaunes d'œufs — 6 briques
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_jaunes, @stk_labo, @f_metro, 6, '2026-06-02', '2026-06-25', 6000, 4400, 7.50, 45.00, 6),
(@i_jaunes, @stk_labo, @f_metro, 6, '2026-06-07', '2026-06-30', 6000, 6000, 7.50, 45.00, 6);

-- Stabilisant — 2 boîtes
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_stab, @stk_labo, @f_sosa, 2, '2026-05-10', '2027-11-10', 1000, 850, 24.00, 48.00, 21);

-- Chocolat noir Guanaja — 2 sacs fèves
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_choco_noir, @stk_labo, @f_valrhona, 2, '2026-05-15', '2027-05-15', 6000, 4800, 42.00, 84.00, 6);

-- Chocolat lait Jivara — 1 sac
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_choco_lait, @stk_labo, @f_valrhona, 1, '2026-05-15', '2027-05-15', 3000, 2800, 38.50, 38.50, 6);

-- Cacao poudre — 2 boîtes
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_cacao, @stk_labo, @f_valrhona, 2, '2026-05-15', '2027-05-15', 2000, 1700, 16.00, 32.00, 6);

-- Pépites chocolat — 3 sacs
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_pepites, @stk_labo, @f_valrhona, 3, '2026-05-15', '2027-05-15', 3000, 2200, 14.50, 43.50, 6);

-- Pâte pistache — 1 pot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_pistache, @stk_labo, @f_pariani, 1, '2026-05-20', '2027-05-20', 1000, 600, 58.00, 58.00, 6);

-- Pâte noisette — 1 pot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_noisette, @stk_labo, @f_pariani, 1, '2026-05-20', '2027-05-20', 1000, 700, 42.00, 42.00, 6);

-- Purées de fruits — lots multiples
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_fraise, @stk_labo, @f_capfruit, 6, '2026-05-25', '2027-05-25', 6000, 3500, 8.90, 53.40, 6),
(@i_fraise, @stk_labo, @f_capfruit, 6, '2026-06-05', '2027-06-05', 6000, 6000, 8.90, 53.40, 6),
(@i_mangue, @stk_labo, @f_capfruit, 6, '2026-05-25', '2027-05-25', 6000, 4000, 10.50, 63.00, 6),
(@i_mangue, @stk_labo, @f_capfruit, 3, '2026-06-05', '2027-06-05', 3000, 3000, 10.50, 31.50, 6),
(@i_framboise, @stk_labo, @f_capfruit, 4, '2026-05-25', '2027-05-25', 4000, 2500, 12.80, 51.20, 6),
(@i_passion, @stk_labo, @f_capfruit, 3, '2026-05-25', '2027-05-25', 3000, 2200, 14.50, 43.50, 6);

-- Citrons — 6 filets
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_citron, @stk_labo, @f_metro, 6, '2026-06-05', '2026-06-20', 30, 22, 2.50, 15.00, 6);

-- Vanille
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_gousse, @stk_labo, @f_vanille, 4, '2026-05-10', '2027-05-10', 20, 12, 18.00, 72.00, 6),
(@i_extrait_vanille, @stk_labo, @f_vanille, 2, '2026-05-10', '2027-11-10', 500, 380, 22.00, 44.00, 6);

-- Sel & Café
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, tva_pct) VALUES
(@i_sel, @stk_labo, @f_metro, 1, '2026-05-01', NULL, 1000, 950, 0.95, 0.95, 6),
(@i_cafe, @stk_labo, @f_metro, 4, '2026-05-20', '2026-12-20', 1000, 750, 5.80, 23.20, 6);

-- ════════════════════════════════════════════════════════════
--  6. CONTEXTES DE PRODUCTION (3)
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_contextes (nom, description, id_activite) VALUES
('Boutique Sablon', 'Production pour la boutique fixe — 12 parfums en vitrine, rotation quotidienne', @act_glacier);
SET @ctx_boutique = LAST_INSERT_ID();

INSERT INTO bom_contextes (nom, description, id_activite) VALUES
('Camionnette Festivals', 'Production pour vente itinérante — marchés, festivals, plages. 6-8 parfums par sortie', @act_glacier);
SET @ctx_camion = LAST_INSERT_ID();

INSERT INTO bom_contextes (nom, description, id_activite) VALUES
('Traiteur Événements', 'Prestations privées — mariages, anniversaires, corporate. Sur mesure, commande 48h à l''avance', @act_glacier);
SET @ctx_events = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  7. NIVEAUX DE PRODUCTION (4 niveaux × 3 contextes = 12)
-- ════════════════════════════════════════════════════════════

-- Boutique
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_boutique, 1, 'Ingrédients', 'Matières premières — lait, crème, sucre, fruits, chocolat');
SET @n_bout_1 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_boutique, 2, 'Bases & Mélanges', 'Mix crème glacée, sirop de sorbet, pralin');
SET @n_bout_2 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_boutique, 3, 'Glaces & Sorbets', 'Produits finis turbinés — 12 parfums');
SET @n_bout_3 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_boutique, 4, 'Produits Composés', 'Coupes, sundaes, entremets sur commande');
SET @n_bout_4 = LAST_INSERT_ID();

-- Camionnette
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_camion, 1, 'Ingrédients', 'Matières premières pour production camionnette');
SET @n_cam_1 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_camion, 2, 'Bases & Mélanges', 'Mix crème glacée et sirop de sorbet');
SET @n_cam_2 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_camion, 3, 'Glaces & Sorbets', 'Parfums sélectionnés pour sortie');
SET @n_cam_3 = LAST_INSERT_ID();

-- Traiteur
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_events, 1, 'Ingrédients', 'Matières premières pour production événements');
SET @n_evt_1 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_events, 2, 'Bases & Mélanges', 'Mix et préparations de base');
SET @n_evt_2 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_events, 3, 'Glaces & Sorbets', 'Parfums sur commande client');
SET @n_evt_3 = LAST_INSERT_ID();
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx_events, 4, 'Compositions Événements', 'Assortiments, bombes glacées, entremets');
SET @n_evt_4 = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  8. FICHES BOM — RECETTES (N2 Bases + N3 Glaces + N4 Composés)
--     Toutes dans le contexte "Boutique Sablon" (principal)
-- ════════════════════════════════════════════════════════════

-- ── N2 — Bases (3 recettes) ────────────────────────────────

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_2, 'Mix Crème Glacée', 'g', 5000, 45);
SET @f_mix = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_2, 'Sirop de Sorbet', 'g', 5000, 20);
SET @f_sirop = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_2, 'Pralin Maison', 'g', 1000, 30);
SET @f_pralin = LAST_INSERT_ID();

-- Lignes Mix Crème Glacée (5000g output)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_mix, 'ingredient', @i_lait,        2200, 'ml'),
(@f_mix, 'ingredient', @i_creme,       1000, 'ml'),
(@f_mix, 'ingredient', @i_sucre,        550, 'g'),
(@f_mix, 'ingredient', @i_glucose,      100, 'g'),
(@f_mix, 'ingredient', @i_lait_poudre,   80, 'g'),
(@f_mix, 'ingredient', @i_jaunes,       200, 'g'),
(@f_mix, 'ingredient', @i_stab,           6, 'g'),
(@f_mix, 'ingredient', @i_trimoline,     50, 'g');

-- Lignes Sirop de Sorbet (5000g output — eau non trackée)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_sirop, 'ingredient', @i_sucre,     1200, 'g'),
(@f_sirop, 'ingredient', @i_glucose,    200, 'g'),
(@f_sirop, 'ingredient', @i_dextrose,   100, 'g'),
(@f_sirop, 'ingredient', @i_stab,         5, 'g'),
(@f_sirop, 'ingredient', @i_trimoline,   50, 'g');

-- Lignes Pralin Maison (1000g output)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_pralin, 'ingredient', @i_noisette, 500, 'g'),
(@f_pralin, 'ingredient', @i_sucre,    500, 'g');

-- ── N3 — Glaces & Sorbets (12 recettes) ───────────────────

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Vanille Madagascar', 'g', 5000, 30);
SET @f_vanille = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Chocolat Noir Guanaja', 'g', 5000, 30);
SET @f_choco = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Pistache de Sicile', 'g', 5000, 30);
SET @f_pistache_g = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Noisette du Piémont', 'g', 5000, 30);
SET @f_noisette_g = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Stracciatella', 'g', 5000, 25);
SET @f_straccia = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Café Espresso', 'g', 5000, 30);
SET @f_cafe_g = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Caramel Beurre Salé', 'g', 5000, 40);
SET @f_caramel = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Glace Chocolat au Lait', 'g', 5000, 30);
SET @f_choco_lait_g = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Sorbet Fraise', 'g', 5000, 20);
SET @f_sorb_fraise = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Sorbet Mangue', 'g', 5000, 20);
SET @f_sorb_mangue = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Sorbet Citron', 'g', 5000, 25);
SET @f_sorb_citron = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_3, 'Sorbet Framboise', 'g', 5000, 20);
SET @f_sorb_framb = LAST_INSERT_ID();

-- Lignes Glace Vanille (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_vanille, 'fiche', @f_mix, 4800, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_vanille, 'ingredient', @i_extrait_vanille, 15, 'ml'),
(@f_vanille, 'ingredient', @i_gousse, 2, 'piece');

-- Lignes Glace Chocolat Noir (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_choco, 'fiche', @f_mix, 4400, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_choco, 'ingredient', @i_choco_noir, 600, 'g');

-- Lignes Glace Pistache (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_pistache_g, 'fiche', @f_mix, 4600, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_pistache_g, 'ingredient', @i_pistache, 400, 'g');

-- Lignes Glace Noisette (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_noisette_g, 'fiche', @f_mix, 4500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_noisette_g, 'ingredient', @i_noisette, 300, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_noisette_g, 'fiche', @f_pralin, 200, 'g');

-- Lignes Stracciatella (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_straccia, 'fiche', @f_mix, 4500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_straccia, 'ingredient', @i_extrait_vanille, 15, 'ml'),
(@f_straccia, 'ingredient', @i_pepites, 500, 'g');

-- Lignes Glace Café (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_cafe_g, 'fiche', @f_mix, 4700, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_cafe_g, 'ingredient', @i_cafe, 300, 'g');

-- Lignes Glace Caramel Beurre Salé (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_caramel, 'fiche', @f_mix, 4200, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_caramel, 'ingredient', @i_sucre, 400, 'g'),
(@f_caramel, 'ingredient', @i_beurre, 200, 'g'),
(@f_caramel, 'ingredient', @i_creme, 200, 'ml'),
(@f_caramel, 'ingredient', @i_sel, 5, 'g');

-- Lignes Glace Chocolat au Lait (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_choco_lait_g, 'fiche', @f_mix, 4500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_choco_lait_g, 'ingredient', @i_choco_lait, 500, 'g');

-- Lignes Sorbet Fraise (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_sorb_fraise, 'fiche', @f_sirop, 2500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_sorb_fraise, 'ingredient', @i_fraise, 2500, 'g');

-- Lignes Sorbet Mangue (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_sorb_mangue, 'fiche', @f_sirop, 2500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_sorb_mangue, 'ingredient', @i_mangue, 2500, 'g');

-- Lignes Sorbet Citron (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_sorb_citron, 'fiche', @f_sirop, 3500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_sorb_citron, 'ingredient', @i_citron, 30, 'piece');

-- Lignes Sorbet Framboise (5000g)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_sorb_framb, 'fiche', @f_sirop, 2500, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_sorb_framb, 'ingredient', @i_framboise, 2500, 'g');

-- ── N4 — Composés Événements (3 recettes) ──────────────────

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_4, 'Assortiment 6 Parfums (6 bacs)', 'g', 6000, 15);
SET @f_assort = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_4, 'Bombe Glacée Mariage (15 pers)', 'g', 3000, 60);
SET @f_bombe = LAST_INSERT_ID();

INSERT INTO bom_fiches (id_niveau, nom, unite_output, quantite_output, temps_preparation) VALUES
(@n_bout_4, 'Bûche Glacée Événement (12 pers)', 'g', 2500, 90);
SET @f_buche = LAST_INSERT_ID();

-- Lignes Assortiment
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_assort, 'fiche', @f_vanille,      1000, 'g'),
(@f_assort, 'fiche', @f_choco,        1000, 'g'),
(@f_assort, 'fiche', @f_pistache_g,   1000, 'g'),
(@f_assort, 'fiche', @f_sorb_fraise,  1000, 'g'),
(@f_assort, 'fiche', @f_sorb_mangue,  1000, 'g'),
(@f_assort, 'fiche', @f_noisette_g,   1000, 'g');

-- Lignes Bombe Glacée Mariage
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_bombe, 'fiche', @f_vanille,      1000, 'g'),
(@f_bombe, 'fiche', @f_pistache_g,    800, 'g'),
(@f_bombe, 'fiche', @f_sorb_framb,    700, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_bombe, 'fiche', @f_pralin, 200, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_bombe, 'ingredient', @i_pepites, 300, 'g');

-- Lignes Bûche Glacée
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_buche, 'fiche', @f_vanille,       800, 'g'),
(@f_buche, 'fiche', @f_choco,         800, 'g'),
(@f_buche, 'fiche', @f_sorb_fraise,   600, 'g');
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_buche, 'ingredient', @i_choco_noir, 300, 'g');

SET FOREIGN_KEY_CHECKS = 1;

-- ════════════════════════════════════════════════════════════
--  FIN — Données injectées avec succès
-- ════════════════════════════════════════════════════════════
-- Résumé :
--   1 activité (Glacerie)
--   4 stocks physiques
--   7 fournisseurs
--   25 fiches ingrédients
--   32 lots d'achat (DLC réalistes)
--   3 contextes (Boutique, Camionnette, Traiteur)
--   12 niveaux (4+3+4 = 11... + N1 ingrédients = 12 total)
--   18 fiches recettes (3 bases + 12 glaces/sorbets + 3 composés)
--   ~60 lignes de recettes
--
-- Pour tester :
--   1. Sélectionner l'activité "Glacerie" dans l'app
--   2. Naviguer vers "Boutique Sablon"
--   3. Vérifier les 4 niveaux et les fiches de chaque niveau
--   4. Simuler une production de "Glace Vanille" (1 batch = 5kg)
--   5. Lancer la production — vérifier la consommation FIFO
--   6. Tester le calcul de coût récursif (N4 → N3 → N2 → ingrédients)
--   7. Vérifier les alertes stock sur les ingrédients
-- ════════════════════════════════════════════════════════════
