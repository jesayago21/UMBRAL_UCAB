import Constants from 'expo-constants'

function readEnv(key: string, fallback: string): string {
  const fromProcess = process.env[key]
  if (typeof fromProcess === 'string' && fromProcess.length > 0) return fromProcess
  const extra = Constants.expoConfig?.extra as Record<string, string> | undefined
  if (extra?.[key]) return extra[key]
  return fallback
}

export function resolveApiBaseUrl(): string {
  return readEnv('EXPO_PUBLIC_API_URL', 'http://localhost:8000').replace(/\/$/, '')
}

export function resolveSignalRBaseUrl(): string {
  return readEnv('EXPO_PUBLIC_SIGNALR_URL', 'http://localhost:5000').replace(/\/$/, '')
}

export function resolveKeycloakUrl(): string {
  return readEnv('EXPO_PUBLIC_KEYCLOAK_URL', 'http://localhost:8080').replace(/\/$/, '')
}

export function resolveKeycloakRealm(): string {
  return readEnv('EXPO_PUBLIC_KEYCLOAK_REALM', 'umbral')
}

export function resolveKeycloakClientId(): string {
  return readEnv('EXPO_PUBLIC_KEYCLOAK_CLIENT_ID', 'umbral-mobile')
}
