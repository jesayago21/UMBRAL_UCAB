# Iteración 3 — IniciarSesion + PausarReanudar (HU-14 + HU-15)

**Fase:** 1 (Domain + Unit Tests)  
**Fecha:** 2026-05-27  
**Fuentes:** `umbral-backend-spec.md §2.4`, `umbral-quality-spec.md §4.1`, ERS HU-14, HU-15

---

## Caso de uso

El operador controla el ciclo de vida activo de la sesión:

1. **AbrirParaRegistro** — `Programada → EnPreparacion` (habilita registro de equipos)
2. **Iniciar** — `EnPreparacion → Activa` (comienza el juego, requiere ≥1 equipo)
3. **Pausar** — `Activa → Pausada` (detiene la sesión temporalmente)
4. **Reanudar** — `Pausada → Activa` (retoma la sesión)

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| RB-14-01 | `Iniciar` solo válido desde `EnPreparacion` |
| RB-14-02 | `Iniciar` requiere ≥1 equipo registrado |
| RB-14-03 | `Iniciar` emite `SesionIniciada(SesionId, TipoSesion)` |
| RB-14-04 | `Iniciar` registra `IniciadaEn = DateTime.UtcNow` |
| RB-14-05 | `AbrirParaRegistro` solo válido desde `Programada` |
| RB-15-01 | `Pausar` solo válido desde `Activa` |
| RB-15-02 | `Pausar` emite `SesionPausada(SesionId)` |
| RB-15-03 | `Reanudar` solo válido desde `Pausada` |
| RB-15-04 | `Reanudar` emite `SesionReanudada(SesionId)` |
| RB-ALL | Todos los cambios de estado registran entrada en `HistorialEventos` |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/Events/SesionPausada.cs` | **Nuevo** |
| `Sesion/Events/SesionReanudada.cs` | **Nuevo** |
| `Sesion/Events/SesionFinalizada.cs` | **Nuevo** (preparado para iter-07) |
| `Sesion/Sesion.cs` | `Pausar`/`Reanudar`/`Finalizar` emiten domain events; `AbrirParaRegistro` e `Iniciar` registran en historial |
| `tests/.../SesionCicloVidaTests.cs` | **Nuevo** — 32 tests HU-14 + HU-15 |

---

## Tests (32 nuevos → 77 total)

### AbrirParaRegistro (3 + 1 Theory×4 = 7)
| Test | Escenario |
|------|-----------|
| `AbrirParaRegistro_CuandoEstaProgramada_CambiaEstadoAEnPreparacion` | Happy |
| `AbrirParaRegistro_CuandoEstaProgramada_RegistraEventoEnHistorial` | Historial |
| `AbrirParaRegistro_CuandoNoEstaProgramada_LanzaDomainException` | Theory×4 estados |

### Iniciar (6 + 1 Theory×4 = 10)
| Test | Escenario |
|------|-----------|
| `Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva` | Happy + `IniciadaEn` |
| `Iniciar_CuandoEstaEnPreparacion_EmiteSesionIniciadaEvent` | Domain event |
| `Iniciar_CuandoEstaEnPreparacion_EventoContieneSesionIdYTipo` | Payload evento |
| `Iniciar_CuandoEstaEnPreparacion_RegistraEventoEnHistorial` | Historial |
| `Iniciar_SinEquiposRegistrados_LanzaDomainException` | Guard equipos |
| `Iniciar_CuandoEstadoNoEsEnPreparacion_LanzaDomainException` | Theory×4 estados |

### Pausar (4 + 1 Theory×5 = 9)
| Test | Escenario |
|------|-----------|
| `Pausar_CuandoEstaActiva_CambiaEstadoAPausada` | Happy |
| `Pausar_CuandoEstaActiva_EmiteSesionPausadaEvent` | Domain event |
| `Pausar_CuandoEstaActiva_RegistraEventoEnHistorial` | Historial |
| `Pausar_CuandoNoEstaActiva_LanzaDomainException` | Theory×5 estados |

### Reanudar (4 + 1 Theory×5 = 9)
| Test | Escenario |
|------|-----------|
| `Reanudar_CuandoEstaPausada_CambiaEstadoAActiva` | Happy |
| `Reanudar_CuandoEstaPausada_EmiteSesionReanudadaEvent` | Domain event |
| `Reanudar_CuandoEstaPausada_RegistraEventoEnHistorial` | Historial |
| `Reanudar_CuandoNoEstaPausada_LanzaDomainException` | Theory×5 estados |

### Ciclo completo (1)
| Test | Escenario |
|------|-----------|
| `CicloCompleto_AbrirRegistrarIniciarPausarReanudar_EstadosCorrectos` | Flujo end-to-end dominio |

---

## Historias de usuario (ERS)

| HU | Título | Estado |
|----|--------|--------|
| **HU-14** | Control de inicio de sesión | ✅ |
| **HU-15** | Pausa y reanudación de sesión | ✅ |
| HU-13 | Registrar equipos | ✅ (iter-02) |
| HU-12 | Crear sesión BT | ✅ (iter-01) |

**Tests:** 77/77  
**Tracker:** [TRACKER.md](TRACKER.md)

---

## Pendiente — Iteración 4 (HU-16 AplicarPenalizacion)

- Tests dedicados: puntaje rebaja, motivo vacío, sesión no activa, equipo inexistente
- `SesionBuilder` ya soporta `.Activa().ConEquipo(nombre)` para setup
