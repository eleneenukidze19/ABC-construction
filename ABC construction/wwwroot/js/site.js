/*
 * ABC Construction — public site behaviour.
 *
 * Scope is deliberately small: the site is server-rendered and every page works
 * without JavaScript. This file only adds the mobile menu and a lightbox, both
 * of which degrade to plain markup if the script fails to load.
 */
(function () {
    "use strict";

    /* ---------------------------------------------------------------------
     * Mobile navigation
     * ------------------------------------------------------------------- */
    function initMobileNav() {
        var toggle = document.querySelector("[data-nav-toggle]");
        var drawer = document.getElementById("mobile-nav");

        if (!toggle || !drawer) {
            return;
        }

        // Labels come from the markup so they follow the page's language.
        var openLabel = toggle.getAttribute("data-label-open") || "Open menu";
        var closeLabel = toggle.getAttribute("data-label-close") || "Close menu";

        function setOpen(open) {
            drawer.setAttribute("data-open", open ? "true" : "false");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
            toggle.setAttribute("aria-label", open ? closeLabel : openLabel);
        }

        toggle.addEventListener("click", function () {
            setOpen(toggle.getAttribute("aria-expanded") !== "true");
        });

        // Escape closes the drawer and returns focus to the control that
        // opened it, so keyboard users are never stranded.
        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && toggle.getAttribute("aria-expanded") === "true") {
                setOpen(false);
                toggle.focus();
            }
        });

        // Reset state when resizing up into the desktop layout, otherwise the
        // drawer can stay flagged open behind the horizontal nav.
        var desktop = window.matchMedia("(min-width: 1024px)");
        var onChange = function (event) {
            if (event.matches) {
                setOpen(false);
            }
        };

        if (typeof desktop.addEventListener === "function") {
            desktop.addEventListener("change", onChange);
        }
    }

    /* ---------------------------------------------------------------------
     * Gallery lightbox
     *
     * Images stay plain <img> in the markup; the dialog is only wired up when
     * the browser supports <dialog>.
     * ------------------------------------------------------------------- */
    function initGallery() {
        var gallery = document.querySelector("[data-gallery]");

        if (!gallery || typeof HTMLDialogElement === "undefined") {
            return;
        }

        var dialog = document.createElement("dialog");
        dialog.className = "lightbox";
        dialog.innerHTML =
            '<button type="button" class="lightbox__close">&times;</button>' +
            '<img class="lightbox__img" alt="" />';

        document.body.appendChild(dialog);

        var image = dialog.querySelector(".lightbox__img");
        var closeButton = dialog.querySelector(".lightbox__close");

        // Set as an attribute, not in the HTML string, so a translated label
        // can never break the markup.
        closeButton.setAttribute("aria-label",
            gallery.getAttribute("data-close-label") || "Close image");

        gallery.addEventListener("click", function (event) {
            var trigger = event.target.closest("[data-lightbox]");

            if (!trigger) {
                return;
            }

            event.preventDefault();
            image.src = trigger.getAttribute("data-lightbox");
            image.alt = trigger.getAttribute("data-lightbox-alt") || "";
            dialog.showModal();
        });

        closeButton.addEventListener("click", function () {
            dialog.close();
        });

        // Clicking the backdrop (the dialog element itself) dismisses it.
        dialog.addEventListener("click", function (event) {
            if (event.target === dialog) {
                dialog.close();
            }
        });

        // Release the image once closed so it is not held in memory.
        dialog.addEventListener("close", function () {
            image.removeAttribute("src");
        });
    }

    function init() {
        initMobileNav();
        initGallery();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
