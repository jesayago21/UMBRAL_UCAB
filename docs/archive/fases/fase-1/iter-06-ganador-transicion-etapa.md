# Iteración 6 — Ganador único + Transición de etapa (HU-19 + HU-20)

**Fase:** 1 (Domain + Unit Tests)  
**Fecha:** 2026-05-27  
**Fuentes:** `umbral-backend-spec.md §6.1`, `umbral-product-spec.md` Módulo 7, ERS HU-19, HU-20

---

## Caso de uso

Cuando un participante envía la primera evidencia QR **válida** en una etapa:

1. Recibe **100 puntos** (`CalculoPuntajeBusquedaService`, RB-04)
2. Se emite `EvidenciaValidada`
3. Se emite `EtapaCompletada` y se registra en historial
4. Si no es la última etapa → `ContextoBT.AvanzarEtapa()` (RB-05, HU-20)
5. Si es la última etapa → `Sesion.Finalizar()` automático

Participantes posteriores con QR válido para etapa ya ganada reciben `Invalida` (sin puntaje).

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| RB-04 | Primer participante válido gana la etapa; demás no puntúan |
| RB-05 | Al completar nodo, todos avanzan al siguiente (via `AvanzarEtapa`) |
| RF-09 | Puntaje asignado al ganador (100 pts/etapa) |
| RF-21 | Transición automática al siguiente nodo |
| — | Última etapa completada → sesión `Finalizada` automáticamente |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/Events/EvidenciaValidada.cs` | **Nuevo** |
| `Sesion/Events/EtapaCompletada.cs` | **Nuevo** |
| `Sesion/CalculoPuntajeBusquedaService.cs` | **Nuevo** — 100 pts ganador |
| `Sesion/ContextoBusquedaTesoro.cs` | `GanadorEtapaActualId`, `RegistrarGanadorEtapa`, `AvanzarEtapa` |
| `Sesion/Sesion.cs` | `ProcesarEvidenciaGanadora`, check `YaHayGanadorEnEtapaActual` |
| `tests/.../CalculoPuntajeBusquedaServiceTests.cs` | **Nuevo** — 2 tests |
| `tests/.../SesionGanadorTransicionTests.cs` | **Nuevo** — 10 tests |
| `tests/.../SesionRegistrarEvidenciaTests.cs` | Actualizado test 2 participantes (RB-04) |

---

## Tests (12 nuevos → 149 total)

| Clase | Tests | Cubre |
|-------|-------|-------|
| `CalculoPuntajeBusquedaServiceTests` | 2 | 100 pts ganador, 0 no ganador |
| `SesionGanadorTransicionTests` | 10 | puntaje, events, avance, RB-04, auto-finalizar, 200 pts 2 etapas |
| `SesionRegistrarEvidenciaTests` | 1 actualizado | 2 participantes: solo primero puntúa |

---

## Historias de usuario (ERS)

| HU | Título | Estado |
|----|--------|--------|
| **HU-19** | Validar ganador único de etapa | ✅ |
| **HU-20** | Transición automática de fase | ✅ |
| HU-18 | Enviar evidencia QR | ✅ (iter-05) |

**Tests:** 149/149  
**Tracker:** [TRACKER.md](TRACKER.md)

---

## Pendiente — Iteración 7 (HU-23 Cerrar/Cancelar sesión)

- Tests dedicados para `Finalizar()` y `Cancelar()` con domain events e historial
- Reporte final de sesión (dominio)
