# Iteración E1-3 — CI con gate de cobertura backend

**Fase:** Entrega 1 — Fase A (devops)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `umbral-quality-spec.md §11.1.b`, `§11.3`, `PLAN.md §6`

> **Objetivo:** pipeline que ejecuta `dotnet test` + cobertura y **falla** si líneas
> backend < 90% (RNF-09).

---

## Artefactos

| Archivo | Rol |
|---------|-----|
| `.github/workflows/ci.yml` | Workflow GitHub Actions (push/PR) |
| `scripts/run-coverage.sh` | Mismo flujo que `run-coverage.ps1` en Linux/CI |
| `coverlet.runsettings` | Exclusiones del recolector (§11.1.b) |

---

## Qué hace el pipeline

1. **Checkout** + **.NET 8**
2. `dotnet restore` + `dotnet build` Release
3. `bash scripts/run-coverage.sh --threshold 90`
   - Corre los 385 tests (Docker/Testcontainers en el runner)
   - Genera `coverage/report/index.html`
   - **Exit 1** si line coverage < 90%
4. Sube artefacto **`coverage-report`** (HTML, 14 días)

**Triggers:** `push` y `pull_request` en `main`, `develop`, `feature/**`.

---

## Ejecución local (equivalente a CI)

```powershell
# Windows
.\scripts\run-coverage.ps1 -Threshold 90
```

```bash
# Linux / macOS / WSL
bash scripts/run-coverage.sh --threshold 90
```

Requisito: **Docker en marcha** (Testcontainers).

---

## Gates activos en E1-3 (Entrega 1)

| Gate | Umbral | Estado |
|------|--------|--------|
| Tests backend pasan | 100% | ✅ |
| Line coverage backend | ≥ 90% | ✅ (96% al cerrar E1-4) |
| Frontend / E2E / Docker smoke | — | Fuera de E1-3 → Entrega 2 o E1-5 |

Los demás gates de `quality-spec §11.3` se activan cuando exista frontend (Vitest)
y E2E.

---

## Verificación

Tras push a GitHub, comprobar en **Actions** que el job `Backend (test + cobertura)`
esté en verde y descargar el artefacto `coverage-report`.

---

## Pendiente

| Ítem | Acción |
|------|--------|
| **E1-2** | Frontend CRUD (Vitest + gate 80% cuando exista `umbral-web`) |
| **E1-5** | README con badge/link a Actions |
