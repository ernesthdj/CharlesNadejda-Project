<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Lecture seule + décrémentation à la commande.
 * Table existante gérée par l'ERP C#.
 */
class BomStock extends Model
{
    protected $table = 'bom_stocks';
    public $timestamps = false;

    protected $fillable = ['quantite_disponible'];
}
