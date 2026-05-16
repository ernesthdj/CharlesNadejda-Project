# FrmBomFicheEdit
> Source: `CharlesNadejda/Forms/FrmBomFicheEdit.cs`
> Type: code

## Description
Editeur de fiche BOM (recette). Header: nom, description, unite output, quantite output, temps prep. Corps: ajout/retrait de lignes (ingredient ou sous-fiche). ComboBox input dynamique selon type (ingredients ou fiches du niveau inferieur). Unite verrouillee apres selection input. DGV lignes en bas.

## Methodes
- `FrmBomFicheEdit_Load() AfficherInfoInput() ChargerInputsDisponibles() SynchroniserUniteInput() BtnAjouterLigne_Click() BtnRetirerLigne_Click() RafraichirGrilleLignes() Valider()->bool Sauvegarder()`

## Regles (JOURNAL.md)
- #7 unite de la ligne = unite input source, ComboBox verrouille apres selection
- #4 fiche liee a un niveau specifique

## Relations
Appelle: BomFicheDAL.Insert/Update, BomFicheLigneDAL.GetByFiche, IngredientDAL.GetAll, BomFicheDAL.GetByNiveau. Herite: FrmEditBase.

### Connexions sortantes
- --inherits--> FrmEditBase
- --references--> bool
- --references--> ComboBox
- --references--> Label
- --references--> TextBox
- --references--> NumericUpDown
- --references--> DataGridView
- --references--> List
- --references--> BomFiche
- --references--> BomNiveau
- --method--> .FrmBomFicheEdit_Load()
- --method--> .AfficherInfoInput()
- --method--> .ChargerInputsDisponibles()
- --method--> .SynchroniserUniteInput()
- --method--> .BtnAjouterLigne_Click()
- ... +4 autres

### Connexions entrantes
- FrmBomFicheEdit.cs --contains-->
