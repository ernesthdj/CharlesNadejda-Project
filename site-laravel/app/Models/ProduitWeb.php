<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\DB;

/**
 * Produit mis en vente sur la boutique web.
 *
 * Lie a une fiche BOM (Bill of Materials) via id_bom_fiche.
 * Le stock vendable est calcule dynamiquement depuis bom_stocks / quantite_output.
 *
 * @property int    $id
 * @property string $nom_commercial
 * @property float  $prix_vente
 * @property int    $en_vente
 * @property int    $id_bom_fiche
 * @property int    $id_categorie
 * @property int    $ordre_affichage
 *
 * @property-read float $stock_disponible  Stock en unites vendables (accessor).
 * @property-read bool  $en_stock          True si stock_disponible > 0 (accessor).
 * @property-read CategorieWeb|null $categorie
 * @property-read BomFiche|null     $bomFiche
 */
class ProduitWeb extends Model
{
    protected $table = 'produits_web';

    const CREATED_AT = 'date_creation';
    const UPDATED_AT = 'date_modification';

    /** Categorie d'affichage sur la boutique (nullable). */
    public function categorie(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
        return $this->belongsTo(CategorieWeb::class, 'id_categorie');
    }

    /** Fiche BOM (Bill of Materials) liee — source du calcul de stock. */
    public function bomFiche(): \Illuminate\Database\Eloquent\Relations\BelongsTo
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

    /** Indique si le produit est disponible a la vente (stock > 0). */
    public function getEnStockAttribute(): bool
    {
        return $this->stock_disponible > 0;
    }
}
