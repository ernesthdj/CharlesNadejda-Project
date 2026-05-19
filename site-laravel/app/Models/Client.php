<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Client extends Model
{
    protected $table = 'clients';

    protected $fillable = [
        'nom', 'prenom', 'email', 'mot_de_passe',
        'telephone', 'adresse_rue', 'adresse_cp',
        'adresse_ville', 'adresse_pays',
    ];

    protected $hidden = ['mot_de_passe'];

    const CREATED_AT = 'date_creation';
    const UPDATED_AT = 'date_modification';

    public function commandes()
    {
        return $this->hasMany(CommandeWeb::class, 'id_client');
    }

    public function panierActif()
    {
        return $this->hasOne(CommandeWeb::class, 'id_client')
                    ->where('statut', 'panier');
    }

    public function getAdresseCompleteAttribute(): string
    {
        return collect([
            $this->adresse_rue,
            trim(($this->adresse_cp ?? '') . ' ' . ($this->adresse_ville ?? '')),
            $this->adresse_pays,
        ])->filter()->implode(', ');
    }
}
