using System.Security.Cryptography;
using SirJoshua.Web.Content;
using SirJoshua.Web.Data;
using SirJoshua.Web.Models;
using SirJoshua.Web.ViewModels;

namespace SirJoshua.Web.Services;

public interface IEnquiryService
{
    Task<EnquiryResult> CaptureLeadAsync(EnquiryRequest request, CancellationToken ct = default);
    Task<EnquiryResult> CaptureWritingFeedbackAsync(WritingFeedbackRequest request, CancellationToken ct = default);
}

/// <summary>
/// Persists funnel form submissions and returns the customer-facing confirmation copy for each
/// type. Notifications are fired through <see cref="INotificationService"/>; if a mail provider is
/// added later the messaging here ("we'll email you") becomes literally true with no UI change.
/// </summary>
public class EnquiryService : IEnquiryService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notify;
    private readonly ILogger<EnquiryService> _log;

    public EnquiryService(AppDbContext db, INotificationService notify, ILogger<EnquiryService> log)
    {
        _db = db;
        _notify = notify;
        _log = log;
    }

    public async Task<EnquiryResult> CaptureLeadAsync(EnquiryRequest request, CancellationToken ct = default)
    {
        var type = ParseType(request.Type);
        var reference = GenerateReference();
        var name = request.FullName!.Trim();
        var email = request.Email!.Trim();

        var lead = new Lead
        {
            ReferenceNumber = reference,
            Type = type,
            FullName = name,
            Email = email,
            WhatsApp = Clean(request.WhatsApp),
            InterestType = Clean(request.InterestType) ?? DefaultInterest(type),
            ProductOrServiceId = Clean(request.ProductOrServiceId),
            IeltsModule = Clean(request.IeltsModule),
            CurrentBand = Clean(request.CurrentBand),
            TargetBand = Clean(request.TargetBand),
            TestDate = Clean(request.TestDate),
            PreferredDate = Clean(request.PreferredDate),
            PreferredTime = Clean(request.PreferredTime),
            TimeZone = Clean(request.TimeZone),
            FocusArea = Clean(request.FocusArea),
            Message = Clean(request.Message),
            PaymentMethod = Clean(request.PaymentMethod),
            SourcePage = Clean(request.SourcePage),
        };
        _db.Leads.Add(lead);

        // A package request also opens a 3-credit tracking record (Service 2).
        if (type == LeadType.WritingFeedbackPackage)
        {
            _db.WritingFeedbackPackages.Add(new WritingFeedbackPackage
            {
                ReferenceNumber = reference,
                CustomerName = name,
                CustomerEmail = email,
                WhatsApp = lead.WhatsApp,
                TotalCredits = 3,
                UsedCredits = 0,
                RemainingCredits = 3,
                PaymentMethod = lead.PaymentMethod,
            });
        }

        await _db.SaveChangesAsync(ct);

        await _notify.NotifyAdminAsync($"{Title(type)} — {reference}", new Dictionary<string, string?>
        {
            ["Type"] = type.ToString(),
            ["Ref"] = reference,
            ["Name"] = name,
            ["Email"] = email,
            ["WhatsApp"] = lead.WhatsApp,
            ["Interest"] = lead.InterestType,
            ["Item"] = lead.ProductOrServiceId,
            ["Module"] = lead.IeltsModule,
            ["CurrentBand"] = lead.CurrentBand,
            ["TargetBand"] = lead.TargetBand,
            ["TestDate"] = lead.TestDate,
            ["PreferredDate"] = lead.PreferredDate,
            ["PreferredTime"] = lead.PreferredTime,
            ["TimeZone"] = lead.TimeZone,
            ["FocusArea"] = lead.FocusArea,
            ["PaymentMethod"] = lead.PaymentMethod,
            ["Source"] = lead.SourcePage,
        }, ct);

        // For the free checklist, attempt to deliver the resource and tailor the confirmation copy
        // to what actually happened (sent now vs. queued for follow-up), so we never imply an email
        // went out when it didn't.
        if (type == LeadType.FreeChecklist)
        {
            var sent = await _notify.SendCustomerResourceAsync(email, "Your Free IELTS Study Checklist",
                "Thanks for requesting the Free IELTS Study Checklist. Here it is — plus a short study series to help you prepare for Band 7+.", ct);

            _log.LogInformation("Captured {Type} lead {Ref} (checklist emailed: {Sent})", type, reference, sent);
            return new EnquiryResult
            {
                ReferenceNumber = reference,
                Message = sent
                    ? "Thank you! Your Free IELTS Study Checklist is on its way — please check your email (including spam/promotions)."
                    : "Thank you! Your request has been received — we'll email your Free IELTS Study Checklist shortly.",
            };
        }

        _log.LogInformation("Captured {Type} lead {Ref}", type, reference);
        return new EnquiryResult { ReferenceNumber = reference, Message = ConfirmationFor(type) };
    }

    public async Task<EnquiryResult> CaptureWritingFeedbackAsync(WritingFeedbackRequest request, CancellationToken ct = default)
    {
        var reference = GenerateReference();
        var submission = new WritingFeedbackSubmission
        {
            ReferenceNumber = reference,
            CustomerName = request.FullName!.Trim(),
            Email = request.Email!.Trim(),
            WhatsApp = Clean(request.WhatsApp),
            IeltsModule = Clean(request.IeltsModule),
            TaskType = Clean(request.TaskType),
            TargetBand = Clean(request.TargetBand),
            TestDate = Clean(request.TestDate),
            WritingText = Clean(request.WritingText),
            Notes = Clean(request.Notes),
            PaymentMethod = Clean(request.PaymentMethod),
        };
        _db.WritingFeedbackSubmissions.Add(submission);
        await _db.SaveChangesAsync(ct);

        await _notify.NotifyAdminAsync($"Writing Feedback submission — {reference}", new Dictionary<string, string?>
        {
            ["Ref"] = reference,
            ["Name"] = submission.CustomerName,
            ["Email"] = submission.Email,
            ["WhatsApp"] = submission.WhatsApp,
            ["Module"] = submission.IeltsModule,
            ["Task"] = submission.TaskType,
            ["TargetBand"] = submission.TargetBand,
            ["TestDate"] = submission.TestDate,
            ["PaymentMethod"] = submission.PaymentMethod,
            ["WritingChars"] = submission.WritingText?.Length.ToString(),
        }, ct);

        _log.LogInformation("Captured writing feedback submission {Ref}", reference);
        return new EnquiryResult
        {
            ReferenceNumber = reference,
            Message = "Thank you. Your writing feedback request has been received. We will review your submission and contact you with the next steps.",
        };
    }

    // ── helpers ───────────────────────────────────────────────────────────────────────────────
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static LeadType ParseType(string? raw) =>
        Enum.TryParse<LeadType>(raw, ignoreCase: true, out var t) ? t : LeadType.General;

    private static string Title(LeadType t) => t switch
    {
        LeadType.FreeChecklist => "Free checklist request",
        LeadType.ProductEnquiry => "Band 7+ Formula enquiry",
        LeadType.SpeakingAssessment => "Speaking assessment booking",
        LeadType.StrategyConsultation => "Strategy consultation booking",
        LeadType.WritingFeedbackPackage => "Writing feedback package request",
        LeadType.Waitlist => "Waitlist signup",
        LeadType.Contact => "Contact enquiry",
        _ => "General enquiry",
    };

    private static string? DefaultInterest(LeadType t) => t switch
    {
        LeadType.ProductEnquiry => "Band 7+ Formula",
        LeadType.SpeakingAssessment => "Speaking Assessment",
        LeadType.StrategyConsultation => "Strategy Consultation",
        LeadType.WritingFeedbackPackage => "Writing Feedback Package",
        LeadType.FreeChecklist => "Free IELTS Checklist",
        _ => null,
    };

    private static string ConfirmationFor(LeadType t) => t switch
    {
        LeadType.ProductEnquiry =>
            "Thank you. We received your request for IELTS Writing Mastery: The Band 7+ Formula. We will send payment/access instructions shortly.",
        LeadType.SpeakingAssessment =>
            "Thank you. Your speaking assessment request has been received. We will confirm your session schedule soon.",
        LeadType.StrategyConsultation =>
            "Thank you. Your strategy consultation request has been received. We will contact you to confirm your 60-minute session.",
        LeadType.WritingFeedbackPackage =>
            "Thank you. Your request for 3 writing reviews has been received. We will send payment/access instructions and set up your three review credits shortly.",
        LeadType.FreeChecklist =>
            "Thank you. Your Free IELTS Study Checklist request has been received — we'll email it to you shortly.",
        LeadType.Contact =>
            "Thank you. Your enquiry has been received. We will get back to you soon.",
        LeadType.Waitlist =>
            "Thank you. You're on the waitlist — we'll let you know as soon as this resource is available.",
        _ => "Thank you. Your enquiry has been received. We will get back to you soon.",
    };

    /// <summary>Short, unambiguous, hard-to-guess reference (no leaked sequential ids).</summary>
    private static string GenerateReference()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> chars = stackalloc char[5];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        return $"B7-{new string(chars)}";
    }
}
