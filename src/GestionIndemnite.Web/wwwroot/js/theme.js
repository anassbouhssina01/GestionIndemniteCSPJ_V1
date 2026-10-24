// theme.js — comportements du thème CSPJ pour l'application rendue côté serveur.
// Contenu volontairement minimal : bascule de la barre latérale et sous-menus.
(function () {
    "use strict";

    function showToast(message, type) {
        if (!message) return;
        var container = document.getElementById('toast-container');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'toast' + (type === 'success' ? ' toast-success' : ' toast-error');
        toast.innerHTML = '<span class="toast-message"></span><button type="button" class="toast-close" aria-label="Fermer">&times;</button>';
        var msgEl = toast.querySelector('.toast-message');
        if (msgEl) msgEl.textContent = message;
        container.appendChild(toast);
        requestAnimationFrame(function () { toast.classList.add('show'); });
        var closeBtn = toast.querySelector('.toast-close');
        function hide() {
            if (toast.classList.contains('hiding')) return;
            toast.classList.add('hiding');
            setTimeout(function () { if (toast.parentNode) toast.parentNode.removeChild(toast); }, 300);
        }
        // La notification reste affichée : seul le bouton "×" la ferme.
        if (closeBtn) closeBtn.addEventListener('click', hide);
    }

    function ready(fn) {
        if (document.readyState !== "loading") {
            fn();
        } else {
            document.addEventListener("DOMContentLoaded", fn);
        }
    }

    ready(function () {
        try {
            var successInput = document.getElementById('toast-data-success');
            if (successInput && successInput.value) { showToast(successInput.value, 'success'); }
        } catch (e) { }
        try {
            var errorInput = document.getElementById('toast-data-error');
            if (errorInput && errorInput.value) { showToast(errorInput.value, 'error'); }
        } catch (e) { }

        var sidebar = document.getElementById("sidebar");
        var wrapper = document.getElementById("main-wrapper");
        var toggle = document.getElementById("btn-toggle-sidebar");

        if (toggle && sidebar) {
            toggle.addEventListener("click", function () {
                if (window.innerWidth <= 1024) {
                    sidebar.classList.toggle("mobile-open");
                } else {
                    sidebar.classList.toggle("collapsed");
                    if (wrapper) {
                        wrapper.classList.toggle("expanded");
                    }
                }
            });
        }

        // Sous-menus repliables (Paramétrage, Historiques, Rapports...)
        var toggles = document.querySelectorAll("[data-submenu-toggle]");
        Array.prototype.forEach.call(toggles, function (btn) {
            btn.addEventListener("click", function () {
                var selector = btn.getAttribute("data-submenu-toggle");
                var target = selector ? document.querySelector(selector) : null;
                if (!target) {
                    return;
                }
                var isOpen = target.classList.toggle("open");
                btn.classList.toggle("open", isOpen);
            });
        });

        // Fermer la barre latérale mobile lors d'un clic à l'extérieur
        document.addEventListener("click", function (event) {
            if (window.innerWidth > 1024 || !sidebar) {
                return;
            }
            if (sidebar.contains(event.target) || (toggle && toggle.contains(event.target))) {
                return;
            }
            sidebar.classList.remove("mobile-open");
        });
    });
})();
