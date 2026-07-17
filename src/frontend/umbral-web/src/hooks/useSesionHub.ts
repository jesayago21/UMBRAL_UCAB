import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useQueryClient } from '@tanstack/react-query'
import type { HubConnection } from '@microsoft/signalr'
import { crearSesionHub } from '@/services/sesionHubService'
import { useAuthStore } from '@/store/authStore'
import {
  HISTORIAL_KEY,
  MI_INSCRIPCION_PARTICIPANTE_KEY,
  RANKING_KEY,
  SESIONES_OPERATIVAS_KEY,
  SESION_DETALLE_KEY,
  TRIVIA_ESTADO_KEY,
} from '@/hooks/useSesiones'
import type {
  EstadoTriviaSesionDto,
  MiInscripcionParticipanteDto,
  PosicionRankingDto,
  SesionDetalleDto,
} from '@/types/sesion.types'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import { resolveSignalRBaseUrl } from '@/lib/apiUrls'

export type SesionHubConnectionStatus =
  | 'desconectado'
  | 'conectando'
  | 'conectado'
  | 'reconectando'

export interface SesionEstadoPayload {
  sesionId: string
  nuevoEstado: string
}

export interface PenalizacionAplicadaPayload {
  sesionId: string
  participanteId: string
  puntos: number
  motivo: string
}

export interface ParticipanteExpulsadoPayload {
  sesionId: string
  participanteId: string
  motivo: string
}

function normalizarRankingPayload(
  raw: unknown,
  sesionIdEsperado: string,
): PosicionRankingDto[] | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const id = String(o.sesionId ?? o.SesionId ?? '')
  if (id && !mismoSesionId(id, sesionIdEsperado)) return null

  const lista = (o.ranking ?? o.Ranking) as unknown
  if (!Array.isArray(lista)) return null

  return ordenarYNumerarRanking(
    lista
      .map((item) => {
        const row = (item ?? {}) as Record<string, unknown>
        const participanteId = String(row.participanteId ?? row.ParticipanteId ?? '')
        if (!participanteId) return null
        return {
          // La posición del wire puede venir mal; se renumeran abajo.
          posicion: Number(row.posicion ?? row.Posicion ?? 0),
          participanteId,
          nombreParticipante: String(row.nombreParticipante ?? row.NombreParticipante ?? ''),
          puntajeTotal: Number(row.puntajeTotal ?? row.PuntajeTotal ?? 0),
          tiempoAcumuladoMs: Number(row.tiempoAcumuladoMs ?? row.TiempoAcumuladoMs ?? 0),
        } satisfies PosicionRankingDto
      })
      .filter((r): r is PosicionRankingDto => r != null),
  )
}

type RolHub = 'operador' | 'participante'

interface UseSesionHubOptions {
  sesionId: string
  rol: RolHub
  participanteId?: string
  enabled?: boolean
  /** Solo aplicable al participante afectado (HU-16 motivo en vivo). */
  onPenalizacionAplicada?: (payload: PenalizacionAplicadaPayload) => void
  /** HU-32 — expulsión de sala; solo el participante afectado. */
  onParticipanteExpulsado?: (payload: ParticipanteExpulsadoPayload) => void
}

function normalizarPayloadEstado(raw: unknown): SesionEstadoPayload | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const sesionId = String(o.sesionId ?? o.SesionId ?? '')
  const nuevoEstado = String(o.nuevoEstado ?? o.NuevoEstado ?? '')
  if (!sesionId || !nuevoEstado) return null
  return { sesionId, nuevoEstado }
}

function normalizarPenalizacion(raw: unknown): PenalizacionAplicadaPayload | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const sesionId = String(o.sesionId ?? o.SesionId ?? '')
  const participanteId = String(o.participanteId ?? o.ParticipanteId ?? '')
  const puntos = Number(o.puntos ?? o.Puntos ?? 0)
  const motivo = String(o.motivo ?? o.Motivo ?? '').trim()
  if (!sesionId || !participanteId || !Number.isFinite(puntos) || puntos <= 0) return null
  return { sesionId, participanteId, puntos, motivo: motivo || 'Sin motivo indicado' }
}

function normalizarExpulsado(raw: unknown): ParticipanteExpulsadoPayload | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const sesionId = String(o.sesionId ?? o.SesionId ?? '')
  const participanteId = String(o.participanteId ?? o.ParticipanteId ?? '')
  const motivo = String(o.motivo ?? o.Motivo ?? '').trim()
  if (!sesionId || !participanteId) return null
  return { sesionId, participanteId, motivo: motivo || 'Sin motivo indicado' }
}

function mismoSesionId(a: string, b: string): boolean {
  return a.toLowerCase() === b.toLowerCase()
}

function leerToken(oidcAccessToken: string | undefined): string | null {
  return oidcAccessToken ?? useAuthStore.getState().token
}

export function useSesionHub({
  sesionId,
  rol,
  participanteId,
  enabled = true,
  onPenalizacionAplicada,
  onParticipanteExpulsado,
}: UseSesionHubOptions): SesionHubConnectionStatus {
  const auth = useAuth()
  const oidcToken = auth.user?.access_token
  const storeToken = useAuthStore((s) => s.token)
  const token = oidcToken ?? storeToken
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<SesionHubConnectionStatus>('desconectado')

  useEffect(() => {
    if (!enabled || !token || !sesionId) {
      setStatus('desconectado')
      return
    }
    if (rol === 'participante' && !participanteId) {
      setStatus('desconectado')
      return
    }

    let cancelled = false
    const connection: HubConnection = crearSesionHub(() => leerToken(auth.user?.access_token))
    setStatus('conectando')

    const unirse = async () => {
      if (rol === 'operador') {
        await connection.invoke('UnirseComoOperador', sesionId)
      } else {
        await connection.invoke('UnirseASesion', sesionId, participanteId)
      }
    }

    const refetchTablero = () => {
      void queryClient.refetchQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.refetchQueries({ queryKey: [...RANKING_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
    }

    const onEstado = (raw: unknown) => {
      const payload = normalizarPayloadEstado(raw)
      if (!payload) return
      if (!mismoSesionId(payload.sesionId, sesionId)) return

      queryClient.setQueryData<MiInscripcionParticipanteDto | null>(
        MI_INSCRIPCION_PARTICIPANTE_KEY,
        (prev) =>
          prev && mismoSesionId(prev.sesionId, sesionId)
            ? { ...prev, estado: payload.nuevoEstado }
            : prev,
      )
      queryClient.setQueryData<SesionDetalleDto>(
        [...SESION_DETALLE_KEY, sesionId],
        (prev) => (prev ? { ...prev, estado: payload.nuevoEstado } : prev),
      )

      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
      void queryClient.refetchQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })

      if (payload.nuevoEstado === 'Finalizada' || payload.nuevoEstado === 'Cancelada') {
        queryClient.removeQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      }
    }

    const onPista = () => {
      void queryClient.refetchQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
    }

    const onParticipantes = (raw: unknown) => {
      if (raw && typeof raw === 'object') {
        const o = raw as Record<string, unknown>
        const id = String(o.sesionId ?? o.SesionId ?? '')
        if (id && !mismoSesionId(id, sesionId)) return
      }
      // Participante ve la lista en el ranking; operador en detalle de sesión.
      void queryClient.refetchQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      void queryClient.refetchQueries({ queryKey: [...RANKING_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
    }

    const onEtapaAvanzada = (raw: unknown) => {
      if (raw && typeof raw === 'object') {
        const o = raw as Record<string, unknown>
        const id = String(o.sesionId ?? o.SesionId ?? '')
        if (id && !mismoSesionId(id, sesionId)) return
      }
      refetchTablero()
    }

    const onRankingActualizado = (raw: unknown) => {
      const ranking = normalizarRankingPayload(raw, sesionId)
      if (ranking && ranking.length > 0) {
        // HU-21: aplicar snapshot al instante (sin esperar GET /ranking).
        queryClient.setQueryData<PosicionRankingDto[]>([...RANKING_KEY, sesionId], ranking)
      }
      // Siempre reconciliar con REST: el ganador ya invalidaba por mutación; el resto
      // depende de este evento (o de EtapaAvanzada). Si el push falló el parse, GET salva.
      void queryClient.refetchQueries({ queryKey: [...RANKING_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...HISTORIAL_KEY, sesionId] })
      void queryClient.refetchQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    }

    const onPenalizacion = (raw: unknown) => {
      const payload = normalizarPenalizacion(raw)
      if (!payload) return
      if (!mismoSesionId(payload.sesionId, sesionId)) return
      if (
        participanteId &&
        payload.participanteId.toLowerCase() !== participanteId.toLowerCase()
      ) {
        return
      }
      onPenalizacionAplicada?.(payload)
      void queryClient.refetchQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
      void queryClient.refetchQueries({ queryKey: [...RANKING_KEY, sesionId] })
    }

    const onExpulsado = (raw: unknown) => {
      const payload = normalizarExpulsado(raw)
      if (!payload) return
      if (!mismoSesionId(payload.sesionId, sesionId)) return
      if (
        participanteId &&
        payload.participanteId.toLowerCase() !== participanteId.toLowerCase()
      ) {
        return
      }
      onParticipanteExpulsado?.(payload)
      void queryClient.setQueryData(MI_INSCRIPCION_PARTICIPANTE_KEY, null)
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    }

    const onPreguntaTrivia = (raw: unknown) => {
      if (raw && typeof raw === 'object') {
        const o = raw as Record<string, unknown>
        const id = String(o.sesionId ?? o.SesionId ?? '')
        if (id && !mismoSesionId(id, sesionId)) return

        const preguntaId = String(o.preguntaId ?? o.PreguntaId ?? '')
        const orden = Number(o.orden ?? o.Orden ?? 0)
        const enunciado = String(o.enunciado ?? o.Enunciado ?? '')
        const opcionesRaw = (o.opciones ?? o.Opciones) as unknown
        const opciones = Array.isArray(opcionesRaw)
          ? opcionesRaw.map((x) => String(x))
          : []
        const timerRaw = o.timerCerradoEnUtc ?? o.TimerCerradoEnUtc
        const timerCerradoEnUtc =
          timerRaw instanceof Date
            ? timerRaw.toISOString()
            : timerRaw
              ? String(timerRaw)
              : null
        const totalPreguntas = Number(o.totalPreguntas ?? o.TotalPreguntas ?? 0)

        if (preguntaId && enunciado) {
          queryClient.setQueryData<EstadoTriviaSesionDto>([...TRIVIA_ESTADO_KEY, sesionId], {
            fase: 'PreguntaActiva',
            orden: Number.isFinite(orden) ? orden : null,
            preguntaId,
            enunciado,
            dificultad: null,
            opciones,
            timerCerradoEnUtc,
            transicionHastaUtc: null,
            preguntaIndexActual: Number.isFinite(orden) && orden > 0 ? orden - 1 : 0,
            totalPreguntas: Number.isFinite(totalPreguntas) ? totalPreguntas : 0,
            yaRespondio: false,
            indiceOpcionSeleccionada: null,
          })
          queryClient.setQueryData<SesionDetalleDto>([...SESION_DETALLE_KEY, sesionId], (prev) =>
            prev ? { ...prev, triviaFase: 'PreguntaActiva' } : prev,
          )
        }
      }
      void queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
    }

    const onTriviaTransicion = (raw: unknown) => {
      if (raw && typeof raw === 'object') {
        const o = raw as Record<string, unknown>
        const id = String(o.sesionId ?? o.SesionId ?? '')
        if (id && !mismoSesionId(id, sesionId)) return

        const ordenCerrada = Number(o.ordenPreguntaCerrada ?? o.OrdenPreguntaCerrada ?? 0)
        const totalPreguntas = Number(o.totalPreguntas ?? o.TotalPreguntas ?? 0)
        const secuenciaTerminada =
          Number.isFinite(ordenCerrada) &&
          Number.isFinite(totalPreguntas) &&
          totalPreguntas > 0 &&
          ordenCerrada >= totalPreguntas
        queryClient.setQueryData<EstadoTriviaSesionDto>([...TRIVIA_ESTADO_KEY, sesionId], (prev) => ({
          fase: secuenciaTerminada ? 'Esperando' : 'Transicion',
          orden: null,
          preguntaId: null,
          enunciado: null,
          dificultad: null,
          opciones: null,
          timerCerradoEnUtc: null,
          transicionHastaUtc: prev?.transicionHastaUtc ?? null,
          preguntaIndexActual:
            Number.isFinite(ordenCerrada) && ordenCerrada > 0
              ? ordenCerrada - 1
              : (prev?.preguntaIndexActual ?? 0),
          totalPreguntas: Number.isFinite(totalPreguntas)
            ? totalPreguntas
            : (prev?.totalPreguntas ?? 0),
          yaRespondio: false,
          indiceOpcionSeleccionada: null,
        }))
        queryClient.setQueryData<SesionDetalleDto>([...SESION_DETALLE_KEY, sesionId], (prev) =>
          prev
            ? {
                ...prev,
                triviaFase: secuenciaTerminada ? 'Esperando' : 'Transicion',
              }
            : prev,
        )
      }
      void queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      void queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
    }

    connection.on('SesionEstadoCambiado', onEstado)
    connection.on('sesionEstadoCambiado', onEstado)
    connection.on('ParticipantesActualizados', onParticipantes)
    connection.on('participantesActualizados', onParticipantes)
    connection.on('PistaLiberada', onPista)
    connection.on('pistaLiberada', onPista)
    connection.on('EtapaAvanzada', onEtapaAvanzada)
    connection.on('etapaAvanzada', onEtapaAvanzada)
    connection.on('RankingActualizado', onRankingActualizado)
    connection.on('rankingActualizado', onRankingActualizado)
    connection.on('PenalizacionAplicada', onPenalizacion)
    connection.on('penalizacionAplicada', onPenalizacion)
    connection.on('ParticipanteExpulsado', onExpulsado)
    connection.on('participanteExpulsado', onExpulsado)
    connection.on('PreguntaTriviaIniciada', onPreguntaTrivia)
    connection.on('preguntaTriviaIniciada', onPreguntaTrivia)
    connection.on('TriviaEnTransicion', onTriviaTransicion)
    connection.on('triviaEnTransicion', onTriviaTransicion)

    connection.onreconnecting(() => {
      if (!cancelled) setStatus('reconectando')
    })

    connection.onreconnected(() => {
      void unirse()
        .then(() => {
          if (!cancelled) setStatus('conectado')
          refetchTablero()
        })
        .catch((err: unknown) => {
          console.error('[SesionHub] re-join falló', err)
          if (!cancelled) setStatus('reconectando')
        })
    })

    connection.onclose((err) => {
      if (err) console.error('[SesionHub] conexión cerrada', err)
      if (!cancelled) setStatus('desconectado')
    })

    void connection
      .start()
      .then(unirse)
      .then(() => {
        if (!cancelled) setStatus('conectado')
      })
      .catch((err: unknown) => {
        console.error(
          '[SesionHub] no se pudo conectar a',
          `${resolveSignalRBaseUrl()}/hubs/sesion`,
          err,
        )
        if (!cancelled) setStatus('desconectado')
      })

    return () => {
      cancelled = true
      connection.off('SesionEstadoCambiado', onEstado)
      connection.off('sesionEstadoCambiado', onEstado)
      connection.off('ParticipantesActualizados', onParticipantes)
      connection.off('participantesActualizados', onParticipantes)
      connection.off('PistaLiberada', onPista)
      connection.off('pistaLiberada', onPista)
      connection.off('EtapaAvanzada', onEtapaAvanzada)
      connection.off('etapaAvanzada', onEtapaAvanzada)
      connection.off('RankingActualizado', onRankingActualizado)
      connection.off('rankingActualizado', onRankingActualizado)
      connection.off('PenalizacionAplicada', onPenalizacion)
      connection.off('penalizacionAplicada', onPenalizacion)
      connection.off('ParticipanteExpulsado', onExpulsado)
      connection.off('participanteExpulsado', onExpulsado)
      connection.off('PreguntaTriviaIniciada', onPreguntaTrivia)
      connection.off('preguntaTriviaIniciada', onPreguntaTrivia)
      connection.off('TriviaEnTransicion', onTriviaTransicion)
      connection.off('triviaEnTransicion', onTriviaTransicion)
      void connection.stop()
    }
    // Callbacks: el padre debe estabilizar con ref/useEffectEvent si cambian a menudo
  }, [
    token,
    oidcToken,
    sesionId,
    rol,
    participanteId,
    enabled,
    queryClient,
    auth.user?.access_token,
    onPenalizacionAplicada,
    onParticipanteExpulsado,
  ])

  return status
}

export function etiquetaEstadoHub(status: SesionHubConnectionStatus): string | null {
  switch (status) {
    case 'conectando':
      return 'Conectando en tiempo real…'
    case 'reconectando':
      return 'Reconectando…'
    case 'desconectado':
      return 'Sin conexión en tiempo real'
    default:
      return null
  }
}
