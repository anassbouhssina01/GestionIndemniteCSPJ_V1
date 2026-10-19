// theme.js — comportements du thème CSPJ pour l'application rendue côté serveur.
// Contenu volontairement minimal : bascule de la barre latérale et sous-menus.
(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") {
            fn();
        } else {
            document.addEventListener("DOMContentLoaded", fn);
        }
    }

    ready(function () {
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
