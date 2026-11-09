# PC上のディレクトリ構成・用途記録

最終確認日: 2026-11-09

この文書は、DMmonitor の開発・ビルド作業で実際に確認できた Windows 上のディレクトリと用途を記録するものです。PC全体をスキャンした一覧ではなく、PowerShell の実行記録、ビルドログ、プロジェクト設定から確認できた範囲を記載しています。

## 1. リポジトリの場所

| 種別 | パス | 用途・状態 |
|---|---|---|
| リポジトリルート | **E:\_library\projects\DMmonitor\** | DMmonitor のソースコード、開発ツール、ビルド成果物をまとめる作業場所。PowerShell でこのディレクトリへ移動して作業している。 |
| アプリプロジェクト | **E:\_library\projects\DMmonitor\src\InstagramDmMonitor.App\** | WinUI 3 アプリ本体のプロジェクトファイル、XAML、C#コード、マニフェスト等を配置。 |

## 1.1 作業ドライブとMSIXパッケージボリューム

2026-11-09 の PowerShell 出力で確認した E: ドライブの状態:

| 項目 | 確認値 |
|---|---|
| ドライブ | `E:` |
| ファイルシステム | NTFS |
| DriveType | Removable（リムーバブル） |
| HealthStatus | Healthy |
| 総容量 | 62,518,194,176 bytes（約58.2 GiB） |
| 空き容量 | 5,521,375,232 bytes（約5.14 GiB） |

2026-11-09 の後続確認で、管理者PowerShellから `Add-AppxVolume -Path "E:\WindowsApps"` を実行し、E: をAppXボリュームとして正常に登録しました。

| 項目 | 登録後の確認値 |
|---|---|
| PackageStorePath | `E:\WindowsApps` |
| IsOffline | `False` |
| IsSystemVolume | `False` |
| C: システムボリューム | 引き続き既定・システムボリューム |

この登録はE:を追加ボリュームとして利用可能にするもので、既定のAppXインストール先は変更していません。WinDbgをE:へ導入する場合は、インストール時に `Add-AppxPackage -Volume` へこのボリュームを明示指定します。

Windowsのパッケージボリュームはリムーバブルメディアにも作成できます。ただし、E: を取り外すと、そのボリュームに配置したパッケージはオフラインとなり利用できません。WinDbg等のMSIXアプリをE:に配置する場合は、使用時にドライブが常時接続されていることと、空き容量を確保することが必要です。

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
| DOTNET_ROOT_X64 | ユーザー環境変数として `E:\_library\projects\DMmonitor\.tools\dotnet` を登録済み。x64アプリのapphostがポータブルRuntimeを検索するために使用。 |

`DOTNET_ROOT`、`DOTNET_CLI_HOME`、`NUGET_PACKAGES` は現在のPowerShellセッション限定です。新しいターミナルでは再設定が必要です。一方、`DOTNET_ROOT_X64` はユーザー環境変数として永続登録済みです。2026-11-09 にユーザーが設定コマンドの成功出力を確認しました。既に起動中のExplorer等には変更が伝播していない場合があるため、初回反映にはサインアウト／サインインが必要です。Windows全体のPATHは変更していません。

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

### 3.1 MSIXのテスト出力フォルダー内

2026-11-09 にユーザーが実行した `Get-ChildItem -Recurse -Force` の結果から、次の項目を確認しています。

| 項目 | 用途 |
|---|---|
| `InstagramDmMonitor.App_0.1.0.0_x64_Debug.msix` | アプリ本体のMSIXパッケージ（約64 MB）。 |
| `Add-AppDevPackage.ps1` | 開発用パッケージのインストール補助スクリプト。 |
| `Install.ps1` | パッケージ出力に含まれるインストール用スクリプト。 |
| `Add-AppDevPackage.resources\` | インストール補助スクリプトの表示文言などのリソース。 |
| `cs-CZ\`、`de-DE\`、`en-US\`、`es-ES\`、`fr-FR\`、`it-IT\`、`ja-JP\`、`ko-KR\`、`pl-PL\`、`pt-BR\`、`ru-RU\`、`tr-TR\`、`zh-CN\`、`zh-TW\` | インストール補助スクリプトの言語別リソース。 |
| `Add-AppDevPackage.psd1` | 言語別リソースに含まれるPowerShellデータファイル。 |
| `Dependencies\` | Windows App SDK等の実行時依存パッケージを格納。 |
| `Dependencies\x64\`、`x86\`、`arm64\`、`win32\` | アーキテクチャ別の依存パッケージ配置先。 |
| `Microsoft.WindowsAppRuntime.2.msix` | Windows App Runtime 2 系の依存パッケージ。複数のアーキテクチャ／構成向けファイルが含まれる。 |

依存パッケージや言語リソースはインストール用の構成要素です。内容を理解せず削除しないでください。x64 PCへのインストール時は、生成スクリプトに依存関係の選択を任せる方針です。

**証明書について:** 提示された一覧には `.cer` と `.pfx` が表示されていません。このため、MSIXの署名証明書がどこに保存されているか、またインストールに必要な信頼設定が済んでいるかは未確認です。インストール前にプロジェクトの `src\InstagramDmMonitor.App\` と出力フォルダーを検索して確認してください。

### 3.2 開発用署名証明書

2026-11-09 に次の公開証明書ファイルが作成されたことを確認しています。

| 項目 | 値 |
|---|---|
| 保存ディレクトリ | `artifacts\certs\` |
| 公開証明書 | `artifacts\certs\DMmonitor-Development.cer` |
| 絶対パス | `E:\_library\projects\DMmonitor\artifacts\certs\DMmonitor-Development.cer` |
| Subject / Publisher | `CN=InstagramDmMonitor-Development` |
| Thumbprint | `F88472DFA0672BC5F2135251D908A60D74C2905C` |
| 有効期限 | 2027-11-09 13:49:20（PCに表示されたローカル時刻） |
| 秘密鍵の保存先 | 作成時に指定した `Cert:\CurrentUser\My` 証明書ストア。CERファイルには秘密鍵を含まない。 |

この証明書でMSIXの署名付きビルドに成功し、2026-11-09 に `Get-AuthenticodeSignature` が `Status: Valid` を返すことを確認しました。

- 秘密鍵を含む証明書: `Cert:\CurrentUser\My` に作成。
- 公開証明書の信頼先: `Cert:\LocalMachine\TrustedPeople` への登録成功を確認。
- `Cert:\LocalMachine\TrustedPeople` の証明書Thumbprint: `F88472DFA0672BC5F2135251D908A60D74C2905C`。

証明書の信頼登録には管理者権限が必要です。自己署名証明書を `Trusted Root Certification Authorities` に追加する必要はありません。

### 3.3 Windows Error Reporting のクラッシュダンプ

ユーザーの実行結果から、次のWindows共通ダンプ保存先とDMmonitorのダンプファイルを確認しています。

| 項目 | 値・用途 |
|---|---|
| 保存ディレクトリ | `%LOCALAPPDATA%\CrashDumps\` |
| このPCでの絶対パス | `C:\Users\mango\AppData\Local\CrashDumps\` |
| 用途 | Windows Error Reporting（WER）がアプリケーションのクラッシュ時に作成するダンプの保存先。 |
| DMmonitorのファイル例 | `InstagramDmMonitor.App.exe.17116.dmp`、`InstagramDmMonitor.App.exe.13364.dmp` など。 |
| 関連するレジストリ | `HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\InstagramDmMonitor.App.exe` |
| 設定値 | `DumpFolder=%LOCALAPPDATA%\CrashDumps`、`DumpType=2`（フルダンプ）、`DumpCount=3`。 |

このフォルダーにはDMmonitor以外のプロセスのダンプも含まれます。ダンプにはメモリ内容や個人情報が含まれる可能性があるため、外部へ共有しないでください。今回の一覧には `backgroundTaskHost.exe` と `python.exe` のダンプも存在しており、DMmonitor本体の調査では `InstagramDmMonitor.App.exe.*.dmp` のみを対象とします。

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
