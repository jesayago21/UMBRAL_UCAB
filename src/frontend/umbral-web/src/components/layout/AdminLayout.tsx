import { NavLink, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  [
    'rounded-md px-3 py-2 text-sm font-medium transition-colors',
    isActive
      ? 'bg-indigo-600 text-white'
      : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
  ].join(' ')

export function AdminLayout() {
  const { username, rol, logout } = useAuthStore()

  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4">
          <div>
            <p className="text-lg font-semibold text-indigo-700">UMBRAL</p>
            <p className="text-xs text-slate-500">
              {username ?? '—'} · {rol ?? 'sin rol'}
            </p>
          </div>
          <nav className="flex flex-wrap items-center gap-2">
            <NavLink to="/admin/misiones" className={linkClass}>
              Misiones
            </NavLink>
            <NavLink to="/admin/categorias" className={linkClass}>
              Categorías
            </NavLink>
            <NavLink to="/admin/preguntas" className={linkClass}>
              Preguntas
            </NavLink>
            <button
              type="button"
              onClick={() => {
                logout()
                window.location.href = '/login'
              }}
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
