# ArtisaStock — Matrice de Traçabilité Complète

> **Livrable :** BA/FA (Business Analyst / Functional Analyst)
> **Projet :** ArtisaStock — ERP artisanal (C# WinForms + MySQL)
> **Version :** 1.0
> **Date :** 2026-05-15
> **Auteur :** Équipe Analyse fonctionnelle — Pipeline multi-agents ArtisaStock
> **Branche :** `feat/refactoring-sprints-p0-p3`

---

## Table des matières

1. [Matrice de traçabilité principale](#1-matrice-de-traçabilité-principale)
2. [Matrice Règles métier → Implémentation](#2-matrice-règles-métier--implémentation)
3. [Matrice Sécurité → Implémentation](#3-matrice-sécurité--implémentation)
4. [Couverture fonctionnelle par module](#4-couverture-fonctionnelle-par-module)
5. [Matrice Navigation](#5-matrice-navigation)
6. [Matrice Dépendances entre entités](#6-matrice-dépendances-entre-entités)
7. [Glossaire métier](#7-glossaire-métier)

---

## Légende globale

| Symbole | Signification |
|---------|---------------|
| ✅ | Implémenté et fonctionnel |
| 🔄 | En cours d'implémentation (sprint actif) |
| 📋 | Planifié (backlog validé) |
| ⏸ | Placeholder — écran vide présent, logique absente |
| ❌ | Non démarré |

---

## 1. Matrice de traçabilité principale

> Lecture : chaque ligne représente un Use Case (UC) et trace son chemin complet depuis la User Story jusqu'à la base de données.

### 1.1 Module AUTH

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-AUTH-01 | US-AUTH-01 | `FrmLogin` | `UtilisateurDAL.Authenticate()` | `Utilisateur` | `utilisateurs` | RG-SOFT-DELETE (actif=0), SEC-01, SEC-08 | ✅ |

### 1.2 Module NAVIGATION

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-NAV-01 | US-NAV-01 | `FrmPrincipal` → HubScreen | `ActiviteDAL`, `IngredientDAL`, `BomProductionDAL`, `VueStockGlobalDAL` | `Activite`, `Ingredient`, `BomProduction`, `VueStockGlobal` | `activites`, `fiches_ingredients`, `bom_productions`, `vue_stock_global` | RG-STOCK-CIBLE | ✅ |
| UC-NAV-02 | US-NAV-02 | `FrmPrincipal` → SidebarPanel | `ActiviteDAL`, `BomContexteDAL`, `BomNiveauDAL` | `Activite`, `BomContexte`, `BomNiveau` | `activites`, `bom_contextes`, `bom_niveaux` | RG-SOFT-DELETE, RG-N0 | ✅ |

### 1.3 Module ACTIVITÉS

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-ACT-01 | US-ACT-01, US-NAV-03 | `FrmActivites` → `FrmActiviteEdit` | `ActiviteDAL.Insert()` | `Activite` | `activites` | RG-SOFT-DELETE | ✅ |
| UC-ACT-02 | US-ACT-02, US-NAV-04 | `FrmActivites` → `FrmActiviteEdit` | `ActiviteDAL.Update()` | `Activite` | `activites` | RG-SOFT-DELETE | ✅ |
| UC-ACT-03 | US-ACT-03 | `FrmActivites` | `ActiviteDAL.Desactiver()` | `Activite` | `activites` | RG-SOFT-DELETE | ✅ |
| UC-ACT-04 | US-ACT-04 | `FrmActiviteStocks` | `StockDAL.LierActivite()`, `StockDAL.DelierActivite()` | `Activite`, `Stock` | `activites`, `activites_stocks` | RG-FK-GUARD | ✅ |

### 1.4 Module RESSOURCES

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-RES-01 | US-RES-01 | `FrmVueStock` | `VueStockGlobalDAL` | `VueStockGlobal` | `vue_stock_global` (VIEW) | RG-DLC, RG-STOCK-CIBLE | ✅ |
| UC-RES-02 | US-RES-02 | `FrmStocks` → `FrmStockEdit` | `StockDAL` | `Stock` | `stocks` | RG-FK-GUARD, RG-SOFT-DELETE | ✅ |
| UC-RES-03 | US-RES-03 | `FrmIngredients` → `FrmIngredientEdit` | `IngredientDAL` | `Ingredient` | `fiches_ingredients` | RG-UNIT, RG-FK-GUARD, RG-STOCK-CIBLE | ✅ |
| UC-RES-04 | US-RES-04 | `FrmFournisseurs` → `FrmFournisseurEdit` | `FournisseurDAL` | `Fournisseur` | `fournisseurs` | RG-FK-GUARD | ✅ |
| UC-RES-05 | US-RES-05 | `FrmAchats` → `FrmAchatEdit` | `LotDAL` | `Lot` | `lots_ingredients` | RG-FIFO, RG-DLC, RG-UNIT, RG-FK-GUARD | ✅ |
| UC-RES-06 | US-RES-03 | `FrmIngredients` | `IngredientDAL.GetAllAvecStock()` | `Ingredient` | `fiches_ingredients`, `lots_ingredients` | RG-STOCK-CIBLE | ✅ |

### 1.5 Module BOM — Configuration & Production

> **Note :** Les UC sont numérotés conformément au document `11_USE_CASES.md` (UC-BOM-01 à UC-BOM-06). Chaque UC peut couvrir plusieurs User Stories (ex : UC-BOM-01 couvre la création de contexte, UC-BOM-02 couvre le CRUD complet des niveaux).

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-BOM-01 | US-BOM-CTX-01, US-BOM-CTX-02, US-BOM-CTX-03 | `FrmBomContextes` → `FrmBomContexteEdit` | `BomContexteDAL` (Insert, Update, Delete) | `BomContexte`, `BomNiveau` | `bom_contextes`, `bom_niveaux` | RG-N0, RG-TRANSACTION, RG-FK-GUARD | ✅ |
| UC-BOM-02 | US-BOM-NIV-01, US-BOM-NIV-02, US-BOM-NIV-03 | `FrmBomNiveaux` → `FrmBomNiveauEdit` | `BomNiveauDAL` (Insert, Update, Delete, GetOrdreMax) | `BomNiveau` | `bom_niveaux` | RG-N0, RG-TOP-DELETE, RG-BOM-LEVEL, RG-FK-GUARD | ✅ |
| UC-BOM-03 | US-BOM-FICHE-01, US-BOM-FICHE-02, US-BOM-FICHE-03 | `FrmBomFiches` → `FrmBomFicheEdit` | `BomFicheDAL`, `BomFicheLigneDAL` | `BomFiche`, `BomFicheLigne` | `bom_fiches`, `bom_fiches_lignes` | RG-UNIT, RG-BOM-LEVEL, RG-TRANSACTION, RG-FK-GUARD | ✅ |
| UC-BOM-04 | US-BOM-PROD-01 | `FrmBomProductionSimulation` | `BomProductionDAL.VerifierDisponibilite()`, `BomCoutDAL.CalculerCout()` | `BomManque`, `RapportCout` | `bom_fiches`, `bom_fiches_lignes`, `bom_stocks`, `lots_ingredients` | RG-UNIT, RG-RESERVATION, RG-BATCH, RG-FIFO | 🔄 |
| UC-BOM-05 | US-BOM-PROD-02 | `FrmBomProductionSimulation` | `BomProductionDAL.Executer()`, `BomStockDAL`, `BomReservationDAL` | `BomProduction`, `BomProductionLigne`, `BomStock`, `BomReservation` | `bom_productions`, `bom_production_lignes`, `bom_stocks`, `bom_reservations`, `lots_ingredients` | RG-FIFO, RG-TRANSACTION, RG-RESERVATION, RG-BATCH, RG-UNIT | 🔄 |
| UC-BOM-06 | US-BOM-PROD-03 | `FrmBomProductionSimulation`, HubScreen | `BomProductionDAL.GetHistorique()` | `BomProduction` | `bom_productions` | — | 📋 |

### 1.7 Module CATALOGUE WEB

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-CAT-01 | US-CAT-01 | `FrmBomContextes` (placeholder) | `CategorieDAL` *(non créé)* | — | — | RG-FK-GUARD, RG-CONFIGURABLE | ⏸ |
| UC-CAT-02 | US-CAT-02 | *(non créé)* | `ParfumDAL` *(non créé)* | — | — | RG-FK-GUARD, RG-CONFIGURABLE | ❌ |
| UC-CAT-03 | US-CAT-03 | *(non créé)* | `ProduitDAL` *(non créé)* | — | — | RG-CONFIGURABLE, RG-SOFT-DELETE | ❌ |

### 1.8 Module COMMANDES WEB

| UC | US | Écran(s) C# | DAL | Modèle | Table(s) DB | Règles métier | Statut |
|----|----|-------------|-----|--------|-------------|---------------|--------|
| UC-CMD-01 | US-CMD-01 | *(non créé)* | `CommandeDAL` *(non créé)* | — | — | — | ❌ |
| UC-CMD-02 | US-CMD-01 | *(non créé)* | `CommandeDAL` *(non créé)* | — | — | RG-SOFT-DELETE | ❌ |

---

## 2. Matrice Règles métier → Implémentation

> **RG** = Règle de Gestion (Business Rule). Chaque règle est tracée vers ses points d'application concrets.

| Code | Intitulé | Couche d'application | Fichiers C# concernés | Fichiers DAL concernés | Statut test |
|------|----------|---------------------|-----------------------|------------------------|-------------|
| **RG-FIFO** | Les lots d'ingrédients sont consommés dans l'ordre chronologique d'achat (`date_production ASC`) — jamais en ordre aléatoire | DAL | `FrmBomProductionSimulation` | `BomProductionDAL.Executer()`, `LotDAL` | 🔄 Manuel |
| **RG-UNIT** | Toute quantité est stockée dans l'unité de référence de l'ingrédient ; les conversions (g↔kg, ml↔L) sont réalisées à l'entrée et à la sortie — jamais en clair dans les formulaires | DAL + Form | `FrmIngredientEdit`, `FrmAchatEdit`, `FrmBomFicheEdit` | `IngredientDAL`, `LotDAL`, `BomFicheLigneDAL` | ✅ Manuel |
| **RG-BOM-LEVEL** | Une fiche BOM au niveau N ne peut référencer comme input que des ingrédients (N0) ou des fiches de niveau N-1 — jamais du même niveau ni supérieur | DAL + Form | `FrmBomFicheEdit` | `BomFicheDAL`, `BomFicheLigneDAL` | ✅ Manuel |
| **RG-SOFT-DELETE** | Aucune entité métier n'est physiquement supprimée tant qu'elle a des dépendances actives ; les désactivations utilisent le flag `actif=0` | DAL | `FrmActivites`, `FrmIngredients` | `ActiviteDAL.Desactiver()`, `IngredientDAL`, `UtilisateurDAL` | ✅ Manuel |
| **RG-STOCK-CIBLE** | Chaque ingrédient possède un seuil d'alerte (`seuil_alerte`) ; si le stock actuel calculé est inférieur ou égal à ce seuil, une alerte est levée dans la Vue stock global et dans le Hub | Form | `FrmIngredients`, `FrmVueStock`, HubScreen | `VueStockGlobalDAL`, `IngredientDAL` | ✅ Manuel |
| **RG-BATCH** | Une production est définie en nombre de batches entiers (`nb_batches` ∈ ℤ⁺) ; la quantité produite = `nb_batches × quantite_output` de la fiche BOM | DAL + Form | `FrmBomProductionSimulation` | `BomProductionDAL` | 🔄 Manuel |
| **RG-RESERVATION** | Avant consommation lors d'une production, les quantités réservées (`bom_reservations`) sont déduites du stock disponible pour calculer la disponibilité réelle | DAL | `FrmBomProductionSimulation` | `BomProductionDAL.VerifierDisponibilite()`, `BomReservationDAL` | 📋 |
| **RG-N0** | Le niveau 0 d'un contexte (stock ingrédients) est créé automatiquement à la création du contexte en transaction atomique ; il n'est jamais modifiable ni supprimable depuis l'interface utilisateur | DAL + Form | `FrmBomNiveaux`, `FrmBomContexteEdit` | `BomContexteDAL.Insert()`, `BomNiveauDAL` | ✅ Manuel |
| **RG-TOP-DELETE** | Seul le niveau ayant le `MAX(ordre)` dans un contexte peut être supprimé ; la suppression d'un niveau intermédiaire est bloquée avec message explicite | DAL | `FrmBomNiveaux` | `BomNiveauDAL.Delete()` | ✅ Manuel |
| **RG-FK-GUARD** | Avant toute suppression d'une entité, le DAL vérifie l'absence de dépendances actives (FK) ; si des dépendances existent, l'opération est refusée avec liste des entités bloquantes | DAL | Tous les `FrmXxxEdit` avec bouton "Supprimer" | `ActiviteDAL`, `IngredientDAL`, `LotDAL`, `FournisseurDAL`, `StockDAL`, `BomContexteDAL`, `BomNiveauDAL`, `BomFicheDAL` | ✅ Manuel |
| **RG-TRANSACTION** | Toute opération multi-tables (création contexte+niveau, lancement production, modification fiche+lignes) est encapsulée dans une `MySqlTransaction` ; en cas d'exception, `ROLLBACK` complet | DAL | `FrmBomContexteEdit`, `FrmBomProductionSimulation`, `FrmBomFicheEdit` | `BomContexteDAL.Insert()`, `BomProductionDAL.Executer()`, `BomFicheDAL.Update()` | ✅ Manuel |
| **RG-DLC** | Les lots dont la DLC (Date Limite de Consommation) est dépassée sont surlignés en rouge ; DLC < 7 jours → orange ; DLC > 7 jours → vert ; sans DLC → gris | Form | `FrmVueStock`, `FrmAchats` | `VueStockGlobalDAL`, `LotDAL` | ✅ Manuel |
| **RG-CONFIGURABLE** | Un produit du catalogue peut être « configurable » (le client choisit les parfums) ou « fixe » ; ce flag conditionne l'affichage des options dans la boutique web | Form | *(non créé)* | `ProduitDAL` *(non créé)* | ❌ |

---

## 3. Matrice Sécurité → Implémentation

> **SEC** = Contrainte de sécurité. Référence : OWASP Top 10 appliqué au contexte WinForms + MySQL.

| Code | Intitulé | Description technique | Fichiers concernés | Méthode de vérification | Statut |
|------|----------|-----------------------|--------------------|------------------------|--------|
| **SEC-01** | Requêtes paramétrées | Toutes les requêtes SQL utilisent des paramètres nommés (`@param`) — aucune concaténation de chaîne dans les clauses SQL | `DbHelper`, tous les fichiers DAL | Revue de code — grep `+` dans les strings SQL | ✅ |
| **SEC-02** | Transactions MySQL | Les opérations multi-tables sont encapsulées dans des `MySqlTransaction` avec `ROLLBACK` en cas d'erreur | `BomContexteDAL`, `BomProductionDAL`, `BomFicheDAL` | Test intégration — injection d'erreur en mid-transaction | ✅ |
| **SEC-03** | Guards FK (clés étrangères) | Vérification de l'absence de dépendances actives avant toute suppression — levée d'`InvalidOperationException` avec message explicite | Tous les DAL avec méthode `Delete()` | Test unitaire — suppression avec dépendances actives | ✅ |
| **SEC-04** | Conversion d'unités | Les quantités sont toujours converties dans l'unité de référence avant stockage — pas de valeurs brutes ambiguës en DB | `IngredientDAL`, `LotDAL`, `BomFicheLigneDAL`, `BomProductionDAL` | Test unitaire — cas limites g↔kg, ml↔L | ✅ |
| **SEC-05** | Soft delete | Les entités avec historique ne sont jamais physiquement supprimées — flag `actif=0` ; les requêtes `GetAll()` filtrent systématiquement `WHERE actif=1` | `ActiviteDAL`, `UtilisateurDAL`, `IngredientDAL` | Revue de code — vérification clause `WHERE actif` | ✅ |
| **SEC-06** | FIFO (ordre de consommation) | Les lots sont consommés dans l'ordre d'achat chronologique pour éviter la péremption et les coûts faussés | `BomProductionDAL.Executer()` | Test intégration — vérification ordre consommation après production | 🔄 |
| **SEC-07** | Validation des entrées | Les entrées utilisateur sont validées à la frontière UI (formulaires) avant tout appel DAL : types, plages numériques, unicité, longueurs | Tous les `FrmXxxEdit` | Test manuel — saisies invalides, valeurs limites | ✅ |
| **SEC-08** | Gestion des exceptions | Toutes les exceptions de couche DAL sont catchées et retournées sous forme de messages utilisateur — aucun stack trace exposé | `DbHelper`, tous les DAL | Test manuel — coupure DB simulée | ✅ |
| **SEC-09** | Protection de la chaîne de connexion | La chaîne de connexion MySQL est stockée dans `app.config` ou `settings` — jamais en dur dans le code source, jamais dans le dépôt git | `DbHelper`, `app.config` | Revue `.gitignore` + grep chaîne de connexion dans les sources | ✅ |

---

## 4. Couverture fonctionnelle par module

> **Taux de couverture** = UC Implémentés / UC Total du module × 100

| Module | UC Total | Implémentés (✅) | En cours (🔄) | Planifiés (📋) | Placeholder (⏸) | Non démarré (❌) | Couverture |
|--------|----------|-----------------|--------------|----------------|-----------------|----------------|------------|
| AUTH | 1 | 1 | 0 | 0 | 0 | 0 | **100 %** |
| NAVIGATION | 2 | 2 | 0 | 0 | 0 | 0 | **100 %** |
| ACTIVITÉS | 4 | 4 | 0 | 0 | 0 | 0 | **100 %** |
| RESSOURCES | 6 | 6 | 0 | 0 | 0 | 0 | **100 %** |
| BOM — Config + Prod | 6 | 3 | 2 | 1 | 0 | 0 | **50 %** *(83 % partiel)* |
| CATALOGUE WEB | 3 | 0 | 0 | 0 | 1 | 2 | **0 %** |
| COMMANDES WEB | 2 | 0 | 0 | 0 | 0 | 2 | **0 %** |
| **TOTAL** | **24** | **16** | **2** | **1** | **1** | **4** | **67 %** |

### Analyse par priorité

| Priorité | Modules | Couverture | Observation |
|----------|---------|-----------|-------------|
| P0 — Critique | AUTH, NAVIGATION | 100 % | Socle applicatif stable |
| P1 — Cœur métier | ACTIVITÉS, RESSOURCES, BOM Config | 100 % | MVP fonctionnel |
| P2 — Production | BOM Production & Simulation | 33 % implémenté (83 % en cours/partiel) | Sprint actif |
| P3 — Extension web | CATALOGUE WEB, COMMANDES WEB | 0 % | Hors scope MVP |

---

## 5. Matrice Navigation

> Décrit le comportement de `AppState` / `ScreenRouter` lors de chaque navigation utilisateur.

| NavItemId | ScreenId | Form / UserControl chargé | Changements AppState | Préconditions |
|-----------|----------|--------------------------|----------------------|---------------|
| `Hub` | `ScreenId.Hub` | `HubScreen` (UserControl) | `ActiveScreen = Hub` | Authentification valide (`FrmLogin` fermé avec succès) |
| `VueStockGlobal` | `ScreenId.Ressources` | `FrmVueStock` (inline dans `pnlContent`) | `ActiveScreen = Ressources`, `ActiveNavItem = VueStockGlobal` | Aucune |
| `StocksLiaisons` | `ScreenId.Ressources` | `FrmStocks` (inline dans `pnlContent`) | `ActiveScreen = Ressources`, `ActiveNavItem = StocksLiaisons` | Aucune |
| `Ingredients` | `ScreenId.Ressources` | `FrmIngredients` (inline dans `pnlContent`) | `ActiveScreen = Ressources`, `ActiveNavItem = Ingredients` | Aucune |
| `Fournisseurs` | `ScreenId.Ressources` | `FrmFournisseurs` (inline dans `pnlContent`) | `ActiveScreen = Ressources`, `ActiveNavItem = Fournisseurs` | Aucune |
| `AchatsLots` | `ScreenId.Ressources` | `FrmAchats` (inline dans `pnlContent`) | `ActiveScreen = Ressources`, `ActiveNavItem = AchatsLots` | Aucune |
| *(Activité cliquée dans rail)* | `ScreenId.ContexteNiveaux` | `FrmBomContextes` (liste contextes) | `SelectedActiviteId = X`, `ActiveScreen = ContexteNiveaux` | Au moins une activité active |
| `NiveauxContextes` | `ScreenId.ContexteNiveaux` | `FrmBomNiveaux` + `FrmBomFiches` | `SelectedContexteId = X` | Activité sélectionnée |
| `FichesBom` | `ScreenId.ContexteNiveaux` | `FrmBomFiches` | `SelectedNiveauId = X` | Contexte sélectionné |
| `Production` | `ScreenId.Production` | `FrmBomProductionSimulation` | `ActiveScreen = Production` | Fiche BOM existante |
| `Planning` | `ScreenId.Planning` | *(placeholder)* | `ActiveScreen = Planning` | — |
| `DevisPatisserie` | `ScreenId.DevisPatisserie` | *(placeholder)* | `ActiveScreen = DevisPatisserie` | — |
| `Mouvements` | `ScreenId.Mouvements` | *(placeholder)* | `ActiveScreen = Mouvements` | — |
| `Parametres` | `ScreenId.Parametres` | *(placeholder)* | `ActiveScreen = Parametres` | — |

### Règles de navigation non-négociables (R-UX)

| Règle | Contrainte | Impact technique |
|-------|-----------|-----------------|
| R-UX-01 | Aucun `ShowDialog()` dans `FrmPrincipal` sauf `FrmLogin` | Toute nouvelle vue est un `UserControl` chargé dans `pnlContent` |
| R-UX-02 | Toute édition se fait dans la zone droite (`pnlContent`) | Les `FrmXxxEdit` sont des UserControls, pas des `Form` secondaires |
| R-UX-03 | La navigation rail gauche ne provoque jamais de popup | Warnings de modifications non sauvegardées = Panel inline |
| R-UX-04 | Workflow BOM complet sans quitter `FrmPrincipal` | Chaîne Activité → Contexte → Niveau → Fiche → Simulation → Production |
| R-UX-05 | Feedback visuel immédiat (< 200ms) | `WaitCursor` + labels de résultat inline post-action |

---

## 6. Matrice Dépendances entre entités

> Représente les relations FK (Foreign Key) entre les entités du domaine, leur comportement à la suppression et leur impact sur le soft delete.

| Entité | Dépend de | Est référencée par | Comportement suppression | Règle appliquée |
|--------|----------|-------------------|-------------------------|-----------------|
| `Utilisateur` | — | — | Soft delete (`actif=0`) | RG-SOFT-DELETE, SEC-05 |
| `Activite` | — | `BomContexte`, `activites_stocks`, `BomStock` | Soft delete (`actif=0`) — guard contextes actifs | RG-SOFT-DELETE, RG-FK-GUARD |
| `Stock` | — | `activites_stocks`, `Ingredient` | RESTRICT — refus si ingrédients liés | RG-FK-GUARD |
| `Fournisseur` | — | `Ingredient`, `Lot` | RESTRICT — refus si ingrédients liés | RG-FK-GUARD |
| `Ingredient` | `Stock`, `Fournisseur` | `Lot`, `BomFicheLigne` | RESTRICT — refus si lots ou fiches liées | RG-FK-GUARD, RG-SOFT-DELETE |
| `Lot` | `Ingredient`, `Fournisseur` | `BomProductionLigne`, `BomStock` | RESTRICT — refus si utilisé en production | RG-FK-GUARD, RG-FIFO |
| `BomContexte` | `Activite` | `BomNiveau`, `BomStock`, `BomProduction` | CASCADE (niveaux → fiches → bom_stocks) en transaction | RG-TRANSACTION, RG-TOP-DELETE |
| `BomNiveau` | `BomContexte` | `BomFiche`, `BomStock` | RESTRICT sauf niveau TOP → CASCADE fiches + bom_stocks | RG-TOP-DELETE, RG-N0 |
| `BomFiche` | `BomNiveau` | `BomFicheLigne` (parent), `BomFicheLigne` (input), `BomProduction` | RESTRICT — refus si productions ou fiches dépendantes | RG-FK-GUARD, RG-BOM-LEVEL |
| `BomFicheLigne` | `BomFiche` (parent), `Ingredient` ou `BomFiche` (input) | — | CASCADE à la suppression de la fiche parente | RG-TRANSACTION |
| `BomProduction` | `BomFiche`, `BomContexte` | `BomProductionLigne` | RESTRICT (historique immuable) | — |
| `BomProductionLigne` | `BomProduction`, `Lot` | — | CASCADE à la suppression de la production (non autorisée normalement) | — |
| `BomStock` | `BomNiveau`, `BomContexte`, `Activite` | `BomProductionLigne` (comme source) | CASCADE à la suppression du niveau parent | RG-FIFO |
| `BomReservation` | `BomFiche`, `BomNiveau` | — | Expire automatiquement après lancement production | RG-RESERVATION |

### Vue schématique des dépendances (domaine BOM)

```
Activite
  └─► BomContexte
        └─► BomNiveau (N0 auto-créé)
              └─► BomFiche
                    └─► BomFicheLigne ──► Ingredient / BomFiche(N-1)
              └─► BomStock ──────────► Lot (FIFO)
        └─► BomProduction
              └─► BomProductionLigne ──► Lot / BomStock

Ingredient ──► Lot ──► BomFicheLigne / BomProductionLigne
Fournisseur ──► Ingredient
Fournisseur ──► Lot
Stock ──► activites_stocks ──► Activite
```

---

## 7. Glossaire métier

> Définitions des termes métier utilisés dans l'application ArtisaStock et dans ce document. Référence pour toute l'équipe projet.

| Terme | Définition | Entité(s) liée(s) |
|-------|-----------|-------------------|
| **Activité** | Domaine de production de l'artisan (ex : "Chocolaterie", "Pâtisserie", "Glacier"). Sert à segmenter les stocks, contextes et fiches BOM par spécialité. Une activité peut être désactivée sans perte d'historique. | `Activite`, table `activites` |
| **Contexte de production** | Regroupement logique d'une campagne ou d'une gamme de production au sein d'une activité (ex : "Pralines Noël 2026"). Contient les niveaux, fiches BOM et stocks de production associés. | `BomContexte`, table `bom_contextes` |
| **Niveau** | Étape hiérarchique dans la nomenclature d'un contexte. Le niveau N0 (automatique) représente les matières premières (ingrédients). Les niveaux N1, N2… représentent des étapes de production progressives (préparations, assemblages, finitions). | `BomNiveau`, table `bom_niveaux` |
| **N0 (Niveau 0)** | Niveau de base d'un contexte, créé automatiquement à la création du contexte (RG-N0). Représente le stock de matières premières (ingrédients). Non modifiable et non supprimable par l'utilisateur. | `BomNiveau` (ordre=0) |
| **Fiche BOM** | BOM = Bill of Materials (nomenclature de fabrication). Recette de production définissant les inputs (ingrédients ou fiches de niveau N-1) et la quantité produite par batch. Équivalent d'une recette industrielle. | `BomFiche`, `BomFicheLigne`, tables `bom_fiches`, `bom_fiches_lignes` |
| **BOM Explosion** | Opération de décomposition récursive d'une fiche BOM pour calculer la quantité totale de chaque ingrédient de base nécessaire à une production, en traversant tous les niveaux hiérarchiques. | `BomProductionDAL.VerifierDisponibilite()` |
| **Batch** | Unité de production d'une fiche BOM. Chaque batch produit `quantite_output` unités du produit fini. Une production est toujours exprimée en nombre de batches entiers (RG-BATCH). | `BomProduction.nb_batches`, `BomFiche.quantite_output` |
| **Lot** | Unité d'achat d'un ingrédient, enregistrée lors d'un achat fournisseur. Un lot possède une date d'achat, une quantité, un prix et optionnellement une DLC. La gestion FIFO se fait au niveau des lots. | `Lot`, table `lots_ingredients` |
| **FIFO** | First In, First Out — principe de consommation des lots. Les lots achetés en premier sont consommés en premier lors des productions, afin de minimiser les pertes liées à la péremption (RG-FIFO). | `BomProductionDAL`, `LotDAL` |
| **Stock cible** | Quantité idéale d'un ingrédient en stock, paramétrée par l'artisan. Représente le **100 % de la jauge** dans l'interface. Le ratio `stock_actuel / stock_cible` détermine la couleur de la jauge (RG-STOCK-CIBLE : rouge <20%, orange 20-50%, vert 50-100%, bleu >100%). | `Ingredient.stock_cible`, `vue_stock_global` |
| **Seuil d'alerte** | Quantité **minimale acceptable** d'un ingrédient en stock. Si `stock_actuel <= seuil_alerte`, une alerte est déclenchée (ligne rouge dans le DGV, notification Hub). Distinct du stock cible : le seuil est un plancher d'urgence, le stock cible est un objectif. | `Ingredient.seuil_alerte` |
| **DLC** | Date Limite de Consommation d'un lot d'ingrédient. Passée cette date, le lot est considéré périmé et affiché en rouge dans les listes (RG-DLC). | `Lot.dlc`, table `lots_ingredients` |
| **Vue stock global** | Vue MySQL (`vue_stock_global`) agrégeant les stocks disponibles de tous les ingrédients et produits BOM de tous les contextes et activités. Affichée dans le screen "Vue stock global". | `VueStockGlobal`, VIEW `vue_stock_global` |
| **BOM Stock** | Stock d'un produit semi-fini ou fini créé par une production BOM, associé à un niveau et un contexte. Consommé lors des productions de niveau supérieur (FIFO) ou lors de ventes. | `BomStock`, table `bom_stocks` |
| **Réservation** | Pré-allocation d'une quantité de stock pour une production planifiée mais non encore exécutée. Déduite de la disponibilité réelle lors des simulations suivantes (RG-RESERVATION). | `BomReservation`, table `bom_reservations` |
| **Simulation** | Vérification préalable à une production : calcul de la disponibilité de chaque ingrédient nécessaire sans modifier le stock. Génère une liste des pénuries éventuelles. Aucune modification en base de données. | `BomProductionDAL.VerifierDisponibilite()`, `BomManque` |
| **Rapport coût** | Calcul du coût de production d'une fiche BOM pour un nombre de batches donné, basé sur les prix d'achat réels des lots (FIFO). Comprend coût total batch et coût unitaire par pièce. | `RapportCout`, `BomCoutDAL` |
| **Soft delete** | Stratégie de suppression logique : au lieu de supprimer physiquement un enregistrement, le flag `actif` est passé à `0`. L'enregistrement reste en base pour l'historique mais n'apparaît plus dans les listes actives (RG-SOFT-DELETE). | Plusieurs entités — `activites.actif`, `fiches_ingredients.actif` |
| **Guard FK** | Vérification dans la couche DAL de l'absence de dépendances actives avant toute suppression. Si des dépendances existent, une `InvalidOperationException` est levée avec un message décrivant les entités bloquantes (RG-FK-GUARD, SEC-03). | Tous les DAL avec `Delete()` |
| **TypePhysique** | Catégorie physique d'un ingrédient : `Solide`, `Liquide`, `Pièce`. Conditionne l'unité de mesure de référence (g pour solide, ml pour liquide, unité pour pièce) et la visibilité du champ Densité. | `Ingredient.type_physique` |
| **Conditionnement** | Format d'achat d'un ingrédient chez le fournisseur (ex : "Sac 25kg", "Bouteille 1L"). Multiplicateur entre le nombre de conditionnements achetés et la quantité totale reçue en stock. | `Ingredient.conditionnement`, `Ingredient.qte_par_conditionnement` |
| **SPA-style** | Single-Form Application — architecture UI où un seul formulaire principal (`FrmPrincipal`) accueille tous les écrans en tant que UserControls chargés dynamiquement. Aucune fenêtre secondaire n'est ouverte (sauf `FrmLogin` au démarrage). | `FrmPrincipal`, `AppState`, `ScreenRouter` |
| **UserControl** | Composant WinForms réutilisable représentant un écran fonctionnel complet (liste, formulaire d'édition, tableau de bord). Chargé dynamiquement dans `pnlContent` lors de la navigation. | Tous les `FrmXxx` refactorisés en UserControl |
| **Rail gauche** | Barre de navigation fixe (260px) sur le côté gauche de `FrmPrincipal`, organisée en sections : RESSOURCES (statique), ACTIVITÉS (dynamique), CONTEXTES (filtrés), NIVEAUX (filtrés). | `SidebarPanel`, `AppState` |
| **pnlContent** | Panneau central de `FrmPrincipal` dans lequel les UserControls sont chargés dynamiquement via `DockStyle.Fill`. Toute navigation correspond à un `Controls.Clear()` suivi d'un `Controls.Add(userControl)`. | `FrmPrincipal.pnlContent`, `ScreenRouter` |
| **Migration DB** | Script SQL versionné (`v01` à `v14`) créant ou modifiant le schéma de la base de données MySQL. Chaque migration est numérotée et inclut un `DROP/ALTER` inverse documenté. | Scripts `v01` à `v14` |

---

*Document généré le 2026-05-15 — ArtisaStock v1.0 — Livrable BA/FA*
*Mis à jour automatiquement après chaque sprint via le pipeline multi-agents (Agent #1 PO → Agent #2 Architecte → Agent #3 UI/UX → Agent #4 Backend).*
