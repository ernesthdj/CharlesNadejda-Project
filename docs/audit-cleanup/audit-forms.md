# Audit Forms -- ArtisaStock
> Date: 2026-06-11 | Agent: Forms Auditor (#3)
> Perimetre: 39 fichiers dans `app-csharp/CharlesNadejda/CharlesNadejda/Forms/` + `Forms/Shell/`

---

## Resume

**22 findings** (3 critique, 10 important, 9 mineur)

| Severite   | Nombre |
|------------|--------|
| CRITIQUE   | 3      |
| IMPORTANT  | 10     |
| MINEUR     | 9      |

### Repartition par type
| Type              | Nombre |
|-------------------|--------|
| Heritage manque   | 3      |
| Redondance        | 5      |
| Dispose/Cleanup   | 2      |
| Incoherence       | 3      |
| Pattern manque    | 3      |
| Magic numbers     | 2      |
| Accessibilite     | 3      |
| Dead code         | 1      |

---

## Findings

### [F-FRM-001] FrmActivites n'herite pas de FrmListeBase
- **Severite:** CRITIQUE
- **Fichier(s):** `Forms/FrmActivites.cs:14`
- **Type:** Heritage manque
- **Description:** `FrmActivites` herite directement de `Form` au lieu de `FrmListeBase<Activite>`. Elle reimplemente manuellement tout le pattern CRUD liste (DGV, boutons, chargement, selection, confirmation suppression) que FrmListeBase fournit deja. C'est le seul formulaire liste metier qui n'utilise pas la classe de base.
- **Impact:** ~300 lignes de code duplique. Le style DGV (colonnes explicites ajoutees manuellement, Rows.Add au lieu de DataSource binding) diverge du pattern centralise. Toute evolution de FrmListeBase (ex: nouveau bouton, nouveau style) ne sera pas propagee ici.
- **Suggestion:** Migrer vers `FrmListeBase<Activite>`. Le seul point non-standard est le bouton "Desactiver/Reactiver" -- utiliser le hook `BtnYExtra` pour l'ajouter comme FrmBomContextes fait avec "Niveaux".

### [F-FRM-002] FrmStocks n'herite pas de FrmListeBase
- **Severite:** CRITIQUE
- **Fichier(s):** `Forms/FrmStocks.cs:22`
- **Type:** Heritage manque
- **Description:** `FrmStocks` herite de `Form` et reconstruit manuellement le layout DGV + boutons CRUD. Elle a en plus un SplitContainer avec un CheckedListBox de liaisons activites, ce qui justifiait historiquement la non-migration. Cependant le pattern DGV (colonnes explicites, Rows.Add, selection par cellule) et le CRUD (Nouveau/Modifier/Supprimer) sont identiques a FrmListeBase.
- **Impact:** Meme risque que F-FRM-001. De plus, le DGV utilise des colonnes explicites ajoutees manuellement (`_dgv.Columns.Add(new DataGridViewTextBoxColumn {...})`) au lieu du DataSource binding, ce qui diverge du pattern standard.
- **Suggestion:** Migrer le coeur CRUD vers FrmListeBase<Stock>, puis ajouter le SplitContainer liaison en surchargeant le layout dans OnLoad. Alternatif: garder standalone mais factoriser le helper `CreerBtn` dans FrmListeBase ou FormHelper.

### [F-FRM-003] FrmFournisseurs utilise Designer au lieu du heritage FrmListeBase
- **Severite:** CRITIQUE
- **Fichier(s):** `Forms/FrmFournisseurs.cs:8`, `Forms/FrmFournisseurs.Designer.cs`
- **Type:** Heritage manque + Incoherence
- **Description:** `FrmFournisseurs` est le seul formulaire liste qui utilise encore `partial class` + Designer.cs au lieu d'heriter de `FrmListeBase<Fournisseur>`. Elle declare ses propres `dgv`, `btnAjouter`, `btnModifier`, `btnSupprimer`, `btnFermer`, `lblTitre` dans le Designer -- exactement les memes champs que FrmListeBase fournit. Le `FormBorderStyle` est `FixedSingle` (non redimensionnable) alors que toutes les autres listes sont `Sizable`.
- **Impact:** Seul form liste avec un look different (pas de lignes alternees creme, pas de MinimumSize). Le DGV utilise `AutoSizeColumnsMode.Fill` alors que FrmListeBase utilise `AllCells`. Divergence visuelle pour l'utilisateur final.
- **Suggestion:** Migrer vers `FrmListeBase<Fournisseur>` comme les autres (FrmAchats, FrmIngredients, FrmBomContextes, etc.). Supprimer FrmFournisseurs.Designer.cs ensuite.

---

### [F-FRM-004] CreerBouton / CreerBtn duplique dans 3 fichiers
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmActivites.cs:136-151`, `Forms/FrmStocks.cs:199-211`, `Forms/FrmListeBase.cs:290-308`
- **Type:** Redondance
- **Description:** Trois methodes factory de boutons flat quasi-identiques:
  - `FrmListeBase.CreerBouton(text, x, y, anchor, backColor, foreColor)` -- private static
  - `FrmActivites.CreerBouton(text, bg, fg, x)` -- private
  - `FrmStocks.CreerBtn(text, bg, fg, x)` -- private
  Meme pattern (FlatStyle.Flat, Segoe UI, Cursor.Hand, BorderSize=0/1) avec des signatures legerement differentes.
- **Impact:** Maintenance triple. Si la charte graphique des boutons change, il faut modifier 3 endroits.
- **Suggestion:** Extraire un `FormHelper.CreerBoutonFlat(...)` unique avec une signature uniforme. Les 3 fichiers l'appellent. Alternativement, migrer FrmActivites et FrmStocks vers FrmListeBase (cf. F-FRM-001/002) elimine le probleme.

### [F-FRM-005] MakeRow duplique dans 3 formulaires Edit
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmFournisseurEdit.cs:33-44`, `Forms/FrmCategorieWebEdit.cs:93-107`, `Forms/FrmProduitWebEdit.cs:316-330`
- **Type:** Redondance
- **Description:** Trois methodes locales `MakeRow` qui creent un Label + TextBox. Chacune a une signature legerement differente:
  - FrmFournisseurEdit: `TextBox MakeRow(string label, int y, bool multi = false)` -- inline dans constructeur
  - FrmCategorieWebEdit: `TextBox MakeRow(string label, int y)` -- methode privee, layout different (label a x=16, txt a x=160)
  - FrmProduitWebEdit: `TextBox MakeRow(string label, ref int y)` -- ref y pour auto-increment
  Le pattern est identique: ajouter un label + textbox avec police Segoe UI 9-10pt.
- **Impact:** Maintenance triple, et les layouts different legerement entre les 3 (largeur, position du label).
- **Suggestion:** Extraire dans FrmEditBase ou FormHelper une methode `AddLabeledTextBox(Controls, label, x, y, width)` que tous les FrmEdit peuvent appeler. FrmIngredientEdit a deja son propre `AddField` qui est plus complet -- s'en inspirer.

### [F-FRM-006] Style DGV header duplique dans FrmActivites et FrmStocks
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmActivites.cs:87-92`, `Forms/FrmStocks.cs:99-104`
- **Type:** Redondance
- **Description:** La configuration du style des headers DGV (BackColor, ForeColor, Font, SelectionBackColor, SelectionForeColor) est copiee-collee ligne par ligne dans FrmActivites et FrmStocks. Ce meme bloc est deja centralise dans FrmListeBase (lignes 81-88), mais les deux forms standalone le dupliquent.
- **Impact:** Si la palette des headers DGV change dans AppColors, les 2 forms autonomes garderont l'ancien style.
- **Suggestion:** Apres migration vers FrmListeBase (F-FRM-001/002), le probleme disparait. Si standalone maintenu, extraire un `FormHelper.StylerDgv(dgv)`.

### [F-FRM-007] Bandeau header chocolat duplique dans 4 formulaires
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmActivites.cs:43-68`, `Forms/FrmStocks.cs:56-80`, `Forms/FrmActiviteStocks.cs:42-60`, `Forms/FrmVueStock.cs:64-80`
- **Type:** Redondance
- **Description:** Le pattern "Panel Dock.Top, Height=48, BackColor=ChocoBrand, avec Label titre en Or + Label hint en HintOnDark" est reconstruit manuellement dans 4 formulaires. Le code est quasi identique a chaque fois (seuls le titre et le hint changent).
- **Impact:** Maintenance x4. Risque de divergence stylistique.
- **Suggestion:** Extraire un helper `FormHelper.CreerBandeauHeader(string titre, string hint)` retournant un Panel pret a docker. Ou creer un UserControl `HeaderPanel`.

### [F-FRM-008] Controls.Clear() sans Dispose dans FrmBoutiqueWeb et FrmVueStock
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmPrincipal.BoutiqueWeb.cs:389`, `Forms/FrmVueStock.cs:274`, `Forms/FrmVueStock.cs:471`
- **Type:** Dispose/Cleanup
- **Description:** `_pnlDetailCommande.Controls.Clear()` (BoutiqueWeb:389) et `_flowChips.Controls.Clear()` (VueStock:274) appellent Clear() sans disposer les controles enfants au prealable. C'est exactement le pattern interdit par la regle JOURNAL #18. Par contraste, `FrmPrincipal.Production.cs` fait correctement le foreach+Dispose avant Clear (lignes 739-741, 786-788, 1189-1191, 1462-1464).
- **Impact:** Fuite memoire progressive. Les controles retires du parent ne sont pas garbage-collectes immediatement (ils restent references par leurs event handlers).
- **Suggestion:** Ajouter un foreach Dispose avant chaque Controls.Clear(), comme le fait deja FrmPrincipal.Production. Ou utiliser le helper `ClearAndDisposePanel` de FrmPrincipal si accessible.

### [F-FRM-009] Controls.Clear() sans Dispose dans FrmPrincipal.Production ligne 724
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmPrincipal.Production.cs:724`
- **Type:** Dispose/Cleanup
- **Description:** `_prodFlowNiveaux?.Controls.Clear()` a la ligne 724 (branche "pas de contexte selectionne") fait Clear() SANS le foreach+Dispose qui est pourtant present dans la methode `ProdRefreshNiveauxCards()` (lignes 739-741). C'est une incoherence dans le meme fichier.
- **Impact:** Si l'utilisateur change rapidement de contexte vers un contexte invalide, les anciens controles niveaux ne sont pas disposes.
- **Suggestion:** Remplacer la ligne 724 par un appel a un helper `DisposeAndClear(_prodFlowNiveaux)` ou dupliquer le pattern foreach+Dispose+Clear.

### [F-FRM-010] FrmEditBase ne definit pas AcceptButton/CancelButton
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmEditBase.cs`
- **Type:** Pattern manque
- **Description:** FrmEditBase cree `btnEnregistrer` et `btnAnnuler` mais ne les assigne pas comme `AcceptButton` et `CancelButton` du formulaire. L'utilisateur ne peut donc pas valider avec Enter ni annuler avec Escape dans aucun des ~10 formulaires Edit heritants. Seul FrmBomContexteEdit le fait sur son mini-dialogue interne (ligne 221-222).
- **Impact:** Manque d'ergonomie clavier critique. Un utilisateur expert perd du temps a cliquer au lieu d'utiliser Enter/Escape. Nielsen #7 (flexibilite et raccourcis).
- **Suggestion:** Ajouter dans le constructeur de FrmEditBase: `AcceptButton = btnEnregistrer; CancelButton = btnAnnuler;`

### [F-FRM-011] FrmListeBase ne gere aucun raccourci clavier
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmListeBase.cs`
- **Type:** Pattern manque
- **Description:** Aucun raccourci clavier n'est defini dans FrmListeBase: pas de Ctrl+N (Ajouter), Ctrl+E (Modifier), Delete (Supprimer), Escape (Fermer). Le StatusBar affiche pourtant "Ctrl+N Nouveau" (`Shell/StatusBarPanel.cs:67`), mais ce raccourci n'existe pas.
- **Impact:** Le hint clavier dans la barre d'etat ment a l'utilisateur. Manque d'accessibilite clavier.
- **Suggestion:** Surcharger `ProcessCmdKey` dans FrmListeBase pour capter Ctrl+N, Ctrl+E, Delete et Escape.

### [F-FRM-012] Couleurs inline non centralisees dans AppColors
- **Severite:** IMPORTANT
- **Fichier(s):** `Forms/FrmBomContexteEdit.cs:119,125,134,139,172,180`, `Forms/FrmBomFicheEdit.cs:178`, `Forms/FrmAchatEdit.cs:114,177,203,247,248,254,255`
- **Type:** Magic numbers
- **Description:** 162 occurrences de `Color.FromArgb(...)` dans les fichiers Forms, dont beaucoup sont des couleurs inline non definies dans AppColors. Exemples concrets:
  - `FrmBomContexteEdit.cs:119` -- `Color.FromArgb(220, 210, 200)` pour un separateur (devrait etre `AppColors.Line1` ou `AppColors.Border`)
  - `FrmBomContexteEdit.cs:125` -- `Color.FromArgb(140, 110, 80)` pour un label de section (devrait etre `AppColors.ChocoMed` ou nouveau token)
  - `FrmBomContexteEdit.cs:134` -- `Color.FromArgb(232, 244, 255)` bleu clair pour le panel N1 (pas dans la palette chocolat du tout)
  - `FrmBomContexteEdit.cs:172` -- `Color.FromArgb(92, 184, 92)` vert bootstrap pour le bouton + (devrait etre `AppColors.GreenOk`)
  - `FrmAchatEdit.cs:114,203,247,248,254,255` -- 6+ couleurs inline pour labels de prix
- **Impact:** La charte graphique n'est pas 100% centralisee. Changer le style global necessite une chasse aux Color.FromArgb inline.
- **Suggestion:** Ajouter les couleurs manquantes dans AppColors (ex: `AppColors.InfoBluePale`, `AppColors.HintText`) et remplacer les inline par les references centralisees.

### [F-FRM-013] CboActivite dans FrmBomContexteEdit n'utilise pas FormHelper.SelectionnerParId
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmBomContexteEdit.cs:88-98`
- **Type:** Incoherence
- **Description:** Le chargement du ComboBox activite dans FrmBomContexteEdit_Load utilise un foreach/if/break manuel pour la selection par Id (lignes 90-92, 96-98). Le helper `FormHelper.SelectionnerParId<T>` a ete cree exactement pour ca (TICKET-27) et est utilise ailleurs (FrmAchatEdit, FrmBomProductionSimulation).
- **Impact:** Incoherence de pattern. Si le helper evolue (ex: logging, fallback), cet endroit ne beneficiera pas.
- **Suggestion:** Remplacer les 2 boucles foreach par `FormHelper.SelectionnerParId<Activite>(cboActivite, a => a.Id, _activiteForce.Id)` et idem pour `_contexte.IdActivite`.

### [F-FRM-014] FrmProduitWebEdit.Sauvegarder() fait du I/O fichier sans try-catch adequat
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmProduitWebEdit.cs:276-313`
- **Type:** Pattern manque
- **Description:** La methode Sauvegarder fait `File.Copy` et `File.Move` pour copier l'image vers le storage Laravel, mais le bloc est protege uniquement par le try/catch generique de FrmEditBase.Confirmer(). Si la copie echoue (permissions, disque plein), le MessageBox affichera une erreur technique mais le produit aura deja ete insere en DB (Insert est appele AVANT le rename). De plus, si le `return` manquant apres le warning "Chemin Laravel introuvable" (ligne 281) laisse l'execution continuer.
- **Impact:** Risque de produit en DB sans image si le rename echoue. Le `return` manquant apres le warning permet a `File.Copy` d'etre appele meme si le chemin est invalide.
- **Suggestion:** 1) Ajouter `return;` ou `else {` apres le `MessageBox.Show` du check de chemin (ligne 282). 2) Faire la copie d'image dans un bloc separe avec rollback si echec. 3) Envelopper Insert + rename dans une seule operation atomique.

### [F-FRM-015] FrmCategorieWebEdit utilise Size au lieu de ClientSize
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmCategorieWebEdit.cs:26`
- **Type:** Incoherence
- **Description:** `FrmCategorieWebEdit` definit `Size = new Size(420, 300)` alors que tous les autres FrmEdit utilisent `ClientSize = new Size(...)`. La difference est que `Size` inclut les bordures de la fenetre et la barre de titre, ce qui rend la taille de la zone client non-deterministe selon les versions de Windows et les parametres DPI.
- **Impact:** Positionnement des boutons PositionnerBoutons(210) calcule par rapport a ClientSize mais la taille reelle du client depend de l'OS. Peut causer des boutons tronques sur certaines configurations.
- **Suggestion:** Remplacer `Size = new Size(420, 300)` par `ClientSize = new Size(406, 260)` (ajuster les valeurs pour obtenir le meme resultat visuel).

### [F-FRM-016] Pas de TabIndex dans la majorite des labels et controles secondaires
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmBomContexteEdit.cs`, `Forms/FrmBomFicheEdit.cs`, `Forms/FrmProduitWebEdit.cs`, `Forms/FrmCategorieWebEdit.cs`
- **Type:** Accessibilite
- **Description:** Seuls FrmActiviteEdit, FrmBomNiveauEdit, FrmStockEdit, FrmFournisseurEdit et FrmAchatEdit definissent systematiquement le TabIndex. Les formulaires complexes comme FrmBomContexteEdit, FrmProduitWebEdit et FrmCategorieWebEdit ne definissent le TabIndex que sur certains controles, laissant l'ordre de tabulation par defaut (ordre d'ajout aux Controls, qui ne correspond pas toujours a l'ordre visuel).
- **Impact:** L'utilisateur qui navigue au clavier (Tab) peut sauter des champs ou les parcourir dans un ordre illogique.
- **Suggestion:** Definir explicitement TabIndex sur tous les controles interactifs dans chaque formulaire, par ordre visuel de haut en bas, gauche a droite.

### [F-FRM-017] FrmIngredients.CreerChip utilise une largeur calculee approximative
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmIngredients.cs:139`
- **Type:** Magic numbers
- **Description:** `Width = texte.Length * 8 + 20` -- la largeur du chip est calculee avec un multiplicateur magique de 8px par caractere. Ce calcul ne tient pas compte de la police reelle (Segoe UI 8.5F) ni des caracteres larges/etroits. Le meme pattern est probablement present dans FrmVueStock.
- **Impact:** Les chips peuvent etre tronques avec des noms longs ou avoir trop d'espace avec des noms courts.
- **Suggestion:** Utiliser `TextRenderer.MeasureText(texte, font).Width + padding` pour un calcul precis.

### [F-FRM-018] FrmPrincipal.Designer.cs declare un MenuStrip visible=false
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmPrincipal.Designer.cs:44`
- **Type:** Dead code
- **Description:** Le MenuStrip est declare, configure avec des items (Fournisseurs, Catalogue web, Commandes, Session), puis cache avec `Visible = false`. Il n'est jamais rendu visible dans aucun partial. La navigation est entierement geree par la SidebarPanel + ScreenRouter.
- **Impact:** ~60 lignes de code mort dans le Designer. Les event handlers `menuFournisseurs_Click`, `menuCatCategories_Click`, etc. doivent exister dans FrmPrincipal.cs sous peine d'erreur de compilation, meme s'ils ne sont jamais appeles.
- **Suggestion:** Supprimer le MenuStrip et tous ses items du Designer. Supprimer les event handlers correspondants dans FrmPrincipal.cs.

### [F-FRM-019] FrmActivites et FrmStocks construisent des Fonts inline sans Dispose
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmActivites.cs:162-167`, `Forms/FrmStocks.cs:83-101`
- **Type:** Dispose/Cleanup
- **Description:** Dans la methode `Charger()` de FrmActivites, un `new Font("Segoe UI", 9.5F, FontStyle.Italic)` est cree a chaque rechargement de la grille (ligne 167) pour les lignes inactives. Ce Font n'est jamais dispose car il est assigne a `row.DefaultCellStyle.Font` qui n'est pas dispose non plus. Au total, 228 occurrences de `new Font(...)` dans les Forms, dont la plupart sont creees une seule fois dans les constructeurs (acceptable), mais certaines dans des methodes de rechargement.
- **Impact:** Fuite mineure de GDI handles lors de rechargements repetes. Chaque Font non dispose consomme un handle GDI (limite systeme de ~10000).
- **Suggestion:** Pour les Fonts utilises dans les boucles de Charger(), creer le Font une seule fois en champ static readonly (comme AppColors) et le reutiliser.

### [F-FRM-020] Absence de focus initial dans plusieurs formulaires
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmBomFicheEdit.cs`, `Forms/FrmBomContexteEdit.cs`, `Forms/FrmIngredientEdit.cs`, `Forms/FrmCategorieWebEdit.cs`, `Forms/FrmProduitWebEdit.cs`
- **Type:** Accessibilite
- **Description:** FrmActiviteEdit et FrmStockEdit definissent correctement `txtNom.Focus()` dans le handler Load. Mais FrmBomFicheEdit, FrmBomContexteEdit, FrmIngredientEdit, FrmCategorieWebEdit et FrmProduitWebEdit ne definissent pas de focus initial. Le premier controle dans l'ordre TabIndex recevra le focus par defaut, ce qui n'est pas toujours le champ souhaite.
- **Impact:** UX mineure -- l'utilisateur doit cliquer avant de taper.
- **Suggestion:** Ajouter `txtNom.Focus()` (ou le premier champ interactif) dans le handler Load de chaque formulaire Edit.

### [F-FRM-021] FrmBomNiveauEdit pre-remplit dans le constructeur au lieu du Load
- **Severite:** MINEUR
- **Fichier(s):** `Forms/FrmBomNiveauEdit.cs:57-61`
- **Type:** Incoherence
- **Description:** FrmBomNiveauEdit pre-remplit les champs (`txtNom.Text`, `txtDescription.Text`) directement dans le constructeur (lignes 57-61), alors que tous les autres FrmEdit le font dans le handler `Load`. Ce n'est pas un bug, mais c'est une incoherence de pattern. FrmActiviteEdit, FrmStockEdit, FrmAchatEdit, FrmIngredientEdit utilisent tous `Load += (s,e) => { ... }` pour le pre-remplissage.
- **Impact:** Incoherence de maintenance. Si un pattern "initialisation apres Load" est requis (ex: chargement async), ce form ne suivra pas.
- **Suggestion:** Deplacer le pre-remplissage dans un handler Load pour coherence.

### [F-FRM-022] SidebarPanel.CboActivite_DrawItem cree des Brushes sans using
- **Severite:** MINEUR
- **Fichier(s):** `Forms/Shell/SidebarPanel.cs:633-635, 644`
- **Type:** Dispose/Cleanup
- **Description:** Dans `CboActivite_DrawItem`, les lignes 633-635 et 644 creent des `new SolidBrush(...)` sans `using`:
  ```
  g.DrawString(nom, fNom, new SolidBrush(AppColors.SidebarTxt), ...);
  g.DrawString(subText, fSub, new SolidBrush(Color.FromArgb(130, 245, 230, 211)), ...);
  ```
  Chaque appel de DrawItem (survol de la liste deroulante) cree un Brush non-dispose.
- **Impact:** Fuite de GDI handles lors du parcours du dropdown activite. En pratique, le GC finira par les collecter, mais c'est un mauvais pattern.
- **Suggestion:** Envelopper dans `using` ou utiliser des Brushes statiques comme pour les Fonts.

---

## Synthese par fichier

| Fichier | Findings | Pire severite |
|---------|----------|---------------|
| FrmActivites.cs | F-001, F-004, F-006, F-007, F-019 | CRITIQUE |
| FrmStocks.cs | F-002, F-004, F-006, F-007, F-019 | CRITIQUE |
| FrmFournisseurs.cs + .Designer.cs | F-003 | CRITIQUE |
| FrmEditBase.cs | F-010 | IMPORTANT |
| FrmListeBase.cs | F-011 | IMPORTANT |
| FrmPrincipal.BoutiqueWeb.cs | F-008 | IMPORTANT |
| FrmPrincipal.Production.cs | F-009 | IMPORTANT |
| FrmBomContexteEdit.cs | F-012, F-013 | IMPORTANT |
| FrmAchatEdit.cs | F-012 | IMPORTANT |
| FrmProduitWebEdit.cs | F-014 | MINEUR |
| FrmCategorieWebEdit.cs | F-015, F-016 | MINEUR |
| FrmFournisseurEdit.cs | F-005 | IMPORTANT |
| FrmIngredients.cs | F-017 | MINEUR |
| FrmPrincipal.Designer.cs | F-018 | MINEUR |
| FrmBomNiveauEdit.cs | F-021 | MINEUR |
| Shell/SidebarPanel.cs | F-022 | MINEUR |
| FrmBomFicheEdit.cs | F-016, F-020 | MINEUR |
| FrmIngredientEdit.cs | F-016, F-020 | MINEUR |
| FrmVueStock.cs | F-007, F-008 | IMPORTANT |
| FrmActiviteStocks.cs | F-007 | IMPORTANT |

## Points positifs (ne pas casser)

1. **FrmEditBase** est bien concu: cycle Valider/Sauvegarder, errorProvider, PositionnerBoutons -- toutes les sous-classes l'utilisent correctement.
2. **FrmListeBase<T>** est un excellent Template Method pattern: 7 formulaires l'heritent proprement (FrmAchats, FrmIngredients, FrmBomContextes, FrmBomFiches, FrmBomNiveaux).
3. **ClearAndDisposePanel** dans FrmPrincipal.cs est correctement implemente et utilise dans 7 endroits (Hub, Production, Parametres, BoutiqueWeb, Ressource, Placeholder, Onboarding).
4. **FrmPrincipal.Production.cs** suit systematiquement le pattern foreach+Dispose+Clear pour les FlowLayoutPanels dynamiques (4 endroits).
5. **AppColors** est bien centralise et utilise partout comme source de verite pour la palette.
6. **FormHelper** grandit correctement avec des utilitaires reutilisables (ActiverPointDecimal, SelectionnerParId, ActiverSelectionAuFocus).
7. **Shell/ (SidebarPanel, TitleBarPanel, AppStatusBar)** sont bien encapsules, DoubleBuffered, et separent les responsabilites.

## Priorisation recommandee

| Sprint | Findings | Effort estime |
|--------|----------|---------------|
| P0 (critique) | F-010 (AcceptButton/CancelButton), F-008, F-009 (Dispose) | 1h |
| P1 (heritage) | F-001 (FrmActivites), F-002 (FrmStocks), F-003 (FrmFournisseurs) | 4-6h |
| P2 (refactoring) | F-004, F-005, F-006, F-007 (extraction helpers) | 3-4h |
| P3 (polish) | F-011, F-012, F-013, F-014, F-015-F-022 | 3-4h |
