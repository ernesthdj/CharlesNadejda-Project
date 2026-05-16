# MODULE: Fiches & Niveaux BOM
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomFicheDAL.cs`
> Type: rationale

## Description
Fiche = recette avec lignes. Ligne.TypeInput = ingredient (matiere premiere) ou fiche (produit intermediaire). Niveaux = hierarchie dans un contexte (N1 matieres, N2 intermediaires, N3 finals). Un niveau N peut referencer tout niveau inferieur (pas seulement N-1). Insert/Update transactionnels.

## Relations
### Connexions sortantes
- --documents--> [[BomFicheDAL|BomFicheDAL]]
- --documents--> [[BomFicheLigneDAL|BomFicheLigneDAL]]
- --documents--> [[BomNiveauDAL|BomNiveauDAL]]
