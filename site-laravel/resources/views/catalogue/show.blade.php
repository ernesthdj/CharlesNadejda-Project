@extends('layouts.app')
@section('title', $produit->nom_commercial . ' — ArtisaStock')

@section('content')
<a href="{{ route('catalogue') }}" class="text-sm text-(--color-text-light) hover:text-(--color-or) mb-4 inline-block">&larr; Retour au catalogue</a>

<div class="grid grid-cols-1 md:grid-cols-5 gap-8">
    {{-- Image (3/5) --}}
    <div class="md:col-span-3 rounded-lg overflow-hidden bg-(--color-creme)">
        @if($produit->image_path)
            <img src="{{ asset('storage/' . $produit->image_path) }}" alt="{{ $produit->nom_commercial }}"
                 class="w-full object-cover rounded-lg">
        @else
            <div class="w-full aspect-[4/3] flex items-center justify-center text-7xl bg-(--color-choco)/10">&#127851;</div>
        @endif
    </div>

    {{-- Infos (2/5) --}}
    <div class="md:col-span-2 flex flex-col">
        @if($produit->categorie)
            <span class="text-sm text-(--color-text-light) uppercase tracking-wide">{{ $produit->categorie->nom }}</span>
        @endif

        <h1 class="text-2xl font-display font-bold text-(--color-choco) mt-1">{{ $produit->nom_commercial }}</h1>

        @if($produit->description)
            <p class="text-(--color-text-light) mt-4 leading-relaxed">{{ $produit->description }}</p>
        @endif

        <div class="mt-6">
            <span class="text-3xl font-bold text-(--color-or)">{{ number_format($produit->prix_vente, 2, ',', ' ') }} &euro;</span>
        </div>

        <div class="mt-3">
            @if($produit->en_stock)
                <span class="inline-flex items-center gap-2 text-sm font-medium text-green-700">
                    <span class="w-2.5 h-2.5 bg-green-500 rounded-full"></span>
                    {{ number_format($produit->stock_disponible, 0) }} disponible(s)
                </span>
            @else
                <span class="inline-flex items-center gap-2 text-sm font-medium text-red-700">
                    <span class="w-2.5 h-2.5 bg-red-500 rounded-full"></span>
                    Rupture de stock
                </span>
            @endif
        </div>

        @if(session('client_id') && $produit->en_stock)
            <div class="mt-6 flex items-center gap-3">
                <label for="qte" class="text-sm font-medium">Quantite :</label>
                <input type="number" id="qte" value="1" min="1" max="{{ intval($produit->stock_disponible) }}"
                       class="w-20 px-3 py-2 border border-gray-300 rounded text-center focus:ring-2 focus:ring-(--color-or) outline-none">
            </div>
            <button onclick="ajouterAuPanier({{ $produit->id }}, parseInt(document.getElementById('qte').value))"
                    class="mt-4 w-full bg-(--color-choco) text-(--color-creme) py-3 rounded font-semibold
                           hover:brightness-125 transition-all text-base cursor-pointer">
                Ajouter au panier
            </button>
        @elseif(!session('client_id'))
            <a href="{{ route('login') }}"
               class="mt-6 block text-center bg-(--color-or) text-(--color-choco) py-3 rounded font-semibold
                      hover:brightness-110 transition-all">
                Connectez-vous pour commander
            </a>
        @else
            <button disabled class="mt-6 w-full bg-gray-300 text-gray-500 py-3 rounded font-semibold cursor-not-allowed">
                Indisponible
            </button>
        @endif
    </div>
</div>
@endsection

@push('scripts')
<script src="{{ asset('js/panier.js') }}"></script>
@endpush
