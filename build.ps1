$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'src\InstagramDmMonitor.App\InstagramDmMonitor.App.csproj'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET 10 SDK が見つかりません。README.md の必要環境を確認してください。'
}

dotnet restore $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build $project -c Debug -p:Platform=x64
exit $LASTEXITCODE
