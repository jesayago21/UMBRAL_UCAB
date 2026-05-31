import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { isEquipoWebEnabled } from '@/auth/equipoWebAccess'
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
  const [equipoWebBlocked, setEquipoWebBlocked] = useState(false)

  useEffect(() => {
    if (auth.isLoading) return
    if (auth.error) return
    if (!auth.isAuthenticated || !auth.user?.access_token) return

    const rol: RolUsuario | null = syncOidcSession(auth.user.access_token)

    if (!rol) {
      setRolError(
        'Tu usuario no tiene un rol válido (Administrador, Operador o Equipo).',
      )
      return
    }

    if (rol === 'EquipoParticipante' && !isEquipoWebEnabled()) {
      setEquipoWebBlocked(true)
      return
    }

    navigate(getHomePathForRol(rol), { replace: true })
  }, [auth.isLoading, auth.error, auth.isAuthenticated, auth.user?.access_token, navigate])

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

  if (equipoWebBlocked) {
    return (
      <SessionEndActions message="El rol equipo está deshabilitado en web. Usa la app mobile cuando esté disponible." />
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <LoadingState label="Completando inicio de sesión…" />
    </div>
  )
}
