import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { ParticipanteTriviaGameplayShell } from '@/components/participante/ParticipanteTriviaGameplayShell'
import {
  clearParticipanteSesionInscrita,
  getParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'

export function ParticipanteTriviaSesionPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const inscripcion = getParticipanteSesionInscrita()

  if (
    !inscripcion ||
    inscripcion.sesionId !== sesionId ||
    (inscripcion.tipoSesion && inscripcion.tipoSesion !== 'Trivia')
  ) {
    return <Navigate to="/participante/trivia" replace />
  }

  const handleSalir = () => {
    clearParticipanteSesionInscrita()
    navigate('/participante/trivia')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Partida trivia"
        description="Vista previa de preguntas (E1). Responder en vivo llega en E2."
      />
      <ParticipanteTriviaGameplayShell inscripcion={inscripcion} onSalir={handleSalir} />
    </div>
  )
}
