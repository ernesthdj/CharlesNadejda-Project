<?php

namespace App\Http\Controllers;

use App\Models\BomStock;
use App\Models\Client;
use App\Models\CommandeWeb;
use App\Http\Requests\CheckoutRequest;
use Illuminate\Support\Facades\DB;

/**
 * Gestion des commandes — recap, validation (checkout) et historique.
 *
 * La validation utilise une transaction DB avec lockForUpdate pour garantir
 * la coherence du stock lors de la decrementation FIFO.
 */
class CommandeController extends Controller
{
    /**
     * GET /commande/recap — Afficher le recapitulatif avant paiement.
     *
     * Redirige vers le panier si celui-ci est vide.
     *
     * @return \Illuminate\Contracts\View\View|\Illuminate\Http\RedirectResponse
     */
    public function recap(): \Illuminate\Contracts\View\View|\Illuminate\Http\RedirectResponse
    {
        $panier = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->with('lignes.produit')
            ->first();

        if (!$panier || $panier->lignes->isEmpty()) {
            return redirect()->route('panier')
                ->with('error', 'Votre panier est vide.');
        }

        $client = Client::findOrFail(session('client_id'));

        return view('commandes.recap', compact('panier', 'client'));
    }

    /**
     * POST /commande/valider — Valider la commande (checkout).
     *
     * Deroule dans une transaction DB :
     * 1. Verrouille le panier (lockForUpdate) pour eviter les race conditions.
     * 2. Decremente le stock FIFO lot par lot pour chaque ligne de commande.
     * 3. Snapshot l'adresse de livraison et finalise la commande (statut = payee).
     * Rollback complet si le stock est insuffisant ou en cas d'erreur.
     *
     * @return \Illuminate\Http\RedirectResponse
     */
    public function valider(CheckoutRequest $request): \Illuminate\Http\RedirectResponse
    {
        DB::beginTransaction();
        try {
            $panier = CommandeWeb::where('id_client', session('client_id'))
                ->where('statut', 'panier')
                ->with('lignes.produit')
                ->lockForUpdate()
                ->first();

            if (!$panier || $panier->lignes->isEmpty()) {
                DB::rollBack();
                return redirect()->route('panier')
                    ->with('error', 'Votre panier est vide.');
            }

            // Décrémentation FIFO pour chaque ligne
            foreach ($panier->lignes as $ligne) {
                $restant = $ligne->quantite;
                $idFiche = $ligne->produit->id_bom_fiche;

                $stocks = BomStock::where('id_fiche', $idFiche)
                    ->where('quantite_disponible', '>', 0)
                    ->orderBy('date_production', 'asc')
                    ->lockForUpdate()
                    ->get();

                $totalDispo = $stocks->sum('quantite_disponible');
                if ($totalDispo < $restant) {
                    DB::rollBack();
                    return redirect()->route('panier')
                        ->with('error', 'Stock insuffisant pour « ' . $ligne->produit->nom_commercial . ' ». Veuillez ajuster votre panier.');
                }

                foreach ($stocks as $stock) {
                    if ($restant <= 0) break;

                    $aConsommer = min($restant, $stock->quantite_disponible);
                    $stock->quantite_disponible -= $aConsommer;
                    $stock->save();
                    $restant -= $aConsommer;
                }
            }

            // Snapshot adresse livraison
            $adresse = collect([
                $request->adresse_rue,
                trim(($request->adresse_cp ?? '') . ' ' . ($request->adresse_ville ?? '')),
                $request->adresse_pays ?? 'Belgique',
            ])->filter()->implode(', ');

            // Finaliser la commande
            $panier->update([
                'statut'            => 'payee',
                'date_commande'     => now(),
                'adresse_livraison' => $adresse,
                'total_ttc'         => $panier->lignes->sum('sous_total'),
            ]);

            DB::commit();

            // Reset le compteur panier en session (le panier vient d'être converti en commande)
            session(['panier_count' => 0]);

            return redirect()->route('commande.detail', $panier->id)
                ->with('success', 'Commande validée avec succès !');

        } catch (\Exception $e) {
            DB::rollBack();
            return redirect()->route('panier')
                ->with('error', 'Une erreur est survenue. Veuillez réessayer.');
        }
    }

    /**
     * GET /mes-commandes — Afficher l'historique des commandes du client.
     *
     * Exclut les paniers en cours (statut != 'panier'), tri antéchronologique.
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function historique(): \Illuminate\Contracts\View\View
    {
        $commandes = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', '!=', 'panier')
            ->with('lignes')
            ->orderByDesc('date_commande')
            ->get();

        return view('commandes.historique', compact('commandes'));
    }

    /**
     * GET /commande/{id} — Afficher le detail/confirmation d'une commande.
     *
     * Ownership check : seul le client proprietaire peut voir sa commande.
     * Retourne 404 si la commande n'appartient pas au client connecte.
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function detail(int $id): \Illuminate\Contracts\View\View
    {
        // QA-04 : ownership check obligatoire
        $commande = CommandeWeb::where('id', $id)
            ->where('id_client', session('client_id'))
            ->where('statut', '!=', 'panier')
            ->with('lignes.produit')
            ->firstOrFail();

        return view('commandes.confirmation', compact('commande'));
    }
}
