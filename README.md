# Sir Joshua Academy — WebCommerce

An ASP.NET Core 8 MVC application implementing the **Check Homepage** design from
[Claude Design](https://claude.ai/design), rebranded as **Sir Joshua Academy**: an
IELTS / TOEFL / PTE course site with an integrated ebook store, cart, and a secure
**PayPal** checkout. Data is persisted in **PostgreSQL** via Entity Framework Core.

The homepage is a pixel-faithful recreation of `Check Homepage.html` (editorial teal /
terracotta / cream palette, Fraunces + Plus Jakarta Sans, light/dark theme toggle,
EN ⇄ ID language toggle, ebook showcase, slide-over cart drawer). The static prototype's
mock checkout has been replaced with a real, server-driven PayPal payment flow.

## Stack

| Concern            | Choice                                             |
|--------------------|----------------------------------------------------|
| Framework          | ASP.NET Core 8 MVC                                  |
| Database           | PostgreSQL 16 (Npgsql + EF Core 8)                  |
| Payments           | PayPal REST **Orders v2** (server-side capture)     |
| Front-end          | Tailwind (Play CDN) + vanilla JS, as in the design |

## Project layout

```
src/SirJoshua.Web/
  Controllers/
    HomeController.cs        # renders the homepage (ebooks loaded from the DB)
    CheckoutController.cs     # POST create-order / capture-order (JSON API)
  Data/
    AppDbContext.cs, SeedData.cs, Migrations/
  Models/                     # Ebook, Order, OrderItem
  Options/PayPalOptions.cs    # PayPal + pricing configuration
  Services/
    PayPalClient.cs           # OAuth + Orders v2 create/capture
    OrderService.cs           # server-side re-pricing + order persistence
  Middleware/SecurityHeadersMiddleware.cs
  ViewModels/CheckoutModels.cs
  Views/Home/Index.cshtml     # the full homepage
```

## Running locally

### 1. PostgreSQL

```bash
# Create the database and an app user (adjust credentials as you like)
sudo -u postgres psql -c "CREATE DATABASE sirjoshua;"
sudo -u postgres psql -c "CREATE USER sirjoshua_app WITH PASSWORD 'change-me';"
sudo -u postgres psql -c "ALTER DATABASE sirjoshua OWNER TO sirjoshua_app;"
```

The connection string lives in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
For anything but local dev, override it with an environment variable or user-secret rather
than editing the file.

### 2. PayPal credentials (sandbox)

Create a sandbox app at <https://developer.paypal.com/dashboard/> and supply the
**Client ID** and **Secret**. Never commit them — use user-secrets in development:

```bash
cd src/SirJoshua.Web
dotnet user-secrets set "PayPal:ClientId" "<your-sandbox-client-id>"
dotnet user-secrets set "PayPal:ClientSecret" "<your-sandbox-secret>"
```

In production, provide them as environment variables instead:
`PayPal__ClientId`, `PayPal__ClientSecret`, `ConnectionStrings__DefaultConnection`.

> Until real credentials are configured the checkout drawer shows a friendly
> "checkout not yet active" notice instead of the PayPal buttons, so the rest of the
> site is fully usable without them.

### 3. Run

```bash
cd src/SirJoshua.Web
dotnet run        # migrations are applied automatically at startup
```

Then open the printed URL (e.g. `https://localhost:7xxx`).

## How the PayPal checkout works

1. **Add to cart** → items live in `localStorage` (display only).
2. **Checkout** → the buyer enters name + email; the PayPal Smart Buttons render.
3. **Create order** (`POST /api/checkout/create-order`) — the server **ignores any prices
   from the browser**, looks every line item up in the database, recomputes subtotal /
   discount / total, converts the IDR total to USD, asks PayPal to create an Orders-v2
   order for that exact amount, and records a `Pending` order.
4. **Approve** in the PayPal popup.
5. **Capture** (`POST /api/checkout/capture-order`) — the server captures the PayPal order,
   verifies the **settled amount and currency match** what it created, and marks the order
   `Paid`. (Emailing the download links is left as a clearly-marked `TODO`.)

> **Currency note:** PayPal does not support IDR, so the catalog is priced in IDR and the
> charge is settled in **USD**, converted at `PayPal:IdrPerUsd` (default 16 000). Align this
> with your real settlement rate in production.

## Security practices applied

- **Server is the pricing authority.** Amounts are recomputed from the DB on every
  create *and* re-verified against PayPal's captured amount; a tampered cart cannot change
  what is charged.
- **Anti-forgery tokens** required on every state-changing request (`AutoValidateAntiforgeryToken`
  globally; token sent via the `X-CSRF-TOKEN` header on fetch calls).
- **Secrets stay out of source** — only the public `ClientId` reaches the browser; the secret is
  used solely server-side via a confidential OAuth client-credentials grant.
- **HTTPS redirection + HSTS** (preload, 1-year, include-subdomains) and
  `UseForwardedHeaders` for correct scheme behind a TLS-terminating proxy.
- **Security headers** on every response: a scoped `Content-Security-Policy`,
  `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`,
  `Permissions-Policy`, `Cross-Origin-Opener-Policy`.
- **Model validation** on all inputs; **parameterized queries** throughout (EF Core);
  **idempotent** PayPal create/capture (via `PayPal-Request-Id` and a paid-order short-circuit);
  non-sequential, hard-to-guess order numbers.

### Known hardening trade-off

The CSP allows `'unsafe-inline'`/`'unsafe-eval'` for scripts because the design ships inline
scripts and the **Tailwind Play CDN compiles styles at runtime via `eval`**. For a hardened
production build, precompile Tailwind to a static stylesheet and serve scripts with
per-request nonces — that lets you drop both `'unsafe-*'` tokens. This is documented inline
in `SecurityHeadersMiddleware.cs`.

## Database migrations

```bash
cd src/SirJoshua.Web
dotnet ef migrations add <Name>
dotnet ef database update
```

The app also calls `Database.Migrate()` on startup, so a fresh database is created and seeded
(7 ebooks) automatically on first run.
