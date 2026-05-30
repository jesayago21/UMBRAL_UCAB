/**
 * Placeholder hasta E1-2b (CRUD sesión BT en UI).
 */
export function OperadorSesionesPage() {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6">
      <h2 className="text-xl font-semibold">Sesiones de búsqueda del tesoro</h2>
      <p className="mt-2 text-sm text-slate-600">
        La gestión de sesiones (crear, equipos, iniciar/pausar, ranking) se implementa en{' '}
        <strong>E1-2b</strong>. El login OIDC (E1-K4) ya te identifica como operador.
      </p>
      <p className="mt-4 text-sm text-slate-500">
        La API <code className="text-xs">/api/v1/sesiones</code> está lista; la UI llegará en la
        siguiente iteración.
      </p>
    </div>
  )
}
