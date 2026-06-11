<?php

namespace App\Http\Controllers;

use App\Models\CommandeWeb;
use App\Models\CommandeWebLigne;
use App\Models\ProduitWeb;
use Illuminate\Http\Request;

/**
 * Gestion du panier client — operations CRUD en AJAX.
 *
 * Le panier est une CommandeWeb avec statut = 'panier'.
 * Chaque operation verifie l'ownership (le panier appartient au client connecte)
 * et le stock disponible avant modification.
 */
class PanierController extends Controller
{
    /**
     * GET /panier — Afficher le contenu du panier.
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function index(): \Illuminate\Contracts\View\View
    {
        $panier = $this->getPanierActif();

        return view('panier.index', compact('panier'));
    }

    /**
     * POST /panier/ajouter — Ajouter un produit au panier (AJAX).
     *
     * Verifie le stock disponible avant ajout. Incremente la quantite si le produit
     * est deja present dans le panier. Cree le panier si inexistant.
     *
     * @return \Illuminate\Http\JsonResponse
     */
    public function ajouter(Request $request): \Illuminate\Http\JsonResponse
    {
        $request->validate([
            'id_produit' => 'required|integer|exists:produits_web,id',
            'quantite'   => 'required|integer|min:1',
        ]);

        $produit = ProduitWeb::withStockDisponible()->findOrFail($request->id_produit);

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

        $count = $this->refreshPanierCount($panier);

        return response()->json([
            'success'      => true,
            'message'      => $produit->nom_commercial . ' ajouté au panier.',
            'panier_count' => $count,
        ]);
    }

    /**
     * PATCH /panier/quantite — Modifier la quantite d'une ligne du panier (AJAX).
     *
     * Verifie l'ownership de la ligne (403 si le panier ne lui appartient pas)
     * et le stock disponible avant mise a jour.
     *
     * @return \Illuminate\Http\JsonResponse
     */
    public function updateQuantite(Request $request): \Illuminate\Http\JsonResponse
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

        // Recharger les lignes pour avoir les totaux à jour
        $panier->load('lignes');
        $count = $this->refreshPanierCount($panier);

        return response()->json([
            'success'      => true,
            'sous_total'   => number_format($ligne->fresh()->sous_total, 2, ',', ' '),
            'total'        => number_format($panier->lignes->sum('sous_total'), 2, ',', ' '),
            'panier_count' => $count,
        ]);
    }

    /**
     * DELETE /panier/supprimer — Retirer une ligne du panier (AJAX).
     *
     * Verifie l'ownership de la ligne avant suppression.
     *
     * @return \Illuminate\Http\JsonResponse
     */
    public function supprimer(Request $request): \Illuminate\Http\JsonResponse
    {
        $request->validate(['id_ligne' => 'required|integer']);

        $ligne = CommandeWebLigne::findOrFail($request->id_ligne);

        $panier = $this->getPanierActif();
        if (!$panier || $ligne->id_commande !== $panier->id) {
            return response()->json(['success' => false, 'message' => 'Accès non autorisé.'], 403);
        }

        $ligne->delete();

        // Recharger les lignes après suppression
        $panier->load('lignes');
        $count = $this->refreshPanierCount($panier);

        return response()->json([
            'success'      => true,
            'total'        => number_format($panier->lignes->sum('sous_total'), 2, ',', ' '),
            'panier_count' => $count,
        ]);
    }

    /**
     * GET /panier/count — Compteur panier pour le badge du header (AJAX).
     *
     * Retourne le nombre d'articles en session (pas de requete DB).
     *
     * @return \Illuminate\Http\JsonResponse
     */
    public function count(): \Illuminate\Http\JsonResponse
    {
        return response()->json(['count' => (int) session('panier_count', 0)]);
    }

    // -- Helpers privees -----------------------------------------------

    /** Recuperer le panier actif du client connecte avec ses lignes (ou null). */
    private function getPanierActif(): ?CommandeWeb
    {
        return CommandeWeb::where('id_client', session('client_id'))
            ->where('statut', 'panier')
            ->with('lignes.produit')
            ->first();
    }

    /** Recuperer ou creer le panier actif du client connecte. */
    private function getOrCreatePanier(): CommandeWeb
    {
        return CommandeWeb::firstOrCreate(
            ['id_client' => session('client_id'), 'statut' => 'panier'],
            ['total_ttc' => 0]
        );
    }

    /**
     * Rafraîchit le compteur panier en session depuis un panier déjà chargé.
     * Élimine les requêtes DB redondantes (anciennement getPanierCount).
     */
    private function refreshPanierCount(?CommandeWeb $panier = null): int
    {
        if (!$panier) {
            $panier = CommandeWeb::where('id_client', session('client_id'))
                ->where('statut', 'panier')
                ->first();
        }

        $count = $panier ? (int) $panier->lignes()->sum('quantite') : 0;
        session(['panier_count' => $count]);

        return $count;
    }
}
