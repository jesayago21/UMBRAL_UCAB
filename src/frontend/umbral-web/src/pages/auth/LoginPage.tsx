import { useEffect } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { isParticipanteWebEnabled } from '@/auth/participanteWebAccess'
import { syncOidcSession } from '@/auth/syncOidcSession'
import { SessionEndActions } from '@/components/shared/SessionEndActions'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { clearOidcBrowserState } from '@/auth/clearOidcBrowserState'
import { btnLink, btnPrimary } from '@/styles/ui'
import { useAuthStore } from '@/store/authStore'

export function LoginPage() {
  const auth = useAuth()
  const rol = useAuthStore((s) => s.rol)
  const logoutStore = useAuthStore((s) => s.logout)

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

  if (auth.isAuthenticated && rol === 'Participante') {
    if (!isParticipanteWebEnabled()) {
      return (
        <SessionEndActions message="El rol participante no está disponible en web. Usa la app mobile." />
      )
    }
    return <Navigate to="/participante" replace />
  }

  if (auth.isAuthenticated && rol && rol !== 'Participante') {
    return <Navigate to={getHomePathForRol(rol)} replace />
  }

  const handleLogin = async () => {
    try {
      // Evita invalid_code / tokens huérfanos tras reiniciar o recrear Keycloak.
      clearOidcBrowserState()
      logoutStore()
      await auth.clearStaleState()
      await auth.removeUser()
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

        {auth.error && (
          <div className="mt-4">
            <ErrorState message={auth.error.message} />
          </div>
        )}

        <button
          type="button"
          onClick={() => void handleLogin()}
          disabled={!!auth.activeNavigator}
          className={`mt-6 w-full ${btnPrimary}`}
        >
          {auth.activeNavigator ? 'Redirigiendo…' : 'Iniciar sesión'}
        </button>

        {isParticipanteWebEnabled() && (
          <p className="mt-4 text-center text-sm text-slate-600">
            ¿Eres participante y no tienes cuenta?{' '}
            <Link to="/registro" className={btnLink}>
              Crear cuenta de participante
            </Link>
          </p>
        )}
      </div>
    </div>
  )
}
