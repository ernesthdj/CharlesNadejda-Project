# PO — FlowScope

> **Product Owner :** Agent #1 — Équipe IA mentalyas
> **Date :** 2026-05-14
> **Version :** 1.0 — MVP
> **Projet cible initial :** Charles & Nadejda (ERP artisanal C# + Laravel + MySQL)

---

## Vision produit

FlowScope est un **navigateur interactif de logigrammes** qui cartographie visuellement la structure et la logique d'un projet logiciel. L'outil transforme du code source (SQL, C#) en graphes interactifs navigables, offrant une vue d'ensemble immédiate de l'architecture d'un projet.

**Proposition de valeur :** Un développeur solo ou une petite équipe peut, en quelques secondes, visualiser les couches de son application (DB, Forms, DAL (Data Access Layer — couche d'accès aux données), Models, pipelines métier) sous forme de graphes cliquables, au lieu de naviguer manuellement entre dizaines de fichiers.

**Positionnement :** Outil local, léger, projet-agnostique. Pas un IDE, pas un outil de collaboration — une **boussole de développement** et un **support de présentation**.

---

## Personas

### P1 — Le Développeur Solo (persona principal)

- **Qui :** mentalyas, étudiant 2e année Bachelier IT, développeur du projet Charles & Nadejda
- **Contexte :** Gère seul un projet avec ~50 tables MySQL, ~40 classes utiles (sur ~80 fichiers C#), un pipeline BOM (Bill of Materials — nomenclature de production) multi-niveaux, et un scaffold Laravel
- **Frustration :** Perd du temps à naviguer entre fichiers pour comprendre les relations. Difficile d'avoir une vue d'ensemble quand le projet grossit
- **Besoin :** Voir d'un coup d'œil comment les pièces s'assemblent ; cliquer pour plonger dans le détail
- **Critère de succès :** "Je charge mon projet, je vois le graphe, je clique sur une table et je vois ses FK (Foreign Keys — clés étrangères) et qui la consomme"

### P2 — Le Présentateur

- **Qui :** Le même développeur, mais dans un contexte d'examen ou de démo
- **Besoin :** Montrer l'architecture de son projet de manière claire et professionnelle
- **Critère de succès :** "Je peux expliquer mon architecture en naviguant visuellement, sans ouvrir VS Code"

---

## Definition of Done (globale)

Chaque User Story est considérée **terminée** quand :

1. **Code** : TypeScript strict (`strict: true`), pas de `any`, pas de `console.log` de debug
2. **Style** : Tailwind CSS, thème dark appliqué, responsive desktop (min 1280px)
3. **Accessibilité** : Focus clavier fonctionnel, contraste WCAG AA (4.5:1 texte, 3:1 éléments larges)
4. **Tests** : Comportement testé manuellement selon les critères d'acceptation listés
5. **Intégration** : Le composant s'intègre sans régression dans l'application existante
6. **Schéma** : Toute donnée consommée respecte le schéma JSON universel défini
7. **Build** : `npm run build` passe sans erreur ni warning TypeScript

---

## Backlog MVP — User Stories

### Sprint 1 : Fondations

> **Objectif :** Projet initialisé, schéma JSON défini, un graphe minimal rendu à l'écran.

---

#### US-1.1 — Initialisation du projet

**En tant que** développeur, **je veux** un projet React + TypeScript + Vite initialisé avec les dépendances core **afin de** démarrer le développement sur une base propre.

**Critères d'acceptation :**
- [ ] Projet créé avec Vite + React + TypeScript template
- [ ] Dépendances installées : `@xyflow/react`, `tailwindcss`, `lucide-react`, `fuse.js`
- [ ] `tsconfig.json` avec `strict: true`, paths alias `@/` vers `src/`
- [ ] Tailwind configuré avec thème dark par défaut (couleurs custom définies dans `tailwind.config.ts`)
- [ ] Structure de dossiers créée :
  ```
  src/
  ├── components/     # Composants React réutilisables
  ├── layouts/        # Layout principal (sidebar + content)
  ├── views/          # Vues par système (DB, CSharp, BOM, etc.)
  ├── types/          # Interfaces TypeScript (schéma JSON)
  ├── hooks/          # Custom hooks React
  ├── stores/         # État global (si nécessaire)
  ├── utils/          # Fonctions utilitaires
  ├── data/           # Fichiers JSON de données
  └── parsers/        # Scripts de parsing (Node.js)
  ```
- [ ] Page d'accueil affiche "FlowScope" avec le nom du projet chargé
- [ ] `npm run dev` démarre sans erreur
- [ ] `npm run build` compile sans erreur ni warning

---

#### US-1.2 — Schéma JSON universel (types TypeScript)

**En tant que** développeur, **je veux** que les interfaces TypeScript du schéma JSON universel soient définies et exportées **afin que** tous les composants et parsers partagent un contrat de données unique.

**Critères d'acceptation :**
- [ ] Fichier `src/types/schema.ts` créé avec les interfaces :
  - `FlowScopeProject` (name, version, lastParsed, systems)
  - `SystemDefinition` (id, label, icon, description, nodes, edges)
  - `FlowNode` (id, type, label, description?, filePath?, metadata, tags?, group?, position?)
  - `FlowEdge` (id, source, target, label?, type?, animated?)
  - `NodeType` (union type : "table" | "form" | "dal" | "model" | "controller" | "route" | "view" | "process" | "stock" | "custom")
  - `EdgeType` (union type : "dependency" | "inheritance" | "fk" | "flow" | "data")
- [ ] Fichier `src/types/index.ts` réexporte tout
- [ ] Un fichier JSON d'exemple (`src/data/example-project.json`) valide le schéma avec au moins 2 systèmes, 3 nodes et 2 edges chacun
- [ ] Les types sont utilisables sans cast ni `as` dans les composants

---

#### US-1.3 — Rendu d'un graphe minimal React Flow

**En tant que** développeur, **je veux** voir un graphe React Flow rendu à l'écran avec des nodes et edges depuis un fichier JSON **afin de** valider que le pipeline données → rendu fonctionne.

**Critères d'acceptation :**
- [ ] Composant `FlowCanvas` créé, wrappé dans `<ReactFlowProvider>`
- [ ] Charge les données depuis le fichier JSON d'exemple (US-1.2)
- [ ] Affiche les nodes avec leur label visible
- [ ] Affiche les edges avec les connexions correctes
- [ ] Zoom, pan et drag fonctionnent (comportement par défaut React Flow)
- [ ] Fond sombre (thème dark) avec grille de points visible
- [ ] MiniMap affichée en bas à droite
- [ ] Le graphe occupe 100% de l'espace disponible (pas de scroll page)

---

#### US-1.4 — Node custom richement typé

**En tant que** utilisateur, **je veux** que chaque node affiche son type via une icône, son nom, et un résumé visuel (nombre de champs/méthodes) **afin de** distinguer rapidement les éléments du graphe.

**Critères d'acceptation :**
- [ ] Composant `CustomNode` créé, enregistré comme nodeType dans React Flow
- [ ] Affiche : icône du type (Lucide), label en gras, badge du type (ex: "TABLE", "FORM")
- [ ] Si `metadata` contient des clés (ex: `columns`, `methods`), affiche le compte (ex: "8 cols", "3 methods")
- [ ] Couleurs différentes par NodeType (palette cohérente dark theme) :
  - `table` : bleu · `form` : violet · `dal` : orange · `model` : vert
  - `controller` : rouge · `route` : cyan · `view` : rose · `process` : jaune
  - `stock` : émeraude · `custom` : gris
- [ ] Node sélectionné a un contour lumineux (glow)
- [ ] Taille du node s'adapte au contenu (pas de troncature du label)

---

#### US-1.5 — Layout principal (shell de l'application)

**En tant que** utilisateur, **je veux** une interface avec une sidebar à gauche et le graphe à droite **afin de** naviguer entre les vues.

**Critères d'acceptation :**
- [ ] Layout avec sidebar fixe (240px) à gauche + zone de contenu à droite
- [ ] Sidebar affiche le nom du projet en haut
- [ ] Sidebar liste les systèmes disponibles (issus du JSON) avec icône + label
- [ ] Le système actif est visuellement distingué (fond coloré ou bordure)
- [ ] Clic sur un système dans la sidebar charge ses nodes/edges dans le canvas
- [ ] Thème dark cohérent : sidebar plus sombre que le canvas
- [ ] Le layout occupe 100vh, pas de scroll vertical sur la page

---

### Sprint 2 : Parsers & Données

> **Objectif :** Les parsers SQL et C# génèrent des fichiers JSON conformes au schéma. Le projet Charles & Nadejda a ses données réelles.

---

#### US-2.1 — Parser SQL (tables, colonnes, FK)

**En tant que** développeur, **je veux** un script Node.js/TypeScript qui parse des fichiers `.sql` (CREATE TABLE) et génère un JSON conforme au schéma FlowScope **afin d'** alimenter automatiquement la vue DB.

**Critères d'acceptation :**
- [ ] Script `src/parsers/sql-parser.ts` exécutable via `npx tsx src/parsers/sql-parser.ts <chemin-dossier-sql>`
- [ ] Parse les instructions `CREATE TABLE` :
  - Extrait le nom de table → devient un `FlowNode` de type `"table"`
  - Extrait les colonnes → stockées dans `metadata.columns` (array de strings "nom: TYPE")
  - Extrait les clés primaires → stockées dans `metadata.primaryKeys`
  - Extrait les `FOREIGN KEY ... REFERENCES` → génère des `FlowEdge` de type `"fk"`
- [ ] Gère les fichiers contenant plusieurs `CREATE TABLE`
- [ ] Ignore les `INSERT INTO`, `ALTER TABLE`, commentaires SQL
- [ ] Génère un fichier `output/db-system.json` conforme au schéma `SystemDefinition`
- [ ] Testé sur les migrations v01–v13 de Charles & Nadejda (~50 tables)
- [ ] Log en console : nombre de tables parsées, nombre de FK détectées

**DoD spécifique :**
- [ ] Le JSON généré se charge dans FlowCanvas sans erreur
- [ ] Toutes les FK sont des edges visibles entre les bonnes tables

---

#### US-2.2 — Parser C# (Forms, DAL, Models)

**En tant que** développeur, **je veux** un script qui parse les fichiers `.cs` et génère un JSON identifiant les Forms, classes DAL et Models avec leurs relations **afin d'** alimenter la vue C#.

**Critères d'acceptation :**
- [ ] Script `src/parsers/csharp-parser.ts` exécutable via `npx tsx src/parsers/csharp-parser.ts <chemin-dossier-cs>`
- [ ] Détecte et classifie les classes :
  - Fichier dans `Forms/` ou classe héritant de `Form` → type `"form"`
  - Fichier dans `DAL/` ou nom finissant par `DAL` → type `"dal"`
  - Fichier dans `Models/` → type `"model"`
- [ ] Pour chaque classe extraite :
  - `label` = nom de la classe
  - `filePath` = chemin relatif du fichier source
  - `metadata.methods` = liste des méthodes publiques (signature simplifiée)
  - `metadata.properties` = liste des propriétés publiques
- [ ] Détecte les relations :
  - Héritage (`class X : Y`) → edge type `"inheritance"`
  - Instanciation (`new XxxDAL()`) dans un Form → edge type `"dependency"`
  - Paramètre ou propriété de type Model dans DAL → edge type `"dependency"`
- [ ] Génère un fichier `output/csharp-system.json` conforme au schéma
- [ ] Testé sur les ~40 classes utiles (sur ~80 fichiers C#) de Charles & Nadejda
- [ ] Log en console : nombre de classes parsées par type

**DoD spécifique :**
- [ ] Le JSON généré se charge dans FlowCanvas sans erreur
- [ ] Les héritages et dépendances sont visibles comme edges

---

#### US-2.3 — Assembleur de projet (merge des systèmes)

**En tant que** développeur, **je veux** un script qui assemble les JSON générés par les parsers en un seul fichier `FlowScopeProject` **afin d'** avoir un point d'entrée unique pour l'application.

**Critères d'acceptation :**
- [ ] Script `src/parsers/assemble.ts` exécutable via `npx tsx src/parsers/assemble.ts`
- [ ] Lit tous les fichiers `output/*-system.json`
- [ ] Génère `src/data/project.json` conforme à `FlowScopeProject`
- [ ] Renseigne `name`, `version` (depuis `package.json`), `lastParsed` (timestamp ISO)
- [ ] Le fichier `project.json` est chargeable directement par l'application
- [ ] Si un fichier `output/overrides.json` existe, ses valeurs sont mergées (deep merge) sur les nodes/edges correspondants (par `id`)
- [ ] L'override permet de modifier : `description`, `position`, `tags`, `group`

---

#### US-2.4 — Données Charles & Nadejda générées

**En tant que** utilisateur, **je veux** que le projet Charles & Nadejda soit parsé et ses données disponibles dans FlowScope **afin de** visualiser l'architecture réelle du projet.

**Critères d'acceptation :**
- [ ] Le parser SQL a été exécuté sur les migrations v01–v13 → système "Base de données MySQL"
- [ ] Le parser C# a été exécuté sur le dossier C# → système "Application C# WinForms"
- [ ] Un système "Pipeline BOM" est créé manuellement (JSON) avec les 7 nodes : Ingrédients, Contextes, Niveaux, Fiches Recettes, Productions, Stocks, Réservations — et les edges du flux de production
- [ ] Un système "Overview" est créé manuellement avec 4 blocs : DB, C# App, BOM Pipeline, Laravel (placeholder)
- [ ] Le fichier `project.json` assemblé contient au minimum 4 systèmes
- [ ] L'application charge et affiche ces données sans erreur

---

### Sprint 3 : Vues & Navigation

> **Objectif :** Les vues détaillées sont navigables depuis l'Overview. Le breadcrumb permet de remonter.

---

#### US-3.1 — Vue Overview (graphe haut niveau)

**En tant que** utilisateur, **je veux** une vue d'ensemble affichant les grands systèmes du projet comme des blocs cliquables **afin d'** avoir une carte mentale de l'architecture.

**Critères d'acceptation :**
- [ ] Vue par défaut au chargement de l'application
- [ ] Chaque système du JSON est affiché comme un gros node (type "overview") avec :
  - Icône du système
  - Nom du système
  - Nombre de nodes internes (ex: "50 tables", "42 classes")
  - Description courte
- [ ] Les nodes sont disposés avec un auto-layout lisible (pas d'empilement)
- [ ] Double-clic ou clic + entrée sur un bloc → navigation vers la vue détaillée de ce système
- [ ] Les blocs sont visuellement plus grands que les nodes normaux (min 200x120px)
- [ ] Pas d'edges entre les blocs Overview (les relations sont dans les vues détaillées)

---

#### US-3.2 — Navigation entre vues (breadcrumb)

**En tant que** utilisateur, **je veux** un fil d'Ariane (breadcrumb) en haut du canvas qui indique ma position et me permette de remonter **afin de** ne jamais me perdre dans la navigation.

**Critères d'acceptation :**
- [ ] Breadcrumb affiché en haut du canvas, au-dessus du graphe
- [ ] Format : `Overview > [Nom du système]`
- [ ] Clic sur "Overview" dans le breadcrumb → retour à la vue Overview
- [ ] La sidebar reflète la vue active (élément sélectionné)
- [ ] Changement de vue via sidebar met à jour le breadcrumb
- [ ] Transition fluide entre vues (pas de flash blanc)
- [ ] L'URL (ou un state) reflète la vue active (pour F5 sans perdre la position — nice to have)

---

#### US-3.3 — Vue DB détaillée

**En tant que** utilisateur, **je veux** voir toutes les tables de la base de données avec leurs colonnes et FK sous forme de graphe **afin de** comprendre le schéma relationnel.

**Critères d'acceptation :**
- [ ] Charge les données du système "Base de données MySQL"
- [ ] Chaque table est un node affichant :
  - Icône table (Lucide `Database` ou `Table2`)
  - Nom de la table
  - Liste des colonnes (scrollable si > 8, ou tronquée avec "... +N")
  - PK (Primary Key) marquée avec icône clé
  - Colonnes FK marquées visuellement (ex: icône lien)
- [ ] Les edges FK relient les tables avec le label du champ FK
- [ ] Les edges sont de type `"fk"` et ont une couleur distincte
- [ ] Auto-layout appliqué (Dagre ou ELK) pour éviter les chevauchements
- [ ] Les tables sont regroupables visuellement par domaine si `group` est renseigné (ex: "BOM", "Catalogue", "Auth")

---

#### US-3.4 — Vue C# détaillée

**En tant que** utilisateur, **je veux** voir les classes C# (Forms, DAL, Models) avec leurs héritages et dépendances **afin de** comprendre l'architecture logicielle.

**Critères d'acceptation :**
- [ ] Charge les données du système "Application C# WinForms"
- [ ] Chaque classe est un node typé (form/dal/model) avec :
  - Icône correspondant au type
  - Nom de la classe
  - Nombre de méthodes et propriétés
- [ ] Les edges d'héritage sont en pointillés avec flèche ouverte (UML-style)
- [ ] Les edges de dépendance sont en trait plein avec flèche fermée
- [ ] Les classes sont regroupées visuellement par type (Forms ensemble, DAL ensemble, Models ensemble)
- [ ] Auto-layout respectant les groupes

---

#### US-3.5 — Vue BOM Pipeline

**En tant que** utilisateur, **je veux** voir le pipeline de production BOM sous forme de flux directionnel **afin de** comprendre le chemin Ingrédients → Niveaux, Contextes → Niveaux, Productions → Stocks, Stocks → Réservations.

**Critères d'acceptation :**
- [ ] Charge les données du système "Pipeline BOM"
- [ ] Les nodes sont disposés de gauche à droite (flux directionnel)
- [ ] Les edges sont animés (flow) pour indiquer le sens du flux
- [ ] Chaque node du pipeline affiche :
  - Icône métier (ex: paquet pour ingrédients, engrenage pour production, boîte pour stock)
  - Nom de l'étape
  - Description courte du rôle
- [ ] Le layout est horizontal (LR) avec auto-layout Dagre direction "LR"

---

### Sprint 4 : Inspecteur & Recherche

> **Objectif :** L'utilisateur peut inspecter un node en détail et chercher n'importe quel élément.

---

#### US-4.1 — Panneau inspecteur latéral

**En tant que** utilisateur, **je veux** qu'un panneau latéral s'ouvre à droite quand je clique sur un node **afin de** voir ses propriétés détaillées sans quitter le graphe.

**Critères d'acceptation :**
- [ ] Clic sur un node → panneau inspecteur s'ouvre à droite (320px de large)
- [ ] Le panneau affiche :
  - Nom du node (titre)
  - Badge du type (ex: "TABLE", "FORM")
  - Description (si renseignée)
  - Chemin du fichier source (si renseigné), avec bouton "copier le chemin"
  - Section "Métadonnées" : toutes les clés de `metadata` affichées en listes
  - Section "Tags" : badges pour chaque tag
  - Section "Connexions" : liste des nodes connectés (entrants + sortants) avec le label de l'edge
- [ ] Clic sur un node connecté dans la section "Connexions" → sélectionne et centre ce node dans le canvas
- [ ] Clic en dehors du node ou bouton "×" ou touche Escape → ferme le panneau et désélectionne le node
- [ ] Changement de vue ferme automatiquement le panneau inspecteur et désélectionne le node
- [ ] Le panneau est scrollable si le contenu dépasse la hauteur
- [ ] Transition d'ouverture/fermeture animée (slide-in 250ms)
- [ ] Le canvas se redimensionne pour laisser place au panneau (pas de superposition)

---

#### US-4.2 — Recherche globale (fuzzy search)

**En tant que** utilisateur, **je veux** rechercher un node par nom dans toutes les vues **afin de** trouver rapidement un élément sans naviguer manuellement.

**Critères d'acceptation :**
- [ ] Raccourci `Ctrl+K` ou `Cmd+K` ouvre une modale de recherche (command palette style)
- [ ] Champ de recherche avec placeholder "Rechercher un node..."
- [ ] La recherche utilise Fuse.js en fuzzy sur les champs `label`, `description`, `tags`
- [ ] Les résultats affichent : icône du type, label du node, nom du système parent
- [ ] Maximum 10 résultats affichés, triés par pertinence
- [ ] Sélection d'un résultat (clic ou Entrée) :
  - Navigue vers la vue du système contenant le node
  - Centre et sélectionne le node dans le canvas
  - Ouvre le panneau inspecteur
- [ ] `Escape` ferme la modale
- [ ] La recherche est instantanée (< 100ms pour ~100 nodes)
- [ ] Un bouton de recherche (icône loupe) est aussi disponible dans la sidebar

---

#### US-4.3 — Raccourcis clavier

**En tant que** utilisateur, **je veux** des raccourcis clavier pour les actions fréquentes **afin de** naviguer plus rapidement.

**Critères d'acceptation :**
- [ ] `Ctrl+K` / `Cmd+K` : ouvre la recherche
- [ ] `Escape` : ferme le panneau inspecteur ou la modale de recherche
- [ ] `1` à `9` : navigation rapide vers la vue correspondante (1 = Overview, 2 = DB, etc.)
- [ ] `Backspace` ou `Alt+←` : retour à la vue précédente (breadcrumb arrière)
- [ ] `F` : fit-to-view (zoom pour voir tous les nodes)
- [ ] `?` : affiche un overlay avec la liste des raccourcis
- [ ] Les raccourcis ne se déclenchent pas quand un champ de texte est focus

---

### Sprint 5 : Polish & Intégration

> **Objectif :** L'application est visuellement finie, performante et utilisable sur le projet Charles & Nadejda.

---

#### US-5.1 — Thème dark cohérent

**En tant que** utilisateur, **je veux** un thème dark professionnel et cohérent sur toute l'application **afin d'** avoir un confort visuel optimal pour un outil de développement.

**Critères d'acceptation :**
- [ ] Palette dark définie dans `tailwind.config.ts` (tokens) :
  - Background principal : ~`#0d1117` (style GitHub dark)
  - Surface (sidebar, panels) : ~`#161b22`
  - Bordures : ~`#30363d`
  - Texte principal : ~`#e6edf3`
  - Texte secondaire : ~`#8b949e`
  - Accent : ~`#58a6ff` (bleu) pour les interactions
- [ ] Tous les composants utilisent les tokens (pas de couleurs en dur)
- [ ] Contraste WCAG AA respecté sur tout le texte
- [ ] Les nodes React Flow utilisent la palette avec les couleurs par type (US-1.4)
- [ ] Les edges ont un style cohérent (couleur semi-transparente, épaisseur 1.5-2px)
- [ ] Transitions hover sur tous les éléments interactifs

---

#### US-5.2 — Auto-layout des graphes

**En tant que** utilisateur, **je veux** que les graphes soient disposés automatiquement de manière lisible **afin de** ne pas avoir à repositionner manuellement les nodes.

**Critères d'acceptation :**
- [ ] Intégration de Dagre (`@dagrejs/dagre`) ou ELK pour le calcul de layout
- [ ] Layout appliqué automatiquement au chargement d'une vue
- [ ] Direction configurable par système : TB (Top-Bottom) pour DB/C#, LR (Left-Right) pour BOM
- [ ] Espacement suffisant entre nodes (pas de chevauchement de labels)
- [ ] Si des positions sont sauvegardées dans le JSON (`position` sur les nodes), elles sont utilisées à la place de l'auto-layout
- [ ] Bouton "Réorganiser" dans la toolbar du canvas pour réappliquer le layout
- [ ] Le layout est recalculé en < 500ms pour ~50 nodes

---

#### US-5.3 — Toolbar du canvas

**En tant que** utilisateur, **je veux** une barre d'outils en haut du canvas avec les actions utiles **afin d'** accéder rapidement aux fonctions de navigation.

**Critères d'acceptation :**
- [ ] Barre d'outils positionnée en haut à droite du canvas (overlay, ne prend pas de place)
- [ ] Boutons (icônes Lucide) :
  - Zoom in / Zoom out / Fit view
  - Réorganiser le layout (US-5.2)
  - Recherche (ouvre la modale Ctrl+K)
- [ ] Tooltips sur chaque bouton
- [ ] Style cohérent avec le thème dark (fond semi-transparent, bordures subtiles)
- [ ] Les boutons sont accessibles au clavier (tab order logique)

---

#### US-5.4 — Edges typés visuellement

**En tant que** utilisateur, **je veux** que les edges soient visuellement distincts selon leur type **afin de** comprendre la nature des relations sans ouvrir l'inspecteur.

**Critères d'acceptation :**
- [ ] Chaque type d'edge a un style visuel distinct :
  - `"fk"` : trait plein bleu, flèche fermée, label visible
  - `"inheritance"` : trait pointillé violet, flèche ouverte (triangle vide)
  - `"dependency"` : trait plein gris, flèche fermée
  - `"flow"` : trait plein vert, animé (pointillés mouvants), flèche fermée
  - `"data"` : trait ondulé ou pointillé orange
- [ ] Les edges sélectionnés (au survol ou au clic) sont mis en évidence (épaisseur + opacité accrue)
- [ ] Le label de l'edge est lisible (fond semi-transparent derrière le texte si nécessaire)

---

#### US-5.5 — Intégration finale Charles & Nadejda

**En tant que** utilisateur, **je veux** que FlowScope affiche correctement l'intégralité du projet Charles & Nadejda **afin de** valider l'outil sur un cas réel.

**Critères d'acceptation :**
- [ ] Vue Overview : 4 blocs (DB, C# App, BOM Pipeline, Laravel placeholder)
- [ ] Vue DB : les ~50 tables avec toutes leurs FK visibles et correctes
- [ ] Vue C# : les ~40 classes utiles (sur ~80 fichiers C#) avec héritages (`FrmListeBase<T>`, `FrmEditBase`) et dépendances DAL
- [ ] Vue BOM : pipeline complet Ingrédients → Niveaux → Fiches → Productions → Stocks → Réservations
- [ ] Recherche : un node trouvé dans chaque vue (test transversal)
- [ ] Inspecteur : les métadonnées sont renseignées sur au moins 10 nodes clés
- [ ] Performance : navigation fluide, pas de lag perceptible au changement de vue
- [ ] Aucune erreur console

---

## Backlog v2+ (non détaillé)

Les fonctionnalités suivantes sont identifiées pour les versions futures. Elles ne sont **pas** découpées en stories.

| ID | Fonctionnalité | Priorité | Notes |
|----|---------------|----------|-------|
| V2-1 | Parser Laravel (routes, controllers, views, models) | Haute | Nécessaire pour le projet Charles & Nadejda côté web |
| V2-2 | Parser générique configurable (YAML) | Moyenne | Permet d'adapter FlowScope à n'importe quel projet sans coder un parser |
| V2-3 | Export image/PDF du graphe actif | Haute | Indispensable pour les présentations et la documentation |
| V2-4 | Mode présentation (plein écran, navigation guidée) | Moyenne | Pour les soutenances et démos |
| V2-5 | Diff visuel entre deux versions du JSON | Basse | Voir ce qui a changé entre deux parsings |
| V2-6 | Filtres par type de node | Moyenne | Masquer/afficher certains types dans une vue |
| V2-7 | Annotations manuelles sur les edges | Basse | Ajouter des notes explicatives |
| V2-8 | Multi-projets (switcher de projet) | Basse | Charger plusieurs projets dans la même instance |
| V2-9 | Positions sauvegardées après drag | Moyenne | Persister le layout modifié manuellement |
| V2-10 | Vue croisée (relations entre systèmes) | Haute | Ex: quel Form utilise quelle table via quel DAL |
| V2-11 | Vue Navigation UI (arbre des NavItems du Shell) | Moyenne | Mapper les 13 NavItems vers les Forms |

---

## Décisions PO

| # | Décision | Justification |
|---|----------|---------------|
| D1 | **MVP 100% local, pas de backend** | Simplicité maximale. JSON + React = déployable en `npm run dev`. Pas de serveur à gérer. |
| D2 | **Parsers en TypeScript (pas Python)** | Stack unifiée. Un seul langage pour tout le projet. Exécution via `npx tsx`. |
| D3 | **React Flow comme moteur de graphe** | Librairie mature, TypeScript-native, API riche (custom nodes, minimap, controls). Alternative : d3.js (trop bas niveau), Mermaid (pas interactif). |
| D4 | **Schéma JSON universel comme pivot** | Découple les parsers de l'UI. Permet d'ajouter des parsers sans toucher au frontend. Format ouvert et lisible. |
| D5 | **5 sprints MVP, pas de sprint 0** | Le brainstorm fait office de phase de cadrage. Sprint 1 = fondations + premier rendu visible. Feedback rapide. |
| D6 | **Auto-layout par défaut, positions overridables** | L'utilisateur ne devrait pas avoir à positionner manuellement. Mais les parsers ne peuvent pas deviner un layout "métier". Le JSON d'override résout ce problème. |
| D7 | **Pas de tests unitaires automatisés dans le MVP** | Développeur solo, projet outil interne. Les critères d'acceptation sont validés manuellement. Les tests automatisés arriveront si le projet grandit. |
| D8 | **Fuse.js pour la recherche** | Léger (~5kb), fuzzy search native, pas de backend nécessaire. Suffisant pour < 500 nodes. |
| D9 | **Un override global (pas un par système)** | Simplifie la gestion. L'ID du node est unique projet-wide grâce au préfixe système. |
| D10 | **Vue BOM créée manuellement (pas parsée)** | Le pipeline BOM est un concept métier, pas une structure de code. Les tables DB sont parsées, mais le flux logique est décrit à la main. |

---

## Glossaire

| Terme | Définition |
|-------|-----------|
| **BOM** | Bill of Materials — Nomenclature de production. Structure hiérarchique décrivant les composants nécessaires à la fabrication d'un produit. |
| **Breadcrumb** | Fil d'Ariane — élément de navigation affichant le chemin parcouru (ex: Overview > DB). |
| **Canvas** | Zone principale de dessin où le graphe React Flow est rendu. |
| **DAL** | Data Access Layer — Couche d'accès aux données. Classes C# qui encapsulent les requêtes SQL. |
| **Dagre** | Bibliothèque JavaScript de layout de graphes dirigés. Calcule les positions optimales des nodes. |
| **Edge** | Arête/lien entre deux nodes dans un graphe. Représente une relation (FK, héritage, dépendance, flux). |
| **ELK** | Eclipse Layout Kernel — Moteur de layout de graphes alternatif à Dagre, plus puissant pour les grands graphes. |
| **FK** | Foreign Key — Clé étrangère. Contrainte SQL liant une colonne à la clé primaire d'une autre table. |
| **Fuse.js** | Bibliothèque JavaScript de recherche fuzzy (approximative) côté client. |
| **Fuzzy search** | Recherche approximative tolérant les fautes de frappe (ex: "produt" trouve "produit"). |
| **Node** | Nœud dans un graphe. Représente un élément du projet (table, classe, étape de pipeline). |
| **Override** | Fichier JSON permettant d'enrichir ou corriger les données générées par les parsers. |
| **Parser** | Script qui analyse du code source et en extrait une structure de données (JSON). |
| **PK** | Primary Key — Clé primaire. Identifiant unique d'un enregistrement dans une table SQL. |
| **React Flow** | Bibliothèque React pour créer des graphes interactifs avec des nodes et edges personnalisables. |
| **Shell** | Structure de base de l'interface (sidebar + content area + status bar). |
| **Système** | Dans FlowScope, un "système" est un sous-graphe cohérent du projet (ex: "DB", "C# App", "BOM"). |
| **WCAG AA** | Web Content Accessibility Guidelines niveau AA — standard d'accessibilité web (contraste, navigation clavier). |
