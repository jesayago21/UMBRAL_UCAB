import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  actualizarCategoria,
  crearCategoria,
  eliminarCategoria,
  listCategorias,
} from '@/services/categoriaService'
import type {
  ActualizarCategoriaRequest,
  CrearCategoriaRequest,
  ListCategoriasParams,
} from '@/types/trivia.types'

export const CATEGORIAS_KEY = ['categorias'] as const

export function useCategorias(params: ListCategoriasParams = {}) {
  return useQuery({
    queryKey: [...CATEGORIAS_KEY, params],
    queryFn: () => listCategorias(params),
  })
}

export function useCrearCategoria() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearCategoriaRequest) => crearCategoria(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: CATEGORIAS_KEY })
    },
  })
}

export function useActualizarCategoria() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ActualizarCategoriaRequest }) =>
      actualizarCategoria(id, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: CATEGORIAS_KEY })
    },
  })
}

export function useEliminarCategoria() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => eliminarCategoria(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: CATEGORIAS_KEY })
    },
  })
}
