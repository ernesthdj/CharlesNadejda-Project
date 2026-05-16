# FrmBomProductionSimulation
> Source: `CharlesNadejda/Forms/FrmBomProductionSimulation.cs`
> Type: code

## Description
Formulaire modal legacy (ShowDialog) pour simulation + lancement production. Cascade ComboBox : Contexte > Niveau > Fiche. Simule via BomProductionDAL.Simuler() puis colorie les lignes DGV (vert=OK, rouge=manque). Lancement async via btnLancerProduction_Click avec confirmation MessageBox.

## Methodes
- `ChargerContextes() cboContexte_SelectedIndexChanged() cboNiveau_SelectedIndexChanged() ReinitialiserResultats() btnSimuler_Click() ConfigurerGrille() ColoriserLignes() btnLancerProduction_Click()[async] SelectionValide()->bool`

## Relations
Appelle: BomProductionDAL.Simuler/Executer, BomContexteDAL.GetAll, BomNiveauDAL.GetByContexte, BomFicheDAL.GetByNiveau. Appele par: FrmPrincipal (ancien flux, remplace progressivement par Production inline).

### Connexions sortantes
- --references--> bool
- --references--> int
- --inherits--> Form
- --references--> BomContexte
- --references--> List
- --references--> BomNiveau
- --method--> .FrmBomProductionSimulation_Load()
- --method--> .ChargerContextes()
- --method--> .cboContexte_SelectedIndexChanged()
- --method--> .cboNiveau_SelectedIndexChanged()
- --method--> .cboFiche_SelectedIndexChanged()
- --method--> .RéinitialiserRésultats()
- --method--> .btnSimuler_Click()
- --method--> .ConfigurerGrille()
- --method--> .DgvSimulation_CellFormatting()
- ... +4 autres

### Connexions entrantes
- FrmBomProductionSimulation.cs --contains-->
