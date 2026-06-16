namespace SirJoshua.Web.Options;

/// <summary>
/// Where operational notifications go and how email is delivered. Bound from the "Notifications"
/// configuration section. No secrets live in committed config: the SendGrid API key must come from
/// a secret store / environment variable (e.g. <c>Notifications__SendGrid__ApiKey</c>), never from
/// appsettings.json checked into source control.
/// </summary>
public class NotificationOptions
{
    public const string SectionName = "Notifications";

    /// <summary>Inbox that should receive new-lead/booking/submission notifications.</summary>
    public string AdminEmail { get; set; } = "hello@sirjoshua.id";

    /// <summary>SendGrid email-delivery settings.</summary>
    public SendGridOptions SendGrid { get; set; } = new();
}

/// <summary>
/// SendGrid transactional-email settings. <see cref="ApiKey"/> is a secret — supply it via an
/// environment variable (<c>Notifications__SendGrid__ApiKey</c>) or user-secrets, not committed
/// configuration. The sender must be a SendGrid-verified single sender or authenticated domain.
/// </summary>
public class SendGridOptions
{
    /// <summary>SendGrid API key. Empty (or the placeholder) disables email and the app falls back
    /// to logging notifications.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Verified "from" address used for every outgoing email.</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>Display name shown on outgoing email.</summary>
    public string FromName { get; set; } = "IELTS Band 7 Plus";

    /// <summary>Public link to the Free IELTS Study Checklist resource. When set, it is included as a
    /// download button in the checklist email; when empty, the email simply confirms receipt.</summary>
    public string? ChecklistUrl { get; set; }

    /// <summary>True only when a usable API key and a sender address are configured.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !ApiKey.Equals("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(FromEmail);
}
