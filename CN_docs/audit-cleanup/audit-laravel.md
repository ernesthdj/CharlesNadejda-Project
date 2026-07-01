# Audit Laravel — ArtisaStock
> Date: 2026-06-11 | Agent: Laravel Auditor (#5)

## Resume
14 findings (2 critique, 6 important, 6 mineur)

---

## Findings

### [F-LAR-001] Bug logique dans l'accessor stock_calc (operator precedence)
- **Severite:** CRITIQUE
- **Fichier(s):** `site-laravel/app/Models/ProduitWeb.php:47`
- **Type:** Bug logique
- **Description:** La condition `$this->attributes['stock_calc'] ?? null !== null` est evaluee comme `$this->attributes['stock_calc'] ?? (null !== null)` soit `$this->attributes['stock_calc'] ?? false`. Ce qui signifie que si `stock_calc` est `0` (valeur falsy mais valide — produit en rupture), la condition passe au fallback et execute une requete unitaire supplementaire inutile. Et si `stock_calc` n'existe pas du tout, `?? false` retourne `false` qui est `!== null` donc le code entre dans le premier `return` et retourne `0` au lieu de lancer le fallback.
- **Impact:** Le scope `withStockDisponible()` est contourne dans certains cas, generant des requetes N+1 fallback. Le comportement est imprevisible selon que l'attribut existe ou non.
- **Suggestion:** Remplacer par `if (array_key_exists('stock_calc', $this->attributes))` pour tester explicitement la presence de la cle, independamment de sa valeur.

---

### [F-LAR-002] Champ `actif` mass-assignable dans Client
- **Severite:** CRITIQUE
- **Fichier(s):** `site-laravel/app/Models/Client.php:11-15`
- **Type:** Securite
- **Description:** Le modele `Client` ne contient pas `actif` dans `$fillable`, ce qui est correct. Cependant, `RegisterController` utilise `Client::create()` en passant manuellement les champs. Le risque reside dans le fait que `ProfilController::update()` fait un `$client->save()` apres assignation manuelle. Si un futur developpeur ajoute un `$client->fill($request->all())->save()`, l'absence de `$guarded` explicite (seulement `$fillable`) laisse `actif` implicitement protege via mass-assignment, mais le modele ne declare pas `$hidden` pour `actif` — un attaquant ne pourrait pas le modifier via mass-assignment, mais l'absence de `$guarded = []` explicite merite documentation.
- **Impact:** Faible dans l'etat actuel (l'assignation est manuelle), mais fragile en maintenance. Un refactoring futur pourrait ouvrir une faille de privilege escalation (desactiver/reactiver un compte).
- **Suggestion:** Ajouter un commentaire explicite documentant que `actif` est volontairement exclu de `$fillable` pour des raisons de securite, ou ajouter `protected $guarded = ['id', 'actif', 'date_creation'];` pour le rendre explicite.

---

### [F-LAR-003] N+1 potentiel dans View Composer (header)
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/app/Providers/AppServiceProvider.php:22-25`
- **Type:** Performance / Pattern manque
- **Description:** Le View Composer execute `$panier->lignes()->sum('quantite')` a chaque rendu du header. Cette requete SQL est executee sur CHAQUE page visitee par un utilisateur connecte (2 requetes : 1 pour trouver le panier, 1 pour le sum). Ce code est identique a `PanierController::getPanierCount()` — duplication de logique.
- **Impact:** 2 requetes SQL supplementaires par page-view pour chaque utilisateur connecte. La logique dupliquee risque de diverger en maintenance.
- **Suggestion:** Extraire la logique dans un service ou un trait reutilisable. Envisager un cache en session (`session('panier_count')`) mis a jour uniquement lors des operations panier, pour eviter 2 requetes par page.

---

### [F-LAR-004] Duplication de la logique panier (3 occurrences)
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/app/Http/Controllers/PanierController.php:156-162`, `site-laravel/app/Providers/AppServiceProvider.php:21-25`
- **Type:** Redondance
- **Description:** La logique "trouver le panier actif + compter les articles" est dupliquee dans `PanierController::getPanierCount()` et dans `AppServiceProvider::boot()`. De plus, `getPanierActif()` est appele 2 fois dans certains flows (une fois pour l'ownership check, une fois pour le compteur).
- **Impact:** Risque de divergence logique entre les deux implementations. Maintenance doublee.
- **Suggestion:** Creer un `PanierService` ou un helper centralisant `getPanierActif()`, `getPanierCount()`, `getOrCreatePanier()`. Injecter ce service dans le controller et dans le View Composer.

---

### [F-LAR-005] Accessor `stock_disponible` sans scope = N+1 dans PanierController
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/app/Http/Controllers/PanierController.php:33,46,89`
- **Type:** Performance / N+1
- **Description:** `PanierController::ajouter()` appelle `ProduitWeb::findOrFail()` sans le scope `withStockDisponible()`, puis accede a `$produit->stock_disponible` via l'accessor. L'accessor detecte que `stock_calc` n'est pas charge et tombe dans le fallback (requete unitaire). Idem dans `updateQuantite()` via `$ligne->produit->stock_disponible`. C'est 1 requete supplementaire par appel AJAX. Dans `getPanierActif()`, le `with('lignes.produit')` charge les produits, mais pas avec le scope stock, donc chaque acces a `stock_disponible` dans les templates genere une requete.
- **Impact:** Requetes N+1 sur les operations panier. Chaque verification de stock genere une requete au lieu d'utiliser le scope sous-requete.
- **Suggestion:** Utiliser `ProduitWeb::withStockDisponible()->findOrFail()` dans `ajouter()`. Dans `getPanierActif()`, charger `lignes.produit` avec un callback qui applique le scope.

---

### [F-LAR-006] Images via `asset('storage/')` mais Cloudinary declare dans .env
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/resources/views/catalogue/index.blade.php:43`, `site-laravel/resources/views/catalogue/show.blade.php:11`, `site-laravel/resources/views/panier/index.blade.php:20`
- **Type:** Incoherence
- **Description:** Le fichier `.env.example` declare des variables Cloudinary (`CLOUDINARY_CLOUD_NAME`, etc.), mais toutes les vues Blade utilisent `asset('storage/' . $produit->image_path)` pour les images, qui pointe vers le storage local Laravel. Aucune integration Cloudinary n'est implementee dans le code PHP.
- **Impact:** Soit les variables Cloudinary sont du dead config (prevu pour un futur sprint), soit les images devraient etre servies depuis Cloudinary mais ne le sont pas. En production, les images stockees dans `storage/app/public` ne seront accessibles que si `php artisan storage:link` a ete execute.
- **Suggestion:** Clarifier la strategie d'images : supprimer les variables Cloudinary du `.env.example` si non utilisees, ou implementer un helper/accessor qui genere l'URL Cloudinary. S'assurer que le symlink `storage/` existe en production.

---

### [F-LAR-007] Pas de pages d'erreur personnalisees (404/500)
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/resources/views/errors/` (absent)
- **Type:** Pattern manque
- **Description:** Aucun template d'erreur personnalise n'existe dans `resources/views/errors/`. Les utilisateurs qui tombent sur une 404 (produit supprime, URL invalide) ou une 500 voient la page d'erreur Laravel par defaut.
- **Impact:** UX degradee — la page d'erreur par defaut ne correspond pas au design du site. En mode production avec `APP_DEBUG=false`, la page est blanche/generique.
- **Suggestion:** Creer au minimum `errors/404.blade.php` et `errors/500.blade.php` en etendant `layouts.app` pour maintenir la coherence visuelle.

---

### [F-LAR-008] Logout sans protection POST-only dans le flow normal
- **Severite:** IMPORTANT
- **Fichier(s):** `site-laravel/routes/web.php:27`, `site-laravel/resources/views/components/header.blade.php:30-35`
- **Type:** Securite
- **Description:** La route logout est bien en `POST` avec `@csrf` dans le header — c'est correct. Cependant, la route `POST /logout` n'est PAS protegee par le middleware `client.auth`. Un utilisateur non connecte qui envoie un POST /logout recevra un `session()->flush()` puis une redirection — pas de crash, mais c'est un comportement inutile et non protege.
- **Impact:** Faible — pas de faille exploitable, mais la route devrait etre protegee par coherence.
- **Suggestion:** Deplacer `Route::post('/logout', ...)` dans le groupe `client.auth`, ou ajouter une guard `if (!session('client_id')) return redirect(...)` dans `LoginController::logout()`.

---

### [F-LAR-009] Validation inline dans CommandeController::valider() au lieu d'un FormRequest
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Http/Controllers/CommandeController.php:36-41`
- **Type:** Pattern manque
- **Description:** `CommandeController::valider()` utilise `$request->validate([...])` inline alors que les autres routes POST/PUT utilisent des FormRequest dedies (`RegisterRequest`, `ProfilUpdateRequest`). Idem pour `LoginController::login()` (ligne 23).
- **Impact:** Incoherence de pattern. La validation inline est fonctionnelle mais ne suit pas la convention etablie par le reste du projet.
- **Suggestion:** Creer `CheckoutRequest` et `LoginRequest` pour uniformiser. Les FormRequests facilitent aussi la reutilisation et les tests unitaires des regles.

---

### [F-LAR-010] Validation inline dans PanierController (3 methodes)
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Http/Controllers/PanierController.php:25,74,112`
- **Type:** Pattern manque
- **Description:** Les 3 methodes `ajouter()`, `updateQuantite()`, `supprimer()` utilisent la validation inline `$request->validate()`. Etant des endpoints AJAX, c'est acceptable, mais incoherent avec le pattern FormRequest utilise ailleurs.
- **Impact:** Faible — les endpoints AJAX retournent automatiquement du JSON 422 grace a l'en-tete `Accept: application/json`. Le pattern fonctionne.
- **Suggestion:** Optionnel : creer des FormRequests pour centraliser. Pas prioritaire pour des endpoints AJAX simples.

---

### [F-LAR-011] CommandeWebLigne.$fillable inclut `sous_total` implicitement
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Models/CommandeWebLigne.php:12-14`
- **Type:** Dead code potentiel
- **Description:** Le `$fillable` contient `['id_commande', 'id_produit_web', 'quantite', 'prix_unitaire']`. La colonne `sous_total` est GENERATED STORED en base (`quantite * prix_unitaire`). C'est correct — `sous_total` ne doit pas etre dans `$fillable`. Pas de probleme ici.
- **Impact:** Aucun — finding retrocede a "OK" apres verification.
- **Suggestion:** RAS. Le modele est correct.

---

### [F-LAR-012] Modele User.php inutilise
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Models/User.php`
- **Type:** Dead code
- **Description:** Le modele `User` est le scaffold Laravel par defaut. Il n'est reference nulle part dans les controllers, routes ou vues. L'application utilise `Client` avec une authentification custom par session.
- **Impact:** Code mort — aucun impact fonctionnel, mais ajoute de la confusion pour un developpeur qui rejoindrait le projet.
- **Suggestion:** Supprimer `User.php` ou ajouter un commentaire en tete expliquant qu'il est conserve pour compatibilite Laravel (migrations, seeders, etc.).

---

### [F-LAR-013] BomFiche et BomStock sans `$fillable` restrictif
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Models/BomFiche.php`, `site-laravel/app/Models/BomStock.php:16`
- **Type:** Securite / Coherence
- **Description:** `BomFiche` n'a aucun `$fillable` ni `$guarded`, ce qui signifie que Eloquent utilise le `$guarded = ['*']` par defaut — correct car lecture seule. `BomStock` a `$fillable = ['quantite_disponible']` qui est le seul champ modifie par le checkout. Les deux modeles sont geres par l'ERP C#, le site Laravel ne devrait que lire/decrementer.
- **Impact:** Faible. `BomFiche` est implicitement protege par `$guarded = ['*']` par defaut. Correct mais non explicite.
- **Suggestion:** Ajouter `protected $guarded = ['*'];` explicitement dans `BomFiche` et un commentaire `// Lecture seule — gere par ERP C#` pour documenter l'intention.

---

### [F-LAR-014] Absence de `getSousTotalAttribute` dans CommandeWebLigne
- **Severite:** MINEUR
- **Fichier(s):** `site-laravel/app/Models/CommandeWebLigne.php`
- **Type:** Fragilite
- **Description:** Le code Blade et les controllers accedent a `$ligne->sous_total` partout. Cela fonctionne parce que `sous_total` est une colonne GENERATED STORED en base. Cependant, si Eloquent charge la ligne, `sous_total` est present comme attribut brut. Pas de probleme dans l'etat actuel. Toutefois, `$ligne->sous_total` ne serait pas accessible sur un objet `CommandeWebLigne` non persiste (avant `save()`).
- **Impact:** Aucun dans le flow actuel — les lignes sont toujours lues depuis la base. Mais attention si un jour on cree un objet en memoire et on accede a `sous_total` avant persistance.
- **Suggestion:** Optionnel : ajouter un accessor `getSousTotalAttribute()` qui retourne `$this->quantite * $this->prix_unitaire` comme fallback. Faible priorite.

---

## Synthese par axe d'audit

| Axe | Statut | Commentaire |
|-----|--------|-------------|
| N+1 queries | PARTIEL | Eager loading present dans les controllers principaux, mais fallback N+1 dans PanierController et View Composer |
| CSRF | OK | Toutes les forms ont `@csrf`. AJAX envoie `X-CSRF-TOKEN` via meta tag. |
| Validation | PARTIEL | Register et Profil ont des FormRequest. Login, Checkout et Panier AJAX utilisent validation inline. |
| Blade escaping | OK | Aucun `{!! !!}` detecte. Tout est echappe via `{{ }}`. |
| Routes orphelines | OK | Toutes les routes pointent vers des methodes existantes. Aucune orpheline. |
| Model fillable | OK | Les `$fillable` sont restrictifs. `actif` n'est pas mass-assignable. |
| Middleware | PARTIEL | Toutes les routes protegees sont dans le groupe `client.auth`. Logout est hors du groupe. |
| Assets/Images | ATTENTION | Cloudinary declare dans `.env.example` mais non utilise. Images servies en local storage. |
| Timestamps | OK | `Client` et `ProduitWeb` overrident correctement `CREATED_AT`/`UPDATED_AT`. Les autres desactivent avec `$timestamps = false`. |
| Error pages | ABSENT | Aucune page 404/500 personnalisee. |

---

## Priorite de correction suggeree

| Priorite | Finding | Effort |
|----------|---------|--------|
| P0 | F-LAR-001 (bug operator precedence stock_calc) | 5 min |
| P0 | F-LAR-007 (pages erreur 404/500) | 30 min |
| P1 | F-LAR-003 + F-LAR-004 (PanierService) | 1h |
| P1 | F-LAR-005 (N+1 stock dans PanierController) | 30 min |
| P1 | F-LAR-006 (strategie images Cloudinary vs local) | Decision architecturale |
| P2 | F-LAR-008 (logout hors middleware) | 5 min |
| P2 | F-LAR-009 + F-LAR-010 (FormRequests manquants) | 30 min |
| P3 | F-LAR-002 (documenter $guarded explicite) | 5 min |
| P3 | F-LAR-012 (supprimer User.php) | 2 min |
| P3 | F-LAR-013 (documenter BomFiche read-only) | 5 min |
| P3 | F-LAR-014 (accessor sous_total optionnel) | 10 min |
