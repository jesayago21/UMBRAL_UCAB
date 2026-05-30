import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  actualizarMision,
  crearMision,
  desactivarMision,
  getMision,
  listMisiones,
} from '@/services/misionService'
import type {
  ActualizarMisionRequest,
  CrearMisionRequest,
  ListMisionesParams,
} from '@/types/mision.types'

export const MISIONES_KEY = ['misiones'] as const

export function useMisiones(params: ListMisionesParams) {
  return useQuery({
    queryKey: [...MISIONES_KEY, params],
    queryFn: () => listMisiones(params),
  })
}

export function useMision(id: string | undefined) {
  return useQuery({
    queryKey: [...MISIONES_KEY, id],
    queryFn: () => getMision(id!),
    enabled: !!id,
  })
}

export function useCrearMision() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearMisionRequest) => crearMision(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MISIONES_KEY })
    },
  })
}

export function useActualizarMision() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ActualizarMisionRequest }) =>
      actualizarMision(id, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MISIONES_KEY })
    },
  })
}

export function useDesactivarMision() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => desactivarMision(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MISIONES_KEY })
    },
  })
}
