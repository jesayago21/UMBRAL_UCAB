# Iteración 4 — AplicarPenalizacion (HU-16)

**Fase:** 1 (Domain + Unit Tests)  
**Fecha:** 2026-05-27  
**Fuentes:** `docs/TRAZABILIDAD.md`, `umbral-backend-spec.md §2.4`, ERS **HU-16**

---

## Caso de uso

El operador aplica una penalización de puntaje a un participante durante una sesión activa.

---

## Reglas implementadas

| ID | Regla |
|----|-------|
| RB-16-01 | `AplicarPenalizacion` solo válido en estado `Activa` |
| RB-16-02 | El participante debe pertenecer a la sesión (por `ParticipanteId`) |
| RB-16-03 | El puntaje no baja de cero (`Math.Max(0, valor - cantidad)`) → regla global **RB-24** |
| RB-16-04 | Emite `PenalizacionAplicada(SesionId, ParticipanteId, Puntos, Motivo, OperadorId)` |
| RB-16-05 | Registra entrada en `HistorialEventos` con tipo `PenalizacionAplicada` |
| RB-16-P1 | `Penalizacion.Puntos` debe ser `> 0` |
| RB-16-P2 | `Penalizacion.Motivo` no puede estar vacío ni solo espacios → **RB-20** |

---

## Archivos tocados

| Archivo | Cambio |
|---------|--------|
| `Sesion/Events/PenalizacionAplicada.cs` | **Nuevo** — domain event con SesionId, ParticipanteId, Puntos, Motivo, OperadorId |
| `Sesion/Sesion.cs` | `AplicarPenalizacion`: emit `PenalizacionAplicada` + registra historial |
| `tests/.../PuntajeTests.cs` | **Nuevo** — 13 tests del VO `Puntaje` (umbral-quality-spec §4.2) |
| `tests/.../SesionAplicarPenalizacionTests.cs` | **Nuevo** — 17 tests HU-16 |
| `tests/.../Builders/SesionBuilder.cs` | `Activa()` ya no pre-agrega "EquipoDefault" a lista interna |

---

## Tests (30 nuevos → 107 total)

### PuntajeTests — Value Object (13)

| Test | Tipo |
|------|------|
| `Crear_CuandoValorNegativo_LanzaDomainException` | Guard |
| `Crear_CuandoValorCero_EsValido` | Happy |
| `Crear_CuandoValorPositivo_AlmacenaValor` | Happy |
| `Zero_RetornaPuntajeCero` | Happy |
| `Sumar_RetornaValorCorrecto` | Happy |
| `Sumar_NoMutaInstanciaOriginal` | Inmutabilidad |
| `Sumar_VariasVeces_AcumulaCorrectamente` | Happy |
| `Restar_CuandoResultadoSeriaNegatvo_RetornaCero` | Floor |
| `Restar_CuandoCantidadExacta_RetornaCero` | Borde |
| `Restar_CuandoCantidadMenor_RestaCorrectamente` | Happy |
| `Restar_NoMutaInstanciaOriginal` | Inmutabilidad |
| `Igualdad_CuandoMismoValor_SonIguales` | ValueObject |
| `Igualdad_CuandoDistintoValor_NoSonIguales` | ValueObject |

### SesionAplicarPenalizacionTests (17)

| Test | Tipo |
|------|------|
| `AplicarPenalizacion_CuandoSesionActiva_RestarPuntaje` | Happy (spec §4.1) |
| `AplicarPenalizacion_CuandoSesionActiva_EmitePenalizacionAplicadaEvent` | Domain event |
| `AplicarPenalizacion_CuandoSesionActiva_EventoContieneDatosCorrectos` | Payload evento |
| `AplicarPenalizacion_CuandoSesionActiva_RegistraEventoEnHistorial` | Historial |
| `AplicarPenalizacion_CuandoPuntajeMenorQuePenalizacion_ResultaCero` | Floor |
| `AplicarPenalizacion_CuandoParticipanteConPuntajeCero_PermaneceCero` | Floor |
| `AplicarPenalizacion_VariasPenalizaciones_AcumulanCorrectamente` | Acumulado |
| `AplicarPenalizacion_CuandoSesionNoActiva_LanzaDomainException` | Theory×5 estados |
| `AplicarPenalizacion_CuandoParticipanteNoPerteneceSesion_LanzaDomainException` | Guard participante |
| `AplicarPenalizacion_CuandoPuntosInvalidos_LanzaDomainException` | Theory×2 (0, -5) |
| `AplicarPenalizacion_CuandoMotivoVacio_LanzaDomainException` | Theory×2 ("", "   ") |

---

## Historias de usuario (ERS)

| HU | Título | Estado |
|----|--------|--------|
| **HU-16** | Aplicar penalización a participante | ✅ |
| HU-15 | Pausar / reanudar sesión | ✅ (iter-03) |
| HU-14 | Iniciar sesión | ✅ (iter-03) |
| HU-13 | Registrar participantes | ✅ (iter-02) |
| HU-12 | Crear sesión BT | ✅ (iter-01) |

**Tests:** 107/107  
**Tracker:** [TRACKER.md](TRACKER.md)

---

## Pendiente — Iteración 5 (HU-18 RegistrarEvidencia)

- Equipo envía evidencia QR → validar código contra etapa activa del `ContextoBT`
- Dominio: `ContextoBusquedaTesoro.ValidarQR(string qr)` → registra hallazgo
- Emitirá `EvidenciaRegistrada` domain event
