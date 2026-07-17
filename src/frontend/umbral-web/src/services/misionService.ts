import { apiClient } from '@/services/apiClient'
import type { CreatedIdResponse } from '@/types/api.types'
import type {
  ActualizarMisionRequest,
  AgregarEtapaMisionRequest,
  AgregarPistaEtapaRequest,
  CrearMisionRequest,
  EditarEtapaMisionRequest,
  EditarPistaEtapaRequest,
  ListMisionesParams,
  MisionDto,
} from '@/types/mision.types'

export async function listMisiones(params: ListMisionesParams = {}): Promise<MisionDto[]> {
  const { data } = await apiClient.get<MisionDto[]>('/misiones', { params })
  return data
}

export async function getMision(id: string): Promise<MisionDto> {
  const { data } = await apiClient.get<MisionDto>(`/misiones/${id}`)
  return data
}

export async function crearMision(body: CrearMisionRequest): Promise<string> {
  const { data, headers } = await apiClient.post<CreatedIdResponse>('/misiones', body)
  if (data?.id) return data.id
  const location = headers.location as string | undefined
  if (location) return location.split('/').pop() ?? ''
  throw new Error('La API no devolvió el id de la misión creada.')
}

export async function actualizarMision(
  id: string,
  body: ActualizarMisionRequest,
): Promise<void> {
  await apiClient.put(`/misiones/${id}`, body)
}

export async function eliminarMision(id: string): Promise<void> {
  await apiClient.delete(`/misiones/${id}`)
}

export async function agregarEtapaMision(
  misionId: string,
  body: AgregarEtapaMisionRequest,
): Promise<string> {
  const { data } = await apiClient.post<CreatedIdResponse>(`/misiones/${misionId}/etapas`, body)
  return data.id
}

export async function editarEtapaMision(
  misionId: string,
  etapaId: string,
  body: EditarEtapaMisionRequest,
): Promise<void> {
  await apiClient.put(`/misiones/${misionId}/etapas/${etapaId}`, body)
}

export async function eliminarEtapaMision(misionId: string, etapaId: string): Promise<void> {
  await apiClient.delete(`/misiones/${misionId}/etapas/${etapaId}`)
}

export async function agregarPistaEtapa(
  misionId: string,
  etapaId: string,
  body: AgregarPistaEtapaRequest,
): Promise<void> {
  await apiClient.post(`/misiones/${misionId}/etapas/${etapaId}/pistas`, body)
}

export async function editarPistaEtapa(
  misionId: string,
  etapaId: string,
  pistaId: string,
  body: EditarPistaEtapaRequest,
): Promise<void> {
  await apiClient.put(`/misiones/${misionId}/etapas/${etapaId}/pistas/${pistaId}`, body)
}

export async function eliminarPistaEtapa(
  misionId: string,
  etapaId: string,
  pistaId: string,
): Promise<void> {
  await apiClient.delete(`/misiones/${misionId}/etapas/${etapaId}/pistas/${pistaId}`)
}
