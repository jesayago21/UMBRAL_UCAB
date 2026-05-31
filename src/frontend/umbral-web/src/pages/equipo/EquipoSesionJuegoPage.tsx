import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { EquipoGameplayShell } from '@/components/equipo/EquipoGameplayShell'
import {
  clearEquipoSesionInscrita,
  getEquipoSesionInscrita,
} from '@/lib/equipoSesionStorage'

export function EquipoSesionJuegoPage() {
  const { sesionId } = useParams<{ sesionId: string }>()
  const navigate = useNavigate()
  const inscripcion = getEquipoSesionInscrita()

  if (!inscripcion || inscripcion.sesionId !== sesionId) {
    return <Navigate to="/equipo/busqueda" replace />
  }

  const handleSalir = () => {
    clearEquipoSesionInscrita()
    navigate('/equipo/busqueda')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Partida en curso"
        description="Vista de juego (plantilla E1). La interacción real llega en E2."
      />
      <EquipoGameplayShell inscripcion={inscripcion} onSalir={handleSalir} />
    </div>
  )
}
