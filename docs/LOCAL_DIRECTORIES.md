# PC上のディレクトリ構成・用途記録

最終確認日: 2026-11-09

この文書は、DMmonitor の開発・ビルド作業で実際に確認できた Windows 上のディレクトリと用途を記録するものです。PC全体をスキャンした一覧ではなく、PowerShell の実行記録、ビルドログ、プロジェクト設定から確認できた範囲を記載しています。

## 1. リポジトリの場所

| 種別 | パス | 用途・状態 |
|---|---|---|
| リポジトリルート | **E:\_library\projects\DMmonitor\** | DMmonitor のソースコード、開発ツール、ビルド成果物をまとめる作業場所。PowerShell でこのディレクトリへ移動して作業している。 |
| アプリプロジェクト | **E:\_library\projects\DMmonitor\src\InstagramDmMonitor.App\** | WinUI 3 アプリ本体のプロジェクトファイル、XAML、C#コード、マニフェスト等を配置。 |

## 2. ポータブル開発ツール

以下はリポジトリ内に配置するポータブル開発環境です。環境変数は PowerShell の現在のセッションに設定して使用します。

| ディレクトリ／ファイル | 環境変数・用途 |
|---|---|
| **.tools\dotnet\** | ポータブル .NET 10 SDK の配置先。確認されたSDKバージョンは **10.0.403**。DOTNET_ROOT に設定。 |
| **.tools\dotnet-home\** | .NET CLI のユーザー領域。設定変数は DOTNET_CLI_HOME。 |
| **.tools\nuget\** | NuGet パッケージのキャッシュ・復元先。設定変数は NUGET_PACKAGES。 |
| **.tools\dotnet-install.ps1** | Microsoft公式の dotnet-install スクリプト。ポータブルSDKの取得・展開に使用。 |

実際の絶対パスは、リポジトリルートを基準にした次の場所です。

- **E:\_library\projects\DMmonitor\.tools\dotnet\**
- **E:\_library\projects\DMmonitor\.tools\dotnet-home\**
- **E:\_library\projects\DMmonitor\.tools\nuget\**
- **E:\_library\projects\DMmonitor\.tools\dotnet-install.ps1**

PowerShell で設定している環境変数:

| 変数 | 値 |
|---|---|
| DOTNET_ROOT | E:\_library\projects\DMmonitor\.tools\dotnet |
| DOTNET_CLI_HOME | E:\_library\projects\DMmonitor\.tools\dotnet-home |
| NUGET_PACKAGES | E:\_library\projects\DMmonitor\.tools\nuget |
| PATH | 現在のPowerShellセッションで DOTNET_ROOT を先頭に追加 |

これらの環境変数設定は現在のPowerShellセッション限定です。新しいPowerShellを開いたときは再設定が必要です。Windows全体のPATHや永続環境変数を変更する設定ではありません。

## 3. ビルド出力

| ディレクトリ | 用途・状態 |
|---|---|
| **src\InstagramDmMonitor.App\bin\x64\Debug\net10.0-windows10.0.26100.0\** | 通常ビルドの出力先。DLL等のビルド成果物が生成されたことを確認済み。 |
| **src\InstagramDmMonitor.App\obj\** | .NET/MSBuild が使用する中間生成物の標準出力先。ビルド時に自動生成される領域。 |
| **artifacts\msix\** | MSIXパッケージの出力先として今回の作業で作成。 |
| **artifacts\msix\InstagramDmMonitor.App_0.1.0.0_x64_Debug_Test\** | MSIXパッケージの出力フォルダー。 |
| **artifacts\msix\InstagramDmMonitor.App_0.1.0.0_x64_Debug_Test\InstagramDmMonitor.App_0.1.0.0_x64_Debug.msix** | 2026-11-09 のビルドで生成成功を確認したMSIXパッケージ。 |

確認済みのMSIXファイルの絶対パス:

**E:\_library\projects\DMmonitor\artifacts\msix\InstagramDmMonitor.App_0.1.0.0_x64_Debug_Test\InstagramDmMonitor.App_0.1.0.0_x64_Debug.msix**

## 4. アプリのローカルデータ

アプリの通知ログは、Windows の ApplicationData.Current.LocalFolder 配下に **notifications.jsonl** として保存する設計です。

- 保存先の種類: アプリごとの Windows ローカルデータ領域
- ファイル名: notifications.jsonl
- 用途: 取得した通知のタイトル、本文、rawテキスト、日時等をJSONL形式で保存
- 実際の絶対パス: 現時点ではアプリを起動して画面に表示される保存先を未確認

このログにはDMの通知プレビューなど個人情報が含まれる可能性があります。共有前に内容を確認してください。

## 5. Git管理・削除時の注意

- .tools\ はSDK、NuGetキャッシュ、CLI作業データの保存先です。Gitへ追加しないでください。
- bin\ と obj\ は再生成可能なビルド成果物・中間ファイルです。
- artifacts\msix\ は配布・テスト用の生成物です。必要に応じて再生成できます。
- .cer、.pfx、秘密鍵を含む証明書ファイルはGitへコミットしないでください。特にPFXの秘密鍵は厳重に管理してください。
- 現在の .gitignore では bin/、obj/、MSIX、証明書ファイルが除外されています。.tools/ と artifacts/ の除外ルールは別途確認・整備が必要です。

## 6. 未確認事項

次の項目は、まだ実機で確認できていません。

- アプリ起動後に画面へ表示される LocalFolder の絶対パス
- MSIX署名に使う証明書の実際の保存場所
- インストール後のアプリ本体の配置場所（Windowsが管理するパッケージ領域）
- 今後追加されるログ、設定、エクスポート先のディレクトリ

確認できた時点でこの文書を更新してください。
