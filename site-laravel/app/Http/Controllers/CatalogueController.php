<?php

namespace App\Http\Controllers;

use App\Models\CategorieWeb;
use App\Models\ProduitWeb;

// 📌 SCRIPT DEFENSE — Étape 6.1 : Catalogue — Eloquent with() eager loading, match() tri, scope stock
class CatalogueController extends Controller
{
    public function index()
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

    public function show(int $id)
    {
        $produit = ProduitWeb::where('id', $id)
            ->where('en_vente', 1)
            ->with('categorie')
            ->withStockDisponible()
            ->firstOrFail();

        return view('catalogue.show', compact('produit'));
    }
}
