#!/usr/bin/env bash
# Cobertura backend (E1-3 / RNF-09) — Linux/macOS y GitHub Actions.
# Uso: ./scripts/run-coverage.sh [--threshold 90] [--no-per-assembly]
set -euo pipefail

THRESHOLD=0
PER_ASSEMBLY=1
while [[ $# -gt 0 ]]; do
  case "$1" in
    --threshold) THRESHOLD="$2"; shift 2 ;;
    --no-per-assembly) PER_ASSEMBLY=0; shift ;;
    *) echo "Uso: $0 [--threshold 90] [--no-per-assembly]"; exit 1 ;;
  esac
done

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

COVERAGE_DIR="$ROOT/coverage"
REPORT_DIR="$COVERAGE_DIR/report"
RUNSETTINGS="$ROOT/coverlet.runsettings"
SUMMARY="$REPORT_DIR/Summary.txt"

ASSEMBLIES=(
  "Umbral.Domain"
  "Umbral.Application"
  "Umbral.Infrastructure"
  "Umbral.API"
)

rm -rf "$COVERAGE_DIR"

echo ">> dotnet test (Release + cobertura)..."
dotnet test "$ROOT/Umbral.sln" -c Release \
  --collect:"XPlat Code Coverage" \
  --settings "$RUNSETTINGS" \
  --results-directory "$COVERAGE_DIR"

dotnet tool install -g dotnet-reportgenerator-globaltool >/dev/null 2>&1 || true
export PATH="$HOME/.dotnet/tools:$PATH"

echo ">> Generando reporte HTML..."
reportgenerator \
  -reports:"$COVERAGE_DIR/**/coverage.cobertura.xml" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:"Html;TextSummary"

TOTAL_PCT="$(grep -m1 'Line coverage:' "$SUMMARY" | grep -oE '[0-9]+(\.[0-9]+)?' | head -1)"

echo ""
echo "=== Cobertura por ensamblado (produccion) ==="
FAILED=0
for asm in "${ASSEMBLIES[@]}"; do
  PCT="$(grep -E "^${asm}[[:space:]]+[0-9]" "$SUMMARY" | grep -oE '[0-9]+(\.[0-9]+)?%' | head -1 | tr -d '%')"
  if [[ -z "$PCT" ]]; then
    echo "  $asm  (no encontrado en Summary.txt)"
    FAILED=1
    continue
  fi
  if [[ "$THRESHOLD" -gt 0 && "$PER_ASSEMBLY" -eq 1 ]]; then
    if awk -v p="$PCT" -v t="$THRESHOLD" 'BEGIN { exit (p+0 >= t+0) ? 0 : 1 }'; then
      printf "  %-28s %5.1f%%  OK (>= %s%%)\n" "$asm" "$PCT" "$THRESHOLD"
    else
      printf "  %-28s %5.1f%%  FAIL (< %s%%)\n" "$asm" "$PCT" "$THRESHOLD"
      FAILED=1
    fi
  else
    printf "  %-28s %5.1f%%\n" "$asm" "$PCT"
  fi
done

echo ""
echo "=== Cobertura total backend ==="
echo "  Line coverage: ${TOTAL_PCT}%"
echo ""
echo "Detalle por clase: $REPORT_DIR/index.html"
echo "Resumen texto:     $SUMMARY"

if [[ "$THRESHOLD" -gt 0 ]]; then
  if awk -v p="$TOTAL_PCT" -v t="$THRESHOLD" 'BEGIN { exit (p+0 >= t+0) ? 0 : 1 }'; then
    echo "OK: cobertura total ${TOTAL_PCT}% >= umbral ${THRESHOLD}%"
  else
    echo "FAIL: cobertura total ${TOTAL_PCT}% < umbral ${THRESHOLD}%"
    FAILED=1
  fi
fi

if [[ "$FAILED" -ne 0 ]]; then
  exit 1
fi
