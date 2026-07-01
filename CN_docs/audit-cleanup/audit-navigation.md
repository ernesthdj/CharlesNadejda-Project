# Audit Navigation — ArtisaStock
> Date: 2026-06-11 | Agent: Navigation Auditor (#4)

## Fichiers audites

| Fichier | Lignes |
|---------|--------|
| `Navigation/ScreenId.cs` | 21 |
| `Navigation/NavItemId.cs` | 33 |
| `Navigation/NavigationParams.cs` | 12 |
| `Navigation/RessourceType.cs` | 11 |
| `Navigation/AppState.cs` | 65 |
| `Navigation/ScreenRouter.cs` | 111 |
| `Forms/FrmPrincipal.cs` | ~800 (partiel, partial class) |
| `Forms/Shell/SidebarPanel.cs` | 669 |

---

## Resume

**11 findings** (1 critique, 5 important, 5 mineur)

| Severite | Count |
|----------|-------|
| CRITIQUE | 1 |
| IMPORTANT | 5 |
| MINEUR | 5 |

---

## Findings

### [F-NAV-001] ScreenId.ContexteNiveaux est un alias mort — deux callbacks pour un seul ecran
- **Severite:** IMPORTANT
- **Fichier(s):** `Navigation/ScreenRouter.cs:22,96,98` · `Navigation/ScreenId.cs:7`
- **Type:** Redondance | Dead code
- **Description:** `ScreenRouter` declare un callback `OnContexteNiveaux` (ligne 22) qui n'est jamais invoque directement par le switch — `case ScreenId.ContexteNiveaux` (ligne 96) appelle `OnProduction`, pas `OnContexteNiveaux`. Pourtant, dans `FrmPrincipal.InitRouter()` (ligne 112), `OnContexteNiveaux` est cable vers `ShowProductionScreen`. Ce callback est donc assigne mais jamais utilise par le routeur lui-meme. Le `ScreenId.ContexteNiveaux` existe comme valeur d'enum mais le routeur le redirige hardcode vers `OnProduction`.
- **Impact:** Confusion pour tout developpeur qui lirait le code : on pense que `OnContexteNiveaux` est utilise, mais c'est `OnProduction` qui est appele. Si quelqu'un branche une logique differente sur `OnContexteNiveaux`, elle ne sera jamais executee.
- **Suggestion:** Supprimer le callback `OnContexteNiveaux` de `ScreenRouter`. Deprecier ou supprimer `ScreenId.ContexteNiveaux` en faveur de `ScreenId.Production`. Mettre un commentaire `[Obsolete]` si la retrocompat l'exige.

---

### [F-NAV-002] NavItemId.FichesBom declare mais absent de la sidebar
- **Severite:** IMPORTANT
- **Fichier(s):** `Navigation/NavItemId.cs:24` · `Forms/Shell/SidebarPanel.cs:423-450`
- **Type:** Orphelin
- **Description:** L'enum `NavItemId.FichesBom` est declare (ligne 24) et gere dans le switch de `FrmPrincipal.OnSidebarNavigation()` (ligne 225 — redirige vers `ScreenId.Production`), mais aucun item de navigation n'est cree pour lui dans `SidebarPanel.BuildNavSections()`. Aucun bouton ni lien ne genere jamais un `NavigationRequested(NavItemId.FichesBom)`.
- **Impact:** Code mort : le case dans le switch ne sera jamais atteint. Alourdit l'enum et le switch sans benefice.
- **Suggestion:** Supprimer `NavItemId.FichesBom` de l'enum et le case correspondant dans `OnSidebarNavigation`. Si c'est un placeholder futur, l'annoter avec un commentaire `// Phase 2+`.

---

### [F-NAV-003] NavItemId.NiveauxContextes declare mais absent de la sidebar
- **Severite:** MINEUR
- **Fichier(s):** `Navigation/NavItemId.cs:26` · `Forms/Shell/SidebarPanel.cs:423-450`
- **Type:** Orphelin
- **Description:** Meme probleme que F-NAV-002. `NavItemId.NiveauxContextes` est dans l'enum et gere dans le switch (ligne 222), mais aucun item de sidebar ne le reference. `BuildNavSections()` n'appelle jamais `AddNavItem(y, NavItemId.NiveauxContextes, ...)`.
- **Impact:** Code mort dans le switch. Moins grave car l'enum pourrait servir de reference interne, mais reste du bruit.
- **Suggestion:** Supprimer ou commenter en `// Phase 2+` pour signaler que c'est intentionnel.

---

### [F-NAV-004] NavigationParams — 3 champs sur 5 jamais lus
- **Severite:** IMPORTANT
- **Fichier(s):** `Navigation/NavigationParams.cs:5-8`
- **Type:** Dead code
- **Description:** La classe `NavigationParams` declare 5 proprietes. L'audit du codebase montre :
  - `ScrollToId` — **jamais lu** nulle part. Declare ligne 5, aucune reference en lecture.
  - `Entity` — **jamais lu** nulle part. Declare ligne 6, aucune reference en lecture.
  - `IsEdit` — **jamais lu** nulle part. Declare ligne 7, aucune reference en lecture.
  - `RessourceType` — **jamais lu** nulle part. Declare ligne 8, aucune reference en lecture. Le type de ressource est passe via `AppState.RessourceActive`, pas via les params.
  - `FiltreAlertesSeulement` — **jamais lu via les params** non plus. Le flag est lu depuis `AppState.FiltreAlertesSeulement` dans `ShowRessourceScreen` (FrmPrincipal.cs:582), pas depuis les params.

  En pratique, `NavigationParams` est instancie (ligne 88 de ScreenRouter.cs) mais **aucune de ses proprietes n'est jamais lue par aucun ecran**.
- **Impact:** Classe quasi-inutile. Chaque callback recoit un `NavigationParams p` qu'il ignore. Fausse complexite pour les nouveaux developpeurs.
- **Suggestion:** Evaluer si `NavigationParams` a un futur reel. Si non, supprimer la classe et changer les callbacks en `Action` (sans parametre). Si oui, nettoyer les champs inutilises et ne garder que ceux effectivement prevus.

---

### [F-NAV-005] Guard anti-re-render absent pour BoutiqueWeb et Parametres
- **Severite:** MINEUR
- **Fichier(s):** `Navigation/ScreenRouter.cs:56-107`
- **Type:** Pattern manque
- **Description:** Le guard anti-re-render dans `Navigate()` gere explicitement Hub (ligne 75 — `return`), ContexteNiveaux/Production (lignes 64-66 — comparaison contexte), et Ressources (lignes 70-72 — comparaison type). Mais `BoutiqueWeb` et `Parametres` n'ont aucun guard specifique. Si l'utilisateur reclique sur "Boutique web" ou "Parametres" alors qu'il est deja dessus, l'ecran sera reconstruit inutilement (appel `ClearAndDisposePanel` + reconstruction complete).
- **Impact:** Re-rendu inutile et potentiel flash visuel. Impact faible car ces ecrans sont legers, mais rompt la coherence du pattern.
- **Suggestion:** Ajouter ces deux ScreenId dans le guard `if (_lastScreen == screen) return;` comme pour Hub, ou generaliser le guard pour tous les ecrans sans etat contextuel.

---

### [F-NAV-006] Placeholder screens (Planning, DevisPatisserie, Mouvements) — guard manquant aussi
- **Severite:** MINEUR
- **Fichier(s):** `Navigation/ScreenRouter.cs:104-106`
- **Type:** Pattern manque
- **Description:** Les trois placeholders tombent tous sur `OnPlaceholder` mais ne sont pas proteges par le guard. Recliquer "Planning" reconstruit le placeholder a chaque fois.
- **Impact:** Negligeable vu que c'est un ecran statique "coming soon". Mais si un placeholder evolue vers un vrai ecran sans ajouter de guard, le probleme deviendra reel.
- **Suggestion:** Meme correction que F-NAV-005 — proteger les ecrans statiques.

---

### [F-NAV-007] RessourceType — tous les types sont utilises, mapping coherent
- **Severite:** (aucun finding negatif)
- **Fichier(s):** `Navigation/RessourceType.cs` · `Forms/FrmPrincipal.cs:586-594`
- **Type:** Verification OK
- **Description:** Les 5 valeurs de `RessourceType` (`Stocks`, `Ingredients`, `Fournisseurs`, `Achats`, `VueStock`) sont toutes utilisees dans le switch de `ShowRessourceScreen` et chacune mappe vers un Form distinct. Aucun orphelin, aucun type manquant.
- **Impact:** Aucun.
- **Suggestion:** RAS — mapping propre.

---

### [F-NAV-008] AppState.StateChanged — zero abonne dans le code applicatif
- **Severite:** CRITIQUE
- **Fichier(s):** `Navigation/AppState.cs:21,62`
- **Type:** Dead code | Pattern manque
- **Description:** L'event `StateChanged` est declare (ligne 21) et fire par `RaiseChanged()` (ligne 62) a chaque appel de `SetActivite`, `SetContexte`, `SetNiveau`, `SetRessource`. Cependant, **aucun formulaire ne s'abonne a cet event** dans tout le codebase C#. La recherche de `StateChanged +=` ne retourne aucun resultat dans le code applicatif (uniquement des references dans la doc et les packages MySql).

  La navigation est entierement pilotee par les appels directs a `NavigateTo()` dans `FrmPrincipal`, pas par des reactions a `StateChanged`. Ce qui signifie que l'event est fire dans le vide a chaque changement d'etat.
- **Impact:**
  1. **Event fire inutilement** a chaque mutation d'etat — pas de bug car `?.Invoke` protege, mais overhead semantique.
  2. **Risque futur** : un developpeur pourrait s'abonner a `StateChanged` en pensant que c'est le pattern etabli, alors que la navigation reelle passe par un autre canal.
  3. **Pas de probleme de leak** puisque personne ne s'abonne — mais l'event est un contrat non tenu.
- **Suggestion:** Soit retirer l'event `StateChanged` si la navigation reste pilotee par `NavigateTo()`, soit l'utiliser reellement (par ex. pour la StatusBar ou la TitleBar qui pourraient reagir automatiquement aux changements d'etat au lieu d'etre mises a jour manuellement).

---

### [F-NAV-009] FiltreAlertesSeulement — dualite AppState / NavigationParams
- **Severite:** IMPORTANT
- **Fichier(s):** `Navigation/AppState.cs:19,56-58` · `Navigation/NavigationParams.cs:10` · `Forms/FrmPrincipal.cs:582-583`
- **Type:** Incoherence
- **Description:** Le flag `FiltreAlertesSeulement` existe a **deux endroits** :
  1. `AppState.FiltreAlertesSeulement` (ligne 19) — c'est celui qui est effectivement lu dans `ShowRessourceScreen` (FrmPrincipal.cs:582).
  2. `NavigationParams.FiltreAlertesSeulement` (ligne 10) — jamais lu nulle part.

  Le flag est positionne via `_state.SetFiltreAlertes(true)` depuis le Hub (FrmPrincipal.Hub.cs), lu dans `ShowRessourceScreen`, puis reset a `false`. Le champ dans `NavigationParams` est un vestige inutilise.
- **Impact:** Confusion sur quel objet porte l'etat du filtre. Si un developpeur passe le flag via `NavigationParams`, il sera ignore.
- **Suggestion:** Supprimer `NavigationParams.FiltreAlertesSeulement`. Le flag vit dans `AppState` et c'est le seul chemin utilise.

---

### [F-NAV-010] UpdateTitleBar — BoutiqueWeb absent du dictionnaire de titres
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmPrincipal.cs:358-372`
- **Type:** Incoherence
- **Description:** Le dictionnaire `titles` dans `UpdateTitleBar()` mappe chaque `ScreenId` vers un titre lisible. Cependant, `ScreenId.BoutiqueWeb` n'y figure pas. Quand l'utilisateur navigue vers la boutique, `TryGetValue` retourne `false` et le titre fallback est "ArtisaStock" (ligne 372). Ce n'est pas un bug visible mais c'est une incoherence — tous les autres ecrans ont un titre explicite.
- **Impact:** Titre generique au lieu d'un titre specifique ("Boutique en ligne" par exemple). Experience utilisateur legerement degradee.
- **Suggestion:** Ajouter `{ ScreenId.BoutiqueWeb, "Boutique en ligne" }` dans le dictionnaire.

---

### [F-NAV-011] Sidebar events — pas de desabonnement mais pas de leak non plus
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmPrincipal.cs:161-167` · `Forms/Shell/SidebarPanel.cs`
- **Type:** Verification OK (avec reserve)
- **Description:** `FrmPrincipal` s'abonne aux 7 events de `SidebarPanel` dans `BuildShell()` (lignes 161-167) mais ne se desabonne jamais. En theorie, c'est un potential event leak. En pratique, `SidebarPanel` est un enfant de `FrmPrincipal` — ils ont le meme cycle de vie. Quand `FrmPrincipal` se ferme (et c'est la fermeture de l'app), `SidebarPanel` est dispose avec lui. Aucune fuite reelle possible.

  Concernant `AppState.StateChanged` : comme identifie en F-NAV-008, personne ne s'abonne, donc pas de leak possible.
- **Impact:** Aucun impact reel. Le pattern est acceptable dans un contexte SFA ou le Form principal = la duree de vie de l'app.
- **Suggestion:** RAS pour le moment. Si un jour des ecrans s'abonnent a `StateChanged` de maniere ephemere, il faudra implementer un pattern `IDisposable` ou `-=` dans `FormClosed`.

---

## Matrice de couverture ScreenId / NavItemId

| ScreenId | NavItemId correspondant(s) | Callback Router | Sidebar item | Statut |
|----------|---------------------------|-----------------|-------------|--------|
| `Onboarding` | _(aucun — affiche auto)_ | `OnOnboarding` | Non | OK — interne |
| `Hub` | `Hub` | `OnHub` | Oui | OK |
| `ContexteNiveaux` | `NiveauxContextes` | `OnContexteNiveaux` (**jamais appele**) | **Non** | ORPHELIN — redirige vers OnProduction |
| `Ressources` | `StocksLiaisons`, `VueStockGlobal`, `AchatsLots`, `Fournisseurs`, `Ingredients` | `OnRessources` | Oui (5 items) | OK |
| `Production` | `Production`, `NiveauxContextes`*, `FichesBom`* | `OnProduction` | Oui (1 item) | OK (les 2 alias sont dead code sidebar) |
| `Planning` | `Planning` | `OnPlaceholder` | Oui | OK — placeholder |
| `DevisPatisserie` | `DevisPatisserie` | `OnPlaceholder` | Oui | OK — placeholder |
| `Mouvements` | `Mouvements` | `OnPlaceholder` | Oui | OK — placeholder |
| `Parametres` | `Parametres` | `OnParametres` | Oui | OK |
| `BoutiqueWeb` | `BoutiqueWeb` | `OnBoutiqueWeb` | Oui | OK (titre manquant F-NAV-010) |

_*NavItemId present dans l'enum + switch mais absent de la sidebar (jamais emis)._

---

## Synthese des actions recommandees

| Priorite | Action | Findings |
|----------|--------|----------|
| P0 | Decider du sort de `StateChanged` : supprimer ou utiliser reellement | F-NAV-008 |
| P1 | Supprimer `OnContexteNiveaux` du router + deprecier `ScreenId.ContexteNiveaux` | F-NAV-001 |
| P1 | Nettoyer `NavigationParams` (supprimer les 4-5 champs morts) | F-NAV-004, F-NAV-009 |
| P1 | Supprimer `NavItemId.FichesBom` et `NavItemId.NiveauxContextes` (orphelins) | F-NAV-002, F-NAV-003 |
| P2 | Homogeneiser le guard anti-re-render pour tous les ecrans | F-NAV-005, F-NAV-006 |
| P2 | Ajouter `BoutiqueWeb` dans le dictionnaire de titres | F-NAV-010 |
