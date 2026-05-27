# Iteración 1 — CrearSesionBusquedaTesoro

**Fase:** 1 (Domain + Unit Tests)
**Fecha:** 2026-05-26
**Fuentes de verdad:** `umbral-backend-spec.md §2.4`, `umbral-quality-spec.md §4.1`, `ddd-modeling-skill.md §3`

---

## Contexto

Esta es la iteración 0→1 de Fase 1. Antes de poder "iniciar" una sesión
es necesario poder *crearla*. El spec define dos factory methods separados:
`CrearBusquedaTesoro(snapshot, operadorId)` y `CrearTrivia(preguntas, operadorId)`.
Esta iteración implementa solo el primero, que requiere la cadena
**CatalogoBusquedaTesoro → MisionSnapshot → Sesion**.

---

## Objetos implementados

### Shared (rebuildados — sin genérico, como indica el spec)

| Archivo | Cambio |
|---------|--------|
| `Shared/IDomainEvent.cs` | Agregado `Guid EventId { get; }` |
| `Shared/Entity.cs` | Non-generic: `Entity` con `IdEquals(Entity)` + `GetIdHashCode()` |
| `Shared/AggregateRoot.cs` | `AggregateRoot : Entity` sin `<TId>`; `RaiseDomainEvent` (no `AddDomainEvent`) |
| `Shared/ValueObject.cs` | Alineado al spec canónico |

### BC CatalogoBusquedaTesoro (nuevo)

```
CatalogoBusquedaTesoro/Mision/
  MisionId.cs          ← record MisionId(Guid Valor)
  EtapaId.cs           ← record EtapaId(Guid Valor)
  PistaId.cs           ← record PistaId(Guid Valor)
  EstadoMision.cs      ← enum: Borrador | Activa
  TipoLiberacion.cs    ← enum: PorTiempo | PorGanador
  Pista.cs             ← Entity (internal static factory)
  Etapa.cs             ← Entity (Composite: Etapa → List<Pista>)
  Mision.cs            ← AggregateRoot (Composite: Mision → List<Etapa>)
  MisionSnapshot.cs    ← ValueObject (copia inmutable = ACL hacia Sesion)
  EtapaSnapshot.cs     ← ValueObject
  IMisionRepository.cs ← port de salida
  Events/
    MisionCreada.cs
    MisionActivada.cs
```

### BC Sesion (refactorizado)

| Archivo | Cambio |
|---------|--------|
| `Sesion/ValueObjects/SesionId.cs` | `record SesionId(Guid Valor)` (era clase genérica) |
| `Sesion/ValueObjects/EquipoId.cs` | Nuevo: reemplaza `EquipoSesionId` |
| `Sesion/ValueObjects/UsuarioId.cs` | Nuevo: para `OperadorId` |
| `Sesion/ValueObjects/NombreEquipo.cs` | Nuevo: VO con validación |
| `Sesion/ValueObjects/CodigoAcceso.cs` | Nuevo: VO para acceso de equipos |
| `Sesion/ValueObjects/Puntaje.cs` | Nuevo: VO con `Sumar`/`Restar` |
| `Sesion/EquipoSesion.cs` | Actualizado: usa `EquipoId`, `NombreEquipo`, `CodigoAcceso`, `Puntaje` |
| `Sesion/Penalizacion.cs` | Nuevo: VO con validación de puntos y motivo |
| `Sesion/ContextoBusquedaTesoro.cs` | Nuevo: Entity que contiene `MisionSnapshot` |
| `Sesion/Sesion.cs` | `CrearBusquedaTesoro()` reemplaza el antiguo `Crear()`; máquina de estados completa |
| `Sesion/Events/SesionCreada.cs` | Nuevo: emitido por `CrearBusquedaTesoro` |
| `Sesion/Events/SesionIniciada.cs` | Actualizado: `record` con `EventId` + `OcurridoEn` en body |
| `Sesion/ISesionRepository.cs` | Port de salida |

### Ports

```
Domain/Ports/
  IEventPublisher.cs        ← PublishBatchAsync(events, ct)
  INotificacionRealTime.cs  ← NotificarCambioEstadoSesionAsync(...)
```

---

## Reglas de negocio cubiertas

| ID | Regla |
|----|-------|
| R-CB-01 | La sesión nace en estado `Programada` |
| R-CB-02 | `TipoSesion == BusquedaTesoro` se asigna en el factory |
| R-CB-03 | Se emite `SesionCreada(SesionId, TipoSesion, OperadorId)` |
| R-CB-04 | `ContextoBT` contiene el `MisionSnapshot` (copia inmutable de la misión) |
| R-CB-05 | Dos creaciones generan `SesionId` distintos (unicidad garantizada por `Guid.NewGuid()`) |
| R-CB-06 | `snapshot == null` → `ArgumentNullException` |
| R-CB-07 | `operadorId == null` → `ArgumentNullException` |
| R-M-01 | `Mision.Crear` valida nombre no vacío → `DomainException` |
| R-M-02 | `Mision.Activar` requiere al menos una etapa |
| R-M-03 | `Mision.PuedeUsarseParaSesion()` solo es `true` si `EstadoMision.Activa` |
| R-MS-01 | `MisionSnapshot.Desde(mision)` es inmutable: cambios post-snapshot no afectan la copia |

---

## Tests agregados

### `SesionCrearTests.cs` — 11 tests

| Nombre del test | Tipo |
|-----------------|------|
| `CrearBusquedaTesoro_ConDatosValidos_RetornaSesionEnEstadoProgramada` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_TipoSesionEsBusquedaTesoro` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_OperadorIdAsignado` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_SesionIdNoEsDefault` | Happy |
| `CrearBusquedaTesoro_DosCalls_GeneranSesionIdsDistintos` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_EmiteSesionCreadaEvent` | Domain Event |
| `CrearBusquedaTesoro_ConDatosValidos_EventoSesionCreadaTienePayloadCorrecto` | Domain Event |
| `CrearBusquedaTesoro_ConDatosValidos_ContextoBTNoEsNull` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_ContextoBTContieneMisionSnapshot` | Happy |
| `CrearBusquedaTesoro_ConDatosValidos_SesionNaceConEquiposVacios` | Happy |
| `CrearBusquedaTesoro_CuandoSnapshotEsNull_LanzaArgumentNullException` | Error |
| `CrearBusquedaTesoro_CuandoOperadorIdEsNull_LanzaArgumentNullException` | Error |

### `MisionTests.cs` — 18 tests

Cubre: `Mision.Crear`, `AgregarEtapa`, `Activar`, `PuedeUsarseParaSesion`,
y `MisionSnapshot.Desde` (incluyendo el test de inmutabilidad del ACL).

**Total iteración 1: 30/30 ✅**

---

## Decisiones de diseño

### 1. Entity no-genérica (breaking change vs iteración anterior)
El spec define `AggregateRoot : Entity` sin `<TId>`. Cada agregado implementa
`IdEquals(Entity other)` y `GetIdHashCode()` de forma explícita. Esto es más
verboso pero evita el acoplamiento al tipo del ID en la clase base.

### 2. IDs como `record` con propiedad `Valor`
Los IDs son `sealed record SesionId(Guid Valor)`. Los records proveen igualdad
estructural automática, lo que satisface la comparación `sesionA.SesionId == sesionB.SesionId`.

### 3. MisionSnapshot como ACL inmutable
`MisionSnapshot.Desde(mision)` toma una *foto* en el momento de crear la sesión.
Cambios posteriores en `Mision` no afectan sesiones en curso. Esto implementa
la regla de dominio DA-05 del spec.

### 4. `AbrirParaRegistro()` se mantiene
La transición `Programada → EnPreparacion` requiere un método explícito en el
dominio, aunque no aparece nombrado en el spec de `Sesion`. El Application Layer
lo invocará en el Command Handler de apertura de sala. Se mantiene para preservar
la máquina de estados completa.

---

## Patrón aplicado

- **Factory Method**: `Sesion.CrearBusquedaTesoro(snapshot, operadorId)` — garantiza que el objeto nace en estado válido con todos los invariantes satisfechos.
- **Anti-Corruption Layer (ACL)**: `MisionSnapshot` es la frontera entre `CatalogoBusquedaTesoro` y el BC `Sesion`. El BC `Sesion` nunca importa `Mision` directamente.
- **Domain Event**: `SesionCreada` se emite inmediatamente en el factory; será despachado por el Application Layer *después* de persistir.

---

## Pendiente para Iteración 2 — RegistrarEquipo

- Verificar `Estado` permitido para registro (spec: cualquier estado no terminal).
- Tests para nombre duplicado, estado terminal.
- El builder `SesionBuilder` ya tiene soporte para `ConEstado` y `ConEquipo`.
