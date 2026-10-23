// Core Audit — minimal progressive-enhancement JS (no extra libraries).
(function () {
    'use strict';

    // ---- Back-to-top button ----
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

    // ---- Mobile navbar behaviour ----
    var navCollapse = document.getElementById('mainNav');
    var navToggler = document.querySelector('.navbar-toggler[data-bs-target="#mainNav"]');

    // Returns a Bootstrap Collapse instance for the menu, or null if Bootstrap
    // isn't available. toggle:false means getting the instance never opens it.
    function getCollapse() {
        if (!navCollapse || !window.bootstrap || !window.bootstrap.Collapse) {
            return null;
        }
        return window.bootstrap.Collapse.getInstance(navCollapse)
            || new window.bootstrap.Collapse(navCollapse, { toggle: false });
    }

    function isMenuOpen() {
        return !!navCollapse && navCollapse.classList.contains('show');
    }

    function hideMenu() {
        var c = getCollapse();
        if (c) {
            c.hide();
        } else if (navCollapse) {
            // Fallback if Bootstrap JS failed to load: collapse manually.
            navCollapse.classList.remove('show');
            if (navToggler) { navToggler.setAttribute('aria-expanded', 'false'); }
        }
    }

    if (navCollapse) {
        // Close the menu after a nav link / CTA is tapped (mobile only — the
        // toggler is hidden at >= lg, so this is a no-op on desktop).
        var navLinks = navCollapse.querySelectorAll('.nav-link, .btn-cta');
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (isMenuOpen()) { hideMenu(); }
            });
        });

        // Close when clicking/tapping outside the open menu.
        document.addEventListener('click', function (e) {
            if (!isMenuOpen()) { return; }
            var insideMenu = navCollapse.contains(e.target);
            var onToggler = navToggler && navToggler.contains(e.target);
            if (!insideMenu && !onToggler) { hideMenu(); }
        });

        // Close on Escape and return focus to the toggler for keyboard users.
        document.addEventListener('keydown', function (e) {
            if ((e.key === 'Escape' || e.key === 'Esc') && isMenuOpen()) {
                hideMenu();
                if (navToggler) { navToggler.focus(); }
            }
        });

        // Keep aria-expanded correct even when Bootstrap drives the collapse.
        if (navToggler) {
            navCollapse.addEventListener('shown.bs.collapse', function () {
                navToggler.setAttribute('aria-expanded', 'true');
            });
            navCollapse.addEventListener('hidden.bs.collapse', function () {
                navToggler.setAttribute('aria-expanded', 'false');
            });
        }
    }
})();
