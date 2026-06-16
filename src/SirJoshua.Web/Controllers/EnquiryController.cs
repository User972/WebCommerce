using Microsoft.AspNetCore.Mvc;
using SirJoshua.Web.Services;
using SirJoshua.Web.ViewModels;

namespace SirJoshua.Web.Controllers;

/// <summary>
/// Lead-capture and service-request endpoints for the IELTS Band 7 Plus™ funnel. Every form on the
/// site posts JSON here. A valid anti-forgery token is required (enforced globally) and all inputs
/// are validated server-side via data annotations before anything is persisted. No card details are
/// ever accepted or stored — payment is settled out of band via a payment link.
/// </summary>
[Route("api/enquiry")]
[ApiController]
[Consumes("application/json")]
public class EnquiryController : ControllerBase
{
    private readonly IEnquiryService _enquiries;
    private readonly ILogger<EnquiryController> _log;

    public EnquiryController(IEnquiryService enquiries, ILogger<EnquiryController> log)
    {
        _enquiries = enquiries;
        _log = log;
    }

    // Generic, non-revealing response for honeypot hits — the bot believes it succeeded while
    // nothing is persisted and no notification is sent.
    private static readonly EnquiryResult SpamResult = new()
    {
        ReferenceNumber = string.Empty,
        Message = "Thank you. Your request has been received.",
    };

    /// <summary>Generic lead/booking capture (checklist, product enquiry, bookings, package, waitlist, contact).</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] EnquiryRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Website))
        {
            _log.LogInformation("Discarded enquiry: honeypot filled.");
            return Ok(SpamResult);
        }
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _enquiries.CaptureLeadAsync(request, ct);
        return Ok(result);
    }

    /// <summary>IELTS Writing Feedback submission (Service 1) — captures the candidate's writing for review.</summary>
    [HttpPost("writing-feedback")]
    public async Task<IActionResult> WritingFeedback([FromBody] WritingFeedbackRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Website))
        {
            _log.LogInformation("Discarded writing-feedback: honeypot filled.");
            return Ok(SpamResult);
        }
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _enquiries.CaptureWritingFeedbackAsync(request, ct);
        return Ok(result);
    }
}
