<?php

namespace App\Http\Controllers;

use App\Models\CategorieWeb;
use App\Models\ProduitWeb;

/**
 * Catalogue public de la boutique — listing et detail des produits en vente.
 *
 * Les produits sont charges avec eager loading (categorie) et le scope
 * withStockDisponible() pour eviter les N+1 sur le calcul de stock.
 */
class CatalogueController extends Controller
{
    /**
     * GET / — Afficher le catalogue avec filtrage par categorie et tri.
     *
     * Filtres query string : ?categorie={id}&tri=prix_asc|prix_desc|defaut
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function index(): \Illuminate\Contracts\View\View
    {
        $query = ProduitWeb::where('en_vente', 1)
            ->with('categorie')
            ->withStockDisponible();

        // Filtre par catégorie
        if (request('categorie')) {
            $query->where('id_categorie', request('categorie'));
        }

        // Tri
        $tri = request('tri', 'defaut');
        match ($tri) {
            'prix_asc'  => $query->orderBy('prix_vente', 'asc'),
            'prix_desc' => $query->orderBy('prix_vente', 'desc'),
            default     => $query->orderBy('ordre_affichage')->orderBy('nom_commercial'),
        };

        $produits   = $query->get();
        $categories = CategorieWeb::where('actif', 1)
            ->orderBy('ordre_affichage')
            ->get();

        return view('catalogue.index', compact('produits', 'categories', 'tri'));
    }

    /**
     * GET /produit/{id} — Afficher la fiche detail d'un produit en vente.
     *
     * Retourne 404 si le produit n'existe pas ou n'est pas en vente.
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function show(int $id): \Illuminate\Contracts\View\View
    {
        $produit = ProduitWeb::where('id', $id)
            ->where('en_vente', 1)
            ->with('categorie')
            ->withStockDisponible()
            ->firstOrFail();

        return view('catalogue.show', compact('produit'));
    }
}
