-- ============================================================
-- Charles & Nadejda — Données de test (seed) v4
-- Auteur  : Ernest
-- Date    : 2026-06-11
-- Schema  : 20 tables (create_database.sql v4.0)
-- À exécuter APRÈS create_database.sql
-- ============================================================
-- Comptes de test (mot de passe = 'password' pour tous) :
--   Admin  : charles@charlesnadejda.be
--   Admin  : nadejda@charlesnadejda.be
--   Client : marie.dupont@test.be (table clients)
-- ⚠️ CHANGER LES MOTS DE PASSE EN PRODUCTION
-- ============================================================
-- Scénario : Chocolaterie artisanale à Bruxelles
--   - 1 activité "Chocolaterie"
--   - 1 stock physique "Stock Atelier"
--   - 2 fournisseurs (Barry Callebaut, Metro)
--   - 10 fiches ingrédients (chocolats, crème, beurre, sucre, etc.)
--   - 8 lots d'achat avec DLC réalistes (FIFO)
--   - 1 contexte BOM "Pralines"
--   - 2 niveaux (Ganaches, Enrobage)
--   - 3 fiches BOM avec lignes
--   - 1 production test avec lignes et stock BOM
--   - 1 catégorie web "Ballotins"
--   - 1 client (Marie Dupont)
--   - 1 produit web lié à une fiche BOM
--   - 1 commande web avec lignes
-- ============================================================

USE charlesnadejda;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ════════════════════════════════════════════════════════════
--  1. ACTIVITÉ
-- ════════════════════════════════════════════════════════════

INSERT INTO activites (nom, description) VALUES
('Chocolaterie', 'Production artisanale de pralines, truffes et bonbons chocolat');
SET @act = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  2. STOCK PHYSIQUE
-- ════════════════════════════════════════════════════════════

INSERT INTO stocks (nom, description) VALUES
('Stock Atelier', 'Stock principal de l''atelier chocolaterie — chambre froide 16°C');
SET @stk = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  3. LIAISON ACTIVITÉ ↔ STOCK
-- ════════════════════════════════════════════════════════════

INSERT INTO activites_stocks (id_activite, id_stock) VALUES
(@act, @stk);

-- ════════════════════════════════════════════════════════════
--  4. FOURNISSEURS
-- ════════════════════════════════════════════════════════════

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Barry Callebaut Belgique', 'Service Artisanat', 'artisans@callebaut.com', '+32 3 555 00 00', 'Aalstersestraat 122, 9280 Lebbeke');
SET @f_barry = LAST_INSERT_ID();

INSERT INTO fournisseurs (nom, contact, email, telephone, adresse) VALUES
('Metro Bruxelles', 'Service Pro', 'pro@metro.be', '+32 2 555 00 00', 'Chaussée de Mons 1424, 1070 Anderlecht');
SET @f_metro = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  5. FICHES INGRÉDIENTS (10)
-- ════════════════════════════════════════════════════════════

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Chocolat noir 70%', 'Callebaut', 'g', 'solide', 'Sac 2.5kg', 2500, 4, 28.50, 730, 'Grand Cru Belgique', @f_barry, 5000);
SET @i_choco_noir = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Chocolat lait 33%', 'Callebaut', 'g', 'solide', 'Sac 2.5kg', 2500, 4, 26.00, 730, 'Qualité Pro', @f_barry, 5000);
SET @i_choco_lait = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Chocolat blanc', 'Callebaut', 'g', 'solide', 'Sac 2.5kg', 2500, 4, 30.00, 730, 'Qualité Pro', @f_barry, 2500);
SET @i_choco_blanc = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock, densite) VALUES
('Crème fraîche 35%', NULL, 'ml', 'liquide', 'Brique 1L', 1000, 6, 3.20, 14, NULL, @f_metro, 3000, 1.0050);
SET @i_creme = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Beurre doux 82%', NULL, 'g', 'solide', 'Plaquette 250g', 250, 8, 2.85, 60, 'AOP Charentes', @f_metro, 1000);
SET @i_beurre = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Sucre semoule', NULL, 'g', 'poudre', 'Sac 5kg', 5000, 2, 4.50, 730, NULL, @f_metro, 5000);
SET @i_sucre = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Praliné noisette', 'Callebaut', 'g', 'solide', 'Seau 1kg', 1000, 2, 18.00, 365, 'Noisettes Piémont IGP', @f_barry, 1000);
SET @i_praline = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Glucose liquide', NULL, 'g', 'liquide', 'Seau 1kg', 1000, 2, 5.80, 365, NULL, @f_metro, 1000);
SET @i_glucose = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Fleur de sel', NULL, 'g', 'poudre', 'Boîte 250g', 250, 2, 3.50, 1095, 'Guérande', @f_metro, 100);
SET @i_sel = LAST_INSERT_ID();

INSERT INTO fiches_ingredients (nom, marque, unite_mesure, type_physique, conditionnement_label, qte_par_conditionnement, nb_par_lot, prix_achat_reference, dlc_jours_reference, qualite_label, id_fournisseur_defaut, seuil_alerte_stock) VALUES
('Cacao en poudre', 'Callebaut', 'g', 'poudre', 'Boîte 1kg', 1000, 2, 16.00, 730, 'Extra Brute', @f_barry, 500);
SET @i_cacao = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  6. LOTS D'ACHAT (FIFO — traçabilité AFSCA)
-- ════════════════════════════════════════════════════════════

-- Chocolat noir — 2 lots (ancien partiellement consommé, nouveau plein)
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, numero_lot, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_choco_noir, @stk, 'CAL-2026-047', @f_barry, 4, '2026-03-01', '2028-03-01', 10000, 8500, 28.50, 114.00, 'FAC-CAL-2026-089', 6);
SET @lot_choco_noir_1 = LAST_INSERT_ID();

INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, numero_lot, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_choco_noir, @stk, 'CAL-2026-098', @f_barry, 4, '2026-05-15', '2028-05-15', 10000, 10000, 28.50, 114.00, 'FAC-CAL-2026-142', 6);

-- Chocolat lait — 1 lot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, numero_lot, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_choco_lait, @stk, 'CAL-2026-048', @f_barry, 4, '2026-03-01', '2028-03-01', 10000, 8200, 26.00, 104.00, 'FAC-CAL-2026-089', 6);
SET @lot_choco_lait_1 = LAST_INSERT_ID();

-- Crème fraîche — 1 lot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_creme, @stk, @f_metro, 6, '2026-06-01', '2026-06-15', 6000, 5200, 3.20, 19.20, 'FAC-MET-2026-201', 6);
SET @lot_creme_1 = LAST_INSERT_ID();

-- Beurre — 1 lot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_beurre, @stk, @f_metro, 8, '2026-05-20', '2026-07-20', 2000, 1850, 2.85, 22.80, 'FAC-MET-2026-178', 6);
SET @lot_beurre_1 = LAST_INSERT_ID();

-- Sucre — 1 lot (longue conservation)
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_sucre, @stk, @f_metro, 2, '2026-04-10', '2028-04-10', 10000, 8500, 4.50, 9.00, 'FAC-MET-2026-130', 6);

-- Praliné noisette — 1 lot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, numero_lot, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_praline, @stk, 'CAL-2026-031', @f_barry, 2, '2026-02-15', '2027-02-15', 2000, 1600, 18.00, 36.00, 'FAC-CAL-2026-071', 6);
SET @lot_praline_1 = LAST_INSERT_ID();

-- Fleur de sel — 1 lot
INSERT INTO lots_ingredients (id_fiche_ingredient, id_stock, id_fournisseur, nb_conditionnements, date_achat, date_peremption, quantite_initiale, quantite_disponible, prix_unitaire, prix_achat_reel, reference_facture, tva_pct) VALUES
(@i_sel, @stk, @f_metro, 2, '2026-03-01', '2029-03-01', 500, 480, 3.50, 7.00, 'FAC-MET-2026-089', 6);

-- ════════════════════════════════════════════════════════════
--  7. UTILISATEURS (admin — app C# WinForms)
-- ════════════════════════════════════════════════════════════

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role, telephone, adresse, code_postal, ville) VALUES
('Artisan', 'Charles', 'charles@charlesnadejda.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
 'admin', '+32 2 123 45 67', 'Rue de la Chocolaterie 12', '1000', 'Bruxelles'),
('Artisan', 'Nadejda', 'nadejda@charlesnadejda.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
 'admin', '+32 2 123 45 67', 'Rue de la Chocolaterie 12', '1000', 'Bruxelles');

-- ════════════════════════════════════════════════════════════
--  8. CONTEXTE BOM
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_contextes (nom, description, id_activite) VALUES
('Pralines', 'Production de pralines et truffes artisanales — gamme chocolaterie', @act);
SET @ctx = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  9. NIVEAUX BOM (2)
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx, 1, 'Ganaches & Intérieurs', 'Préparation des ganaches, pralinés et garnitures intérieures');
SET @n_ganaches = LAST_INSERT_ID();

INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES
(@ctx, 2, 'Enrobage & Finition', 'Enrobage chocolat, trempage et finition des pralines');
SET @n_enrobage = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  10. FICHES BOM (3 recettes)
-- ════════════════════════════════════════════════════════════

-- N1 : Ganache praliné noisette (intérieur)
INSERT INTO bom_fiches (id_niveau, nom, description, unite_output, quantite_output, temps_preparation) VALUES
(@n_ganaches, 'Ganache Praliné Noisette',
 'Ganache onctueuse au praliné noisette du Piémont. Base pour praline signature.',
 'g', 1000, 30);
SET @f_ganache_praline = LAST_INSERT_ID();

-- N1 : Ganache caramel fleur de sel (intérieur)
INSERT INTO bom_fiches (id_niveau, nom, description, unite_output, quantite_output, temps_preparation) VALUES
(@n_ganaches, 'Ganache Caramel Fleur de Sel',
 'Ganache caramel au beurre salé avec fleur de sel de Guérande. Texture coulante.',
 'g', 1000, 45);
SET @f_ganache_caramel = LAST_INSERT_ID();

-- N2 : Praline finie (enrobée)
INSERT INTO bom_fiches (id_niveau, nom, description, unite_output, quantite_output, temps_preparation, stock_cible) VALUES
(@n_enrobage, 'Praline Praliné Noisette (x20)',
 'Praline signature — ganache praliné noisette enrobée de chocolat au lait. Lot de 20 pièces.',
 'piece', 20, 45, 100);
SET @f_praline_finie = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  11. LIGNES BOM (composition des fiches)
-- ════════════════════════════════════════════════════════════

-- Ganache Praliné Noisette (1000g output)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_ganache_praline, 'ingredient', @i_praline, 400, 'g'),   -- 400g praliné
(@f_ganache_praline, 'ingredient', @i_creme,   250, 'ml'),  -- 250ml crème
(@f_ganache_praline, 'ingredient', @i_choco_lait, 300, 'g'),-- 300g chocolat lait
(@f_ganache_praline, 'ingredient', @i_beurre,   50, 'g');   -- 50g beurre

-- Ganache Caramel Fleur de Sel (1000g output)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_ganache_caramel, 'ingredient', @i_sucre,    250, 'g'),  -- 250g sucre (caramélisé)
(@f_ganache_caramel, 'ingredient', @i_creme,    300, 'ml'), -- 300ml crème
(@f_ganache_caramel, 'ingredient', @i_beurre,   100, 'g'),  -- 100g beurre
(@f_ganache_caramel, 'ingredient', @i_glucose,   50, 'g'),  -- 50g glucose
(@f_ganache_caramel, 'ingredient', @i_sel,        5, 'g'),  -- 5g fleur de sel
(@f_ganache_caramel, 'ingredient', @i_choco_noir, 300, 'g');-- 300g chocolat noir (enrobage caramel)

-- Praline Praliné finie (20 pièces) — utilise la ganache N1 + chocolat enrobage
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_fiche, quantite, unite_mesure) VALUES
(@f_praline_finie, 'fiche', @f_ganache_praline, 260, 'g');  -- 260g ganache (13g par praline)
INSERT INTO bom_fiches_lignes (id_fiche, type_input, id_input_ingredient, quantite, unite_mesure) VALUES
(@f_praline_finie, 'ingredient', @i_choco_lait, 200, 'g');  -- 200g chocolat lait (enrobage)

-- ════════════════════════════════════════════════════════════
--  12. PRODUCTION DE TEST
--      Praline Praliné Noisette x20 — 1 batch
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_productions (id_niveau, id_fiche, quantite_produite, cout_ingredients, cout_unitaire, date_production, notes) VALUES
(@n_enrobage, @f_praline_finie, 20, 8.42, 0.4210, '2026-06-08 14:30:00', 'Lot test — 20 pralines praliné noisette, tempérage réussi');
SET @prod = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  13. LIGNES DE PRODUCTION (consommation FIFO)
-- ════════════════════════════════════════════════════════════

-- Consommation directe depuis lots d'ingrédients
-- 200g chocolat lait (enrobage)
INSERT INTO bom_productions_lignes (id_production, type_source, id_lot_ingredient, quantite_consommee, cout_unitaire_moment) VALUES
(@prod, 'lot_ingredient', @lot_choco_lait_1, 200, 0.0104);   -- 26.00€/2500g = 0.0104€/g

-- On ne détaille pas la consommation de la sous-fiche ganache ici (simplification seed)
-- En production réelle, l'app décompose récursivement la fiche ganache

-- ════════════════════════════════════════════════════════════
--  14. STOCK BOM (output de production)
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_stocks (id_niveau, id_contexte, id_activite, id_fiche, id_production, quantite_disponible, cout_unitaire, date_production, date_dlc) VALUES
(@n_enrobage, @ctx, @act, @f_praline_finie, @prod, 20, 0.4210, '2026-06-08', '2026-07-06');
SET @bom_stock_praline = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  15. RÉSERVATION DE TEST (optionnel — lot crème réservé pour production)
-- ════════════════════════════════════════════════════════════

INSERT INTO bom_reservations (id_lot, id_contexte, quantite_reservee, notes) VALUES
(@lot_creme_1, @ctx, 500, 'Réservation crème pour production ganaches semaine 24');

-- ════════════════════════════════════════════════════════════
--  16. CATÉGORIES WEB
-- ════════════════════════════════════════════════════════════

INSERT INTO categories_web (nom, description, ordre_affichage) VALUES
('Ballotins', 'Assortiments de pralines artisanales en coffret cadeau', 1),
('Truffes', 'Truffes enrobées de cacao, noisettes ou chocolat', 2);

SET @cat_ballotins = (SELECT id FROM categories_web WHERE nom = 'Ballotins');

-- ════════════════════════════════════════════════════════════
--  17. CLIENT (site e-commerce Laravel)
-- ════════════════════════════════════════════════════════════

INSERT INTO clients (nom, prenom, email, mot_de_passe, telephone, adresse_rue, adresse_cp, adresse_ville, adresse_pays) VALUES
('Dupont', 'Marie', 'marie.dupont@test.be',
 '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
 '+32 475 12 34 56', 'Avenue Louise 42', '1050', 'Ixelles', 'Belgique');
SET @client_marie = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  18. PRODUIT WEB (lié à fiche BOM)
-- ════════════════════════════════════════════════════════════

INSERT INTO produits_web (id_bom_fiche, id_categorie, nom_commercial, description, prix_vente, en_vente, ordre_affichage) VALUES
(@f_praline_finie, @cat_ballotins,
 'Ballotin Pralinés Noisette (20 pcs)',
 'Coffret de 20 pralines artisanales au praliné noisette du Piémont, enrobées de chocolat au lait belge. Coffret doré avec ruban satin.',
 18.50, 1, 1);
SET @produit_ballotin = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  19. COMMANDE WEB
--      Marie : 1 Ballotin Pralinés + retrait
-- ════════════════════════════════════════════════════════════

INSERT INTO commandes_web (id_client, statut, total_ttc, adresse_livraison, date_commande) VALUES
(@client_marie, 'payee', 18.50,
 'Retrait en boutique — Avenue Louise 42, 1050 Ixelles',
 '2026-06-09 16:30:00');
SET @cmd = LAST_INSERT_ID();

-- ════════════════════════════════════════════════════════════
--  20. LIGNES COMMANDE WEB
-- ════════════════════════════════════════════════════════════

INSERT INTO commandes_web_lignes (id_commande, id_produit_web, quantite, prix_unitaire) VALUES
(@cmd, @produit_ballotin, 1, 18.50);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN DU SEED — v4
-- ============================================================
-- Tables peuplées : 18/20 (activites_stocks et bom_reservations incluses)
-- Tables vides autorisées : aucune table ignorée — toutes les 20 sont touchées
--
-- Pour tester :
--   1. Connexion C# WinForms : charles@charlesnadejda.be / password
--   2. Connexion Laravel (admin) : nadejda@charlesnadejda.be / password
--   3. Connexion Laravel (client) : marie.dupont@test.be / password
--   4. Naviguer Chocolaterie → Pralines → Ganaches / Enrobage
--   5. Vérifier stock BOM : 20 pralines dispo (DLC 2026-07-06)
--   6. Vérifier commande Marie : 1 ballotin payé
--   7. Vérifier vue_stock_global : lots ingrédients + stock produit fini
-- ============================================================
