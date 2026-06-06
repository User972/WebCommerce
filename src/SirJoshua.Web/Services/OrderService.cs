using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SirJoshua.Web.Data;
using SirJoshua.Web.Models;
using SirJoshua.Web.Options;
using SirJoshua.Web.ViewModels;

namespace SirJoshua.Web.Services;

public record PricedCart(int SubtotalIdr, int DiscountIdr, int TotalIdr, decimal AmountUsd, string? PromoCode, List<OrderItem> Items);

public interface IOrderService
{
    /// <summary>
    /// Validates the requested line items against the catalog and computes every amount on
    /// the server. Throws <see cref="CartValidationException"/> if the cart is empty or
    /// references unknown products.
    /// </summary>
    Task<PricedCart> PriceCartAsync(IEnumerable<CartLine> lines, string? promoCode, CancellationToken ct = default);

    Task<Order> CreatePendingOrderAsync(CheckoutRequest request, PricedCart priced, string paypalOrderId, CancellationToken ct = default);

    Task<Order?> GetByPayPalOrderIdAsync(string paypalOrderId, CancellationToken ct = default);

    Task MarkPaidAsync(Order order, string captureId, decimal amountUsd, CancellationToken ct = default);

    Task MarkFailedAsync(Order order, CancellationToken ct = default);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly PayPalOptions _opt;

    public OrderService(AppDbContext db, IOptions<PayPalOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<PricedCart> PriceCartAsync(IEnumerable<CartLine> lines, string? promoCode, CancellationToken ct = default)
    {
        // Collapse duplicate ids and clamp quantities to a sane range.
        var requested = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var line in lines ?? Enumerable.Empty<CartLine>())
        {
            if (string.IsNullOrWhiteSpace(line.Id)) continue;
            var qty = Math.Clamp(line.Qty, 1, 99);
            requested[line.Id] = requested.TryGetValue(line.Id, out var existing)
                ? Math.Clamp(existing + qty, 1, 99)
                : qty;
        }

        if (requested.Count == 0)
            throw new CartValidationException("Keranjang kosong.");

        var ids = requested.Keys.ToList();
        var products = await _db.Ebooks
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, ct);

        if (products.Count != requested.Count)
            throw new CartValidationException("Ada produk yang tidak ditemukan.");

        var items = new List<OrderItem>();
        var subtotal = 0;
        foreach (var (id, qty) in requested)
        {
            var p = products[id];
            subtotal += p.PriceIdr * qty;
            items.Add(new OrderItem
            {
                EbookId = p.Id,
                Title = p.Title,
                UnitPriceIdr = p.PriceIdr,
                Quantity = qty
            });
        }

        // Promo validation happens on the server; the code from the client is only a hint.
        string? appliedPromo = null;
        var discount = 0;
        if (!string.IsNullOrWhiteSpace(promoCode) &&
            string.Equals(promoCode.Trim(), _opt.PromoCode, StringComparison.OrdinalIgnoreCase))
        {
            appliedPromo = _opt.PromoCode;
            discount = (int)Math.Round(subtotal * _opt.PromoRate, MidpointRounding.AwayFromZero);
        }

        var total = subtotal - discount;
        var amountUsd = Math.Round(total / _opt.IdrPerUsd, 2, MidpointRounding.AwayFromZero);
        // PayPal rejects a zero/negative charge; enforce a small floor.
        if (amountUsd < 0.01m) amountUsd = 0.01m;

        return new PricedCart(subtotal, discount, total, amountUsd, appliedPromo, items);
    }

    public async Task<Order> CreatePendingOrderAsync(CheckoutRequest request, PricedCart priced, string paypalOrderId, CancellationToken ct = default)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerName = request.Name!.Trim(),
            CustomerEmail = request.Email!.Trim(),
            Status = OrderStatus.Pending,
            SubtotalIdr = priced.SubtotalIdr,
            DiscountIdr = priced.DiscountIdr,
            TotalIdr = priced.TotalIdr,
            PromoCode = priced.PromoCode,
            AmountUsd = priced.AmountUsd,
            Currency = _opt.Currency,
            PayPalOrderId = paypalOrderId,
            Items = priced.Items
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public Task<Order?> GetByPayPalOrderIdAsync(string paypalOrderId, CancellationToken ct = default) =>
        _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.PayPalOrderId == paypalOrderId, ct);

    public async Task MarkPaidAsync(Order order, string captureId, decimal amountUsd, CancellationToken ct = default)
    {
        order.Status = OrderStatus.Paid;
        order.PayPalCaptureId = captureId;
        order.AmountUsd = amountUsd;
        order.PaidAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Order order, CancellationToken ct = default)
    {
        order.Status = OrderStatus.Failed;
        await _db.SaveChangesAsync(ct);
    }

    private static string GenerateOrderNumber()
    {
        // Short, unambiguous, hard to guess (no sequential ids leaked to customers).
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> chars = stackalloc char[5];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        return $"SJ-{new string(chars)}";
    }
}

public class CartValidationException : Exception
{
    public CartValidationException(string message) : base(message) { }
}
