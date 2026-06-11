<?php

namespace App\Http\Controllers\Auth;

use App\Http\Controllers\Controller;
use App\Models\Client;
use App\Models\CommandeWeb;
use Illuminate\Http\Request;

/**
 * Authentification client — connexion et deconnexion.
 *
 * Utilise password_verify() contre le hash BCrypt stocke en base.
 * Message d'erreur generique (pas de distinction email/password) pour la securite.
 * Session regeneree apres connexion pour prevenir le session fixation.
 */
class LoginController extends Controller
{
    /**
     * GET /login — Afficher le formulaire de connexion.
     *
     * Redirige vers le catalogue si le client est deja connecte.
     *
     * @return \Illuminate\Contracts\View\View|\Illuminate\Http\RedirectResponse
     */
    public function showForm(): \Illuminate\Contracts\View\View|\Illuminate\Http\RedirectResponse
    {
        if (session()->has('client_id')) {
            return redirect()->route('catalogue');
        }

        return view('auth.login');
    }

    /**
     * POST /login — Authentifier le client.
     *
     * Verifie email + mot de passe BCrypt + compte actif.
     * Initialise la session avec les infos client et le compteur panier.
     *
     * @return \Illuminate\Http\RedirectResponse
     */
    public function login(Request $request): \Illuminate\Http\RedirectResponse
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

    /**
     * POST /logout — Deconnecter le client et vider la session.
     *
     * @return \Illuminate\Http\RedirectResponse
     */
    public function logout(): \Illuminate\Http\RedirectResponse
    {
        session()->flush();

        return redirect()->route('catalogue')
            ->with('success', 'Vous avez été déconnecté.');
    }
}
