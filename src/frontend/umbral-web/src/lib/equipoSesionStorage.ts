const STORAGE_KEY = 'umbral.equipo.sesionBt'

export interface EquipoSesionInscrita {
  sesionId: string
  titulo: string
  equipoId: string
  joinedAt: string
}

export function getEquipoSesionInscrita(): EquipoSesionInscrita | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    return JSON.parse(raw) as EquipoSesionInscrita
  } catch {
    return null
  }
}

export function saveEquipoSesionInscrita(data: EquipoSesionInscrita): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
}

export function clearEquipoSesionInscrita(): void {
  localStorage.removeItem(STORAGE_KEY)
}
