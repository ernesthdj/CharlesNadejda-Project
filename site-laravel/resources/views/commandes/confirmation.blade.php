@extends('layouts.app')
@section('title', 'Commande confirmée — ArtisaStock')

@section('content')
<div class="max-w-2xl mx-auto">
    {{-- Bannière succès paiement --}}
    <div class="bg-green-50 border border-green-200 rounded-lg p-6 text-center mb-8">
        <div class="text-5xl mb-3">&#9989;</div>
        <h1 class="text-2xl font-display font-bold text-green-800 mb-1">Paiement confirmé</h1>
        <p class="text-green-700 text-sm">
            Votre paiement de
            <span class="font-bold">{{ number_format($commande->total_ttc, 2, ',', ' ') }} &euro;</span>
            a été accepté avec succès.
        </p>
        <p class="text-(--color-text-light) text-xs mt-2">
            Commande n&deg;{{ $commande->id }} &mdash; {{ $commande->date_commande?->format('d/m/Y à H:i') }}
        </p>
    </div>

    {{-- Détail des articles --}}
    <div class="bg-white rounded-lg shadow p-6 mb-6">
        <h2 class="font-semibold text-lg mb-4 text-(--color-choco)">Récapitulatif de votre commande</h2>

        <div class="divide-y divide-(--color-border)">
            @foreach($commande->lignes as $ligne)
                <div class="flex justify-between items-center py-3">
                    <div>
                        <span class="font-medium">{{ $ligne->produit->nom_commercial }}</span>
                        <span class="text-(--color-text-light) text-sm ml-2">&times; {{ $ligne->quantite }}</span>
                    </div>
                    <span class="font-semibold">{{ number_format($ligne->sous_total, 2, ',', ' ') }} &euro;</span>
                </div>
            @endforeach
        </div>

        <div class="flex justify-between pt-4 mt-2 border-t-2 border-(--color-choco)/20 text-lg font-bold">
            <span>Total TTC</span>
            <span class="text-(--color-or)">{{ number_format($commande->total_ttc, 2, ',', ' ') }} &euro;</span>
        </div>
    </div>

    {{-- Confirmation décompte stock --}}
    <div class="bg-blue-50 border border-blue-200 rounded-lg p-5 mb-6">
        <div class="flex items-start gap-3">
            <span class="text-xl">&#128230;</span>
            <div class="text-sm text-blue-800">
                <p class="font-semibold mb-1">Stock mis à jour</p>
                <p>
                    Les quantités commandées ont été décomptées du stock selon la méthode
                    <strong>FIFO</strong> (First In, First Out — les lots les plus anciens sont consommés en premier).
                </p>
                <ul class="mt-2 space-y-1">
                    @foreach($commande->lignes as $ligne)
                        <li>&minus; {{ $ligne->quantite }} &times; {{ $ligne->produit->nom_commercial }}</li>
                    @endforeach
                </ul>
            </div>
        </div>
    </div>

    @if($commande->adresse_livraison)
        <div class="bg-white rounded-lg shadow p-5 mb-6">
            <h3 class="font-semibold text-sm text-(--color-text-light) uppercase tracking-wide mb-2">Adresse de livraison</h3>
            <p class="text-sm">{{ $commande->adresse_livraison }}</p>
        </div>
    @endif

    {{-- Actions --}}
    <div class="flex justify-center gap-4 mt-8">
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
