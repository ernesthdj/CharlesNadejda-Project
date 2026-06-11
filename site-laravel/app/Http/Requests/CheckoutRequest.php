<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

/**
 * Validation du formulaire de checkout (validation commande).
 * Tous les champs sont optionnels car le client peut commander en retrait.
 */
class CheckoutRequest extends FormRequest
{
    public function authorize(): bool
    {
        return session()->has('client_id');
    }

    public function rules(): array
    {
        return [
            'adresse_rue'   => 'nullable|string|max:255',
            'adresse_cp'    => 'nullable|string|max:10',
            'adresse_ville' => 'nullable|string|max:100',
            'adresse_pays'  => 'nullable|string|max:100',
        ];
    }
}
