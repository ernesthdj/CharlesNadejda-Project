# Audit Qualité Global — ArtisaStock
> **Date :** 2026-07-02
> **Périmètre :** C# WinForms · Laravel 11 · MySQL 8 · Sécurité OWASP Top 10
> **Méthodologie :** Analyse statique par lecture exhaustive du code source (2 agents Explore parallèles)
> **Version logicielle :** C# .NET Framework 4.8.1 · Laravel 11 / PHP 8.3 · MySQL 8.0 · Docker Compose
> **Branche analysée :** `feat/refactoring-sprints-p0-p3`

---

## 1. Synthèse exécutive

| Couche | Score | Verdict |
|--------|-------|---------|
| C# WinForms (DAL + Forms) | **7 / 10** | Bon socle, 2 critiques bloquants |
| Laravel (Controllers + Blade) | **4.7 / 5** | Quasi-exemplaire, 1 timing leak |
| SQL / Migrations | **5 / 5** | Schéma excellent, zéro dette structurelle |
| Sécurité OWASP (global) | **8 / 10** | Safe sur injection/XSS, credential leak critique |

**Score global estimé : 7.5 / 10**

### Findings par sévérité

| Sévérité | C# | Laravel | SQL | Total |
|----------|----|---------|-----|-------|
| CRITICAL | 2  | 0       | 0   | **2** |
| WARNING  | 3  | 1       | 0   | **4** |
| INFO     | 13 | 2       | 0   | **15**|
| **Total**| 18 | 3       | 0   | **21**|

### Ticket P0 immédiat (bloquants avant démo)

| ID | Description | Sévérité | Statut |
|----|-------------|----------|--------|
| **SEC-1** | `App.config` — `Uid=root;Pwd=root` en clair, versionné en git | CRITICAL | ✅ RÉSOLU (commit 2232ead, mai 2026) |
| **SEC-2** | `FrmLogin.cs:21-24` — bloc `#if DEBUG` avec credentials tracké en git | CRITICAL | ✅ RÉSOLU (2026-07-02) |

---

## 2. Findings C# WinForms

### 2.1 Sécurité (CRITICAL)

#### SEC-1 — Credentials DB en dur dans App.config ✅ RÉSOLU
- **Fichier :** `app-csharp/CharlesNadejda/CharlesNadejda/App.config`
- **Sévérité :** CRITICAL → **FERMÉ**
- **Résolution (commit 2232ead — 2026-05-26) :** `App.config` retiré du tracking git, `.gitignore` mis à jour (`app-csharp/**/App.config`), `App.config.example` créé avec placeholders `REMPLACER`. Gap résiduel corrigé le 2026-07-02 : ajout de la clé `LaravelStoragePath` dans `App.config.example`.
- **État actuel :** Aucun credential en git. Le fichier local `App.config` (dev Docker) reste sur la machine uniquement.

#### SEC-2 — Bloc #if DEBUG avec credentials dans FrmLogin.cs ✅ RÉSOLU
- **Fichier :** `app-csharp/CharlesNadejda/CharlesNadejda/Forms/FrmLogin.cs:21-24`
- **Sévérité :** CRITICAL → **FERMÉ**
- **Description originale :** Bloc `#if DEBUG` pré-remplissant `txtEmail` (`charles@charlesnadejda.be`) et `txtMotDePasse` (`password`) — credential réel valide en DB locale, versionné en git.
- **Résolution (2026-07-02) :** Bloc `#if DEBUG` supprimé intégralement de `FrmLogin.cs`. Le constructeur ne contient plus que `InitializeComponent()`. Les credentials de test doivent désormais être saisis manuellement en dev.

---

### 2.2 DAL — Data Access Layer (WARNING)

#### DAL-1 — Castings directs sans guard DBNull
- **Fichiers :** `DAL/IngredientDAL.cs`, `DAL/LotDAL.cs`, `DAL/ProductionDAL.cs`, `DAL/CommandeDAL.cs` (et ~8 autres DAL)
- **Sévérité :** WARNING
- **Occurrences estimées :** 15–20 lignes dans l'ensemble des DAL
- **Description :** Les colonnes nullable de la DB sont castées directement : `(decimal)r["prix_unitaire"]`, `(int)r["id_stock"]`, etc. Si la valeur SQL est `NULL`, le cast lève une `InvalidCastException` non catchée, crashant silencieusement le formulaire parent.
- **Pattern problématique :**
  ```csharp
  // Dangereux — crash si NULL en DB
  ingredient.PrixUnitaire = (decimal)r["prix_unitaire"];
  ```
- **Recommandation :** Utiliser un helper guard systématique :
  ```csharp
  // Sécurisé
  ingredient.PrixUnitaire = r["prix_unitaire"] == DBNull.Value
      ? 0m : (decimal)r["prix_unitaire"];
  ```
  Ou créer une méthode d'extension `GetDecimalOrDefault(this IDataRecord r, string col)`.

---

### 2.3 GDI+ — Ressources graphiques (WARNING)

#### GDI-1 — Allocations Font non disposées
- **Fichiers :** Multiples formulaires WinForms (estimation ~30 occurrences)
- **Sévérité :** WARNING
- **Description :** Des objets `Font` sont créés inline lors d'événements `Paint` ou `DrawItem` sans être wrappés dans `using`. Chaque cycle de rendu alloue un handle GDI+ non libéré jusqu'au GC, pouvant provoquer des fuites mémoire sur usage prolongé.
- **Note :** Une session précédente (2026-06-18) a corrigé les `SolidBrush` — le même pattern subsiste pour `Font`.
- **Recommandation :** Déclarer les `Font` fréquentes comme champs `static readonly` du formulaire, ou wrapper systématiquement en `using` dans les handlers événementiels.

---

### 2.4 Validation UI (WARNING)

#### UI-1 — Absence de MaxLength sur les TextBox
- **Fichiers :** Formulaires `FrmEdit*` (FrmIngredientEdit, FrmAchatEdit, FrmProduitWebEdit, etc.)
- **Sévérité :** WARNING
- **Description :** Les `TextBox` de saisie n'ont pas de propriété `MaxLength` définie, permettant des saisies dépassant la capacité des colonnes SQL (`VARCHAR(100)`, `VARCHAR(255)`). Cela produit une exception SQL non contrôlée à l'INSERT/UPDATE plutôt qu'un message d'erreur utilisateur.
- **Recommandation :** Aligner `TextBox.MaxLength` sur la longueur de la colonne SQL correspondante pour chaque champ texte de saisie.

---

### 2.5 Conventions et patterns (INFO)

#### CONV-1 — Magic strings pour rôles et types métier
- **Fichiers :** `DAL/*.cs`, `Forms/FrmPrincipal.cs`, `Navigation/`
- **Sévérité :** INFO
- **Description :** Les rôles (`"admin"`, `"employe"`) et les types de lots/ingrédients sont comparés comme chaînes littérales dispersées dans le code. Risque de typo silencieuse et de maintenance difficile.
- **Recommandation :** Centraliser dans des `enum` ou des classes `static` de constantes (`Roles.Admin`, `TypeIngredient.Alcool`).

#### CONV-2 — Inconsistance Designer vs Code-only
- **Fichiers :** ~8 formulaires gérés par `DesignerBridges.cs`
- **Sévérité :** INFO
- **Description :** Certains formulaires sont créés programmatiquement (code-only), d'autres via le Designer VS. Cette dualité complique la revue code et le debug de layout.
- **Recommandation :** Harmoniser vers une approche unique sur les nouveaux formulaires. La migration est cosmétique — documenter la convention choisie.

#### CONV-3 — Absence de tests unitaires DAL
- **Périmètre :** 13 fichiers DAL
- **Sévérité :** INFO
- **Description :** Aucun test unitaire ou d'intégration ne couvre le DAL. Toute régression sur `GetAll`, `GetById`, `Insert`, `Update` n'est détectée qu'à l'exécution manuelle.
- **Recommandation :** Introduire un projet `CharlesNadejda.Tests` avec NUnit ou MSTest. Mocker `IDbConnection` pour les tests unitaires DAL. Priorité basse (contexte académique), mais recommandé avant production réelle.

#### CONV-4 — Gestion d'exceptions trop générique dans certains forms
- **Sévérité :** INFO
- **Description :** Plusieurs catch blocs capturent `Exception` générique sans log ni distinction. Les corrections de session 2026-06-18 ont introduit `Trace.TraceError()` dans plusieurs endroits, mais ce pattern n'est pas uniformément appliqué.
- **Recommandation :** Audit ciblé des catch silencieux restants. Appliquer `Trace.TraceError()` systématiquement, ou lever une exception applicative custom.

#### CONV-5 — Héritage `FrmEditBase` non généralisé
- **Fichiers :** `Forms/FrmIngredientEdit.cs`, `Forms/FrmBomFicheEdit.cs`
- **Sévérité :** INFO
- **Description :** Ces deux formulaires n'héritent pas de `FrmEditBase` (TICKET-15 de l'audit 2026-04-22, toujours ouvert). La classe de base factorisant la validation, le mode ajout/édition et le cycle save/cancel n'est donc pas adoptée partout.
- **Recommandation :** Migrer `FrmIngredientEdit` et `FrmBomFicheEdit` vers `FrmEditBase` pour uniformiser le pattern CRUD.

#### CONV-6 — Artefacts orphelins potentiels
- **Sévérité :** INFO
- **Description :** Des formulaires et DAL orphelins (référencés dans TICKET-23 de l'audit précédent : `FrmArtisaStock`, `FrmBomProduction`, `FrmRecettes`) pourraient subsister dans la solution sans être appelés.
- **Recommandation :** Confirmer la liste via une recherche d'usages dans VS. Supprimer les artefacts confirmés inutilisés.

#### CONV-7 — `ShowDialog()` dans `FrmListeBase<T>`
- **Fichiers :** `Forms/FrmListeBase.cs` (méthodes `OnAjouter`, `OnModifier`)
- **Sévérité :** INFO
- **Description :** Le pattern `ShowDialog()` est encodé dans la classe de base, standardisant une approche multi-fenêtres au lieu du Single-Form-Area (SFA) souhaité par l'architecture. Ce n'est pas une régression fonctionnelle, mais un choix architectural à documenter.
- **Recommandation :** Documenter le choix explicitement dans ARCHITECTURE.md. Si SFA est visé, planifier la migration comme un sprint dédié.

#### CONV-8 — `DataGridView` : widths et tri non configurés uniformément
- **Sévérité :** INFO
- **Description :** Certains DGV n'appliquent pas `SortMode = NotSortable` sur les colonnes non-triables et n'ont pas de `MinimumWidth` défini, pouvant produire un affichage dégradé sur petits écrans ou avec des données longues.
- **Recommandation :** Appliquer le template DGV standard du référentiel (`winforms-referential.md`) à tous les DataGridView.

#### INFO-1 à INFO-5 — Mineurs non bloquants
- Absence de `tooltip` sur les boutons icône (UX hint)
- `lblActivite` vs `cboActivite` : choix de read-only non documenté dans FrmBomFicheEdit
- Nommage de variables non uniforme (mix `fr`/`en`) dans quelques DAL
- Commentaires XML doc absents sur les méthodes publiques DAL
- `App.config` non versionnable proprement (configuration dev = configuration prod)

---

## 3. Findings Laravel

### 3.1 Controllers (WARNING)

#### LAR-1 — Timing leak dans PanierController::updateQuantite()
- **Fichier :** `site-laravel/app/Http/Controllers/PanierController.php` — méthode `updateQuantite()`
- **Sévérité :** WARNING
- **Description :** La méthode appelle `findOrFail($id_ligne)` **avant** de vérifier que la ligne appartient à l'utilisateur courant. Si l'`id_ligne` n'existe pas → 404. S'il existe mais appartient à quelqu'un d'autre → 403 après la requête. Cette différence de timing/réponse révèle l'existence d'`id_ligne` valides à un attaquant faisant de l'énumération d'IDs.
- **Pattern problématique :**
  ```php
  // Problème : findOrFail() d'abord, ownership check ensuite
  $ligne = LignePanier::findOrFail($id_ligne);         // 404 si inexistant
  if ($ligne->id_client !== Auth::id()) abort(403);    // 403 si pas propriétaire
  ```
- **Recommandation :** Fusionner les deux conditions pour retourner systématiquement 404, empêchant l'énumération :
  ```php
  $ligne = LignePanier::where('id', $id_ligne)
      ->where('id_client', Auth::id())
      ->firstOrFail(); // 404 dans les deux cas
  ```

---

### 3.2 Routes et middleware (INFO)

#### LAR-2 — Absence de throttle sur routes POST protégées
- **Fichier :** `site-laravel/routes/web.php`
- **Sévérité :** INFO
- **Description :** Les routes POST protégées par `ClientAuth` (panier, profil, checkout) ne sont pas soumises à un middleware `throttle`. La route de login dispose bien d'un throttle, mais les actions post-authentification peuvent être spammées sans limitation.
- **Impact :** Risque de DoS applicatif (flood d'ajouts au panier, de mises à jour profil) et d'abus de l'API Stripe en test mode.
- **Recommandation :** Ajouter `->middleware('throttle:60,1')` sur les groupes de routes sensibles, ou configurer un throttle global dans `Kernel.php`.

#### LAR-3 — Migrations sans zéro-padding (ordre alphabétique)
- **Fichier :** `database/migrations/` (nommage `v1_..` à `v22_..`)
- **Sévérité :** INFO
- **Description :** Les migrations sont nommées `v1_`, `v2_`, ..., `v22_`. Le tri alphabétique produit l'ordre `v1, v10, v11, v12, v13, v14, v15, v2, v3...` au lieu de l'ordre chronologique. Ce problème est hérité du contexte SQL (migrations manuelles), mais si les fichiers sont jamais portés vers les migrations Artisan natives, l'ordre sera brisé.
- **Recommandation :** Renommer les migrations `v01_`, `v02_`, ..., `v22_` (zéro-padding à 2 chiffres). Opération de renommage pur, aucun impact fonctionnel SQL.

---

### 3.3 Blade / Frontend Laravel (✅ Aucun finding critique)

- **XSS :** Toutes les variables sont interpolées via `{{ }}` (échappement automatique Blade). Aucun usage de `{!! !!}` non justifié détecté.
- **CSRF :** `@csrf` présent sur tous les formulaires POST recensés.
- **Auth :** Middleware `ClientAuth` vérifie `session('client')`, `session()->regenerate()` appelé au login.
- **Validation :** Form Requests utilisés pour les entrées sensibles.

---

## 4. Findings SQL / Migrations

### 4.1 Schéma (✅ Aucun finding)

Le schéma MySQL présente un niveau de qualité élevé :

| Critère | Statut |
|---------|--------|
| Normalisation (3NF) | ✅ 20 tables bien normalisées |
| Clés étrangères | ✅ FK avec `ON DELETE`/`ON UPDATE` appropriés |
| Index | ✅ 13 index couvrant les colonnes de jointure et filtrage fréquents |
| CHECK constraints | ✅ 8 contraintes CHECK (quantités positives, types énumérés) |
| Nommage | ✅ `snake_case` pluriel uniforme |
| Séquencement | ✅ Migrations v1→v22 couvrant toute l'évolution du schéma |
| FIFO stock | ✅ Suivi par lots avec `date_reception` — logique FIFO supportée structurellement |

### 4.2 Observations positives

- La migration v22 (`id_stock_defaut` sur `ingredients`) montre une bonne pratique : colonne nullable FK avec `ON DELETE SET NULL`, permettant la dissociation propre.
- Les tables `lots_ingredients` et `mouvements_stock` implémentent correctement la traçabilité des stocks.
- Aucune table sans clé primaire `AUTO_INCREMENT`.

---

## 5. Sécurité OWASP Top 10 — Tableau consolidé

| # | Risque OWASP | C# WinForms | Laravel | Statut global |
|---|--------------|-------------|---------|---------------|
| A01 | Broken Access Control | Rôles vérifiés en session | `ClientAuth` middleware présent | ⚠️ Partiel (pas de RBAC fine-grained C#) |
| A02 | Cryptographic Failures | **`root/root` en clair App.config** | `.env` hors git (✅) | ❌ CRITICAL côté C# |
| A03 | Injection (SQL) | ✅ Paramètres ADO.NET partout | ✅ Eloquent ORM | ✅ Safe |
| A04 | Insecure Design | TICKET-7 (`ShowDialog` SFA) | Architecture MVC propre | ⚠️ Mineur C# |
| A05 | Security Misconfiguration | `App.config` root/root | Throttle partiel | ⚠️ App.config critique |
| A06 | Vulnerable Components | .NET 4.8.1 (EOL rapproché) | Laravel 11 (✅ LTS) | ⚠️ Framework C# à surveiller |
| A07 | Auth & Identification | **FrmLogin.cs:23 hardcoded pwd** | `password_verify` + regenerate | ❌ CRITICAL côté C# |
| A08 | Software & Data Integrity | Aucune signature des builds | Composer.lock (✅) | ⚠️ Mineur |
| A09 | Logging & Monitoring | `Trace.TraceError()` partiel | Logs Laravel configurés | ⚠️ Partiel C# |
| A10 | SSRF | N/A (desktop) | Cloudinary via SDK (✅) | ✅ N/A / Safe |

### Synthèse sécurité

| Niveau | Nombre | Détail |
|--------|--------|--------|
| ❌ CRITICAL | 2 | SEC-1 (App.config) · SEC-2 (FrmLogin hardcoded pwd) |
| ⚠️ WARNING | 2 | Rate limiting partiel (Laravel) · .NET 4.8.1 support lifecycle |
| ✅ Safe | 6 | SQL injection · XSS · CSRF · BCrypt passwords · session handling · Cloudinary |

---

## 6. Plan de remédiation priorisé

### P0 — Bloquants sécurité (avant toute démo/partage du dépôt)

| ID | Action | Fichier | Effort |
|----|--------|---------|--------|
| SEC-1 | Supprimer `Uid=root;Pwd=root` de `App.config`, injecter via variable d'environnement ou fichier local exclu du git | `App.config` | 2h |
| SEC-2 | Supprimer le bloc `#if DEBUG` avec credential hardcodé dans `FrmLogin.cs:23` | `FrmLogin.cs` | 30min |

### P1 — Qualité et stabilité (sprint court)

| ID | Action | Fichier(s) | Effort |
|----|--------|-----------|--------|
| DAL-1 | Ajouter guard DBNull sur toutes les colonnes nullable (15-20 lignes) | `DAL/*.cs` | 3h |
| GDI-1 | Déclarer les `Font` répétées en `static readonly` ou wrapper `using` | Forms `Paint`/`DrawItem` | 2h |
| UI-1 | Ajouter `MaxLength` sur tous les TextBox alignés sur les colonnes SQL | `FrmEdit*.cs` | 1h |
| LAR-1 | Fusionner `findOrFail` + ownership check dans `updateQuantite()` | `PanierController.php` | 30min |

### P2 — Conventions et dette technique (sprint moyen)

| ID | Action | Effort |
|----|--------|--------|
| CONV-1 | Créer classe `Roles` et `TypeIngredient` avec constantes | 1h |
| CONV-5 | Migrer `FrmIngredientEdit` et `FrmBomFicheEdit` vers `FrmEditBase` | 3h |
| LAR-2 | Ajouter `throttle` sur routes POST protégées | 30min |
| LAR-3 | Renommer migrations `v01_` à `v22_` (zéro-padding) | 30min |

### P3 — Amélioration long terme

| ID | Action | Effort |
|----|--------|--------|
| CONV-3 | Créer projet `CharlesNadejda.Tests` avec tests DAL de base | 4h+ |
| CONV-6 | Purger les artefacts orphelins (`FrmArtisaStock`, etc.) | 1h |
| CONV-7 | Documenter le choix SFA vs ShowDialog dans ARCHITECTURE.md | 1h |
| CONV-8 | Uniformiser le template DGV sur tous les DataGridView | 2h |

---

## 7. Tableau de bord final

### Scores par domaine

```
C# WinForms       ████████░░  7.0/10   Bon socle — 2 critiques à corriger en P0
Laravel           ████████████ 4.7/5   Quasi-exemplaire — 1 timing leak mineur
SQL/Migrations    ██████████  5.0/5   Excellent — aucune dette structurelle
Sécurité OWASP    ████████░░  8.0/10  Safe sur injection/XSS, 2 critiques credentials

SCORE GLOBAL      ████████░░  7.5/10
```

### Compteurs

| Métrique | Valeur |
|----------|--------|
| Fichiers analysés (C#) | ~55 (13 DAL + ~40 Forms + Models + Navigation) |
| Fichiers analysés (Laravel) | ~30 (Controllers, Models, Views, Routes, Middleware) |
| Tables SQL | 20 |
| Migrations SQL | 22 (v1→v22) |
| Findings total | 21 |
| Findings CRITICAL | **2** |
| Findings WARNING | **4** |
| Findings INFO | **15** |

### Recommandations finales

1. **Traiter SEC-1 et SEC-2 immédiatement** — avant tout push public ou partage du dépôt avec l'examinateur.
2. **DAL-1 (guard DBNull)** est le finding le plus probable à provoquer un crash en démo — traiter en P1 prioritaire.
3. **Laravel est le point fort du projet** — la qualité du code Blade/Controller est au-dessus de la moyenne pour un projet académique de 2e année.
4. **Le schéma SQL est une valeur sûre** — aucun point à défendre sur la DB, c'est un atout à valoriser lors de la défense.
5. **Pour la défense orale** : mentionner la correction DAL-1 et SEC-1/SEC-2 comme "points d'amélioration identifiés et planifiés" démontre une bonne maîtrise des enjeux sécurité.

---

*Audit produit par analyse statique — aucun fichier source modifié dans cette session.*
*Rapport complémentaire aux audits : `AUDIT_2026-04-21.md` · `AUDIT_2026-04-22.md`*
