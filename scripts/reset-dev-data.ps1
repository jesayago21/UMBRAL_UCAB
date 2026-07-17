# Limpia misiones y sesiones de desarrollo (PostgreSQL en Docker).
# No borra categorías ni preguntas del banco de trivia.
# Uso: .\scripts\reset-dev-data.ps1

$ErrorActionPreference = "Stop"
$Container = "umbral_postgres"

$running = docker ps --filter "name=$Container" --filter "status=running" -q
if (-not $running) {
    Write-Error "El contenedor '$Container' no está en ejecución. Ejecuta: docker compose up postgres -d"
}

$sql = @"
TRUNCATE TABLE
  respuestas_trivia,
  evidencias,
  eventos_sesion,
  participantes_sesion,
  contextos_bt,
  contextos_trivia,
  contextos_mision,
  sesiones,
  pistas,
  etapas,
  misiones
RESTART IDENTITY CASCADE;
"@

docker exec $Container psql -U umbral_user -d umbral_db -c $sql
Write-Host "OK - misiones y sesiones eliminadas (categorias y preguntas conservadas)."
