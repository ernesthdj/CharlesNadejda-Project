# 00 — Contexte & Vision Globale du Projet

> **Fichier de recontextualisation** — À lire en début de session pour reprendre le travail là où on l'a laissé.

---

## 🎯 Objectif du Projet

Réaliser un projet académique double qui répond simultanément aux deux modalités d'examen :

| Cours | Modalité | Technos obligatoires |
|-------|----------|----------------------|
| **PDWEB** | Site web dynamique | PHP (Laravel accepté), MySQL, Sessions, Formulaires, Ajax |
| **PDSGBD** | Application desktop de gestion | C# (Windows Forms), MySQL, POO |

Le projet est construit autour d'un **site de pâtisserie-chocolaterie artisanale** pour les parents de l'étudiant (Charles & Nadejda), à Bruxelles.

---

## 🏗️ Architecture Globale

```
┌─────────────────────────────────────────────────────┐
│                   BASE MYSQL COMMUNE                │
│         (partagée par les deux applications)        │
└──────────────────┬──────────────────────────────────┘
                   │
       ┌───────────┴───────────┐
       │                       │
┌──────▼──────┐         ┌──────▼──────┐
│  Site PHP   │         │  App C#     │
│  (PDWEB)    │         │  (PDSGBD)   │
│             │         │             │
│ → clients   │         │ → artisans  │
│ → visiteurs │         │ → admin     │
└─────────────┘         └─────────────┘
```

### Application 1 — Site Web PHP (PDWEB)
**Audience** : Clients du magasin + admins (Charles & Nadejda)
**Rôle** : Site vitrine + système de commande en ligne
- Catalogue de produits (chocolats, pâtisseries)
- Configurateur de ballotins (choix des parfums)
- Panier persistant via sessions PHP
- Espace client (historique commandes)
- Espace admin (gestion commandes, messages)
- Formulaires : inscription, connexion, commande, contact
- Ajax : ajout panier, filtres catalogue, recherche

### Application 2 — Application C# Windows Forms (PDSGBD)
**Audience** : Les artisans eux-mêmes (usage interne)
**Rôle** : Back-office de gestion du catalogue et des commandes
- Gestion CRUD des produits et catégories
- Gestion CRUD des parfums
- Gestion des commandes (consultation, changement statut)
- Gestion des clients
- Sécurité anti-injection SQL
- Intégrité référentielle gérée

---

## 📂 Structure des Fichiers du Projet

```
CharlesNadejda_Project/
├── docs/                          ← Documentation (CE DOSSIER)
│   ├── 00_CONTEXTE_PROJET.md     ← Ce fichier
│   ├── 01_BASE_DE_DONNEES.md     ← Schéma MySQL complet
│   ├── 02_PLAN_PDWEB.md          ← Plan du site Laravel
│   ├── 03_PLAN_PDSGBD.md         ← Plan de l'app C#
│   ├── 04_GRILLES_EXAMEN.md      ← Mapping critères ↔ fonctionnalités
│   ├── 05_ARTISASTOCK_INTEGRATION.md ← Intégration concepts ArtisaStock
│   ├── 06_EXERCICES_PROF_PDWEB.md    ← Analyse exercices + framework .pdweb.php
│   └── 07_DOCKER_SETUP.md            ← Config Docker Desktop (MySQL + Laravel)
│
├── charles-nadejda-projet/        ← Ancien projet Next.js (archivé, ne pas toucher)
│   └── charles-nadejda/
│
├── site-laravel/                  ← (À CRÉER) Site PHP Laravel - PDWEB
│   ├── app/Http/Controllers/
│   ├── app/Models/
│   ├── resources/views/
│   ├── routes/web.php
│   └── database/migrations/
│
├── app-csharp/                    ← (À CRÉER) Application C# - PDSGBD
│   └── CharlesNadejda.sln
│
└── sql/                           ← (À CRÉER) Scripts SQL
    ├── create_database.sql
    └── seed_data.sql
```

---

## 🔑 Ce qu'on récupère de l'ancien projet Next.js

| Élément | Réutilisable ? | Comment |
|---------|---------------|---------|
| Code React/Next.js | ❌ Non | Mauvaise techno pour les examens |
| Charte graphique (couleurs, fonts) | ✅ Oui | Reproduire en CSS pur dans le site PHP |
| Catalogue produits (15 parfums, produits) | ✅ Oui | Injecter comme données SQL (`seed_data.sql`) |
| Logique configurateur | ✅ Oui | Reimplémenter en PHP + Ajax |
| Structure des pages | ✅ Oui | Adapter en PHP |
| Design du panier/checkout | ✅ Oui | Adapter avec sessions PHP |

---

## 🎨 Charte Graphique (à reprendre)

```css
--chocolat-fonce:  #3D2817  /* Textes, titres */
--chocolat-moyen:  #6F4E37  /* Éléments interactifs */
--creme:           #F5E6D3  /* Fonds clairs */
--or:              #D4AF37  /* Accents premium */
--rouge-framboise: #C72C48  /* Boutons CTA */
```

**Typographie** : Playfair Display (titres) + Inter (corps)
→ Disponibles sur Google Fonts

---

## 📦 Catalogue Produits (données de seed)

### Catégories
- Ballotins
- Boules de Noël (saisonniers)
- Créations Originales
- Pâtisseries (à développer)

### Produits configurables (avec choix de parfums)
| Produit | Capacité | Prix TTC |
|---------|----------|----------|
| Ballotin 250g | 25 chocolats | 30,00 € |
| Ballotin 500g | 50 chocolats | 60,00 € |
| Boule Noël 220g | 20 chocolats | 35,00 € |
| Boule Noël 500g | 45 chocolats | 70,00 € |

### Créations fixes (sans configuration)
| Produit | Prix TTC |
|---------|----------|
| Chaussure en chocolat | 35,00 € |
| Bouteille champagne chocolat | 35,00 € |

### 15 Parfums disponibles
Praliné, Caramel, Noir 70%, Lait, Blanc, Café, Noisette, Orange, Framboise, Citron, Passion, Rhum Raisin, Champagne, Grand Marnier, Fleur de sel

---

## 📅 État d'Avancement

| Phase | Statut |
|-------|--------|
| Définition du projet | ✅ Terminé |
| Documentation (ce dossier) | ✅ Terminé (v2 avec ArtisaStock) |
| Schéma BDD MySQL (boutique + ArtisaStock) | ✅ Terminé |
| Script SQL création + seed | ✅ Dans `01_BASE_DE_DONNEES.md` |
| Plan site Laravel (PDWEB) | ✅ Dans `02_PLAN_PDWEB.md` |
| Plan app C# (PDSGBD + ArtisaStock) | ✅ Dans `03_PLAN_PDSGBD.md` |
| Intégration ArtisaStock | ✅ Dans `05_ARTISASTOCK_INTEGRATION.md` |
| Implémentation app C# | ⏳ À faire |
| Implémentation site PHP | ⏳ À faire |

---

## 📝 Notes pour Claude (recontextualisation)

- L'étudiant est en **2ème année de Bachelier IT**, orientation Business Analyst
- Les deux fichiers de modalités d'examen sont dans `/docs/../modalites_examen.20250825.txt` (PDWEB) et `modalites_examen.20250825 (1).txt` (PDSGBD)
- L'étudiant avait un projet antérieur **ArtisaStock** (gestion stock pour artisans) — ses concepts sont intégrés dans la couche C# (ingrédients, recettes, productions, mouvements stock)
- Le dossier `Project ArtisaStock/Doc ArtisaStock/` contient l'ancienne doc pour référence
- Le prof de PDWEB peut fournir des exercices → à intégrer dans `docs/06_EXERCICES_PROF_PDWEB.md` quand disponibles
- Priorité : **documenter d'abord**, coder ensuite
- **Environnement local : Docker Desktop** (MySQL + Laravel dans des containers, voir `07_DOCKER_SETUP.md`)
- Le prof utilise WAMP Server — ce n'est pas un problème, la BDD MySQL est la même, seul l'hébergement local diffère
- L'app C# Windows Forms tourne **nativement sur Windows** (pas dans Docker), elle se connecte au container MySQL via `localhost:3306`
- Stack C# : Windows Forms + MySql.Data (NuGet) — PAS d'ASP.NET Core, PAS d'EF Core
- Stack PHP : **Laravel** (PHP 8+) — le prof accepte Laravel tant que c'est du PHP. Laravel = MVC, Eloquent, Blade, Auth middleware. L'oral porte sur l'implémentation du projet, pas sur des questions pièges de raw PHP.
- Laravel est aussi un bon investissement pour la 3ème année (modules PHP avancés)
- **Langue du site** : Français + Anglais (i18n Laravel) — les noms de produits restent en FR dans la BDD
- **Services tiers** :
  - **Cloudinary** : stockage images produits (upload depuis app C#, URL stockée en DB)
  - **Stripe** : paiement Bancontact en mode test pour l'exam (webhook + facture auto)
  - Stripe CLI ou ngrok requis pour les webhooks en local (voir `07_DOCKER_SETUP.md`)
- **Livraison** : retrait magasin (gratuit) + livraison Belgique (6,95 €) + Nord France (9,95 €)
- **Règle configurateur** : total sélectionné doit être EXACTEMENT égal à `capacite_max` du produit (19/39/8/19 selon le produit). Validé côté PHP ET JS.
- **Prix exacts** (source : lib/products.js ancien site) : Ballotin 250g=13€/19 chocs, 500g=23€/39 chocs, Boule 220g=15€/8 chocs, Boule 500g=29€/19 chocs, Chaussure=15€, Bouteille=15€, Pâte tartiner=10€. TVA 21%.
- **Écran login C#** : l'app Windows Forms démarre sur FrmLogin, vérifie role='admin' dans la BDD, hash BCrypt
