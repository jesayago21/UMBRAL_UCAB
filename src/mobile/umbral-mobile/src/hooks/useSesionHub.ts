import { useEffect, useRef, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import type { HubConnection } from '@microsoft/signalr'
import {
  MI_INSCRIPCION_PARTICIPANTE_KEY,
  RANKING_KEY,
  TRIVIA_ESTADO_KEY,
} from '@/hooks/useSesiones'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import { crearSesionHub } from '@/services/sesionHubService'
import { useAuthStore } from '@/store/authStore'
import type { PosicionRankingDto } from '@/types/sesion.types'

export type SesionHubConnectionStatus =
  | 'desconectado'
  | 'conectando'
  | 'conectado'
  | 'reconectando'

export interface ParticipanteExpulsadoPayload {
  sesionId: string
  participanteId: string
  motivo: string
}

export interface PenalizacionAplicadaPayload {
  sesionId: string
  participanteId: string
  puntos: number
  motivo: string
}

function mismoSesionId(a: string, b: string): boolean {
  return a.toLowerCase() === b.toLowerCase()
}

function mismoParticipanteId(a: string, b: string): boolean {
  return a.toLowerCase() === b.toLowerCase()
}

function normalizarRanking(raw: unknown, sesionId: string): PosicionRankingDto[] | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const id = String(o.sesionId ?? o.SesionId ?? '')
  if (id && !mismoSesionId(id, sesionId)) return null
  const lista = (o.ranking ?? o.Ranking) as unknown
  if (!Array.isArray(lista)) return null
  return ordenarYNumerarRanking(
    lista
      .map((item): PosicionRankingDto | null => {
        const row = (item ?? {}) as Record<string, unknown>
        const participanteId = String(row.participanteId ?? row.ParticipanteId ?? '')
        if (!participanteId) return null
        return {
          posicion: Number(row.posicion ?? row.Posicion ?? 0),
          participanteId,
          nombreParticipante: String(row.nombreParticipante ?? row.NombreParticipante ?? ''),
          puntajeTotal: Number(row.puntajeTotal ?? row.PuntajeTotal ?? 0),
          tiempoAcumuladoMs: Number(row.tiempoAcumuladoMs ?? row.TiempoAcumuladoMs ?? 0),
        }
      })
      .filter((r): r is PosicionRankingDto => r != null),
  )
}

function normalizarPenalizacion(raw: unknown): PenalizacionAplicadaPayload | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const sesionId = String(o.sesionId ?? o.SesionId ?? '')
  const participanteId = String(o.participanteId ?? o.ParticipanteId ?? '')
  const puntos = Number(o.puntos ?? o.Puntos ?? 0)
  const motivo = String(o.motivo ?? o.Motivo ?? '').trim()
  if (!sesionId || !participanteId || !Number.isFinite(puntos) || puntos <= 0) return null
  return {
    sesionId,
    participanteId,
    puntos,
    motivo: motivo || 'Sin motivo indicado',
  }
}

interface UseSesionHubOptions {
  sesionId: string
  participanteId?: string
  enabled?: boolean
  onParticipanteExpulsado?: (payload: ParticipanteExpulsadoPayload) => void
  onPenalizacionAplicada?: (payload: PenalizacionAplicadaPayload) => void
  /** NuevoEstado del payload SesionEstadoCambiado (ej. Finalizada). */
  onEstadoSesionCambiado?: (nuevoEstado: string) => void
}

export function useSesionHub({
  sesionId,
  participanteId,
  enabled = true,
  onParticipanteExpulsado,
  onPenalizacionAplicada,
  onEstadoSesionCambiado,
}: UseSesionHubOptions): SesionHubConnectionStatus {
  const queryClient = useQueryClient()
  const token = useAuthStore((s) => s.token)
  const [status, setStatus] = useState<SesionHubConnectionStatus>('desconectado')
  const connectionRef = useRef<HubConnection | null>(null)
  const onExpulsadoRef = useRef(onParticipanteExpulsado)
  onExpulsadoRef.current = onParticipanteExpulsado
  const onPenalizacionRef = useRef(onPenalizacionAplicada)
  onPenalizacionRef.current = onPenalizacionAplicada
  const onEstadoRef = useRef(onEstadoSesionCambiado)
  onEstadoRef.current = onEstadoSesionCambiado

  useEffect(() => {
    if (!enabled || !sesionId || !participanteId || !token) {
      setStatus('desconectado')
      return
    }

    let cancelled = false
    const connection = crearSesionHub(() => useAuthStore.getState().token)
    connectionRef.current = connection

    const invalidateInscripcion = () => {
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    }

    const unirseGrupos = async () => {
      // SesionHub.UnirseASesion → grupos sesion-{id} + equipo-{participanteId}
      // (PenalizacionAplicada solo llega al grupo equipo-*)
      await connection.invoke('UnirseASesion', sesionId, participanteId)
    }

    connection.on('SesionEstadoCambiado', (payload: unknown) => {
      const o = (payload ?? {}) as Record<string, unknown>
      const estado = String(o.nuevoEstado ?? o.NuevoEstado ?? '')
      if (estado) onEstadoRef.current?.(estado)
      invalidateInscripcion()
    })
    connection.on('EtapaAvanzada', () => invalidateInscripcion())
    connection.on('PistaLiberada', () => invalidateInscripcion())
    connection.on('RankingActualizado', (payload: unknown) => {
      const ranking = normalizarRanking(payload, sesionId)
      if (ranking) {
        queryClient.setQueryData([...RANKING_KEY, sesionId], ranking)
      } else {
        void queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
      }
      invalidateInscripcion()
    })
    connection.on('TriviaEstado', () => {
      void queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
    })
    const invalidateTrivia = () => {
      void queryClient.invalidateQueries({ queryKey: [...TRIVIA_ESTADO_KEY, sesionId] })
      invalidateInscripcion()
    }
    // SignalR JS a veces entrega el nombre en minúsculas (preguntatriviainiciada).
    connection.on('PreguntaTriviaIniciada', invalidateTrivia)
    connection.on('preguntaTriviaIniciada', invalidateTrivia)
    connection.on('preguntatriviainiciada', invalidateTrivia)
    connection.on('TriviaEnTransicion', invalidateTrivia)
    connection.on('triviaEnTransicion', invalidateTrivia)
    connection.on('triviaentransicion', invalidateTrivia)
    connection.on('PenalizacionAplicada', (payload: unknown) => {
      const parsed = normalizarPenalizacion(payload)
      if (!parsed) return
      if (!mismoSesionId(parsed.sesionId, sesionId)) return
      if (!mismoParticipanteId(parsed.participanteId, participanteId)) return
      onPenalizacionRef.current?.(parsed)
      invalidateInscripcion()
      void queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
    })
    connection.on('ParticipanteExpulsado', (payload: unknown) => {
      const o = (payload ?? {}) as Record<string, unknown>
      const pid = String(o.participanteId ?? o.ParticipanteId ?? '')
      const sid = String(o.sesionId ?? o.SesionId ?? '')
      const motivo = String(o.motivo ?? o.Motivo ?? 'Expulsado')
      if (mismoParticipanteId(pid, participanteId) && mismoSesionId(sid || sesionId, sesionId)) {
        onExpulsadoRef.current?.({ sesionId, participanteId: pid, motivo })
      }
    })

    connection.onreconnecting(() => setStatus('reconectando'))
    connection.onreconnected(() => {
      setStatus('conectado')
      void unirseGrupos().catch(() => undefined)
    })
    connection.onclose(() => setStatus('desconectado'))

    setStatus('conectando')
    void connection
      .start()
      .then(async () => {
        if (cancelled) return
        await unirseGrupos()
        setStatus('conectado')
      })
      .catch(() => {
        if (!cancelled) setStatus('desconectado')
      })

    return () => {
      cancelled = true
      connectionRef.current = null
      void connection.stop()
    }
  }, [enabled, participanteId, queryClient, sesionId, token])

  return status
}
