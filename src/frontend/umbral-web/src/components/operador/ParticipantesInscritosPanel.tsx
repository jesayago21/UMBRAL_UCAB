import { cardClass } from '@/styles/ui'
import type { ParticipanteSesionDto } from '@/types/sesion.types'

interface ParticipantesInscritosPanelProps {
  participantes: ParticipanteSesionDto[]
}

export function ParticipantesInscritosPanel({ participantes }: ParticipantesInscritosPanelProps) {
  return (
    <section className={`${cardClass} space-y-4`}>
      <div>
        <h3 className="font-medium text-slate-900">Participantes inscritos</h3>
        <p className="mt-1 text-sm text-slate-600">
          Los jugadores se unen solos con login + código de sesión. Necesitas al menos uno para iniciar.
        </p>
      </div>

      {participantes.length === 0 ? (
        <p className="text-sm text-slate-600">Aún no hay participantes inscritos.</p>
      ) : (
        <ul className="divide-y divide-slate-100 rounded-lg border border-slate-200">
          {participantes.map((eq) => (
            <li key={eq.participanteId} className="px-4 py-3 text-sm">
              <span className="font-medium text-slate-900">{eq.nombre}</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
