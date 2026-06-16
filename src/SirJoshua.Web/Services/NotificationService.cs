using Microsoft.Extensions.Options;
using SirJoshua.Web.Options;

namespace SirJoshua.Web.Services;

/// <summary>
/// Sends operational notifications when a lead or submission is captured. There is no mail
/// provider wired up yet, so the default implementation writes a clear, structured entry to the
/// application log and every record is also persisted to the database. Swap in an SMTP/SendGrid
/// implementation behind this interface to start emailing the admin recipient and (for the free
/// checklist) the customer.
/// </summary>
public interface INotificationService
{
    /// <summary>Notify the team that a new lead/booking/submission arrived.</summary>
    Task NotifyAdminAsync(string subject, IReadOnlyDictionary<string, string?> details, CancellationToken ct = default);

    /// <summary>
    /// Deliver a resource (e.g. the free checklist link) to a customer. Returns false when no mail
    /// channel is configured, so the caller can fall back to "we'll email you shortly" messaging.
    /// </summary>
    Task<bool> SendCustomerResourceAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}

public class LoggingNotificationService : INotificationService
{
    private readonly ILogger<LoggingNotificationService> _log;
    private readonly NotificationOptions _opt;

    public LoggingNotificationService(ILogger<LoggingNotificationService> log, IOptions<NotificationOptions> opt)
    {
        _log = log;
        _opt = opt.Value;
    }

    public Task NotifyAdminAsync(string subject, IReadOnlyDictionary<string, string?> details, CancellationToken ct = default)
    {
        // Compact, greppable single-line summary; never logs payment credentials (we don't store any).
        var summary = string.Join(" | ", details
            .Where(d => !string.IsNullOrWhiteSpace(d.Value))
            .Select(d => $"{d.Key}={d.Value}"));
        _log.LogInformation("NEW ENQUIRY → {Admin} :: {Subject} :: {Summary}", _opt.AdminEmail, subject, summary);
        // TODO: send an email to _opt.AdminEmail via your mail provider (SMTP/SendGrid/etc.).
        return Task.CompletedTask;
    }

    public Task<bool> SendCustomerResourceAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        // TODO: deliver the resource (e.g. the Free IELTS Study Checklist link) by email.
        // No mail provider is configured, so report "not sent" and let the UI show a graceful
        // "we'll email it to you shortly" message instead of implying an email already went out.
        _log.LogInformation("Resource delivery requested for {Email}: {Subject} (no mail provider configured)", toEmail, subject);
        return Task.FromResult(false);
    }
}
