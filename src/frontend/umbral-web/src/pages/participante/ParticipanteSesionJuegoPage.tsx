import { useCallback, useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { ParticipanteGameplayShell } from '@/components/participante/ParticipanteGameplayShell'
import { LoadingState } from '@/components/shared/LoadingState'
import { useAbandonarSesion, useMiInscripcionParticipante } from '@/hooks/useSesiones'
import type { ParticipanteExpulsadoPayload } from '@/hooks/useSesionHub'
import {
  clearParticipanteSesionInscrita,
  confirmarAbandonarSesion,
  getParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'

export function ParticipanteSesionJuegoPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const { data: inscripcionServidor, isLoading } = useMiInscripcionParticipante()
  const inscripcionLocal = getParticipanteSesionInscrita()
  const abandonar = useAbandonarSesion(sesionId ?? '')

  const handleExpulsado = useCallback(
    (payload: ParticipanteExpulsadoPayload) => {
      clearParticipanteSesionInscrita()
      navigate('/participante', {
        replace: true,
        state: { expulsadoMotivo: payload.motivo },
      })
    },
    [navigate],
  )

  if (isLoading) return <LoadingState />

  if (inscripcionServidor && inscripcionServidor.sesionId !== sesionId) {
    return <Navigate to={`/participante/sesiones/${inscripcionServidor.sesionId}`} replace />
  }

  if (inscripcionServidor && inscripcionServidor.sesionId === sesionId) {
    const inscripcion = {
      sesionId: inscripcionServidor.sesionId,
      titulo: inscripcionServidor.titulo,
      participanteId: inscripcionServidor.participanteId,
      joinedAt: inscripcionLocal?.joinedAt ?? new Date().toISOString(),
      tipoSesion: 'Mision' as const,
    }
    saveParticipanteSesionInscrita(inscripcion)

    const postJuego =
      inscripcionServidor.estado === 'Finalizada' ||
      inscripcionServidor.estado === 'Cancelada'

    const handleAbandonar = async () => {
      if (!confirmarAbandonarSesion({ postJuego })) return
      setError(null)
      try {
        await abandonar.mutateAsync()
        clearParticipanteSesionInscrita()
        navigate('/participante')
      } catch (err) {
        setError(getApiErrorMessage(err))
      }
    }

    return (
      <div className="space-y-6">
        <PageHeader title={postJuego ? 'Resultados' : 'Partida'} />
        {error && <p className="text-sm text-red-600">{error}</p>}
        <ParticipanteGameplayShell
          inscripcion={inscripcion}
          inscripcionServidor={inscripcionServidor}
          onAbandonar={handleAbandonar}
          abandonando={abandonar.isPending}
          puedeAbandonar={
            inscripcionServidor.estado !== 'Activa' && inscripcionServidor.estado !== 'Pausada'
          }
          onParticipanteExpulsado={handleExpulsado}
        />
      </div>
    )
  }

  if (!inscripcionLocal || inscripcionLocal.sesionId !== sesionId) {
    return <Navigate to="/participante" replace />
  }

  return <Navigate to="/participante" replace />
}
