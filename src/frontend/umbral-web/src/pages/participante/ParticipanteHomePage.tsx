import { Link } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { getParticipanteSesionInscrita, rutaPartidaParticipante } from '@/lib/participanteSesionStorage'
import { btnPrimary, cardClass } from '@/styles/ui'

export function ParticipanteHomePage() {
  const inscripcion = getParticipanteSesionInscrita()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Panel del participante"
        description="Únete a sesiones de misión activas con el código del operador."
      />

      {inscripcion && (
        <section className={`${cardClass} border-emerald-200 bg-emerald-50/50`}>
          <p className="text-sm font-medium text-emerald-900">Sesión activa (inscrito)</p>
          <p className="mt-1 text-slate-800">{inscripcion.titulo}</p>
          <Link to={rutaPartidaParticipante(inscripcion)} className={`${btnPrimary} mt-3 inline-block`}>
            Continuar partida
          </Link>
        </section>
      )}

      <Link to="/participante/sesiones" className={`${cardClass} block hover:border-indigo-300`}>
        <h3 className="font-semibold text-slate-900">Sesiones activas</h3>
        <p className="mt-2 text-sm text-slate-600">
          Listado unificado de sesiones de misión abiertas (etapas BT y Trivia).
        </p>
      </Link>
    </div>
  )
}
