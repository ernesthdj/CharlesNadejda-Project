<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class RegisterRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'prenom'       => 'required|string|max:100',
            'nom'          => 'required|string|max:100',
            'email'        => 'required|email|max:255|unique:clients,email',
            'password'     => 'required|string|min:8|confirmed',
            'telephone'    => 'nullable|string|max:20',
            'adresse_rue'  => 'nullable|string|max:255',
            'adresse_cp'   => 'nullable|string|max:10',
            'adresse_ville' => 'nullable|string|max:100',
            'adresse_pays' => 'nullable|string|max:100',
        ];
    }

    public function messages(): array
    {
        return [
            'prenom.required'    => 'Le prénom est obligatoire.',
            'nom.required'       => 'Le nom est obligatoire.',
            'email.required'     => 'L\'adresse email est obligatoire.',
            'email.email'        => 'L\'adresse email n\'est pas valide.',
            'email.unique'       => 'Cette adresse email est déjà utilisée.',
            'password.required'  => 'Le mot de passe est obligatoire.',
            'password.min'       => 'Le mot de passe doit contenir au moins 8 caractères.',
            'password.confirmed' => 'Les mots de passe ne correspondent pas.',
        ];
    }
}
