using InstagramDmMonitor.Models;
using InstagramDmMonitor.Notifications;
using Microsoft.UI.Xaml;
using Windows.UI.Notifications.Management;

namespace InstagramDmMonitor;

public sealed partial class MainWindow : Window
{
    private readonly NotificationCaptureService _captureService = new();

    public MainWindow()
    {
        InitializeComponent();

        _captureService.StatusMessage += AppendLog;
        _captureService.NotificationCaptured += OnNotificationCaptured;
        Closed += (_, _) => _captureService.StopMonitoring();

        UpdateStatus($"状態: {FormatAccessStatus(_captureService.GetAccessStatus())}");
        AppendLog("起動しました。");
        AppendLog("1) 通知アクセスを許可  2) 現在の通知を確認  3) 監視開始 の順で試してください。");
    }

    private async void GrantAccessButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var status = await _captureService.RequestAccessAsync();
            UpdateStatus($"状態: {FormatAccessStatus(status)}");
            AppendLog($"通知アクセス結果: {status}");
        }
        catch (Exception ex)
        {
            AppendLog($"アクセス許可の要求に失敗しました: {ex.Message}");
        }
    }

    private async void ReadCurrentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var records = await _captureService.ReadCurrentEdgeNotificationsAsync();
            AppendLog($"現在のEdge / Instagram候補通知: {records.Count} 件");

            foreach (var record in records)
            {
                AppendRecord(record, "[現在の通知]");
            }

            if (records.Count == 0)
            {
                AppendLog("候補通知はありません。EdgeでInstagramの通知を有効にし、新しいDM通知が届いた後に再確認してください。");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"通知の読み込みに失敗しました: {ex.Message}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _captureService.StartMonitoringAsync();
            UpdateStatus("状態: 監視中（このアプリが起動している間）");
        }
        catch (Exception ex)
        {
            AppendLog(ex.Message);
            UpdateStatus($"状態: {_captureService.GetAccessStatus()}");
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _captureService.StopMonitoring();
        UpdateStatus("状態: 停止");
    }

    private async void DeleteLogButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _captureService.DeleteLogAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"ログ削除に失敗しました: {ex.Message}");
        }
    }

    private void OnNotificationCaptured(CapturedNotification record)
    {
        DispatcherQueue.TryEnqueue(() => AppendRecord(record, "[新着通知]"));
    }

    private void AppendRecord(CapturedNotification record, string label)
    {
        AppendLog($"{label} {record.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        AppendLog($"  アプリ: {record.SourceApp}");
        AppendLog($"  タイトル: {record.Title}");
        AppendLog($"  本文: {record.Body}");
        AppendLog($"  raw: {string.Join(" | ", record.RawTexts)}");
    }

    private void AppendLog(string message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            OutputBox.Text = string.IsNullOrEmpty(OutputBox.Text)
                ? line
                : OutputBox.Text + Environment.NewLine + line;
        });
    }

    private void UpdateStatus(string message)
    {
        DispatcherQueue.TryEnqueue(() => StatusText.Text = message);
    }

    private static string FormatAccessStatus(UserNotificationListenerAccessStatus status) => status switch
    {
        UserNotificationListenerAccessStatus.Allowed => "許可済み",
        UserNotificationListenerAccessStatus.Denied => "拒否（Windows設定で変更が必要）",
        _ => "未設定"
    };
}
