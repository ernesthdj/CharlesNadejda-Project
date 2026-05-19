@extends('layouts.app')
@section('title', 'Catalogue — ArtisaStock')

@section('content')
<h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6">Notre catalogue</h1>

{{-- Filtres categories --}}
<div class="flex flex-wrap gap-2 mb-6">
    <a href="{{ route('catalogue') }}"
       class="px-4 py-2 rounded-full text-sm font-medium transition-all
              {{ !request('categorie') ? 'bg-(--color-choco) text-(--color-creme)' : 'bg-white text-(--color-choco) border border-(--color-border) hover:bg-(--color-creme)' }}">
        Tous
    </a>
    @foreach($categories as $cat)
        <a href="{{ route('catalogue', ['categorie' => $cat->id, 'tri' => $tri]) }}"
           class="px-4 py-2 rounded-full text-sm font-medium transition-all
                  {{ request('categorie') == $cat->id ? 'bg-(--color-choco) text-(--color-creme)' : 'bg-white text-(--color-choco) border border-(--color-border) hover:bg-(--color-creme)' }}">
            {{ $cat->nom }}
        </a>
    @endforeach
</div>

{{-- Tri --}}
<div class="flex justify-end mb-4">
    <select onchange="window.location.href=this.value"
            class="text-sm border border-gray-300 rounded px-3 py-1.5 bg-white">
        <option value="{{ route('catalogue', array_merge(request()->query(), ['tri' => 'defaut'])) }}" {{ $tri == 'defaut' ? 'selected' : '' }}>Par defaut</option>
        <option value="{{ route('catalogue', array_merge(request()->query(), ['tri' => 'prix_asc'])) }}" {{ $tri == 'prix_asc' ? 'selected' : '' }}>Prix croissant</option>
        <option value="{{ route('catalogue', array_merge(request()->query(), ['tri' => 'prix_desc'])) }}" {{ $tri == 'prix_desc' ? 'selected' : '' }}>Prix decroissant</option>
    </select>
</div>

{{-- Grille produits --}}
@if($produits->isEmpty())
    <p class="text-(--color-text-light) text-center py-12">Aucun produit disponible pour le moment.</p>
@else
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @foreach($produits as $produit)
            <div class="bg-(--color-creme) rounded-lg overflow-hidden shadow hover:shadow-lg
                        transition-all duration-200 hover:scale-[1.02] flex flex-col">
                <div class="aspect-[4/3] overflow-hidden bg-(--color-choco)/10">
                    @if($produit->image_path)
                        <img src="{{ asset('storage/' . $produit->image_path) }}"
                             alt="{{ $produit->nom_commercial }}"
                             class="w-full h-full object-cover">
                    @else
                        <div class="w-full h-full flex items-center justify-center text-5xl">&#127851;</div>
                    @endif
                </div>
                <div class="p-4 flex flex-col flex-1">
                    @if($produit->categorie)
                        <span class="text-xs text-(--color-text-light) uppercase tracking-wide">{{ $produit->categorie->nom }}</span>
                    @endif
                    <h3 class="font-semibold mt-1 line-clamp-2">{{ $produit->nom_commercial }}</h3>
                    <div class="mt-auto pt-3 flex items-center justify-between">
                        <span class="text-xl font-bold text-(--color-or)">{{ number_format($produit->prix_vente, 2, ',', ' ') }} &euro;</span>
                        @if($produit->en_stock)
                            <span class="inline-flex items-center gap-1 text-xs font-medium text-green-700 bg-green-100 px-2 py-1 rounded-full">
                                <span class="w-2 h-2 bg-green-500 rounded-full"></span> En stock
                            </span>
                        @else
                            <span class="inline-flex items-center gap-1 text-xs font-medium text-red-700 bg-red-100 px-2 py-1 rounded-full">
                                <span class="w-2 h-2 bg-red-500 rounded-full"></span> Rupture
                            </span>
                        @endif
                    </div>
                    <a href="{{ route('produit.show', $produit->id) }}"
                       class="mt-3 block text-center bg-(--color-choco) text-(--color-creme) py-2 rounded
                              hover:brightness-125 transition-all text-sm font-medium">
                        Voir le produit
                    </a>
                </div>
            </div>
        @endforeach
    </div>
@endif
@endsection
