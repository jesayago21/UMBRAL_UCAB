import { apiClient } from '@/services/apiClient'
import type { MisionActivaDto } from '@/types/sesion.types'
import type {
  CancelarSesionRequest,
  CrearSesionBusquedaTesoroRequest,
  CrearSesionBusquedaTesoroResponse,
  CrearSesionTriviaRequest,
  CrearSesionTriviaResponse,
  PosicionRankingDto,
  PreguntaTriviaEquipoDto,
  SesionDetalleDto,
  SesionDisponibleEquipoDto,
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

export async function listSesionesDisponiblesBusqueda(): Promise<SesionDisponibleEquipoDto[]> {
  const { data } = await apiClient.get<SesionDisponibleEquipoDto[]>(
    '/sesiones/disponibles/busqueda-tesoro',
  )
  return data
}

export async function listSesionesDisponiblesTrivia(): Promise<SesionDisponibleEquipoDto[]> {
  const { data } = await apiClient.get<SesionDisponibleEquipoDto[]>(
    '/sesiones/disponibles/trivia',
  )
  return data
}

export async function obtenerSesionDetalle(sesionId: string): Promise<SesionDetalleDto> {
  const { data } = await apiClient.get<SesionDetalleDto>(`/sesiones/${sesionId}`)
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

export async function listPreguntasTriviaSesionEquipo(
  sesionId: string,
): Promise<PreguntaTriviaEquipoDto[]> {
  const { data } = await apiClient.get<PreguntaTriviaEquipoDto[]>(
    `/sesiones/${sesionId}/trivia/preguntas`,
  )
  return data
}
