<?php

namespace App\Http\Controllers;

use App\Http\Requests\ProfilUpdateRequest;
use App\Models\Client;

/**
 * Gestion du profil client — consultation et mise a jour des informations personnelles.
 */
class ProfilController extends Controller
{
    /**
     * GET /profil — Afficher le formulaire d'edition du profil.
     *
     * @return \Illuminate\Contracts\View\View
     */
    public function edit(): \Illuminate\Contracts\View\View
    {
        $client = Client::findOrFail(session('client_id'));

        return view('profil.edit', compact('client'));
    }

    /**
     * PUT /profil — Mettre a jour les informations du profil client.
     *
     * Met a jour nom, prenom, adresse, telephone. Le mot de passe n'est
     * modifie que s'il est renseigne (champ optionnel).
     * La session est synchronisee apres modification.
     *
     * @return \Illuminate\Http\RedirectResponse
     */
    public function update(ProfilUpdateRequest $request): \Illuminate\Http\RedirectResponse
    {
        $client = Client::findOrFail(session('client_id'));

        $client->prenom       = $request->prenom;
        $client->nom          = $request->nom;
        $client->telephone    = $request->telephone;
        $client->adresse_rue  = $request->adresse_rue;
        $client->adresse_cp   = $request->adresse_cp;
        $client->adresse_ville = $request->adresse_ville;
        $client->adresse_pays = $request->adresse_pays ?? 'Belgique';

        if ($request->filled('password')) {
            $client->mot_de_passe = password_hash($request->password, PASSWORD_BCRYPT);
        }

        $client->save();

        // Mettre à jour la session
        session([
            'client_nom'    => $client->nom,
            'client_prenom' => $client->prenom,
        ]);

        return back()->with('success', 'Profil mis à jour avec succès.');
    }
}
