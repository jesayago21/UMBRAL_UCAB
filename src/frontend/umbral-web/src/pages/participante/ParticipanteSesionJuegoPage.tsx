import { useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { ParticipanteGameplayShell } from '@/components/participante/ParticipanteGameplayShell'
import { useAbandonarSesion, useMiInscripcionParticipante } from '@/hooks/useSesiones'
import {
  clearParticipanteSesionInscrita,
  confirmarAbandonarSesion,
  getParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { LoadingState } from '@/components/shared/LoadingState'

export function ParticipanteSesionJuegoPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const { data: inscripcionServidor, isLoading } = useMiInscripcionParticipante()
  const inscripcionLocal = getParticipanteSesionInscrita()
  const abandonar = useAbandonarSesion(sesionId ?? '')

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

    const handleAbandonar = async () => {
      if (!confirmarAbandonarSesion()) return
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
        <PageHeader
          title="Partida en curso"
          description="Vista de juego (plantilla E1). La interacción real llega en E2."
        />
        {error && <p className="text-sm text-red-600">{error}</p>}
        <ParticipanteGameplayShell
          inscripcion={inscripcion}
          inscripcionServidor={inscripcionServidor}
          onAbandonar={handleAbandonar}
          abandonando={abandonar.isPending}
          puedeAbandonar={inscripcionServidor.estado !== 'Activa' && inscripcionServidor.estado !== 'Pausada'}
        />
      </div>
    )
  }

  if (!inscripcionLocal || inscripcionLocal.sesionId !== sesionId) {
    return <Navigate to="/participante" replace />
  }

  return <Navigate to="/participante" replace />
}
