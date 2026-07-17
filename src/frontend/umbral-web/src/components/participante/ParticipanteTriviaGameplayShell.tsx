import { ParticipanteTriviaLivePanel } from '@/components/participante/ParticipanteTriviaLivePanel'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import { useSesionHub } from '@/hooks/useSesionHub'
import type { ParticipanteSesionInscrita } from '@/lib/participanteSesionStorage'
import { btnSecondary, cardClass } from '@/styles/ui'

interface ParticipanteTriviaGameplayShellProps {
  inscripcion: ParticipanteSesionInscrita
  onSalir?: () => void
}

/** Shell de sesión trivia dedicada — ronda en vivo HU-33. */
export function ParticipanteTriviaGameplayShell({
  inscripcion,
  onSalir,
}: ParticipanteTriviaGameplayShellProps) {
  const hubStatus = useSesionHub({
    sesionId: inscripcion.sesionId,
    rol: 'participante',
    participanteId: inscripcion.participanteId,
    enabled: true,
  })

  return (
    <div className="space-y-6">
      <section className={`${cardClass} border-emerald-200 bg-emerald-50/40`}>
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-emerald-800">
              Inscrito en sesión trivia
            </p>
            <h2 className="mt-1 text-lg font-semibold text-slate-900">{inscripcion.titulo}</h2>
            <p className="mt-1 font-mono text-xs text-slate-600">
              Participante {inscripcion.participanteId.slice(0, 8)}…
            </p>
          </div>
          {onSalir && (
            <button type="button" onClick={onSalir} className={btnSecondary}>
              Salir de sesión
            </button>
          )}
        </div>
      </section>

      <ParticipanteTriviaLivePanel sesionId={inscripcion.sesionId} enabled />

      <SesionRankingPanel
        sesionId={inscripcion.sesionId}
        enabled
        participanteIdDestacado={inscripcion.participanteId}
        hubStatus={hubStatus}
      />
    </div>
  )
}
