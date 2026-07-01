# 06 — Analyse des Exercices du Prof PDWEB

> Ce document analyse le style de codage PHP attendu par le professeur,
> extrait des deux exercices fournis (`form/` et `labo/`).
> Ces observations doivent guider l'implémentation du site PHP.

---

## 📂 Ce qui a été fourni

```
Exercices Prof/
├── form/                    ← Exercice sur les formulaires PHP
│   ├── .pdweb.php           ← Framework maison du prof (biblio PHP)
│   ├── .config.php          ← Config charset (ANSI ou UTF-8)
│   ├── _site.php            ← Composants de mise en page du site
│   ├── index.php            ← Page avec formulaire
│   ├── traiter.php          ← Page réceptrice (var_dump du POST)
│   └── base.css             ← Styles de base
│
└── labo/                    ← Laboratoire PHP (bases du langage)
    ├── index.php            ← Exercices PHP : types, fichiers, tableaux
    └── data/dd3.txt         ← Fichier texte tabulé (données à lire)
```

---

## 🔑 Observations Clés sur le Style du Prof

### 1. Charset : windows-1252 (ANSI) par défaut

Le prof utilise **windows-1252** et non UTF-8 par défaut. C'est visible dans :
- `define("CHARSET", ANSI)` dans `labo/index.php`
- `header("content-type:text/html;charset=windows-1252")` dans `traiter.php`
- Le `.config.php` du framework permet de choisir ANSI ou UTF8

**Pour notre projet** : UTF-8 est plus adapté (noms français, accents). On déclarera explicitement UTF-8 partout :
```php
header("content-type:text/html;charset=utf-8");
// Et dans les meta HTML :
// <meta charset="utf-8"/>
```

---

### 2. Le Framework PDWEB du Prof (`.pdweb.php`)

Le prof a construit une **bibliothèque de fonctions PHP** qui génère du HTML. Elle expose :

| Fonction | Rôle |
|----------|------|
| `PDWEB_WriteHtmlDocument($head, $body, $ctx)` | Génère le squelette HTML complet |
| `PDWEB_WriteHtmlTag($tabs, $tag, $attrs, $content)` | Génère une balise HTML avec indentation |
| `PDWEB_WriteForm($tabs, $name, $action, $method, $options, $fields)` | Génère un formulaire complet depuis un tableau déclaratif |
| `PDWEB_DeclareContentCharset()` | Envoie le header Content-Type |
| `PDWEB_Include($filename)` | Include sécurisée d'un fichier |
| `PDWEB_HtmlContent($str)` | Échappe le contenu pour HTML |
| `PDWEB_IsInteger($v)` / `PDWEB_IsReal($v)` | Validations de types |

**Style caractéristique** : utilisation intensive de **callbacks/closures** passées en paramètre.

```php
// Style du prof : tout est généré via des callbacks
SITE_EcrirePage("Titre", "style.css", function($tabs, &$contexte)
{
    PDWEB_WriteHtmlTag($tabs, "p", null, "Mon paragraphe");
    PDWEB_WriteForm($tabs, "MonFormulaire", "traiter.php", POST, [],
    [
        "Nom" => [
            "type"     => "Text",
            "labelText"=> "Votre nom :",
            "required" => true,
            "minLength"=> 2,
            "maxLength"=> 30
        ]
    ]);
});
```

**⚠️ Question importante** : le prof attend-il qu'on utilise **son framework** dans notre projet, ou c'est juste un exemple de cours ? À clarifier avec lui. Si oui, on récupère le fichier `.pdweb.php` et on l'intègre.

---

### 3. Style de Codage PHP du Prof (labo)

Extrait du `labo/index.php` — ce qu'il enseigne et valorise :

#### Constantes avec `define()`
```php
define("CHARSET", ANSI);   // Utilise des constantes nommées, pas des strings magiques
define("TITLE", "Mon titre");
```

#### Comparaisons strictes `===`
```php
// Le prof distingue == (égalité faible) et === (égalité stricte)
if ($x === $y) { ... }        // ✅ Préféré
if ($x == $z) { ... }         // ⚠️ Éviter sauf intention explicite
if (@file_get_contents(...) !== false) { ... }   // Toujours tester !== false
```

#### Lecture de fichier texte tabulé
```php
if (($contenu = @file_get_contents("data/fichier.txt")) !== false)
{
    $lignes = explode("\n", str_replace(["\r\n", "\r"], "\n", $contenu));
    foreach ($lignes as $ligne)
    {
        $champs = explode("\t", $ligne);
        // Traitement...
    }
}
```
→ **Couverture du critère "manipulation de fichier texte"** pour PDWEB.

#### Tri personnalisé avec `usort()`
```php
usort($tableau, function($a, $b)
{
    if (strcasecmp($a, "special") == 0) $a = chr(255); // Toujours en dernier
    return strcasecmp($a, $b);  // Tri alphabétique insensible à la casse
});
```

#### Fonctions utilitaires réutilisables
```php
// Accès à un tableau multidimensionnel par chemin string
function Element(&$tableau, $chemin)
{
    $chemin = explode("/", $chemin);
    $noeud = &$tableau;
    foreach ($chemin as $cle)
    {
        if (!is_array($noeud) || !isset($noeud[$cle])) return false;
        $noeud = &$noeud[$cle];
    }
    return $noeud;
}
// Usage : Element($vaisseau, "Propulsion/Reacteur/Puissance")
```

#### Gestion des erreurs silencieuse avec `@`
```php
@print($x);           // Supprime l'erreur si $x n'existe pas
@file_get_contents(); // Supprime le warning si le fichier n'existe pas
```

---

### 4. Formulaires PHP (form/index.php + traiter.php)

**Structure du formulaire dans l'exercice** :

```php
PDWEB_WriteForm($tabs, "Inscription", "traiter.php", POST, [ "target" => "_blank" ],
[
    "Nom" => [
        "type"       => "Text",
        "labelText"  => "Nom de famille :",
        "accessKey"  => "n",
        "tooltip"    => "Veuillez encoder votre nom de famille",
        "placeholder"=> "Votre nom",
        "required"   => true,
        "minLength"  => 2,
        "maxLength"  => 30
    ],
    "Prenom" => [
        "type"       => "Text",
        "labelText"  => "Prénom(s) :",
        "required"   => true,
        "minLength"  => 2,
        "maxLength"  => 40
    ],
    "Inscrire" => [
        "type"       => "submit",
        "buttonText" => "Inscrivez-vous"
    ]
]);
```

**Ce que le prof valorise dans les formulaires** (extrait de la grille d'exam) :
- Labels explicites liés aux champs (`for`/`id`)
- `accessKey` pour l'accessibilité
- `tooltip` (title ou placeholder)
- `required`, `minLength`, `maxLength` en HTML5
- Le POST est traité côté serveur avec validation

**Page traiter.php** : très simple dans l'exercice (juste un `var_dump`). Dans notre projet, elle doit valider et insérer en BDD.

---

## 🎯 Ce qu'on applique dans notre projet

### Style PHP à adopter

```php
<?php
// 1. Toujours déclarer le charset en premier
header("content-type:text/html;charset=utf-8");

// 2. Constantes pour les configs
define("DB_HOST", "localhost");
define("DB_NAME", "charlesnadejda");

// 3. Comparaisons strictes partout où c'est pertinent
if ($stmt->rowCount() === 0) { ... }
if ($role === "admin") { ... }

// 4. Validation PHP des formulaires AVANT tout INSERT
$erreurs = [];
$nom = trim($_POST["nom"] ?? "");
if (empty($nom)) {
    $erreurs["nom"] = "Le nom est obligatoire.";
} elseif (strlen($nom) < 2 || strlen($nom) > 100) {
    $erreurs["nom"] = "Le nom doit faire entre 2 et 100 caractères.";
}

// 5. Fonctions utilitaires dans includes/functions.php
function sanitize($valeur) {
    return htmlspecialchars(trim($valeur ?? ""), ENT_QUOTES, "utf-8");
}
```

### Lecture de fichier texte (pour couvrir le critère PDWEB)

Notre projet peut lire un fichier de configuration ou de données :
```php
// Exemple : lire un fichier de config ou de logs
if (($contenu = @file_get_contents("data/config.txt")) !== false) {
    $lignes = explode("\n", str_replace(["\r\n", "\r"], "\n", $contenu));
    foreach ($lignes as $ligne) {
        // Traitement...
    }
}
```

### Formulaires HTML (style proche du prof)

```html
<form method="POST" action="traiter.php">
    <div class="champ <?= isset($erreurs['nom']) ? 'erreur' : '' ?>">
        <label for="nom" accesskey="n">
            Nom de famille : <span class="requis">*</span>
        </label>
        <input type="text"
               id="nom"
               name="nom"
               value="<?= sanitize($_POST['nom'] ?? '') ?>"
               title="Veuillez encoder votre nom de famille"
               placeholder="Votre nom"
               required
               minlength="2"
               maxlength="100">
        <?php if (isset($erreurs['nom'])): ?>
            <span class="msg-erreur"><?= $erreurs['nom'] ?></span>
        <?php endif; ?>
    </div>
</form>
```

---

## ❓ Questions à poser au Prof

1. **Doit-on utiliser ton framework `.pdweb.php`** dans notre projet, ou peut-on coder en PHP standard ?
2. **Charset** : UTF-8 ou windows-1252 pour le projet ?
3. **PDO obligatoire** pour MySQL, ou `mysqli` accepté aussi ?
4. **Structure des fichiers** : le prof a-t-il des conventions sur l'organisation des dossiers ?

---

## 📝 Notes pour Claude (recontextualisation)

- Le prof a une librairie PHP maison (`.pdweb.php`) qui génère du HTML via callbacks
- Il enseigne PHP **procédural** (pas OOP, pas de framework type Laravel)
- Charset windows-1252 dans ses exemples, mais UTF-8 préférable pour nous
- Il valorise : comparaisons strictes `===`, `define()` pour constantes, `usort()`, lecture fichiers texte
- Le critère "manipulation de données externes" peut être couvert par la lecture d'un fichier texte OU MySQL (on fait les deux)
- La grille PDWEB mentionne "fichier texte, fichier XML OU base MySQL" — les trois sont acceptés
- Exercices dans : `Project ArtisaStock/Doc ArtisaStock/Exercices Prof/`
