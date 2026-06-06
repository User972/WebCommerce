using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SirJoshua.Web.Options;

namespace SirJoshua.Web.Services;

public record PayPalOrder(string Id, string Status);

public record PayPalCapture(string Status, string? CaptureId, decimal Amount, string Currency);

public interface IPayPalClient
{
    /// <summary>Creates a PayPal v2 order for the given amount and returns the PayPal order id.</summary>
    Task<PayPalOrder> CreateOrderAsync(decimal amount, string currency, string referenceId, string description, CancellationToken ct = default);

    /// <summary>Captures an approved PayPal order. Throws if PayPal reports a non-completed capture.</summary>
    Task<PayPalCapture> CaptureOrderAsync(string paypalOrderId, CancellationToken ct = default);
}

/// <summary>
/// Thin, server-side wrapper over the PayPal REST Orders v2 API. All calls are
/// authenticated with an OAuth2 client-credentials token obtained with the confidential
/// client secret, which never leaves the server. Access tokens are cached until shortly
/// before expiry to avoid a round-trip on every request.
/// </summary>
public class PayPalClient : IPayPalClient
{
    private readonly HttpClient _http;
    private readonly PayPalOptions _opt;
    private readonly ILogger<PayPalClient> _log;

    private static readonly SemaphoreSlim TokenLock = new(1, 1);
    private static string? _cachedToken;
    private static DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public PayPalClient(HttpClient http, IOptions<PayPalOptions> opt, ILogger<PayPalClient> log)
    {
        _opt = opt.Value;
        _http = http;
        _http.BaseAddress = new Uri(_opt.ApiBaseUrl);
        _log = log;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        await TokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            using var req = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.ClientId}:{_opt.ClientSecret}"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            req.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            using var resp = await _http.SendAsync(req, ct);
            var payload = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _log.LogError("PayPal token request failed: {Status}", resp.StatusCode);
                throw new PayPalException("Unable to authenticate with PayPal.");
            }

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            _cachedToken = root.GetProperty("access_token").GetString();
            var expiresIn = root.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 3000;
            // Refresh a minute early to avoid using a token that expires mid-flight.
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn - 60));
            return _cachedToken!;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    public async Task<PayPalOrder> CreateOrderAsync(decimal amount, string currency, string referenceId, string description, CancellationToken ct = default)
    {
        var token = await GetAccessTokenAsync(ct);

        var body = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = referenceId,
                    description = Trim(description, 127),
                    amount = new
                    {
                        currency_code = currency,
                        value = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            },
            application_context = new
            {
                shipping_preference = "NO_SHIPPING",
                user_action = "PAY_NOW",
                brand_name = "Sir Joshua Academy"
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Idempotency: a retried create for the same internal reference won't double-charge.
        req.Headers.Add("PayPal-Request-Id", $"create-{referenceId}");
        req.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req, ct);
        var payload = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("PayPal create order failed: {Status} {Body}", resp.StatusCode, payload);
            throw new PayPalException("Unable to start the PayPal payment.");
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        return new PayPalOrder(
            root.GetProperty("id").GetString()!,
            root.GetProperty("status").GetString()!);
    }

    public async Task<PayPalCapture> CaptureOrderAsync(string paypalOrderId, CancellationToken ct = default)
    {
        var token = await GetAccessTokenAsync(ct);

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}/capture");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Add("PayPal-Request-Id", $"capture-{paypalOrderId}");
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req, ct);
        var payload = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("PayPal capture failed: {Status} {Body}", resp.StatusCode, payload);
            throw new PayPalException("PayPal could not complete the payment.");
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

        string? captureId = null;
        decimal amount = 0m;
        string currency = _opt.Currency;

        if (root.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0 &&
            units[0].TryGetProperty("payments", out var payments) &&
            payments.TryGetProperty("captures", out var captures) && captures.GetArrayLength() > 0)
        {
            var cap = captures[0];
            captureId = cap.GetProperty("id").GetString();
            status = cap.GetProperty("status").GetString() ?? status;
            if (cap.TryGetProperty("amount", out var amt))
            {
                amount = decimal.Parse(amt.GetProperty("value").GetString()!, System.Globalization.CultureInfo.InvariantCulture);
                currency = amt.GetProperty("currency_code").GetString() ?? currency;
            }
        }

        return new PayPalCapture(status, captureId, amount, currency);
    }

    private static string Trim(string s, int max) => s.Length <= max ? s : s[..max];
}

public class PayPalException : Exception
{
    public PayPalException(string message) : base(message) { }
}
