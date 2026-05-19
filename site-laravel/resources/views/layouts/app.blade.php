<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', 'ArtisaStock Boutique')</title>
    @vite(['resources/css/app.css', 'resources/js/app.js'])
</head>
<body class="bg-(--color-bg) text-(--color-text-main) font-sans min-h-screen flex flex-col">

    @include('components.header')

    {{-- Flash messages --}}
    <div class="max-w-[1200px] mx-auto w-full px-6">
        @if(session('success'))
            <div class="mt-4 px-4 py-3 rounded text-sm font-medium bg-green-100 text-green-800 border border-green-200">
                {{ session('success') }}
            </div>
        @endif
        @if(session('error'))
            <div class="mt-4 px-4 py-3 rounded text-sm font-medium bg-red-100 text-red-800 border border-red-200">
                {{ session('error') }}
            </div>
        @endif
    </div>

    <main class="flex-1 max-w-[1200px] mx-auto w-full px-6 py-8">
        @yield('content')
    </main>

    @include('components.footer')

    {{-- Toast container for AJAX notifications --}}
    <div id="toast-container" class="fixed top-4 right-4 z-50 space-y-2"></div>

    @stack('scripts')
</body>
</html>
