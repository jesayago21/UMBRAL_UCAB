import { apiClient } from '@/services/apiClient'
import type {
  MiInscripcionParticipanteDto,
  PosicionRankingDto,
  SesionDisponibleParticipanteDto,
  EstadoTriviaSesionDto,
  SubmitEvidenciaRequest,
  SubmitEvidenciaResponse,
  SubmitRespuestaTriviaRequest,
  SubmitRespuestaTriviaResponse,
  UnirseSesionRequest,
  UnirseSesionResponse,
} from '@/types/sesion.types'

export async function listSesionesDisponibles(): Promise<SesionDisponibleParticipanteDto[]> {
  const { data } = await apiClient.get<SesionDisponibleParticipanteDto[]>('/sesiones/disponibles')
  return data
}

export async function getMiInscripcionParticipante(): Promise<MiInscripcionParticipanteDto | null> {
  const { status, data } = await apiClient.get<MiInscripcionParticipanteDto>(
    '/sesiones/mi-inscripcion',
    { validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204) return null
  // Respuestas vacías / basura no cuentan como inscripción (evita fantasma en cache).
  if (!data || typeof data !== 'object' || !('sesionId' in data) || !data.sesionId) {
    return null
  }
  return data
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

export async function abandonarSesion(sesionId: string): Promise<void> {
  await apiClient.post(`/sesiones/${sesionId}/abandonar`)
}

export async function obtenerRankingSesion(sesionId: string): Promise<PosicionRankingDto[]> {
  const { data } = await apiClient.get<PosicionRankingDto[]>(`/sesiones/${sesionId}/ranking`)
  return data
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
): Promise<SubmitRespuestaTriviaResponse> {
  const { data } = await apiClient.post<SubmitRespuestaTriviaResponse>(
    `/sesiones/${sesionId}/trivia/respuestas`,
    body,
  )
  return data
}
