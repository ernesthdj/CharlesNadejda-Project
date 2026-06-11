<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Fiche BOM (Bill of Materials) — recette de production d'un produit fini.
 *
 * Lecture seule cote Laravel — table geree par l'ERP C# (ArtisaStock).
 * Contient la quantite_output (nb d'unites produites par lot),
 * utilisee pour convertir le stock brut en unites vendables.
 *
 * @property int    $id
 * @property string $nom
 * @property float  $quantite_output  Nb d'unites produites par execution de la recette.
 *
 * @property-read ProduitWeb|null $produitWeb
 * @property-read \Illuminate\Database\Eloquent\Collection<BomStock> $stocks
 */
class BomFiche extends Model
{
    protected $table = 'bom_fiches';
    public $timestamps = false;

    /** Produit web lie a cette fiche (relation inverse 1:1). */
    public function produitWeb(): \Illuminate\Database\Eloquent\Relations\HasOne
    {
        return $this->hasOne(ProduitWeb::class, 'id_bom_fiche');
    }

    /** Lots en stock pour cette fiche (FIFO par date_production). */
    public function stocks(): \Illuminate\Database\Eloquent\Relations\HasMany
    {
        return $this->hasMany(BomStock::class, 'id_fiche');
    }
}
