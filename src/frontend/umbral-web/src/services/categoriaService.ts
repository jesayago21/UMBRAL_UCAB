import { apiClient } from '@/services/apiClient'
import type { CreatedIdResponse } from '@/types/api.types'
import type {
  ActualizarCategoriaRequest,
  CategoriaDto,
  CrearCategoriaRequest,
  ListCategoriasParams,
} from '@/types/trivia.types'

export async function listCategorias(
  params: ListCategoriasParams = {},
): Promise<CategoriaDto[]> {
  const { data } = await apiClient.get<CategoriaDto[]>('/categorias', { params })
  return data
}

export async function getCategoria(id: string): Promise<CategoriaDto> {
  const { data } = await apiClient.get<CategoriaDto>(`/categorias/${id}`)
  return data
}

export async function crearCategoria(body: CrearCategoriaRequest): Promise<string> {
  const { data, headers } = await apiClient.post<CreatedIdResponse>('/categorias', body)
  if (data?.id) return data.id
  const location = headers.location as string | undefined
  if (location) return location.split('/').pop() ?? ''
  throw new Error('La API no devolvió el id de la categoría creada.')
}

export async function actualizarCategoria(
  id: string,
  body: ActualizarCategoriaRequest,
): Promise<void> {
  await apiClient.put(`/categorias/${id}`, body)
}

export async function eliminarCategoria(id: string): Promise<void> {
  await apiClient.delete(`/categorias/${id}`)
}
