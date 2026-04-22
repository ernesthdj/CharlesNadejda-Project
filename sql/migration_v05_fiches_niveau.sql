-- ============================================================
-- Migration v5 — Lier bom_fiches à un niveau de contexte
-- Problème résolu : bom_fiches n'était liée qu'à une activité,
-- pas à un niveau spécifique d'un contexte. Sans ce lien, il
-- était impossible de filtrer les fiches par niveau, d'imposer
-- que les inputs viennent du niveau N-1, ou de séparer les
-- fiches de deux contextes différents.
-- ============================================================

USE charlesnadejda;
SET FOREIGN_KEY_CHECKS = 0;

-- 1. Ajouter id_niveau (DEFAULT 0 temporaire pour tables non-vides)
ALTER TABLE bom_fiches
    ADD COLUMN id_niveau INT NOT NULL DEFAULT 0
    AFTER id;

-- 2. Assigner les fiches existantes au premier niveau disponible
--    (évite la violation de FK pour les données de développement existantes)
UPDATE bom_fiches f
    INNER JOIN (SELECT MIN(id) AS id_min FROM bom_niveaux) n ON 1=1
    SET f.id_niveau = n.id_min
    WHERE f.id_niveau = 0;

-- 3. Supprimer l'ancien UNIQUE sur nom seul
ALTER TABLE bom_fiches DROP INDEX nom;

-- 4. Ajouter UNIQUE (nom, id_niveau) — même nom possible dans niveaux différents
ALTER TABLE bom_fiches
    ADD UNIQUE KEY uq_fiche_nom_niveau (nom, id_niveau);

-- 5. Supprimer le DEFAULT 0
ALTER TABLE bom_fiches
    MODIFY COLUMN id_niveau INT NOT NULL;

-- 6. Ajouter la FK vers bom_niveaux
ALTER TABLE bom_fiches
    ADD CONSTRAINT fk_bf_niveau
        FOREIGN KEY (id_niveau) REFERENCES bom_niveaux(id)
        ON DELETE RESTRICT ON UPDATE CASCADE;

-- 7. Corriger le nom du N1 créé vide par défaut (migration correctrice)
UPDATE bom_niveaux SET nom = 'Ingrédients' WHERE nom = '' AND ordre = 1;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN migration v5
-- Colonnes ajoutées : bom_fiches.id_niveau (FK → bom_niveaux)
-- UNIQUE modifié : (nom) → (nom, id_niveau)
-- ============================================================
