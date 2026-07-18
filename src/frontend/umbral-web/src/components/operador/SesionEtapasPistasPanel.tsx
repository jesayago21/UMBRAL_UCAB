import { CodigoQrTesoro } from '@/components/shared/CodigoQrTesoro'
import { MapaTesoroViewer } from '@/components/shared/MapaTesoroViewer'
import { useCategorias } from '@/hooks/useCategorias'
import { formatCategoriaIds } from '@/lib/formatCategoriaIds'
import { cardClass } from '@/styles/ui'
import type { EtapaSesionDto } from '@/types/sesion.types'

const TIPO_LIBERACION_LABEL: Record<string, string> = {
  PorTiempo: 'Por tiempo',
  PorGanador: 'Al inicio',
}

function liberacionTexto(pista: NonNullable<EtapaSesionDto['pistas']>[number]): string {
  const tipo = TIPO_LIBERACION_LABEL[pista.tipoLiberacion] ?? pista.tipoLiberacion
  if (pista.tipoLiberacion === 'PorTiempo' && pista.segundosLiberacion != null) {
    return `${tipo} (${pista.segundosLiberacion}s)`
  }
  return tipo
}

function tieneUbicacion(etapa: EtapaSesionDto): boolean {
  return etapa.latitud != null && etapa.longitud != null && etapa.radioMetros != null
}

interface SesionEtapasPistasPanelProps {
  etapas: EtapaSesionDto[]
}

export function SesionEtapasPistasPanel({ etapas }: SesionEtapasPistasPanelProps) {
  const { data: categorias } = useCategorias()
  const categoriaOptions = (categorias ?? []).map((c) => ({ id: c.id, nombre: c.nombre }))

  return (
    <section className={`${cardClass} space-y-4`}>
      <h3 className="font-medium text-slate-900">Etapas de la misión</h3>

      <ul className="space-y-3">
        {etapas.map((etapa) => {
          const pistas = etapa.pistas ?? []
          const esTrivia = etapa.tipoEtapa === 'Trivia'
          const esBt = !esTrivia

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
                {esBt && (
                  <span className="text-xs text-slate-500">
                    {pistas.length} pista{pistas.length === 1 ? '' : 's'}
                  </span>
                )}
              </div>

              {esBt && (
                <div className="mt-3 grid gap-3 md:grid-cols-2">
                  {tieneUbicacion(etapa) && (
                    <div className="space-y-1">
                      <MapaTesoroViewer
                        latitud={etapa.latitud!}
                        longitud={etapa.longitud!}
                        radioMetros={etapa.radioMetros!}
                        heightClass="h-48"
                      />
                    </div>
                  )}
                  {etapa.codigoQrSolucion ? (
                    <CodigoQrTesoro codigo={etapa.codigoQrSolucion} />
                  ) : null}
                </div>
              )}

              {esTrivia ? (
                <p className="mt-2 text-xs text-slate-600">
                  Categorías: {formatCategoriaIds(etapa.categoriaIds, categoriaOptions)}
                </p>
              ) : pistas.length === 0 ? null : (
                <ol className="mt-3 space-y-2">
                  {pistas.map((pista, index) => (
                    <li
                      key={`${etapa.orden}-${index}`}
                      className="rounded border border-slate-200 bg-white px-3 py-2 text-sm"
                    >
                      <p className="text-slate-800">{pista.contenido}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {liberacionTexto(pista)}
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
