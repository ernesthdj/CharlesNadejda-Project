# 02 — Plan PDWEB : Site Laravel Charles & Nadejda

> Plan complet du site web dynamique en **Laravel** (PHP 8+).
> Laravel est accepté par le professeur tant que c'est du PHP.
> Ce document couvre la structure MVC, les routes, les contrôleurs, les modèles Eloquent,
> les vues Blade, les sessions et les appels Ajax.

---

## 🎯 Rappel des critères PDWEB → mapping Laravel

| Critère | Points | Comment Laravel le couvre |
|---------|--------|--------------------------|
| Utilise PHP | — | Laravel est un framework PHP — 100% PHP |
| Formulaire avec gestion d'erreurs | /10 | `$request->validate()` + `@error` Blade |
| Sessions | /10 | `session()`, `auth()->user()`, panier en session |
| Contenus distincts selon l'utilisateur | — | `@auth` / `@guest` + middleware `auth` / `admin` |
| Sécurisation d'accès | — | Middleware `auth` sur les routes protégées |
| Données MySQL | /10 | Eloquent ORM (génère du SQL sous le capot) |
| Ajax | /5 | Routes JSON dans `web.php` + `fetch()` côté JS |

> **Pour l'oral** : le prof pose des questions sur ton implémentation — sois capable d'expliquer
> tes routes, tes contrôleurs, tes modèles Eloquent et la logique de ton panier en session.

---

## 📂 Structure du Projet Laravel

```
site-laravel/                       ← Projet Laravel (créé avec `composer create-project laravel/laravel`)
│
├── app/
│   ├── Http/
│   │   ├── Controllers/
│   │   │   ├── Auth/
│   │   │   │   ├── LoginController.php
│   │   │   │   └── RegisterController.php
│   │   │   ├── ProduitController.php        ← Catalogue public
│   │   │   ├── PanierController.php         ← Panier (session)
│   │   │   ├── CommandeController.php       ← Checkout + confirmation
│   │   │   ├── ContactController.php        ← Formulaire contact
│   │   │   ├── CompteController.php         ← Espace client
│   │   │   └── Admin/
│   │   │       ├── DashboardController.php
│   │   │       ├── CommandeController.php
│   │   │       └── ContactController.php
│   │   ├── Middleware/
│   │   │   └── AdminMiddleware.php          ← Vérifie role = 'admin'
│   │   └── Requests/
│   │       ├── LoginRequest.php             ← Validation formulaire connexion
│   │       ├── RegisterRequest.php          ← Validation formulaire inscription
│   │       ├── CommandeRequest.php          ← Validation formulaire commande
│   │       └── ContactRequest.php           ← Validation formulaire contact
│   │
│   └── Models/
│       ├── User.php                         ← Utilisateurs (auth + rôle)
│       ├── Produit.php
│       ├── Categorie.php
│       ├── Parfum.php
│       ├── Commande.php
│       ├── LigneCommande.php
│       ├── SelectionParfum.php
│       ├── Facture.php
│       └── Contact.php
│
├── resources/
│   └── views/
│       ├── layouts/
│       │   └── app.blade.php               ← Layout principal (nav + footer)
│       ├── accueil.blade.php
│       ├── produits/
│       │   ├── index.blade.php             ← Catalogue
│       │   └── show.blade.php              ← Fiche produit + configurateur
│       ├── panier/
│       │   └── index.blade.php
│       ├── commandes/
│       │   ├── create.blade.php            ← Checkout
│       │   └── confirmation.blade.php
│       ├── contact/
│       │   └── index.blade.php
│       ├── compte/
│       │   └── index.blade.php             ← Historique commandes
│       ├── auth/
│       │   ├── login.blade.php
│       │   └── register.blade.php
│       └── admin/
│           ├── dashboard.blade.php
│           ├── commandes/
│           │   ├── index.blade.php          ← liste classique
│           │   ├── show.blade.php           ← détail commande
│           │   ├── kanban.blade.php         ← vue Kanban par statut
│           │   ├── calendrier.blade.php     ← vue Calendrier par date_souhaitee
│           │   └── bon.blade.php            ← bon de commande imprimable
│           ├── crm/
│           │   ├── index.blade.php          ← liste clients avec stats
│           │   └── show.blade.php           ← fiche client (historique complet)
│           └── contacts/
│               └── index.blade.php
│
├── routes/
│   └── web.php                             ← Toutes les routes
│
├── database/
│   └── migrations/                         ← (Optionnel — on peut aussi utiliser le SQL direct)
│
└── public/
    ├── css/
    │   └── style.css                       ← Charte chocolat/or
    └── js/
        ├── panier.js
        └── catalogue.js
```

---

## 🗺️ Routes (`routes/web.php`)

```php
<?php
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Auth\LoginController;
use App\Http\Controllers\Auth\RegisterController;
use App\Http\Controllers\ProduitController;
use App\Http\Controllers\PanierController;
use App\Http\Controllers\CommandeController;
use App\Http\Controllers\ContactController;
use App\Http\Controllers\CompteController;
use App\Http\Controllers\Admin;

// ─── PUBLIC ────────────────────────────────────────────────
Route::get('/', fn() => view('accueil'))->name('accueil');

Route::get('/catalogue',          [ProduitController::class, 'index'])->name('produits.index');
Route::get('/produit/{id}',       [ProduitController::class, 'show'])->name('produits.show');

Route::get('/contact',            [ContactController::class, 'show'])->name('contact.show');
Route::post('/contact',           [ContactController::class, 'store'])->name('contact.store');

// ─── AUTH ───────────────────────────────────────────────────
Route::get('/connexion',          [LoginController::class, 'showForm'])->name('login');
Route::post('/connexion',         [LoginController::class, 'login']);
Route::post('/deconnexion',       [LoginController::class, 'logout'])->name('logout');

Route::get('/inscription',        [RegisterController::class, 'showForm'])->name('register');
Route::post('/inscription',       [RegisterController::class, 'register']);

// ─── PANIER (accessible sans connexion) ────────────────────
Route::get('/panier',             [PanierController::class, 'show'])->name('panier.show');

// ─── CLIENT CONNECTÉ ────────────────────────────────────────
Route::middleware('auth')->group(function () {
    Route::get('/mon-compte',         [CompteController::class, 'index'])->name('compte.index');
    Route::get('/checkout',           [CommandeController::class, 'create'])->name('commandes.create');
    Route::post('/checkout',          [CommandeController::class, 'store'])->name('commandes.store');
    Route::get('/confirmation/{id}',  [CommandeController::class, 'confirmation'])->name('commandes.confirmation');
});

// ─── AJAX (pas d'auth requise pour le panier) ───────────────
Route::post('/ajax/panier/ajouter',   [PanierController::class, 'ajouter'])->name('panier.ajouter');
Route::post('/ajax/panier/modifier',  [PanierController::class, 'modifier'])->name('panier.modifier');
Route::post('/ajax/panier/supprimer', [PanierController::class, 'supprimer'])->name('panier.supprimer');
Route::get('/ajax/panier/compter',    [PanierController::class, 'compter'])->name('panier.compter');
Route::get('/ajax/produits/filtrer',  [ProduitController::class, 'filtrer'])->name('produits.filtrer');

// ─── PAIEMENT ───────────────────────────────────────────────
Route::middleware('auth')->group(function () {
    Route::get('/paiement/{commande}',         [PaiementController::class, 'show'])->name('paiement.show');
    Route::post('/paiement/{commande}/stripe', [PaiementController::class, 'stripeCheckout'])->name('paiement.stripe');
    Route::get('/paiement/succes',             [PaiementController::class, 'succes'])->name('paiement.succes');
    Route::get('/paiement/echec',              [PaiementController::class, 'echec'])->name('paiement.echec');
});
// Webhook Stripe (sans middleware auth — appelé par Stripe)
Route::post('/webhook/stripe', [PaiementController::class, 'webhook'])->name('webhook.stripe');

// ─── ADMIN ──────────────────────────────────────────────────
Route::middleware(['auth', 'admin'])->prefix('admin')->name('admin.')->group(function () {
    Route::get('/',                           [Admin\DashboardController::class, 'index'])->name('dashboard');

    // Commandes
    Route::get('/commandes',                  [Admin\CommandeController::class, 'index'])->name('commandes.index');
    Route::get('/commandes/kanban',           [Admin\CommandeController::class, 'kanban'])->name('commandes.kanban');
    Route::get('/commandes/calendrier',       [Admin\CommandeController::class, 'calendrier'])->name('commandes.calendrier');
    Route::get('/commandes/{id}',             [Admin\CommandeController::class, 'show'])->name('commandes.show');
    Route::patch('/commandes/{id}/statut',    [Admin\CommandeController::class, 'updateStatut'])->name('commandes.statut');
    Route::get('/commandes/{id}/bon',         [Admin\CommandeController::class, 'bon'])->name('commandes.bon');     // imprimable
    Route::post('/commandes/{id}/note',       [Admin\CommandeController::class, 'saveNote'])->name('commandes.note'); // note interne CRM

    // CRM Clients
    Route::get('/crm',                        [Admin\CrmController::class, 'index'])->name('crm.index');
    Route::get('/crm/{id}',                   [Admin\CrmController::class, 'show'])->name('crm.show');

    // Contacts
    Route::get('/contacts',                   [Admin\ContactController::class, 'index'])->name('contacts.index');
});
```

---

## 🔐 Authentification & Middleware

### `app/Http/Middleware/AdminMiddleware.php`

```php
<?php
namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;

class AdminMiddleware
{
    public function handle(Request $request, Closure $next)
    {
        if (!auth()->check() || auth()->user()->role !== 'admin') {
            abort(403, 'Accès interdit.');
        }
        return $next($request);
    }
}
```

Enregistrer dans `bootstrap/app.php` (Laravel 11) ou `app/Http/Kernel.php` (Laravel 10) :
```php
'admin' => \App\Http\Middleware\AdminMiddleware::class,
```

### `app/Models/User.php`

```php
<?php
namespace App\Models;

use Illuminate\Foundation\Auth\User as Authenticatable;

class User extends Authenticatable
{
    protected $fillable = ['nom', 'prenom', 'email', 'password', 'telephone', 'adresse', 'role'];
    protected $hidden   = ['password', 'remember_token'];

    public function isAdmin(): bool
    {
        return $this->role === 'admin';
    }

    public function commandes()
    {
        return $this->hasMany(Commande::class, 'id_utilisateur');
    }
}
```

---

## 📝 Formulaires & Validation

### `app/Http/Controllers/Auth/LoginController.php`

```php
<?php
namespace App\Http\Controllers\Auth;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class LoginController extends Controller
{
    public function showForm()
    {
        return view('auth.login');
    }

    public function login(Request $request)
    {
        $credentials = $request->validate([
            'email'    => ['required', 'email'],
            'password' => ['required'],
        ]);

        if (Auth::attempt($credentials)) {
            $request->session()->regenerate();
            return redirect()->intended(
                auth()->user()->isAdmin() ? route('admin.dashboard') : route('compte.index')
            );
        }

        return back()->withErrors([
            'email' => 'Email ou mot de passe incorrect.',
        ])->onlyInput('email');
    }

    public function logout(Request $request)
    {
        Auth::logout();
        $request->session()->invalidate();
        $request->session()->regenerateToken();
        return redirect()->route('accueil');
    }
}
```

### `app/Http/Requests/RegisterRequest.php`

```php
<?php
namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class RegisterRequest extends FormRequest
{
    public function rules(): array
    {
        return [
            'nom'       => ['required', 'string', 'max:100'],
            'prenom'    => ['required', 'string', 'max:100'],
            'email'     => ['required', 'email', 'unique:users,email'],
            'password'  => ['required', 'min:8', 'confirmed'],   // confirmed = password_confirmation requis
            'telephone' => ['nullable', 'string', 'max:20'],
            'adresse'   => ['nullable', 'string', 'max:255'],
        ];
    }

    public function messages(): array
    {
        return [
            'email.unique'       => 'Cette adresse email est déjà utilisée.',
            'password.min'       => 'Le mot de passe doit contenir au moins 8 caractères.',
            'password.confirmed' => 'Les mots de passe ne correspondent pas.',
        ];
    }
}
```

### Vue Blade avec affichage des erreurs (`resources/views/auth/register.blade.php`)

```blade
@extends('layouts.app')

@section('content')
<form method="POST" action="{{ route('register') }}">
    @csrf

    <div class="form-group">
        <label for="email">Email *</label>
        <input type="email"
               id="email"
               name="email"
               value="{{ old('email') }}"
               class="{{ $errors->has('email') ? 'input-error' : '' }}">
        @error('email')
            <span class="error-msg">{{ $message }}</span>
        @enderror
    </div>

    <div class="form-group">
        <label for="password">Mot de passe *</label>
        <input type="password" id="password" name="password">
        @error('password')
            <span class="error-msg">{{ $message }}</span>
        @enderror
    </div>

    <div class="form-group">
        <label for="password_confirmation">Confirmer le mot de passe *</label>
        <input type="password" id="password_confirmation" name="password_confirmation">
    </div>

    <button type="submit" class="btn-chocolat">Créer mon compte</button>
</form>
@endsection
```

---

## 🛒 Panier en Session

### `app/Http/Controllers/PanierController.php`

```php
<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Models\Produit;

class PanierController extends Controller
{
    public function show()
    {
        $panier = session('panier', ['items' => [], 'total_ttc' => 0]);
        return view('panier.index', compact('panier'));
    }

    public function ajouter(Request $request)
    {
        $data = $request->validate([
            'id_produit' => ['required', 'integer', 'exists:produits,id'],
            'quantite'   => ['required', 'integer', 'min:1'],
            'parfums'    => ['nullable', 'array'],
        ]);

        $produit = Produit::findOrFail($data['id_produit']);
        $panier  = session('panier', ['items' => [], 'total_ttc' => 0]);

        // Ajouter ou incrémenter
        $found = false;
        foreach ($panier['items'] as &$item) {
            if ($item['id_produit'] === $produit->id) {
                $item['quantite'] += $data['quantite'];
                $found = true;
                break;
            }
        }
        if (!$found) {
            $panier['items'][] = [
                'id_produit'    => $produit->id,
                'nom'           => $produit->nom,
                'prix_unitaire' => $produit->prix_ttc,
                'quantite'      => $data['quantite'],
                'parfums'       => $data['parfums'] ?? [],
            ];
        }

        // Recalcul total
        $panier['total_ttc'] = collect($panier['items'])
            ->sum(fn($i) => $i['prix_unitaire'] * $i['quantite']);

        session(['panier' => $panier]);

        return response()->json([
            'success'     => true,
            'nb_articles' => count($panier['items']),
            'total'       => number_format($panier['total_ttc'], 2, ',', ' ') . ' €',
        ]);
    }

    public function compter()
    {
        $panier = session('panier', ['items' => []]);
        return response()->json(['nb_articles' => count($panier['items'])]);
    }
}
```

---

## 🧩 Modèles Eloquent

### `app/Models/Produit.php`

```php
<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Produit extends Model
{
    protected $table      = 'produits';
    protected $primaryKey = 'id';
    public $timestamps    = false;

    protected $fillable = ['nom', 'description', 'prix_ttc', 'configurable',
                           'capacite_max', 'disponible', 'image', 'id_categorie', 'id_recette'];

    // Relations
    public function categorie()
    {
        return $this->belongsTo(Categorie::class, 'id_categorie');
    }

    public function parfums()
    {
        return $this->belongsToMany(Parfum::class, 'produits_parfums', 'id_produit', 'id_parfum');
    }

    // Scope : uniquement les produits disponibles
    public function scopeDisponible($query)
    {
        return $query->where('disponible', 1);
    }
}
```

### `app/Models/Commande.php`

```php
<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Commande extends Model
{
    protected $table   = 'commandes';
    public $timestamps = false;

    protected $fillable = ['id_utilisateur', 'date_commande', 'statut',
                           'total_ttc', 'adresse_livraison', 'notes'];

    public function lignes()
    {
        return $this->hasMany(LigneCommande::class, 'id_commande');
    }

    public function utilisateur()
    {
        return $this->belongsTo(User::class, 'id_utilisateur');
    }
}
```

---

## 🔄 Ajax

### `app/Http/Controllers/ProduitController.php` — méthode `filtrer`

```php
public function filtrer(Request $request)
{
    $query = Produit::disponible()->with('categorie');

    if ($request->filled('categorie')) {
        $query->where('id_categorie', $request->categorie);
    }
    if ($request->filled('prix_max')) {
        $query->where('prix_ttc', '<=', $request->prix_max);
    }

    return response()->json($query->get());
}
```

### `public/js/catalogue.js`

```javascript
document.querySelectorAll('.filtre-categorie').forEach(btn => {
    btn.addEventListener('click', function () {
        const categorie = this.dataset.id;

        fetch(`/ajax/produits/filtrer?categorie=${categorie}`)
            .then(res => res.json())
            .then(produits => {
                const grid = document.getElementById('produits-grid');
                grid.innerHTML = '';
                produits.forEach(p => {
                    grid.innerHTML += `
                        <div class="produit-card">
                            <img src="/images/produits/${p.image}" alt="${p.nom}">
                            <h3>${p.nom}</h3>
                            <p>${parseFloat(p.prix_ttc).toFixed(2)} €</p>
                            <a href="/produit/${p.id}" class="btn-chocolat">Voir</a>
                        </div>`;
                });
            });
    });
});
```

---

## 🎨 Layout Blade (`resources/views/layouts/app.blade.php`)

```blade
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', 'Charles & Nadejda — Chocolaterie Artisanale')</title>
    <link rel="stylesheet" href="{{ asset('css/style.css') }}">
    <link href="https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;700&family=Inter:wght@400;500&display=swap" rel="stylesheet">
</head>
<body>

<header>
    <nav class="navbar">
        <a href="{{ route('accueil') }}" class="brand">Charles & Nadejda</a>
        <ul class="nav-links">
            <li><a href="{{ route('produits.index') }}">Boutique</a></li>
            <li><a href="{{ route('contact.show') }}">Contact</a></li>
            <li>
                <a href="{{ route('panier.show') }}" class="panier-link">
                    🛒 <span id="panier-count">0</span>
                </a>
            </li>
            @guest
                <li><a href="{{ route('login') }}">Connexion</a></li>
                <li><a href="{{ route('register') }}">Inscription</a></li>
            @endguest
            @auth
                <li><a href="{{ route('compte.index') }}">Mon compte</a></li>
                @if(auth()->user()->isAdmin())
                    <li><a href="{{ route('admin.dashboard') }}">Admin</a></li>
                @endif
                <li>
                    <form method="POST" action="{{ route('logout') }}" style="display:inline">
                        @csrf
                        <button type="submit" class="btn-link">Déconnexion</button>
                    </form>
                </li>
            @endauth
        </ul>
    </nav>

    @if(session('success'))
        <div class="flash flash-success">{{ session('success') }}</div>
    @endif
    @if(session('error'))
        <div class="flash flash-error">{{ session('error') }}</div>
    @endif
</header>

<main>
    @yield('content')
</main>

<footer>
    <p>© {{ date('Y') }} Charles & Nadejda — Chocolaterie Artisanale, Bruxelles</p>
</footer>

<script src="{{ asset('js/panier.js') }}"></script>
@stack('scripts')
</body>
</html>
```

---

## 📋 Contenu par rôle

| Page | Visiteur | Client connecté | Admin |
|------|----------|-----------------|-------|
| Accueil | ✅ | ✅ | ✅ |
| Catalogue | ✅ | ✅ | ✅ |
| Fiche produit + configurateur | ✅ | ✅ | ✅ |
| Panier (lecture) | ✅ | ✅ | ✅ |
| Checkout + choix date livraison | ❌ → redirect login | ✅ | ✅ |
| Page paiement (Bancontact/Stripe) | ❌ | ✅ | ✅ |
| Confirmation + facture PDF | ❌ | ✅ | ✅ |
| Mon compte + historique | ❌ | ✅ | ✅ |
| Contact | ✅ | ✅ | ✅ |
| Admin / dashboard | ❌ | ❌ | ✅ |
| Admin / commandes (liste) | ❌ | ❌ | ✅ |
| Admin / commandes (kanban) | ❌ | ❌ | ✅ |
| Admin / commandes (calendrier) | ❌ | ❌ | ✅ |
| Admin / bon de commande (print) | ❌ | ❌ | ✅ |
| Admin / CRM clients | ❌ | ❌ | ✅ |
| Admin / fiche client | ❌ | ❌ | ✅ |
| Admin / contacts | ❌ | ❌ | ✅ |

---

---

## 💳 Flux de Commande & Paiement

```
[Client]
   1. Sélectionne produits → Panier (session)
   2. Checkout :
      - Informations livraison
      - Choix date souhaitée (datepicker, min = aujourd'hui + 7 jours)
      - Choix type : livraison ou retrait en magasin
      → POST /checkout → CommandeController@store
         → Validation date_souhaitee >= now() + 7j (règle métier)
         → INSERT commandes (statut = 'en_attente', statut_paiement = 'en_attente')
         → INSERT lignes_commandes + selections_parfums
         → Vider panier session
         → Redirect → /paiement/{commande}

   3. Page Paiement :
      - Récapitulatif commande
      - Bouton "Payer par Bancontact" → Stripe Checkout Session
      → POST /paiement/{commande}/stripe → PaiementController@stripeCheckout
         → Créer Stripe Checkout Session (mode = 'payment', méthode = 'bancontact')
         → Redirect vers URL Stripe

   4. Stripe traite le paiement (Bancontact)
      → Succès : redirect → /paiement/succes?session_id=...
      → Echec   : redirect → /paiement/echec

   5. Webhook Stripe (POST /webhook/stripe) :
      → Vérifie signature
      → event = 'checkout.session.completed'
         → UPDATE commandes SET statut_paiement = 'paye', statut = 'confirmee'
         → INSERT factures (numéro auto, calcul HTVA + TVA)
         → Envoyer email facture au client (Laravel Mail + Mailable)

[Admin — déclenché automatiquement après étape 5]
   → Commande apparaît dans le kanban colonne "Confirmée"
   → Commande apparaît dans le calendrier à la date_souhaitee
   → Fiche client mise à jour dans le CRM
```

### Règle du configurateur (source : ancien site Next.js)

Le client choisit librement les quantités de chaque parfum via des boutons `+` / `-`.
**La règle absolue** : le total doit être **exactement égal** à `capacite_max` du produit.

| Produit | `capacite_max` | Prix TTC | Note |
|---------|----------------|----------|------|
| Ballotin 250g | 19 | 13,00 € | — |
| Ballotin 500g | 39 | 23,00 € | — |
| Boule de Noël 220g | 8 | 15,00 € | Saisonnière oct–jan |
| Boule de Noël 500g | 19 | 29,00 € | Saisonnière oct–jan |
| Chaussure en chocolat | — | 15,00 € | Non configurable |
| Bouteille Champagne | — | 15,00 € | Non configurable |
| Pâte à tartiner | — | 10,00 € | Non configurable |

**TVA 21%** sur tous les produits.

Validation dans `PanierController@ajouter` :
```php
if (array_sum(array_column($data['parfums'], 'quantite')) !== $produit->capacite_max) {
    return response()->json([
        'success' => false,
        'message' => "Sélectionnez exactement {$produit->capacite_max} chocolats.",
    ], 422);
}
```

---

### i18n — Site Français + Anglais (Laravel Localization)

```
resources/lang/
├── fr/
│   ├── messages.php      ← textes UI (boutons, labels, flash messages)
│   └── validation.php    ← erreurs de formulaire
└── en/
    ├── messages.php
    └── validation.php
```

Dans les vues : `{{ __('messages.ajouter_panier') }}`
Switcher FR/EN dans le header → `session(['locale' => 'en'])` → middleware `SetLocale`.
Les noms produits/parfums restent en français dans la BDD — seule l'interface est traduite.

---

### Livraison — Zones et frais

```php
// Dans CommandeRequest — calcul automatique des frais selon le pays
'pays_livraison' => ['required_if:type_reception,livraison', 'in:Belgique,France'],
'code_postal_livraison' => ['required_if:type_reception,livraison', 'string'],
```

```php
// CommandeController@store — calcul des frais avant INSERT
$frais = 0;
if ($data['type_reception'] === 'livraison') {
    $zone = ZoneLivraison::where('pays_code', $data['pays_code'])->first();
    $frais = $zone ? $zone->frais : 9.95;
}
```

| Zone | Frais | Délai |
|------|-------|-------|
| Belgique | 6,95 € | 3 jours ouvrables |
| Nord de la France | 9,95 € | 4 jours ouvrables |
| Retrait en magasin | 0,00 € | date_souhaitee choisie |

---

### Règle des 7 jours — validation dans `CommandeRequest`

```php
'date_souhaitee' => [
    'required',
    'date',
    'after:' . now()->addDays(7)->format('Y-m-d'),
],
// Message d'erreur :
'date_souhaitee.after' => 'La date de réception doit être au minimum 7 jours après aujourd\'hui.',
```

---

## 🗂️ CRM Admin

### Vue Kanban (`admin/commandes/kanban.blade.php`)

Les colonnes correspondent aux statuts de la commande :

```
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│  En attente  │ │  Confirmée   │ │En préparation│ │    Prête     │ │   Livrée     │
│  (paiement)  │ │  (payée)     │ │              │ │  (à récup.)  │ │  / Récupérée │
├──────────────┤ ├──────────────┤ ├──────────────┤ ├──────────────┤ ├──────────────┤
│ CMD #0042    │ │ CMD #0041    │ │ CMD #0038    │ │ CMD #0035    │ │ CMD #0030    │
│ Marie D.     │ │ Jean P.      │ │ Sophie M.    │ │ Luc B.       │ │ Anna K.      │
│ 12/02 → 19/02│ │ 10/02 → 17/02│ │ 08/02 → 15/02│ │ 05/02 → 12/02│ │ 01/02       │
│ 60,00 €      │ │ 95,00 €      │ │ 30,00 €      │ │ 70,00 €      │ │ 35,00 €      │
│ [Voir] [Bon] │ │ [Voir] [Bon] │ │ [Voir] [Bon] │ │ [Voir] [Bon] │ │ [Voir]       │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

Implémentation : colonnes HTML/CSS, chaque carte est un lien vers `admin.commandes.show`. Le changement de statut se fait via un `<select>` + bouton dans la vue détail (pas de drag&drop requis pour l'exam, mais peut être ajouté).

### Vue Calendrier (`admin/commandes/calendrier.blade.php`)

```html
<!-- FullCalendar.js (CDN) -->
<link href='https://cdn.jsdelivr.net/npm/fullcalendar@6/index.global.min.css' rel='stylesheet' />
<script src='https://cdn.jsdelivr.net/npm/fullcalendar@6/index.global.min.js'></script>

<div id='calendar'></div>
<script>
const calendar = new FullCalendar.Calendar(document.getElementById('calendar'), {
    initialView: 'dayGridMonth',
    locale: 'fr',
    events: @json($events),   // passé depuis le controller
});
calendar.render();
</script>
```

Le controller passe les commandes comme events FullCalendar :
```php
$events = Commande::with('utilisateur')
    ->whereNotIn('statut', ['annulee', 'livree'])
    ->get()
    ->map(fn($c) => [
        'title' => "CMD #{$c->id} — {$c->utilisateur->prenom}",
        'start' => $c->date_souhaitee,
        'url'   => route('admin.commandes.show', $c->id),
        'color' => match($c->statut) {
            'en_attente'     => '#C72C48',
            'confirmee'      => '#D4AF37',
            'en_preparation' => '#6F4E37',
            'prete'          => '#3D7A3D',
            default          => '#999',
        },
    ]);
```

### Bon de Commande Imprimable (`admin/commandes/bon.blade.php`)

```blade
@extends('layouts.print')   {{-- layout minimal sans nav --}}

@section('content')
<div class="bon-commande">
    <header class="bon-header">
        <div class="logo-shop">Charles & Nadejda — Chocolaterie Artisanale</div>
        <div class="bon-meta">
            <strong>BON DE COMMANDE #{{ str_pad($commande->id, 4, '0', STR_PAD_LEFT) }}</strong><br>
            Commandé le : {{ $commande->date_commande->format('d/m/Y') }}<br>
            À préparer pour : <strong>{{ \Carbon\Carbon::parse($commande->date_souhaitee)->format('d/m/Y') }}</strong><br>
            Type : {{ $commande->type_reception === 'retrait' ? 'Retrait en magasin' : 'Livraison' }}
        </div>
    </header>

    <section class="client-info">
        <h3>Client</h3>
        <p>{{ $commande->utilisateur->prenom }} {{ $commande->utilisateur->nom }}</p>
        <p>{{ $commande->utilisateur->telephone }}</p>
        @if($commande->type_reception === 'livraison')
            <p>{{ $commande->adresse_livraison }}, {{ $commande->code_postal_livraison }} {{ $commande->ville_livraison }}</p>
        @endif
    </section>

    <table class="bon-lignes">
        <thead>
            <tr><th>Produit</th><th>Parfums choisis</th><th>Qté</th><th>Prix unit.</th><th>Sous-total</th></tr>
        </thead>
        <tbody>
            @foreach($commande->lignes as $ligne)
            <tr>
                <td>{{ $ligne->produit->nom }}</td>
                <td>
                    @foreach($ligne->selections as $sel)
                        {{ $sel->parfum->nom }} (×{{ $sel->quantite }})@if(!$loop->last), @endif
                    @endforeach
                </td>
                <td>{{ $ligne->quantite }}</td>
                <td>{{ number_format($ligne->prix_unitaire, 2) }} €</td>
                <td>{{ number_format($ligne->prix_unitaire * $ligne->quantite, 2) }} €</td>
            </tr>
            @endforeach
        </tbody>
        <tfoot>
            <tr><td colspan="4"><strong>TOTAL TTC</strong></td><td><strong>{{ number_format($commande->total_ttc, 2) }} €</strong></td></tr>
        </tfoot>
    </table>

    @if($commande->notes)
        <section class="notes"><h3>Notes client</h3><p>{{ $commande->notes }}</p></section>
    @endif
</div>

<button class="no-print" onclick="window.print()">🖨️ Imprimer</button>
@endsection
```

CSS print (dans `style.css`) :
```css
@media print {
    .no-print, nav, footer { display: none !important; }
    .bon-commande { font-size: 12pt; }
}
```

### Fiche Client CRM (`Admin\CrmController`)

```php
public function show($id)
{
    $client = User::withCount('commandes')
        ->withSum('commandes', 'total_ttc')
        ->findOrFail($id);

    $commandes = Commande::where('id_client', $id)
        ->with(['lignes.produit', 'factures'])
        ->orderByDesc('date_commande')
        ->get();

    return view('admin.crm.show', compact('client', 'commandes'));
}
```

La vue affiche :
- Coordonnées complètes du client
- Statistiques : nb commandes, CA total, commande moyenne, date 1ère/dernière commande
- Historique de toutes les commandes avec statut et liens
- Zone de notes internes (une par commande, sauvée en AJAX via `admin.commandes.note`)

---

## 📦 Installation & Setup (Docker)

> L'environnement de développement utilise **Docker Desktop**.
> Voir `07_DOCKER_SETUP.md` pour la configuration complète du `docker-compose.yml`.
> L'app C# Windows Forms se connecte directement à `localhost:3306` (port mappé par Docker).

```bash
# 1. Démarrer les containers (MySQL + Laravel + phpMyAdmin)
cd CharlesNadejda_Project
docker-compose up -d

# 2. Créer le projet Laravel (si pas encore fait)
docker-compose exec app composer create-project laravel/laravel .

# 3. Configurer .env Laravel — DB_HOST = nom du service Docker, pas localhost
DB_CONNECTION=mysql
DB_HOST=mysql
DB_PORT=3306
DB_DATABASE=charlesnadejda
DB_USERNAME=root
DB_PASSWORD=root

# 4. Importer le schéma et les données
docker exec -i charlesnadejda_mysql mysql -uroot -proot charlesnadejda < sql/create_database.sql
docker exec -i charlesnadejda_mysql mysql -uroot -proot charlesnadejda < sql/seed_data.sql

# 5. Accès
#    Site Laravel  → http://localhost:8000
#    phpMyAdmin    → http://localhost:8080
#    MySQL (C# app)→ localhost:3306

# 6. Enregistrer le middleware admin dans bootstrap/app.php (Laravel 11) :
# $middleware->alias(['admin' => \App\Http\Middleware\AdminMiddleware::class]);
```

---

## 🚀 Checklist de développement PDWEB (Laravel)

### Infrastructure
- [ ] Installer Laravel + configurer `.env` (BDD charlesnadejda)
- [ ] Importer le script `create_database.sql` + `seed_data.sql`
- [ ] Créer le layout `layouts/app.blade.php` + CSS charte graphique
- [ ] Enregistrer le `AdminMiddleware`

### Modèles
- [ ] `User.php` — avec `isAdmin()` + relation `commandes()`
- [ ] `Produit.php` — avec scopes `disponible()` + relations
- [ ] `Categorie.php`, `Parfum.php`
- [ ] `Commande.php` + `LigneCommande.php` + `SelectionParfum.php`
- [ ] `Facture.php` — avec génération numéro auto (`FAC-YYYY-XXXX`)
- [ ] `Contact.php`

### Auth
- [ ] `LoginController` + vue `auth/login.blade.php`
- [ ] `RegisterController` + `RegisterRequest` + vue `auth/register.blade.php`
- [ ] Routes auth dans `web.php`

### Pages publiques
- [ ] `accueil.blade.php` — vitrine avec produits mis en avant
- [ ] `ProduitController@index` + `produits/index.blade.php` — catalogue
- [ ] `ProduitController@show` + `produits/show.blade.php` — fiche + configurateur
- [ ] `ContactController` + `ContactRequest` + vue contact

### Panier & Commande
- [ ] `PanierController` (show, ajouter, modifier, supprimer, compter)
- [ ] `panier/index.blade.php`
- [ ] `CommandeController` (create, store, confirmation) — middleware `auth`
- [ ] `CommandeRequest` — validation checkout avec règle date_souhaitee >= aujourd'hui + 7j
- [ ] `CompteController@index` + `compte/index.blade.php`

### Paiement
- [ ] `PaiementController` (show, stripeCheckout, succes, echec, webhook)
- [ ] Vue `paiement/show.blade.php` — récapitulatif + bouton Bancontact
- [ ] Intégration Stripe (package `stripe/stripe-php`) — mode test d'abord
- [ ] Après paiement validé → créer `Facture` + envoyer email confirmation
- [ ] Génération numéro facture : `FAC-` + année + `-` + ID paddé sur 4 chiffres

### Ajax
- [ ] `public/js/panier.js` — fetch panier routes
- [ ] `public/js/catalogue.js` — fetch filtres
- [ ] `ProduitController@filtrer` — retourne JSON

### Espace Admin — Commandes
- [ ] `Admin\CommandeController` (index, show, updateStatut, bon, saveNote)
- [ ] Vue `admin/commandes/index.blade.php` — liste avec filtres statut
- [ ] Vue `admin/commandes/show.blade.php` — détail + zone note interne CRM
- [ ] Vue `admin/commandes/kanban.blade.php` — colonnes par statut (drag optionnel)
- [ ] Vue `admin/commandes/calendrier.blade.php` — affichage par date_souhaitee (FullCalendar.js)
- [ ] Vue `admin/commandes/bon.blade.php` — bon de commande imprimable (`@media print` CSS)

### Espace Admin — CRM Clients
- [ ] `Admin\CrmController` (index, show)
- [ ] Vue `admin/crm/index.blade.php` — liste clients avec : nb commandes, total dépensé, dernière commande
- [ ] Vue `admin/crm/show.blade.php` — fiche client : infos + toutes commandes + notes internes

### Espace Admin — Autres
- [ ] `Admin\DashboardController` — stats : commandes du jour, CA semaine, commandes en attente
- [ ] `Admin\ContactController`
- [ ] Middleware `admin` bien déclaré sur le groupe de routes
