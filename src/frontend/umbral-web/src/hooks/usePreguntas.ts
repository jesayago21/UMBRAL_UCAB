import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  actualizarPregunta,
  crearPregunta,
  eliminarPregunta,
  listPreguntas,
} from '@/services/preguntaService'
import type {
  ActualizarPreguntaRequest,
  CrearPreguntaRequest,
  ListPreguntasParams,
} from '@/types/trivia.types'

export const PREGUNTAS_KEY = ['preguntas'] as const

export function usePreguntas(params: ListPreguntasParams = {}) {
  return useQuery({
    queryKey: [...PREGUNTAS_KEY, params],
    queryFn: () => listPreguntas(params),
  })
}

export function useCrearPregunta() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearPreguntaRequest) => crearPregunta(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: PREGUNTAS_KEY })
    },
  })
}

export function useActualizarPregunta() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ActualizarPreguntaRequest }) =>
      actualizarPregunta(id, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: PREGUNTAS_KEY })
    },
  })
}

export function useEliminarPregunta() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => eliminarPregunta(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: PREGUNTAS_KEY })
    },
  })
}
