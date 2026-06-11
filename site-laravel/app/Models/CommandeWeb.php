<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Commande web d'un client (ou panier en cours).
 *
 * Cycle de vie : panier -> payee -> expediee -> livree.
 * Le statut 'panier' represente le panier actif du client (une seule par client).
 * La conversion panier -> commande se fait via CommandeController::valider().
 *
 * @property int         $id
 * @property int         $id_client
 * @property string      $statut            panier|payee|expediee|livree|annulee
 * @property float       $total_ttc
 * @property string|null $adresse_livraison Snapshot de l'adresse au moment de la commande.
 * @property \Carbon\Carbon|null $date_commande
 *
 * @property-read Client $client
 * @property-read \Illuminate\Database\Eloquent\Collection<CommandeWebLigne> $lignes
 */
class CommandeWeb extends Model
{
    protected $table = 'commandes_web';
    public $timestamps = false;

    protected $fillable = [
        'id_client', 'statut', 'total_ttc',
        'adresse_livraison', 'date_commande',
    ];

    protected $casts = [
        'date_commande' => 'datetime',
        'date_creation' => 'datetime',
        'total_ttc'     => 'decimal:2',
    ];

    /** Client proprietaire de cette commande. */
    public function client(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
        return $this->belongsTo(Client::class, 'id_client');
    }

    /** Lignes de commande (produit + quantite + prix). */
    public function lignes(): \Illuminate\Database\Eloquent\Relations\HasMany
    {
        return $this->hasMany(CommandeWebLigne::class, 'id_commande');
    }
}
