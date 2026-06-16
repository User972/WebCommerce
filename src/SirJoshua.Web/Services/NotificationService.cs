using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SirJoshua.Web.Options;

namespace SirJoshua.Web.Services;

/// <summary>
/// Sends operational notifications when a lead or submission is captured. Two implementations are
/// available and chosen at startup based on configuration:
/// <list type="bullet">
/// <item><see cref="SendGridNotificationService"/> — emails the admin and (for the free checklist)
/// the customer when a SendGrid API key is configured.</item>
/// <item><see cref="LoggingNotificationService"/> — the no-provider fallback: writes a structured
/// log entry and reports "not sent" so the UI keeps its graceful "we'll email you shortly" copy.</item>
/// </list>
/// Every record is also persisted to the database regardless of which notifier is active.
/// </summary>
public interface INotificationService
{
    /// <summary>Notify the team that a new lead/booking/submission arrived.</summary>
    Task NotifyAdminAsync(string subject, IReadOnlyDictionary<string, string?> details, CancellationToken ct = default);

    /// <summary>
    /// Deliver a resource (e.g. the free checklist link) to a customer. Returns false when the email
    /// could not be sent (no mail channel configured, or SendGrid rejected it), so the caller can fall
    /// back to "we'll email you shortly" messaging.
    /// </summary>
    Task<bool> SendCustomerResourceAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}

/// <summary>
/// SendGrid-backed notifier. Admin alerts go to <see cref="NotificationOptions.AdminEmail"/>;
/// customer resource emails go to the lead's address. Delivery is best-effort: any SendGrid error is
/// caught and logged — a notification failure must never fail the customer's request.
/// </summary>
public class SendGridNotificationService : INotificationService
{
    private readonly ISendGridClient _client;
    private readonly NotificationOptions _opt;
    private readonly SendGridOptions _sg;
    private readonly ILogger<SendGridNotificationService> _log;

    public SendGridNotificationService(
        ISendGridClient client,
        IOptions<NotificationOptions> opt,
        ILogger<SendGridNotificationService> log)
    {
        _client = client;
        _opt = opt.Value;
        _sg = _opt.SendGrid;
        _log = log;
    }

    public Task NotifyAdminAsync(string subject, IReadOnlyDictionary<string, string?> details, CancellationToken ct = default)
    {
        var rows = details.Where(d => !string.IsNullOrWhiteSpace(d.Value)).ToList();

        var text = new StringBuilder().AppendLine(subject).AppendLine();
        foreach (var d in rows) text.Append(d.Key).Append(": ").AppendLine(d.Value);

        var html = new StringBuilder()
            .Append("<h2 style=\"font-family:sans-serif\">").Append(Enc(subject)).Append("</h2>")
            .Append("<table style=\"font-family:sans-serif;border-collapse:collapse\">");
        foreach (var d in rows)
            html.Append("<tr><td style=\"padding:4px 12px 4px 0;color:#555\"><strong>").Append(Enc(d.Key))
                .Append("</strong></td><td style=\"padding:4px 0\">").Append(Enc(d.Value)).Append("</td></tr>");
        html.Append("</table>");

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_sg.FromEmail, _sg.FromName),
            Subject = $"[Band 7 Plus] {subject}",
            PlainTextContent = text.ToString(),
            HtmlContent = html.ToString(),
        };
        msg.AddTo(new EmailAddress(_opt.AdminEmail));

        return SendAsync(msg, $"admin notification \"{subject}\"", ct);
    }

    public async Task<bool> SendCustomerResourceAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        var button = string.IsNullOrWhiteSpace(_sg.ChecklistUrl)
            ? string.Empty
            : $"<p style=\"margin:24px 0\"><a href=\"{Enc(_sg.ChecklistUrl)}\" " +
              "style=\"background:#D97757;color:#fff;text-decoration:none;font-family:sans-serif;" +
              "font-weight:600;padding:12px 22px;border-radius:9999px;display:inline-block\">" +
              "Download your checklist</a></p>";

        var html =
            "<div style=\"font-family:sans-serif;font-size:15px;line-height:1.6;color:#16201f\">" +
            $"<p>{Enc(body)}</p>{button}" +
            "<p style=\"color:#777;font-size:13px\">You're receiving this because you requested the Free IELTS " +
            "Study Checklist from IELTS Band 7 Plus™. Reply to this email if you have any questions.</p></div>";

        var plain = string.IsNullOrWhiteSpace(_sg.ChecklistUrl)
            ? body
            : $"{body}\n\nDownload your checklist: {_sg.ChecklistUrl}";

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_sg.FromEmail, _sg.FromName),
            Subject = subject,
            PlainTextContent = plain,
            HtmlContent = html,
        };
        msg.AddTo(new EmailAddress(toEmail));

        return await SendAsync(msg, $"resource email to {toEmail}", ct);
    }

    /// <summary>Sends a message and returns true only on a 2xx from SendGrid. Never throws.</summary>
    private async Task<bool> SendAsync(SendGridMessage msg, string what, CancellationToken ct)
    {
        try
        {
            var resp = await _client.SendEmailAsync(msg, ct);
            var status = (int)resp.StatusCode;
            if (status is >= 200 and < 300)
            {
                _log.LogInformation("Sent {What} via SendGrid ({Status}).", what, status);
                return true;
            }

            var detail = await resp.Body.ReadAsStringAsync(ct);
            _log.LogError("SendGrid rejected {What}: {Status} {Body}", what, status, detail);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to send {What} via SendGrid.", what);
            return false;
        }
    }

    private static string Enc(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
}

/// <summary>
/// No-provider fallback used when SendGrid is not configured. Writes a clear, structured log entry
/// for the admin and reports resource delivery as "not sent" so the UI shows a graceful
/// "we'll email it to you shortly" message instead of implying an email already went out.
/// </summary>
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
        return Task.CompletedTask;
    }

    public Task<bool> SendCustomerResourceAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        // No mail provider is configured, so report "not sent" and let the UI show a graceful
        // "we'll email it to you shortly" message instead of implying an email already went out.
        _log.LogInformation("Resource delivery requested for {Email}: {Subject} (no mail provider configured)", toEmail, subject);
        return Task.FromResult(false);
    }
}
