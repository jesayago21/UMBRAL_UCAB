# Iteración E1-1 / E1-4 — Cobertura backend ≥ 90%

**Fase:** Entrega 1 — medición (A) + cierre de brecha (C)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `umbral-quality-spec.md §11.1.b`, `PLAN.md §6`

> **Resultado:** cobertura de líneas backend **96% (1572/1637)** — RNF-09 (≥ 90%) **cumplido**.
> 385 pruebas en verde.

---

## Cómo ejecutarla (para la defensa)

> Requiere **Docker en marcha** (los tests de Infrastructure y API levantan PostgreSQL
> con Testcontainers) y el SDK de .NET 8.

### Opción A — un solo comando (recomendado)

```powershell
.\scripts\run-coverage.ps1 -Open
```

Esto:
1. corre **todas** las pruebas en Release con recolección de cobertura,
2. genera el reporte HTML en `coverage/report/index.html`,
3. con `-Open` lo abre en el navegador,
4. imprime el resumen (línea **96%**).

Para mostrar el quality gate en vivo:

```powershell
.\scripts\run-coverage.ps1 -Threshold 90
# ...
# OK: cobertura de lineas 96% >= umbral 90%
```

### Opción B — comandos manuales (si no se quiere el script)

```powershell
dotnet test Umbral.sln -c Release `
  --collect:"XPlat Code Coverage" `
  --settings coverlet.runsettings `
  --results-directory coverage

reportgenerator `
  -reports:"coverage/**/coverage.cobertura.xml" `
  -targetdir:"coverage/report" `
  -reporttypes:"Html;TextSummary"

start coverage/report/index.html
```

### Qué enseñar al profesor

1. La consola con `Passed!` de los 4 proyectos de test y el `Line coverage: 96%`.
2. El `index.html`: barra global **96%** y el desglose por ensamblado.
3. (Opcional) `scripts/run-coverage.ps1 -Threshold 90` mostrando el gate en verde,
   que es el mismo criterio que correrá en CI (E1-3).

---

## Resultado final (2026-05-29)

**Pruebas:** 385/385 ✅ — Domain 207, Application 88, Infrastructure 14, API 76.

| Métrica | Valor | Meta RNF-09 |
|---------|-------|-------------|
| **Líneas (total)** | **96%** (1572/1637) | ≥ 90% ✅ |
| Ramas | 76.3% (165/216) | informativo |
| Métodos | 89.4% (280/313) | informativo |

| Ensamblado | Líneas |
|------------|--------|
| Umbral.Application | 100% |
| Umbral.Infrastructure | 99.4% |
| Umbral.API | 97.6% |
| Umbral.Domain | 89.1% |

> El total ya cumple ≥ 90% sin necesidad de reporte aparte. `Umbral.Domain` queda en
> 89.1% por factories `*Id.Nuevo()`/`ToString()` poco usadas; no afecta el total.

---

## Qué se hizo para llegar al 90%+

### 1. Configuración correcta de exclusiones (`coverlet.runsettings`)

El recolector `--collect:"XPlat Code Coverage"` **no** lee las props de
`tests/Directory.Build.props`; usa un `.runsettings`. Se creó `coverlet.runsettings`
en la raíz con las exclusiones que el `quality-spec §11/§13` ya autoriza:

- `ExcludeByFile`: `**/Migrations/**/*.cs` — migraciones EF Core y `ModelSnapshot`
  (código generado por la CLI).
- `ExcludeByAttribute`: `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`,
  `ExcludeFromCodeCoverageAttribute` — boilerplate de `record` (Equals/GetHashCode/init
  generados) y artefactos de diseño.
- `UmbralDbContextFactory` marcada `[ExcludeFromCodeCoverage]` (factoría de diseño,
  solo la usa la CLI de migraciones).

### 2. Tests TDD para cerrar huecos de negocio reales

| Área | Antes | Después | Tests añadidos |
|------|-------|---------|----------------|
| `ResultExtensions` | 20% | **100%** | `ResultExtensionsTests` (Ok/Created/NoContent/Fail) |
| `ApiServiceCollectionExtensions` + mapeo `realm_access` | 21% | **100%** | `KeycloakWiringTests` (wiring prod + roles) |
| `KeycloakOptions` | 0% | **100%** | idem |
| `ListMisionesQueryHandler` | 52% | **100%** | `ListMisionesQueryHandlerTests` (filtros nombre/estado) |
| `ActualizarPreguntaCommandHandler` | 76% | **100%** | +4 casos (eliminada, categoría asignar/inexistente/eliminada) |
| `ValueObject` / `Entity` (Domain.Shared) | 38% / 50% | **100%** | `ValueObjectTests`, `EntityTests` |

---

## Baseline previo (para registro de la brecha)

Antes de E1-4 (sin runsettings, contando migraciones y boilerplate):

- Línea total: **87.6%** (4553/5196) — brecha **−2.4 pp**.
- Por ensamblado: API 84%, Application 96%, Domain 87%, Infrastructure 86.1%.

El salto a 96% proviene de (a) excluir correctamente código generado/CLI en la métrica
oficial y (b) cubrir con tests las clases de negocio que sí debían probarse.

---

## Pendiente

| Ítem | Acción |
|------|--------|
| **E1-3** | Workflow CI que ejecute `run-coverage.ps1 -Threshold 90` (o equivalente) y publique el HTML como artefacto |
