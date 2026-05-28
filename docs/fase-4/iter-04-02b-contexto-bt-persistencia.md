# Iteración 04-02b — Persistencia ContextoBusquedaTesoro (cierre deuda Fase 3)

## Objetivo

Persistir `Sesion.ContextoBT` en PostgreSQL para que sesiones recargadas desde BD conserven snapshot de misión, índice de etapa y ganador de etapa.

## Cambios implementados

- `SesionConfiguration`: `OwnsOne` → tabla `contextos_bt` (ya no `Ignore`).
- Columnas: `"SesionId"` (PK/FK), `mision_snapshot_json` (jsonb), `etapa_actual_index`, `ganador_etapa_actual_id`.
- `MisionSnapshotPersistence` + `MisionSnapshotJsonValueConverter` (DTO JSON en Infrastructure).
- Métodos `Rehydrate` en `MisionSnapshot`, `EtapaSnapshot`, `ContextoBusquedaTesoro` (internos; `InternalsVisibleTo` Infrastructure).
- Migración EF: `20260528000441_AddContextoBusquedaTesoro`.
- `SesionRepository`: `Include(s => s.ContextoBT)` en lecturas; `SyncContextoBtAsync` (UPSERT) en actualizaciones.
- Tests: `ContextoBusquedaTesoroPersistenceTests`.

## Validación

```powershell
dotnet build Umbral.sln
dotnet test Umbral.sln
```

Aplicar migración en entornos locales:

```powershell
dotnet ef database update --project src\backend\Umbral.Infrastructure --startup-project src\backend\Umbral.API
```

## Notas

- La columna FK quedó como `"SesionId"` (convención EF owned type); el UPSERT raw SQL usa ese nombre entre comillas.
- Sesiones **antiguas** creadas antes de esta migración no tendrán fila en `contextos_bt`; recrear sesión o script de backfill si hace falta en dev.
