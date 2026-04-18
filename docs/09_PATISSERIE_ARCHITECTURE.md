# 09 — Architecture Pâtisserie (Gâteaux)

> **Décisions issues des Q&A avec Nadejda** — Session de travail avec Cowork.
> Ce fichier définit les règles métier de la couche pâtisserie, distincte de la chocolaterie.

---

## Principe directeur : 100% générique et auto-configuré

> **Règle absolue de conception** : ArtisaStock ne contient aucune valeur codée en dur.
> Tout — formes, tailles, types de couches, allergènes, décorations, taux de charges, taux horaire —
> est configurable par l'artisane depuis l'interface. Les calculs de quantités et de devis
> s'effectuent **automatiquement** en cascade depuis les fiches ingrédients → recettes de couches
> → fiches gâteau → commande.

La chaîne de calcul complète :

```
Fiche ingrédient  (prix_achat / unité, stock_actuel, seuil_alerte, allergènes)
        ↓
Recette couche    (ingrédients × quantité_ref à taille_ref, mode_scaling)
        ↓
Fiche gâteau      (couches ordonnées, taille_ref, forme, mode classique ou configurable)
        ↓
Commande          (taille choisie, étages, options déco)
        ↓
ArtisaStock calcule automatiquement :
  - Quantités réelles de chaque ingrédient (scaling)
  - Coût matières (prix_achat × quantité_réelle)
  - Coût main d'œuvre (temps_couche × taux_horaire)
  - Charges (% configurable)
  - Devis total → affiché client sur le site web
```

---

## Règles métier fondamentales (Q1–Q4)

| Question | Réponse | Implication technique |
|----------|---------|----------------------|
| **Q1 — Mode production** | Uniquement sur commande | Pas de `stock_gateaux`. La commande déclenche la production. |
| **Q2 — Structure recette** | Un gâteau = N couches ordonnées, chaque couche a sa propre recette | `fiches_gateau` + `couches_gateau` + `recettes_couche` distinctes |
| **Q3 — Gestion tailles** | Calcul proportionnel depuis une taille de référence | Système facteur de scaling automatique (override manuel possible) |
| **Q4 — Pièces montées** | OUI — étages multiples avec tailles différentes | Un gâteau peut avoir N étages, chaque étage = taille + couches indépendantes |

---

## Ce que l'artisane configure dans ArtisaStock

### Niveau 1 — Paramètres globaux (réglages du système)

Tout est modifiable depuis un écran "Paramètres" :

| Paramètre | Exemple | Usage |
|-----------|---------|-------|
| Taux horaire main d'œuvre | 25 €/h | Calcul coût main d'œuvre |
| Taux charges globales | 15 % | Appliqué sur le coût total |
| Supplément mixte pièce montée | 20 € | Si ≥ 2 étages saveurs différentes |
| Marge bénéficiaire cible | 30 % | Suggestion prix de vente |
| Unités de mesure disponibles | g, kg, ml, L, pièce, c.à.s | Dropdown dans fiches ingrédients |

### Niveau 2 — Référentiels (catalogues)

Tous gérables depuis ArtisaStock avec CRUD complet :

| Table | Contenu | Gérable par l'artisane |
|-------|---------|----------------------|
| `formes_gateau` | Rond, Carré, Cœur, Rectangle... | ✅ Ajouter/modifier/archiver |
| `tailles_gateau` | 16cm, 18cm, 20cm... liées à une forme | ✅ Ajouter des tailles par forme |
| `types_couche` | Biscuit, Sirop, Crème, Mousse, Glaçage, Garniture, Décoration | ✅ Ajouter des types |
| `allergenes` | Gluten, Lait, Œufs, Noix... (14 EU) | Pré-rempli EU, extensible |
| `options_decoration` | Anniversaire enfant, Mariage, Communion... | ✅ Ajouter/modifier/archiver + prix |

### Niveau 3 — Fiches ingrédients

Même logique que la chocolaterie, enrichie pour la pâtisserie :

```
ingredients
  id, nom, description
  stock_actuel, unite_stock (g/kg/ml/L/pièce)
  prix_achat_ht, unite_achat (prix au kg, au L...)
  seuil_alerte
  fournisseur_id
  allergenes[]          ← liaison vers table allergenes
  actif
```

### Niveau 4 — Recettes de couches

Chaque couche est une recette indépendante et réutilisable :

```
recettes_couche
  id, nom (ex: "Génoise Vanille", "Crème Mousseline Citron")
  type_couche_id        ← Biscuit / Crème / Glaçage...
  taille_ref            ← ex: 20 (cm), reference pour les calculs
  forme_ref_id          ← Rond (pour le calcul π·r²)
  hauteur_ref           ← ex: 1.5 (cm), utilisé si mode_scaling = volume
  mode_scaling          ← volume | surface_dessus | surface_laterale | fixe | manuel
  temps_preparation     ← en minutes (pour le calcul main d'œuvre)
  notes_artisan
  actif

recettes_couche_ingredients
  recette_couche_id
  ingredient_id
  quantite_ref          ← quantité pour la taille_ref
  unite                 ← g, ml, pièce...
```

**L'artisane peut** : créer une recette "Génoise Vanille 20cm" une seule fois et la réutiliser dans autant de gâteaux qu'elle veut.

### Niveau 5 — Fiches gâteau

```
fiches_gateau
  id, nom (ex: "Fraisier", "Entremet 3 couches")
  description, image_url
  forme_id              ← Rond, Carré...
  taille_ref_id         ← taille de référence pour les recettes de couches
  mode                  ← 'classique' (verrouillé) | 'configurable' (libre)
  disponible_site       ← visible sur le site web client
  actif

fiches_gateau_couches   ← pour les classiques uniquement
  fiche_gateau_id
  recette_couche_id
  ordre                 ← 1, 2, 3...
  verrouillé = true     ← implicite pour les classiques

couches_disponibles_config  ← pour le mode configurable
  type_couche_id        ← ex: l'artisane autorise N recettes de type "Biscuit"
  recette_couche_id
  actif                 ← l'artisane active/désactive une couche du configurateur
```

---

## Calcul automatique des quantités (scaling)

Quand une commande arrive avec une taille choisie, ArtisaStock recalcule chaque couche :

| Mode | Formule facteur | Explication |
|------|----------------|-------------|
| `volume` | (r_cmd² × h_cmd) / (r_ref² × h_ref) | Génoise, mousse — scale en volume |
| `surface_dessus` | r_cmd² / r_ref² | Glaçage miroir — couvre la surface du dessus |
| `surface_laterale` | (r_cmd × h_cmd) / (r_ref × h_ref) | Décoration pourtour |
| `fixe` | 1 (aucun scaling) | Figurines, décos fixes |
| `manuel` | Saisi par l'artisane au moment de la commande | Cas particuliers |

**Pour les formes carrées** : remplacer π·r² par côté² dans les formules.
**La forme et ses dimensions** sont portées par `tailles_gateau` (forme_id → formule associée).

---

## Calcul automatique du devis

```
Pour chaque couche de la commande :
  facteur = f(mode_scaling, taille_choisie, taille_ref)

  Pour chaque ingrédient de la couche :
    quantite_reelle = quantite_ref × facteur
    cout_ingredient = quantite_reelle × (prix_achat / unite_achat)

  cout_main_oeuvre_couche = (temps_preparation / 60) × taux_horaire

Sous-total = Σ cout_ingredient + Σ cout_main_oeuvre_couche
Charges = sous-total × (taux_charges / 100)
Options déco = prix option_decoration sélectionnée
Supplément mixte = si ≥ 2 étages saveurs différentes → montant_supplement_mixte

TOTAL COÛT = sous-total + charges + options_déco + supplément_mixte
PRIX SUGGÉRÉ = total_coût / (1 - marge_cible)
```

L'artisane peut **ajuster le prix final** avant de le valider dans le devis.

---

## Deux modes de gâteau (même philosophie que la chocolaterie)

### Mode Classique (verrouillé)
- Nadejda crée le gâteau avec ses couches figées
- Client choisit : **taille** + **option déco** (si thème)
- Devis calculé automatiquement, prix affiché directement sur le site

### Mode Configurateur (libre)
- Nadejda expose un catalogue de couches disponibles par type
- Client assemble : **base biscuit** + **N couches** + **taille** + **option déco**
- Devis calculé dynamiquement en temps réel dans le configurateur web
- Validation par Nadejda avant confirmation (pour vérifier la cohérence)

---

## Allergènes — propagation automatique

```
ingredient → allergenes[]
    ↓ (automatique)
recette_couche → allergenes (union de tous les ingrédients)
    ↓ (automatique)
fiche_gateau → allergenes (union de toutes les couches)
    ↓
Affiché sur le site web + sur le bon de commande ArtisaStock
```

Ajout d'un allergène à un ingrédient → se propage automatiquement partout.

---

## Règles tarifaires

| Situation | Règle |
|-----------|-------|
| Décoration basique | Incluse dans le prix de base |
| Décoration à thème | + prix `options_decoration` |
| Pièce montée saveurs identiques | Prix normal |
| Pièce montée ≥ 2 saveurs différentes | + `supplement_mixte` sur la commande entière |

---

## Décisions finales Q11–Q16 (session 2)

| Question | Décision | Implication technique |
|----------|----------|-----------------------|
| **Q11 — Tailles** | Système de gabarits 100% libre : l'artisane crée ses formes et saisit les dimensions | `pat_formes` + `pat_gabarits` avec volumes/surfaces auto-calculés |
| **Q12 — Types de couches** | 100% configurable, aucun type hardcodé | `pat_types_couche` : CRUD libre depuis ArtisaStock |
| **Q13 — Hauteur par couche** | Définie par l'artisane sur chaque recette de couche (hauteur_ref_cm) | Champ `hauteur_ref_cm` dans `pat_recettes_couche` |
| **Q14 — Déco à thème** | Catalogue 100% géré par l'artisane | `pat_options_deco` : CRUD libre, prix libre |
| **Q15 — Paramètres financiers** | Rien de codé en dur — table clé/valeur modifiable depuis ArtisaStock | `pat_parametres` : taux_horaire, taux_charges, marge_cible, supplement_mixte |
| **Q16 — Pièces montées** | L'artisane configure librement chaque étage (fiche + gabarit au choix) | `pat_etages` : chaque étage = fiche_gateau + gabarit indépendants |

---

## Architecture DB pâtisserie — 16 tables (préfixe `pat_`)

> Toutes les tables pâtisserie sont préfixées `pat_` pour les distinguer clairement
> des tables chocolaterie. Les `fiches_ingredients` et `lots_ingredients` sont **partagés**.

```
NIVEAU 0 — Référentiels configurables (CRUD libre depuis ArtisaStock)
══════════════════════════════════════════════════════════════════════
pat_formes              ← id, nom, type_calcul (circulaire|rectangulaire|libre), actif
pat_gabarits            ← id, forme_id, nom, dimension_1_cm, dimension_2_cm,
                           hauteur_cm, volume_cm3*, surface_dessus_cm2*, surface_laterale_cm2*
                           (* calculés et stockés au save — utilisés par le moteur de scaling)
pat_types_couche        ← id, nom, mode_scaling_defaut, actif
pat_allergenes          ← id, nom, code_eu (14 EU pré-chargés, extensibles)
pat_options_deco        ← id, nom, description, prix, image_url, actif
pat_parametres          ← cle UNIQUE, valeur, description (table clé/valeur)

NIVEAU 1 — Extension des ingrédients (liaison avec fiches_ingredients existantes)
══════════════════════════════════════════════════════════════════════════════════
pat_ingredients_allergenes ← ingredient_id → fiches_ingredients, allergene_id → pat_allergenes

NIVEAU 2 — Recettes de couches (briques réutilisables)
══════════════════════════════════════════════════════
pat_recettes_couche     ← id, nom, type_couche_id, gabarit_ref_id, hauteur_ref_cm,
                           mode_scaling (override ou NULL = utilise le défaut du type),
                           temps_preparation_min, notes, actif
pat_recettes_couche_ingr← recette_couche_id, ingredient_id, quantite_ref, unite

NIVEAU 3 — Fiches gâteau (templates classiques ou configurables)
════════════════════════════════════════════════════════════════
pat_fiches_gateau       ← id, nom, description, image_url, forme_id, gabarit_ref_id,
                           mode (classique|configurable), disponible_site, actif
pat_fiches_gateau_couches ← fiche_gateau_id, recette_couche_id, ordre
                           (pour mode classique : couches fixes et ordonnées)
pat_couches_dispo_config  ← fiche_gateau_id, type_couche_id, recette_couche_id, actif
                           (pour mode configurable : quelles recettes proposées au client)

NIVEAU 4 — Commandes pâtisserie (déclenche la production)
══════════════════════════════════════════════════════════
pat_commandes           ← id, commande_id → commandes, type (simple|piece_montee),
                           supplement_mixte_applique, notes_artisan,
                           statut_production, date_souhaitee
pat_etages              ← id, pat_commande_id, ordre, fiche_gateau_id, gabarit_id,
                           option_deco_id (NULL si pas de déco thème)
pat_etages_couches      ← id, etage_id, recette_couche_id, ordre
                           (pour mode configurable : couches choisies par le client)

NIVEAU 5 — Devis (calculé par ArtisaStock, validé par l'artisane)
══════════════════════════════════════════════════════════════════
pat_devis               ← id, pat_commande_id, cout_matieres, cout_main_oeuvre,
                           cout_charges, cout_options_deco, supplement_mixte,
                           total_cout, prix_suggere, prix_final,
                           notes_ajustement, date_creation
```

### Calcul des volumes/surfaces dans `pat_gabarits`

L'artisane saisit les dimensions — ArtisaStock calcule et stocke les valeurs :

| type_calcul | volume_cm3 | surface_dessus_cm2 | surface_laterale_cm2 |
|-------------|-----------|-------------------|---------------------|
| `circulaire` | π × r² × h | π × r² | 2 × π × r × h |
| `rectangulaire` | l × w × h | l × w | 2 × (l + w) × h |
| `libre` | saisi manuellement | saisi manuellement | saisi manuellement |

### Propagation automatique des allergènes

```
fiches_ingredients → pat_ingredients_allergenes → pat_allergenes
        ↓ (via recettes_couche_ingr)
pat_recettes_couche → allergènes calculés à la volée (union des ingrédients)
        ↓ (via fiches_gateau_couches)
pat_fiches_gateau   → allergènes calculés à la volée
        ↓
Site web + bon de commande artisan
```

### Pièces montées — logique

Une pièce montée = `pat_commande` avec `type = 'piece_montee'` et N `pat_etages`.
Chaque étage est libre : fiche_gateau et gabarit indépendants.
Le supplément mixte se déclenche automatiquement si ≥ 2 étages ont des `fiche_gateau_id` différents.

---

## Paramètres système (`pat_parametres` — clés pré-définies)

| Clé | Description | Exemple |
|-----|-------------|---------|
| `taux_horaire` | €/heure main d'œuvre | `25.00` |
| `taux_charges` | % charges sur coût total | `15` |
| `marge_cible` | % marge bénéficiaire souhaitée | `30` |
| `supplement_mixte` | € supplément pièce montée saveurs mixtes | `20.00` |
| `tva_taux` | % TVA appliquée | `6` |

> Toutes ces valeurs sont `NULL` à l'installation — l'artisane les définit au premier lancement.
