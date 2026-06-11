<?php

namespace App\Providers;

use App\Models\CommandeWeb;
use Illuminate\Support\Facades\View;
use Illuminate\Support\ServiceProvider;

class AppServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        //
    }

    public function boot(): void
    {
        // Compteur panier via session (mis à jour par PanierController)
        // Élimine 2 requêtes DB par page pour les utilisateurs connectés
        View::composer('components.header', function ($view) {
            $view->with('panierCount', (int) session('panier_count', 0));
        });
    }
}
