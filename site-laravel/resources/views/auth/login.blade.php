@extends('layouts.app')
@section('title', 'Connexion — ArtisaStock')

@section('content')
<div class="max-w-sm mx-auto">
    <h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6 text-center">Connexion</h1>

    <form method="POST" action="{{ route('login') }}" class="bg-white rounded-lg shadow p-8 space-y-4">
        @csrf

        <div>
            <label for="email" class="block text-sm font-medium mb-1">Email <span class="text-red-500">*</span></label>
            <input type="email" id="email" name="email" value="{{ old('email') }}" required autofocus
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <div>
            <label for="password" class="block text-sm font-medium mb-1">Mot de passe <span class="text-red-500">*</span></label>
            <input type="password" id="password" name="password" required
                   class="w-full px-3 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-(--color-or) outline-none">
        </div>

        <button type="submit"
                class="w-full bg-(--color-choco) text-(--color-creme) py-3 rounded font-semibold
                       hover:brightness-125 transition-all text-base">
            Se connecter
        </button>

        <p class="text-center text-sm text-(--color-text-light)">
            Pas encore de compte ? <a href="{{ route('register') }}" class="text-(--color-or) font-semibold hover:underline">S'inscrire</a>
        </p>
    </form>
</div>
@endsection
