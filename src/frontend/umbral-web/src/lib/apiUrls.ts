/**
 * REST: VITE_API_URL (gateway :8000 por defecto).
 * SignalR: VITE_SIGNALR_URL (API directa :5000) — el gateway YARP solo enruta /api/v1/**
 * y sin esto el hub falla en :8000; el ganador ve ranking por REST y el resto no.
 */
export function resolveApiBaseUrl(): string {
  return (import.meta.env.VITE_API_URL ?? 'http://localhost:8000').replace(/\/$/, '')
}

export function resolveSignalRBaseUrl(): string {
  const explicit = import.meta.env.VITE_SIGNALR_URL as string | undefined
  if (explicit?.trim()) return explicit.replace(/\/$/, '')

  // Si REST apunta al gateway, SignalR debe ir a la API.
  const api = resolveApiBaseUrl()
  try {
    const u = new URL(api)
    if (u.port === '8000') {
      u.port = '5000'
      return u.origin
    }
  } catch {
    /* ignore */
  }

  return api || 'http://localhost:5000'
}
