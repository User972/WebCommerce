using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.Models;

public enum OrderStatus
{
    /// <summary>A PayPal order was created and is awaiting buyer approval/capture.</summary>
    Pending = 0,
    /// <summary>Payment captured successfully.</summary>
    Paid = 1,
    /// <summary>Capture failed or was declined.</summary>
    Failed = 2
}

/// <summary>
/// A customer order. All monetary amounts are recomputed on the server from the
/// <see cref="Ebook"/> catalog at the moment the order is created; values that arrive from
/// the browser are never used for pricing. The PayPal charge is settled in USD (PayPal does
/// not support IDR), converted from the IDR total at a configured rate.
/// </summary>
public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Human-friendly reference shown to the customer, e.g. "SJ-3F9K2".</summary>
    [Required, MaxLength(24)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public int SubtotalIdr { get; set; }
    public int DiscountIdr { get; set; }
    public int TotalIdr { get; set; }

    [MaxLength(32)]
    public string? PromoCode { get; set; }

    /// <summary>Amount actually charged via PayPal, in USD with 2 decimals.</summary>
    public decimal AmountUsd { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; } = "USD";

    /// <summary>PayPal order id (v2 Orders API).</summary>
    [MaxLength(64)]
    public string? PayPalOrderId { get; set; }

    /// <summary>PayPal capture id once payment settles.</summary>
    [MaxLength(64)]
    public string? PayPalCaptureId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    [Required, MaxLength(64)]
    public string EbookId { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Unit price captured at purchase time, in whole IDR.</summary>
    public int UnitPriceIdr { get; set; }

    public int Quantity { get; set; }
}
