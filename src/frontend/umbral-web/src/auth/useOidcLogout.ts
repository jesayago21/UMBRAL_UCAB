import { useCallback } from 'react'
import { useAuth } from 'react-oidc-context'
import { useAuthStore } from '@/store/authStore'

export function useOidcLogout() {
  const auth = useAuth()
  const logoutStore = useAuthStore((s) => s.logout)

  return useCallback(async () => {
    logoutStore()
    try {
      await auth.signoutRedirect()
    } catch {
      window.location.href = '/login'
    }
  }, [auth, logoutStore])
}
