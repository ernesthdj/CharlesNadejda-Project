# SCRIPT DE DEFENSE -- EXAMEN ORAL PDSGBD + PDWEB

> **Projet :** ArtisaStock — ERP patisserie artisanale (adapte Bar)
> **Etudiant :** Ernest — 2e annee Bachelier IT
> **Modules :** PDSGBD (C# WinForms) + PDWEB (Laravel)
> **Scenario de demonstration :** Bar de ville — Long Island Ice Tea (3 niveaux de production)
> **Date :** 2026-07-01

---

## INTRODUCTION (30 secondes)

> "Bonjour. Je vais vous presenter ArtisaStock, une application de gestion de production et de stocks.
> Pour la demonstration, j'ai adapte le domaine metier en bar : je vais produire un Long Island Ice Tea
> en trois niveaux imbriques, puis le vendre via la boutique Laravel. Le tout partage une seule base MySQL."

**Architecture en une phrase :**
- **PDSGBD** = C# WinForms — gestion des stocks, des recettes BOM (Bill of Materials — nomenclature de fabrication), de la production
- **PDWEB** = Laravel 11 — boutique client, panier, commandes
- **MySQL 8.0** = source de verite unique, Docker Compose

---

## ARCHITECTURE GLOBALE (1 minute)

#### Ce que tu montres

1. Ouvrir un terminal et taper `docker ps` pour montrer les conteneurs actifs
2. Montrer la structure du projet dans l'explorateur : `app-csharp/` et `site-laravel/` cote a cote
3. Ouvrir `docker-compose.yml` et pointer les services : `mysql`, `php-fpm`, `nginx`, `phpmyadmin`

**Fichiers a ouvrir dans Visual Studio :**
- `CharlesNadejda/CharlesNadejda.csproj` — pour montrer la structure
- `CharlesNadejda/DAL/` — montrer les 13 fichiers DAL

#### Ce que tu dis

> "Le projet est structure en Clean Architecture simplifiee. La couche DAL (Data Access Layer — couche
> centralisant tous les acces a la base de donnees) isole completement MySQL du reste de l'application.
> Les formulaires WinForms ne font jamais de SQL directement. Toutes les requetes passent par le DAL,
> qui utilise des requetes parametrees pour prevenir l'injection SQL."

> "Cote Laravel, le meme principe : les Controllers delegent aux Eloquent Models avec eager loading
> pour eviter le probleme N+1 (N requetes supplementaires pour charger N relations)."

**Diagramme verbal :**
```
[WinForms] --> [DAL statique] --> [MySqlConnection] --> [MySQL 8.0] <-- [Eloquent ORM] <-- [Laravel Controllers]
```

#### Questions probables

**Q : Pourquoi une DAL statique en C# plutot qu'un ORM comme Entity Framework ?**
> "Choix pedagogique et de controle. Une DAL statique avec des methodes `GetAll`, `GetById`, `Insert`,
> `Update`, `Delete` est lisible et debuggable facilement. Entity Framework ajoute une couche d'abstraction
> que je ne maitrise pas encore completement. Les requetes parametrees manuelles me donnent un controle
> total et garantissent l'absence d'injection SQL."

**Q : Pourquoi Docker ?**
> "Docker garantit un environnement identique en developpement et en production. Le fichier
> `docker-compose.yml` declare tous les services (MySQL, PHP-FPM, Nginx) avec leurs versions exactes.
> Un `docker compose up` suffit pour lancer tout le systeme."

**Q : Comment C# et Laravel partagent-ils la meme base ?**
> "Les deux utilisent les memes credentials MySQL definis dans `.env` pour Laravel et
> `App.config` pour C#. La base de donnees est le contrat commun : les 16 tables sont
> creees par les migrations SQL versionnees dans `/sql/`."

---

## ETAPE 0 -- Login

### 0.1 Lancer ArtisaStock

#### Ce que tu montres

1. Lancer `CharlesNadejda.exe` depuis Visual Studio (F5) ou l'executable
2. L'ecran de login `FrmLogin` s'affiche : champs Email + Mot de passe
3. Saisir les credentials admin : `admin@artisastock.be` / `admin123`
4. Cliquer "Se connecter"
5. L'application redirige vers le **Hub** (`FrmHub`) — ecran d'accueil avec les activites
6. Montrer que la sidebar contient : Hub | Stocks | Activites | Contextes | Fiches & Stock | Recettes BOM | Production | Commandes | Deconnexion

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/FrmLogin.cs`
- `DAL/UtilisateurDAL.cs` — methode `Authenticate`

#### Ce que tu dis

> "La methode `Authenticate` dans `UtilisateurDAL` fait exactement deux choses :
>
> Premiere chose : une requete parametree qui cherche l'utilisateur par email.
> Remarquez le double filtre — `actif = 1` et `role = 'admin'`. Seuls les administrateurs actifs
> peuvent se connecter a l'ERP. Cote boutique Laravel, les clients utilisent la meme table `utilisateurs`
> mais via un controleur PHP qui ne filtre pas le role — les deux applications cohabitent.
>
> Deuxieme chose : si la ligne existe, on verifie le mot de passe avec `BCrypt.Net.BCrypt.Verify()`.
> Le hash n'est jamais compare en clair. C'est le meme algorithme BCrypt que cote Laravel
> avec `password_hash()` / `password_verify()` — les deux applications sont interoperables sur les mots de passe.
>
> La methode est encapsulee dans un `using` double : `DbHelper.GetConnection()` et `conn.CreateCommand()`.
> Les deux sont `IDisposable` — le `using` garantit que la connexion et la commande sont fermees
> meme si une exception est levee. Pas de fuites de connexions."

```csharp
// DAL/UtilisateurDAL.cs — code reel
public static Utilisateur Authenticate(string email, string motDePasse)
{
    using (var conn = DbHelper.GetConnection())
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"
            SELECT id, nom, prenom, email, role, mot_de_passe
            FROM utilisateurs
            WHERE email = @email AND actif = 1 AND role = 'admin'";

        cmd.Parameters.AddWithValue("@email", email);

        using (var reader = cmd.ExecuteReader())
        {
            if (!reader.Read())
                return null;   // Email inconnu ou role != admin

            string hash = reader["mot_de_passe"].ToString();

            if (!BCrypt.Net.BCrypt.Verify(motDePasse, hash))
                return null;   // Mot de passe incorrect

            return new Utilisateur
            {
                Id     = (int)reader["id"],
                Nom    = reader["nom"].ToString(),
                Prenom = reader["prenom"].ToString(),
                Email  = reader["email"].ToString(),
                Role   = reader["role"].ToString()
            };
        }
    }
}
```

> "Notez que le message d'erreur dans `FrmLogin` est toujours generique : 'Email ou mot de passe incorrect.'
> On ne distingue jamais si c'est l'email ou le mot de passe qui est faux — cela evite de confirmer
> a un attaquant qu'un email est enregistre en base. C'est l'OWASP — Open Web Application Security Project,
> reference mondiale des vulnerabilites — qui recommande ce message neutre."

#### Questions probables

**Q : Pourquoi BCrypt et pas MD5 ou SHA256 ?**
> "MD5 et SHA256 sont des algorithmes rapides, conçus pour la performance — ce qui les rend
> vulnerables aux attaques par force brute. BCrypt est intentionnellement lent et son facteur
> de cout est ajustable. De plus, BCrypt integre automatiquement un salt unique par hash,
> ce qui rend les attaques par rainbow table (table de correspondances hash → mot de passe) impossibles."

**Q : Qu'est-ce qu'une requete parametree ?**
> "Au lieu de concatener les variables dans la chaine SQL — ce qui permettrait a un attaquant
> d'injecter du SQL malveillant — on utilise des placeholders (`@email`) que MySqlCommand
> remplace de maniere securisee. Le driver escape automatiquement les caracteres speciaux.
> C'est la defense principale contre l'injection SQL, qui est le #1 du Top 10 OWASP (Open Web
> Application Security Project — reference mondiale des vulnerabilites web)."

---

## ETAPE 1 -- Infrastructure (Stock, Activite, Contexte)

### 1.1 Creer le Stock "Bar de ville"

#### Ce que tu montres

1. Dans la sidebar, cliquer sur **"Stocks"** → `FrmStocks` s'ouvre
2. La DataGridView (DGV) affiche les stocks existants
3. Cliquer **"+ Ajouter"** → `FrmStockEdit` s'ouvre en mode creation
4. Remplir :
   - Nom : `Bar de ville`
   - Description : `Stock principal du bar`
   - Adresse : `Rue de la Paix 1, 1000 Bruxelles`
5. Cliquer **"Enregistrer"**
6. Le stock apparait dans la DGV

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Stocks/FrmStocks.cs` — herite de `FrmListeBase<Stock>`
- `Forms/Stocks/FrmStockEdit.cs` — herite de `FrmEditBase`
- `DAL/StockDAL.cs`

#### Ce que tu dis

> "FrmStocks herite de FrmListeBase<T> (T = type generique — ici Stock), une classe de base generique
> qui fournit la DGV, les boutons CRUD (Create Read Update Delete) et le rafraichissement automatique.
> Je n'ai pas a reimplementer ces comportements pour chaque entite."

```csharp
// Forms/Base/FrmListeBase.cs (pattern generique)
public abstract class FrmListeBase<T> : Form
{
    protected DataGridView dgv;
    protected abstract List<T> Charger();
    protected abstract void OuvrirFormulaire(T entite = default);

    protected void BtnAjouter_Click(object sender, EventArgs e)
        => OuvrirFormulaire();

    protected void BtnModifier_Click(object sender, EventArgs e)
    {
        if (dgv.CurrentRow?.DataBoundItem is T item)
            OuvrirFormulaire(item);
    }
}
```

> "FrmStockEdit herite de FrmEditBase (classe abstraite de base pour tous les formulaires d'edition).
> La classe abstraite (abstract class) impose que chaque formulaire implemente Valider() et Enregistrer().
> Cela garantit une structure coherente sans dupliquer du code."

#### Questions probables

**Q : Qu'est-ce qu'une classe abstraite ? Pourquoi l'utiliser ici ?**
> "Une classe abstraite ne peut pas etre instanciee directement. Elle definit un contrat — des methodes
> abstraites que chaque sous-classe DOIT implementer. Ici, FrmEditBase force chaque formulaire d'edition
> a implementer Valider() et Enregistrer(). Si j'oublie, le compilateur refuse de compiler.
> C'est du principe ouvert/ferme (Open/Closed Principle) : ouvert a l'extension, ferme a la modification."

**Q : Pourquoi les generiques (<T>) dans FrmListeBase ?**
> "Sans generiques, j'aurais une FrmListeStock, une FrmListeActivite, etc., toutes identiques sauf
> le type. Avec FrmListeBase<T>, j'ecris le code une seule fois. Le compilateur specialise la classe
> pour chaque type concret. C'est le principe DRY (Don't Repeat Yourself)."

**Q : Qu'est-ce qu'une partial class en C# ?**
> "Une partial class permet de decouper la definition d'une classe en plusieurs fichiers .cs.
> WinForms l'utilise systematiquement : `FrmStocks.cs` contient la logique metier, et
> `FrmStocks.Designer.cs` contient le code genere par le designer visuel (positions des controles,
> tailles, etc.). La regle CS0136 (variable deja declaree dans la portee parente) peut surgir
> si on declare la meme variable dans les deux parties."

---

### 1.2 Creer l'Activite "Bar" et lier le stock

#### Ce que tu montres

1. Dans la sidebar, cliquer **"Activites"** → `FrmActivites` s'ouvre
2. Cliquer **"+ Ajouter"** → `FrmActiviteEdit` s'ouvre
3. Remplir :
   - Nom : `Bar`
   - Description : `Gestion des cocktails et boissons`
4. Dans la section **"Stocks associes"**, cliquer **"Ajouter"**
5. Selectionner `Bar de ville` dans la liste
6. Cliquer **"Enregistrer"**
7. L'activite `Bar` apparait dans la liste

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Activites/FrmActiviteEdit.cs`
- `DAL/ActiviteDAL.cs` — montrer la transaction INSERT + liaison

#### Ce que tu dis

> "La liaison Activite ↔ Stock est une relation many-to-many (plusieurs stocks peuvent appartenir
> a plusieurs activites). Elle est geree par la table de jonction `activite_stocks`.
> L'enregistrement utilise une transaction pour garantir l'atomicite ACID (Atomicity, Consistency,
> Isolation, Durability) : soit les deux INSERT reussissent, soit aucun."

```csharp
// DAL/ActiviteDAL.cs
public static void Insert(Activite a, List<int> idStocks)
{
    using var conn = Connexion.Ouvrir();
    using var trans = conn.BeginTransaction();
    try
    {
        // INSERT dans activites
        var cmd = new MySqlCommand(
            "INSERT INTO activites (nom, description) VALUES (@nom, @desc)", conn, trans);
        cmd.Parameters.AddWithValue("@nom", a.Nom);
        cmd.Parameters.AddWithValue("@desc", a.Description);
        cmd.ExecuteNonQuery();
        long idActivite = cmd.LastInsertId;

        // INSERT dans activite_stocks pour chaque stock
        foreach (int idStock in idStocks)
        {
            var cmdLien = new MySqlCommand(
                "INSERT INTO activite_stocks (id_activite, id_stock) VALUES (@idA, @idS)",
                conn, trans);
            cmdLien.Parameters.AddWithValue("@idA", idActivite);
            cmdLien.Parameters.AddWithValue("@idS", idStock);
            cmdLien.ExecuteNonQuery();
        }
        trans.Commit();
    }
    catch
    {
        trans.Rollback(); // ACID -- Atomicite garantie
        throw;
    }
}
```

#### Questions probables

**Q : Qu'est-ce qu'une transaction et pourquoi est-elle necessaire ici ?**
> "Une transaction groupe plusieurs operations en une seule unite atomique. Si l'INSERT dans
> `activite_stocks` echoue apres que l'INSERT dans `activites` a reussi, le rollback annule
> tout. Sans transaction, on aurait une activite sans stock associe — un etat incoherent.
> C'est le A de ACID : Atomicite."

**Q : C'est quoi ACID ?**
> "ACID est le standard de fiabilite des bases de donnees relationnelles :
> A = Atomicite (tout ou rien), C = Consistance (la DB reste valide apres la transaction),
> I = Isolation (les transactions concurrentes ne s'interferent pas),
> D = Durabilite (une fois committee, la donnee est persistee meme apres un crash)."

---

### 1.3 Creer le Contexte "Cocktails" avec 3 niveaux

#### Ce que tu montres

1. Dans la sidebar, cliquer **"Contextes"** → `FrmContextes` s'ouvre
2. Cliquer **"+ Ajouter"** → `FrmContexteEdit` s'ouvre
3. Remplir :
   - Nom : `Cocktails`
   - Activite associee : `Bar` (combobox)
4. Dans la section **"Niveaux de production"**, ajouter :
   - Niveau 1 : `Ingredients` (matieres premieres)
   - Niveau 2 : `Premix` (melange des alcools et jus)
   - Niveau 3 : `Cocktail` (assemblage final)
5. Cliquer **"Enregistrer"**

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Contextes/FrmContexteEdit.cs`
- `DAL/ContexteDAL.cs`
- `Models/Contexte.cs`

#### Ce que tu dis

> "Le Contexte definit la hierarchie de production : combien de niveaux existent et ce qu'ils
> representent. Pour un Long Island Ice Tea, j'ai 3 niveaux :
> N1 = les ingredients bruts (vodka, tequila, cola...) achetes en stock,
> N2 = le Premix produit a partir des ingredients N1,
> N3 = le cocktail final produit a partir du Premix N2 + cola N1."

> "Cette hierarchie en BOM (Bill of Materials) est le coeur du systeme. Elle permet de tracer
> exactement quelles matieres premieres ont ete consommees pour produire chaque cocktail."

#### Questions probables

**Q : Pourquoi separer le Premix de l'assemblage final ?**
> "En production reelle, le Premix peut etre prepare en grande quantite a l'avance et conserve.
> On peut ensuite produire des cocktails individuels a la commande. Ca permet aussi de gerer
> le cout de revient a chaque niveau : je sais exactement ce que coute le Premix seul,
> puis le cocktail final. C'est une BOM multi-niveaux, comme en industrie manufacturiere."

---

## ETAPE 2 -- Fiches & Stock (nouveau ecran unifie)

### 2.1 Creer les fiches ingredients (mode Fiches)

#### Ce que tu montres

1. Dans la sidebar, cliquer **"Fiches & Stock"** → `FrmIngredients` s'ouvre
2. Observer le **toggle en haut a droite** : "Fiches / Stock reel" — il est sur **Fiches** par defaut
3. Le mode Fiches affiche le catalogue avec colonnes : Ingredient, Conditionnement, Qte/cond., Type physique, Densite, Fournisseur, Stock cible (pieces)
4. Verifier que le **FlowLayoutPanel** de chips est visible : bouton "Tous" + chips par stock
5. Selectionner le chip **"Bar de ville"** — la liste se filtre
6. Cliquer **"+ Ajouter"** pour creer la fiche **Vodka** :
   - Nom : `Vodka`
   - Conditionnement : `Bouteille`
   - Quantite par conditionnement : `700` ml
   - Type physique : `Liquide`
   - Densite : `0.79`
   - Fournisseur par defaut : (selectionner ou creer)
   - Stock cible (pieces) : `10`
   - Stock par defaut : `Bar de ville`
7. Repeter pour **Tequila blanche** (700ml, 18€, densite 0.79)
8. Repeter pour **Rhum blanc** (700ml, 12€, densite 0.79)
9. Repeter pour **Gin** (700ml, 14€, densite 0.83)
10. Repeter pour **Triple Sec** (700ml, 10€, densite 0.83)
11. Repeter pour **Jus de citron** (500ml, 2€, densite 1.05)
12. Repeter pour **Sirop de canne** (700ml, 5€, densite 1.30)
13. Repeter pour **Cola** (330ml, 0.80€, densite 1.04)

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Ingredients/FrmIngredients.cs`
- `DAL/IngredientDAL.cs` — montrer les deux branches GetAll
- `Forms/Ingredients/FrmIngredientEdit.cs`

#### Ce que tu dis

> "FrmIngredients est le nouvel ecran unifie qui remplace les anciens ecrans separes 'Achats' et 'Lots'.
> Le toggle en haut a droite change le mode : en mode Fiches, je vois le catalogue — la definition
> de l'ingredient (sa classe, ses caracteristiques physiques). En mode Stock reel, je vois les
> instances physiques avec les quantites disponibles."

> "Une Fiche ingredient, c'est la definition : 'une bouteille de Vodka de 700ml'.
> Un Lot, c'est l'achat concret : 'j'ai achete 5 bouteilles de Vodka le 15 juin'.
> La fiche est la classe, le lot est l'instance."

> "Le DAL a deux branches dans GetAll selon le mode :"

```csharp
public static List<Ingredient> GetAll(int idStock = 0, bool stockReelSeulement = false)
{
    if (!stockReelSeulement)
    {
        cmd.CommandText = @"SELECT fi.*, 0 AS stock_actuel, f.nom AS nom_fournisseur
            FROM fiches_ingredients fi
            LEFT JOIN fournisseurs f ON f.id = fi.id_fournisseur_defaut
            WHERE fi.actif = 1";
        if (idStock > 0)
            cmd.CommandText += " AND fi.id_stock_defaut = @idStock";
        cmd.CommandText += " ORDER BY fi.nom";
    }
    else
    {
        cmd.CommandText = @"SELECT fi.*, COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel,
            f.nom AS nom_fournisseur
            FROM fiches_ingredients fi
            LEFT JOIN fournisseurs f ON f.id = fi.id_fournisseur_defaut
            LEFT JOIN lots_ingredients l ON l.id_fiche_ingredient = fi.id
            WHERE fi.actif = 1";
        if (idStock > 0)
            cmd.CommandText += @" AND fi.id IN (SELECT DISTINCT id_fiche_ingredient
                FROM lots_ingredients WHERE id_stock = @idStock AND quantite_disponible > 0)";
        cmd.CommandText += " GROUP BY fi.id HAVING stock_actuel > 0 ORDER BY fi.nom";
    }
}
```

> "En mode Fiches, la requete est simple : SELECT sur `fiches_ingredients` avec LEFT JOIN fournisseurs.
> Pas de JOIN sur `lots_ingredients`, donc 0 AS stock_actuel.
> En mode Stock reel, je fais un LEFT JOIN sur `lots_ingredients` et un SUM groupee.
> COALESCE (retourne la premiere valeur non-NULL) gere le cas ou il n'y a aucun lot :
> SUM d'un ensemble vide retourne NULL, COALESCE(NULL, 0) retourne 0."

> "Les chips de filtre utilisent un FlowLayoutPanel : 'Tous' + un chip par stock de l'activite courante.
> En mode Fiches, le chip filtre sur `id_stock_defaut` de la fiche.
> En mode Stock reel, il filtre sur `id_stock` des lots."

#### Questions probables

**Q : Pourquoi LEFT JOIN et pas INNER JOIN pour les fournisseurs ?**
> "Un INNER JOIN n'afficherait que les ingredients qui ont un fournisseur assigne.
> Si un ingredient n'a pas encore de fournisseur par defaut, il disparaitrait de la liste.
> Avec LEFT JOIN, tous les ingredients s'affichent, et `nom_fournisseur` est NULL si pas de fournisseur.
> C'est le principe de preservation des donnees : ne pas perdre d'information par un JOIN trop strict."

**Q : COALESCE, comment ca marche exactement ?**
> "COALESCE est une fonction SQL qui prend N arguments et retourne le premier qui n'est pas NULL.
> `COALESCE(SUM(l.quantite_disponible), 0)` : si SUM retourne NULL (aucun lot), on retourne 0.
> C'est equivalent a ISNULL en T-SQL (SQL Server) ou IFNULL en MySQL, mais COALESCE est standard SQL."

**Q : Qu'est-ce que GROUP BY HAVING ici ?**
> "GROUP BY fi.id groupe tous les lots d'un meme ingredient ensemble, pour que SUM calcule
> le stock total par ingredient. HAVING stock_actuel > 0 filtre les groupes apres agregation —
> contrairement a WHERE qui filtre avant. Je ne peux pas ecrire WHERE stock_actuel > 0 car
> stock_actuel est une colonne calculee par agregation."

---

### 2.2 Acheter les lots (mode Stock reel)

#### Ce que tu montres

1. Basculer le toggle sur **"Stock reel"**
2. Observer le changement des colonnes : Ingredient, Conditionnement, Qte/cond., Type physique, Densite, Fournisseur, **Pieces en stock**, **Poids/Volume**
3. Observer le changement des boutons :
   - "＋ Ajouter" → "＋ Nouveau achat"
   - "✎ Modifier" → "☰ Voir les achats"
   - "Supprimer" disparait
4. Cliquer **"＋ Nouveau achat"** sur la ligne **Vodka** → `FrmAchatEdit` s'ouvre
5. Remplir :
   - Ingredient : `Vodka` (pre-rempli)
   - Stock de destination : `Bar de ville`
   - Quantite achetee : `5` bouteilles
   - Prix unitaire HT : `15.00` €
   - TVA : `21` %
   - Fournisseur : (selectionner)
   - Date achat : (aujourd'hui)
6. Observer le **prix live** se mettre a jour en saisissant les valeurs
7. Cliquer **"Enregistrer"**
8. La colonne "Pieces en stock" affiche maintenant `5 pieces`
9. La colonne "Poids/Volume" affiche `3,50 l` (5 × 700ml = 3500ml → 3,50 l)
10. Repeter les achats pour tous les ingredients :
    - Tequila : 3 bouteilles × 18€
    - Rhum blanc : 3 bouteilles × 12€
    - Gin : 3 bouteilles × 14€
    - Triple Sec : 3 bouteilles × 10€
    - Jus de citron : 5 bouteilles × 2€
    - Sirop de canne : 3 bouteilles × 5€
    - Cola : 10 boites × 0.80€

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Ingredients/FrmIngredients.cs` — methode `AppliquerMode`
- `Forms/Achats/FrmAchatEdit.cs` — ReadLive et MajPrix
- `Helpers/UnitConvertisseur.cs` — FormatQte
- `Forms/Ingredients/FrmIngredients.cs` — SouscrireCellFormatting

#### Ce que tu dis

> "Le toggle change completement l'interface via la methode AppliquerMode :"

```csharp
private void AppliquerMode(bool stockReelSeulement)
{
    _stockReelSeulement = stockReelSeulement;
    if (stockReelSeulement)
    {
        btnAjouter.Text      = "＋  Nouveau achat";
        btnModifier.Text     = "☰  Voir les achats";
        btnSupprimer.Visible = false;
    }
    else
    {
        btnAjouter.Text      = "＋  Ajouter";
        btnModifier.Text     = "✎  Modifier";
        btnSupprimer.Visible = true;
    }
    Charger();
}
```

> "En Stock reel, supprimer est masque car on ne supprime pas un lot physique — on le consomme
> via la production. C'est une decision de traçabilite : l'historique des achats doit rester intact."

> "La colonne Poids/Volume utilise CellFormatting pour formater la valeur brute en millilitres
> en une chaine lisible :"

```csharp
private void SouscrireCellFormatting()
{
    dgv.CellFormatting += (s, ev) =>
    {
        if (ev.RowIndex < 0 || !_stockReelSeulement) return;
        var col  = dgv.Columns[ev.ColumnIndex];
        var item = dgv.Rows[ev.RowIndex].DataBoundItem as Ingredient;
        if (col == null || item == null) return;
        if (col.Name == "StockPieces")
            ev.Value = item.StockPieces > 0 ? $"{item.StockPieces:0} pièces" : "—";
        else if (col.Name == "StockActuel")
            ev.Value = item.StockActuel > 0
                ? UnitConvertisseur.FormatQte(item.StockActuel, item.UniteMesure)
                : "—";
    };
}
```

> "CellFormatting intercepte l'affichage de chaque cellule sans modifier le modele.
> Je ne change pas la valeur stockee en base — je change uniquement ce que l'utilisateur voit.
> Exemple : 3500 ml dans la DB s'affiche '3,50 l' a l'ecran. UnitConvertisseur.FormatQte
> convertit automatiquement selon l'unite : ml → cl → l selon le seuil."

> "Pour le prix live dans FrmAchatEdit, j'ai du resoudre un probleme subtil :
> NumericUpDown ne declenche ValueChanged que quand on quitte le champ (LostFocus).
> Si l'utilisateur tape '15' et regarde le prix sans cliquer ailleurs, le prix ne se met pas a jour.
> La solution : s'abonner aussi a TextChanged et lire la valeur avec decimal.TryParse :"

```csharp
private static decimal ReadLive(NumericUpDown nud)
{
    return decimal.TryParse(nud.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal v)
        ? Math.Max(nud.Minimum, Math.Min(nud.Maximum, v))
        : nud.Value;
}

// Abonnements : ValueChanged (spinner) + TextChanged (frappe clavier)
nudPrix.ValueChanged += (s, e) => MajPrix();
nudPrix.TextChanged  += (s, e) => MajPrix();
```

> "Math.Max(nud.Minimum, Math.Min(nud.Maximum, v)) clampe la valeur dans les bornes du NumericUpDown.
> Si l'utilisateur tape '999999' mais le max est 9999, on garde 9999. Ca evite des prix aberrants."

#### Questions probables

**Q : Pourquoi TextChanged et pas seulement ValueChanged ?**
> "ValueChanged de NumericUpDown se declenche quand la valeur est validee — generalement a LostFocus
> ou quand on clique sur les fleches du spinner. Si l'utilisateur tape au clavier, ValueChanged
> ne se declenche pas avant qu'il quitte le champ. TextChanged se declenche a chaque caractere tape,
> ce qui permet un retour visuel immediat. C'est une question d'UX (User Experience) :
> feedback immediat < 200ms selon les heuristiques de Nielsen (Nielsen — chercheur en ergonomie,
> auteur des 10 heuristiques de reference pour l'evaluation des interfaces)."

**Q : Qu'est-ce que CellFormatting et pourquoi pas modifier le DataSource ?**
> "CellFormatting est un evenement de la DataGridView qui se declenche juste avant qu'une cellule
> soit rendue a l'ecran. Je peux changer ev.Value (la valeur affichee) sans toucher au DataSource.
> Si je modifiais le DataSource, je corromprais les donnees : la DB stockerait '3,50 l' comme string
> au lieu de 3500 comme decimal. La separation affichage/modele est fondamentale."

**Q : Pourquoi masquer Supprimer en mode Stock reel et pas juste le desactiver ?**
> "Un bouton desactive reste visible et cree de la confusion cognitive : l'utilisateur se demande
> pourquoi il ne peut pas cliquer. Le masquer applique la progressive disclosure (divulgation
> progressive — n'afficher que ce qui est pertinent dans le contexte actuel). En mode Stock reel,
> supprimer n'a pas de sens semantique, donc on l'elimine visuellement."

---

## ETAPE 3 -- Recettes BOM

### 3.1 Fiche N2 : Premix Long Island (inputs = ingredients N1)

#### Ce que tu montres

1. Dans la sidebar, cliquer **"Recettes BOM"** → `FrmBOM` s'ouvre
2. Cliquer **"+ Ajouter"** → `FrmBOMEdit` s'ouvre
3. Remplir l'entete :
   - Nom : `Premix Long Island`
   - Contexte : `Cocktails`
   - Niveau de production : `N2 — Premix`
   - Unite de sortie : `ml`
   - Quantite produite par batch : `135`
4. Dans la section **"Ingredients"**, ajouter les 7 composants N1 :
   - Vodka : `30` ml
   - Tequila blanche : `15` ml
   - Rhum blanc : `15` ml
   - Gin : `15` ml
   - Triple Sec : `15` ml
   - Jus de citron : `30` ml
   - Sirop de canne : `15` ml
5. Verifier que le total des inputs = 120ml (output = 135ml, 15ml = gaz dissous + residu)
6. Cliquer **"Enregistrer"**
7. La fiche N2 apparait dans la liste BOM

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/BOM/FrmBOMEdit.cs`
- `DAL/BOMDAL.cs`
- `Models/RecetteBOM.cs` + `Models/LigneBOM.cs`

#### Ce que tu dis

> "La recette BOM N2 definit comment produire 135ml de Premix Long Island a partir d'ingredients bruts.
> Chaque ligne BOM specifies un ingredient source (N1), la quantite requise, et l'unite.
> Le systeme calcule automatiquement le cout de revient par batch en croisant avec les prix des lots."

> "Un batch = une unite de production. Si je lance 10 batches de Premix, je produis 1350ml de Premix
> et je consomme 300ml de Vodka, 150ml de Tequila, etc."

> "La structure en base de donnees :"
```
recettes_bom (id, nom, id_contexte, id_niveau, qte_produite, unite_sortie)
    |
    └── lignes_bom (id, id_recette, id_fiche_ingredient, qte_requise, unite)
```

#### Questions probables

**Q : Comment le systeme sait combien prendre de chaque bouteille ?**
> "La quantite en ligne BOM est en ml. Le DAL sait que la bouteille de Vodka fait 700ml.
> Si la recette demande 300ml de Vodka pour 10 batches, le systeme cherche des lots avec
> quantite_disponible > 0, prend FIFO (First In First Out — premier entre, premier sorti),
> et consomme dans l'ordre d'anciennete. Si un lot n'a que 200ml, il prend 200ml la,
> puis 100ml dans le lot suivant."

**Q : Pourquoi la quantite output (135ml) est differente de la somme des inputs (120ml) ?**
> "En pratique, il y a des pertes : evaporation, residus dans les recipients, mousse, CO2 dissous.
> La fiche BOM distingue qte_requise (ce qu'on prend en stock) et qte_produite (ce qu'on obtient).
> Le ratio rendement = 120/135 ≈ 89%. C'est un parametre reglable par l'utilisateur selon sa production."

---

### 3.2 Fiche N3 : Long Island Ice Tea (inputs = premix N2 + cola N1)

#### Ce que tu montres

1. Cliquer **"+ Ajouter"** dans FrmBOM
2. Remplir :
   - Nom : `Long Island Ice Tea`
   - Contexte : `Cocktails`
   - Niveau de production : `N3 — Cocktail`
   - Unite de sortie : `ml`
   - Quantite produite par batch : `195` (1 cocktail)
3. Dans la section **"Ingredients"**, ajouter :
   - Source N2 (BOM stock) : `Premix Long Island` → `135` ml
   - Source N1 (ingredient) : `Cola` → `60` ml
4. Montrer que le champ source distingue `lots_ingredients` vs `bom_stocks`
5. Cliquer **"Enregistrer"**

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/BOM/FrmBOMEdit.cs` — section sources mixtes N1/N2
- `DAL/BOMDAL.cs` — GetSourcesDisponibles
- `Models/LigneBOM.cs` — champ `type_source` (enum : Ingredient / BOMStock)

#### Ce que tu dis

> "La fiche N3 est speciale : elle consomme deux types de sources differentes.
> Le Premix (135ml) vient de `bom_stocks` — c'est du stock produit par la production N2.
> Le Cola (60ml) vient de `lots_ingredients` — c'est un ingredient achete directement."

> "LigneBOM a un champ type_source qui distingue les deux cas :
> `Ingredient` → chercher dans `lots_ingredients`
> `BOMStock` → chercher dans `bom_stocks` (table ou va le produit de la production)"

```csharp
// Models/LigneBOM.cs
public enum TypeSource { Ingredient, BOMStock }

public class LigneBOM
{
    public int Id { get; set; }
    public int IdRecette { get; set; }
    public TypeSource TypeSource { get; set; }
    public int IdSource { get; set; }        // id_fiche_ingredient OU id_recette_source
    public decimal QteRequise { get; set; }
    public string Unite { get; set; }
}
```

#### Questions probables

**Q : Pourquoi ne pas tout mettre dans `lots_ingredients` ?**
> "Les produits finis (Premix) et les matieres premieres (Vodka) ont des natures differentes.
> Les lots_ingredients tracent l'historique d'achat avec fournisseur, date facture, TVA.
> Les bom_stocks tracent l'historique de production avec date, batch, cout de revient calcule.
> Melanger les deux dans une seule table perdrait la traçabilite specifique a chaque type.
> C'est le principe de separation des responsabilites au niveau base de donnees."

**Q : Comment le systeme sait quel Premix consommer en premier ?**
> "FIFO : les bom_stocks sont ordonnes par date_production ASC. On consomme d'abord le plus ancien.
> C'est identique a la logique des lots_ingredients. La fraicheur est importante pour la qualite
> des cocktails — un premix de la veille est preferable a un premix de la semaine passee."

---

## ETAPE 4 -- Production (2 passes)

### 4.1 Produire le Premix (N2)

#### Ce que tu montres

1. Dans la sidebar, cliquer **"Production"** → `FrmProduction` s'ouvre
2. Selectionner :
   - Activite : `Bar`
   - Contexte : `Cocktails`
   - Niveau : `N2 — Premix`
3. La liste affiche `Premix Long Island` avec le stock disponible de chaque ingredient
4. Selectionner `Premix Long Island`
5. Remplir :
   - Nombre de batches : `10`
   - Quantite totale a produire : `1350` ml (auto-calcule : 10 × 135ml)
6. Cliquer **"Verifier le stock"** → le systeme affiche pour chaque ingredient :
   - Vodka : besoin 300ml / disponible 3500ml ✓
   - Tequila : besoin 150ml / disponible 2100ml ✓
   - Rhum : besoin 150ml / disponible 2100ml ✓
   - Gin : besoin 150ml / disponible 2100ml ✓
   - Triple Sec : besoin 150ml / disponible 2100ml ✓
   - Jus citron : besoin 300ml / disponible 2500ml ✓
   - Sirop canne : besoin 150ml / disponible 2100ml ✓
7. Cliquer **"Lancer la production"**
8. Confirmation : "Production lancee — 1350ml de Premix Long Island cree"
9. Aller dans Fiches & Stock → Stock reel → filtrer Bar de ville
10. Vodka : stock passe de 3500ml a 3200ml (consommation FIFO visible)

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Production/FrmProduction.cs`
- `DAL/ProductionDAL.cs` — methode LancerProduction avec FIFO
- `DAL/ProductionDAL.cs` — boucle FIFO

#### Ce que tu dis

> "La production se fait en deux passes. D'abord le N2 (Premix), car le N3 (cocktail) en a besoin.
> Si j'essaie de produire le N3 sans stock de Premix, le systeme refuse — stock insuffisant."

> "Le coeur de la production, c'est la boucle FIFO qui consomme les lots dans l'ordre d'anciennete :"

```csharp
foreach (var (idLot, dispo, prixUnit) in lots)
{
    if (restant <= 0) break;
    decimal pris = Math.Min(restant, dispo);
    // UPDATE lots_ingredients SET quantite_disponible -= pris
    restant -= pris;
    coutLigne += pris * prixUnit;
}
```

> "Pour chaque ingredient de la recette, je recupere les lots ordonnes par date_achat ASC.
> Je prends Math.Min(restant, dispo) dans chaque lot — je ne depasse pas ce qu'il y a.
> Je decremente `quantite_disponible` dans la DB avec un UPDATE.
> Je cumule le cout proportionnel pour calculer le prix de revient du Premix produit."

> "A la fin, j'INSERT dans `bom_stocks` : 1350ml de Premix Long Island, cree maintenant,
> avec le cout de revient calcule. Ce stock sera consomme par la production N3."

> "Tout ca est dans une transaction — si un UPDATE echoue a mi-chemin, tout est annule."

#### Questions probables

**Q : Pourquoi FIFO et pas LIFO ou random ?**
> "FIFO (First In, First Out) est le standard alimentaire et en gestion de stock perishable.
> On utilise d'abord ce qui est entre en premier pour eviter que les anciens stocks ne periment.
> LIFO (Last In, First Out — dernier entre, premier sorti) ferait le contraire et serait dangereux
> pour la fraicheur. Random serait imprevisible et difficile a auditer lors d'un controle sanitaire."

**Q : Que se passe-t-il si le stock est insuffisant en cours de production ?**
> "La verification pre-production calcule le besoin total avant de toucher aux stocks.
> Si le besoin depasse le disponible, la production est refusee AVANT de commencer.
> Le bouton 'Lancer la production' reste desactive tant que la verification n'est pas passee.
> C'est de la prevention d'erreur (heuristique #5 de Nielsen) plutot que de la gestion d'erreur."

**Q : Comment le cout de revient du Premix est-il calcule ?**
> "Je cumule `pris × prixUnit` pour chaque lot consomme. Le prix unitaire d'un lot vient
> du prix d'achat enregistre dans FrmAchatEdit. Si j'ai pris 200ml d'un lot a 15€/bouteille
> de 700ml, le cout est 200 × (15/700) = 4.29€. La somme de toutes les lignes donne
> le cout de revient total du batch, stocke dans `bom_stocks.cout_revient`."

---

### 4.2 Produire les Long Island Ice Tea (N3)

#### Ce que tu montres

1. Dans FrmProduction, changer le niveau : **N3 — Cocktail**
2. Selectionner `Long Island Ice Tea`
3. Remplir :
   - Nombre de batches : `10` (10 cocktails)
   - Quantite totale : `1950` ml
4. Cliquer **"Verifier le stock"** :
   - Premix Long Island (bom_stocks) : besoin 1350ml / disponible 1350ml ✓
   - Cola (lots_ingredients) : besoin 600ml / disponible 3300ml ✓
5. Cliquer **"Lancer la production"**
6. Confirmation : "10 Long Island Ice Tea produits"
7. Verifier dans Fiches & Stock → Stock reel :
   - Le Premix n'apparait plus (stock epuise, HAVING stock_actuel > 0 l'exclut)
   - Cola : 3300ml → 2700ml
8. Verifier dans les BOM stocks que 10 unites de Long Island sont disponibles

**Fichiers a ouvrir dans Visual Studio :**
- `DAL/ProductionDAL.cs` — logique deux types de sources

#### Ce que tu dis

> "La production N3 consomme deux types de sources. Pour chaque ligne BOM, le systeme
> detecte le type_source et va chercher dans la bonne table :"

```csharp
// Pour type_source = BOMStock : consommer dans bom_stocks
// Pour type_source = Ingredient : consommer dans lots_ingredients (meme boucle FIFO)

foreach (LigneBOM ligne in recette.Lignes)
{
    if (ligne.TypeSource == TypeSource.BOMStock)
    {
        var stocks = BomStockDAL.GetDisponibles(ligne.IdSource, idStockDest);
        // Boucle FIFO sur bom_stocks, ORDER BY date_production ASC
    }
    else
    {
        var lots = LotIngredientDAL.GetDisponibles(ligne.IdSource, idStockDest);
        // Boucle FIFO sur lots_ingredients, ORDER BY date_achat ASC
    }
}
```

> "Le Premix consomme dans `bom_stocks` via FIFO sur `date_production`.
> Le Cola consomme dans `lots_ingredients` via FIFO sur `date_achat`.
> Les deux ont le meme principe FIFO mais sur des tables differentes."

> "Apres la production N3, j'ai 10 unites de Long Island Ice Tea dans `bom_stocks`.
> Ces unites peuvent maintenant etre publiees sur la boutique web."

#### Questions probables

**Q : Montre la traçabilite d'un cocktail : depuis quels lots de Vodka vient-il ?**
> "Chaque production cree des entrees dans `production_lignes` qui tracent : quel lot a ete consomme,
> quelle quantite, pour quelle production. La traçabilite remonte de N3 → N2 → N1 :
> ce cocktail contient du Premix du batch 001, qui contenait de la Vodka du lot 003,
> achete le 14 juin. C'est la traçabilite complete que demande la restauration."

**Q : Pourquoi le Premix N2 disparait de la liste Stock reel apres la production N3 ?**
> "Le HAVING stock_actuel > 0 dans la requete mode Stock reel exclut les ingredients/produits
> avec stock epuise. Le Premix a ete entierement consomme (1350ml → 0ml), donc COALESCE(SUM(), 0)
> retourne 0, et HAVING 0 > 0 est faux. L'ingredient est cache, pas supprime. Si on rachete
> ou reproduise du Premix, il reapparairait automatiquement."

---

## ETAPE 5 -- Publier sur la boutique web

#### Ce que tu montres

1. Dans la sidebar C#, aller dans la section de gestion des produits web
2. Selectionner `Long Island Ice Tea` dans la liste des BOM N3 disponibles
3. Cliquer **"Publier sur la boutique"**
4. Remplir :
   - Nom public : `Long Island Ice Tea`
   - Description : `Le classique cocktail americain — vodka, tequila, rhum, gin, triple sec, citron et cola`
   - Prix de vente : `12.00` €
   - TVA : `21%`
   - Stock disponible : `10` (auto-rempli depuis bom_stocks)
5. Cliquer **"Publier"**
6. Ouvrir le navigateur sur `http://localhost` → La boutique Laravel s'affiche
7. Le Long Island Ice Tea apparait dans le catalogue avec son prix et son stock

**Fichiers a ouvrir dans Visual Studio :**
- `DAL/ProduitWebDAL.cs` — INSERT dans table `produits`

**Fichiers a ouvrir dans VS Code (Laravel) :**
- `app/Models/Produit.php` — meme table MySQL

#### Ce que tu dis

> "La publication cree un enregistrement dans la table `produits` que Laravel lit directement.
> Il n'y a pas d'API entre C# et Laravel — les deux partagent la meme DB MySQL.
> C# ecrit dans `produits`, Laravel lit dans `produits`. La coherence est garantie par la DB,
> pas par une communication applicative."

> "Le champ `id_bom_stock_source` dans `produits` pointe vers la recette BOM.
> Quand un client achete, le checkout Laravel decrementera le `bom_stocks` correspondant via FIFO."

#### Questions probables

**Q : N'est-il pas risque que C# et Laravel partagent la meme DB sans API ?**
> "C'est un compromis de l'architecture hybride. Dans un systeme de production, une API REST
> serait preferable pour isoler les systemes. Ici, c'est un contexte academique : la complexite
> d'une API ajouterait du travail sans valeur pedagogique supplementaire. La securite est assuree
> par les contraintes DB (foreign keys, transactions) qui s'appliquent quel que soit le client."

---

## ETAPE 6 -- Parcours client Laravel

### 6.1 Catalogue (visiteur)

#### Ce que tu montres

1. Ouvrir `http://localhost` dans le navigateur
2. La page d'accueil affiche les produits disponibles
3. Le Long Island Ice Tea apparait avec prix et stock
4. Cliquer sur le produit → page detail avec description complete
5. Le bouton "Ajouter au panier" est visible mais redirige vers login si non connecte

**Fichiers a ouvrir dans VS Code (Laravel) :**
- `resources/views/boutique/index.blade.php`
- `app/Http/Controllers/BoutiqueController.php` — methode index
- `routes/web.php` — routes publiques

#### Ce que tu dis

> "La page catalogue est publique — pas d'authentification requise pour voir les produits.
> Le Controller charge les produits avec eager loading pour eviter le N+1 :"

```php
// app/Http/Controllers/BoutiqueController.php
public function index()
{
    $produits = Produit::where('actif', 1)
        ->where('stock_disponible', '>', 0)
        ->with('categorie')    // eager loading -- 1 requete au lieu de N
        ->orderBy('nom')
        ->get();
    return view('boutique.index', compact('produits'));
}
```

> "Sans eager loading (with('categorie')), Eloquent ferait 1 requete pour les produits + 1 requete
> par produit pour sa categorie = N+1 requetes. Avec eager loading : 2 requetes au total."

#### Questions probables

**Q : C'est quoi le probleme N+1 ?**
> "Si j'ai 20 produits et que chacun a une categorie, sans eager loading Eloquent fait :
> 1 SELECT sur produits (retourne 20 lignes)
> puis pour chaque produit, 1 SELECT sur categories = 20 requetes.
> Total : 21 requetes. Avec ->with('categorie'), Eloquent fait :
> 1 SELECT sur produits + 1 SELECT categories WHERE id IN (1,2,3...) = 2 requetes.
> Sur 1000 produits, N+1 = 1001 requetes vs 2. C'est critique pour les performances."

---

### 6.2 Inscription client

#### Ce que tu montres

1. Cliquer **"S'inscrire"** dans le menu
2. Remplir le formulaire : Nom, Prenom, Email, Mot de passe, Confirmation
3. Soumettre
4. Redirection vers le catalogue avec message de succes

**Fichiers a ouvrir dans VS Code :**
- `app/Http/Controllers/ClientAuthController.php` — methode register
- `app/Http/Requests/RegisterRequest.php` — Form Request validation
- `app/Models/Client.php`

#### Ce que tu dis

> "La validation passe par un Form Request (RegisterRequest) — pas dans le Controller.
> Form Request centralise les regles de validation et les messages d'erreur.
> Le Controller reste propre et ne gere que le flux."

```php
// app/Http/Requests/RegisterRequest.php
public function rules(): array
{
    return [
        'nom'                  => 'required|string|max:100',
        'prenom'               => 'required|string|max:100',
        'email'                => 'required|email|unique:clients,email',
        'password'             => 'required|min:8|confirmed',  // confirmed = password_confirmation
    ];
}
```

> "Le mot de passe est hache avec BCrypt via Laravel (`Hash::make()`), compatible avec C#.
> Le meme client pourrait theoriquement se connecter sur les deux interfaces."

#### Questions probables

**Q : Qu'est-ce que CSRF et comment Laravel le protege ?**
> "CSRF (Cross-Site Request Forgery — falsification de requete inter-site) est une attaque
> ou un site malveillant fait envoyer une requete a votre application par le navigateur de la victime.
> Laravel genere un token CSRF unique par session et l'inclut dans chaque formulaire via @csrf.
> A chaque POST, Laravel verifie que le token soumis correspond au token de session.
> Un site tiers ne peut pas connaitre ce token, donc il ne peut pas forger la requete."

**Q : Pourquoi valider dans un Form Request plutot que dans le Controller ?**
> "Single Responsibility Principle (principe de responsabilite unique) : le Controller orchestre
> le flux, le Form Request valide les entrees. Si la logique de validation change, je touche
> uniquement RegisterRequest.php, pas le Controller. C'est plus testable et plus lisible."

---

### 6.3 Connexion

#### Ce que tu montres

1. Cliquer **"Se connecter"**
2. Saisir email + mot de passe
3. Connexion reussie → redirection vers catalogue
4. Le menu affiche maintenant "Mon compte" et "Panier (0)"

**Fichiers a ouvrir dans VS Code :**
- `app/Http/Controllers/ClientAuthController.php` — methode login
- `app/Http/Middleware/ClientAuth.php`

#### Ce que tu dis

> "Laravel utilise des sessions HTTP httpOnly pour l'authentification des clients.
> Le cookie de session est httpOnly : JavaScript ne peut pas y acceder, ce qui protege
> contre les attaques XSS (Cross-Site Scripting — injection de code JavaScript malveillant).
> La session stocke l'ID du client, pas ses donnees completes."

```php
// app/Http/Controllers/ClientAuthController.php
public function login(LoginRequest $request)
{
    $client = Client::where('email', $request->email)->first();
    if (!$client || !Hash::check($request->password, $client->password)) {
        return back()->withErrors(['email' => 'Identifiants incorrects']);
    }
    session(['client_id' => $client->id]);
    return redirect()->route('boutique.index');
}
```

#### Questions probables

**Q : Pourquoi ne pas utiliser le systeme Auth de Laravel (Auth::attempt) ?**
> "Auth::attempt est conçu pour le modele User de Laravel. Ici, le modele Client est separe
> et utilise une table `clients` distincte des `utilisateurs` (les comptes ERP C#).
> Un client Laravel ne doit pas avoir acces a l'ERP. La separation des guards garantit
> qu'un client ne peut pas usurper un role admin."

**Q : C'est quoi XSS et comment le cookie httpOnly protege ?**
> "XSS (Cross-Site Scripting) = injection de JavaScript malveillant dans une page.
> Si un attaquant injecte `<script>document.cookie</script>`, il peut voler le cookie de session.
> httpOnly empeche JavaScript d'acceder au cookie — il n'est transmis que via HTTP.
> Laravel echappe aussi automatiquement les variables Blade ({{ $var }}) contre l'injection HTML."

---

### 6.4 Panier AJAX

#### Ce que tu montres

1. Sur la page catalogue, cliquer **"Ajouter au panier"** sur le Long Island Ice Tea
2. Observer : pas de rechargement de page — la quantite dans le menu se met a jour dynamiquement
3. Cliquer sur **"Panier"** → page panier affiche : 1× Long Island Ice Tea, 12.00€
4. Modifier la quantite a 2 → sous-total se met a jour
5. Cliquer "Commander"

**Fichiers a ouvrir dans VS Code :**
- `app/Http/Controllers/PanierController.php`
- `resources/views/boutique/partials/panier-count.blade.php`
- `routes/web.php` — route POST /panier/ajouter

#### Ce que tu dis

> "L'ajout au panier utilise une requete AJAX (Asynchronous JavaScript And XML — requete
> HTTP en arriere-plan sans rechargement de page) vers l'API interne Laravel.
> Le Controller retourne du JSON, le JavaScript met a jour le compteur du menu sans recharger.
> C'est du rendu serveur partiel — uniquement le panier utilise AJAX, le reste est Blade classique."

```php
// app/Http/Controllers/PanierController.php
public function ajouter(Request $request)
{
    $idProduit = $request->integer('id_produit');
    $quantite  = $request->integer('quantite', 1);
    $panier    = session()->get('panier', []);
    $panier[$idProduit] = ($panier[$idProduit] ?? 0) + $quantite;
    session()->put('panier', $panier);
    return response()->json([
        'success' => true,
        'total'   => array_sum($panier)
    ]);
}
```

#### Questions probables

**Q : Le panier est stocke en session ou en base de donnees ?**
> "En session PHP pour la simplicite. La session est stockee cote serveur (fichier ou cache),
> le client ne voit que le cookie de session. Inconvenient : si le client change d'appareil,
> son panier est perdu. En production, on stockerait le panier en DB pour la persistance
> multi-appareil. C'est un compromis de simplicite acceptable pour un projet academique."

---

### 6.5 Checkout FIFO

#### Ce que tu montres

1. Sur la page panier, cliquer **"Commander"**
2. Page checkout : resume commande, adresse de livraison, mode de paiement (Stripe test)
3. Saisir carte de test Stripe : `4242 4242 4242 4242`
4. Cliquer **"Payer 24.00€"** (2 × 12€)
5. Confirmation de commande → numero de commande genere
6. Ouvrir phpMyAdmin → verifier `bom_stocks` : `quantite_disponible` des Long Island passe de 10 a 8

**Fichiers a ouvrir dans VS Code :**
- `app/Http/Controllers/CheckoutController.php` — methode valider
- `app/Models/BomStock.php`

#### Ce que tu dis

> "Le checkout est l'operation la plus critique — elle touche au stock et au paiement.
> Tout est dans une transaction DB avec lockForUpdate pour eviter les conditions de course :"

```php
DB::beginTransaction();
$stocks = BomStock::where('id_fiche', $idFiche)
    ->where('quantite_disponible', '>', 0)
    ->orderBy('date_production', 'asc')
    ->lockForUpdate()->get();
foreach ($stocks as $stock) {
    if ($restant <= 0) break;
    $aConsommer = min($restant, $stock->quantite_disponible);
    $stock->quantite_disponible -= $aConsommer;
    $stock->save();
    $restant -= $aConsommer;
}
$panier->update(['statut' => 'payee', 'date_commande' => now()]);
DB::commit();
```

> "lockForUpdate() pose un verrou exclusif sur les lignes lues — aucune autre transaction
> ne peut les modifier jusqu'au COMMIT. Si deux clients essaient d'acheter le dernier stock
> simultanement, le second attendra que le premier ait committe. C'est le I de ACID : Isolation.
> Sans ce verrou, on pourrait vendre plus de stock qu'il n'en existe — overselling."

> "Le FIFO se fait via orderBy('date_production', 'asc') : on consomme d'abord
> les Long Island produits en premier (les plus anciens)."

#### Questions probables

**Q : Que se passe-t-il si le paiement Stripe echoue apres avoir decremente le stock ?**
> "Le paiement est valide AVANT de decrementer le stock. Si Stripe retourne une erreur,
> on ne touche pas au stock. Si le stock est decremente et que la mise a jour du statut commande
> echoue, le rollback() remet le stock a son etat initial. La transaction garantit la coherence."

**Q : Qu'est-ce qu'une condition de course (race condition) ?**
> "Imaginons 2 clients qui veulent le dernier Long Island. Sans verrou :
> Client A lit stock = 1. Client B lit stock = 1. Client A decremente → stock = 0 et commit.
> Client B decremente → stock = -1 et commit. On a vendu quelque chose qui n'existe plus.
> lockForUpdate() empeche ca : Client B attend que Client A committe, puis il lit stock = 0
> et recoit un message 'rupture de stock'."

---

## ETAPE 7 -- Retour ERP : verification stock et commandes

#### Ce que tu montres

1. Retourner dans l'application C# WinForms
2. Aller dans **"Fiches & Stock"** → mode Stock reel → chip "Bar de ville"
3. Verifier le Cola : stock reduit apres consommation N3 (600ml consommes pour 10 cocktails)
4. Aller dans **"Commandes"** → `FrmCommandes` liste les commandes recues depuis la boutique
5. La commande du client est visible avec statut "Payee", date, montant total
6. Cliquer sur la commande → detail : 2× Long Island Ice Tea, adresse livraison, info client
7. Changer le statut a "En preparation"
8. Changer le statut a "Expediee"
9. Retourner sur la boutique Laravel → aller dans "Mon compte" → "Mes commandes"
10. Le client voit son statut mis a jour : "Expediee"

**Fichiers a ouvrir dans Visual Studio :**
- `Forms/Commandes/FrmCommandes.cs`
- `DAL/CommandeDAL.cs`

#### Ce que tu dis

> "Les commandes creees par Laravel sont immediatement visibles dans l'ERP C# car elles sont
> dans la meme table MySQL `commandes`. L'ERP permet a l'operateur de gerer le workflow :
> Payee → En preparation → Expediee → Livree.
> Chaque changement de statut est horodate dans `commande_historique` pour la traçabilite."

> "La synchronisation entre C# et Laravel est instantanee et bidirectionnelle grace a la DB partagee.
> C'est la valeur cle de l'architecture hybride : un seul referentiel de donnees."

#### Questions probables

**Q : Et si l'ERP et le site web essaient de modifier la meme commande en meme temps ?**
> "On est dans le meme cas que pour le stock : lockForUpdate() dans les transactions critiques.
> Pour les mises a jour de statut, c'est moins critique car c'est sequentiel en pratique.
> On pourrait ajouter un champ `updated_at` et un check optimiste : si `updated_at` a change
> depuis qu'on a lu, on refuse la mise a jour et on demande de recharger. C'est le verrouillage
> optimiste (Optimistic Locking) — adapte quand les conflits sont rares."

---

## ETAPE 8 -- Defense de la base de donnees

### 8.1 Schema global

#### Ce que tu montres

1. Ouvrir phpMyAdmin → `http://localhost:8080`
2. Selectionner la base de donnees `artisastock`
3. Aller dans "Structure" pour montrer les tables
4. Pointer les tables cles et leurs relations

**Tables principales :**
```
utilisateurs          -- comptes ERP (admin, operateurs)
clients               -- comptes boutique web
stocks                -- entrepots physiques
activites             -- domaines metier (Bar, Patisserie...)
activite_stocks       -- liaison many-to-many activites ↔ stocks
contextes             -- hierarchies de production
niveaux_production    -- N1/N2/N3 par contexte
fiches_ingredients    -- catalogue ingredients (classe/definition)
fournisseurs          -- fournisseurs
lots_ingredients      -- achats physiques (instances avec stock)
recettes_bom          -- recettes de fabrication
lignes_bom            -- composants de chaque recette
bom_stocks            -- stock de produits finis (apres production)
productions           -- historique des productions
produits              -- catalogue boutique web (synchronise avec bom)
commandes             -- commandes clients
lignes_commande       -- detail de chaque commande
commande_historique   -- log des changements de statut
```

#### Ce que tu dis

> "La base est normalisee en 3NF (Troisieme Forme Normale — 3rd Normal Form) :
> 1NF = pas de valeurs multiples dans une colonne (une case = une valeur atomique)
> 2NF = chaque attribut non-cle depend de toute la cle (pas de dependances partielles)
> 3NF = pas de dependances transitives (pas d'attribut qui depend d'un autre attribut non-cle)
> Toutes les relations many-to-many ont leur table de jonction. Les foreign keys sont declarees."

#### Questions probables

**Q : Pourquoi separer `fiches_ingredients` et `lots_ingredients` ?**
> "La fiche ingredient est la definition (classe) : Vodka, 700ml, liquide, densite 0.79.
> Le lot est l'achat concret (instance) : 5 bouteilles de Vodka achetees le 14 juin chez X a 15€.
> Cette separation permet de : (1) avoir plusieurs fournisseurs pour le meme ingredient,
> (2) tracker le cout reel par lot, (3) implementer FIFO sur les lots."

---

### 8.2 Tables cles et contraintes

#### Ce que tu montres

1. Ouvrir la table `lignes_bom` dans phpMyAdmin → montrer les colonnes et les types
2. Montrer les FOREIGN KEYS : `id_recette`, `id_fiche_ingredient`
3. Montrer la contrainte ON DELETE RESTRICT (on ne peut pas supprimer une fiche utilisee dans une recette)
4. Ouvrir `lots_ingredients` → montrer la colonne `quantite_disponible` et sa contrainte CHECK >= 0
5. Ouvrir la table `commandes` → enum statut : `en_attente | payee | en_preparation | expediee | livree | annulee`

#### Ce que tu dis

> "Les contraintes d'integrite referentielle sont definies au niveau DB, pas seulement dans le code.
> Meme si le code C# ou PHP a un bug, la DB refusera de creer une incoherence.
> C'est la defense en profondeur : plusieurs couches de validation."

> "La contrainte CHECK (quantite_disponible >= 0) empeche un stock negatif meme en cas de bug
> dans la logique FIFO. Si un UPDATE essaie de mettre -1, MySQL lance une erreur,
> la transaction est rollbackee, le stock reste coherent."

```sql
-- Extrait de /sql/005_lots_ingredients.sql
CREATE TABLE lots_ingredients (
    id                   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    id_fiche_ingredient  INT UNSIGNED NOT NULL,
    id_stock             INT UNSIGNED NOT NULL,
    quantite_initiale    DECIMAL(10,3) NOT NULL,
    quantite_disponible  DECIMAL(10,3) NOT NULL,
    prix_unitaire_ht     DECIMAL(10,4) NOT NULL,
    tva_pct              DECIMAL(5,2)  NOT NULL DEFAULT 21.00,
    date_achat           DATE          NOT NULL,
    id_fournisseur       INT UNSIGNED,
    PRIMARY KEY (id),
    CONSTRAINT chk_qte_dispo CHECK (quantite_disponible >= 0),
    CONSTRAINT chk_qte_init  CHECK (quantite_initiale > 0),
    FOREIGN KEY (id_fiche_ingredient) REFERENCES fiches_ingredients(id) ON DELETE RESTRICT,
    FOREIGN KEY (id_stock) REFERENCES stocks(id) ON DELETE RESTRICT
);
```

#### Questions probables

**Q : Pourquoi ON DELETE RESTRICT et pas ON DELETE CASCADE ?**
> "CASCADE supprimerait les lots si on supprime la fiche ingredient. On perdrait l'historique
> des achats et la traçabilite. RESTRICT empeche la suppression si des lots existent.
> L'operateur doit d'abord archiver les lots, puis il peut desactiver la fiche (soft delete via actif=0).
> On ne supprime jamais physiquement les donnees de traçabilite — seulement soft delete."

**Q : Qu'est-ce que la normalisation 3NF et pourquoi c'est important ?**
> "La 3NF elimine la redondance des donnees. Sans normalisation, si je stocke le nom du fournisseur
> dans chaque lot, une correction du nom necessite N UPDATEs avec risque de coherence.
> Normalise, le nom est dans `fournisseurs`, les lots ont juste `id_fournisseur`. 1 seul UPDATE.
> La redondance cause des anomalies d'insertion, de mise a jour et de suppression."

**Q : Qu'est-ce qu'un soft delete et pourquoi l'utiliser ?**
> "Un soft delete (suppression douce) marque l'enregistrement comme inactif (actif = 0)
> au lieu de le supprimer physiquement. Avantages : traçabilite preservee, foreign keys intactes,
> restauration possible. Inconvenient : les requetes doivent toujours filtrer WHERE actif = 1.
> C'est le standard dans les systemes ou l'audit est important (finance, restauration, sante)."

---

## QUESTIONS FLASH (recap)

Reponses courtes aux questions les plus frequentes en defense :

| Question | Reponse courte |
|----------|----------------|
| **Qu'est-ce que le DAL ?** | Data Access Layer — couche qui centralise tous les acces DB. Aucun SQL dans les formulaires. |
| **Injection SQL ?** | Requetes parametrees (`@param`). Le driver escape automatiquement. OWASP #1. |
| **BCrypt vs MD5 ?** | BCrypt est lent par design + salt integre = resistant force brute + rainbow tables. |
| **FIFO ?** | First In First Out — ORDER BY date_achat ASC. On consomme le plus ancien en premier. |
| **ACID ?** | Atomicite + Coherence + Isolation + Durabilite. Garantis par les transactions MySQL. |
| **N+1 ?** | N requetes pour N relations. Fix : eager loading (WITH) = 2 requetes. |
| **CSRF ?** | Token unique par session dans chaque formulaire POST. Laravel verifie a chaque soumission. |
| **lockForUpdate ?** | Verrou exclusif en lecture dans une transaction. Empeche les conditions de race. |
| **LEFT JOIN ?** | Garde toutes les lignes de gauche meme sans correspondance a droite (NULL). |
| **COALESCE ?** | Premier argument non-NULL. COALESCE(SUM(), 0) = 0 si aucun lot. |
| **3NF ?** | Pas de redondance, pas de dependances transitives. 1 fait = 1 endroit. |
| **partial class ?** | Classe C# decoupee en plusieurs fichiers. WinForms separe logique et Designer. |
| **TextChanged vs ValueChanged ?** | ValueChanged = apres validation/LostFocus. TextChanged = a chaque frappe. UX immediat. |
| **CellFormatting ?** | Intercepte l'affichage DGV sans modifier le modele. Separation affichage/donnees. |
| **FrmEditBase abstraite ?** | Impose Valider() et Enregistrer() a chaque sous-classe. Coherence sans copier-coller. |
| **FrmListeBase<T> generique ?** | Un seul code pour toutes les listes. DRY. T = type concret (Stock, Activite...). |
| **OWASP Top 10 ?** | Les 10 vulnerabilites web les plus critiques. #1 = Injection. #2 = Auth cassee. |
| **bom_stocks vs lots_ingredients ?** | lots = achats. bom_stocks = produits finis issus de la production. |
| **Soft delete vs hard delete ?** | actif = 0 au lieu de DELETE. Preserve la traçabilite et les foreign keys. |
| **GROUP BY + HAVING ?** | GROUP BY = agreger par groupe. HAVING = filtrer apres agregation (WHERE = avant). |
| **AppliquerMode ?** | Methode qui change textes boutons + visibilite + recharge la DGV selon le mode toggle. |
| **TypeSource enum ?** | Distingue si une ligne BOM pointe vers lots_ingredients ou bom_stocks. |
| **Condition de race ?** | 2 transactions lisent le meme stock = 1 en meme temps. lockForUpdate empeche ca. |
| **httpOnly cookie ?** | Cookie inaccessible depuis JavaScript. Protege contre vol de session via XSS. |
| **Form Request Laravel ?** | Classe de validation separee du Controller. Single Responsibility Principle. |

---

## MEMO TIMING

| Etape | Duree estimee | Cumulee |
|-------|--------------|---------|
| Introduction + Architecture | 2 min | 2 min |
| Etape 0 — Login | 2 min | 4 min |
| Etape 1 — Infrastructure | 5 min | 9 min |
| Etape 2 — Fiches & Stock | 8 min | 17 min |
| Etape 3 — Recettes BOM | 5 min | 22 min |
| Etape 4 — Production (2 passes) | 6 min | 28 min |
| Etape 5 — Publication web | 2 min | 30 min |
| Etape 6 — Parcours client Laravel | 6 min | 36 min |
| Etape 7 — Retour ERP | 3 min | 39 min |
| Etape 8 — Defense DB | 5 min | 44 min |
| Questions libres | ~15 min | ~60 min |

---

## POINTS DE STRESS -- A ne pas rater

1. **Toggle Fiches/Stock reel** — Montrer les deux modes et expliquer AppliquerMode. Les boutons changent, les colonnes changent, le DAL change.
2. **Prix live TextChanged** — Demontrer que le calcul se met a jour en temps reel a la frappe, avant LostFocus.
3. **Production en 2 passes** — N3 refuse si N2 n'a pas ete produit. Montrer le message d'erreur si necessaire.
4. **FIFO visible** — Apres production, montrer que quantite_disponible a baisse dans le bon lot (le plus ancien d'abord).
5. **lockForUpdate** — Mentionner la protection contre les conditions de race meme sans la demontrer en direct.
6. **Transaction rollback** — Savoir expliquer le scenario ou une transaction echoue et montre le rollback.
7. **BCrypt compatible** — Souligner la compatibilite C# ↔ PHP : meme algorithme, meme hash, meme table possible.
8. **HAVING vs WHERE** — Etre pret a expliquer pourquoi HAVING et pas WHERE pour filtrer stock_actuel > 0.
9. **LEFT JOIN vs INNER JOIN** — L'examinateur peut tester sur n'importe quel JOIN du code.
10. **partial class CS0136** — Si on parle des formulaires WinForms, savoir expliquer la regle et le probleme de portee variable.

---

*Fin du script de defense — ArtisaStock — Ernest — 2e annee Bachelier IT — 2026-07-01*
