// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const mobileNav = document.querySelector("#mobile-nav");
const mobileNavToggle = document.querySelector("[data-mobile-nav-toggle]");
const notification = document.querySelector("#app-notification");
let notificationTimeout;

function closeMobileNav() {
    if (!mobileNav || !mobileNavToggle) return;
    mobileNav.classList.add("hidden");
    mobileNavToggle.setAttribute("aria-expanded", "false");
    mobileNavToggle.setAttribute("aria-label", "Open navigation menu");
}

function notify(message) {
    if (!notification) return;
    notification.textContent = message;
    notification.classList.remove("hidden");
    window.clearTimeout(notificationTimeout);
    notificationTimeout = window.setTimeout(() => notification.classList.add("hidden"), 2500);
}

window.Wplay = { notify };

document.addEventListener("click", async (event) => {
    const target = event.target;
    if (!(target instanceof Element)) return;

    const toggle = target.closest("[data-mobile-nav-toggle]");
    if (toggle instanceof HTMLButtonElement && mobileNav) {
        const isExpanded = toggle.getAttribute("aria-expanded") === "true";
        toggle.setAttribute("aria-expanded", String(!isExpanded));
        toggle.setAttribute("aria-label", isExpanded ? "Open navigation menu" : "Close navigation menu");
        mobileNav.classList.toggle("hidden", isExpanded);
        if (!isExpanded) mobileNav.querySelector("a")?.focus();
        return;
    }

    if (mobileNav && !mobileNav.classList.contains("hidden")) {
        if (!mobileNav.contains(target) || target.closest("#mobile-nav a, #mobile-nav button[type='submit']")) {
            closeMobileNav();
        }
    }

    const tab = target.closest("[data-tab-target]");
    if (tab instanceof HTMLButtonElement) {
        const tabs = tab.closest("[data-tabs]");
        if (!tabs) return;
        const panelId = tab.dataset.tabTarget;
        const panel = panelId ? tabs.querySelector(`#${CSS.escape(panelId)}`) : null;
        if (!panel) return;

        for (const candidate of tabs.querySelectorAll('[role="tab"]')) {
            const selected = candidate === tab;
            candidate.setAttribute("aria-selected", String(selected));
            candidate.tabIndex = selected ? 0 : -1;
        }
        for (const candidate of tabs.querySelectorAll('[role="tabpanel"]')) {
            candidate.hidden = candidate !== panel;
        }
        return;
    }

    const copyButton = target.closest("[data-copy-target]");
    if (copyButton instanceof HTMLButtonElement) {
        const input = document.querySelector(copyButton.dataset.copyTarget);
        if (!(input instanceof HTMLInputElement)) {
            notify("Could not find the URL to copy.");
            return;
        }
        if (!navigator.clipboard?.writeText) {
            notify("Clipboard access is unavailable in this browser.");
            return;
        }
        try {
            await navigator.clipboard.writeText(input.value);
            notify(copyButton.dataset.copyMessage || "Copied to clipboard.");
        } catch (error) {
            console.error("Could not copy the value to the clipboard.", error);
            notify("Could not copy to clipboard. Select and copy the URL instead.");
        }
    }
});

document.addEventListener("keydown", (event) => {
    const target = event.target;
    if (!(target instanceof Element)) return;

    if (event.key === "Escape" && mobileNav && !mobileNav.classList.contains("hidden")) {
        closeMobileNav();
        mobileNavToggle?.focus();
        return;
    }

    const tab = target.closest('[role="tab"]');
    if (!(tab instanceof HTMLButtonElement)) return;
    const tabs = tab.closest("[data-tabs]");
    if (!tabs) return;
    const tabButtons = [...tabs.querySelectorAll('[role="tab"]')];
    const currentIndex = tabButtons.indexOf(tab);
    let nextIndex;

    if (event.key === "ArrowRight") nextIndex = (currentIndex + 1) % tabButtons.length;
    else if (event.key === "ArrowLeft") nextIndex = (currentIndex - 1 + tabButtons.length) % tabButtons.length;
    else if (event.key === "Home") nextIndex = 0;
    else if (event.key === "End") nextIndex = tabButtons.length - 1;
    else return;

    event.preventDefault();
    tabButtons[nextIndex].focus();
    tabButtons[nextIndex].click();
});
