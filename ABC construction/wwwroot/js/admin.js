/*
 * ABC Construction — admin panel behaviour.
 *
 * Progressive enhancement only. Every form submits and every destructive
 * action works with JavaScript disabled; this file adds the mobile nav, local
 * image previews, and a confirmation step before deletes.
 */
(function () {
    "use strict";

    /* ---------------------------------------------------------------------
     * Sidebar navigation (mobile)
     * ------------------------------------------------------------------- */
    function initSidebar() {
        var toggle = document.querySelector("[data-admin-nav-toggle]");
        var nav = document.getElementById("admin-nav");

        if (!toggle || !nav) {
            return;
        }

        function setOpen(open) {
            nav.setAttribute("data-open", open ? "true" : "false");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
            toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
        }

        toggle.addEventListener("click", function () {
            setOpen(toggle.getAttribute("aria-expanded") !== "true");
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && toggle.getAttribute("aria-expanded") === "true") {
                setOpen(false);
                toggle.focus();
            }
        });
    }

    /* ---------------------------------------------------------------------
     * Local image preview
     *
     * Shows the chosen file before upload. Server-side validation in
     * FileUploadService remains the authority — this is convenience only.
     * ------------------------------------------------------------------- */
    function initImagePreviews() {
        var inputs = document.querySelectorAll("[data-preview-target]");

        Array.prototype.forEach.call(inputs, function (input) {
            var preview = document.querySelector(input.getAttribute("data-preview-target"));

            if (!preview) {
                return;
            }

            input.addEventListener("change", function () {
                var file = input.files && input.files[0];

                if (!file) {
                    return;
                }

                // Revoke the previous object URL so blobs are not leaked as the
                // admin cycles through files.
                if (preview.dataset.objectUrl) {
                    URL.revokeObjectURL(preview.dataset.objectUrl);
                }

                var url = URL.createObjectURL(file);
                preview.dataset.objectUrl = url;
                preview.src = url;
                preview.hidden = false;
            });
        });
    }

    /* ---------------------------------------------------------------------
     * Delete confirmation
     *
     * The form posts regardless; this only inserts a confirm step so a
     * mis-click does not destroy a record.
     * ------------------------------------------------------------------- */
    function initDeleteConfirms() {
        document.addEventListener("submit", function (event) {
            var form = event.target;

            if (!form.matches || !form.matches("[data-confirm]")) {
                return;
            }

            if (!window.confirm(form.getAttribute("data-confirm"))) {
                event.preventDefault();
            }
        });
    }

    /* ---------------------------------------------------------------------
     * Submit-once guard
     *
     * Prevents duplicate records from an impatient double-click, and gives
     * immediate feedback that the request is in flight.
     * ------------------------------------------------------------------- */
    function initSubmitGuards() {
        document.addEventListener("submit", function (event) {
            var form = event.target;

            if (event.defaultPrevented || !form.matches || !form.matches("[data-submit-guard]")) {
                return;
            }

            var button = form.querySelector('button[type="submit"]');

            if (!button) {
                return;
            }

            // Defer so the button's value still posts with the form.
            window.setTimeout(function () {
                button.disabled = true;
                button.textContent = button.getAttribute("data-busy-text") || "Saving…";
            }, 0);
        });
    }

    function init() {
        initSidebar();
        initImagePreviews();
        initDeleteConfirms();
        initSubmitGuards();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
