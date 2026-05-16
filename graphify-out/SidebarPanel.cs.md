# SidebarPanel.cs
> Source: `CharlesNadejda/Forms/Shell/SidebarPanel.cs`
> Type: code

## Description
Sidebar 224px style Odoo. ActivitySwitcher (ComboBox OwnerDraw) en haut. 3 groupes navigation : Workflow (Hub/Production/Planning/Devis), Stock & Achats (Stocks/VueGlobal/Mouvements/Achats/Fournisseurs), Referentiels (Fiches/Ingredients/Niveaux/Parametres). Items avec hover/active + badges dores. Events: NavigationRequested, ActivityChanged, ManageActivitiesRequested.

## Methodes
- `SidebarPanel() SetActivities(List<Activite>) SetSelectedActivity(Activite) SetBadge(NavItemId,int) SetActiveItem(NavItemId)`

## Relations
Appele par: FrmPrincipal.BuildShell(). Emet: NavigationRequested->FrmPrincipal.OnSidebarNavigation, ActivityChanged->FrmPrincipal.OnActivityChanged.

### Connexions sortantes
- --contains--> CharlesNadejda.Forms.Shell
- --contains--> SidebarPanel
