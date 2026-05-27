# Fase 1 · Iteración 1 — `IniciarSesion`

> **BC afectado:** Ejecución de Sesión  
> **Aggregate:** `Sesion`  
> **Método de dominio:** `Sesion.Iniciar(DateTime ahora)`  
> **Estado:** implementado y testeado ✓

---

## 1. Descripción del caso de uso

El operador inicia una sesión que estaba en estado `Preparacion`.
A partir de ese momento la sesión pasa a `Activa` y los equipos participantes
pueden comenzar a interactuar con ella (escanear QR, responder trivia, etc.).

---

## 2. Reglas de negocio cubiertas

| ID | Regla | Consecuencia de violación |
|----|-------|--------------------------|
| **R1** | Una sesión no puede iniciarse si no tiene al menos un equipo registrado | `DomainException("No se puede iniciar la sesión sin equipos registrados.")` |
| **R2** | La transición `Iniciar()` solo es válida desde el estado `Preparacion` | `DomainException("No se puede iniciar una sesión en estado '...'.")` |

---

## 3. Diagrama de estado (fragmento relevante)

```
             Iniciar()
               [R1: equipos > 0]
               [R2: estado == Preparacion]
 ┌─────────────┐                   ┌────────┐
 │ Preparacion │ ──────────────────► Activa │
 └─────────────┘                   └────────┘

 Cualquier otro estado (Activa, Pausada, Finalizada, Cancelada)
 → DomainException al intentar Iniciar()
```

---

## 4. Objetos de dominio creados en esta iteración

### Cimientos `Shared/`

| Archivo | Tipo | Responsabilidad |
|---------|------|-----------------|
| `IDomainEvent.cs` | Interface | Contrato de todos los eventos de dominio |
| `Entity<TId>` | Abstract class | Base con identidad tipada y equality por Id |
| `AggregateRoot<TId>` | Abstract class | Hereda `Entity`, gestiona lista de `DomainEvents` |
| `ValueObject` | Abstract class | Equality estructural por componentes |
| `DomainException` | Sealed class | Excepción de regla de negocio (sin deps externas) |

### BC `Sesion/`

| Archivo | Tipo | Notas |
|---------|------|-------|
| `SesionId` | Typed ID (`IEquatable<SesionId>`) | Wraps `Guid`; rechaza `Guid.Empty` |
| `EquipoSesionId` | Typed ID | Igual que `SesionId` |
| `EstadoSesion` | Enum | `Preparacion=1, Activa=2, Pausada=3, Finalizada=4, Cancelada=5` |
| `TipoSesion` | Enum | `BusquedaTesoro=1, Trivia=2` |
| `EquipoSesion` | Entity | Mínimo: `Id + Nombre` validado. Se expande en Iter 2 |
| `Events/SesionIniciada` | Domain Event (record) | `SesionId + OcurridoEn` |
| `Sesion` | AggregateRoot | Factory `Crear()`, `RegistrarEquipo()` stub, `Iniciar()` |

---

## 5. Flujo dentro del agregado

```
Sesion.Iniciar(ahora)
│
├─► Guard R2: Estado != Preparacion → DomainException
│
├─► Guard R1: _equipos.Count == 0  → DomainException
│
├─► _estado     = EstadoSesion.Activa
├─► _iniciadaEn = ahora
│
└─► AddDomainEvent(new SesionIniciada(Id, ahora))
```

El Domain Event es **transitorio** (vive solo en memoria durante el ciclo de
vida del agregado). El handler de Application lo leerá con
`sesion.DomainEvents` y lo publicará vía `IEventPublisher` al finalizar.

---

## 6. Tests unitarios

**Archivo:** `tests/Umbral.Domain.Tests/Sesion/SesionIniciarTests.cs`

| Test | Tipo | Verifica |
|------|------|----------|
| `Iniciar_ConUnEquipoRegistrado_CambiaEstadoAActiva` | Happy path | R1 + R2 OK → estado `Activa` |
| `Iniciar_ConVariosEquipos_CambiaEstadoAActiva` | Happy path | Funciona con N equipos |
| `Iniciar_RegistraTimestampDeInicio` | Happy path | `IniciadaEn` se asigna correctamente |
| `Iniciar_EmiteDomainEventSesionIniciada` | Happy path | Evento emitido con datos correctos |
| `Iniciar_SinEquipos_LanzaDomainException` | Error R1 | Mensaje contiene "sin equipos" |
| `Iniciar_SinEquipos_NoEmiteDomainEvent` | Error R1 | Sin efecto secundario |
| `Iniciar_DesdeEstadoActiva_LanzaDomainException` | Error R2 | Mensaje contiene "'Activa'" |
| `Iniciar_DesdeEstado_Finalizada` | Error R2 (Theory) | `DomainException` con estado en mensaje |
| `Iniciar_DesdeEstado_Cancelada` | Error R2 (Theory) | ídem |
| `Iniciar_DesdeEstado_Pausada` | Error R2 (Theory) | ídem |

**Resultado:** `10 / 10 Passed`

---

## 7. Patrones aplicados

| Patrón | Aplicación en esta iteración |
|--------|------------------------------|
| **State** (base) | El estado vive en campo privado con setter `private`. Solo `Iniciar()` puede cambiarlo. Las transiciones futuras (`Pausar`, `Finalizar`) se añadirán de forma análoga. |
| **Factory Method** | `Sesion.Crear(id, tipo)` — único punto de creación del agregado. |
| **Domain Events** | `SesionIniciada` emitido como record inmutable transitorio; no hay dependencia de infraestructura. |
| **Typed IDs** | `SesionId` y `EquipoSesionId` previenen confundir IDs en tiempo de compilación. |

---

## 8. Decisiones de diseño

**¿Por qué `Iniciar` recibe `DateTime ahora` como parámetro?**  
Elimina la dependencia de `DateTime.UtcNow` dentro del dominio, manteniendo
el núcleo puro y los tests deterministas. La capa Application inyectará
`IDateTimeProvider.UtcNow` cuando se implemente el handler.

**¿Por qué `RegistrarEquipo` está como stub sin reglas completas?**  
El caso de uso `RegistrarEquipo` es la Iteración 2. Tener el stub permite
preparar el estado en los tests de la Iteración 1 sin implementar reglas
que aún no se deben testear en esta iteración.

**¿Por qué los IDs tipados en lugar de `Guid` directo?**  
Evita errores en llamadas como `Iniciar(sesionId: equipoId)` que compilarían
pero serían incorrectas en tiempo de ejecución.

---

## 9. Qué queda pendiente (Iteración 2)

- `Sesion.RegistrarEquipo(equipo)` con reglas completas:
  - Nombre único por sesión
  - Rechazo si estado es `Finalizada` o `Cancelada`
- Domain Event `EquipoRegistrado`
- Tests: nombre duplicado, sesión terminal, happy path
