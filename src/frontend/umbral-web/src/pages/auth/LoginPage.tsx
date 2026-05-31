import { useEffect } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { isEquipoWebEnabled } from '@/auth/equipoWebAccess'
import { syncOidcSession } from '@/auth/syncOidcSession'
import { SessionEndActions } from '@/components/shared/SessionEndActions'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { btnPrimary } from '@/styles/ui'
import { useAuthStore } from '@/store/authStore'

export function LoginPage() {
  const auth = useAuth()
  const rol = useAuthStore((s) => s.rol)

  useEffect(() => {
    if (auth.isLoading || !auth.isAuthenticated || !auth.user?.access_token) return
    syncOidcSession(auth.user.access_token)
  }, [auth.isLoading, auth.isAuthenticated, auth.user?.access_token])

  if (auth.isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <LoadingState label="Verificando sesión…" />
      </div>
    )
  }

  if (auth.isAuthenticated && rol === 'EquipoParticipante') {
    if (!isEquipoWebEnabled()) {
      return (
        <SessionEndActions message="El rol equipo está deshabilitado en web. Usa la app mobile cuando esté disponible." />
      )
    }
    return <Navigate to="/equipo" replace />
  }

  if (auth.isAuthenticated && rol && rol !== 'EquipoParticipante') {
    return <Navigate to={getHomePathForRol(rol)} replace />
  }

  const handleLogin = async () => {
    try {
      await auth.signinRedirect({
        prompt: 'login',
      })
    } catch (err) {
      console.error('signinRedirect failed', err)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-lg rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="text-2xl font-bold text-indigo-700">UMBRAL</h1>
        <p className="mt-2 text-sm text-slate-600">
          Inicia sesión con el realm <code className="text-xs">umbral</code> en Keycloak.
          Usuarios demo: <strong>admin</strong> / <strong>operador</strong> / <strong>equipo</strong> — contraseña{' '}
          <code className="text-xs">Umbral123!</code>
          {' '}(jugador: panel <code className="text-xs">/equipo</code> en web durante E1)
        </p>

        <p className="mt-2 text-xs text-slate-500">
          Keycloak: {import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080'}
        </p>

        {auth.error && (
          <div className="mt-4">
            <ErrorState
              message={`${auth.error.message} — Comprueba: docker compose up -d keycloak`}
            />
          </div>
        )}

        <button
          type="button"
          onClick={() => void handleLogin()}
          disabled={!!auth.activeNavigator}
          className={`mt-6 w-full ${btnPrimary}`}
        >
          {auth.activeNavigator ? 'Redirigiendo a Keycloak…' : 'Iniciar sesión con Keycloak'}
        </button>
      </div>
    </div>
  )
}
