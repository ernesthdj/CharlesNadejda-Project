<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CategorieWeb extends Model
{
    protected $table = 'categories_web';
    public $timestamps = false;

    public function produits()
    {
        return $this->hasMany(ProduitWeb::class, 'id_categorie');
    }

    public function produitsEnVente()
    {
        return $this->produits()->where('en_vente', 1);
    }
}
