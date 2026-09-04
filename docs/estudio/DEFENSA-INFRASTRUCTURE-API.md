# Estudio Infrastructure + API (defensa)

Complemento de:
- [`DEFENSA-BACKEND-PREGUNTAS.md`](DEFENSA-BACKEND-PREGUNTAS.md) — evidencia / FluentValidation
- [`DEFENSA-DOMINIO-APPLICATION.md`](DEFENSA-DOMINIO-APPLICATION.md) — Domain + Application

Aquí: **adaptadores** (EF, SignalR, RabbitMQ, Keycloak) y **borde HTTP** (controllers, middleware, JWT, Gateway).

---

## 1. Dónde estás en el hexágono

```
                    ┌─────────────┐
   Web / Mobile ───▶│ Umbral.API  │  Controllers, JWT, middleware
                    └──────┬──────┘
                           │ MediatR
                    ┌──────▼──────┐
                    │ Application │  Handlers (orquestan)
                    └──────┬──────┘
                           │ puertos (interfaces)
              ┌────────────┼────────────┐
              ▼            ▼            ▼
           Domain     Infrastructure   (mismo proceso)
         (núcleo)     EF / SignalR / RabbitMQ / Keycloak
```

**Frase:**  
> “API e Infrastructure son adaptadores. Domain no los conoce. Application solo ve interfaces.”

---

## 2. Qué registra Infrastructure

Archivo: `InfrastructureServiceCollectionExtensions.cs`

| Registro | Puerto | Implementación real |
|----------|--------|---------------------|
| EF + Postgres | — | `UmbralDbContext` |
| Sesiones / misiones / trivia | `ISesionRepository`, etc. | `*Repository` |
| Eventos de dominio (cola genérica) | `IEventPublisher` | **`NoOpEventPublisher`** (E1) |
| Tiempo real | `INotificacionRealTime` | `NotificacionRealTimeService` (SignalR) |
| Mensajería trivia | `IRespuestaTriviaBus` | MassTransit → RabbitMQ |
| Identidad | `IIdentityService` | `KeycloakIdentityService` (HttpClient) |
| Background | — | pistas por tiempo + ciclo trivia |

**Frase clave sobre RabbitMQ y evidencia:**  
> “Al enviar evidencia el handler llama `IEventPublisher`, pero hoy es NoOp: no hay round-trip a RabbitMQ. Lo que sí es vivo es SignalR vía `INotificacionRealTime`. MassTransit real está en respuestas trivia.”

---

## 3. Repositorios EF (sin lógica de negocio)

- Interfaces en Domain (puertos).
- Implementación en Infrastructure: carga/guarda el agregado.
- Mappings: `IEntityTypeConfiguration<T>` (enums como string, VOs con converters).
- Domain events del agregado: **no se persisten como entidades** (Ignore en EF); se despachan después.

**Pregunta trampa:** “¿El handler usa `DbContext`?”  
**R:** No. Usa `ISesionRepository`. El `DbContext` vive en Infrastructure.

---

## 4. SignalR (tiempo real)

### Hub (`SesionHub`)
- Solo une conexiones a **grupos** (`sesion-{id}`, `equipo-{participanteId}`, `operador-{id}`).
- **Sin reglas de negocio** (no valida QR, no cambia estado).

### Notificación
Handler → `INotificacionRealTime` → `NotificacionRealTimeService` → `Clients.Group(...).Metodo(...)`.

**Ejemplos de eventos:** ranking, estado sesión, pista liberada, penalización (grupo `equipo-*`), trivia.

**Frase:**  
> “El hub es el canal. El dominio decide; Application notifica; Infrastructure empuja por WebSocket.”

**Gateway:** REST puede ir por `:8000` (YARP). SignalR suele ir **directo a API `:5000`** (hubs no pasan bien por el proxy en E1).

---

## 5. RabbitMQ + MassTransit (qué sí / qué no)

| Pieza | Estado E1 |
|-------|-----------|
| Contenedor RabbitMQ en Docker | Sí (infra) |
| `IEventPublisher` genérico | **NoOp** (no publica dominio a cola) |
| `IRespuestaTriviaBus` + consumer `ProcesarRespuestaTriviaConsumer` | **Sí** (MassTransit + cola `umbral.respuestas-trivia`) |
| Tests | InMemory MassTransit (`Testing`) |

**Por qué en monolito local se siente “instantáneo”:**  
Publicar/consumir en la misma máquina (o NoOp) no añade latencia visible. El valor de la cola es **desacoplar** (reintentos, no bloquear HTTP, escalar consumidores), no “hacer más lento el demo”.

**Frase defensa:**  
> “Diseñamos el puerto de eventos desde E1. El bus genérico queda NoOp; la cola real arranca con trivia. RabbitMQ está levantado y MassTransit cableado para ese camino.”

---

## 6. API: controllers delgados

Patrón:

```
HTTP → Request contract → Command/Query → _sender.Send → Result/DTO → HTTP
```

- No ponen reglas de juego.
- Traducen claims JWT → ids (jugador/operador).
- En Development/Testing: `TestAuthHandler` permite probar sin Keycloak.

**Result vs excepción:**

| Camino | Cómo llega a HTTP |
|--------|-------------------|
| `Result.Failure` (negocio esperado) | `ResultExtensions` → 400 |
| `ValidationException` | Middleware → 400 `ValidationError` |
| `DomainException` | Middleware → 400 `DomainError` |
| `NotFoundException` | Middleware → 404 |
| No controlada | Middleware → 500 |

**Frase:**  
> “Hay dos estilos: Result para fallos de caso de uso controlados, y excepciones para validación / dominio / no encontrado que el middleware unifica.”

---

## 7. Autenticación / autorización

- Producción: JWT Bearer Keycloak (realm `umbral`).
- Roles: `Administrador`, `Operador`, `Participante` (alias de `EquipoParticipante`).
- Policies en controllers / hubs `[Authorize]`.

**Frase:**  
> “La API no inventa usuarios: valida tokens Keycloak. El espejo `UsuarioAdministrable` es para administración local sincronizada.”

---

## 8. Gateway (YARP)

| Puerto | Rol |
|--------|-----|
| 8000 | `Umbral.Gateway` — edge REST `/api/v1/**` |
| 5000 | `Umbral.API` — monolito (REST + SignalR) |

No es microservicio: es **proxy de borde** frente al mismo monolito.

---

## 9. Background services

- `PistasPorTiempoBackgroundService` — libera pistas con timer.
- `TriviaCicloBackgroundService` — avanza fases trivia.

Disparan Commands MediatR; la regla sigue en dominio.

---

## 10. Preguntas tipo oral (Infra / API)

### I1
¿Por qué Infrastructure no es referenciada por Domain?

**R:** Regla hexagonal: dependencias hacia adentro. Domain no puede acoplarse a EF/SignalR.

---

### I2
Al escanear un QR, ¿pasa por RabbitMQ?

**R:** Hoy no de forma real: `IEventPublisher` es NoOp. Sí hay persistencia + SignalR.

---

### I3
¿Dónde está la lógica de “QR inválido”?

**R:** Dominio (`RegistrarEvidencia` / validación de evidencia). No en el Hub ni en el Controller.

---

### I4
¿Qué hace `UnirseASesion` en el Hub?

**R:** Solo agregar la conexión a grupos SignalR. No inscribe al participante en la sesión (eso es HTTP `UnirseSesionCommand`).

---

### I5
Diferencia `IEventPublisher` vs `INotificacionRealTime`.

**R:** Eventos de dominio / integración async (cola) vs push en vivo a clientes (SignalR). Hoy el primero es NoOp; el segundo está activo.

---

### I6
¿Quién mapea `ValidationException` a 400?

**R:** `ExceptionHandlingMiddleware` en API, no el Validator ni el Handler.

---

### I7
¿El Gateway contiene handlers MediatR?

**R:** No. Solo YARP. Sin Domain/Application.

---

### I8
¿Para qué sirve el consumer de trivia?

**R:** Procesar `RespuestaTriviaRecibida` de forma asíncrona (desacoplar submit del procesamiento pesado / reintentos).

---

### I9
En Testing, ¿MassTransit usa RabbitMQ real?

**R:** No: `UsingInMemory` cuando el environment es `Testing`.

---

### I10
¿Por qué SignalR no va (o no debe ir) solo por el Gateway en E1?

**R:** Hubs WebSocket: en la práctica el cliente apunta a API `:5000`; el Gateway cubre REST.

---

## 11. Mini simulacro (45–60 s cada uno)

1. “Explica el camino de un POST evidencia desde el mobile hasta que el operador ve ranking.”  
2. “¿Está RabbitMQ ‘falso’ en el proyecto?”  
3. “Diferencia Hub SignalR vs Controller HTTP para unirse a sesión.”  
4. “Dibuja las 4 capas y marca dónde vive el `DbContext`.”

**Pistas de respuesta:**  
1) HTTP → Command → validator → handler → `RegistrarEvidencia` → Save → notificar SignalR ranking.  
2) Broker real en Docker; publicador genérico NoOp; trivia sí usa MassTransit.  
3) HTTP = caso de uso / agregado; Hub = suscripción a grupos de eventos.  
4) DbContext solo Infrastructure.

---

## 12. Archivos para recorrer en 20 minutos

| Tema | Abrir |
|------|--------|
| DI Infra | `InfrastructureServiceCollectionExtensions.cs` |
| NoOp eventos | `NoOpEventPublisher.cs` |
| MassTransit | `MessagingServiceExtensions.cs` |
| Hub | `SesionHub.cs` |
| Notificación | `NotificacionRealTimeService.cs` |
| Middleware | `ExceptionHandlingMiddleware.cs` |
| Result HTTP | `ResultExtensions.cs` |
| Auth | `ApiServiceCollectionExtensions.cs` |
| Evidencia (repaso) | `SubmitEvidenciaCommandHandler.cs` |

---

## 13. Frases de cierre (Infra + API)

> “Infrastructure adapta el mundo exterior: Postgres, Keycloak, SignalR y RabbitMQ.”

> “La API es delgada: MediatR + traducción HTTP. El middleware unifica errores.”

> “Tiempo real = SignalR activo. Cola genérica de dominio = puerto listo, NoOp en E1; trivia ya usa MassTransit.”
