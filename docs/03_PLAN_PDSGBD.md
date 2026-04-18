# 03 — Plan PDSGBD : Application C# Windows Forms (Artisastock)

> Plan complet de l'application desktop de gestion interne.
> Cette app se connecte à la **même base MySQL** que le site PHP.
>
> **Concept** : c'est l'adaptation des idées d'ArtisaStock (voir `05_ARTISASTOCK_INTEGRATION.md`)
> en Windows Forms + MySQL, conforme aux exigences du cours PDSGBD.

---

## 🎯 Rappel des critères PDSGBD (grille de notation)

| Critère | Points | Couvert par |
|---------|--------|-------------|
| C# - langage | /10 | Toute l'application |
| C# - techniques POO | /10 | Classes Produit, Commande, Client... |
| C# - contrôles d'édition | /10 | TextBox, DateTimePicker, NumericUpDown |
| C# - contrôles de sélection | /10 | ComboBox, ListBox, DataGridView, CheckBox |
| C# - gestion formulaires & interactions | /5 | Navigation entre formulaires, passage de données |
| MySQL - INSERT | /5 | Ajout produit, parfum, etc. |
| MySQL - UPDATE | /5 | Modification statut, produit, client |
| MySQL - DELETE | /5 | Suppression produit, parfum |
| MySQL - jointure | /10 | Affichage commandes avec clients et produits |
| MySQL - critère de sélection | /5 | Filtres et recherches |
| MySQL - protection injection + formatage | /10 | Requêtes paramétrées partout |
| Édition - création / modification | /10 | Formulaire produit, client |
| Édition - unicité à l'ajout | /5 | Vérif email unique, nom produit unique |
| Édition - validité données | /10 | Validation avant INSERT/UPDATE |
| Édition - unicité à la modification | /10 | Vérif email ≠ autre utilisateur |
| Édition - suppression | /5 | Bouton supprimer avec confirmation |
| Édition - suppression en cascade | /5 | Supprimer commande → lignes supprimées |
| Édition - vérification avant suppression | /5 | Bloquer si FK violated |
| Affichage - liste d'enregistrements | /5 | DataGridView produits, clients |
| Affichage - liste avec FK | /10 | Commandes avec nom client + produits |
| Affichage - tri pertinent | /5 | Tri par date, nom, statut |
| Affichage - filtrage pertinent | /5 | Filtre par statut commande, catégorie |
| **TOTAL** | **/150** | |

---

## 🔐 Écran de Connexion (`FrmLogin`)

L'app démarre sur un écran de login — usage interne mais sécurisé.

```
┌─────────────────────────────────────┐
│   🍫 Charles & Nadejda — Artisastock │
│                                     │
│   Email    [___________________]    │
│   Mot de passe [________________]   │
│                                     │
│          [Se connecter]             │
└─────────────────────────────────────┘
```

```csharp
// FrmLogin.cs — vérification contre la table utilisateurs (role = 'admin')
private void btnConnexion_Click(object sender, EventArgs e)
{
    string email = txtEmail.Text.Trim();
    string mdp   = txtPassword.Text;

    var user = UtilisateurDAL.Authenticate(email, mdp); // vérifie password_verify côté C#
    if (user != null && user.Role == "admin")
    {
        var principal = new FrmPrincipal(user);
        principal.Show();
        this.Hide();
    }
    else
    {
        lblErreur.Text = "Email ou mot de passe incorrect.";
        lblErreur.Visible = true;
    }
}
```

> **Note** : `password_verify()` est côté PHP. En C#, les mots de passe admin sont hashés avec BCrypt.
> Package NuGet à ajouter : `BCrypt.Net-Next`.

---

## 🏗️ Architecture de l'Application

L'app est organisée en **5 modules** correspondant chacun à un onglet/menu dans `FrmPrincipal` :

```
CharlesNadejda.sln
└── CharlesNadejda/
    ├── Models/                    ← Classes de données (POO)
    │   ├── Categorie.cs
    │   ├── Parfum.cs
    │   ├── Produit.cs
    │   ├── Ingredient.cs          ← NOUVEAU (ArtisaStock)
    │   ├── Fournisseur.cs         ← NOUVEAU
    │   ├── Recette.cs             ← NOUVEAU
    │   ├── RecetteIngredient.cs   ← NOUVEAU
    │   ├── Production.cs          ← NOUVEAU
    │   ├── MouvementStock.cs      ← NOUVEAU
    │   ├── Utilisateur.cs
    │   ├── Commande.cs
    │   └── LigneCommande.cs
    │
    ├── DAL/                       ← Data Access Layer
    │   ├── DatabaseHelper.cs      ← Connexion MySQL (singleton)
    │   ├── CategorieDAL.cs
    │   ├── ParfumDAL.cs
    │   ├── ProduitDAL.cs
    │   ├── IngredientDAL.cs       ← NOUVEAU
    │   ├── FournisseurDAL.cs      ← NOUVEAU
    │   ├── RecetteDAL.cs          ← NOUVEAU (le plus complexe)
    │   ├── ProductionDAL.cs       ← NOUVEAU
    │   ├── MouvementStockDAL.cs   ← NOUVEAU
    │   ├── UtilisateurDAL.cs
    │   └── CommandeDAL.cs
    │
    ├── Forms/
    │   ├── FrmPrincipal.cs           ← Fenêtre principale avec MenuStrip
    │   │
    │   │   ── MODULE 1 : CATALOGUE (impact direct boutique PHP) ──
    │   ├── FrmCategories.cs
    │   ├── FrmCategorieEdit.cs
    │   ├── FrmParfums.cs
    │   ├── FrmParfumEdit.cs
    │   ├── FrmProduits.cs
    │   ├── FrmProduitEdit.cs
    │   │
    │   │   ── MODULE 2 : INGRÉDIENTS & STOCK ──
    │   ├── FrmIngredients.cs         ← Liste + alertes stock (rouge si bas)
    │   ├── FrmIngredientEdit.cs      ← CRUD avec NumericUpDown, ComboBox
    │   ├── FrmAchatStock.cs          ← Enregistrer un achat (entrée stock)
    │   ├── FrmMouvementsStock.cs     ← Historique avec filtres date/type
    │   │
    │   │   ── MODULE 3 : RECETTES ──
    │   ├── FrmRecettes.cs            ← Liste avec coût de revient calculé
    │   ├── FrmRecetteEdit.cs         ← Formulaire complexe (ajout dynamique ingrédients)
    │   │
    │   │   ── MODULE 4 : PRODUCTION ──
    │   ├── FrmProduction.cs          ← Valider + lancer une production
    │   ├── FrmHistoriqueProductions.cs ← Historique avec marges
    │   │
    │   │   ── MODULE 5 : COMMANDES (lecture du site PHP) ──
    │   ├── FrmCommandes.cs
    │   └── FrmCommandeDetail.cs
    │
    └── Program.cs
```

---

## 🖥️ FrmPrincipal — Navigation par menus

```
┌─────────────────────────────────────────────────────────────────┐
│ Charles & Nadejda — Gestion Artisanale              [v1.0]      │
├──────────┬──────────────┬──────────┬──────────────┬────────────┤
│ Catalogue│  Ingrédients │ Recettes │  Production  │ Commandes  │
│──────────│──────────────│──────────│──────────────│────────────│
│ Catégories│ Ingrédients │ Mes      │ Lancer une   │ Voir les   │
│ Parfums  │ Achats       │ recettes │ production   │ commandes  │
│ Produits │ Mouvements   │          │ Historique   │            │
└──────────┴──────────────┴──────────┴──────────────┴────────────┘
                    Contenu du formulaire actif
```

---

## 🧱 Modèles (Classes C#)

```csharp
// Models/Produit.cs
public class Produit
{
    public int    Id           { get; set; }
    public int    IdCategorie  { get; set; }
    public string Categorie    { get; set; }  // Jointure
    public string Nom          { get; set; }
    public string Description  { get; set; }
    public decimal PrixTTC     { get; set; }
    public int    Stock        { get; set; }
    public string Image        { get; set; }
    public bool   Configurable { get; set; }
    public int?   CapaciteMax  { get; set; }
    public bool   Disponible   { get; set; }

    public override string ToString() => $"{Nom} ({PrixTTC:F2} €)";
}

// Models/Commande.cs
public class Commande
{
    public int      Id             { get; set; }
    public int      IdClient       { get; set; }
    public string   NomClient      { get; set; }  // Jointure
    public string   PrenomClient   { get; set; }  // Jointure
    public DateTime DateCommande   { get; set; }
    public string   Statut         { get; set; }
    public decimal  TotalTTC       { get; set; }
    public string   Adresse        { get; set; }
    public string   Notes          { get; set; }

    public List<LigneCommande> Lignes { get; set; } = new();
}
```

---

## 🔌 Couche d'Accès aux Données (DAL)

### `DAL/DatabaseHelper.cs`

```csharp
using MySql.Data.MySqlClient;

public class DatabaseHelper
{
    private static MySqlConnection _connection = null;
    private const string CONNECTION_STRING =
        "Server=localhost;Database=charlesnadejda;Uid=root;Pwd=;CharSet=utf8mb4;";

    public static MySqlConnection GetConnection()
    {
        if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
        {
            _connection = new MySqlConnection(CONNECTION_STRING);
            _connection.Open();
        }
        return _connection;
    }
}
```

### `DAL/ProduitDAL.cs` (exemple complet)

```csharp
public class ProduitDAL
{
    // ── READ : liste avec jointure catégorie
    public List<Produit> GetAll(int? idCategorie = null, bool seulementDisponibles = false)
    {
        var produits = new List<Produit>();
        string sql = @"
            SELECT p.id, p.nom, p.description, p.prix_ttc, p.stock,
                   p.configurable, p.capacite_max, p.disponible,
                   c.id AS id_categorie, c.nom AS categorie
            FROM produits p
            INNER JOIN categories c ON p.id_categorie = c.id
            WHERE 1=1";

        if (idCategorie.HasValue) sql += " AND p.id_categorie = @idCat";
        if (seulementDisponibles)  sql += " AND p.disponible = 1";
        sql += " ORDER BY c.ordre_affichage, p.nom";

        using var cmd = new MySqlCommand(sql, DatabaseHelper.GetConnection());
        if (idCategorie.HasValue) cmd.Parameters.AddWithValue("@idCat", idCategorie.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            produits.Add(new Produit {
                Id           = reader.GetInt32("id"),
                Nom          = reader.GetString("nom"),
                Description  = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                PrixTTC      = reader.GetDecimal("prix_ttc"),
                Stock        = reader.GetInt32("stock"),
                Configurable = reader.GetBoolean("configurable"),
                CapaciteMax  = reader.IsDBNull(reader.GetOrdinal("capacite_max")) ? null : reader.GetInt32("capacite_max"),
                Disponible   = reader.GetBoolean("disponible"),
                IdCategorie  = reader.GetInt32("id_categorie"),
                Categorie    = reader.GetString("categorie")
            });
        }
        return produits;
    }

    // ── CREATE
    public void Insert(Produit p)
    {
        // Vérifier unicité du nom d'abord
        if (NomExiste(p.Nom))
            throw new Exception($"Un produit nommé '{p.Nom}' existe déjà.");

        string sql = @"
            INSERT INTO produits
                (id_categorie, nom, description, prix_ttc, stock,
                 configurable, capacite_max, image_url, disponible)
            VALUES
                (@idCat, @nom, @desc, @prix, @stock,
                 @conf, @cap, @imgUrl, @dispo)";

        using var cmd = new MySqlCommand(sql, DatabaseHelper.GetConnection());
        cmd.Parameters.AddWithValue("@idCat",  p.IdCategorie);
        cmd.Parameters.AddWithValue("@nom",    p.Nom);
        cmd.Parameters.AddWithValue("@desc",   p.Description);
        cmd.Parameters.AddWithValue("@prix",   p.PrixTTC);
        cmd.Parameters.AddWithValue("@stock",  p.Stock);
        cmd.Parameters.AddWithValue("@conf",   p.Configurable);
        cmd.Parameters.AddWithValue("@cap",    (object)p.CapaciteMax ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@imgUrl", (object)p.ImageUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dispo",  p.Disponible);
        cmd.ExecuteNonQuery();
    }

    // ── UPDATE
    public void Update(Produit p)
    {
        if (NomExiste(p.Nom, p.Id))
            throw new Exception($"Un autre produit porte déjà le nom '{p.Nom}'.");

        string sql = @"
            UPDATE produits
            SET id_categorie = @idCat, nom = @nom, description = @desc,
                prix_ttc = @prix, stock = @stock, disponible = @dispo
            WHERE id = @id";

        using var cmd = new MySqlCommand(sql, DatabaseHelper.GetConnection());
        cmd.Parameters.AddWithValue("@idCat", p.IdCategorie);
        cmd.Parameters.AddWithValue("@nom",   p.Nom);
        cmd.Parameters.AddWithValue("@desc",  p.Description);
        cmd.Parameters.AddWithValue("@prix",  p.PrixTTC);
        cmd.Parameters.AddWithValue("@stock", p.Stock);
        cmd.Parameters.AddWithValue("@dispo", p.Disponible);
        cmd.Parameters.AddWithValue("@id",    p.Id);
        cmd.ExecuteNonQuery();
    }

    // ── DELETE (avec vérification FK)
    public void Delete(int id)
    {
        // Vérifier si des lignes de commande référencent ce produit
        string checkSql = "SELECT COUNT(*) FROM lignes_commandes WHERE id_produit = @id";
        using var checkCmd = new MySqlCommand(checkSql, DatabaseHelper.GetConnection());
        checkCmd.Parameters.AddWithValue("@id", id);
        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (count > 0)
            throw new Exception($"Impossible de supprimer : {count} commande(s) contiennent ce produit.");

        string sql = "DELETE FROM produits WHERE id = @id";
        using var cmd = new MySqlCommand(sql, DatabaseHelper.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ── Unicité
    private bool NomExiste(string nom, int? excludeId = null)
    {
        string sql = "SELECT COUNT(*) FROM produits WHERE nom = @nom";
        if (excludeId.HasValue) sql += " AND id != @id";

        using var cmd = new MySqlCommand(sql, DatabaseHelper.GetConnection());
        cmd.Parameters.AddWithValue("@nom", nom);
        if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
```

---

## 🖥️ Formulaires Windows Forms

### `FrmCommandes` — Liste des commandes (le plus complexe, grosse valeur d'exam)

**Contrôles :**
- `DataGridView dgvCommandes` — liste des commandes
- `ComboBox cmbStatut` — filtre par statut
- `DateTimePicker dtpDebut / dtpFin` — filtre par date
- `TextBox txtRecherche` — recherche par nom client
- `Button btnVoir` — ouvrir le détail
- `Button btnChgStatut` — changer le statut de la commande

**Affichage avec jointure :**
```sql
SELECT c.id, u.nom, u.prenom, u.email,
       c.date_commande, c.statut, c.total_ttc, c.ville_livraison
FROM commandes c
INNER JOIN utilisateurs u ON c.id_client = u.id
WHERE (@statut = '' OR c.statut = @statut)
  AND c.date_commande BETWEEN @debut AND @fin
  AND (u.nom LIKE @search OR u.prenom LIKE @search)
ORDER BY c.date_commande DESC
```

---

### `FrmProduitEdit` — Ajout / Modification Produit

**Contrôles d'édition :**
- `TextBox txtNom`
- `RichTextBox rtbDescription`
- `NumericUpDown nudPrix` (décimal, min 0)
- `NumericUpDown nudStock` (entier, min 0)
- `CheckBox chkConfigurable`
- `NumericUpDown nudCapacite` (activé seulement si configurable coché)
- `CheckBox chkDisponible`

**Contrôles de sélection :**
- `ComboBox cmbCategorie` (rempli depuis BDD)
- `CheckedListBox clbParfums` (visible si configurable)

**Validation avant sauvegarde :**
```csharp
private bool Valider()
{
    errorProvider.Clear();
    bool ok = true;

    if (string.IsNullOrWhiteSpace(txtNom.Text)) {
        errorProvider.SetError(txtNom, "Le nom est obligatoire.");
        ok = false;
    }
    if (nudPrix.Value <= 0) {
        errorProvider.SetError(nudPrix, "Le prix doit être positif.");
        ok = false;
    }
    if (cmbCategorie.SelectedIndex < 0) {
        errorProvider.SetError(cmbCategorie, "Choisissez une catégorie.");
        ok = false;
    }
    return ok;
}
```

---

### Passage de données entre formulaires

```csharp
// Dans FrmProduits — clic sur "Modifier"
private void btnModifier_Click(object sender, EventArgs e)
{
    if (dgvProduits.SelectedRows.Count == 0) return;
    int id = (int)dgvProduits.SelectedRows[0].Cells["id"].Value;
    Produit p = _produitDAL.GetById(id);

    using var frm = new FrmProduitEdit(p);  // Passe l'objet au formulaire
    if (frm.ShowDialog() == DialogResult.OK)
    {
        ChargerProduits();  // Recharge la liste
    }
}

// Dans FrmProduitEdit — constructeur
public FrmProduitEdit(Produit produit = null)
{
    InitializeComponent();
    _produit = produit;
    _estModification = produit != null;
    Text = _estModification ? "Modifier un produit" : "Ajouter un produit";
}
```

---

## 🧱 Modèles ArtisaStock (nouvelles classes C#)

```csharp
// Models/Ingredient.cs
public class Ingredient
{
    public int     Id             { get; set; }
    public string  Nom            { get; set; }
    public string  Marque         { get; set; }
    public string  UniteMesure    { get; set; }   // "kg", "g", "l", "ml", "piece"
    public decimal PrixUnitaire   { get; set; }
    public decimal StockActuel    { get; set; }
    public decimal? SeuilAlerte   { get; set; }
    public string  Fournisseur    { get; set; }   // Jointure optionnelle

    // Propriété calculée (POO)
    public bool EstEnAlerte => SeuilAlerte.HasValue && StockActuel <= SeuilAlerte.Value;

    public override string ToString() => $"{Nom} ({StockActuel} {UniteMesure})";
}

// Models/Recette.cs
public class Recette
{
    public int    Id                  { get; set; }
    public string Nom                 { get; set; }
    public string Description         { get; set; }
    public decimal RendementQuantite  { get; set; }
    public int?   TempsPreparation    { get; set; }
    public bool   Active              { get; set; }

    // Navigation
    public List<RecetteIngredient> Ingredients { get; set; } = new();

    // Propriété calculée — coût total d'une batch
    public decimal CoutBatch =>
        Ingredients.Sum(ri => ri.Quantite * ri.PrixUnitaire);

    // Coût par pièce
    public decimal CoutUnitaire =>
        RendementQuantite > 0 ? CoutBatch / RendementQuantite : 0;
}

// Models/RecetteIngredient.cs
public class RecetteIngredient
{
    public int     IdRecette     { get; set; }
    public int     IdIngredient  { get; set; }
    public string  NomIngredient { get; set; }   // Jointure
    public string  Unite         { get; set; }   // Jointure
    public decimal Quantite      { get; set; }
    public decimal PrixUnitaire  { get; set; }   // Jointure

    public decimal SousTotal => Quantite * PrixUnitaire;
}

// Models/Production.cs
public class Production
{
    public int      Id                { get; set; }
    public int      IdRecette         { get; set; }
    public string   NomRecette        { get; set; }   // Jointure
    public DateTime DateProduction    { get; set; }
    public decimal  QuantiteProduite  { get; set; }
    public decimal  CoutTotal         { get; set; }
    public decimal  CoutUnitaire      { get; set; }
    public decimal? PrixVenteReel     { get; set; }
    public decimal? MargeBrute        { get; set; }
    public string   Notes             { get; set; }

    public decimal? MargePercent =>
        PrixVenteReel > 0 && MargeBrute.HasValue
            ? Math.Round((MargeBrute.Value / (PrixVenteReel.Value * QuantiteProduite)) * 100, 1)
            : null;
}
```

---

## 🖥️ FrmRecetteEdit — Le formulaire le plus riche (cœur du PDSGBD)

C'est ici que se concentrent le plus de critères d'examen :

```
┌─────────────────────────────────────────────────────────────────┐
│  Ajouter / Modifier une recette                                  │
├──────────────────────────────────────┬──────────────────────────┤
│  Nom de la recette :  [____________] │  Rendement : [20] pièces │
│  Description :        [____________] │  Temps prépa: [45] min   │
│  [x] Recette active                  │                          │
├──────────────────────────────────────┴──────────────────────────┤
│  Ingrédients de la recette                                       │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Sélectionner un ingrédient : [ComboBox▼] Qté: [____] [+]│   │
│  ├──────────┬───────────────┬──────────┬────────────────────┤   │
│  │ Ingrédient│ Unité        │ Quantité │ Sous-total  [suppr]│   │
│  ├──────────┼───────────────┼──────────┼────────────────────┤   │
│  │ Chocolat │ kg            │ 0,200    │ 2,20 €      [X]   │   │
│  │ Praliné  │ kg            │ 0,150    │ 2,70 €      [X]   │   │
│  └──────────┴───────────────┴──────────┴────────────────────┘   │
│                                                                   │
│  Coût de revient : 4,90 € / batch (20 pièces) = 0,245 €/pièce  │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  │
│                        [Annuler]   [Enregistrer]                 │
└─────────────────────────────────────────────────────────────────┘
```

**Critères couverts par ce seul formulaire** :
- Contrôles d'édition (TextBox, NumericUpDown)
- Contrôles de sélection (ComboBox pour ingrédients)
- Ajout dynamique de lignes → recalcul du coût en temps réel
- Validation : nom non vide, au moins 1 ingrédient, quantités > 0
- Unicité du nom
- INSERT + liaison recettes_ingredients

---

## 🖥️ FrmProduction — Workflow de production

```
┌─────────────────────────────────────────────────────────────────┐
│  Lancer une Production                                           │
├─────────────────────────────────────────────────────────────────┤
│  Recette : [ComboBox ▼ — Praline Praliné        ]               │
│  Quantité à produire : [40] pièces                              │
│                                                                   │
│  ── Vérification du stock ──────────────────────────────────    │
│  ┌───────────────┬────────────┬─────────────┬──────────────┐   │
│  │ Ingrédient    │ En stock   │ Nécessaire  │ Statut       │   │
│  ├───────────────┼────────────┼─────────────┼──────────────┤   │
│  │ Chocolat lait │ 4,000 kg   │ 0,400 kg    │ ✅ OK        │   │
│  │ Praliné       │ 0,200 kg   │ 0,300 kg    │ ❌ MANQUANT  │   │
│  │ Beurre        │ 2,000 kg   │ 0,060 kg    │ ✅ OK        │   │
│  └───────────────┴────────────┴─────────────┴──────────────┘   │
│                                                                   │
│  Coût total estimé : 8,64 €    Coût/pièce : 0,216 €             │
│  Prix de vente réel (optionnel) : [______] €                     │
│                                                                   │
│  ⚠️ Stock insuffisant pour Praliné. Commandez avant de produire. │
│                                                                   │
│            [Annuler]   [✅ Valider et produire]  (grisé si ❌)   │
└─────────────────────────────────────────────────────────────────┘
```

**Logique C# du bouton "Valider et produire"** :
```csharp
private void btnProduire_Click(object sender, EventArgs e)
{
    // 1. Vérifier que tous les ingrédients sont OK
    if (!_verificationOK)
    {
        MessageBox.Show("Stock insuffisant pour produire cette quantité.");
        return;
    }

    // 2. Confirmation
    var result = MessageBox.Show(
        $"Produire {nudQuantite.Value} {recette.Nom} ?\n" +
        $"Coût total : {coutTotal:F2} €\n" +
        $"Cette action déduira les ingrédients du stock.",
        "Confirmer la production",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question
    );
    if (result != DialogResult.OK) return;

    // 3. Exécuter la production (INSERT + UPDATE stock x N ingrédients)
    try
    {
        _productionDAL.LancerProduction(_recette, (decimal)nudQuantite.Value, prixVente);
        MessageBox.Show("Production enregistrée avec succès !");
        this.DialogResult = DialogResult.OK;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erreur lors de la production : {ex.Message}");
    }
}
```

---

## 📸 Upload d'images via Cloudinary

Les images produits sont gérées depuis l'app C# et stockées sur Cloudinary. L'URL générée est sauvée dans `produits.image_url`.

**NuGet à installer** : pas de SDK officiel Cloudinary pour .NET, on utilise `HttpClient` avec l'API REST.

```csharp
// CloudinaryHelper.cs
using System.Net.Http;
using System.Net.Http.Headers;

public static class CloudinaryHelper
{
    private static readonly string CloudName = "VOTRE_CLOUD_NAME";
    private static readonly string ApiKey    = "VOTRE_API_KEY";
    private static readonly string ApiSecret = "VOTRE_API_SECRET";

    // Retourne l'URL Cloudinary après upload
    public static async Task<string> UploadImageAsync(string cheminFichierLocal)
    {
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string signature = GenererSignature(timestamp);

        using var client  = new HttpClient();
        using var content = new MultipartFormDataContent();

        var imageBytes = await File.ReadAllBytesAsync(cheminFichierLocal);
        content.Add(new ByteArrayContent(imageBytes), "file", Path.GetFileName(cheminFichierLocal));
        content.Add(new StringContent(ApiKey),    "api_key");
        content.Add(new StringContent(timestamp), "timestamp");
        content.Add(new StringContent(signature), "signature");
        content.Add(new StringContent("charlesnadejda/produits"), "folder");

        var response = await client.PostAsync(
            $"https://api.cloudinary.com/v1_1/{CloudName}/image/upload", content);

        var json = await response.Content.ReadAsStringAsync();
        // Parser la réponse JSON pour extraire "secure_url"
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        return result.secure_url;
    }

    private static string GenererSignature(string timestamp)
    {
        string str = $"folder=charlesnadejda/produits&timestamp={timestamp}{ApiSecret}";
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
```

**Utilisation dans `FrmProduitEdit`** :

```csharp
private async void btnChoisirImage_Click(object sender, EventArgs e)
{
    using var dialog = new OpenFileDialog();
    dialog.Filter = "Images|*.jpg;*.jpeg;*.png;*.webp";

    if (dialog.ShowDialog() == DialogResult.OK)
    {
        btnChoisirImage.Enabled = false;
        lblImageStatus.Text = "Upload en cours...";

        try
        {
            string url = await CloudinaryHelper.UploadImageAsync(dialog.FileName);
            _imageUrl = url;
            picApercu.ImageLocation = url;   // aperçu dans le formulaire
            lblImageStatus.Text = "Image uploadée ✓";
        }
        catch (Exception ex)
        {
            lblImageStatus.Text = "Erreur : " + ex.Message;
        }
        finally
        {
            btnChoisirImage.Enabled = true;
        }
    }
}
```

**NuGet supplémentaire** : `Newtonsoft.Json` pour parser la réponse Cloudinary.

---

## 🌐 Langue du site (i18n) — Impact sur les données

Le site est en **Français + Anglais**. Cela n'affecte pas la structure BDD (les noms de produits restent en une seule langue dans la BDD). Les traductions sont gérées côté Laravel avec les fichiers de langue. L'app C# travaille uniquement en français.

---

## 🚀 Checklist de développement PDSGBD

### Setup
- [ ] Créer solution Visual Studio `CharlesNadejda.sln`
- [ ] Installer NuGet : `MySql.Data` (Connector/NET)
- [ ] Créer `DatabaseHelper.cs` + tester la connexion

### Modèles
- [ ] `Categorie.cs`, `Parfum.cs`, `Produit.cs`
- [ ] `Ingredient.cs`, `Fournisseur.cs`
- [ ] `Recette.cs`, `RecetteIngredient.cs`
- [ ] `Production.cs`, `MouvementStock.cs`
- [ ] `Commande.cs`, `LigneCommande.cs`

### DAL (dans cet ordre)
- [ ] `DatabaseHelper.cs`
- [ ] `CategorieDAL.cs` : GetAll
- [ ] `ParfumDAL.cs` : GetAll, Insert, Update, Delete
- [ ] `ProduitDAL.cs` : GetAll (jointure cat), Insert, Update, Delete
- [ ] `FournisseurDAL.cs` : GetAll
- [ ] `IngredientDAL.cs` : GetAll (jointure fournisseur), Insert, Update, Delete, UpdateStock
- [ ] `RecetteDAL.cs` : GetAll (coût calculé), GetById (avec ingrédients), Insert, Update, Delete
- [ ] `ProductionDAL.cs` : LancerProduction (transaction), GetAll (jointure)
- [ ] `MouvementStockDAL.cs` : GetByIngredient (filtres), Insert
- [ ] `CommandeDAL.cs` : GetAll (jointures), GetDetail, UpdateStatut

### Formulaires — Module Catalogue
- [ ] `FrmPrincipal` : MenuStrip avec 5 menus
- [ ] `FrmCategories` + `FrmCategorieEdit`
- [ ] `FrmParfums` + `FrmParfumEdit`
- [ ] `FrmProduits` + `FrmProduitEdit`

### Formulaires — Module Ingrédients
- [ ] `FrmIngredients` (DataGridView avec alertes colorées)
- [ ] `FrmIngredientEdit`
- [ ] `FrmAchatStock`
- [ ] `FrmMouvementsStock` (filtres date + type)

### Formulaires — Module Recettes
- [ ] `FrmRecettes` (liste avec coût de revient)
- [ ] `FrmRecetteEdit` (ajout dynamique d'ingrédients)

### Formulaires — Module Production
- [ ] `FrmProduction` (vérif stock + workflow)
- [ ] `FrmHistoriqueProductions`

### Formulaires — Module Commandes
- [ ] `FrmCommandes` + `FrmCommandeDetail`

### Points clés à soigner partout
- [ ] Toutes les requêtes avec `AddWithValue` (anti-injection)
- [ ] `ErrorProvider` sur tous les formulaires d'édition
- [ ] Messages d'erreur explicites en cas de violation FK (try/catch)
- [ ] Confirmations avant suppression (`MessageBox.Show`)
- [ ] Tri et filtrage fonctionnels dans les DataGridView
- [ ] Coloration conditionnelle : rouge si stock bas, vert/orange/rouge pour marges
