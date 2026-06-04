import { NavLink, Outlet } from 'react-router-dom'

const subLinkClass = ({ isActive }: { isActive: boolean }) =>
  [
    'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
    isActive
      ? 'bg-indigo-100 text-indigo-800'
      : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
  ].join(' ')

/** Sub-navegación del banco de trivia (categorías / preguntas). */
export function TriviaLayout() {
  return (
    <div className="space-y-6">
      <nav
        className="flex flex-wrap gap-2 border-b border-slate-200 pb-3"
        aria-label="Banco de trivia"
      >
        <NavLink to="/admin/trivia/categorias" className={subLinkClass}>
          Categorías
        </NavLink>
        <NavLink to="/admin/trivia/preguntas" className={subLinkClass}>
          Preguntas
        </NavLink>
      </nav>
      <Outlet />
    </div>
  )
}
