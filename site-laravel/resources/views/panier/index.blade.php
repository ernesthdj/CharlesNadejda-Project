@extends('layouts.app')
@section('title', 'Mon panier — ArtisaStock')

@section('content')
<h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6">Mon panier</h1>

@if(!$panier || $panier->lignes->isEmpty())
    <div class="text-center py-12">
        <p class="text-(--color-text-light) text-lg mb-4">Votre panier est vide.</p>
        <a href="{{ route('catalogue') }}" class="text-(--color-or) font-semibold hover:underline">Decouvrez notre catalogue &rarr;</a>
    </div>
@else
    <div class="bg-white rounded-lg shadow overflow-hidden">
        @foreach($panier->lignes as $ligne)
            <div class="flex items-center gap-4 p-4 border-b border-(--color-border) transition-opacity duration-300"
                 data-ligne="{{ $ligne->id }}">
                {{-- Image mini --}}
                <div class="w-16 h-16 rounded overflow-hidden bg-(--color-creme) shrink-0">
                    @if($ligne->produit->image_path)
                        <img src="{{ asset('storage/' . $ligne->produit->image_path) }}" class="w-full h-full object-cover">
                    @else
                        <div class="w-full h-full flex items-center justify-center text-2xl">&#127851;</div>
                    @endif
                </div>

                {{-- Nom --}}
                <div class="flex-1">
                    <h3 class="font-semibold">{{ $ligne->produit->nom_commercial }}</h3>
                    <p class="text-sm text-(--color-text-light)">{{ number_format($ligne->prix_unitaire, 2, ',', ' ') }} &euro; / unite</p>
                </div>

                {{-- Quantite --}}
                <div class="flex items-center gap-2">
                    <button onclick="updateQuantite({{ $ligne->id }}, {{ $ligne->quantite - 1 }})"
                            class="w-8 h-8 rounded bg-gray-200 hover:bg-gray-300 text-lg font-bold
                                   disabled:opacity-50 disabled:cursor-not-allowed"
                            {{ $ligne->quantite <= 1 ? 'disabled' : '' }}>&minus;</button>
                    <span class="w-8 text-center font-semibold">{{ $ligne->quantite }}</span>
                    <button onclick="updateQuantite({{ $ligne->id }}, {{ $ligne->quantite + 1 }})"
                            class="w-8 h-8 rounded bg-gray-200 hover:bg-gray-300 text-lg font-bold">+</button>
                </div>

                {{-- Sous-total --}}
                <div class="w-24 text-right">
                    <span class="font-bold text-(--color-or) sous-total">{{ number_format($ligne->sous_total, 2, ',', ' ') }} &euro;</span>
                </div>

                {{-- Supprimer --}}
                <button onclick="supprimerDuPanier({{ $ligne->id }})"
                        class="text-red-400 hover:text-red-600 text-lg cursor-pointer" title="Supprimer">&#128465;</button>
            </div>
        @endforeach
    </div>

    <div class="mt-6 flex items-center justify-between">
        <span class="text-lg font-semibold">Total :</span>
        <span class="text-2xl font-bold text-(--color-or)" id="total-panier">
            {{ number_format($panier->lignes->sum('sous_total'), 2, ',', ' ') }} &euro;
        </span>
    </div>

    <div class="mt-6 text-right">
        <a href="{{ route('commande.recap') }}"
           class="inline-block bg-(--color-choco) text-(--color-creme) px-8 py-3 rounded font-semibold
                  hover:brightness-125 transition-all text-base">
            Passer commande &rarr;
        </a>
    </div>
@endif
@endsection

@push('scripts')
<script src="{{ asset('js/panier.js') }}"></script>
@endpush
