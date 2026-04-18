# Sauvegarde Session 0 — Charles & Nadejda
> Reprise de contexte pour nouvelle instance Claude (terminal Visual Studio)
> Date : 29 mars 2026

---

## Contexte du projet

Projet académique double répondant à deux examens (défense orale fin juin 2026) :

| Examen | Techno | Minimum requis |
|--------|--------|----------------|
| **PDWEB** | Laravel 11 + PHP 8.3 + MySQL | PHP, sessions, formulaires, MySQL, AJAX |
| **PDSGBD** | C# Windows Forms + MySQL | CRUD complet, jointures, requêtes paramétrées, POO |

Les deux apps partagent la **même base MySQL** `charlesnadejda` (Docker).

**Sujet** : Site pâtisserie-chocolaterie artisanale + ERP desktop pour les parents (Charles & Nadejda, Bruxelles).

---

## Infrastructure (Phase 0 — TERMINÉE ✅)

### Docker (4 containers actifs)
```
docker compose up -d   ← depuis la racine du projet
```
| Service | URL / Port |
|---------|------------|
| MySQL 8 | localhost:3306 (root / root) |
| Laravel (PHP-FPM) | interne |
| Nginx | http://localhost:8000 |
| phpMyAdmin | http://localhost:8080 |

La DB `charlesnadejda` contient **42 tables** déjà créées (create_database.sql + seed_data.sql exécutés au 1er démarrage Docker).

### Laravel
- Scaffoldé dans `site-laravel/` (Laravel 13.2, PHP 8.3)
- `.env` configuré : `DB_HOST=mysql`, `DB_DATABASE=charlesnadejda`, `DB_USERNAME=root`, `DB_PASSWORD=root`
- `php artisan migrate` exécuté — tables internes Laravel créées en DB
- **Prochaine étape Laravel : Phase 3** (après Phase 1 & 2 C#)

### App C# Windows Forms
- Solution créée dans `app-csharp/CharlesNadejda/`
- .NET Framework 4.8.1
- NuGet installés : `MySql.Data`, `BCrypt.Net-Next`, `Newtonsoft.Json`
- `App.config` configuré :
  ```xml
  <connectionStrings>
    <add name="charlesnadejda"
         connectionString="Server=localhost;Port=3306;Database=charlesnadejda;Uid=root;Pwd=root;"
         providerName="MySql.Data.MySqlClient"/>
  </connectionStrings>
  <appSettings>
    <add key="CloudinaryCloudName" value="REMPLACER"/>
    <add key="CloudinaryApiKey"    value="REMPLACER"/>
    <add key="CloudinaryApiSecret" value="REMPLACER"/>
  </appSettings>
  ```
- Dossiers créés : `Models/`, `DAL/`, `Forms/`

---

## Phase 1 — App C# : Fondations (EN COURS ⏳)

### Objectif
Login BCrypt fonctionnel → ouvre FrmPrincipal.
Critères d'examen couverts : connexion DB, POO, contrôles de formulaire.

### Fichiers à créer (dans cet ordre)

#### 1. `Models/Utilisateur.cs`
```csharp
public class Utilisateur
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public override string ToString() => $"{Prenom} {Nom} ({Role})";
}
```

#### 2. `DAL/DbHelper.cs`
```csharp
using MySql.Data.MySqlClient;
using System.Configuration;

public static class DbHelper
{
    public static MySqlConnection GetConnection()
    {
        string cs = ConfigurationManager.ConnectionStrings["charlesnadejda"].ConnectionString;
        var conn = new MySqlConnection(cs);
        conn.Open();
        return conn;
    }
}
```

#### 3. `DAL/UtilisateurDAL.cs`
```csharp
// SELECT * FROM utilisateurs WHERE email = @email AND actif = 1
// Puis BCrypt.Net.BCrypt.Verify(motDePasse, row["mot_de_passe"])
// ET row["role"] == "admin"
// Retourne Utilisateur ou null
public static Utilisateur Authenticate(string email, string motDePasse)
```

#### 4. `Forms/FrmLogin.cs`
Contrôles :
- `txtEmail` (TextBox)
- `txtMotDePasse` (TextBox, PasswordChar = '•')
- `btnConnexion` (Button)
- `lblErreur` (Label, rouge, caché par défaut)

Logique `btnConnexion_Click` :
```
Utilisateur u = UtilisateurDAL.Authenticate(txtEmail.Text, txtMotDePasse.Text)
si u != null → ouvrir FrmPrincipal(u), fermer FrmLogin
sinon → afficher lblErreur "Email ou mot de passe incorrect"
```

#### 5. `Forms/FrmPrincipal.cs`
- MDI parent ou form avec MenuStrip
- Menu : Catalogue | Stock | Commandes | (nom utilisateur connecté)
- Reçoit `Utilisateur` au constructeur

### Credentials de test (seed_data.sql)
```
Email    : charles@charlesnadejda.be
Password : password   (hash BCrypt en DB)
Role     : admin
```

### Validation Phase 1
- Login correct → FrmPrincipal s'ouvre avec le nom de Charles
- Login incorrect → message d'erreur, pas de crash
- Mauvais email (inexistant) → même message générique

---

## Phase 2 — App C# : Catalogue (APRÈS Phase 1)

Fichiers : `CategorieDAL`, `ParfumDAL`, `ProduitDAL`, `CloudinaryHelper`
Forms : `FrmCategories`, `FrmCategorieEdit`, `FrmParfums`, `FrmParfumEdit`, `FrmProduits`, `FrmProduitEdit`

**Critères d'examen clés à démontrer :**
- DataGridView avec jointure (produit + catégorie)
- CRUD complet avec validation + ErrorProvider
- Requêtes paramétrées (`cmd.Parameters.AddWithValue`)
- Unicité à l'ajout ET à la modification
- Suppression avec confirmation + gestion FK

---

## Règles importantes (ne jamais déroger)

1. **Toutes les requêtes SQL** → `cmd.Parameters.AddWithValue("@param", valeur)` — jamais de concaténation
2. **Toute opération multi-tables** → `MySqlTransaction` (commit/rollback)
3. **Nommage tables** : `utilisateurs` (pas `users`), `mot_de_passe` (pas `password`)
4. **BCrypt** : `BCrypt.Net.BCrypt.Verify(inputMdp, hashEnDB)`
5. **App.config** : connection string name = `"charlesnadejda"`

---

## Structure dossiers projet

```
CharlesNadejda_Project/
├── docs/                    ← Documentation complète (00 à 09)
├── sql/
│   ├── create_database.sql  ✅
│   └── seed_data.sql        ✅
├── docker-compose.yml       ✅
├── docker/php/Dockerfile    ✅
├── docker/nginx/default.conf ✅
├── site-laravel/            ✅ Laravel scaffoldé
└── app-csharp/CharlesNadejda/ ✅ Solution VS créée
    ├── App.config           ✅
    ├── Models/              ✅ (vide)
    ├── DAL/                 ✅ (vide)
    └── Forms/               ✅ (vide)
```

---

## Commande utile depuis terminal VS

Pour vérifier que Docker tourne avant de coder :
```bash
docker compose -f "C:\Users\ernes\Desktop\Bachelier Informatique\Projects Personnels\CharlesNadejda_Project\docker-compose.yml" ps
```
