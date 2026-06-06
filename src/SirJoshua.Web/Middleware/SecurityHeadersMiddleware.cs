namespace SirJoshua.Web.Middleware;

/// <summary>
/// Adds a baseline set of defensive HTTP response headers, including a Content-Security-Policy.
///
/// The CSP is scoped to exactly the third parties this page needs: the Tailwind Play CDN
/// (jsDelivr), Google Fonts, and PayPal. Note it includes 'unsafe-inline'/'unsafe-eval' for
/// scripts because the design ships inline scripts and the Tailwind Play CDN compiles styles
/// at runtime via eval. For a hardened production build you would precompile Tailwind to a
/// static stylesheet and serve scripts with per-request nonces, which lets you drop both
/// 'unsafe-*' tokens.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    private const string Csp =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://fonts.gstatic.com data:; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://www.paypal.com https://www.paypalobjects.com; " +
        "connect-src 'self' https://www.paypal.com https://api-m.paypal.com https://api-m.sandbox.paypal.com; " +
        "frame-src https://www.paypal.com https://www.sandbox.paypal.com;";

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["Content-Security-Policy"] = Csp;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(self)";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        // Remove server fingerprinting if present.
        headers.Remove("X-Powered-By");

        await _next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
