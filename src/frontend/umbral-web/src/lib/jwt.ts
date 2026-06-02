import type { RolUsuario } from '@/types/api.types'

interface RealmAccess {
  roles?: string[]
}

export function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split('.')[1]
    if (!part) return null
    const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'))
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

export function extractRoles(payload: Record<string, unknown>): string[] {
  const realmAccess = payload.realm_access as RealmAccess | undefined
  return realmAccess?.roles ?? []
}

export function resolveRol(roles: string[]): RolUsuario | null {
  if (roles.includes('Administrador')) return 'Administrador'
  if (roles.includes('Operador')) return 'Operador'
  if (roles.includes('Participante')) return 'Participante'
  return null
}

export function extractUsername(payload: Record<string, unknown>): string {
  const preferred = payload.preferred_username
  if (typeof preferred === 'string' && preferred.length > 0) return preferred
  const sub = payload.sub
  if (typeof sub === 'string') return sub
  return 'usuario'
}
