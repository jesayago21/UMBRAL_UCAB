import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  abandonarSesion,
  abrirInscripcionSesion,
  aplicarPenalizacion,
  liberarPistaManual,
  cancelarSesion,
  crearSesionBusquedaTesoro,
  crearSesionMision,
  crearSesionTrivia,
  enviarEvidencia,
  expulsarParticipante,
  getMiInscripcionParticipante,
  listSesionesDisponibles,
  finalizarSesion,
  iniciarSesion,
  listMisionesActivas,
  listSesionesDisponiblesTrivia,
  listSesionesOperativas,
  listPreguntasTriviaSesionParticipante,
  obtenerEstadoTriviaSesion,
  lanzarPreguntaTrivia,
  cerrarPreguntaTrivia,
  obtenerHistorialSesion,
  obtenerRankingSesion,
  obtenerReporteFinalSesion,
  obtenerSesionDetalle,
  pausarSesion,
  reanudarSesion,
  submitRespuestaTrivia,
  unirseSesion,
} from '@/services/sesionService'
import type {
  AplicarPenalizacionRequest,
  LiberarPistaManualRequest,
  CancelarSesionRequest,
  CrearSesionBusquedaTesoroRequest,
  CrearSesionTriviaRequest,
  SubmitEvidenciaRequest,
  SubmitRespuestaTriviaRequest,
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
export const HISTORIAL_KEY = ['sesiones', 'historial'] as const
export const REPORTE_FINAL_KEY = ['sesiones', 'reporte-final'] as const
export const TRIVIA_PREGUNTAS_EQUIPO_KEY = ['sesiones', 'trivia', 'preguntas'] as const
export const TRIVIA_ESTADO_KEY = ['sesiones', 'trivia', 'estado'] as const

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
    // Red de seguridad si SignalR no conecta (JWT largo / WS). Con hub OK el setQueryData es inmediato.
    refetchInterval: 5_000,
  })
}

export function useAbandonarSesion(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => abandonarSesion(sesionId),
    onSuccess: () => {
      queryClient.setQueryData(MI_INSCRIPCION_PARTICIPANTE_KEY, null)
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_BT_KEY })
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY })
    },
  })
}

export function useExpulsarParticipante(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: { participanteId: string; motivo: string }) =>
      expulsarParticipante(sesionId, body.participanteId, { motivo: body.motivo }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
      await queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
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
    // Red de seguridad: lobby (participantes que se unen) sin depender solo de SignalR.
    refetchInterval: 3_000,
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

export function useRankingSesion(
  sesionId: string,
  enabled = true,
  options?: { refetchIntervalMs?: number | false },
) {
  return useQuery({
    queryKey: [...RANKING_KEY, sesionId],
    queryFn: () => obtenerRankingSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
    // Debe refrescar al instante vía SignalR / invalidate (HU-17), no esperar staleTime global.
    staleTime: 0,
    // Si el hub no está conectado, poll para que el resto vea puntajes del ganador (BT).
    refetchInterval: options?.refetchIntervalMs ?? false,
  })
}

export function usePreguntasTriviaSesionParticipante(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...TRIVIA_PREGUNTAS_EQUIPO_KEY, sesionId],
    queryFn: () => listPreguntasTriviaSesionParticipante(sesionId),
    enabled: enabled && Boolean(sesionId),
  })
}

export function useEstadoTriviaSesion(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...TRIVIA_ESTADO_KEY, sesionId],
    queryFn: () => obtenerEstadoTriviaSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
    refetchInterval: (query) => {
      const fase = query.state.data?.fase
      if (fase === 'PreguntaActiva' || fase === 'Transicion') return 1_000
      return 5_000
    },
  })
}

export function useLanzarPreguntaTrivia(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body?: { duracionSegundos?: number }) => lanzarPreguntaTrivia(sesionId, body),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

export function useCerrarPreguntaTrivia(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => cerrarPreguntaTrivia(sesionId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

export function useAplicarPenalizacion(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: AplicarPenalizacionRequest) => aplicarPenalizacion(sesionId, body),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: RANKING_KEY })
      await queryClient.refetchQueries({ queryKey: [...RANKING_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

export function useLiberarPistaManual(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: LiberarPistaManualRequest) => liberarPistaManual(sesionId, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

export function useEnviarEvidencia(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: SubmitEvidenciaRequest) => enviarEvidencia(sesionId, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

/** HU-34 — confirmar opción de trivia. */
export function useSubmitRespuestaTrivia(sesionId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: SubmitRespuestaTriviaRequest) => submitRespuestaTrivia(sesionId, body),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    },
  })
}

export function useHistorialSesion(sesionId: string, pagina = 1, enabled = true) {
  return useQuery({
    queryKey: [...HISTORIAL_KEY, sesionId, pagina],
    queryFn: () => obtenerHistorialSesion(sesionId, pagina),
    enabled: enabled && Boolean(sesionId),
  })
}

export function useReporteFinalSesion(sesionId: string, enabled = true) {
  return useQuery({
    queryKey: [...REPORTE_FINAL_KEY, sesionId],
    queryFn: () => obtenerReporteFinalSesion(sesionId),
    enabled: enabled && Boolean(sesionId),
  })
}
