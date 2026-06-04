import { Link } from 'react-router-dom'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { usePreguntasTriviaSesionParticipante } from '@/hooks/useSesiones'
import type { ParticipanteSesionInscrita } from '@/lib/participanteSesionStorage'
import { rutaListadoParticipante } from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnSecondary, cardClass } from '@/styles/ui'

interface ParticipanteTriviaGameplayShellProps {
  inscripcion: ParticipanteSesionInscrita
  onSalir?: () => void
}

function E2Badge() {
  return (
    <span className="rounded bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-900">
      E2
    </span>
  )
}

/** Vista previa de trivia E1 — preguntas y opciones; responder en vivo en E2. */
export function ParticipanteTriviaGameplayShell({
  inscripcion,
  onSalir,
}: ParticipanteTriviaGameplayShellProps) {
  const {
    data: preguntas,
    isLoading,
    isError,
    error,
    refetch,
  } = usePreguntasTriviaSesionParticipante(inscripcion.sesionId)

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
              Participante {inscripcion.participanteId.slice(0, 8)}… · desde{' '}
              {new Date(inscripcion.joinedAt).toLocaleString()}
            </p>
          </div>
          {onSalir && (
            <button type="button" onClick={onSalir} className={btnSecondary}>
              Salir de sesión
            </button>
          )}
        </div>
        <p className="mt-3 text-sm text-slate-600">
          Aquí puedes revisar las preguntas de la partida. El operador las lanzará en vivo en E2;
          por ahora las opciones son solo lectura.
        </p>
      </section>

      {isLoading && <LoadingState label="Cargando preguntas…" />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {preguntas && preguntas.length === 0 && !isLoading && (
        <EmptyState
          title="Sin preguntas"
          description="Esta sesión no tiene preguntas asignadas."
        />
      )}

      {preguntas && preguntas.length > 0 && (
        <section className="space-y-4">
          <div className="flex items-center gap-2">
            <h3 className="font-medium text-slate-900">
              Preguntas ({preguntas.length})
            </h3>
            <E2Badge />
            <span className="text-xs text-slate-500">responder en vivo</span>
          </div>
          <ol className="space-y-4">
            {preguntas.map((p) => (
              <li key={p.id} className={`${cardClass} space-y-3`}>
                <div className="flex flex-wrap items-baseline gap-2">
                  <span className="text-xs font-semibold uppercase text-indigo-600">
                    #{p.orden}
                  </span>
                  <span className="rounded bg-slate-100 px-2 py-0.5 text-xs text-slate-600">
                    {p.dificultad}
                  </span>
                </div>
                <p className="font-medium text-slate-900">{p.enunciado}</p>
                <ul className="space-y-2">
                  {p.opciones.map((opcion, idx) => (
                    <li
                      key={`${p.id}-${idx}`}
                      className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-700"
                    >
                      {String.fromCharCode(65 + idx)}. {opcion}
                    </li>
                  ))}
                </ul>
              </li>
            ))}
          </ol>
        </section>
      )}

      <SesionRankingPanel
        sesionId={inscripcion.sesionId}
        enabled
        participanteIdDestacado={inscripcion.participanteId}
        emptyParticipantesMessage="Tu participante aparecerá aquí cuando el operador inicie la sesión."
      />

      <p className="text-center text-xs text-slate-500">
        <Link to={rutaListadoParticipante('Trivia')} className="text-indigo-600 hover:underline">
          Volver al listado de sesiones trivia
        </Link>
      </p>
    </div>
  )
}
