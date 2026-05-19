<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Lecture seule — table existante gérée par l'ERP C#.
 */
class BomFiche extends Model
{
    protected $table = 'bom_fiches';
    public $timestamps = false;

    public function produitWeb()
    {
        return $this->hasOne(ProduitWeb::class, 'id_bom_fiche');
    }

    public function stocks()
    {
        return $this->hasMany(BomStock::class, 'id_fiche');
    }
}
