// Hub Esportes Lages — interações leves do front-end.
(function () {
    "use strict";

    // Menu responsivo
    var toggle = document.getElementById("navToggle");
    var links = document.getElementById("navLinks");
    if (toggle && links) {
        toggle.addEventListener("click", function () {
            links.classList.toggle("aberto");
        });
    }

    // Auto-submit dos selects de filtro marcados com [data-auto-submit]
    document.querySelectorAll("[data-auto-submit] select").forEach(function (sel) {
        sel.addEventListener("change", function () {
            sel.closest("form").submit();
        });
    });
})();
