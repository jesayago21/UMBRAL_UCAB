import { NavLink, Outlet } from 'react-router-dom'
import { useOidcLogout } from '@/auth/useOidcLogout'
import { isParticipanteWebEnabled } from '@/auth/participanteWebAccess'
import { SessionEndActions } from '@/components/shared/SessionEndActions'
import { useAuthStore } from '@/store/authStore'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  [
    'rounded-md px-3 py-2 text-sm font-medium transition-colors',
    isActive
      ? 'bg-indigo-600 text-white'
      : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
  ].join(' ')

export function ParticipanteLayout() {
  const { username } = useAuthStore()
  const logout = useOidcLogout()

  if (!isParticipanteWebEnabled()) {
    return (
      <SessionEndActions message="El panel participante en web está desactivado. Usa la app mobile." />
    )
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-3">
            <div>
              <p className="text-lg font-semibold text-indigo-700">UMBRAL</p>
              <p className="text-xs text-slate-500">Jugador · {username ?? '—'}</p>
            </div>
            <span className="rounded-full bg-emerald-100 px-2.5 py-0.5 text-xs font-medium text-emerald-900">
              Participante
            </span>
          </div>
          <nav className="flex flex-wrap items-center gap-2" aria-label="Panel participante">
            <NavLink to="/participante" className={linkClass} end>
              Inicio
            </NavLink>
            <button
              type="button"
              onClick={() => void logout()}
              className="rounded-md px-3 py-2 text-sm text-slate-600 hover:bg-slate-100"
            >
              Salir
            </button>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
