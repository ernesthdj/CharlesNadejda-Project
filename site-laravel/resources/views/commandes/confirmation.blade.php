@extends('layouts.app')
@section('title', 'Commande confirmee — ArtisaStock')

@section('content')
<div class="max-w-2xl mx-auto text-center">
    <div class="text-5xl mb-4">&#9989;</div>
    <h1 class="text-2xl font-display font-bold text-(--color-choco) mb-2">Commande confirmee !</h1>
    <p class="text-(--color-text-light) mb-8">Commande n&deg;{{ $commande->id }} &mdash; {{ $commande->date_commande?->format('d/m/Y H:i') }}</p>

    <div class="bg-white rounded-lg shadow p-6 text-left">
        @foreach($commande->lignes as $ligne)
            <div class="flex justify-between py-2 border-b border-(--color-border) last:border-0">
                <span>{{ $ligne->produit->nom_commercial }} <span class="text-(--color-text-light)">&times;{{ $ligne->quantite }}</span></span>
                <span class="font-semibold">{{ number_format($ligne->sous_total, 2, ',', ' ') }} &euro;</span>
            </div>
        @endforeach
        <div class="flex justify-between pt-4 text-lg font-bold">
            <span>Total</span>
            <span class="text-(--color-or)">{{ number_format($commande->total_ttc, 2, ',', ' ') }} &euro;</span>
        </div>
    </div>

    <div class="mt-8 flex justify-center gap-4">
        <a href="{{ route('commandes.historique') }}"
           class="bg-(--color-choco) text-(--color-creme) px-6 py-2.5 rounded font-semibold hover:brightness-125 transition-all">
            Voir mes commandes
        </a>
        <a href="{{ route('catalogue') }}"
           class="border border-(--color-choco) text-(--color-choco) px-6 py-2.5 rounded font-semibold hover:bg-(--color-creme) transition-all">
            Retour au catalogue
        </a>
    </div>
</div>
@endsection
