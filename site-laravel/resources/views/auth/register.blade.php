@extends('layouts.app')
@section('title', 'Inscription — ArtisaStock')

@section('content')
<div class="max-w-md mx-auto">
    <h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6 text-center">Creer votre compte</h1>

    <form method="POST" action="{{ route('register') }}" class="bg-white rounded-lg shadow p-8 space-y-4">
        @csrf

        <div>
            <label for="prenom" class="block text-sm font-medium mb-1">Prenom <span class="text-red-500">*</span></label>
            <input type="text" id="prenom" name="prenom" value="{{ old('prenom') }}" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('prenom') ? 'border-red-500' : 'border-gray-300' }}">
            @error('prenom') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label for="nom" class="block text-sm font-medium mb-1">Nom <span class="text-red-500">*</span></label>
            <input type="text" id="nom" name="nom" value="{{ old('nom') }}" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('nom') ? 'border-red-500' : 'border-gray-300' }}">
            @error('nom') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label for="email" class="block text-sm font-medium mb-1">Email <span class="text-red-500">*</span></label>
            <input type="email" id="email" name="email" value="{{ old('email') }}" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('email') ? 'border-red-500' : 'border-gray-300' }}">
            @error('email') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label for="password" class="block text-sm font-medium mb-1">Mot de passe <span class="text-red-500">*</span></label>
            <input type="password" id="password" name="password" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none
                          {{ $errors->has('password') ? 'border-red-500' : 'border-gray-300' }}">
            @error('password') <p class="mt-1 text-sm text-red-600">{{ $message }}</p> @enderror
        </div>

        <div>
            <label for="password_confirmation" class="block text-sm font-medium mb-1">Confirmer <span class="text-red-500">*</span></label>
            <input type="password" id="password_confirmation" name="password_confirmation" required
                   class="w-full px-3 py-2 border rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <hr class="border-(--color-border)">

        <div>
            <label for="telephone" class="block text-sm font-medium mb-1">Telephone</label>
            <input type="text" id="telephone" name="telephone" value="{{ old('telephone') }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <div>
            <label for="adresse_rue" class="block text-sm font-medium mb-1">Rue</label>
            <input type="text" id="adresse_rue" name="adresse_rue" value="{{ old('adresse_rue') }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <div class="grid grid-cols-3 gap-3">
            <div>
                <label for="adresse_cp" class="block text-sm font-medium mb-1">Code postal</label>
                <input type="text" id="adresse_cp" name="adresse_cp" value="{{ old('adresse_cp') }}"
                       class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
            </div>
            <div class="col-span-2">
                <label for="adresse_ville" class="block text-sm font-medium mb-1">Ville</label>
                <input type="text" id="adresse_ville" name="adresse_ville" value="{{ old('adresse_ville') }}"
                       class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
            </div>
        </div>

        <div>
            <label for="adresse_pays" class="block text-sm font-medium mb-1">Pays</label>
            <input type="text" id="adresse_pays" name="adresse_pays" value="{{ old('adresse_pays', 'Belgique') }}"
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <button type="submit"
                class="w-full bg-(--color-choco) text-(--color-creme) py-3 rounded font-semibold
                       hover:brightness-125 transition-all text-base">
            Creer mon compte
        </button>

        <p class="text-center text-sm text-(--color-text-light)">
            Deja un compte ? <a href="{{ route('login') }}" class="text-(--color-or) font-semibold hover:underline">Se connecter</a>
        </p>
    </form>
</div>
@endsection
