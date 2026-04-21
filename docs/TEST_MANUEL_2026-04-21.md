# Plan de test manuel — ArtisaStock
> Date : 2026-04-21
> Objectif : valider le flux complet création activité → stock → ingrédients → achats → contexte BOM → niveaux → fiches → production
> Deux activités différentes : **Chocolaterie** et **Savonnerie**

---

## 0. Prérequis — Reset de la base

```bash
mysql -u root -p charlesnadejda < sql/reset_db_for_tests.sql
```

Lancer l'application → se connecter avec votre compte.

---

## SCÉNARIO A — Chocolaterie Charles

> Recette : Truffes au chocolat noir
> Flux : Ingrédients → Ganache (N2) → Truffes finies (N3)

---

### A1. Créer l'activité

**Menu :** ⚙ (roue dentée, bandeau gauche) → "Gérer les activités" → + Nouvelle activité

| Champ | Valeur |
|-------|--------|
| Nom | `Chocolaterie Charles` |
| Description | `Production de chocolats artisanaux et truffes` |

→ Enregistrer. Sélectionner **Chocolaterie Charles** dans la sidebar.

---

### A2. Créer le stock physique

**Menu :** ⚙ à droite du bandeau "Stocks" → + Nouveau

| Champ | Valeur |
|-------|--------|
| Nom | `Cave sèche chocolaterie` |
| Description | `Stockage matières premières — temp. 15°C` |

---

### A3. Créer les fournisseurs

**Menu :** Catalogue → Fournisseurs → + Ajouter (×2)

**Fournisseur 1**

| Champ | Valeur |
|-------|--------|
| Nom | `Valrhona Belgium` |
| Contact | `Service Pro` |
| Email | `pro@valrhona.be` |
| Téléphone | `+32 2 555 0100` |
| Notes | `Couvertures et beurre de cacao haut de gamme` |

**Fournisseur 2**

| Champ     | Valeur                        |
| --------- | ----------------------------- |
| Nom       | `Metro Bruxelles`             |
| Contact   | `Caisse professionnelle`      |
| Téléphone | `+32 2 555 0200`              |
| Notes     | `Produits laitiers et sucres` |

---

### A4. Créer les fiches ingrédients

**Menu :** Stock/Liaisons → Fiches ingrédients → + Ajouter (×4)

**Ingrédient 1 — Chocolat noir 70%**

| Champ                          | Valeur                      |
| ------------------------------ | --------------------------- |
| Nom                            | `Chocolat noir 70% Caraïbe` |
| Marque                         | `Valrhona`                  |
| Unité de mesure                | `g`                         |
| Type physique                  | `solide`                    |
| Densité                        | *(laisser vide — solide)*   |
| Conditionnement                | `Tablette 1 kg`             |
| Qté par conditionnement        | `1000`                      |
| Prix d'achat / conditionnement | `18.50`                     |
| Seuil d'alerte stock           | `500`                       |
| Fournisseur par défaut         | `Valrhona Belgium`          |
| Stock                          | `Cave sèche chocolaterie`   |

**Ingrédient 2 — Crème liquide 35%**

| Champ                          | Valeur                    |
| ------------------------------ | ------------------------- |
| Nom                            | `Crème liquide UHT 35%`   |
| Unité de mesure                | `ml`                      |
| Type physique                  | `liquide`                 |
| Densité                        | `1.01`                    |
| Conditionnement                | `Brique 1 L`              |
| Qté par conditionnement        | `1000`                    |
| Prix d'achat / conditionnement | `3.80`                    |
| Seuil d'alerte stock           | `200`                     |
| Fournisseur par défaut         | `Metro Bruxelles`         |
| Stock                          | `Cave sèche chocolaterie` |

**Ingrédient 3 — Beurre de cacao**

| Champ                          | Valeur                    |
| ------------------------------ | ------------------------- |
| Nom                            | `Beurre de cacao pur`     |
| Marque                         | `Valrhona`                |
| Unité de mesure                | `g`                       |
| Type physique                  | `solide`                  |
| Conditionnement                | `Bloc 1 kg`               |
| Qté par conditionnement        | `1000`                    |
| Prix d'achat / conditionnement | `24.00`                   |
| Seuil d'alerte stock           | `200`                     |
| Fournisseur par défaut         | `Valrhona Belgium`        |
| Stock                          | `Cave sèche chocolaterie` |

**Ingrédient 4 — Cacao en poudre**

| Champ                          | Valeur                      |
| ------------------------------ | --------------------------- |
| Nom                            | `Cacao en poudre non sucré` |
| Unité de mesure                | `g`                         |
| Type physique                  | `poudre`                    |
| Densité                        | `0.60`                      |
| Conditionnement                | `Sac 500 g`                 |
| Qté par conditionnement        | `500`                       |
| Prix d'achat / conditionnement | `5.20`                      |
| Seuil d'alerte stock           | `100`                       |
| Fournisseur par défaut         | `Metro Bruxelles`           |
| Stock                          | `Cave sèche chocolaterie`   |

---

### A5. Enregistrer les achats (lots de stock)

**Menu :** Achats / Lots → + Ajouter (×3 lots)

**Lot 1 — Chocolat noir**

| Champ                    | Valeur                      |
| ------------------------ | --------------------------- |
| Ingrédient               | `Chocolat noir 70% Caraïbe` |
| Fournisseur              | `Valrhona Belgium`          |
| N° de lot                | `VLR-CHN-2026-04`           |
| Date d'achat             | `2026-04-10`                |
| DLC                      | `2027-06-30`                |
| Nb de conditionnements   | `3`                         |
| Prix unitaire HT / cond. | `18.50`                     |
| TVA %                    | `6`                         |
| Réf. facture             | `FA-V-2204-01`              |

> Résultat attendu : **3 000 g** disponibles en stock.

**Lot 2 — Crème liquide**

| Champ                    | Valeur                  |
| ------------------------ | ----------------------- |
| Ingrédient               | `Crème liquide UHT 35%` |
| Fournisseur              | `Metro Bruxelles`       |
| N° de lot                | `MTR-CR-2026-04`        |
| Date d'achat             | `2026-04-15`            |
| DLC                      | `2026-05-20`            |
| Nb de conditionnements   | `2`                     |
| Prix unitaire HT / cond. | `3.80`                  |
| TVA %                    | `6`                     |

> Résultat attendu : **2 000 ml** disponibles.

**Lot 3 — Cacao en poudre**

| Champ                    | Valeur                      |
| ------------------------ | --------------------------- |
| Ingrédient               | `Cacao en poudre non sucré` |
| Fournisseur              | `Metro Bruxelles`           |
| N° de lot                | `MTR-CAP-2026-04`           |
| Date d'achat             | `2026-04-15`                |
| Nb de conditionnements   | `2`                         |
| Prix unitaire HT / cond. | `5.20`                      |
| TVA %                    | `6`                         |

> Résultat attendu : **1 000 g** disponibles.

---

### A6. Créer le contexte BOM + niveaux

**Depuis la sidebar :** Chocolaterie Charles → Vue stock de production → + Nouveau contexte

| Champ              | Valeur                                    |
| ------------------ | ----------------------------------------- |
| Nom du contexte    | `Truffes Chocolat Noir`                   |
| Description        | `Production de truffes enrobées de cacao` |
| Nom niveau 1       | `Ingrédients` *(pré-rempli)*              |
| + Ajouter niveau 2 | `Ganache`                                 |
| + Ajouter niveau 3 | `Truffes finies`                          |

→ Enregistrer → 3 niveaux créés automatiquement.

---

### A7. Créer les fiches BOM

#### Fiche N2 — Ganache Chocolat Noir

**Cliquer sur le niveau "Ganache"** → + Nouvelle fiche

| Champ                   | Valeur                                        |
| ----------------------- | --------------------------------------------- |
| Nom                     | `Ganache chocolat noir 60/40`                 |
| Description             | `Ganache classique : 60% chocolat, 40% crème` |
| Unité output            | `g`                                           |
| Quantité output / batch | `1000`                                        |
| Temps préparation       | `30` min                                      |

**Lignes d'input :**

| Input | Quantité | Unité |
|-------|----------|-------|
| `Chocolat noir 70% Caraïbe` | `600` | `g` |
| `Crème liquide UHT 35%` | `400` | `ml` |

→ Enregistrer.

#### Fiche N3 — Truffes finies

**Cliquer sur le niveau "Truffes finies"** → + Nouvelle fiche

| Champ                   | Valeur                                      |
| ----------------------- | ------------------------------------------- |
| Nom                     | `Truffes enrobées cacao — plaque 20`        |
| Description             | `20 truffes façonnées et enrobées de cacao` |
| Unité output            | `piece`                                     |
| Quantité output / batch | `20`                                        |
| Temps préparation       | `45` min                                    |

**Lignes d'input :**

| Input (fiche N2) | Quantité | Unité |
|------------------|----------|-------|
| `Ganache chocolat noir 60/40` | `500` | `g` |
| `Cacao en poudre non sucré` | `100` | `g` |

→ Enregistrer.

---

### A8. Tester la production

**Menu :** Production → Sélectionner :

| Champ | Valeur |
|-------|--------|
| Contexte | `Truffes Chocolat Noir` |
| Niveau | `Ganache` *(N2)* |
| Fiche | `Ganache chocolat noir 60/40` |
| Nombre de batches | `2` |

→ **Simuler** → vérifier :

| Input | Nécessaire | Disponible | Statut |
|-------|-----------|-----------|--------|
| Chocolat noir 70% | 1 200 g | 3 000 g | ✔ vert |
| Crème liquide UHT | 800 ml | 2 000 ml | ✔ vert |

**Coûts estimés attendus (simulation) :**

| Ligne de coût | Calcul | **Valeur attendue** |
|---|---|---|
| Chocolat noir | 1 200 g × 0,0185 €/g | **22,20 €** |
| Crème liquide | 800 ml × 0,0038 €/ml | **3,04 €** |
| **Coût total ingrédients** | | **25,24 €** |
| Qté produite | 2 batches × 1 000 g | **2 000 g** |
| **Coût unitaire ganache** | 25,24 € ÷ 2 000 g | **0,01262 €/g** |

→ **Lancer la production** → confirmer
→ Vérifier que **2 000 g de Ganache** apparaissent dans le stock du niveau Ganache.

---

**Ensuite, produire les truffes (N3) :**

| Champ | Valeur |
|-------|--------|
| Niveau | `Truffes finies` *(N3)* |
| Fiche | `Truffes enrobées cacao — plaque 20` |
| Batches | `1` |

→ Simuler → vérifier :

| Input | Nécessaire | Disponible | Statut |
|-------|-----------|-----------|--------|
| Ganache chocolat 60/40 *(N2)* | 500 g | 2 000 g | ✔ vert |
| Cacao en poudre | 100 g | 1 000 g | ✔ vert |

**Coûts estimés attendus (simulation) :**

| Ligne de coût | Calcul | **Valeur attendue** |
|---|---|---|
| Ganache N2 | 500 g × 0,01262 €/g | **6,31 €** |
| Cacao en poudre | 100 g × 0,0104 €/g | **1,04 €** |
| **Coût total ingrédients** | | **7,35 €** |
| Qté produite | 1 batch × 20 pcs | **20 pièces** |
| **Coût unitaire truffe** | 7,35 € ÷ 20 pcs | **0,3675 €/pièce** |

→ Lancer → **20 pièces de truffes** créées en stock N3.

---

## SCÉNARIO B — Savonnerie Nadejda

> Recette : Savon froid à la lavande
> Flux : Huiles + soude → Masse saponifiée (N2) → Savons découpés (N3)

---

### B1. Créer l'activité

**Menu :** ⚙ → Gérer les activités → + Nouvelle activité

| Champ       | Valeur                                   |
| ----------- | ---------------------------------------- |
| Nom         | `Savonnerie Nadejda`                     |
| Description | `Fabrication de savons naturels à froid` |

→ Sélectionner **Savonnerie Nadejda** dans la sidebar.

---

### B2. Créer le stock physique

| Nom         | `Atelier cosmétique`                                |
| ----------- | --------------------------------------------------- |
| Description | `Stockage huiles et matières premières cosmétiques` |
|             |                                                     |

---

### B3. Créer les fournisseurs

**Fournisseur 3**

| Champ | Valeur                                               |
| ----- | ---------------------------------------------------- |
| Nom   | `Aromazone Pro`                                      |
| Email | `pro@aromazone.com`                                  |
| Notes | `Huiles végétales et huiles essentielles certifiées` |

**Fournisseur 4**

| Champ     | Valeur                                               |
| --------- | ---------------------------------------------------- |
| Nom       | `Destillerie Bio SPRL`                               |
| Téléphone | `+32 4 555 0300`                                     |
| Notes     | `Soude caustique et eau distillée — livraison hebdo` |

---

### B4. Créer les fiches ingrédients (×5)

**Ingrédient 5 — Huile d'olive vierge**

| Champ           | Valeur                           |
| --------------- | -------------------------------- |
| Nom             | `Huile d'olive extra vierge BIO` |
| Unité de mesure | `ml`                             |
| Type physique   | `liquide`                        |
| Densité         | `0.91`                           |
| Conditionnement | `Bidon 1 L`                      |
| Qté / cond.     | `1000`                           |
| Prix / cond.    | `8.50`                           |
| Seuil alerte    | `500`                            |
| Fournisseur     | `Aromazone Pro`                  |
| Stock           | `Atelier cosmétique`             |

**Ingrédient 6 — Huile de coco vierge**

| Champ           | Valeur                     |
| --------------- | -------------------------- |
| Nom             | `Huile de coco vierge BIO` |
| Unité de mesure | `g`                        |
| Type physique   | `solide`                   |
| Conditionnement | `Pot 1 kg`                 |
| Qté / cond.     | `1000`                     |
| Prix / cond.    | `9.20`                     |
| Seuil alerte    | `200`                      |
| Fournisseur     | `Aromazone Pro`            |
| Stock           | `Atelier cosmétique`       |

**Ingrédient 7 — Soude caustique NaOH**

| Champ           | Valeur                     |
| --------------- | -------------------------- |
| Nom             | `Soude caustique NaOH 99%` |
| Unité de mesure | `g`                        |
| Type physique   | `poudre`                   |
| Densité         | `2.13`                     |
| Conditionnement | `Sac 500 g`                |
| Qté / cond.     | `500`                      |
| Prix / cond.    | `3.80`                     |
| Seuil alerte    | `100`                      |
| Fournisseur     | `Destillerie Bio SPRL`     |
| Stock           | `Atelier cosmétique`       |

**Ingrédient 8 — Eau distillée**

| Champ | Valeur |
|-------|--------|
| Nom | `Eau distillée purifiée` |
| Unité de mesure | `ml` |
| Type physique | `liquide` |
| Densité | `1.00` |
| Conditionnement | `Bidon 5 L` |
| Qté / cond. | `5000` |
| Prix / cond. | `4.50` |
| Seuil alerte | `1000` |
| Fournisseur | `Destillerie Bio SPRL` |
| Stock | `Atelier cosmétique` |

**Ingrédient 9 — Huile essentielle lavande**

| Champ | Valeur |
|-------|--------|
| Nom | `HE Lavande vraie (Lavandula angustifolia)` |
| Unité de mesure | `ml` |
| Type physique | `liquide` |
| Densité | `0.88` |
| Conditionnement | `Flacon 100 ml` |
| Qté / cond. | `100` |
| Prix / cond. | `12.00` |
| Seuil alerte | `20` |
| Fournisseur | `Aromazone Pro` |
| Stock | `Atelier cosmétique` |

---

### B5. Enregistrer les achats (lots)

**Lot 4 — Huile d'olive**

| Champ | Valeur |
|-------|--------|
| Ingrédient | `Huile d'olive extra vierge BIO` |
| N° lot | `AZ-HO-2026-04` |
| Date achat | `2026-04-12` |
| DLC | `2027-04-12` |
| Nb cond. | `2` |
| Prix / cond. | `8.50` |
| TVA % | `6` |

> Stock : **2 000 ml**

**Lot 5 — Huile de coco**

| Champ | Valeur |
|-------|--------|
| Ingrédient | `Huile de coco vierge BIO` |
| N° lot | `AZ-HC-2026-04` |
| Date achat | `2026-04-12` |
| DLC | `2027-10-01` |
| Nb cond. | `1` |
| Prix / cond. | `9.20` |
| TVA % | `6` |

> Stock : **1 000 g**

**Lot 6 — Soude caustique**

| Champ | Valeur |
|-------|--------|
| Ingrédient | `Soude caustique NaOH 99%` |
| N° lot | `DB-NaOH-2026-04` |
| Date achat | `2026-04-14` |
| DLC | `2028-01-01` |
| Nb cond. | `2` |
| Prix / cond. | `3.80` |
| TVA % | `21` |

> Stock : **1 000 g**

**Lot 7 — Eau distillée**

| Champ | Valeur |
|-------|--------|
| Ingrédient | `Eau distillée purifiée` |
| N° lot | `DB-EAU-2026-04` |
| Date achat | `2026-04-14` |
| Nb cond. | `1` |
| Prix / cond. | `4.50` |
| TVA % | `6` |

> Stock : **5 000 ml**

**Lot 8 — HE Lavande**

| Champ | Valeur |
|-------|--------|
| Ingrédient | `HE Lavande vraie` |
| N° lot | `AZ-HEL-2026-04` |
| Date achat | `2026-04-12` |
| DLC | `2028-06-01` |
| Nb cond. | `1` |
| Prix / cond. | `12.00` |
| TVA % | `21` |

> Stock : **100 ml**

---

### B6. Créer le contexte BOM + niveaux

| Champ | Valeur |
|-------|--------|
| Nom du contexte | `Savon Lavande Artisanal` |
| Description | `Saponification à froid — cure 4 semaines` |
| Niveau 1 | `Ingrédients` |
| Niveau 2 | `Masse saponifiée` |
| Niveau 3 | `Savons découpés` |

---

### B7. Créer les fiches BOM

#### Fiche N2 — Masse saponifiée lavande

**Niveau "Masse saponifiée"** → + Nouvelle fiche

| Champ | Valeur |
|-------|--------|
| Nom | `Masse sapo lavande — 1 kg` |
| Description | `Saponification à froid huile olive/coco + soude` |
| Unité output | `g` |
| Quantité output / batch | `1000` |
| Temps préparation | `60` min |

**Lignes d'input :**

| Input | Quantité | Unité |
|-------|----------|-------|
| `Huile d'olive extra vierge BIO` | `350` | `ml` |
| `Huile de coco vierge BIO` | `150` | `g` |
| `Soude caustique NaOH 99%` | `67` | `g` |
| `Eau distillée purifiée` | `200` | `ml` |
| `HE Lavande vraie` | `20` | `ml` |

→ Enregistrer.

#### Fiche N3 — Savons découpés

**Niveau "Savons découpés"** → + Nouvelle fiche

| Champ | Valeur |
|-------|--------|
| Nom | `Savon lavande 100g — fournée 8 pcs` |
| Description | `Découpe après cure — 8 barres de ~100g` |
| Unité output | `piece` |
| Quantité output / batch | `8` |
| Temps préparation | `15` min |

**Ligne d'input (fiche N2) :**

| Input (fiche) | Quantité | Unité |
|---------------|----------|-------|
| `Masse sapo lavande — 1 kg` | `800` | `g` |

→ Enregistrer.

---

### B8. Tester la production

**Production N2 — Masse saponifiée**

| Champ | Valeur |
|-------|--------|
| Contexte | `Savon Lavande Artisanal` |
| Niveau | `Masse saponifiée` |
| Fiche | `Masse sapo lavande — 1 kg` |
| Batches | `1` |

→ Simuler → vérifier :

| Input | Nécessaire | Disponible | Statut |
|-------|-----------|-----------|--------|
| Huile d'olive | 350 ml | 2 000 ml | ✔ vert |
| Huile de coco | 150 g | 1 000 g | ✔ vert |
| Soude NaOH | 67 g | 1 000 g | ✔ vert |
| Eau distillée | 200 ml | 5 000 ml | ✔ vert |
| HE Lavande | 20 ml | 100 ml | ✔ vert |

**Coûts estimés attendus (simulation) :**

| Ligne de coût | Calcul | **Valeur attendue** |
|---|---|---|
| Huile d'olive | 350 ml × 0,0085 €/ml | **2,975 €** |
| Huile de coco | 150 g × 0,0092 €/g | **1,380 €** |
| Soude NaOH | 67 g × 0,0076 €/g | **0,509 €** |
| Eau distillée | 200 ml × 0,0009 €/ml | **0,180 €** |
| HE Lavande | 20 ml × 0,1200 €/ml | **2,400 €** |
| **Coût total ingrédients** | | **7,444 €** |
| Qté produite | 1 batch × 1 000 g | **1 000 g** |
| **Coût unitaire masse sapo** | 7,444 € ÷ 1 000 g | **0,007444 €/g** |

→ Lancer → Vérifier **1 000 g de masse saponifiée** en stock N2.

---

**Production N3 — Savons découpés**

| Champ | Valeur |
|-------|--------|
| Niveau | `Savons découpés` |
| Fiche | `Savon lavande 100g — fournée 8 pcs` |
| Batches | `1` |

→ Simuler → vérifier :

| Input | Nécessaire | Disponible | Statut |
|-------|-----------|-----------|--------|
| Masse sapo lavande *(N2)* | 800 g | 1 000 g | ✔ vert |

**Coûts estimés attendus (simulation) :**

| Ligne de coût | Calcul | **Valeur attendue** |
|---|---|---|
| Masse sapo N2 | 800 g × 0,007444 €/g | **5,955 €** |
| **Coût total ingrédients** | | **5,955 €** |
| Qté produite | 1 batch × 8 pcs | **8 pièces** |
| **Coût unitaire savon** | 5,955 € ÷ 8 pcs | **0,7444 €/pièce** |

→ Lancer → **8 savons** en stock N3.

---

## SCÉNARIO C — Tests d'erreurs attendues

> Vérifier que l'app gère correctement les cas limites.

| Test | Action | Résultat attendu |
|------|--------|-----------------|
| **Pénurie** | Production N2 chocolaterie, 5 batches (nécessite 3 000 g chocolat → disponible 3 000 g après le lot A8) | ✔ ou ✘ selon stock restant — vérifier le code couleur rouge |
| **Cycle** | *(non testable manuellement — protection T-09 code)* | — |
| **Niveau vide** | Créer un contexte N3 sans fiche en N2, tenter de créer une fiche N3 | Message "Aucune fiche disponible au niveau N2" |
| **Nom doublon** | Créer une fiche avec un nom déjà existant | ErrorProvider sur le champ Nom |
| **Déconnexion** | Cliquer Déconnexion → Oui | Retour écran login |
| **Suppression activité avec données** | Tenter de supprimer "Chocolaterie Charles" | Blocage ou confirmation cascade |

---

## Checklist de vérification finale

### Après le scénario A (Chocolaterie)

- [ ] Vue stock global : chocolat noir affiché avec **3 000 g** → après production N2 ×2 : **1 800 g restants** (3000 − 1200)
- [ ] Crème liquide : **2 000 ml** → après production N2 ×2 : **1 200 ml restants** (2000 − 800)
- [ ] Stock niveau Ganache : **2 000 g** visibles
- [ ] Stock niveau Truffes finies : **20 pièces** visibles
- [ ] Historique production : 2 lignes affichées avec coût estimé

### Après le scénario B (Savonnerie)

- [ ] HE Lavande : **100 ml** → après production N2 : **80 ml restants** (100 − 20)
- [ ] Stock niveau Masse saponifiée : **1 000 g** visibles
- [ ] Stock niveau Savons découpés : **8 pièces** visibles

### Navigation SFA (Single-Form Application)

- [ ] Passage Chocolaterie → Savonnerie dans la sidebar : panneau droit se rafraîchit sans ouvrir de nouvelle fenêtre
- [ ] Catalogue → Catégories s'ouvre **inline** (pas de ShowDialog)
- [ ] Catalogue → Parfums s'ouvre **inline**
- [ ] Catalogue → Produits s'ouvre **inline**

---

---

## Résultats monétaires attendus

> À comparer avec les valeurs affichées par l'app lors des simulations et dans l'historique de production.
> Toutes les valeurs sont **HTVA**. Arrondi à 4 décimales.

---

### Prix unitaires en base (par unité de mesure)

#### Chocolaterie Charles

| Ingrédient | Prix / cond. | Qté / cond. | **€ / unité base** |
|---|---|---|---|
| Chocolat noir 70% Caraïbe | 18,50 € | 1 000 g | **0,0185 €/g** |
| Crème liquide UHT 35% | 3,80 € | 1 000 ml | **0,0038 €/ml** |
| Beurre de cacao pur | 24,00 € | 1 000 g | **0,0240 €/g** |
| Cacao en poudre non sucré | 5,20 € | 500 g | **0,0104 €/g** |

#### Savonnerie Nadejda

| Ingrédient | Prix / cond. | Qté / cond. | **€ / unité base** |
|---|---|---|---|
| Huile d'olive extra vierge BIO | 8,50 € | 1 000 ml | **0,0085 €/ml** |
| Huile de coco vierge BIO | 9,20 € | 1 000 g | **0,0092 €/g** |
| Soude caustique NaOH 99% | 3,80 € | 500 g | **0,0076 €/g** |
| Eau distillée purifiée | 4,50 € | 5 000 ml | **0,0009 €/ml** |
| HE Lavande vraie | 12,00 € | 100 ml | **0,1200 €/ml** |

---

### Totaux HTVA des achats (lots)

#### Chocolaterie

| Lot | Nb cond. | Prix / cond. | **Total HTVA** |
|---|---|---|---|
| Chocolat noir — VLR-CHN-2026-04 | 3 | 18,50 € | **55,50 €** |
| Crème liquide — MTR-CR-2026-04 | 2 | 3,80 € | **7,60 €** |
| Cacao en poudre — MTR-CAP-2026-04 | 2 | 5,20 € | **10,40 €** |
| **Total dépenses chocolaterie** | | | **73,50 €** |

#### Savonnerie

| Lot | Nb cond. | Prix / cond. | **Total HTVA** |
|---|---|---|---|
| Huile d'olive — AZ-HO-2026-04 | 2 | 8,50 € | **17,00 €** |
| Huile de coco — AZ-HC-2026-04 | 1 | 9,20 € | **9,20 €** |
| Soude NaOH — DB-NaOH-2026-04 | 2 | 3,80 € | **7,60 €** |
| Eau distillée — DB-EAU-2026-04 | 1 | 4,50 € | **4,50 €** |
| HE Lavande — AZ-HEL-2026-04 | 1 | 12,00 € | **12,00 €** |
| **Total dépenses savonnerie** | | | **50,30 €** |

---

### Coût de revient des fiches BOM (par batch)

#### A — Fiche N2 : Ganache chocolat noir 60/40

> 1 batch → 1 000 g de ganache

| Input | Qté | Unité | €/unité | **Sous-total** |
|---|---|---|---|---|
| Chocolat noir 70% Caraïbe | 600 | g | 0,0185 | **11,10 €** |
| Crème liquide UHT 35% | 400 | ml | 0,0038 | **1,52 €** |
| **Coût total / batch** | | | | **12,62 €** |
| **Coût unitaire output** | | | | **0,01262 €/g** |

> **2 batches lancés → coût total : 25,24 € → 2 000 g produits**

---

#### A — Fiche N3 : Truffes enrobées cacao — plaque 20

> 1 batch → 20 pièces de truffes

| Input | Qté | Unité | €/unité | **Sous-total** |
|---|---|---|---|---|
| Ganache chocolat noir 60/40 *(N2)* | 500 | g | 0,01262 | **6,31 €** |
| Cacao en poudre non sucré | 100 | g | 0,0104 | **1,04 €** |
| **Coût total / batch** | | | | **7,35 €** |
| **Coût unitaire output** | | | | **0,3675 €/pièce** |

---

#### B — Fiche N2 : Masse sapo lavande — 1 kg

> 1 batch → 1 000 g de masse saponifiée

| Input | Qté | Unité | €/unité | **Sous-total** |
|---|---|---|---|---|
| Huile d'olive extra vierge BIO | 350 | ml | 0,0085 | **2,975 €** |
| Huile de coco vierge BIO | 150 | g | 0,0092 | **1,380 €** |
| Soude caustique NaOH 99% | 67 | g | 0,0076 | **0,509 €** |
| Eau distillée purifiée | 200 | ml | 0,0009 | **0,180 €** |
| HE Lavande vraie | 20 | ml | 0,1200 | **2,400 €** |
| **Coût total / batch** | | | | **7,444 €** |
| **Coût unitaire output** | | | | **0,007444 €/g** |

---

#### B — Fiche N3 : Savon lavande 100g — fournée 8 pcs

> 1 batch → 8 savons

| Input | Qté | Unité | €/unité | **Sous-total** |
|---|---|---|---|---|
| Masse sapo lavande *(N2)* | 800 | g | 0,007444 | **5,955 €** |
| **Coût total / batch** | | | | **5,955 €** |
| **Coût unitaire output** | | | | **0,7444 €/pièce** |

---

### Stocks restants après toutes les productions

#### Chocolaterie — après N2 ×2 batches + N3 ×1 batch

| Ingrédient / Stock | Initial | Consommé | **Restant** |
|---|---|---|---|
| Chocolat noir 70% | 3 000 g | 1 200 g *(N2×2)* | **1 800 g** |
| Crème liquide UHT | 2 000 ml | 800 ml *(N2×2)* | **1 200 ml** |
| Cacao en poudre | 1 000 g | 100 g *(N3×1)* | **900 g** |
| **Ganache N2** | — | — | **1 500 g** *(2000 produits − 500 consommés)* |
| **Truffes N3** | — | — | **20 pièces** |

#### Savonnerie — après N2 ×1 batch + N3 ×1 batch

| Ingrédient / Stock | Initial | Consommé | **Restant** |
|---|---|---|---|
| Huile d'olive | 2 000 ml | 350 ml *(N2×1)* | **1 650 ml** |
| Huile de coco | 1 000 g | 150 g *(N2×1)* | **850 g** |
| Soude NaOH | 1 000 g | 67 g *(N2×1)* | **933 g** |
| Eau distillée | 5 000 ml | 200 ml *(N2×1)* | **4 800 ml** |
| HE Lavande | 100 ml | 20 ml *(N2×1)* | **80 ml** |
| **Masse sapo N2** | — | — | **200 g** *(1000 produits − 800 consommés)* |
| **Savons N3** | — | — | **8 pièces** |

---

## Données de référence — récapitulatif rapide

| Activité | Contexte | N2 | N3 | Stock N2 après 1 batch | Stock N3 après 1 batch |
|----------|----------|----|----|------------------------|------------------------|
| Chocolaterie Charles | Truffes Chocolat Noir | Ganache chocolat 60/40 | Truffes enrobées | 1 000 g | 20 pcs |
| Savonnerie Nadejda | Savon Lavande Artisanal | Masse sapo lavande | Savon lavande 100g | 1 000 g | 8 pcs |
