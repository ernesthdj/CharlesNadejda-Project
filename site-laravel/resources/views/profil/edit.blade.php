@extends('layouts.app')
@section('title', 'Mon profil — ArtisaStock')

@section('content')
<div class="max-w-md mx-auto">
    <h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6 text-center">Mon profil</h1>

    <form method="POST" action="{{ route('profil.update') }}" class="bg-white rounded-lg shadow p-8 space-y-4">
        @csrf
        @method('PUT')

        <div>
            <label class="block text-sm font-medium mb-1">Prenom <span class="text-red-500">*</span></label>
            <input type="text" name="prenom" value="{{ old('prenom', $client->prenom) }}" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('prenom') ? 'border-red-500' : 'border-gray-300' }}">
            @error('prenom') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Nom <span class="text-red-500">*</span></label>
            <input type="text" name="nom" value="{{ old('nom', $client->nom) }}" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('nom') ? 'border-red-500' : 'border-gray-300' }}">
            @error('nom') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Email</label>
            <input type="email" value="{{ $client->email }}" disabled
                   class="w-full px-3 py-2 border border-gray-200 rounded bg-gray-50 text-gray-500">
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Telephone</label>
            <input type="text" name="telephone" value="{{ old('telephone', $client->telephone) }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Rue</label>
            <input type="text" name="adresse_rue" value="{{ old('adresse_rue', $client->adresse_rue) }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <div class="grid grid-cols-3 gap-3">
            <div>
                <label class="block text-sm font-medium mb-1">CP</label>
                <input type="text" name="adresse_cp" value="{{ old('adresse_cp', $client->adresse_cp) }}"
                       class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
            </div>
            <div class="col-span-2">
                <label class="block text-sm font-medium mb-1">Ville</label>
                <input type="text" name="adresse_ville" value="{{ old('adresse_ville', $client->adresse_ville) }}"
                       class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
            </div>
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Pays</label>
            <input type="text" name="adresse_pays" value="{{ old('adresse_pays', $client->adresse_pays) }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <hr class="border-(--color-border)">

        <div>
            <label class="block text-sm font-medium mb-1">Nouveau mot de passe <span class="text-(--color-text-light) text-xs">(laisser vide pour ne pas changer)</span></label>
            <input type="password" name="password"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('password') ? 'border-red-500' : '' }}">
            @error('password') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label class="block text-sm font-medium mb-1">Confirmer le nouveau mot de passe</label>
            <input type="password" name="password_confirmation"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <button type="submit"
                class="w-full bg-(--color-choco) text-(--color-creme) py-3 rounded font-semibold
                       hover:brightness-125 transition-all text-base">
            Mettre a jour
        </button>
    </form>
</div>
@endsection
