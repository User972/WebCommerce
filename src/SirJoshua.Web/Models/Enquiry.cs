using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.Models;

public enum LeadType
{
    FreeChecklist = 0,
    ProductEnquiry = 1,
    SpeakingAssessment = 2,
    StrategyConsultation = 3,
    WritingFeedbackPackage = 4,
    Waitlist = 5,
    Contact = 6,
    General = 7,
}

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Closed = 2,
}

/// <summary>Where a record is in its (manual) payment lifecycle. No card data is ever stored.</summary>
public enum PaymentState
{
    AwaitingInstructions = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
}

public enum ReviewState
{
    Pending = 0,
    InReview = 1,
    Delivered = 2,
}

/// <summary>
/// A generic lead / booking / enquiry captured from any of the funnel forms (free checklist,
/// product enquiry, speaking assessment, strategy consultation, package request, waitlist or the
/// contact form). All values are validated server-side before persistence; free-text is length
/// limited. This is intentionally one flexible table rather than seven near-identical ones.
/// </summary>
public class Lead
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Short human-friendly reference shown to the customer, e.g. "B7-3F9K2".</summary>
    [Required, MaxLength(24)]
    public string ReferenceNumber { get; set; } = string.Empty;

    public LeadType Type { get; set; } = LeadType.General;

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? WhatsApp { get; set; }

    /// <summary>Human label of what the lead is interested in (service/product/contact interest).</summary>
    [MaxLength(80)]
    public string? InterestType { get; set; }

    /// <summary>Stable id of the related product or service, when applicable.</summary>
    [MaxLength(64)]
    public string? ProductOrServiceId { get; set; }

    /// <summary>"Academic" or "General Training" — relevant for assessment bookings.</summary>
    [MaxLength(32)]
    public string? IeltsModule { get; set; }

    [MaxLength(16)]
    public string? CurrentBand { get; set; }

    [MaxLength(16)]
    public string? TargetBand { get; set; }

    [MaxLength(40)]
    public string? TestDate { get; set; }

    [MaxLength(40)]
    public string? PreferredDate { get; set; }

    [MaxLength(40)]
    public string? PreferredTime { get; set; }

    [MaxLength(64)]
    public string? TimeZone { get; set; }

    [MaxLength(60)]
    public string? FocusArea { get; set; }

    [MaxLength(2000)]
    public string? Message { get; set; }

    /// <summary>Customer-selected payment method id (e.g. "paypal", "wise"); never card data.</summary>
    [MaxLength(32)]
    public string? PaymentMethod { get; set; }

    public PaymentState PaymentStatus { get; set; } = PaymentState.AwaitingInstructions;

    /// <summary>Page the enquiry was submitted from, for attribution.</summary>
    [MaxLength(120)]
    public string? SourcePage { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// A single IELTS Writing Feedback submission (Service 1). Holds the candidate's writing for
/// review plus the fields a rater needs; <see cref="FeedbackNotes"/> and
/// <see cref="EstimatedBandScore"/> are filled in by the team once reviewed.
/// </summary>
public class WritingFeedbackSubmission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(24)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? WhatsApp { get; set; }

    /// <summary>"Academic" or "General Training".</summary>
    [MaxLength(32)]
    public string? IeltsModule { get; set; }

    /// <summary>"Task 1" or "Task 2".</summary>
    [MaxLength(16)]
    public string? TaskType { get; set; }

    [MaxLength(16)]
    public string? TargetBand { get; set; }

    [MaxLength(40)]
    public string? TestDate { get; set; }

    [MaxLength(12000)]
    public string? WritingText { get; set; }

    /// <summary>Path/URL to an optional uploaded file (stored outside the web root).</summary>
    [MaxLength(400)]
    public string? UploadedFileUrl { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(32)]
    public string? PaymentMethod { get; set; }

    public PaymentState PaymentStatus { get; set; } = PaymentState.AwaitingInstructions;
    public ReviewState ReviewStatus { get; set; } = ReviewState.Pending;

    [MaxLength(16)]
    public string? EstimatedBandScore { get; set; }

    [MaxLength(4000)]
    public string? FeedbackNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Tracks an IELTS Writing Feedback Package (Service 2): three writing-review credits the customer
/// can spend over time. Created when a package is requested; credits are decremented as reviews are
/// delivered by the team.
/// </summary>
public class WritingFeedbackPackage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(24)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(254)]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? WhatsApp { get; set; }

    public int TotalCredits { get; set; } = 3;
    public int UsedCredits { get; set; }

    /// <summary>Computed convenience — not mapped redundantly; persisted for easy admin queries.</summary>
    public int RemainingCredits { get; set; } = 3;

    [MaxLength(32)]
    public string? PaymentMethod { get; set; }

    public PaymentState PaymentStatus { get; set; } = PaymentState.AwaitingInstructions;
    public LeadStatus Status { get; set; } = LeadStatus.New;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
