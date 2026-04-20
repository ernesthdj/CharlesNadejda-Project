# Agent #1 — PO · ArtisaStock · User Stories & Definition of Done

> **Produit par :** Agent #1 Product Owner — Pipeline multi-agents ArtisaStock
> **Date :** 2026-04-19
> **Version :** 1.0
> **Entrée consommée :**
> - `docs/03_PLAN_PDSGBD.md` — architecture ERP réelle (sessions 1–12), BOM multi-niveaux, formulaires implémentés
> - `docs/JOURNAL.md` — règles apprises, historique sessions 1–12
> - `docs/05_ARTISASTOCK_INTEGRATION.md` — concepts métier ArtisaStock
> - `docs/00_CONTEXTE_PROJET.md` — vision globale
> **Selfdoubt appliqué :** Architecture BOM confirmée par lecture directe des sources. Mockup SPA-style décrit dans le prompt — aucun fichier de maquette trouvé dans le dépôt (basse certitude sur les dimensions exactes, mais les contraintes fonctionnelles sont claires).

---

## Contexte du problème

L'application actuelle ouvre un `ShowDialog()` pour chaque action utilisateur. L'artisan (Charles ou Nadejda) doit fermer chaque fenêtre modale pour naviguer ailleurs — expérience fragmentée, perte de contexte, fatigue cognitive. La refonte transforme `FrmPrincipal` en **Single-Form Application** : un seul formulaire, navigation par rail gauche, contenu qui se remplace à droite sans jamais ouvrir de fenêtre secondaire (sauf `FrmLogin`).

---

## 1. User Stories

### MODULE AUTH

---

#### US-AUTH-01 — Connexion sécurisée
**En tant qu'** artisan (Charles ou Nadejda),
**je veux** me connecter à ArtisaStock avec mon email et mon mot de passe,
**afin de** protéger les données de production de l'atelier contre tout accès non autorisé.

**Cas limites couverts :**
- Email valide mais mot de passe incorrect → message d'erreur non révélateur ("Identifiants incorrects", pas "mot de passe invalide")
- Compte désactivé (`actif=0`) → refus d'accès silencieux
- Champ email ou mot de passe vide → validation inline avant appel DB
- Rôle non-admin (client web) → accès refusé avec message clair
- Appui touche Entrée dans le champ mot de passe → déclenche la connexion

**Definition of Done :**
- [ ] `FrmLogin` est le seul formulaire qui s'affiche au démarrage
- [ ] `UtilisateurDAL.Authenticate()` utilise `BCrypt.Net.BCrypt.Verify` — jamais de comparaison en clair
- [ ] Requête paramétrée (`@email`) — aucune concaténation
- [ ] Compte avec `role != 'admin'` ne peut pas accéder à `FrmPrincipal`
- [ ] Tentative échouée : `lblErreur` visible, aucun crash, aucune info DB exposée
- [ ] Succès : `FrmLogin` se ferme, `FrmPrincipal` s'ouvre en plein écran
- [ ] `txtMotDePasse.PasswordChar = '•'` — jamais affiché en clair

---

### MODULE NAVIGATION (SPA-style)

---

#### US-NAV-01 — Hub de démarrage (HubScreen)
**En tant qu'** artisan,
**je veux** voir un tableau de bord récapitulatif dès l'ouverture de l'application,
**afin de** prendre rapidement les décisions du jour (productions à lancer, alertes stock).

**Cas limites couverts :**
- Aucune production enregistrée → StatCard "Productions 7j" affiche 0 sans erreur
- Aucun ingrédient en alerte → zone alertes affiche "Aucune alerte"
- Aucune fiche BOM configurée → StatCard "Fiches BOM" affiche 0

**Definition of Done :**
- [ ] HubScreen s'affiche à l'ouverture de `FrmPrincipal` (avant toute sélection)
- [ ] 4 StatCards : Ingrédients total / En alerte / Fiches BOM / Productions 7 derniers jours
- [ ] Tableau des dernières productions (colonnes : Fiche, Batches, Date, Coût total)
- [ ] Liste des alertes stock (Ingrédient, Stock actuel, Seuil, Activité) — cliquable → navigue vers Ingrédients
- [ ] Données chargées en un seul passage DB (pas de N+1)
- [ ] Zéro `ShowDialog()` déclenché

---

#### US-NAV-02 — Navigation rail gauche
**En tant qu'** artisan,
**je veux** naviguer dans l'application via un rail gauche structuré en sections,
**afin de** toujours savoir où je suis sans perdre mon contexte de travail.

**Cas limites couverts :**
- Aucune activité créée → section ACTIVITÉS affiche message d'onboarding "Créez votre première activité →"
- Activité sélectionnée mais aucun contexte → section CONTEXTES affiche "Aucun contexte — bouton +"
- Contexte sélectionné mais aucun niveau → section NIVEAUX affiche le Niveau 0 par défaut
- Clic sur un élément déjà sélectionné → pas de rechargement inutile

**Definition of Done :**
- [ ] Rail fixe de 260px, non redimensionnable par l'utilisateur
- [ ] Section RESSOURCES : 5 items (Vue stock global / Stocks / Ingrédients / Fournisseurs / Achats·Lots) — toujours visibles
- [ ] Section ACTIVITÉS : liste dynamique, chaque item = chip badge initiale + nom ; item actif surligné
- [ ] Section CONTEXTES : visible uniquement après sélection d'une activité ; liste filtrée par activité
- [ ] Section NIVEAUX : visible uniquement après sélection d'un contexte ; niveaux ordonnés
- [ ] Clic sur item RESSOURCES → charge le screen correspondant dans la zone droite
- [ ] Clic sur une activité → charge ContexteListScreen dans la zone droite ; filtre CONTEXTES
- [ ] Clic sur un contexte → charge ContexteNiveauxScreen dans la zone droite ; affiche NIVEAUX
- [ ] Zéro `ShowDialog()` dans le rail

---

#### US-NAV-03 — Bouton "+ Activité" dans le bandeau
**En tant qu'** artisan,
**je veux** créer une nouvelle activité depuis le bandeau supérieur sans naviguer ailleurs,
**afin de** rester dans mon flux de travail courant.

**Definition of Done :**
- [ ] Bouton "+ Activité" (couleur or `#D4AF37`) dans le TitleBar, toujours visible
- [ ] Clic → ouvre `ActivityEditScreen` inline dans la zone droite (pas `ShowDialog`)
- [ ] Après sauvegarde → rafraîchit la liste ACTIVITÉS dans le rail + sélectionne la nouvelle activité
- [ ] Annulation → retour au screen précédent sans perte de contexte

---

#### US-NAV-04 — Bouton "⚙ Activités" dans le bandeau
**En tant qu'** artisan,
**je veux** accéder à la gestion complète des activités (modifier, désactiver) depuis le bandeau,
**afin de** configurer les activités sans quitter l'application principale.

**Definition of Done :**
- [ ] Bouton "⚙ Activités" (outline) dans le TitleBar, toujours visible
- [ ] Clic → charge `ActivitesScreen` (liste CRUD) dans la zone droite
- [ ] Édition/désactivation d'une activité → se fait inline dans la zone droite
- [ ] Zéro `ShowDialog()`

---

### MODULE RESSOURCES

---

#### US-RES-01 — Vue stock global
**En tant qu'** artisan,
**je veux** voir en un coup d'œil tous les stocks disponibles (ingrédients + produits BOM),
**afin de** évaluer la capacité de production disponible.

**Cas limites couverts :**
- Lots périmés → ligne surlignée en rouge ; colonne DLC affiche la date en rouge
- Lots avec DLC dans moins de 7 jours → surlignage orange
- Stock = 0 → ligne grisée

**Definition of Done :**
- [ ] Clique sur "Vue stock global" dans le rail → charge `VueStockScreen` dans la zone droite
- [ ] Données issues de `vue_stock_global` (VIEW MySQL) via `VueStockGlobalDAL`
- [ ] Chips filtres : "Tous" + une chip par activité
- [ ] DGV colonnes : Référence / Type / Activité / Disponible / Unité / DLC / Statut
- [ ] Code couleur DLC : rouge (<0j), orange (<7j), vert (>7j), gris (sans DLC)
- [ ] Colonne triable — tri par défaut : DLC ASC (les plus urgents en premier)
- [ ] Zéro `ShowDialog()`

---

#### US-RES-02 — Gestion des stocks physiques (CRUD)
**En tant qu'** artisan,
**je veux** créer, modifier et supprimer des stocks physiques (ex: "Cave Chocolaterie", "Frigo Pâtisserie"),
**afin de** organiser mes ingrédients par espace de stockage réel.

**Cas limites couverts :**
- Suppression d'un stock contenant des ingrédients liés → refusée avec message explicite
- Nom de stock déjà existant → erreur d'unicité inline (pas de popup)
- Stock lié à une activité → impossible de supprimer sans délier d'abord

**Definition of Done :**
- [ ] Clique "Stocks" dans le rail → charge `StocksScreen` dans la zone droite
- [ ] DGV : Id / Nom / Description / Nb ingrédients liés
- [ ] Bouton "Nouveau" → `StockEditScreen` inline dans zone droite
- [ ] Bouton "Modifier" (ou double-clic DGV) → `StockEditScreen` en mode édition
- [ ] Bouton "Supprimer" → confirmation inline (pas MessageBox `ShowDialog` non plus si possible) ; guard FK `IngredientDAL.ExisteIngredientsDansStock()` avant DELETE
- [ ] `StockDAL.NomExiste(nom, excludeId)` validé avant INSERT et UPDATE
- [ ] Zéro `ShowDialog()` hors confirmation de suppression

---

#### US-RES-03 — Gestion des ingrédients (CRUD)
**En tant qu'** artisan,
**je veux** gérer mes ingrédients (créer, modifier, supprimer) avec toutes leurs propriétés (conditionnement, type physique, densité, seuil d'alerte),
**afin de** avoir une base précise pour les calculs BOM et les alertes.

**Cas limites couverts :**
- Ingrédient sous seuil d'alerte → ligne surlignée dans la liste (fond MistyRose)
- Suppression d'un ingrédient utilisé dans une fiche BOM → refusée avec liste des fiches impactées
- Nom d'ingrédient déjà existant → erreur d'unicité inline
- TypePhysique "Liquide" → affiche champ Densité ; "Solide"/"Pièce" → masque Densité
- Unité "pièce" → désactive `NudQteParConditionnement` (forcé à 1)
- Création premier ingrédient → message d'onboarding suggère de lier un stock

**Definition of Done :**
- [ ] Clique "Ingrédients" dans le rail → charge `IngredientsScreen` dans la zone droite
- [ ] Chips filtres par stock (+ "Tous")
- [ ] DGV colonnes : Nom / Type / Stock / Conditionnement / Stock actuel / Unité / Seuil / Prix réf.
- [ ] Ligne rouge (MistyRose) si `stock_actuel <= seuil_alerte`
- [ ] Bouton "Nouveau" → `IngredientEditScreen` inline
- [ ] Formulaire d'édition inline : TypePhysique, Densité (conditionnel), Conditionnement, QteParConditionnement, PrixAchatRef, SeuilAlerte, IdFournisseur (ComboBox)
- [ ] `IngredientDAL.NomExiste(nom, excludeId)` validé avant INSERT et UPDATE
- [ ] Guard FK avant DELETE : fiches BOM, lignes de production
- [ ] Zéro `ShowDialog()`

---

#### US-RES-04 — Gestion des fournisseurs (CRUD)
**En tant qu'** artisan,
**je veux** gérer mes fournisseurs (nom, contact, téléphone, email),
**afin de** retrouver rapidement les coordonnées lors des réapprovisionnements.

**Cas limites couverts :**
- Suppression d'un fournisseur lié à des ingrédients → refusée avec nb d'ingrédients impactés
- Email fournisseur invalide → validation format inline

**Definition of Done :**
- [ ] Clique "Fournisseurs" dans le rail → charge `FournisseursScreen` dans la zone droite
- [ ] DGV : Nom / Contact / Téléphone / Email / Nb ingrédients liés
- [ ] CRUD complet inline — formulaire d'édition dans zone droite
- [ ] Guard FK avant DELETE
- [ ] Zéro `ShowDialog()`

---

#### US-RES-05 — Gestion des achats / lots (CRUD)
**En tant qu'** artisan,
**je veux** enregistrer mes achats de matières premières (lot = ingrédient + quantité + prix + DLC),
**afin de** maintenir un stock précis et calculer les coûts de production au prix réel.

**Cas limites couverts :**
- Achat en HTVA → calcul automatique du prix TVAC affiché (preview live)
- Achat en TVAC → calcul automatique du prix HTVA affiché
- `NbConditionnements × QteParConditionnement` → preview "= X g en stock" mis à jour en temps réel
- DLC passée → warning visible avant enregistrement
- Suppression d'un lot utilisé dans une `bom_production_ligne` → refusée

**Definition of Done :**
- [ ] Clique "Achats · Lots" dans le rail → charge `AchatsScreen` dans la zone droite
- [ ] Chips filtres par stock + activité
- [ ] DGV : Ingrédient / Fournisseur / Date achat / NbConditionnements / Qte totale / Prix HTVA / Prix TVAC / DLC / Statut
- [ ] Formulaire d'édition inline : radio HTVA/TVAC, NudNbConditionnements, preview "= Xg en stock", DateTimePicker DLC, ComboBox ingrédient
- [ ] Guard FK avant DELETE
- [ ] Zéro `ShowDialog()`

---

### MODULE ACTIVITÉS

---

#### US-ACT-01 — Créer une activité
**En tant qu'** artisan,
**je veux** créer une activité nommée (ex: "Chocolaterie", "Pâtisserie", "Glacier"),
**afin de** segmenter ma production par domaine et éviter les confusions de stock.

**Cas limites couverts :**
- **Première activité** → message d'onboarding ("Aucune activité — commencez ici") visible dans le rail et dans la zone droite avant création
- Nom d'activité déjà existant → erreur d'unicité inline
- Description optionnelle → ne bloque pas la sauvegarde

**Definition of Done :**
- [ ] Clic "+ Activité" (bandeau) OU "Nouveau" (ActivitesScreen) → `ActivityEditScreen` inline
- [ ] Champs : Nom (obligatoire), Description (optionnelle)
- [ ] `ActiviteDAL.NomExiste(nom)` validé avant INSERT
- [ ] Après sauvegarde : activité apparaît dans le rail gauche (section ACTIVITÉS) sans rechargement complet
- [ ] Zéro `ShowDialog()`

---

#### US-ACT-02 — Modifier une activité
**En tant qu'** artisan,
**je veux** renommer ou modifier la description d'une activité existante,
**afin de** corriger une erreur de saisie ou refléter un changement organisationnel.

**Cas limites couverts :**
- Nouveau nom identique à une autre activité → erreur d'unicité (`NomExiste(nom, excludeId)`)
- Modification du nom → rafraîchit immédiatement le rail gauche

**Definition of Done :**
- [ ] Double-clic sur activité dans le rail OU bouton "Modifier" dans ActivitesScreen → `ActivityEditScreen` en mode édition
- [ ] `ActiviteDAL.NomExiste(nom, excludeId)` validé avant UPDATE
- [ ] Rail gauche rafraîchi après sauvegarde
- [ ] Zéro `ShowDialog()`

---

#### US-ACT-03 — Désactiver une activité
**En tant qu'** artisan,
**je veux** désactiver une activité sans la supprimer définitivement,
**afin de** conserver l'historique de production lié à cette activité.

**Cas limites couverts :**
- Activité avec contextes actifs → warning "X contextes actifs seront suspendus" avant confirmation
- Activité désactivée → disparaît du rail gauche ; reste visible dans l'historique productions
- Réactivation possible depuis ActivitesScreen (toggle)

**Definition of Done :**
- [ ] Bouton "Désactiver" dans ActivitesScreen → confirmation inline (label dans zone droite, pas MessageBox)
- [ ] `ActiviteDAL.Desactiver()` — soft delete (`actif=0`), jamais DELETE physique
- [ ] Activité `actif=0` exclue des requêtes `GetAll()` (rail gauche, ComboBox contextes)
- [ ] Zéro `ShowDialog()`

---

#### US-ACT-04 — Lier des stocks à une activité
**En tant qu'** artisan,
**je veux** associer un ou plusieurs stocks physiques à une activité,
**afin que** les ingrédients de cette activité soient rangés dans les bons espaces de stockage.

**Cas limites couverts :**
- Aucun stock créé → message "Créez d'abord un stock physique" avec lien de navigation
- Délier un stock contenant des ingrédients utilisés par des fiches BOM actives → warning

**Definition of Done :**
- [ ] Bouton "📦 Stocks liés" dans ActivitesScreen (ou inline) → charge `ActiviteStocksScreen` dans zone droite
- [ ] CheckedListBox de tous les stocks disponibles ; cochés = liés
- [ ] Sauvegarde → `StockDAL.LierActivite / DelierActivite`
- [ ] Zéro `ShowDialog()`

---

### MODULE BOM — CONFIGURATION

---

#### US-BOM-CTX-01 — Créer un contexte de production
**En tant qu'** artisan,
**je veux** créer un contexte de production nommé au sein d'une activité (ex: "Pralines Noël 2026", "Ganaches classiques"),
**afin de** regrouper les niveaux, fiches et stocks d'une campagne de production spécifique.

**Cas limites couverts :**
- **Première activité sans contexte** → section CONTEXTES affiche "Aucun contexte — [+ Créer]"
- Nom de contexte déjà existant pour cette activité → erreur d'unicité inline
- Contexte créé → niveau par défaut (N0 = stock ingrédients) automatiquement créé en transaction atomique

**Definition of Done :**
- [ ] Sélection activité dans le rail → bouton "+ Contexte" visible dans ContexteListScreen
- [ ] Clic → `ContexteEditScreen` inline dans zone droite
- [ ] Champs : Nom (obligatoire), Activité (lecture seule, déduit du contexte), Description
- [ ] `BomContexteDAL.NomExiste(nom, idActivite)` validé avant INSERT
- [ ] `BomContexteDAL.Insert()` crée le contexte ET le niveau 0 dans une `MySqlTransaction`
- [ ] Après sauvegarde : contexte apparaît dans le rail section CONTEXTES
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-CTX-02 — Modifier un contexte
**En tant qu'** artisan,
**je veux** renommer ou modifier la description d'un contexte existant,
**afin de** corriger une erreur de saisie ou adapter la campagne.

**Definition of Done :**
- [ ] Double-clic sur contexte dans le rail OU bouton "Modifier" dans ContexteListScreen → `ContexteEditScreen` en mode édition
- [ ] `BomContexteDAL.NomExiste(nom, idActivite, excludeId)` validé avant UPDATE
- [ ] Rail section CONTEXTES rafraîchi après sauvegarde
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-CTX-03 — Supprimer un contexte
**En tant qu'** artisan,
**je veux** supprimer un contexte de production qui n'est plus utilisé,
**afin de** garder le rail propre.

**Cas limites couverts :**
- Contexte avec des productions associées → refus avec message "X productions enregistrées — suppression impossible"
- Contexte avec niveaux > N0 → tous les niveaux, fiches et stocks associés doivent être listés dans le message de confirmation

**Definition of Done :**
- [ ] Bouton "✕" dans ContexteNiveauxScreen → confirmation inline avec liste des dépendances
- [ ] `BomContexteDAL.Delete()` : guard productions ; en l'absence, cascade DELETE niveaux → fiches → bom_stocks en transaction
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-NIV-01 — Ajouter un niveau de production
**En tant qu'** artisan,
**je veux** ajouter un nouveau niveau de production à un contexte (ex: N1 = Préparations de base, N2 = Assemblages finaux),
**afin de** structurer la production en étapes hiérarchiques.

**Cas limites couverts :**
- Niveau N0 (stock ingrédients) → toujours présent, jamais modifiable depuis l'UI
- Nom de niveau déjà existant dans ce contexte → erreur d'unicité inline
- Ordre du nouveau niveau = `MAX(ordre) + 1` — jamais saisi manuellement

**Definition of Done :**
- [ ] Bouton "+ Niveau" dans ContexteNiveauxScreen → `NiveauEditScreen` inline
- [ ] Champs : Nom (obligatoire), Description ; Ordre affiché en lecture seule
- [ ] `BomNiveauDAL.Insert()` avec `ordre = GetOrdreMax(idContexte) + 1`
- [ ] `BomNiveauDAL.NomExiste(nom, idContexte)` validé avant INSERT
- [ ] Après sauvegarde : niveau apparaît dans ContexteNiveauxScreen ET dans le rail section NIVEAUX
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-NIV-02 — Modifier un niveau
**En tant qu'** artisan,
**je veux** renommer un niveau de production,
**afin de** clarifier son rôle dans la nomenclature.

**Cas limites couverts :**
- Tentative de modifier N0 → bouton "Modifier" désactivé sur la ligne N0

**Definition of Done :**
- [ ] Bouton "Modifier" dans ContexteNiveauxScreen → `NiveauEditScreen` en mode édition
- [ ] Champ Ordre en lecture seule — non modifiable
- [ ] `BomNiveauDAL.NomExiste(nom, idContexte, excludeId)` validé
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-NIV-03 — Supprimer un niveau
**En tant qu'** artisan,
**je veux** supprimer le dernier niveau d'un contexte,
**afin de** simplifier la structure quand une étape devient inutile.

**Cas limites couverts :**
- **Niveau N0** → bouton "✕" absent / désactivé ; suppression techniquement impossible (guard DB)
- **Niveau intermédiaire** (pas `MAX(ordre)`) → refus avec message "Supprimez d'abord le niveau Nx"
- Niveau avec fiches liées → confirmation avec liste des fiches ; DELETE en cascade

**Definition of Done :**
- [ ] Bouton "✕" visible uniquement sur la ligne de niveau `MAX(ordre)` (jamais sur N0)
- [ ] `BomNiveauDAL.Delete()` lève `InvalidOperationException` si le niveau n'est pas top-level
- [ ] Message d'erreur inline (zone droite) — pas de MessageBox modal
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-FICHE-01 — Créer une fiche BOM (recette de production)
**En tant qu'** artisan,
**je veux** créer une fiche BOM (recette) en listant les inputs (ingrédients ou fiches d'un niveau inférieur) et la quantité produite,
**afin de** définir précisément la composition d'un produit fabriqué.

**Cas limites couverts :**
- Aucun ingrédient créé → liste d'inputs "Ingrédient" est vide ; message "Ajoutez d'abord des ingrédients"
- Aucune fiche au niveau N-1 → option "Fiche (N-1)" grisée si aucune fiche disponible
- Ajout d'une ligne avec unité incompatible avec l'ingrédient → erreur inline (ex: ml sur un ingrédient en g)
- Unité verrouillée sur l'input source après sélection (ComboBox `Enabled=false`)
- Input de type "pièce" → `NudQteLigne` forcé à 1 et désactivé
- Coût estimé recalculé en temps réel à chaque modification de ligne
- Fiche sans lignes → sauvegarde refusée ("Une fiche doit avoir au moins une ligne")
- `QuantiteOutput` = 0 → refus ("Le rendement ne peut être nul")

**Definition of Done :**
- [ ] Clique "Fiches" dans ContexteNiveauxScreen → charge `FichesListScreen` dans zone droite
- [ ] Bouton "Nouvelle fiche" → `FicheEditScreen` inline
- [ ] Sélection niveau dans ComboBox (filtrés par contexte) ; Nom fiche (obligatoire)
- [ ] `BomFicheDAL.NomExiste(nom, idNiveau)` validé avant INSERT
- [ ] Tableau d'édition des lignes : ComboBox TypeInput / ComboBox Input / Nud Quantité / ComboBox Unité (verrouillée) / SousTotal
- [ ] Ajout/suppression de lignes dynamique (bouton + / bouton ✕ par ligne)
- [ ] Labels en bas : CoutBatch / CoutUnitaire (recalcul via `BomCoutDAL` ou calcul local)
- [ ] `BomFicheDAL.Insert()` en transaction (header + lignes)
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-FICHE-02 — Modifier une fiche BOM
**En tant qu'** artisan,
**je veux** modifier une fiche BOM existante (ajouter/supprimer/modifier des lignes),
**afin de** corriger une recette sans perdre l'historique de production.

**Cas limites couverts :**
- Fiche ayant déjà des productions → modification autorisée mais warning "X productions utilisent cette version"
- Suppression d'une ligne dont l'ingrédient est en rupture → pas de blocage (c'est une modification de recette, pas une production)

**Definition of Done :**
- [ ] Double-clic dans FichesListScreen OU bouton "Modifier" → `FicheEditScreen` en mode édition
- [ ] `BomFicheDAL.Update()` en transaction (DELETE lignes existantes + INSERT nouvelles lignes)
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-FICHE-03 — Supprimer une fiche BOM
**En tant qu'** artisan,
**je veux** supprimer une fiche BOM obsolète,
**afin de** garder la liste des recettes propre.

**Cas limites couverts :**
- Fiche utilisée dans des productions passées → refus avec nb de productions impactées
- Fiche utilisée comme input dans d'autres fiches (niveau N+1) → refus avec liste des fiches dépendantes

**Definition of Done :**
- [ ] Bouton "Supprimer" → confirmation inline avec liste des dépendances
- [ ] Guard FK : `bom_productions` et `bom_fiches_lignes` (TypeInput = "fiche")
- [ ] Zéro `ShowDialog()`

---

### MODULE BOM — PRODUCTION & SIMULATION

---

#### US-BOM-PROD-01 — Simuler une production
**En tant qu'** artisan,
**je veux** simuler une production (vérifier la disponibilité des ingrédients) avant de la lancer,
**afin de** ne pas me retrouver en rupture au milieu d'une fabrication.

**Cas limites couverts :**
- Aucune fiche dans le niveau sélectionné → ComboBox fiche vide ; message explicite
- **Pénurie partielle** : certains ingrédients ok, d'autres en rupture → grille mixte vert/rouge ; bouton "Lancer" désactivé
- **Pénurie totale** : tout rouge → bouton "Lancer" désactivé ; message "Approvisionnement requis"
- Conversion d'unités : fiche en g, stock en kg → affiché de façon cohérente après conversion
- `NbBatches = 0` → validation empêche la simulation
- Fiche dont un input est une fiche N-1 non encore produite → pénurie détectée (bom_stocks vide pour ce niveau)

**Definition of Done :**
- [ ] Clic "Simuler" dans ContexteNiveauxScreen → charge `SimulationScreen` inline OU section simulation dans la zone droite
- [ ] Sélecteurs : Contexte (prérempli depuis rail) / Niveau / Fiche / NbBatches
- [ ] Label "1 batch = X pièces" mis à jour lors du choix de fiche
- [ ] Bouton "Simuler" → appelle `BomProductionDAL.VerifierDisponibilite()` hors transaction
- [ ] DGV résultat : Ingrédient/Source / En stock (converti) / Nécessaire (converti) / Statut (✅ / ❌)
- [ ] Label "Coût estimé : XX,XX € — X,XX €/pièce" via `BomCoutDAL.CalculerCout()`
- [ ] `btnLancerProduction.Enabled = (pénuries.Count == 0)`
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-PROD-02 — Lancer une production
**En tant qu'** artisan,
**je veux** lancer la production après simulation réussie,
**afin que** le stock des ingrédients soit automatiquement décrémenté et que le stock du niveau N soit créé.

**Cas limites couverts :**
- Rupture de stock détectée entre la simulation et l'exécution (concurrent rare) → transaction annulée, message d'erreur détaillé
- **Règle FIFO** : lots d'ingrédients consommés dans l'ordre `date_production ASC`
- `quantiteCible` = nombre de batches (pas quantité finale) → `qteProduite = batches × QuantiteOutput`
- Réservations actives (`bom_reservations`) déduites de la disponibilité réelle avant consommation
- Coût total calculé et inscrit dans `bom_productions` après exécution

**Definition of Done :**
- [ ] Bouton "✅ Lancer la production" activé uniquement après simulation sans pénurie
- [ ] Appelle `BomProductionDAL.Executer()` dans une `MySqlTransaction`
- [ ] Workflow transaction atomique :
  1. INSERT `bom_productions` (cout_ingredients = 0 provisoire)
  2. Pour chaque ligne fiche : ConvertirUnités → ConsumeStock FIFO (UPDATE lots/bom_stocks + INSERT bom_productions_lignes)
  3. UPDATE `bom_productions` SET cout_ingredients, cout_unitaire
  4. INSERT `bom_stocks` (lot produit niveau N, avec id_contexte + id_activite)
  5. COMMIT
- [ ] En cas d'exception : ROLLBACK complet, aucune modification en DB
- [ ] Après exécution : HubScreen StatCard "Productions 7j" incrémentée ; `VueStockScreen` reflète les nouvelles quantités
- [ ] `WaitCursor` pendant l'exécution
- [ ] Zéro `ShowDialog()`

---

#### US-BOM-PROD-03 — Consulter l'historique des productions
**En tant qu'** artisan,
**je veux** consulter l'historique de toutes les productions passées avec leurs coûts,
**afin de** analyser mes performances et coûts de revient dans le temps.

**Cas limites couverts :**
- Aucune production → DGV vide avec message "Aucune production enregistrée"
- Production avec prix_vente_reel renseigné → calcul marge_brute affiché
- Filtre par activité ou par période

**Definition of Done :**
- [ ] Accessible depuis HubScreen (tableau productions) ET depuis ContexteNiveauxScreen
- [ ] DGV colonnes : Date / Fiche / Contexte / Niveau / Nb batches / Qte produite / Coût total / Coût unitaire / Marge (si renseigné)
- [ ] Filtres : activité (ComboBox) + période (DateTimePicker début/fin)
- [ ] Zéro `ShowDialog()`

---

### MODULE CATALOGUE WEB (placeholder → inline)

---

#### US-CAT-01 — Gérer les catégories produits
**En tant qu'** artisan,
**je veux** créer, modifier et supprimer les catégories du catalogue web (Ballotins, Pâtisseries…),
**afin que** les produits soient organisés correctement sur la boutique en ligne.

**Cas limites couverts :**
- Suppression d'une catégorie avec des produits liés → refus avec nb de produits impactés
- Nom de catégorie déjà existant → erreur d'unicité inline

**Definition of Done :**
- [ ] Accessible depuis le menu "Catalogue web" dans le bandeau (débloquer le placeholder)
- [ ] `CategoriesScreen` + `CategorieEditScreen` inline (zone droite) — jamais `ShowDialog()`
- [ ] `CategorieDAL.NomExiste(nom, excludeId)` ; guard FK avant DELETE
- [ ] Zéro `ShowDialog()`

---

#### US-CAT-02 — Gérer les parfums
**En tant qu'** artisan,
**je veux** gérer les parfums disponibles (Praliné, Caramel, Noir 70%…) avec leur couleur d'affichage,
**afin que** le configurateur de ballotins sur le site web soit à jour.

**Cas limites couverts :**
- Suppression d'un parfum utilisé dans des commandes existantes → refusée

**Definition of Done :**
- [ ] `ParfumsScreen` + `ParfumEditScreen` inline
- [ ] Champs : Nom, Type (ComboBox), Couleur (color picker ou texte hex), Preview couleur live
- [ ] Zéro `ShowDialog()`

---

#### US-CAT-03 — Gérer les produits du catalogue
**En tant qu'** artisan,
**je veux** créer et modifier les produits du catalogue web (nom, prix, disponibilité, parfums, image),
**afin de** contrôler ce qui s'affiche sur la boutique en ligne.

**Cas limites couverts :**
- `configurable = true` → CheckedListBox parfums visible ; `configurable = false` → masqué
- Désactiver un produit (`disponible=0`) → disparaît de la boutique sans suppression
- Image Cloudinary : sélection fichier local → upload async → URL stockée en DB ; bouton désactivé pendant l'upload

**Definition of Done :**
- [ ] `ProduitsScreen` + `ProduitEditScreen` inline
- [ ] DGV avec thumbnail image (PictureBox dans cellule)
- [ ] Filtre par catégorie (ComboBox)
- [ ] CheckedListBox parfums conditionnelle selon `configurable`
- [ ] `CloudinaryHelper.UploadImageAsync()` depuis `ProduitEditScreen` — `WaitCursor` pendant l'upload
- [ ] Toggle disponible/indisponible depuis la liste (bouton ou checkbox dans DGV)
- [ ] Zéro `ShowDialog()`

---

### MODULE COMMANDES WEB (lecture)

---

#### US-CMD-01 — Consulter les commandes en ligne
**En tant qu'** artisan,
**je veux** consulter les commandes passées sur le site web depuis l'application C#,
**afin de** préparer les commandes sans ouvrir un navigateur.

**Cas limites couverts :**
- Aucune commande → message "Aucune commande" (pas d'erreur DB)
- Connexion DB perdue → message d'erreur explicite avec bouton "Réessayer"

**Definition of Done :**
- [ ] `CommandesScreen` accessible depuis menu "Commandes web" dans le bandeau
- [ ] DGV : Numéro / Date / Client / Produits / Total TTC / Statut / Date souhaitée
- [ ] Filtre par statut (ComboBox : Toutes / En attente / Confirmées / En préparation / Prêtes / Livrées)
- [ ] Double-clic → `CommandeDetailScreen` inline : lignes de commande, parfums sélectionnés, coordonnées client, notes internes
- [ ] Changement de statut inline (pas de navigation vers le site)
- [ ] Zéro `ShowDialog()`

---

## 2. Mapping Écrans → Screens (Ancienne Form → Nouveau Screen inline)

| Ancienne Form (ShowDialog) | Nouveau Screen inline | Déclencheur de navigation |
|---|---|---|
| `FrmActivites` | `ActivitesScreen` (zone droite) | Bouton "⚙ Activités" dans le bandeau |
| `FrmActiviteEdit` (create) | `ActivityEditScreen` inline | Bouton "+ Activité" dans le bandeau OU "Nouveau" dans ActivitesScreen |
| `FrmActiviteEdit` (edit) | `ActivityEditScreen` inline | Double-clic dans ActivitesScreen OU double-clic dans le rail |
| `FrmActiviteStocks` | `ActiviteStocksScreen` inline | Bouton "📦 Stocks liés" dans ActivitesScreen |
| `FrmStocks` | `StocksScreen` (zone droite) | Clic "Stocks" dans le rail |
| `FrmStockEdit` | `StockEditScreen` inline | "Nouveau" ou double-clic dans StocksScreen |
| `FrmIngredients` | `IngredientsScreen` (zone droite) | Clic "Ingrédients" dans le rail |
| `FrmIngredientEdit` | `IngredientEditScreen` inline | "Nouveau" ou double-clic dans IngredientsScreen |
| `FrmAchats` | `AchatsScreen` (zone droite) | Clic "Achats · Lots" dans le rail |
| `FrmAchatEdit` | `AchatEditScreen` inline | "Nouveau" ou double-clic dans AchatsScreen |
| `FrmFournisseurs` | `FournisseursScreen` (zone droite) | Clic "Fournisseurs" dans le rail |
| `FrmFournisseurEdit` | `FournisseurEditScreen` inline | "Nouveau" ou double-clic dans FournisseursScreen |
| `FrmVueStock` | `VueStockScreen` (zone droite) | Clic "Vue stock global" dans le rail |
| `FrmBomContextes` | `ContexteListScreen` (zone droite) | Clic sur une Activité dans le rail |
| `FrmBomContexteEdit` | `ContexteEditScreen` inline | Bouton "+ Contexte" dans ContexteListScreen OU double-clic |
| `FrmBomNiveaux` | Section niveaux dans `ContexteNiveauxScreen` | Clic sur un Contexte dans le rail |
| `FrmBomNiveauEdit` | `NiveauEditScreen` inline | Bouton "+ Niveau" ou double-clic dans ContexteNiveauxScreen |
| `FrmBomFiches` | `FichesListScreen` (bas de ContexteNiveauxScreen ou zone droite) | Bouton "Fiches" sur la ligne du niveau concerné |
| `FrmBomFicheEdit` | `FicheEditScreen` inline | Bouton "Nouvelle fiche" ou double-clic dans FichesListScreen |
| `FrmBomProductionSimulation` | `SimulationProductionScreen` inline | Bouton "Simuler" ou "Produire" sur la ligne du niveau |
| `FrmCategories` | `CategoriesScreen` (zone droite) | Menu "Catalogue web → Catégories" dans le bandeau |
| `FrmCategorieEdit` | `CategorieEditScreen` inline | "Nouveau" ou double-clic dans CategoriesScreen |
| `FrmParfums` | `ParfumsScreen` (zone droite) | Menu "Catalogue web → Parfums" |
| `FrmParfumEdit` | `ParfumEditScreen` inline | "Nouveau" ou double-clic dans ParfumsScreen |
| `FrmProduits` | `ProduitsScreen` (zone droite) | Menu "Catalogue web → Produits" |
| `FrmProduitEdit` | `ProduitEditScreen` inline | "Nouveau" ou double-clic dans ProduitsScreen |
| *(nouveau)* | `HubScreen` | Démarrage de l'application (avant toute sélection dans le rail) |
| *(nouveau)* | `CommandesScreen` | Menu "Commandes web" dans le bandeau |
| *(nouveau)* | `CommandeDetailScreen` inline | Double-clic dans CommandesScreen |

> **Règle de chargement :** chaque Screen est un `UserControl` chargé dynamiquement dans un `Panel` central (`pnlContent`). Le `Panel` vide ses contrôles (`pnlContent.Controls.Clear()`) puis ajoute le nouveau `UserControl` avec `DockStyle.Fill`. Aucun `ShowDialog()` ni `Show()` secondaire.

---

## 3. Règles UX non-négociables

### R-UX-01 — Aucun ShowDialog dans FrmPrincipal (sauf FrmLogin)
- `FrmLogin` est la seule exception autorisée — c'est le point d'entrée de l'application.
- Toute tentative d'utiliser `ShowDialog()` dans `FrmPrincipal` ou dans un `UserControl` est un bug bloquant.
- Les confirmations de suppression utilisent un `Panel` de confirmation inline dans la zone droite (pas `MessageBox.Show()`).
- Les erreurs de validation s'affichent via `ErrorProvider` ou `lblErreur` inline — pas de `MessageBox`.

### R-UX-02 — Toute édition se fait dans la zone droite (content area)
- Créer, modifier, supprimer : toujours dans `pnlContent` (`DockStyle.Fill`).
- Un formulaire d'édition (`EditScreen`) remplace temporairement le screen liste correspondant.
- Bouton "Annuler" → retour au screen liste sans rechargement complet (conserver le scroll position si possible).
- Bouton "Enregistrer" → sauvegarde + retour screen liste + rafraîchissement DGV.

### R-UX-03 — La navigation rail gauche ne provoque jamais de popup
- Clic sur un item du rail → `LoadScreen(userControl)` dans `pnlContent`. Point.
- Si un formulaire d'édition est en cours avec des modifications non sauvegardées → warning inline dans la zone droite ("Modifications non sauvegardées — Continuer ?") avant de naviguer. Ce warning est un `Label` + 2 `Button` dans un `Panel` superposé, pas un `MessageBox`.

### R-UX-04 — Workflow BOM complet sans quitter FrmPrincipal
- L'utilisateur doit pouvoir enchaîner : Créer Activité → Créer Contexte → Ajouter Niveau → Créer Fiche → Simuler → Produire sans jamais fermer `FrmPrincipal`.
- Chaque étape navigue naturellement vers la suivante (après sauvegarde contexte → sélectionne auto le contexte dans le rail → charge ContexteNiveauxScreen).
- Bouton "Produire" sur une ligne de niveau → pré-sélectionne ce niveau dans `SimulationProductionScreen`.

### R-UX-05 — Feedback visuel immédiat
- Toute opération DB longue (> 200ms estimée) → `WaitCursor` pendant l'exécution.
- Après `Executer()` BOM production → message de succès inline ("✅ Production lancée — X batches / Y pièces / Z,ZZ € de coût") visible 3 secondes puis disparaît.
- Après sauvegarde → DGV rafraîchi sans clignotement (réappliquer la sélection précédente si l'entité modifiée est toujours présente).

### R-UX-06 — Proportions et palette (obligatoires)
- Rail gauche : 260px fixe.
- Palette : CHOCOLAT_FONCE `#3D2817`, CREME `#F5E6D3`, OR `#D4AF37`, fond blanc `#FFFFFF`.
- TitleBar : dégradé `#3D2817` → `#6F4E37`.
- Boutons primaires (Enregistrer, + Activité, Lancer production) : `CHOCOLAT_FONCE` fond + blanc texte.
- Boutons outline (⚙ Activités) : fond blanc, bordure `CHOCOLAT_FONCE`, texte `CHOCOLAT_FONCE`.
- Niveau N0 : badge distinct (fond gris neutre) + non supprimable.
- Niveaux N1+ : bordure gauche colorée selon l'ordre.

---

## 4. Alertes sécurité / contraintes techniques pour l'Architect

### SEC-01 — Requêtes paramétrées obligatoires (OWASP A03:2021 Injection)
Tous les DAL existants utilisent `AddWithValue` — à vérifier sur les nouveaux écrans inline. Toute valeur utilisateur passée dans une requête SQL doit être paramétrée. **Jamais de concaténation de chaîne dans un DAL.**

### SEC-02 — Transactions atomiques sur opérations multi-tables
Les opérations suivantes doivent être dans une `MySqlTransaction` :
- `BomContexteDAL.Insert()` (contexte + niveau N0)
- `BomFicheDAL.Insert()` / `Update()` (header + lignes)
- `BomProductionDAL.Executer()` (production + consommations FIFO + bom_stocks)
- `BomContexteDAL.Delete()` (cascade niveaux → fiches → bom_stocks)
Si une de ces transactions n'est pas atomique, la base peut se retrouver dans un état incohérent.

### SEC-03 — Guard FK avant suppression (OWASP A04:2021 Insecure Design)
Chaque DELETE doit être précédé d'un SELECT COUNT de toutes les FK descendantes. L'UI ne doit jamais supposer qu'une entité peut être supprimée sans vérification. Ordre de vérification pour une Fiche BOM :
1. Utilisée dans `bom_productions` ?
2. Utilisée comme input dans `bom_fiches_lignes` (TypeInput = "fiche") ?

### SEC-04 — Conversion d'unités avant toute comparaison (bug métier critique)
`UnitConvertisseur.Convertir()` doit être appelé **avant** toute comparaison de quantités entre stock disponible et quantité nécessaire. Oublier cette conversion génère des pénuries fantômes (ex: stock = 2 kg vs besoin = 1500 g → fausse pénurie si non converti). Règle déjà apprise (#16 dans JOURNAL.md) — à documenter comme contrainte non-négociable dans tous les DAL de production.

### SEC-05 — Soft delete uniquement pour les Activités (cohérence historique)
Les activités ne doivent jamais être supprimées physiquement (elles sont référencées dans `bom_productions` et `bom_stocks`). Seul le champ `actif=0` est autorisé. Tout DELETE physique sur `activites` est interdit côté applicatif.

### SEC-06 — FIFO strict sur la consommation des lots
La consommation de stock doit toujours respecter l'ordre `date_production ASC` (ou `date_achat ASC` pour les lots ingrédients). Si cette règle est brisée, les coûts de revient seront incorrects et la traçabilité perdue. Contrainte à vérifier dans `BomProductionDAL` à chaque modification.

### SEC-07 — Validation des entrées utilisateur côté C# avant insertion DB
- Quantités : `decimal > 0` obligatoire (pas de valeur négative ou nulle)
- Noms : trim + longueur max (selon colonnes VARCHAR en DB)
- Emails (fournisseurs) : validation format regex basique avant INSERT
- Prix : `decimal >= 0` ; jamais de valeur négative en base

### SEC-08 — Gestion des exceptions DB non-silencieuse
Aucune exception DB ne doit être avalée silencieusement (`catch { }` vide interdit). Toute exception DB doit être :
1. Loggée dans un fichier texte local (pas de données sensibles dans le log)
2. Affichée à l'utilisateur via `lblErreur` ou un `Panel` d'erreur inline (message user-friendly, pas le stack trace)

### SEC-09 — Données de connexion non exposées dans l'UI
La `ConnectionString` de `App.config` ne doit jamais être affichée dans l'UI ni loggée. Le mot de passe MySQL (`Pwd=root`) doit rester dans `App.config` uniquement (déjà respecté — à maintenir).

### TECH-01 — Architecture UserControl : cycle de vie et rechargement
Les `UserControl` sont instanciés et détruits à chaque navigation (`Controls.Clear()` + `Dispose()`). Si un `UserControl` démarre des timers ou des connexions async, il doit implémenter `IDisposable` et nettoyer ses ressources dans `Dispose()` — sinon risque de memory leak ou d'appels DB après fermeture.

### TECH-02 — Rafraîchissement rail gauche sans rechargement complet
Quand une activité ou un contexte est créé/modifié, seule la liste concernée du rail doit être rechargée (pas reconstruire tout `FrmPrincipal`). L'Architect doit prévoir un mécanisme d'événements ou de callbacks (`Action onDataChanged`) entre les `UserControl` et `FrmPrincipal`.

### TECH-03 — Compatibilité .NET Framework 4.8 — pas d'async/await dans les DAL
La stack actuelle utilise `.NET Framework 4.8.1` avec `MySql.Data`. Les opérations DB sont synchrones. Pas d'`async/await` dans les DAL. Le `WaitCursor` doit être appliqué avant et après l'appel synchrone. Si une opération longue doit être asynchrone, utiliser `BackgroundWorker` (disponible en WinForms 4.8).

### TECH-04 — UserControl "FrmBomFicheEdit" : complexité maximale
Ce screen est le plus complexe de l'application (cascade ComboBox, unités verrouillées, recalcul temps réel, ajout/suppression dynamique de lignes, distinction TypeInput Ingrédient/Fiche). L'Architect doit prévoir un test de non-régression après la migration en inline — notamment la règle unité verrouillée (#7 JOURNAL.md) et la règle paramètres nommés (#13 JOURNAL.md).

---

## Journal Agent #1

```markdown
## [Agent #1 PO] — 2026-04-19 (session courante)
**Entrée consommée :**
  - docs/03_PLAN_PDSGBD.md (architecture complète sessions 1-12, formulaires, DAL, BOM)
  - docs/JOURNAL.md (17 règles apprises, historique sessions 1-12)
  - docs/05_ARTISASTOCK_INTEGRATION.md (concepts métier, stack technique)
  - docs/00_CONTEXTE_PROJET.md (vision globale)
  - docs/08_PLAN_IMPLEMENTATION.md (phases d'implémentation)

**Output produit :**
  - 27 User Stories couvrant Auth, Navigation SPA, Ressources (Stocks/Ingrédients/Fournisseurs/Achats),
    Activités (CRUD + soft delete + liaison stocks), BOM Contextes/Niveaux/Fiches/Simulation/Production,
    Catalogue web (Catégories/Parfums/Produits), Commandes web
  - Mapping complet 27 anciennes Forms → nouveaux Screens inline
  - 6 règles UX non-négociables
  - 9 alertes sécurité + 4 contraintes techniques pour l'Architect

**Décisions clés :**
  - UserControl chargé dans pnlContent (DockStyle.Fill) = pattern de migration sans réécriture complète
  - Confirmations de suppression : Panel inline (pas MessageBox) — règle UX stricte
  - N0 non supprimable = cas limite documenté explicitement dans US-BOM-NIV-03
  - Soft delete activités = contrainte SEC-05 (historique productions)

**Selfdoubt appliqué :**
  - Architecture UserControl : ✅ Certain (pattern WinForms standard, confirmé par contrainte TECH-01)
  - Dimensions rail 260px : ⚠️ Probable (décrit dans le mockup du prompt, pas trouvé en fichier)
  - Couleurs exactes TitleBar dégradé : ✅ Certain (docs/00_CONTEXTE_PROJET.md + mockup prompt)
  - Formulaires déjà implémentés (sessions 1-12) : ✅ Certain (docs/03_PLAN_PDSGBD.md checklist)

**Alerte Agent suivant (Architect #2) :**
  - TECH-02 (mécanisme refresh rail) est le point d'architecture le plus critique
  - TECH-04 (FicheEdit complexité) nécessite un plan de migration prudent
  - TECH-01 (IDisposable sur UserControl) doit être standard dès le UserControlBase
  - Le DAL BomProductionDAL.Executer() existant est validé — ne pas réécrire, adapter l'appel
```
