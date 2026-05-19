<header class="bg-(--color-choco) text-(--color-creme) shadow-md">
    <div class="max-w-[1200px] mx-auto px-6 py-3 flex items-center justify-between">
        <a href="{{ route('catalogue') }}" class="text-xl font-display font-bold tracking-wide">
            ArtisaStock
        </a>

        <nav class="flex items-center gap-6 text-sm">
            <a href="{{ route('catalogue') }}" class="hover:text-(--color-or) transition-colors">Catalogue</a>

            @if(session('client_id'))
                <a href="{{ route('panier') }}" class="relative hover:text-(--color-or) transition-colors">
                    Panier
                    <span id="panier-badge"
                          class="absolute -top-2 -right-5 bg-(--color-or) text-(--color-choco)
                                 text-xs font-bold rounded-full w-5 h-5
                                 flex items-center justify-center">
                        {{ $panierCount ?? 0 }}
                    </span>
                </a>
                <a href="{{ route('commandes.historique') }}" class="hover:text-(--color-or) transition-colors">
                    Mes commandes
                </a>
                <div class="relative group">
                    <span class="cursor-pointer hover:text-(--color-or) transition-colors">
                        {{ session('client_prenom') }} &#9662;
                    </span>
                    <div class="hidden group-hover:block absolute right-0 mt-1
                                bg-white text-(--color-text-main) rounded shadow-lg py-2 min-w-[150px] z-50">
                        <a href="{{ route('profil.edit') }}" class="block px-4 py-2 hover:bg-gray-100 text-sm">Profil</a>
                        <form method="POST" action="{{ route('logout') }}">
                            @csrf
                            <button class="block w-full text-left px-4 py-2 hover:bg-gray-100 text-sm">
                                Deconnexion
                            </button>
                        </form>
                    </div>
                </div>
            @else
                <a href="{{ route('login') }}" class="hover:text-(--color-or) transition-colors">Se connecter</a>
                <a href="{{ route('register') }}"
                   class="bg-(--color-or) text-(--color-choco) px-4 py-2 rounded font-semibold
                          hover:brightness-110 transition-all">
                    S'inscrire
                </a>
            @endif
        </nav>
    </div>
</header>
