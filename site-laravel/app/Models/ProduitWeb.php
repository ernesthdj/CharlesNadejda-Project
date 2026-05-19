<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ProduitWeb extends Model
{
    protected $table = 'produits_web';

    const CREATED_AT = 'date_creation';
    const UPDATED_AT = 'date_modification';

    public function categorie()
    {
        return $this->belongsTo(CategorieWeb::class, 'id_categorie');
    }

    public function bomFiche()
    {
        return $this->belongsTo(BomFiche::class, 'id_bom_fiche');
    }

    /**
     * Stock calculé dynamiquement depuis bom_stocks.
     */
    public function getStockDisponibleAttribute(): float
    {
        return (float) BomStock::where('id_fiche', $this->id_bom_fiche)
            ->where('quantite_disponible', '>', 0)
            ->sum('quantite_disponible');
    }

    public function getEnStockAttribute(): bool
    {
        return $this->stock_disponible > 0;
    }
}
