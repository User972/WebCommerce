/* ============================================================================
   IELTS Band 7 Plus™ — storefront behaviour
   localStorage cart + PayPal drawer checkout, theme toggle, mobile menu.
   Shared by the homepage and the product page. Server re-prices every order from
   the database, so the client catalog values here are display-only.
   ============================================================================ */

/* ===== CART + PAYPAL CHECKOUT ===== */
(function () {
  var CART_KEY = 'check-cart';
  var CFG = window.SJ_CONFIG || { paypalEnabled: false, currency: 'USD', idrPerUsd: 16000, promoCode: 'SIRJOSHUA10' };
  var fmt = function (n) { return 'Rp ' + (n || 0).toLocaleString('id-ID'); };

  var catalog = {};
  document.querySelectorAll('[data-product]').forEach(function (el) {
    var id = el.getAttribute('data-id');
    catalog[id] = {
      id: id,
      title: el.getAttribute('data-title'),
      author: el.getAttribute('data-author'),
      price: parseInt(el.getAttribute('data-price'), 10) || 0,
      cat: el.getAttribute('data-cat'),
      cover: el.getAttribute('data-cover')
    };
  });

  function load() { try { return JSON.parse(localStorage.getItem(CART_KEY)) || {}; } catch (e) { return {}; } }
  function persist() { try { localStorage.setItem(CART_KEY, JSON.stringify(cart)); } catch (e) { } }
  var cart = load();
  var promo = null, promoErr = false, step = 'cart', lastOrder = null, paypalRendered = false;

  var overlay = document.getElementById('cartOverlay');
  var drawer = document.getElementById('cartDrawer');
  if (!overlay || !drawer) return; // no cart UI on this page
  var bodyEl = document.getElementById('cartBody');
  var footEl = document.getElementById('cartFooter');
  var titleEl = document.getElementById('drawerTitle');
  var subEl = document.getElementById('drawerSub');
  var badgeEl = document.getElementById('cartBadge');
  var cartBtn = document.getElementById('cartBtn');
  var lastDrawerFocus = null;

  // ── Focus management (drawer a11y) ────────────────────────────────────────
  function focusables(container) {
    return Array.prototype.slice.call(container.querySelectorAll(
      'a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])'
    )).filter(function (el) { return el.offsetWidth || el.offsetHeight || el === document.activeElement; });
  }
  function trapTab(container, e) {
    if (e.key !== 'Tab') return;
    var f = focusables(container); if (!f.length) return;
    var first = f[0], last = f[f.length - 1];
    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
  }
  function pageInert(on) {
    Array.prototype.forEach.call(document.body.children, function (el) {
      if (el === overlay || el === drawer || el.tagName === 'SCRIPT') return;
      if (on) { el.setAttribute('inert', ''); el.setAttribute('aria-hidden', 'true'); }
      else { el.removeAttribute('inert'); el.removeAttribute('aria-hidden'); }
    });
  }

  function count() { var c = 0; for (var k in cart) { c += cart[k]; } return c; }
  function subtotal() { var s = 0; for (var k in cart) { if (catalog[k]) s += catalog[k].price * cart[k]; } return s; }
  function discount() { return promo ? Math.round(subtotal() * promo.rate) : 0; }
  function total() { return subtotal() - discount(); }
  function usd() { var u = total() / (CFG.idrPerUsd || 16000); u = Math.round(u * 100) / 100; return u < 0.01 ? 0.01 : u; }

  function updateBadge() { var n = count(); if (!badgeEl) return; badgeEl.textContent = n; badgeEl.classList.toggle('hide', n === 0); }
  function esc(s) { return (s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); }
  function thumb(cover) { return '<div class="ebook-cover cart-thumb cover-' + cover + '"><span class="frame"></span></div>'; }
  function csrf() { var el = document.querySelector('input[name="__RequestVerificationToken"]'); return el ? el.value : ''; }
  function validEmail(e) { return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(e); }

  function row(p, q) {
    return '' +
      '<div class="flex gap-3 py-4 border-b hairline">' + thumb(p.cover) +
      '<div class="flex-1 min-w-0">' +
      '<div class="flex items-start justify-between gap-2">' +
      '<div class="min-w-0 pr-1">' +
      '<h4 class="font-display text-[15px] font-semibold leading-tight">' + esc(p.title) + '</h4>' +
      '<p class="text-[12px] text-ink/55 mt-0.5">' + esc(p.author) + '</p>' +
      '</div>' +
      '<button data-act="rem" data-id="' + p.id + '" aria-label="Remove" class="shrink-0 text-ink/40 hover:text-terra-600 transition"><svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/></svg></button>' +
      '</div>' +
      '<div class="mt-2.5 flex items-center justify-between">' +
      '<div class="flex items-center gap-1 border hairline rounded-full p-0.5">' +
      '<button class="qty-btn hover:bg-paper transition" data-act="dec" data-id="' + p.id + '" aria-label="Decrease">-</button>' +
      '<span class="w-7 text-center text-[13px] font-semibold tabular-nums">' + q + '</span>' +
      '<button class="qty-btn hover:bg-paper transition" data-act="inc" data-id="' + p.id + '" aria-label="Increase">+</button>' +
      '</div>' +
      '<div class="font-display text-[15px] font-semibold">' + fmt(p.price * q) + '</div>' +
      '</div>' +
      '</div>' +
      '</div>';
  }

  function emptyHtml() {
    return '' +
      '<div class="flex flex-col items-center justify-center text-center py-20">' +
      '<div class="w-16 h-16 rounded-full bg-paper flex items-center justify-center text-ink/40 mb-4"><svg class="w-7 h-7" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M6 6h14.5l-1.6 8H8.1L6 6z"/><path d="M6 6 5.5 3.5H3"/><circle cx="9.5" cy="19" r="1.4"/><circle cx="17" cy="19" r="1.4"/></svg></div>' +
      '<h4 class="font-display text-[20px] font-semibold">Your cart is empty</h4>' +
      '<p class="text-[13.5px] text-ink/55 mt-1.5 max-w-[240px]">Add a resource to get started.</p>' +
      '<button data-act="browse" class="mt-5 px-5 py-2.5 rounded-full bg-teal-600 text-parch text-[14px] font-semibold hover:bg-teal-700 transition">Browse resources</button>' +
      '</div>';
  }

  function cartFootHtml() {
    var d = discount();
    return '' +
      '<div class="py-4 border-t hairline">' +
      '<div class="flex gap-2">' +
      '<input id="promoInput" type="text" value="' + (promo ? promo.code : '') + '" placeholder="Promo code (' + CFG.promoCode + ')" class="flex-1 px-3 py-2.5 rounded-lg border hairline bg-card text-[13px] placeholder-ink/35"/>' +
      '<button data-act="promo" class="px-4 py-2.5 rounded-lg border hairline text-[13px] font-semibold hover:border-teal-600 transition">Apply</button>' +
      '</div>' +
      (promo ? '<p class="text-[12px] text-teal-700 font-medium mt-2 flex items-center gap-1.5"><svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4"><path d="M20 6 9 17l-5-5"/></svg>Code ' + promo.code + ' applied — 10% off</p>' : '') +
      (promoErr ? '<p class="text-[12px] text-terra-600 font-medium mt-2">Invalid promo code.</p>' : '') +
      '<div class="space-y-1.5 text-[13.5px] mt-4">' +
      '<div class="flex justify-between text-ink/65"><span>Subtotal</span><span>' + fmt(subtotal()) + '</span></div>' +
      (d > 0 ? '<div class="flex justify-between text-teal-700"><span>Discount</span><span>- ' + fmt(d) + '</span></div>' : '') +
      '<div class="flex justify-between items-baseline pt-2 mt-1 border-t hairline"><span class="font-semibold">Total</span><span class="font-display text-[20px] font-semibold">' + fmt(total()) + '</span></div>' +
      '</div>' +
      '<button data-act="checkout" class="w-full mt-4 py-3.5 rounded-full bg-terra-500 text-white font-semibold hover:bg-terra-600 transition shadow-[0_10px_24px_-12px_rgba(217,119,87,.85)]">Continue to payment →</button>' +
      '<p class="text-center text-[11.5px] text-ink/45 mt-2.5">Delivered to your email · lifetime access</p>' +
      '</div>';
  }

  function checkoutBodyHtml() {
    var items = Object.keys(cart).map(function (id) { var p = catalog[id]; return '<div class="flex justify-between text-[13px] text-ink/70 py-1"><span class="truncate pr-3">' + esc(p.title) + ' × ' + cart[id] + '</span><span class="shrink-0">' + fmt(p.price * cart[id]) + '</span></div>'; }).join('');
    return '' +
      '<div class="py-4">' +
      '<button data-act="back" class="text-[13px] text-teal-700 font-semibold inline-flex items-center gap-1 mb-5 hover:gap-1.5 transition-all">← Back to cart</button>' +
      '<label class="block mb-3"><span class="block text-[12.5px] font-medium text-ink/75 mb-1">Full name</span>' +
      '<input id="coName" type="text" autocomplete="name" maxlength="120" value="' + esc(field('name')) + '" placeholder="e.g. Putri Ramadhani" class="w-full px-3.5 py-3 rounded-lg border hairline bg-card text-[15px] placeholder-ink/35"/></label>' +
      '<label class="block mb-1"><span class="block text-[12.5px] font-medium text-ink/75 mb-1">Email</span>' +
      '<input id="coEmail" type="email" autocomplete="email" maxlength="254" value="' + esc(field('email')) + '" placeholder="you@email.com" class="w-full px-3.5 py-3 rounded-lg border hairline bg-card text-[15px] placeholder-ink/35"/></label>' +
      '<p class="text-[11.5px] text-ink/45 mb-4">Your download link is sent to this email.</p>' +
      '<span class="block text-[12.5px] font-medium text-ink/75 mb-2">Payment method</span>' +
      '<div class="flex items-center gap-3 px-4 py-3 rounded-xl border hairline bg-card mb-5">' +
      '<svg class="w-7 h-7 shrink-0" viewBox="0 0 24 24" fill="#0F3D3E"><path d="M7.5 3.5h6.2c2.5 0 4.3 1.3 4 3.9-.3 2.6-2.1 4-4.8 4H10l-.8 5.1H6.3L7.5 3.5z" opacity=".55"/><path d="M9.2 6.2h6.2c2.5 0 4.3 1.3 4 3.9-.3 2.6-2.1 4-4.8 4h-2.9l-.8 5.1H7.9L9.2 6.2z"/></svg>' +
      '<span class="flex-1"><span class="block text-[14px] font-semibold leading-tight">PayPal</span><span class="block text-[12px] text-ink/55">Card or PayPal balance — charged ' + CFG.currency + ' ' + usd().toFixed(2) + '</span></span>' +
      '</div>' +
      '<div class="rounded-xl border hairline bg-card p-4">' +
      '<div class="font-mono uppercase tracking-[0.16em] text-[10.5px] text-ink/45 mb-2">Order summary</div>' + items +
      (discount() > 0 ? '<div class="flex justify-between text-[13px] text-teal-700 py-1"><span>Discount (' + promo.code + ')</span><span>- ' + fmt(discount()) + '</span></div>' : '') +
      '<div class="flex justify-between items-baseline pt-2 mt-1 border-t hairline"><span class="font-semibold text-[14px]">Total</span><span class="font-display text-[19px] font-semibold">' + fmt(total()) + '</span></div>' +
      '</div>' +
      '<p id="coErr" class="text-[12.5px] text-terra-600 font-medium mt-3 hidden">Please enter a valid name and email first.</p>' +
      '</div>';
  }

  function checkoutFootHtml() {
    if (!CFG.paypalEnabled) {
      return '<div class="py-4 border-t hairline"><div class="rounded-xl bg-paper text-ink/70 text-[12.5px] p-3 text-center leading-relaxed">PayPal checkout is not active yet. To pay now, use “Request payment link” on the product, or set the PayPal credentials on the server to enable instant checkout.</div></div>';
    }
    return '<div class="py-4 border-t hairline">' +
      '<div id="paypal-button-container" class="min-h-[46px]"></div>' +
      '<p id="payErr" class="text-[12.5px] text-terra-600 font-medium mt-2 hidden"></p>' +
      '<p class="text-center text-[11.5px] text-ink/45 mt-2.5 flex items-center justify-center gap-1.5"><svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="3" y="11" width="18" height="10" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>Secure, encrypted payment via PayPal</p>' +
      '</div>';
  }

  function doneBodyHtml() {
    return '<div class="flex flex-col items-center text-center py-12">' +
      '<div class="w-16 h-16 rounded-full bg-teal-50 text-teal-700 flex items-center justify-center mb-5"><svg class="w-8 h-8" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6 9 17l-5-5"/></svg></div>' +
      '<h4 class="font-display text-[26px] font-semibold leading-tight">Order received!</h4>' +
      '<p class="text-[14px] text-ink/65 mt-2 max-w-[300px]">We’ve sent your download link to <span class="font-semibold text-ink">' + esc(lastOrder.email) + '</span>. Check your inbox (or promotions folder).</p>' +
      '<div class="mt-6 w-full rounded-xl border hairline bg-card p-4 text-left">' +
      '<div class="flex justify-between text-[13px] text-ink/60 py-1"><span>Order no.</span><span class="font-mono font-semibold text-ink">' + esc(lastOrder.no) + '</span></div>' +
      '<div class="flex justify-between text-[13px] text-ink/60 py-1"><span>Total paid</span><span class="font-display text-[16px] font-semibold text-ink">' + fmt(lastOrder.total) + '</span></div>' +
      '</div>' +
      '</div>';
  }
  function doneFootHtml() {
    return '<div class="py-4 border-t hairline"><button data-act="done-close" class="w-full py-3.5 rounded-full bg-teal-600 text-parch font-semibold hover:bg-teal-700 transition">Done</button></div>';
  }

  var fields = { name: '', email: '' };
  function field(k) { return fields[k] || ''; }
  function snapshotFields() {
    var n = document.getElementById('coName'); var e = document.getElementById('coEmail');
    if (n) fields.name = n.value; if (e) fields.email = e.value;
  }

  function render() {
    updateBadge();
    var ids = Object.keys(cart);
    if (step === 'done' && lastOrder) {
      titleEl.textContent = 'Thank you!'; subEl.textContent = 'Payment successful';
      bodyEl.innerHTML = doneBodyHtml(); footEl.innerHTML = doneFootHtml();
      return;
    }
    if (step === 'checkout') {
      titleEl.textContent = 'Payment'; subEl.textContent = 'Step 2 of 2';
      bodyEl.innerHTML = checkoutBodyHtml(); footEl.innerHTML = checkoutFootHtml();
      paypalRendered = false; mountPayPal(); return;
    }
    titleEl.textContent = 'Cart'; subEl.textContent = count() + (count() === 1 ? ' item' : ' items');
    if (ids.length === 0) { bodyEl.innerHTML = emptyHtml(); footEl.innerHTML = ''; return; }
    bodyEl.innerHTML = ids.map(function (id) { return row(catalog[id], cart[id]); }).join('');
    footEl.innerHTML = cartFootHtml();
  }

  function showCoErr() { var e = document.getElementById('coErr'); if (e) e.classList.remove('hidden'); }
  function showPayErr(msg) { var e = document.getElementById('payErr'); if (e) { e.textContent = msg; e.classList.remove('hidden'); } }

  function mountPayPal() {
    if (!CFG.paypalEnabled || !window.paypal || paypalRendered) return;
    var container = document.getElementById('paypal-button-container');
    if (!container) return;
    paypalRendered = true;
    window.paypal.Buttons({
      style: { shape: 'pill', color: 'gold', layout: 'vertical', label: 'pay', height: 46 },
      onClick: function (data, actions) {
        snapshotFields();
        if (!fields.name || fields.name.trim().length < 2 || !validEmail(fields.email.trim())) { showCoErr(); return actions.reject(); }
        return actions.resolve();
      },
      createOrder: function () {
        var items = Object.keys(cart).map(function (id) { return { id: id, qty: cart[id] }; });
        return fetch('/api/checkout/create-order', {
          method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf() },
          body: JSON.stringify({ items: items, name: fields.name.trim(), email: fields.email.trim(), promoCode: promo ? promo.code : null })
        }).then(function (r) { return r.json().then(function (d) { if (!r.ok) throw new Error((d && d.error) || 'Could not start payment.'); return d.id; }); });
      },
      onApprove: function (data) {
        return fetch('/api/checkout/capture-order', {
          method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf() },
          body: JSON.stringify({ payPalOrderId: data.orderID })
        }).then(function (r) {
          return r.json().then(function (d) {
            if (!r.ok) throw new Error((d && d.error) || 'Payment could not be processed.');
            lastOrder = { no: d.orderNumber, email: d.email, total: d.totalIdr };
            cart = {}; persist(); promo = null; promoErr = false; step = 'done'; render();
          });
        });
      },
      onError: function (err) { showPayErr('Something went wrong while processing payment. Please try again.'); }
    }).render('#paypal-button-container');
  }

  function openDrawer() {
    lastDrawerFocus = document.activeElement;
    overlay.classList.add('open'); drawer.classList.add('open'); drawer.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden'; pageInert(true); render();
    var cc = document.getElementById('cartClose'); if (cc) setTimeout(function () { cc.focus(); }, 60);
  }
  function closeDrawer() {
    overlay.classList.remove('open'); drawer.classList.remove('open'); drawer.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = ''; pageInert(false); if (step !== 'done') step = 'cart'; promoErr = false;
    if (lastDrawerFocus && lastDrawerFocus.focus) lastDrawerFocus.focus();
  }

  function add(id) {
    if (!catalog[id]) return;
    cart[id] = (cart[id] || 0) + 1; persist(); updateBadge();
    if (cartBtn) { cartBtn.classList.remove('cartbtn-bump'); void cartBtn.offsetWidth; cartBtn.classList.add('cartbtn-bump'); }
    step = 'cart'; openDrawer();
  }

  document.querySelectorAll('.add-cart').forEach(function (btn) {
    btn.addEventListener('click', function () { add(btn.getAttribute('data-id')); });
  });

  drawer.addEventListener('click', function (e) {
    var t = e.target.closest('[data-act]'); if (!t) return;
    var act = t.getAttribute('data-act'); var id = t.getAttribute('data-id');
    if (act === 'inc') { cart[id]++; persist(); render(); }
    else if (act === 'dec') { cart[id]--; if (cart[id] <= 0) delete cart[id]; persist(); render(); }
    else if (act === 'rem') { delete cart[id]; persist(); render(); }
    else if (act === 'browse') { closeDrawer(); var s = document.getElementById('resources'); if (s) { window.scrollTo({ top: s.offsetTop - 70, behavior: 'smooth' }); } else { window.location.href = '/#resources'; } }
    else if (act === 'promo') {
      var v = (document.getElementById('promoInput').value || '').trim().toUpperCase();
      if (v === (CFG.promoCode || '').toUpperCase()) { promo = { code: CFG.promoCode, rate: .10 }; promoErr = false; }
      else { promo = null; promoErr = (v !== ''); }
      render();
    }
    else if (act === 'checkout') { if (Object.keys(cart).length) { step = 'checkout'; render(); bodyEl.scrollTop = 0; } }
    else if (act === 'back') { snapshotFields(); step = 'cart'; render(); }
    else if (act === 'done-close') { step = 'cart'; closeDrawer(); render(); }
  });

  drawer.addEventListener('input', function (e) {
    if (e.target && (e.target.id === 'coName' || e.target.id === 'coEmail')) snapshotFields();
  });

  if (cartBtn) cartBtn.addEventListener('click', function () { if (step === 'done') { step = 'cart'; } openDrawer(); });
  var cc = document.getElementById('cartClose'); if (cc) cc.addEventListener('click', closeDrawer);
  overlay.addEventListener('click', closeDrawer);
  document.addEventListener('keydown', function (e) {
    if (!drawer.classList.contains('open')) return;
    if (e.key === 'Escape') closeDrawer();
    else if (e.key === 'Tab') trapTab(drawer, e);
  });

  var filters = document.getElementById('ebookFilters');
  if (filters) {
    filters.addEventListener('click', function (e) {
      var b = e.target.closest('[data-filter]'); if (!b) return;
      var f = b.getAttribute('data-filter');
      filters.querySelectorAll('[data-filter]').forEach(function (c) {
        var on = c === b;
        c.classList.toggle('bg-teal-600', on); c.classList.toggle('text-parch', on); c.classList.toggle('border-teal-600', on);
        c.classList.toggle('bg-card', !on); c.classList.toggle('text-ink/70', !on); c.classList.toggle('hairline', !on);
      });
      var shown = 0;
      document.querySelectorAll('.ebook-card[data-cat]').forEach(function (card) {
        var ok = (f === 'all' || card.getAttribute('data-cat') === f);
        card.style.display = ok ? '' : 'none'; if (ok) shown++;
      });
      var emp = document.getElementById('ebookEmpty'); if (emp) emp.classList.toggle('hidden', shown !== 0);
    });
  }

  updateBadge();
})();

/* ===== THEME TOGGLE ===== */
(function () {
  var KEY = 'check-theme', root = document.documentElement;
  function apply(t) { root.setAttribute('data-theme', t); try { localStorage.setItem(KEY, t); } catch (e) { } }
  var btn = document.getElementById('themeToggle');
  if (btn) {
    btn.addEventListener('click', function () {
      var cur = root.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
      apply(cur === 'dark' ? 'light' : 'dark');
    });
  }
})();

/* ===== MOBILE MENU ===== */
(function () {
  var btn = document.getElementById('menuBtn'), menu = document.getElementById('mobileMenu');
  if (!btn || !menu) return;
  function setOpen(open) { menu.classList.toggle('hidden', !open); btn.setAttribute('aria-expanded', open ? 'true' : 'false'); }
  btn.addEventListener('click', function () { setOpen(menu.classList.contains('hidden')); });
  menu.querySelectorAll('.menu-link').forEach(function (a) { a.addEventListener('click', function () { setOpen(false); }); });
})();
