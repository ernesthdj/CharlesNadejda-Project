<?php

namespace App\Http\Middleware;

use App\Models\Client;
use Closure;
use Illuminate\Http\Request;

class ClientAuth
{
    public function handle(Request $request, Closure $next)
    {
        if (!session()->has('client_id')) {
            return redirect()->route('login')
                ->with('error', 'Connectez-vous pour accéder à cette page.');
        }

        $client = Client::where('id', session('client_id'))
            ->where('actif', 1)
            ->first();

        if (!$client) {
            session()->flush();
            return redirect()->route('login')
                ->with('error', 'Compte désactivé ou introuvable.');
        }

        return $next($request);
    }
}
