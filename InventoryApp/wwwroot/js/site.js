document.addEventListener('DOMContentLoaded', () => {
    initNotifications();
    initTopbarSearch();
});

function initNotifications() {
    const button = document.getElementById('notificationButton');
    const panel = document.getElementById('notificationPanel');
    const close = document.getElementById('notificationClose');
    const countEl = document.getElementById('notificationCount');
    const summaryEl = document.getElementById('notificationSummary');
    const listEl = document.getElementById('notificationList');

    if (!button || !panel || !countEl || !summaryEl || !listEl) return;

    const setOpen = (open) => {
        panel.hidden = !open;
        button.setAttribute('aria-expanded', String(open));
    };

    const render = (data) => {
        const count = Number(data?.count || 0);
        countEl.textContent = count > 99 ? '99+' : String(count);
        countEl.hidden = count === 0;
        summaryEl.textContent = count === 0
            ? "You're all caught up."
            : `${count} stock alert${count === 1 ? '' : 's'} need attention`;

        if (!Array.isArray(data?.items) || data.items.length === 0) {
            listEl.innerHTML = `
                <div class="notification-empty">
                    <i class="bi bi-check-circle-fill"></i>
                    <div><strong>No new alerts</strong><span>Your stock levels look healthy.</span></div>
                </div>`;
            return;
        }

        listEl.innerHTML = data.items.map(item => {
            const isOut = item.severity === 'out';
            const title = isOut ? 'Out of stock' : 'Low stock';
            const detail = isOut
                ? `${escapeHtml(item.name)} is currently at 0 units.`
                : `${escapeHtml(item.name)} has ${Number(item.stock).toLocaleString()} units left.`;
            const icon = isOut ? 'bi-x-octagon-fill' : 'bi-exclamation-triangle-fill';
            const cls = isOut ? 'danger' : 'warning';

            return `
                <a class="notification-item ${cls}" href="/Inventory/LowStockAlerts">
                    <span class="notification-icon"><i class="bi ${icon}"></i></span>
                    <span class="notification-copy">
                        <strong>${title}</strong>
                        <span>${detail}</span>
                    </span>
                    <i class="bi bi-chevron-right notification-arrow"></i>
                </a>`;
        }).join('');
    };

    const load = async () => {
        summaryEl.textContent = 'Checking for alerts...';
        try {
            const response = await fetch('/Inventory/Notifications', {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: 'no-store'
            });
            if (!response.ok) throw new Error(`Notification request failed (${response.status})`);
            const data = await response.json();
            render(data);
        } catch (error) {
            console.error(error);
            summaryEl.textContent = 'Could not load alerts.';
            listEl.innerHTML = `
                <div class="notification-empty notification-error">
                    <i class="bi bi-wifi-off"></i>
                    <div><strong>Notifications unavailable</strong><span>Please refresh and try again.</span></div>
                </div>`;
        }
    };

    button.addEventListener('click', async (event) => {
        event.stopPropagation();
        const open = panel.hidden;
        setOpen(open);
        if (open) await load();
    });

    close?.addEventListener('click', (event) => {
        event.stopPropagation();
        setOpen(false);
    });

    document.addEventListener('click', (event) => {
        if (!panel.hidden && !panel.contains(event.target) && !button.contains(event.target)) {
            setOpen(false);
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') setOpen(false);
    });

    load();
}

function initTopbarSearch() {
    const input = document.querySelector('.topbar-search input[name="searchString"]');
    const form = input?.closest('form');
    if (!input || !form) return;

    form.addEventListener('submit', (event) => {
        const query = input.value.trim();
        if (!query) {
            event.preventDefault();
            input.focus();
        }
    });
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}
