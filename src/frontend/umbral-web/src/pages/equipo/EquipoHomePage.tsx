import { Link } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { getEquipoSesionInscrita } from '@/lib/equipoSesionStorage'
import { btnPrimary, cardClass } from '@/styles/ui'

export function EquipoHomePage() {
  const inscripcion = getEquipoSesionInscrita()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Panel del equipo"
        description="Elige el tipo de partida y únete con el código que te dé el operador."
      />

      {inscripcion && (
        <section className={`${cardClass} border-emerald-200 bg-emerald-50/50`}>
          <p className="text-sm font-medium text-emerald-900">Sesión activa (inscrito)</p>
          <p className="mt-1 text-slate-800">{inscripcion.titulo}</p>
          <Link
            to={`/equipo/busqueda/${inscripcion.sesionId}`}
            className={`${btnPrimary} mt-3 inline-block`}
          >
            Continuar partida
          </Link>
        </section>
      )}

      <div className="grid gap-4 sm:grid-cols-2">
        <Link to="/equipo/busqueda" className={`${cardClass} block hover:border-indigo-300`}>
          <h3 className="font-semibold text-slate-900">Búsqueda del tesoro</h3>
          <p className="mt-2 text-sm text-slate-600">
            Sesiones en vivo con misiones, etapas y códigos QR (gameplay completo en E2).
          </p>
        </Link>
        <Link to="/equipo/trivia" className={`${cardClass} block hover:border-indigo-300`}>
          <h3 className="font-semibold text-slate-900">Trivia</h3>
          <p className="mt-2 text-sm text-slate-600">
            Sesiones de preguntas en vivo (disponible cuando el operador cree sesiones trivia).
          </p>
        </Link>
      </div>
    </div>
  )
}
