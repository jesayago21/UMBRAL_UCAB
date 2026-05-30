import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useAuthStore } from '@/store/authStore'

export function CallbackPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const { rol, estaAutenticado } = useAuthStore()

  useEffect(() => {
    if (auth.isLoading) return
    if (auth.error) return
    if (estaAutenticado() && rol) {
      navigate(getHomePathForRol(rol), { replace: true })
    }
  }, [auth.isLoading, auth.error, estaAutenticado, rol, navigate])

  if (auth.error) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <ErrorState
          message={
            auth.error.message ??
            'No se pudo completar el inicio de sesión con Keycloak.'
          }
        />
      </div>
    )
  }

  return <LoadingState label="Completando inicio de sesión…" />
}
