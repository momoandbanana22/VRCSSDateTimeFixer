# Script to publish the release on GitHub
# This should be run after finalize_release.ps1

# Configuration
. .\version.ps1

# Check if GitHub CLI is installed (robust PATH + well-known locations)
$ghCmd = $null
$gh = Get-Command gh -ErrorAction SilentlyContinue
if ($gh) { $ghCmd = $gh.Source }
if (-not $ghCmd) {
    $candidates = @()
    if ($Env:ProgramFiles) { $candidates += (Join-Path $Env:ProgramFiles 'GitHub CLI\gh.exe') }
    if (${Env:ProgramFiles(x86)}) { $candidates += (Join-Path ${Env:ProgramFiles(x86)} 'GitHub CLI\gh.exe') }
    if ($Env:LOCALAPPDATA) {
        $candidates += (Join-Path $Env:LOCALAPPDATA 'Programs\GitHub CLI\gh.exe')
        $candidates += (Join-Path $Env:LOCALAPPDATA 'Programs\gh\bin\gh.exe')
        $candidates += (Join-Path $Env:LOCALAPPDATA 'Microsoft\WinGet\Links\gh.exe')
    }
    if ($Env:ChocolateyInstall) { $candidates += (Join-Path $Env:ChocolateyInstall 'bin\gh.exe') }
    foreach ($p in $candidates) {
        if ($p -and (Test-Path $p)) { $ghCmd = $p; break }
    }
}
if (-not $ghCmd) {
    Write-Error "GitHub CLI (gh) is not installed or not found in PATH. Please install from https://cli.github.com/ and restart your shell."
    exit 1
}

# Check if user is logged in to GitHub
$ghAuth = & $ghCmd auth status 2>&1
if ($ghAuth -like "*not logged*" -or $ghAuth -like "*not authenticated*") {
    Write-Host "You need to authenticate with GitHub. Please follow the prompts..." -ForegroundColor Yellow
    & $ghCmd auth login
}

# Verify the release
Write-Host "=== Verifying Release ===" -ForegroundColor Cyan
Write-Host "Version: $version"
Write-Host "Tag: v$version"
Write-Host "\nThis will:"
Write-Host "1. Push the main branch to GitHub"
Write-Host "2. Push the v$version tag to GitHub"
Write-Host "3. Create a GitHub release with the release notes"
Write-Host "4. Upload the release assets"

$confirm = Read-Host "\nDo you want to continue? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Release cancelled." -ForegroundColor Yellow
    exit 0
}

try {
    # Push changes to GitHub
    Write-Host "\n=== Pushing changes to GitHub ===" -ForegroundColor Cyan
    git push origin main
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to push changes to main branch"
    }

    # Push the tag
    Write-Host "\n=== Pushing tag v$version ===" -ForegroundColor Cyan
    git push origin "v$version"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to push tag v$version"
    }

    # Create the GitHub release
    Write-Host "\n=== Creating GitHub Release ===" -ForegroundColor Cyan
    .\create_release.ps1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub release"
    }

    # Final message
    Write-Host "\n=== Release Published Successfully! ===" -ForegroundColor Green
    Write-Host "\nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Review the release at: https://github.com/momoandbanana22/VRCSSDateTimeFixer/releases"
    Write-Host "2. Consider announcing the release to your users"
    Write-Host "3. Update any documentation or websites that reference the release"
    Write-Host "\nThank you for using VRCSSDateTimeFixer!" -ForegroundColor Green
}
catch {
    Write-Error "Failed to publish release: $_"
    exit 1
}
