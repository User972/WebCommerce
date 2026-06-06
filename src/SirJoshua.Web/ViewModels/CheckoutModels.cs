using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.ViewModels;

public class CartLine
{
    [Required, MaxLength(64)]
    public string Id { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Qty { get; set; } = 1;
}

/// <summary>Payload posted by the browser to start a PayPal checkout.</summary>
public class CheckoutRequest
{
    [Required, MinLength(1), MaxLength(50)]
    public List<CartLine> Items { get; set; } = new();

    [Required, StringLength(120, MinimumLength = 2)]
    public string? Name { get; set; }

    [Required, EmailAddress, MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? PromoCode { get; set; }
}

public class CaptureRequest
{
    [Required, MaxLength(64)]
    public string PayPalOrderId { get; set; } = string.Empty;
}

/// <summary>Result returned to the browser after a successful capture.</summary>
public class OrderConfirmation
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalIdr { get; set; }
    public decimal AmountUsd { get; set; }
    public string Currency { get; set; } = "USD";
}

public class HomeViewModel
{
    public List<Models.Ebook> Ebooks { get; set; } = new();
    public string PayPalClientId { get; set; } = string.Empty;
    public string PayPalCurrency { get; set; } = "USD";
    public bool PayPalEnabled { get; set; }
    public decimal IdrPerUsd { get; set; } = 16_000m;
    public string PromoCode { get; set; } = "SIRJOSHUA10";
}
