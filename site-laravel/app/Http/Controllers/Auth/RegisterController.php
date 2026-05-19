<?php

namespace App\Http\Controllers\Auth;

use App\Http\Controllers\Controller;
use App\Http\Requests\RegisterRequest;
use App\Models\Client;

class RegisterController extends Controller
{
    public function showForm()
    {
        if (session()->has('client_id')) {
            return redirect()->route('catalogue');
        }

        return view('auth.register');
    }

    public function register(RegisterRequest $request)
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
            'client_id'     => $client->id,
            'client_nom'    => $client->nom,
            'client_prenom' => $client->prenom,
        ]);
        session()->regenerate();

        return redirect()->route('catalogue')
            ->with('success', 'Bienvenue ' . $client->prenom . ' ! Votre compte a été créé.');
    }
}
