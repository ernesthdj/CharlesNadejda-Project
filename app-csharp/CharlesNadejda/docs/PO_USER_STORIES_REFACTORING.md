# PO — User Stories Refactoring ArtisaStock
**Version :** 1.0  
**Date :** 2026-04-19  
**Auteur :** Agent #1 PO  
**Source d'entrée :** `PO_AUDIT_FONCTIONNALITES_DESIGN.md` (Agent #3 UI/UX) + lecture directe DAL + codebase  
**Destinataires :** Agent #4 Backend · Agent #5 Frontend (WinForms) · Agent #6 QA

---

## Contexte & Périmètre

Ce document traduit les gaps identifiés dans l'audit design (Agent #3) en User Stories
actionnables, priorisées par valeur métier. Chaque story est associée à son gap d'origine,
à ses critères d'acceptance (DoD — Definition of Done) et à ses contraintes techniques.

**Architecture cible :** Single-Form Application (SFA) — ScreenRouter + AppState.
**Alerte sécurité active :** R8 — mot de passe debug en dur dans `FrmLogin.cs` ligne 17.
À corriger en priorité absolue avant tout autre développement.

---

## Tableau de bord Priorités

| Priorité | Stories | Effort estimé |
|----------|---------|---------------|
| 🔴 BLOQUANT (P0) | US-00, US-01, US-02 | Sprint 0 — avant tout commit |
| 🟠 IMPORTANT (P1) | US-03, US-04, US-05 | Sprint 1 |
| 🟡 MOYEN (P2) | US-06, US-07, US-08, US-09 | Sprint 2 |
| 🟢 FAIBLE (P3) | US-10, US-11, US-12 | Sprint 3 |

---

## P0 — BLOQUANT (à résoudre avant le premier commit)

---

### US-00 — Sécurité : Supprimer le mot de passe debug en dur
**Gap :** R8 (alerte sécurité MEMORY.md)  
**Référence :** `FrmLogin.cs`, ligne 17

**En tant que** développeur responsable de la sécurité,  
**je veux** supprimer le mot de passe hardcodé dans `FrmLogin.cs`,  
**afin de** ne jamais exposer de credentials dans le code source.

**Critères d'acceptance (DoD) :**
- [ ] Aucun mot de passe, hash ou secret ne figure dans `FrmLogin.cs` ni dans aucun fichier `.cs`
- [ ] L'authentification utilise uniquement `BCrypt.Verify()` contre la valeur stockée en base
- [ ] `git log` ne révèle aucun secret résiduel (si nécessaire : rewrite history avec confirmation explicite)
- [ ] Tests : tentative de connexion avec le mot de passe debug → refusée

**Contraintes techniques :**
- Ne jamais passer de credentials en paramètre de méthode publique
- Logger uniquement "Échec authentification" — jamais le mot de passe saisi

---

### US-01 — Corriger le filtre Ingrédients : activité → stock physique
**Gap :** G2 (🔴 BLOQUANT design v10)  
**Référence :** `IngredientDAL.cs` + `FrmIngredients.cs`

**En tant que** artisan utilisant l'application,  
**je veux** filtrer mes ingrédients par stock physique (Frigo, Réserve sèche, Cave…),  
**afin de** voir rapidement ce qui est disponible à un endroit précis, indépendamment de l'activité.

**Critères d'acceptance (DoD) :**
- [ ] `FrmIngredients` reçoit `idStock` (et non plus `idActivite`) comme paramètre de filtre
- [ ] `IngredientDAL.GetAll(idStock)` est appelé — la surcharge `idActivite` reste disponible mais n'est plus utilisée dans ce contexte
- [ ] ChipBar affiche la liste des stocks physiques (tous + chaque stock nommé)
- [ ] Sélectionner une chip filtre instantanément la DataGridView sans rechargement de la Form
- [ ] L'option "Tous" affiche l'ensemble des ingrédients actifs
- [ ] Colonnes affichées conformes au design : Nom · Conditionnement · Unité base · Stock (lieu) · Type physique · Densité · Dispo · €/cond.
- [ ] Aucune régression sur les fiches BOM existantes (les fiches référencent les ingrédients par `id_fiche_ingredient`, pas par `id_stock`)

**Contraintes techniques :**
- `IngredientDAL.GetAll(idStock: int)` — requête paramétrée, déjà présente et correcte
- Le filtre `idActivite` dans `IngredientDAL` doit rester (utilisé dans d'autres contextes potentiels)
- Aucune modification du schéma DB nécessaire

---

### US-02 — Câbler le point d'entrée "Créer un contexte"
**Gap :** G1 (🔴 BLOQUANT — flux principal cassé)  
**Référence :** `FrmPrincipal.cs` (`BuildHub()`) + `Navigation` Sidebar

**En tant que** artisan démarrant une nouvelle ligne de production,  
**je veux** un bouton visible pour créer un contexte de production,  
**afin de** démarrer le workflow BOM sans chercher comment accéder à cette fonctionnalité.

**Critères d'acceptance (DoD) :**
- [ ] Un bouton "＋ Nouveau contexte" est visible dans la sidebar, dans la section CONTEXTES, quand une activité est sélectionnée
- [ ] Ce bouton est absent tant qu'aucune activité n'est sélectionnée (cohérence avec la cascade navigation)
- [ ] Clic → ouvre le formulaire de création de contexte (existant — FrmContexte ou équivalent)
- [ ] Après création, le nouveau contexte apparaît dans la sidebar et est sélectionné automatiquement
- [ ] Si l'activité courante n'a pas encore de contexte, un message d'onboarding "Aucun contexte — créez-en un pour démarrer la production" est affiché dans la zone centrale

**Contraintes techniques :**
- Architecture SFA : pas de `new FrmContexte().ShowDialog()` depuis la sidebar — passer par `ScreenRouter`
- `AppState.StateChanged` doit être émis après création pour mettre à jour la sidebar

---

## P1 — IMPORTANT

---

### US-03 — Panel liaison M:N Stock ↔ Activité dans Stocks Screen
**Gap :** G3 (🟠 IMPORTANT)  
**Référence :** `FrmStocks.cs` · table `activites_stocks`

**En tant que** gestionnaire des ressources,  
**je veux** lier ou délier des activités à un stock physique depuis l'écran Stocks,  
**afin de** contrôler quelles activités ont accès à quel espace de stockage.

**Critères d'acceptance (DoD) :**
- [ ] Sélectionner une ligne dans la DGV Stocks affiche un panel latéral (GroupBox) listant toutes les activités existantes
- [ ] Chaque activité est représentée par une checkbox cochée si la liaison `activites_stocks` existe, décochée sinon
- [ ] Cocher une checkbox → insère la ligne dans `activites_stocks` (INSERT immédiat — pas de bouton "Enregistrer")
- [ ] Décocher → supprime la liaison (DELETE immédiat)
- [ ] Supprimer un stock est désactivé (bouton grisé) si le stock contient des `fiches_ingredients` ou des `lots_ingredients`
- [ ] Un tooltip sur le bouton Supprimer désactivé explique la raison

**Contraintes techniques :**
- INSERT/DELETE paramétrés — jamais de concaténation SQL
- Aucune modification de schéma — `activites_stocks (id_activite, id_stock)` existe déjà
- Pattern SFA : le panel est dans la même Form (Dock=Right ou SplitContainer), pas une Form séparée

---

### US-04 — Transaction FIFO : Lancer une production depuis Production Simulation
**Gap :** G4 (🟠 IMPORTANT)  
**Référence :** `BomProductionDAL.cs` · `BomStockDAL.cs` · `FrmBomProductionSimulation.cs`

**En tant que** artisan en cours de production,  
**je veux** cliquer "▶ Lancer la production" après avoir simulé et vérifié la disponibilité,  
**afin d'** enregistrer la production, décrémenter les stocks N-1 en FIFO et créer le lot au niveau N.

**Critères d'acceptance (DoD) :**
- [ ] Le bouton "▶ Lancer la production" est **désactivé** tant que la simulation révèle ≥ 1 pénurie (champ `Manque > 0`)
- [ ] Le bouton est **activé** uniquement quand `BomProductionDAL.VerifierDisponibilite()` retourne une liste vide
- [ ] Clic → appel `BomProductionDAL.Executer(idNiveau, idFiche, quantiteCible, notes)` dans un thread UI-safe (pas de blocage de la Form)
- [ ] La transaction est atomique : si une étape échoue, rollback complet (déjà géré par `BomProductionDAL`)
- [ ] En cas de succès : message de confirmation "Production enregistrée — X unités créées au niveau N · Coût : Y €"
- [ ] En cas d'échec (exception) : message d'erreur lisible, pas de stack trace exposée à l'utilisateur
- [ ] Après production réussie : rechargement de la simulation (quantités disponibles mises à jour)
- [ ] Aucune possibilité de double-clic accidentel (désactiver le bouton pendant le traitement, réactiver après)

**Contraintes techniques :**
- `BomProductionDAL.Executer()` est déjà implémenté et testé — ne pas le modifier
- Le moteur FIFO (`ConsumeStock`) est dans `BomProductionDAL` — ne pas le dupliquer
- Utiliser `async/await` ou `BackgroundWorker` pour ne pas bloquer le thread UI
- Logger l'id de production créée dans `docs/JOURNAL.md` via Agent #8

---

### US-05 — Correction Architecture SFA : FrmPrincipal reste la Form active
**Gap :** R2 (MEMORY.md — bloquant SFA)  
**Référence :** `FrmPrincipal.cs` · `Program.cs`

**En tant que** développeur,  
**je veux** que `FrmPrincipal` reste la seule Form parente active (non fermée) pendant toute la durée de session,  
**afin que** l'architecture SFA (ScreenRouter + AppState) fonctionne sans perte d'état.

**Critères d'acceptance (DoD) :**
- [ ] `Application.Run(new FrmPrincipal())` est le seul point d'entrée post-login dans `Program.cs`
- [ ] `FrmLogin` se ferme proprement après login réussi sans fermer `FrmPrincipal`
- [ ] Aucun `this.Close()` ou `Application.Exit()` n'est appelé depuis `FrmPrincipal` sauf sur la croix de fermeture
- [ ] `FrmActivites` reste la seule Form modale légitime (exception documentée dans le code)
- [ ] Test : ouvrir l'app → naviguer entre 5 écrans → aucune Form zombie dans le TaskManager

**Contraintes techniques :**
- Pattern `FrmLogin → FrmPrincipal` : utiliser `Hide()` sur FrmLogin puis `Show()` sur FrmPrincipal, ou gérer via `Program.cs` avec une variable de session
- Documenter clairement l'exception modale `FrmActivites` avec un commentaire XML dans le code

---

## P2 — MOYEN

---

### US-06 — Exporter CSV depuis Vue Stock Global
**Gap :** G5 (🟡 MOYEN)  
**Référence :** `FrmVueStockGlobal.cs` · `vue_stock_global` (vue SQL)

**En tant que** gestionnaire des stocks,  
**je veux** exporter la vue stock global en CSV,  
**afin de** partager ou analyser les données dans un tableur.

**Critères d'acceptance (DoD) :**
- [ ] Bouton "🖨 Exporter CSV" présent dans la toolbar de `FrmVueStockGlobal`
- [ ] Clic → ouvre un `SaveFileDialog` filtré `*.csv` avec nom par défaut `stock_global_YYYY-MM-DD.csv`
- [ ] Le fichier CSV généré contient les mêmes colonnes que la DGV, dans le même ordre
- [ ] Séparateur : point-virgule (`;`) — compatible Excel FR
- [ ] Encodage : UTF-8 avec BOM (pour compatibilité Excel)
- [ ] Les lignes filtrées (activité sélectionnée dans la ChipBar) sont les seules exportées
- [ ] Message de succès après export : "Fichier exporté : [chemin]"

**Contraintes techniques :**
- Aucune dépendance externe — utiliser `System.IO.StreamWriter` uniquement
- Ne pas logger le chemin du fichier dans JOURNAL.md (donnée utilisateur)
- `SaveFileDialog` : initialiser `InitialDirectory` sur `Environment.SpecialFolder.MyDocuments`

---

### US-07 — Dupliquer une fiche BOM
**Gap :** G6 (🟡 MOYEN)  
**Référence :** `BomFicheDAL.cs` · `FrmContexteNiveaux.cs`

**En tant que** artisan gérant ses recettes de production,  
**je veux** dupliquer une fiche BOM existante,  
**afin de** créer rapidement une variante sans ressaisir toutes les lignes de composition.

**Critères d'acceptance (DoD) :**
- [ ] Bouton "📋 Dupliquer" présent dans la toolbar fiches BOM, désactivé si aucune fiche sélectionnée
- [ ] Clic → demande de confirmation avec le nom de la fiche source
- [ ] La fiche dupliquée est créée dans le **même niveau** avec le nom `"Copie de [nom original]"`
- [ ] Toutes les lignes de composition (`bom_fiches_lignes`) sont copiées à l'identique
- [ ] Après duplication, la fiche copie est sélectionnée automatiquement dans la DGV
- [ ] Si le nom "Copie de [nom original]" existe déjà : suffixe numérique `(2)`, `(3)`…

**Contraintes techniques :**
- Ajouter `BomFicheDAL.Duplicate(int idFiche, string nouveauNom)` qui réutilise `Insert()` avec les lignes copiées
- La transaction est identique à `BomFicheDAL.Insert()` — wrapper propre, pas de duplication de logique
- Validation unicité du nom : appeler `BomFicheDAL.NomExiste()` avant l'insert

---

### US-08 — StatCards Hub cliquables avec navigation contextuelle
**Gap :** G7 (🟡 MOYEN)  
**Référence :** `FrmPrincipal.cs` (`BuildHub()`) · ScreenRouter

**En tant que** artisan sur le Hub de son activité,  
**je veux** cliquer sur les StatCards du Hub pour naviguer directement vers la ressource concernée,  
**afin de** gagner du temps sans passer par la sidebar.

**Critères d'acceptance (DoD) :**
- [ ] StatCard "Ingrédients" (H3) → navigue vers `FrmIngredients` filtré sur l'activité courante
- [ ] StatCard "En alerte" (H4, couleur danger) → navigue vers `FrmIngredients` filtré sur les ingrédients sous le seuil d'alerte
- [ ] StatCard "Fiches BOM" (H5, couleur gold) → navigue vers `FrmContexteNiveaux` du premier contexte de l'activité
- [ ] StatCard "Productions 7j" (H6, couleur success) → navigue vers l'historique des productions (écran ou DGV filtrée)
- [ ] Curseur `hand` au survol des StatCards cliquables (visibilité Hick-Hyman)
- [ ] StatCards non cliquables (si aucune donnée) restent visuellement distinctes mais sans curseur hand

**Contraintes techniques :**
- Architecture SFA : naviguer via `ScreenRouter.Navigate(ScreenId, params)` — pas de `new Form().Show()`
- `AppState` doit transmettre le filtre "alertes seulement" à `FrmIngredients`
- Respecter la grille 8px sur la taille des cards (min 44px hauteur pour Fitts)

---

### US-09 — Rapport du jour (Hub H1)
**Gap :** G8 (🟡 MOYEN)  
**Référence :** `FrmPrincipal.cs` (`BuildHub()`) · `BomProductionDAL.GetRecentByActivite()`

**En tant que** artisan en fin de journée,  
**je veux** générer un rapport journalier de production en un clic,  
**afin d'** avoir un récapitulatif imprimable des productions, coûts et alertes du jour.

**Critères d'acceptance (DoD) :**
- [ ] Bouton "🖨 Rapport du jour" présent dans le Hub, visible uniquement si l'activité a des productions
- [ ] Clic → ouvre un `PrintPreviewDialog` avec le rapport formaté
- [ ] Contenu du rapport :
  - En-tête : Nom activité · Date du jour · Généré à HH:MM
  - Section 1 — Productions du jour : liste des productions (`BomProductionDAL.GetRecentByActivite` filtré sur aujourd'hui)
  - Section 2 — Alertes de stock : ingrédients sous seuil (`IngredientDAL.GetAll` filtré `StockActuel < SeuilAlerteStock`)
  - Pied de page : coût total des productions du jour
- [ ] Le rapport peut être imprimé via `PrintDocument` standard WinForms
- [ ] Si aucune production aujourd'hui : rapport vide avec message "Aucune production ce jour"

**Contraintes techniques :**
- Utiliser `System.Drawing.Printing.PrintDocument` — pas de dépendance externe (PDF)
- Pas de `SaveFileDialog` requis pour cette US — impression directe ou aperçu
- Police rapport : Segoe UI 10pt corps, 9pt Bold sections (cohérence palette)

---

## P3 — FAIBLE

---

### US-10 — Onboarding : lien "créer un stock d'abord"
**Gap :** G9 (🟢 FAIBLE) — confirmé absent dans le codebase  
**Référence :** `FrmPrincipal.cs` (écran onboarding vide) · `FrmStocks.cs`

**En tant que** nouvel utilisateur sans aucune donnée,  
**je veux** un lien cliquable "créer un stock d'abord" dans l'écran d'onboarding,  
**afin de** comprendre que la création d'un stock précède celle d'une activité.

**Critères d'acceptance (DoD) :**
- [ ] Dans l'écran onboarding (aucune activité), le texte "créer un stock d'abord" est un lien cliquable (Label avec `ForeColor=OR`, `Cursor=Hand`)
- [ ] Clic → navigue vers `FrmStocks` via `ScreenRouter`
- [ ] Le workflow 4 étapes (stock → activité → liaison → contexte) est affiché textuellement dans l'onboarding

---

### US-11 — Badge "top — supprimable" sur le niveau top dans ContexteNiveaux
**Gap :** G10 (🟢 FAIBLE)  
**Référence :** `FrmContexteNiveaux.cs` · `BomNiveauDAL.cs`

**En tant que** artisan gérant ses niveaux de production,  
**je veux** identifier visuellement le niveau supprimable (top = ordre max),  
**afin de** ne pas me tromper sur quel niveau je peux supprimer.

**Critères d'acceptance (DoD) :**
- [ ] Le niveau de plus haut ordre affiche un badge pill "top · supprimable" (couleur OR `#D4AF37`)
- [ ] N0 (stock global, ordre 0) affiche un badge pill "verrouillé" (couleur CHOCO_MED `#6F4E37`)
- [ ] Les autres niveaux n'affichent aucun badge
- [ ] Le bouton "✕ Supprimer" dans la toolbar est désactivé si le niveau sélectionné n'est pas le top
- [ ] Tooltip sur le bouton Supprimer désactivé : "Seul le niveau le plus haut (ordre max) est supprimable"

---

### US-12 — Filtre par activité dans Vue Stock Global
**Gap :** VS3 (🟢 FAIBLE — non coté dans audit mais identifié comme manquant)  
**Référence :** `FrmVueStockGlobal.cs` · `vue_stock_global`

**En tant que** gestionnaire supervisant plusieurs activités,  
**je veux** filtrer la vue stock global par activité,  
**afin de** voir uniquement les lots appartenant aux stocks liés à une activité donnée.

**Critères d'acceptance (DoD) :**
- [ ] ChipBar affiche "Tous" + une chip par activité
- [ ] Sélectionner une chip filtre la DGV par `id_activite` (via `activites_stocks`)
- [ ] L'export CSV (US-06) respecte le filtre actif
- [ ] "Tous" est la chip sélectionnée par défaut

**Contraintes techniques :**
- Requête SQL : jointure `activites_stocks` sur `id_stock` des lots
- Filtre appliqué côté DAL, pas côté DGV (performance sur grands volumes)

---

## Annexe A — Mapping Gaps → User Stories

| Gap | Priorité | US | Statut |
|-----|----------|----|--------|
| R8 — Sécurité password hardcodé | 🔴 P0 | US-00 | À faire |
| G2 — Filtre ingrédients activité→stock | 🔴 P0 | US-01 | À faire |
| G1 — Créer un contexte (point d'entrée) | 🔴 P0 | US-02 | À faire |
| R2 — FrmPrincipal SFA active | 🔴 P0 | US-05 | À faire |
| G3 — Panel M:N Stock↔Activité | 🟠 P1 | US-03 | À faire |
| G4 — Transaction FIFO lancer production | 🟠 P1 | US-04 | À faire |
| G5 — Export CSV Vue Stock Global | 🟡 P2 | US-06 | À faire |
| G6 — Dupliquer fiche BOM | 🟡 P2 | US-07 | À faire |
| G7 — StatCards Hub cliquables | 🟡 P2 | US-08 | À faire |
| G8 — Rapport du jour | 🟡 P2 | US-09 | À faire |
| G9 — Onboarding lien stock | 🟢 P3 | US-10 | À faire |
| G10 — Badge top niveau | 🟢 P3 | US-11 | À faire |
| VS3 — Filtre activité Vue Stock Global | 🟢 P3 | US-12 | À faire |

---

## Annexe B — Réponses aux questions ouvertes (Agent #3)

**Q1 — G1 : Depuis où créer un contexte ?**  
Décision PO : bouton "＋ Nouveau contexte" dans la sidebar, section CONTEXTES, visible uniquement si activité sélectionnée. Pas de bouton dans le Hub (confirmation audit : `BuildHub()` n'en a pas et le design ne le montre pas).

**Q2 — G2 : Impact filtre activité→stock sur fiches BOM ?**  
Aucun impact. Les fiches BOM référencent les ingrédients via `id_fiche_ingredient` (clé sur `fiches_ingredients.id`), pas sur `id_stock`. La migration est purement UI : changer le paramètre passé à `FrmIngredients`.

**Q3 — Transaction FIFO déjà implémentée ?**  
Confirmé après lecture : `BomProductionDAL.Executer()` est **entièrement implémenté** (transaction atomique, FIFO, multi-niveaux, rollback). Il manque uniquement le câblage UI dans `FrmBomProductionSimulation` (US-04).

**Q4 — Rapport du jour : format ?**  
Décision PO : `PrintDocument` WinForms standard (aperçu + impression). Pas de PDF (évite une dépendance externe). CSV séparé via Vue Stock Global (US-06).

**Q5 — Duplication fiche BOM : même niveau ou niveau cible ?**  
Décision PO : même niveau par défaut (simplicité — cas d'usage principal = variante). Pas de sélecteur de niveau cible pour cette US.

---

## Annexe C — Contraintes techniques transversales

1. **Sécurité OWASP :** Toutes les requêtes SQL via paramètres ADO.NET — aucune concaténation.
2. **Thread UI :** Toute opération longue (production FIFO, export CSV, génération rapport) en `async/await` ou `BackgroundWorker`.
3. **Architecture SFA :** Aucun `new Form().Show()` sauf `FrmActivites` (exception documentée). Navigation via `ScreenRouter`.
4. **DockStyle :** Respecter l'ordre `Controls.Add` : `Right` avant `Fill`, `Top` en dernier (règle MEMORY.md).
5. **SplitterDistance :** Uniquement dans un `LayoutEventHandler` après `Width > 0` (règle MEMORY.md).
6. **Palette :** Utiliser les constantes `CHOCO_DARK / OR / GREEN / RED / GREY` définies dans le design system — pas de couleurs littérales.

---

*Document produit par l'Agent #1 PO — 2026-04-19*  
*Inputs : `PO_AUDIT_FONCTIONNALITES_DESIGN.md` · `IngredientDAL.cs` · `BomProductionDAL.cs` · `BomFicheDAL.cs` · recherche PrintDocument/Export (résultat : absent)*
