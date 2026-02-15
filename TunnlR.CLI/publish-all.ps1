# Save as publish-all.ps1 in TunnlR.CLI folder

Write-Host "Publishing TunnlR CLI for all platforms..." -ForegroundColor Cyan

# Windows
Write-Host "`nBuilding for Windows..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish/win-x64

# macOS Intel
Write-Host "`nBuilding for macOS (Intel)..." -ForegroundColor Yellow
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ./publish/osx-x64

# macOS Apple Silicon
Write-Host "`nBuilding for macOS (Apple Silicon)..." -ForegroundColor Yellow
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o ./publish/osx-arm64

# Linux
Write-Host "`nBuilding for Linux..." -ForegroundColor Yellow
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./publish/linux-x64

Write-Host "`n✓ All builds complete!" -ForegroundColor Green
Write-Host "Binaries are in ./publish/ folder" -ForegroundColor Cyan