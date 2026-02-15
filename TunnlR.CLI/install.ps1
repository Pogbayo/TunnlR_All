# ============================================
# TunnlR CLI Installation Script for Windows
# ============================================

# Display installation start message in cyan color
Write-Host "Installing TunnlR CLI..." -ForegroundColor Cyan

# Detect if user is running 64-bit or 32-bit Windows
# Most modern Windows are 64-bit, but this ensures compatibility
$arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }

# Build the download URL for the correct Windows binary from GitHub releases
# "latest" automatically gets the most recent release
$downloadUrl = "https://github.com/Pogbayo/TunnlR_All/releases/latest/download/tunnlr-windows-$arch.exe"

# Set installation location to user's AppData folder
# %LOCALAPPDATA% is typically C:\Users\YourName\AppData\Local
$installPath = "$env:LOCALAPPDATA\tunnlr"

# Full path where the executable will be saved
$exePath = "$installPath\tunnlr.exe"

# Create the installation directory if it doesn't exist
# -Force means don't throw error if directory already exists
# Out-Null hides the output (we don't need to see "Directory created")
New-Item -ItemType Directory -Force -Path $installPath | Out-Null

# Download the binary from GitHub and save it to the install location
Write-Host "Downloading..." -ForegroundColor Yellow
Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath

# Get the current user's PATH environment variable
# PATH tells Windows where to look for executable files
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")

# Check if our install path is already in PATH
# -notlike means "does not contain"
if ($userPath -notlike "*$installPath*") {
    Write-Host "Adding to PATH..." -ForegroundColor Yellow
    
    # Add our install directory to PATH permanently (for this user)
    # This allows running "tunnlr" from any folder
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$installPath", "User")
    
    # Also add to PATH for current session (so it works immediately)
    $env:Path = "$env:Path;$installPath"
}

# Success message
Write-Host "`n✓ TunnlR installed successfully!" -ForegroundColor Green
Write-Host "Run: " -NoNewline
Write-Host "tunnlr" -ForegroundColor Cyan
Write-Host "`nNote: You may need to restart your terminal." -ForegroundColor DarkGray