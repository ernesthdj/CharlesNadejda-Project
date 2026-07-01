# 03 — Plan PDSGBD : Application C# Windows Forms (ArtisaStock ERP)

> **⚠️ Ce document reflète l'implémentation RÉELLE (sessions 1–12).**
> L'architecture initiale (plan simple) a été entièrement remplacée par un ERP générique BOM.
> Pour l'historique complet des décisions : voir `docs/JOURNAL.md`.

---

## 🎯 Rappel des critères PDSGBD

| Critère | Points | Couvert par |
|---------|--------|-------------|
| C# — langage | /10 | Toute l'application |
| C# — techniques POO | /10 | Models, classes de base, propriétés calculées |
| C# — contrôles d'édition | /10 | TextBox, NumericUpDown, DateTimePicker |
| C# — contrôles de sélection | /10 | ComboBox, ListBox custom-draw, DataGridView, CheckBox |
| C# — gestion formulaires & interactions | /5 | Navigation entre formulaires, passage de données |
| MySQL — INSERT | /5 | Achats, fiches, productions, activités… |
| MySQL — UPDATE | /5 | Stocks, statuts, ingrédients… |
| MySQL — DELETE | /5 | Suppression avec vérification FK |
| MySQL — jointure | /10 | Toutes les listes (activités, stocks, niveaux) |
| MySQL — critère de sélection | /5 | Filtres par activité, niveau, statut DLC |
| MySQL — protection injection + formatage | /10 | `AddWithValue` systématique dans tous les DAL |
| Édition — création / modification | /10 | FrmEditBase + formulaires spécialisés |
| Édition — unicité à l'ajout | /5 | `NomExiste()` dans chaque DAL |
| Édition — validité données | /10 | Validation avant INSERT/UPDATE |
| Édition — unicité à la modification | /10 | `NomExiste(nom, excludeId)` |
| Édition — suppression | /5 | Bouton supprimer + confirmation |
| Édition — suppression en cascade | /5 | Transactions multi-tables |
| Édition — vérification avant suppression | /5 | Guard FK dans chaque DAL |
| Affichage — liste d'enregistrements | /5 | DataGridView (DGV) partout |
| Affichage — liste avec FK | /10 | JOIN activités, stocks, niveaux, fiches |
| Affichage — tri pertinent | /5 | ORDER BY date, nom, statut |
| Affichage — filtrage pertinent | /5 | Chips par activité/stock, filtres DGV |
| **TOTAL** | **/150** | |

---

## 🏗️ Architecture Générale

```
app-csharp/CharlesNadejda/
├── CharlesNadejda.sln          ← Solution Visual Studio (.NET Framework 4.8.1)
├── App.config                  ← Connection string MySQL + clés Cloudinary
├── Models/                     ← Classes de données (POO)
├── DAL/                        ← Data Access Layer — requêtes paramétrées
├── Forms/                      ← Windows Forms (UI)
│   └── UnitConvertisseur.cs    ← Utilitaire conversions (namespace racine, PAS Forms)
└── CharlesNadejda.csproj       ← IMPORTANT : tout nouveau .cs doit être ajouté manuellement
```

### NuGet installés
- `MySql.Data` — connecteur MySQL
- `BCrypt.Net-Next` — hash mots de passe (compatible PHP password_hash)
- `Newtonsoft.Json` — parsing réponses Cloudinary

### Connexion DB
```xml
<!-- App.config -->
<connectionStrings>
  <add name="charlesnadejda"
       connectionString="Server=localhost;Port=3306;Database=charlesnadejda;Uid=root;Pwd=root;"
       providerName="MySql.Data.MySqlClient"/>
</connectionStrings>
```

```csharp
// DAL/DbHelper.cs
public static MySqlConnection GetConnection()
{
    string cs = ConfigurationManager.ConnectionStrings["charlesnadejda"].ConnectionString;
    var conn = new MySqlConnection(cs);
    conn.Open();
    return conn;
}
```

---

## 🔑 Règles absolues (ne jamais déroger)

1. **Requêtes paramétrées** → `cmd.Parameters.AddWithValue("@param", valeur)` — jamais de concaténation
2. **Multi-tables** → `MySqlTransaction` (BEGIN / COMMIT / ROLLBACK)
3. **Multi-statements interdits** → utiliser `cmd.LastInsertedId` après INSERT (pas `SELECT LAST_INSERT_ID()`)
4. **Nouveau fichier .cs** → ajouter `<Compile Include="..."/>` dans `CharlesNadejda.csproj`
5. **Activités** → toujours via objet `Activite` (Id + Nom) depuis `ActiviteDAL` — jamais de string hardcodée
6. **Lambdas dans handlers `EventArgs e`** → utiliser `ev` (pas `e`) pour éviter CS0136
7. **Conversion d'unités** → `UnitConvertisseur.Convertir()` dans `namespace CharlesNadejda` (pas Forms)
8. **Suppression niveau BOM** → vérifier `MAX(ordre)` — jamais supprimer un niveau intermédiaire
9. **DB_HOST Laravel** → `mysql` (nom service Docker) jamais `localhost`

---

## 📐 Classes de base (héritage UI)

### `Forms/FrmListeBase.cs`
Classe générique pour tous les formulaires liste CRUD.
- Palette CHOCOLAT_FONCE / CREME / OR centralisée
- DGV styling : header crème, alternance, sélection chocolat, BorderStyle=None
- 4 boutons FlatStyle sémantiques : chocolat=Ajouter, vert=Modifier, rouge=Supprimer, gris=Fermer
- `CellDoubleClick` → `OnModifier()`
- `DefaultButton.Button2` sur toutes les confirmations de suppression (Nielsen #3)

> ⚠️ Le Designer VS ne peut pas ouvrir les Forms héritant d'une classe générique.
> Toujours vider le `Designer.cs` correspondant.

### `Forms/FrmEditBase.cs`
Classe de base pour les formulaires d'édition.
- `ErrorProvider.BlinkStyle = NeverBlink`
- `btnEnregistrer` CHOCOLAT_FONCE + Cursor=Hand
- `btnAnnuler` ton neutre FlatStyle

---

## 🗄️ Base de données — État actuel

**42 tables** dans `charlesnadejda` (MySQL 8 via Docker).

### Tables principales (hors BOM)

| Table | Rôle |
|-------|------|
| `utilisateurs` | Authentification (BCrypt) + rôle admin/client |
| `categories` | Catégories produits boutique |
| `produits` | Catalogue vitrine web |
| `parfums` | 15 parfums disponibles |
| `fournisseurs` | Fournisseurs matières premières |
| `fiches_ingredients` | Ingrédients avec conditionnement + type physique + densité |
| `lots_ingredients` | Achats de matières premières (FIFO, DLC, prix HTVA) |
| `activites` | Activités ERP (Chocolaterie, Pâtisserie, Glacier…) — dynamiques |
| `stocks` | Stocks physiques indépendants |
| `activites_stocks` | M:N — une activité peut pointer N stocks |

### Tables BOM (préfixe `bom_`)

| Table | Rôle |
|-------|------|
| `bom_contextes` | Configurations de production nommées, liées à une activité |
| `bom_niveaux` | Niveaux ordonnés dans un contexte (N0=ingrédients, N1, N2…) |
| `bom_fiches` | Templates de recettes globaux (liés à un niveau) |
| `bom_fiches_lignes` | Composition d'une fiche (inputs = ingrédient OU autre fiche N-1) |
| `bom_productions` | Log d'exécution d'une fiche dans un contexte |
| `bom_stocks` | Stock par niveau × fiche (lot de production), avec id_contexte + id_activite |
| `bom_productions_lignes` | Traçabilité détaillée des consommations FIFO |
| `bom_reservations` | Réservations sur lots — dispo réelle = quantite - SUM(réservations actives) |

### VIEW

| View | Rôle |
|------|------|
| `vue_stock_global` | Union lots ingrédients + bom_stocks — consultée par FrmVueStock |

---

## 📦 Models (Classes C#)

### Models ERP de base
| Fichier | Table |
|---------|-------|
| `Utilisateur.cs` | utilisateurs |
| `Categorie.cs` | categories |
| `Produit.cs` | produits |
| `Parfum.cs` | parfums |
| `Fournisseur.cs` | fournisseurs |
| `Ingredient.cs` | fiches_ingredients (+TypePhysique, +Densite, +ConditionnementLabel, +QteParConditionnement) |
| `Lot.cs` | lots_ingredients (+TvaPct, +NbConditionnements) |
| `Activite.cs` | activites |
| `Stock.cs` | stocks |

### Models BOM
| Fichier | Table | Propriétés calculées |
|---------|-------|----------------------|
| `BomContexte.cs` | bom_contextes | — |
| `BomNiveau.cs` | bom_niveaux | — |
| `BomFiche.cs` | bom_fiches | `CoutBatch`, `CoutUnitaire` |
| `BomFicheLigne.cs` | bom_fiches_lignes | `SousTotal` |
| `BomProduction.cs` | bom_productions | — |
| `BomStock.cs` | bom_stocks | `EstPerime` |
| `BomProductionLigne.cs` | bom_productions_lignes | `SousTotal` |
| `BomReservation.cs` | bom_reservations | — |
| `VueStockGlobal.cs` | vue_stock_global (VIEW) | lecture seule |
| `RapportCout.cs` | — (calcul en mémoire) | `LigneCout` récursif |

---

## 🔌 DAL (Data Access Layer)

### DAL de base
| Fichier | Responsabilités clés |
|---------|----------------------|
| `DbHelper.cs` | `GetConnection()` depuis App.config |
| `UtilisateurDAL.cs` | `Authenticate(email, mdp)` — BCrypt.Verify |
| `CategorieDAL.cs` | GetAll, Insert, Update, Delete |
| `ProduitDAL.cs` | GetAll (JOIN catégorie), Insert, Update, Delete |
| `ParfumDAL.cs` | GetAll, Insert, Update, Delete |
| `FournisseurDAL.cs` | GetAll, Insert, Update, Delete |
| `IngredientDAL.cs` | GetAll(idStock, idActivite), Insert, Update, Delete |
| `LotDAL.cs` | GetAll (FIFO), Insert, Update, Delete — prix HTVA |
| `ActiviteDAL.cs` | GetAll, GetById, NomExiste, Insert, Update, Desactiver (soft delete avec guard contextes/ingrédients actifs) |
| `StockDAL.cs` | GetAll, GetByActivite, Insert, Update, Delete (guard ingrédients), LierActivite, DelierActivite |

### DAL BOM
| Fichier | Responsabilités clés |
|---------|----------------------|
| `BomContexteDAL.cs` | GetAll(idActivite), GetById, NomExiste, Insert (+ niveau par défaut en transaction), Update, Delete |
| `BomNiveauDAL.cs` | GetByContexte, GetById, GetOrdreMax, Insert (ordre=MAX+1), Update, Delete (lève `InvalidOperationException` si pas top-level) |
| `BomFicheDAL.cs` | GetAll(idActivite), GetById (avec lignes), NomExiste(nom, idNiveau), Insert (transaction header+lignes), Update (replace lignes), Delete |
| `BomFicheLigneDAL.cs` | GetByFiche avec COALESCE sur `fiches_ingredients` + `bom_fiches` |
| `BomStockDAL.cs` | GetByNiveau, GetDisponible, GetLotsDispoFIFO (ORDER BY date_production ASC) |
| `BomReservationDAL.cs` | GetByContexte, Insert, Liberer, LibererToutContexte |
| `BomProductionDAL.cs` | VerifierDisponibilite, Simuler, Executer (transaction atomique FIFO) |
| `BomCoutDAL.cs` | CalculerCout(idFiche, nBatches) — récursif multi-niveaux ; GetPrixMoyenIngredient() — moyenne pondérée lots |
| `VueStockGlobalDAL.cs` | GetAll, GetByActivite, GetByContexte, GetByNiveau |

---

## ⚙️ UnitConvertisseur

**Fichier** : `Forms/UnitConvertisseur.cs`
**Namespace** : `CharlesNadejda` (racine — PAS `CharlesNadejda.Forms`)

> Raison : le DAL l'appelle sans créer une dépendance DAL→Forms (Clean Architecture).

```csharp
// Groupes supportés
// Masse  : mg / g / cg / kg
// Volume : ml / cl / dl / l
// Pièce  : piece

UnitConvertisseur.Convertir(decimal valeur, string uniteSource, string uniteCible)
UnitConvertisseur.SontCompatibles(string u1, string u2)
UnitConvertisseur.UnitesCompatibles(string unite)  // retourne le groupe
UnitConvertisseur.UniteBase(string unite)           // retourne "g" ou "ml"
UnitConvertisseur.VersUniteBase(decimal valeur, string unite)
```

**Règle critique** : dans `BomProductionDAL`, toujours convertir avant toute comparaison stock.
```
// TypeInput == "fiche" : ligne.UniteMesureInput = unite_output de la fiche source
qteNecessaire = UnitConvertisseur.Convertir(qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
```

---

## 🏭 Architecture BOM (Bill of Materials)

### Principe
```
Niveau 0 = Stock ingrédients global
           ↑ consommé par
Contexte ─── Niveau 1 (nommé par artisan) ─── Niveau 2 ─── Niveau N
             stock propre                      stock propre   stock propre
```

### Règles métier
1. Niveau 0 = stock ingrédients global (partagé)
2. Tout contexte naît avec 1 niveau par défaut vide
3. Niveaux supprimables uniquement depuis le haut (`MAX(ordre)` du contexte)
4. Niveau N consomme **uniquement** le niveau N-1 (pas de saut)
5. Fiches = templates réutilisables entre contextes (liées à un niveau)
6. Production : vérifie dispo N-1 → consomme → crée stock N
7. `quantiteCible` dans `BomProductionDAL` = **nombre de batches** (pas quantité finale)
   - `qteProduite = quantiteCible × fiche.QuantiteOutput`
8. Dispo réelle lot = `quantite_disponible - SUM(bom_reservations WHERE actif=1)`
9. FIFO sur consommation (`ORDER BY date_production ASC`)
10. Conversions d'unités obligatoires avant toute comparaison stock

### Workflow Production (BomProductionDAL.Executer)
```
1. VerifierDisponibilite()        hors transaction — lève exception si manque
2. BEGIN TRANSACTION
   a. INSERT bom_productions      (cout = 0 provisoire)
   b. Pour chaque ligne de fiche :
      - ConvertirUnités()         UnitConvertisseur
      - ConsumeStock() FIFO       UPDATE lots/bom_stocks + INSERT bom_productions_lignes
   c. UPDATE bom_productions      SET cout_ingredients, cout_unitaire
   d. INSERT bom_stocks           lot produit au niveau N (avec id_contexte + id_activite)
3. COMMIT (ou ROLLBACK si exception)
```

### Calcul de coût (BomCoutDAL.CalculerCout)
- Descente récursive sur tous les niveaux jusqu'aux ingrédients N0
- Règle de 3 inter-niveaux : `nBatchesSource = qteConsommée / QuantiteOutput(source)`
- Prix ingrédients = moyenne pondérée lots disponibles (fallback `prix_achat_reference`)
- Résultat : `RapportCout` (coût total + coût par unité output)

---

## 🖥️ Formulaires — Architecture de navigation

### FrmLogin
- Écran de démarrage
- `txtEmail` + `txtMotDePasse` (PasswordChar)
- `UtilisateurDAL.Authenticate()` → BCrypt.Verify
- Redirige vers `FrmPrincipal` si role = 'admin'
- Credentials de test : `charles@charlesnadejda.be` / `password`

### FrmPrincipal — Navigation principale
```
┌─ Bandeau haut ──────────────────────────────────────────┐
│  "ArtisaStock"  [+ Activité]  [📦 Stocks]  [⚙ Config]  │
└─────────────────────────────────────────────────────────┘
┌─ Rail gauche ───────────────────────────────────────────┐
│  RESSOURCES                                             │
│    📊 Vue stock global                                  │
│    🥦 Ingrédients                                       │
│    🚚 Fournisseurs                                      │
│                                                         │
│  ACTIVITÉS (liste verticale, custom-draw)               │
│    ▶ Chocolaterie    ← sélection active                 │
│      Pâtisserie                                         │
│      [+ Activité]                                       │
│                                                         │
│  CONTEXTES (filtrés par activité sélectionnée)          │
│    ▶ Pralines Noël                                      │
│      Ganaches classiques                                 │
└─────────────────────────────────────────────────────────┘
┌─ Zone droite — Niveaux du contexte sélectionné ─────────┐
│  Niveau 0 : Stock ingrédients (global)                  │
│  Niveau 1 : Préparations de base                        │
│  Niveau 2 : Assemblages finaux                          │
│  [Fiches] [Produire] [Simuler]                          │
└─────────────────────────────────────────────────────────┘
```

**Menus** (MenuStrip) : Catalogue web [à venir] | Commandes web [à venir]
> Le catalogue web est en placeholder — focus ERP actif.

---

## 📋 Formulaires implémentés

### Module Activités
| Form | Rôle |
|------|------|
| `FrmActivites` | CRUD activités — DGV + Nouveau/Modifier/Désactiver + bouton "📦 Stocks liés" |
| `FrmActiviteEdit` | Création/modification (Nom + Description) |
| `FrmActiviteStocks` | CheckedListBox — lier/délier des stocks à une activité |

### Module Stocks & Ingrédients
| Form | Rôle |
|------|------|
| `FrmStocks` | CRUD stocks physiques/logiques |
| `FrmStockEdit` | Formulaire stock (Nom + Description, validation unicité) |
| `FrmIngredients` | Liste avec chips "Tous / Stock A / Stock B…" — filtre par stock |
| `FrmIngredientEdit` | CRUD ingrédient (TypePhysique, Densité, Conditionnement, Prix) |
| `FrmAchats` | Liste des achats (lots) par stock/activité |
| `FrmAchatEdit` | Enregistrer un achat — radio HTVA/TVAC, live preview "× Xg = Yg en stock" |
| `FrmVueStock` | Vue unifiée (vue_stock_global) — lots + produits fabriqués, code couleur DLC, chips activité |

### Module Fournisseurs
| Form | Rôle |
|------|------|
| `FrmFournisseurs` | CRUD fournisseurs |
| `FrmFournisseurEdit` | Formulaire fournisseur |

### Module BOM — Configuration
| Form | Rôle |
|------|------|
| `FrmBomContextes` | Liste des contextes de production d'une activité |
| `FrmBomContexteEdit` | Création/modification contexte (Nom, Activité, Description) |
| `FrmBomNiveaux` | Liste des niveaux d'un contexte (ordre topologique) |
| `FrmBomNiveauEdit` | Création/modification niveau (Ordre en lecture seule) |
| `FrmBomFiches` | Liste globale des fiches BOM |
| `FrmBomFicheEdit` | Formulaire fiche BOM — sélection input (ingrédient/fiche), quantité, unité verrouillée sur l'input source, recalcul coût en temps réel |

### Module BOM — Production
| Form | Rôle |
|------|------|
| `FrmBomProductionSimulation` | **Formulaire unifié** : sélection recette + nb batches → Simuler → grille vert/rouge → "Lancer la production" activé si 0 pénuries → BomProductionDAL.Executer(). Affiche coût estimé via BomCoutDAL |

### Catalogue web (placeholder)
| Form | Statut |
|------|--------|
| `FrmCategories / Edit` | Implémenté mais accessible via placeholder menu |
| `FrmProduits / Edit` | Implémenté mais accessible via placeholder menu |
| `FrmParfums / Edit` | Implémenté mais accessible via placeholder menu |

---

## 🎨 FrmBomFicheEdit — Le formulaire le plus complexe

**Cascade de sélection :**
1. `cboTypeInput` : "Ingrédient" ou "Fiche (N-1)"
2. `cboInput` : peuplé selon le type — tous les ingrédients du stock OU toutes les fiches du niveau N-1
3. `nudQteLigne` : quantité (si pièce → forcé à 1, désactivé)
4. `cboUniteLigne` : **verrouillé sur l'unité de l'input sélectionné** (jamais libre)

**Recalcul temps réel :**
```
Sur chaque modification → recalculer SousTotal de chaque ligne
→ CoutBatch = Σ(SousTotaux)
→ CoutUnitaire = CoutBatch / nudRendement.Value
```

---

## 🖥️ FrmBomProductionSimulation — Workflow de production

```
[Sélectionner contexte ▼]  →  [Sélectionner niveau ▼]  →  [Sélectionner fiche ▼]
[Nombre de batches: ___]       lblInfoBatch: "1 batch = 20 pièces"

[Simuler ▶]
       ↓
┌──────────────┬──────────────┬──────────────┬──────────────┐
│ Ingrédient   │ En stock     │ Nécessaire   │ Statut       │
├──────────────┼──────────────┼──────────────┼──────────────┤
│ Chocolat lait│ 3,200 kg     │ 1,600 kg     │ ✅ OK (vert) │
│ Beurre cacao │ 0,800 kg     │ 1,200 kg     │ ❌ PÉNURIE   │
└──────────────┴──────────────┴──────────────┴──────────────┘
lblCoutEstime : "Coût estimé : 16,80 € — 0,21 €/pièce"

[Annuler]   [✅ Lancer la production]  ← GRISÉ si pénuries, ACTIF si tout OK
```

**Règle** : `btnLancerProduction.Enabled = (pénuries.Count == 0)`

---

## 🔄 Conditionnement universel

Le conditionnement définit l'identité d'un ingrédient :
- 2 tailles de sac = 2 ingrédients distincts
- Stock toujours stocké en **unité de base** (g, ml, piece)
- À l'achat : `QuantiteInitiale = NbConditionnements × QteParConditionnement`
- Au BOM : conversion automatique via `UnitConvertisseur`
- Prix BOM : `prix_ref = prix_achat_reference / qte_par_conditionnement` (€/unité de base)

---

## 🚀 Checklist de développement (état réel)

### ✅ Terminé
- [x] Infrastructure Docker + App.config
- [x] DbHelper + UtilisateurDAL + FrmLogin
- [x] FrmListeBase + FrmEditBase (classes de base)
- [x] UnitConvertisseur (namespace racine)
- [x] ActiviteDAL + FrmActivites/Edit
- [x] StockDAL + FrmStocks/Edit + FrmActiviteStocks
- [x] IngredientDAL (avec conditionnement) + FrmIngredients/Edit
- [x] LotDAL (HTVA/TVAC) + FrmAchats/Edit
- [x] FournisseurDAL + FrmFournisseurs/Edit
- [x] BomContexteDAL + BomNiveauDAL + BomFicheDAL + BomFicheLigneDAL
- [x] BomStockDAL + BomReservationDAL + BomProductionDAL
- [x] BomCoutDAL (calcul coût récursif)
- [x] VueStockGlobalDAL + FrmVueStock
- [x] FrmBomContextes/Edit + FrmBomNiveaux/Edit
- [x] FrmBomFiches + FrmBomFicheEdit
- [x] FrmBomProductionSimulation (fusion simulation + production)
- [x] FrmPrincipal : rail gauche + activités dynamiques + onboarding
- [x] Rework UI/UX complet : palette chocolat, DGV styling, FlatStyle, DefaultButton.Button2

### ⏳ À faire
- [ ] Commandes web : FrmCommandes + FrmCommandeDetail (lecture BDD Laravel)
- [ ] Catalogue web : FrmCategories/Produits/Parfums (débloquer depuis placeholder)
- [ ] Cloudinary : upload images produits depuis FrmProduitEdit
- [ ] Rapport BOM explosé (tous niveaux) + export PDF

---

## 📝 Règles apprises (extraites du JOURNAL.md)

| # | Règle |
|---|-------|
| 1 | Nouveau `.cs` → ajouter `<Compile>` dans `.csproj` manuellement |
| 2 | `DB_HOST=mysql` sous Docker, jamais `localhost` |
| 3 | MySQL 8 n'enforce pas CHECK sur FK — validation dans le DAL |
| 4 | `bom_fiches` liée à `id_niveau`, pas juste à une activité |
| 5 | Nouveau champ Model → vérifier SELECT / INSERT / UPDATE / Map() |
| 6 | DGV : `Sizable` + `AllCells` + `MinimumWidth` + Anchor 4 bords |
| 7 | Unité ligne BOM = unité de l'input source — ComboBox verrouillé |
| 8 | ENUM → FK : supprimer l'ancienne colonne ET ajouter la nouvelle dans le même ALTER |
| 9 | Activités : jamais hardcodées — toujours via `Activite` (Id + Nom) depuis ActiviteDAL |
| 10 | Après refactor de signature DAL : grep TOUS les call sites avant de clore |
| 11 | DockStyle WinForms (non-partial) : Fill index 0, Bottom index 1, Top index 2 |
| 12 | Lambda dans `EventArgs e` : utiliser `ev` (pas `e`) → évite CS0136 |
| 13 | Paramètres optionnels réordonnés : utiliser paramètres nommés dans tous les call sites |
| 14 | `lots_ingredients.date_peremption` ≠ `date_dlc` — aliaser dans les VIEWs |
| 15 | `quantiteCible` = **nombre de batches** ; `qteProduite = batches × QuantiteOutput` |
| 16 | TypeInput == "fiche" : toujours convertir `qteNecessaire` avant comparaison stock |
| 17 | VIEW MySQL ne contient que les colonnes explicites — JOIN dans le DAL pour les FK labels |
