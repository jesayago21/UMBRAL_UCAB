import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { isParticipanteWebEnabled } from '@/auth/participanteWebAccess'
import { syncOidcSession } from '@/auth/syncOidcSession'
import { clearOidcBrowserState } from '@/auth/clearOidcBrowserState'
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
  const [participanteWebBlocked, setParticipanteWebBlocked] = useState(false)

  useEffect(() => {
    if (auth.isLoading) return
    if (auth.error) {
      clearOidcBrowserState()
      return
    }
    if (!auth.isAuthenticated || !auth.user?.access_token) return

    const rol: RolUsuario | null = syncOidcSession(auth.user.access_token)

    if (!rol) {
      setRolError(
        'Tu usuario no tiene un rol válido (Administrador, Operador o Participante).',
      )
      return
    }

    if (rol === 'Participante' && !isParticipanteWebEnabled()) {
      setParticipanteWebBlocked(true)
      return
    }

    navigate(getHomePathForRol(rol), { replace: true })
  }, [auth.isLoading, auth.error, auth.isAuthenticated, auth.user?.access_token, navigate])

  if (auth.error) {
    const isInvalidCode =
      /invalid_code|No matching state|Code mismatch|invalid_grant/i.test(
        auth.error.message ?? '',
      )
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="max-w-md space-y-4 text-center">
          <ErrorState
            message={
              isInvalidCode
                ? 'Sesión OIDC inválida (suele pasar tras reiniciar Keycloak). Vuelve al login e inténtalo de nuevo.'
                : (auth.error.message ??
                  'No se pudo completar el inicio de sesión. ¿Está Keycloak en marcha (puerto 8080)?')
            }
          />
          <button
            type="button"
            onClick={() => {
              clearOidcBrowserState()
              void logout()
              navigate('/login', { replace: true })
            }}
            className={btnPrimary}
          >
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

  if (participanteWebBlocked) {
    return (
      <SessionEndActions message="El rol participante está deshabilitado en web. Usa la app mobile cuando esté disponible." />
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <LoadingState label="Completando inicio de sesión…" />
    </div>
  )
}
