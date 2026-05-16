# MODULE: Activites
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/DAL/ActiviteDAL.cs`
> Type: rationale

## Description
Activite = domaine metier dynamique (Chocolaterie, Patisserie...). JAMAIS hardcode. Chaque activite a des stocks lies (M:N). Desactiver verifie les cascades. ActivitySwitcher dans la sidebar. AppState.SetActivite() propage le changement a tout le shell.

## Relations
### Connexions sortantes
- --documents--> [[ActiviteDAL|ActiviteDAL]]
