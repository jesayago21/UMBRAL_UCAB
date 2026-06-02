import { cardClass } from '@/styles/ui'
import type { EtapaSesionDto } from '@/types/sesion.types'

const TIPO_LIBERACION_LABEL: Record<string, string> = {
  PorTiempo: 'Por tiempo',
  PorGanador: 'Al ganador de la etapa',
}

function liberacionTexto(pista: NonNullable<EtapaSesionDto['pistas']>[number]): string {
  const tipo = TIPO_LIBERACION_LABEL[pista.tipoLiberacion] ?? pista.tipoLiberacion
  if (pista.tipoLiberacion === 'PorTiempo' && pista.segundosLiberacion != null) {
    return `${tipo} (${pista.segundosLiberacion}s)`
  }
  return tipo
}

interface SesionEtapasPistasPanelProps {
  etapas: EtapaSesionDto[]
}

export function SesionEtapasPistasPanel({ etapas }: SesionEtapasPistasPanelProps) {
  const totalPistas = etapas.reduce((n, e) => n + (e.pistas?.length ?? 0), 0)
  const tieneTrivia = etapas.some((e) => e.tipoEtapa === 'Trivia')

  return (
    <section className={`${cardClass} space-y-4`}>
      <div>
        <h3 className="font-medium text-slate-900">
          {tieneTrivia && totalPistas === 0 ? 'Etapas de la misión' : 'Pistas por etapa'}
        </h3>
        <p className="mt-1 text-sm text-slate-600">
          Copia de la misión al crear la sesión (definida por el administrador). La liberación
          automática en juego llega en la segunda entrega con WebSockets.
        </p>
      </div>

      <ul className="space-y-3">
        {etapas.map((etapa) => {
          const pistas = etapa.pistas ?? []
          const esTrivia = etapa.tipoEtapa === 'Trivia'

          return (
            <li
              key={etapa.orden}
              className={`rounded-lg border px-4 py-3 ${
                etapa.esActual
                  ? 'border-indigo-300 bg-indigo-50/60'
                  : 'border-slate-200 bg-slate-50/50'
              }`}
            >
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <p className="font-medium text-slate-900">
                  Etapa {etapa.orden}
                  <span className="ml-2 text-xs font-normal text-slate-500">
                    {esTrivia ? 'Trivia' : 'Búsqueda del tesoro'}
                  </span>
                  {etapa.esActual && (
                    <span className="ml-2 rounded-full bg-indigo-600 px-2 py-0.5 text-xs font-semibold text-white">
                      Actual
                    </span>
                  )}
                </p>
                {!esTrivia && (
                  <span className="text-xs text-slate-500">
                    {pistas.length} pista{pistas.length === 1 ? '' : 's'}
                  </span>
                )}
              </div>
              <p className="mt-1 text-sm text-slate-600">{etapa.descripcion}</p>

              {esTrivia ? (
                <p className="mt-2 text-xs text-slate-600">
                  Categorías:{' '}
                  {(etapa.categoriaIds ?? []).length > 0
                    ? etapa.categoriaIds!.join(', ')
                    : '—'}
                </p>
              ) : pistas.length === 0 ? (
                <p className="mt-2 text-xs text-slate-500">Sin pistas en esta etapa.</p>
              ) : (
                <ol className="mt-3 space-y-2">
                  {pistas.map((pista, index) => (
                    <li
                      key={`${etapa.orden}-${index}`}
                      className="rounded border border-slate-200 bg-white px-3 py-2 text-sm"
                    >
                      <p className="text-slate-800">{pista.contenido}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        Liberación: {liberacionTexto(pista)}
                      </p>
                    </li>
                  ))}
                </ol>
              )}
            </li>
          )
        })}
      </ul>
    </section>
  )
}
