# IELTS Band 7 Plus™ — Implementation Summary

Repositioned the Sir Joshua storefront into a focused, conversion-oriented **IELTS Band 7 Plus™**
sales & lead-generation funnel. Sir Joshua Academy remains the parent trust/authority brand; the
**Band 7+ Formula** product and paid services are the conversion core. Implemented on the existing
ASP.NET Core 8 MVC + EF Core (PostgreSQL) + PayPal stack, reusing the original premium design system.

---

## 1. What changed & why

| Area | Before | After |
|------|--------|-------|
| Positioning | Broad IELTS/TOEFL/PTE academy, Indonesian-first | Focused **IELTS Band 7 Plus™** funnel, English-first |
| Homepage | One 1,840-line view, academy sections | Thin shell rendering 13 section partials |
| Offer | Ebook shop only | Flagship product + 4 paid services + free lead magnet + coming-soon |
| Lead capture | PayPal checkout only | Enquiry modal + inline checklist/contact forms → DB + notifications |
| Claims | Unverified partners/credentials/testimonials/branches/stats | Removed/softened; claim-safe throughout |
| Nav | Programs/Teachers/Locations/Ebook/FAQ | Home · The Band 7+ Formula · Services · Success Stories · Free Resources · About Sir Joshua · Contact |
| Content | Hard-coded in the view | Centralized in typed `Content/SiteContent.cs` |

The funnel reuses the proven server-authoritative PayPal pricing for the flagship product, and adds a
config-driven, enquiry-based path for every other payment method.

## 2. The funnel (how it works)

1. **Hero** → primary CTA scrolls to the flagship product; secondary opens the Strategy Consultation form; tertiary → free checklist.
2. **Problem** (6 pain points) → solution band → product / services.
3. **Flagship product** (`#formula`): launch price IDR 399,000 (was 599,000). "Get the Band 7+ Formula" adds it to the **PayPal cart**; "Request payment link" opens the enquiry modal (captures a lead with chosen method). Full detail page at **`/band-7-formula`** (alias `/ielts-writing-mastery`).
4. **Services** (`#services`): 4 cards; each CTA opens the enquiry modal pre-filled for that service.
5. **Free checklist** (`#free-checklist`): inline lead form → `Lead` (type `FreeChecklist`).
6. **Coming soon**: 4 future products, each with a **Join Waitlist** enquiry (no purchase).
7. **Success stories** (`#success`): safe outcome framing + results disclaimer (no fabricated testimonials).
8. **About Sir Joshua** (`#about`): authority copy, no invented credentials.
9. **Resources** (`#resources`): the reframed secondary practice library (PayPal cart).
10. **Payment** (`#payment`): config-driven method list; PayPal = instant checkout, others = secure payment link.
11. **FAQ**, **Final CTA** (3 paths), **Contact** (interest dropdown + form).

All forms: client + server validation, loading/success/error states, accessible labels, anti-forgery
header, and a honeypot. Submissions persist to the DB and fire an admin notification (logged today;
swap in email behind `INotificationService`).

## 3. New / changed files

**Backend (C#)**
* `Content/SiteContent.cs` — single source of truth: flagship product, services, lead magnet, coming-soon, payment methods, success outcomes, general FAQ, nav, interests, `Money.Rp` formatter.
* `Content/StructuredData.cs` — schema.org JSON-LD (Organization + Product + Services), HTML-safe.
* `Models/Enquiry.cs` — `Lead`, `WritingFeedbackSubmission`, `WritingFeedbackPackage` (+ enums).
* `ViewModels/EnquiryModels.cs` — `EnquiryRequest`, `WritingFeedbackRequest`, `EnquiryResult`.
* `Services/EnquiryService.cs` — persists submissions, returns the brief's confirmation copy.
* `Services/NotificationService.cs` + `Options/NotificationOptions.cs` — logging notifier (+ email TODO).
* `Controllers/EnquiryController.cs` — `POST /api/enquiry/submit`, `POST /api/enquiry/writing-feedback`.
* `Controllers/HomeController.cs` — richer view model; `/band-7-formula` (+ alias) action.
* `Data/AppDbContext.cs` — registers the 3 new entities; `Data/SeedData.cs` — flagship row + English library.
* `Data/AppDbContextFactory.cs` — design-time factory (keeps the EF CLI off the production DB).
* `Data/Migrations/*_AddLeadsAndEnquiries.cs` — new tables + catalog reseed.
* `Program.cs` — DI for enquiry + notification services and `NotificationOptions`.

**Frontend**
* `Views/Home/Index.cshtml` — shell (SEO/OG/JSON-LD, nav, footer, partials, drawers, scripts).
* `Views/Home/BandSevenFormula.cshtml` — product detail page.
* `Views/Home/Sections/*.cshtml` — 13 section partials (`_Hero … _Contact`).
* `Views/Shared/_CartDrawer.cshtml`, `_EnquiryModal.cshtml` — shared, reused by both pages.
* `wwwroot/css/b7plus.css` — funnel design system (extracted + extended).
* `wwwroot/js/funnel.js` — cart + PayPal checkout + theme + mobile menu.
* `wwwroot/js/forms.js` — enquiry modal + inline form engine (validation, states, anti-forgery).
* `Views/_ViewImports.cshtml` — global usings for `Content` + `ViewModels`.
* `appsettings.json` — added `Notifications` section.

## 4. New routes / pages / forms / data

* **Routes:** `/band-7-formula` (+ `/ielts-writing-mastery`); `POST /api/enquiry/submit`; `POST /api/enquiry/writing-feedback`.
* **Forms:** enquiry modal (product, writing-feedback, writing-package, speaking-assessment, strategy-consultation, waitlist) + inline Free Checklist + inline Contact.
* **Data model:** `Lead` (generic enquiry/booking), `WritingFeedbackSubmission` (Service 1), `WritingFeedbackPackage` (Service 2, 3 credits).

## 5. Payment implementation status

Config-driven, **enquiry-based** (the customer's registered rails are unconfirmed):
* **PayPal** = real instant checkout when credentials are set (server-authoritative pricing, unchanged). Currently sandbox placeholder (`CHANGE_ME`), so the UI shows "confirmed via secure payment link".
* **Stripe / Wise / Visa / Mastercard / QRIS** = displayed as accepted; settled via a "Request payment link" enquiry that records the selected method. No card data is ever collected or stored. Methods live in `SiteContent.PaymentMethods` — edit there to enable/disable or add instructions.

## 6. How to update things later

* **Prices / copy / inclusions / CTAs:** edit `Content/SiteContent.cs` (flagship in `SeedData.cs` too — it backs PayPal pricing; change both then add an EF migration).
* **Payment methods:** `SiteContent.PaymentMethods` (`SupportsCheckout` = has live checkout).
* **Admin notification inbox:** `Notifications:AdminEmail` in config.
* **Email delivery (checklist, confirmations):** implement `INotificationService` with SMTP/SendGrid — the UI copy already promises email, so it becomes literally true with no view change.

## 7. Testing performed

* `dotnet build` — **0 warnings / 0 errors** (Razor views compile at build).
* Ran the app against a **throwaway local Postgres** (never the production DB) — both pages render **200**; structural check: **1 `<h1>`**, 13 `aria-labelledby` sections, **0 images without alt**, **no broken in-page anchors**.
* Exercised every enquiry endpoint with anti-forgery: checklist, writing feedback, speaking, package, strategy, contact → 200 with the brief's exact confirmation messages; DB rows persisted; `WritingFeedbackPackage` created with **3 credits**; **honeypot spam discarded (0 persisted)**; missing CSRF → 400; invalid email / short writing → 400.
* Migration verified: flagship seeded `band-7-formula @ 399,000 (compareAt 599,000)`; off-brand TOEFL/PTE rows removed.
* Multi-agent adversarial review (17 agents) across claims-safety, brief-compliance, accessibility, forms-wiring, responsive/CSS, and code-quality lenses, with adversarial verification of each finding. Confirmed fixes applied and re-verified:
  * **Data-loss bug:** the paid Speaking Assessment booking silently dropped the Academic/General-Training choice (`IeltsModule` missing from `EnquiryRequest`/`Lead`). Added the field end-to-end — re-verified it now persists (`IeltsModule = Academic`).
  * **Accessibility:** added a skip link + `<main>` landmark (both pages); focus trap + background `inert` for the enquiry modal and the cart drawer (focus enters, is trapped, and is restored on close); `role="dialog"` on the cart drawer.
  * **Responsive:** fixed the WhatsApp FAB / mobile sticky-CTA overlap on tablets and added body bottom-padding so the sticky bar no longer hides the footer; product-page back-nav now reachable on the smallest screens.
  * **Claims-safety:** zeroed all catalog ratings/review counts and removed "Best seller"/"New" badges (re-verified 0 fake ratings, 0 badges in the DB).
  * **English-only:** translated the remaining Indonesian checkout/cart error strings.
  * **Security:** see §9.

## 8. Known limitations

* No email provider wired — leads are stored + logged; checklist delivery is a documented TODO.
* Writing-feedback accepts pasted text (secure); file upload is a documented TODO (no AV scanning infra yet).
* Bookings are request-based, not calendar-integrated.
* Tailwind via the Play CDN (matches the original); precompiling Tailwind + nonce'd scripts would let the CSP drop `unsafe-inline`/`unsafe-eval` (documented in `SecurityHeadersMiddleware`).
* Legacy Indonesian→English client dictionary removed (the funnel is English-only) to avoid mixed-language CTAs; proper i18n (resource files) is the recommended path if bilingual is wanted.
* Privacy/Terms footer links are placeholders (`#`).

## 9. ⚠ Security note (action taken + customer follow-up)

When this work started, the **working tree** of `appsettings.json` held a **live Azure PostgreSQL
connection string with username + password** (the committed HEAD value is a safe localhost dev
placeholder). To prevent the secret from being committed alongside these changes, the connection
string has been **restored to the safe dev placeholder**. The real connection string should be
supplied at runtime via Azure App Service configuration / an environment variable
(`ConnectionStrings__DefaultConnection`) — `Program.cs` already reads it through `GetConnectionString`.

**Customer follow-up (important):** the exposed Azure PostgreSQL password should be treated as
compromised and **rotated**, and if it ever reached a commit, scrubbed from git history
(`git filter-repo` / BFG). The CSP and EF tooling are configured so the design-time CLI cannot reach
the production database.

## 10. Customer confirmations required

1. Should IELTS Band 7 Plus™ fully replace Sir Joshua Academy branding, or sit under it? *(Implemented: nested under the academy — the brief's recommended default.)*
2. Are Stripe, PayPal, Wise, Visa, Mastercard and QRIS actually available to the business? *(Shown as accepted; only PayPal has live checkout.)*
3. Payments fully automated now, or via payment links / manual confirmation? *(Implemented: enquiry-based + PayPal-when-configured.)*
4. Are any existing testimonials / teacher profiles / branch locations / score stats real and approved? *(Removed as unverified — re-add once approved.)*
5. Is "IELTS Band 7 Plus™" a registered trademark or just a brand name? *(Used as a brand mark only — no legal claim added.)*
6. English-only, Indonesian-only, or bilingual? *(Implemented English-only.)*
7. Should the free checklist be emailed instantly? *(Needs an email provider — currently stored + logged.)*
8. Who receives admin notifications for purchases/bookings/submissions? *(`Notifications:AdminEmail`.)*
9. Writing submissions: file upload, text, or both? *(Text now; upload is a TODO.)*
10. Should speaking assessments / consultations integrate with calendar scheduling now, or stay request-based? *(Request-based.)*

> Also needed: real WhatsApp number and contact email (placeholders `6281100000000` / `hello@sirjoshua.id`
> live in `SiteContent.cs`), and a real OG share image.
