# FournisseurDAL
> Source: `CharlesNadejda/DAL/FournisseurDAL.cs`
> Type: code

## Description
CRUD simple des fournisseurs. Champs: Nom, Contact, Telephone, Email, Adresse, Notes.

## Methodes
- `GetAll() -> List<Fournisseur>`
- `GetById(id) -> Fournisseur`
- `Insert(f) -> int`
- `Update(f)`
- `Delete(id)`
- `Bind(f,cmd)`
- `Map(reader) -> Fournisseur`

## Relations
Appele par: FrmFournisseurs, FrmFournisseurEdit, FrmIngredientEdit (ComboBox fournisseur)

### Connexions sortantes
- --method--> .GetAll()
- --method--> .NomExiste()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- FournisseurDAL.cs --contains-->
- [[MODULE_Catalogue_(Ingredients_&_Fournisseurs)|MODULE: Catalogue (Ingredients & Fournisseurs)]] --documents-->
