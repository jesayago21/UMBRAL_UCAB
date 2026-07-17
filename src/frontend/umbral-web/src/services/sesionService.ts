import { apiClient } from '@/services/apiClient'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import type { MisionActivaDto } from '@/types/sesion.types'
import type {
  MiInscripcionParticipanteDto,
  AplicarPenalizacionRequest,
  LiberarPistaManualRequest,
  CancelarSesionRequest,
  CrearSesionBusquedaTesoroRequest,
  CrearSesionBusquedaTesoroResponse,
  CrearSesionTriviaRequest,
  CrearSesionTriviaResponse,
  HistorialSesionDto,
  PosicionRankingDto,
  PreguntaTriviaParticipanteDto,
  EstadoTriviaSesionDto,
  ReporteFinalSesionDto,
  SesionDetalleDto,
  SesionDisponibleParticipanteDto,
  SesionResumenDto,
  SubmitEvidenciaRequest,
  SubmitEvidenciaResponse,
  SubmitRespuestaTriviaRequest,
  SubmitRespuestaTriviaResponse,
  TriviaRespuestaFeedback,
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

export async function expulsarParticipante(
  sesionId: string,
  participanteId: string,
  body: { motivo: string },
): Promise<void> {
  await apiClient.delete(`/sesiones/${sesionId}/participantes/${participanteId}`, { data: body })
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
  return ordenarYNumerarRanking(data ?? [])
}

export async function listPreguntasTriviaSesionParticipante(
  sesionId: string,
): Promise<PreguntaTriviaParticipanteDto[]> {
  const { data } = await apiClient.get<PreguntaTriviaParticipanteDto[]>(
    `/sesiones/${sesionId}/trivia/preguntas`,
  )
  return data
}

export async function obtenerEstadoTriviaSesion(
  sesionId: string,
): Promise<EstadoTriviaSesionDto> {
  const { data } = await apiClient.get<EstadoTriviaSesionDto>(
    `/sesiones/${sesionId}/trivia/estado`,
  )
  return data
}

export async function submitRespuestaTrivia(
  sesionId: string,
  body: SubmitRespuestaTriviaRequest,
): Promise<TriviaRespuestaFeedback> {
  const response = await apiClient.post<SubmitRespuestaTriviaResponse>(
    `/sesiones/${sesionId}/trivia/respuestas`,
    body,
  )

  if (response.status === 202 || response.data.status === 'Accepted') {
    return pollTriviaFeedback(sesionId)
  }

  return response.data as unknown as TriviaRespuestaFeedback
}

async function pollTriviaFeedback(
  sesionId: string,
  maxAttempts = 25,
): Promise<TriviaRespuestaFeedback> {
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    await new Promise((resolve) => setTimeout(resolve, 150))
    const estado = await obtenerEstadoTriviaSesion(sesionId)
    if (
      estado.yaRespondio &&
      estado.ultimaRespuestaPuntos != null &&
      estado.puntajeTotalParticipante != null
    ) {
      return {
        esCorrecta: estado.ultimaRespuestaEsCorrecta ?? false,
        fueraDeTiempo: estado.ultimaRespuestaFueraDeTiempo ?? false,
        puntosOtorgados: estado.ultimaRespuestaPuntos,
        puntajeTotal: estado.puntajeTotalParticipante,
      }
    }
  }

  throw new Error('Tiempo de espera agotado al procesar la respuesta trivia.')
}

export async function lanzarPreguntaTrivia(
  sesionId: string,
  body?: { duracionSegundos?: number },
): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/trivia/lanzar`, body ?? {})
}

export async function cerrarPreguntaTrivia(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/trivia/cerrar`)
}

export async function aplicarPenalizacion(
  sesionId: string,
  body: AplicarPenalizacionRequest,
): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/penalizaciones`, body)
}

export async function liberarPistaManual(
  sesionId: string,
  body: LiberarPistaManualRequest,
): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/pistas-manuales`, body)
}

export async function enviarEvidencia(
  sesionId: string,
  body: SubmitEvidenciaRequest,
): Promise<SubmitEvidenciaResponse> {
  const { data } = await apiClient.post<SubmitEvidenciaResponse>(
    `/sesiones/${sesionId}/evidencias`,
    body,
  )
  return data
}

export async function obtenerHistorialSesion(
  sesionId: string,
  pagina = 1,
  tamanoPagina = 50,
): Promise<HistorialSesionDto> {
  const { data } = await apiClient.get<HistorialSesionDto>(`/sesiones/${sesionId}/historial`, {
    params: { pagina, tamanoPagina },
  })
  return data
}

export async function obtenerReporteFinalSesion(sesionId: string): Promise<ReporteFinalSesionDto> {
  const { data } = await apiClient.get<ReporteFinalSesionDto>(`/sesiones/${sesionId}/reporte-final`)
  return {
    ...data,
    ranking: ordenarYNumerarRanking(data.ranking ?? []),
  }
}
