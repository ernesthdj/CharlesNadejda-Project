# Sauvegarde Session 2 — Charles & Nadejda
> Reprise de contexte pour nouvelle instance Claude
> Date : 30 mars 2026
> Suite de : sauvegarde1.md

---

## Ce qui a été accompli en session 2

### 1. DAL (Data Access Layer) BOM — 7 fichiers créés ✅

| Fichier | Responsabilités clés |
|---------|----------------------|
| `DAL/BomContexteDAL.cs` | GetAll(activite), GetById, NomExiste, Insert (+ crée niveau par défaut en transaction), Update, Delete |
| `DAL/BomNiveauDAL.cs` | GetByContexte, GetById, GetOrdreMax, Insert (ordre = MAX+1 auto), Update, Delete (lève `InvalidOperationException` si pas top-level) |
| `DAL/BomFicheDAL.cs` | GetAll(activite), GetById(avec lignes), NomExiste, Insert (transaction + lignes), Update (replace lignes), Delete |
| `DAL/BomFicheLigneDAL.cs` | GetByFiche avec COALESCE sur `fiches_ingredients` + `bom_fiches` |
| `DAL/BomStockDAL.cs` | GetByNiveau, GetDisponible, GetDisponibleIngredient (- réservations), GetLotsDispoFIFO, GetBomStocksFIFO |
| `DAL/BomReservationDAL.cs` | GetByContexte, GetTotalReservePourLot, Insert, Update, Liberer, LibererToutContexte |
| `DAL/BomProductionDAL.cs` | GetByNiveau, **VerifierDisponibilite** (List\<BomManque\>), **Simuler** (ALL lignes), **Executer** (transaction atomique FIFO) |

#### Algorithme Executer() — transaction atomique
```
1. VerifierDisponibilite() hors transaction — lève exception si manque
2. BEGIN TRANSACTION
   a. INSERT bom_productions (cout = 0 provisoire)
   b. Pour chaque ligne de fiche :
      - ConsumeStock() FIFO → UPDATE lots/bom_stocks + INSERT bom_productions_lignes
   c. UPDATE bom_productions SET cout_ingredients, cout_unitaire
   d. INSERT bom_stocks (lot produit au niveau N)
3. COMMIT (ou ROLLBACK si exception)
```

#### Méthode Simuler() — ajoutée en session 2
Même logique que VerifierDisponibilite() mais retourne **toutes** les lignes
(pas seulement les pénuries). `BomManque.Manque = 0` si OK, `> 0` si pénurie.
Utilisée par FrmBomSimulation pour l'affichage code couleur.

---

### 2. Forms BOM — 8 paires créées ✅

| Form | Rôle |
|------|------|
| `FrmBomContextes` / `.Designer` | Liste des contextes avec DGV, boutons Ajouter/Modifier/Gérer niveaux/Supprimer |
| `FrmBomContexteEdit` / `.Designer` | Création/édition contexte (Nom, Activite, Description) + validation NomExiste |
| `FrmBomNiveaux` / `.Designer` | Liste des niveaux d'un BomContexte ; note "Niveau 0 = stock ingrédients" ; gère `InvalidOperationException` sur Delete |
| `FrmBomNiveauEdit` / `.Designer` | Création/édition niveau ; Ordre affiché en lecture seule |
| `FrmBomFiches` / `.Designer` | Liste globale des fiches BOM, colonnes coût cachées |
| `FrmBomFicheEdit` / `.Designer` | Formulaire complexe : header + composition en ligne (cascade cboTypeInput→cboInput, nudQteLigne, cboUniteLigne, +/- lignes, dgvLignes). Classe interne `InputItem`. |
| `FrmBomProduction` / `.Designer` | Sélecteurs cascade → "Vérifier le stock" → lblResultat (vert/rouge) + dgvManques → "Exécuter la production" (activé seulement si stock OK) |
| `FrmBomSimulation` / `.Designer` | **NOUVEAU session 2** — lecture seule, DGV toutes lignes color-codées (vert = OK, rouge = pénurie), légende, pas de bouton Exécuter |

---

### 3. FrmPrincipal — câblage complet ✅

Les menus **🍫 Chocolaterie** et **🎂 Pâtisserie** ont désormais un sous-menu
**"Production BOM"** avec 4 entrées chacun :

```
Production BOM
├── Contextes de production  → FrmBomContextes(activite)
├── Fiches recettes BOM      → FrmBomFiches(activite)
├── ─────────────────────────
├── Exécuter une production  → FrmBomProduction(activite)   [bold]
└── Simuler une production   → FrmBomSimulation(activite)
```

L'ancien `menuChocProductions` (AfficherAVenir) a été supprimé et remplacé.

---

### 4. .csproj — registrations manuelles ✅

Le projet est .NET Framework 4.8.1 (format classique, non SDK-style) :
**chaque fichier `.cs` doit être listé explicitement dans `<Compile Include="..." />`**.
26 entrées ajoutées (7 DAL + 9 Models + 10 Forms — 5 paires cs/Designer).

**Erreur rencontrée et résolue :**
> `Le nom de type ou d'espace de noms 'FrmBomProduction' est introuvable`
→ Cause : fichiers présents sur disque mais absents du `.csproj`
→ Fix : ajout de toutes les entrées `<Compile>` manquantes

---

## État complet du projet C# (après session 2)

### Models (18 fichiers)
- **Anciens (9)** : Utilisateur, Categorie, Produit, Parfum, Fournisseur, Ingredient (+TypePhysique, +Densite), Recette, RecetteIngredient, Lot
- **BOM (9)** : BomContexte, BomNiveau, BomFiche, BomFicheLigne, BomProduction, BomStock, BomProductionLigne, BomReservation, BomManque

### DAL (16 fichiers)
- **Anciens (9)** : DbHelper, UtilisateurDAL, CategorieDAL, ProduitDAL, ParfumDAL, FournisseurDAL, IngredientDAL, RecetteDAL, LotDAL
- **BOM (7)** : BomContexteDAL, BomNiveauDAL, BomFicheDAL, BomFicheLigneDAL, BomStockDAL, BomReservationDAL, BomProductionDAL

### Forms (19 paires = 38 fichiers)
- **Anciens (11 paires)** : Login, Principal, Categories/Edit, Produits/Edit, Parfums/Edit, Fournisseurs/Edit, Ingredients/Edit, Achats/Edit, Recettes/Edit
- **BOM (8 paires)** : BomContextes, BomContexteEdit, BomNiveaux, BomNiveauEdit, BomFiches, BomFicheEdit, BomProduction, BomSimulation

---

## Ce qui reste à faire — C#

| Priorité | Tâche | Notes |
|----------|-------|-------|
| Moyenne | FrmBomSimulation : rapport BOM explosé | Récursif — décompose une fiche en ingrédients de base à tous niveaux |
| Moyenne | Export PDF du rapport BOM | Bibliothèque à choisir (iTextSharp ou FastReport) |
| Basse | Conversion automatique d'unités | Formules masse↔volume via densité de l'ingrédient |
| Basse | Historique des productions | FrmHistoriqueProductions — appelle GetByNiveau |
| Future | Modules pâtisserie (formes, couches, devis) | AfficherAVenir pour l'instant |

---

## Ce qui reste à faire — Laravel (non démarré)

| Phase | Module |
|-------|--------|
| 3 | Catalogue public (produits, parfums, chocolats) |
| 4 | Auth + Panier + Configurateur pâtisserie |
| 5 | Commandes + paiement Stripe |
| 6 | Admin CRM |
| 7 | i18n (fr/nl/en) |

---

## Règles invariantes (rappel)

1. Toutes requêtes SQL → `cmd.Parameters.AddWithValue` — jamais de concaténation
2. Toute opération multi-tables → `MySqlTransaction` (commit/rollback)
3. Dispo réelle lot ingrédient = `quantite_disponible - SUM(bom_reservations WHERE actif=1)`
4. Suppression niveau BOM → vérifier `MAX(ordre)` avant DELETE
5. CHECK constraints sur FK = interdit MySQL 8 → validation enforced dans les DAL
6. FIFO = ORDER BY `date_achat`/`date_production` ASC sur les lots
7. `.csproj` format classique → tout nouveau fichier `.cs` doit être ajouté manuellement
8. **Abréviations IT** : toujours expliquer à la première utilisation (règle CLAUDE.md)

---

## Structure dossiers (mise à jour)

```
CharlesNadejda_Project/
├── docs/                        ← Documentation (00 à 09)
├── sql/
│   ├── create_database.sql      ✅ v4 — 50 tables
│   ├── seed_data.sql            ✅
│   └── migration_v4_bom.sql     ✅ appliquée sur Docker
├── docker-compose.yml           ✅
├── site-laravel/                ✅ scaffoldé (phases 3-8 non démarrées)
├── sauvegarde0.md               ✅ Session 0
├── sauvegarde1.md               ✅ Session 1 — BOM architecture + Models
├── sauvegarde2.md               ← CE FICHIER
└── app-csharp/CharlesNadejda/
    ├── CharlesNadejda.csproj    ✅ 26 entrées BOM ajoutées
    ├── Models/                  ✅ 18 fichiers (9 anciens + 9 BOM)
    ├── DAL/                     ✅ 16 fichiers (9 anciens + 7 BOM)
    └── Forms/                   ✅ 19 paires (11 anciens + 8 BOM)
```
