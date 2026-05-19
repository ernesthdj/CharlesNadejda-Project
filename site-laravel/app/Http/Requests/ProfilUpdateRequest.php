<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class ProfilUpdateRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

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
