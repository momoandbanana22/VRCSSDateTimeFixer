# Build and package script for VRCSSDateTimeFixer

# Configuration
$version = "1.0.0"
$configuration = "Release"
$runtime = "win-x64"
$projectPath = "VRCSSDateTimeFixer\VRCSSDateTimeFixer.csproj"
$outputDir = "bin\$configuration\Publish"
$publishDir = "$outputDir\VRCSSDateTimeFixer-$version-$runtime"
$zipFile = "$outputDir\VRCSSDateTimeFixer-$version-$runtime.zip"

# Clean up
Write-Host "Cleaning up previous builds..." -ForegroundColor Cyan
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore $projectPath

# Publish the application
Write-Host "Publishing application for $runtime..." -ForegroundColor Cyan
dotnet publish $projectPath `
    -c $configuration `
    -r $runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -p:DebugType=embedded `
    -p:DebugSymbols=true `
    -o $publishDir

# Create a README.txt in the publish directory
@"
VRChat Screenshot Date/Time Fixer v$version
=======================================

このツールは、VRChatのスクリーンショットのファイル名から日時情報を抽出し、
ファイルのタイムスタンプとExif情報を更新します。

使い方:
  VRCSSDateTimeFixer.exe <ファイルまたはディレクトリのパス> [オプション]

オプション:
  -r, --recursive   サブディレクトリを再帰的に処理します
  --version         バージョン情報を表示
  -?, -h, --help   ヘルプを表示

詳細は https://github.com/momoandbanana22/VRCSSDateTimeFixer を参照してください。
"@ | Out-File -FilePath "$publishDir\README.txt" -Encoding utf8

# Create a ZIP archive
Write-Host "Creating ZIP archive..." -ForegroundColor Cyan
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile -Force

Write-Host "`nBuild and package completed successfully!" -ForegroundColor Green
Write-Host "Output file: $((Get-Item $zipFile).FullName)" -ForegroundColor Green
