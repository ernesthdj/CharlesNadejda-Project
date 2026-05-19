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
        // QA-01 : View Composer pour le compteur panier dans le header
        View::composer('components.header', function ($view) {
            $count = 0;
            if (session()->has('client_id')) {
                $panier = CommandeWeb::where('id_client', session('client_id'))
                    ->where('statut', 'panier')
                    ->first();
                $count = $panier ? (int) $panier->lignes()->sum('quantite') : 0;
            }
            $view->with('panierCount', $count);
        });
    }
}
