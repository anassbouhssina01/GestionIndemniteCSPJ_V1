// autocomplete.js — suggestions d'autocomplétion pour les champs de recherche.
// Chaque champ porte data-autocomplete="source[:champ]" et reçoit une datalist
// alimentée à la saisie depuis /Autocomplete/<source>.
(function () {
    "use strict";

    var compteur = 0;

    function debounce(fn, delai) {
        var timer = null;
        return function () {
            var args = arguments;
            var self = this;
            if (timer) clearTimeout(timer);
            timer = setTimeout(function () { fn.apply(self, args); }, delai);
        };
    }

    function creerListe(input, source, champ) {
        compteur += 1;
        var id = 'ac-' + source + '-' + (champ || 'valeur') + '-' + compteur;
        var liste = document.createElement('datalist');
        liste.id = id;
        input.setAttribute('list', id);
        // On garde l'historique du navigateur mais on privilégie nos suggestions.
        input.setAttribute('autocomplete', 'off');
        input.parentNode.appendChild(liste);
        return liste;
    }

    function remplir(liste, valeurs) {
        liste.innerHTML = '';
        (valeurs || []).forEach(function (valeur) {
            if (!valeur) return;
            var option = document.createElement('option');
            option.value = valeur;
            liste.appendChild(option);
        });
    }

    function activer(input) {
        var spec = (input.getAttribute('data-autocomplete') || '').trim();
        if (!spec) return;

        var morceaux = spec.split(':');
        var source = morceaux[0];
        var champ = morceaux.length > 1 ? morceaux[1] : '';
        if (!source) return;

        var liste = creerListe(input, source, champ);
        var dernierTerme = null;
        var chargement = false;

        function charger() {
            if (chargement) return;
            var terme = input.value.trim();
            if (terme === dernierTerme && liste.options.length > 0) return;

            dernierTerme = terme;
            chargement = true;

            var url = '/Autocomplete/' + encodeURIComponent(source)
                + '?terme=' + encodeURIComponent(terme);
            if (champ) url += '&champ=' + encodeURIComponent(champ);

            fetch(url, { headers: { 'Accept': 'application/json' } })
                .then(function (reponse) {
                    if (!reponse.ok) throw new Error('HTTP ' + reponse.status);
                    return reponse.json();
                })
                .then(function (valeurs) { remplir(liste, valeurs); })
                .catch(function () { /* suggestions silencieusement ignorées */ })
                .then(function () { chargement = false; });
        }

        input.addEventListener('input', debounce(charger, 200));
        input.addEventListener('focus', charger);
    }

    document.addEventListener('DOMContentLoaded', function () {
        Array.prototype.forEach.call(
            document.querySelectorAll('input[data-autocomplete]'),
            activer);
    });
})();
