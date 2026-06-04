import { mapEstadoSesionFromApi } from '@/lib/sesionUi'
import type { OperadorSesionState, SesionDetalleDto } from '@/types/sesion.types'

export function detalleToOperadorState(detalle: SesionDetalleDto): OperadorSesionState {
  return {
    sesionId: detalle.id,
    tipoSesion: detalle.tipoSesion,
    misionId: detalle.misionId,
    nombre: detalle.nombre,
    misionNombre: detalle.misionNombre,
    estado: mapEstadoSesionFromApi(detalle.estado),
    codigoAcceso: detalle.codigoAcceso,
    participantes: detalle.participantes.map((e) => ({
      participanteId: e.participanteId,
      jugadorId: e.jugadorId,
      nombre: e.nombre,
    })),
    iniciadaEn: detalle.iniciadaEn,
    finalizadaEn: detalle.finalizadaEn,
    etapaActualOrden: detalle.etapaActualOrden,
    totalEtapas: detalle.totalEtapas,
    etapaDescripcion: detalle.etapaActualDescripcion,
  }
}

const storageKey = (sesionId: string) => `umbral-operador-sesion-${sesionId}`

export function loadOperadorSesionState(sesionId: string): OperadorSesionState | null {
  try {
    const raw = sessionStorage.getItem(storageKey(sesionId))
    if (!raw) return null
    return JSON.parse(raw) as OperadorSesionState
  } catch {
    return null
  }
}

export function saveOperadorSesionState(state: OperadorSesionState): void {
  sessionStorage.setItem(storageKey(state.sesionId), JSON.stringify(state))
}

export function createInitialSesionState(
  sesionId: string,
  tipoSesion: OperadorSesionState['tipoSesion'],
  misionId: string,
  nombre: string,
  misionNombre: string,
  codigoAcceso: string,
): OperadorSesionState {
  return {
    sesionId,
    tipoSesion,
    misionId,
    nombre,
    misionNombre,
    codigoAcceso,
    estado: 'programada',
    participantes: [],
  }
}
