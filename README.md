# Instagram DM Monitor (Windows / Edge)

Windows の通知センターに届いた Edge / Instagram 通知を読み取り、通知テキストを画面表示・アプリのローカル領域へ JSONL 保存する検証用 WinUI 3 アプリです。

> **重要な制約**: このアプリは Instagram のDM APIやブラウザー内部のログイン情報を使いません。Windows通知に実際に含まれているテキストだけを読み取ります。Instagramが通知本文を省略した場合、本文は取得できません。過去のDM履歴や未読状態も取得しません。

## 現在の実装範囲

- `UserNotificationListener` のアクセス許可要求
- Windows の現在のトースト通知一覧から Edge / Instagram 名義の通知候補を表示
- アプリ起動中の `NotificationChanged` イベントを監視し、新規通知IDを差分検出
- 通知のタイトル、本文、元のテキスト要素、通知日時をローカル JSONL に保存
- 保存ログの削除
- 通知元アプリ名に `Edge` または `Instagram` が含まれるものだけを候補とする

**まだ実装していないもの**: DM通知だけを識別する精密なパーサー、バックグラウンドタスク、ログイン時自動起動、SQLite、Python分析、MSIXの配布署名。

## 必要環境

- Windows 10 22H2 / Windows 11（通知リスナー利用可能な環境）
- Visual Studio 2026 または対応する WinUI 3 / Windows App SDK 開発環境
- .NET 10 SDK
- Windows 10.0.26100 SDK と MSIX Packaging Tools
- Windows App SDK 2.6.4（NuGetから復元）

## ビルド

PowerShell でリポジトリのルートから実行します。

```powershell
dotnet restore .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj
dotnet build .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj -c Debug -p:Platform=x64
```

最初の実行・パッケージ署名は Visual Studio の `Blank App, Packaged (WinUI 3 in Desktop)` プロジェクトと同様に、開発用証明書の信頼設定が必要になる場合があります。Visual Studio で `InstagramDmMonitor.sln` を開き、x64 / Debug で配置・起動してください。署名エラーが出る場合は、Visual Studio のパッケージ化設定からローカル開発用テスト証明書を作成し、証明書の発行者名を `Package.appxmanifest` の `Publisher` と一致させてください。

## 動作確認手順

1. Edge の専用プロファイルを作り、Instagramだけにログインします。
2. Edge の `instagram.com` サイト通知と Instagram のDM通知を有効にします。
3. Windows の通知設定で Edge の通知が許可されていることを確認します。
4. アプリを起動し、「通知アクセスを許可」を押します。
5. すでに表示中の通知を調べる場合は「現在の通知を確認」を押します。
6. 「監視開始」を押してからテスト用DMを受信します。
7. アプリ画面に表示された `アプリ`、`タイトル`、`本文`、`raw` を確認します。

初期版では Edge 由来のサイト通知をまとめて候補にします。専用プロファイルにすることで、Instagram以外のサイト通知が混ざる可能性を減らせますが、サイト単位の識別をWindows APIが保証するわけではありません。

## 保存先

`ApplicationData.Current.LocalFolder` 配下の `notifications.jsonl` に保存します。画面に実際のパスを表示します。通知本文は個人的な情報を含むため、端末外へ送信しません。必要がなくなったログは画面の「保存ログを削除」で消去してください。

## 既知の制約

- `UserNotificationListener` はWindows上の通知全般にアクセスできる強い権限です。ユーザーの明示的な許可が必要です。許可しない場合は動作しません。アプリ側ではEdge / Instagram名義に絞りますが、OS権限そのものがEdge通知だけに限定されるわけではありません。
- このアプリは監視開始後、アプリが起動している間だけイベントを監視します。閉じている間の新着通知をリアルタイムに保存しません。
- 通知が表示されてすぐ消去された場合、一覧の差分同期より先に消えて取得できない場合があります。
- WindowsやEdgeの通知設定、Instagramの通知形式、メッセージプレビュー設定によって、送信者名・本文が通知に含まれないことがあります。
- 取得対象は通知プレビューであり、DMの完全な本文・添付ファイル・過去履歴を保証しません。
- `NotificationChanged` は変更内容そのものを渡さないため、通知一覧を再取得してID差分を取っています。

## 設計上のプライバシー方針

- ログ保存はユーザーが「監視開始」を押した後に限ります。
- Edge / Instagram名義の通知候補のみを保存します。
- 通知を消去・既読化する操作は行いません。
- ブラウザーのCookie、セッション、パスワード、DM画面のDOMにはアクセスしません。
- ログはローカル保存のみです。
