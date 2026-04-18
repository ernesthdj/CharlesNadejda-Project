# Sauvegarde Session 1 — Charles & Nadejda
> Reprise de contexte pour nouvelle instance Claude
> Date : 30 mars 2026

---

## Contexte du projet

Projet académique double répondant à deux examens (défense orale fin juin 2026) :

| Examen | Techno | Minimum requis |
|--------|--------|----------------|
| **PDWEB** | Laravel 11 + PHP 8.3 + MySQL | PHP, sessions, formulaires, MySQL, AJAX |
| **PDSGBD** | C# Windows Forms + MySQL | CRUD complet, jointures, requêtes paramétrées, POO |

Les deux apps partagent la **même base MySQL** `charlesnadejda` (Docker).
**Sujet** : Site pâtisserie-chocolaterie artisanale + ERP (Enterprise Resource Planning — logiciel de gestion intégré) desktop pour les parents (Charles & Nadejda, Bruxelles).

---

## Infrastructure (Phase 0 — TERMINÉE ✅)

### Docker (4 containers actifs)
```
docker compose up -d   ← depuis la racine du projet
```
| Service | URL / Port |
|---------|------------|
| MySQL 8 | localhost:3306 (root / root) |
| Laravel (PHP-FPM) | interne |
| Nginx | http://localhost:8000 |
| phpMyAdmin | http://localhost:8080 |

### Laravel
- Scaffoldé dans `site-laravel/` (Laravel 13.2, PHP 8.3)
- `.env` configuré : `DB_HOST=mysql`, `DB_DATABASE=charlesnadejda`
- `php artisan migrate` exécuté

### App C# Windows Forms
- Solution dans `app-csharp/CharlesNadejda/` — .NET Framework 4.8.1
- NuGet : `MySql.Data`, `BCrypt.Net-Next`, `Newtonsoft.Json`
- Credentials de test : `charles@charlesnadejda.be` / `password` / role `admin`

---

## État du code C# (Phases 1, 2, 7 partielles — TERMINÉES ✅)

### Models implémentés
| Fichier | Table |
|---------|-------|
| `Utilisateur.cs` | utilisateurs |
| `Categorie.cs` | categories |
| `Produit.cs` | produits |
| `Parfum.cs` | parfums |
| `Fournisseur.cs` | fournisseurs |
| `Ingredient.cs` | fiches_ingredients (**mis à jour** : +TypePhysique, +Densite) |
| `Recette.cs` | fiches_recettes (ancien système rigide) |
| `RecetteIngredient.cs` | recettes_ingredients |
| `Lot.cs` | lots_ingredients |

### DAL (Data Access Layer — couche d'accès aux données) implémentés
`DbHelper`, `UtilisateurDAL`, `CategorieDAL`, `ProduitDAL`, `ParfumDAL`, `FournisseurDAL`, `IngredientDAL`, `RecetteDAL`, `LotDAL`

### Forms implémentés
`FrmLogin`, `FrmPrincipal` (menu complet avec AfficherAVenir pour modules futurs),
`FrmCategories/Edit`, `FrmProduits/Edit`, `FrmParfums/Edit`,
`FrmFournisseurs/Edit`, `FrmIngredients/Edit`, `FrmRecettes/Edit`, `FrmAchats/Edit`

---

## Architecture BOM (Bill of Materials — nomenclature de production) Générique — NOUVEAU ✅

### Décision architecturale (session du 30 mars 2026)
Remplacement du système de production rigide (3 niveaux codés en dur) par un
**BOM (Bill of Materials) générique à profondeur variable**.

### Principe
```
Stock Ingrédients (global, partagé entre tous les contextes)
       ↑ consommé par
Contexte A ─── Niveau 1 (nommé par artisan) ─── Niveau 2 ─── Niveau N
               stock propre                      stock propre   stock propre
```

### Règles métier
1. Niveau 0 = stock ingrédients global
2. Chaque contexte naît avec 1 niveau par défaut (vide, à configurer)
3. Niveaux ajoutables librement, supprimables uniquement par le haut (jamais intermédiaire)
4. Strict : niveau N consomme UNIQUEMENT niveau N-1 (pas de saut)
5. Fiches = templates globaux réutilisables entre contextes
6. Production = valide dispo N-1 → consomme → crée stock N
7. Si manque → liste détaillée de ce qui manque + quantités
8. Rapport BOM explosé : composition complète tous niveaux + export PDF
9. Simulation : écran séparé FrmSimulation
10. Conversions d'unités : automatiques (masse ↔ volume via densité)
11. Stock partagé + réservations : table `bom_reservations` (Option C)
    → dispo réelle = lot.quantite_disponible - SUM(reservations actives)
12. Choco vs Pâtisserie : même moteur, contextes séparés par activité

---

## Base de données — État actuel (50 tables)

### Migration v4 appliquée ✅ (30 mars 2026)
- `fiches_ingredients` : +`type_physique`, +`densite`, `unite_mesure` étendu (+cl)
- `lots_ingredients` : +`prix_unitaire`
- 8 nouvelles tables BOM créées

### Tables BOM (préfixe `bom_`)
| Table | Rôle |
|-------|------|
| `bom_contextes` | Configs de production nommées (chocolaterie/patisserie) |
| `bom_niveaux` | Niveaux ordonnés dans un contexte |
| `bom_fiches` | Templates de recettes globaux réutilisables |
| `bom_fiches_lignes` | Composition d'une fiche (inputs = ingrédient ou autre fiche) |
| `bom_productions` | Log d'exécution d'une fiche dans un contexte |
| `bom_stocks` | Stock par niveau × fiche (lot de production) |
| `bom_productions_lignes` | Traçabilité détaillée des consommations |
| `bom_reservations` | Réservations sur lots d'ingrédients par contexte |

### Note importante
Les CHECK constraints sur colonnes FK sont interdites en MySQL 8.
La validation `type_input ↔ id_input_*` et `type_source ↔ id_source_*`
est enforced dans les DAL C#.

---

## Models BOM créés ✅ (session du 30 mars 2026)

| Fichier | Table | Propriétés calculées |
|---------|-------|----------------------|
| `BomContexte.cs` | bom_contextes | — |
| `BomNiveau.cs` | bom_niveaux | — |
| `BomFiche.cs` | bom_fiches | `CoutBatch`, `CoutUnitaire` |
| `BomFicheLigne.cs` | bom_fiches_lignes | `SousTotal` |
| `BomProduction.cs` | bom_productions | — |
| `BomStock.cs` | bom_stocks | `EstPerime` |
| `BomProductionLigne.cs` | bom_productions_lignes | `SousTotal` |
| `BomReservation.cs` | bom_reservations | — |

---

## Prochaine étape — DAL (Data Access Layer) BOM

Ordre d'implémentation :
1. `BomContexteDAL` — CRUD (Create, Read, Update, Delete) contextes
2. `BomNiveauDAL` — CRUD niveaux + validation suppression par le haut
3. `BomFicheDAL` — CRUD fiches + chargement lignes
4. `BomFicheLigneDAL` — CRUD lignes + validation type_input
5. `BomStockDAL` — lecture stock par niveau, calcul dispo réelle (- réservations)
6. `BomProductionDAL` — exécution production (transaction multi-tables)
7. `BomReservationDAL` — CRUD réservations

---

## Règles importantes (ne jamais déroger)

1. Toutes requêtes SQL → `cmd.Parameters.AddWithValue` — jamais de concaténation
2. Toute opération multi-tables → `MySqlTransaction` (commit/rollback)
3. Disponible réel lot = `quantite_disponible - SUM(bom_reservations WHERE actif=1)`
4. Suppression niveau → vérifier `MAX(ordre)` du contexte avant DELETE
5. Validation `type_input` dans BomFicheLigneDAL (CHECK non disponible en MySQL 8 sur FK)
6. FIFO sur les lots à la consommation (ORDER BY date_achat ASC)
7. **Abréviations** : toujours expliquer à la première utilisation (règle CLAUDE.md)

---

## Structure dossiers
```
CharlesNadejda_Project/
├── docs/                    ← Documentation complète (00 à 09)
├── sql/
│   ├── create_database.sql  ✅ (v4 — 50 tables)
│   ├── seed_data.sql        ✅
│   └── migration_v4_bom.sql ✅
├── docker-compose.yml       ✅
├── site-laravel/            ✅ Laravel scaffoldé (phases 3-8 non démarrées)
├── sauvegarde0.md           ✅ Session 0
├── sauvegarde1.md           ← CE FICHIER
└── app-csharp/CharlesNadejda/
    ├── Models/              ✅ 9 anciens + 8 BOM
    ├── DAL/                 ✅ 9 anciens — BOM à faire
    └── Forms/               ✅ existants — BOM forms à faire
```
