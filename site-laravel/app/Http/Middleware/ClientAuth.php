<?php

namespace App\Http\Middleware;

use App\Models\Client;
use Closure;
use Illuminate\Http\Request;

// 📌 SCRIPT DEFENSE — Étape 6.4 : Middleware — session check + vérif client actif en base
class ClientAuth
{
    public function handle(Request $request, Closure $next)
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
