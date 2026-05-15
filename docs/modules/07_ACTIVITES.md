# 07 — Activites
> Module de gestion des activites artisanales (domaines metier dynamiques)
> Derniere mise a jour : 2026-05-16

## Vue d'ensemble

Les **activites** representent les domaines metier de l'artisan (Chocolaterie, Patisserie, Glacier, Cocktails...). Elles sont **entierement dynamiques** — jamais codees en dur dans l'application. Chaque activite possede son propre perimetre de stocks d'ingredients et ses contextes de production BOM.

### Regles metier cles

- **Regle #9 (Journal)** : Ne jamais coder en dur les noms d'activites — tout passe par la table `activites` en DB.
- **Regle #10 (Journal)** : Apres tout refactoring touchant les activites, grep tous les sites d'appel pour verifier la coherence.
- Relation M:N entre activites et stocks via la table de jonction `activites_stocks`.
- La desactivation est bloquee si des contextes BOM actifs ou des ingredients actifs sont rattaches.
- La suppression est physique avec cascade DB (FK CASCADE, migration v9).

---

## Fichiers source

| Fichier | Role |
|---------|------|
| `Models/Activite.cs` | Entite metier |
| `DAL/ActiviteDAL.cs` | CRUD + desactivation soft |
| `Forms/FrmActivites.cs` | Liste CRUD (UI programmatique) |
| `Forms/FrmActiviteEdit.cs` | Creation/modification (herite FrmEditBase) |
| `Forms/FrmActiviteStocks.cs` | Gestion jonction M:N activites-stocks |

---

## Model — `Activite`

```csharp
namespace CharlesNadejda.Models
{
    public class Activite
    {
        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        public override string ToString() => Nom;
    }
}
```

Table DB : `activites` (id, nom, description, actif, date_creation).

---

## DAL — `ActiviteDAL`

Classe statique. Toutes les methodes utilisent `DbHelper.GetConnection()` et des requetes parametrees.

| Methode | Signature | Description |
|---------|-----------|-------------|
| `GetAll` | `List<Activite> GetAll(bool includeInactifs = false)` | Retourne les activites triees par nom. Par defaut, uniquement les actives. |
| `GetById` | `Activite GetById(int id)` | Retourne une activite par son ID, ou `null`. |
| `NomExiste` | `bool NomExiste(string nom, int excludeId = 0)` | Verifie l'unicite du nom (exclut un ID pour le mode edition). |
| `Insert` | `int Insert(Activite a)` | Insere une nouvelle activite (actif=1). Retourne le `LastInsertedId`. |
| `Update` | `void Update(Activite a)` | Met a jour nom et description. |
| `Delete` | `void Delete(int id)` | Suppression physique. Cascade DB sur fiches_ingredients et bom_contextes. |
| `Desactiver` | `void Desactiver(int id)` | Soft-delete (actif=0). Leve `InvalidOperationException` si contextes ou ingredients actifs lies. |

### Methodes privees

| Methode | Role |
|---------|------|
| `Bind(MySqlCommand, Activite)` | Lie les parametres @nom et @desc. |
| `Map(MySqlDataReader)` | Mappe un reader vers un objet `Activite`. |

### Logique de `Desactiver`

1. Verifie `COUNT(*)` dans `bom_contextes WHERE id_activite = @id AND actif = 1`
2. Verifie `COUNT(*)` des ingredients actifs via jointure `fiches_ingredients -> stocks -> activites_stocks`
3. Si aucun dependant actif : `UPDATE activites SET actif = 0`
4. Sinon : leve une exception avec message explicatif

---

## Forms — `FrmActivites`

Formulaire liste non-partial, UI construite programmatiquement. Accessible depuis le bouton gear du bandeau dans FrmPrincipal.

### Structure UI

- **Header** : Panel docked Top, fond `AppColors.ChocoBrand`, titre "ACTIVITES" en or
- **DGV** : DataGridView docked Fill, colonnes Id (masquee), Nom, Description, DateCreation
- **Barre d'actions** : Panel docked Bottom, 5 boutons positionnes par coordonnees absolues

### Boutons (Fitts : action principale a gauche)

| Bouton | Action | Couleur |
|--------|--------|---------|
| + Nouvelle activite | `Nouveau()` | ChocoBrand |
| Modifier | `Modifier()` | Vert (90,130,80) |
| Desactiver | `Desactiver()` | Orange (160,120,60) |
| Supprimer | `Supprimer()` | Rouge (180,50,40) |
| Stocks lies | `GererStocks()` | Bleu (60,110,160) |

### Methodes internes

| Methode | Comportement |
|---------|--------------|
| `Charger()` | Vide le DGV, recharge via `ActiviteDAL.GetAll()` |
| `Nouveau()` | Ouvre `FrmActiviteEdit()` en mode creation |
| `Modifier()` | Ouvre `FrmActiviteEdit(activite)` en mode edition |
| `Desactiver()` | Confirmation + appel DAL, catch InvalidOperationException |
| `Supprimer()` | Double confirmation (cascade irreversible) + appel DAL |
| `GererStocks()` | Ouvre `FrmActiviteStocks(activite)` |
| `ActiviteSelectionnee()` | Retourne l'activite selectionnee dans le DGV ou null |

---

## Forms — `FrmActiviteEdit`

Herite de `FrmEditBase`. Formulaire de creation/modification d'une activite.

### Champs

| Controle | Type | Validation |
|----------|------|------------|
| `txtNom` | TextBox (MaxLength=100) | Obligatoire + unicite via `ActiviteDAL.NomExiste()` |
| `txtDescription` | TextBox (Multiline) | Optionnel, converti en NULL si vide via `.NullIfEmpty()` |

### Cycle FrmEditBase

```
Clic Enregistrer -> errorProvider.Clear() -> Valider() -> Sauvegarder() -> DialogResult.OK
```

- `Valider()` : verifie nom non vide + unicite
- `Sauvegarder()` : appelle `Insert` ou `Update` selon `_isEdit`

---

## Forms — `FrmActiviteStocks`

Gere la relation M:N entre une activite et les stocks disponibles via `activites_stocks`.

### Fonctionnement

1. Au `Load` : charge tous les stocks via `StockDAL.GetAll()`, coche ceux deja lies via `StockDAL.GetByActivite(id)`
2. L'utilisateur coche/decoche les stocks
3. A l'enregistrement : synchronisation differentielle (compare cochage actuel vs etat initial)
   - Coche et pas encore lie -> `StockDAL.LierActivite(idActivite, idStock)`
   - Decoche et etait lie -> `StockDAL.DelierActivite(idActivite, idStock)`

### UI

- CheckedListBox avec `CheckOnClick = true`
- Header identique au style global (fond chocolat fonce, titre or)
- Boutons Enregistrer / Annuler en bas

---

## Relations avec les autres modules

```
Activite (07)
  |
  |-- M:N --> Stock (05) via activites_stocks
  |-- 1:N --> BomContexte (04) via id_activite
  |-- indirect --> FicheIngredient (03) via stock
  |
  FrmPrincipal (01) -- bouton gear --> FrmActivites
  FrmPrincipal (01) -- sidebar --> activity switcher (filtre global)
```

---

## Communautes graphify

C16, C19, C8
