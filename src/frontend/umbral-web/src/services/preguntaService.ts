import { apiClient } from '@/services/apiClient'
import type { CreatedIdResponse } from '@/types/api.types'
import type {
  ActualizarPreguntaRequest,
  CrearPreguntaRequest,
  ListPreguntasParams,
  PreguntaDto,
} from '@/types/trivia.types'

export async function listPreguntas(
  params: ListPreguntasParams = {},
): Promise<PreguntaDto[]> {
  const { data } = await apiClient.get<PreguntaDto[]>('/preguntas', { params })
  return data
}

export async function getPregunta(id: string): Promise<PreguntaDto> {
  const { data } = await apiClient.get<PreguntaDto>(`/preguntas/${id}`)
  return data
}

export async function crearPregunta(body: CrearPreguntaRequest): Promise<string> {
  const { data, headers } = await apiClient.post<CreatedIdResponse>('/preguntas', body)
  if (data?.id) return data.id
  const location = headers.location as string | undefined
  if (location) return location.split('/').pop() ?? ''
  throw new Error('La API no devolvió el id de la pregunta creada.')
}

export async function actualizarPregunta(
  id: string,
  body: ActualizarPreguntaRequest,
): Promise<void> {
  await apiClient.put(`/preguntas/${id}`, body)
}

export async function eliminarPregunta(id: string): Promise<void> {
  await apiClient.delete(`/preguntas/${id}`)
}
