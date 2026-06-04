import type { EstadoSesionUi } from '@/types/sesion.types'

export function mapEstadoSesionFromApi(estado: string): EstadoSesionUi {
  const map: Record<string, EstadoSesionUi> = {
    Programada: 'programada',
    EnPreparacion: 'enPreparacion',
    Activa: 'activa',
    Pausada: 'pausada',
    Finalizada: 'finalizada',
    Cancelada: 'cancelada',
  }
  return map[estado] ?? 'programada'
}

export function formatDuracion(totalSegundos: number): string {
  const h = Math.floor(totalSegundos / 3600)
  const m = Math.floor((totalSegundos % 3600) / 60)
  const s = totalSegundos % 60
  if (h > 0) {
    return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  }
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
}
