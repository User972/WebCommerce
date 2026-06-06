using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.Models;

/// <summary>
/// A purchasable ebook in the Sir Joshua store. Prices are stored as a whole number of
/// Indonesian Rupiah (IDR has no minor unit in practice), which avoids any floating-point
/// rounding on money. This catalog is the server-side source of truth for pricing — the
/// browser cart is only a convenience and is never trusted for amounts.
/// </summary>
public class Ebook
{
    /// <summary>Stable slug used as the public id (e.g. "writing-playbook").</summary>
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Price in whole IDR.</summary>
    public int PriceIdr { get; set; }

    /// <summary>IELTS, TOEFL, PTE, Umum, or Bundle.</summary>
    [Required, MaxLength(32)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Designed cover colorway key: teal, terra, ink, deep, gold, sand.</summary>
    [MaxLength(16)]
    public string Cover { get; set; } = "teal";

    /// <summary>Series label shown on the designed cover (e.g. "IELTS · Writing").</summary>
    [MaxLength(64)]
    public string Series { get; set; } = string.Empty;

    /// <summary>Meta line such as "248 hal · PDF + EPUB".</summary>
    [MaxLength(48)]
    public string Meta { get; set; } = string.Empty;

    public decimal Rating { get; set; }

    public int ReviewCount { get; set; }

    /// <summary>Optional badge: "Terlaris", "Baru", or null.</summary>
    [MaxLength(16)]
    public string? Badge { get; set; }

    /// <summary>Bundles render as the wide banner rather than a card.</summary>
    public bool IsBundle { get; set; }

    /// <summary>Strike-through original price for bundles (whole IDR), if any.</summary>
    public int? CompareAtIdr { get; set; }

    /// <summary>Controls grid ordering.</summary>
    public int SortOrder { get; set; }
}
