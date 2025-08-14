# Performance test script for VRCSSDateTimeFixer

# Configuration
$testDir = "$PSScriptRoot\\PerformanceTest"
$testExe = "..\\VRCSSDateTimeFixer\\bin\\Release\\net8.0\\win-x64\\publish\\VRCSSDateTimeFixer.exe"

# Number of test files to create
$fileCount = 1000

# Create test directory if it doesn't exist
if (-not (Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir | Out-Null
}

# Create subdirectories for testing recursive processing
$subDirs = @("sub1", "sub2", "sub1\\sub1_1", "sub1\\sub1_2")
foreach ($dir in $subDirs) {
    $fullPath = Join-Path $testDir $dir
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath | Out-Null
    }
}

# Function to create test files with specific timestamps
function Create-PerformanceTestFiles($count, $baseDir) {
    $startTime = Get-Date
    $filesPerDir = [math]::Ceiling($count / $subDirs.Count)
    
    foreach ($dir in (Get-ChildItem -Path $baseDir -Directory -Recurse)) {
        Write-Host "Creating $filesPerDir test files in $($dir.FullName)..."
        
        for ($i = 1; $i -le $filesPerDir; $i++) {
            $date = (Get-Date).AddDays(-$i).ToString("yyyy-MM-dd")
            $time = Get-Date -Format "HH-mm-ss.fff"
            $fileName = "VRChat_${date}_${time}_1920x1080_${i}.png"
            $filePath = Join-Path $dir.FullName $fileName
            
            # Create a small file with test content
            "Performance test file $i" | Out-File -FilePath $filePath -Encoding UTF8
            
            # Set file timestamps to a fixed date
            $fileTime = (Get-Date "2020-01-01").AddMinutes($i)
            (Get-Item $filePath).CreationTime = $fileTime
            (Get-Item $filePath).LastWriteTime = $fileTime
            
            # Show progress
            if (($i % 100) -eq 0) {
                Write-Host "  Created $i files..."
            }
        }
    }
    
    $duration = ((Get-Date) - $startTime).TotalSeconds
    Write-Host "Created $count test files in $duration seconds" -ForegroundColor Green
}

# Run performance test
Write-Host "=== Performance Test ===" -ForegroundColor Cyan
Write-Host "Creating $fileCount test files in $testDir..."

# Create test files
Create-PerformanceTestFiles -Count $fileCount -BaseDir $testDir

# Get initial file count
$initialFiles = (Get-ChildItem -Path $testDir -File -Recurse).Count
Write-Host "Total test files created: $initialFiles" -ForegroundColor Green

# Test 1: Measure processing time for the entire directory
Write-Host "`n=== Test 1: Process entire directory (recursive) ===" -ForegroundColor Cyan
$startTime = Get-Date
& $testExe $testDir -r
$duration = ((Get-Date) - $startTime).TotalSeconds
Write-Host "Processed $initialFiles files in $duration seconds" -ForegroundColor Green

# Calculate files per second
$filesPerSecond = [math]::Round($initialFiles / $duration, 2)
Write-Host "Processing speed: $filesPerSecond files/second" -ForegroundColor Green

# Test 2: Verify timestamps were updated correctly
Write-Host "`n=== Test 2: Verify timestamps ===" -ForegroundColor Cyan
$sampleFile = Get-ChildItem -Path $testDir -File -Recurse | Select-Object -First 1
if ($sampleFile) {
    Write-Host "Sample file: $($sampleFile.Name)" -ForegroundColor Yellow
    Write-Host "  Creation Time: $($sampleFile.CreationTime)"
    Write-Host "  Last Write Time: $($sampleFile.LastWriteTime)"
    
    # Check if timestamp was updated from the original (2020-01-01)
    if ($sampleFile.CreationTime.Year -eq 2020) {
        Write-Host "WARNING: File timestamp was not updated!" -ForegroundColor Red
    } else {
        Write-Host "SUCCESS: File timestamp was updated correctly" -ForegroundColor Green
    }
}

# Clean up (uncomment to enable)
# Write-Host "`nCleaning up test files..." -ForegroundColor Cyan
# Remove-Item -Path $testDir -Recurse -Force

Write-Host "`nPerformance test complete!" -ForegroundColor Green
