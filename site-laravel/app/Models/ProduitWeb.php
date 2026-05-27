<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\DB;

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
     * Scope : ajoute une sous-requête stock_disponible pour éviter le N+1.
     * Usage : ProduitWeb::withStockDisponible()->get()
     */
    public function scopeWithStockDisponible(Builder $query): Builder
    {
        return $query->addSelect([
            'stock_calc' => BomStock::selectRaw('COALESCE(SUM(quantite_disponible), 0)')
                ->whereColumn('id_fiche', 'produits_web.id_bom_fiche')
                ->where('quantite_disponible', '>', 0),
        ]);
    }

    /**
     * Stock calculé dynamiquement depuis bom_stocks.
     * Utilise stock_calc si chargé via scope, sinon requête unitaire (fallback).
     */
    public function getStockDisponibleAttribute(): float
    {
        if ($this->attributes['stock_calc'] ?? null !== null) {
            return (float) ($this->attributes['stock_calc'] ?? 0);
        }

        return (float) BomStock::where('id_fiche', $this->id_bom_fiche)
            ->where('quantite_disponible', '>', 0)
            ->sum('quantite_disponible');
    }

    public function getEnStockAttribute(): bool
    {
        return $this->stock_disponible > 0;
    }
}
