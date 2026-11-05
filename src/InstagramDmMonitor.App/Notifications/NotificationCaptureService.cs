using System.Text.Json;
using InstagramDmMonitor.Models;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.Storage;

namespace InstagramDmMonitor.Notifications;

/// <summary>
/// Reads Windows toast notifications while this app is running.
/// Only notifications whose source app name contains "Edge" or "Instagram"
/// are surfaced or written to the local JSONL log.
/// </summary>
public sealed class NotificationCaptureService
{
    private readonly UserNotificationListener _listener = UserNotificationListener.Current;
    private readonly SemaphoreSlim _syncGate = new(1, 1);
    private readonly HashSet<uint> _knownNotificationIds = new();
    private bool _isMonitoring;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public event Action<CapturedNotification>? NotificationCaptured;
    public event Action<string>? StatusMessage;

    public string LogFilePath => Path.Combine(
        ApplicationData.Current.LocalFolder.Path,
        "notifications.jsonl");

    public UserNotificationListenerAccessStatus GetAccessStatus() =>
        _listener.GetAccessStatus();

    /// <remarks>Must be called from the UI thread.</remarks>
    public async Task<UserNotificationListenerAccessStatus> RequestAccessAsync() =>
        await _listener.RequestAccessAsync();

    public async Task<IReadOnlyList<CapturedNotification>> ReadCurrentEdgeNotificationsAsync()
    {
        EnsureAccess();

        var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
        var results = new List<CapturedNotification>();

        foreach (var notification in notifications)
        {
            try
            {
                var record = Extract(notification);
                if (record is not null && IsCandidateSource(record.SourceApp))
                {
                    results.Add(record);
                }
            }
            catch (Exception ex)
            {
                StatusMessage?.Invoke($"通知 {notification.Id} の解析をスキップしました: {ex.Message}");
            }
        }

        return results.OrderByDescending(x => x.CreatedAt).ToArray();
    }

    public async Task StartMonitoringAsync()
    {
        EnsureAccess();

        if (_isMonitoring)
        {
            StatusMessage?.Invoke("監視はすでに開始されています。");
            return;
        }

        // Existing notifications are a baseline. They are not re-saved as new messages.
        var existing = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
        _knownNotificationIds.Clear();
        foreach (var notification in existing)
        {
            _knownNotificationIds.Add(notification.Id);
        }

        _listener.NotificationChanged += OnNotificationChanged;
        _isMonitoring = true;
        StatusMessage?.Invoke($"監視を開始しました。既存通知 {existing.Count} 件を基準として登録しました。");
        StatusMessage?.Invoke($"保存先: {LogFilePath}");
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
        {
            return;
        }

        _listener.NotificationChanged -= OnNotificationChanged;
        _isMonitoring = false;
        StatusMessage?.Invoke("監視を停止しました。");
    }

    public async Task DeleteLogAsync()
    {
        if (File.Exists(LogFilePath))
        {
            await Task.Run(() => File.Delete(LogFilePath));
        }

        StatusMessage?.Invoke("ローカル保存ログを削除しました。");
    }

    private void EnsureAccess()
    {
        if (_listener.GetAccessStatus() != UserNotificationListenerAccessStatus.Allowed)
        {
            throw new InvalidOperationException(
                "通知アクセスが許可されていません。先に「通知アクセスを許可」を実行してください。");
        }
    }

    private async void OnNotificationChanged(
        UserNotificationListener sender,
        UserNotificationChangedEventArgs args)
    {
        await _syncGate.WaitAsync();
        try
        {
            var current = await sender.GetNotificationsAsync(NotificationKinds.Toast);
            var currentIds = new HashSet<uint>();

            foreach (var notification in current)
            {
                currentIds.Add(notification.Id);

                // New IDs only. The event itself does not contain the changed notification.
                if (_knownNotificationIds.Add(notification.Id))
                {
                    try
                    {
                        var record = Extract(notification);
                        if (record is not null && IsCandidateSource(record.SourceApp))
                        {
                            await AppendToLocalLogAsync(record);
                            NotificationCaptured?.Invoke(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusMessage?.Invoke($"新着通知の処理に失敗しました: {ex.Message}");
                    }
                }
            }

            // Remove dismissed notification IDs from the baseline.
            _knownNotificationIds.IntersectWith(currentIds);
        }
        catch (Exception ex)
        {
            StatusMessage?.Invoke($"通知一覧の同期に失敗しました: {ex.Message}");
        }
        finally
        {
            _syncGate.Release();
        }
    }

    private static CapturedNotification? Extract(UserNotification notification)
    {
        var sourceApp = notification.AppInfo?.DisplayInfo?.DisplayName ?? "Unknown";
        var binding = notification.Notification?.Visual?.GetBinding(
            KnownNotificationBindings.ToastGeneric);

        if (binding is null)
        {
            return null;
        }

        var rawTexts = binding.GetTextElements()
            .Select(element => element.Text?.Trim() ?? string.Empty)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        if (rawTexts.Length == 0)
        {
            return null;
        }

        var title = rawTexts[0];
        var body = string.Join(Environment.NewLine, rawTexts.Skip(1));

        return new CapturedNotification(
            NotificationId: notification.Id,
            SourceApp: sourceApp,
            CreatedAt: notification.CreationTime,
            CapturedAt: DateTimeOffset.Now,
            Title: title,
            Body: body,
            RawTexts: rawTexts);
    }

    private static bool IsCandidateSource(string sourceApp) =>
        sourceApp.Contains("Edge", StringComparison.OrdinalIgnoreCase) ||
        sourceApp.Contains("Instagram", StringComparison.OrdinalIgnoreCase);

    private async Task AppendToLocalLogAsync(CapturedNotification record)
    {
        var json = JsonSerializer.Serialize(record, JsonOptions);
        await File.AppendAllTextAsync(LogFilePath, json + Environment.NewLine);
    }
}
