# Agent #1 — Product Owner
## User Stories & Definition of Done — Boutique Web ArtisaStock
> Date : 2026-05-19
> Input consommé : `docs/BRAINSTORM_PDWEB_Boutique.md`
> Phase : Définition

---

## Contexte examen

L'examen PDWEB évalue : PHP (/10), Formulaire (/10), Sessions (/10), MySQL (/10), AJAX (/5).
Le site doit cocher **tous** ces critères. Ampleur visée : 20-25/25 (qualité > quantité).

---

## Épopée (Epic)

**EPIC-WEB** : En tant qu'artisan, je veux une boutique en ligne connectée à mon ERP ArtisaStock, pour que mes clients puissent consulter mes produits et passer commande, avec un stock synchronisé en temps réel.

---

## User Stories — Côté Laravel (Client Web)

### US-W01 — Inscription client
> En tant que **visiteur**, je veux **créer un compte** avec mes coordonnées, pour **pouvoir passer commande**.

**Critères d'acceptation :**
- [ ] Formulaire : nom, prénom, email, mot de passe, confirmation mot de passe, téléphone (optionnel), adresse complète (rue, CP, ville, pays=Belgique par défaut)
- [ ] Validation serveur PHP : email format valide + unique en DB, mot de passe ≥ 8 caractères, confirmation identique, nom/prénom non vides
- [ ] En cas d'erreur : re-remplissage des champs + messages d'erreur par champ (pas de perte de saisie)
- [ ] Hash BCrypt via `password_hash()` (interopérable C#)
- [ ] INSERT dans `clients`
- [ ] Auto-login après inscription (session démarrée)
- [ ] Redirect vers le catalogue

**Critères PDWEB cochés :** Formulaire ✅ · MySQL ✅ · Sessions ✅ · PHP ✅

**DoD :** Formulaire fonctionnel, validation serveur complète, hash BCrypt, auto-login, pas de régression sur les autres pages.

---

### US-W02 — Connexion client
> En tant que **visiteur ayant un compte**, je veux **me connecter**, pour **accéder à mon panier et mes commandes**.

**Critères d'acceptation :**
- [ ] Formulaire : email + mot de passe
- [ ] Validation : `password_verify()` BCrypt
- [ ] Session PHP : stocke `client_id`, `client_nom`, `client_prenom`
- [ ] En cas d'erreur : message générique "Email ou mot de passe incorrect" (pas d'indication sur lequel est faux)
- [ ] Redirect vers la page précédente ou le catalogue
- [ ] Rate limiting : max 5 tentatives / minute (middleware `throttle` Laravel)

**Critères PDWEB cochés :** Formulaire ✅ · Sessions ✅ · PHP ✅

**DoD :** Login fonctionnel, session persistante, rate limiting actif, messages d'erreur sécurisés.

---

### US-W03 — Déconnexion
> En tant que **client connecté**, je veux **me déconnecter**, pour **sécuriser mon compte**.

**Critères d'acceptation :**
- [ ] Bouton "Déconnexion" visible dans le header quand connecté
- [ ] Destruction de la session PHP (`session_destroy()` ou `Auth::logout()`)
- [ ] Redirect vers la page d'accueil / catalogue

**DoD :** Session détruite, redirect OK, bouton visible uniquement si connecté.

---

### US-W04 — Catalogue produits
> En tant que **visiteur ou client**, je veux **consulter le catalogue** des produits en vente, pour **découvrir l'offre**.

**Critères d'acceptation :**
- [ ] Page publique (pas d'auth requise)
- [ ] Affiche uniquement les produits avec `en_vente = 1` et catégorie `actif = 1`
- [ ] Grille de cards : image, nom commercial, prix, badge "En stock" / "Rupture"
- [ ] Stock calculé dynamiquement : `SUM(bom_stocks.quantite_disponible) WHERE id_fiche = produit.id_bom_fiche`
- [ ] Filtre par catégorie (liens ou boutons)
- [ ] Tri : par défaut `ordre_affichage`, option par prix croissant/décroissant
- [ ] Si client connecté : bouton "Ajouter au panier" sur chaque card. Sinon : "Connectez-vous pour commander"

**Critères PDWEB cochés :** PHP ✅ · MySQL ✅ · Sessions (contenu conditionnel) ✅

**DoD :** Catalogue affiche les produits publiés, stock temps réel, filtre catégorie fonctionne, contenu conditionnel visiteur/client.

---

### US-W05 — Détail produit
> En tant que **visiteur ou client**, je veux **voir le détail d'un produit**, pour **décider si je l'achète**.

**Critères d'acceptation :**
- [ ] Page publique avec URL `/produit/{id}`
- [ ] Affiche : image (grande), nom commercial, description complète, prix, catégorie, état stock (quantité ou "Rupture de stock")
- [ ] Si client connecté + stock > 0 : sélecteur de quantité + bouton "Ajouter au panier" (AJAX)
- [ ] AJAX : l'ajout au panier ne recharge pas la page, affiche une notification de succès + met à jour le compteur panier dans le header
- [ ] Si rupture : bouton désactivé + message

**Critères PDWEB cochés :** PHP ✅ · MySQL ✅ · AJAX ✅ · Sessions ✅

**DoD :** Page détail complète, ajout panier AJAX sans reload, compteur header mis à jour, gestion rupture.

---

### US-W06 — Gestion du panier
> En tant que **client connecté**, je veux **gérer mon panier** (ajouter, modifier quantité, supprimer), pour **préparer ma commande**.

**Critères d'acceptation :**
- [ ] Accès réservé aux clients connectés (middleware)
- [ ] Le panier = `commandes_web` avec `statut = 'panier'`. Un client a au plus 1 panier actif
- [ ] Ajout au panier : INSERT ou UPDATE `commandes_web_lignes` (si produit déjà dans le panier → incrémenter quantité)
- [ ] Prix snapshot : `prix_unitaire` copié depuis `produits_web.prix_vente` au moment de l'ajout
- [ ] Modification quantité : AJAX, recalcul sous-total + total en live
- [ ] Suppression item : AJAX, retrait de la ligne
- [ ] Validation quantité : ne peut pas dépasser le stock disponible. Message d'erreur si tentative
- [ ] Affichage : liste des items (image, nom, prix unit, quantité, sous-total), total général
- [ ] Panier vide : message + lien vers le catalogue

**Critères PDWEB cochés :** AJAX ✅ · Sessions ✅ · MySQL ✅ · PHP ✅

**DoD :** CRUD panier complet en AJAX, validation stock, prix snapshot, total dynamique.

---

### US-W07 — Passer commande (simulation paiement)
> En tant que **client connecté**, je veux **valider mon panier** et **simuler un paiement**, pour **finaliser ma commande**.

**Critères d'acceptation :**
- [ ] Page récapitulatif : items du panier + total + adresse de livraison (pré-remplie depuis profil client, modifiable)
- [ ] Bouton "Payer" (simulation — pas de Stripe)
- [ ] Au clic : transaction MySQL atomique :
  1. Re-vérifier stock disponible pour chaque ligne (anti TOCTOU)
  2. Décrémenter `bom_stocks` en FIFO (date_production ASC)
  3. UPDATE `commandes_web` : `statut = 'payee'`, `date_commande = NOW()`, snapshot `adresse_livraison`
  4. Recalculer `total_ttc` depuis les lignes
- [ ] Si stock insuffisant entre-temps : ROLLBACK, message d'erreur, retour au panier avec items problématiques signalés
- [ ] Si OK : page de confirmation avec n° de commande + récapitulatif

**Critères PDWEB cochés :** PHP ✅ · MySQL (transaction) ✅ · Formulaire ✅ · Sessions ✅

**DoD :** Transaction atomique, FIFO respecté, stock décrémenté, confirmation affichée, gestion d'erreur stock.

---

### US-W08 — Historique des commandes
> En tant que **client connecté**, je veux **consulter mes commandes passées**, pour **suivre mes achats**.

**Critères d'acceptation :**
- [ ] Liste des commandes `payee` du client connecté, triées par date DESC
- [ ] Par commande : n° commande, date, statut, total TTC, nombre d'articles
- [ ] Clic sur une commande → détail : liste des lignes (produit, quantité, prix unitaire, sous-total)
- [ ] Pas d'action possible (pas d'annulation en V1)

**Critères PDWEB cochés :** PHP ✅ · MySQL (jointures) ✅ · Sessions ✅

**DoD :** Liste + détail fonctionnels, données correctes, accès réservé au client propriétaire.

---

### US-W09 — Profil client
> En tant que **client connecté**, je veux **modifier mes coordonnées**, pour **garder mes infos à jour**.

**Critères d'acceptation :**
- [ ] Formulaire pré-rempli : nom, prénom, email (readonly), téléphone, adresse
- [ ] Validation serveur identique à l'inscription (sauf email non modifiable)
- [ ] Champ optionnel : nouveau mot de passe + confirmation (vide = pas de changement)
- [ ] UPDATE `clients`
- [ ] Message de confirmation après sauvegarde

**Critères PDWEB cochés :** Formulaire ✅ · MySQL ✅ · Sessions ✅ · PHP ✅

**DoD :** Formulaire pré-rempli, validation, update OK, message confirmation.

---

## User Stories — Côté ArtisaStock C# (Admin ERP — Mini CMS)

### US-W10 — CRUD Catégories web
> En tant qu'**admin**, je veux **gérer les catégories** de ma boutique en ligne depuis l'ERP, pour **organiser mon catalogue**.

**Critères d'acceptation :**
- [ ] Nouvel écran "Boutique en ligne" accessible via la sidebar (nouveau NavItemId)
- [ ] Onglet / section "Catégories" avec DGV : nom, description, ordre, actif
- [ ] Ajouter / Modifier / Supprimer catégorie
- [ ] Validation : nom unique, non vide
- [ ] Suppression : FK SET NULL sur `produits_web` (le produit perd sa catégorie mais n'est pas supprimé)
- [ ] Soft toggle : `actif = 0` masque la catégorie + ses produits sur le web

**DoD :** CRUD complet, validation, DGV fonctionnel, impact web vérifié.

---

### US-W11 — Publier / dépublier un produit web
> En tant qu'**admin**, je veux **publier une fiche BOM dans la boutique** avec un nom commercial, un prix, une image et une catégorie, pour **la rendre visible aux clients**.

**Critères d'acceptation :**
- [ ] Onglet / section "Produits" dans l'écran Boutique
- [ ] DGV : nom commercial, catégorie, prix, en vente (oui/non), stock dispo (calculé)
- [ ] Ajouter : ComboBox des `bom_fiches` non encore publiées → formulaire (catégorie, nom commercial, description, prix, image via FileDialog)
- [ ] Image : copie locale dans un dossier partagé accessible par Laravel (`storage/app/public/produits/`)
- [ ] Modifier : tous les champs éditables sauf `id_bom_fiche` (non modifiable après création)
- [ ] Toggle `en_vente` : publier / dépublier sans supprimer
- [ ] Supprimer : uniquement si aucune `commandes_web_lignes` ne référence ce produit (RESTRICT)
- [ ] Stock affiché = `SUM(bom_stocks.quantite_disponible) WHERE id_fiche = produit.id_bom_fiche`

**DoD :** Publication fonctionnelle, image copiée, toggle en_vente, stock dynamique affiché.

---

### US-W12 — Consulter les commandes web
> En tant qu'**admin**, je veux **voir les commandes passées par les clients** depuis l'ERP, pour **suivre les ventes**.

**Critères d'acceptation :**
- [ ] Onglet / section "Commandes" dans l'écran Boutique
- [ ] DGV : n° commande, nom client, date, statut, total TTC, nb articles
- [ ] Clic sur une commande → panneau détail : lignes (produit, qté, prix unit, sous-total) + infos client (nom, email, adresse livraison)
- [ ] Filtre par statut (tous / payee / annulee)
- [ ] Lecture seule (pas de modification depuis l'ERP en V1)

**DoD :** Liste + détail fonctionnels, données correctes, filtrage par statut.

---

## Matrice de couverture PDWEB

| Critère examen | User Stories qui couvrent | Poids |
|----------------|--------------------------|-------|
| **PHP** | Toutes (US-W01 à US-W09) | /10 |
| **Formulaire + erreurs** | US-W01 (inscription, 7+ champs), US-W02 (login), US-W09 (profil), US-W07 (adresse livraison) | /10 |
| **Sessions** | US-W02 (login), US-W03 (logout), US-W04/W05 (conditionnel), US-W06/W07/W08/W09 (auth) | /10 |
| **MySQL** | US-W01 (INSERT), US-W04 (SELECT+JOIN+SUM), US-W06 (CRUD), US-W07 (TX FIFO), US-W08 (JOIN) | /10 |
| **AJAX** | US-W05 (ajout panier), US-W06 (quantités + total live) | /5 |

**Couverture : 100% des critères examen — chaque critère couvert par au moins 2 US.**

---

## Priorisation (MoSCoW)

| Priorité | User Stories | Justification |
|----------|-------------|---------------|
| **Must** | US-W01, W02, W03, W04, W06, W07 | Couvrent tous les critères examen obligatoires |
| **Should** | US-W05, W08, W09 | Complètent l'expérience + AJAX |
| **Must (ERP)** | US-W10, W11 | Sans ça, pas de produits sur le web |
| **Should (ERP)** | US-W12 | Consultation commandes = valeur ajoutée pour la démo |

**Ordre d'implémentation recommandé :**
1. Migration SQL v15 (tables)
2. US-W10 + US-W11 (ERP : catégories + produits → alimenter la boutique)
3. US-W01 + US-W02 + US-W03 (Auth Laravel)
4. US-W04 + US-W05 (Catalogue + Détail)
5. US-W06 + US-W07 (Panier + Commande)
6. US-W08 + US-W09 (Historique + Profil)
7. US-W12 (ERP : commandes)

---

## JOURNAL — Agent #1 PO

**Phase :** Définition
**Itération :** 1
**Entrée consommée :** Brainstorm PDWEB finalisé (docs/BRAINSTORM_PDWEB_Boutique.md)
**Output produit :** 12 User Stories (9 Laravel + 3 ERP) avec critères d'acceptation + DoD + matrice examen
**Décisions clés :** Priorisation MoSCoW alignée sur les critères d'examen. Ordre d'implémentation ERP-first (sans produits publiés, le web est vide)
**Selfdoubt appliqué :** Confiance élevée — le brainstorm est complet et toutes les décisions ont été prises
**Impact :** Les US couvrent 100% de la grille PDWEB. L'ordre ERP-first évite de coder des pages web sans données
**Alerte agent suivant :** L'Architect doit prévoir le partage d'images entre C# (FileDialog → dossier local) et Laravel (`storage/` symlink). Le dossier physique doit être le même.
