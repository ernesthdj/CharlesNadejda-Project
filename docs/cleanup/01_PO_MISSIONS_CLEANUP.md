# AGENT #1 — PO : MISSIONS DE NETTOYAGE & REFACTORING
> **Date :** 2026-05-20
> **Codebase :** CharlesNadejda — ERP artisanal pâtisserie/chocolaterie
> **Objectif global :** Clean code, suppression orphelins, refactoring ciblé, performance, fluidité
> **Règle :** Documentation-first. Aucun code ne sera modifié avant validation QA de toutes les docs.

---

## ÉTAT DES LIEUX

| Métrique | Valeur |
|----------|--------|
| Fichiers .cs | 91 |
| Lignes de code | ~13 800 |
| Couches | Models (21) · DAL (19) · Forms (48) · Navigation (6) · Services (2) |
| Score santé | 8/10 — solide, mais 3 fichiers lourds et quelques patterns à affiner |
| Orphelins détectés | 0 fichier inutilisé (vérifié par scan d'imports) |
| Code mort | Minimal — quelques blocs commentés explicatifs |

---

## MISSIONS PAR AGENT

---

### MISSION #2 — ARCHITECT

**Périmètre :** Architecture globale, couplage inter-couches, patterns structurels

**Objectifs :**
1. **Analyser le couplage** entre Forms → DAL → Models → Navigation
   - Identifier les dépendances directes (Form qui appelle 5+ DALs)
   - Proposer des médiateurs ou services intermédiaires si justifié
2. **Évaluer les 3 fichiers lourds** et proposer des stratégies de décomposition :
   - `FrmPrincipal.cs` (1 961 lignes + 2 partials = 3 414 lignes logiques)
   - `BomProductionDAL.cs` (526 lignes — moteur de production FIFO)
   - `FrmVueStock.cs` (867 lignes)
3. **Pattern static DAL** — évaluer si le pattern all-static reste viable ou si une interface+DI légère serait bénéfique (pragmatisme : pas de sur-architecture)
4. **Vérifier la cohérence du flux Navigation** : AppState → ScreenRouter → FrmPrincipal → écrans inline
5. **Identifier les responsabilités mal placées** : logique métier dans les Forms ? Calculs dans les DAL qui devraient être dans un Service ?

**Livrables :**
- Carte des dépendances (quels Forms appellent quels DALs)
- Liste de refactorings proposés avec effort/impact
- Avis sur le pattern static DAL vs DI

**Critères d'acceptation :**
- Chaque suggestion est justifiée (pourquoi, quel gain)
- Pas de sur-architecture — on reste pragmatique pour un projet étudiant
- Chaque refactoring proposé est étiqueté : `QUICK` (<30 min) / `MEDIUM` (1-2h) / `HEAVY` (>2h)

---

### MISSION #3 — UI/UX DESIGNER

**Périmètre :** Couche Forms — code de présentation, ergonomie du code (pas l'ergonomie utilisateur)

**Objectifs :**
1. **Audit de cohérence UI dans le code :**
   - Tous les Forms héritent-ils de FrmEditBase/FrmListeBase quand ils le devraient ?
   - Y a-t-il des Forms "custom" (sans héritage) qui pourraient bénéficier des bases ?
   - Les constantes UI (marges, tailles, fonts) sont-elles centralisées ou hardcodées ?
2. **BuildUI() : qualité et maintenabilité :**
   - Identifier les méthodes BuildUI() de plus de 100 lignes
   - Proposer des extractions en sous-méthodes nommées (BuildHeader, BuildGrid, BuildDetail, etc.)
   - Vérifier la cohérence des Dock/Anchor/Padding entre forms
3. **AppColors.cs — couverture complète ?**
   - Y a-t-il des `Color.FromArgb(...)` hardcodés dans les Forms au lieu d'utiliser AppColors ?
   - Des couleurs dans AppColors qui ne sont jamais utilisées ?
4. **Patterns de rendering du volet détail :**
   - RenderDetailIngredient / RenderDetailProduit — code dupliqué ?
   - Les helpers AddDetailRow/AddDetailSection sont-ils réutilisés partout ?
5. **FormHelper.cs — est-il utilisé ? Pourrait-il absorber plus de logique commune ?**

**Livrables :**
- Liste des incohérences UI trouvées
- Propositions d'extraction de méthodes longues
- Inventaire des couleurs hardcodées vs AppColors

**Critères d'acceptation :**
- Chaque observation est localisée (fichier:ligne)
- Les propositions ne cassent pas le comportement visuel existant
- Priorisation : `COSMETIC` / `MAINTENABILITÉ` / `COHÉRENCE`

---

### MISSION #4 — BACKEND DEVELOPER

**Périmètre :** DAL (19 fichiers), Models (21 fichiers), Services (2 fichiers), DbHelper

**Objectifs :**
1. **Audit des Models :**
   - Propriétés inutilisées ? Propriétés calculées qui devraient être en DB ou inversement ?
   - Cohérence des types (nullable vs non-nullable) — correspondent-ils au schéma DB ?
   - Vérifier que TOUS les Models sont des POCO purs (aucune logique métier)
2. **Audit des DAL :**
   - **SQL orphelin** : des requêtes SQL qui référencent des colonnes/tables qui n'existent plus ?
   - **Requêtes N+1** : des boucles qui appellent GetById() en boucle au lieu d'un GetByIds() batch ?
   - **Bind/Map cohérence** : même pattern partout ? Cas oubliés de DBNull.Value ?
   - **Transactions** : toutes les opérations multi-tables sont-elles en transaction ?
   - **Index implicites** : des WHERE/JOIN sur colonnes non indexées fréquemment ?
3. **BomProductionDAL.cs (526 lignes) — deep dive :**
   - Complexité cyclomatique de VerifierDisponibilite(), Simuler(), ConsumeStock()
   - Logique FIFO : est-elle correcte et testable ?
   - Proposer une extraction en `ProductionService` si justifié
4. **BomCoutDAL.cs — récursion :**
   - Le HashSet<int> de détection de cycles est-il suffisant ?
   - Performance sur des arbres profonds (>5 niveaux) ?
5. **Services/SimulationService.cs :**
   - Est-il bien utilisé ? Ou les Forms appellent-ils directement BomProductionDAL.Simuler() ?
   - Le Task.Run() est-il justifié ?

**Livrables :**
- Inventaire des problèmes par DAL (tableau)
- Propositions de refactoring avec priorité
- Avis sur l'extraction BomProductionDAL → Service

**Critères d'acceptation :**
- Chaque problème SQL est vérifié contre le schéma DB réel
- Les optimisations proposées sont mesurables (N+1 éliminé, requêtes réduites)
- Pas de changement d'API publique sans justification forte

---

### MISSION #6 — QA ENGINEER

**Périmètre :** Validation croisée de TOUTES les docs agents (#2, #3, #4)

**Objectifs :**
1. **Cohérence inter-agents :**
   - L'Architect propose-t-il un refactoring que le Backend contredit ?
   - L'UI/UX propose-t-il des changements qui impactent le Backend sans le mentionner ?
   - Les priorités sont-elles alignées entre agents ?
2. **Faisabilité :**
   - Les refactorings proposés sont-ils réalistes dans le contexte d'un projet étudiant ?
   - Y a-t-il des risques de régression non mentionnés ?
3. **Complétude :**
   - Chaque couche a-t-elle été couverte ?
   - Des fichiers importants ont-ils été oubliés ?
4. **Verdict final :**
   - VALIDÉ / VALIDÉ AVEC RÉSERVES / REJETÉ (avec motif)
   - Liste consolidée des actions retenues, ordonnées par priorité

**Livrables :**
- Matrice de cohérence (agent × agent)
- Liste des conflits détectés
- Plan d'action consolidé final (priorisé)

**Critères d'acceptation :**
- Chaque conflit est résolu avec une recommandation
- Le plan final est actionnable (chaque item = 1 PR potentielle)
- Aucun refactoring à risque sans mitigation documentée

---

## SÉQUENCE D'EXÉCUTION

```
[PO] ──doc──→ [Architect] ──doc──→ ┐
                                    ├──→ [QA] ──validation──→ PLAN FINAL
[PO] ──doc──→ [UI/UX]     ──doc──→ ┤
                                    │
[PO] ──doc──→ [Backend]   ──doc──→ ┘
```

- Architect, UI/UX et Backend travaillent **en parallèle** après le PO
- Chaque agent consulte la doc PO + l'inventaire codebase
- Le QA attend TOUTES les docs avant de valider

---

## RÈGLES TRANSVERSALES

1. **Aucun code modifié** — cette phase est documentation-only
2. **Chaque suggestion est localisée** — fichier, ligne, méthode
3. **Pragmatisme** — projet étudiant, pas d'over-engineering
4. **Sécurité** — toute faille détectée est signalée immédiatement, même en phase doc
5. **Format des docs** — Markdown, tableaux structurés, pas de prose inutile
