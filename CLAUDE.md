# CLAUDE.md — CharlesNadejda Project (ArtisaStock)

> **Projet :** ArtisaStock — ERP patisserie artisanale
> **Type :** Full-Stack Desktop + Web (Hybrid)
> **Description :** Projet academique (Bachelier IT, 2e annee) — App C# WinForms + Site Laravel, MySQL partage, Docker.

---

## Contexte

Deux volets d'examen simultanes sur une meme base de donnees MySQL :
- **PDSGBD** — Application C# Windows Forms (gestion production, stocks, BOM)
- **PDWEB** — Site e-commerce Laravel (boutique en ligne, commandes)

## Stack

| Couche | Technologie |
|--------|-------------|
| Desktop | C# .NET Framework 4.8.1 · Windows Forms |
| Web | Laravel 11 · PHP 8.3-FPM · Blade · Tailwind |
| Database | MySQL 8.0 (partagee) |
| Infra | Docker Compose · Nginx · phpMyAdmin |
| Auth | BCrypt (compatible PHP <-> C#) |
| Paiement | Stripe / Bancontact (test mode) |
| Images | Cloudinary |
| Build | Vite (Laravel frontend) |

## Structure

```
CharlesNadejda_Project/
├── app-csharp/          # Application C# WinForms
│   └── CharlesNadejda/
│       └── CharlesNadejda/
│           ├── DAL/     # Data Access Layer (13 fichiers)
│           ├── Forms/   # ~40 formulaires WinForms
│           ├── Models/  # 15 modeles C#
│           └── Navigation/
├── site-laravel/        # Site e-commerce Laravel
│   ├── app/             # Models, Controllers, Middleware
│   ├── resources/views/ # Templates Blade
│   └── routes/          # web.php (21 routes)
├── docker/              # Nginx + PHP-FPM configs
├── sql/                 # 16 migrations SQL versionnees
├── docs/                # 28 fichiers documentation
│   └── JOURNAL.md       # 32 regles, 21 sessions
└── docker-compose.yml
```

## Regles specifiques

### C# WinForms
- `.csproj` necessite des entrees `<Compile>` manuelles (CS0246)
- Ajout de champ Model = 4 MAJ dans le DAL (SELECT, INSERT, UPDATE, Map)
- `Controls.Clear()` ne dispose pas les enfants — utiliser un helper
- DataGridView : `CellFormatting` pour le formatage, pas de manipulation du model
- Utiliser `AllCellsExceptHeader` ou widths fixes pour les performances DGV

### Laravel
- Eager loading obligatoire (pas de N+1)
- Form Requests pour la validation
- Auth via middleware `ClientAuth`

### Base de donnees
- 16 tables normalisees, migrations SQL versionnees dans `/sql/`
- Stock FIFO avec suivi par lots

## Workflows actifs

- [x] Brainstorm initial
- [x] Pipeline agents (multi-agent flowscope)
- [ ] Refactoring sprints (P0-P3 en cours)
- [ ] Defense examen
