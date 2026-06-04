# Iteración 04-04 — Sesiones: penalización y evidencia

## Objetivo

Exponer HU-16 y HU-18 vía REST, con tests de integración.

## Cambios implementados

- `SesionesController`:
  - `POST /api/v1/sesiones/{id}/penalizaciones` → `AplicarPenalizacionCommand` (204; `OperadorId` desde claim)
  - `POST /api/v1/sesiones/{id}/evidencias` → `SubmitEvidenciaCommand` (201 + `SubmitEvidenciaResponse`)
- Contratos: `AplicarPenalizacionRequest`, `SubmitEvidenciaRequest`, `SubmitEvidenciaResponse`
- `TestAuthHandler`: claim `Participante` para probar evidencias en Development/Testing
- Tests API: feliz, validación, dominio y not found por endpoint

## Validación

```powershell
dotnet build Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

## Notas

- Evidencias: rol `Participante` (spec §7.2). En 04-05b el JWT de participante reemplazará el claim de prueba.
- QR de etapa 1 en tests de integración: `QR-API-001` (`ApiTestData.SeedMisionActivaAsync`).
