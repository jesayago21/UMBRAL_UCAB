# Iteración 2 — RegistrarEquipo (HU-13)

**Fase:** 1 (Domain + Unit Tests)  
**Fecha:** 2026-05-26  
**Fuentes:** `umbral-backend-spec.md §2.4`, `umbral-quality-spec.md §4.1`, ERS HU-13

---

## Caso de uso

El operador inscribe equipos en una sesión de Búsqueda del Tesoro antes de iniciarla.
Cada equipo recibe un `CodigoAcceso` único para unirse desde mobile (Entrega 1, Fase 7).

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| RB-13-01 | Nombre único por sesión (comparación sin distinguir mayúsculas) |
| RB-13-02 | Rechazo en `Finalizada` y `Cancelada` |
| RB-13-03 | `CodigoAcceso` generado por equipo al registrarse |
| RB-13-04 | `NombreEquipo` valida y recorta espacios |
| RB-13-05 | Entrada en `HistorialEventos` con tipo `EquipoRegistrado` |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/EventoSesion.cs` | **Nuevo** — auditoría interna del agregado |
| `Sesion/Sesion.cs` | `HistorialEventos`, `RegistrarEvento`, `RegistrarEquipo` alineado al spec |
| `tests/.../SesionRegistrarEquipoTests.cs` | **Nuevo** — 15 tests HU-13 |
| `tests/.../SesionBuilder.cs` | `SinEquipos()` para setups explícitos |

---

## Tests (15 nuevos → 45 total)

| Test | Escenario |
|------|-----------|
| `RegistrarEquipo_CuandoNombreUnico_AgregaEquipo` | Happy path + `CodigoAcceso` |
| `RegistrarEquipo_CuandoNombreUnico_AsociaSesionIdDelAgregado` | `SesionId` / `EquipoId` |
| `RegistrarEquipo_CuandoNombreUnico_PuntajeInicialEsCero` | `Puntaje.Zero()` |
| `RegistrarEquipo_CuandoDosNombresDistintos_AgregaDosEquipos` | Múltiples equipos |
| `RegistrarEquipo_CuandoDosEquipos_CodigosAccesoSonDistintos` | Códigos distintos |
| `RegistrarEquipo_CuandoSesionNoEstaCerrada_PermiteRegistro` | `Programada` y `EnPreparacion` |
| `RegistrarEquipo_CuandoNombreConEspacios_AplicaTrim` | Trim |
| `RegistrarEquipo_CuandoNombreUnico_RegistraEventoEnHistorial` | `EventoSesion` |
| `RegistrarEquipo_CuandoNombreDuplicado_LanzaDomainException` | Duplicado exacto |
| `RegistrarEquipo_CuandoDuplicadoIgnoraMayusculas_LanzaDomainException` | Case-insensitive |
| `RegistrarEquipo_CuandoNombreVacio_LanzaDomainException` | Theory `""`, `"   "` |
| `RegistrarEquipo_CuandoSesionCerrada_LanzaDomainException` | Theory `Finalizada`, `Cancelada` |

---

## Historias de usuario (ERS)

| HU | Título | Estado |
|----|--------|--------|
| **HU-13** | Inscripción de equipos | ✅ |
| HU-12 | Crear sesión BT | ✅ (iter-01) |

**Tests:** 45/45  
**Tracker:** [TRACKER.md](TRACKER.md)

---

## Pendiente — Iteración 3 (HU-14 IniciarSesion)

- Tests dedicados para `Iniciar()`, `AbrirParaRegistro()`, estados inválidos
- Domain events `SesionIniciada` en historial (si aplica)
