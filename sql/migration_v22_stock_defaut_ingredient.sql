-- Migration v22 : stock de rangement par défaut sur les fiches ingrédients
-- Permet d'associer une fiche à un stock physique indépendamment des lots achetés.
-- Utilisé par le filtre chip "Fiches" de FrmIngredients pour regrouper le catalogue.
--
-- ON DELETE SET NULL : si un stock est supprimé, la fiche perd sa référence
-- (pas de suppression en cascade — la fiche reste dans le catalogue).

ALTER TABLE fiches_ingredients
    ADD COLUMN id_stock_defaut INT NULL AFTER id_fournisseur_defaut,
    ADD CONSTRAINT fk_fi_stock_defaut
        FOREIGN KEY (id_stock_defaut)
        REFERENCES stocks(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;
