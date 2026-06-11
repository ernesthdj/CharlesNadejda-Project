<?php

namespace App\Http\Controllers\Auth;

use App\Http\Controllers\Controller;
use App\Http\Requests\RegisterRequest;
use App\Models\Client;

/**
 * Inscription client — creation de compte sur la boutique.
 *
 * Le mot de passe est hashe en BCrypt (compatible avec l'ERP C#).
 * La session est initialisee et regeneree immediatement apres inscription.
 */
class RegisterController extends Controller
{
    /**
     * GET /register — Afficher le formulaire d'inscription.
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

        return view('auth.register');
    }

    /**
     * POST /register — Creer un nouveau compte client.
     *
     * Validation via RegisterRequest (FormRequest).
     * Hash BCrypt du mot de passe, creation en DB, puis auto-login.
     *
     * @return \Illuminate\Http\RedirectResponse
     */
    public function register(RegisterRequest $request): \Illuminate\Http\RedirectResponse
    {
        $client = Client::create([
            'prenom'       => $request->prenom,
            'nom'          => $request->nom,
            'email'        => $request->email,
            'mot_de_passe' => password_hash($request->password, PASSWORD_BCRYPT),
            'telephone'    => $request->telephone,
            'adresse_rue'  => $request->adresse_rue,
            'adresse_cp'   => $request->adresse_cp,
            'adresse_ville' => $request->adresse_ville,
            'adresse_pays' => $request->adresse_pays ?? 'Belgique',
        ]);

        session([
            'client_id'          => $client->id,
            'client_nom'         => $client->nom,
            'client_prenom'      => $client->prenom,
            'panier_count'       => 0,
            'client_verified_at' => time(),
        ]);
        session()->regenerate();

        return redirect()->route('catalogue')
            ->with('success', 'Bienvenue ' . $client->prenom . ' ! Votre compte a été créé.');
    }
}
