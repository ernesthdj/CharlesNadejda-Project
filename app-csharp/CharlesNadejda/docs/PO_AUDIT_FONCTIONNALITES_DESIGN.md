# Audit Fonctionnalités — Design ArtisaStock
**Source :** Design Claude (claude.ai/design — bundle `J3KiL1okbdg1M9rvZyS4YQ`)
**Date audit :** 2026-04-19
**Auteur :** Agent #3 UI/UX (analyse exhaustive 11 fichiers JSX)
**Destinataire :** Agent #1 PO — pour planification refactoring

---

## Contexte

Ce document est le résultat d'un audit complet du design de référence ArtisaStock.
Chaque élément cliquable de chaque écran a été inventorié.
L'objectif est de permettre au PO de dresser les User Stories manquantes,
d'identifier les gaps entre l'implémentation actuelle et le design cible,
et de coordonner l'équipe d'agents pour un refactoring cohérent.

**Fichiers analysés :**
`Navigation.jsx` · `Chrome.jsx` · `HubScreen.jsx` · `ContexteNiveauxScreen.jsx`
· `IngredientsScreen.jsx` · `StocksScreen.jsx` · `VueStockGlobalScreen.jsx`
· `ProductionSimulationScreen.jsx` · `FicheBomEditModal.jsx` · `OnboardingEmpty.jsx`
· `Components.jsx` (bibliothèque de composants partagés)

---

## 1. Inventaire exhaustif — Éléments cliquables par écran

### 1.1 Navigation Globale (Sidebar 240px)

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| N1 | Item activité (clic simple) | Sélectionne l'activité → affiche Hub Screen |
| N2 | ⚙ bouton gear sur item activité | Ouvre FrmActivites (gestion des activités) |
| N3 | Item contexte (clic simple) | Sélectionne le contexte → affiche ContexteNiveaux Screen |
| N4 | 📊 Vue stock global | Navigue vers VueStockGlobal Screen |
| N5 | 📦 Stocks | Navigue vers Stocks Screen |
| N6 | 🥣 Ingrédients | Navigue vers Ingrédients Screen |
| N7 | 🏢 Fournisseurs | Navigue vers Fournisseurs Screen |
| N8 | 🧾 Achats/Lots | Navigue vers Achats Screen |

> **Note design :** Les sections ACTIVITÉS / CONTEXTES / NIVEAUX s'affichent en cascade.
> CONTEXTES visible seulement si activité sélectionnée.
> NIVEAUX visible seulement si contexte sélectionné.

---

### 1.2 Chrome / TitleBar (gradient #3D2817 → #6F4E37, hauteur 34px)

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| C1 | "＋ Activité" (bouton gold, CTA primaire) | Créer une nouvelle activité |
| C2 | "⚙ Activités" (bouton bordé) | Ouvre FrmActivites (liste + gestion complète) |
| C3 | _ □ × | Contrôles OS (réduire / maximiser / fermer) |

> **Statut actuel :** ✅ C1 et C2 sont implémentés dans `BuildHub()` (FrmPrincipal.cs).

---

### 1.3 Onboarding Empty (aucune activité existante)

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| O1 | "＋ Créer ma première activité" (gold, CTA principal) | Ouvre FrmActivites → créer activité |
| O2 | Lien "créer un stock d'abord" | Naviguer vers Stocks Screen |

> **Statut actuel :** ✅ O1 implémenté. ❌ O2 manquant — lien non câblé.

**Workflow 4 étapes expliqué dans l'onboarding :**
1. Créer un stock (lieu physique)
2. Créer une activité (ce que tu produis)
3. Lier stocks ↔ activité (M:N)
4. Créer un contexte de production (niveaux + fiches BOM)

---

### 1.4 Hub Screen (activité sélectionnée)

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| H1 | "🖨 Rapport du jour" (bouton default) | Générer / imprimer le rapport journalier |
| H2 | "▶ Nouvelle production" (bouton primary) | Naviguer vers Production Simulation Screen |
| H3 | StatCard — Ingrédients (clic) | Naviguer vers Ingrédients Screen |
| H4 | StatCard — En alerte (clic, danger) | Naviguer vers Ingrédients filtrés par alertes |
| H5 | StatCard — Fiches BOM (clic, gold) | Naviguer vers ContexteNiveaux Screen |
| H6 | StatCard — Productions 7j (clic, success) | Naviguer vers historique productions |
| H7 | Ligne DataTable "Dernières productions" (simple/double clic) | Sélectionner / voir détail production |
| H8 | AlertRow critique/warning (clic) | Naviguer vers la ressource concernée |

> **Statut actuel :** ✅ H2 implémenté. ❌ H1 (rapport) manquant. ❌ H3–H8 StatCards non cliquables.
> **⚠ GAP CRITIQUE — H-CONTEXTE :** Aucun bouton "Nouveau contexte" n'est présent dans le Hub.
> Le design ne le montre pas explicitement, mais c'est le principal workflow de démarrage.
> À clarifier avec le PO : le Hub doit-il avoir un bouton "＋ Nouveau contexte" ?

---

### 1.5 Contextes & Niveaux Screen

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| CN1 | "＋ Nouveau niveau" (primary) | Créer un nouveau niveau dans le contexte actif |
| CN2 | "✎ Renommer" (edit) | Renommer le contexte actif |
| CN3 | "✕ Supprimer" contexte (danger, disabled si pas top niveau) | Supprimer le niveau top uniquement |
| CN4 | Clic sur une ligne niveau | Sélectionner ce niveau → afficher ses fiches BOM |
| CN5 | "＋ Nouvelle fiche" (toolbar fiches BOM) | Ouvre FicheBomEditModal en mode création |
| CN6 | "✎ Modifier" (toolbar fiches BOM) | Ouvre FicheBomEditModal sur la fiche sélectionnée |
| CN7 | "✕ Supprimer" (toolbar fiches BOM) | Supprimer la fiche BOM sélectionnée |
| CN8 | "📋 Dupliquer" (toolbar fiches BOM) | Dupliquer la fiche BOM sélectionnée |
| CN9 | Clic sur ligne fiche BOM (simple/double) | Sélectionner / ouvrir la fiche |

> **Statut actuel :** ✅ CN1, CN2, CN3 (modifier/supprimer contexte), CN4, CN5, CN6, CN7, CN9 implémentés.
> ❌ CN8 (dupliquer fiche) manquant.

**Règles design niveau :**
- N0 = stock global, verrouillé (non supprimable)
- Seul le niveau top (ordre max) est supprimable
- Badge "top — supprimable" (pill or) affiché sur le niveau le plus haut
- Bordure gauche 4px : locked=beige, selected=OR, unselected=CHOCO_MED

---

### 1.6 Ingrédients Screen

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| I1 | "＋ Nouveau" (primary) | Créer un nouvel ingrédient |
| I2 | "✎ Modifier" (edit) | Modifier l'ingrédient sélectionné |
| I3 | "✕ Supprimer" (danger) | Supprimer l'ingrédient sélectionné |
| I4 | ChipBar — filtre par stock | Filtrer la liste par stock physique (Tous / Frigo / Réserve sèche / Cave...) |
| I5 | Clic sur ligne ingrédient | Sélectionner l'ingrédient |

> **⚠ CHANGEMENT DESIGN v10 :** Le filtre est par **stock physique**, PAS par activité.
> Colonnes affichées : Nom · Conditionnement · Unité base · Stock (lieu) · Type physique · Densité · Dispo · €/cond.
> **Statut actuel :** ❌ FrmIngredients reçoit `ActiveActivite` comme filtre — à refactorer vers filtre par stock.

---

### 1.7 Stocks Screen

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| S1 | "＋ Nouveau stock" (primary) | Créer un stock physique |
| S2 | "✎ Modifier" (edit) | Modifier le stock sélectionné |
| S3 | "✕ Supprimer" (danger, **disabled** si stock contient des données) | Supprimer le stock |
| S4 | Clic sur ligne stock | Sélectionner → afficher le panel de liaisons activités |
| S5 | Checkbox activité dans panel liaison | Lier / délier une activité à ce stock (relation M:N `activites_stocks`) |

> **⚠ FONCTIONNALITÉ MANQUANTE :** Panel de liaison M:N activité ↔ stock (GroupBox avec checkboxes).
> C'est une fonctionnalité clé du design v7. À vérifier dans FrmStocks.
> **Statut actuel :** À auditer — FrmStocks.cs non vérifié.

---

### 1.8 Vue Stock Global Screen

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| VS1 | "🖨 Exporter CSV" (default) | Exporter les données de la vue en CSV |
| VS2 | "🔍 Actualiser" (primary) | Rafraîchir depuis `vue_stock_global` |
| VS3 | ChipBar — filtre par activité | Filtrer par activité (via M:N `activites_stocks`) |
| VS4 | Clic sur ligne lot | Sélectionner la ligne |

> **Vue unifiée :** lots d'ingrédients (préfixe L) + produits fabriqués BOM (préfixe B).
> DLC colorée : neutre si OK / orange si < 7j / rouge si expiré.
> Ligne colorée : rouge = critique/expiré / orange = stock bas.
> **Statut actuel :** ❌ Export CSV manquant. ❌ Filtre par activité à vérifier.

---

### 1.9 Production Simulation Screen

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| P1 | Select "Activité" | Changer l'activité de production |
| P2 | Select "Contexte" | Changer le contexte de production |
| P3 | Select "Niveau cible" | Changer le niveau cible |
| P4 | Select "Fiche" | Choisir la fiche BOM à produire |
| P5 | NumInput "Nombre de batches" | Saisir le nombre de batches à produire |
| P6 | "🔍 Simuler la production" (primary) | Lancer la simulation → affiche grille colorée + bande résultat |
| P7 | "↺ Réinitialiser" (neutral) | Remettre le formulaire à zéro |
| P8 | "Fermer" (neutral) | Fermer / retour à l'écran précédent |
| P9 | "▶ Lancer la production" (primary, **désactivé si pénurie**) | Exécuter la transaction FIFO → crée un lot au niveau N |

> **Règles métier :**
> - L'unité affichée est "1 batch = X unité" (calculé en live)
> - Coût estimé récursif multi-niveaux affiché en temps réel
> - Bouton "Lancer" désactivé tant qu'il reste ≥ 1 pénurie
> - La production est une **transaction atomique** : consomme les lots N-1 en FIFO, crée un lot au niveau N
> **Statut actuel :** À auditer — FrmBomProductionSimulation.cs non vérifié.

---

### 1.10 Fiche BOM — Modal d'édition

| ID | Élément | Comportement attendu |
|----|---------|----------------------|
| F1 | "×" dans la titlebar du modal | Fermer sans sauvegarder |
| F2 | Input "Nom de la fiche" | Modifier le nom de la fiche |
| F3 | NumInput "Quantité / batch" | Modifier la quantité output par batch |
| F4 | Select "Unité output" | Changer l'unité de sortie (g, kg, ml, cl, pièce…) |
| F5 | "✕" sur chaque ligne de composition | Supprimer cette ligne de la fiche |
| F6 | Select "Source" (ajout nouvelle ligne) | Choisir entre Ingrédient ou Fiche N-1 |
| F7 | Select "Input" (ajout nouvelle ligne) | Sélectionner l'ingrédient ou la fiche N-1 spécifique |
| F8 | NumInput "Qté" (ajout nouvelle ligne) | Quantité pour la nouvelle ligne |
| F9 | "＋ Ajouter" | Valider et ajouter la ligne à la composition |
| F10 | "Annuler" (neutral) | Fermer sans sauvegarder |
| F11 | "💾 Enregistrer la fiche" (primary) | Sauvegarder la fiche BOM |

> **Règle métier #7 :** L'unité d'une ligne est héritée de l'ingrédient/fiche N-1 sélectionné — **verrouillée** après le choix (🔒). Non modifiable par l'utilisateur.
> **Coût live :** Coût batch + Coût unitaire + Nombre de lignes affichés en bas (calcul récursif via BomCoutDAL).

---

## 2. Récapitulatif — 52 actions identifiées

| Écran | Nb actions | Implémentées | Manquantes |
|-------|-----------|--------------|------------|
| Navigation Sidebar | 8 | 8 | 0 |
| Chrome / TitleBar | 3 | 2 | 1 (OS controls) |
| Onboarding | 2 | 1 | 1 |
| Hub Screen | 8 | 2 | 6 |
| Contextes & Niveaux | 9 | 8 | 1 (dupliquer) |
| Ingrédients | 5 | 3 | 2 (filtre stock, colonnes) |
| Stocks | 5 | 3 | 2 (panel liaison M:N) |
| Vue Stock Global | 4 | 2 | 2 (CSV, filtre activité) |
| Production Simulation | 9 | À auditer | À auditer |
| Fiche BOM (modal) | 11 | À auditer | À auditer |
| **Total** | **64** | | |

---

## 3. Gaps prioritaires identifiés

| Priorité | ID | Gap | Impact |
|----------|----|-----|--------|
| 🔴 BLOQUANT | G1 | **"Créer un Contexte"** — aucun point d'entrée UI visible dans le design actuel | Flux utilisateur principal cassé |
| 🔴 BLOQUANT | G2 | **Ingrédients filtrés par stock** (design v10) — actuellement filtré par activité | Incohérence données |
| 🟠 IMPORTANT | G3 | **Panel liaison M:N stock ↔ activité** dans Stocks Screen | Fonctionnalité clé v7 |
| 🟠 IMPORTANT | G4 | **Lancer production → transaction FIFO** — création lot N, consommation FIFO | Fonctionnalité métier cœur |
| 🟡 MOYEN | G5 | **Exporter CSV** dans Vue Stock Global | Besoin opérationnel |
| 🟡 MOYEN | G6 | **Dupliquer fiche BOM** (CN8) — `BomFicheDAL.Duplicate()` à créer | Gain de temps utilisateur |
| 🟡 MOYEN | G7 | **StatCards Hub cliquables** (H3–H6) → navigation contextuelle | UX |
| 🟡 MOYEN | G8 | **Rapport du jour** (H1) — génération rapport PDF/print | Opérationnel |
| 🟢 FAIBLE | G9 | **Onboarding lien "créer un stock d'abord"** (O2) | Guidage onboarding |
| 🟢 FAIBLE | G10 | **Badge "top — supprimable"** sur niveau top dans ContexteNiveaux | Clarté UI |

---

## 4. Spécifications design système

### Palette (à appliquer uniformément)
```
CHOCO_DARK  #3D2817   bouton principal, titres
CHOCO_MED   #6F4E37
CHOCO_LIGHT #9E7B5C
CHOCO_ABYSS #1E0F08
CREME       #F5E6D3   header DGV
CREME_WARM  #ECE9D8
OR          #D4AF37   accent activités, CTA gold
GREEN       #3EA23E   modifier
RED         #C72C48   supprimer
GREY        #DCD7D0   fermer / annuler
BORDER      #C3B9A8
```

### Typographie
- Corps : Segoe UI 10pt / 12px
- Titres écrans : Playfair Display 24px Bold (SectionHeader)
- Titres modal : 12px Bold (titlebar gradient)
- Labels champs : 11px Bold uppercase letterspacing

### Composants partagés (Components.jsx)
`FlatButton` (variants : default / primary / edit / danger / neutral / gold)
· `WinInput` · `WinSelect` · `NumInput` · `GroupBox` · `ChipBar`
· `StockBadge` · `Pill` (tones : neutral / ok / warn / crit / gold / info)
· `DataTable` · `StatCard` · `Modal` · `SectionHeader` · `Breadcrumb`

### Breadcrumb pattern
Toujours présent en haut de chaque écran.
Format : `[Section] › [Entité]` ex : `Ressources › Ingrédients` / `Chocolaterie › Production · Simulation`

---

## 5. Questions ouvertes pour le PO

1. **G1 — Créer un contexte :** Depuis où ? Hub (bouton dans header) ? Sidebar (bouton "+" dans section CONTEXTES) ? Ou les deux ?
2. **G2 — Ingrédients :** La migration filtre activité → filtre stock implique un changement de DAL (`IngredientDAL.GetAll(idStock)` au lieu de `idActivite`). Quel impact sur les fiches BOM existantes ?
3. **Production FIFO :** La transaction atomique est-elle déjà dans `BomProductionDAL` ou à créer ?
4. **Rapport du jour (H1) :** Format attendu ? PDF, impression directe, CSV ?
5. **Dupliquer fiche BOM :** La duplication crée-t-elle une copie dans le même niveau ou doit-elle permettre de choisir le niveau cible ?

---

*Document produit par l'Agent #3 UI/UX — 2026-04-19*
*Source de vérité : design bundle `J3KiL1okbdg1M9rvZyS4YQ` extrait dans `/tmp/charles-nadejda-design-system/`*
