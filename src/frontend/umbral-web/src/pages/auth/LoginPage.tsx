import { Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { getHomePathForRol } from '@/auth/authPaths'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useAuthStore } from '@/store/authStore'

export function LoginPage() {
  const auth = useAuth()
  const { estaAutenticado, rol } = useAuthStore()

  if (auth.isLoading) {
    return <LoadingState label="Verificando sesión…" />
  }

  if (estaAutenticado() && rol) {
    return <Navigate to={getHomePathForRol(rol)} replace />
  }

  const handleLogin = () => {
    void auth.signinRedirect()
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-lg rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="text-2xl font-bold text-indigo-700">UMBRAL</h1>
        <p className="mt-2 text-sm text-slate-600">
          Inicia sesión con el realm <code className="text-xs">umbral</code> en Keycloak.
          Usuarios demo: <strong>admin</strong> / <strong>operador</strong> — contraseña{' '}
          <code className="text-xs">Umbral123!</code>
        </p>

        {auth.error && (
          <div className="mt-4">
            <ErrorState message={auth.error.message} />
          </div>
        )}

        <button
          type="button"
          onClick={handleLogin}
          disabled={!!auth.activeNavigator}
          className="mt-6 w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {auth.activeNavigator ? 'Redirigiendo a Keycloak…' : 'Iniciar sesión con Keycloak'}
        </button>
      </div>
    </div>
  )
}
