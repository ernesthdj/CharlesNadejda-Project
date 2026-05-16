# MODULE: Shell & Navigation
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/Forms/FrmPrincipal.cs`
> Type: rationale

## Description
Architecture SFA. FrmPrincipal = shell unique (TitleBar+Sidebar+StatusBar+ContentFill). Navigation via ScreenRouter/AppState. 13 NavItemId mappes vers ScreenId. Activites dynamiques switchables. MenuStrip invisible (escape hatch).

## Relations
### Connexions sortantes
- --documents--> [[FrmPrincipal|FrmPrincipal]]
- --documents--> [[AppState|AppState]]
