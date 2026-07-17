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

function mismoSesionId(a: string, b: string): boolean {
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

interface UseSesionHubOptions {
  sesionId: string
  participanteId?: string
  enabled?: boolean
  onParticipanteExpulsado?: (payload: ParticipanteExpulsadoPayload) => void
}

export function useSesionHub({
  sesionId,
  participanteId,
  enabled = true,
  onParticipanteExpulsado,
}: UseSesionHubOptions): SesionHubConnectionStatus {
  const queryClient = useQueryClient()
  const token = useAuthStore((s) => s.token)
  const [status, setStatus] = useState<SesionHubConnectionStatus>('desconectado')
  const connectionRef = useRef<HubConnection | null>(null)
  const onExpulsadoRef = useRef(onParticipanteExpulsado)
  onExpulsadoRef.current = onParticipanteExpulsado

  useEffect(() => {
    if (!enabled || !sesionId || !token) {
      setStatus('desconectado')
      return
    }

    let cancelled = false
    const connection = crearSesionHub(() => useAuthStore.getState().token)
    connectionRef.current = connection

    const invalidateInscripcion = () => {
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    }

    connection.on('EstadoSesionCambiado', () => invalidateInscripcion())
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
    connection.on('ParticipanteExpulsado', (payload: unknown) => {
      const o = (payload ?? {}) as Record<string, unknown>
      const pid = String(o.participanteId ?? o.ParticipanteId ?? '')
      const sid = String(o.sesionId ?? o.SesionId ?? '')
      const motivo = String(o.motivo ?? o.Motivo ?? 'Expulsado')
      if (participanteId && pid === participanteId && mismoSesionId(sid || sesionId, sesionId)) {
        onExpulsadoRef.current?.({ sesionId, participanteId: pid, motivo })
      }
    })

    connection.onreconnecting(() => setStatus('reconectando'))
    connection.onreconnected(() => {
      setStatus('conectado')
      void connection.invoke('UnirseGrupoSesion', sesionId).catch(() => undefined)
    })
    connection.onclose(() => setStatus('desconectado'))

    setStatus('conectando')
    void connection
      .start()
      .then(async () => {
        if (cancelled) return
        await connection.invoke('UnirseGrupoSesion', sesionId)
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
