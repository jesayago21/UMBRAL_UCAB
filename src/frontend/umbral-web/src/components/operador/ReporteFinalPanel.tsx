import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useReporteFinalSesion } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { cardClass } from '@/styles/ui'
import type { PosicionRankingDto } from '@/types/sesion.types'

interface ReporteFinalPanelProps {
  sesionId: string
  enabled?: boolean
}

function RankingFinalTable({ ranking }: { ranking: PosicionRankingDto[] }) {
  if (ranking.length === 0) {
    return <p className="text-sm text-slate-600">No hay participantes en el reporte final.</p>
  }

  return (
    <div className="overflow-hidden rounded-lg border border-slate-200">
      <table className="min-w-full text-sm">
        <thead className="bg-indigo-50 text-left text-slate-700">
          <tr>
            <th className="px-4 py-2 font-medium">Posición</th>
            <th className="px-4 py-2 font-medium">Participante</th>
            <th className="px-4 py-2 font-medium">Puntaje final</th>
          </tr>
        </thead>
        <tbody>
          {ranking.map((row) => (
            <tr key={row.participanteId} className="border-t border-slate-100">
              <td className="px-4 py-2 font-semibold text-indigo-800">{row.posicion}</td>
              <td className="px-4 py-2">{row.nombreParticipante}</td>
              <td className="px-4 py-2">{row.puntajeTotal}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function ReporteFinalPanel({ sesionId, enabled = true }: ReporteFinalPanelProps) {
  const { data, isLoading, isError, error, refetch } = useReporteFinalSesion(sesionId, enabled)

  if (!enabled) return null

  return (
    <section className={`${cardClass} space-y-3 border-indigo-200 bg-indigo-50/30`}>
      <h3 className="font-medium text-slate-900">Reporte final de ganadores</h3>

      {isLoading && <LoadingState label="Generando reporte final…" />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && (
        <>
          <p className="text-sm text-slate-600">
            Sesión <strong>{data.estado}</strong>
            {data.finalizadaEn && (
              <> · cerrada el {new Date(data.finalizadaEn).toLocaleString()}</>
            )}
          </p>
          <RankingFinalTable ranking={data.ranking} />
        </>
      )}
    </section>
  )
}
