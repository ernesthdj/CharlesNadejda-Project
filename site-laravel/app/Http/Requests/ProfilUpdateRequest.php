<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

/**
 * Validation du formulaire de mise a jour du profil client.
 *
 * Regles : prenom et nom obligatoires, mot de passe optionnel (8 chars min si renseigne).
 * Pas de modification d'email (champ non editable cote formulaire).
 */
class ProfilUpdateRequest extends FormRequest
{
    /** Toujours autorise — le middleware ClientAuth protege deja la route. */
    public function authorize(): bool
    {
        return true;
    }

    /** @return array<string, string> Regles de validation par champ. */
    public function rules(): array
    {
        return [
            'prenom'        => 'required|string|max:100',
            'nom'           => 'required|string|max:100',
            'telephone'     => 'nullable|string|max:20',
            'adresse_rue'   => 'nullable|string|max:255',
            'adresse_cp'    => 'nullable|string|max:10',
            'adresse_ville' => 'nullable|string|max:100',
            'adresse_pays'  => 'nullable|string|max:100',
            'password'      => 'nullable|string|min:8|confirmed',
        ];
    }

    /** @return array<string, string> Messages d'erreur personnalises en francais. */
    public function messages(): array
    {
        return [
            'prenom.required'    => 'Le prénom est obligatoire.',
            'nom.required'       => 'Le nom est obligatoire.',
            'password.min'       => 'Le mot de passe doit contenir au moins 8 caractères.',
            'password.confirmed' => 'Les mots de passe ne correspondent pas.',
        ];
    }
}
