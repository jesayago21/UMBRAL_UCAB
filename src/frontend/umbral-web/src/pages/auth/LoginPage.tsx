import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import {
  extractRoles,
  extractUsername,
  parseJwtPayload,
  resolveRol,
} from '@/lib/jwt'
import { useAuthStore } from '@/store/authStore'

export function LoginPage() {
  const { login, estaAutenticado } = useAuthStore()
  const [token, setToken] = useState('')
  const [error, setError] = useState<string | null>(null)

  if (estaAutenticado()) {
    return <Navigate to="/admin/misiones" replace />
  }

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)

    const trimmed = token.trim()
    if (!trimmed) {
      setError('Pega un token JWT de Keycloak.')
      return
    }

    const payload = parseJwtPayload(trimmed)
    if (!payload) {
      setError('Token JWT inválido.')
      return
    }

    const rol = resolveRol(extractRoles(payload))
    if (rol !== 'Administrador') {
      setError('Se requiere rol Administrador para el catálogo (E1-2).')
      return
    }

    login({
      token: trimmed,
      rol,
      username: extractUsername(payload),
    })
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-lg rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="text-2xl font-bold text-indigo-700">UMBRAL — Admin</h1>
        <p className="mt-2 text-sm text-slate-600">
          Acceso temporal con token Keycloak (E1-K4 reemplazará esto por login OIDC).
          Obtén el token del realm <code className="text-xs">umbral</code> con usuario
          admin demo.
        </p>

        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
          <label className="block text-sm font-medium text-slate-700">
            Bearer token
            <textarea
              value={token}
              onChange={(e) => setToken(e.target.value)}
              rows={5}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-xs"
              placeholder="eyJhbGciOi..."
            />
          </label>

          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>
          )}

          <button
            type="submit"
            className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700"
          >
            Entrar al catálogo
          </button>
        </form>
      </div>
    </div>
  )
}
