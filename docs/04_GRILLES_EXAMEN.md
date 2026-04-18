# 04 — Mapping Grilles d'Examen ↔ Fonctionnalités du Projet

> Ce document fait le lien direct entre **chaque critère de notation** et **où/comment il est couvert** dans le projet.
> À relire avant l'examen oral pour préparer les démonstrations.

---

## ✅ PDWEB — Grille de Notation

### Critère 1 : PHP obligatoire
**Couvert par** : 100% du site web
**Où le montrer** : N'importe quelle page, le traitement des formulaires

---

### Critère 2 : Formulaire avec gestion cohérente et conviviale des erreurs (/10)
**Couvert par** :
- `pages/inscription.php` — validation de 7 champs : nom, prénom, email (format + unicité), mot de passe (longueur), confirmation (identique), téléphone
- `pages/connexion.php` — email + mot de passe, message générique en cas d'échec
- `pages/contact.php` — nom, email, sujet, message
- `pages/checkout.php` — formulaire de commande (10+ champs)

**Points importants à montrer** :
- Les messages d'erreur s'affichent **à côté de chaque champ** (pas juste en haut de page)
- La valeur saisie est **conservée** après soumission invalide (repopulation du formulaire)
- Les erreurs sont claires et en français
- Validation à la fois côté PHP (obligatoire) et JS (UX)

**Exemple de code à préparer** :
```php
// Affichage d'une erreur sous un champ
<input type="email" name="email"
       value="<?= htmlspecialchars($_POST['email'] ?? '') ?>">
<?php if (isset($erreurs['email'])): ?>
    <span class="erreur"><?= $erreurs['email'] ?></span>
<?php endif; ?>
```

---

### Critère 3 : Sessions (/10)
**Couvert par** :
- **Panier** (`$_SESSION['panier']`) — persistant sur tout le site, survit à la navigation
- **Authentification** (`$_SESSION['user_id']`, `$_SESSION['user_role']`) — connexion et maintien de l'état

**Points importants à montrer** :
- Ajouter un produit au panier → naviguer vers une autre page → le panier est toujours là
- Se connecter → le nom de l'utilisateur s'affiche dans le header
- Se déconnecter → `session_destroy()` → plus accès à l'espace client
- Flash messages (`$_SESSION['flash']`) — affiché une fois puis effacé

**Utilités des sessions à expliquer à l'oral** :
- Maintenir un état entre les requêtes HTTP (qui sont stateless par nature)
- Éviter de retransmettre les données sensibles à chaque requête
- Stocker des données temporaires (panier) sans base de données

---

### Critère 4 : Contenus distincts selon l'identification
**Couvert par** :
- **Visiteur non connecté** : voit le catalogue, peut voir le panier, mais ne peut pas commander → redirect vers connexion
- **Client connecté** : accède à `mon-compte.php` (historique), peut passer commande
- **Admin** : voit le menu "Admin" dans le header, accès complet à `/admin/`

**Exemple à montrer** :
```php
// Dans header.php
<?php if (isAdmin()): ?>
    <a href="/admin/">Espace Admin</a>
<?php elseif (isLoggedIn()): ?>
    <a href="/pages/mon-compte.php">Mon Compte</a>
<?php else: ?>
    <a href="/pages/connexion.php">Connexion</a>
<?php endif; ?>
```

---

### Critère 5 : Sécurisation d'accès
**Couvert par** :
- Middleware `auth` sur les routes checkout, paiement, mon-compte → redirect login si visiteur
- Middleware `admin` sur toutes les routes `/admin/*` → 403 si non-admin
- `AdminMiddleware.php` : vérifie `auth()->user()->role === 'admin'`

**Scénario à démontrer** :
1. Taper directement `http://localhost:8000/admin/commandes` sans être connecté
2. → Redirection automatique vers la page de connexion
3. Se connecter avec un compte client → accéder à `/admin/` → 403 Forbidden

---

### Critère 6 : Données MySQL (/10)
**Couvert par** : Tout le site via **Eloquent ORM**, particulièrement :
- Catalogue : `Produit::disponible()->with('categorie')->get()`
- Inscription : `User::create([...])`
- Connexion : `Auth::attempt($credentials)` → Eloquent + `password_verify` sous le capot
- Commande : `Commande::create()` + `$commande->lignes()->create()` + `$ligne->selections()->create()`
- Mon compte : `Commande::where('id_client', auth()->id())->get()`
- Admin commandes : `$commande->update(['statut' => 'en_preparation'])`
- **CRM** : `User::withCount('commandes')->withSum('commandes', 'total_ttc')->get()`
- **Calendrier** : `Commande::whereDate('date_souhaitee', '>=', today())->with('utilisateur')->get()`
- **Facture** : `Facture::create(['numero_facture' => 'FAC-'.date('Y').'-'.str_pad($id, 4, '0', STR_PAD_LEFT)])`

**Important** : Eloquent génère des requêtes préparées automatiquement → sécurisé contre l'injection SQL.
À expliquer à l'oral : "Eloquent utilise PDO sous le capot avec des bindings paramétrés."

---

### Critère 7 : Ajax (/5)
**Couvert par** :
- **Ajout panier** (`POST /ajax/panier/ajouter`) : clic "Ajouter" → badge header mis à jour sans rechargement
- **Filtre catalogue** (`GET /ajax/produits/filtrer?categorie=X`) : les produits changent dynamiquement
- **Note interne CRM** (`POST /admin/commandes/{id}/note`) : sauvegarde instantanée sans quitter la page

**Utilité à expliquer** :
- Meilleure expérience utilisateur (pas de rechargement de page)
- Les routes Laravel retournent `response()->json(...)` → le JS (`fetch()`) met à jour le DOM
- Le token CSRF est passé dans le header des requêtes POST via `<meta name="csrf-token">`

**Exemple de code Ajax à préparer** :
```javascript
// Dans panier.js
async function ajouterAuPanier(idProduit, quantite) {
    const response = await fetch('/ajax/panier/ajouter', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
        },
        body: JSON.stringify({ id_produit: idProduit, quantite: quantite }),
    });
    const data = await response.json();
    if (data.success) {
        document.getElementById('panier-count').textContent = data.nb_articles;
    }
}
```

---

## ✅ PDSGBD — Grille de Notation

### C# - Langage (/10)
**Montrer** : Syntaxe C#, types, conditions, boucles, try/catch
**Où** : N'importe quel fichier DAL ou formulaire

---

### C# - Techniques POO (/10)
**Montrer** :
- Classes avec propriétés (`Models/Produit.cs`, `Commande.cs`, etc.)
- Encapsulation (propriétés get/set, méthodes privées)
- Instanciation et passage d'objets entre formulaires
- `override ToString()` sur les modèles

```csharp
// Exemple POO à préparer
public class Produit
{
    public string Nom { get; set; }
    public decimal PrixTTC { get; set; }

    // Méthode métier
    public string FormatPrix() => $"{PrixTTC:F2} €";

    public override string ToString() => $"{Nom} - {FormatPrix()}";
}
```

---

### C# - Contrôles d'édition (/10)
**Montrer dans `FrmProduitEdit`** :
- `TextBox` : nom, description
- `NumericUpDown` : prix (décimal), stock (entier), capacité max
- `CheckBox` : configurable, disponible
- `RichTextBox` : description longue
- `ErrorProvider` : affichage des erreurs de validation

---

### C# - Contrôles de sélection (/10)
**Montrer** :
- `ComboBox cmbCategorie` (rempli depuis BDD, affiche catégorie)
- `ComboBox cmbStatut` (filtre dans `FrmCommandes`)
- `DataGridView dgvProduits` / `dgvCommandes` (liste principale)
- `CheckedListBox clbParfums` (sélection multiple des parfums)
- `DateTimePicker` (filtre de dates sur commandes)

---

### C# - Gestion formulaires & interactions (/5)
**Montrer** :
- Navigation : `FrmProduits` → clic "Modifier" → ouvre `FrmProduitEdit`
- Retour de résultat : `DialogResult.OK` → recharge la liste dans `FrmProduits`
- Passage d'objet au constructeur du formulaire enfant
- Formulaire modal vs non-modal

---

### MySQL - INSERT (/5)
**Montrer dans** : `FrmProduitEdit` → bouton "Enregistrer" (mode création)
→ `ProduitDAL.Insert(produit)` avec requêtes paramétrées

---

### MySQL - UPDATE (/5)
**Montrer dans** :
- `FrmProduitEdit` → bouton "Enregistrer" (mode modification)
- `FrmCommandes` → bouton "Changer statut" → `UPDATE commandes SET statut = @statut WHERE id = @id`

---

### MySQL - DELETE (/5)
**Montrer dans** : `FrmProduits` → bouton "Supprimer"
- Demande de confirmation (`MessageBox.Show`)
- Si OK → `ProduitDAL.Delete(id)`
- Si FK violation → catch + message d'erreur

---

### MySQL - Jointure (/10) ⭐ CRITIQUE
**Montrer dans** : `FrmCommandes` — la liste affiche :
- `commandes.id`, `commandes.total_ttc`, `commandes.statut`
- `utilisateurs.nom`, `utilisateurs.prenom` (INNER JOIN)

Et dans `FrmCommandeDetail` :
- Détail avec produits ET parfums choisis (3 tables jointes)

```sql
-- La requête clé à préparer
SELECT lc.id, p.nom AS produit, lc.quantite, lc.prix_unitaire,
       f.nom AS parfum, sp.quantite AS qte_parfum
FROM lignes_commandes lc
INNER JOIN produits p ON lc.id_produit = p.id
LEFT JOIN selections_parfums sp ON sp.id_ligne_commande = lc.id
LEFT JOIN parfums f ON sp.id_parfum = f.id
WHERE lc.id_commande = @id
```

---

### MySQL - Critère de sélection (/5)
**Montrer dans** : `FrmCommandes` — filtres :
- Par statut : `WHERE statut = @statut`
- Par période : `WHERE date_commande BETWEEN @debut AND @fin`
- Par nom client : `WHERE u.nom LIKE @search`

---

### MySQL - Protection injection + formatage (/10) ⭐ CRITIQUE
**Principe à expliquer** :
```csharp
// ❌ DANGEREUX (injection SQL possible)
string sql = "SELECT * FROM produits WHERE nom = '" + txtNom.Text + "'";

// ✅ SÉCURISÉ (requête paramétrée)
string sql = "SELECT * FROM produits WHERE nom = @nom";
cmd.Parameters.AddWithValue("@nom", txtNom.Text);
```

**Montrer** que TOUTES les requêtes utilisent `AddWithValue` ou `Parameters.Add`

---

### Édition : création / modification (/10)
**Montrer** : `FrmProduitEdit` fonctionne en double mode
- Mode création : champs vides, titre "Ajouter un produit"
- Mode modification : champs pré-remplis, titre "Modifier un produit"

---

### Édition : unicité à l'ajout (/5)
**Montrer** : avant INSERT, vérifier qu'aucun produit n'a le même nom
```csharp
if (_produitDAL.NomExiste(txtNom.Text))
    MessageBox.Show("Un produit avec ce nom existe déjà.");
```

---

### Édition : validité des données (/10)
**Montrer** : `ErrorProvider` sur `FrmProduitEdit`
- Nom obligatoire
- Prix > 0
- Stock ≥ 0
- Catégorie sélectionnée
- Si configurable → capacité max obligatoire et > 0

---

### Édition : unicité à la modification (/10)
**Montrer** : même vérification que l'ajout, mais **en excluant l'enregistrement en cours**
```sql
SELECT COUNT(*) FROM produits WHERE nom = @nom AND id != @id
```

---

### Suppression + cascade + vérification (/5+/5+/5)
**Montrer** :
1. Tenter de supprimer un produit qui a des commandes → message d'erreur explicite
2. Supprimer une commande → ses lignes disparaissent automatiquement (CASCADE MySQL)
3. Confirmation avant toute suppression (`MessageBox.Show("Êtes-vous sûr ?")`)

---

### Affichage liste (/5) + liste avec FK (/10) + tri (/5) + filtre (/5)
**Montrer** :
- `FrmProduits` : liste simple avec tri par nom/catégorie/prix
- `FrmCommandes` : liste avec nom client (FK jointure), tri par date, filtre par statut
- `DataGridView.Sort()` ou tri intégré
- Filtre dynamique qui recharge le `DataGridView`

---

## 📋 Scénarios de Démonstration Recommandés

### Démo PDWEB (15-20 min)
1. Parcours client : accueil → catalogue → configurer un ballotin → ajouter au panier → voir le badge se mettre à jour (Ajax)
2. S'inscrire (formulaire avec erreurs intentionnelles) → se connecter
3. Passer une commande → voir la confirmation
4. Se connecter en admin → voir les commandes → changer un statut
5. Accès direct à `/admin/` sans être connecté → redirect

### Démo PDSGBD (15-20 min)
1. Ouvrir `FrmProduits` → montrer la liste avec jointure catégorie
2. Ajouter un produit (validation d'erreurs, unicité)
3. Modifier le produit ajouté
4. Tenter de supprimer un produit avec commandes → erreur explicite
5. Ouvrir `FrmCommandes` → filtrer par statut "en_attente" → changer en "confirmee"
6. Ouvrir le détail d'une commande → montrer les jointures (client + produits + parfums)
7. Montrer le code d'une requête paramétrée dans Visual Studio
