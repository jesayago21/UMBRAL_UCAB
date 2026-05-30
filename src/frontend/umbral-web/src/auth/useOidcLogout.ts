import { useCallback } from 'react'
import { useAuth } from 'react-oidc-context'
import { useAuthStore } from '@/store/authStore'

/**
 * Cierra sesión en Zustand y en Keycloak (SSO) para poder elegir otro usuario.
 */
export function useOidcLogout() {
  const auth = useAuth()
  const logoutStore = useAuthStore((s) => s.logout)

  return useCallback(async () => {
    logoutStore()

    try {
      await auth.signoutRedirect({
        post_logout_redirect_uri: `${window.location.origin}/login`,
      })
      return
    } catch (error) {
      console.warn('signoutRedirect falló, limpiando sesión local', error)
    }

    try {
      await auth.removeUser()
    } catch {
      // ignore
    }

    window.location.replace('/login')
  }, [auth, logoutStore])
}
