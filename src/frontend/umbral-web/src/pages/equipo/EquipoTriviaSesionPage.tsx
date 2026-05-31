import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { EquipoTriviaGameplayShell } from '@/components/equipo/EquipoTriviaGameplayShell'
import {
  clearEquipoSesionInscrita,
  getEquipoSesionInscrita,
} from '@/lib/equipoSesionStorage'

export function EquipoTriviaSesionPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const inscripcion = getEquipoSesionInscrita()

  if (
    !inscripcion ||
    inscripcion.sesionId !== sesionId ||
    (inscripcion.tipoSesion && inscripcion.tipoSesion !== 'Trivia')
  ) {
    return <Navigate to="/equipo/trivia" replace />
  }

  const handleSalir = () => {
    clearEquipoSesionInscrita()
    navigate('/equipo/trivia')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Partida trivia"
        description="Vista previa de preguntas (E1). Responder en vivo llega en E2."
      />
      <EquipoTriviaGameplayShell inscripcion={inscripcion} onSalir={handleSalir} />
    </div>
  )
}
