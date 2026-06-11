<?php

use App\Http\Controllers\Auth\LoginController;
use App\Http\Controllers\Auth\RegisterController;
use App\Http\Controllers\CatalogueController;
use App\Http\Controllers\CommandeController;
use App\Http\Controllers\PanierController;
use App\Http\Controllers\ProfilController;
use Illuminate\Support\Facades\Route;

// -- Catalogue (public) ------------------------------------------------
// Listing produits en vente + fiche detail
Route::get('/',              [CatalogueController::class, 'index'])->name('catalogue');
Route::get('/produit/{id}',  [CatalogueController::class, 'show'])->name('produit.show');

// -- Auth (guest) ------------------------------------------------------
// Inscription, connexion, deconnexion — throttle 5 req/min sur POST
Route::get('/register',  [RegisterController::class, 'showForm'])->name('register');
Route::post('/register', [RegisterController::class, 'register'])->middleware('throttle:5,1');

Route::get('/login',     [LoginController::class, 'showForm'])->name('login');
Route::post('/login',    [LoginController::class, 'login'])->middleware('throttle:5,1');
Route::post('/logout',   [LoginController::class, 'logout'])->name('logout');

// -- Routes protegees (client connecte) --------------------------------
// Middleware ClientAuth : verifie session + compte actif en base (cache 5 min)
Route::middleware('client.auth')->group(function () {

    // -- Panier (AJAX sauf index) --------------------------------------
    Route::get('/panier',              [PanierController::class, 'index'])->name('panier');
    Route::post('/panier/ajouter',     [PanierController::class, 'ajouter'])->name('panier.ajouter');
    Route::patch('/panier/quantite',   [PanierController::class, 'updateQuantite'])->name('panier.quantite');
    Route::delete('/panier/supprimer', [PanierController::class, 'supprimer'])->name('panier.supprimer');
    Route::get('/panier/count',        [PanierController::class, 'count'])->name('panier.count');

    // -- Commandes (checkout + historique) ------------------------------
    Route::get('/commande/recap',      [CommandeController::class, 'recap'])->name('commande.recap');
    Route::post('/commande/valider',   [CommandeController::class, 'valider'])->name('commande.valider');
    Route::get('/mes-commandes',       [CommandeController::class, 'historique'])->name('commandes.historique');
    Route::get('/commande/{id}',       [CommandeController::class, 'detail'])->name('commande.detail');

    // -- Profil client -------------------------------------------------
    Route::get('/profil',              [ProfilController::class, 'edit'])->name('profil.edit');
    Route::put('/profil',              [ProfilController::class, 'update'])->name('profil.update');
});
