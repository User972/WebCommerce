using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.Options;

/// <summary>
/// PayPal + pricing configuration. The client secret must come from a secret store
/// (user-secrets in development, environment variables / a vault in production) and is
/// never sent to the browser. Only <see cref="ClientId"/> is public.
/// </summary>
public class PayPalOptions
{
    public const string SectionName = "PayPal";

    /// <summary>"sandbox" or "live".</summary>
    public string Mode { get; set; } = "sandbox";

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Currency PayPal settles in. PayPal does not support IDR, so we charge in USD.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// IDR per 1 unit of <see cref="Currency"/>, used to convert the IDR cart total into the
    /// PayPal charge amount. Keep this aligned with your real FX/settlement rate in production.
    /// </summary>
    [Range(1, 1_000_000)]
    public decimal IdrPerUsd { get; set; } = 16_000m;

    /// <summary>Single promo code supported by the store, matching the design.</summary>
    public string PromoCode { get; set; } = "SIRJOSHUA10";

    /// <summary>Discount fraction applied when the promo code matches (0.10 = 10%).</summary>
    [Range(0, 1)]
    public decimal PromoRate { get; set; } = 0.10m;

    public bool IsLive => string.Equals(Mode, "live", StringComparison.OrdinalIgnoreCase);

    public string ApiBaseUrl => IsLive
        ? "https://api-m.paypal.com"
        : "https://api-m.sandbox.paypal.com";

    /// <summary>True when real PayPal credentials are configured (vs. placeholder/empty).</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !ClientId.Equals("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        !ClientSecret.Equals("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
}
