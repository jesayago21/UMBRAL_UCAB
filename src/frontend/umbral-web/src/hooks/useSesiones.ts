import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  abandonarSesion,
  abrirInscripcionSesion,
  cancelarSesion,
  crearSesionBusquedaTesoro,
  crearSesionMision,
  crearSesionTrivia,
  getMiInscripcionParticipante,
  listSesionesDisponibles,
  finalizarSesion,
  iniciarSesion,
  listMisionesActivas,
  listSesionesDisponiblesTrivia,
  listSesionesOperativas,
  listPreguntasTriviaSesionParticipante,
  obtenerRankingSesion,
  obtenerSesionDetalle,
  pausarSesion,
  reanudarSesion,
  unirseSesion,
} from '@/services/sesionService'
import type {
  CancelarSesionRequest,
  CrearSesionBusquedaTesoroRequest,
  CrearSesionTriviaRequest,
  UnirseSesionRequest,
} from '@/types/sesion.types'

export const MISIONES_ACTIVAS_KEY = ['misiones', 'activas'] as const
export const SESIONES_OPERATIVAS_KEY = ['sesiones', 'operativas'] as const
export const SESIONES_DISPONIBLES_KEY = ['sesiones', 'disponibles'] as const
export const MI_INSCRIPCION_PARTICIPANTE_KEY = ['sesiones', 'mi-inscripcion'] as const
export const SESIONES_DISPONIBLES_BT_KEY = ['sesiones', 'disponibles', 'bt'] as const
export const SESIONES_DISPONIBLES_TRIVIA_KEY = ['sesiones', 'disponibles', 'trivia'] as const
export const SESION_DETALLE_KEY = ['sesiones', 'detalle'] as const
export const RANKING_KEY = ['sesiones', 'ranking'] as const
export const TRIVIA_PREGUNTAS_EQUIPO_KEY = ['sesiones', 'trivia', 'preguntas'] as const

export function useSesionesOperativas() {
  return useQuery({
    queryKey: SESIONES_OPERATIVAS_KEY,
    queryFn: listSesionesOperativas,
    refetchInterval: 15_000,
  })
}

export function useSesionesDisponibles() {
  return useQuery({
    queryKey: SESIONES_DISPONIBLES_KEY,
    queryFn: listSesionesDisponibles,
    refetchInterval: 10_000,
  })
}

export function useMiInscripcionParticipante() {
  return useQuery({
    queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY,
    queryFn: getMiInscripcionParticipante,
    refetchInterval: 15_000,
  })
}

export function useAbandonarSesion(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => abandonarSesion(sesionId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_BT_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY })
    },
  })
}

export function useSesionesDisponiblesBusqueda() {
  return useSesionesDisponibles()
}

export function useSesionesDisponiblesTrivia() {
  return useQuery({
    queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY,
    queryFn: listSesionesDisponiblesTrivia,
    refetchInterval: 10_000,
  })
}

export function useSesionDetalle(sesionId: string) {
  return useQuery({
    queryKey: [...SESION_DETALLE_KEY, sesionId],
    queryFn: () => obtenerSesionDetalle(sesionId),
    enabled: Boolean(sesionId),
  })
}

export function useMisionesActivas() {
  return useQuery({
    queryKey: MISIONES_ACTIVAS_KEY,
    queryFn: listMisionesActivas,
  })
}

export function useCrearSesionMision() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: { misionId: string; nombreSesion: string }) => crearSesionMision(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
    },
  })
}

export function useCrearSesionBusquedaTesoro() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearSesionBusquedaTesoroRequest) => crearSesionBusquedaTesoro(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
    },
  })
}

export function useCrearSesionTrivia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CrearSesionTriviaRequest) => crearSesionTrivia(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
    },
  })
}

export function useAbrirInscripcionSesion(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => abrirInscripcionSesion(sesionId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_BT_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY })
    },
  })
}

export function useUnirseSesion(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UnirseSesionRequest) => unirseSesion(sesionId, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_BT_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY })
    },
  })
}

export function useIniciarSesion(sesionId: string) {
  return useMutation({ mutationFn: () => iniciarSesion(sesionId) })
}

export function usePausarSesion(sesionId: string) {
  return useMutation({ mutationFn: () => pausarSesion(sesionId) })
}

export function useReanudarSesion(sesionId: string) {
  return useMutation({ mutationFn: () => reanudarSesion(sesionId) })
}

export function useFinalizarSesion(sesionId: string) {
  return useMutation({ mutationFn: () => finalizarSesion(sesionId) })
}

export function useCancelarSesion(sesionId: string) {
  return useMutation({
    mutationFn: (body: CancelarSesionRequest) => cancelarSesion(sesionId, body),
  })
}

export function useRankingSesion(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...RANKING_KEY, sesionId],
    queryFn: () => obtenerRankingSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
  })
}

export function usePreguntasTriviaSesionParticipante(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...TRIVIA_PREGUNTAS_EQUIPO_KEY, sesionId],
    queryFn: () => listPreguntasTriviaSesionParticipante(sesionId),
    enabled: enabled && Boolean(sesionId),
  })
}
