# Charles & Nadejda — Pâtisserie Chocolaterie Artisanale

Projet académique Bachelier IT (2e année) — répondant simultanément à deux modalités d'examen :

| Examen | Technologie | Description |
|--------|-------------|-------------|
| **PDWEB** | Laravel (PHP 8.3) + MySQL | Site vitrine + boutique en ligne avec paiement Bancontact |
| **PDSGBD** | C# Windows Forms + MySQL | Application de gestion artisanale (catalogue, stock, productions) |

Les deux applications partagent la **même base de données MySQL** `charlesnadejda`.

---

## 🚀 Démarrage rapide

```bash
# 1. Démarrer l'infrastructure (MySQL + Laravel + phpMyAdmin)
docker-compose up -d --build

# 2. Accès
#    Site Laravel  → http://localhost:8000
#    phpMyAdmin    → http://localhost:8080  (root / root)

# 3. Scaffolder Laravel (première fois uniquement)
docker-compose exec app composer create-project laravel/laravel .
cp site-laravel/.env.example site-laravel/.env
docker-compose exec app php artisan key:generate
```

L'application C# (Visual Studio 2022) se connecte à MySQL via `localhost:3306`.

---

## 📁 Structure

```
├── docs/                   Documentation complète (00 → 08)
├── sql/
│   ├── create_database.sql  Schéma complet (16 tables)
│   └── seed_data.sql        Données de test
├── docker-compose.yml       Infrastructure Docker
├── docker/                  Dockerfile PHP + config Nginx
├── site-laravel/            Projet Laravel (PDWEB)
├── app-csharp/              Projet C# Windows Forms (PDSGBD) — à créer
└── .zed/                    Configuration Zed Editor
```

---

## 📚 Documentation

| Fichier | Contenu |
|---------|---------|
| `docs/00_CONTEXTE_PROJET.md` | Vue d'ensemble, stack, décisions techniques |
| `docs/01_BASE_DE_DONNEES.md` | Schéma MySQL complet + requêtes clés |
| `docs/02_PLAN_PDWEB.md` | Plan complet site Laravel |
| `docs/03_PLAN_PDSGBD.md` | Plan complet app C# Windows Forms |
| `docs/04_GRILLES_EXAMEN.md` | Mapping critères examen ↔ implémentation |
| `docs/05_ARTISASTOCK_INTEGRATION.md` | Module gestion artisanale (ingrédients, recettes, stock) |
| `docs/06_EXERCICES_PROF_PDWEB.md` | Exercices prof PDWEB analysés |
| `docs/07_DOCKER_SETUP.md` | Setup Docker détaillé |
| `docs/08_PLAN_IMPLEMENTATION.md` | **Plan d'implémentation par phases — À LIRE EN PREMIER** |
| `docs/09_PATISSERIE_ARCHITECTURE.md` | Architecture module pâtisserie (règles métier, scaling, devis, configurateur) |

---

## 🛠️ Stack technique

- **Backend web** : Laravel 11 + PHP 8.3-FPM
- **App desktop** : C# .NET + Windows Forms
- **Base de données** : MySQL 8 (partagée)
- **Paiement** : Stripe / Bancontact (mode test)
- **Images** : Cloudinary (upload C#, URL en DB)
- **Auth** : BCrypt (compatible PHP `password_hash` ↔ C# `BCrypt.Net-Next`)
- **i18n** : Français + Anglais
- **IDE Web** : Zed Editor
- **IDE Desktop** : Visual Studio 2022

---

*Projet pour Charles & Nadejda — Bruxelles*
