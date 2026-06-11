<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Client inscrit sur la boutique web.
 *
 * Authentification custom par session (pas de Laravel Auth guard).
 * Le mot de passe est hashe en BCrypt, compatible avec l'ERP C#.
 *
 * @property int    $id
 * @property string $nom
 * @property string $prenom
 * @property string $email
 * @property string $mot_de_passe  Hash BCrypt (hidden).
 * @property string|null $telephone
 * @property string|null $adresse_rue
 * @property string|null $adresse_cp
 * @property string|null $adresse_ville
 * @property string|null $adresse_pays
 *
 * @property-read string $adresse_complete  Adresse formatee sur une ligne (accessor).
 * @property-read \Illuminate\Database\Eloquent\Collection<CommandeWeb> $commandes
 * @property-read CommandeWeb|null $panierActif
 */
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

    /** Toutes les commandes du client (panier inclus). */
    public function commandes(): \Illuminate\Database\Eloquent\Relations\HasMany
    {
        return $this->hasMany(CommandeWeb::class, 'id_client');
    }

    /** Panier en cours (statut = 'panier'), null si aucun. */
    public function panierActif(): \Illuminate\Database\Eloquent\Relations\HasOne
    {
        return $this->hasOne(CommandeWeb::class, 'id_client')
                    ->where('statut', 'panier');
    }

    /** Adresse postale complete formatee sur une ligne (rue, CP ville, pays). */
    public function getAdresseCompleteAttribute(): string
    {
        return collect([
            $this->adresse_rue,
            trim(($this->adresse_cp ?? '') . ' ' . ($this->adresse_ville ?? '')),
            $this->adresse_pays,
        ])->filter()->implode(', ');
    }
}
