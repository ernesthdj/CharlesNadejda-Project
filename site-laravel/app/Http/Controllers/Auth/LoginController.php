<?php

namespace App\Http\Controllers\Auth;

use App\Http\Controllers\Controller;
use App\Models\Client;
use App\Models\CommandeWeb;
use Illuminate\Http\Request;

// 📌 SCRIPT DEFENSE — Étape 6.3 : Login — password_verify, message générique, session regenerate
class LoginController extends Controller
{
    public function showForm()
    {
        if (session()->has('client_id')) {
            return redirect()->route('catalogue');
        }

        return view('auth.login');
    }

    public function login(Request $request)
    {
        $request->validate([
            'email'    => 'required|email',
            'password' => 'required|string',
        ]);

        $client = Client::where('email', $request->email)
            ->where('actif', 1)
            ->first();

        if (!$client || !password_verify($request->password, $client->mot_de_passe)) {
            return back()
                ->withInput($request->only('email'))
                ->with('error', 'Email ou mot de passe incorrect.');
        }

        // Charger le compteur panier existant pour le cache session
        $panier = CommandeWeb::where('id_client', $client->id)
            ->where('statut', 'panier')
            ->first();
        $panierCount = $panier ? (int) $panier->lignes()->sum('quantite') : 0;

        session([
            'client_id'          => $client->id,
            'client_nom'         => $client->nom,
            'client_prenom'      => $client->prenom,
            'panier_count'       => $panierCount,
            'client_verified_at' => time(),
        ]);
        session()->regenerate();

        return redirect()->intended(route('catalogue'))
            ->with('success', 'Bon retour, ' . $client->prenom . ' !');
    }

    public function logout()
    {
        session()->flush();

        return redirect()->route('catalogue')
            ->with('success', 'Vous avez été déconnecté.');
    }
}
