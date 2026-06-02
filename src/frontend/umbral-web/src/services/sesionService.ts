import { apiClient } from '@/services/apiClient'
import type { MisionActivaDto } from '@/types/sesion.types'
import type {
  MiInscripcionParticipanteDto,
  CancelarSesionRequest,
  CrearSesionBusquedaTesoroRequest,
  CrearSesionBusquedaTesoroResponse,
  CrearSesionTriviaRequest,
  CrearSesionTriviaResponse,
  PosicionRankingDto,
  PreguntaTriviaParticipanteDto,
  SesionDetalleDto,
  SesionDisponibleParticipanteDto,
  SesionResumenDto,
  UnirseSesionRequest,
  UnirseSesionResponse,
} from '@/types/sesion.types'

export async function listMisionesActivas(): Promise<MisionActivaDto[]> {
  const { data } = await apiClient.get<MisionActivaDto[]>('/misiones/activas')
  return data
}

export async function listSesionesOperativas(): Promise<SesionResumenDto[]> {
  const { data } = await apiClient.get<SesionResumenDto[]>('/sesiones')
  return data
}

export async function listSesionesDisponibles(): Promise<SesionDisponibleParticipanteDto[]> {
  const { data } = await apiClient.get<SesionDisponibleParticipanteDto[]>('/sesiones/disponibles')
  return data
}

export async function getMiInscripcionParticipante(): Promise<MiInscripcionParticipanteDto | null> {
  const { status, data } = await apiClient.get<MiInscripcionParticipanteDto>('/sesiones/mi-inscripcion', {
    validateStatus: (s) => s === 200 || s === 204,
  })
  if (status === 204) return null
  return data
}

export async function abandonarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/abandonar`)
}

/** @deprecated Usar listSesionesDisponibles */
export async function listSesionesDisponiblesBusqueda(): Promise<SesionDisponibleParticipanteDto[]> {
  return listSesionesDisponibles()
}

export async function listSesionesDisponiblesTrivia(): Promise<SesionDisponibleParticipanteDto[]> {
  const { data } = await apiClient.get<SesionDisponibleParticipanteDto[]>(
    '/sesiones/disponibles/trivia',
  )
  return data
}

export async function obtenerSesionDetalle(sesionId: string): Promise<SesionDetalleDto> {
  const { data } = await apiClient.get<SesionDetalleDto>(`/sesiones/${sesionId}`)
  return data
}

export async function crearSesionMision(body: {
  misionId: string
  nombreSesion: string
}): Promise<{
  id: string
  codigoAcceso: string
  nombreSesion: string
  misionNombre: string
}> {
  const { data } = await apiClient.post<{
    id: string
    codigoAcceso: string
    nombreSesion: string
    misionNombre: string
  }>('/sesiones', body)
  return data
}

export async function crearSesionBusquedaTesoro(
  body: CrearSesionBusquedaTesoroRequest,
): Promise<CrearSesionBusquedaTesoroResponse> {
  const { data } = await apiClient.post<CrearSesionBusquedaTesoroResponse>(
    '/sesiones/busqueda-tesoro',
    body,
  )
  return data
}

export async function crearSesionTrivia(
  body: CrearSesionTriviaRequest,
): Promise<CrearSesionTriviaResponse> {
  const { data } = await apiClient.post<CrearSesionTriviaResponse>('/sesiones/trivia', body)
  return data
}

export async function abrirInscripcionSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/abrir-inscripcion`)
}

export async function unirseSesion(
  sesionId: string,
  body: UnirseSesionRequest,
): Promise<UnirseSesionResponse> {
  const { data } = await apiClient.post<UnirseSesionResponse>(
    `/sesiones/${sesionId}/unirse`,
    body,
  )
  return data
}

export async function iniciarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/iniciar`)
}

export async function pausarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/pausar`)
}

export async function reanudarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/reanudar`)
}

export async function finalizarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/finalizar`)
}

export async function cancelarSesion(
  sesionId: string,
  body: CancelarSesionRequest,
): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/cancelar`, body)
}

export async function obtenerRankingSesion(sesionId: string): Promise<PosicionRankingDto[]> {
  const { data } = await apiClient.get<PosicionRankingDto[]>(`/sesiones/${sesionId}/ranking`)
  return data
}

export async function listPreguntasTriviaSesionParticipante(
  sesionId: string,
): Promise<PreguntaTriviaParticipanteDto[]> {
  const { data } = await apiClient.get<PreguntaTriviaParticipanteDto[]>(
    `/sesiones/${sesionId}/trivia/preguntas`,
  )
  return data
}
