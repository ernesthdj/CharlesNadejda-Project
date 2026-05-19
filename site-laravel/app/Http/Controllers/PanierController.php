<?php

namespace App\Http\Controllers;

use App\Models\CommandeWeb;
use App\Models\CommandeWebLigne;
use App\Models\ProduitWeb;
use Illuminate\Http\Request;

class PanierController extends Controller
{
    public function index()
    {
        $panier = $this->getPanierActif();

        return view('panier.index', compact('panier'));
    }

    /**
     * Ajouter un produit au panier (AJAX).
     */
    public function ajouter(Request $request)
    {
        $request->validate([
            'id_produit' => 'required|integer|exists:produits_web,id',
            'quantite'   => 'required|integer|min:1',
        ]);

        $produit = ProduitWeb::findOrFail($request->id_produit);

        // Vérifier stock
        if ($produit->stock_disponible < $request->quantite) {
            return response()->json([
                'success' => false,
                'message' => 'Stock insuffisant. Disponible : ' . $produit->stock_disponible,
            ]);
        }

        $panier = $this->getOrCreatePanier();

        // Si produit déjà dans le panier, incrémenter
        $ligne = $panier->lignes()->where('id_produit_web', $produit->id)->first();
        if ($ligne) {
            $newQte = $ligne->quantite + $request->quantite;
            if ($newQte > $produit->stock_disponible) {
                return response()->json([
                    'success' => false,
                    'message' => 'Quantité maximale atteinte (stock : ' . $produit->stock_disponible . ').',
                ]);
            }
            $ligne->update(['quantite' => $newQte]);
        } else {
            CommandeWebLigne::create([
                'id_commande'    => $panier->id,
                'id_produit_web' => $produit->id,
                'quantite'       => $request->quantite,
                'prix_unitaire'  => $produit->prix_vente,
            ]);
        }

        return response()->json([
            'success'      => true,
            'message'      => $produit->nom_commercial . ' ajouté au panier.',
            'panier_count' => $this->getPanierCount(),
        ]);
    }

    /**
     * Modifier la quantité d'une ligne (AJAX).
     */
    public function updateQuantite(Request $request)
    {
        $request->validate([
            'id_ligne' => 'required|integer',
            'quantite' => 'required|integer|min:1',
        ]);

        $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

        // Vérifier ownership
        $panier = $this->getPanierActif();
        if (!$panier || $ligne->id_commande !== $panier->id) {
            return response()->json(['success' => false, 'message' => 'Accès non autorisé.'], 403);
        }

        // Vérifier stock
        $produit = $ligne->produit;
        if ($request->quantite > $produit->stock_disponible) {
            return response()->json([
                'success' => false,
                'message' => 'Stock insuffisant (disponible : ' . $produit->stock_disponible . ').',
            ]);
        }

        $ligne->update(['quantite' => $request->quantite]);
        $ligne->refresh();

        return response()->json([
            'success'      => true,
            'sous_total'   => number_format($ligne->sous_total, 2, ',', ' '),
            'total'        => number_format($panier->lignes()->sum('sous_total'), 2, ',', ' '),
            'panier_count' => $this->getPanierCount(),
        ]);
    }

    /**
     * Supprimer une ligne du panier (AJAX).
     */
    public function supprimer(Request $request)
    {
        $request->validate(['id_ligne' => 'required|integer']);

        $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

        $panier = $this->getPanierActif();
        if (!$panier || $ligne->id_commande !== $panier->id) {
            return response()->json(['success' => false, 'message' => 'Accès non autorisé.'], 403);
        }

        $ligne->delete();

        return response()->json([
            'success'      => true,
            'total'        => number_format($panier->lignes()->sum('sous_total'), 2, ',', ' '),
            'panier_count' => $this->getPanierCount(),
        ]);
    }

    /**
     * Compteur panier pour le badge header (AJAX).
     */
    public function count()
    {
        return response()->json(['count' => $this->getPanierCount()]);
    }

    // ── Helpers ──────────────────────────────────────────────

    private function getPanierActif(): ?CommandeWeb
    {
        return CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->with('lignes.produit')
            ->first();
    }

    private function getOrCreatePanier(): CommandeWeb
    {
        return CommandeWeb::firstOrCreate(
            ['id_client' => session('client_id'), 'statut' => 'panier'],
            ['total_ttc' => 0]
        );
    }

    private function getPanierCount(): int
    {
        $panier = CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->first();

        return $panier ? (int) $panier->lignes()->sum('quantite') : 0;
    }
}
