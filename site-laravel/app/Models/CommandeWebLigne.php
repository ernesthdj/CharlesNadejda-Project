<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Ligne de commande web — associe un produit a une commande avec quantite et prix.
 *
 * Le prix_unitaire est fige au moment de l'ajout au panier (snapshot).
 *
 * @property int   $id
 * @property int   $id_commande
 * @property int   $id_produit_web
 * @property int   $quantite
 * @property float $prix_unitaire   Prix fige au moment de l'ajout.
 *
 * @property-read CommandeWeb $commande
 * @property-read ProduitWeb  $produit
 */
class CommandeWebLigne extends Model
{
    protected $table = 'commandes_web_lignes';
    public $timestamps = false;

    protected $fillable = [
        'id_commande', 'id_produit_web', 'quantite', 'prix_unitaire',
    ];

    /** Commande parente a laquelle appartient cette ligne. */
    public function commande(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
        return $this->belongsTo(CommandeWeb::class, 'id_commande');
    }

    /** Produit web reference par cette ligne. */
    public function produit(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
        return $this->belongsTo(ProduitWeb::class, 'id_produit_web');
    }
}
