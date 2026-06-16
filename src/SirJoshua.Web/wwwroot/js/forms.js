/* ============================================================================
   IELTS Band 7 Plus™ — enquiry forms
   Powers the shared enquiry modal (product enquiry + the four paid services +
   waitlist) and the inline lead forms (Free Checklist, Contact). One submit path,
   consistent validation, loading / success / error states, anti-forgery header.
   No card data is ever collected here — payment is settled out of band.

   Trigger contract:
     • <button data-enquiry="<formType>" [data-interest=".." data-item="<id>"
        data-title=".."]>  → opens the modal for that form type.
     • <form data-enquiry-form="<LeadType>"> → inline lead form (submitted to
        /api/enquiry/submit with Type = <LeadType>).
   ============================================================================ */
(function () {
  'use strict';

  var CFG = window.B7_CONFIG || {};
  var PAYMENTS = (CFG.payments && CFG.payments.length) ? CFG.payments : [
    { id: 'paypal', name: 'PayPal' }, { id: 'stripe', name: 'Stripe' },
    { id: 'wise', name: 'Wise' }, { id: 'visa', name: 'Visa' },
    { id: 'mastercard', name: 'Mastercard' }, { id: 'qris', name: 'QRIS' }
  ];
  var SOURCE = (location && location.pathname) ? location.pathname : '/';

  function csrf() {
    var el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
  }
  function validEmail(e) { return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(e || ''); }
  function esc(s) { return (s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }
  function payOptions() {
    return PAYMENTS.map(function (p) { return '<option value="' + esc(p.id) + '">' + esc(p.name) + '</option>'; }).join('');
  }

  // ── Field schema per form type ────────────────────────────────────────────
  var MODULE = ['Academic', 'General Training'];
  var TASK = ['Task 1', 'Task 2'];
  var FOCUS = ['Writing', 'Speaking', 'Reading', 'Listening', 'Overall Strategy'];
  var BANDS = ['5.0', '5.5', '6.0', '6.5', '7.0', '7.5', '8.0', '8.5', '9.0'];

  function sel(name, label, opts, opt) {
    opt = opt || {};
    var o = (opt.placeholder ? '<option value="">' + esc(opt.placeholder) + '</option>' : '') +
      opts.map(function (v) { return '<option value="' + esc(v) + '">' + esc(v) + '</option>'; }).join('');
    return field(name, label, '<select class="field-select" name="' + name + '"' + (opt.required ? ' required' : '') + '>' + o + '</select>', opt);
  }
  function txt(name, label, opt) {
    opt = opt || {};
    var type = opt.type || 'text';
    return field(name, label, '<input class="field-input" type="' + type + '" name="' + name + '"' +
      (opt.required ? ' required' : '') + (opt.placeholder ? ' placeholder="' + esc(opt.placeholder) + '"' : '') +
      (opt.autocomplete ? ' autocomplete="' + opt.autocomplete + '"' : '') + ' />', opt);
  }
  function area(name, label, opt) {
    opt = opt || {};
    return field(name, label, '<textarea class="field-textarea" name="' + name + '"' + (opt.required ? ' required' : '') +
      (opt.placeholder ? ' placeholder="' + esc(opt.placeholder) + '"' : '') + '></textarea>', opt);
  }
  function field(name, label, control, opt) {
    opt = opt || {};
    var id = 'f_' + name;
    var req = opt.required ? ' <span aria-hidden="true" class="text-terra-600">*</span>' : '';
    var hint = opt.hint ? '<p class="field-hint">' + esc(opt.hint) + '</p>' : '';
    return '<div class="form-row' + (opt.half ? ' half' : '') + '">' +
      '<label class="field-label" for="' + id + '">' + esc(label) + req + '</label>' +
      control.replace('name="' + name + '"', 'name="' + name + '" id="' + id + '"') +
      '<p class="field-error" data-error-for="' + name + '" hidden></p>' + hint + '</div>';
  }

  var NAME = function () { return txt('FullName', 'Full name', { required: true, autocomplete: 'name', placeholder: 'e.g. Putri Ramadhani' }); };
  var EMAIL = function () { return txt('Email', 'Email', { required: true, type: 'email', autocomplete: 'email', placeholder: 'you@email.com' }); };
  var WA = function (req) { return txt('WhatsApp', 'WhatsApp number' + (req ? '' : ' (optional)'), { required: !!req, type: 'tel', placeholder: '+62 812-3456-7890', half: true }); };
  var PAY = function () { return field('PaymentMethod', 'Preferred payment method', '<select class="field-select" name="PaymentMethod">' + payOptions() + '</select>', { half: true, hint: 'We’ll send secure payment instructions for your choice.' }); };

  var SCHEMA = {
    'product': {
      type: 'ProductEnquiry', endpoint: 'submit',
      title: 'Get the Band 7+ Formula',
      subtitle: 'Tell us where to send your access & payment instructions.',
      submit: 'Request access',
      fields: function () { return [NAME(), EMAIL(), WA(false), PAY(), area('Message', 'Anything we should know? (optional)', { placeholder: 'Your target band, test date, questions…' })]; }
    },
    'writing-feedback': {
      type: 'WritingFeedback', endpoint: 'writing-feedback',
      title: 'IELTS Writing Feedback',
      subtitle: 'Paste one writing task and we’ll return a detailed review with an estimated band score.',
      submit: 'Submit for feedback',
      fields: function () {
        return [NAME(), EMAIL(), WA(false),
          sel('IeltsModule', 'IELTS module', MODULE, { half: true, placeholder: 'Select…' }),
          sel('TaskType', 'Task type', TASK, { half: true, placeholder: 'Select…' }),
          sel('TargetBand', 'Target band', BANDS, { half: true, placeholder: 'Select…' }),
          txt('TestDate', 'Test date (optional)', { type: 'date', half: true }),
          area('WritingText', 'Your writing answer', { required: true, placeholder: 'Paste the full text of your Task 1 or Task 2 response here…', hint: 'Prefer to send a file? Submit the text here and email any attachment after you receive your reference number.' }),
          area('Notes', 'Notes / questions (optional)'),
          PAY()];
      }
    },
    'writing-package': {
      type: 'WritingFeedbackPackage', endpoint: 'submit',
      title: 'IELTS Writing Feedback Package — 3 reviews',
      subtitle: 'Request your package of three writing reviews. We’ll set up your three review credits.',
      submit: 'Request 3 reviews',
      fields: function () { return [NAME(), EMAIL(), WA(false), sel('TargetBand', 'Target band (optional)', BANDS, { half: true, placeholder: 'Select…' }), PAY(), area('Message', 'Notes (optional)', { placeholder: 'When do you plan to submit your tasks?' })]; }
    },
    'speaking-assessment': {
      type: 'SpeakingAssessment', endpoint: 'submit',
      title: 'Book a Speaking Assessment',
      subtitle: 'A simulated Speaking test with an estimated band score and feedback. We’ll confirm your session schedule.',
      submit: 'Request booking',
      fields: function () {
        return [NAME(), EMAIL(), WA(true),
          sel('IeltsModule', 'IELTS module', MODULE, { half: true, placeholder: 'Select…' }),
          sel('CurrentBand', 'Current estimated band', BANDS, { half: true, placeholder: 'Select…' }),
          sel('TargetBand', 'Target band', BANDS, { half: true, placeholder: 'Select…' }),
          txt('TestDate', 'Test date (optional)', { type: 'date', half: true }),
          txt('PreferredDate', 'Preferred session date', { type: 'date', required: true, half: true }),
          txt('PreferredTime', 'Preferred time slot', { required: true, placeholder: 'e.g. 19:00', half: true }),
          txt('TimeZone', 'Time zone', { placeholder: 'e.g. GMT+7 (WIB)', half: true }),
          PAY(), area('Message', 'Notes (optional)')];
      }
    },
    'strategy-consultation': {
      type: 'StrategyConsultation', endpoint: 'submit',
      title: 'Book a Strategy Consultation',
      subtitle: 'A 60-minute session with a personalized study plan based on your target and timeline.',
      submit: 'Request booking',
      fields: function () {
        return [NAME(), EMAIL(), WA(true),
          sel('CurrentBand', 'Current band (if known)', BANDS, { half: true, placeholder: 'Select…' }),
          sel('TargetBand', 'Target band', BANDS, { half: true, placeholder: 'Select…' }),
          txt('TestDate', 'Test date / timeline', { placeholder: 'e.g. August 2026', half: true }),
          sel('FocusArea', 'Main problem area', FOCUS, { half: true, placeholder: 'Select…' }),
          txt('PreferredDate', 'Preferred date', { type: 'date', required: true, half: true }),
          txt('PreferredTime', 'Preferred time', { required: true, placeholder: 'e.g. 19:00', half: true }),
          PAY(), area('Message', 'Notes (optional)')];
      }
    },
    'waitlist': {
      type: 'Waitlist', endpoint: 'submit',
      title: 'Join the waitlist',
      subtitle: 'We’ll let you know the moment this resource is available.',
      submit: 'Join waitlist',
      fields: function () { return [NAME(), EMAIL(), WA(false)]; }
    }
  };

  // ── Modal plumbing ────────────────────────────────────────────────────────
  var overlay = document.getElementById('enquiryModal');
  if (!overlay) return; // modal markup not present (e.g. minimal pages)
  var card = overlay.querySelector('.modal-card');
  var lastFocus = null;

  // ── Focus management (modal a11y) ─────────────────────────────────────────
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
  // Make everything behind the dialog inert + hidden from assistive tech while it is open.
  function pageInert(on) {
    Array.prototype.forEach.call(document.body.children, function (el) {
      if (el === overlay || el.tagName === 'SCRIPT') return;
      if (on) { el.setAttribute('inert', ''); el.setAttribute('aria-hidden', 'true'); }
      else { el.removeAttribute('inert'); el.removeAttribute('aria-hidden'); }
    });
  }

  function modalHtml(def, ctx) {
    var rows = def.fields().join('');
    return '' +
      '<form id="enquiryForm" novalidate>' +
      '<div class="flex items-start justify-between gap-4 px-6 pt-6">' +
      '<div><h3 class="font-display text-[24px] font-semibold leading-tight" id="enquiryTitle">' + esc(def.title) + '</h3>' +
      '<p class="mt-1 text-[13.5px] text-ink/60">' + esc(def.subtitle) + '</p></div>' +
      '<button type="button" data-modal-close aria-label="Close" class="shrink-0 w-9 h-9 rounded-full border hairline flex items-center justify-center text-ink/60 hover:text-ink hover:border-teal-600 transition"><svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 6 6 18M6 6l12 12"/></svg></button>' +
      '</div>' +
      '<div class="px-6 py-5 grid grid-cols-2 gap-x-4 gap-y-3.5 enquiry-grid">' + rows + '</div>' +
      '<div class="honeypot"><label>Leave this empty<input type="text" name="Website" tabindex="-1" autocomplete="off"></label></div>' +
      '<p class="px-6 text-[12.5px] text-terra-600 font-medium" data-form-error hidden></p>' +
      '<div class="px-6 pb-6 pt-2 flex items-center gap-3 border-t hairline mt-1">' +
      '<button type="submit" class="btn-primary flex-1"><span class="btn-text">' + esc(def.submit) + '</span></button>' +
      '<button type="button" data-modal-close class="btn-ghost">Cancel</button>' +
      '</div>' +
      '<input type="hidden" name="Type" value="' + esc(def.type) + '">' +
      '<input type="hidden" name="InterestType" value="' + esc(ctx.interest || '') + '">' +
      '<input type="hidden" name="ProductOrServiceId" value="' + esc(ctx.item || '') + '">' +
      '<input type="hidden" name="SourcePage" value="' + esc(SOURCE) + '">' +
      '</form>';
  }

  function openModal(formType, ctx) {
    var def = SCHEMA[formType];
    if (!def) return;
    lastFocus = document.activeElement;
    card.innerHTML = modalHtml(def, ctx || {});
    overlay.classList.add('open');
    overlay.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    pageInert(true);
    var first = card.querySelector('input, select, textarea');
    if (first) setTimeout(function () { first.focus(); }, 60);
    card.querySelector('#enquiryForm').addEventListener('submit', onSubmit);
  }

  function closeModal() {
    overlay.classList.remove('open');
    overlay.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    pageInert(false);
    card.innerHTML = '';
    if (lastFocus && lastFocus.focus) lastFocus.focus();
  }

  overlay.addEventListener('click', function (e) {
    if (e.target === overlay || e.target.closest('[data-modal-close]')) closeModal();
  });
  document.addEventListener('keydown', function (e) {
    if (!overlay.classList.contains('open')) return;
    if (e.key === 'Escape') closeModal();
    else if (e.key === 'Tab') trapTab(card, e);
  });

  // Open from any trigger on the page.
  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-enquiry]');
    if (!t) return;
    e.preventDefault();
    openModal(t.getAttribute('data-enquiry'), {
      interest: t.getAttribute('data-interest'),
      item: t.getAttribute('data-item'),
      title: t.getAttribute('data-title')
    });
  });

  // ── Validation + submit (shared by modal + inline forms) ──────────────────
  function clearErrors(form) {
    form.querySelectorAll('[data-error-for]').forEach(function (el) { el.hidden = true; el.textContent = ''; });
    var fe = form.querySelector('[data-form-error]'); if (fe) { fe.hidden = true; fe.textContent = ''; }
  }
  function setError(form, name, msg) {
    var el = form.querySelector('[data-error-for="' + name + '"]');
    if (el) { el.textContent = msg; el.hidden = false; }
    var input = form.querySelector('[name="' + name + '"]');
    if (input) input.setAttribute('aria-invalid', 'true');
  }
  function validate(form) {
    clearErrors(form);
    var ok = true, focusEl = null;
    form.querySelectorAll('[name]').forEach(function (el) {
      if (el.type === 'hidden') return;
      el.removeAttribute('aria-invalid');
      var name = el.getAttribute('name');
      var val = (el.value || '').trim();
      if (el.hasAttribute('required') && !val) { setError(form, name, 'This field is required.'); ok = false; focusEl = focusEl || el; }
      else if (name === 'Email' && val && !validEmail(val)) { setError(form, name, 'Please enter a valid email address.'); ok = false; focusEl = focusEl || el; }
      else if (name === 'WritingText' && el.hasAttribute('required') && val.length < 40) { setError(form, name, 'Please paste your full writing answer (at least 40 characters).'); ok = false; focusEl = focusEl || el; }
    });
    if (focusEl) focusEl.focus();
    return ok;
  }
  function collect(form) {
    var data = {};
    form.querySelectorAll('[name]').forEach(function (el) {
      var v = (el.value || '').trim();
      if (v) data[el.getAttribute('name')] = v;
    });
    // Always include honeypot key so the server can verify it is empty.
    if (!('Website' in data)) data.Website = '';
    return data;
  }
  function endpointFor(form) {
    // writing-feedback posts to its own endpoint; everything else to /submit.
    return (form.querySelector('[name="Type"]') && form.querySelector('[name="Type"]').value === 'WritingFeedback')
      ? '/api/enquiry/writing-feedback' : '/api/enquiry/submit';
  }

  function submitting(btn, on) {
    if (!btn) return;
    btn.disabled = on;
    var label = btn.querySelector('.btn-text');
    if (on) {
      btn.dataset._label = label ? label.textContent : btn.textContent;
      btn.innerHTML = '<span class="spinner" aria-hidden="true"></span><span>Sending…</span>';
    } else if (btn.dataset._label) {
      btn.innerHTML = '<span class="btn-text">' + esc(btn.dataset._label) + '</span>';
    }
  }

  function onSubmit(e) {
    e.preventDefault();
    var form = e.currentTarget;
    if (!validate(form)) return;
    var btn = form.querySelector('button[type="submit"]');
    submitting(btn, true);
    var fe = form.querySelector('[data-form-error]');
    fetch(endpointFor(form), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf() },
      body: JSON.stringify(collect(form))
    }).then(function (r) {
      return r.json().catch(function () { return {}; }).then(function (d) {
        if (!r.ok) throw new Error((d && (d.title || d.error)) || 'Something went wrong. Please try again.');
        return d;
      });
    }).then(function (d) {
      showSuccess(form, d);
    }).catch(function (err) {
      submitting(btn, false);
      if (fe) { fe.textContent = err.message || 'Something went wrong. Please try again.'; fe.hidden = false; }
    });
  }

  function successCard(d) {
    var ref = d && d.referenceNumber ? '<div class="mt-4 inline-flex items-center gap-2 rounded-lg border hairline bg-card px-4 py-2 text-[13px]"><span class="text-ink/55">Reference</span><span class="font-mono font-semibold">' + esc(d.referenceNumber) + '</span></div>' : '';
    return '<div class="px-6 py-10 text-center">' +
      '<div class="w-14 h-14 mx-auto rounded-full bg-teal-50 text-teal-700 flex items-center justify-center mb-4"><svg class="w-7 h-7" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6 9 17l-5-5"/></svg></div>' +
      '<h3 class="font-display text-[22px] font-semibold">Thank you!</h3>' +
      '<p class="mt-2 text-[14px] text-ink/65 max-w-[360px] mx-auto">' + esc((d && d.message) || 'Your request has been received. We will get back to you soon.') + '</p>' +
      ref + '</div>';
  }

  function showSuccess(form, d) {
    if (form.closest('.modal-card')) {
      card.innerHTML =
        '<div class="flex justify-end px-4 pt-4"><button type="button" data-modal-close aria-label="Close" class="w-9 h-9 rounded-full border hairline flex items-center justify-center text-ink/60 hover:text-ink"><svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 6 6 18M6 6l12 12"/></svg></button></div>' +
        successCard(d) +
        '<div class="px-6 pb-6"><button type="button" data-modal-close class="btn-primary w-full"><span class="btn-text">Done</span></button></div>';
    } else {
      // Inline form: replace it with an inline success panel.
      var panel = document.createElement('div');
      panel.className = 'rounded-2xl border hairline bg-card p-6 text-center';
      panel.setAttribute('role', 'status');
      panel.innerHTML =
        '<div class="w-12 h-12 mx-auto rounded-full bg-teal-50 text-teal-700 flex items-center justify-center mb-3"><svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6 9 17l-5-5"/></svg></div>' +
        '<h3 class="font-display text-[20px] font-semibold">Thank you!</h3>' +
        '<p class="mt-1.5 text-[14px] text-ink/65">' + esc((d && d.message) || 'Your request has been received.') + '</p>' +
        (d && d.referenceNumber ? '<p class="mt-3 text-[12.5px] text-ink/55">Reference <span class="font-mono font-semibold text-ink">' + esc(d.referenceNumber) + '</span></p>' : '');
      form.parentNode.replaceChild(panel, form);
    }
  }

  // Inline lead forms (Free Checklist, Contact).
  document.querySelectorAll('form[data-enquiry-form]').forEach(function (form) {
    // Ensure a hidden Type + SourcePage + honeypot are present.
    if (!form.querySelector('[name="Type"]')) {
      var t = document.createElement('input'); t.type = 'hidden'; t.name = 'Type';
      t.value = form.getAttribute('data-enquiry-form'); form.appendChild(t);
    }
    if (!form.querySelector('[name="SourcePage"]')) {
      var s = document.createElement('input'); s.type = 'hidden'; s.name = 'SourcePage'; s.value = SOURCE; form.appendChild(s);
    }
    form.addEventListener('submit', onSubmit);
  });
})();
