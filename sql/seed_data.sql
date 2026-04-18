-- ============================================================
-- Charles & Nadejda — Données de test (seed) v3
-- À exécuter APRÈS create_database.sql
-- ============================================================
-- Comptes de test (mot de passe = 'password' pour tous) :
--   Admin  : charles@charlesnadejda.be
--   Admin  : nadejda@charlesnadejda.be
--   Client : marie.dupont@test.be
-- ⚠️ CHANGER LES MOTS DE PASSE EN PRODUCTION
-- ============================================================

USE charlesnadejda;

-- ──────────────────────────────────────────────────────────────
-- FRAIS GÉNÉRAUX (config mensuelle artisan)
-- ──────────────────────────────────────────────────────────────
INSERT INTO frais_generaux_config (type, libelle, valeur_mensuelle) VALUES
('electricite',    'Consommation four et éclairage atelier', 85.00),
('eau',            'Eau atelier et nettoyage',               18.00),
('usure_materiel', 'Amortissement matériel chocolaterie',    45.00);

-- ──────────────────────────────────────────────────────────────
-- FOURNISSEURS
-- ──────────────────────────────────────────────────────────────
INSERT INTO fournisseurs (nom, contact, email, telephone) VALUES
('Barry Callebaut Belgique', 'Service Artisanat', 'artisans@callebaut.com', '+32 3 555 00 00'),
('Métro Bruxelles',          'Service Pro',       'pro@metro.be',           '+32 2 555 00 00');

-- ──────────────────────────────────────────────────────────────
-- FICHES INGRÉDIENTS (templates master data)
-- ──────────────────────────────────────────────────────────────
INSERT INTO fiches_ingredients (nom, marque, unite_mesure, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Chocolat noir 70%', 'Callebaut', 'kg',  12.50, 730, 'Grand Cru',   1, 1.000),
('Chocolat lait 33%', 'Callebaut', 'kg',  11.00, 730, 'Qualité Pro', 1, 1.000),
('Chocolat blanc',    'Callebaut', 'kg',  13.00, 730, 'Qualité Pro', 1, 0.500),
('Crème fraîche 35%', NULL,        'l',    2.80, 14,  NULL,          2, 0.500),
('Beurre doux AOP',   NULL,        'kg',   8.50, 60,  'AOP',         2, 0.500),
('Sucre fin',         NULL,        'kg',   1.20, 365, NULL,          2, 1.000),
('Praliné noisette',  'Callebaut', 'kg',  18.00, 365, 'Qualité Pro', 1, 0.500),
('Caramel liquide',   NULL,        'kg',   4.50, 180, NULL,          2, 0.300),
('Fleur de sel',      NULL,        'kg',   6.00, 730, 'Guérande',    2, 0.100),
('Jus d''orange bio', NULL,        'l',    3.50, 7,   'Bio',         2, 0.200);

-- ──────────────────────────────────────────────────────────────
-- LOTS D'ACHAT (traçabilité AFSCA)
-- ──────────────────────────────────────────────────────────────
INSERT INTO lots_ingredients (id_fiche_ingredient, numero_lot, id_fournisseur, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_achat_reel, reference_facture) VALUES
(1, 'CAL-2026-047', 1, '2026-03-01', '2028-03-01', 5.000, 4.800, 12.50, 'FAC-CAL-2026-089'),
(2, 'CAL-2026-048', 1, '2026-03-01', '2028-03-01', 4.000, 3.800, 11.00, 'FAC-CAL-2026-089'),
(3, 'CAL-2026-049', 1, '2026-03-01', '2028-03-01', 3.000, 3.000, 13.00, 'FAC-CAL-2026-089'),
(4, NULL,           2, '2026-03-20', '2026-04-03', 3.000, 2.950,  2.80, 'FAC-MET-2026-112'),
(5, NULL,           2, '2026-03-20', '2026-05-20', 2.000, 1.970,  8.50, 'FAC-MET-2026-112'),
(7, 'CAL-2026-031', 1, '2026-02-15', '2027-02-15', 2.000, 1.850, 17.80, 'FAC-CAL-2026-071'),
(8, NULL,           2, '2026-03-20', '2026-09-20', 1.500, 1.380,  4.50, 'FAC-MET-2026-112'),
(9, NULL,           2, '2026-03-20', '2028-03-20', 0.500, 0.495,  6.00, 'FAC-MET-2026-112');

-- ──────────────────────────────────────────────────────────────
-- FICHES RECETTES
-- conservation_jours : pralines bien tempérées ≈ 21-28 jours
-- ──────────────────────────────────────────────────────────────
INSERT INTO fiches_recettes (nom, description, type_rendement, rendement_quantite, conservation_jours, temps_preparation) VALUES
('Praline Praliné Noisette',
 'Ganache praliné noisette enrobée de chocolat au lait. Classique de la maison.',
 'par_lot', 20, 28, 45),
('Praline Caramel Fleur de Sel',
 'Caramel coulant au beurre salé avec fleur de sel de Guérande, enrobé chocolat noir.',
 'par_lot', 20, 28, 60),
('Truffe Champagne',
 'Ganache champagne rosé enrobée de chocolat blanc, roulée dans le cacao pur.',
 'par_lot', 24, 21, 30);

-- Composition recette 1 : Praline Praliné Noisette (pour 20 pièces)
INSERT INTO recettes_ingredients (id_recette, id_fiche_ingredient, quantite) VALUES
(1, 2, 0.200),  -- 200g chocolat lait (enrobage)
(1, 7, 0.150),  -- 150g praliné noisette (garniture)
(1, 5, 0.030),  -- 30g beurre
(1, 4, 0.050);  -- 50ml crème fraîche

-- Composition recette 2 : Caramel Fleur de Sel (pour 20 pièces)
INSERT INTO recettes_ingredients (id_recette, id_fiche_ingredient, quantite) VALUES
(2, 1, 0.200),  -- 200g chocolat noir (enrobage)
(2, 8, 0.120),  -- 120g caramel
(2, 5, 0.040),  -- 40g beurre
(2, 9, 0.005);  -- 5g fleur de sel

-- Composition recette 3 : Truffe Champagne (pour 24 pièces)
INSERT INTO recettes_ingredients (id_recette, id_fiche_ingredient, quantite) VALUES
(3, 3, 0.150),  -- 150g chocolat blanc
(3, 4, 0.080),  -- 80ml crème
(3, 5, 0.020);  -- 20g beurre

-- ──────────────────────────────────────────────────────────────
-- CATÉGORIES
-- ──────────────────────────────────────────────────────────────
INSERT INTO categories (nom, description, ordre_affichage) VALUES
('Ballotins',            'Assortiments de chocolats fins. Composition libre selon vos envies.',     1),
('Boules de Noël',       'Chocolats de Noël en boule décorative, disponibles octobre–janvier.',    2),
('Créations Originales', 'Pièces artistiques en chocolat faites à la main sur mesure.',            3),
('Pâtes à tartiner',     'Pâtes à tartiner artisanales au chocolat. 250g.',                        4),
('Pâtisseries',          'Gâteaux et entremets de saison (disponibles bientôt).',                  5);

-- ──────────────────────────────────────────────────────────────
-- PARFUMS (avec lien vers fiches_recettes pour déduction stock)
-- id_recette NULL = parfum sans recette liée (à compléter au fur et à mesure)
-- ──────────────────────────────────────────────────────────────
INSERT INTO parfums (nom, description, type_parfum, couleur_hex, id_recette) VALUES
('Praliné',       'Praliné noisette onctueux, la signature de la maison.',  'Classique',    '#C8860A', 1),
('Caramel',       'Caramel au beurre salé, texture coulante.',              'Classique',    '#D4A017', 2),
('Noir 70%',      'Chocolat noir grand cru, intense et légèrement amer.',   'Pur chocolat', '#1C1008', NULL),
('Lait',          'Chocolat au lait crémeux, douceur belge.',               'Pur chocolat', '#8B5E3C', NULL),
('Blanc',         'Chocolat blanc velouté, notes vanillées.',               'Pur chocolat', '#F5E6C8', NULL),
('Café',          'Ganache café arabica grand cru.',                        'Ganache',      '#3D1C02', NULL),
('Noisette',      'Praliné noisette torréfié, croquant.',                   'Ganache',      '#9C5A2D', NULL),
('Orange',        'Écorces d''orange confites, fraîcheur agrume.',          'Fruité',       '#E8791A', NULL),
('Framboise',     'Ganache framboise acidulée.',                            'Fruité',       '#C72C48', NULL),
('Citron',        'Ganache citron bergamote.',                              'Fruité',       '#F5D033', NULL),
('Passion',       'Ganache fruit de la passion, exotique et vif.',          'Fruité',       '#FF6B35', NULL),
('Rhum Raisin',   'Raisins Sultana macérés au rhum ambré.',                 'Alcoolisé',    '#6B1E1E', NULL),
('Champagne',     'Ganache champagne rosé, festif et élégant.',             'Alcoolisé',    '#E8D5A3', 3),
('Grand Marnier', 'Ganache Grand Marnier, agrumes et cognac.',              'Alcoolisé',    '#FF8C00', NULL),
('Fleur de sel',  'Caramel fleur de sel de Guérande, accord sucré-salé.',  'Original',     '#D4AF37', 2);

-- ──────────────────────────────────────────────────────────────
-- FICHES COMPOSITIONS
-- ──────────────────────────────────────────────────────────────
INSERT INTO fiches_compositions (id_categorie, nom, description, type_composition, poids_cible_grammes, capacite_max, frais_emballage, afficher_stock_web, mode_prix, prix_vente_ttc) VALUES
-- Configurables (client choisit ses parfums — pas de stock pré-assemblé)
(1, 'Ballotin 250g',
 'Coffret doré avec ruban satin chocolat. Composez librement vos 19 chocolats parmi 15 parfums.',
 'configurable', 250.00, 19, 1.20, 0, 'manuel', 13.00),
(1, 'Ballotin 500g',
 'Grand coffret doré avec ruban satin. Composez librement vos 39 chocolats parmi 15 parfums.',
 'configurable', 500.00, 39, 1.80, 0, 'manuel', 23.00),
(2, 'Boule de Noël 220g',
 'Boule décorative réutilisable. Garnie de 8 chocolats au choix. Oct–jan.',
 'configurable', 220.00, 8, 2.50, 0, 'manuel', 15.00),
(2, 'Boule de Noël 500g',
 'Grande boule décorative réutilisable. 19 chocolats au choix. Oct–jan.',
 'configurable', 500.00, 19, 3.00, 0, 'manuel', 29.00),
-- Fixe (pré-assemblé, stock affiché sur le site)
(1, 'Sachet Truffes Champagne (6 pcs)',
 'Sachet cadeau de 6 truffes au champagne rosé. Idéal pour les fêtes.',
 'fixe', 90.00, NULL, 0.80, 1, 'coefficient', 9.50);

-- Composition du Sachet Truffes (fixe) : 6 truffes champagne
INSERT INTO compositions_recettes (id_composition, id_recette, quantite) VALUES
(5, 3, 6);  -- 6 Truffes Champagne

-- ──────────────────────────────────────────────────────────────
-- PRODUITS VITRINE WEB (synchronisés depuis fiches_compositions)
-- ──────────────────────────────────────────────────────────────

-- Ballotins configurables
INSERT INTO produits (id_categorie, id_composition, nom, description, prix_ttc, stock, configurable, capacite_max, disponible) VALUES
(1, 1, 'Ballotin 250g',
 'Coffret doré avec ruban satin chocolat. Composez librement vos 19 chocolats parmi 15 parfums artisanaux.',
 13.00, 0, 1, 19, 1),
(1, 2, 'Ballotin 500g',
 'Grand coffret doré avec ruban satin. Composez librement vos 39 chocolats parmi 15 parfums artisanaux.',
 23.00, 0, 1, 39, 1);

-- Boules de Noël configurables
INSERT INTO produits (id_categorie, id_composition, nom, description, prix_ttc, stock, configurable, capacite_max, saisonnier, saison_debut, saison_fin, disponible) VALUES
(2, 3, 'Boule de Noël 220g',
 'Boule décorative réutilisable. Garnie de 8 chocolats au choix. Disponible oct–jan.',
 15.00, 0, 1, 8, 1, 'octobre', 'janvier', 1),
(2, 4, 'Boule de Noël 500g',
 'Grande boule décorative réutilisable. 19 chocolats au choix. Oct–jan.',
 29.00, 0, 1, 19, 1, 'octobre', 'janvier', 1);

-- Sachet Truffes (fixe, stock affiché)
INSERT INTO produits (id_categorie, id_composition, nom, description, prix_ttc, stock, afficher_stock, configurable, disponible) VALUES
(1, 5, 'Sachet Truffes Champagne (6 pcs)',
 'Sachet cadeau de 6 truffes au champagne rosé.',
 9.50, 0, 1, 0, 1);

-- Créations originales (sans lien ArtisaStock)
INSERT INTO produits (id_categorie, nom, description, prix_ttc, stock, configurable, disponible) VALUES
(3, 'Chaussure en chocolat',
 'Sculpture artisanale en chocolat noir belge. Pièce unique, idéal cadeau.',
 15.00, 5, 0, 1),
(3, 'Bouteille de Champagne en chocolat',
 'Réplique d''une bouteille sculptée en chocolat. Idéal célébration.',
 15.00, 5, 0, 1),
(4, 'La Pâte à tartiner',
 'Pâte à tartiner artisanale au chocolat noir belge. 250g. À conserver au frais.',
 10.00, 20, 0, 1);

-- Tous les produits configurables ↔ tous les parfums
INSERT INTO produits_parfums (id_produit, id_parfum)
SELECT p.id, f.id FROM produits p CROSS JOIN parfums f WHERE p.configurable = 1;

-- ──────────────────────────────────────────────────────────────
-- ZONES DE LIVRAISON
-- ──────────────────────────────────────────────────────────────
INSERT INTO zones_livraison (nom, pays_code, frais, delai_jours) VALUES
('Belgique',          'BE', 6.95, 3),
('Nord de la France', 'FR', 9.95, 4);

-- ──────────────────────────────────────────────────────────────
-- UTILISATEURS
-- Hash BCrypt de 'password' (généré par Laravel php artisan tinker)
-- ──────────────────────────────────────────────────────────────
INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role, telephone, ville) VALUES
('Artisan', 'Charles', 'charles@charlesnadejda.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'admin', '+32 2 123 45 67', 'Bruxelles'),
('Artisan', 'Nadejda', 'nadejda@charlesnadejda.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'admin', '+32 2 123 45 67', 'Bruxelles'),
('Dupont',  'Marie',   'marie.dupont@test.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'client', NULL, 'Bruxelles');

-- ──────────────────────────────────────────────────────────────
-- COMMANDE DE TEST
-- Marie : 1 Ballotin 250g + 1 Ballotin 500g (retrait, 8 jours)
-- ──────────────────────────────────────────────────────────────
INSERT INTO commandes (id_client, date_souhaitee, type_reception, statut, statut_paiement,
    methode_paiement, frais_livraison, total_ttc, notes)
VALUES (3, DATE_ADD(CURDATE(), INTERVAL 8 DAY), 'retrait', 'confirmee', 'paye',
    'bancontact', 0.00, 36.00, 'Cadeau anniversaire, emballage soigné s''il vous plaît.');

INSERT INTO lignes_commandes (id_commande, id_produit, quantite, prix_unitaire) VALUES
(1, 1, 1, 13.00),  -- Ballotin 250g
(1, 2, 1, 23.00);  -- Ballotin 500g

-- Ballotin 250g : EXACTEMENT 19 chocolats
INSERT INTO selections_parfums (id_ligne_commande, id_parfum, quantite) VALUES
(1,1,5),(1,2,4),(1,3,4),(1,8,3),(1,9,3);  -- total = 19 ✓

-- Ballotin 500g : EXACTEMENT 39 chocolats
INSERT INTO selections_parfums (id_ligne_commande, id_parfum, quantite) VALUES
(2,1,6),(2,2,6),(2,3,5),(2,4,5),(2,6,5),(2,8,4),(2,9,4),(2,15,4);  -- total = 39 ✓

INSERT INTO factures (id_commande, numero_facture, total_htva, montant_tva, total_ttc)
VALUES (1, 'FAC-2026-0001', ROUND(36.00/1.21,2), ROUND(36.00 - 36.00/1.21,2), 36.00);

-- ──────────────────────────────────────────────────────────────
-- PRODUCTION DE TEST (Praline Praliné Noisette — 40 pièces)
-- ──────────────────────────────────────────────────────────────
-- Calcul coût ingrédients pour 2 lots de 20 :
-- 400g choc lait (11.00€/kg) = 4.40 + 300g praliné (17.80€/kg) = 5.34
-- 60g beurre (8.50€/kg) = 0.51 + 100ml crème (2.80€/l) = 0.28 → total = 10.53
INSERT INTO productions_recettes (id_recette, date_production, quantite_produite, poids_lot_reel, poids_unitaire_reel, cout_ingredients, frais_fixes_alloues, cout_total, cout_unitaire, notes)
VALUES (1, NOW(), 40, 520.00, 13.00, 10.53, 3.24, 13.77, 0.3443, 'Lot de test — 40 pièces Praliné Noisette');

-- Mouvements lots (traçabilité AFSCA)
INSERT INTO mouvements_lots_ingredients (id_lot, id_production_recette, type_mouvement, quantite, prix_unitaire_moment, motif) VALUES
(2, 1, 'sortie', 0.400, 11.00, 'Production Praline Praliné x40 — chocolat lait'),
(7, 1, 'sortie', 0.300, 17.80, 'Production Praline Praliné x40 — praliné noisette'),
(5, 1, 'sortie', 0.060,  8.50, 'Production Praline Praliné x40 — beurre'),
(4, 1, 'sortie', 0.100,  2.80, 'Production Praline Praliné x40 — crème');

-- Mise à jour quantités lots (reflète les sorties)
UPDATE lots_ingredients SET quantite_disponible = quantite_disponible - 0.400 WHERE id = 2;
UPDATE lots_ingredients SET quantite_disponible = quantite_disponible - 0.300 WHERE id = 7;
UPDATE lots_ingredients SET quantite_disponible = quantite_disponible - 0.060 WHERE id = 5;
UPDATE lots_ingredients SET quantite_disponible = quantite_disponible - 0.100 WHERE id = 4;

-- Stock pralines créé
INSERT INTO stock_produits (id_recette, id_production_recette, quantite_disponible, cout_unitaire, date_production, date_dlc, statut)
VALUES (1, 1, 40.000, 0.3443, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 28 DAY), 'normal');

-- ──────────────────────────────────────────────────────────────
-- MESSAGES DE CONTACT
-- ──────────────────────────────────────────────────────────────
INSERT INTO contacts (nom, email, sujet, message) VALUES
('Thomas Lebrun', 'thomas@test.be', 'Commande mariage 80 personnes',
 'Bonjour, je souhaite commander des ballotins personnalisés pour un mariage de 80 personnes en juin. Offre spéciale possible ?'),
('Isabelle Morin', 'isabelle@test.be', 'Question allergènes',
 'Bonjour, y a-t-il des produits sans fruits à coque ? Mon fils est allergique aux noisettes. Merci.');

-- ============================================================
-- MODULE PÂTISSERIE — Seed data
-- Principe : tout est configurable — on seed juste les référentiels
-- de base pour que l'artisane puisse démarrer sans écran vide.
-- Les paramètres financiers sont laissés à NULL (à définir au 1er lancement).
-- ============================================================

-- ──────────────────────────────────────────────────────────────
-- pat_formes — 3 formes de base
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_formes (nom, type_calcul, description) VALUES
('Circulaire',    'circulaire',    'Gâteaux ronds — calcul via π·r²'),
('Rectangulaire', 'rectangulaire', 'Gâteaux carrés ou rectangulaires — calcul via L×l'),
('Libre',         'libre',         'Forme personnalisée — dimensions saisies manuellement');

-- ──────────────────────────────────────────────────────────────
-- pat_gabarits — Tailles standard par forme
-- Circulaire : dimension_1 = rayon (cm), dimension_2 = NULL
-- Rectangulaire : dimension_1 = largeur, dimension_2 = longueur
-- Volumes et surfaces calculés (π=3.14159265)
-- Circulaire v=π·r²·h  s_dessus=π·r²  s_lat=2·π·r·h
-- Rectangulaire v=l·w·h  s_dessus=l·w  s_lat=2·(l+w)·h
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_gabarits (id_forme, nom, dimension_1_cm, dimension_2_cm, hauteur_cm,
                           volume_cm3, surface_dessus_cm2, surface_laterale_cm2) VALUES
-- Circulaires (r=8, 9, 10, 11, 12 cm — diamètres 16/18/20/22/24 cm)
(1, 'Rond Ø16cm h10cm',  8.0,  NULL, 10.0,  2010.62,  201.06,  502.65),
(1, 'Rond Ø18cm h10cm',  9.0,  NULL, 10.0,  2544.69,  254.47,  565.49),
(1, 'Rond Ø20cm h10cm', 10.0,  NULL, 10.0,  3141.59,  314.16,  628.32),
(1, 'Rond Ø22cm h10cm', 11.0,  NULL, 10.0,  3801.33,  380.13,  690.79),
(1, 'Rond Ø24cm h10cm', 12.0,  NULL, 10.0,  4523.89,  452.39,  753.98),
(1, 'Rond Ø20cm h8cm',  10.0,  NULL,  8.0,  2513.27,  314.16,  502.65),
(1, 'Rond Ø24cm h8cm',  12.0,  NULL,  8.0,  3619.11,  452.39,  603.19),
-- Rectangulaires (carrés courants)
(2, 'Carré 18×18cm h8cm',  18.0, 18.0,  8.0,  2592.00,  324.00,  576.00),
(2, 'Carré 20×20cm h8cm',  20.0, 20.0,  8.0,  3200.00,  400.00,  640.00),
(2, 'Carré 24×24cm h8cm',  24.0, 24.0,  8.0,  4608.00,  576.00,  768.00),
(2, 'Rect 30×20cm h8cm',   30.0, 20.0,  8.0,  4800.00,  600.00,  800.00);

-- ──────────────────────────────────────────────────────────────
-- pat_types_couche — Types de couches de base (100% modifiables)
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_types_couche (nom, mode_scaling_defaut, description) VALUES
('Biscuit / Génoise',   'volume',           'Base du gâteau — scale en volume complet'),
('Sirop d''imbibage',   'volume',           'Sirop pour imbiber le biscuit — scale en volume'),
('Crème / Mousse',      'volume',           'Couche de crème ou mousse — scale en volume'),
('Garniture / Insert',  'surface_dessus',   'Fruits, confitures, inserts — scale en surface'),
('Glaçage',             'surface_dessus',   'Glaçage miroir ou autre — scale en surface dessus'),
('Décoration pourtour', 'surface_laterale', 'Décoration sur les côtés — scale en surface latérale'),
('Décoration fixe',     'fixe',             'Figurines, pièces fixes — ne scale pas');

-- ──────────────────────────────────────────────────────────────
-- pat_allergenes — 14 allergènes majeurs EU (règlement 1169/2011)
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_allergenes (nom, code_eu) VALUES
('Gluten (céréales)',         'EU-01'),
('Crustacés',                 'EU-02'),
('Œufs',                      'EU-03'),
('Poisson',                   'EU-04'),
('Arachides',                 'EU-05'),
('Soja',                      'EU-06'),
('Lait / Lactose',            'EU-07'),
('Fruits à coque',            'EU-08'),
('Céleri',                    'EU-09'),
('Moutarde',                  'EU-10'),
('Graines de sésame',         'EU-11'),
('Dioxyde de soufre / Sulfites', 'EU-12'),
('Lupin',                     'EU-13'),
('Mollusques',                'EU-14');

-- ──────────────────────────────────────────────────────────────
-- pat_options_deco — Exemples de décorations à thème
-- (l'artisane peut tout modifier / ajouter / archiver)
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_options_deco (nom, description, prix) VALUES
('Anniversaire Enfant',   'Déco colorée avec figurine thème enfant, bougie numérotée', 25.00),
('Anniversaire Adulte',   'Déco élégante avec prénom en sucre, fleurs en pâte à sucre', 20.00),
('Mariage',               'Décoration blanche et dorée, fleurs et initiales des mariés',  45.00),
('Communion / Baptême',   'Déco sobre ivoire et blanc, croix ou colombe en sucre',        30.00),
('Saint-Valentin',        'Déco rouge et rose, cœurs en sucre et ruban',                  20.00),
('Thème Libre',           'Thème personnalisé à définir avec l''artisane — prix variable', 35.00);

-- ──────────────────────────────────────────────────────────────
-- pat_parametres — Paramètres système (valeurs à NULL)
-- L'artisane les définit au premier lancement depuis ArtisaStock
-- ──────────────────────────────────────────────────────────────
INSERT INTO pat_parametres (cle, valeur, description) VALUES
('taux_horaire',      NULL, 'Taux horaire main d''œuvre en €/heure'),
('taux_charges',      NULL, 'Taux de charges globales en % appliqué sur le coût total'),
('marge_cible',       NULL, 'Marge bénéficiaire cible en % pour le calcul du prix suggéré'),
('supplement_mixte',  NULL, 'Supplément en € pour pièce montée avec saveurs différentes par étage'),
('tva_taux',          NULL, 'Taux TVA applicable en % (généralement 6% pour la pâtisserie en Belgique)');

-- ──────────────────────────────────────────────────────────────
-- FIN DU SEED
-- Tables totales seedées : 42
-- ──────────────────────────────────────────────────────────────
