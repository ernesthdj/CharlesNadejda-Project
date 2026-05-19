# 12 — Diagrammes d'Activité — ArtisaStock ERP

> **Livrable BA/FA (Business Analyst / Functional Analyst)**
> Projet : ArtisaStock — ERP artisanal (Chocolaterie / Pâtisserie)
> Stack : C# WinForms · MySQL · Architecture SFA (Single-Form Architecture)
> Version : 1.0 — Mai 2026
> Auteur : Équipe projet Charles & Nadejda

---

## Table des matières

| # | Diagramme | Description |
|---|-----------|-------------|
| 1 | [Flux d'authentification](#1-flux-dauthentification) | Connexion, BCrypt, rôles |
| 2 | [Flux de navigation SFA](#2-flux-de-navigation-sfa) | ScreenRouter, AppState, panneau droit |
| 3 | [Flux de gestion des activités](#3-flux-de-gestion-des-activités) | CRUD activités, soft delete, sidebar |
| 4 | [Flux d'approvisionnement](#4-flux-dapprovisionnement-achatslots) | Achats, lots, FIFO, DLC |
| 5 | [Flux de gestion des ingrédients](#5-flux-de-gestion-des-ingrédients) | Création, seuils, jauge stock cible |
| 6 | [Flux de configuration BOM complet](#6-flux-de-configuration-bom-complet) | Contextes, niveaux, fiches, lignes |
| 7 | [Flux de simulation de production](#7-flux-de-simulation-de-production) | Explosion BOM, disponibilité, réservation |
| 8 | [Flux d'exécution de production](#8-flux-dexécution-de-production) | Transaction FIFO, lots consommés, stocks sortie |
| 9 | [Flux de gestion du catalogue web](#9-flux-de-gestion-du-catalogue-web) | CRUD produits, Cloudinary, configurateur |
| 10 | [Flux de traitement des commandes web](#10-flux-de-traitement-des-commandes-web) | Statuts, DGV, mise à jour partagée |
| 11 | [Flux de conversion d'unités](#11-flux-de-conversion-dunités-transversal) | UnitConvertisseur, densité, FormatQte |
| 12 | [Vue d'ensemble — Flux métier global](#12-vue-densemble--flux-métier-global) | Macro-vision bout en bout |

---

## Conventions graphiques

| Symbole Mermaid | Signification |
|-----------------|---------------|
| `([Nœud])` | Début / Fin (terminal) |
| `[Action]` | Processus / étape |
| `{Condition?}` | Décision (losange) |
| `[(Base de données)]` | Stockage persistant |
| `>Annotation]` | Commentaire / note |
| `subgraph Acteur` | Couloir de nage (swimlane) |
| Flèche `-- Oui -->` | Branche de décision étiquetée |

---

## 1. Flux d'authentification

> **Acteurs :** Utilisateur, FrmLogin, DAL (Data Access Layer — couche d'abstraction des accès base de données), MySQL
> **Déclencheur :** Lancement de l'application
> **Résultat attendu :** Session utilisateur ouverte sur FrmPrincipal (rôle admin validé)

```mermaid
flowchart TD
    A([Démarrage application]) --> B[Instancier FrmLogin]
    B --> C[Afficher formulaire de connexion]
    C --> D[Utilisateur saisit email + mot de passe]
    D --> E{Champs vides ?}

    E -- Oui --> F[Afficher erreur inline :\nVeuillez remplir tous les champs]
    F --> C

    E -- Non --> G[UtilisateurDAL.GetByLogin]

    subgraph DAL_Auth [Couche DAL — Accès données]
        G --> H[(SELECT utilisateur\nWHERE email = @email\nAND actif = 1)]
        H --> I{Utilisateur trouvé ?}
    end

    I -- Non --> J[Afficher lblErreur :\nIdentifiants incorrects]
    J --> C

    I -- Oui --> N[BCrypt.Verify\nmdp_saisi vs hash_stocké]
    N --> Q{Hash valide ?}

    Q -- Non --> J

    Q -- Oui --> R{Rôle = admin ?}
    R -- Non --> S[Afficher erreur : accès réservé\nadministrateurs]
    S --> C

    R -- Oui --> T[Créer AppState.UtilisateurConnecte]
    T --> U[Logger événement connexion]
    U --> V[Fermer FrmLogin]
    V --> W[Instancier FrmPrincipal]
    W --> X([Session ouverte — FrmPrincipal affiché])
```

**Points clés :**
- Le mot de passe n'est jamais stocké en clair — BCrypt (hachage adaptatif avec sel) est utilisé.
- La requête DAL est paramétrée (`@email`) — aucune concaténation SQL.
- Le message d'erreur est neutre ("Identifiants incorrects") — ne révèle pas si l'email ou le mot de passe est faux.
- Seul le rôle `admin` accède à l'ERP desktop ; les rôles `client` sont réservés au portail web.

---

## 2. Flux de navigation SFA

> **Acteurs :** Utilisateur, Sidebar, ScreenRouter, AppState, _pnlDroit
> **SFA (Single-Form Architecture) :** une seule FrmPrincipal — les écrans sont des UserControls chargés dynamiquement dans le panneau droit.
> **Déclencheur :** Clic sur un élément de la sidebar

```mermaid
flowchart TD
    A([Utilisateur connecté\nFrmPrincipal affiché]) --> B[Clic sur item sidebar]
    B --> C[Récupérer NavItemId de l'item cliqué]
    C --> D[Appeler ScreenRouter.Navigate NavItemId]

    subgraph ScreenRouter [ScreenRouter — Routeur d'écrans]
        D --> E{NavItemId ?}

        E -- Hub --> F[Charger HubScreen]
        E -- Ingredients --> G[Charger FrmIngredients]
        E -- AchatsLots --> H[Charger FrmAchats]
        E -- Production --> I[Charger FrmBomProductionSimulation]
        E -- VueStockGlobal --> J[Charger FrmVueStock]
        E -- StocksLiaisons --> K[Charger FrmStocks]
        E -- Fournisseurs --> L[Charger FrmFournisseurs]
        E -- NiveauxContextes --> M[Charger ContexteNiveauxScreen]
        E -- FichesBom --> N[Charger FrmBomFiches]
        E -- Inconnu --> O[Logger erreur de routage]
    end

    subgraph AppState [AppState — État global partagé]
        F & G & H & I & J & K & L & M & N --> P[AppState.ActiveScreen = ScreenId]
        P --> Q[AppState.ActiveActivite\nvérification contextuelle]
    end

    Q --> R{Activité requise\npour cet écran ?}
    R -- Oui, non définie --> S[Rediriger vers UcActivites\nMessage : sélectionnez une activité]
    S --> T([Écran activités affiché])

    R -- Non / Activité déjà définie --> U[Vider _pnlDroit\npnlDroit.Controls.Clear]
    U --> V[Ajouter UC dans _pnlDroit\npnlDroit.Controls.Add UC]
    V --> W[UC.Dock = Fill]
    W --> X[Appeler UC.ChargerDonnees]
    X --> Y[Mettre à jour item actif\ndans la sidebar couleur sélectionnée]
    Y --> Z([Écran affiché et données chargées])

    O --> AA([Erreur loggée — aucun changement d'écran])
```

**Points clés :**
- Un seul formulaire principal reste ouvert tout au long de la session (SFA).
- `AppState` centralise l'état : `ActiveActivite`, `ActiveContexte`, `ActiveNiveau`, `ActiveScreen`, `RessourceActive`.
- `_pnlDroit` est vidé (`ClearAndDisposePanel`) et rechargé à chaque navigation — pas de cache.
- Certains écrans (Ingrédients, Achats, Production, Fiches) nécessitent qu'une activité soit sélectionnée.

---

## 3. Flux de gestion des activités

> **Acteurs :** Utilisateur, UcActivites, FrmActiviteEdit, DAL, Sidebar
> **Déclencheur :** Navigation vers l'écran Activités
> **Résultat attendu :** Activité créée/modifiée/désactivée, sidebar rafraîchie

```mermaid
flowchart TD
    A([Écran UcActivites affiché]) --> B[Charger liste activités\nDAL.GetActivites actif=true]
    B --> C[Afficher DGV activités]
    C --> D{Action utilisateur ?}

    D -- Créer --> E[Ouvrir FrmActiviteEdit\nmode=Création]
    D -- Modifier --> F{Ligne sélectionnée ?}
    D -- Désactiver --> G{Ligne sélectionnée ?}
    D -- Sélectionner --> H[AppState.ActiviteSelectionnee = activité]

    F -- Non --> I[Message : sélectionnez une ligne]
    I --> C
    F -- Oui --> J[Ouvrir FrmActiviteEdit\nmode=Édition, données pré-remplies]

    G -- Non --> I
    G -- Oui --> K{Confirmer désactivation ?}
    K -- Non --> C
    K -- Oui --> L[Vérifier fiches liées actives]
    L --> M{Fiches actives\nexistantes ?}
    M -- Oui --> N[Avertir : X fiches liées\nDésactiver quand même ?]
    N -- Non --> C
    N -- Oui --> O[DAL.DesactiverActivite\nSET actif=0\nSoft delete]
    M -- Non --> O
    O --> P[Rafraîchir sidebar\nSidebar.RechargerItems]
    P --> Q[Recharger DGV]
    Q --> C

    subgraph FrmActiviteEdit [FrmActiviteEdit — Formulaire de saisie]
        E & J --> R[Afficher champs :\nNom, Description, Couleur]
        R --> S[Utilisateur saisit les données]
        S --> T{Valider formulaire}
        T -- Champs invalides --> U[Surligner champs en erreur\nMessage explicite]
        U --> S
        T -- Valide --> V{Mode ?}
        V -- Création --> W[DAL.InsererActivite\nINSERT INTO activites]
        V -- Édition --> X[DAL.ModifierActivite\nUPDATE activites SET ... WHERE id=@id]
    end

    W & X --> Y[Fermer FrmActiviteEdit]
    Y --> P

    H --> Z[Mettre à jour titre de l'activité\ndans la barre de navigation]
    Z --> AA[Filtrer sidebar\npar activité sélectionnée]
    AA --> C
```

**Points clés :**
- La désactivation est un **soft delete** — l'enregistrement reste en base avec `actif = 0`.
- La sidebar est rafraîchie dynamiquement après chaque opération.
- Une activité liée à des fiches actives déclenche un avertissement, pas un blocage.
- Les couleurs d'activité permettent une identification visuelle rapide dans la sidebar.

---

## 4. Flux d'approvisionnement (Achats/Lots)

> **Acteurs :** Utilisateur, UcLots, FrmAchatEdit, DAL, stock_ingredients
> **Déclencheur :** Navigation vers l'écran Lots / Achats
> **Résultat attendu :** Lot créé, stock mis à jour, FIFO respecté

```mermaid
flowchart TD
    A([Écran UcLots affiché]) --> B[Charger ingrédients actifs\nde l'activité courante]
    B --> C[Afficher liste ingrédients\navec stock actuel]
    C --> D[Utilisateur sélectionne un ingrédient]
    D --> E[Afficher historique des lots\nde cet ingrédient triés par date_achat ASC]
    E --> F{Action ?}

    F -- Nouveau lot --> G[Ouvrir FrmAchatEdit\nmode=Création]
    F -- Annuler lot --> H{Lot sélectionné ?}

    H -- Non --> I[Message : sélectionnez un lot]
    I --> F
    H -- Oui --> J{Lot partiellement\nconsommé ?}
    J -- Oui --> K[Bloquer annulation\nMessage : lot en cours d'utilisation]
    K --> F
    J -- Non --> L[Confirmer annulation]
    L --> M[DAL.SupprimerLot\nDELETE ou SET annule=1]
    M --> N[Recalculer stock ingrédient\nSUM quantite_restante WHERE actif=1]
    N --> O[Rafraîchir affichage]
    O --> C

    subgraph FrmAchatEdit [FrmAchatEdit — Saisie du lot d'achat]
        G --> P[Afficher champs :\nFournisseur, Quantité, Unité]
        P --> Q[Saisir prix unitaire HT\nOU prix total HT]
        Q --> R{Mode de saisie ?}
        R -- Prix unitaire --> S[Calculer total HT\ntotal = qte × prix_unitaire]
        R -- Prix total --> T[Calculer prix unitaire\nprix_u = total / qte]
        S & T --> U[Calculer TTC\nTTC = HT × 1 + TVA/100]
        U --> V[Afficher résumé : HT / TVA / TTC]
        V --> W[Saisir DLC date limite de consommation]
        W --> X{DLC valide ?\nDLC > aujourd hui}
        X -- Non --> Y[Avertir : DLC passée ou invalide]
        Y --> W
        X -- Oui --> Z[Saisir numéro de lot fournisseur\noptionnel]
        Z --> AA{Valider formulaire}
        AA -- Invalide --> AB[Afficher erreurs de saisie]
        AB --> P
        AA -- Valide --> AC[DAL.InsererLot\nINSERT INTO lots\nquantite_restante = quantite_initiale]
    end

    AC --> AD[Recalculer stock ingrédient\nSUM quantite_restante]
    AD --> AE{Stock > seuil_alerte ?}
    AE -- Non --> AF[Marquer ingrédient en alerte\ncouleur rouge dans DGV]
    AE -- Oui --> AG[Retirer marquage alerte]
    AF & AG --> AH[Rafraîchir jauge stock cible\nratio = stock_actuel / stock_cible]
    AH --> O
```

**Points clés :**
- Les lots sont ordonnés par `date_achat ASC` — c'est le fondement du FIFO (First In, First Out — premier entré, premier sorti).
- Le calcul HT/TTC est bidirectionnel : l'utilisateur peut saisir l'un ou l'autre.
- La DLC est validée à la saisie pour éviter l'intégration de lots périmés.
- `quantite_restante` diminue lors des consommations de production — jamais modifiée directement.

---

## 5. Flux de gestion des ingrédients

> **Acteurs :** Utilisateur, UcIngredients, FrmIngredientEdit, DAL, UcStockDetail
> **Déclencheur :** Navigation vers l'écran Ingrédients
> **Résultat attendu :** Ingrédient créé/modifié avec champs conditionnels, seuils configurés

```mermaid
flowchart TD
    A([Écran UcIngredients affiché]) --> B[Charger ingrédients\nde l'activité courante]
    B --> C[Afficher DGV avec :\nNom, Stock actuel, Seuil alerte, Jauge stock cible]
    C --> D{Action utilisateur ?}

    D -- Créer --> E[Ouvrir FrmIngredientEdit\nmode=Création]
    D -- Modifier --> F[Ouvrir FrmIngredientEdit\nmode=Édition]
    D -- Voir détail stock --> G[Ouvrir volet UcStockDetail\nen panneau droit ou drawer]
    D -- Désactiver --> H[Soft delete\nSET actif=0]

    subgraph FrmIngredientEdit [FrmIngredientEdit — Formulaire ingrédient]
        E & F --> I[Afficher champs communs :\nNom, Unité de base, Catégorie]
        I --> J{Type physique\nde l'unité ?}

        J -- Masse g/kg --> K[Champs masse uniquement\nPas de densité]
        J -- Volume ml/l --> L[Afficher champ Densité\ng/ml — requis pour conversion masse↔volume]
        J -- Pièce --> M[Masquer champs conversion\nPas de conversion possible]

        K & L & M --> N[Saisir seuil_alerte\nQuantité minimale souhaitée]
        N --> O[Saisir stock_cible\nQuantité idéale stock plein]
        O --> P{stock_cible >=\nseuil_alerte ?}
        P -- Non --> Q[Erreur : stock cible doit être\n≥ seuil alerte]
        Q --> O
        P -- Oui --> R[Saisir conditionnement\noptionnel : format emballage fournisseur]
        R --> S{Valider formulaire}
        S -- Invalide --> T[Surligner champs en erreur]
        T --> I
        S -- Valide --> U{Mode ?}
        U -- Création --> V[DAL.InsererIngredient\nINSERT INTO ingredients]
        U -- Édition --> W[DAL.ModifierIngredient\nUPDATE ingredients]
    end

    V & W --> X[Recalculer jauge stock cible]

    subgraph JaugeCalc [Calcul jauge stock cible — RG-STOCK-CIBLE]
        X --> Y[ratio = stock_actuel / stock_cible]
        Y --> Z{Valeur ratio ?}
        Z -- "ratio > 100%" --> ZA[Couleur BLEUE — surplus]
        Z -- "50% à 100%" --> AA[Couleur VERTE — stock OK]
        Z -- "20% à 50%" --> AB[Couleur ORANGE — stock faible]
        Z -- "ratio < 20%" --> AC[Couleur ROUGE — stock critique]
    end

    ZA & AA & AB & AC --> AD

    AD[Afficher jauge dans DGV\nProgressBar colorée]
    AD --> AE{Stock actuel <=\nseuil_alerte ?}
    AE -- Oui --> AF[Afficher icône alerte\ndans colonne DGV]
    AE -- Non --> AG[Aucune icône alerte]
    AF & AG --> AH[Rafraîchir DGV]
    AH --> C

    subgraph UcStockDetail [Volet détail stock]
        G --> AI[Charger lots actifs\nde l'ingrédient sélectionné]
        AI --> AJ[Afficher :\nLot n°, Qte restante, DLC, Fournisseur]
        AJ --> AK[Afficher traçabilité BOM\nFiches utilisant cet ingrédient]
        AK --> AL[Afficher historique\nconsommations production]
    end
```

**Points clés :**
- Le champ densité n'apparaît que pour les unités volumiques — nécessaire à la conversion masse ↔ volume.
- La jauge stock cible utilise 4 paliers de couleur : rouge (<20%), orange (20-50%), vert (50-100%), bleu (>100% surplus).
- Le `seuil_alerte` déclenche une icône visuelle dans la DGV (DataGridView — grille de données).
- Le volet détail offre la traçabilité complète : lots, fiches consommatrices, historique.

---

## 6. Flux de configuration BOM complet

> **BOM (Bill of Materials) :** nomenclature hiérarchique multi-niveaux décrivant la composition d'un produit fini.
> **Acteurs :** Utilisateur, UcActivites (contextes), UcNiveaux, UcFiches, FrmFicheEdit, DAL
> **Déclencheur :** Configuration d'une nouvelle activité de production
> **Résultat attendu :** Arbre BOM complet — contexte → niveaux → fiches → lignes

```mermaid
flowchart TD
    A([Activité sélectionnée]) --> B[Étape 1 : Gérer les contextes BOM]

    subgraph ContextesBOM [Contextes BOM — classification des fiches]
        B --> C[Charger contextes existants\npour l'activité]
        C --> D[Afficher liste contextes\nexemple : Recettes de base, Garnitures, Produits finis]
        D --> E{Créer nouveau contexte ?}
        E -- Oui --> F[Saisir nom + description]
        F --> G[DAL.InsererContexte\nINSERT INTO bom_contextes]
        G --> D
        E -- Non --> H[Sélectionner contexte existant]
    end

    H --> I[Étape 2 : Gérer les niveaux du contexte]

    subgraph NiveauxBOM [Niveaux BOM — hiérarchie N-1]
        I --> J[Charger niveaux du contexte\nordonnés par ordre_affichage]
        J --> K[Afficher arbre niveaux\nexemple : N1=Ingrédients bruts, N2=Préparations, N3=Assemblages]
        K --> L{Action niveau ?}
        L -- Créer niveau --> M[Saisir nom + niveau parent optionnel]
        M --> N{Niveau parent\ndéfini ?}
        N -- Oui --> O[Vérifier contrainte N-1\nniveau = parent.niveau + 1]
        O --> P{Contrainte N-1\nrespectée ?}
        P -- Non --> Q[Erreur : un niveau ne peut être\nenfant que du niveau immédiatement supérieur]
        Q --> M
        P -- Oui --> R[DAL.InsererNiveau\nINSERT INTO bom_niveaux]
        N -- Non --> R
        R --> J
        L -- Supprimer --> S{Niveau vide ?\naucune fiche liée}
        S -- Non --> T[Bloquer : déplacer ou supprimer\nles fiches d'abord]
        S -- Oui --> U[DAL.SupprimerNiveau]
        U --> J
    end

    R --> V[Étape 3 : Créer et configurer les fiches]

    subgraph FichesBOM [Fiches BOM — recettes / assemblages]
        V --> W[Sélectionner niveau cible]
        W --> X[Afficher fiches du niveau]
        X --> Y{Action fiche ?}
        Y -- Créer --> Z[Ouvrir FrmFicheEdit\nmode=Création]
        Y -- Modifier --> AA[Ouvrir FrmFicheEdit\nmode=Édition]
        Y -- Dupliquer --> AB[Copier fiche + lignes\navec suffixe _copie]
        AB --> X

        subgraph FrmFicheEdit [FrmFicheEdit — Éditeur de fiche]
            Z & AA --> AC[Saisir nom, description, rendement\nnb unités produites par batch]
            AC --> AD[Saisir unité de sortie\nverrouillée après première utilisation en sous-fiche]
            AD --> AE[Gérer les lignes de fiche]

            subgraph LignesFiche [Lignes de fiche — BOM lines]
                AE --> AF{Type de ligne ?}
                AF -- Ingrédient --> AG[Sélectionner ingrédient\ndepuis l'activité courante]
                AG --> AH[Saisir quantité + unité]
                AH --> AI{Unité compatible\navec ingrédient ?}
                AI -- Non --> AJ[Erreur : conversion impossible\nvérifier type physique]
                AJ --> AH
                AI -- Oui --> AK[DAL.InsererLigneFiche\nINSERT INTO bom_fiches_lignes type=ingredient]

                AF -- Sous-fiche --> AL[Sélectionner fiche de niveau inférieur\ncontrainte N-1]
                AL --> AM{Fiche sélectionnable ?\nniveau fiche < niveau courant}
                AM -- Non --> AN[Bloquer : référence circulaire\nou violation N-1]
                AM -- Oui --> AO[Saisir nombre de portions\ncombien de batches de la sous-fiche]
                AO --> AP[DAL.InsererLigneFiche\nINSERT INTO bom_fiches_lignes type=sous_fiche]
                AP --> AK
            end

            AK --> AQ[Calculer coût estimé fiche\nSOMMe coûts ingrédients + coûts sous-fiches]
            AQ --> AR[Afficher coût par unité de sortie]
        end
    end

    AR --> AS[Cascade coûts vers fiches parentes\nrecalcul automatique]
    AS --> AT([Configuration BOM complète])
```

**Points clés :**
- La contrainte N-1 garantit que la hiérarchie reste cohérente — pas de sauts de niveaux.
- L'unité de sortie d'une fiche est verrouillée dès qu'elle est référencée comme sous-fiche.
- Les coûts sont calculés en cascade : modifier une ligne impacte toutes les fiches parentes.
- La duplication de fiche permet de créer des variantes rapidement.

---

## 7. Flux de simulation de production

> **Acteurs :** Utilisateur, UcProduction, BomExploseur, UnitConvertisseur, DAL
> **Déclencheur :** Utilisateur souhaite produire un lot
> **Résultat attendu :** Rapport de disponibilité par ingrédient, bouton "Lancer" activé ou désactivé

```mermaid
flowchart TD
    A([Écran UcProduction affiché]) --> B[Charger fiches disponibles\nde l'activité courante]
    B --> C[Afficher liste fiches\navec coût unitaire estimé]
    C --> D[Utilisateur sélectionne une fiche]
    D --> E[Afficher détail fiche\nrecette et rendement]
    E --> F[Utilisateur saisit nombre de batches\nnb_batches > 0]
    F --> G{Valeur valide ?}
    G -- Non --> H[Message : nombre de batches invalide]
    H --> F
    G -- Oui --> I[Appeler BomExploseur.Exploser\nfiche, nb_batches]

    subgraph BomExploseur [BomExploseur — Explosion récursive BOM]
        I --> J[Pour chaque ligne de la fiche]
        J --> K{Type ligne ?}
        K -- Ingrédient --> L[Calculer quantite_requise\n= qte_ligne × nb_batches]
        K -- Sous-fiche --> M[Appel récursif\nBomExploseur.Exploser sous-fiche, portions × nb_batches]
        M --> L
        L --> N[Accumuler par ingrédient_id\nfusion des quantités si même ingrédient]
        N --> O{Autre ligne ?}
        O -- Oui --> J
        O -- Non --> P[Retourner liste consolidée\nDict ingrédient_id → qte_requise_unité_base]
    end

    P --> Q[Pour chaque ingrédient de la liste]

    subgraph DispoCheck [Vérification disponibilité — par ingrédient]
        Q --> R[UnitConvertisseur.Convertir\nqte_requise → unité base ingrédient]
        R --> S[DAL.GetLotsDisponibles\nWHERE ingredient_id = @id\nAND quantite_restante > 0\nORDER BY date_achat ASC FIFO]
        S --> T[Calculer stock_total_disponible\nSUM quantite_restante]
        T --> U{stock_disponible\n>= qte_requise ?}
        U -- Oui --> V[Marquer vert\nDisponible]
        U -- Non --> W[Marquer rouge\nManque = qte_requise - stock_disponible]
        V & W --> X{Autre ingrédient ?}
        X -- Oui --> Q
        X -- Non --> Y[Compilation résultats]
    end

    Y --> Z[Afficher tableau résultats\npar ingrédient : requis / disponible / statut]
    Z --> AA{Tous les ingrédients\nsont disponibles ?}
    AA -- Oui --> AB[Activer bouton LANCER\ncouleur verte]
    AA -- Non --> AC[Désactiver bouton LANCER\nAfficher liste manques]
    AB & AC --> AD{Utilisateur clique\nLANCER ?}
    AD -- Non, ajuste batches --> F
    AD -- Oui --> AE[Passer au flux d'exécution\ndiagramme 8]
    AD -- Annuler --> AF([Retour à la sélection fiche])
```

**Points clés :**
- L'explosion BOM est récursive — une fiche peut contenir des sous-fiches qui contiennent elles-mêmes des sous-fiches.
- Les quantités sont consolidées par `ingrédient_id` pour éviter de lire deux fois le même stock.
- Le FIFO est respecté dès la simulation : les lots sont triés par `date_achat ASC`.
- Le bouton "Lancer" est un CTA (Call To Action) unique, activé uniquement si tous les stocks sont suffisants.

---

## 8. Flux d'exécution de production

> **Acteurs :** Utilisateur, UcProduction, DAL, UnitConvertisseur, MySQL (transaction)
> **Déclencheur :** Clic sur "Lancer la production" (post-simulation validée)
> **Résultat attendu :** Lots consommés en FIFO, stock de sortie créé, coûts calculés et persistés

```mermaid
flowchart TD
    A([Simulation validée\nBouton LANCER cliqué]) --> B[Confirmer lancement\nMessageBox : Démarrer la production ?]
    B --> C{Confirmation ?}
    C -- Non --> D([Retour simulation])
    C -- Oui --> E[Ouvrir transaction MySQL\nBEGIN TRANSACTION]

    subgraph Transaction [Transaction MySQL — atomique]
        E --> F[Insérer enregistrement production\nINSERT INTO productions\nstatut=en_cours, date_debut=now]
        F --> G[Pour chaque ingrédient requis\npar ordre alphabétique stabilisé]

        subgraph ConsoFIFO [Consommation FIFO par ingrédient]
            G --> H[Charger lots disponibles\ntriés date_achat ASC]
            H --> I[qte_a_consommer = qte_requise convertie unité base]
            I --> J{qte_a_consommer > 0 ?}
            J -- Non --> K[Ingrédient soldé — continuer]
            J -- Oui --> L[Prendre lot suivant FIFO]
            L --> M{quantite_restante\n>= qte_a_consommer ?}
            M -- Oui --> N[Consommer totalement qte_a_consommer\nSET quantite_restante -= qte_a_consommer]
            N --> O[Insérer production_ligne\nINSERT INTO production_lignes\nlot_id, qte_consommee, cout_unitaire]
            O --> K
            M -- Non --> P[Consommer lot entier\nSET quantite_restante = 0]
            P --> Q[Insérer production_ligne pour ce lot]
            Q --> R[qte_a_consommer -= quantite_restante_lot]
            R --> S{Autre lot disponible ?}
            S -- Oui --> L
            S -- Non --> T[ERREUR : stock insuffisant\nen cours de transaction !]
            T --> U[ROLLBACK transaction]
            U --> V[Afficher erreur critique\nStock modifié entre simulation et exécution]
            V --> W([Retour simulation — relancer vérification])
        end

        K --> X{Autre ingrédient ?}
        X -- Oui --> G
        X -- Non --> Y[Calculer coût total production\nSOMMe coût_unitaire × qte_consommee\npour toutes les production_lignes]

        Y --> Z[Créer entrée bom_stocks\nINSERT INTO bom_stocks\ntype=sortie_production\nqte = nb_batches × rendement_fiche\nunite = unite_sortie_fiche\ncout_unitaire = cout_total / qte_produite]

        Z --> AA[Mettre à jour production\nSET statut=termine\nSET date_fin=now\nSET cout_total=@cout]
    end

    AA --> AB[COMMIT transaction]
    AB --> AC[Rafraîchir stock ingrédients\nSUM quantite_restante par ingrédient]
    AC --> AD[Afficher récapitulatif production\nQté produite, coût unitaire, lots consommés]
    AD --> AE{Imprimer / Exporter\nla fiche de production ?}
    AE -- Oui --> AF[Générer rapport PDF / CSV]
    AE -- Non --> AG([Production enregistrée\nRetour écran production])
    AF --> AG
```

**Points clés :**
- Toute l'exécution est dans une transaction MySQL — en cas d'erreur, le `ROLLBACK` annule tout.
- La consommation FIFO lot par lot peut fractioner la quantité sur plusieurs lots successifs.
- Le stock peut avoir changé entre la simulation et l'exécution (autre session) — le ROLLBACK protège contre ce cas.
- `bom_stocks` centralise à la fois les entrées (achats) et les sorties (productions) — vision unifiée du stock.

---

## 9. Flux de gestion du catalogue web

> **Acteurs :** Utilisateur, UcCatalogue, FrmProduitEdit, Cloudinary (CDN images), DAL, MySQL partagé
> **Déclencheur :** Navigation vers l'écran Catalogue
> **Résultat attendu :** Produit web créé/modifié, image hébergée, configurateur activé si besoin

```mermaid
flowchart TD
    A([Écran UcCatalogue affiché]) --> B[Charger produits du catalogue\nDAL.GetProduitsCatalogue]
    B --> C[Afficher DGV produits\nNom, Prix, Catégorie, Statut, Configurable]
    C --> D{Action utilisateur ?}

    D -- Créer produit --> E[Ouvrir FrmProduitEdit mode=Création]
    D -- Modifier --> F[Ouvrir FrmProduitEdit mode=Édition]
    D -- Dépublier --> G[SET publie=0\nProduit masqué sur le web]
    D -- Supprimer --> H{Commandes liées\nactives ?}
    H -- Oui --> I[Bloquer suppression\nMessage : commandes en cours]
    H -- Non --> J[DAL.SupprimerProduit\nSoft delete]
    J & G --> K[Rafraîchir DGV]
    K --> C

    subgraph FrmProduitEdit [FrmProduitEdit — Éditeur produit catalogue]
        E & F --> L[Saisir nom, description, prix]
        L --> M[Sélectionner catégorie\nexistante ou créer]
        M --> N{Catégorie inexistante ?}
        N -- Oui --> O[Mini-form : créer catégorie\nINSERT INTO categories]
        O --> M
        N -- Non --> P{Produit configurable ?\nex: gâteau sur mesure}
        P -- Oui --> Q[Activer section configurateur]

        subgraph Configurateur [Section configurateur — saveurs]
            Q --> R[Charger liste saveurs\ndisponibles pour cette catégorie]
            R --> S[Afficher checkboxes saveurs\nexemple : vanille, chocolat, fraise]
            S --> T{Ajouter nouvelle saveur ?}
            T -- Oui --> U[Saisir nom saveur\nINSERT INTO saveurs]
            U --> R
            T -- Non --> V[Sélectionner saveurs autorisées\npour ce produit]
            V --> W[DAL.LierSaveursProduit\nINSERT INTO produit_saveurs]
        end

        P -- Non --> X[Configurable = false\npas de saveurs]
        X & W --> Y[Gérer image produit]

        subgraph ImageUpload [Upload image — Cloudinary]
            Y --> Z[Bouton : Choisir une image]
            Z --> AA[Ouvrir dialogue fichier\nfiltres : jpg, png, webp]
            AA --> AB{Fichier sélectionné ?}
            AB -- Non --> AC[Conserver image existante]
            AB -- Oui --> AD{Taille <= 5 Mo ?}
            AD -- Non --> AE[Erreur : fichier trop lourd\nMax 5 Mo]
            AE --> Z
            AD -- Oui --> AF[Uploader vers Cloudinary API]
            AF --> AG{Upload réussi ?}
            AG -- Non --> AH[Erreur réseau\nReessayer ou annuler]
            AH --> Z
            AG -- Oui --> AI[Récupérer URL publique Cloudinary]
            AI --> AJ[Stocker url_image dans formulaire]
        end

        AC & AJ --> AK{Valider formulaire}
        AK -- Invalide --> AL[Afficher erreurs]
        AL --> L
        AK -- Valide --> AM{Mode ?}
        AM -- Création --> AN[DAL.InsererProduit\nINSERT INTO produits_catalogue]
        AM -- Édition --> AO[DAL.ModifierProduit\nUPDATE produits_catalogue]
    end

    AN & AO --> AP[Fermer FrmProduitEdit]
    AP --> K
```

**Points clés :**
- Cloudinary est un CDN (Content Delivery Network) — les images ne sont pas stockées sur le serveur local.
- Le configurateur (saveurs) n'est affiché que si `configurable = true` — progressive disclosure.
- La suppression est bloquée si des commandes actives référencent le produit.
- L'URL Cloudinary est la seule référence stockée en base — pas de fichiers binaires en DB.

---

## 10. Flux de traitement des commandes web

> **Acteurs :** Utilisateur ERP, UcCommandes, DAL, MySQL partagé (commandes du portail web Laravel)
> **Déclencheur :** Navigation vers l'écran Commandes
> **Résultat attendu :** Statut des commandes mis à jour, workflow respecté

```mermaid
flowchart TD
    A([Écran UcCommandes affiché]) --> B[DAL.GetCommandes\nSELECT depuis MySQL partagé\nORDER BY date_commande DESC]
    B --> C[Afficher DGV commandes\nN° commande, Client, Total, Statut, Date]
    C --> D{Filtrer par statut ?}
    D -- Oui --> E[Appliquer filtre statut\nrecharger DGV filtré]
    E --> C
    D -- Non --> F[Sélectionner une commande]
    F --> G[Afficher détail commande\nlignes, produits, quantités, client]
    G --> H{Action sur la commande ?}

    H -- Changer statut --> I[Vérifier transition autorisée]

    subgraph StatutWorkflow [Workflow de statuts — transitions validées]
        I --> J{Statut actuel ?}
        J -- en_attente --> K[Peut passer à :\nconfirmée OU annulée]
        J -- confirmée --> L[Peut passer à :\nen_preparation]
        J -- en_preparation --> M[Peut passer à :\nprête]
        J -- prête --> N[Peut passer à :\nlivrée]
        J -- livrée --> O[Statut terminal\naucune transition possible]
        J -- annulée --> O
    end

    K & L & M & N --> P{Transition valide\nsélectionnée ?}
    P -- Non --> Q[Message : transition non autorisée\ndepuis ce statut]
    Q --> H
    P -- Oui --> R[Confirmer changement de statut]
    R --> S{Confirmé ?}
    S -- Non --> H
    S -- Oui --> T[DAL.UpdateStatutCommande\nUPDATE commandes\nSET statut=@nouveau_statut\nWHERE id=@id]
    T --> U{Nouveau statut\n= confirmée ?}
    U -- Oui --> V[Envoyer notification email\nau client optionnel si SMTP configuré]
    V --> W[Rafraîchir commande dans DGV]
    U -- Non --> W

    W --> X{Nouveau statut\n= livrée ?}
    X -- Oui --> Y[Marquer commande archivée\npour statistiques]
    X -- Non --> Z[Continuer workflow]
    Y & Z --> C

    H -- Imprimer bon --> AA[Générer bon de préparation PDF\nliste des articles + client]
    AA --> C
    H -- Annuler --> C
```

**Points clés :**
- ArtisaStock (C# WinForms) et le portail web (Laravel) partagent la même base MySQL — lecture/écriture directe.
- Le workflow de statuts est linéaire et contraint — pas de retour en arrière possible (sauf `en_attente → annulée`).
- La notification email est optionnelle et conditionnée à la configuration SMTP du serveur.
- Les commandes livrées sont archivées pour les statistiques de vente.

---

## 11. Flux de conversion d'unités (transversal)

> **Acteurs :** UnitConvertisseur (service transversal), appelé par BomExploseur, UcProduction, UcIngredients
> **Déclencheur :** Toute opération nécessitant une comparaison ou conversion de quantités
> **Résultat attendu :** Quantité exprimée dans l'unité cible, ou erreur si conversion impossible

```mermaid
flowchart TD
    A([Appel UnitConvertisseur.Convertir\nqte, unite_source, unite_cible, densité optionnel]) --> B[Déterminer type_physique_source\nmasse / volume / pièce]
    B --> C[Déterminer type_physique_cible]
    C --> D{Même type physique ?}

    D -- Oui, masse↔masse --> E{Conversion directe}
    E -- g → kg --> F[qte / 1000]
    E -- kg → g --> G[qte × 1000]
    E -- mg → g --> H[qte / 1000]
    F & G & H --> I[Résultat en unité cible]

    D -- Oui, volume↔volume --> J{Conversion directe}
    J -- ml → l --> K[qte / 1000]
    J -- l → ml --> L[qte × 1000]
    J -- cl → l --> M[qte / 100]
    K & L & M --> I

    D -- Non, masse↔volume --> N{Densité disponible ?}
    N -- Non --> O[ERREUR : conversion impossible\nDensité requise non fournie]
    O --> P([Lancer exception\nConversionImpossibleException])

    N -- Oui --> Q{Direction conversion ?}
    Q -- masse → volume --> R[volume = masse / densité\nexemple : 500g / 1.03 g/ml = 485.4 ml]
    Q -- volume → masse --> S[masse = volume × densité\nexemple : 250ml × 0.91 g/ml = 227.5 g]
    R & S --> I

    D -- Pièce --> T{Source ET cible = pièce ?}
    T -- Oui --> U[Pas de conversion\nqte reste identique]
    U --> I
    T -- Non --> V[ERREUR : pièce incompatible\navec masse ou volume]
    V --> P

    I --> W[FormatQte — auto-scaling pour affichage]

    subgraph FormatQte [FormatQte — Formatage intelligent]
        W --> X{Valeur >= 1000 et\nunité = g ou ml ?}
        X -- Oui, g --> Y[Afficher en kg\nexemple : 1524g → 1.52 kg]
        X -- Oui, ml --> Z[Afficher en litres\nexemple : 652ml → 0.65 l]
        X -- Non --> AA[Afficher valeur brute\navec 2 décimales max]
        Y & Z & AA --> AB[Retourner string formatée\nexemple : 1.52 kg]
    end

    AB --> AC([Valeur convertie + formatée\nretournée à l'appelant])
```

**Points clés :**
- `UnitConvertisseur` est un service statique transversal — appelé par tous les modules nécessitant des conversions.
- La densité est stockée sur l'ingrédient — essentielle pour les conversions masse ↔ volume (ex : huile, lait).
- Les pièces (unités de comptage) ne peuvent pas être converties en masse ou volume — exception explicite.
- `FormatQte` assure un affichage lisible (6520 ml → 6.52 l) sans changer la valeur en base.

---

## 12. Vue d'ensemble — Flux métier global

> **Macro-vision :** comment tous les flux s'articulent, du premier démarrage à la livraison d'une commande web.
> **Lecture :** de gauche à droite, par phase métier.

```mermaid
flowchart LR
    subgraph ONBOARDING [Phase 1 — Onboarding]
        A([Démarrage]) --> B[Authentification\nBCrypt + Rôle admin]
        B --> C[FrmPrincipal\nShell SFA]
        C --> D[Créer activité\nChocolaterie / Pâtisserie]
        D --> E[Configurer sidebar\npar activité]
    end

    subgraph CONFIG [Phase 2 — Configuration]
        E --> F[Créer ingrédients\nUnités, Densités, Seuils]
        F --> G[Configurer BOM\nContextes → Niveaux → Fiches]
        G --> H[Définir lignes fiches\nIngrédients + Sous-fiches]
        H --> I[Calculer coûts estimés\ncascade BOM]
    end

    subgraph APPROVISIONNEMENT [Phase 3 — Approvisionnement]
        F --> J[Créer lots d'achat\nHT/TTC + DLC]
        J --> K[Stock ingrédients\nFIFO ordonné]
        K --> L{Seuil alerte ?}
        L -- Oui --> M[Alerte stock faible\nJauge rouge]
        L -- Non --> N[Jauge verte/orange]
        M & N --> O[Stock prêt]
    end

    subgraph PRODUCTION [Phase 4 — Production]
        O --> P[Sélectionner fiche\n+ nb batches]
        P --> Q[Simulation BOM\nExplosion récursive]
        Q --> R{Tous stocks OK ?}
        R -- Non --> S[Afficher manques\nAjuster batches]
        S --> P
        R -- Oui --> T[Exécuter production\nTransaction FIFO]
        T --> U[Consommer lots\nproduction_lignes]
        U --> V[Créer bom_stocks sortie\nProduit fini stocké]
        V --> W[Calculer coût réel\npar unité produite]
    end

    subgraph CATALOGUE [Phase 5 — Catalogue Web]
        V --> X[Créer produit catalogue\nNom, Prix, Image Cloudinary]
        X --> Y{Configurable ?}
        Y -- Oui --> Z[Lier saveurs\nconfigurator web]
        Y -- Non --> AA[Produit simple]
        Z & AA --> AB[Publier produit\nvisible sur portail Laravel]
    end

    subgraph COMMANDES [Phase 6 — Commandes Web]
        AB --> AC[Client commande\nvia portail web Laravel]
        AC --> AD[Commande visible dans ERP\nMySQL partagé]
        AD --> AE[Workflow statuts\nen attente → confirmée]
        AE --> AF[en préparation\nBon de préparation]
        AF --> AG[prête → livrée]
        AG --> AH([Commande archivée\nStatistiques])
    end

    subgraph TRANSVERSAL [Services transversaux]
        TC[UnitConvertisseur\nConversion + FormatQte]
        TR[ScreenRouter\nNavigation SFA]
        TA[AppState\nÉtat global session]
    end

    Q -.appelle.-> TC
    T -.appelle.-> TC
    C -.utilise.-> TR
    C -.utilise.-> TA
```

**Lecture du diagramme global :**

| Phase | Entrée | Sortie |
|-------|--------|--------|
| **Onboarding** | Démarrage application | Shell configuré par activité |
| **Configuration** | Activité créée | BOM complet + coûts estimés |
| **Approvisionnement** | Ingrédients définis | Lots en stock FIFO |
| **Production** | Stock disponible + BOM | Produit fini en bom_stocks |
| **Catalogue** | Produit fini | Fiche produit publiée web |
| **Commandes** | Commande client web | Livraison archivée |

---

## Annexe — Glossaire des termes techniques

| Terme | Définition |
|-------|------------|
| **SFA** (Single-Form Architecture) | Architecture WinForms où une seule fenêtre principale charge dynamiquement des UserControls dans un panneau droit |
| **BOM** (Bill of Materials) | Nomenclature — liste hiérarchique des composants nécessaires à la fabrication d'un produit |
| **DAL** (Data Access Layer) | Couche d'accès aux données — centralise toutes les requêtes SQL paramétrées |
| **FIFO** (First In, First Out) | Stratégie de consommation des lots : le plus ancien est consommé en premier |
| **DLC** | Date Limite de Consommation — expiration d'un lot d'ingrédient |
| **HT / TTC** | Hors Taxes / Toutes Taxes Comprises — modes de calcul du prix d'achat |
| **CDN** (Content Delivery Network) | Réseau de distribution de contenu — Cloudinary héberge les images produits |
| **Soft delete** | Suppression logique : l'enregistrement reste en base avec un flag `actif = 0` |
| **AppState** | Singleton C# centralisant l'état de navigation : utilisateur connecté, activité sélectionnée, écran courant |
| **BCrypt** | Algorithme de hachage adaptatif pour les mots de passe — résistant aux attaques par force brute |
| **CTA** (Call To Action) | Bouton d'action principal — unique par écran, clairement identifiable |
| **DGV** (DataGridView) | Composant WinForms de grille de données — affiche les listes paginées |
| **N-1** | Contrainte BOM : un niveau ne peut être enfant que du niveau immédiatement supérieur |
| **ROLLBACK** | Annulation d'une transaction MySQL — restaure l'état précédent si une erreur survient |
| **SMTP** | Simple Mail Transfer Protocol — protocole d'envoi d'emails pour les notifications commandes |

---

*Document généré dans le cadre du projet ArtisaStock — Double examen PDSGBD (C# WinForms) et PDWEB (Laravel).*
*Livrable BA/FA — Diagrammes d'activité conformes à la notation UML 2.x adaptée Mermaid.*
