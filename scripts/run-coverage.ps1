# Cobertura backend (E1-1 / E1-4 / RNF-09).
#
# Uso:
#   .\scripts\run-coverage.ps1                 # mide y genera reporte HTML
#   .\scripts\run-coverage.ps1 -Open           # ademas abre el reporte en el navegador
#   .\scripts\run-coverage.ps1 -Threshold 90   # falla si el total o algun ensamblado < 90%
#
# Requiere Docker en marcha (los tests de Infrastructure/API usan Testcontainers).
param(
    [int]$Threshold = 0,
    [switch]$Open,
    [switch]$PerAssembly = $true
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$CoverageDir = Join-Path $Root "coverage"
$ReportDir   = Join-Path $CoverageDir "report"
$RunSettings = Join-Path $Root "coverlet.runsettings"

$BackendAssemblies = @(
    "Umbral.Domain",
    "Umbral.Application",
    "Umbral.Infrastructure",
    "Umbral.API"
)

function Get-AssemblyLineCoverage {
    param([string]$SummaryPath)
    $result = [ordered]@{}
    foreach ($line in Get-Content $SummaryPath) {
        if ($line -match "^(Umbral\.(?:Domain|Application|Infrastructure|API))\s+([\d.]+)%") {
            $result[$Matches[1]] = [double]$Matches[2]
        }
    }
    return $result
}

function Get-TotalLineCoverage {
    param([string]$SummaryPath)
    $line = Get-Content $SummaryPath | Where-Object { $_ -match "^\s*Line coverage:" } | Select-Object -First 1
    if ($line -match "([\d.]+)%") {
        return [double]$Matches[1]
    }
    return $null
}

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
$assemblyCoverage = Get-AssemblyLineCoverage $summaryPath
$totalPct = Get-TotalLineCoverage $summaryPath

Write-Host ""
Write-Host "=== Cobertura por ensamblado (produccion) ===" -ForegroundColor Green
foreach ($asm in $BackendAssemblies) {
    if ($assemblyCoverage.Contains($asm)) {
        $pct = $assemblyCoverage[$asm]
        $status = ""
        if ($Threshold -gt 0 -and $PerAssembly) {
            $status = if ($pct -ge $Threshold) { " OK" } else { " FAIL" }
            $color = if ($pct -ge $Threshold) { "Green" } else { "Red" }
            Write-Host ("  {0,-28} {1,5:N1}%  (meta >= {2}%){3}" -f $asm, $pct, $Threshold, $status) -ForegroundColor $color
        }
        else {
            Write-Host ("  {0,-28} {1,5:N1}%" -f $asm, $pct)
        }
    }
    else {
        Write-Host "  $asm  (no encontrado en Summary.txt)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Cobertura total backend ===" -ForegroundColor Green
if ($null -ne $totalPct) {
    Write-Host ("  Line coverage: {0:N1}%" -f $totalPct)
}
Write-Host ""
Write-Host "Detalle por clase: $ReportDir\index.html (secciones Umbral.Domain, ...)" -ForegroundColor Green
Write-Host "Resumen texto:     $summaryPath" -ForegroundColor Green

if ($Open) {
    Start-Process (Join-Path $ReportDir "index.html")
}

$failed = $false
if ($Threshold -gt 0) {
    if ($null -ne $totalPct -and $totalPct -lt $Threshold) {
        Write-Host "FAIL: cobertura total $totalPct% < umbral $Threshold%" -ForegroundColor Red
        $failed = $true
    }
    elseif ($null -ne $totalPct) {
        Write-Host "OK: cobertura total $totalPct% >= umbral $Threshold%" -ForegroundColor Green
    }

    if ($PerAssembly) {
        foreach ($asm in $BackendAssemblies) {
            if (-not $assemblyCoverage.Contains($asm)) {
                Write-Host "FAIL: no se pudo leer cobertura de $asm" -ForegroundColor Red
                $failed = $true
                continue
            }
            $pct = $assemblyCoverage[$asm]
            if ($pct -lt $Threshold) {
                Write-Host "FAIL: $asm $pct% < umbral $Threshold%" -ForegroundColor Red
                $failed = $true
            }
            else {
                Write-Host "OK: $asm $pct% >= umbral $Threshold%" -ForegroundColor Green
            }
        }
    }
}

if ($failed) {
    exit 1
}
