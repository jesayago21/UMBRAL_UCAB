import { apiClient } from '@/services/apiClient'
import type { CreatedIdResponse } from '@/types/api.types'
import type {
  ActualizarMisionRequest,
  CrearMisionRequest,
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

export async function desactivarMision(id: string): Promise<void> {
  await apiClient.delete(`/misiones/${id}`)
}
