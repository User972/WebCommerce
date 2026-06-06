using Microsoft.AspNetCore.Mvc;
using SirJoshua.Web.Services;
using SirJoshua.Web.ViewModels;

namespace SirJoshua.Web.Controllers;

/// <summary>
/// Server-side checkout endpoints. The browser never tells us a price: for both
/// create and capture we recompute the authoritative total from the catalog in the
/// database, so a tampered cart cannot change what is charged. All endpoints require a
/// valid anti-forgery token (enforced globally) and accept JSON only.
/// </summary>
[Route("api/checkout")]
[ApiController]
[Consumes("application/json")]
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly IPayPalClient _paypal;
    private readonly ILogger<CheckoutController> _log;

    public CheckoutController(IOrderService orders, IPayPalClient paypal, ILogger<CheckoutController> log)
    {
        _orders = orders;
        _paypal = paypal;
        _log = log;
    }

    /// <summary>Validates the cart, creates a PayPal order for the server-computed amount, and
    /// records a pending order. Returns the PayPal order id for the JS SDK to approve.</summary>
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        PricedCart priced;
        try
        {
            priced = await _orders.PriceCartAsync(request.Items, request.PromoCode, ct);
        }
        catch (CartValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        try
        {
            var description = $"Sir Joshua ebooks ({priced.Items.Sum(i => i.Quantity)} item)";
            var paypalOrder = await _paypal.CreateOrderAsync(
                priced.AmountUsd, "USD", referenceId: Guid.NewGuid().ToString("N"), description, ct);

            var order = await _orders.CreatePendingOrderAsync(request, priced, paypalOrder.Id, ct);
            _log.LogInformation("Created pending order {OrderNumber} (PayPal {PayPalId})", order.OrderNumber, paypalOrder.Id);

            return Ok(new { id = paypalOrder.Id });
        }
        catch (PayPalException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }

    /// <summary>Captures an approved PayPal order, verifies the settled amount matches what we
    /// recorded, and finalizes the order.</summary>
    [HttpPost("capture-order")]
    public async Task<IActionResult> CaptureOrder([FromBody] CaptureRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var order = await _orders.GetByPayPalOrderIdAsync(request.PayPalOrderId, ct);
        if (order is null)
            return NotFound(new { error = "Pesanan tidak ditemukan." });

        // Idempotent: if a retry hits an already-captured order, just return the confirmation.
        if (order.Status == Models.OrderStatus.Paid)
            return Ok(Confirm(order));

        PayPalCapture capture;
        try
        {
            capture = await _paypal.CaptureOrderAsync(request.PayPalOrderId, ct);
        }
        catch (PayPalException ex)
        {
            await _orders.MarkFailedAsync(order, ct);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }

        if (!string.Equals(capture.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            await _orders.MarkFailedAsync(order, ct);
            return BadRequest(new { error = "Pembayaran tidak selesai." });
        }

        // Defense in depth: confirm PayPal settled the exact amount and currency we created.
        if (capture.Currency != order.Currency || Math.Abs(capture.Amount - order.AmountUsd) > 0.01m)
        {
            _log.LogWarning("Amount mismatch on {OrderNumber}: expected {Expected} {Cur}, captured {Actual} {CapCur}",
                order.OrderNumber, order.AmountUsd, order.Currency, capture.Amount, capture.Currency);
            await _orders.MarkFailedAsync(order, ct);
            return BadRequest(new { error = "Jumlah pembayaran tidak sesuai." });
        }

        await _orders.MarkPaidAsync(order, capture.CaptureId ?? string.Empty, capture.Amount, ct);
        _log.LogInformation("Order {OrderNumber} paid (capture {CaptureId})", order.OrderNumber, capture.CaptureId);

        // TODO: dispatch the ebook download links to order.CustomerEmail via your mail provider.
        return Ok(Confirm(order));
    }

    private static OrderConfirmation Confirm(Models.Order order) => new()
    {
        OrderNumber = order.OrderNumber,
        Email = order.CustomerEmail,
        TotalIdr = order.TotalIdr,
        AmountUsd = order.AmountUsd,
        Currency = order.Currency
    };
}
