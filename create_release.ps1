# Script to create a GitHub release and upload assets
# Requires GitHub CLI (gh) to be installed and authenticated

# Configuration
. "$PSScriptRoot\version.ps1"
$releaseNotes = @"
## What's New in Version $version

### Features
- Initial release of VRChat Screenshot Date/Time Fixer
- Extract date/time from VRChat screenshot filenames
- Update file timestamps (creation and last modified)
- Update EXIF metadata for supported image formats
- Process single files or entire directories
- Recursive directory processing option

### Bug Fixes
- Initial release - no bug fixes yet

### Known Issues
- None at this time

### Dependencies
- .NET 8.0 Runtime (included in self-contained package)
"@

# Build the release
Write-Host "=== Building Release ===" -ForegroundColor Cyan
.\Release.ps1

# Get the path to the release assets
$releaseDir = "bin\\Release\\Publish"
$zipFile = "$releaseDir\\VRCSSDateTimeFixer-$version-win-x64.zip"

if (-not (Test-Path $zipFile)) {
    Write-Error "Release package not found at $zipFile"
    exit 1
}

# Create GitHub release
Write-Host "`n=== Creating GitHub Release ===" -ForegroundColor Cyan
$releaseNotesFile = "$releaseDir\\release_notes.md"
$releaseNotes | Out-File -FilePath $releaseNotesFile -Encoding utf8

try {
    # Create the release draft
    gh release create "v$version" `
        --title "Version $version" `
        --notes-file "$releaseNotesFile" `
        --draft `
        --prerelease

    # Upload the release asset
    Write-Host "`n=== Uploading Release Asset ===" -ForegroundColor Cyan
    gh release upload "v$version" "$zipFile" --clobber

    # Publish the release (uncomment when ready)
    # gh release edit "v$version" --draft=false

    Write-Host "`nRelease created successfully!" -ForegroundColor Green
    Write-Host "Review the draft release at: https://github.com/momoandbanana22/VRCSSDateTimeFixer/releases" -ForegroundColor Blue
    Write-Host "When ready, uncomment the 'gh release edit' line in this script to publish the release." -ForegroundColor Yellow
}
catch {
    Write-Error "Failed to create release: $_"
    exit 1
}
finally {
    # Clean up
    if (Test-Path $releaseNotesFile) {
        Remove-Item $releaseNotesFile -Force
    }
}
