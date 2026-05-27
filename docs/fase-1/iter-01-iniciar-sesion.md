# Fase 1 · Iteración 1 — `IniciarSesion`

> **BC afectado:** Ejecución de Sesión  
> **Aggregate:** `Sesion`  
> **Métodos de dominio:** `Sesion.AbrirParaRegistro()` + `Sesion.Iniciar()`  
> **Fuentes de verdad:** `umbral-backend-spec.md §2.4`, `umbral-quality-spec.md §4.1`  
> **Estado:** implementado y testeado ✓

---

## 1. Descripción del caso de uso

El operador abre la sesión para registro de equipos y luego la inicia.
La sesión parte de `Programada`, avanza a `EnPreparacion` (donde los equipos
se registran) y finalmente pasa a `Activa`.

**Flujo real del operador:**

```
Crear sesión → AbrirParaRegistro() → RegistrarEquipo(s) → Iniciar()
[Programada]    [EnPreparacion]       [EnPreparacion]       [Activa]
```

---

## 2. Máquina de estados (fragmento relevante)

```
 ┌───────────┐  AbrirParaRegistro()  ┌───────────────┐  Iniciar()  ┌────────┐
 │ Programada│ ─────────────────────► │ EnPreparacion │ ───────────► Activa  │
 └───────────┘                        └───────────────┘             └────────┘
                                           [R1: equipos > 0]
                                           [R2: estado == EnPreparacion]
```

Cualquier otro estado al intentar `Iniciar()` → `DomainException`.

---

## 3. Reglas de negocio cubiertas

| ID | Regla | Consecuencia |
|----|-------|-------------|
| **R1** | Una sesión no puede iniciarse sin al menos un equipo registrado | `DomainException("...al menos un equipo registrado.")` |
| **R2** | `Iniciar()` solo es válido desde `EnPreparacion` | `DomainException("No se puede iniciar una sesión en estado '...'.")` |

---

## 4. Objetos de dominio creados en esta iteración

### Cimientos `Shared/`

| Archivo | Tipo | Responsabilidad |
|---------|------|-----------------|
| `IDomainEvent.cs` | Interface | Contrato de todos los eventos de dominio |
| `Entity<TId>` | Abstract class | Base con identidad tipada y equality por Id |
| `AggregateRoot<TId>` | Abstract class | Hereda `Entity`, gestiona `DomainEvents` |
| `ValueObject` | Abstract class | Equality estructural por componentes |
| `DomainException` | Sealed class | Excepción de regla de negocio (sin deps externas) |

### BC `Sesion/`

| Archivo | Tipo | Notas |
|---------|------|-------|
| `SesionId` | Typed ID | Wraps `Guid`; rechaza `Guid.Empty` |
| `EquipoSesionId` | Typed ID | Igual que `SesionId` |
| `EstadoSesion` | Enum | `Programada=1, EnPreparacion=2, Activa=3, Pausada=4, Finalizada=5, Cancelada=6` |
| `TipoSesion` | Enum | `BusquedaTesoro=1, Trivia=2` |
| `EquipoSesion` | Entity | Mínimo: `Id + Nombre` validado. Se expande en Iter 2 |
| `Events/SesionIniciada` | Domain Event (record) | `SesionId + TipoSesion + OcurridoEn` |
| `Sesion` | AggregateRoot | `Crear()`, `AbrirParaRegistro()`, `RegistrarEquipo()` stub, `Iniciar()` |

---

## 5. Flujo dentro del agregado

```
Sesion.AbrirParaRegistro()
│
└─► Guard: Estado != Programada → DomainException
└─► _estado = EnPreparacion

Sesion.Iniciar()
│
├─► Guard R2: Estado != EnPreparacion → DomainException
├─► Guard R1: _equipos.Count == 0    → DomainException
│
├─► _estado     = EstadoSesion.Activa
├─► _iniciadaEn = DateTime.UtcNow
│
└─► AddDomainEvent(new SesionIniciada(Id, TipoSesion, IniciadaEn))
```

El Domain Event es **transitorio**: vive en memoria durante el ciclo del agregado.
El handler de Application (Fase 2) lo leerá con `sesion.DomainEvents` y lo
publicará vía `IEventPublisher`.

---

## 6. Tests unitarios

**Archivo:** `tests/Umbral.Domain.Tests/Sesion/SesionIniciarTests.cs`

| Test | Tipo | Verifica |
|------|------|----------|
| `Iniciar_CuandoEstaEnPreparacionConEquipo_CambiaEstadoAActiva` | Happy path | Flujo completo exitoso |
| `Iniciar_CuandoEstaEnPreparacion_RegistraTimestampDeInicio` | Happy path | `IniciadaEn ≈ UtcNow` |
| `Iniciar_CuandoEstaEnPreparacion_EmiteSesionIniciada` | Happy path | Evento con `SesionId + TipoSesion` correcto |
| `Iniciar_CuandoHayVariosEquipos_CambiaEstadoAActiva` | Happy path | N equipos → OK |
| `Iniciar_SinEquiposRegistrados_LanzaDomainException` | Error R1 | Mensaje contiene "al menos un equipo" |
| `Iniciar_SinEquiposRegistrados_NoEmiteNingunEvento` | Error R1 | Sin efecto secundario |
| `Iniciar_CuandoEstadoNoEsEnPreparacion (Theory ×5)` | Error R2 | Programada, Activa, Pausada, Finalizada, Cancelada |
| `AbrirParaRegistro_DesdeProgramada_CambiaEstadoAEnPreparacion` | Happy path | Transición previa a Iniciar |
| `AbrirParaRegistro_CuandoNoEstaProgramada (Theory ×4)` | Error estado | EnPreparacion, Activa, Finalizada, Cancelada |

**Resultado:** `16 / 16 Passed`

---

## 7. Patrones aplicados

| Patrón | Aplicación |
|--------|-----------|
| **State** | Estado privado con setter `private`. Cada transición es un método explícito con guards. |
| **Factory Method** | `Sesion.Crear(id, tipo)` — único punto de creación. |
| **Domain Events** | `SesionIniciada` como `record` inmutable, transitorio en `AggregateRoot`. |
| **Typed IDs** | `SesionId`, `EquipoSesionId` previenen confundir IDs en compilación. |

---

## 8. Decisiones de diseño

**¿Por qué `Iniciar()` sin parámetro `DateTime`?**  
Sigue el backend spec: usa `DateTime.UtcNow` internamente. En Application (Fase 2)
se puede inyectar `IDateTimeProvider` si se necesita mayor testabilidad. El quality
spec acepta esta approach usando `BeCloseTo(DateTime.UtcNow, ...)`.

**¿Por qué `Sesion.Crear()` en lugar de `CrearBusquedaTesoro()` del spec?**  
La factory completa requiere `MisionSnapshot` y `OperadorId`, que pertenecen
a Fases 2–3 (Application + Infrastructure). El factory simplificado es
suficiente para cubrir el comportamiento de Domain en Fase 1.

**¿Por qué `AbrirParaRegistro()` si el spec no lo nombra explícitamente?**  
La máquina de estados del product spec (`Programada → EnPreparacion → Activa`)
implica una transición. `AbrirParaRegistro()` la hace explícita con su guard,
en lugar de saltar directamente de `Programada` a `EnPreparacion` sin control.

---

## 9. Qué queda pendiente (Iteración 2)

- `Sesion.RegistrarEquipo()` con reglas completas del backend spec:
  - Nombre único por sesión → `DomainException`
  - Rechazo si estado es `Finalizada` o `Cancelada` → `DomainException`
- `EquipoSesion` con `Puntaje` (Value Object) y `CodigoAcceso`
- Domain Event `EquipoRegistrado` (si aplica según spec)
- Tests: nombre duplicado, sesión terminal, happy path con `CodigoAcceso`
