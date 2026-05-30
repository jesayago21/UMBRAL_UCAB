import { extractRoles, extractUsername, parseJwtPayload, resolveRol } from '@/lib/jwt'
import { useAuthStore } from '@/store/authStore'
import type { RolUsuario } from '@/types/api.types'

export function syncOidcSession(accessToken: string): RolUsuario | null {
  const payload = parseJwtPayload(accessToken)
  if (!payload) return null

  const rol = resolveRol(extractRoles(payload))
  if (!rol) return null

  useAuthStore.getState().login({
    token: accessToken,
    rol,
    username: extractUsername(payload),
  })

  return rol
}
