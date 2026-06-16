namespace SirJoshua.Web.Options;

/// <summary>
/// Where operational notifications go. Bound from the "Notifications" configuration section.
/// No secrets live here; an actual mail provider's API key must come from a secret store /
/// environment variable, never from committed configuration.
/// </summary>
public class NotificationOptions
{
    public const string SectionName = "Notifications";

    /// <summary>Inbox that should receive new-lead/booking/submission notifications.</summary>
    public string AdminEmail { get; set; } = "hello@sirjoshua.id";
}
