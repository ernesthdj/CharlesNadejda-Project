# Agent #2 — Software Architect
## Architecture & Stack — Boutique Web ArtisaStock
> Date : 2026-05-19
> Input consommé : Brainstorm PDWEB + `01_PO_UserStories.md`
> Phase : Définition

---

## 1. Vue d'ensemble architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    BASE DE DONNÉES PARTAGÉE                  │
│                 MySQL 9.6 · charlesnadejda                   │
│                                                              │
│  ┌──────────────┐  ┌───────────────┐  ┌──────────────────┐  │
│  │ Tables ERP   │  │ Tables Web    │  │ Tables partagées │  │
│  │ bom_*        │  │ clients       │  │ bom_fiches       │  │
│  │ lots_*       │  │ categories_web│  │ bom_stocks       │  │
│  │ activites    │  │ produits_web  │  │                  │  │
│  │ stocks       │  │ commandes_web │  │                  │  │
│  │ fournisseurs │  │ cmd_web_lignes│  │                  │  │
│  └──────────────┘  └───────────────┘  └──────────────────┘  │
└──────────────┬──────────────────────────────┬────────────────┘
               │                              │
               │ MySql.Data                   │ Eloquent ORM
               │ (requêtes paramétrées)       │ (requêtes paramétrées)
               │                              │
    ┌──────────┴──────────┐       ┌───────────┴──────────┐
    │  ArtisaStock ERP    │       │  Laravel Boutique    │
    │  C# WinForms        │       │  PHP 8.3             │
    │  .NET 4.8.1         │       │  Blade + Tailwind    │
    │                     │       │  Vanilla JS (AJAX)   │
    │  Mini CMS :         │       │                      │
    │  - Catégories web   │       │  Client :            │
    │  - Produits web     │       │  - Catalogue         │
    │  - Commandes web    │       │  - Panier            │
    └─────────────────────┘       │  - Commande          │
                                  └──────────────────────┘
```

**Pattern : Shared Database** — les deux apps lisent/écrivent dans la même DB MySQL. Pas d'API REST entre elles (hors scope V1). La synchronisation est immédiate car la source de vérité est unique.

---

## 2. Stack technique

| Couche | Technologie | Version | Justification |
|--------|-------------|---------|---------------|
| **Backend Web** | Laravel | 11.x | Imposé PDWEB. MVC natif, Eloquent, Blade, middleware |
| **PHP** | PHP | 8.3 | Requis par Laravel 11 |
| **Templates** | Blade | (intégré) | Moteur natif Laravel. Escape auto `{{ }}` contre XSS |
| **CSS** | Tailwind CSS | 3.x | Utility-first, rapide à prototyper, responsive |
| **JavaScript** | Vanilla JS | ES6+ | AJAX `fetch()`. Pas besoin de framework pour 2-3 interactions |
| **ERP Desktop** | C# WinForms | .NET 4.8.1 | Existant (PDSGBD) |
| **Base de données** | MySQL | 9.6 | Existante, partagée |
| **Auth Web** | Sessions PHP natives | - | Imposé PDWEB. `$_SESSION` ou wrapper Laravel `session()` |
| **Hash** | BCrypt | - | `password_hash()` PHP ↔ `BCrypt.Net` C#. Interopérable |
| **Serveur dev** | `php artisan serve` | - | Port 8000. Local uniquement |
| **Images** | Stockage local | - | `storage/app/public/produits/` avec symlink `public/storage` |

---

## 3. Structure Laravel

```
site-laravel/
├── app/
│   ├── Http/
│   │   ├── Controllers/
│   │   │   ├── Auth/
│   │   │   │   ├── RegisterController.php      # US-W01
│   │   │   │   ├── LoginController.php         # US-W02, W03
│   │   │   ├── CatalogueController.php         # US-W04, W05
│   │   │   ├── PanierController.php            # US-W06 (AJAX)
│   │   │   ├── CommandeController.php          # US-W07, W08
│   │   │   └── ProfilController.php            # US-W09
│   │   ├── Middleware/
│   │   │   └── ClientAuth.php                  # vérifie session client
│   │   └── Requests/
│   │       ├── RegisterRequest.php             # FormRequest validation
│   │       ├── LoginRequest.php
│   │       └── ProfilUpdateRequest.php
│   └── Models/
│       ├── Client.php                          # table clients
│       ├── CategorieWeb.php                    # table categories_web
│       ├── ProduitWeb.php                      # table produits_web
│       ├── CommandeWeb.php                     # table commandes_web
│       ├── CommandeWebLigne.php                # table commandes_web_lignes
│       ├── BomFiche.php                        # lecture seule (existante)
│       └── BomStock.php                        # lecture seule (existante)
│
├── resources/views/
│   ├── layouts/
│   │   └── app.blade.php                      # Layout principal
│   ├── components/
│   │   ├── header.blade.php                   # Nav + compteur panier
│   │   ├── footer.blade.php
│   │   └── product-card.blade.php             # Card produit réutilisable
│   ├── auth/
│   │   ├── register.blade.php                 # US-W01
│   │   └── login.blade.php                    # US-W02
│   ├── catalogue/
│   │   ├── index.blade.php                    # US-W04
│   │   └── show.blade.php                     # US-W05
│   ├── panier/
│   │   └── index.blade.php                    # US-W06
│   ├── commandes/
│   │   ├── recap.blade.php                    # US-W07 (avant paiement)
│   │   ├── confirmation.blade.php             # US-W07 (après paiement)
│   │   └── historique.blade.php               # US-W08
│   └── profil/
│       └── edit.blade.php                     # US-W09
│
├── routes/
│   └── web.php                                # Toutes les routes
│
├── public/
│   ├── css/                                   # Tailwind compilé
│   ├── js/
│   │   └── panier.js                          # AJAX panier
│   └── storage -> ../storage/app/public       # Symlink images
│
├── storage/
│   └── app/public/produits/                   # Images uploadées depuis C#
│
└── database/
    └── migrations/
        └── 2026_05_xx_create_boutique_tables.php
```

---

## 4. Routes Laravel

```php
// === ROUTES PUBLIQUES ===
Route::get('/',                    [CatalogueController::class, 'index'])->name('catalogue');
Route::get('/produit/{id}',        [CatalogueController::class, 'show'])->name('produit.show');
Route::get('/categorie/{id}',      [CatalogueController::class, 'parCategorie'])->name('catalogue.categorie');

// === AUTH ===
Route::get('/register',            [RegisterController::class, 'showForm'])->name('register');
Route::post('/register',           [RegisterController::class, 'register']);
Route::get('/login',               [LoginController::class, 'showForm'])->name('login');
Route::post('/login',              [LoginController::class, 'login']);
Route::post('/logout',             [LoginController::class, 'logout'])->name('logout');

// === ROUTES PROTÉGÉES (middleware ClientAuth) ===
Route::middleware('client.auth')->group(function () {

    // Panier
    Route::get('/panier',              [PanierController::class, 'index'])->name('panier');
    Route::post('/panier/ajouter',     [PanierController::class, 'ajouter'])->name('panier.ajouter');      // AJAX
    Route::patch('/panier/quantite',   [PanierController::class, 'updateQuantite'])->name('panier.quantite'); // AJAX
    Route::delete('/panier/supprimer', [PanierController::class, 'supprimer'])->name('panier.supprimer');    // AJAX
    Route::get('/panier/count',        [PanierController::class, 'count'])->name('panier.count');            // AJAX

    // Commande
    Route::get('/commande/recap',      [CommandeController::class, 'recap'])->name('commande.recap');
    Route::post('/commande/valider',   [CommandeController::class, 'valider'])->name('commande.valider');
    Route::get('/mes-commandes',       [CommandeController::class, 'historique'])->name('commandes.historique');
    Route::get('/commande/{id}',       [CommandeController::class, 'detail'])->name('commande.detail');

    // Profil
    Route::get('/profil',              [ProfilController::class, 'edit'])->name('profil.edit');
    Route::put('/profil',              [ProfilController::class, 'update'])->name('profil.update');
});
```

---

## 5. Modèles Eloquent & Relations

```php
// Client.php
class Client extends Model {
    protected $table = 'clients';
    protected $hidden = ['mot_de_passe'];

    public function commandes()     { return $this->hasMany(CommandeWeb::class, 'id_client'); }
    public function panierActif()   { return $this->hasOne(CommandeWeb::class, 'id_client')->where('statut', 'panier'); }
}

// CategorieWeb.php
class CategorieWeb extends Model {
    protected $table = 'categories_web';

    public function produits()      { return $this->hasMany(ProduitWeb::class, 'id_categorie'); }
    public function produitsEnVente() { return $this->produits()->where('en_vente', 1); }
}

// ProduitWeb.php
class ProduitWeb extends Model {
    protected $table = 'produits_web';

    public function categorie()     { return $this->belongsTo(CategorieWeb::class, 'id_categorie'); }
    public function bomFiche()      { return $this->belongsTo(BomFiche::class, 'id_bom_fiche'); }

    // Stock dynamique : SUM(bom_stocks.quantite_disponible)
    public function getStockDisponibleAttribute(): float {
        return BomStock::where('id_fiche', $this->id_bom_fiche)
                       ->where('quantite_disponible', '>', 0)
                       ->sum('quantite_disponible');
    }

    public function getEnStockAttribute(): bool {
        return $this->stock_disponible > 0;
    }
}

// CommandeWeb.php
class CommandeWeb extends Model {
    protected $table = 'commandes_web';

    public function client()        { return $this->belongsTo(Client::class, 'id_client'); }
    public function lignes()        { return $this->hasMany(CommandeWebLigne::class, 'id_commande'); }
}

// CommandeWebLigne.php
class CommandeWebLigne extends Model {
    protected $table = 'commandes_web_lignes';

    public function commande()      { return $this->belongsTo(CommandeWeb::class, 'id_commande'); }
    public function produit()       { return $this->belongsTo(ProduitWeb::class, 'id_produit_web'); }
}

// BomFiche.php (lecture seule — table existante)
class BomFiche extends Model {
    protected $table = 'bom_fiches';

    public function produitWeb()    { return $this->hasOne(ProduitWeb::class, 'id_bom_fiche'); }
    public function stocks()        { return $this->hasMany(BomStock::class, 'id_fiche'); }
}

// BomStock.php (lecture seule + décrémentation à la commande)
class BomStock extends Model {
    protected $table = 'bom_stocks';
}
```

---

## 6. Algorithme critique : Décrémentation FIFO à la commande

```php
// CommandeController::valider()
DB::beginTransaction();
try {
    $commande = CommandeWeb::where('id_client', session('client_id'))
                           ->where('statut', 'panier')
                           ->with('lignes.produit')
                           ->firstOrFail();

    foreach ($commande->lignes as $ligne) {
        $restant = $ligne->quantite;
        $idFiche = $ligne->produit->id_bom_fiche;

        // Récupérer les stocks FIFO (plus ancien d'abord)
        $stocks = BomStock::where('id_fiche', $idFiche)
                          ->where('quantite_disponible', '>', 0)
                          ->orderBy('date_production', 'asc')
                          ->lockForUpdate()              // verrou pessimiste anti-TOCTOU
                          ->get();

        $totalDispo = $stocks->sum('quantite_disponible');
        if ($totalDispo < $restant) {
            DB::rollBack();
            return back()->with('error', "Stock insuffisant pour : {$ligne->produit->nom_commercial}");
        }

        // Consommation FIFO
        foreach ($stocks as $stock) {
            if ($restant <= 0) break;

            $aConsommer = min($restant, $stock->quantite_disponible);
            $stock->quantite_disponible -= $aConsommer;
            $stock->save();
            $restant -= $aConsommer;
        }
    }

    // Finaliser la commande
    $commande->statut = 'payee';
    $commande->date_commande = now();
    $commande->adresse_livraison = $this->buildAdresseLivraison($commande->client);
    $commande->total_ttc = $commande->lignes->sum('sous_total');
    $commande->save();

    DB::commit();
    return redirect()->route('commande.detail', $commande->id)
                     ->with('success', 'Commande validée !');

} catch (\Exception $e) {
    DB::rollBack();
    return back()->with('error', 'Erreur lors de la validation. Réessayez.');
}
```

---

## 7. Partage d'images C# ↔ Laravel

**Problème soulevé par le PO :** l'admin upload une image depuis C# (FileDialog), Laravel doit la servir.

**Solution : dossier partagé unique.**

```
Dossier physique : site-laravel/storage/app/public/produits/

C# écrit ici :
    string destDir = @"C:\...\site-laravel\storage\app\public\produits\";
    File.Copy(sourceImage, Path.Combine(destDir, fileName));

    // En DB : image_path = "produits/chocolat-noir.jpg"

Laravel lit ici :
    <img src="{{ asset('storage/' . $produit->image_path) }}" />

    // Nécessite : php artisan storage:link (symlink public/storage → storage/app/public)
```

**Convention de nommage image :** `{id_produit}_{timestamp}.{ext}` pour éviter les collisions.

---

## 8. Middleware ClientAuth

```php
// app/Http/Middleware/ClientAuth.php
class ClientAuth
{
    public function handle(Request $request, Closure $next)
    {
        if (!session()->has('client_id')) {
            return redirect()->route('login')
                             ->with('error', 'Connectez-vous pour accéder à cette page.');
        }

        // Vérifier que le client existe encore et est actif
        $client = Client::where('id', session('client_id'))
                        ->where('actif', 1)
                        ->first();

        if (!$client) {
            session()->flush();
            return redirect()->route('login')
                             ->with('error', 'Compte désactivé ou introuvable.');
        }

        return $next($request);
    }
}

// Enregistrement dans bootstrap/app.php (Laravel 11) :
->withMiddleware(function (Middleware $middleware) {
    $middleware->alias([
        'client.auth' => \App\Http\Middleware\ClientAuth::class,
    ]);
})
```

---

## 9. Structure C# — Mini CMS (nouvelles classes)

```
CharlesNadejda/
├── Models/
│   ├── CategorieWeb.cs             # nouveau
│   ├── ProduitWeb.cs               # nouveau
│   └── CommandeWeb.cs              # nouveau (lecture seule)
│   └── CommandeWebLigne.cs         # nouveau (lecture seule)
├── DAL/
│   ├── CategorieWebDAL.cs          # CRUD
│   ├── ProduitWebDAL.cs            # CRUD + stock dynamique
│   └── CommandeWebDAL.cs           # lecture seule
├── Forms/
│   ├── FrmBoutiqueWeb.cs           # écran principal avec onglets
│   ├── FrmCategorieWebEdit.cs      # formulaire catégorie
│   └── FrmProduitWebEdit.cs        # formulaire produit (avec FileDialog image)
└── Navigation/
    └── NavItemId.cs                # + BoutiqueWeb
    └── ScreenId.cs                 # + BoutiqueWeb
```

---

## 10. Sécurité — Décisions architecture

| Aspect | Décision | Justification |
|--------|----------|---------------|
| **SQL Injection** | Eloquent (Laravel) + requêtes paramétrées (C#) | Aucune concaténation SQL |
| **XSS** | Blade `{{ }}` (escape auto) | Jamais `{!! !!}` sur du user input |
| **CSRF** | `@csrf` sur tous les formulaires POST | Middleware `VerifyCsrfToken` Laravel |
| **Session fixation** | `session()->regenerate()` après login | Empêche détournement de session |
| **Mot de passe** | `password_hash(PASSWORD_BCRYPT)` PHP | Interopérable avec `BCrypt.Net` C# |
| **Rate limiting** | `throttle:5,1` sur `/login` et `/register` | Anti brute-force |
| **TOCTOU stock** | `lockForUpdate()` dans la TX commande | Verrou pessimiste MySQL |
| **Upload image** | Validation extension (jpg, png, webp) + MIME type + max 2MB | C# : FileDialog filtré |

---

## JOURNAL — Agent #2 Architect

**Phase :** Définition
**Itération :** 1
**Entrée consommée :** Brainstorm PDWEB + 01_PO_UserStories.md
**Output produit :** Architecture Shared DB, stack technique, structure Laravel complète, routes, modèles Eloquent, algo FIFO, middleware, structure C# CMS, sécurité
**Décisions clés :**
- Shared Database (pas d'API REST en V1) — le plus simple et suffisant pour l'examen
- Sessions PHP natives (pas de JWT) — imposé par critères PDWEB
- Vanilla JS pour AJAX (pas de framework) — minimal et explicable à l'oral
- Dossier images partagé physiquement entre C# et Laravel
- `lockForUpdate()` pour le TOCTOU stock à la commande
**Selfdoubt appliqué :**
- ✅ Certain : Eloquent, Blade, sessions, BCrypt interop — vérifié dans la doc Laravel 11
- ✅ Certain : FIFO pattern — identique au BomProductionDAL existant
- ⚠️ Probable : le `lockForUpdate()` Eloquent fonctionne bien avec MySQL InnoDB (à tester)
- ⚠️ Probable : symlink `storage:link` fonctionne sous Windows (à vérifier)
**Impact :** Architecture simple, explicable à l'oral, tous les critères PDWEB couverts
**Alerte agent suivant :** L'UI/UX doit concevoir : 8 pages web (responsive) + 3 écrans ERP (FrmBoutiqueWeb avec onglets). Le layout Blade doit inclure un header avec compteur panier AJAX.
