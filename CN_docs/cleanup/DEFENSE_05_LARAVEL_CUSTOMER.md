# DEFENSE 05 — Parcours Client Laravel (Etapes 10-14)

> Document de reference pour la defense orale. Code EXACT extrait du projet.
> Projet : `site-laravel/` | Framework : Laravel 11 | CSS : Tailwind v4

---

## Table des matieres

1. [Etape 10 — Catalogue](#etape-10--catalogue)
2. [Etape 11 — Inscription](#etape-11--inscription)
3. [Etape 12 — Connexion](#etape-12--connexion)
4. [Etape 13 — Panier (AJAX)](#etape-13--panier-ajax)
5. [Etape 14 — Checkout & Commande](#etape-14--checkout--commande)

---

## Etape 10 — Catalogue

### Architecture

| Couche | Fichier |
|--------|---------|
| Controller | `app/Http/Controllers/CatalogueController.php` |
| Model | `app/Models/ProduitWeb.php`, `app/Models/CategorieWeb.php` |
| Views | `resources/views/catalogue/index.blade.php`, `show.blade.php` |
| Route | `GET /` (index), `GET /produit/{id}` (show) |

### CatalogueController@index — Requete & Filtres

```php
public function index(): \Illuminate\Contracts\View\View
{
    $query = ProduitWeb::where('en_vente', 1)
        ->with('categorie')           // Eager loading — evite N+1
        ->withStockDisponible();      // Sous-requete FLOOR(SUM/output)

    // Filtre par categorie (query string ?categorie=X)
    if (request('categorie')) {
        $query->where('id_categorie', request('categorie'));
    }

    // Tri (query string ?tri=prix_asc|prix_desc|defaut)
    $tri = request('tri', 'defaut');
    match ($tri) {
        'prix_asc'  => $query->orderBy('prix_vente', 'asc'),
        'prix_desc' => $query->orderBy('prix_vente', 'desc'),
        default     => $query->orderBy('ordre_affichage')->orderBy('nom_commercial'),
    };

    $produits   = $query->get();
    $categories = CategorieWeb::where('actif', 1)
        ->orderBy('ordre_affichage')
        ->get();

    return view('catalogue.index', compact('produits', 'categories', 'tri'));
}
```

**SQL genere (simplifie) :**
```sql
SELECT produits_web.*, (
    SELECT FLOOR(COALESCE(SUM(bs.quantite_disponible), 0) / bf.quantite_output)
    FROM bom_stocks bs
    INNER JOIN bom_fiches bf ON bf.id = bs.id_fiche
    WHERE bs.id_fiche = produits_web.id_bom_fiche
      AND bs.quantite_disponible > 0
    GROUP BY bf.quantite_output
) AS stock_calc
FROM produits_web WHERE en_vente = 1 [AND id_categorie = ?]
ORDER BY ordre_affichage, nom_commercial;

SELECT * FROM categories_web WHERE actif = 1 ORDER BY ordre_affichage;
```

**Principe stock_calc** : La sous-requete divise le stock brut (somme des lots disponibles) par `quantite_output` de la fiche BOM, puis arrondit vers le bas avec `FLOOR`. Cela donne le nombre d'unites vendables reelles — aligne avec le calcul cote ERP C#.

### Vue catalogue/index.blade.php — Structure

1. **Category pills** — `<div class="flex flex-wrap gap-2">` avec lien "Tous" + boucle `@foreach($categories as $cat)`.
   - Pill active = `bg-(--color-choco) text-(--color-creme)`.
   - Pill inactive = `bg-white text-(--color-choco) border`.
2. **Select tri** — `<select onchange="window.location.href=this.value">` avec 3 options.
3. **Grille produits** — `grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6`.
4. **Card produit** :
   - Image (ou emoji fallback `&#127851;`)
   - Badge categorie, nom, prix (`text-(--color-or)`)
   - Badge stock vert/rouge via `$produit->en_stock`
   - Lien "Voir le produit" vers `route('produit.show', $produit->id)`

### CatalogueController@show — Page detail

```php
public function show(int $id): \Illuminate\Contracts\View\View
{
    $produit = ProduitWeb::where('id', $id)
        ->where('en_vente', 1)
        ->with('categorie')
        ->withStockDisponible()   // Stock vendable pre-charge (evite requete N+1)
        ->firstOrFail();          // 404 si introuvable

    return view('catalogue.show', compact('produit'));
}
```

### Vue catalogue/show.blade.php — Logique conditionnelle

- Layout 60/40 : image (3/5) + infos (2/5).
- Stock affiche : `{{ number_format($produit->stock_disponible, 0) }} disponible(s)`.
- **3 etats du bouton "Ajouter au panier"** :
  1. `session('client_id') && $produit->en_stock` → bouton fonctionnel + input quantite.
  2. `!session('client_id')` → lien vers login "Connectez-vous pour commander".
  3. Sinon (rupture) → bouton disabled "Indisponible".

### Calcul du stock — Scope + Accessor

**Approche double** : un scope SQL pour les listings (0 requete supplementaire), un accessor avec fallback pour les appels unitaires.

**1. Scope `withStockDisponible` (listing — 0 requete N+1) :**
```php
// ProduitWeb.php
public function scopeWithStockDisponible(Builder $query): Builder
{
    return $query->selectRaw('produits_web.*, (
        SELECT FLOOR(COALESCE(SUM(bs.quantite_disponible), 0) / bf.quantite_output)
        FROM bom_stocks bs
        INNER JOIN bom_fiches bf ON bf.id = bs.id_fiche
        WHERE bs.id_fiche = produits_web.id_bom_fiche
          AND bs.quantite_disponible > 0
        GROUP BY bf.quantite_output
    ) AS stock_calc');
}
```

**2. Accessor `stock_disponible` (utilise stock_calc si charge, sinon fallback DB) :**
```php
public function getStockDisponibleAttribute(): float
{
    // Utilise le cache du scope si disponible
    if (array_key_exists('stock_calc', $this->attributes) && $this->attributes['stock_calc'] !== null) {
        return (float) $this->attributes['stock_calc'];
    }

    // Fallback : requete unitaire (meme formule FLOOR)
    return (float) DB::selectOne('
        SELECT FLOOR(COALESCE(SUM(bs.quantite_disponible), 0) / bf.quantite_output) AS stock
        FROM bom_stocks bs
        INNER JOIN bom_fiches bf ON bf.id = bs.id_fiche
        WHERE bs.id_fiche = ?
          AND bs.quantite_disponible > 0
        GROUP BY bf.quantite_output
    ', [$this->id_bom_fiche])?->stock ?? 0;
}

public function getEnStockAttribute(): bool
{
    return $this->stock_disponible > 0;
}
```

**Principe** : Le produit web est lie a une `bom_fiche` (fiche de production ERP). Le stock vendable = `FLOOR(SUM(quantite_disponible) / quantite_output)` — c'est le nombre d'unites finies realisables a partir du stock brut, aligne avec le calcul cote ERP C#. Le scope charge cette valeur en une seule requete pour le listing ; l'accessor la reutilise ou la recalcule en fallback.

---

## Etape 11 — Inscription

### Architecture

| Couche | Fichier |
|--------|---------|
| Controller | `app/Http/Controllers/Auth/RegisterController.php` |
| FormRequest | `app/Http/Requests/RegisterRequest.php` |
| Model | `app/Models/Client.php` |
| View | `resources/views/auth/register.blade.php` |
| Route | `GET /register` (form), `POST /register` (submit) |
| Middleware | `throttle:5,1` sur POST (5 tentatives/min) |

### RegisterRequest — Regles de validation

```php
public function rules(): array
{
    return [
        'prenom'        => 'required|string|max:100',
        'nom'           => 'required|string|max:100',
        'email'         => 'required|email|max:255|unique:clients,email',
        'password'      => 'required|string|min:8|confirmed',
        'telephone'     => 'nullable|string|max:20',
        'adresse_rue'   => 'nullable|string|max:255',
        'adresse_cp'    => 'nullable|string|max:10',
        'adresse_ville' => 'nullable|string|max:100',
        'adresse_pays'  => 'nullable|string|max:100',
    ];
}
```

**Messages personnalises FR :**
```php
public function messages(): array
{
    return [
        'prenom.required'    => 'Le prenom est obligatoire.',
        'nom.required'       => 'Le nom est obligatoire.',
        'email.required'     => 'L\'adresse email est obligatoire.',
        'email.email'        => 'L\'adresse email n\'est pas valide.',
        'email.unique'       => 'Cette adresse email est deja utilisee.',
        'password.required'  => 'Le mot de passe est obligatoire.',
        'password.min'       => 'Le mot de passe doit contenir au moins 8 caracteres.',
        'password.confirmed' => 'Les mots de passe ne correspondent pas.',
    ];
}
```

### RegisterController@register — Creation du client

```php
public function register(RegisterRequest $request): \Illuminate\Http\RedirectResponse
{
    $client = Client::create([
        'prenom'       => $request->prenom,
        'nom'          => $request->nom,
        'email'        => $request->email,
        'mot_de_passe' => password_hash($request->password, PASSWORD_BCRYPT),
        'telephone'    => $request->telephone,
        'adresse_rue'  => $request->adresse_rue,
        'adresse_cp'   => $request->adresse_cp,
        'adresse_ville' => $request->adresse_ville,
        'adresse_pays' => $request->adresse_pays ?? 'Belgique',
    ]);

    session([
        'client_id'          => $client->id,
        'client_nom'         => $client->nom,
        'client_prenom'      => $client->prenom,
        'panier_count'       => 0,          // Nouveau compte = panier vide
        'client_verified_at' => time(),     // Timestamp pour cache middleware
    ]);
    session()->regenerate();  // Securite : session fixation

    return redirect()->route('catalogue')
        ->with('success', 'Bienvenue ' . $client->prenom . ' ! Votre compte a été créé.');
}
```

**Points de securite :**
- `PASSWORD_BCRYPT` — hachage irreversible.
- `unique:clients,email` — pas de doublon.
- `confirmed` — champ `password_confirmation` requis.
- `session()->regenerate()` — empeche session fixation.
- `throttle:5,1` — rate limiting sur le POST.

### Vue register.blade.php — Affichage des erreurs

Chaque champ utilise le pattern :
```blade
<input ... class="{{ $errors->has('prenom') ? 'border-red-500' : 'border-gray-300' }}">
@error('prenom') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
```

Champs obligatoires marques avec `<span class="text-red-500">*</span>`.
Valeurs conservees en cas d'erreur via `value="{{ old('prenom') }}"`.

### Model Client — Fillable & Hidden

```php
protected $fillable = [
    'nom', 'prenom', 'email', 'mot_de_passe',
    'telephone', 'adresse_rue', 'adresse_cp',
    'adresse_ville', 'adresse_pays',
];
protected $hidden = ['mot_de_passe'];  // Jamais expose
```

---

## Etape 12 — Connexion

### Architecture

| Couche | Fichier |
|--------|---------|
| Controller | `app/Http/Controllers/Auth/LoginController.php` |
| FormRequest | `app/Http/Requests/LoginRequest.php` |
| View | `resources/views/auth/login.blade.php` |
| Route | `GET /login` (form), `POST /login` (submit), `POST /logout` |
| Middleware | `throttle:5,1` sur POST login |

### LoginController@login — Authentification

```php
public function login(LoginRequest $request): \Illuminate\Http\RedirectResponse
{
    $client = Client::where('email', $request->email)
        ->where('actif', 1)   // Seuls les comptes actifs
        ->first();

    if (!$client || !password_verify($request->password, $client->mot_de_passe)) {
        return back()
            ->withInput($request->only('email'))  // Conserve l'email
            ->with('error', 'Email ou mot de passe incorrect.');
    }

    // Charger le compteur panier existant pour le cache session
    $panier = CommandeWeb::where('id_client', $client->id)
        ->where('statut', 'panier')
        ->first();
    $panierCount = $panier ? (int) $panier->lignes()->sum('quantite') : 0;

    session([
        'client_id'          => $client->id,
        'client_nom'         => $client->nom,
        'client_prenom'      => $client->prenom,
        'panier_count'       => $panierCount,       // Cache depuis DB au login
        'client_verified_at' => time(),             // Timestamp pour cache middleware
    ]);
    session()->regenerate();  // Securite : session fixation

    return redirect()->intended(route('catalogue'))
        ->with('success', 'Bon retour, ' . $client->prenom . ' !');
}
```

La validation est extraite dans un `LoginRequest` (FormRequest) :
```php
// app/Http/Requests/LoginRequest.php
public function rules(): array
{
    return [
        'email'    => 'required|email',
        'password' => 'required|string',
    ];
}
```

**Points de securite :**
- `LoginRequest` — validation via FormRequest (separation des responsabilites).
- `password_verify()` — comparaison bcrypt constante-time.
- Message d'erreur generique (ne revele pas si l'email existe).
- `where('actif', 1)` — comptes desactives refuses.
- `session()->regenerate()` — nouveau session ID apres login.
- `redirect()->intended()` — retour a la page demandee initialement.
- `throttle:5,1` — max 5 tentatives par minute par IP.

### LoginController@logout

```php
public function logout()
{
    session()->flush();  // Detruit TOUTES les donnees de session
    return redirect()->route('catalogue')
        ->with('success', 'Vous avez ete deconnecte.');
}
```

### Cles de session stockees

| Cle | Valeur | Usage |
|-----|--------|-------|
| `client_id` | `int` | Identification dans les requetes Eloquent |
| `client_nom` | `string` | Affichage header |
| `client_prenom` | `string` | Affichage header + messages flash |
| `panier_count` | `int` | Badge panier header (cache session, evite 2 requetes DB/page) |
| `client_verified_at` | `int` (timestamp) | Cache middleware — re-verifie en DB apres 5 min |

### Vue login.blade.php

Formulaire minimal : email + password + bouton submit. Message flash `session('error')` affiche dans le layout `app.blade.php`.

---

## Etape 13 — Panier (AJAX)

### Architecture

| Couche | Fichier |
|--------|---------|
| Controller | `app/Http/Controllers/PanierController.php` |
| JS Client | `public/js/panier.js` |
| View | `resources/views/panier/index.blade.php` |
| Model | `CommandeWeb` (statut='panier'), `CommandeWebLigne` |
| Middleware | `client.auth` (toutes les routes panier) |

### Les 5 methodes du PanierController

#### 1. `index()` — Affichage du panier

```php
public function index(): \Illuminate\Contracts\View\View
{
    $panier = $this->getPanierActif();
    return view('panier.index', compact('panier'));
}
```

#### 2. `ajouter(Request $request)` — Ajout AJAX

```php
public function ajouter(Request $request): \Illuminate\Http\JsonResponse
{
    $request->validate([
        'id_produit' => 'required|integer|exists:produits_web,id',
        'quantite'   => 'required|integer|min:1',
    ]);

    $produit = ProduitWeb::withStockDisponible()->findOrFail($request->id_produit);

    // VERIFICATION STOCK (stock vendable = FLOOR(brut / output))
    if ($produit->stock_disponible < $request->quantite) {
        return response()->json([
            'success' => false,
            'message' => 'Stock insuffisant. Disponible : ' . $produit->stock_disponible,
        ]);
    }

    $panier = $this->getOrCreatePanier();

    // Si deja en panier → incrementer (avec re-verification stock)
    $ligne = $panier->lignes()->where('id_produit_web', $produit->id)->first();
    if ($ligne) {
        $newQte = $ligne->quantite + $request->quantite;
        if ($newQte > $produit->stock_disponible) {
            return response()->json([
                'success' => false,
                'message' => 'Quantité maximale atteinte (stock : ' . $produit->stock_disponible . ').',
            ]);
        }
        $ligne->update(['quantite' => $newQte]);
    } else {
        CommandeWebLigne::create([
            'id_commande'    => $panier->id,
            'id_produit_web' => $produit->id,
            'quantite'       => $request->quantite,
            'prix_unitaire'  => $produit->prix_vente,  // Snapshot du prix
        ]);
    }

    $count = $this->refreshPanierCount($panier);

    return response()->json([
        'success'      => true,
        'message'      => $produit->nom_commercial . ' ajouté au panier.',
        'panier_count' => $count,
    ]);
}
```

#### 3. `updateQuantite(Request $request)` — Modification AJAX

```php
public function updateQuantite(Request $request): \Illuminate\Http\JsonResponse
{
    $request->validate([
        'id_ligne' => 'required|integer',
        'quantite' => 'required|integer|min:1',
    ]);

    $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

    // OWNERSHIP CHECK
    $panier = $this->getPanierActif();
    if (!$panier || $ligne->id_commande !== $panier->id) {
        return response()->json(['success' => false, 'message' => 'Accès non autorisé.'], 403);
    }

    // STOCK CHECK
    $produit = $ligne->produit;
    if ($request->quantite > $produit->stock_disponible) {
        return response()->json([
            'success' => false,
            'message' => 'Stock insuffisant (disponible : ' . $produit->stock_disponible . ').',
        ]);
    }

    $ligne->update(['quantite' => $request->quantite]);

    // Recharger les lignes pour avoir les totaux a jour
    $panier->load('lignes');
    $count = $this->refreshPanierCount($panier);

    return response()->json([
        'success'      => true,
        'sous_total'   => number_format($ligne->fresh()->sous_total, 2, ',', ' '),
        'total'        => number_format($panier->lignes->sum('sous_total'), 2, ',', ' '),
        'panier_count' => $count,
    ]);
}
```

**Note** : `$panier->lignes->sum('sous_total')` utilise la collection chargee (pas de requete supplementaire), tandis que `$panier->lignes()->sum(...)` declencherait un nouveau `SELECT SUM(...)`. La collection est preferee car les lignes sont deja en memoire apres `load('lignes')`.

#### 4. `supprimer(Request $request)` — Suppression AJAX

```php
public function supprimer(Request $request): \Illuminate\Http\JsonResponse
{
    $request->validate(['id_ligne' => 'required|integer']);
    $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

    // OWNERSHIP CHECK
    $panier = $this->getPanierActif();
    if (!$panier || $ligne->id_commande !== $panier->id) {
        return response()->json(['success' => false, 'message' => 'Accès non autorisé.'], 403);
    }

    $ligne->delete();

    // Recharger les lignes apres suppression
    $panier->load('lignes');
    $count = $this->refreshPanierCount($panier);

    return response()->json([
        'success'      => true,
        'total'        => number_format($panier->lignes->sum('sous_total'), 2, ',', ' '),
        'panier_count' => $count,
    ]);
}
```

#### 5. `count()` — Badge header AJAX

```php
public function count(): \Illuminate\Http\JsonResponse
{
    return response()->json(['count' => (int) session('panier_count', 0)]);
}
```

Le compteur est lu directement depuis la session — aucune requete DB. Il est mis a jour par `refreshPanierCount()` a chaque operation sur le panier.

### Helpers prives

```php
private function getPanierActif(): ?CommandeWeb
{
    return CommandeWeb::where('id_client', session('client_id'))
        ->where('statut', 'panier')
        ->with('lignes.produit')
        ->first();
}

private function getOrCreatePanier(): CommandeWeb
{
    return CommandeWeb::firstOrCreate(
        ['id_client' => session('client_id'), 'statut' => 'panier'],
        ['total_ttc' => 0]
    );
}

/**
 * Rafraichit le compteur panier en session depuis un panier deja charge.
 * Elimine les requetes DB redondantes (remplace l'ancien getPanierCount).
 */
private function refreshPanierCount(?CommandeWeb $panier = null): int
{
    if (!$panier) {
        $panier = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->first();
    }

    $count = $panier ? (int) $panier->lignes()->sum('quantite') : 0;
    session(['panier_count' => $count]);

    return $count;
}
```

**Difference avec l'ancien `getPanierCount()`** : La methode accepte un panier deja charge en parametre (evite une requete supplementaire), et persiste le compteur en session. Toutes les methodes AJAX (`ajouter`, `updateQuantite`, `supprimer`) appellent `refreshPanierCount($panier)` apres modification.

### JavaScript — public/js/panier.js

**Extraction du token CSRF :**
```javascript
const CSRF = document.querySelector('meta[name="csrf-token"]')?.content;
```

Le layout `app.blade.php` contient : `<meta name="csrf-token" content="{{ csrf_token() }}">`.

**Pattern fetch commun (exemple ajouter) :**
```javascript
async function ajouterAuPanier(idProduit, quantite = 1) {
    if (quantite < 1) { showToast('Quantite minimum : 1', 'error'); return; }

    const res = await fetch('/panier/ajouter', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': CSRF,
            'Accept': 'application/json',
        },
        body: JSON.stringify({ id_produit: idProduit, quantite }),
    });
    const data = await res.json();

    if (data.success) {
        document.getElementById('panier-badge').textContent = data.panier_count;
        showToast(data.message);
    } else {
        showToast(data.message, 'error');
    }
}
```

**3 fonctions AJAX :**

| Fonction | Verbe HTTP | Endpoint | Comportement |
|----------|-----------|----------|--------------|
| `ajouterAuPanier(id, qte)` | POST | `/panier/ajouter` | Update badge + toast |
| `updateQuantite(idLigne, qte)` | PATCH | `/panier/quantite` | `location.reload()` |
| `supprimerDuPanier(idLigne)` | DELETE | `/panier/supprimer` | Animation fade-out + remove DOM |

**Toast notification :**
```javascript
function showToast(message, type = 'success') {
    // Cree un <div> dans #toast-container (fixed top-right)
    // bg-green-600 (success) ou bg-red-600 (error)
    // Auto-disparition apres 3s avec fade-out
}
```

### Securite du panier

1. **CSRF** — Token transmis via header `X-CSRF-TOKEN` lu depuis `<meta>`.
2. **Middleware `client.auth`** — Toutes les routes panier necessitent `session('client_id')`.
3. **Ownership check** — `$ligne->id_commande !== $panier->id` → 403.
4. **Stock validation** — Verifie a l'ajout ET a la modification de quantite.
5. **Validation cote client** — `if (quantite < 1)` avant le fetch (QA-03).

### Concept : le panier EST une commande

Le panier est simplement une `commandes_web` avec `statut = 'panier'`. La transition vers commande confirmee ne cree pas de nouvelle ligne — elle change le statut. Design pattern : "Shopping Cart as Order Draft".

### View Composer — Badge panier (cache session)

```php
// AppServiceProvider::boot()
View::composer('components.header', function ($view) {
    $view->with('panierCount', (int) session('panier_count', 0));
});
```

**Avant** : 2 requetes DB a chaque page (SELECT commande + SUM lignes). **Apres** : lecture session uniquement. Le compteur est maintenu par `refreshPanierCount()` dans PanierController et initialise au login/register.

### sous_total — Colonne SQL calculee

```sql
-- migration_v15_boutique_web.sql
sous_total DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
```

Le `sous_total` est une **colonne GENERATED STORED** au niveau MySQL. Pas d'accessor Laravel, pas de calcul applicatif — le SGBD le maintient automatiquement. Quand `quantite` est modifiee, `sous_total` se met a jour tout seul.

---

## Etape 14 — Checkout & Commande

### Architecture

| Couche | Fichier |
|--------|---------|
| Controller | `app/Http/Controllers/CommandeController.php` |
| FormRequest | `app/Http/Requests/CheckoutRequest.php` |
| Models | `CommandeWeb`, `BomStock` |
| Views | `recap.blade.php`, `confirmation.blade.php`, `historique.blade.php` |
| Routes | `GET /commande/recap`, `POST /commande/valider`, `GET /mes-commandes`, `GET /commande/{id}` |
| Middleware | `client.auth` |

### CommandeController@recap — Recapitulatif

```php
public function recap(): \Illuminate\Contracts\View\View|\Illuminate\Http\RedirectResponse
{
    $panier = CommandeWeb::where('id_client', session('client_id'))
        ->where('statut', 'panier')
        ->with('lignes.produit')
        ->first();

    if (!$panier || $panier->lignes->isEmpty()) {
        return redirect()->route('panier')
            ->with('error', 'Votre panier est vide.');
    }

    $client = Client::findOrFail(session('client_id'));

    return view('commandes.recap', compact('panier', 'client'));
}
```

**Vue recap.blade.php :**
- Colonne gauche (2/3) : liste des articles avec sous-totaux + total.
- Colonne droite (1/3) : formulaire adresse pre-remplie depuis `$client` + mention "Simulation de paiement" + bouton "Simuler le paiement".

### CommandeController@valider — TRANSACTION COMPLETE FIFO

La validation utilise un `CheckoutRequest` (FormRequest) au lieu de validation inline :
```php
// app/Http/Requests/CheckoutRequest.php
class CheckoutRequest extends FormRequest
{
    public function authorize(): bool
    {
        return session()->has('client_id');
    }

    public function rules(): array
    {
        return [
            'adresse_rue'   => 'nullable|string|max:255',
            'adresse_cp'    => 'nullable|string|max:10',
            'adresse_ville' => 'nullable|string|max:100',
            'adresse_pays'  => 'nullable|string|max:100',
        ];
    }
}
```

```php
public function valider(CheckoutRequest $request): \Illuminate\Http\RedirectResponse
{
    DB::beginTransaction();
    try {
        // 1. LOCK le panier (evite double-submit)
        $panier = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->with('lignes.produit')
            ->lockForUpdate()
            ->first();

        if (!$panier || $panier->lignes->isEmpty()) {
            DB::rollBack();
            return redirect()->route('panier')
                ->with('error', 'Votre panier est vide.');
        }

        // 2. DECREMENTATION FIFO pour chaque ligne
        foreach ($panier->lignes as $ligne) {
            $restant = $ligne->quantite;
            $idFiche = $ligne->produit->id_bom_fiche;

            // Chercher les lots par date de production croissante (FIFO)
            $stocks = BomStock::where('id_fiche', $idFiche)
                ->where('quantite_disponible', '>', 0)
                ->orderBy('date_production', 'asc')   // FIFO !
                ->lockForUpdate()                      // Lock pessimiste
                ->get();

            // Verifier stock total
            $totalDispo = $stocks->sum('quantite_disponible');
            if ($totalDispo < $restant) {
                DB::rollBack();
                return redirect()->route('panier')
                    ->with('error', 'Stock insuffisant pour « ' . $ligne->produit->nom_commercial . ' ». Veuillez ajuster votre panier.');
            }

            // Consommer lot par lot
            foreach ($stocks as $stock) {
                if ($restant <= 0) break;

                $aConsommer = min($restant, $stock->quantite_disponible);
                $stock->quantite_disponible -= $aConsommer;
                $stock->save();
                $restant -= $aConsommer;
            }
        }

        // 3. SNAPSHOT adresse livraison
        $adresse = collect([
            $request->adresse_rue,
            trim(($request->adresse_cp ?? '') . ' ' . ($request->adresse_ville ?? '')),
            $request->adresse_pays ?? 'Belgique',
        ])->filter()->implode(', ');

        // 4. FINALISER la commande (panier → payee)
        $panier->update([
            'statut'            => 'payee',
            'date_commande'     => now(),
            'adresse_livraison' => $adresse,
            'total_ttc'         => $panier->lignes->sum('sous_total'),
        ]);

        DB::commit();

        // 5. Reset le compteur panier en session (apres commit)
        session(['panier_count' => 0]);

        return redirect()->route('commande.detail', $panier->id)
            ->with('success', 'Commande validée avec succès !');

    } catch (\Exception $e) {
        DB::rollBack();
        return redirect()->route('panier')
            ->with('error', 'Une erreur est survenue. Veuillez réessayer.');
    }
}
```

### Algorithme FIFO — Explication detaillee

```
Pour chaque ligne du panier :
  1. Identifier la bom_fiche liee au produit
  2. SELECT * FROM bom_stocks
     WHERE id_fiche = ? AND quantite_disponible > 0
     ORDER BY date_production ASC    -- Plus ancien d'abord
     FOR UPDATE                      -- Lock pessimiste
  3. Verifier : SUM(quantite_disponible) >= quantite demandee
  4. Boucle de consommation :
     - Prendre le lot le plus ancien
     - aConsommer = min(restant, lot.quantite_disponible)
     - lot.quantite_disponible -= aConsommer
     - lot.save()
     - restant -= aConsommer
     - Si restant = 0 → stop
```

**Garanties transactionnelles :**
- `DB::beginTransaction()` + `DB::commit()` / `DB::rollBack()`.
- `lockForUpdate()` sur le panier — empeche un second submit concurrent.
- `lockForUpdate()` sur les stocks — empeche 2 commandes de consommer le meme stock.
- Rollback si stock insuffisant → message d'erreur user-friendly.
- `try/catch` global → rollback sur toute exception inattendue.

### Ce qui arrive au panier apres checkout

Le panier **n'est pas supprime**. Son statut passe de `'panier'` a `'payee'`. La prochaine fois que le client ajoute un produit, `getOrCreatePanier()` via `firstOrCreate` creera une NOUVELLE commande avec `statut = 'panier'`.

### CommandeController@historique — Liste des commandes

```php
public function historique(): \Illuminate\Contracts\View\View
{
    $commandes = CommandeWeb::where('id_client', session('client_id'))
        ->where('statut', '!=', 'panier')  // Exclut le panier actif
        ->with('lignes')
        ->orderByDesc('date_commande')
        ->get();

    return view('commandes.historique', compact('commandes'));
}
```

### CommandeController@detail — Detail + Ownership (QA-04)

```php
public function detail(int $id): \Illuminate\Contracts\View\View
{
    // QA-04 : ownership check obligatoire
    $commande = CommandeWeb::where('id', $id)
        ->where('id_client', session('client_id'))    // FILTRE PAR CLIENT
        ->where('statut', '!=', 'panier')
        ->with('lignes.produit')
        ->firstOrFail();  // 404 si pas proprietaire

    return view('commandes.confirmation', compact('commande'));
}
```

**Securite ownership** : Un client ne peut JAMAIS voir la commande d'un autre. Le `where('id_client', session('client_id'))` + `firstOrFail()` garantit un 404 en cas de tentative d'acces a une commande etrangere (pas de message revelateur).

---

## Resume securite — Toutes etapes

| Mesure | Implementation |
|--------|---------------|
| **CSRF** | `@csrf` dans forms + `<meta name="csrf-token">` + header `X-CSRF-TOKEN` pour AJAX |
| **Hachage mdp** | `password_hash(PASSWORD_BCRYPT)` + `password_verify()` |
| **Session fixation** | `session()->regenerate()` apres login/register |
| **Rate limiting** | `throttle:5,1` sur login et register |
| **Ownership** | `where('id_client', session('client_id'))` systematique |
| **Middleware auth** | `client.auth` verifie session + compte actif (cache 5 min) |
| **Lock pessimiste** | `lockForUpdate()` sur stock + panier pendant checkout |
| **Transaction ACID** | `DB::beginTransaction()` / commit / rollBack |
| **Validation entrees** | FormRequest (register, login, checkout) + `$request->validate()` (panier AJAX) |
| **Message generique** | "Email ou mot de passe incorrect" — ne revele pas si l'email existe |
| **XSS** | Blade echappe par defaut avec `{{ }}` |
| **Mass assignment** | `$fillable` defini sur les models |

---

## Middleware client.auth — Garde de session (cache 5 min)

```php
// app/Http/Middleware/ClientAuth.php
public function handle(Request $request, Closure $next): mixed
{
    if (!session()->has('client_id')) {
        return redirect()->route('login')
            ->with('error', 'Connectez-vous pour accéder à cette page.');
    }

    // Re-verifier en DB toutes les 5 minutes (pas a chaque requete)
    $lastCheck = session('client_verified_at', 0);
    if (time() - $lastCheck > 300) {
        $client = Client::where('id', session('client_id'))
            ->where('actif', 1)
            ->first();

        if (!$client) {
            session()->flush();
            return redirect()->route('login')
                ->with('error', 'Compte désactivé ou introuvable.');
        }

        session(['client_verified_at' => time()]);
    }

    return $next($request);
}
```

**Optimisation** : Au lieu de verifier le compte en DB a chaque requete (1 SELECT/page), le middleware ne re-verifie que si `client_verified_at` date de plus de 300 secondes (5 minutes). Le timestamp est initialise au login/register. Cela elimine ~1 requete DB par page pour les clients actifs, tout en gardant la detection de comptes desactives avec un delai maximal de 5 minutes.

---

## Pages d'erreur personnalisees

Les pages 404 et 500 sont personnalisees dans `resources/views/errors/` :
- `404.blade.php` — affichee automatiquement par Laravel quand `firstOrFail()` echoue ou quand une route n'existe pas.
- `500.blade.php` — affichee en cas d'erreur serveur inattendue (mode production).

Ces pages utilisent le layout de la boutique (`@extends('layouts.app')`) pour une experience coherente meme en cas d'erreur.

---

## PHPDoc — Documentation du code

Toutes les classes et methodes publiques du site Laravel ont des PHPDoc complets :
- **Classes** : description du role, proprietes `@property`/`@property-read` sur les models.
- **Methodes** : description, `@param` pour les parametres, `@return` avec le type exact.
- **Return types PHP 8** : toutes les methodes publiques ont des return types natifs (`\Illuminate\Contracts\View\View`, `\Illuminate\Http\JsonResponse`, `\Illuminate\Http\RedirectResponse`, `mixed`).

---

## Configuration Nginx — Performance

Le serveur Nginx est configure avec plusieurs optimisations :
- **gzip** active pour les assets texte (HTML, CSS, JS, JSON).
- **fastcgi_buffers** `16 16k` — buffers agrandis pour eviter les ecritures temporaires sur disque.
- **Cache Vite** — assets `build/` servis avec `Cache-Control: max-age=31536000, immutable` (1 an, hash dans le nom).
- **Cache images** — fichiers statiques servis avec `max-age=604800` (7 jours).
- **Font display** — `font-display: swap` pour eviter le FOIT (Flash of Invisible Text).
