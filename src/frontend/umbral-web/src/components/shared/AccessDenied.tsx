import { Link } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { useOidcLogout } from '@/auth/useOidcLogout'
import { btnPrimary, btnSecondary } from '@/styles/ui'
import type { RolUsuario } from '@/types/api.types'
import { useAuthStore } from '@/store/authStore'

interface AccessDeniedProps {
  title?: string
  message: string
}

function isWebRole(rol: RolUsuario): rol is 'Administrador' | 'Operador' {
  return rol === 'Administrador' || rol === 'Operador'
}

export function AccessDenied({
  title = 'Acceso denegado',
  message,
}: AccessDeniedProps) {
  const auth = useAuth()
  const rol = useAuthStore((s) => s.rol)
  const token = useAuthStore((s) => s.token)
  const logout = useOidcLogout()

  const hasSession = auth.isAuthenticated || Boolean(token)
  const mustSwitchAccount = hasSession && (!rol || !isWebRole(rol))
  const canGoHome = hasSession && rol && isWebRole(rol)

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="max-w-md rounded-xl border border-red-200 bg-red-50 p-6 text-center">
        <h1 className="text-lg font-semibold text-red-800">{title}</h1>
        <p className="mt-2 text-sm text-red-700">{message}</p>

        <div className="mt-5 flex flex-col gap-2">
          {mustSwitchAccount && (
            <button type="button" onClick={() => void logout()} className={btnPrimary}>
              Cerrar sesión e iniciar con otra cuenta
            </button>
          )}

          {canGoHome && (
            <Link to={getHomePathForRol(rol)} className={`${btnSecondary} inline-block`}>
              Volver a mi inicio
            </Link>
          )}

          {!hasSession && (
            <Link to="/login" className={`${btnSecondary} inline-block`}>
              Ir al login
            </Link>
          )}
        </div>
      </div>
    </div>
  )
}
