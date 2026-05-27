# Iteración 5 — RegistrarEvidencia base (HU-18 + HU-19 parcial)

**Fase:** 1 (Domain + Unit Tests)  
**Fecha:** 2026-05-27  
**Fuentes:** `umbral-backend-spec.md §6.2`, `umbral-product-spec.md` Módulo 7, `ddd-modeling-skill.md`, ERS HU-18

---

## Caso de uso

Un equipo escanea un código QR y lo envía como evidencia. El sistema:

1. Valida que la sesión esté **Activa** (RB-19)
2. Compara el QR con el de la **etapa activa** (RB-06, RB-22)
3. Registra la evidencia con timestamp y resultado (`Valida` / `Invalida` / `Rechazada`) — RF-11
4. Emite `EvidenciaRegistrada` domain event

**HU-19 parcial:** un mismo equipo no puede registrar dos evidencias **válidas** en la misma etapa.  
**Pendiente iter-06:** ganador único (RB-04), bloqueo de otros equipos, puntaje y transición de etapa.

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| RB-06 | Solo se valida contra la etapa activa en `ContextoBT` |
| RB-19 | Sesión no activa → `ResultadoValidacion.Rechazada` |
| RB-22 | QR debe coincidir con `CodigoQRSolucion` de etapa activa |
| RF-11 | Cada envío queda registrado con fecha, equipo, sesión y resultado |
| HU-19p | Mismo equipo + misma etapa + segunda válida → `Invalida` |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/ResultadoValidacion.cs` | **Nuevo** — enum Valida/Invalida/Rechazada |
| `Sesion/ValueObjects/CodigoQR.cs` | **Nuevo** — VO con `CoincideCon` case-insensitive |
| `Sesion/ValueObjects/EvidenciaId.cs` | **Nuevo** — typed ID |
| `Sesion/Evidencia.cs` | **Nuevo** — entity con timestamp y resultado |
| `Sesion/Events/EvidenciaRegistrada.cs` | **Nuevo** — domain event |
| `Sesion/Validacion/ValidacionEvidenciaService.cs` | **Nuevo** — domain service (base chain) |
| `Sesion/ContextoBusquedaTesoro.cs` | `ObtenerEtapaActual()`, `EsUltimaEtapa()` |
| `Sesion/Sesion.cs` | `_evidencias`, `RegistrarEvidencia()` |
| `tests/.../ContextoBusquedaTesoroTests.cs` | **Nuevo** — 3 tests |
| `tests/.../CodigoQRTests.cs` | **Nuevo** — 6 tests |
| `tests/.../ValidacionEvidenciaServiceTests.cs` | **Nuevo** — 7 tests |
| `tests/.../SesionRegistrarEvidenciaTests.cs` | **Nuevo** — 14 tests |

---

## Tests (30 nuevos → 137 total)

| Clase | Tests | Cubre |
|-------|-------|-------|
| `ContextoBusquedaTesoroTests` | 3 | Etapa activa, es última |
| `CodigoQRTests` | 6 | Crear, trim, CoincideCon, igualdad |
| `ValidacionEvidenciaServiceTests` | 7 | Valida, Invalida, Rechazada×5 estados |
| `SesionRegistrarEvidenciaTests` | 14 | Happy, evento, historial, QR inválido, pausada, cerrada, guards, duplicado, multi-equipo |

---

## Historias de usuario (ERS)

| HU | Título | Estado |
|----|--------|--------|
| **HU-18** | Equipo envía evidencia QR | ✅ |
| HU-19 | Ganador único de etapa | 🔶 parcial (duplicado mismo equipo); completo en iter-06 |
| HU-16 | Aplicar penalización | ✅ (iter-04) |

**Tests:** 137/137  
**Tracker:** [TRACKER.md](TRACKER.md)

---

## Pendiente — Iteración 6 (HU-19 completo + HU-20)

- Primer equipo válido gana la etapa (RB-04) → puntaje + `EvidenciaValidada`
- Bloquear puntaje a equipos posteriores en la misma etapa
- `ContextoBT.AvanzarEtapa()` → transición automática (HU-20)
- Emitir `EtapaCompletada` domain event
