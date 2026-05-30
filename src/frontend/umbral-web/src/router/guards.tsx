import { Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { AccessDenied } from '@/components/shared/AccessDenied'
import { LoadingState } from '@/components/shared/LoadingState'
import type { RolUsuario } from '@/types/api.types'
import { useAuthStore } from '@/store/authStore'

function useSessionReady() {
  const auth = useAuth()
  const estaAutenticado = useAuthStore((s) => s.estaAutenticado)
  return {
    isLoading: auth.isLoading,
    isAuthenticated: auth.isAuthenticated || estaAutenticado(),
  }
}

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const { isLoading, isAuthenticated } = useSessionReady()

  if (isLoading) return <LoadingState label="Verificando sesión…" />
  if (!isAuthenticated) return <Navigate to="/login" replace />

  return <>{children}</>
}

export function RequireRoles({
  roles,
  children,
  deniedMessage,
}: {
  roles: RolUsuario[]
  children: React.ReactNode
  deniedMessage: string
}) {
  const { isLoading, isAuthenticated } = useSessionReady()
  const rol = useAuthStore((s) => s.rol)

  if (isLoading) return <LoadingState label="Verificando sesión…" />
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (!rol || !roles.includes(rol)) {
    return <AccessDenied message={deniedMessage} />
  }

  return <>{children}</>
}

export function HomeRedirect() {
  const { isLoading, isAuthenticated } = useSessionReady()
  const rol = useAuthStore((s) => s.rol)

  if (isLoading) return <LoadingState />
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (rol === 'Administrador') return <Navigate to="/admin/misiones" replace />
  if (rol === 'Operador') return <Navigate to="/operador/sesiones" replace />

  return (
    <AccessDenied message="El rol EquipoParticipante usa la app mobile (E2 / E1-M1 opcional)." />
  )
}
