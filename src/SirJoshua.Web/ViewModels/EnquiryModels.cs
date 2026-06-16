using System.ComponentModel.DataAnnotations;

namespace SirJoshua.Web.ViewModels;

/// <summary>
/// Payload posted by every funnel form except the writing-feedback submission. A single flexible
/// DTO keeps the client (one enquiry modal + the inline checklist/contact forms) simple; the
/// controller maps <see cref="Type"/> to the right persistence + confirmation message and applies
/// any per-type required-field rules.
/// </summary>
public class EnquiryRequest
{
    /// <summary>One of the LeadType names, e.g. "FreeChecklist", "SpeakingAssessment", "Contact".</summary>
    [Required, MaxLength(40)]
    public string? Type { get; set; }

    [Required(ErrorMessage = "Please enter your name."), StringLength(120, MinimumLength = 2)]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Please enter a valid email address."), EmailAddress, MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(40)]
    public string? WhatsApp { get; set; }

    [MaxLength(80)]
    public string? InterestType { get; set; }

    [MaxLength(64)]
    public string? ProductOrServiceId { get; set; }

    /// <summary>"Academic" or "General Training" — captured on assessment bookings.</summary>
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

    [MaxLength(32)]
    public string? PaymentMethod { get; set; }

    [MaxLength(120)]
    public string? SourcePage { get; set; }

    /// <summary>Honeypot — must stay empty. Real users never see or fill this field. Validated in
    /// the controller (a filled value is silently treated as spam) rather than via an attribute.</summary>
    [MaxLength(200)]
    public string? Website { get; set; }
}

/// <summary>Payload for the IELTS Writing Feedback submission (Service 1).</summary>
public class WritingFeedbackRequest
{
    [Required(ErrorMessage = "Please enter your name."), StringLength(120, MinimumLength = 2)]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Please enter a valid email address."), EmailAddress, MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(40)]
    public string? WhatsApp { get; set; }

    [MaxLength(32)]
    public string? IeltsModule { get; set; }

    [MaxLength(16)]
    public string? TaskType { get; set; }

    [MaxLength(16)]
    public string? TargetBand { get; set; }

    [MaxLength(40)]
    public string? TestDate { get; set; }

    [Required(ErrorMessage = "Please paste your writing answer."), StringLength(12000, MinimumLength = 40)]
    public string? WritingText { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(32)]
    public string? PaymentMethod { get; set; }

    /// <summary>Honeypot — must stay empty. Validated in the controller, not via an attribute.</summary>
    [MaxLength(200)]
    public string? Website { get; set; }
}

/// <summary>Response returned to the browser after a successful enquiry/submission.</summary>
public class EnquiryResult
{
    public bool Ok { get; set; } = true;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
