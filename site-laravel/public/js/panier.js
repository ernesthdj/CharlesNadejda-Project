/**
 * ArtisaStock Boutique — Interactions AJAX panier
 * Vanilla JS (ES6+) — fetch API
 */

const CSRF = document.querySelector('meta[name="csrf-token"]')?.content;

// ══════════════════════════════════════════════════════════════
//  TOAST NOTIFICATION
// ══════════════════════════════════════════════════════════════

function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `px-4 py-3 rounded shadow-lg text-white text-sm transition-opacity duration-300 ${
        type === 'success' ? 'bg-green-600' : 'bg-red-600'
    }`;
    toast.textContent = message;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ══════════════════════════════════════════════════════════════
//  AJOUTER AU PANIER
// ══════════════════════════════════════════════════════════════

async function ajouterAuPanier(idProduit, quantite = 1) {
    // QA-03 : validation côté client
    if (quantite < 1) { showToast('Quantité minimum : 1', 'error'); return; }

    try {
        const res = await fetch('/panier/ajouter', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': CSRF,
                'Accept': 'application/json',
            },
            body: JSON.stringify({ id_produit: idProduit, quantite }),
        });
        const data = await res.json();

        if (data.success) {
            const badge = document.getElementById('panier-badge');
            if (badge) badge.textContent = data.panier_count;
            showToast(data.message);
        } else {
            showToast(data.message, 'error');
        }
    } catch (e) {
        showToast('Erreur réseau. Réessayez.', 'error');
    }
}

// ══════════════════════════════════════════════════════════════
//  MODIFIER QUANTITÉ
// ══════════════════════════════════════════════════════════════

async function updateQuantite(idLigne, nouvelleQuantite) {
    // QA-03 : validation côté client
    if (nouvelleQuantite < 1) { showToast('Quantité minimum : 1', 'error'); return; }

    try {
        const res = await fetch('/panier/quantite', {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': CSRF,
                'Accept': 'application/json',
            },
            body: JSON.stringify({ id_ligne: idLigne, quantite: nouvelleQuantite }),
        });
        const data = await res.json();

        if (data.success) {
            // Reload page to update all values cleanly
            location.reload();
        } else {
            showToast(data.message, 'error');
        }
    } catch (e) {
        showToast('Erreur réseau. Réessayez.', 'error');
    }
}

// ══════════════════════════════════════════════════════════════
//  SUPPRIMER DU PANIER
// ══════════════════════════════════════════════════════════════

async function supprimerDuPanier(idLigne) {
    try {
        const res = await fetch('/panier/supprimer', {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': CSRF,
                'Accept': 'application/json',
            },
            body: JSON.stringify({ id_ligne: idLigne }),
        });
        const data = await res.json();

        if (data.success) {
            const row = document.querySelector(`[data-ligne="${idLigne}"]`);
            if (row) {
                row.style.opacity = '0';
                setTimeout(() => {
                    row.remove();
                    // Update total
                    const totalEl = document.getElementById('total-panier');
                    if (totalEl) totalEl.textContent = data.total + ' €';
                    // Update badge
                    const badge = document.getElementById('panier-badge');
                    if (badge) badge.textContent = data.panier_count;
                    // If empty, reload to show empty message
                    if (data.panier_count === 0) location.reload();
                }, 300);
            }
        } else {
            showToast(data.message, 'error');
        }
    } catch (e) {
        showToast('Erreur réseau. Réessayez.', 'error');
    }
}
