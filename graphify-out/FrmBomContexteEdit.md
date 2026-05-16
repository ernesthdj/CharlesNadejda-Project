# FrmBomContexteEdit
> Source: `CharlesNadejda/Forms/FrmBomContexteEdit.cs`
> Type: code

## Description
Editeur de contexte BOM (herite FrmEditBase). Header: nom, description, activite (ComboBox ou forcee). Section niveaux dynamique : N1 obligatoire (TextBox), niveaux supplementaires (ListBox + boutons Ajouter/Supprimer dernier). InsertAvecNiveaux en une seule TX.

## Methodes
- `FrmBomContexteEdit_Load() AjouterSectionNiveaux() BtnAjouterNiveau_Click() BtnSupprimerDernierNiveau_Click() Valider()->bool Sauvegarder()`

## Regles (JOURNAL.md)
- #8 migration ENUM vers FK dans meme ALTER TABLE

## Relations
Appelle: BomContexteDAL.InsertAvecNiveaux/Update, ActiviteDAL.GetAll. Herite: FrmEditBase.

### Connexions sortantes
- --references--> Color
- --inherits--> FrmEditBase
- --references--> bool
- --references--> ComboBox
- --references--> TextBox
- --references--> Activite
- --references--> Button
- --references--> BomContexte
- --references--> List
- --references--> ListBox
- --method--> .FrmBomContexteEdit_Load()
- --method--> .AjouterSectionNiveaux()
- --method--> .BtnAjouterNiveau_Click()
- --method--> .BtnSupprimerDernierNiveau_Click()
- --method--> .Valider()
- ... +1 autres

### Connexions entrantes
- FrmBomContexteEdit.cs --contains-->
