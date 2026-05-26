# Skill: WebSocket + SignalR — Proyecto UMBRAL

## Propósito
Guía canónica para implementar comunicación en tiempo real con ASP.NET Core
SignalR en UMBRAL. Cubre los **dos hubs** del sistema: `SesionHub` (eventos
comunes + BusquedaTesoro) y `TriviaHub` (eventos exclusivos de Trivia),
grupos por sesión/equipo/operador, clientes TypeScript (web) y React Native
(mobile), reconexión automática y autenticación.

> **Fuente de verdad:** `.cursor/specs/umbral-product-spec.md` y
> `.cursor/rules/project-rules.md`. Una sesión es **siempre** de un único tipo.

---

## 1. Dos hubs — uno por dominio de eventos

```
Backend
  │
  ├── /hubs/sesion  → SesionHub
  │     Eventos comunes a ambos modos + exclusivos BusquedaTesoro:
  │     - sesion:estado-cambiado        (ambos)
  │     - sesion:etapa-avanzada         (BT)
  │     - sesion:pista-liberada         (BT)
  │     - sesion:ranking-actualizado    (ambos)
  │     - sesion:penalizacion           (ambos)
  │
  └── /hubs/trivia  → TriviaHub
        Eventos exclusivos de Trivia:
        - trivia:pregunta-lanzada
        - trivia:tiempo-agotado
        - trivia:resultado-ronda
```

**Regla:** Una sesión de Trivia se conecta a **ambos** hubs (el sesion para
ranking/estado y el trivia para preguntas). Una sesión de BusquedaTesoro
solo se conecta al hub `sesion`.

---

## 2. Grupos SignalR

| Grupo | Participantes | Hub |
|-------|---------------|-----|
| `sesion-{sesionId}` | Admin, Operador, todos los Equipos | SesionHub |
| `equipo-{equipoId}` | Solo el equipo (pistas privadas) | SesionHub |
| `operador-{sesionId}` | Admin + Operador | SesionHub |
| `trivia-{sesionId}` | Todos en la sesión Trivia | TriviaHub |

---

## 3. Estructura de carpetas

```
src/Umbral.Infrastructure/
└── RealTime/
    ├── Hubs/
    │   ├── SesionHub.cs               ← Hub sesión + BT
    │   ├── ISesionHubClient.cs        ← interfaz tipada del cliente
    │   ├── TriviaHub.cs               ← Hub exclusivo Trivia
    │   └── ITriviaHubClient.cs        ← interfaz tipada del cliente
    └── NotificacionRealTimeService.cs ← implementación de INotificacionRealTime

src/Umbral.Domain/
└── Ports/
    └── INotificacionRealTime.cs       ← puerto de salida (en Domain)
```

> Los Hubs viven en `Infrastructure/RealTime/Hubs/` y se mapean desde `Program.cs` en la API.

---

## 4. Interfaces tipadas de cliente

```csharp
// Infrastructure/RealTime/Hubs/ISesionHubClient.cs
namespace Umbral.Infrastructure.RealTime.Hubs;

/// <summary>
/// Métodos que el SERVIDOR invoca en los CLIENTES conectados al SesionHub.
/// Nombres en PascalCase — deben coincidir exactamente con los listeners del cliente.
/// </summary>
public interface ISesionHubClient
{
    // ── Ciclo de vida (ambos modos) ───────────────────────────
    Task SesionEstadoCambiado(SesionEstadoPayload payload);
    Task RankingActualizado(RankingActualizadoPayload payload);
    Task PenalizacionAplicada(PenalizacionPayload payload);

    // ── BusquedaTesoro ────────────────────────────────────────
    Task EtapaAvanzada(EtapaAvanzadaPayload payload);
    Task PistaLiberada(PistaLiberadaPayload payload);   // puede ser privada (1 equipo) o global
}

// Infrastructure/RealTime/Hubs/ITriviaHubClient.cs
public interface ITriviaHubClient
{
    // ── Trivia ────────────────────────────────────────────────
    Task PreguntaLanzada(PreguntaLanzadaPayload payload);
    Task TiempoAgotado(TiempoAgotadoPayload payload);
    Task ResultadoRonda(ResultadoRondaPayload payload);
}

// ── Payloads ─────────────────────────────────────────────────
public sealed record SesionEstadoPayload(Guid SesionId, string NuevoEstado);
public sealed record RankingActualizadoPayload(Guid SesionId, IReadOnlyList<PosicionDto> Ranking);
public sealed record PosicionDto(Guid EquipoId, string NombreEquipo, int Puntaje, int Posicion);
public sealed record PenalizacionPayload(Guid SesionId, Guid EquipoId, int Puntos, string Motivo);
public sealed record EtapaAvanzadaPayload(Guid SesionId, int EtapaIndex);
public sealed record PistaLiberadaPayload(Guid SesionId, Guid EquipoId, Guid PistaId, string Contenido);
public sealed record PreguntaLanzadaPayload(Guid SesionId, Guid PreguntaId, string Enunciado, IReadOnlyList<string> Opciones, int TimerMs);
public sealed record TiempoAgotadoPayload(Guid SesionId, Guid PreguntaId);
public sealed record ResultadoRondaPayload(Guid SesionId, Guid RespuestaCorrectaId, IReadOnlyList<PosicionDto> Ranking);
```

---

## 5. SesionHub

```csharp
// Infrastructure/RealTime/Hubs/SesionHub.cs
namespace Umbral.Infrastructure.RealTime.Hubs;

[Authorize]
public sealed class SesionHub : Hub<ISesionHubClient>
{
    private readonly ISender _sender;
    private readonly ILogger<SesionHub> _logger;

    public SesionHub(ISender sender, ILogger<SesionHub> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Cliente conectado: {ConnId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Cliente desconectado: {ConnId} | {Error}",
            Context.ConnectionId, exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Equipo se une al grupo de la sesión y al grupo de su equipo.</summary>
    public async Task UnirseASesion(Guid sesionId, Guid equipoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"equipo-{equipoId}");
        _logger.LogInformation("Equipo {EquipoId} unido a sesion {SesionId}", equipoId, sesionId);
    }

    /// <summary>Operador/Admin se une al grupo de sesión y al grupo de operadores.</summary>
    public async Task UnirseComoOperador(Guid sesionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"operador-{sesionId}");
    }
}
```

---

## 6. TriviaHub

```csharp
// Infrastructure/RealTime/Hubs/TriviaHub.cs
namespace Umbral.Infrastructure.RealTime.Hubs;

[Authorize]
public sealed class TriviaHub : Hub<ITriviaHubClient>
{
    private readonly ISender _sender;
    private readonly ILogger<TriviaHub> _logger;

    public TriviaHub(ISender sender, ILogger<TriviaHub> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>Equipo/Operador se une al grupo Trivia de la sesión.</summary>
    public async Task UnirseATriviaSession(Guid sesionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"trivia-{sesionId}");
        _logger.LogInformation(
            "Cliente {ConnId} unido a trivia-{SesionId}", Context.ConnectionId, sesionId);
    }

    /// <summary>
    /// El Equipo envía su respuesta a la pregunta activa.
    /// El Hub delega al Command Handler — no contiene lógica de negocio.
    /// </summary>
    public async Task ResponderPregunta(ResponderPreguntaHubRequest request)
    {
        try
        {
            await _sender.Send(new RegistrarRespuestaTriviaCommand(
                request.SesionId,
                request.EquipoId,
                request.PreguntaId,
                request.OpcionSeleccionadaId,
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            await Clients.Caller.PreguntaLanzada(null!);  // se reemplaza por ErrorRecibido si se agrega al interfaz
            _logger.LogWarning("Error de dominio al responder pregunta: {Msg}", ex.Message);
        }
    }
}

public sealed record ResponderPreguntaHubRequest(
    Guid SesionId,
    Guid EquipoId,
    Guid PreguntaId,
    Guid OpcionSeleccionadaId);
```

---

## 7. Puerto de salida — INotificacionRealTime

```csharp
// Domain/Ports/INotificacionRealTime.cs
namespace Umbral.Domain.Ports;

/// <summary>
/// Puerto de salida para notificaciones en tiempo real via SignalR.
/// La capa de Application/Domain no conoce SignalR directamente.
/// </summary>
public interface INotificacionRealTime
{
    // ── Ambos modos ────────────────────────────────────────────
    Task NotificarEstadoSesionAsync(Guid sesionId, string nuevoEstado, CancellationToken ct = default);
    Task NotificarRankingAsync(Guid sesionId, IReadOnlyList<PosicionDto> ranking, CancellationToken ct = default);
    Task NotificarPenalizacionAsync(Guid sesionId, Guid equipoId, int puntos, string motivo, CancellationToken ct = default);

    // ── BusquedaTesoro ─────────────────────────────────────────
    Task NotificarEtapaAvanzadaAsync(Guid sesionId, int etapaIndex, CancellationToken ct = default);
    Task NotificarPistaLiberadaAsync(Guid sesionId, Guid? equipoId, Guid pistaId, string contenido, CancellationToken ct = default);
    // equipoId null = libera para todos; con valor = solo ese equipo

    // ── Trivia ─────────────────────────────────────────────────
    Task NotificarPreguntaLanzadaAsync(Guid sesionId, PreguntaLanzadaPayload payload, CancellationToken ct = default);
    Task NotificarTiempoAgotadoAsync(Guid sesionId, Guid preguntaId, CancellationToken ct = default);
    Task NotificarResultadoRondaAsync(Guid sesionId, ResultadoRondaPayload payload, CancellationToken ct = default);
}
```

---

## 8. Implementación con IHubContext

```csharp
// Infrastructure/RealTime/NotificacionRealTimeService.cs
namespace Umbral.Infrastructure.RealTime;

internal sealed class NotificacionRealTimeService : INotificacionRealTime
{
    private readonly IHubContext<SesionHub, ISesionHubClient> _sesionHub;
    private readonly IHubContext<TriviaHub, ITriviaHubClient> _triviaHub;

    public NotificacionRealTimeService(
        IHubContext<SesionHub, ISesionHubClient> sesionHub,
        IHubContext<TriviaHub, ITriviaHubClient> triviaHub)
    {
        _sesionHub = sesionHub;
        _triviaHub = triviaHub;
    }

    public Task NotificarEstadoSesionAsync(Guid sesionId, string nuevoEstado, CancellationToken ct)
        => _sesionHub.Clients
            .Group($"sesion-{sesionId}")
            .SesionEstadoCambiado(new SesionEstadoPayload(sesionId, nuevoEstado));

    public Task NotificarRankingAsync(Guid sesionId, IReadOnlyList<PosicionDto> ranking, CancellationToken ct)
        => _sesionHub.Clients
            .Group($"sesion-{sesionId}")
            .RankingActualizado(new RankingActualizadoPayload(sesionId, ranking));

    public Task NotificarEtapaAvanzadaAsync(Guid sesionId, int etapaIndex, CancellationToken ct)
        => _sesionHub.Clients
            .Group($"sesion-{sesionId}")
            .EtapaAvanzada(new EtapaAvanzadaPayload(sesionId, etapaIndex));

    public Task NotificarPistaLiberadaAsync(
        Guid sesionId, Guid? equipoId, Guid pistaId, string contenido, CancellationToken ct)
    {
        var payload = new PistaLiberadaPayload(sesionId, equipoId ?? Guid.Empty, pistaId, contenido);

        if (equipoId.HasValue)
            return _sesionHub.Clients
                .Group($"equipo-{equipoId.Value}")
                .PistaLiberada(payload);
        else
            return _sesionHub.Clients
                .Group($"sesion-{sesionId}")
                .PistaLiberada(payload);
    }

    public Task NotificarPreguntaLanzadaAsync(Guid sesionId, PreguntaLanzadaPayload payload, CancellationToken ct)
        => _triviaHub.Clients
            .Group($"trivia-{sesionId}")
            .PreguntaLanzada(payload);

    public Task NotificarTiempoAgotadoAsync(Guid sesionId, Guid preguntaId, CancellationToken ct)
        => _triviaHub.Clients
            .Group($"trivia-{sesionId}")
            .TiempoAgotado(new TiempoAgotadoPayload(sesionId, preguntaId));

    public Task NotificarResultadoRondaAsync(Guid sesionId, ResultadoRondaPayload payload, CancellationToken ct)
        => _triviaHub.Clients
            .Group($"trivia-{sesionId}")
            .ResultadoRonda(payload);

    public Task NotificarPenalizacionAsync(
        Guid sesionId, Guid equipoId, int puntos, string motivo, CancellationToken ct)
        => _sesionHub.Clients
            .Group($"sesion-{sesionId}")
            .PenalizacionAplicada(new PenalizacionPayload(sesionId, equipoId, puntos, motivo));
}
```

---

## 9. Uso desde Domain Event Handler (Application Layer)

```csharp
// Application/Sesion/EventHandlers/SesionIniciadaEventHandler.cs
internal sealed class SesionIniciadaEventHandler
    : INotificationHandler<SesionIniciada>
{
    private readonly INotificacionRealTime _notifier;  // ← puerto, no IHubContext
    private readonly IEventPublisher _eventPublisher;

    public SesionIniciadaEventHandler(
        INotificacionRealTime notifier,
        IEventPublisher eventPublisher)
    {
        _notifier = notifier;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(SesionIniciada notification, CancellationToken ct)
    {
        // A. Notificar estado via SignalR
        await _notifier.NotificarEstadoSesionAsync(
            notification.SesionId.Valor, "Activa", ct);

        // B. Publicar Integration Event en RabbitMQ
        await _eventPublisher.PublicarAsync(
            new SesionIniciadaIntegrationEvent
            {
                SesionId   = notification.SesionId.Valor,
                TipoSesion = notification.TipoSesion.ToString()
            }, ct);
    }
}
```

---

## 10. Registro de servicios (Program.cs)

```csharp
// Program.cs
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval     = TimeSpan.FromSeconds(15);
});

// INotificacionRealTime → implementación
builder.Services.AddScoped<INotificacionRealTime, NotificacionRealTimeService>();

var app = builder.Build();

// Mapear ambos hubs
app.MapHub<SesionHub>("/hubs/sesion");
app.MapHub<TriviaHub>("/hubs/trivia");
```

---

## 11. Cliente TypeScript — Web (Admin/Operador)

```typescript
// src/services/sesionHubService.ts
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export function crearSesionHub(token: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_API_URL + "/hubs/sesion", {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();
}

export function crearTriviaHub(token: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_API_URL + "/hubs/trivia", {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(LogLevel.Warning)
    .build();
}

// src/hooks/useSesionHub.ts
export function useSesionHub(sesionId: string, tipoSesion: "BusquedaTesoro" | "Trivia") {
  const token = useAuthStore((s) => s.token);
  const sesionConnRef = useRef<HubConnection | null>(null);
  const triviaConnRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (!token || !sesionId) return;

    // Siempre conectar al hub de sesión
    const sesionConn = crearSesionHub(token);
    sesionConnRef.current = sesionConn;

    sesionConn.on("SesionEstadoCambiado", (payload) => {
      useSesionStore.getState().setEstado(payload.nuevoEstado);
    });

    sesionConn.on("RankingActualizado", (payload) => {
      useSesionStore.getState().setRanking(payload.ranking);
    });

    sesionConn.on("EtapaAvanzada", (payload) => {
      useSesionStore.getState().setEtapaActual(payload.etapaIndex);
    });

    sesionConn.on("PistaLiberada", (payload) => {
      useSesionStore.getState().agregarPista(payload);
    });

    sesionConn.onreconnected(async () => {
      await sesionConn.invoke("UnirseComoOperador", sesionId);
    });

    sesionConn.start().then(() =>
      sesionConn.invoke("UnirseComoOperador", sesionId)
    );

    // Solo conectar al hub de trivia si el modo es Trivia
    if (tipoSesion === "Trivia") {
      const triviaConn = crearTriviaHub(token);
      triviaConnRef.current = triviaConn;

      triviaConn.on("PreguntaLanzada", (payload) => {
        useSesionStore.getState().setPreguntaActual(payload);
      });

      triviaConn.on("TiempoAgotado", () => {
        useSesionStore.getState().cerrarRespuesta();
      });

      triviaConn.on("ResultadoRonda", (payload) => {
        useSesionStore.getState().setResultadoRonda(payload);
      });

      triviaConn.onreconnected(async () => {
        await triviaConn.invoke("UnirseATriviaSession", sesionId);
      });

      triviaConn.start().then(() =>
        triviaConn.invoke("UnirseATriviaSession", sesionId)
      );
    }

    return () => {
      sesionConn.stop();
      triviaConnRef.current?.stop();
    };
  }, [token, sesionId, tipoSesion]);
}
```

---

## 12. Cliente React Native — Mobile (Equipo Participante)

```typescript
// src/hooks/useEquipoHub.ts (React Native)
import { HubConnectionBuilder, HttpTransportType, LogLevel } from "@microsoft/signalr";

export function useEquipoHub(
  sesionId: string,
  equipoId: string,
  tipoSesion: "BusquedaTesoro" | "Trivia",
  token: string
) {
  const sesionConnRef = useRef<HubConnection | null>(null);
  const triviaConnRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    const baseUrl = process.env.EXPO_PUBLIC_API_URL;

    // Hub de sesión (siempre)
    const sesionConn = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/sesion`, {
        accessTokenFactory: () => token,
        transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 3000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    sesionConn.on("SesionEstadoCambiado", (p) => handleEstadoCambiado(p));
    sesionConn.on("RankingActualizado", (p) => handleRankingActualizado(p));
    sesionConn.on("PistaLiberada", (p) => handlePistaLiberada(p));  // BT

    sesionConn.onreconnected(async () => {
      await sesionConn.invoke("UnirseASesion", sesionId, equipoId);
    });

    sesionConn.start()
      .then(() => sesionConn.invoke("UnirseASesion", sesionId, equipoId));

    sesionConnRef.current = sesionConn;

    // Hub de trivia (solo si el modo es Trivia)
    if (tipoSesion === "Trivia") {
      const triviaConn = new HubConnectionBuilder()
        .withUrl(`${baseUrl}/hubs/trivia`, {
          accessTokenFactory: () => token,
          transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 3000, 10000, 30000])
        .build();

      triviaConn.on("PreguntaLanzada", (p) => handlePreguntaLanzada(p));
      triviaConn.on("TiempoAgotado", (p) => handleTiempoAgotado(p));
      triviaConn.on("ResultadoRonda", (p) => handleResultadoRonda(p));

      triviaConn.onreconnected(async () => {
        await triviaConn.invoke("UnirseATriviaSession", sesionId);
      });

      triviaConn.start()
        .then(() => triviaConn.invoke("UnirseATriviaSession", sesionId));

      triviaConnRef.current = triviaConn;
    }

    return () => {
      sesionConnRef.current?.stop();
      triviaConnRef.current?.stop();
    };
  }, [sesionId, equipoId, tipoSesion, token]);

  const responderPregunta = async (preguntaId: string, opcionId: string) => {
    await triviaConnRef.current?.invoke("ResponderPregunta", {
      sesionId, equipoId, preguntaId, opcionSeleccionadaId: opcionId,
    });
  };

  return { responderPregunta };
}
```

---

## 13. Reglas SignalR — UMBRAL

| # | Regla | Motivo |
|---|-------|--------|
| 1 | **Dos hubs**: `SesionHub` para eventos comunes + BT, `TriviaHub` para eventos de Trivia. | Separación por dominio de eventos. |
| 2 | Los Hubs **no contienen lógica de negocio**; delegan a `ISender` (MediatR). | El Hub es solo adaptador de transporte. |
| 3 | Usar **Hub tipado** (`Hub<ISesionHubClient>`, `Hub<ITriviaHubClient>`). | Evita strings mágicos en nombres de métodos. |
| 4 | La capa de Aplicación usa **`INotificacionRealTime`** (port en Domain), no `IHubContext`. | Mantiene la aplicación desacoplada de SignalR. |
| 5 | Los payloads son **records inmutables** con nombres en PascalCase. | Serialización limpia con System.Text.Json. |
| 6 | Los clientes implementan **reconexión automática** con backoff exponencial. | Resiliencia ante pérdidas de conectividad. |
| 7 | Los clientes re-invocan el método de unión al grupo (`UnirseASesion`, etc.) **al reconectar**. | Los grupos SignalR se pierden al reconectar. |
| 8 | Sesión BT: solo se conecta a `SesionHub`. Sesión Trivia: se conecta a **ambos** hubs. | Una sesión es de un único tipo, nunca mixta. |
| 9 | Las pistas privadas se envían al grupo `equipo-{id}`. Las globales al grupo `sesion-{id}`. | Control de visibilidad por equipo (RB-07). |
| 10 | `EnableDetailedErrors` **solo en Development**. | Evita filtración de información en producción. |

---

## 14. Checklist al agregar un nuevo evento en tiempo real

```
□ ¿El método está declarado en ISesionHubClient o ITriviaHubClient (según el modo)?
□ ¿Existe el Payload record correspondiente?
□ ¿INotificacionRealTime tiene el método de notificación?
□ ¿NotificacionRealTimeService implementa el nuevo método con el hub correcto?
□ ¿El Domain Event Handler llama a INotificacionRealTime (no a IHubContext)?
□ ¿Los clientes web tienen el listener conn.on("NuevoEvento") en el hook correcto?
□ ¿Los clientes mobile tienen el listener correspondiente?
□ ¿El grupo objetivo es correcto (sesion-*, equipo-*, operador-*, trivia-*)?
□ ¿Los clientes re-unen el grupo al reconectar?
□ ¿El evento es del modo correcto (BT en SesionHub, Trivia en TriviaHub)?
```

---

## 15. Anti-patrones a evitar

```csharp
// ❌ MALO — un solo hub para todo (Trivia + BT mezclados)
public sealed class UmbralHub : Hub
{
    Task PreguntaLanzada(...) { }   // Trivia
    Task PistaLiberada(...) { }      // BT
    // ← mezclado, sin separación de responsabilidades
}

// ✅ BUENO — hub separado por dominio
// SesionHub → eventos comunes + BT
// TriviaHub  → solo Trivia

// ❌ MALO — lógica de negocio en el Hub
public async Task IniciarSesion(Guid sesionId)
{
    var sesion = await _repo.ObtenerPorIdAsync(sesionId);
    sesion.Estado = EstadoSesion.Activa;  // ← negocio directo en Hub
}

// ✅ BUENO — Hub delega al Command Handler
public async Task IniciarSesion(Guid sesionId)
    => await _sender.Send(new IniciarSesionCommand(sesionId));

// ❌ MALO — usar IHubContext directamente en Application
// (rompe la inversión de dependencias)
public async Task Handle(SesionIniciada e, CancellationToken ct)
{
    var hub = _serviceProvider.GetRequiredService<IHubContext<SesionHub>>();
    await hub.Clients.All.SendAsync("SesionEstadoCambiado", ...);
}

// ✅ BUENO — usar INotificacionRealTime (port)
public async Task Handle(SesionIniciada e, CancellationToken ct)
    => await _notifier.NotificarEstadoSesionAsync(e.SesionId.Valor, "Activa", ct);

// ❌ MALO — cliente Trivia solo conectado a SesionHub (pierde preguntas)
// En sesión Trivia el cliente DEBE conectarse a ambos hubs.

// ❌ MALO — no re-unirse al grupo al reconectar
conn.onreconnected(() => console.log("Reconectado"));  // ← no re-join al grupo

// ✅ BUENO — re-join al grupo al reconectar
conn.onreconnected(async () => {
    await conn.invoke("UnirseASesion", sesionId, equipoId);
});
```

---

## 16. Guía de uso para Cursor AI

Cuando el usuario pida agregar funcionalidad en tiempo real en UMBRAL:

1. **Determinar el modo** → ¿BusquedaTesoro, Trivia, o ambos?
2. **Seleccionar el hub correcto** → SesionHub (BT + comunes) o TriviaHub (Trivia).
3. **Declarar el método** en `ISesionHubClient` o `ITriviaHubClient` con su Payload record.
4. **Agregar el método** en `INotificacionRealTime` (port en Domain).
5. **Implementar** en `NotificacionRealTimeService` usando el grupo correcto.
6. **Invocar desde el Domain Event Handler** vía `INotificacionRealTime`.
7. **Agregar el listener** en el cliente TypeScript en el hook correcto (`useSesionHub` o la sección trivia).
8. **Agregar el listener** en el hook React Native.
9. **Asegurar re-unión al grupo** en el handler de reconexión.
10. **Correr checklist** de la sección 14 y advertir si hay anti-patrones de la sección 15.
