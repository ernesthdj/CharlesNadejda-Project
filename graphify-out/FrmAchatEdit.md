# FrmAchatEdit
> Source: `CharlesNadejda/Forms/FrmAchatEdit.cs`
> Type: code

## Description
Editeur de lot d achat (herite FrmEditBase). Champs : ingredient (ComboBox), fournisseur, numero lot, quantite, prix HT/TTC (calcul auto TVA), reference facture, date achat, DLC optionnelle, notes. MajInfoConditionnement affiche unite/prix conditionne dynamiquement selon ingredient selectionne.

## Methodes
- `ChargerIngredients() ChargerFournisseurs() MajInfoConditionnement() Valider()->bool Sauvegarder() FrmAchatEdit_Load()`

## Regles (JOURNAL.md)
- #13 apres refactor signature avec params optionnels reordonnes, grepper tous call sites
- #28 conversion pieces<->unite de base

## Relations
Appelle: LotDAL.Insert/Update, IngredientDAL.GetAll, FournisseurDAL.GetAll. Herite: FrmEditBase.

### Connexions sortantes
- --inherits--> FrmEditBase
- --references--> Lot
- --references--> bool
- --references--> int
- --references--> Ingredient
- --references--> ComboBox
- --references--> Label
- --references--> TextBox
- --references--> NumericUpDown
- --references--> RadioButton
- --references--> DateTimePicker
- --references--> CheckBox
- --method--> .FrmAchatEdit_Load()
- --method--> .ChargerIngredients()
- --method--> .PreremplirEdition()
- ... +5 autres

### Connexions entrantes
- FrmAchatEdit.cs --contains-->
