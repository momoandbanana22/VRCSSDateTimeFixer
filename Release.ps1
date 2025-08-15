# Release script for VRCSSDateTimeFixer
# This script builds, tests, and packages the application for release

# Stop on first error
$ErrorActionPreference = "Stop"

# Configuration
. "$PSScriptRoot\version.ps1"
$configuration = "Release"
$runtime = "win-x64"
$projectPath = "VRCSSDateTimeFixer\VRCSSDateTimeFixer.csproj"
$testProjectPath = "VRCSSDateTimeFixer.Tests\VRCSSDateTimeFixer.Tests.csproj"
$outputDir = "bin\$configuration\Publish"
$publishDir = "$outputDir\VRCSSDateTimeFixer-$version-$runtime"
$zipFile = "$outputDir\VRCSSDateTimeFixer-$version-$runtime.zip"

# Clean up
Write-Host "=== Cleaning up previous builds ===" -ForegroundColor Cyan
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

# Restore NuGet packages
Write-Host "`n=== Restoring NuGet packages ===" -ForegroundColor Cyan
dotnet restore

# Build the solution
Write-Host "`n=== Building solution ===" -ForegroundColor Cyan
dotnet build -c $configuration --no-restore

# Run tests
Write-Host "`n=== Running tests ===" -ForegroundColor Cyan
dotnet test $testProjectPath -c $configuration --no-build --verbosity normal

# Publish the application
Write-Host "`n=== Publishing application ===" -ForegroundColor Cyan
dotnet publish $projectPath `
    -c $configuration `
    -r $runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -p:DebugType=embedded `
    -p:DebugSymbols=true `
    -o $publishDir `
    --no-build

# Copy additional files to publish directory
Copy-Item -Path "README.md" -Destination $publishDir -Force
Copy-Item -Path "LICENSE" -Destination $publishDir -Force
Copy-Item -Path "CHANGELOG.md" -Destination $publishDir -Force

# Create a README.txt in the publish directory
@"
VRChat Screenshot Date/Time Fixer v$version
========================================

このツールは、VRChatのスクリーンショットのファイル名から日時情報を抽出し、
ファイルのタイムスタンプとExif情報を更新します。

使い方:
  VRChatScreenshotDateTimeFixer.exe <ファイルまたはディレクトリのパス> [オプション]

オプション:
  -r, --recursive   サブディレクトリを再帰的に処理します
  --version         バージョン情報を表示
  -?, -h, --help   ヘルプを表示

詳細は https://github.com/momoandbanana22/VRCSSDateTimeFixer を参照してください。
"@ | Out-File -FilePath "$publishDir\README.txt" -Encoding utf8

# Create a ZIP archive
Write-Host "`n=== Creating ZIP archive ===" -ForegroundColor Cyan
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile -Force

# Display completion message
$zipFileInfo = Get-Item $zipFile
$zipSize = "{0:N2} MB" -f ($zipFileInfo.Length / 1MB)

Write-Host "`n=== Release package created successfully! ===" -ForegroundColor Green
Write-Host "Output directory: $((Get-Item $publishDir).FullName)" -ForegroundColor Green
Write-Host "ZIP file: $($zipFileInfo.FullName) ($zipSize)" -ForegroundColor Green

# Instructions for creating a GitHub release
Write-Host "`n=== Next steps for GitHub release ===" -ForegroundColor Cyan
Write-Host "1. Create a new release on GitHub:"
Write-Host "   https://github.com/momoandbanana22/VRCSSDateTimeFixer/releases/new" -ForegroundColor Blue
Write-Host "2. Set the tag to 'v$version'"
Write-Host "3. Set the release title to 'Version $version'"
Write-Host "4. Copy the contents of CHANGELOG.md for this version into the release notes"
Write-Host "5. Attach the following file to the release:"
Write-Host "   $($zipFileInfo.FullName)" -ForegroundColor Blue
Write-Host "6. Publish the release!"
