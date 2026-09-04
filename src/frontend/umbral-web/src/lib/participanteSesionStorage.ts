import type { TipoSesionApi } from '@/types/sesion.types'

const STORAGE_KEY = 'umbral.participante.sesion'

export interface ParticipanteSesionInscrita {
  sesionId: string
  titulo: string
  participanteId: string
  joinedAt: string
  /** Ausente en sessionStorage antiguo → búsqueda del tesoro */
  tipoSesion?: TipoSesionApi
}

export function getParticipanteSesionInscrita(): ParticipanteSesionInscrita | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY) ?? localStorage.getItem('umbral.participante.sesionBt')
    if (!raw) return null
    return JSON.parse(raw) as ParticipanteSesionInscrita
  } catch {
    return null
  }
}

export function saveParticipanteSesionInscrita(data: ParticipanteSesionInscrita): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
  localStorage.removeItem('umbral.participante.sesionBt')
}

export function clearParticipanteSesionInscrita(): void {
  localStorage.removeItem(STORAGE_KEY)
  localStorage.removeItem('umbral.participante.sesionBt')
}

export function rutaPartidaParticipante(inscripcion: ParticipanteSesionInscrita): string {
  return `/participante/sesiones/${inscripcion.sesionId}`
}

export function rutaListadoParticipante(_tipoSesion?: TipoSesionApi): string {
  return '/participante'
}

export function confirmarAbandonarSesion(opciones?: { postJuego?: boolean }): boolean {
  if (opciones?.postJuego) {
    return window.confirm(
      '¿Salir de esta sesión?\n\nPodrás unirte a otra cuando el operador tenga una abierta.',
    )
  }
  return window.confirm(
    '¿Abandonar esta sesión?\n\nSaldrás del recorrido actual. Podrás unirte a otra sesión después si el operador la tiene abierta.',
  )
}
