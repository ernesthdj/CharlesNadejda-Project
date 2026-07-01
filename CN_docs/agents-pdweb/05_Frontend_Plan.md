# Agent #5 — Frontend Developer
## Plan d'implémentation Frontend — Boutique Web ArtisaStock
> Date : 2026-05-19
> Input consommé : Brainstorm + 01_PO + 02_Architect + 03_UIUX + 04_Backend
> Phase : Définition

---

## 1. Périmètre Frontend

- **10 vues Blade** (layout + 8 pages + composants)
- **1 fichier JS** (`panier.js`) pour les interactions AJAX
- **Tailwind CSS** compilé via Vite (intégré Laravel)
- **Aucun framework JS** — Vanilla ES6+ avec `fetch()`

---

## 2. Layout principal — `layouts/app.blade.php`

### Structure HTML
```html
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', 'ArtisaStock Boutique')</title>
    @vite(['resources/css/app.css', 'resources/js/app.js'])
    @stack('scripts')
</head>
<body class="bg-[#FFFDF7] text-[#2D1810] font-['Inter'] min-h-screen flex flex-col">

    {{-- HEADER --}}
    @include('components.header')

    {{-- FLASH MESSAGES --}}
    @if(session('success'))
        <x-alert type="success" :message="session('success')" />
    @endif
    @if(session('error'))
        <x-alert type="error" :message="session('error')" />
    @endif

    {{-- CONTENU --}}
    <main class="flex-1 max-w-[1200px] mx-auto w-full px-6 py-8">
        @yield('content')
    </main>

    {{-- FOOTER --}}
    @include('components.footer')

    {{-- TOAST AJAX --}}
    <div id="toast-container" class="fixed top-4 right-4 z-50 space-y-2"></div>

</body>
</html>
```

### Header conditionnel
```blade
<header class="bg-[#3C1A0B] text-[#F5E6C8] shadow-md">
  <div class="max-w-[1200px] mx-auto px-6 py-3 flex items-center justify-between">

    {{-- Logo --}}
    <a href="{{ route('catalogue') }}" class="text-xl font-['Playfair_Display'] font-bold">
        ArtisaStock
    </a>

    {{-- Nav --}}
    <nav class="flex items-center gap-6 text-sm">
        <a href="{{ route('catalogue') }}">Catalogue</a>

        @if(session('client_id'))
            {{-- Client connecté --}}
            <a href="{{ route('panier') }}" class="relative">
                🛒 Panier
                <span id="panier-badge"
                      class="absolute -top-2 -right-4 bg-[#C8A951] text-[#3C1A0B]
                             text-xs font-bold rounded-full w-5 h-5
                             flex items-center justify-center">
                    {{ $panierCount ?? 0 }}
                </span>
            </a>
            <a href="{{ route('commandes.historique') }}">Mes commandes</a>
            <div class="relative group">
                <span class="cursor-pointer">{{ session('client_prenom') }} ▾</span>
                <div class="hidden group-hover:block absolute right-0 mt-1
                            bg-white text-[#2D1810] rounded shadow-lg py-2 min-w-[150px]">
                    <a href="{{ route('profil.edit') }}" class="block px-4 py-2 hover:bg-gray-100">Profil</a>
                    <form method="POST" action="{{ route('logout') }}">
                        @csrf
                        <button class="block w-full text-left px-4 py-2 hover:bg-gray-100">
                            Déconnexion
                        </button>
                    </form>
                </div>
            </div>
        @else
            {{-- Visiteur --}}
            <a href="{{ route('login') }}">Se connecter</a>
            <a href="{{ route('register') }}" class="bg-[#C8A951] text-[#3C1A0B]
                       px-4 py-2 rounded font-semibold hover:brightness-110">
                S'inscrire
            </a>
        @endif
    </nav>
  </div>
</header>
```

---

## 3. JavaScript AJAX — `panier.js`

```javascript
// public/js/panier.js

const CSRF = document.querySelector('meta[name="csrf-token"]')?.content;

// === TOAST NOTIFICATION ===
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `px-4 py-3 rounded shadow-lg text-white text-sm transition-opacity duration-300
        ${type === 'success' ? 'bg-green-600' : 'bg-red-600'}`;
    toast.textContent = message;
    container.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 300); }, 3000);
}

// === AJOUTER AU PANIER ===
async function ajouterAuPanier(idProduit, quantite = 1) {
    try {
        const res = await fetch('/panier/ajouter', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': CSRF },
            body: JSON.stringify({ id_produit: idProduit, quantite })
        });
        const data = await res.json();
        if (data.success) {
            document.getElementById('panier-badge').textContent = data.panier_count;
            showToast(data.message);
        } else {
            showToast(data.message, 'error');
        }
    } catch (e) {
        showToast('Erreur réseau', 'error');
    }
}

// === MODIFIER QUANTITÉ ===
async function updateQuantite(idLigne, nouvelleQuantite) {
    try {
        const res = await fetch('/panier/quantite', {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': CSRF },
            body: JSON.stringify({ id_ligne: idLigne, quantite: nouvelleQuantite })
        });
        const data = await res.json();
        if (data.success) {
            document.querySelector(`[data-ligne="${idLigne}"] .sous-total`).textContent = data.sous_total + ' €';
            document.getElementById('total-panier').textContent = data.total + ' €';
            document.getElementById('panier-badge').textContent = data.panier_count;
        } else {
            showToast(data.message, 'error');
        }
    } catch (e) {
        showToast('Erreur réseau', 'error');
    }
}

// === SUPPRIMER DU PANIER ===
async function supprimerDuPanier(idLigne) {
    try {
        const res = await fetch('/panier/supprimer', {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': CSRF },
            body: JSON.stringify({ id_ligne: idLigne })
        });
        const data = await res.json();
        if (data.success) {
            const row = document.querySelector(`[data-ligne="${idLigne}"]`);
            row.style.opacity = '0';
            setTimeout(() => row.remove(), 300);
            document.getElementById('total-panier').textContent = data.total + ' €';
            document.getElementById('panier-badge').textContent = data.panier_count;
            if (data.panier_count === 0) location.reload(); // afficher message panier vide
        }
    } catch (e) {
        showToast('Erreur réseau', 'error');
    }
}
```

**Points clés :**
- CSRF token lu depuis `<meta>` (pattern Laravel standard)
- `fetch()` avec JSON — pas de jQuery
- Toast auto-disparaissant (3s)
- Fade-out animation sur suppression ligne
- Mise à jour compteur header sans reload
- Gestion d'erreur : try/catch + messages serveur

---

## 4. Composants Blade

### `x-product-card`
```blade
{{-- resources/views/components/product-card.blade.php --}}
@props(['produit'])

<div class="bg-[#F5E6C8] rounded-lg overflow-hidden shadow hover:shadow-lg
            transition-all duration-200 hover:scale-[1.02] flex flex-col">
    {{-- Image --}}
    <div class="aspect-[4/3] overflow-hidden">
        @if($produit->image_path)
            <img src="{{ asset('storage/' . $produit->image_path) }}"
                 alt="{{ $produit->nom_commercial }}"
                 class="w-full h-full object-cover">
        @else
            <div class="w-full h-full bg-[#3C1A0B]/10 flex items-center justify-center text-4xl">
                🍫
            </div>
        @endif
    </div>

    {{-- Infos --}}
    <div class="p-4 flex flex-col flex-1">
        @if($produit->categorie)
            <span class="text-xs text-[#8B7355] uppercase tracking-wide">
                {{ $produit->categorie->nom }}
            </span>
        @endif

        <h3 class="font-semibold mt-1 line-clamp-2">{{ $produit->nom_commercial }}</h3>

        <div class="mt-auto pt-3 flex items-center justify-between">
            <span class="text-xl font-bold text-[#C8A951]">
                {{ number_format($produit->prix_vente, 2, ',', ' ') }} €
            </span>
            <x-badge-stock :enStock="$produit->en_stock" />
        </div>

        <a href="{{ route('produit.show', $produit->id) }}"
           class="mt-3 block text-center bg-[#3C1A0B] text-[#F5E6C8] py-2 rounded
                  hover:brightness-125 transition-all text-sm font-medium">
            Voir le produit
        </a>
    </div>
</div>
```

### `x-badge-stock`
```blade
@props(['enStock'])

@if($enStock)
    <span class="inline-flex items-center gap-1 text-xs font-medium text-green-700 bg-green-100 px-2 py-1 rounded-full">
        <span class="w-2 h-2 bg-green-500 rounded-full"></span> En stock
    </span>
@else
    <span class="inline-flex items-center gap-1 text-xs font-medium text-red-700 bg-red-100 px-2 py-1 rounded-full">
        <span class="w-2 h-2 bg-red-500 rounded-full"></span> Rupture
    </span>
@endif
```

### `x-alert`
```blade
@props(['type' => 'success', 'message'])

<div class="max-w-[1200px] mx-auto px-6 mt-4">
    <div class="px-4 py-3 rounded text-sm font-medium
        {{ $type === 'success' ? 'bg-green-100 text-green-800 border border-green-200' : '' }}
        {{ $type === 'error' ? 'bg-red-100 text-red-800 border border-red-200' : '' }}">
        {{ $message }}
    </div>
</div>
```

### `x-quantity-selector`
```blade
@props(['value' => 1, 'max' => 99, 'ligneId' => null, 'produitId' => null])

<div class="flex items-center gap-2">
    <button onclick="updateQuantite({{ $ligneId }}, {{ $value - 1 }})"
            class="w-8 h-8 rounded bg-gray-200 hover:bg-gray-300 text-lg font-bold
                   disabled:opacity-50 disabled:cursor-not-allowed"
            {{ $value <= 1 ? 'disabled' : '' }}>−</button>

    <span class="w-8 text-center font-semibold">{{ $value }}</span>

    <button onclick="updateQuantite({{ $ligneId }}, {{ $value + 1 }})"
            class="w-8 h-8 rounded bg-gray-200 hover:bg-gray-300 text-lg font-bold
                   disabled:opacity-50 disabled:cursor-not-allowed"
            {{ $value >= $max ? 'disabled' : '' }}>+</button>
</div>
```

---

## 5. Formulaire d'inscription — Gestion d'erreurs

```blade
{{-- Exemple champ avec validation --}}
<div class="mb-4">
    <label for="email" class="block text-sm font-medium mb-1">
        Email <span class="text-red-500">*</span>
    </label>
    <input type="email" id="email" name="email"
           value="{{ old('email') }}"
           class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-[#C8A951] outline-none
                  {{ $errors->has('email') ? 'border-red-500' : 'border-gray-300' }}"
           required>
    @error('email')
        <p class="mt-1 text-sm text-red-600">{{ $message }}</p>
    @enderror
</div>
```

**Pattern répété pour chaque champ.** Blade `old()` + `@error` = re-remplissage + erreur par champ. Critère PDWEB "gestion conviviale des erreurs" cochée.

---

## 6. Tailwind config — Tokens personnalisés

```javascript
// tailwind.config.js
export default {
    content: ['./resources/views/**/*.blade.php', './resources/js/**/*.js'],
    theme: {
        extend: {
            colors: {
                'choco':      '#3C1A0B',
                'creme':      '#F5E6C8',
                'or':         '#C8A951',
                'framboise':  '#D94F5C',
                'bg':         '#FFFDF7',
                'text-main':  '#2D1810',
                'text-light': '#8B7355',
            },
            fontFamily: {
                'display': ['Playfair Display', 'serif'],
                'body':    ['Inter', 'sans-serif'],
            },
        },
    },
}
```

---

## 7. Gestion fonts — Mode local (sans internet)

Puisque l'examen est 100% local, les Google Fonts doivent être embarquées :

```
public/fonts/
├── PlayfairDisplay-Bold.woff2
├── Inter-Regular.woff2
└── Inter-SemiBold.woff2
```

```css
/* resources/css/app.css */
@font-face {
    font-family: 'Playfair Display';
    src: url('/fonts/PlayfairDisplay-Bold.woff2') format('woff2');
    font-weight: 700;
    font-display: swap;
}
@font-face {
    font-family: 'Inter';
    src: url('/fonts/Inter-Regular.woff2') format('woff2');
    font-weight: 400;
    font-display: swap;
}
@font-face {
    font-family: 'Inter';
    src: url('/fonts/Inter-SemiBold.woff2') format('woff2');
    font-weight: 600;
    font-display: swap;
}

@tailwind base;
@tailwind components;
@tailwind utilities;
```

---

## 8. Ordre d'implémentation Frontend

| Étape | Tâche | Dépendance |
|-------|-------|------------|
| 1 | Layout `app.blade.php` + header + footer | Tailwind config |
| 2 | Composants : `x-alert`, `x-badge-stock`, `x-product-card` | Layout |
| 3 | `register.blade.php` + `login.blade.php` | Controllers Auth |
| 4 | `catalogue/index.blade.php` (grille + filtres catégories) | CatalogueController |
| 5 | `catalogue/show.blade.php` (détail produit) | CatalogueController |
| 6 | `panier.js` (AJAX complet) | PanierController |
| 7 | `panier/index.blade.php` | panier.js |
| 8 | `commandes/recap.blade.php` + `confirmation.blade.php` | CommandeController |
| 9 | `commandes/historique.blade.php` | CommandeController |
| 10 | `profil/edit.blade.php` | ProfilController |
| 11 | Responsive testing (1024 / 768 / 375) | Toutes les vues |
| 12 | Fonts locales + fallback test | — |

---

## JOURNAL — Agent #5 Frontend

**Phase :** Définition
**Itération :** 1
**Entrée consommée :** Brainstorm + PO + Architect + UI/UX + Backend
**Output produit :** Layout Blade complet, header conditionnel, panier.js (AJAX fetch), 4 composants Blade, formulaire avec validation visuelle, Tailwind config tokens, fonts locales, ordre d'implémentation
**Décisions clés :**
- Vanilla JS `fetch()` (pas jQuery/Alpine) → explicable à l'oral, minimal
- Fonts embarquées en local (woff2) → fonctionne sans internet
- Tailwind tokens alignés sur la charte ArtisaStock → cohérence visuelle
- CSRF via `<meta>` tag + header `X-CSRF-TOKEN` pour les requêtes AJAX
- Toast notifications pour le feedback AJAX (pattern non-intrusif)
**Selfdoubt appliqué :**
- ✅ Certain : Blade `old()` + `@error` = pattern Laravel standard
- ✅ Certain : `fetch()` + JSON + CSRF header = fonctionne avec Laravel
- ⚠️ Probable : `line-clamp-2` Tailwind nécessite le plugin `@tailwindcss/line-clamp` (à vérifier, intégré depuis Tailwind 3.3+)
- ⚠️ Probable : Vite dev server nécessaire pour compiler Tailwind (`npm run dev`)
**Impact :** 10 vues + 1 JS couvrent toutes les US. Responsive 3 breakpoints
**Alerte agent suivant :** Le QA doit vérifier : (1) cohérence routes Backend ↔ liens Blade, (2) CSRF sur tous les POST/PATCH/DELETE AJAX, (3) le `panier-badge` ID existe bien dans le header pour le JS, (4) fallback image si pas d'upload.
