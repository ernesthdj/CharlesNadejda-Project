<?php

namespace App\Http\Middleware;

use App\Models\Client;
use Closure;
use Illuminate\Http\Request;

/**
 * Middleware d'authentification client custom (pas de Laravel Auth guard).
 *
 * Verifie que le client est connecte (session client_id) et actif en base.
 * Cache le statut en session pendant 5 minutes pour eviter une requete DB a chaque page.
 * Si le compte est desactive entre-temps, le client est deconnecte au prochain check.
 */
class ClientAuth
{
    /**
     * Verifier l'authentification et le statut actif du client.
     *
     * @return \Illuminate\Http\RedirectResponse|mixed
     */
    public function handle(Request $request, Closure $next): mixed
    {
        if (!session()->has('client_id')) {
            return redirect()->route('login')
                ->with('error', 'Connectez-vous pour accéder à cette page.');
        }

        // Re-vérifier en DB toutes les 5 minutes (pas à chaque requête)
        $lastCheck = session('client_verified_at', 0);
        if (time() - $lastCheck > 300) {
            $client = Client::where('id', session('client_id'))
                ->where('actif', 1)
                ->first();

            if (!$client) {
                session()->flush();
                return redirect()->route('login')
                    ->with('error', 'Compte désactivé ou introuvable.');
            }

            session(['client_verified_at' => time()]);
        }

        return $next($request);
    }
}
