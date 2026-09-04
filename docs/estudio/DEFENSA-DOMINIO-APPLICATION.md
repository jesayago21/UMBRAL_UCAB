# Estudio Domain + Application (más allá de evidencia)

Complemento de `DEFENSA-BACKEND-PREGUNTAS.md` (enfocado en SubmitEvidencia).  
Aquí: qué pueden preguntar de **Dominio** y **Application** en general.

**Siguiente bloque (Infrastructure + API):**  
[`DEFENSA-INFRASTRUCTURE-API.md`](DEFENSA-INFRASTRUCTURE-API.md)

---

## 1. Bounded Contexts (Dominio)

Tres contextos en el mismo monolito (no microservicios):

| BC | Agregados / piezas | Para qué |
|----|--------------------|----------|
| **Catálogo de Misiones** | `Mision`, `Etapa`, `Pista` | Plantillas BT (+ etapas trivia en misión unificada) |
| **Catálogo de Trivia** | `Pregunta`, `Categoria`, `OpcionRespuesta` | Banco de preguntas |
| **Ejecución de Sesión** | `Sesion`, `ContextoMision`, participantes, evidencias, respuestas trivia | Partida en vivo |

**Frase:**  
> “Una sesión es Búsqueda del Tesoro o Trivia según sus etapas; no mezclamos dos contextos de juego distintos en la misma lógica a lo loco: el snapshot de la misión define qué etapas hay.”

---

## 2. Agregado raíz (Aggregate Root)

**Qué es:** entidad que protege invariantes y es la puerta de entrada.  
No se modifica el interior “desde afuera” con setters; se llama a métodos.

**En UMBRAL:**
- `Mision` — catálogo
- `Sesion` — partida
- `Pregunta` / `Categoria` — trivia
- `UsuarioAdministrable` — identidad espejo

**Cómo se crea:** constructor privado + `static Crear(...)` / `CrearDesdeMision(...)`.

**Frase:**  
> “El handler no pone propiedades a mano. Carga el agregado y llama métodos como `Iniciar()`, `UnirseParticipante()`, `RegistrarEvidencia()`.”

---

## 3. Ciclo de vida de sesión (State)

Estados típicos (enum `EstadoSesion`):  
Programada / EnPreparacion → Activa → Pausada → Finalizada / Cancelada (según transiciones del código).

Métodos en `Sesion.cs`:
- `AbrirParaRegistro()`
- `UnirseParticipante(...)`
- `Iniciar()`, `Pausar()`, `Reanudar()`, `Finalizar()`, `Cancelar(...)`

**Pregunta típica:** “¿Se puede iniciar sin participantes?”  
→ Responder mirando la regla en `Iniciar()` (el dominio lo decide; no el controller).

**Frase:**  
> “El patrón State está en las transiciones del agregado Sesion: solo ciertos cambios de estado son válidos.”

---

## 4. Snapshot vs entidad de catálogo

| Catálogo (`EtapaBusquedaTesoro`) | Snapshot (`EtapaBusquedaTesoroSnapshot`) |
|----------------------------------|------------------------------------------|
| Editable por admin | Inmutable |
| Vive en `Mision` | Vive dentro de `ContextoMision` / sesión |
| Plantilla | Foto al crear la sesión |

`Sesion.CrearDesdeMision(MisionSnapshot ...)` congela la misión.

---

## 5. `ContextoMision` (Facade de progreso)

Dentro de la sesión guarda:
- el `MisionSnapshot`
- índice de etapa actual
- datos de trivia en curso (timer, fase, etc.)
- progreso relacionado a pistas / ganadores de etapa

**Frase:**  
> “ContextoMision coordina en qué etapa estamos y qué datos de juego aplican, sin que el handler conozca todo el detalle.”

---

## 6. Value Objects e IDs tipados

Ejemplos: `SesionId`, `MisionId`, `UsuarioId`, `CodigoQR`, `CodigoAcceso`.

El handler recibe `Guid`/`string` del Command y arma VOs: `new SesionId(command.SesionId)`.

**Por qué:** evita confundir un Guid de sesión con uno de usuario.

---

## 7. Domain Events

El agregado hace `RaiseDomainEvent(...)`.  
El handler, después de guardar, puede publicar / notificar (SignalR vía `INotificacionRealTime`).

Ejemplos de eventos: pistas liberadas, etapa completada, sesión iniciada, etc.

**Frase:**  
> “Los eventos nacen en el dominio; Application/Infrastructure los despachan. El hub SignalR no tiene reglas de negocio.”

---

## 8. Excepciones de dominio vs validación de entrada

| | Entrada (Application) | Negocio (Domain) |
|--|----------------------|------------------|
| Clase | `ValidationException` (FluentValidation) | `DomainException` |
| Cuándo | Request mal formado | Invariante rota |
| Quién | `ValidationBehavior` | Agregado / domain service |
| HTTP | 400 ValidationError | 400 DomainError |

`NotFoundException` (Application) → 404 si no existe el agregado.

---

## 9. Application: CQRS + MediatR

- **Commands** → cambian estado → suelen devolver `Result<T>`
- **Queries** → solo leen → devuelven `*Dto`
- **Handlers** `internal sealed` + `IRequestHandler<...>`
- **Validators** por Command (FluentValidation)
- **Pipeline:** `ValidationBehavior` registrado en DI

**Result&lt;T&gt;:** éxito o fallo de negocio **sin** tirar siempre excepción (el handler puede devolver Fail; excepciones de dominio también existen).

---

## 10. Flujos que sí te pueden preguntar (además de evidencia)

### A) Unirse a sesión
1. Command `UnirseSesionCommand` (sesión, código, jugador, nombre)
2. Validator: campos no vacíos
3. Handler: carga `Sesion` → `UnirseParticipante(...)` → Save
4. Dominio valida código de acceso y estado (inscripción abierta)

### B) Iniciar sesión (operador)
1. `IniciarSesionCommand`
2. Handler → `sesion.Iniciar()`
3. Dominio: solo si el estado y reglas lo permiten
4. Eventos / SignalR de cambio de estado

### C) Crear misión (admin)
1. `CrearMisionCommand` + Validator
2. Handler: `Mision.Crear(...)` + etapas → repo
3. Dominio valida descripciones, QR, categorías trivia, etc.

### D) Trivia en vivo (operador / jugador)
1. Operador: `LanzarPreguntaTrivia` / ciclo automático
2. Jugador: `SubmitRespuestaTrivia` → cola MassTransit → `ProcesarRespuestaTrivia`
3. Dominio: fase trivia, tiempo, puntaje (`CalculoPuntajeTriviaService`)

### E) Consulta operador
1. `GetSesionOperadorQuery`
2. Handler: Find → `ToDetalle()` → DTO  
   (sin modificar estado)

---

## 11. Qué NO va en Application handlers

- Lógica de “¿este QR es el correcto?” → Dominio  
- SQL / `DbContext` directo → Infrastructure (repositorio)  
- `HttpContext` / status codes → API  
- SignalR hub con reglas → solo notificación después del dominio  

**Handler sí puede:** cargar, llamar dominio, guardar, mapear DTO, publicar notificaciones.

---

## 12. Puertos (interfaces) vs adaptadores

En Domain/Application:
- `ISesionRepository`
- `INotificacionRealTime`
- `IEventPublisher`
- `IIdentityService`

En Infrastructure: implementaciones EF, SignalR, Keycloak, MassTransit.

**Frase:**  
> “Application depende de interfaces; Infrastructure las implementa. Por eso es hexagonal.”

---

## 13. Patrones (versión oral corta)

| Patrón | Dónde decirlo |
|--------|----------------|
| **State** | Transiciones `EstadoSesion` en `Sesion` |
| **Aggregate Root** | `Sesion`, `Mision` |
| **Composite** | Misión → Etapas → Pistas |
| **Strategy** | Cálculo de puntaje BT / trivia (services de cálculo) |
| **Facade** | `ContextoMision` o controllers delgados vía MediatR |
| **CQRS** | Commands vs Queries |
| **Template / CoR** | Validación de evidencias / cadena de chequeos (si preguntan, señalar `ValidacionEvidenciaService`) |

Detalle largo: `docs/GUIA-DEFENSA-ARQUITECTURA-Y-TESTS.md`.

---

## 14. Preguntas de estudio (Domain / Application)

### D1
¿Por qué `Mision` y `Sesion` son agregados distintos?

**R:** Catálogo editable vs ejecución de una partida. Sesión lleva snapshot; no muta el catálogo al jugar.

---

### D2
¿Quién decide si se puede `Pausar()` una sesión?

**R:** El dominio (`Sesion.Pausar`), no el controller.

---

### D3
Un Query handler, ¿debe llamar `sesion.Iniciar()`?

**R:** No. Queries solo leen.

---

### D4
¿Dónde vive `ISesionRepository`?

**R:** Puerto en Domain (o Application según el puerto); implementación en Infrastructure.

---

### D5
Diferencia `DomainException` vs `ValidationException`.

**R:** Negocio vs formato/entrada. Ambas pueden ser 400, pero distinta capa y origen.

---

### D6
¿Qué hace `CrearDesdeMision`?

**R:** Crea la sesión a partir de un `MisionSnapshot` (misión congelada + operador).

---

### D7
En Application, ¿el handler contiene las reglas de puntaje?

**R:** No. Delega a domain services / métodos del agregado (`CalculoPuntaje...`).

---

### D8
¿Por qué handlers `internal sealed`?

**R:** Encapsulación del caso de uso; se prueban vía InternalsVisibleTo / MediatR, no como API pública.

---

### D9
Flujo mínimo “abrir inscripción”:

**R:** Command → Validator → Handler → `sesion.AbrirParaRegistro()` → Save → (opcional) notificar.

---

### D10
Si el admin cambia una etapa de la misión mientras hay sesión Activa, ¿cambia el juego?

**R:** No debería: la sesión usa el snapshot creado al inicio.

---

### D11
`ListMisionesQuery` vs `CrearMisionCommand`: ¿cuál escribe?

**R:** Solo el Command.

---

### D12
¿MediatR reemplaza al dominio?

**R:** No. Solo enruta el caso de uso al handler. El dominio sigue teniendo las reglas.

---

## 15. Mini simulacro oral (mezcla)

Responde en voz alta (30 s cada una):

1. Explica las 4 capas backend y la regla de dependencias.  
2. Diferencia Command y Query con un ejemplo cada uno.  
3. ¿Qué es un Aggregate Root en Sesion?  
4. Validator vs DomainException con un ejemplo.  
5. ¿Para qué el snapshot?  
6. Nombra 3 métodos de `Sesion` que no sean evidencia.  
7. ¿Dónde está la implementación de `ISesionRepository`?  
8. ¿Qué hace `ValidationBehavior`?  

**Claves rápidas:**  
1 API→App→Domain←Infra · 2 UnirseSesion / GetSesionOperador · 3 raíz que controla participantes, estado, evidencias · 4 NotEmpty vs sesión no Activa · 5 congelar misión · 6 Iniciar, Pausar, UnirseParticipante · 7 Infrastructure · 8 valida Commands antes del handler  

---

## 16. Archivos para recorrer en 15 minutos

| Tema | Abrir |
|------|--------|
| Agregado sesión | `Domain/Sesion/Sesion.cs` (métodos públicos) |
| Estado | `Domain/Sesion/EstadoSesion.cs` |
| Contexto | `Domain/Sesion/ContextoMision.cs` |
| Validación QR | `Domain/Sesion/Validacion/ValidacionEvidenciaService.cs` |
| Misión catálogo | `Domain/CatalogoMision/Mision/Mision.cs` |
| Unirse | `Application/.../UnirseSesion/*` |
| Iniciar | `Application/.../IniciarSesion/*` |
| Query | `Application/.../GetSesionOperador/*` |
| Pipeline | `Application/Common/Behaviors/ValidationBehavior.cs` |
| Result | `Application/Common/Models/Result.cs` (o similar) |

---

## 17. Frases de cierre (Domain + Application)

> “Application orquesta casos de uso con MediatR; Domain concentra reglas e invariantes en agregados.”

> “Commands cambian; Queries leen DTOs. Nunca devolvemos entidades de dominio por HTTP.”

> “Los puertos viven adentro; Infrastructure adapta EF, Keycloak, SignalR y colas.”
