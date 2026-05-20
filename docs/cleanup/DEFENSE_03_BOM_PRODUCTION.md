# DEFENSE_03 -- Fiches recettes (BOM) & Production

> Document de defense -- Workflow utilisateur exact, etapes 7-8 du pipeline de production.
> Genere a partir du code source reel de l'application CharlesNadejda.

---

## Table des matieres

1. [Etape 7 -- Fiches recettes (BOM)](#etape-7--fiches-recettes-bom)
   - [7.1 Vue liste : FrmBomFiches](#71-vue-liste--frmbomfiches)
   - [7.2 Formulaire : FrmBomFicheEdit](#72-formulaire--frmbomficheedit)
   - [7.3 TypeInput : ingredient vs fiche](#73-typeinput--ingredient-vs-fiche)
   - [7.4 UnitConvertisseur](#74-unitconvertisseur)
   - [7.5 DAL : BomFicheDAL.Insert() -- Transaction](#75-dal--bomfichedalinsert--transaction)
   - [7.6 DAL : BomFicheLigneDAL.GetByFiche() -- SQL jointure](#76-dal--bomfichelignedalgetbyfiche--sql-jointure)
2. [Etape 8 -- Production](#etape-8--production)
   - [8.1 Ecran principal : FrmPrincipal.Production.cs](#81-ecran-principal--frmprincipalprodictioncs)
   - [8.2 Ecran modal : FrmBomProductionSimulation](#82-ecran-modal--frmbomproductionsimulation)
   - [8.3 VerifierDisponibilite -- algorithme](#83-verifierdisponibilite--algorithme)
   - [8.4 Simuler -- tableau complet](#84-simuler--tableau-complet)
   - [8.5 Executer -- transaction atomique complete](#85-executer--transaction-atomique-complete)
   - [8.6 ConsumeStock -- FIFO pas a pas](#86-consumestock--fifo-pas-a-pas)
   - [8.7 Reservations et production](#87-reservations-et-production)
   - [8.8 Resume : diagramme de flux](#88-resume--diagramme-de-flux)

---

## Etape 7 -- Fiches recettes (BOM)

### 7.1 Vue liste : FrmBomFiches

**Fichier :** `Forms/FrmBomFiches.cs`
**Classe :** `FrmBomFiches : FrmListeBase<BomFiche>`

FrmBomFiches herite de `FrmListeBase<T>`, le pattern generique liste+boutons CRUD de l'application. Elle recoit un `BomNiveau` en parametre constructeur : la liste affiche uniquement les fiches de ce niveau.

**Titre dynamique :**
```
"{NomContexte}  >  {NomNiveau}  (N{Ordre})"
```
Exemple : `Chocolaterie  >  Ganaches  (N2)`

**Colonnes affichees :**

| Colonne | HeaderText | Largeur | Description |
|---------|-----------|---------|-------------|
| Nom | Nom de la fiche | 220px | Nom unique dans le scope du niveau |
| QuantiteOutput | Qte/exec. | 100px | Formatte via `UnitConvertisseur.FormatQte()` |
| TempsPreparation | Temps (min) | 90px | Duree estimee en minutes |
| Description | Description | 200px | Texte libre optionnel |

**Colonnes cachees :** Id, IdNiveau, Actif, DateCreation, Lignes, NomNiveau, OrdreNiveau, IdContexte, NomContexte, CoutBatch, CoutUnitaire, UniteOutput.

**Chargement donnees :**
```csharp
protected override List<BomFiche> ChargerDonnees()
    => BomFicheDAL.GetByNiveau(_niveau.Id);
```

**SQL sous-jacent (BomFicheDAL.GetByNiveau) :**
```sql
SELECT f.id, f.id_niveau, f.nom, f.description,
       f.unite_output, f.quantite_output, f.temps_preparation, f.stock_cible, f.actif, f.date_creation,
       n.nom AS nom_niveau, n.ordre AS ordre_niveau,
       c.id  AS id_contexte, c.nom AS nom_contexte, c.id_activite,
       a.nom AS nom_activite
FROM bom_fiches f
INNER JOIN bom_niveaux   n ON n.id = f.id_niveau
INNER JOIN bom_contextes c ON c.id = n.id_contexte
INNER JOIN activites     a ON a.id = c.id_activite
WHERE f.id_niveau = @idNiveau AND f.actif = 1
ORDER BY f.nom
```

**Etat vide (TICKET-19) :** Si `dgv.Rows.Count == 0`, un `Label` italic apparait :
> "Aucune fiche dans ce niveau. Cliquez + Ajouter pour creer la premiere."

**Actions CRUD :**
- **Ajouter** : ouvre `FrmBomFicheEdit(null, _niveau)` -- mode creation
- **Modifier** : ouvre `FrmBomFicheEdit(BomFicheDAL.GetById(element.Id), _niveau)` -- avec lignes chargees
- **Supprimer** : `BomFicheDAL.Delete(element.Id)` -- pas de soft delete, SQL `DELETE FROM bom_fiches WHERE id = @id`

---

### 7.2 Formulaire : FrmBomFicheEdit

**Fichier :** `Forms/FrmBomFicheEdit.cs`
**Classe :** `FrmBomFicheEdit : FrmEditBase`

Ce formulaire cree ou modifie une fiche BOM. Il recoit `BomFiche` (null si creation) et `BomNiveau`.

#### Champs de l'en-tete

| Controle | Type | Champ model | Description |
|----------|------|-------------|-------------|
| `txtNom` | TextBox | `Nom` | Obligatoire. Unique dans le scope du niveau. |
| `lblActiviteValeur` | Label (readonly) | `ActiviteNom` | Affiche l'activite parente (ex: "Chocolaterie") en bold. |
| `cboUniteOutput` | ComboBox | `UniteOutput` | Choix : piece, kg, g, mg, l, dl, cl, ml. Defaut : "piece". |
| `nudQuantiteOutput` | NumericUpDown | `QuantiteOutput` | Min 0.01, Max 100000, 2 decimales. Quantite produite par batch. |
| `nudTemps` | NumericUpDown | `TempsPreparation` | 0-9999 minutes. Optionnel (null si 0). |
| `chkStockCible` | CheckBox | (active `nudStockCible`) | Active/desactive le champ stock cible. |
| `nudStockCible` | NumericUpDown | `StockCible` | 0-99999, 2 decimales. Seuil pour jauge stock. |
| `txtDescription` | TextBox multiline | `Description` | Texte libre optionnel. |

#### Section Lignes -- GroupBox "Composition"

Le GroupBox contient :
- `lblTypeInput` : info contextuelle ("Inputs : ingredients du stock (N1)" ou "Inputs : ingredients + fiches de tous les niveaux inferieurs")
- `cboInput` : ComboBox remplie dynamiquement (voir 7.3)
- `nudQteLigne` : quantite par ligne (0.001 - 99999, 3 decimales)
- `cboUniteLigne` : unite de la ligne, filtree par `UnitConvertisseur.UnitesCompatibles()`
- `dgvLignes` : DataGridView readonly affichant les lignes ajoutees

**Colonnes du DGV lignes :**

| Colonne | HeaderText | Largeur |
|---------|-----------|---------|
| TypeInput | Type | 90px |
| NomInput | Input | Fill |
| Quantite | Quantite | 80px |
| UniteMesure | Unite | 70px |

#### Interaction utilisateur -- ajout d'une ligne

1. L'utilisateur selectionne un input dans `cboInput`
2. `SynchroniserUniteInput()` se declenche :
   - Recupere les unites compatibles via `UnitConvertisseur.UnitesCompatibles(item.Unite)`
   - Remplit `cboUniteLigne` avec ces unites
   - Pre-selectionne l'unite native de l'input
   - Si "piece" : 0 decimales ; sinon 3 decimales
3. L'utilisateur saisit la quantite
4. Clic "+ Ajouter" (`BtnAjouterLigne_Click`) :
   - Validation : input selectionne + quantite > 0
   - Cree un `BomFicheLigne` et l'ajoute a `_lignes`
   - Rafraichit le DGV

```csharp
_lignes.Add(new BomFicheLigne
{
    TypeInput         = item.TypeInput,          // "ingredient" | "fiche"
    IdInputIngredient = item.TypeInput == "ingredient" ? (int?)item.Id : null,
    IdInputFiche      = item.TypeInput == "fiche"      ? (int?)item.Id : null,
    Quantite          = nudQteLigne.Value,
    UniteMesure       = cboUniteLigne.SelectedItem?.ToString() ?? item.Unite,
    NomInput          = item.Nom,
    UniteMesureInput  = item.Unite
});
```

#### Validation (`Valider()`)

1. Nom obligatoire et non vide
2. Unicite du nom dans le niveau : `BomFicheDAL.NomExiste(nom, idNiveau, excludeId)`
3. Au moins une ligne (sinon MessageBox)

#### Sauvegarde (`Sauvegarder()`)

```csharp
_fiche.IdNiveau         = _niveau.Id;
_fiche.Nom              = txtNom.Text.Trim();
_fiche.Description      = txtDescription.Text.Trim().NullIfEmpty();
_fiche.UniteOutput      = cboUniteOutput.SelectedItem?.ToString() ?? "piece";
_fiche.QuantiteOutput   = nudQuantiteOutput.Value;
_fiche.TempsPreparation = nudTemps.Value > 0 ? (int?)nudTemps.Value : null;
_fiche.StockCible       = chkStockCible.Checked && nudStockCible.Value > 0
                            ? nudStockCible.Value : (decimal?)null;
_fiche.Lignes           = _lignes;

if (_isEdit) BomFicheDAL.Update(_fiche);
else         BomFicheDAL.Insert(_fiche);
```

---

### 7.3 TypeInput : ingredient vs fiche

Le `cboInput` est rempli par `ChargerInputsDisponibles()` qui combine deux sources :

**1. Ingredients (toujours disponibles) :**
```csharp
foreach (var ing in IngredientDAL.GetAll(idActivite: _niveau.IdActivite))
    cboInput.Items.Add(new InputItem {
        Id = ing.Id, Nom = "[Ingr.]  " + ing.Nom,
        Unite = ing.UniteMesure, TypeInput = "ingredient"
    });
```
- Charge tous les ingredients de l'activite courante
- Prefixe `[Ingr.]` dans le nom affiche
- `TypeInput = "ingredient"` => `IdInputIngredient` sera rempli

**2. Fiches de niveaux inferieurs (si ordre >= 3) :**
```csharp
foreach (var niv in BomNiveauDAL.GetByContexte(_niveau.IdContexte))
{
    if (niv.Ordre >= _niveau.Ordre || niv.Ordre < 2) continue;
    foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
        cboInput.Items.Add(new InputItem {
            Id = f.Id, Nom = $"[N{niv.Ordre}]  {f.Nom}",
            Unite = f.UniteOutput, TypeInput = "fiche"
        });
}
```
- Filtre : niveaux du meme contexte avec `ordre < ordre courant` ET `ordre >= 2`
- Un niveau N3 peut consommer N2, un N4 peut consommer N2 et N3, etc.
- Prefixe `[N2]`, `[N3]` dans le nom affiche
- `TypeInput = "fiche"` => `IdInputFiche` sera rempli

**Regle fondamentale :** Un niveau N peut consommer n'importe quel niveau inferieur, jamais superieur ni egal.

**Protection :** Si aucun input disponible, `btnEnregistrer.Enabled = false`.

---

### 7.4 UnitConvertisseur

**Fichier :** `Helpers/UnitConvertisseur.cs`
**Classe :** `static UnitConvertisseur`

Convertisseur central utilise dans toute l'application.

**Groupes d'unites :**

| Groupe | Unites | Unite de base | Facteurs vers base |
|--------|--------|--------------|-------------------|
| Masse | mg, cg, g, kg | g | 0.001, 0.01, 1, 1000 |
| Volume | ml, cl, dl, l | ml | 1, 10, 100, 1000 |
| Piece | piece | piece | 1 |

**Algorithme de conversion :**
```
valeur_base = valeur_source * facteur_source
valeur_cible = valeur_base / facteur_cible
```
Exemple : 500 g vers kg = (500 * 1) / 1000 = 0.5 kg

**Methodes cles :**

| Methode | Utilisation |
|---------|-------------|
| `Convertir(valeur, source, cible)` | Production FIFO, simulation |
| `UnitesCompatibles(unite)` | Filtrage `cboUniteLigne` dans FrmBomFicheEdit |
| `FormatQte(valeur, unite)` | Affichage lisible (1500 g => "1.50 kg") |
| `FormatPrix(prix)` | Affichage prix ("30.00 EUR") |

**Points d'appel obligatoires dans la production :**
- `BomProductionDAL.VerifierDisponibilite()` : avant comparaison stock
- `BomProductionDAL.Simuler()` : avant comparaison stock
- `BomProductionDAL.ConsumeStock()` : avant decrementation FIFO
- `FrmBomFicheEdit.SynchroniserUniteInput()` : liste des unites compatibles

---

### 7.5 DAL : BomFicheDAL.Insert() -- Transaction

**Fichier :** `DAL/BomFicheDAL.cs`

L'insertion est transactionnelle : fiche + toutes ses lignes dans la meme transaction.

```csharp
public static int Insert(BomFiche f)
{
    using (var conn = DbHelper.GetConnection())
    using (var tx = conn.BeginTransaction())
    {
        try
        {
            // 1. INSERT bom_fiches
            int idFiche;
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO bom_fiches
                        (id_niveau, nom, description, unite_output, quantite_output,
                         temps_preparation, stock_cible, actif)
                    VALUES (@idNiveau, @nom, @desc, @unite, @qte, @temps, @stockCible, 1)";
                BindHeader(cmd, f);
                cmd.ExecuteNonQuery();
                idFiche = (int)cmd.LastInsertedId;
            }

            // 2. INSERT toutes les lignes
            InsertLignes(conn, tx, idFiche, f.Lignes);

            tx.Commit();
            return idFiche;
        }
        catch { tx.Rollback(); throw; }
    }
}
```

**InsertLignes() -- SQL par ligne :**

```sql
INSERT INTO bom_fiches_lignes
    (id_fiche, type_input, id_input_ingredient, id_input_fiche, quantite, unite_mesure)
VALUES (@idFiche, @type, @idIngr, @idFicheInput, @qte, @unite)
```

Chaque ligne est inseree individuellement dans une boucle `foreach`. Les champs `id_input_ingredient` et `id_input_fiche` sont mutuellement exclusifs (l'un est `DBNull.Value`).

**Validation pre-insert :**
```csharp
if (l.TypeInput == "ingredient" && !l.IdInputIngredient.HasValue)
    throw new ArgumentException("...");
if (l.TypeInput == "fiche" && !l.IdInputFiche.HasValue)
    throw new ArgumentException("...");
```

**Update() suit le meme pattern :**
1. `BEGIN TRANSACTION`
2. `UPDATE bom_fiches SET ... WHERE id=@id`
3. `DELETE FROM bom_fiches_lignes WHERE id_fiche = @id` (purge toutes les lignes)
4. `InsertLignes()` (re-insert toutes les lignes)
5. `COMMIT` ou `ROLLBACK`

**Duplication (`Duplicate()`) :**
- Charge la fiche source avec lignes
- Genere un nom unique ("Copie de X", "Copie de X (2)", ...)
- Appelle `Insert()` avec les memes lignes

---

### 7.6 DAL : BomFicheLigneDAL.GetByFiche() -- SQL jointure

**Fichier :** `DAL/BomFicheLigneDAL.cs`

Une seule requete SQL resout les deux types d'input via `COALESCE` :

```sql
SELECT l.id, l.id_fiche, l.type_input,
       l.id_input_ingredient, l.id_input_fiche,
       l.quantite, l.unite_mesure,
       COALESCE(fi.nom,  bf.nom)                   AS nom_input,
       COALESCE(fi.unite_mesure, bf.unite_output)   AS unite_input,
       COALESCE(
           fi.prix_achat_reference / NULLIF(fi.qte_par_conditionnement, 0),
           0
       ) AS prix_ref
FROM bom_fiches_lignes l
LEFT JOIN fiches_ingredients fi ON fi.id = l.id_input_ingredient
LEFT JOIN bom_fiches         bf ON bf.id = l.id_input_fiche
WHERE l.id_fiche = @idFiche
```

**Logique :**
- `LEFT JOIN fiches_ingredients` : rempli si `type_input = 'ingredient'`
- `LEFT JOIN bom_fiches` : rempli si `type_input = 'fiche'`
- `COALESCE` choisit le bon nom/unite selon le type
- `prix_ref` : prix unitaire de reference = `prix_achat_reference / qte_par_conditionnement`

**Methodes utilitaires :**
- `GetFichesUtilisant(idIngredient)` : quelles fiches consomment un ingredient donne
- `GetFichesConsommant(idFiche)` : quelles fiches consomment une fiche BOM donnee (niveau superieur)

---

## Etape 8 -- Production

### 8.1 Ecran principal : FrmPrincipal.Production.cs

**Fichier :** `Forms/FrmPrincipal.Production.cs`
**Methode :** `ShowProductionScreen(NavigationParams p)`

Cet ecran est embarque dans le panneau droit (`_pnlDroit`) de la forme principale. Il n'ouvre PAS un formulaire modal -- tout est inline.

**Sections de l'ecran :**

| # | Section | Description |
|---|---------|-------------|
| 1 | Header | Titre "Production" + sous-titre |
| 2 | KPI Bar | 4 cartes statistiques : Productions 7j, Cout 7j, Alertes stock, Fiches actives |
| 3 | Parametres | Combos cascade (Contexte > Niveau > Fiche) + Nb batches + Delai conservation + Notes |
| 4 | Simulation | DGV avec gauge visuelle + label resultat + bouton "Lancer la production" |
| 5 | Historique | DGV des 10 dernieres productions |
| 6 | Mini-journal | Liste des 5 dernieres actions textuelles |

**KPI calculees au chargement :**
```csharp
int alertes = ings.Count(i => i.EstEnAlerte);
int prods7j = prods.Count(pp => pp.DateProduction >= DateTime.Now.AddDays(-7));
decimal cout7j = prods.Where(pp => pp.DateProduction >= DateTime.Now.AddDays(-7))
                      .Sum(pp => pp.CoutIngredients);
```

#### Cascade Contexte > Niveau > Fiche

La cascade est identique a FrmBomProductionSimulation :

1. **Contexte** : `BomContexteDAL.GetAll(idActivite)` -- tous les contextes de l'activite
2. **Niveau** : `BomNiveauDAL.GetByContexte(ctx.Id)` -- filtre `Ordre > 1` (N1 ne peut pas etre produit)
3. **Fiche** : `BomFicheDAL.GetByNiveau(niv.Id)` -- fiches actives du niveau

**Pre-selection intelligente :** Si `_state.ActiveContexte` ou `_state.ActiveNiveau` sont definis (navigation depuis la vue stock), les combos sont pre-selectionnes.

#### Simulation inline

Le bouton "Simuler" appelle `BomProductionDAL.Simuler()` puis :
- Remplit le DGV avec colonnes manuelles (AutoGenerateColumns = false)
- Colonne speciale "Gauge" : barre de progression custom painting dans la cellule
- Lignes vertes (suffisant) ou rouges (penurie) via `ProdColoriserLignes()`
- Si 0 penurie : calcul cout via `BomCoutDAL.CalculerCout()` + active "Lancer"
- Si penuries > 0 : message rouge, bouton "Lancer" desactive

#### Custom painting -- colonne Gauge

```csharp
private void ProdDgvSimulation_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
{
    // Calculer le pourcentage
    double pct = Math.Min(1.0, (double)(ligne.QuantiteDisponible / ligne.QuantiteNecessaire));

    // Fond gris
    e.Graphics.FillRectangle(greyBrush, rect);

    // Barre coloree
    Color barColor = pct >= 1.0 ? Success    // vert
                   : pct >= 0.5 ? ORG_WARN   // orange
                   : RED_CRIT;                // rouge

    // Texte pourcentage centre
    e.Graphics.DrawString($"{pctInt}%", font, brush, tx, ty);
}
```

---

### 8.2 Ecran modal : FrmBomProductionSimulation

**Fichier :** `Forms/FrmBomProductionSimulation.cs`
**Classe :** `FrmBomProductionSimulation : Form`

Version modale alternative de la production (ouverte depuis FrmArtisaStock). Meme logique que l'ecran inline, mais en formulaire separe.

**Deux constructeurs :**
- `FrmBomProductionSimulation(int idActivite)` : ouverture standard
- `FrmBomProductionSimulation(BomContexte, BomNiveau)` : avec contexte+niveau pre-selectionnes

**Flux utilisateur identique :**
1. Selection cascade Contexte > Niveau (Ordre > 1) > Fiche > Quantite
2. Info batch : "1 batch = {QuantiteOutput} {UniteOutput} -> saisir le nombre de batches"
3. Clic "Simuler" -> DGV colorise
4. Si OK : "Lancer la production" actif
5. Confirmation via MessageBox avec resume complet
6. `BomProductionDAL.Executer()` via `Task.Run()` (async)

---

### 8.3 VerifierDisponibilite -- algorithme

**Fichier :** `DAL/BomProductionDAL.cs`
**Methode :** `VerifierDisponibilite(idNiveau, idFiche, quantiteCible)`

Retourne une liste de `BomManque` (vide si tout est disponible).

**Algorithme :**

```
Pour chaque ligne de la fiche :
    qteNecessaire = ligne.Quantite * quantiteCible (multiplicateur = nb batches)

    SI type = "ingredient" :
        Convertir qteNecessaire de ligne.UniteMesure vers ligne.UniteMesureInput
        qteDisponible = BomStockDAL.GetDisponibleIngredient(idIngredient)
            => SUM(lots.quantite_disponible) - SUM(reservations actives)

    SI type = "fiche" :
        Trouver le niveau source de la fiche (GetIdNiveauDeFiche)
        qteDisponible = BomStockDAL.GetDisponible(idNiveauSource, idFiche)
            => SUM(bom_stocks.quantite_disponible)
        Convertir qteNecessaire vers unite native

    SI qteDisponible < qteNecessaire :
        Ajouter BomManque { NomInput, Unite, QuantiteNecessaire, QuantiteDisponible }
```

**BomStockDAL.GetDisponibleIngredient() -- SQL :**
```sql
SELECT
    COALESCE(SUM(l.quantite_disponible), 0)
    - COALESCE((
        SELECT SUM(r.quantite_reservee)
        FROM bom_reservations r
        INNER JOIN lots_ingredients lr ON lr.id = r.id_lot
        WHERE lr.id_fiche_ingredient = @idFi AND r.actif = 1
      ), 0)
FROM lots_ingredients l
WHERE l.id_fiche_ingredient = @idFi
```

Cette formule deduit les reservations actives de la quantite physique.

---

### 8.4 Simuler -- tableau complet

**Methode :** `BomProductionDAL.Simuler(idNiveau, idFiche, quantiteCible)`

Difference avec `VerifierDisponibilite` : retourne TOUTES les lignes (pas seulement les penuries).

```csharp
resultat.Add(new BomManque
{
    NomInput           = ligne.NomInput,
    Unite              = ligne.UniteMesureInput,
    QuantiteNecessaire = qteNecessaireConv,
    QuantiteDisponible = qteDisponible
});
```

**Propriete calculee dans BomManque :**
```csharp
public decimal Manque => QuantiteNecessaire > QuantiteDisponible
                           ? QuantiteNecessaire - QuantiteDisponible
                           : 0m;
```

Le DGV affiche chaque ligne avec code couleur :
- **Vert** (BackColor `240,255,240`) : `Manque == 0` -- stock suffisant
- **Rouge** (BackColor `255,240,240`) : `Manque > 0` -- penurie

---

### 8.5 Executer -- transaction atomique complete

**Methode :** `BomProductionDAL.Executer(idNiveau, idFiche, quantiteCible, notes, delaiConservationJours)`

**Transaction MySQL atomique complète :**

```
BEGIN TRANSACTION
|
|-- 1. Charger niveau + fiche (une seule fois)
|
|-- 2. Verifier disponibilite (avec les lignes deja chargees)
|      SI penuries > 0 : ROLLBACK + throw InvalidOperationException
|
|-- 3. Calculer quantiteProduite = quantiteCible * fiche.QuantiteOutput
|      Ex: 2 batches * 5 kg/batch = 10 kg
|
|-- 4. INSERT bom_productions (cout = 0 provisoire)
|      INSERT INTO bom_productions
|          (id_niveau, id_fiche, quantite_produite, cout_ingredients, cout_unitaire, notes)
|      VALUES (@idNiv, @idFiche, @qte, 0, 0, @notes)
|      => Recuperer idProduction = LastInsertedId
|
|-- 5. Pour chaque ligne de la fiche :
|      aConommer = ligne.Quantite * multiplicateur
|      coutLigne = ConsumeStock(conn, tx, ligne, aConommer, idProduction, niveau)
|      coutTotalIngredients += coutLigne
|
|-- 6. UPDATE bom_productions -- injecter le vrai cout
|      UPDATE bom_productions
|      SET cout_ingredients = @cout, cout_unitaire = @coutUnit
|      WHERE id = @id
|      (coutUnitaire = coutTotalIngredients / qteProduite)
|
|-- 7. INSERT bom_stocks -- creer le stock produit
|      INSERT INTO bom_stocks
|          (id_niveau, id_contexte, id_activite, id_fiche, id_production,
|           quantite_disponible, cout_unitaire, date_production, date_dlc)
|      VALUES (@idNiv, @idCtx, @idAct, @idFiche, @idProd, @qte, @coutUnit,
|              CURDATE(), DATE_ADD(CURDATE(), INTERVAL @delaiJours DAY))
|      Note : date_dlc = NULL si delaiConservationJours = 0
|
COMMIT
```

**En cas d'erreur a n'importe quelle etape : ROLLBACK complet.**

---

### 8.6 ConsumeStock -- FIFO pas a pas

**Methode :** `BomProductionDAL.ConsumeStock(conn, tx, ligne, aConommer, idProduction, niveau)`

C'est le coeur de la production. Consomme le stock en FIFO (First In First Out -- les lots les plus anciens sont consommes en premier).

**Constante :** `TOLERANCE_ARRONDI = 0.0001m` (evite les faux negatifs sur reste decimal)

#### Cas 1 : TypeInput = "ingredient"

```
1. Convertir aConommer de ligne.UniteMesure vers ligne.UniteMesureInput
   Ex: 500 g -> 0.5 kg (si l'ingredient est stocke en kg)
   => restant = UnitConvertisseur.Convertir(aConommer, uniteLigne, uniteStock)

2. Charger les lots FIFO :
   BomStockDAL.GetLotsDispoFIFO(idFicheIngredient)
```

**SQL FIFO ingredients :**
```sql
SELECT l.id,
       l.quantite_disponible
       - COALESCE((SELECT SUM(r.quantite_reservee)
                    FROM bom_reservations r
                    WHERE r.id_lot = l.id AND r.actif = 1), 0) AS dispo_nette,
       l.prix_unitaire / NULLIF(fi.qte_par_conditionnement, 0) AS prix_unitaire_base
FROM lots_ingredients l
INNER JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
WHERE l.id_fiche_ingredient = @idFi
HAVING dispo_nette > 0
ORDER BY l.date_achat ASC    -- <== FIFO : le plus ancien d'abord
```

```
3. Pour chaque lot (du plus ancien au plus recent) :
   SI restant <= 0 : BREAK

   pris = MIN(restant, dispo_nette_du_lot)

   a) Decrementer le lot :
      UPDATE lots_ingredients
      SET quantite_disponible = quantite_disponible - @pris
      WHERE id = @idLot

   b) Liberer les reservations actives sur ce lot :
      UPDATE bom_reservations
      SET actif = 0
      WHERE id_lot = @idLot AND actif = 1

   c) Inserer la ligne de tracabilite :
      INSERT INTO bom_productions_lignes
          (id_production, type_source, id_lot_ingredient, id_bom_stock,
           quantite_consommee, cout_unitaire_moment)
      VALUES (@idProd, 'lot_ingredient', @idLot, NULL, @qte, @cout)

   coutLigne += pris * prixUnitaire
   restant -= pris

4. Apres la boucle, si restant > TOLERANCE_ARRONDI :
   throw InvalidOperationException("Stock insuffisant...")
   => ROLLBACK de toute la transaction
```

#### Cas 2 : TypeInput = "fiche" (produit intermediaire)

Meme logique, mais consomme depuis `bom_stocks` au lieu de `lots_ingredients` :

```
1. Trouver le niveau source : GetIdNiveauDeFiche(idInputFiche)

2. Charger les stocks FIFO :
   BomStockDAL.GetBomStocksFIFO(idNiveauSource, idInputFiche)
```

**SQL FIFO bom_stocks :**
```sql
SELECT id, quantite_disponible, cout_unitaire
FROM bom_stocks
WHERE id_niveau = @idNiveau AND id_fiche = @idFiche
  AND quantite_disponible > 0
ORDER BY date_production ASC    -- <== FIFO
```

```
3. Pour chaque entree de stock (du plus ancien au plus recent) :
   pris = MIN(restant, dispo)

   a) Decrementer le stock :
      UPDATE bom_stocks
      SET quantite_disponible = quantite_disponible - @pris
      WHERE id = @id

   b) Inserer la ligne de tracabilite :
      INSERT INTO bom_productions_lignes
          (id_production, type_source, id_lot_ingredient, id_bom_stock,
           quantite_consommee, cout_unitaire_moment)
      VALUES (@idProd, 'bom_stock', NULL, @idStock, @qte, @cout)

   coutLigne += pris * coutUnitaire
   restant -= pris
```

**Pas de liberation de reservations pour les bom_stocks** -- les reservations ne concernent que les lots d'ingredients.

---

### 8.7 Reservations et production

Pendant l'execution de la production, les reservations actives sur les lots d'ingredients consommes sont **liberees** (mises a `actif = 0`) :

```sql
UPDATE bom_reservations
SET actif = 0
WHERE id_lot = @idLot AND actif = 1
```

Cette liberation se fait **dans la meme transaction** que la consommation. Si la production echoue (ROLLBACK), les reservations restent intactes.

**Note sur la disponibilite nette :**
- `GetDisponibleIngredient()` deduit les reservations actives du total
- `GetLotsDispoFIFO()` calcule `dispo_nette = quantite_disponible - SUM(reservations actives)`
- Cela signifie qu'un lot reserve est "invisible" pour la production tant que la reservation est active

---

### 8.8 Resume : diagramme de flux

```
UTILISATEUR                         APPLICATION                          BASE DE DONNEES
    |                                   |                                    |
    |-- Selectionne Contexte ---------> |                                    |
    |                                   |-- BomContexteDAL.GetAll() -------> |
    |                                   |<-- Liste contextes --------------- |
    |                                   |                                    |
    |-- Selectionne Niveau (N > 1) ---> |                                    |
    |                                   |-- BomNiveauDAL.GetByContexte() --> |
    |                                   |<-- Liste niveaux (Ordre > 1) ----- |
    |                                   |                                    |
    |-- Selectionne Fiche + Qty ------> |                                    |
    |                                   |                                    |
    |-- Clic "Simuler" ---------------> |                                    |
    |                                   |-- Simuler() ------>                |
    |                                   |   Pour chaque ligne :              |
    |                                   |   - Convertir unite                |
    |                                   |   - GetDisponible*() ------------> |
    |                                   |   <-- qteDisponible -------------- |
    |                                   |                                    |
    |   <-- DGV colore (vert/rouge) --- |                                    |
    |                                   |                                    |
    |-- Clic "Lancer" (si 0 penurie) -> |                                    |
    |                                   |                                    |
    |-- Confirme "Oui" dans MessageBox  |                                    |
    |                                   |                                    |
    |                                   |== BEGIN TRANSACTION =============> |
    |                                   |                                    |
    |                                   |-- VerifierDisponibilite() -------> |
    |                                   |   (double check dans la TX)        |
    |                                   |                                    |
    |                                   |-- INSERT bom_productions --------> |
    |                                   |   (cout = 0 provisoire)            |
    |                                   |                                    |
    |                                   |-- ConsumeStock() x N lignes -----> |
    |                                   |   Pour chaque ligne :              |
    |                                   |   - GetLotsDispoFIFO() ----------> |
    |                                   |   - UPDATE lot (decremente) -----> |
    |                                   |   - UPDATE reservations (off) ---> |
    |                                   |   - INSERT production_ligne -----> |
    |                                   |                                    |
    |                                   |-- UPDATE bom_productions --------> |
    |                                   |   (injecte cout reel)              |
    |                                   |                                    |
    |                                   |-- INSERT bom_stocks --------------> |
    |                                   |   (stock N cree)                   |
    |                                   |                                    |
    |                                   |== COMMIT ========================> |
    |                                   |                                    |
    |   <-- MessageBox "Production OK"  |                                    |
    |                                   |                                    |
    |                                   |-- Re-Simuler() (rafraichir) -----> |
    |   <-- DGV mis a jour              |                                    |
```

---

## Modeles de donnees impliques

### BomFiche
```csharp
public class BomFiche
{
    int      Id, IdNiveau
    string   Nom, Description, UniteOutput      // piece, kg, g, l, ml, cl
    decimal  QuantiteOutput                      // qte produite par batch
    int?     TempsPreparation                    // minutes
    decimal? StockCible                          // seuil pour jauge stock
    bool     Actif
    DateTime DateCreation
    List<BomFicheLigne> Lignes                   // composition
    decimal  CoutBatch    => Lignes.Sum(l => l.SousTotal)
    decimal  CoutUnitaire => QuantiteOutput > 0 ? CoutBatch / QuantiteOutput : 0
}
```

### BomFicheLigne
```csharp
public class BomFicheLigne
{
    int     Id, IdFiche
    string  TypeInput           // "ingredient" | "fiche"
    int?    IdInputIngredient   // FK fiches_ingredients (si ingredient)
    int?    IdInputFiche        // FK bom_fiches (si fiche)
    decimal Quantite
    string  UniteMesure         // kg, g, l, ml, cl, piece
    string  NomInput            // jointure
    string  UniteMesureInput    // unite native de l'input
    decimal PrixUnitaireRef     // prix reference pour estimation
    decimal SousTotal => Quantite * PrixUnitaireRef
}
```

### BomProduction
```csharp
public class BomProduction
{
    int      Id, IdNiveau, IdFiche
    decimal  QuantiteProduite, CoutIngredients, CoutUnitaire
    DateTime DateProduction
    string   Notes
    List<BomProductionLigne> Lignes
}
```

### BomProductionLigne
```csharp
public class BomProductionLigne
{
    int     Id, IdProduction
    string  TypeSource           // "lot_ingredient" | "bom_stock"
    int?    IdLotIngredient      // FK lots_ingredients
    int?    IdBomStock           // FK bom_stocks
    decimal QuantiteConsommee
    decimal CoutUnitaireMoment   // prix au moment de la consommation
    decimal SousTotal => QuantiteConsommee * CoutUnitaireMoment
}
```

### BomManque
```csharp
public class BomManque
{
    string  NomInput
    string  Unite
    decimal QuantiteNecessaire, QuantiteDisponible
    decimal Manque => Max(0, Necessaire - Disponible)
}
```

---

## Tables SQL impliquees

| Table | Role |
|-------|------|
| `bom_fiches` | En-tete des recettes (nom, unite output, qte output, temps) |
| `bom_fiches_lignes` | Lignes de composition (type_input, FK ingredient ou fiche, qte, unite) |
| `bom_productions` | Log de chaque production executee (fiche, qte, cout, date) |
| `bom_productions_lignes` | Tracabilite FIFO : quel lot/stock a ete consomme, combien, a quel prix |
| `bom_stocks` | Stock produit par niveau (qte disponible, cout unitaire, DLC) |
| `lots_ingredients` | Stock ingredient brut (lots d'achat) |
| `bom_reservations` | Reservations actives sur les lots (liberees a la production) |
| `bom_niveaux` | Definition des niveaux de transformation |
| `bom_contextes` | Regroupement de niveaux |
| `activites` | Activite metier (Chocolaterie, Patisserie, ...) |
| `fiches_ingredients` | Catalogue ingredients (nom, unite, prix reference) |

---

## Points cles pour la defense

1. **Transaction atomique** : INSERT production + consommation FIFO + creation stock = 1 seule transaction. COMMIT ou ROLLBACK total.

2. **FIFO strict** : `ORDER BY date_achat ASC` (ingredients) ou `ORDER BY date_production ASC` (bom_stocks). Les lots les plus anciens sont toujours consommes en premier.

3. **Double verification** : La disponibilite est verifiee une premiere fois pour l'affichage (Simuler), puis une seconde fois DANS la transaction (Executer) pour eviter les conditions de concurrence.

4. **Conversion d'unites** : Toutes les comparaisons et consommations passent par `UnitConvertisseur.Convertir()`. Une fiche peut demander 500g mais le stock est en kg -- la conversion est automatique.

5. **Tracabilite complete** : Chaque production genere N lignes `bom_productions_lignes` tracant exactement quel lot a fourni quelle quantite a quel prix.

6. **Reservations liberees** : Lors de la consommation d'un lot ingredient, les reservations actives sur ce lot sont desactivees dans la meme transaction.

7. **Cout reel** : Le cout unitaire est calcule a posteriori (apres consommation FIFO), base sur les prix reels des lots consommes, pas les prix de reference.

8. **Multi-niveaux generique** : Un niveau N peut consommer n'importe quel niveau inferieur (pas seulement N-1). `GetIdNiveauDeFiche()` determine dynamiquement le niveau source.
