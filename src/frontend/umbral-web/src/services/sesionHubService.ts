import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { resolveSignalRBaseUrl } from '@/lib/apiUrls'

/**
 * LongPolling + Authorization header (no WebSocket query string).
 * Pasa siempre el token fresco vía accessTokenFactory.
 * Hub en la API (:5000), no en el gateway REST (:8000).
 */
export function crearSesionHub(getAccessToken: () => string | null | undefined): HubConnection {
  const hubUrl = `${resolveSignalRBaseUrl()}/hubs/sesion`
  return new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => {
        const token = getAccessToken()
        if (!token) throw new Error('Sin token OIDC para SignalR')
        return token
      },
      transport: HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build()
}

export function isHubConnected(connection: HubConnection | null): boolean {
  return connection?.state === HubConnectionState.Connected
}
