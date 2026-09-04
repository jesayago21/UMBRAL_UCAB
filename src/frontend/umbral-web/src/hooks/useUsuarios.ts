import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  actualizarUsuario,
  asignarRolesUsuario,
  cambiarEstadoUsuario,
  crearUsuario,
  eliminarUsuario,
  listUsuarios,
} from '@/services/usuarioService'
import type {
  ActualizarUsuarioRequest,
  AsignarRolesRequest,
  CrearUsuarioRequest,
} from '@/types/usuario.types'

const QUERY_KEY = ['usuarios'] as const

export function useUsuarios(page = 1, pageSize = 50) {
  return useQuery({
    queryKey: [...QUERY_KEY, page, pageSize],
    queryFn: () => listUsuarios(page, pageSize),
  })
}

export function useCrearUsuario() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearUsuarioRequest) => crearUsuario(body),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useActualizarUsuario() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      keycloakUserId,
      body,
    }: {
      keycloakUserId: string
      body: ActualizarUsuarioRequest
    }) => actualizarUsuario(keycloakUserId, body),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useAsignarRolesUsuario() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      keycloakUserId,
      body,
    }: {
      keycloakUserId: string
      body: AsignarRolesRequest
    }) => asignarRolesUsuario(keycloakUserId, body),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useCambiarEstadoUsuario() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      keycloakUserId,
      accion,
    }: {
      keycloakUserId: string
      accion: 'Activar' | 'Bloquear'
    }) => cambiarEstadoUsuario(keycloakUserId, accion),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}

export function useEliminarUsuario() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (keycloakUserId: string) => eliminarUsuario(keycloakUserId),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  })
}
