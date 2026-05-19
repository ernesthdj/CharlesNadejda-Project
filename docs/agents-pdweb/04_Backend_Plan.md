# Agent #4 — Backend Developer
## Plan d'implémentation Backend — Boutique Web ArtisaStock
> Date : 2026-05-19
> Input consommé : Brainstorm + 01_PO + 02_Architect + 03_UIUX
> Phase : Définition

---

## 1. Périmètre Backend

### Laravel (PHP)
- Migration SQL v15 (5 tables)
- 6 Models Eloquent + relations
- 6 Controllers (Auth×2, Catalogue, Panier, Commande, Profil)
- 3 FormRequests (validation serveur)
- 1 Middleware (ClientAuth)
- Routes web.php

### ArtisaStock C# (Mini CMS)
- 4 Models (CategorieWeb, ProduitWeb, CommandeWeb, CommandeWebLigne)
- 3 DAL (CategorieWebDAL, ProduitWebDAL, CommandeWebDAL)
- 3 Forms (FrmBoutiqueWeb, FrmCategorieWebEdit, FrmProduitWebEdit)
- Navigation (NavItemId + ScreenId + SidebarPanel)

---

## 2. Migration SQL v15

```sql
-- Fichier : sql/migration_v15_boutique_web.sql
-- Exécuter APRÈS toutes les migrations v01-v14

CREATE TABLE IF NOT EXISTS categories_web (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(150)    NOT NULL UNIQUE,
    description     TEXT,
    ordre_affichage INT             DEFAULT 0,
    actif           TINYINT(1)      DEFAULT 1,
    date_creation   DATETIME        DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS clients (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    nom               VARCHAR(100)    NOT NULL,
    prenom            VARCHAR(100)    NOT NULL,
    email             VARCHAR(255)    NOT NULL UNIQUE,
    mot_de_passe      VARCHAR(255)    NOT NULL,
    telephone         VARCHAR(20),
    adresse_rue       VARCHAR(255),
    adresse_cp        VARCHAR(10),
    adresse_ville     VARCHAR(100),
    adresse_pays      VARCHAR(100)    DEFAULT 'Belgique',
    actif             TINYINT(1)      DEFAULT 1,
    date_creation     DATETIME        DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME        DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS produits_web (
    id                INT AUTO_INCREMENT PRIMARY KEY,
    id_bom_fiche      INT             NOT NULL,
    id_categorie      INT,
    nom_commercial    VARCHAR(200)    NOT NULL,
    description       TEXT,
    prix_vente        DECIMAL(10,2)   NOT NULL,
    image_path        VARCHAR(500),
    en_vente          TINYINT(1)      DEFAULT 1,
    ordre_affichage   INT             DEFAULT 0,
    date_creation     DATETIME        DEFAULT CURRENT_TIMESTAMP,
    date_modification DATETIME        DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_prodweb_bomfiche
        FOREIGN KEY (id_bom_fiche) REFERENCES bom_fiches(id) ON DELETE RESTRICT,
    CONSTRAINT fk_prodweb_categorie
        FOREIGN KEY (id_categorie) REFERENCES categories_web(id) ON DELETE SET NULL,
    CONSTRAINT uk_prodweb_fiche
        UNIQUE (id_bom_fiche)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS commandes_web (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_client           INT             NOT NULL,
    statut              ENUM('panier','payee','annulee')
                                        DEFAULT 'panier',
    total_ttc           DECIMAL(10,2)   DEFAULT 0.00,
    adresse_livraison   TEXT,
    date_commande       DATETIME,
    date_creation       DATETIME        DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_cmdweb_client
        FOREIGN KEY (id_client) REFERENCES clients(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS commandes_web_lignes (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    id_commande     INT             NOT NULL,
    id_produit_web  INT             NOT NULL,
    quantite        INT             NOT NULL DEFAULT 1,
    prix_unitaire   DECIMAL(10,2)   NOT NULL,
    sous_total      DECIMAL(10,2)   GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,

    CONSTRAINT fk_cmdligne_cmd
        FOREIGN KEY (id_commande) REFERENCES commandes_web(id) ON DELETE CASCADE,
    CONSTRAINT fk_cmdligne_prodweb
        FOREIGN KEY (id_produit_web) REFERENCES produits_web(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

## 3. Controllers Laravel — Logique métier

### RegisterController
```
showForm()     → vue register avec old() input
register()     → FormRequest validation → password_hash(BCRYPT)
                → INSERT clients → session(['client_id', 'client_nom'])
                → session()->regenerate() → redirect catalogue
```

### LoginController
```
showForm()     → vue login
login()        → SELECT client by email → password_verify()
                → session(['client_id', 'client_nom', 'client_prenom'])
                → session()->regenerate() → redirect intended
logout()       → session()->flush() → redirect catalogue
```

### CatalogueController
```
index()        → ProduitWeb::where('en_vente', 1)
                  ->with('categorie')
                  ->orderBy('ordre_affichage')
                  filtre optionnel par categorie_id (query param)
                  tri optionnel par prix (query param)
                  calcul stock via accessor getStockDisponibleAttribute()

show($id)      → ProduitWeb::findOrFail($id)
                  avec stock_disponible calculé
                  avec categorie eager loaded

parCategorie() → idem index() avec WHERE id_categorie = $id
```

### PanierController (AJAX)
```
index()        → GET panier actif du client (statut='panier')
                  avec lignes + produits eager loaded

ajouter()      → POST {id_produit, quantite} — JSON response
                  1. Vérifier stock dispo ≥ quantité demandée
                  2. Get ou create commande statut='panier'
                  3. Si produit déjà dans le panier → incrémente quantité
                     Sinon → INSERT ligne avec prix snapshot
                  4. Return JSON {success, panier_count, message}

updateQuantite() → PATCH {id_ligne, quantite} — JSON response
                   1. Vérifier stock ≥ nouvelle quantité
                   2. UPDATE commandes_web_lignes.quantite
                   3. Return JSON {success, sous_total, total}

supprimer()    → DELETE {id_ligne} — JSON response
                  1. DELETE commandes_web_lignes
                  2. Return JSON {success, panier_count, total}

count()        → GET — JSON response {count}
                  (pour le badge header AJAX)
```

### CommandeController
```
recap()        → GET panier actif avec lignes + adresse client pré-remplie

valider()      → POST — transaction FIFO (voir Architect algo)
                  1. DB::beginTransaction()
                  2. Pour chaque ligne : lockForUpdate() bom_stocks FIFO
                  3. Décrémenter → si échec : rollback + flash error
                  4. UPDATE commande statut='payee', date, adresse, total
                  5. DB::commit()
                  6. Redirect confirmation

historique()   → GET commandes du client (statut != 'panier') orderBy date DESC

detail($id)    → GET commande avec lignes, vérifie ownership (client_id = session)
```

### ProfilController
```
edit()         → GET client depuis session, formulaire pré-rempli
update()       → PUT FormRequest validation → UPDATE clients
                  Si nouveau mot de passe fourni → hash + update
```

---

## 4. FormRequests — Validation serveur

### RegisterRequest
```php
'prenom'       => 'required|string|max:100',
'nom'          => 'required|string|max:100',
'email'        => 'required|email|max:255|unique:clients,email',
'password'     => 'required|string|min:8|confirmed',
'telephone'    => 'nullable|string|max:20',
'adresse_rue'  => 'nullable|string|max:255',
'adresse_cp'   => 'nullable|string|max:10',
'adresse_ville'=> 'nullable|string|max:100',
'adresse_pays' => 'nullable|string|max:100',
```

### LoginRequest
```php
'email'    => 'required|email',
'password' => 'required|string',
```

### ProfilUpdateRequest
```php
'prenom'       => 'required|string|max:100',
'nom'          => 'required|string|max:100',
'telephone'    => 'nullable|string|max:20',
'adresse_rue'  => 'nullable|string|max:255',
'adresse_cp'   => 'nullable|string|max:10',
'adresse_ville'=> 'nullable|string|max:100',
'adresse_pays' => 'nullable|string|max:100',
'password'     => 'nullable|string|min:8|confirmed',  // vide = pas de changement
```

---

## 5. Plan C# — Mini CMS

### Nouveaux Models

```csharp
public class CategorieWeb {
    public int Id, string Nom, string Description,
    int OrdreAffichage, bool Actif, DateTime DateCreation
}

public class ProduitWeb {
    public int Id, int IdBomFiche, int? IdCategorie,
    string NomCommercial, string Description, decimal PrixVente,
    string ImagePath, bool EnVente, int OrdreAffichage
    // Jointures : NomFiche, NomCategorie, StockDisponible (calculé)
}

public class CommandeWeb {
    public int Id, int IdClient, string Statut,
    decimal TotalTtc, string AdresseLivraison, DateTime? DateCommande
    // Jointures : NomClient, EmailClient, NbArticles
    public List<CommandeWebLigne> Lignes
}

public class CommandeWebLigne {
    public int Id, int IdCommande, int IdProduitWeb,
    int Quantite, decimal PrixUnitaire, decimal SousTotal
    // Jointures : NomProduit
}
```

### Nouveaux DAL

**CategorieWebDAL :**
- `List<CategorieWeb> GetAll(bool includeInactifs = false)`
- `CategorieWeb GetById(int id)`
- `bool NomExiste(string nom, int excludeId = 0)`
- `int Insert(CategorieWeb c)`
- `void Update(CategorieWeb c)`
- `void Delete(int id)` — vérifie pas de produits liés (ou SET NULL)

**ProduitWebDAL :**
- `List<ProduitWeb> GetAll()` — avec stock calculé (LEFT JOIN + SUM bom_stocks)
- `ProduitWeb GetById(int id)`
- `List<BomFiche> GetFichesNonPubliees()` — fiches BOM sans entrée dans produits_web
- `int Insert(ProduitWeb p)`
- `void Update(ProduitWeb p)`
- `void ToggleEnVente(int id)` — toggle publié/dépublié
- `void Delete(int id)` — RESTRICT si commandes_web_lignes existent

**CommandeWebDAL (lecture seule) :**
- `List<CommandeWeb> GetAll(string filtreStatut = null)`
- `CommandeWeb GetById(int id)` — avec lignes + client

### Navigation

```csharp
// NavItemId.cs — ajouter :
BoutiqueWeb

// ScreenId.cs — ajouter :
BoutiqueWeb

// ScreenRouter — ajouter mapping :
NavItemId.BoutiqueWeb → ScreenId.BoutiqueWeb

// SidebarPanel — ajouter dans le groupe "Workflow" ou créer un 4e groupe :
// Icône 🌐 ou 🛒 — "Boutique en ligne"
```

---

## 6. Gestion des images — Workflow complet

```
C# (Admin) :
1. FrmProduitWebEdit → OpenFileDialog (filtre : jpg, png, webp)
2. Copie vers : {cheminProjetLaravel}/storage/app/public/produits/{id}_{timestamp}.{ext}
3. Sauvegarde en DB : image_path = "produits/{id}_{timestamp}.{ext}"

Laravel (Web) :
1. php artisan storage:link (à faire une fois)
2. Dans Blade : <img src="{{ asset('storage/' . $produit->image_path) }}" />

Fallback : si pas d'image → placeholder SVG ou image par défaut
```

**Chemin Laravel configurable dans App.config C# :**
```xml
<appSettings>
    <add key="LaravelStoragePath" value="C:\...\site-laravel\storage\app\public\" />
</appSettings>
```

---

## 7. Ordre d'implémentation Backend

| Étape | Tâche | Dépendance | US |
|-------|-------|------------|-----|
| 1 | Migration SQL v15 | Aucune | Toutes |
| 2 | Models C# (CategorieWeb, ProduitWeb) | v15 | W10, W11 |
| 3 | DAL C# (CategorieWebDAL, ProduitWebDAL) | Models | W10, W11 |
| 4 | FrmBoutiqueWeb onglet Catégories | DAL | W10 |
| 5 | FrmBoutiqueWeb onglet Produits + FrmProduitWebEdit | DAL | W11 |
| 6 | `php artisan storage:link` + config .env MySQL | Aucune | — |
| 7 | Models Laravel (Client, ProduitWeb, etc.) | v15 | Toutes |
| 8 | Middleware ClientAuth | — | W02-W09 |
| 9 | RegisterController + LoginController | Models | W01, W02, W03 |
| 10 | CatalogueController | Models | W04, W05 |
| 11 | PanierController (AJAX) | Models | W06 |
| 12 | CommandeController (TX FIFO) | Models | W07, W08 |
| 13 | ProfilController | Models | W09 |
| 14 | CommandeWebDAL C# + onglet Commandes | Après premières commandes | W12 |

---

## JOURNAL — Agent #4 Backend

**Phase :** Définition
**Itération :** 1
**Entrée consommée :** Brainstorm + PO + Architect + UI/UX
**Output produit :** Migration SQL v15 complète, logique 6 controllers Laravel, 3 FormRequests, plan C# (4 models, 3 DAL, navigation), workflow images, ordre d'implémentation
**Décisions clés :**
- Migration SQL brute (pas Eloquent migration) pour cohérence avec les v01-v14 existantes
- Panier = commande statut 'panier' (pas de table séparée) → simplifie le code
- `lockForUpdate()` pour TOCTOU → à tester en local
- Image path en DB = relatif (`produits/xxx.jpg`), jamais absolu
- Chemin Laravel dans App.config C# → configurable sans recompiler
**Selfdoubt appliqué :**
- ✅ Certain : SQL migration, models, DAL — patterns identiques aux existants
- ✅ Certain : FormRequest validation — doc Laravel 11 vérifiée
- ⚠️ Probable : colonne GENERATED ALWAYS (sous_total) compatible MySQL 9.6 (à tester)
- ⚠️ Probable : `session()->regenerate()` après login protège contre session fixation (doc Laravel confirme)
**Impact :** Plan complet, prêt à coder. 14 étapes séquentielles sans ambiguïté
**Alerte agent suivant :** Le Frontend doit s'assurer que les routes AJAX (panier) retournent bien du JSON. Le QA doit vérifier la cohérence migration SQL ↔ Models Eloquent ↔ Models C#.
