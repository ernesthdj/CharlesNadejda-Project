# DEFENSE_01_INFRASTRUCTURE — Pipeline de production (etapes 1-4)

> Document de defense examen — workflow utilisateur reel, code source verifie.
> Genere le 2026-05-21 par lecture exhaustive du code C#.

---

## Vue d'ensemble du pipeline

```
[1. Stock physique] → [2. Activite] → [3. Contexte BOM] → [4. Niveaux BOM]
     (entrepot)        (atelier)       (famille produit)    (etapes transfo.)
```

La liaison M:N entre Stock et Activite (table `activites_stocks`) permet a un meme stock de servir plusieurs activites, et a une activite d'utiliser plusieurs stocks.

---

## ETAPE 1 — Creation d'un Stock physique

### Navigation

- **Point d'entree** : Sidebar > "Stock & Liaisons" (`NavItemId.StocksLiaisons`)
- **Route** : `FrmPrincipal.NavigateTo(ScreenId.Ressources)` avec `_state.SetRessource(RessourceType.Stocks)`
- **Formulaire liste** : `FrmStocks` (embedded inline dans le panel droit)
- **Formulaire edition** : `FrmStockEdit` (modal, `ShowDialog`)

### FrmStocks — Ecran principal

| Zone | Contenu |
|------|---------|
| Header (48px, chocolat fonce) | Titre "STOCKS" + hint "Contenants physiques ou logiques" |
| SplitContainer (Fill) | Panel1: DGV liste stocks / Panel2: CheckedListBox activites liees |
| Footer (52px) | Boutons: "+ Nouveau stock", "Modifier", "Supprimer" |

**Colonnes DGV** : Id (masque), Nom, Description, Date creation.

**Panel de liaison** (droite, 280px) : GroupBox "Activites liees" avec `CheckedListBox`. Cocher/decocher une activite persiste immediatement en DB via `StockDAL.LierActivite()` / `StockDAL.DelierActivite()`.

### FrmStockEdit — Formulaire creation/modification

**Classe** : `FrmStockEdit : FrmEditBase`
**Ouverture** : `new FrmStockEdit()` (creation) ou `new FrmStockEdit(stock)` (edition)

**Champs visibles** :

| Controle | Label | Type | Obligatoire | Contrainte |
|----------|-------|------|-------------|------------|
| `txtNom` | "Nom *" | TextBox (355px) | OUI | Unicite verifie |
| `txtDescription` | "Description" | TextBox multiline (355x50px) | NON | - |

**Boutons** (geres par FrmEditBase) : "Enregistrer" + "Annuler" a Y=160.

### Validation (`Valider()`)

1. `string.IsNullOrWhiteSpace(txtNom.Text)` → erreur "Le nom est obligatoire."
2. `StockDAL.NomExiste(txtNom.Text.Trim(), excludeId)` → erreur "Ce nom de stock existe deja."

### Sauvegarde (`Sauvegarder()`)

```csharp
_stock.Nom         = txtNom.Text.Trim();
_stock.Description = txtDescription.Text.Trim().NullIfEmpty();

if (_isEdit) StockDAL.Update(_stock);
else         StockDAL.Insert(_stock);
```

### SQL execute (StockDAL)

**Insert** :
```sql
INSERT INTO stocks (nom, description, actif) VALUES (@nom, @desc, 1)
```

**Update** :
```sql
UPDATE stocks SET nom = @nom, description = @desc WHERE id = @id
```

**NomExiste** :
```sql
SELECT COUNT(*) FROM stocks WHERE nom = @nom AND id <> @id
```

**Delete** (avec gardes metier) :
```sql
-- Garde 1 : fiches ingredients
SELECT COUNT(*) FROM fiches_ingredients WHERE id_stock = @id
-- Garde 2 : lots actifs
SELECT COUNT(*) FROM lots_ingredients li
INNER JOIN fiches_ingredients fi ON fi.id = li.id_fiche_ingredient
WHERE fi.id_stock = @id AND li.quantite_disponible > 0
-- Si OK :
DELETE FROM stocks WHERE id = @id
```

### Liaison M:N Stock-Activite (inline dans FrmStocks)

```sql
-- LierActivite :
INSERT IGNORE INTO activites_stocks (id_activite, id_stock) VALUES (@idActivite, @idStock)

-- DelierActivite :
DELETE FROM activites_stocks WHERE id_activite = @idActivite AND id_stock = @idStock

-- GetActivitesLiees :
SELECT id_activite FROM activites_stocks WHERE id_stock = @id
```

---

## ETAPE 2 — Creation d'une Activite artisanale

### Navigation

- **Point d'entree principal** : Sidebar > bouton "Gerer les activites" (icone engrenage)
- **Evenement** : `SidebarPanel.ManageActivitiesRequested` → `FrmPrincipal.OnManageActivities()`
- **Formulaire liste** : `FrmActivites` (modal, `ShowDialog`)
- **Formulaire edition** : `FrmActiviteEdit` (modal depuis FrmActivites)

### FrmActivites — Ecran principal

| Zone | Contenu |
|------|---------|
| Header (48px, chocolat fonce) | Titre "ACTIVITES" + hint "Chaque activite possede son propre stock" |
| DGV (Fill) | Colonnes: Id (masque), Nom, Description, Date creation |
| Footer (52px) | 5 boutons: Nouvelle activite, Modifier, Desactiver, Supprimer, Stocks lies |

**Actions disponibles** :
- **+ Nouvelle activite** → ouvre `FrmActiviteEdit(null)`
- **Modifier** → ouvre `FrmActiviteEdit(activite)`
- **Desactiver** → soft delete avec gardes (contextes actifs, ingredients actifs)
- **Supprimer** → hard delete avec cascade (FK CASCADE en DB)
- **Stocks lies** → ouvre `FrmActiviteStocks(activite)`

### FrmActiviteEdit — Formulaire creation/modification

**Classe** : `FrmActiviteEdit : FrmEditBase`

**Champs visibles** :

| Controle | Label | Type | Obligatoire | Contrainte |
|----------|-------|------|-------------|------------|
| `txtNom` | "Nom *" | TextBox (355px, MaxLength=100) | OUI | Unicite verifie |
| `txtDescription` | "Description" | TextBox multiline + scrollbar (355x64px) | NON | - |

### Validation (`Valider()`)

1. `string.IsNullOrEmpty(nom)` → erreur "Le nom est obligatoire."
2. `ActiviteDAL.NomExiste(nom, excludeId)` → erreur "Une activite avec ce nom existe deja."

### Sauvegarde (`Sauvegarder()`)

```csharp
_activite.Nom         = txtNom.Text.Trim();
_activite.Description = txtDescription.Text.Trim().NullIfEmpty();

if (_isEdit) ActiviteDAL.Update(_activite);
else         ActiviteDAL.Insert(_activite);
```

### SQL execute (ActiviteDAL)

**Insert** :
```sql
INSERT INTO activites (nom, description, actif) VALUES (@nom, @desc, 1)
```

**Update** :
```sql
UPDATE activites SET nom=@nom, description=@desc WHERE id=@id
```

**NomExiste** :
```sql
SELECT COUNT(*) FROM activites WHERE nom = @nom AND id <> @id
```

**Desactiver** (transaction) :
```sql
-- Garde 1 : contextes actifs rattaches
SELECT COUNT(*) FROM bom_contextes WHERE id_activite = @id AND actif = 1
-- Garde 2 : ingredients actifs lies via stocks
SELECT COUNT(*) FROM fiches_ingredients fi
JOIN stocks s ON s.id = fi.id_stock
JOIN activites_stocks ast ON ast.id_stock = s.id
WHERE ast.id_activite = @id AND fi.actif = 1
-- Si OK :
UPDATE activites SET actif = 0 WHERE id = @id
```

**Delete** (cascade geree par FK en DB) :
```sql
DELETE FROM activites WHERE id = @id
```

### FrmActiviteStocks — Liaison M:N (alternative)

**Classe** : `FrmActiviteStocks` (formulaire modal standalone)
**Ouverture** : Bouton "Stocks lies" dans FrmActivites

**UI** : CheckedListBox de tous les stocks. Les stocks deja lies apparaissent coches.
**Sauvegarde** : Synchronisation diff au clic "Enregistrer" — pour chaque stock, compare etat coche vs etat DB et appelle `StockDAL.LierActivite()` ou `StockDAL.DelierActivite()`.

> **Note** : La liaison M:N est geree a la fois depuis FrmStocks (panel droit, persistance immediate) et depuis FrmActiviteStocks (batch au clic Enregistrer). Deux points d'entree, meme resultat.

---

## ETAPE 3 — Creation d'un Contexte de production (BOM)

### Navigation

- **Point d'entree 1** : Sidebar > bouton "+ Nouveau contexte" → `SidebarPanel.NewContextRequested` → `FrmPrincipal.BtnNouveauContexte_Click()`
- **Point d'entree 2** : Hub screen > bouton dans la zone centrale (si aucun contexte existant)
- **Formulaire** : `FrmBomContexteEdit` (modal, `ShowDialog`)

**Pre-requis** : une activite doit etre selectionnee dans la sidebar. Le formulaire force automatiquement l'activite courante (`activiteForce` non null → combobox desactivee).

### FrmBomContexteEdit — Formulaire creation/modification

**Classe** : `FrmBomContexteEdit : FrmEditBase`

**Champs visibles (mode creation)** :

| Controle | Label | Type | Obligatoire |
|----------|-------|------|-------------|
| `txtNom` | "Nom du contexte *" | TextBox (340px) | OUI |
| `cboActivite` | "Activite *" | ComboBox DropDownList (200px) | OUI |
| `txtDescription` | "Description" | TextBox multiline (340x60px) | NON |

**Section supplementaire en mode CREATION uniquement** — "NIVEAUX DE TRANSFORMATION" :

| Zone | Contenu |
|------|---------|
| Panel N1 (fond bleu clair) | Label "N1 - Niveau de base (automatique)" + TextBox pre-rempli "Ingredients" |
| ListBox niveaux sup. | Affiche "N2 . NomNiveau", "N3 . NomNiveau"... |
| Bouton "+" | Ouvre un mini-dialog pour nommer le niveau suivant |
| Bouton "-" | Supprime le dernier niveau ajoute |
| Hint | "Ex: Recettes, Assemblages, Finitions..." |

**En mode EDITION** : seuls Nom, Activite et Description sont visibles (pas de section niveaux). La hauteur du formulaire est 244+56=300px en edition vs 492+56=548px en creation.

### Validation (`Valider()`)

1. `string.IsNullOrWhiteSpace(txtNom.Text)` → "Le nom est obligatoire."
2. `cboActivite.SelectedItem == null` → "L'activite est obligatoire."
3. `BomContexteDAL.NomExiste(nom, activite.Id, excludeId)` → "Un contexte avec ce nom existe deja pour cette activite."

> **Regle metier** : l'unicite du nom est **scopee par activite** (deux activites differentes peuvent avoir un contexte du meme nom).

### Sauvegarde (`Sauvegarder()`)

```csharp
_contexte.Nom         = txtNom.Text.Trim();
_contexte.Description = txtDescription.Text.Trim().NullIfEmpty();
_contexte.IdActivite  = activite.Id;

if (_isEdit)
{
    BomContexteDAL.Update(_contexte);
}
else
{
    // Construit la liste des noms de niveaux
    string nomN1 = _txtNomN1.Text.Trim();  // defaut "Ingredients"
    var tousNoms = new List<string> { nomN1 };
    tousNoms.AddRange(_nomsNiveauxSupp);   // N2, N3... ajoutes par l'utilisateur
    BomContexteDAL.InsertAvecNiveaux(_contexte, tousNoms);
}
```

### SQL execute (BomContexteDAL)

**InsertAvecNiveaux** (transaction atomique) :
```sql
-- 1. Insert le contexte
INSERT INTO bom_contextes (nom, description, id_activite, actif) VALUES (@nom, @desc, @idActivite, 1)

-- 2. Pour chaque niveau (i = 0, 1, 2...) :
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description) VALUES (@idCtx, @ordre, @nom, NULL)
-- @ordre = i + 1 (commence a 1)
```

**Insert(BomContexte c)** (sans noms explicites) :
→ Appelle `InsertAvecNiveaux(c, null)` → cree automatiquement un seul niveau N1 nomme "Ingredients".

**NomExiste** :
```sql
SELECT COUNT(*) FROM bom_contextes WHERE nom = @nom AND id_activite = @idActivite AND id <> @id
```

**Update** :
```sql
UPDATE bom_contextes SET nom=@nom, description=@desc, id_activite=@idActivite WHERE id=@id
```

---

## ETAPE 4 — Gestion des Niveaux BOM

### Navigation

**Niveaux crees automatiquement avec le contexte** (etape 3) — mais peuvent etre ajoutes/modifies/supprimes ulterieurement :

- **Point d'entree 1** : Ecran ContexteNiveaux (Kanban 3 colonnes) > bouton "+ Niveau" (`BtnNouveauNiveau`)
- **Point d'entree 2** : `FrmBomContextes` (liste) > bouton "Niveaux" → `FrmBomNiveaux` (liste) → CRUD
- **Formulaire edition** : `FrmBomNiveauEdit` (modal)

### FrmBomNiveaux — Liste des niveaux d'un contexte

**Classe** : `FrmBomNiveaux : FrmListeBase<BomNiveau>`
**Constructeur** : `FrmBomNiveaux(BomContexte contexte)`

**Titre** : "Niveaux de transformation - {contexte.Nom}"
**Colonnes DGV** : Ordre (60px), Nom du niveau, Description. Colonnes masquees : Id, IdContexte, NomContexte, Activite, DateCreation.

**Logique d'ajout** :
```csharp
int ordreMax = BomNiveauDAL.GetOrdreMax(_contexte.Id);
var nouveau  = new BomNiveau { IdContexte = _contexte.Id, Ordre = ordreMax + 1 };
return new FrmBomNiveauEdit(nouveau, false);
```

### FrmBomNiveauEdit — Formulaire creation/modification

**Classe** : `FrmBomNiveauEdit : FrmEditBase`
**Constructeur** : `FrmBomNiveauEdit(BomNiveau niveau, bool isEdit)`

**Champs visibles** :

| Controle | Label | Type | Obligatoire | Note |
|----------|-------|------|-------------|------|
| `lblOrdre` | - | Label italic gris | Lecture seule | "Ordre dans le contexte : {N}" |
| `txtNom` | "Nom du niveau *" | TextBox (340px) | OUI | - |
| `txtDescription` | "Description" | TextBox multiline (340x60px) | NON | - |

**Titre fenetre** : "Modifier le niveau" (edit) ou "Nouveau niveau (ordre N)" (creation).

### Validation (`Valider()`)

1. `string.IsNullOrWhiteSpace(txtNom.Text)` → "Le nom est obligatoire."

> Pas de verification d'unicite du nom (un meme nom pourrait theoriquement etre reutilise sur deux niveaux differents).

### Sauvegarde (`Sauvegarder()`)

```csharp
_niveau.Nom         = txtNom.Text.Trim();
_niveau.Description = txtDescription.Text.Trim().NullIfEmpty();

if (_isEdit) BomNiveauDAL.Update(_niveau);
else         BomNiveauDAL.Insert(_niveau);
```

### SQL execute (BomNiveauDAL)

**Insert** (auto-calcul de l'ordre) :
```sql
INSERT INTO bom_niveaux (id_contexte, ordre, nom, description)
VALUES (@idCtx,
        (SELECT COALESCE(MAX(ordre),0)+1 FROM bom_niveaux AS sub WHERE sub.id_contexte = @idCtx),
        @nom, @desc)
```

**Update** :
```sql
UPDATE bom_niveaux SET nom=@nom, description=@desc WHERE id=@id
```

**Delete** (garde : uniquement le dernier niveau) :
```sql
-- Verification : niveaux superieurs existent ?
SELECT COUNT(*) FROM bom_niveaux
WHERE id_contexte = (SELECT id_contexte FROM bom_niveaux WHERE id = @id)
  AND ordre > (SELECT ordre FROM bom_niveaux WHERE id = @id)
-- Si > 0 : InvalidOperationException "Supprimez d'abord les niveaux superieurs."
-- Sinon :
DELETE FROM bom_niveaux WHERE id = @id
```

**GetOrdreMax** :
```sql
SELECT COALESCE(MAX(ordre), 0) FROM bom_niveaux WHERE id_contexte = @id
```

### Comment N1 differe de N2+

| Aspect | N1 (Ingredients) | N2+ (Production) |
|--------|-------------------|-------------------|
| Creation | Automatique a la creation du contexte | Manuel (bouton "+" ou FrmBomNiveauEdit) |
| Nom par defaut | "Ingredients" (modifiable) | Choisi par l'utilisateur |
| Contient | Fiches ingredients (matieres premieres) | Fiches BOM (produits fabriques) |
| Stock associe | Stock physique d'ingredients | Stock de production intermediaire |
| Suppression | Possible seulement si dernier (pas de N2 au-dessus) | Du dernier vers le premier uniquement |

---

## Diagramme de sequence complet

```
Utilisateur                    Sidebar/FrmPrincipal         Forms              DAL              DB
    |                                |                      |                   |                |
    |-- Clic "Stocks" ------------->|                      |                   |                |
    |                                |-- NavigateTo ------->|                   |                |
    |                                |                      | FrmStocks         |                |
    |                                |                      |--- GetAll() ----->|-- SELECT ----->|
    |-- Clic "+ Nouveau" ---------->|                      | FrmStockEdit      |                |
    |-- Saisit "Frigo A" --------->|                      |                   |                |
    |-- Clic "Enregistrer" ------->|                      |--- Valider() ---->|                |
    |                                |                      |--- NomExiste() -->|-- SELECT ----->|
    |                                |                      |--- Insert() ----->|-- INSERT ----->|
    |                                |                      |                   |                |
    |-- Clic "Gerer Activites" --->|                      |                   |                |
    |                                |-- ShowDialog ------->| FrmActivites      |                |
    |-- Clic "+ Nouvelle" -------->|                      | FrmActiviteEdit   |                |
    |-- Saisit "Boulangerie" ----->|                      |                   |                |
    |-- Clic "Enregistrer" ------->|                      |--- Insert() ----->|-- INSERT ----->|
    |                                |                      |                   |                |
    |-- Sidebar: "+ Nouveau ctx" -->|                      |                   |                |
    |                                |-- ShowDialog ------->| FrmBomContexteEdit|                |
    |-- Saisit "Pains" + N2 "Cuisson"|                    |                   |                |
    |-- Clic "Enregistrer" ------->|                      |--- InsertAvecNiveaux() -->|        |
    |                                |                      |                   |-- BEGIN TX -->|
    |                                |                      |                   |-- INSERT ctx->|
    |                                |                      |                   |-- INSERT N1 ->|
    |                                |                      |                   |-- INSERT N2 ->|
    |                                |                      |                   |-- COMMIT ---->|
```

---

## Resume des classes et fichiers

| Fichier | Role |
|---------|------|
| `Forms/FrmStocks.cs` | Liste + liaison M:N stocks-activites |
| `Forms/FrmStockEdit.cs` | CRUD stock (FrmEditBase) |
| `DAL/StockDAL.cs` | Persistance stocks + liaison activites_stocks |
| `Forms/FrmActivites.cs` | Liste CRUD activites |
| `Forms/FrmActiviteEdit.cs` | CRUD activite (FrmEditBase) |
| `Forms/FrmActiviteStocks.cs` | Liaison M:N depuis activite |
| `DAL/ActiviteDAL.cs` | Persistance activites + desactivation transactionnelle |
| `Forms/FrmBomContextes.cs` | Liste contextes (FrmListeBase) |
| `Forms/FrmBomContexteEdit.cs` | CRUD contexte + definition niveaux a la creation |
| `DAL/BomContexteDAL.cs` | Persistance contextes + InsertAvecNiveaux atomique |
| `Forms/FrmBomNiveaux.cs` | Liste niveaux d'un contexte (FrmListeBase) |
| `Forms/FrmBomNiveauEdit.cs` | CRUD niveau (FrmEditBase) |
| `DAL/BomNiveauDAL.cs` | Persistance niveaux + gardes suppression |
| `Forms/FrmEditBase.cs` | Classe abstraite : ErrorProvider + boutons + cycle Valider/Sauvegarder |

---

## Points forts pour la defense

1. **Transaction atomique** : la creation contexte+niveaux est dans un `BeginTransaction()` — soit tout passe, soit rollback complet.
2. **Gardes metier dans la DAL** : suppression stock bloquee si ingredients rattaches, suppression niveau bloquee si pas le dernier, desactivation activite bloquee si contextes actifs.
3. **Unicite scopee** : le nom du contexte est unique par activite (pas globalement), ce qui est la bonne granularite metier.
4. **Deux points d'acces liaison M:N** : depuis le stock (persistance immediate, UX rapide) et depuis l'activite (batch, UX deliberee). Meme table pivot `activites_stocks`.
5. **Pattern FrmEditBase** : cycle standardise `errorProvider.Clear() → Valider() → Sauvegarder() → DialogResult.OK` — zero duplication entre les 4 formulaires d'edition.
6. **INSERT IGNORE** sur la table pivot : pas d'exception si la liaison existe deja.
7. **Auto-calcul de l'ordre** : `COALESCE(MAX(ordre),0)+1` dans un sous-SELECT garantit la sequence sans trous meme en cas de suppressions intermediaires.
