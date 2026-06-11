<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Lot de stock pour une fiche BOM — suivi FIFO (First In, First Out).
 *
 * Geree par l'ERP C# (creation lors de la production).
 * Cote Laravel : lecture + decrementation lors de la validation de commande.
 * La decrementation FIFO est effectuee dans CommandeController::valider().
 *
 * @property int   $id
 * @property int   $id_fiche
 * @property float $quantite_disponible  Quantite restante dans ce lot.
 * @property \Carbon\Carbon|null $date_production
 */
class BomStock extends Model
{
    protected $table = 'bom_stocks';
    public $timestamps = false;

    protected $fillable = ['quantite_disponible'];
}
