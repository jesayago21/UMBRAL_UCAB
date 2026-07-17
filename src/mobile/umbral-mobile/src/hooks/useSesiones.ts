import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  abandonarSesion,
  enviarEvidencia,
  getMiInscripcionParticipante,
  listSesionesDisponibles,
  obtenerEstadoTriviaSesion,
  obtenerRankingSesion,
  submitRespuestaTrivia,
  unirseSesion,
} from '@/services/sesionService'
import type {
  SubmitEvidenciaRequest,
  SubmitRespuestaTriviaRequest,
  UnirseSesionRequest,
} from '@/types/sesion.types'

export const SESIONES_DISPONIBLES_KEY = ['sesiones', 'disponibles'] as const
export const MI_INSCRIPCION_PARTICIPANTE_KEY = ['sesiones', 'mi-inscripcion'] as const
export const RANKING_KEY = ['sesiones', 'ranking'] as const
export const TRIVIA_ESTADO_KEY = ['sesiones', 'trivia', 'estado'] as const

export function useSesionesDisponibles() {
  return useQuery({
    queryKey: SESIONES_DISPONIBLES_KEY,
    queryFn: listSesionesDisponibles,
    // Lobby en vivo vía polling (no hay hub de “lista disponibles” aún).
    refetchInterval: 3_000,
    refetchOnMount: 'always',
    refetchOnReconnect: true,
  })
}

export function useMiInscripcionParticipante() {
  return useQuery({
    queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY,
    queryFn: getMiInscripcionParticipante,
    refetchInterval: 5_000,
    refetchOnMount: 'always',
  })
}

export function useRankingSesion(
  sesionId: string,
  enabled: boolean,
  opts?: { refetchIntervalMs?: number | false },
) {
  return useQuery({
    queryKey: [...RANKING_KEY, sesionId],
    queryFn: () => obtenerRankingSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
    refetchInterval: opts?.refetchIntervalMs ?? false,
  })
}

export function useUnirseSesion(sesionId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: UnirseSesionRequest) => unirseSesion(sesionId, body),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      await qc.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
    },
  })
}

export function useAbandonarSesion(sesionId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => abandonarSesion(sesionId),
    onSuccess: async () => {
      qc.setQueryData(MI_INSCRIPCION_PARTICIPANTE_KEY, null)
      await qc.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      await qc.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
    },
  })
}

export function useEnviarEvidencia(sesionId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: SubmitEvidenciaRequest) => enviarEvidencia(sesionId, body),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
      await qc.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    },
  })
}

export function useEstadoTriviaSesion(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...TRIVIA_ESTADO_KEY, sesionId],
    queryFn: () => obtenerEstadoTriviaSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
    refetchInterval: 1_000,
  })
}

export function useSubmitRespuestaTrivia(sesionId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: SubmitRespuestaTriviaRequest) => submitRespuestaTrivia(sesionId, body),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      await qc.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
    },
  })
}
