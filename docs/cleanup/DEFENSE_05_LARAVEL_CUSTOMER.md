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
public function index()
{
    $query = ProduitWeb::where('en_vente', 1)
        ->with('categorie');  // Eager loading — evite N+1

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
SELECT * FROM produits_web WHERE en_vente = 1 [AND id_categorie = ?]
ORDER BY ordre_affichage, nom_commercial;

SELECT * FROM categories_web WHERE actif = 1 ORDER BY ordre_affichage;
```

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
public function show(int $id)
{
    $produit = ProduitWeb::where('id', $id)
        ->where('en_vente', 1)
        ->with('categorie')
        ->firstOrFail();  // 404 si introuvable

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

### Calcul du stock — Model accessor

```php
// ProduitWeb.php
public function getStockDisponibleAttribute(): float
{
    return (float) BomStock::where('id_fiche', $this->id_bom_fiche)
        ->where('quantite_disponible', '>', 0)
        ->sum('quantite_disponible');
}

public function getEnStockAttribute(): bool
{
    return $this->stock_disponible > 0;
}
```

**Principe** : Le produit web est lie a une `bom_fiche` (fiche de production ERP). Le stock disponible est la somme de tous les lots (`bom_stocks`) dont `quantite_disponible > 0` pour cette fiche. Le stock est calcule en temps reel depuis la table geree par l'ERP C#.

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
public function register(RegisterRequest $request)
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
        'client_id'     => $client->id,
        'client_nom'    => $client->nom,
        'client_prenom' => $client->prenom,
    ]);
    session()->regenerate();  // Securite : session fixation

    return redirect()->route('catalogue')
        ->with('success', 'Bienvenue ' . $client->prenom . ' !');
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
| View | `resources/views/auth/login.blade.php` |
| Route | `GET /login` (form), `POST /login` (submit), `POST /logout` |
| Middleware | `throttle:5,1` sur POST login |

### LoginController@login — Authentification

```php
public function login(Request $request)
{
    $request->validate([
        'email'    => 'required|email',
        'password' => 'required|string',
    ]);

    $client = Client::where('email', $request->email)
        ->where('actif', 1)   // Seuls les comptes actifs
        ->first();

    if (!$client || !password_verify($request->password, $client->mot_de_passe)) {
        return back()
            ->withInput($request->only('email'))  // Conserve l'email
            ->with('error', 'Email ou mot de passe incorrect.');
    }

    session([
        'client_id'     => $client->id,
        'client_nom'    => $client->nom,
        'client_prenom' => $client->prenom,
    ]);
    session()->regenerate();  // Securite : session fixation

    return redirect()->intended(route('catalogue'))
        ->with('success', 'Bon retour, ' . $client->prenom . ' !');
}
```

**Points de securite :**
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
public function index()
{
    $panier = $this->getPanierActif();
    return view('panier.index', compact('panier'));
}
```

#### 2. `ajouter(Request $request)` — Ajout AJAX

```php
public function ajouter(Request $request)
{
    $request->validate([
        'id_produit' => 'required|integer|exists:produits_web,id',
        'quantite'   => 'required|integer|min:1',
    ]);

    $produit = ProduitWeb::findOrFail($request->id_produit);

    // VERIFICATION STOCK
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
                'message' => 'Quantite maximale atteinte (stock : ' . $produit->stock_disponible . ').',
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

    return response()->json([
        'success'      => true,
        'message'      => $produit->nom_commercial . ' ajoute au panier.',
        'panier_count' => $this->getPanierCount(),
    ]);
}
```

#### 3. `updateQuantite(Request $request)` — Modification AJAX

```php
public function updateQuantite(Request $request)
{
    $request->validate([
        'id_ligne' => 'required|integer',
        'quantite' => 'required|integer|min:1',
    ]);

    $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

    // OWNERSHIP CHECK
    $panier = $this->getPanierActif();
    if (!$panier || $ligne->id_commande !== $panier->id) {
        return response()->json(['success' => false, 'message' => 'Acces non autorise.'], 403);
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
    // ...
}
```

#### 4. `supprimer(Request $request)` — Suppression AJAX

```php
public function supprimer(Request $request)
{
    $request->validate(['id_ligne' => 'required|integer']);
    $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

    // OWNERSHIP CHECK
    $panier = $this->getPanierActif();
    if (!$panier || $ligne->id_commande !== $panier->id) {
        return response()->json(['success' => false, 'message' => 'Acces non autorise.'], 403);
    }

    $ligne->delete();
    // ...
}
```

#### 5. `count()` — Badge header AJAX

```php
public function count()
{
    return response()->json(['count' => $this->getPanierCount()]);
}
```

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

private function getPanierCount(): int
{
    $panier = CommandeWeb::where('id_client', session('client_id'))
        ->where('statut', 'panier')
        ->first();
    return $panier ? (int) $panier->lignes()->sum('quantite') : 0;
}
```

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

### View Composer — Badge panier

```php
// AppServiceProvider::boot()
View::composer('components.header', function ($view) {
    $count = 0;
    if (session()->has('client_id')) {
        $panier = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->first();
        $count = $panier ? (int) $panier->lignes()->sum('quantite') : 0;
    }
    $view->with('panierCount', $count);
});
```

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
| Models | `CommandeWeb`, `BomStock` |
| Views | `recap.blade.php`, `confirmation.blade.php`, `historique.blade.php` |
| Routes | `GET /commande/recap`, `POST /commande/valider`, `GET /mes-commandes`, `GET /commande/{id}` |
| Middleware | `client.auth` |

### CommandeController@recap — Recapitulatif

```php
public function recap()
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

```php
public function valider(Request $request)
{
    $request->validate([
        'adresse_rue'   => 'nullable|string|max:255',
        'adresse_cp'    => 'nullable|string|max:10',
        'adresse_ville' => 'nullable|string|max:100',
        'adresse_pays'  => 'nullable|string|max:100',
    ]);

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
                    ->with('error', 'Stock insuffisant pour ...');
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

        return redirect()->route('commande.detail', $panier->id)
            ->with('success', 'Commande validee avec succes !');

    } catch (\Exception $e) {
        DB::rollBack();
        return redirect()->route('panier')
            ->with('error', 'Une erreur est survenue. Veuillez reessayer.');
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
public function historique()
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
public function detail(int $id)
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
| **Middleware auth** | `client.auth` verifie session + compte actif |
| **Lock pessimiste** | `lockForUpdate()` sur stock + panier pendant checkout |
| **Transaction ACID** | `DB::beginTransaction()` / commit / rollBack |
| **Validation entrees** | FormRequest (register) + `$request->validate()` (login, panier, commande) |
| **Message generique** | "Email ou mot de passe incorrect" — ne revele pas si l'email existe |
| **XSS** | Blade echappe par defaut avec `{{ }}` |
| **Mass assignment** | `$fillable` defini sur les models |

---

## Middleware client.auth — Garde de session

```php
// app/Http/Middleware/ClientAuth.php
public function handle(Request $request, Closure $next)
{
    if (!session()->has('client_id')) {
        return redirect()->route('login')
            ->with('error', 'Connectez-vous pour acceder a cette page.');
    }

    // Verifie que le compte existe toujours et est actif
    $client = Client::where('id', session('client_id'))
        ->where('actif', 1)
        ->first();

    if (!$client) {
        session()->flush();
        return redirect()->route('login')
            ->with('error', 'Compte desactive ou introuvable.');
    }

    return $next($request);
}
```

Double verification : session presente ET compte toujours actif en DB. Si un admin desactive un client cote ERP, la prochaine requete le deconnecte.
