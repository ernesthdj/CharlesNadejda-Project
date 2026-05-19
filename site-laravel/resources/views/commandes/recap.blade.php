@extends('layouts.app')
@section('title', 'Recapitulatif — ArtisaStock')

@section('content')
<h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6">Recapitulatif de commande</h1>

<div class="grid grid-cols-1 md:grid-cols-3 gap-8">
    {{-- Articles --}}
    <div class="md:col-span-2">
        <div class="bg-white rounded-lg shadow p-6">
            <h2 class="font-semibold text-lg mb-4">Articles</h2>
            @foreach($panier->lignes as $ligne)
                <div class="flex justify-between py-2 border-b border-(--color-border) last:border-0">
                    <span>{{ $ligne->produit->nom_commercial }} <span class="text-(--color-text-light)">&times;{{ $ligne->quantite }}</span></span>
                    <span class="font-semibold">{{ number_format($ligne->sous_total, 2, ',', ' ') }} &euro;</span>
                </div>
            @endforeach
            <div class="flex justify-between pt-4 text-lg font-bold">
                <span>Total</span>
                <span class="text-(--color-or)">{{ number_format($panier->lignes->sum('sous_total'), 2, ',', ' ') }} &euro;</span>
            </div>
        </div>
    </div>

    {{-- Adresse + Paiement --}}
    <div>
        <form method="POST" action="{{ route('commande.valider') }}" class="space-y-6">
            @csrf

            <div class="bg-white rounded-lg shadow p-6">
                <h2 class="font-semibold text-lg mb-4">Adresse de livraison</h2>
                <div class="space-y-3">
                    <input type="text" name="adresse_rue" value="{{ old('adresse_rue', $client->adresse_rue) }}"
                           placeholder="Rue" class="w-full px-3 py-2 border border-gray-300 rounded text-sm">
                    <div class="grid grid-cols-3 gap-2">
                        <input type="text" name="adresse_cp" value="{{ old('adresse_cp', $client->adresse_cp) }}"
                               placeholder="CP" class="px-3 py-2 border border-gray-300 rounded text-sm">
                        <input type="text" name="adresse_ville" value="{{ old('adresse_ville', $client->adresse_ville) }}"
                               placeholder="Ville" class="col-span-2 px-3 py-2 border border-gray-300 rounded text-sm">
                    </div>
                    <input type="text" name="adresse_pays" value="{{ old('adresse_pays', $client->adresse_pays ?? 'Belgique') }}"
                           placeholder="Pays" class="w-full px-3 py-2 border border-gray-300 rounded text-sm">
                </div>
            </div>

            <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-blue-800">
                <strong>Simulation de paiement</strong> — Aucun paiement reel ne sera effectue.
            </div>

            <button type="submit"
                    class="w-full bg-(--color-choco) text-(--color-creme) py-3 rounded font-semibold
                           hover:brightness-125 transition-all text-base cursor-pointer">
                Simuler le paiement
            </button>
        </form>
    </div>
</div>
@endsection
