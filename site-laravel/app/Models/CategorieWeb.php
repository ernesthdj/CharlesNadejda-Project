<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Categorie d'affichage pour le catalogue de la boutique web.
 *
 * Gere le regroupement visuel des produits (ex : Gateaux, Viennoiseries).
 * Les categories inactives (actif = 0) sont masquees du catalogue.
 *
 * @property int    $id
 * @property string $nom
 * @property int    $actif
 * @property int    $ordre_affichage
 *
 * @property-read \Illuminate\Database\Eloquent\Collection<ProduitWeb> $produits
 */
class CategorieWeb extends Model
{
    protected $table = 'categories_web';
    public $timestamps = false;

    /** Tous les produits rattaches a cette categorie. */
    public function produits(): \Illuminate\Database\Eloquent\Relations\HasMany
    {
        return $this->hasMany(ProduitWeb::class, 'id_categorie');
    }

    /** Produits actifs en vente (en_vente = 1) de cette categorie. */
    public function produitsEnVente(): \Illuminate\Database\Eloquent\Relations\HasMany
    {
        return $this->produits()->where('en_vente', 1);
    }
}
