import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useHistorialSesion } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnSecondary, cardClass } from '@/styles/ui'

interface SesionHistorialPanelProps {
  sesionId: string
  enabled?: boolean
}

export function SesionHistorialPanel({ sesionId, enabled = true }: SesionHistorialPanelProps) {
  const [pagina, setPagina] = useState(1)
  const { data, isLoading, isError, error, refetch, isFetching } = useHistorialSesion(
    sesionId,
    pagina,
    enabled,
  )

  const totalPaginas = data ? Math.max(1, Math.ceil(data.totalEventos / data.tamanoPagina)) : 1

  return (
    <section className={`${cardClass} space-y-3`}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="font-medium text-slate-900">Historial de auditoría</h3>
        <button
          type="button"
          className={btnSecondary}
          disabled={isFetching}
          onClick={() => void refetch()}
        >
          Refrescar
        </button>
      </div>

      {isLoading && <LoadingState label="Cargando historial…" />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && data.eventos.length === 0 && (
        <p className="text-sm text-slate-600">No hay eventos registrados todavía.</p>
      )}

      {data && data.eventos.length > 0 && (
        <>
          <ul className="max-h-72 space-y-2 overflow-y-auto text-sm">
            {data.eventos.map((evento) => (
              <li
                key={evento.eventoId}
                className="rounded border border-slate-200 bg-slate-50 px-3 py-2"
              >
                <div className="flex flex-wrap items-baseline justify-between gap-2">
                  <span className="font-medium text-slate-900">{evento.tipo}</span>
                  <time className="text-xs text-slate-500">
                    {new Date(evento.ocurridoEn).toLocaleString()}
                  </time>
                </div>
                {evento.payload && (
                  <p className="mt-1 break-all font-mono text-xs text-slate-600">{evento.payload}</p>
                )}
              </li>
            ))}
          </ul>
          <div className="flex items-center justify-between text-sm text-slate-600">
            <span>
              Página {data.pagina} de {totalPaginas} · {data.totalEventos} evento(s)
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                className={btnSecondary}
                disabled={pagina <= 1}
                onClick={() => setPagina((p) => Math.max(1, p - 1))}
              >
                Anterior
              </button>
              <button
                type="button"
                className={btnSecondary}
                disabled={pagina >= totalPaginas}
                onClick={() => setPagina((p) => p + 1)}
              >
                Siguiente
              </button>
            </div>
          </div>
        </>
      )}
    </section>
  )
}
