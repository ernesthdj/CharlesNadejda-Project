<?php

namespace App\Http\Controllers;

use App\Http\Requests\ProfilUpdateRequest;
use App\Models\Client;

class ProfilController extends Controller
{
    public function edit()
    {
        $client = Client::findOrFail(session('client_id'));

        return view('profil.edit', compact('client'));
    }

    public function update(ProfilUpdateRequest $request)
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
