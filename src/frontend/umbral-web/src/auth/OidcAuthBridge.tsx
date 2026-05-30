import { useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { extractRoles, extractUsername, parseJwtPayload, resolveRol } from '@/lib/jwt'
import { useAuthStore } from '@/store/authStore'

/**
 * Sincroniza el usuario OIDC (Keycloak) con authStore para apiClient e guards.
 */
export function OidcAuthBridge({ children }: { children: React.ReactNode }) {
  const auth = useAuth()
  const { login, logout } = useAuthStore()

  useEffect(() => {
    if (auth.isLoading) return

    const token = auth.user?.access_token
    if (auth.isAuthenticated && token) {
      const payload = parseJwtPayload(token)
      if (!payload) return
      const rol = resolveRol(extractRoles(payload))
      if (!rol) return
      login({
        token,
        rol,
        username: extractUsername(payload),
      })
      return
    }

    if (!auth.isAuthenticated && !auth.activeNavigator) {
      logout()
    }
  }, [
    auth.isLoading,
    auth.isAuthenticated,
    auth.user?.access_token,
    auth.activeNavigator,
    login,
    logout,
  ])

  return <>{children}</>
}
