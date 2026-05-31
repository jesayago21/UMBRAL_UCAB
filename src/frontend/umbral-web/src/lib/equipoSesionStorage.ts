import type { TipoSesionApi } from '@/types/sesion.types'

const STORAGE_KEY = 'umbral.equipo.sesion'

export interface EquipoSesionInscrita {
  sesionId: string
  titulo: string
  equipoId: string
  joinedAt: string
  /** Ausente en sessionStorage antiguo → búsqueda del tesoro */
  tipoSesion?: TipoSesionApi
}

export function getEquipoSesionInscrita(): EquipoSesionInscrita | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY) ?? localStorage.getItem('umbral.equipo.sesionBt')
    if (!raw) return null
    return JSON.parse(raw) as EquipoSesionInscrita
  } catch {
    return null
  }
}

export function saveEquipoSesionInscrita(data: EquipoSesionInscrita): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
  localStorage.removeItem('umbral.equipo.sesionBt')
}

export function clearEquipoSesionInscrita(): void {
  localStorage.removeItem(STORAGE_KEY)
  localStorage.removeItem('umbral.equipo.sesionBt')
}

export function rutaPartidaEquipo(inscripcion: EquipoSesionInscrita): string {
  if (inscripcion.tipoSesion === 'Trivia') {
    return `/equipo/trivia/${inscripcion.sesionId}`
  }
  return `/equipo/busqueda/${inscripcion.sesionId}`
}

export function rutaListadoEquipo(tipoSesion?: TipoSesionApi): string {
  return tipoSesion === 'Trivia' ? '/equipo/trivia' : '/equipo/busqueda'
}
