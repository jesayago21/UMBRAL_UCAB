import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { syncOidcSession } from '@/auth/syncOidcSession'
import { SessionEndActions } from '@/components/shared/SessionEndActions'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useOidcLogout } from '@/auth/useOidcLogout'
import { btnPrimary } from '@/styles/ui'
import type { RolUsuario } from '@/types/api.types'

export function CallbackPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const logout = useOidcLogout()
  const [rolError, setRolError] = useState<string | null>(null)
  const [equipoBlocked, setEquipoBlocked] = useState(false)

  useEffect(() => {
    if (auth.isLoading) return
    if (auth.error) return
    if (!auth.isAuthenticated || !auth.user?.access_token) return

    const rol: RolUsuario | null = syncOidcSession(auth.user.access_token)

    if (!rol) {
      setRolError(
        'Tu usuario no tiene un rol válido para la web (Administrador u Operador).',
      )
      return
    }

    if (rol === 'EquipoParticipante') {
      setEquipoBlocked(true)
      return
    }

    navigate(getHomePathForRol(rol), { replace: true })
  }, [auth.isLoading, auth.error, auth.isAuthenticated, auth.user?.access_token, navigate])

  if (equipoBlocked) {
    return (
      <SessionEndActions message="El rol EquipoParticipante no tiene acceso al panel web." />
    )
  }

  if (auth.error) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="max-w-md space-y-4 text-center">
          <ErrorState
            message={
              auth.error.message ??
              'No se pudo completar el inicio de sesión. ¿Está Keycloak en marcha (puerto 8080)?'
            }
          />
          <button type="button" onClick={() => void logout()} className={btnPrimary}>
            Volver al login
          </button>
        </div>
      </div>
    )
  }

  if (rolError) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="max-w-md space-y-4 text-center">
          <ErrorState message={rolError} />
          <button type="button" onClick={() => void logout()} className={`${btnPrimary} w-full`}>
            Cerrar sesión e iniciar con otra cuenta
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <LoadingState label="Completando inicio de sesión…" />
    </div>
  )
}
