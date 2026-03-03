# Publish AgentNotifier as a standalone executable

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = Join-Path $projectDir "publish"

Write-Host "Publishing AgentNotifier..." -ForegroundColor Cyan

# Clean previous publish
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

# Publish
dotnet publish "$projectDir\AgentNotifier.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $publishDir

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Published successfully!" -ForegroundColor Green
    Write-Host "Location: $publishDir" -ForegroundColor Yellow
    Write-Host ""

    # List files
    Get-ChildItem $publishDir | ForEach-Object {
        Write-Host "  $($_.Name) - $([math]::Round($_.Length / 1MB, 2)) MB"
    }

    Write-Host ""
    Write-Host "Run with: $publishDir\AgentNotifier.exe" -ForegroundColor Cyan
} else {
    Write-Host "Publish failed!" -ForegroundColor Red
}
