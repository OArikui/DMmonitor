# 開発環境構築・ビルドマニュアル（Windows / ポータブル環境）

DMmonitor を Windows 上でビルドするための環境を、可能な限りインストール不要のポータブル構成で用意する手順です。

## 1. 方針と前提

- Visual Studio を必須にせず、まず .NET CLI からビルドします。
- .NET SDK と NuGet パッケージの保存先をリポジトリ配下の .tools に集約します。
- システム PATH、システム環境変数、レジストリは変更しません。
- SDK の取得と NuGet 復元にはインターネット接続が必要です。
- Windows SDK のビルド用コンポーネントは、プロジェクトの NuGet 依存関係から取得する構成を優先します。

> **注意:** WinUI 3 / MSIX は Windows 固有の機能に依存します。.NET SDK はポータブル配置できますが、Windows SDK の一部機能、署名、アプリの配置・実行までをすべてポータブル化できるとは限りません。問題が発生した場合の Visual Studio / Windows SDK の導入は例外的な対応とします。

## 2. 必要な環境

| 項目 | 要件 |
|---|---|
| OS | Windows 10 / Windows 11（本プロジェクトの対象環境に準拠） |
| シェル | Windows PowerShell 5.1 以降、または PowerShell 7 |
| .NET SDK | .NET 10 SDK |
| アーキテクチャ | x64 |
| ネットワーク | .NET SDK と NuGet パッケージの取得時に必要 |

Visual Studio 本体は通常の CLI ビルドでは使用しません。Edge は通知の動作確認時に必要です。

## 3. ポータブル .NET SDK の準備

以下はリポジトリのルートで実行してください。管理者権限は不要です。

### 3.1 ツール用ディレクトリを作成

~~~powershell
New-Item -ItemType Directory -Force .tools\dotnet | Out-Null
New-Item -ItemType Directory -Force .tools\dotnet-home | Out-Null
New-Item -ItemType Directory -Force .tools\nuget | Out-Null
~~~

### 3.2 公式インストールスクリプトを取得

~~~powershell
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile .tools\dotnet-install.ps1
~~~

取得元は Microsoft の公式配布先です。組織のネットワークポリシーなどでダウンロードが制限される場合は、許可されたネットワークから取得してください。

### 3.3 .NET 10 SDK をリポジトリ内に展開

~~~powershell
& .\.tools\dotnet-install.ps1 -Channel 10.0 -InstallDir "$((Get-Location).Path)\.tools\dotnet"
~~~

このコマンドは .NET 10 の SDK を .tools\dotnet に展開します。システム全体へのインストールは行いません。

### 3.4 現在の PowerShell セッションだけに環境変数を設定

~~~powershell
$repo = (Get-Location).Path
$env:DOTNET_ROOT = Join-Path $repo ".tools\dotnet"
$env:DOTNET_CLI_HOME = Join-Path $repo ".tools\dotnet-home"
$env:NUGET_PACKAGES = Join-Path $repo ".tools\nuget"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

dotnet --info
~~~

設定はこの PowerShell セッションに限られます。新しいターミナルを開いた場合は、もう一度設定してください。永続的な PATH の変更は不要です。

## 4. NuGet パッケージの復元とビルド

環境変数を設定した同じ PowerShell セッションで、リポジトリのルートから実行します。

### 4.1 パッケージを復元

~~~powershell
dotnet restore .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj
~~~

Windows App SDK や Windows SDK Build Tools など、プロジェクトで参照しているパッケージが .tools\nuget に保存されます。

### 4.2 x64 / Debug でビルド

~~~powershell
dotnet build .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj -c Debug -p:Platform=x64 --no-restore
~~~

Release ビルドの場合は -c Debug を -c Release に変更してください。

## 5. 通常の作業で使うコマンド

SDK の展開後、毎回の作業では「3.4」の環境変数設定を行ってから、次のコマンドを使います。

~~~powershell
# 依存関係の復元
dotnet restore .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj

# ビルド
dotnet build .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj -c Debug -p:Platform=x64

# ビルド生成物の削除
dotnet clean .\src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj
~~~

## 6. Visual Studio が必要になるケース

次のような場合は、CLI だけでは解決できず Visual Studio Installer や Windows SDK の追加コンポーネントが必要になる可能性があります。

- MSIX のパッケージ化ターゲットが見つからない
- 開発用証明書の作成・信頼設定が必要
- アプリの配置やデバッグに IDE の支援が必要
- Windows SDK の署名ツールなど、NuGet パッケージに含まれないツールが必要

この場合も、最初から Visual Studio を導入するのではなく、エラーメッセージを確認し、不足しているコンポーネントだけを特定してください。Visual Studio / Windows SDK の一部は通常の ZIP 展開だけでは利用できず、完全なポータブル化ができない場合があります。

## 7. トラブルシューティング

### dotnet が見つからない

現在のセッションで環境変数を設定し直してください。

~~~powershell
$repo = (Get-Location).Path
$env:DOTNET_ROOT = Join-Path $repo ".tools\dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet --info
~~~

### NuGet の復元に失敗する

- インターネット接続とプロキシ設定を確認してください。
- 同じ PowerShell セッションで NUGET_PACKAGES が設定されているか確認してください。
- 依存パッケージのバージョンや NuGet ソースのエラーが出ていないか確認してください。

### Windows App SDK / MSIX 関連のターゲットが見つからない

まず dotnet restore を再実行してください。改善しない場合は、エラー全文と dotnet --info の結果を確認し、Windows SDK または Visual Studio の追加コンポーネントが必要か判断します。

### SDK のバージョンが想定と異なる

dotnet --info と dotnet --list-sdks を確認してください。.tools\dotnet 内の SDK が PATH 上の別の SDK より優先されていることも確認してください。

## 8. クリーンアップ

ポータブル環境を削除する場合は、リポジトリのルートで次を実行します。

~~~powershell
Remove-Item -Recurse -Force .tools
~~~

ビルド成果物だけを削除する場合は dotnet clean を使用してください。

## 9. プライバシーとセキュリティ

- .tools には SDK、NuGet パッケージ、CLI の作業データが保存されます。Git にコミットしないでください。
- 開発用証明書の秘密鍵（PFX）を Git にコミットしないでください。
- 通知ログにはDMのプレビューなど個人情報が含まれる可能性があります。テストログをIssueやPRへ貼る前に内容を確認し、必要に応じてマスキングしてください。
- SDK やパッケージは公式配布元・信頼できる NuGet ソースから取得してください。

---

## 現在の制約

この手順は、現行プロジェクト構成に基づくポータブル .NET SDK + CLI ビルドの手順です。クリーンな Windows 環境での完全な再現性、MSIX の署名・インストールまでの無人化はまだ保証していません。実機でビルドを確認しながら、必要なセットアップ処理を段階的に自動化します。
