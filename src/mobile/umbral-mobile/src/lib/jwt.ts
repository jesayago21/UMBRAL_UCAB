import type { RolUsuario } from '@/types/api.types'

interface RealmAccess {
  roles?: string[]
}

function base64UrlDecode(input: string): string {
  const padded = input.replace(/-/g, '+').replace(/_/g, '/')
  const pad = padded.length % 4 === 0 ? '' : '='.repeat(4 - (padded.length % 4))
  const b64 = padded + pad
  if (typeof globalThis.atob === 'function') {
    return globalThis.atob(b64)
  }
  // Decode manual (sin Buffer) para Hermes / RN
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
  let str = ''
  let i = 0
  const cleaned = b64.replace(/[^A-Za-z0-9+/=]/g, '')
  while (i < cleaned.length) {
    const enc1 = chars.indexOf(cleaned.charAt(i++))
    const enc2 = chars.indexOf(cleaned.charAt(i++))
    const enc3 = chars.indexOf(cleaned.charAt(i++))
    const enc4 = chars.indexOf(cleaned.charAt(i++))
    const chr1 = (enc1 << 2) | (enc2 >> 4)
    const chr2 = ((enc2 & 15) << 4) | (enc3 >> 2)
    const chr3 = ((enc3 & 3) << 6) | enc4
    str += String.fromCharCode(chr1)
    if (enc3 !== 64) str += String.fromCharCode(chr2)
    if (enc4 !== 64) str += String.fromCharCode(chr3)
  }
  return str
}

export function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split('.')[1]
    if (!part) return null
    return JSON.parse(base64UrlDecode(part)) as Record<string, unknown>
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
  if (roles.includes('Participante') || roles.includes('EquipoParticipante')) {
    return 'Participante'
  }
  return null
}

export function extractUsername(payload: Record<string, unknown>): string {
  const preferred = payload.preferred_username
  if (typeof preferred === 'string' && preferred.length > 0) return preferred
  const sub = payload.sub
  if (typeof sub === 'string') return sub
  return 'usuario'
}
