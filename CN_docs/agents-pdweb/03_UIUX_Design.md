# Agent #3 — UI/UX Designer
## Design & Composants — Boutique Web ArtisaStock
> Date : 2026-05-19
> Input consommé : Brainstorm + 01_PO + 02_Architect
> Phase : Définition

---

## 1. Charte graphique

Reprise de la charte ArtisaStock existante, adaptée au web :

| Token | Valeur | Usage |
|-------|--------|-------|
| `--color-primary` | `#3C1A0B` | Chocolat foncé — header, titres, boutons principaux |
| `--color-secondary` | `#F5E6C8` | Crème — fond de page, cards |
| `--color-accent` | `#C8A951` | Or — prix, badges, hover |
| `--color-danger` | `#D94F5C` | Rouge framboise — rupture, erreurs |
| `--color-success` | `#4CAF50` | Vert — en stock, confirmations |
| `--color-bg` | `#FFFDF7` | Blanc cassé — fond principal |
| `--color-text` | `#2D1810` | Brun foncé — texte corps |
| `--color-text-light` | `#8B7355` | Brun clair — texte secondaire |

**Typographie :**
- Titres : `Playfair Display` (serif, élégant, artisanal)
- Corps : `Inter` (sans-serif, lisible)
- Tailles : 14px corps, 18px sous-titres, 24px titres pages, 32px hero

**Grille :** 8px base. Container max `1200px` centré. Padding page `24px`.

---

## 2. Layout principal (app.blade.php)

```
┌──────────────────────────────────────────────────────────────┐
│  HEADER                                                       │
│  ┌──────┐  ┌──────────────────────┐  ┌────┐ ┌────┐ ┌──────┐ │
│  │ LOGO │  │ Catalogue  Catégories│  │ 🛒 │ │ 👤 │ │Login │ │
│  └──────┘  └──────────────────────┘  │ (3)│ │Nom │ │      │ │
│                                       └────┘ └────┘ └──────┘ │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│                       CONTENU (slot)                          │
│                       max-w: 1200px                           │
│                       min-h: calc(100vh - header - footer)    │
│                                                               │
├──────────────────────────────────────────────────────────────┤
│  FOOTER                                                       │
│  ArtisaStock © 2026 · Artisanat belge                        │
└──────────────────────────────────────────────────────────────┘
```

**Header conditionnel :**
- Visiteur : `Catalogue` | `S'inscrire` | `Se connecter`
- Client connecté : `Catalogue` | `🛒 Panier (N)` | `Mes commandes` | `Bonjour {Prénom}` (dropdown : Profil, Déconnexion)

**Compteur panier** : badge sur l'icône panier, mis à jour en AJAX après chaque ajout.

---

## 3. Pages web — Wireframes

### Page 1 — Catalogue (`/`)

```
┌──────────────────────────────────────────────────────┐
│ HERO BANNER (optionnel, simple)                       │
│ "Découvrez nos créations artisanales"                │
├──────────────────────────────────────────────────────┤
│ FILTRES CATÉGORIES                                    │
│ [ Tous ] [ Chocolats ] [ Pralinés ] [ Coffrets ]     │
│                                                       │
│ TRI : [ Par défaut ▼ ]                               │
├──────────────────────────────────────────────────────┤
│ GRILLE PRODUITS (3 colonnes desktop, 2 tablet, 1 mob)│
│                                                       │
│ ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │
│ │   [IMAGE]   │  │   [IMAGE]   │  │   [IMAGE]   │   │
│ │             │  │             │  │             │   │
│ │ Nom produit │  │ Nom produit │  │ Nom produit │   │
│ │ Catégorie   │  │ Catégorie   │  │ Catégorie   │   │
│ │             │  │             │  │             │   │
│ │ ██ 25,00 € │  │ ██ 18,50 € │  │ ██ 32,00 € │   │
│ │ ● En stock  │  │ ● En stock  │  │ ● Rupture   │   │
│ │             │  │             │  │   (grisé)   │   │
│ │ [Voir ►]    │  │ [Voir ►]    │  │ [Voir ►]    │   │
│ └─────────────┘  └─────────────┘  └─────────────┘   │
└──────────────────────────────────────────────────────┘
```

**Composant `product-card.blade.php` :**
- Image ratio 4:3, `object-fit: cover`, coins arrondis 8px
- Nom : `font-semibold`, 16px, 2 lignes max (ellipsis)
- Catégorie : `text-sm`, couleur `--color-text-light`
- Prix : `font-bold`, couleur `--color-accent`, 20px
- Badge stock : pastille verte "En stock" ou rouge "Rupture"
- Bouton : lien vers page détail
- Hover : `shadow-lg`, léger `scale(1.02)`, transition 200ms

---

### Page 2 — Détail produit (`/produit/{id}`)

```
┌──────────────────────────────────────────────────────┐
│ ← Retour au catalogue                                │
├──────────────────────────────────────────────────────┤
│                                                       │
│ ┌─────────────────────┐  ┌────────────────────────┐  │
│ │                     │  │ CATÉGORIE              │  │
│ │                     │  │ Nom du produit (H1)    │  │
│ │      IMAGE          │  │                        │  │
│ │      500×375        │  │ Description longue...  │  │
│ │                     │  │ ...                    │  │
│ │                     │  │                        │  │
│ │                     │  │ Prix : 25,00 €         │  │
│ └─────────────────────┘  │                        │  │
│                           │ Stock : 15 disponibles │  │
│                           │ (ou "Rupture de stock")│  │
│                           │                        │  │
│                           │ Quantité : [- 1 +]    │  │
│                           │                        │  │
│                           │ [🛒 Ajouter au panier] │  │
│                           │      (AJAX)            │  │
│                           └────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

**Règles UX :**
- Layout 60/40 (image / infos) — ratio phi
- Bouton "Ajouter" : pleine largeur colonne droite, hauteur 48px (Fitts), couleur `--color-primary`
- Si rupture : bouton désactivé, grisé, texte "Indisponible"
- Sélecteur quantité : min 1, max = stock dispo. Boutons +/- tactiles (44px)
- Feedback AJAX : notification toast "Ajouté au panier ✓" (disparaît après 3s)
- Compteur header mis à jour sans reload

---

### Page 3 — Inscription (`/register`)

```
┌──────────────────────────────────────────────────────┐
│           Créer votre compte                          │
│                                                       │
│  ┌────────────────────────────────────────────────┐   │
│  │ Prénom *          [___________________]        │   │
│  │ Nom *             [___________________]        │   │
│  │ Email *           [___________________]        │   │
│  │ Mot de passe *    [___________________]        │   │
│  │ Confirmer *       [___________________]        │   │
│  │ ─────────────────────────────────────          │   │
│  │ Téléphone         [___________________]        │   │
│  │ Rue               [___________________]        │   │
│  │ Code postal       [_____] Ville [___________]  │   │
│  │ Pays              [Belgique___________▼]       │   │
│  │                                                │   │
│  │           [ Créer mon compte ]                 │   │
│  │                                                │   │
│  │  Déjà un compte ? Se connecter                 │   │
│  └────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────┘
```

**Règles UX :**
- Card centrée, max-width 480px
- Labels au-dessus des champs (pas inline — meilleure lisibilité mobile)
- Champs requis marqués `*`
- Erreurs : bordure rouge + message sous le champ en rouge
- Re-remplissage : tous les champs sauf mot de passe
- Séparateur visuel entre infos obligatoires et adresse optionnelle
- Bouton submit : pleine largeur card, 48px hauteur, `--color-primary`

---

### Page 4 — Login (`/login`)

```
┌──────────────────────────────────────────────────────┐
│              Connexion                                │
│                                                       │
│  ┌────────────────────────────────────────────────┐   │
│  │ Email *           [___________________]        │   │
│  │ Mot de passe *    [___________________]        │   │
│  │                                                │   │
│  │           [ Se connecter ]                     │   │
│  │                                                │   │
│  │  Pas encore de compte ? S'inscrire             │   │
│  └────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────┘
```

- Card centrée, max-width 400px
- Message d'erreur global (pas par champ) : "Email ou mot de passe incorrect"

---

### Page 5 — Panier (`/panier`)

```
┌──────────────────────────────────────────────────────┐
│ Mon panier (3 articles)                               │
├──────────────────────────────────────────────────────┤
│                                                       │
│ ┌──────────────────────────────────────────────────┐  │
│ │ [img] Coffret 12 pralinés    25,00 € × [- 2 +]  │  │
│ │                              = 50,00 €   [🗑]    │  │
│ ├──────────────────────────────────────────────────┤  │
│ │ [img] Tablette noir 70%      8,50 € × [- 1 +]   │  │
│ │                              = 8,50 €    [🗑]    │  │
│ ├──────────────────────────────────────────────────┤  │
│ │ [img] Ballotin mixte         18,00 € × [- 3 +]  │  │
│ │                              = 54,00 €   [🗑]    │  │
│ └──────────────────────────────────────────────────┘  │
│                                                       │
│                           Total : 112,50 €            │
│                                                       │
│           [ Passer commande → ]                       │
│                                                       │
│ Panier vide ? "Votre panier est vide.                 │
│               Découvrez notre catalogue →"            │
└──────────────────────────────────────────────────────┘
```

**Règles UX :**
- Chaque ligne : image mini (64×64), nom, prix unitaire, sélecteur quantité AJAX, sous-total, bouton supprimer
- Modifier quantité → AJAX → recalcul sous-total + total SANS reload
- Supprimer → AJAX → retrait ligne avec animation fade-out
- Total recalculé en temps réel
- Bouton "Passer commande" : visible uniquement si panier non vide

---

### Page 6 — Récap & Paiement (`/commande/recap`)

```
┌──────────────────────────────────────────────────────┐
│ Récapitulatif de commande                             │
├──────────────────────────────────────────────────────┤
│                                                       │
│ ARTICLES                                              │
│ ┌────────────────────────────────────────────────┐    │
│ │ Coffret 12 pralinés    ×2     50,00 €          │    │
│ │ Tablette noir 70%      ×1      8,50 €          │    │
│ │ Ballotin mixte         ×3     54,00 €          │    │
│ ├────────────────────────────────────────────────┤    │
│ │                        Total : 112,50 €        │    │
│ └────────────────────────────────────────────────┘    │
│                                                       │
│ ADRESSE DE LIVRAISON                                  │
│ ┌────────────────────────────────────────────────┐    │
│ │ Rue [pré-rempli depuis profil_____________]    │    │
│ │ CP [____] Ville [_________________________]    │    │
│ │ Pays [Belgique____________________________]    │    │
│ └────────────────────────────────────────────────┘    │
│                                                       │
│ SIMULATION DE PAIEMENT                                │
│ ┌────────────────────────────────────────────────┐    │
│ │  ℹ️ Ceci est une simulation — aucun paiement   │    │
│ │     réel ne sera effectué.                     │    │
│ │                                                │    │
│ │           [ 💳 Simuler le paiement ]           │    │
│ └────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

**Règles UX :**
- Bannière info claire : "Simulation de paiement" (pas d'ambiguïté)
- Adresse pré-remplie depuis le profil client, modifiable
- Bouton paiement : `--color-primary`, 48px, loading spinner au clic (désactivé pendant le traitement)

---

### Page 7 — Confirmation (`/commande/{id}`)

```
┌──────────────────────────────────────────────────────┐
│              ✅ Commande confirmée !                  │
│                                                       │
│  Commande n°42 — 19/05/2026                          │
│                                                       │
│  [Récapitulatif identique au recap]                  │
│                                                       │
│  [ Voir mes commandes ]  [ Retour au catalogue ]     │
└──────────────────────────────────────────────────────┘
```

---

### Page 8 — Historique commandes (`/mes-commandes`)

```
┌──────────────────────────────────────────────────────┐
│ Mes commandes                                         │
├──────────────────────────────────────────────────────┤
│                                                       │
│ ┌────────────────────────────────────────────────┐    │
│ │ #42  │ 19/05/2026 │ 3 articles │ 112,50 € │ Payée│ │
│ │                                    [Voir détail]│    │
│ ├────────────────────────────────────────────────┤    │
│ │ #38  │ 15/05/2026 │ 1 article  │  25,00 € │ Payée│ │
│ │                                    [Voir détail]│    │
│ └────────────────────────────────────────────────┘    │
│                                                       │
│ Pas de commandes ? "Vous n'avez pas encore passé     │
│                     de commande."                     │
└──────────────────────────────────────────────────────┘
```

---

## 4. Écrans ERP — Mini CMS (ArtisaStock WinForms)

### FrmBoutiqueWeb — Écran principal à onglets

```
┌─ Boutique en ligne ──────────────────────────────────────────┐
│ [Catégories] [Produits] [Commandes]                          │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ONGLET CATÉGORIES                                           │
│  ┌──────────────────────────────────────────────────┐        │
│  │ Nom          │ Description       │ Ordre │ Actif │        │
│  ├──────────────┼───────────────────┼───────┼───────┤        │
│  │ Chocolats    │ Tablettes et ...  │   1   │  ✓   │        │
│  │ Pralinés     │ Assortiments ...  │   2   │  ✓   │        │
│  │ Coffrets     │ Coffrets cadeaux  │   3   │  ✓   │        │
│  └──────────────────────────────────────────────────┘        │
│                                                               │
│  [ + Ajouter ]  [ ✏ Modifier ]  [ 🗑 Supprimer ]            │
│                                                               │
│  ONGLET PRODUITS                                             │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Image │ Nom commercial │ Catégorie │ Prix  │Stock│Publié│ │
│  ├───────┼────────────────┼───────────┼───────┼─────┼──────┤ │
│  │ [img] │ Coffret 12 pra │ Pralinés  │25,00€ │ 15  │  ✓  │ │
│  │ [img] │ Tablette 70%   │ Chocolats │ 8,50€ │  0  │  ✗  │ │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  [ + Publier fiche ] [ ✏ Modifier ] [ ⬆⬇ Publier/Dépublier]│
│                                                               │
│  ONGLET COMMANDES                                            │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ N°  │ Client        │ Date       │ Total  │ Statut    │  │
│  ├─────┼───────────────┼────────────┼────────┼───────────┤  │
│  │ 42  │ Dupont Marie  │ 19/05/2026 │112,50€ │ Payée     │  │
│  └────────────────────────────────────────────────────────┘  │
│  Filtre : [ Tous ▼ ]                                         │
│                                                               │
│  Détail commande (panneau bas ou modal) :                    │
│  ┌────────────────────────────────────────────────┐          │
│  │ Client : Marie Dupont · marie@email.com        │          │
│  │ Adresse : Rue de la Loi 42, 1000 Bruxelles    │          │
│  │ Coffret 12 pralinés  ×2  50,00 €              │          │
│  │ Ballotin mixte       ×3  54,00 €              │          │
│  │ Tablette noir 70%    ×1   8,50 €              │          │
│  │                     Total 112,50 €             │          │
│  └────────────────────────────────────────────────┘          │
└──────────────────────────────────────────────────────────────┘
```

**Règles UX ERP :**
- TabControl 3 onglets (pas de navigation séparée)
- DGV template standard ArtisaStock : `Sizable + AllCells + MinimumWidth + Anchor 4 bords`
- Stock dans DGV Produits = colonne calculée (SUM bom_stocks), couleur conditionnelle (rouge si 0)
- Colonne "Publié" : CheckBox dans le DGV avec toggle direct (pas besoin d'ouvrir un form)
- Bouton "Publier fiche" → ComboBox des `bom_fiches` non encore publiées → FrmProduitWebEdit

---

## 5. Responsive breakpoints (Tailwind)

| Breakpoint | Largeur | Grille catalogue | Layout détail |
|------------|---------|-----------------|---------------|
| Desktop | ≥ 1024px | 3 colonnes | 60/40 côte à côte |
| Tablette | 768-1023px | 2 colonnes | 60/40 côte à côte |
| Mobile | < 768px | 1 colonne | Image au-dessus, infos en dessous |

---

## 6. Composants Blade réutilisables

| Composant | Usage | Props |
|-----------|-------|-------|
| `x-product-card` | Card produit dans la grille | `$produit` |
| `x-alert` | Messages flash (succès, erreur) | `$type`, `$message` |
| `x-quantity-selector` | Sélecteur +/- quantité | `$value`, `$max`, `$id` |
| `x-badge-stock` | Pastille "En stock" / "Rupture" | `$enStock` |
| `x-panier-counter` | Compteur panier dans le header | (AJAX auto-update) |

---

## JOURNAL — Agent #3 UI/UX

**Phase :** Définition
**Itération :** 1
**Entrée consommée :** Brainstorm + PO US + Architect Stack
**Output produit :** Charte graphique web, wireframes 8 pages, wireframe ERP (FrmBoutiqueWeb 3 onglets), composants Blade, breakpoints responsive
**Décisions clés :**
- Charte reprise d'ArtisaStock (chocolat/crème/or) → cohérence visuelle
- Playfair Display (titres) + Inter (corps) → élégance artisanale + lisibilité
- Card centrée max-480px pour les formulaires auth → mobile-friendly
- TabControl 3 onglets pour le CMS ERP → simple, tout sur un écran
- AJAX : toast notification + compteur header → feedback immédiat sans reload
**Selfdoubt appliqué :**
- ✅ Certain : wireframes alignés sur les US du PO
- ✅ Certain : règles Fitts/Hick respectées (boutons 48px, max 7 options)
- ⚠️ Probable : Playfair Display disponible via Google Fonts (à vérifier en local sans internet)
**Impact :** Design cohérent, responsive, accessible. Chaque page mappée à une US
**Alerte agent suivant :** Le Backend doit implémenter les composants Blade listés. Le Frontend doit s'assurer que le JS AJAX (panier.js) gère les cas d'erreur (stock insuffisant, session expirée). Attention au fallback si Google Fonts indisponible en local → prévoir `font-family: serif` fallback.
