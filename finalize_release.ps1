# Script to finalize the release process
# This script should be run after all tests have passed and code review is complete

# Configuration
. "$PSScriptRoot\version.ps1"
$releaseDir = "bin\\Release\\Publish"
$distDir = "dist"

# Ensure we're on the main branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Warning "You are not on the 'main' branch. Current branch: $currentBranch"
    $proceed = Read-Host "Do you want to continue? (y/n)"
    if ($proceed -ne 'y') {
        exit 1
    }
}

# Ensure there are no uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Error "There are uncommitted changes. Please commit or stash them before finalizing the release."
    exit 1
}

# Pull the latest changes
Write-Host "=== Updating repository ===" -ForegroundColor Cyan
git pull

# Run tests to ensure everything is working
Write-Host "\n=== Running tests ===" -ForegroundColor Cyan
dotnet test
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed. Please fix the issues before finalizing the release."
    exit 1
}

# Update version in project file (if needed)
# Note: This is already set in the .csproj file

# Create a release commit
Write-Host "\n=== Creating release commit ===" -ForegroundColor Cyan
git add .
git commit -m "Prepare for release v$version"

# Create an annotated tag
Write-Host "\n=== Creating tag v$version ===" -ForegroundColor Cyan
git tag -a "v$version" -m "Version $version"

# Build the release package
Write-Host "\n=== Building release package ===" -ForegroundColor Cyan
.\Release.ps1

# Create distribution directory
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

# Copy release files to dist directory
$releaseFiles = @(
    "$releaseDir\\VRCSSDateTimeFixer-$version-win-x64.zip"
    "README.md"
    "LICENSE"
    "CHANGELOG.md"
)

foreach ($file in $releaseFiles) {
    if (Test-Path $file) {
        Copy-Item -Path $file -Destination $distDir -Force
        Write-Host "Copied $file to $distDir"
    } else {
        Write-Warning "File not found: $file"
    }
}

# Display next steps
Write-Host "\n=== Release Finalization Complete ===" -ForegroundColor Green
Write-Host "\nNext steps:" -ForegroundColor Cyan
Write-Host "1. Review the changes:"
Write-Host "   git log --oneline -n 5"
Write-Host "\n2. Push the changes and tags to the remote repository:"
Write-Host "   git push origin main"
Write-Host "   git push origin v$version"
Write-Host "\n3. Create a GitHub release:"
Write-Host "   .\\create_release.ps1"
Write-Host "\n4. Update the documentation (if needed):"
Write-Host "   - Update README.md with any new features or changes"
Write-Host "   - Update CHANGELOG.md with the release date"
Write-Host "\n5. Announce the release (if applicable):"
Write-Host "   - Update any relevant documentation"
Write-Host "   - Notify users or community"
