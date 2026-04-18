# 05 — Intégration d'ArtisaStock dans le Projet Charles & Nadejda

> Ce document analyse ce qu'on récupère d'ArtisaStock, ce qu'on adapte,
> et comment les concepts s'intègrent aux contraintes des deux examens.

---

## ⚠️ Note importante sur la stack technique

ArtisaStock avait une ancienne recommandation interne ("Architecture Révisée") qui proposait
d'abandonner PHP/Laravel au profit du full .NET. **Cette recommandation ne s'applique pas ici.**

- **Laravel EST du PHP** — le professeur accepte Laravel tant que c'est du PHP. ✅
- **Windows Forms + MySql.Data** est la stack exigée pour PDSGBD (inchangé).
- Laravel apporte MVC, Eloquent, Blade, Auth middleware — c'est un bon choix pour structurer
  le site et prendre de l'avance sur les modules PHP avancés de **3ème année**.
- Pour l'oral PDWEB, le prof interroge sur l'implémentation du projet — pas de questions pièges sur le raw PHP.

Le projet actuel : **Laravel (PHP 8+)** pour le site, **C# Windows Forms + MySql.Data** pour l'app artisan. Les deux partagent une **base MySQL**.

---

## 🔍 Ce que j'ai trouvé dans ArtisaStock

### Concepts métier (à garder intégralement)
Le modèle conceptuel d'ArtisaStock est excellent et correspond exactement au besoin :

| Concept ArtisaStock | Ce que ça devient dans notre projet |
|---------------------|-------------------------------------|
| **Ingredient** (matière première) | Même chose : chocolat, crème, praliné, sucre... |
| **Recipe** (recette) | Recette d'une praline, d'un entremet, d'un ballotin |
| **Production** | L'artisan enregistre ce qu'il a fabriqué ce jour |
| **StockMovement** | Historique de tous les mouvements (achat, production, perte) |
| **AlertThreshold** | Alerte quand le chocolat 70% est presque épuisé |
| **CostCalculation** | Calcul automatique du coût de revient d'une praline |

### Stack technique (adaptée aux examens)

| ArtisaStock original | Pourquoi on adapte | Ce qu'on utilise |
|---------------------|-------------------|-----------------|
| ASP.NET Core Web API | Hors scope PDSGBD | MySql.Data + requêtes préparées directement |
| Entity Framework Core | Hors scope PDSGBD | DAL manuelle (exigée par le cours) |
| PostgreSQL + JSONB | PDSGBD exige MySQL | MySQL via XAMPP |
| React + TypeScript | Hors scope PDWEB | PHP procédural + PDO (exigé par le cours) |
| Clean Architecture (4 couches) | Trop complexe pour l'exam | Architecture simple : Forms + DAL |
| GUID comme PK | PostgreSQL uniquement | INT AUTO_INCREMENT |
| Docker + CI/CD | Hors scope | XAMPP en local |
| Laravel | Pas pour l'exam actuel (PHP procédural requis) | À explorer en 3ème année |

---

## 🎯 Comment ArtisaStock répond aux critères d'examen

### Pour PDSGBD (C# Windows Forms)

Les User Stories d'ArtisaStock se mappent directement sur les critères notés :

| User Story ArtisaStock | Critère PDSGBD couvert | Points |
|------------------------|------------------------|--------|
| US-1.1 Créer ingrédient | INSERT + validation données + unicité | /5 + /10 + /5 |
| US-1.2 Lister ingrédients | Affichage liste + tri + filtrage | /5 + /5 + /5 |
| US-1.3 Modifier ingrédient | UPDATE + unicité à la modif | /5 + /10 |
| US-1.4 Supprimer ingrédient | DELETE + vérif avant suppression | /5 + /5 |
| US-2.1 Créer recette | Contrôles édition + sélection + INSERT | /10 + /10 |
| US-2.3 Visualiser coût revient | Affichage liste avec FK (recette + ingrédients) | /10 |
| US-3.1 Valider production | Jointures multiples + logique métier | /10 |
| US-3.2 Lancer production | INSERT production + UPDATE stock (cascade logique) | /5 + /5 |
| US-4.1 Enregistrer achat stock | INSERT mouvement + UPDATE stock | /5 |
| US-4.3 Historique mouvements | Affichage liste avec FK + filtre période | /10 + /5 |

C'est exactement ce que le prof veut voir. **ArtisaStock remplit quasi entièrement la grille du PDSGBD** une fois adapté en Windows Forms + MySQL.

### Pour PDWEB (PHP)

L'intégration crée une interaction dynamique entre l'app C# et le site PHP :

- L'artisan crée un produit dans l'app C# → il apparaît automatiquement sur le site PHP
- L'artisan marque un produit comme "disponible = 0" → il disparaît de la boutique
- Quand une commande est passée, l'admin peut déclencher une production dans C#
- Le stock d'ingrédients se met à jour → le site peut afficher les stocks en temps réel

---

## 🏗️ Architecture (vision finale)

```
┌──────────────────────────────────────────────────────────────────┐
│                         BASE MYSQL COMMUNE                        │
│                                                                    │
│  [ingredients] [recettes] [recettes_ingredients] [productions]    │
│  [mouvements_stock]                                               │
│        ↕                           ↕                              │
│  [produits] ←→ [categories]   [parfums] [produits_parfums]       │
│        ↕                                                          │
│  [commandes] [lignes_commandes] [selections_parfums]              │
│  [utilisateurs] [contacts]                                        │
└──────────────────────────────────────────────────────────────────┘
         ▲                                    ▲
         │                                    │
┌────────┴────────┐                 ┌─────────┴────────┐
│   App C# (WinForms)               │   Site PHP        │
│   ARTISASTOCK                     │   BOUTIQUE        │
│                 │                 │                   │
│ Gère :          │                 │ Gère :            │
│ • Ingrédients   │  Base MySQL     │ • Catalogue       │
│ • Recettes      │  partagée       │ • Panier          │
│ • Productions   │ ←─────────────► │ • Commandes       │
│ • Stock         │                 │ • Clients         │
│ • Catalogue     │                 │ • Contact         │
│   (admin)       │                 │ • Espace admin    │
└─────────────────┘                 └───────────────────┘
```

### Ce que fait l'app C# (Artisastock adapté)
**Utilisateurs** : Charles et Nadejda (les artisans, en local)

Module 1 — **Catalogue boutique** (impact direct sur le site PHP)
- Gérer les catégories (Ballotins, Pâtisseries, Créations...)
- Créer/modifier/supprimer des produits
- Activer/désactiver un produit → apparaît/disparaît du site
- Gérer les parfums disponibles par produit

Module 2 — **Ingrédients & Stock**
- CRUD ingrédients avec unités, prix, seuil d'alerte
- Enregistrer les achats de matières premières
- Ajustements d'inventaire
- Historique des mouvements

Module 3 — **Recettes**
- Créer une recette "Praline Caramel" = x g chocolat lait + y g crème + z g caramel
- Calcul automatique du coût de revient
- Dupliquer une recette pour créer une variante
- Lier une recette à un produit du catalogue

Module 4 — **Production**
- Valider si on peut produire N unités (vérification stock)
- Enregistrer une production → déstocke les ingrédients automatiquement
- Historique des productions avec coûts et marges

Module 5 — **Commandes** (lecture des commandes du site)
- Consulter les commandes passées en ligne
- Changer le statut (en préparation, prête, livrée)

### Ce que fait le site PHP (boutique)
**Utilisateurs** : Les clients + Charles & Nadejda pour les commandes

- Catalogue dynamique (ce que l'app C# a mis en ligne)
- Configurateur de ballotins (parfums en session)
- Panier + commande + confirmation
- Espace client (historique)
- Admin PHP light : voir commandes + messages contact

---

## 📊 Schéma des nouvelles tables (à ajouter à la BDD)

Ces 5 tables s'ajoutent au schéma existant défini dans `01_BASE_DE_DONNEES.md` :

```
ingredients
    id (PK)
    nom (UNIQUE)
    marque
    unite_mesure  [kg | g | l | ml | piece]
    prix_unitaire (DECIMAL 10,4)
    stock_actuel  (DECIMAL 10,4)
    seuil_alerte  (DECIMAL 10,4, nullable)
    id_fournisseur (FK optionnel → fournisseurs)
    date_creation


recettes
    id (PK)
    nom (UNIQUE)
    description
    rendement_quantite  → combien de pièces une batch produit
    temps_preparation   → en minutes
    active [0|1]


recettes_ingredients   (table de liaison)
    id_recette  (FK → recettes, CASCADE DELETE)
    id_ingredient (FK → ingredients, RESTRICT)
    quantite    (DECIMAL 10,4)
    PRIMARY KEY (id_recette, id_ingredient)


productions
    id (PK)
    id_recette (FK → recettes, RESTRICT)
    date_production
    quantite_produite
    cout_total         → calculé au moment de la production
    cout_unitaire
    prix_vente_reel    → optionnel, pour calculer la marge
    marge_brute        → calculée si prix_vente_reel renseigné
    notes


mouvements_stock
    id (PK)
    id_ingredient (FK → ingredients, RESTRICT)
    id_production (FK → productions, nullable, SET NULL)
    type_mouvement  [entree | sortie | ajustement]
    quantite
    prix_unitaire_moment  → prix au moment du mouvement (historique)
    motif
    reference    → ex: n° facture fournisseur
    date_mouvement
```

**Modification sur la table `produits` existante** :
Ajouter une colonne `id_recette INT DEFAULT NULL` (FK optionnelle vers recettes)
→ Quand un produit a une recette liée, l'app C# peut calculer son coût de revient et déclencher une production.

---

## ✂️ Ce qu'on simplifie par rapport à ArtisaStock original

Ces simplifications sont **justifiées** dans le contexte académique :

| Fonctionnalité ArtisaStock | Pourquoi on simplifie | Version simplifiée |
|---------------------------|----------------------|-------------------|
| GUID comme IDs | MySQL préfère AUTO_INCREMENT | INT AUTO_INCREMENT |
| Metadata JSONB extensible | PostgreSQL only | Champs fixes (simplifie aussi la saisie) |
| Duplication de recette (US-2.5) | Bonus si le temps le permet | À ajouter en dernier |
| Export CSV (US-4.3) | Hors exam | Optionnel |
| Dashboard avec graphiques (US-5.1) | Hors exam Windows Forms | Tableau de bord simple |
| API REST + React frontend | Remplacé par Laravel + Blade (PHP, accepté par prof) | Site Laravel + Eloquent |

---

## 🗺️ Ordre de développement recommandé

### Phase 1 — La base (fait)
✅ Schéma de base de données complet
✅ Documentation du projet

### Phase 2 — App C# : modules catalogue + ingrédients (PDSGBD)
1. Solution Visual Studio + connexion MySQL
2. `FrmPrincipal` avec navigation par menus
3. `FrmCategories` + `FrmCategorieEdit` — CRUD simple pour s'échauffer
4. `FrmIngredients` + `FrmIngredientEdit` — CRUD complet avec validation, unicité, alerte stock
5. `FrmProduits` + `FrmProduitEdit` — CRUD avec ComboBox catégorie, CheckedListBox parfums

### Phase 3 — Site Laravel de base (PDWEB)
1. `composer create-project laravel/laravel site-laravel` + config `.env` BDD
2. Modèles Eloquent (Produit, Categorie, Parfum, User, Commande...)
3. Routes + contrôleurs : catalogue dynamique, auth (Login/Register), panier session
4. Vues Blade : layout, catalogue, configurateur parfums

### Phase 4 — App C# : recettes + production (PDSGBD enrichi)
1. `FrmRecettes` + `FrmRecetteEdit` (avec ajout dynamique d'ingrédients → combobox + bouton +)
2. Calcul du coût de revient en temps réel (événement Change)
3. `FrmProduction` — valider + lancer une production
4. `FrmMouvementsStock` — historique avec filtres

### Phase 5 — Site Laravel complet (PDWEB)
1. Commande + confirmation (CommandeController + CommandeRequest)
2. Espace client (CompteController) + Admin Laravel (routes admin + middleware)
3. Ajax : routes JSON dans web.php + fetch() côté JS (panier + filtres catalogue)

### Phase 6 — Intégration & finitions
1. Tester que les actions C# se reflètent sur le site PHP
2. Préparer les scénarios de démo pour les deux examens
