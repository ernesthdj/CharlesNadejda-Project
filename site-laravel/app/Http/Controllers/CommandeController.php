<?php

namespace App\Http\Controllers;

use App\Models\BomStock;
use App\Models\Client;
use App\Models\CommandeWeb;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;

class CommandeController extends Controller
{
    public function recap()
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
     * Valider la commande — simulation paiement + décrémentation FIFO.
     */
    public function valider(Request $request)
    {
        $request->validate([
            'adresse_rue'   => 'nullable|string|max:255',
            'adresse_cp'    => 'nullable|string|max:10',
            'adresse_ville' => 'nullable|string|max:100',
            'adresse_pays'  => 'nullable|string|max:100',
        ]);

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

            return redirect()->route('commande.detail', $panier->id)
                ->with('success', 'Commande validée avec succès !');

        } catch (\Exception $e) {
            DB::rollBack();
            return redirect()->route('panier')
                ->with('error', 'Une erreur est survenue. Veuillez réessayer.');
        }
    }

    public function historique()
    {
        $commandes = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', '!=', 'panier')
            ->with('lignes')
            ->orderByDesc('date_commande')
            ->get();

        return view('commandes.historique', compact('commandes'));
    }

    public function detail(int $id)
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
