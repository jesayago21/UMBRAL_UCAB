import { useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { syncOidcSession } from '@/auth/syncOidcSession'

function isOidcCallbackRoute(pathname: string, search: string): boolean {
  return pathname === '/callback' || search.includes('code=') || search.includes('state=')
}

/**
 * Sincroniza el usuario OIDC (Keycloak) con authStore para apiClient e guards.
 */
export function OidcAuthBridge({ children }: { children: React.ReactNode }) {
  const auth = useAuth()
  const location = useLocation()
  const logout = useAuthStore((s) => s.logout)

  useEffect(() => {
    if (auth.isLoading) return

    const token = auth.user?.access_token
    if (auth.isAuthenticated && token) {
      syncOidcSession(token)
      return
    }

    const onCallback = isOidcCallbackRoute(location.pathname, location.search)
    if (!auth.isAuthenticated && !auth.activeNavigator && !onCallback) {
      logout()
    }
  }, [
    auth.isLoading,
    auth.isAuthenticated,
    auth.user?.access_token,
    auth.activeNavigator,
    location.pathname,
    location.search,
    logout,
  ])

  return <>{children}</>
}
