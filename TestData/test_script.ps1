# Test script for VRCSSDateTimeFixer

# Configuration
$testDir = "$PSScriptRoot\TestFiles"
$testExe = "..\VRCSSDateTimeFixer\bin\Release\net8.0\win-x64\publish\VRCSSDateTimeFixer.exe"

# Create test directory if it doesn't exist
if (-not (Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir | Out-Null
}

# Function to create a test file with specific timestamp
function Create-TestFile($fileName, $dateTime) {
    $filePath = Join-Path $testDir $fileName
    "Test content for $fileName" | Out-File -FilePath $filePath -Encoding UTF8
    (Get-Item $filePath).CreationTime = $dateTime
    (Get-Item $filePath).LastWriteTime = $dateTime
    return $filePath
}

# Create test files with different timestamps
Write-Host "Creating test files..." -ForegroundColor Cyan

# Current date for reference
$now = Get-Date

# Create test files with different timestamps
$testFile1 = Create-TestFile "VRChat_2023-01-15_14-30-45.123_1920x1080.png" (Get-Date "2020-01-01")
$testFile2 = Create-TestFile "VRChat_2023-02-20_09-15-22.456_1280x720.png" (Get-Date "2020-01-02")
$testFile3 = Create-TestFile "VRChat_2023-03-25_18-45-10.789_3840x2160.png" (Get-Date "2020-01-03")

# Create a subdirectory with more test files
$subDir = Join-Path $testDir "Subdirectory"
if (-not (Test-Path $subDir)) {
    New-Item -ItemType Directory -Path $subDir | Out-Null
}
$testFile4 = Create-TestFile "Subdirectory\VRChat_2023-04-30_12-00-00.000_1920x1080.png" (Get-Date "2020-01-04")

# Create a file with invalid name format
$invalidFile = Create-TestFile "invalid_filename.png" (Get-Date "2020-01-05")

Write-Host "Test files created in: $testDir" -ForegroundColor Green

# Function to display file timestamps
function Show-FileTimestamps($filePath) {
    $file = Get-Item $filePath
    Write-Host "File: $($file.Name)" -ForegroundColor Yellow
    Write-Host "  Creation Time: $($file.CreationTime)"
    Write-Host "  Last Write Time: $($file.LastWriteTime)"
    Write-Host "  Last Access Time: $($file.LastAccessTime)"
    Write-Host ""
}

# Show original timestamps
Write-Host "`nOriginal timestamps:" -ForegroundColor Cyan
Get-ChildItem -Path $testDir -Recurse -File | ForEach-Object { Show-FileTimestamps $_.FullName }

# Test 1: Process a single file
Write-Host "`n=== Test 1: Process a single file ===" -ForegroundColor Cyan
& $testExe $testFile1
Write-Host "`nAfter processing single file:" -ForegroundColor Green
Show-FileTimestamps $testFile1

# Test 2: Process a directory
Write-Host "`n=== Test 2: Process a directory (non-recursive) ===" -ForegroundColor Cyan
& $testExe $testDir
Write-Host "`nAfter processing directory (non-recursive):" -ForegroundColor Green
Get-ChildItem -Path $testDir -File | ForEach-Object { Show-FileTimestamps $_.FullName }

# Test 3: Process recursively
Write-Host "`n=== Test 3: Process directory (recursive) ===" -ForegroundColor Cyan
& $testExe $testDir -r
Write-Host "`nAfter processing directory (recursive):" -ForegroundColor Green
Get-ChildItem -Path $testDir -Recurse -File | ForEach-Object { Show-FileTimestamps $_.FullName }

# Test 4: Show help
Write-Host "`n=== Test 4: Show help ===" -ForegroundColor Cyan
& $testExe --help

# Test 5: Show version
Write-Host "`n=== Test 5: Show version ===" -ForegroundColor Cyan
& $testExe --version

# Clean up (uncomment to enable)
# Write-Host "`nCleaning up test files..." -ForegroundColor Cyan
# Remove-Item -Path $testDir -Recurse -Force

Write-Host "`nTesting complete!" -ForegroundColor Green
