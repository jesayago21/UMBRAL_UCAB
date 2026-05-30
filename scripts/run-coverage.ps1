# Cobertura backend (E1-1 / E1-4 / RNF-09).
#
# Uso:
#   .\scripts\run-coverage.ps1                 # mide y genera reporte HTML
#   .\scripts\run-coverage.ps1 -Open           # ademas abre el reporte en el navegador
#   .\scripts\run-coverage.ps1 -Threshold 90   # falla (exit 1) si el total < 90%
#
# Requiere Docker en marcha (los tests de Infrastructure/API usan Testcontainers).
param(
    [int]$Threshold = 0,
    [switch]$Open
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$CoverageDir = Join-Path $Root "coverage"
$ReportDir   = Join-Path $CoverageDir "report"
$RunSettings = Join-Path $Root "coverlet.runsettings"

# Partir de cero evita mezclar XMLs de corridas anteriores.
if (Test-Path $CoverageDir) {
    Remove-Item $CoverageDir -Recurse -Force
}

Write-Host ">> dotnet test (Release + cobertura)..." -ForegroundColor Cyan
dotnet test (Join-Path $Root "Umbral.sln") -c Release `
    --collect:"XPlat Code Coverage" `
    --settings $RunSettings `
    --results-directory $CoverageDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: hay pruebas en rojo. Cobertura no generada." -ForegroundColor Red
    exit 1
}

if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host ">> Instalando reportgenerator (dotnet global tool)..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool | Out-Null
}

$homeDir = if ($env:USERPROFILE) { $env:USERPROFILE } else { $env:HOME }
$dotnetTools = Join-Path $homeDir ".dotnet/tools"
if ((Test-Path $dotnetTools) -and ($env:PATH -notlike "*$dotnetTools*")) {
    $env:PATH = "$dotnetTools$([IO.Path]::PathSeparator)$env:PATH"
}

Write-Host ">> Generando reporte HTML..." -ForegroundColor Cyan
reportgenerator `
    -reports:(Join-Path $CoverageDir "**/coverage.cobertura.xml") `
    -targetdir:$ReportDir `
    -reporttypes:"Html;TextSummary"

$summaryPath = Join-Path $ReportDir "Summary.txt"

Write-Host ""
Write-Host "=== Resumen de cobertura backend ===" -ForegroundColor Green
Get-Content $summaryPath | Select-Object -First 20

Write-Host ""
Write-Host "Reporte HTML: $ReportDir\index.html" -ForegroundColor Green

if ($Open) {
    Start-Process (Join-Path $ReportDir "index.html")
}

if ($Threshold -gt 0) {
    $line = Get-Content $summaryPath | Where-Object { $_ -match "Line coverage:" } | Select-Object -First 1
    if ($line -match "([\d.]+)%") {
        $pct = [double]$Matches[1]
        if ($pct -lt $Threshold) {
            Write-Host "FAIL: cobertura de lineas $pct% < umbral $Threshold%" -ForegroundColor Red
            exit 1
        }
        Write-Host "OK: cobertura de lineas $pct% >= umbral $Threshold%" -ForegroundColor Green
    }
}
