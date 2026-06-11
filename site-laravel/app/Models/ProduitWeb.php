<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\DB;

// 📌 SCRIPT DEFENSE — Étape 6.1 : Accessor getStockDisponibleAttribute — stock calculé depuis bom_stocks
//                     Étape 6.1 : scopeWithStockDisponible — sous-requête pour éviter N+1
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
     * Scope : ajoute le stock en unités vendables (brut / quantite_output de la fiche BOM).
     * Aligné avec le calcul ArtisaStock C# : FLOOR(SUM(dispo) / quantite_output).
     * Usage : ProduitWeb::withStockDisponible()->get()
     */
    public function scopeWithStockDisponible(Builder $query): Builder
    {
        return $query->selectRaw('produits_web.*, (
            SELECT FLOOR(COALESCE(SUM(bs.quantite_disponible), 0) / bf.quantite_output)
            FROM bom_stocks bs
            INNER JOIN bom_fiches bf ON bf.id = bs.id_fiche
            WHERE bs.id_fiche = produits_web.id_bom_fiche
              AND bs.quantite_disponible > 0
            GROUP BY bf.quantite_output
        ) AS stock_calc');
    }

    /**
     * Stock en unités vendables, calculé depuis bom_stocks / quantite_output.
     * Utilise stock_calc si chargé via scope, sinon requête unitaire (fallback).
     */
    public function getStockDisponibleAttribute(): float
    {
        if (array_key_exists('stock_calc', $this->attributes) && $this->attributes['stock_calc'] !== null) {
            return (float) $this->attributes['stock_calc'];
        }

        return (float) DB::selectOne('
            SELECT FLOOR(COALESCE(SUM(bs.quantite_disponible), 0) / bf.quantite_output) AS stock
            FROM bom_stocks bs
            INNER JOIN bom_fiches bf ON bf.id = bs.id_fiche
            WHERE bs.id_fiche = ?
              AND bs.quantite_disponible > 0
            GROUP BY bf.quantite_output
        ', [$this->id_bom_fiche])?->stock ?? 0;
    }

    public function getEnStockAttribute(): bool
    {
        return $this->stock_disponible > 0;
    }
}
