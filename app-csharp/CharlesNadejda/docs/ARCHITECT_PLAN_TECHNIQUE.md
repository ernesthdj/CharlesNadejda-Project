# Plan Technique — Refactoring ArtisaStock
**Version :** 1.0  
**Date :** 2026-04-19  
**Auteur :** Agent #2 Architecte  
**Destinataires :** Agent #4 Backend (DAL) · Agent #5 Frontend (WinForms UI)  
**Sources lues :** `PO_USER_STORIES_REFACTORING.md` · `PO_AUDIT_FONCTIONNALITES_DESIGN.md` · `FrmLogin.cs` · `FrmIngredients.cs` · `FrmBomProductionSimulation.cs` · `FrmStocks.cs` · `FrmPrincipal.cs` · `FrmVueStock.cs` · `IngredientDAL.cs` · `StockDAL.cs` · `BomFicheDAL.cs` · `BomProductionDAL.cs` · `VueStockGlobalDAL.cs` · `BomNiveauDAL.cs` · `AppState.cs` · `ScreenRouter.cs` · `NavigationParams.cs` · `Program.cs`

---

## SELFDOUBT appliqué — Observations critiques avant implémentation

1. **US-00 — Fausse urgence :** Le mot de passe debug est dans un bloc `#if DEBUG` (lignes 15-18 de `FrmLogin.cs`), pas en clair en production. Risque réel = fuites via git history. Action : supprimer le bloc `#if DEBUG` complet.

2. **US-01 — Partiellement implémentée :** `FrmIngredients.BuildChipPanel()` charge déjà les chips par stock MAIS filtre uniquement via `_activite` (ligne 54-59). La `ChipBar` "Tous" appelle `IngredientDAL.GetAll(idActivite: _activite?.Id ?? 0)` et non `idStock: 0`. Correction mineure — pas de refonte.

3. **US-02 — Handler existe, bouton manque :** `BtnNouveauContexte_Click` est déjà déclaré dans `FrmPrincipal.cs` (ligne 923) mais n'est raccordé à aucun bouton dans `BuildLeftPanel()`. Il suffit d'ajouter le bouton "＋" dans la section CONTEXTES.

4. **US-04 — Presque complète :** `FrmBomProductionSimulation.btnLancerProduction_Click` est implémenté (lignes 232-287) mais de façon synchrone. Le seul gap réel est l'appel `async/await` pour ne pas bloquer le thread UI pendant `BomProductionDAL.Executer()`.

5. **US-05 — Déconnexion à corriger :** `menuDeconnexion_Click` (ligne 1286-1289) fait `new FrmLogin().Show(); this.Close()` — cela termine `FrmPrincipal` et perd l'AppState. À corriger.

6. **US-12 — Déjà implémentée :** `FrmVueStock` contient déjà les chips filtre par activité et `VueStockGlobalDAL.GetByActivite()`. L'US-12 est **done** sauf documentation. L'export CSV (US-06) manque.

7. **US-03 — DAL déjà prête :** `StockDAL.LierActivite()` et `DelierActivite()` existent. Il manque uniquement l'UI dans `FrmStocks`. ATTENTION : `StockDAL.Delete()` ne vérifie que `fiches_ingredients`, pas `lots_ingredients` — gap de protection à corriger dans le même sprint.

---

## Ordre d'implémentation recommandé

### Sprint 0 — BLOQUANT (avant tout commit)
```
US-00  →  US-05  →  US-02  →  US-01
```
Dépendances :
- US-05 (SFA Login) doit précéder tout test d'intégration
- US-02 dépend de US-05 (navigation stable)
- US-01 est indépendante mais P0 — livrer dans le même sprint

### Sprint 1 — IMPORTANT
```
US-03  →  US-04
```
Dépendances :
- US-03 est indépendante (UI FrmStocks)
- US-04 est indépendante (async FrmBomProductionSimulation)

### Sprint 2 — MOYEN
```
US-06  →  US-12  →  US-07  →  US-08  →  US-09
```
Dépendances :
- US-12 est déjà implémentée — vérification et documentation uniquement
- US-06 (CSV) peut s'intégrer en même temps que US-12 (filtre respecté dans l'export)
- US-07 (Duplicate BOM) — DAL uniquement, câblage UI dans FrmPrincipal
- US-08 (StatCards cliquables) — dépend de AppState.FiltreAlertesSeulement à ajouter
- US-09 (Rapport du jour) — dépend de BomProductionDAL.GetRecentByActivite existant

### Sprint 3 — FAIBLE
```
US-10  →  US-11
```
Dépendances : aucune

---

## Fiche technique par US

---

### US-00 — Sécurité : Supprimer le mot de passe debug en dur

**Couche touchée :** Form  
**Risque :** Faible — le code est déjà dans un `#if DEBUG`, pas exposé en prod. Le risque principal est le git history.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmLogin.cs`

**Fichiers à créer :** Aucun

**Changement :**
```csharp
// SUPPRIMER intégralement les lignes 15-18 :
#if DEBUG
    txtEmail.Text      = "charles@charlesnadejda.be";
    txtMotDePasse.Text = "password";
#endif
```

**Changements DB :** Aucun

**Risques :** Aucun risque fonctionnel. Vérifier que `UtilisateurDAL.Authenticate()` utilise BCrypt — si l'authentification repose encore sur une comparaison en clair côté DB, le sprint 0 doit aussi sécuriser la DAL.

---

### US-01 — Corriger le filtre Ingrédients : activité → stock physique

**Couche touchée :** Form uniquement (DAL déjà correcte)  
**Diagnostic :** `FrmIngredients.BuildChipPanel()` n'utilise `_activite` que pour filtrer les stocks affichés dans la ChipBar. Mais `Charger()` (ligne 115) passe `idActivite: _activite?.Id ?? 0` quand `_stockFiltre == null`, ce qui filtre par activité et non par "tous". Le comportement "Tous" doit retourner TOUS les ingrédients actifs, pas ceux d'une activité.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmIngredients.cs`

**Fichiers à créer :** Aucun

**Modifications précises :**

1. `BuildChipPanel()` — lignes 54-59 : remplacer le chargement des stocks filtrés par activité par `StockDAL.GetAll()` pour afficher tous les stocks physiques :
```csharp
// AVANT (ligne 54-59) :
if (_activite != null)
{
    var stocks = StockDAL.GetByActivite(_activite.Id);
    foreach (var s in stocks)
        _pnlChips.Controls.Add(CreerChip(s.Nom, s, false));
}

// APRÈS :
var stocks = StockDAL.GetAll();   // Tous les stocks physiques actifs
foreach (var s in stocks)
    _pnlChips.Controls.Add(CreerChip(s.Nom, s, false));
```

2. `Charger()` — ligne 115 : corriger l'appel "Tous" pour retourner tous les ingrédients sans filtre activité :
```csharp
// AVANT (ligne 115) :
liste = IngredientDAL.GetAll(idActivite: _activite?.Id ?? 0);

// APRÈS :
liste = IngredientDAL.GetAll();   // idStock=0 ET idActivite=0 → tous
```

3. Titre de la form : supprimer la dépendance à `_activite` dans le constructeur (le titre sera désormais générique) — ou afficher "Ingrédients" sans mention d'activité si `_activite == null`.

4. Colonnes DGV (ligne 127-135) : vérifier la conformité au design v10. La colonne `Marque` est absente du design cible. Le design impose : Nom · Conditionnement · Unité base · Stock (lieu) · Type physique · Densité · Dispo · €/cond. Adapter `ConfigCol()` en conséquence.

**Changements DB :** Aucun  
**DAL à modifier :** Aucune  
**Risques :** Le constructeur `FrmIngredients(Activite activite = null)` est appelé depuis `FrmPrincipal.ShowRessourceScreen()` avec `_state.ActiveActivite`. Après correction, le paramètre `activite` peut être conservé pour afficher le titre contextualisé, mais ne doit plus conditionner le filtre des chips.

---

### US-02 — Câbler le point d'entrée "Créer un contexte"

**Couche touchée :** Form (FrmPrincipal)  
**Diagnostic :** `BtnNouveauContexte_Click` (ligne 923 de FrmPrincipal.cs) est implémenté mais n'est connecté à aucun contrôle visible dans la sidebar. Le bouton "＋" doit être ajouté dans `BuildLeftPanel()`, section CONTEXTES.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmPrincipal.cs`

**Fichiers à créer :** Aucun

**Modifications dans `BuildLeftPanel()` :**

Après la ligne déclarant `_pnlHdrContextes = MakeSectionHeader("Contextes")`, ajouter un bouton "＋ Nouveau contexte" dont :
- Visibilité = `_state.ActiveActivite != null` (mis à jour via `StateChanged`)
- Click = `BtnNouveauContexte_Click` (déjà implémenté)
- Positionnement : à droite du header CONTEXTES (Fitts — position prévisible selon guideline ergonomie)

Après création réussie, `BtnNouveauContexte_Click` doit appeler `AppState.StateChanged` → `ChargerContextes()` → la sidebar se met à jour automatiquement.

Ajouter dans `ShowHubScreen()` le message d'onboarding contextuel si l'activité n'a pas de contextes :
```csharp
var contextes = BomContexteDAL.GetAll(_state.ActiveActivite.Id);
if (contextes.Count == 0)
{
    // Afficher un label "Aucun contexte — créez-en un pour démarrer la production"
    // + lien cliquable vers BtnNouveauContexte_Click
}
```

**Changements DB :** Aucun  
**DAL à modifier :** Aucune (BomContexteDAL.GetAll et BomContexteDAL.Delete existent)

**Risques :** Vérifier que `_pnlContextesFull.Visible` est bien conditionné à `_state.ActiveActivite != null` — c'est déjà le cas dans `ChargerContextes()`. Le bouton "＋" doit être masqué/affiché en cohérence.

---

### US-03 — Panel liaison M:N Stock ↔ Activité dans Stocks Screen

**Couche touchée :** Form + DAL (légère correction)  
**Diagnostic :** `StockDAL.LierActivite()` et `DelierActivite()` existent. `FrmStocks` ne contient pas de panel de liaison. La logique de suppression dans `StockDAL.Delete()` ne vérifie que `fiches_ingredients` — elle doit aussi vérifier `lots_ingredients`.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmStocks.cs`
- `CharlesNadejda/DAL/StockDAL.cs`

**Fichiers à créer :** Aucun

**Modifications DAL — `StockDAL.cs` :**

Corriger `Delete()` pour vérifier aussi `lots_ingredients` :
```csharp
// Ajouter avant la suppression :
cmd.CommandText = @"SELECT COUNT(*) FROM lots_ingredients li 
                    INNER JOIN fiches_ingredients fi ON fi.id = li.id_fiche_ingredient
                    WHERE fi.id_stock = @id";
int nbLots = Convert.ToInt32(cmd.ExecuteScalar());
if (nbLots > 0)
    throw new InvalidOperationException(
        $"Impossible de supprimer : ce stock contient {nbLots} lot(s) d'ingrédients actifs.");
```

Ajouter une méthode utilitaire :
```csharp
/// <summary>Retourne les ids des activités liées à un stock.</summary>
public static List<int> GetActivitesLiees(int idStock)
```
Signature complète dans la section Signatures DAL ci-dessous.

**Modifications UI — `FrmStocks.cs` :**

Architecture SFA : `FrmStocks` s'intègre via `EmbedForm()` dans `_pnlDroit`. Elle doit être refactorée en `SplitContainer` (ou `Panel Dock=Right`) pour afficher le panel de liaison en sidebar droite.

Plan du layout :
```
FrmStocks (ClientSize élargi : 820px × 480px)
├── pnlHeader (DockStyle.Top, 48px) — inchangé
├── pnlBas (DockStyle.Bottom, 52px) — inchangé
└── split : SplitContainer (DockStyle.Fill)
    ├── Panel1 (Dock=Fill) : _dgv (liste stocks)
    └── Panel2 (Dock=Fill, Width=280px) : _pnlLiaison
        ├── lblLiaisonHeader ("Activités liées")
        └── _clbActivites : CheckedListBox (Dock=Fill)
```

Règle MEMORY.md : SplitterDistance dans un LayoutEventHandler après Width > 0.
Règle MEMORY.md : Controls.Add — Panel1 (Fill) avant Panel2 (Right).

Comportement :
- `_dgv.SelectionChanged` → `ChargerLiaisons(stockSelectionne.Id)`
- `_clbActivites.ItemCheck` → INSERT ou DELETE immédiat dans `activites_stocks`
- `_btnSupprimer.Enabled` = `!StockContientDonnees(id)` + `ToolTip` explicatif

**Changements DB :** Aucun (table `activites_stocks` existe)

**Risques :**
- L'événement `ItemCheck` se déclenche AVANT la mise à jour de l'état coché. Utiliser `e.NewValue` et non `clb.GetItemChecked(e.Index)`.
- Désactiver `ItemCheck` pendant `ChargerLiaisons()` pour éviter les faux événements.

---

### US-04 — Transaction FIFO : câblage async de "Lancer la production"

**Couche touchée :** Form uniquement  
**Diagnostic :** `FrmBomProductionSimulation.btnLancerProduction_Click` est déjà implémenté de façon synchrone (lignes 232-287). L'appel `BomProductionDAL.Executer()` bloque le thread UI. La correction est l'ajout de `async/await` + protection double-clic. La logique métier est complète.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmBomProductionSimulation.cs`

**Fichiers à créer :** Aucun

**DAL à modifier :** Aucune — `BomProductionDAL.Executer()` reste synchrone. Wrapper async côté Form.

**Modifications précises :**

```csharp
// AVANT (signature synchrone) :
private void btnLancerProduction_Click(object sender, EventArgs e)

// APRÈS :
private async void btnLancerProduction_Click(object sender, EventArgs e)
```

Remplacer le bloc try/catch d'exécution par :
```csharp
btnLancerProduction.Enabled = false;
btnSimuler.Enabled          = false;
Cursor = Cursors.WaitCursor;
try
{
    int idProd = await Task.Run(() =>
        BomProductionDAL.Executer(niveau.Id, fiche.Id, nudQuantite.Value, notes));

    MessageBox.Show(
        $"Production enregistrée (ID #{idProd}).\n" +
        $"{nudQuantite.Value} batch(es) → {qteTotale} {fiche.UniteOutput} ajoutés au stock.",
        "Production réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

    // Rechargement simulation après production
    btnSimuler_Click(sender, e);
}
catch (InvalidOperationException ex)
{
    MessageBox.Show(ex.Message, "Stock insuffisant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    RéinitialiserRésultats();
}
catch (Exception ex)
{
    MessageBox.Show("Erreur lors de la production : " + ex.Message,
        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
finally
{
    btnLancerProduction.Enabled = _simulationValide;
    btnSimuler.Enabled          = true;
    Cursor = Cursors.Default;
}
```

**Note DoD :** Le message de succès doit inclure l'ID de production et le coût. Le coût peut être récupéré en relançant `BomCoutDAL.CalculerCout()` après production.

**Changements DB :** Aucun  
**Risques :** `async void` sur un gestionnaire d'événement est acceptable en WinForms. Les exceptions non interceptées dans `async void` crashent l'application — le try/catch complet est obligatoire.

---

### US-05 — Correction Architecture SFA : FrmPrincipal reste la Form active

**Couche touchée :** Program.cs + Form (FrmLogin, FrmPrincipal)  
**Diagnostic :**
- `Program.cs` : `Application.Run(new FrmLogin())` — FrmLogin est la Form racine. Quand FrmLogin se cache et montre FrmPrincipal, FrmLogin reste parente dans la boucle de messages.
- `FrmPrincipal.menuDeconnexion_Click` (ligne 1286-1289) : `new FrmLogin().Show(); this.Close()` ferme FrmPrincipal et crée une nouvelle instance FrmLogin hors contexte.
- `FrmPrincipal.OnFormClosed` (ligne 1291-1295) : appelle `Application.Exit()` — correct si FrmPrincipal est la form racine.

**Fichiers à modifier :**
- `CharlesNadejda/Program.cs`
- `CharlesNadejda/Forms/FrmLogin.cs`
- `CharlesNadejda/Forms/FrmPrincipal.cs`

**Fichiers à créer :** Aucun

**Pattern cible — option A (recommandé) :**
```csharp
// Program.cs
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    // FrmPrincipal est la Form racine de la boucle de messages.
    // FrmLogin est affichée en dialog bloquant avant de montrer FrmPrincipal.
    var login = new FrmLogin();
    if (login.ShowDialog() != DialogResult.OK)
        return;   // Login annulé → quitter proprement
    
    Application.Run(new FrmPrincipal(login.Utilisateur));
}
```

`FrmLogin` doit exposer :
```csharp
public Utilisateur Utilisateur { get; private set; }
// Dans btnConnexion_Click, après authentification réussie :
Utilisateur = u;
DialogResult = DialogResult.OK;
```

**Modifications FrmPrincipal — déconnexion :**
```csharp
private void menuDeconnexion_Click(object sender, EventArgs e)
{
    if (MessageBox.Show("Se déconnecter ?", "Confirmation",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
    {
        // Relancer Program.Main via Application.Restart() — ou simplement quitter.
        // Application.Restart() recharge l'exe — montrera FrmLogin à nouveau.
        Application.Restart();
    }
}
```

**Note :** `Application.Restart()` est la solution la plus simple et la plus robuste pour le workflow déconnexion/reconnexion. Si une session en mémoire doit survivre (ex : données non sauvegardées), utiliser plutôt `Hide()` et `ShowDialog()` d'une nouvelle FrmLogin.

**Correction de l'exception FrmActivites (documentation dans le code) :**
```csharp
// Dans FrmPrincipal.cs, au-dessus de chaque appel ShowDialog FrmActivites :
/// <exception cref="InvalidOperationException">
/// FrmActivites est la seule Form modale autorisée dans cette SFA.
/// Elle gère les activités et nécessite un retour explicite (DialogResult).
/// Toute autre Form doit être intégrée via EmbedForm().
/// </exception>
```

**Changements DB :** Aucun  
**Risques :** `Application.Restart()` relance le processus — toute donnée non persistée est perdue. Acceptable pour ArtisaStock (pas de données en session mémoire critique).

---

### US-06 — Exporter CSV depuis Vue Stock Global

**Couche touchée :** Form (FrmVueStock)  
**Diagnostic :** `FrmVueStock` a déjà les chips de filtre par activité. Le filtre actif est stocké dans `_idActiviteFiltre` et `_lignes` est toujours la liste filtrée. L'export CSV doit utiliser `_lignes` directement.

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmVueStock.cs`

**Fichiers à créer :** Aucun

**Modifications :**

1. Ajouter un bouton "🖨 Exporter CSV" dans le bandeau `pnlBas` (à gauche des légendes).

2. Ajouter la méthode `ExporterCsv()` :
```csharp
private void ExporterCsv()
{
    using (var dlg = new SaveFileDialog())
    {
        dlg.Filter           = "CSV (*.csv)|*.csv";
        dlg.FileName         = $"stock_global_{DateTime.Today:yyyy-MM-dd}.csv";
        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (dlg.ShowDialog() != DialogResult.OK) return;

        using (var sw = new System.IO.StreamWriter(dlg.FileName, false, new System.Text.UTF8Encoding(true)))
        {
            // En-tête
            sw.WriteLine("Type;Nom;Disponible;Réservé;Total;Unité;DLC;Stock / Activité;Coût unit.");
            
            foreach (var l in _lignes)
            {
                string dlcText  = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "";
                string lieuText = l.EstLot ? (l.StockNom ?? "") : (l.NomActivite ?? "");
                string typeLabel = l.EstLot ? "Ingrédient" : "Produit BOM";
                
                // Échapper les champs pouvant contenir des ";"
                sw.WriteLine(string.Join(";",
                    Escape(typeLabel),
                    Escape(l.Nom),
                    l.QuantiteDispoReelle.ToString("F3"),
                    l.QuantiteReservee.ToString("F3"),
                    l.QuantiteTotale.ToString("F3"),
                    Escape(l.Unite),
                    dlcText,
                    Escape(lieuText),
                    l.CoutUnitaire > 0 ? l.CoutUnitaire.ToString("F4") : ""));
            }
        }
        MessageBox.Show($"Fichier exporté :\n{dlg.FileName}", "Export réussi",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

private static string Escape(string s)
    => s != null && s.Contains(';') ? $"\"{s}\"" : (s ?? "");
```

**Changements DB :** Aucun  
**DAL à modifier :** Aucune  
**Risques :** Le filtre actif dans `_lignes` doit être synchronisé — s'assurer que `ExporterCsv()` est appelé APRÈS `AppliquerFiltre()` et non sur une liste obsolète. C'est déjà garanti par la référence à `_lignes`.

---

### US-07 — Dupliquer une fiche BOM

**Couche touchée :** DAL + Form (FrmPrincipal — section fiches BOM)

**Fichiers à modifier :**
- `CharlesNadejda/DAL/BomFicheDAL.cs`
- `CharlesNadejda/Forms/FrmPrincipal.cs` (ajout bouton "📋 Dupliquer" dans toolbar fiches)

**Fichiers à créer :** Aucun

**Méthode DAL à ajouter dans `BomFicheDAL.cs` :**

```csharp
/// <summary>
/// Duplique une fiche BOM dans le même niveau.
/// Le nom de la copie est "Copie de [nom original]".
/// Si ce nom existe déjà, suffixe numérique (2), (3)...
/// Retourne l'id de la nouvelle fiche.
/// </summary>
public static int Duplicate(int idFiche)
```
Signature complète dans la section Signatures DAL ci-dessous.

**Modifications FrmPrincipal.cs :**

Dans `ShowContexteScreen()`, section `pnlFichesTop` — ajouter après `btnSupFiche` :
```csharp
var btnDupFiche = MakeActionButton("📋 Dupliquer", CHOCO_MED, Color.White);
btnDupFiche.Enabled = false;   // activé uniquement si fiche sélectionnée
btnDupFiche.Click  += (s, ev) => DupliquerFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);
```

Ajouter la méthode `DupliquerFiche(BomFiche fiche)` :
```csharp
private void DupliquerFiche(BomFiche fiche)
{
    if (fiche == null)
    { MessageBox.Show("Sélectionnez une fiche à dupliquer.", "Info", ...); return; }
    
    if (MessageBox.Show($"Dupliquer « {fiche.Nom} » ?", "Confirmation",
            MessageBoxButtons.YesNo, ...) != DialogResult.Yes) return;
    try
    {
        int idCopie = BomFicheDAL.Duplicate(fiche.Id);
        ChargerFiches(_state.ActiveNiveau);
        // Sélectionner la copie dans _dgvFiches
        foreach (DataGridViewRow row in _dgvFiches.Rows)
            if (row.DataBoundItem is BomFiche f && f.Id == idCopie)
            { _dgvFiches.CurrentCell = row.Cells[0]; break; }
    }
    catch (Exception ex)
    { MessageBox.Show("Erreur : " + ex.Message, "Erreur", ...); }
}
```

Gérer `_dgvFiches.SelectionChanged` pour activer/désactiver `btnDupFiche`.

**Changements DB :** Aucun  
**Risques :** La méthode `Duplicate` doit réutiliser `InsertLignes` (déjà privée dans BomFicheDAL). Deux options : rendre `InsertLignes` interne ou dupliquer inline dans `Duplicate` via la connexion ouverte. Recommandé : rendre `InsertLignes` internal (même assembly).

---

### US-08 — StatCards Hub cliquables avec navigation contextuelle

**Couche touchée :** Form (FrmPrincipal — `ShowHubScreen()`) + Navigation (AppState)

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmPrincipal.cs`
- `CharlesNadejda/Navigation/AppState.cs`
- `CharlesNadejda/Navigation/NavigationParams.cs`

**Fichiers à créer :** Aucun

**Modifications AppState.cs :**
```csharp
// Ajouter la propriété :
public bool FiltreAlertesSeulement { get; private set; }

public void SetFiltreAlertes(bool alertesSeulement)
{
    FiltreAlertesSeulement = alertesSeulement;
    RaiseChanged();
}
```

**Modifications NavigationParams.cs :**
```csharp
// Ajouter :
public bool FiltreAlertesSeulement { get; set; }
```

**Modifications FrmPrincipal.cs — `MakeStatCard()` → `MakeStatCardClickable()` :**

Remplacer les appels `MakeStatCard()` pour les 4 cards cliquables par une version acceptant un callback `Action onClick` et un `Cursor.Hand`.

Plan des navigations (section `ShowHubScreen()`) :
```csharp
// H3 — Ingrédients
cardIngredients.Click += (s, ev) => {
    _state.SetFiltreAlertes(false);
    _state.SetRessource(RessourceType.Ingredients);
    _router.Navigate(ScreenId.Ressources);
};
cardIngredients.Cursor = Cursors.Hand;

// H4 — En alerte
cardAlertes.Click += (s, ev) => {
    _state.SetFiltreAlertes(true);
    _state.SetRessource(RessourceType.Ingredients);
    _router.Navigate(ScreenId.Ressources);
};
cardAlertes.Cursor = alertes.Count > 0 ? Cursors.Hand : Cursors.Default;

// H5 — Fiches BOM
cardFiches.Click += (s, ev) => {
    var contextes = BomContexteDAL.GetAll(_state.ActiveActivite.Id);
    if (contextes.Count > 0)
    {
        _state.SetContexte(contextes[0]);
        ChargerNiveaux();
        _router.Navigate(ScreenId.ContexteNiveaux);
    }
};
cardFiches.Cursor = fiches.Count > 0 ? Cursors.Hand : Cursors.Default;

// H6 — Productions 7j (placeholder — pas d'écran historique dédié)
// Navigation vers FrmBomProductionSimulation ou filtre dans Hub
cardProds.Click += (s, ev) => _router.Navigate(ScreenId.Production);
cardProds.Cursor = prods7j > 0 ? Cursors.Hand : Cursors.Default;
```

**Modifications FrmIngredients.cs — lecture du filtre alerte :**

`FrmIngredients` doit accepter un paramètre `filtreAlertes` ou lire `AppState.FiltreAlertesSeulement`. Comme `FrmIngredients` est instanciée dans `ShowRessourceScreen()`, passer le paramètre via le constructeur :
```csharp
// Ajouter un constructeur optionnel ou un paramètre booléen :
public FrmIngredients(Activite activite = null, bool filtreAlertesSeulement = false)

// Dans Charger() :
if (filtreAlertesSeulement)
    liste = liste.Where(i => i.EstEnAlerte).ToList();
```

**Changements DB :** Aucun  
**Risques :** Les cards non cliquables (données null) doivent avoir `Cursor.Default` — ne pas mettre `Cursor.Hand` si la navigation n'est pas disponible (Hick-Hyman : signaler ce qui est actionnable).

---

### US-09 — Rapport du jour (Hub H1)

**Couche touchée :** Form (FrmPrincipal — `ShowHubScreen()`) + DAL (surcharge)

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmPrincipal.cs`
- `CharlesNadejda/DAL/BomProductionDAL.cs`

**Fichiers à créer :** Aucun

**Méthode DAL à ajouter dans `BomProductionDAL.cs` :**
```csharp
/// <summary>Retourne les productions du jour pour une activité (date_production = CURDATE()).</summary>
public static List<BomProduction> GetDuJourByActivite(int idActivite)
```
Signature complète dans la section Signatures DAL ci-dessous.

**Modifications FrmPrincipal.cs :**

Dans `ShowHubScreen()`, ajouter le bouton "🖨 Rapport du jour" dans `pnlHdr` (à gauche de "▶ Nouvelle production").

Ajouter la méthode `GenererRapport()` utilisant `PrintDocument` :
```csharp
private void GenererRapport()
{
    var prodsJour = BomProductionDAL.GetDuJourByActivite(_state.ActiveActivite.Id);
    var alertes   = IngredientDAL.GetAll(idActivite: _state.ActiveActivite.Id)
                                 .Where(i => i.EstEnAlerte).ToList();
    decimal coutJour = prodsJour.Sum(p => p.CoutIngredients);
    
    var doc = new System.Drawing.Printing.PrintDocument();
    doc.DocumentName = $"Rapport_{_state.ActiveActivite.Nom}_{DateTime.Today:yyyy-MM-dd}";
    
    doc.PrintPage += (s, ev) => {
        // Dessin du rapport : en-tête + sections + pied de page
        // Police Segoe UI 10pt corps, 9pt Bold sections
        // Utiliser ev.Graphics.DrawString()
    };
    
    using (var preview = new PrintPreviewDialog { Document = doc })
        preview.ShowDialog(this);
}
```

**Changements DB :** Aucun  
**Risques :** `PrintDocument` dans WinForms peut poser des problèmes de mise en page multi-pages. Pour cette US, le rapport "du jour" est généralement court (< 20 lignes) — une seule page est acceptable.

---

### US-10 — Onboarding : lien "créer un stock d'abord"

**Couche touchée :** Form (FrmPrincipal — `ShowOnboarding()`)

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmPrincipal.cs`

**Fichiers à créer :** Aucun

**Modifications dans `ShowOnboarding()` :**

1. Remplacer le texte actuel par le workflow 4 étapes conforme au design :
```
1. 📦 Créez un stock (lieu physique de stockage)
2. 🎯 Créez une activité (ce que vous produisez)
3. 🔗 Liez vos stocks à l'activité
4. 🏭 Créez un contexte de production
```

2. Ajouter un Label-lien "créer un stock d'abord" :
```csharp
var lnkStock = new LinkLabel
{
    Text      = "→ Créer un stock d'abord",
    Font      = new Font("Segoe UI", 9.5F, FontStyle.Underline),
    ForeColor = OR,
    Cursor    = Cursors.Hand,
    Location  = new Point(28, lien_y), AutoSize = true
};
lnkStock.Click += (s, ev) => {
    _state.SetRessource(RessourceType.Stocks);
    _router.Navigate(ScreenId.Ressources);
};
pnlCenter.Controls.Add(lnkStock);
```

**Changements DB :** Aucun  
**DAL à modifier :** Aucune

---

### US-11 — Badge "top — supprimable" sur le niveau top dans ContexteNiveaux

**Couche touchée :** Form (FrmPrincipal — `MakeNiveauRow()`)

**Fichiers à modifier :**
- `CharlesNadejda/Forms/FrmPrincipal.cs`

**Fichiers à créer :** Aucun

**Modifications dans `MakeNiveauRow()` :**

Le niveau top est celui avec `Ordre == niveaux.Max(n => n.Ordre)`. Passer `ordreMax` en paramètre :
```csharp
private Panel MakeNiveauRow(BomNiveau niv, int ficheCount, int ordreMax)
```

Dans le corps de la méthode, après `lblNom`, ajouter le badge pill :
```csharp
if (niv.Ordre == ordreMax && ordreMax > 0)
{
    var pill = new Label
    {
        Text      = "top · supprimable",
        Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
        BackColor = OR,
        ForeColor = CHOCO_BRAND,
        AutoSize  = true,
        Padding   = new Padding(6, 2, 6, 2)
    };
    // Positionner à droite du lblNom
    row.Controls.Add(pill);
}
else if (niv.Ordre == 0)
{
    var pill = new Label
    {
        Text      = "verrouillé",
        Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
        BackColor = CHOCO_MED,
        ForeColor = Color.White,
        AutoSize  = true,
        Padding   = new Padding(6, 2, 6, 2)
    };
    row.Controls.Add(pill);
}
```

Désactiver `btnDel` avec tooltip si le niveau n'est pas le top :
```csharp
btnDel.Enabled = (niv.Ordre == ordreMax);
var tip = new ToolTip();
if (!btnDel.Enabled)
    tip.SetToolTip(btnDel, "Seul le niveau le plus haut (ordre max) est supprimable");
```

Mettre à jour tous les appels à `MakeNiveauRow()` dans `ShowContexteScreen()` pour passer `niveaux.Max(n => n.Ordre)`.

**Changements DB :** Aucun  
**DAL à modifier :** Aucune (la règle de suppression est déjà dans `BomNiveauDAL.Delete()`)

---

### US-12 — Filtre par activité dans Vue Stock Global

**Couche touchée :** Aucune (déjà implémentée)  
**Diagnostic :** `FrmVueStock` contient déjà les chips filtre par activité (méthode `BuildChips()`, `AjouterChip()`, `AppliquerFiltre()`). `VueStockGlobalDAL.GetByActivite()` existe et est utilisé. **L'US-12 est done.**

**Action requise :** Vérification et documentation uniquement.
- Confirmer que les chips "Tous" + activités sont bien présentes au chargement.
- Confirmer que l'export CSV (US-06) respecte `_idActiviteFiltre` (c'est garanti par `_lignes`).
- Ajouter la contrainte technique : `filtreActivite = 0` → `VueStockGlobalDAL.GetAll()`, sinon `GetByActivite(id)` — déjà respectée.

**Fichiers à modifier :** Aucun (sauf documentation interne)  
**Changements DB :** Aucun

---

## Schéma DB — Changements requis

**Aucun changement de schéma n'est requis pour les 13 US.**

Toutes les tables nécessaires existent :
| Table | Utilisée par |
|-------|-------------|
| `fiches_ingredients` | US-01 |
| `stocks` | US-01, US-03 |
| `activites_stocks` | US-03, US-12 |
| `bom_contextes` | US-02 |
| `bom_productions` | US-04, US-09 |
| `bom_fiches` | US-07 |
| `bom_fiches_lignes` | US-07 |
| `vue_stock_global` (VIEW) | US-06, US-12 |

**Correction DAL non schématique :** `StockDAL.Delete()` doit vérifier `lots_ingredients` en plus de `fiches_ingredients` (US-03 — protection intégrité référentielle).

---

## Signatures DAL — Méthodes à créer/modifier

### BomFicheDAL.cs — Méthode à créer

```csharp
/// <summary>
/// Duplique une fiche BOM dans le même niveau.
/// Génère automatiquement un nom unique (avec suffixe numérique si nécessaire).
/// Retourne l'id de la nouvelle fiche.
/// </summary>
/// <param name="idFiche">Id de la fiche source à dupliquer.</param>
/// <returns>Id de la fiche copiée (int).</returns>
public static int Duplicate(int idFiche)
{
    var source = GetById(idFiche, avecLignes: true);
    if (source == null)
        throw new ArgumentException($"Fiche {idFiche} introuvable.");

    // Générer un nom unique
    string nomBase = $"Copie de {source.Nom}";
    string nom     = nomBase;
    int    suffixe = 2;
    while (NomExiste(nom, source.IdNiveau))
        nom = $"{nomBase} ({suffixe++})";

    var copie = new BomFiche
    {
        IdNiveau         = source.IdNiveau,
        Nom              = nom,
        Description      = source.Description,
        UniteOutput      = source.UniteOutput,
        QuantiteOutput   = source.QuantiteOutput,
        TempsPreparation = source.TempsPreparation,
        Lignes           = source.Lignes   // copie par référence — Insert() les réinsère
    };
    return Insert(copie);
}
```

### BomProductionDAL.cs — Méthode à créer

```csharp
/// <summary>
/// Retourne les productions du jour pour une activité.
/// Filtre sur date_production = CURDATE() côté MySQL.
/// </summary>
/// <param name="idActivite">Id de l'activité.</param>
/// <returns>Liste des BomProduction du jour.</returns>
public static List<BomProduction> GetDuJourByActivite(int idActivite)
{
    var list = new List<BomProduction>();
    using (var conn = DbHelper.GetConnection())
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"
            SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                   p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                   f.nom AS nom_fiche,
                   n.nom AS nom_niveau, n.ordre,
                   c.nom AS nom_contexte
            FROM bom_productions p
            INNER JOIN bom_fiches    f ON f.id = p.id_fiche
            INNER JOIN bom_niveaux   n ON n.id = p.id_niveau
            INNER JOIN bom_contextes c ON c.id = n.id_contexte
            WHERE c.id_activite = @idActivite
              AND DATE(p.date_production) = CURDATE()
            ORDER BY p.date_production DESC";
        cmd.Parameters.AddWithValue("@idActivite", idActivite);
        using (var r = cmd.ExecuteReader())
            while (r.Read()) list.Add(MapHeader(r));
    }
    return list;
}
```

### StockDAL.cs — Méthode à créer

```csharp
/// <summary>
/// Retourne les ids des activités liées à un stock via activites_stocks.
/// Utilisé par FrmStocks pour initialiser les checkboxes du panel de liaison.
/// </summary>
/// <param name="idStock">Id du stock.</param>
/// <returns>Liste des ids d'activités liées.</returns>
public static List<int> GetActivitesLiees(int idStock)
{
    var list = new List<int>();
    using (var conn = DbHelper.GetConnection())
    using (var cmd  = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT id_activite FROM activites_stocks WHERE id_stock = @id";
        cmd.Parameters.AddWithValue("@id", idStock);
        using (var r = cmd.ExecuteReader())
            while (r.Read()) list.Add(Convert.ToInt32(r["id_activite"]));
    }
    return list;
}
```

### StockDAL.cs — Méthode Delete à modifier

```csharp
/// <summary>
/// Suppression physique — bloquée si le stock contient des ingrédients OU des lots.
/// </summary>
public static void Delete(int id)
{
    using (var conn = DbHelper.GetConnection())
    using (var cmd  = conn.CreateCommand())
    {
        // Vérification 1 : fiches_ingredients liées
        cmd.CommandText = "SELECT COUNT(*) FROM fiches_ingredients WHERE id_stock = @id";
        cmd.Parameters.AddWithValue("@id", id);
        int nbFiches = Convert.ToInt32(cmd.ExecuteScalar());
        if (nbFiches > 0)
            throw new InvalidOperationException(
                $"Impossible de supprimer : ce stock contient {nbFiches} fiche(s) d'ingrédients.\n" +
                "Déplacez ou supprimez les ingrédients avant de supprimer le stock.");

        // Vérification 2 : lots_ingredients actifs
        cmd.CommandText = @"SELECT COUNT(*) FROM lots_ingredients li
                            INNER JOIN fiches_ingredients fi ON fi.id = li.id_fiche_ingredient
                            WHERE fi.id_stock = @id AND li.quantite_disponible > 0";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@id", id);
        int nbLots = Convert.ToInt32(cmd.ExecuteScalar());
        if (nbLots > 0)
            throw new InvalidOperationException(
                $"Impossible de supprimer : ce stock contient {nbLots} lot(s) d'ingrédients actifs.");

        cmd.CommandText = "DELETE FROM stocks WHERE id = @id";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
```

### AppState.cs — Propriété à ajouter

```csharp
public bool FiltreAlertesSeulement { get; private set; }

public void SetFiltreAlertes(bool alertesSeulement)
{
    FiltreAlertesSeulement = alertesSeulement;
    // Pas de RaiseChanged() ici — le filtre est lu au moment de la navigation
}
```

---

## Risques et points d'attention

### Risques critiques

| # | Risque | US | Mitigation |
|---|--------|----|------------|
| R1 | `ItemCheck` dans `CheckedListBox` se déclenche avant mise à jour état — écriture DB incorrecte | US-03 | Utiliser `e.NewValue` et non `GetItemChecked(index)` ; désactiver l'événement pendant `ChargerLiaisons()` |
| R2 | `async void` sans try/catch complet crashe silencieusement | US-04 | Le bloc finally est obligatoire ; réactiver les boutons quoi qu'il arrive |
| R3 | `Application.Restart()` perd tout état mémoire | US-05 | Acceptable pour ArtisaStock — documenter dans le commentaire |
| R4 | Duplication fiche BOM : `InsertLignes` est `private` dans `BomFicheDAL` | US-07 | Passer en `internal` ou dupliquer le code inline dans `Duplicate()` |
| R5 | `SplitterDistance` avant Width > 0 dans FrmStocks refactoré | US-03 | Règle MEMORY.md — toujours dans un `LayoutEventHandler firstLayout` |
| R6 | `Controls.Add` ordre dans FrmStocks (Panel M:N à droite) | US-03 | Règle MEMORY.md — ajouter Panel1 (Fill) avant Panel2 (Right) |

### Points d'attention architecturaux

1. **Pas de `new Form().Show()` sauf FrmActivites** : vérifier que tous les appels dans les US Sprint 1-3 utilisent `EmbedForm()` ou `ShowDialog()` uniquement pour FrmActivites.

2. **Palette centralisée** : les constantes couleur sont définies dans `FrmPrincipal` mais pas dans `FrmStocks` et `FrmVueStock`. Les nouvelles Forms doivent copier les constantes localement (pas de classe statique partagée — YAGNI pour ce projet).

3. **Thread UI** : seule US-04 nécessite `Task.Run()`. US-06 (CSV) et US-09 (rapport) sont synchrones et assez rapides (< 200ms attendu) — pas besoin d'async.

4. **BomFicheLigne.Lignes** dans `BomFiche` : la duplication (US-07) copie les références de liste. `Insert()` insère chaque ligne — pas de risque de corruption.

5. **US-12 est done** : ne pas réimplémenter. Vérifier uniquement et marquer dans le JOURNAL.

---

## Journal Agent #2

### [AGENT #2] — 2026-04-19 14:00
**Entrée consommée :**
- `PO_USER_STORIES_REFACTORING.md` (13 US, 4 sprints)
- `PO_AUDIT_FONCTIONNALITES_DESIGN.md` (52 actions, 10 gaps)
- Code lu : `FrmLogin.cs` · `FrmIngredients.cs` · `FrmBomProductionSimulation.cs` · `FrmStocks.cs` · `FrmPrincipal.cs` (1333 lignes) · `FrmVueStock.cs` · `IngredientDAL.cs` · `StockDAL.cs` · `BomFicheDAL.cs` · `BomProductionDAL.cs` · `VueStockGlobalDAL.cs` · `BomNiveauDAL.cs` · `AppState.cs` · `ScreenRouter.cs` · `NavigationParams.cs` · `Program.cs`

**Output produit :**
- `docs/ARCHITECT_PLAN_TECHNIQUE.md` — plan complet pour les 13 US

**Décisions clés :**
1. US-04 : reclassée "partiellement implémentée" — seul l'async manque
2. US-12 : reclassée "done" — implémentée dans FrmVueStock
3. US-05 : pattern `ShowDialog()` recommandé vs `Application.Restart()` pour la déconnexion
4. US-03 : correction proactive de `StockDAL.Delete()` pour couvrir `lots_ingredients`
5. US-07 : `BomFicheDAL.InsertLignes` à passer `internal` pour permettre la réutilisation dans `Duplicate()`

**Selfdoubt appliqué :**
- 14 affirmations vérifiées dans le code source — ratio 0/14 hypothèses non vérifiées
- Aucune décision de schéma DB non vérifiée

**Alerte agent #4 (Backend) :**
- `StockDAL.Delete()` : ajouter la vérification `lots_ingredients` (US-03)
- `BomFicheDAL.InsertLignes` : passer en `internal` pour US-07
- `BomProductionDAL.GetDuJourByActivite()` : nouvelle méthode à créer (US-09)
- `StockDAL.GetActivitesLiees()` : nouvelle méthode à créer (US-03)
- `BomFicheDAL.Duplicate()` : nouvelle méthode à créer (US-07)

**Alerte agent #5 (Frontend WinForms) :**
- `FrmStocks` : refactorer en `SplitContainer` — respecter règles MEMORY.md (SplitterDistance + Controls.Add)
- `FrmIngredients.BuildChipPanel()` : changer `StockDAL.GetByActivite()` → `StockDAL.GetAll()`
- `FrmIngredients.Charger()` : ligne 115 — corriger l'appel "Tous" de `idActivite:` vers `IngredientDAL.GetAll()`
- `FrmPrincipal.ShowHubScreen()` : US-08 StatCards — ajouter `Click` et `Cursor = Cursors.Hand`
- `FrmPrincipal.MakeNiveauRow()` : US-11 — ajouter paramètre `ordreMax`
- `FrmBomProductionSimulation.btnLancerProduction_Click` : rendre `async void` — US-04

---

*Document produit par l'Agent #2 Architecte — 2026-04-19*
*Inputs consommés : 16 fichiers .cs + 2 fichiers .md de spécification*
