# IELTS Band 7 Plus™ — Implementation Plan

> Strategic repositioning of the Sir Joshua storefront into a focused, conversion-oriented
> **IELTS Band 7 Plus™** sales & lead-generation experience.
> Sir Joshua Academy remains the parent trust/authority brand; **IELTS Band 7 Plus™** is the
> flagship product funnel.

---

## 1. Current architecture summary

| Area | Detail |
|------|--------|
| Framework | ASP.NET Core 8 MVC (Razor views), `net8.0`, nullable + implicit usings |
| Frontend | Single self-contained view `Views/Home/Index.cshtml` (`Layout = null`, ~1,840 lines). Tailwind via Play CDN + custom design tokens (teal/terra/gold, Fraunces + Plus Jakarta Sans + JetBrains Mono). Inline `<style>` for ebook covers / cart drawer / theme. |
| Behaviour (JS) | All inline in `Index.cshtml`: localStorage cart + PayPal drawer checkout, light/dark theme toggle (`check-theme`), client-side ID⇄EN DOM translation (`check-lang`, default `id`). |
| Backend | `HomeController.Index` → loads `Ebook` catalog → `HomeViewModel`. `CheckoutController` (`/api/checkout/create-order`, `/capture-order`) drives PayPal Orders v2 with **server-authoritative pricing** (`OrderService`). `PayPalClient` wraps the REST API; secret stays server-side. |
| Data | EF Core + PostgreSQL (Npgsql). Entities: `Ebook`, `Order`, `OrderItem`. One migration `InitialCreate`. Catalog seeded in `SeedData`. `db.Database.Migrate()` runs at startup. |
| Security | Global anti-forgery, `SecurityHeadersMiddleware` (CSP scoped to jsDelivr/Google Fonts/PayPal), HSTS, forwarded headers. |
| Content | Indonesian-first. Heavy broad-academy positioning with **unverified claims** (see §7). |

## 2. Strategic approach

* **Keep** the design system, theme/lang toggles, cart + PayPal checkout, server-authoritative pricing.
* **Reprioritise** the homepage around the IELTS Band 7 Plus™ funnel; demote broad-academy content (Programs/Faculty/Locations) — remove the unverifiable parts, fold the rest into "About" / footer.
* **Reframe** the ebook shop as a secondary "Free & paid resources" area; the flagship *Band 7+ Formula* product becomes primary.
* **English-first** landing copy; the existing EN⇄ID dictionary stays available and is extended/cleaned, defaulting to **English** for the new funnel.
* **Centralise** all offer content into typed C# config so prices/inclusions are edited in one place.

## 3. Files to change / add

**Change**
* `Views/Home/Index.cshtml` — becomes a thin shell (head + SEO/OG/JSON-LD, nav, footer, drawers, scripts) that renders section partials.
* `Controllers/HomeController.cs` — build the richer `HomeViewModel`; add `BandSevenFormula` product page action.
* `ViewModels/CheckoutModels.cs` (`HomeViewModel`) — expose content config + new collections.
* `Data/AppDbContext.cs` — register new entities.
* `Middleware/SecurityHeadersMiddleware.cs` — CSP already permits what we need (no new third parties); revisit only if a payment provider script is added.
* `appsettings*.json` — add `Notifications` + `PaymentMethods`/`Site` config sections (no secrets committed).

**Add**
* `Content/SiteContent.cs` (+ `Content/*.cs`) — typed source of truth: `FlagshipProduct`, `ServiceOffer[]`, `LeadMagnet`, `ComingSoonProduct[]`, `PaymentMethod[]`, `SuccessStory[]`, `FaqItem[]`, nav model.
* `Models/Enquiry.cs` — `Lead`, `WritingFeedbackSubmission`, `WritingFeedbackPackage` (+ enums).
* `ViewModels/EnquiryModels.cs` — request/response DTOs with validation.
* `Services/NotificationService.cs` — `INotificationService` (logging impl + TODO for SMTP/SendGrid).
* `Services/EnquiryService.cs` — persists leads/submissions, returns confirmation copy.
* `Controllers/EnquiryController.cs` — `/api/enquiry/submit`, `/api/enquiry/writing-feedback`.
* `Controllers/ProductController.cs` (or Home action) — `/band-7-formula` detail page.
* `Views/Home/Sections/*.cshtml` — section partials.
* `Views/Home/BandSevenFormula.cshtml` — product detail page.
* `wwwroot/js/forms.js` (or inline) — shared enquiry-modal + form submit/validation helper.
* `Data/Migrations/*_AddLeadsAndEnquiries.cs` — generated.

## 4. Data model changes

```
Lead                         WritingFeedbackSubmission        WritingFeedbackPackage
─ Id (guid)                  ─ Id (guid)                      ─ Id (guid)
─ Type (enum)                ─ ReferenceNumber                ─ ReferenceNumber
─ ReferenceNumber            ─ CustomerName / Email / WhatsApp ─ CustomerName / Email
─ FullName / Email / WhatsApp─ IeltsModule / TaskType         ─ TotalCredits (=3)
─ InterestType (string)      ─ TargetBand / TestDate          ─ UsedCredits / RemainingCredits
─ ProductOrServiceId         ─ WritingText / UploadedFileUrl  ─ PaymentStatus
─ CurrentBand / TargetBand   ─ Notes                          ─ Status / CreatedAt
─ TestDate / PreferredDate   ─ PaymentStatus / ReviewStatus
─ PreferredTime / TimeZone   ─ EstimatedBandScore / FeedbackNotes
─ Message / SourcePage       ─ CreatedAt / UpdatedAt
─ PaymentMethod / Status
─ CreatedAt
```
Enums: `LeadType { FreeChecklist, ProductEnquiry, SpeakingAssessment, StrategyConsultation, WritingFeedbackPackage, Waitlist, Contact, General }`, `LeadStatus { New, Contacted, Closed }`, `PaymentState { AwaitingInstructions, Pending, Paid, Failed }`, `ReviewState { Pending, InReview, Delivered }`.

Generated by `dotnet ef migrations add AddLeadsAndEnquiries`. **`database update` is NOT run from here** (prod DB is Azure; `Program.cs` migrates at startup on deploy).

## 5. Routes / pages

| Route | Purpose |
|-------|---------|
| `/` | Reworked Band 7 Plus™ funnel homepage |
| `/band-7-formula` | Flagship product detail page (hero, who-for, learn, problems, includes, price, payment, FAQ, CTA) |
| `POST /api/enquiry/submit` | Generic lead/booking (checklist, product enquiry, speaking, consultation, package, waitlist, contact) |
| `POST /api/enquiry/writing-feedback` | Writing feedback submission (text/optional upload) |
| `POST /api/checkout/*` | Existing PayPal flow (unchanged) |

## 6. Forms

A single reusable **enquiry modal** (data-driven by `formType`) plus two inline forms:
* Inline **Free IELTS Study Checklist** form (free-resource section).
* Inline **Contact** form (interest dropdown).
* Modal forms: Product enquiry, Writing Feedback (Service 1, text/upload), Writing Feedback Package (Service 2), Speaking Assessment (Service 3, scheduling), Strategy Consultation (Service 4, scheduling), Waitlist.

All forms: accessible labels, client + server validation, loading/success/error states, anti-forgery header, honeypot spam field. Success copy per brief.

## 7. Payment handling

No automated multi-rail checkout exists — only PayPal (sandbox, placeholder creds). Approach = **config-driven, enquiry-based** (Scenario B/C):
* `PaymentMethod[]` config: Stripe, PayPal, Wise, Visa, Mastercard, QRIS — each with `Enabled` + `Instructions`.
* The flagship product can still use the existing **PayPal** cart/checkout when configured; otherwise (and for Wise/QRIS/bank-transfer) the CTA becomes **"Request payment link / instructions"**, capturing the selected product + method as a `Lead`.
* No secrets in client code; PayPal client-id already injected server-side; keys via env/secret store only.

## 8. Risks / assumptions

* **Claims safety:** existing partner/credential/testimonial/branch/stat claims are unverified → removed or softened; documented for customer confirmation.
* **Brand:** assume Band 7 Plus™ sits *under* Sir Joshua Academy (brief's recommended default).
* **Language:** assume **English** for the new funnel; ID dictionary retained but not primary.
* **Email:** no mail provider → leads stored in DB + logged; checklist delivery is a TODO integration point.
* **Payments:** assume manual/enquiry-based settlement until the customer confirms which rails are live.
* **EF tool** is 9.x vs project 8.x — migrations generated against the project's own EF 8 assemblies; verify the generated snapshot diff.
* `™` is treated as a brand mark in copy only — no legal/trademark assertion added.

## 9. Testing checklist

* Build passes (`dotnet build`), no new warnings/errors, migration compiles.
* Nav + all CTAs route/scroll correctly; hero → product, services → forms, free-resource → form, final-CTA three paths.
* Each form: validation, loading, success, error; lead persists; success copy matches brief.
* Pricing matches brief (Formula 599k→399k; services 249k/599k/399k/599k).
* No unverified claims remain; no ID/EN mix in primary funnel.
* Responsive desktop / tablet / mobile / small-mobile; theme light/dark; keyboard nav + alt text + heading order.
* SEO title/meta/OG/JSON-LD present and correct; product page metadata present.

## 10. Customer confirmations required (carried to summary)

Brand replacement vs nesting · live payment rails (Stripe/PayPal/Wise/Visa/MC/QRIS) · automated vs manual settlement · reality of testimonials/teachers/branches/stats · trademark status of "IELTS Band 7 Plus™" · language (EN/ID/bilingual) · instant checklist email · admin notification recipient · writing upload vs text · calendar scheduling vs request-based.
