<?php

namespace App\Providers;

use App\Models\CommandeWeb;
use Illuminate\Support\Facades\View;
use Illuminate\Support\ServiceProvider;

/**
 * Service provider principal de l'application ArtisaStock web.
 *
 * Configure le View Composer pour le compteur panier dans le header.
 */
class AppServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        //
    }

    /**
     * Initialisation des services applicatifs.
     *
     * Enregistre un View Composer sur le header pour injecter le compteur panier
     * depuis la session (evite 2 requetes DB par page pour les utilisateurs connectes).
     */
    public function boot(): void
    {
        View::composer('components.header', function ($view) {
            $view->with('panierCount', (int) session('panier_count', 0));
        });
    }
}
