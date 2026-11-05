namespace InstagramDmMonitor.Models;

public sealed record CapturedNotification(
    uint NotificationId,
    string SourceApp,
    DateTimeOffset CreatedAt,
    DateTimeOffset CapturedAt,
    string Title,
    string Body,
    IReadOnlyList<string> RawTexts);
