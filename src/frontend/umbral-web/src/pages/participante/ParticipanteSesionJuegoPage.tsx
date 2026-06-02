import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { ParticipanteGameplayShell } from '@/components/participante/ParticipanteGameplayShell'
import {
  clearParticipanteSesionInscrita,
  getParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'

export function ParticipanteSesionJuegoPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const inscripcion = getParticipanteSesionInscrita()

  if (
    !inscripcion ||
    inscripcion.sesionId !== sesionId ||
    (inscripcion.tipoSesion && inscripcion.tipoSesion !== 'BusquedaTesoro')
  ) {
    return <Navigate to="/participante/busqueda" replace />
  }

  const handleSalir = () => {
    clearParticipanteSesionInscrita()
    navigate('/participante/busqueda')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Partida en curso"
        description="Vista de juego (plantilla E1). La interacción real llega en E2."
      />
      <ParticipanteGameplayShell inscripcion={inscripcion} onSalir={handleSalir} />
    </div>
  )
}
