// Core Audit — minimal progressive-enhancement JS (no extra libraries).
(function () {
    'use strict';

    // Back-to-top button
    var backToTop = document.getElementById('backToTop');
    if (backToTop) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                backToTop.classList.add('show');
            } else {
                backToTop.classList.remove('show');
            }
        });
        backToTop.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // Collapse the mobile navbar after clicking a nav link
    var navLinks = document.querySelectorAll('#mainNav .nav-link, #mainNav .btn-cta');
    var navCollapse = document.getElementById('mainNav');
    if (navCollapse && window.bootstrap) {
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (navCollapse.classList.contains('show')) {
                    var bsCollapse = window.bootstrap.Collapse.getInstance(navCollapse)
                        || new window.bootstrap.Collapse(navCollapse, { toggle: false });
                    bsCollapse.hide();
                }
            });
        });
    }
})();
