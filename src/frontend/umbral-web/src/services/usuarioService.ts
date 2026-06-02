import { apiClient } from '@/services/apiClient'
import type { AsignarRolesRequest, CrearUsuarioRequest, UsuarioDto } from '@/types/usuario.types'

export async function listUsuarios(page = 1, pageSize = 50): Promise<UsuarioDto[]> {
  const { data } = await apiClient.get<UsuarioDto[]>('/usuarios', { params: { page, pageSize } })
  return data
}

export async function crearUsuario(body: CrearUsuarioRequest): Promise<UsuarioDto> {
  const { data } = await apiClient.post<UsuarioDto>('/usuarios', body)
  return data
}

export async function asignarRolesUsuario(id: string, body: AsignarRolesRequest): Promise<void> {
  await apiClient.put(`/usuarios/${id}/roles`, body)
}

export async function cambiarEstadoUsuario(id: string, accion: 'Activar' | 'Bloquear'): Promise<void> {
  await apiClient.put(`/usuarios/${id}/estado`, { accion })
}
