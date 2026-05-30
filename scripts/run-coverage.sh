#!/usr/bin/env bash
# Cobertura backend (E1-3 / RNF-09) — Linux/macOS y GitHub Actions.
# Uso: ./scripts/run-coverage.sh [--threshold 90]
set -euo pipefail

THRESHOLD=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --threshold) THRESHOLD="$2"; shift 2 ;;
    *) echo "Uso: $0 [--threshold 90]"; exit 1 ;;
  esac
done

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

COVERAGE_DIR="$ROOT/coverage"
REPORT_DIR="$COVERAGE_DIR/report"
RUNSETTINGS="$ROOT/coverlet.runsettings"

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

SUMMARY="$REPORT_DIR/Summary.txt"
echo ""
echo "=== Resumen de cobertura backend ==="
head -n 20 "$SUMMARY"
echo ""
echo "Reporte HTML: $REPORT_DIR/index.html"

if [[ "$THRESHOLD" -gt 0 ]]; then
  PCT="$(grep -m1 'Line coverage:' "$SUMMARY" | grep -oE '[0-9]+(\.[0-9]+)?' | head -1)"
  if awk -v p="$PCT" -v t="$THRESHOLD" 'BEGIN { exit (p+0 >= t+0) ? 0 : 1 }'; then
    echo "OK: cobertura de lineas ${PCT}% >= umbral ${THRESHOLD}%"
  else
    echo "FAIL: cobertura de lineas ${PCT}% < umbral ${THRESHOLD}%"
    exit 1
  fi
fi
