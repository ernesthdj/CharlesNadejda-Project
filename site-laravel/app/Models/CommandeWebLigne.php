<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CommandeWebLigne extends Model
{
    protected $table = 'commandes_web_lignes';
    public $timestamps = false;

    protected $fillable = [
        'id_commande', 'id_produit_web', 'quantite', 'prix_unitaire',
    ];

    public function commande()
    {
        return $this->belongsTo(CommandeWeb::class, 'id_commande');
    }

    public function produit()
    {
        return $this->belongsTo(ProduitWeb::class, 'id_produit_web');
    }
}
