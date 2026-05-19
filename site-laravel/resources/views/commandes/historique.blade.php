@extends('layouts.app')
@section('title', 'Mes commandes — ArtisaStock')

@section('content')
<h1 class="text-2xl font-display font-bold text-(--color-choco) mb-6">Mes commandes</h1>

@if($commandes->isEmpty())
    <div class="text-center py-12">
        <p class="text-(--color-text-light) text-lg mb-4">Vous n'avez pas encore passe de commande.</p>
        <a href="{{ route('catalogue') }}" class="text-(--color-or) font-semibold hover:underline">Decouvrez notre catalogue &rarr;</a>
    </div>
@else
    <div class="space-y-4">
        @foreach($commandes as $cmd)
            <div class="bg-white rounded-lg shadow p-5 flex items-center justify-between">
                <div>
                    <span class="font-semibold">#{{ $cmd->id }}</span>
                    <span class="text-(--color-text-light) mx-2">&middot;</span>
                    <span class="text-sm text-(--color-text-light)">{{ $cmd->date_commande?->format('d/m/Y H:i') }}</span>
                    <span class="text-(--color-text-light) mx-2">&middot;</span>
                    <span class="text-sm">{{ $cmd->lignes->sum('quantite') }} article(s)</span>
                </div>
                <div class="flex items-center gap-4">
                    <span class="font-bold text-(--color-or)">{{ number_format($cmd->total_ttc, 2, ',', ' ') }} &euro;</span>
                    <span class="text-xs font-medium px-2 py-1 rounded-full bg-green-100 text-green-700 uppercase">{{ $cmd->statut }}</span>
                    <a href="{{ route('commande.detail', $cmd->id) }}" class="text-sm text-(--color-or) font-semibold hover:underline">Voir detail</a>
                </div>
            </div>
        @endforeach
    </div>
@endif
@endsection
