import { useOidcLogout } from '@/auth/useOidcLogout'
import { btnPrimary } from '@/styles/ui'

interface SessionEndActionsProps {
  message?: string
}

/** Pantalla auxiliar cuando el rol no aplica a la web (p. ej. equipo). */
export function SessionEndActions({
  message = 'Esta cuenta no puede usar el panel web.',
}: SessionEndActionsProps) {
  const logout = useOidcLogout()

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="max-w-md rounded-xl border border-amber-200 bg-amber-50 p-6 text-center">
        <h1 className="text-lg font-semibold text-amber-900">Cuenta no admitida en web</h1>
        <p className="mt-2 text-sm text-amber-800">{message}</p>
        <p className="mt-2 text-xs text-amber-700">
          El usuario <strong>equipo</strong> es para la app mobile (E2). En web usa{' '}
          <strong>admin</strong> u <strong>operador</strong>.
        </p>
        <button
          type="button"
          onClick={() => void logout()}
          className={`${btnPrimary} mt-5 w-full`}
        >
          Cerrar sesión e iniciar con otra cuenta
        </button>
      </div>
    </div>
  )
}
