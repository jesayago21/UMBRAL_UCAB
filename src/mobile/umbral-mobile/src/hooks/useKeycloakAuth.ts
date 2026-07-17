import { useCallback, useMemo, useState } from 'react'
import {
  extractRoles,
  extractUsername,
  parseJwtPayload,
  resolveRol,
} from '@/lib/jwt'
import {
  resolveKeycloakClientId,
  resolveKeycloakRealm,
  resolveKeycloakUrl,
} from '@/lib/env'
import { useAuthStore } from '@/store/authStore'

function tokenEndpoint(): string {
  return `${resolveKeycloakUrl()}/realms/${resolveKeycloakRealm()}/protocol/openid-connect/token`
}

function applyAccessToken(
  accessToken: string,
  loginStore: ReturnType<typeof useAuthStore.getState>['login'],
): string | null {
  const payload = parseJwtPayload(accessToken)
  if (!payload) return 'Token inválido.'
  const rol = resolveRol(extractRoles(payload))
  if (rol !== 'Participante') return 'Esta app es solo para el rol Participante.'
  loginStore({
    token: accessToken,
    rol,
    username: extractUsername(payload),
  })
  return null
}

/**
 * Auth contra el mismo Keycloak/realm que umbral-web.
 *
 * En Expo Go el redirect OIDC del navegador (como en web) suele romperse
 * (Custom Tabs + exp:// → "Access error 404"). Por eso mobile usa
 * Resource Owner Password (mismo usuario participante / Umbral123!).
 * El token y el rol son los mismos que obtendría la web.
 */
export function useKeycloakAuth() {
  const loginStore = useAuthStore((s) => s.login)
  const logoutStore = useAuthStore((s) => s.logout)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const clientId = resolveKeycloakClientId()
  const keycloakUrl = resolveKeycloakUrl()
  const endpoint = useMemo(() => tokenEndpoint(), [])

  const loginWithPassword = useCallback(
    async (username: string, password: string) => {
      setError(null)
      setBusy(true)
      try {
        const body =
          `grant_type=password` +
          `&client_id=${encodeURIComponent(clientId)}` +
          `&username=${encodeURIComponent(username.trim())}` +
          `&password=${encodeURIComponent(password)}` +
          `&scope=${encodeURIComponent('openid profile email')}`

        const response = await fetch(endpoint, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            Accept: 'application/json',
          },
          body,
        })

        const raw = await response.text()
        if (!raw || raw.trimStart().startsWith('<')) {
          setError(
            `No hay JSON desde Keycloak (${response.status}). ` +
              `URL: ${endpoint}. ¿Keycloak en :8180? (en tu PC :8080 lo ocupa otro proceso). ` +
              `Reinicia: docker compose up keycloak -d --force-recreate y Expo --clear.`,
          )
          return
        }

        let data: {
          access_token?: string
          error?: string
          error_description?: string
        }
        try {
          data = JSON.parse(raw) as typeof data
        } catch {
          setError(`Respuesta inválida (${response.status}): ${raw.slice(0, 100)}`)
          return
        }

        if (!response.ok || !data.access_token) {
          setError(data.error_description ?? data.error ?? `Login falló (${response.status}).`)
          return
        }

        const applyError = applyAccessToken(data.access_token, loginStore)
        if (applyError) setError(applyError)
      } catch (err) {
        setError(
          err instanceof Error
            ? `${err.message}\nKeycloak: ${endpoint}`
            : `Sin conexión a ${endpoint}`,
        )
      } finally {
        setBusy(false)
      }
    },
    [clientId, endpoint, loginStore],
  )

  const logout = useCallback(() => {
    logoutStore()
  }, [logoutStore])

  return {
    loginWithPassword,
    logout,
    error,
    busy,
    keycloakUrl,
    tokenUrl: endpoint,
  }
}
