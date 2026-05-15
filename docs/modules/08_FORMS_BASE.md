# 08 — Forms Base & Utilitaires
> Classes de base, palette centralisee et utilitaires UI partages
> Derniere mise a jour : 2026-05-16

## Vue d'ensemble

Ce module regroupe les **classes abstraites** qui factorisent le comportement commun des formulaires, la **palette de couleurs** centralisee, et les **utilitaires** partages (conversion d'unites, helpers de saisie, extensions string).

### Regles Journal associees

- **Regle #6** : DGV sizing — `AutoSizeColumnsMode.AllCells` + `MinimumWidth` sur chaque colonne
- **Regle #12** : CS0136 — ne pas reutiliser un nom de variable lambda dans un scope englobant
- **Regle #22** : FrmEditBase pattern — toujours heriter, jamais recreer errorProvider/boutons
- **Regle #24** : CellFormatting — styler via `DefaultCellStyle` ou `AppliquerStylesLignes()`, pas event row par row

---

## Fichiers source

| Fichier | Role |
|---------|------|
| `Forms/FrmEditBase.cs` | Classe abstraite formulaire d'edition (template method) |
| `Forms/FrmListeBase.cs` | Classe abstraite generique formulaire liste + CRUD |
| `Forms/FrmLogin.cs` | Authentification BCrypt |
| `Forms/AppColors.cs` | Palette centralisee (source de verite unique) |
| `Forms/UnitConvertisseur.cs` | Conversion d'unites masse/volume/piece |
| `Forms/FormHelper.cs` | Helpers UI (saisie decimale, selection au focus) |
| `Forms/StringExtensions.cs` | Extension `.NullIfEmpty()` pour TextBox -> DB |

---

## `FrmEditBase` — Template Method d'edition

Classe abstraite qui fournit le squelette commun a tous les formulaires Create/Update.

### Membres fournis par la base

| Membre | Type | Role |
|--------|------|------|
| `errorProvider` | `ErrorProvider` | Affichage inline des erreurs (BlinkStyle.NeverBlink) |
| `btnEnregistrer` | `Button` | Bouton principal — fond ChocoBrand, texte blanc |
| `btnAnnuler` | `Button` | Bouton secondaire — fond GreyBtn, texte ChocoBrand |

### Methodes

| Methode | Visibilite | Role |
|---------|------------|------|
| `PositionnerBoutons(int y)` | `protected` | Place les boutons a Y et ajuste ClientSize.Height |
| `Confirmer()` | `private` | Cycle : Clear -> Valider -> Sauvegarder -> OK |
| `Valider()` | `protected abstract` | A implementer : retourne true si donnees valides |
| `Sauvegarder()` | `protected abstract` | A implementer : persiste via le DAL |

### Cycle de vie

```
[Clic Enregistrer]
    |-> errorProvider.Clear()
    |-> Valider()          -- sous-classe
    |   false -> return (erreurs affichees via errorProvider)
    |   true  -> Sauvegarder()  -- sous-classe
    |            |-> success -> DialogResult.OK + Close()
    |            |-> exception -> MessageBox erreur
```

### Proprietes du formulaire

- `FormBorderStyle.FixedDialog`, `MaximizeBox = false`, `MinimizeBox = false`
- `StartPosition = CenterParent`
- `BackColor = AppColors.CremeWarm`

### Protocole d'utilisation

1. Heriter de `FrmEditBase`
2. Dans le constructeur : creer les controles metier, appeler `PositionnerBoutons(y)` en dernier
3. Implementer `Valider()` et `Sauvegarder()`
4. Ne pas declarer btnEnregistrer, btnAnnuler ni errorProvider

---

## `FrmListeBase<T>` — Liste generique CRUD

Classe abstraite generique `where T : class`. Fournit un DGV + boutons CRUD + cycle de vie complet.

### Membres fournis par la base

| Membre | Type | Role |
|--------|------|------|
| `dgv` | `DataGridView` | Grille ancree aux 4 bords, lignes alternees |
| `lblTitre` | `Label` | Titre 13pt Bold ChocoBrand |
| `btnAjouter` | `Button` | "+ Ajouter" — fond ChocoBrand |
| `btnModifier` | `Button` | "Modifier" — fond GreenOk |
| `btnSupprimer` | `Button` | "Supprimer" — fond RedCrit |
| `btnFermer` | `Button` | "Fermer" — fond GreyBtn |
| `BtnX` | `const int = 736` | Position X colonne de boutons |
| `BtnYExtra` | `const int = 196` | Y disponible pour bouton supplementaire |

### Membres abstraits (a implementer)

| Membre | Signature | Role |
|--------|-----------|------|
| `Titre` | `string Titre { get; }` | Titre fenetre et label |
| `ChargerDonnees()` | `List<T> ChargerDonnees()` | Appelle le DAL, retourne la liste |
| `ConfigurerColonnes()` | `void ConfigurerColonnes()` | Configure DGV apres binding |
| `OuvrirFormulaire(T)` | `Form OuvrirFormulaire(T element)` | Retourne le formulaire d'edition (null=creation) |
| `Supprimer(T)` | `void Supprimer(T element)` | Appelle le DAL pour supprimer |

### Membres virtuels (surchargeables)

| Membre | Signature | Default |
|--------|-----------|---------|
| `NomElement(T)` | `string NomElement(T element)` | `element?.ToString() ?? "?"` |
| `AppliquerStylesLignes()` | `void AppliquerStylesLignes()` | Ne fait rien |

### Helpers disponibles

| Methode | Role |
|---------|------|
| `Charger()` | Recharge donnees + configure colonnes + styles |
| `Selectionne()` | Retourne `dgv.CurrentRow?.DataBoundItem as T` |
| `ConfigCol(nom, header, largeur, minimum)` | Configure une colonne par son nom |
| `CacherColonnes(params string[])` | Masque des colonnes par nom |

### Layout

- ClientSize : 882 x 490, MinimumSize : 750 x 400, Sizable
- DGV : Location(12,52), Size(710,420), Anchor 4 bords
- Boutons : colonne droite a x=736, ancre Top|Right (sauf Fermer: Bottom|Right)
- Double-clic DGV -> Modifier (Nielsen #7 : flexibilite)
- Confirmation suppression avec DefaultButton.Button2 (Nielsen #3 : prevention erreurs)

---

## `FrmLogin` — Authentification

Formulaire partial (Designer). Authentifie l'utilisateur via `UtilisateurDAL.Authenticate(email, mdp)`.

### Propriete publique

```csharp
public Utilisateur Utilisateur { get; private set; }
```

Disponible apres `DialogResult.OK`. Program.Main instancie ensuite FrmPrincipal.

### Comportement

- Valide que email et mot de passe ne sont pas vides
- Appelle `UtilisateurDAL.Authenticate()` (BCrypt en interne)
- En cas d'echec : message generique "Email ou mot de passe incorrect" (pas de fuite info)
- Exceptions DB : message generique + `Debug.WriteLine` (jamais en prod)
- Enter dans txtMotDePasse -> declenchement connexion
- Mode `#if DEBUG` : pre-remplissage des champs

### Securite

- Bandeau header avec gradient `ChocoBrand -> ChocoMed`
- Aucune exposition de `ex.Message` a l'utilisateur en production

---

## `AppColors` — Palette centralisee

Classe statique `internal`. Source de verite unique pour toute la charte graphique.

### Couleurs de marque (Chocolat)

| Nom | Hex | Usage |
|-----|-----|-------|
| `ChocoBrand` | #3D2817 | Brun chocolat fonce, couleur principale |
| `ChocoMed` | #6F4E37 | Brun moyen, textes secondaires |
| `ChocoAbyss` | #1E0F08 | Brun tres sombre, profondeur (sidebar active) |
| `ChocoDark` | #2C1810 | Brun sombre, variante intermediaire |

### Fonds (Creme)

| Nom | Hex | Usage |
|-----|-----|-------|
| `Creme` | #F5E6D3 | Fond panneaux principaux |
| `CremeWarm` | #FDFBF6 | Fond zones de contenu |
| `CremeBg` | #ECE9D8 | Fond header / grilles |

### Interface

| Nom | Hex | Usage |
|-----|-----|-------|
| `Or` | #D4AF37 | Bouton CTA principal |
| `GreyBtn` | #EFEAE1 | Fond boutons secondaires |
| `Border` | #C3B9A8 | Bordures et separateurs |

### Sidebar

| Nom | Hex | Usage |
|-----|-----|-------|
| `SidebarTxt` | #E8D9C0 | Texte principal sidebar |
| `SidebarMeta` | #9E7B5C | Metadonnees sidebar |
| `HintOnDark` | #C8AF8C | Texte hint sur fond sombre |
| `SidebarActive` | #43301A | Fond nav item actif |
| `SidebarHover` | #372616 | Fond nav item hover |

### Statuts

| Nom | Hex | Usage |
|-----|-----|-------|
| `GreenOk` | #3EA23E | Stock disponible / OK |
| `RedCrit` | #C72C48 | Rupture / critique |
| `OrgWarn` | #D35400 | Alerte / avertissement |

### Fonds de lignes (vue stock)

| Nom | Hex | Usage |
|-----|-----|-------|
| `VertDispo` | #E0F3E0 | Ingredient disponible |
| `OrangeReserv` | #FFEDCC | Ingredient reserve |
| `RougePenur` | #FFDADA | Ingredient en penurie |

### Shell ERP

| Nom | Hex | Usage |
|-----|-----|-------|
| `Surface` | #FBF9F4 | Fond status bar et toolbar |
| `Line1` | #ECE5D7 | Bordure fine entre sections |
| `Line2` | #DDD4C0 | Bordure moyenne |
| `Success` | #1B7A3E | Vert semantique success |
| `Info` | #316AC5 | Bleu info / selection |

---

## `UnitConvertisseur` — Conversion d'unites

Classe statique. Convertit entre unites du meme groupe de mesure.

### Groupes supportes

| Groupe | Unites (ordre croissant) | Unite de base |
|--------|--------------------------|---------------|
| Masse | mg, cg, g, kg | g |
| Volume | ml, cl, dl, l | ml |
| Piece | piece | piece |

### API publique

| Methode | Signature | Description |
|---------|-----------|-------------|
| `UnitesCompatibles` | `IEnumerable<string> UnitesCompatibles(string unite)` | Retourne toutes les unites du meme groupe |
| `SontCompatibles` | `bool SontCompatibles(string u1, string u2)` | True si meme groupe |
| `UniteBase` | `string UniteBase(string unite)` | Retourne l'unite de base (g, ml ou piece) |
| `VersUniteBase` | `decimal VersUniteBase(decimal valeur, string unite)` | Convertit vers l'unite de base |
| `Convertir` | `decimal Convertir(decimal valeur, string uniteSource, string uniteCible)` | Conversion entre unites compatibles. Leve `InvalidOperationException` si incompatibles. |
| `FormatQte` | `string FormatQte(decimal valeur, string unite)` | Formatage auto-adaptatif (ex: 1500g -> "1,5 kg") |
| `FormatPrix` | `string FormatPrix(decimal prix)` | Formatage prix 2 decimales + EUR. 4 decimales si < 0.01. |

### Algorithme FormatQte

1. Convertit la valeur en unite de base
2. Parcourt les unites du plus grand au plus petit
3. Saute `dl` et `cg` (non courantes en artisanat)
4. Choisit la premiere unite ou la valeur >= 1
5. Arrondit : entier si rond, sinon 2 decimales

### Points d'appel obligatoires

- `BomProductionDAL.VerifierDisponibilite()` : avant comparaison stock
- `BomProductionDAL.Simuler()` : avant comparaison stock
- `BomProductionDAL.ConsumeStock()` : avant decrementation FIFO
- `FrmBomFicheEdit.SynchroniserUniteInput()` : liste des unites compatibles

---

## `FormHelper` — Helpers UI

Classe statique `internal`.

| Methode | Signature | Description |
|---------|-----------|-------------|
| `ActiverPointDecimal` | `void ActiverPointDecimal(params NumericUpDown[] nuds)` | Convertit "." en separateur decimal local sur KeyPress |
| `ActiverSelectionAuFocus` | `void ActiverSelectionAuFocus(params NumericUpDown[] nuds)` | Select all au focus — saisie remplace directement |

### Justification ergonomique

- Pave numerique n'emet que "." — la locale francaise attend ","
- Selection au focus : premier caractere saisi remplace la valeur (pas besoin d'effacer)

---

## `StringExtensions` — Extensions string

Classe statique `internal`.

```csharp
public static string NullIfEmpty(this string s) =>
    string.IsNullOrWhiteSpace(s) ? null : s;
```

Utilisee dans tous les `FrmEdit*` pour convertir les TextBox vides en `NULL` cote DB (au lieu de stocker des chaines vides).

---

## Relations avec les autres modules

```
FrmEditBase (08)
  ^-- FrmActiviteEdit (07)
  ^-- FrmIngredientEdit (06)
  ^-- FrmFournisseurEdit (06)
  ^-- FrmBomContexteEdit (04)
  ^-- FrmBomNiveauEdit (03)
  ^-- FrmBomFicheEdit (03)
  ^-- FrmAchatEdit (05)
  ^-- FrmStockEdit (05)

FrmListeBase<T> (08)
  ^-- FrmFournisseurs (06)
  ^-- FrmIngredients (06)
  ^-- FrmAchats (05)
  ^-- FrmStocks (05)
  ^-- FrmBomContextes (04)

AppColors (08) --> utilise par TOUS les modules UI

UnitConvertisseur (08) --> utilise par modules 02, 03, 05
```

---

## Communautes graphify

C1, C15, C41, C54
