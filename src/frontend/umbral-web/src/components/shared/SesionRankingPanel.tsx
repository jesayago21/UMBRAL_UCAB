import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useRankingSesion } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnSecondary, cardClass } from '@/styles/ui'
import type { PosicionRankingDto } from '@/types/sesion.types'

export interface SesionRankingPanelProps {
  sesionId: string
  /** Si false, no consulta la API (p. ej. sin participantes). */
  enabled?: boolean
  /** Mensaje cuando aún no hay participantes inscritos. */
  emptyParticipantesMessage?: string
  /** Resalta la fila del participante del jugador. */
  participanteIdDestacado?: string
  /** Título de la sección (default: Ranking). */
  title?: string
  className?: string
}

function RankingTable({
  ranking,
  participanteIdDestacado,
}: {
  ranking: PosicionRankingDto[]
  participanteIdDestacado?: string
}) {
  return (
    <div className="overflow-hidden rounded-lg border border-slate-200">
      <table className="min-w-full text-sm">
        <thead className="bg-slate-50 text-left text-slate-600">
          <tr>
            <th className="px-4 py-2 font-medium">#</th>
            <th className="px-4 py-2 font-medium">Participante</th>
            <th className="px-4 py-2 font-medium">Puntaje</th>
          </tr>
        </thead>
        <tbody>
          {ranking.map((row) => {
            const esMiParticipante = participanteIdDestacado === row.participanteId
            return (
              <tr
                key={row.participanteId}
                className={[
                  'border-t border-slate-100',
                  esMiParticipante ? 'bg-indigo-50/80' : '',
                ].join(' ')}
              >
                <td className="px-4 py-2">{row.posicion}</td>
                <td className="px-4 py-2 font-medium text-slate-900">
                  {row.nombreParticipante}
                  {esMiParticipante && (
                    <span className="ml-2 text-xs font-normal text-indigo-700">(tú)</span>
                  )}
                </td>
                <td className="px-4 py-2">{row.puntajeTotal}</td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

/** Ranking de sesión — misma tabla y refresco que en operador. */
export function SesionRankingPanel({
  sesionId,
  enabled = true,
  emptyParticipantesMessage = 'Aún no hay participantes inscritos.',
  participanteIdDestacado,
  title = 'Ranking',
  className = '',
}: SesionRankingPanelProps) {
  const queryEnabled = enabled && Boolean(sesionId)
  const {
    data: ranking,
    isLoading,
    isError,
    error,
    refetch,
    isFetching,
  } = useRankingSesion(sesionId, queryEnabled)

  return (
    <section className={`${cardClass} space-y-4 ${className}`.trim()}>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h3 className="font-medium text-slate-900">{title}</h3>
        <button
          type="button"
          disabled={!queryEnabled || isFetching}
          onClick={() => void refetch()}
          className={btnSecondary}
        >
          {isFetching ? 'Actualizando…' : 'Refrescar ranking'}
        </button>
      </div>

      {!queryEnabled && (
        <p className="text-sm text-slate-600">{emptyParticipantesMessage}</p>
      )}

      {queryEnabled && isLoading && <LoadingState label="Cargando ranking…" />}
      {queryEnabled && isError && (
        <ErrorState
          message={getApiErrorMessage(error)}
          onRetry={() => void refetch()}
        />
      )}

      {queryEnabled && ranking && ranking.length > 0 && (
        <RankingTable ranking={ranking} participanteIdDestacado={participanteIdDestacado} />
      )}

      {queryEnabled && ranking && ranking.length === 0 && !isLoading && !isError && (
        <p className="text-sm text-slate-600">
          Sin puntajes aún. El ranking se actualiza al registrar evidencias (E2).
        </p>
      )}
    </section>
  )
}
