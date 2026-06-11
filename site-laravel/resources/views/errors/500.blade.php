@extends('layouts.app')
@section('title', 'Erreur serveur — ArtisaStock')

@section('content')
<div class="flex flex-col items-center justify-center py-20 text-center">

    {{-- Code d'erreur stylise --}}
    <p class="text-8xl font-display font-bold text-(--color-or) mb-4">500</p>

    {{-- Titre --}}
    <h1 class="text-2xl font-display font-bold text-(--color-choco) mb-3">
        Erreur serveur
    </h1>

    {{-- Message explicatif --}}
    <p class="text-(--color-text-light) max-w-md mb-8 leading-relaxed">
        Une erreur inattendue s'est produite. Notre equipe en est informee.
        Veuillez reessayer dans quelques instants.
    </p>

    {{-- Bouton retour --}}
    <a href="{{ route('catalogue') }}"
       class="inline-block bg-(--color-choco) text-(--color-creme) px-6 py-3 rounded font-semibold
              hover:brightness-125 transition-all text-base">
        Retour a la boutique
    </a>
</div>
@endsection
