# 08 — Plan d'Implémentation

> **Document de référence opérationnelle** — À lire EN PREMIER à chaque session de travail.
> Chaque phase liste exactement les fichiers à créer, les docs de référence à lire, et les critères de validation.
> Objectif : permettre à Claude de reprendre le fil sans hallucinations, même après reset de contexte.

---

## 🛠️ IDEs et Environnement

| Outil | Usage | Notes |
|-------|-------|-------|
| **Docker Desktop** | Infrastructure MySQL + Laravel | `docker-compose up -d` depuis la racine du projet |
| **Zed** | Laravel (site-laravel/) | Extension zed-for-laravel (voir config §IDE ci-dessous) |
| **Visual Studio 2022** | C# Windows Forms (app-csharp/) | .NET Framework 4.8 ou .NET 8 |
| **phpMyAdmin** | Vérification BDD | http://localhost:8080 (root / root) |
| **Stripe CLI** | Test webhooks local | `stripe listen --forward-to localhost:8000/webhook/stripe` |

---

## 🗂️ Structure des dossiers (état final attendu)

```
CharlesNadejda_Project/
├── docs/                          ← Documentation (déjà créée)
├── sql/
│   ├── create_database.sql        ✅ créé
│   └── seed_data.sql              ✅ créé
├── docker-compose.yml             ✅ créé
├── docker/
│   ├── php/Dockerfile             ✅ créé
│   └── nginx/default.conf        ✅ créé
├── site-laravel/                  ← Projet Laravel (à scaffolder)
│   ├── .env.example               ✅ créé
│   ├── .env                       (copier de .env.example + remplir clés)
│   ├── app/
│   │   ├── Http/Controllers/
│   │   ├── Http/Middleware/
│   │   ├── Http/Requests/
│   │   └── Models/
│   ├── resources/
│   │   ├── views/
│   │   └── lang/
│   └── routes/web.php
└── app-csharp/
    └── CharlesNadejda/
        ├── CharlesNadejda.sln
        ├── App.config
        ├── DAL/
        ├── Helpers/
        ├── Models/
        └── Forms/
```

---

## ⚙️ Configuration IDE

### Zed — `.zed/settings.json` (à créer à la racine du projet)

```json
{
  "auto_install_extensions": {
    "php": true,
    "blade": true,
    "env": true,
    "tailwindcss": true
  },
  "languages": {
    "PHP": {
      "language_servers": ["intelephense", "!phpactor"],
      "formatter": {
        "external": {
          "command": "bash",
          "arguments": ["-c", "cat > {buffer_path} && ./vendor/bin/pint --quiet {buffer_path} && cat {buffer_path}"]
        }
      },
      "format_on_save": "on"
    },
    "Blade": {
      "language_servers": ["blade"]
    }
  },
  "inlay_hints": { "enabled": true },
  "git": { "inline_blame": { "enabled": true } }
}
```

### Zed — `.zed/tasks.json` (tâches Artisan accessibles via Ctrl+Shift+T)

```json
[
  { "label": "🐳 Docker Up",        "command": "docker-compose up -d",                    "cwd": "$PROJECT_ROOT" },
  { "label": "🐳 Docker Down",       "command": "docker-compose down",                     "cwd": "$PROJECT_ROOT" },
  { "label": "🚀 Artisan Serve",     "command": "docker-compose exec app php artisan serve","cwd": "$PROJECT_ROOT" },
  { "label": "🔃 Artisan Migrate",   "command": "docker-compose exec app php artisan migrate","cwd": "$PROJECT_ROOT" },
  { "label": "🌱 Artisan Seed",      "command": "docker-compose exec app php artisan db:seed","cwd": "$PROJECT_ROOT" },
  { "label": "📋 Route List",        "command": "docker-compose exec app php artisan route:list","cwd": "$PROJECT_ROOT" },
  { "label": "🧹 Cache Clear",       "command": "docker-compose exec app php artisan optimize:clear","cwd": "$PROJECT_ROOT" },
  { "label": "💳 Stripe Listen",     "command": "stripe listen --forward-to localhost:8000/webhook/stripe", "cwd": "$PROJECT_ROOT" }
]
```

### Visual Studio 2022

- Solution : `app-csharp/CharlesNadejda/CharlesNadejda.sln`
- Projet : Windows Forms App (C#)
- NuGet packages à installer dès la création :
  - `MySql.Data` (Oracle officiel)
  - `BCrypt.Net-Next`
  - `Newtonsoft.Json`

---

## 📋 Phases d'Implémentation

> **Règle pour Claude** : Avant chaque phase, lire les fichiers docs référencés.
> Ne jamais inventer une structure de table — toujours relire `01_BASE_DE_DONNEES.md`.
> Ne jamais inventer une route — toujours relire `02_PLAN_PDWEB.md`.

---

### PHASE 0 — Infrastructure (Prérequis, ~1h)

**Objectif** : MySQL opérationnel, Laravel scaffoldé, C# projet créé

**Docs à lire** : `07_DOCKER_SETUP.md`

**Étapes** :

1. Depuis la racine du projet :
   ```bash
   docker-compose up -d --build
   # Vérifier : http://localhost:8080 → phpMyAdmin doit montrer la BDD charlesnadejda avec toutes les tables
   ```

2. Scaffolder Laravel dans le container :
   ```bash
   docker-compose exec app composer create-project laravel/laravel .
   cp site-laravel/.env.example site-laravel/.env
   docker-compose exec app php artisan key:generate
   ```

3. Tester la connexion DB :
   ```bash
   docker-compose exec app php artisan migrate:status
   # Doit répondre sans erreur (tables de migrations Laravel créées)
   ```

4. Créer la solution Visual Studio :
   - Nouveau projet → Windows Forms App → `app-csharp/CharlesNadejda/`
   - Installer NuGet : `MySql.Data`, `BCrypt.Net-Next`, `Newtonsoft.Json`

**✅ Validation** : phpMyAdmin accessible, `migrate:status` OK, VS solution compilable

---

### PHASE 1 — App C# : Fondations (~2 jours)

**Docs à lire** : `03_PLAN_PDSGBD.md` (sections FrmLogin, DAL, FrmPrincipal)

**Fichiers à créer** :

```
app-csharp/CharlesNadejda/
├── App.config                  ← ConnectionString MySQL
├── DAL/
│   ├── DbHelper.cs             ← Singleton connexion MySQL
│   └── UtilisateurDAL.cs       ← Authenticate(email, mdp) → BCrypt
├── Models/
│   └── Utilisateur.cs          ← POCO : Id, Nom, Prenom, Email, Role
└── Forms/
    ├── FrmLogin.cs             ← txtEmail, txtMdp, btnConnexion, lblErreur
    └── FrmPrincipal.cs         ← MDI parent, MenuStrip (Catalogue | Stock | Commandes)
```

**Contrat `App.config`** :
```xml
<connectionStrings>
  <add name="charlesnadejda"
       connectionString="Server=localhost;Port=3306;Database=charlesnadejda;Uid=root;Pwd=root;"
       providerName="MySql.Data.MySqlClient"/>
</connectionStrings>
```

**Contrat `DbHelper.cs`** :
```csharp
public static MySqlConnection GetConnection()
// Lit App.config → retourne connexion ouverte
// Gérer exception ConnectionString introuvable
```

**Contrat `UtilisateurDAL.Authenticate`** :
```csharp
// SELECT * FROM utilisateurs WHERE email = @email AND actif = 1
// Vérifier BCrypt.Net.BCrypt.Verify(mdp, hash) ET role == "admin"
// Retourne Utilisateur ou null
```

**✅ Validation** :
- Login avec `charles@charlesnadejda.be` / `password` → ouvre FrmPrincipal
- Login avec mauvais mdp → message d'erreur, pas de crash

---

### PHASE 2 — App C# : Catalogue complet (~3 jours)

**Docs à lire** : `03_PLAN_PDSGBD.md` (Catalogue), `01_BASE_DE_DONNEES.md` (tables categories, parfums, produits)

**Fichiers à créer** :

```
DAL/
├── CategorieDAL.cs             ← GetAll(), Insert(), Update(), Delete()
├── ParfumDAL.cs                ← GetAll(), Insert(), Update(), Delete(), GetByProduit(id)
└── ProduitDAL.cs               ← GetAll(), GetById(), Insert(), Update(), Delete(), SetParfums()

Models/
├── Categorie.cs
├── Parfum.cs
└── Produit.cs                  ← id, nom, prix_ttc, stock, configurable, capacite_max, image_url, disponible

Helpers/
└── CloudinaryHelper.cs         ← UploadImageAsync(filePath) → Task<string> (URL Cloudinary)

Forms/
├── FrmCategories.cs            ← DataGridView + boutons Ajouter/Modifier/Supprimer
├── FrmCategorieEdit.cs         ← txtNom, txtDescription, btnSauver
├── FrmParfums.cs               ← DataGridView + CRUD
├── FrmParfumEdit.cs            ← txtNom, comboType, txtCouleur, preview couleur
├── FrmProduits.cs              ← DataGridView avec image thumbnail, filtre par catégorie
└── FrmProduitEdit.cs           ← champs + comboCategorie + CheckedListBoxParfums + PictureBox + btnUploadImage
```

**Contrat `CloudinaryHelper.UploadImageAsync`** :
```csharp
// 1. Lire CLOUDINARY_CLOUD_NAME/API_KEY/API_SECRET depuis App.config
// 2. Construire signature SHA1 : timestamp + api_secret
// 3. POST multipart/form-data vers https://api.cloudinary.com/v1_1/{cloud_name}/image/upload
// 4. Retourner secure_url du JSON de réponse
// Référence complète : docs/03_PLAN_PDSGBD.md section CloudinaryHelper
```

**Règle `FrmProduitEdit` — Parfums** :
- CheckedListBox visible uniquement si `configurable = true`
- Sauvegarde = `ProduitDAL.SetParfums(produitId, selectedIds)` → DELETE + INSERT dans `produits_parfums`

**✅ Validation** :
- Ajouter une catégorie "Test" → apparaît dans liste
- Ajouter un produit avec image Cloudinary → URL visible dans DB via phpMyAdmin
- Modifier stock d'un produit → reflété dans DB

---

### PHASE 3 — Laravel : Fondations + Catalogue public (~3 jours)

**Docs à lire** : `02_PLAN_PDWEB.md` (sections Models, Controllers publics, Routes), `01_BASE_DE_DONNEES.md`

**Fichiers à créer** :

```
app/Models/
├── Categorie.php       ← hasMany(Produit)
├── Parfum.php          ← belongsToMany(Produit, 'produits_parfums')
└── Produit.php         ← belongsTo(Categorie), belongsToMany(Parfum), scope disponible

resources/views/
├── layouts/
│   ├── app.blade.php   ← navbar (logo, nav, langue FR/EN, panier badge), footer, @yield('content')
│   └── admin.blade.php ← layout admin avec sidebar
├── pages/
│   ├── accueil.blade.php
│   ├── catalogue.blade.php
│   ├── produit.blade.php   ← inclut le configurateur si configurable
│   └── contact.blade.php
└── components/
    └── configurateur.blade.php  ← JS compteur, validation exacte capacite_max

routes/web.php          ← Routes publiques (GET /, /catalogue, /produits/{id}, /contact)
```

**Nommage des tables dans les Models** (critique — Laravel pluralise en anglais par défaut !) :
```php
// Dans chaque Model, toujours spécifier :
protected $table = 'produits'; // ou 'categories', 'parfums', etc.
```

**Scope Produit** :
```php
public function scopeDisponible($query) {
    return $query->where('disponible', 1);
}
```

**✅ Validation** :
- `http://localhost:8000` → page d'accueil s'affiche
- `http://localhost:8000/catalogue` → liste les 7 produits du seed
- `http://localhost:8000/produits/1` → Ballotin 250g avec configurateur (19 chocolats max)

---

### PHASE 4 — Laravel : Auth + Panier (~2 jours)

**Docs à lire** : `02_PLAN_PDWEB.md` (Auth, Panier, Routes auth/panier)

**Étapes** :
```bash
docker-compose exec app composer require laravel/breeze
docker-compose exec app php artisan breeze:install blade
docker-compose exec app php artisan migrate
# Note : les tables users de Breeze ne sont PAS utilisées, on utilise notre table 'utilisateurs'
# → Modifier AuthController et le Model User pour pointer vers 'utilisateurs'
```

**Fichiers à créer** :

```
app/Models/
└── Utilisateur.php     ← $table='utilisateurs', $guard='utilisateurs', Auth custom

app/Http/Controllers/
├── Auth/
│   ├── LoginController.php     ← email + mdp → BCrypt verify via password_verify()
│   └── RegisterController.php  ← hash avec password_hash($mdp, PASSWORD_BCRYPT)
└── PanierController.php        ← index, add, update, remove, clear (session 'panier')

resources/views/
├── auth/
│   ├── login.blade.php
│   └── register.blade.php
└── panier/
    ├── index.blade.php         ← liste articles + récap + btn "Passer commande"
    └── checkout.blade.php      ← form date_souhaitee, type_reception, adresse si livraison
```

**Structure session panier** :
```php
// $_SESSION['panier'] ou session('panier') :
[
  'items' => [
    ['produit_id' => 1, 'nom' => 'Ballotin 250g', 'prix' => 13.00, 'qte' => 1,
     'parfums' => [['id' => 1, 'nom' => 'Praliné', 'qte' => 5], ...]]
  ],
  'total' => 36.00
]
```

**Règle validation configurateur (Laravel)** :
```php
// Dans PanierController::addConfigurable() :
$total = collect($request->parfums)->sum('quantite');
if ($total !== $produit->capacite_max) {
    return response()->json(['error' => "Vous devez sélectionner exactement {$produit->capacite_max} chocolats."], 422);
}
```

**✅ Validation** :
- Register → login → session active
- Ajouter Ballotin 250g avec 19 parfums → panier OK
- Ajouter Ballotin 250g avec 18 parfums → erreur JS + erreur serveur

---

### PHASE 5 — Laravel : Commandes + Stripe (~2 jours)

**Docs à lire** : `02_PLAN_PDWEB.md` (sections CommandeRequest, Paiement, Webhook, Facture)

**Installation** :
```bash
docker-compose exec app composer require stripe/stripe-php
```

**Fichiers à créer** :

```
app/Http/Requests/
└── CommandeRequest.php     ← validation date_souhaitee >= +7j, type_reception, adresse si livraison

app/Http/Controllers/
├── CommandeController.php  ← store() : créer commande + lignes + sélections depuis session panier
└── PaiementController.php  ← checkout() : Stripe Session ; webhook() : paiement confirmé → facture

app/Models/
├── Commande.php
├── LigneCommande.php
├── SelectionParfum.php
├── Facture.php
└── ZoneLivraison.php

resources/views/commandes/
├── checkout.blade.php      ← Stripe redirect (pas de form, juste redirect vers Stripe)
└── confirmation.blade.php  ← merci + numéro facture + lien télécharger PDF
```

**Flux exact Stripe** :
```
1. POST /commandes/store        → valider + créer commande en DB (statut: en_attente, paiement: en_attente)
                                  → GARDER l'id_commande en session
2. POST /paiement/checkout      → créer Stripe CheckoutSession avec line_items
                                  → Stripe redirige vers leur page de paiement
3. Stripe → POST /webhook/stripe → vérifier signature → mettre à jour statut_paiement=paye
                                  → créer facture (numero FAC-YYYY-XXXX) → vider panier
4. Stripe redirige vers /commandes/{id}/confirmation
```

**Numérotation facture** :
```php
// Dans PaiementController::handleWebhook() :
$annee = date('Y');
$dernier = Facture::whereYear('date_emission', $annee)->max('id') ?? 0;
$numero = 'FAC-' . $annee . '-' . str_pad($dernier + 1, 4, '0', STR_PAD_LEFT);
```

**Webhook local (Stripe CLI)** :
```bash
stripe listen --forward-to http://localhost:8000/webhook/stripe
# Copier la clé whsec_... dans .env STRIPE_WEBHOOK_SECRET
```

**✅ Validation** :
- Commande avec date dans 6 jours → refusée (règle 7j)
- Paiement test Bancontact (carte `4000 0005 6000 0001`) → commande passe en 'paye'
- Facture FAC-2026-0002 générée en DB

---

### PHASE 6 — Laravel : Admin CRM (~2 jours)

**Docs à lire** : `02_PLAN_PDWEB.md` (sections Admin, Kanban, Calendrier, CRM, Bon de commande)

**Fichiers à créer** :

```
app/Http/Middleware/
└── AdminMiddleware.php     ← vérifie Auth + role == 'admin', sinon abort(403)

app/Http/Controllers/Admin/
├── CommandeController.php  ← index, show, kanban, calendrier, bon, updateStatut, notesAjax
└── CrmController.php       ← index (liste clients), show (fiche client)

resources/views/admin/
├── dashboard.blade.php     ← stats (nb commandes par statut, CA)
├── commandes/
│   ├── index.blade.php     ← tableau DataTable
│   ├── kanban.blade.php    ← 5 colonnes HTML/CSS draggable
│   ├── calendrier.blade.php ← FullCalendar.js CDN
│   ├── show.blade.php      ← détail commande + notes internes AJAX
│   └── bon.blade.php       ← impression @media print, window.print()
└── crm/
    ├── index.blade.php     ← liste clients avec nb commandes
    └── show.blade.php      ← fiche client : stats withCount+withSum, historique, notes
```

**Calendrier — données JSON** :
```php
// Admin\CommandeController::calendrier()
$events = Commande::with('client')
    ->select('id', 'id_client', 'date_souhaitee', 'statut', 'total_avec_livraison')
    ->get()
    ->map(fn($c) => [
        'id'    => $c->id,
        'title' => $c->client->prenom . ' — ' . $c->total_avec_livraison . '€',
        'start' => $c->date_souhaitee,
        'color' => match($c->statut) {
            'en_attente'     => '#FFA500',
            'confirmee'      => '#3788D8',
            'en_preparation' => '#9C27B0',
            'prete'          => '#4CAF50',
            'livree'         => '#607D8B',
            'annulee'        => '#F44336',
        }
    ]);
return view('admin.commandes.calendrier', compact('events'));
```

**AJAX notes internes** :
```php
// Route : PUT /admin/commandes/{id}/notes (retourne JSON)
// Besoin du meta CSRF : <meta name="csrf-token" content="{{ csrf_token() }}">
// JS : fetch('/admin/commandes/'+id+'/notes', { method:'PUT', headers:{'X-CSRF-TOKEN':..., 'Content-Type':'application/json'}, body: JSON.stringify({notes_internes: val}) })
```

**✅ Validation** :
- Accès `/admin` sans être admin → 403
- Kanban affiche la commande de test dans colonne "confirmée"
- Calendrier FullCalendar affiche l'événement à J+8
- Bouton imprimer bon de commande → CSS @media print masque navbar/sidebar

---

### PHASE 7 — App C# : ArtisaStock (~3 jours)

**Docs à lire** : `03_PLAN_PDSGBD.md` (sections ArtisaStock), `05_ARTISASTOCK_INTEGRATION.md`

**Fichiers à créer** :

```
Models/
├── Fournisseur.cs
├── Ingredient.cs       ← avec propriété bool EstEnAlerte → stock_actuel <= seuil_alerte
├── Recette.cs
└── Production.cs

DAL/
├── FournisseurDAL.cs   ← GetAll(), Insert(), Update(), Delete()
├── IngredientDAL.cs    ← GetAll(), GetById(), Insert(), Update(), Delete(), GetEnAlerte()
├── RecetteDAL.cs       ← GetAll(), GetById(), Insert(), Update(), GetIngredients()
├── RecetteIngredientDAL.cs ← SetIngredients(recetteId, items)
└── ProductionDAL.cs    ← Insert() : calcule cout + déstocke ingrédients + insère mouvements

Forms/
├── FrmFournisseurs.cs + FrmFournisseurEdit.cs
├── FrmIngredients.cs   ← DataGridView avec ligne rouge si stock <= seuil_alerte
├── FrmIngredientEdit.cs
├── FrmRecettes.cs
├── FrmRecetteEdit.cs   ← inclut gestion des ingrédients (DataGridView éditable)
├── FrmProductions.cs   ← comboRecette, nudQuantite, lblCoutEstime, btnLancer
└── FrmMouvementsStock.cs ← historique filtré par ingrédient + type
```

**Règle critique `ProductionDAL.Insert()`** :
```csharp
// Dans une transaction MySQL :
// 1. Vérifier que stock_actuel >= quantite_necessaire pour chaque ingrédient
// 2. Si insuffisant → throw Exception avec message explicite (pas de crash silencieux)
// 3. Insérer dans productions
// 4. UPDATE ingredients SET stock_actuel = stock_actuel - @qte WHERE id = @id
// 5. INSERT INTO mouvements_stock (type='sortie') pour chaque ingrédient
// Tout ou rien : MySqlTransaction + Commit/Rollback
```

**Alerte visuelle stock** :
```csharp
// Dans FrmIngredients, event DataGridView.RowPrePaint :
if ((decimal)row.Cells["stock_actuel"].Value <= (decimal)row.Cells["seuil_alerte"].Value)
    row.DefaultCellStyle.BackColor = Color.MistyRose;
```

**✅ Validation** :
- Lancer production 20 pièces de "Praline Praliné" → stocks ingrédients diminuent dans DB
- Stock insuffisant → message d'erreur propre, aucune modification en DB
- Ligne rouge dans FrmIngredients si stock < seuil

---

### PHASE 8 — Laravel : i18n FR/EN (~1 jour)

**Docs à lire** : `02_PLAN_PDWEB.md` (section i18n)

**Fichiers à créer** :

```
app/Http/Middleware/
└── SetLocale.php       ← session('locale') ?? 'fr' → App::setLocale()

resources/lang/
├── fr/
│   ├── nav.php         ← ['catalogue' => 'Boutique', 'panier' => 'Panier', ...]
│   ├── produits.php    ← ['configurer' => 'Personnaliser votre boîte', ...]
│   └── commandes.php   ← ['date_souhaitee' => 'Date de livraison souhaitée', ...]
└── en/
    ├── nav.php         ← ['catalogue' => 'Shop', 'panier' => 'Cart', ...]
    ├── produits.php
    └── commandes.php
```

**Route switcher** :
```php
Route::get('/langue/{locale}', function($locale) {
    if (in_array($locale, ['fr', 'en'])) session(['locale' => $locale]);
    return back();
})->name('langue.set');
```

**✅ Validation** :
- Clic "EN" → navbar passe en anglais, URL reste la même
- `{{ __('nav.catalogue') }}` retourne "Boutique" en FR, "Shop" en EN

---

### PHASE 9 — Tests & Scénarios d'Examen (~1 jour)

**Scénario PDWEB (examen web)** :
1. Visiteur → catalogue → Ballotin 250g → configurateur 19 chocolats → panier
2. Register → login → checkout → Stripe test Bancontact (4000 0005 6000 0001)
3. Webhook → commande confirmée → facture FAC-2026-xxxx
4. Admin → login → kanban → glisser commande vers "En préparation"
5. Admin → calendrier → voir commande à la bonne date
6. Admin → imprimer bon de commande → aperçu impression

**Scénario PDSGBD (examen C#)** :
1. Login Charles → FrmPrincipal
2. Ajouter produit "Ballotin 750g" avec image Cloudinary
3. Lancer production 20 "Praline Praliné" → stocks décrementés
4. Vérifier historique mouvements → 4 lignes "sortie"
5. Ajuster stock Chocolat noir (-0.200) → 1 ligne "ajustement"
6. Ingrédient passe en alerte rouge

---

## 🔁 Règles pour Claude en cours de session

1. **Toujours lire le doc de référence** de la phase en cours avant d'écrire du code.
2. **Nommage tables** : toujours `$table = '...'` explicite dans les models Laravel (évite le pluriel anglais auto).
3. **DB_HOST = mysql** (pas localhost) dans le `.env` Laravel.
4. **BCrypt** : PHP utilise `password_hash/verify`, C# utilise `BCrypt.Net.BCrypt.Verify` — même algo, interopérable.
5. **Transactions MySQL en C#** : toute opération multi-tables (production, commande) doit être dans une `MySqlTransaction`.
6. **CSRF** : tous les formulaires Blade `@csrf`, toutes les requêtes AJAX ont le header `X-CSRF-TOKEN`.
7. **Si un fichier n'est pas trouvé** : chercher d'abord dans `docs/`, puis dans le vieux projet `charles-nadejda-projet/`.
8. **Prix et capacités** : JAMAIS inventer — source unique = `docs/01_BASE_DE_DONNEES.md` section seed.

---

## 📊 Avancement

| Phase | Description | Statut |
|-------|-------------|--------|
| Infrastructure SQL | create_database.sql + seed_data.sql | ✅ Terminé |
| Infrastructure Docker | docker-compose.yml + Dockerfile + nginx | ✅ Terminé |
| Phase 0 | Docker up + Laravel scaffold + VS solution | ⬜ À faire |
| Phase 1 | App C# : Login + DAL + FrmPrincipal | ⬜ À faire |
| Phase 2 | App C# : Catalogue (catégories, parfums, produits, Cloudinary) | ⬜ À faire |
| Phase 3 | Laravel : Fondations + Catalogue public | ⬜ À faire |
| Phase 4 | Laravel : Auth + Panier + Configurateur | ⬜ À faire |
| Phase 5 | Laravel : Commandes + Stripe + Facture | ⬜ À faire |
| Phase 6 | Laravel : Admin CRM + Kanban + Calendrier | ⬜ À faire |
| Phase 7 | App C# : ArtisaStock (ingrédients, recettes, productions) | ⬜ À faire |
| Phase 8 | Laravel : i18n FR/EN | ⬜ À faire |
| Phase 9 | Tests scénarios examen | ⬜ À faire |
