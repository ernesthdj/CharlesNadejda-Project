<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CommandeWeb extends Model
{
    protected $table = 'commandes_web';
    public $timestamps = false;

    protected $fillable = [
        'id_client', 'statut', 'total_ttc',
        'adresse_livraison', 'date_commande',
    ];

    public function client()
    {
        return $this->belongsTo(Client::class, 'id_client');
    }

    public function lignes()
    {
        return $this->hasMany(CommandeWebLigne::class, 'id_commande');
    }
}
