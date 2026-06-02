/** Etapas del participante: detalle de la etapa actual + resumen del recorrido (solo nombres y tipos). */
import { cardClass } from '@/styles/ui'
import type { EtapaSesionDto } from '@/types/sesion.types'

const TIPO_ETAPA_LABEL: Record<string, string> = {
  BusquedaTesoro: 'Búsqueda del tesoro',
  Trivia: 'Trivia',
}

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

function EtapaDestacada({ etapa, titulo }: { etapa: EtapaSesionDto; titulo: string }) {
  const esTrivia = etapa.tipoEtapa === 'Trivia'
  const pistas = etapa.pistas ?? []
  const tipoLabel = TIPO_ETAPA_LABEL[etapa.tipoEtapa] ?? etapa.tipoEtapa

  return (
    <div className={`${cardClass} border-indigo-200 bg-indigo-50/40`}>
      <p className="text-xs font-semibold uppercase tracking-wide text-indigo-800">{titulo}</p>
      <div className="mt-2 flex flex-wrap items-center gap-2">
        <h3 className="text-lg font-semibold text-slate-900">Etapa {etapa.orden}</h3>
        <span
          className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
            esTrivia ? 'bg-violet-100 text-violet-900' : 'bg-amber-100 text-amber-900'
          }`}
        >
          {tipoLabel}
        </span>
      </div>
      <p className="mt-2 text-sm text-slate-700">{etapa.descripcion}</p>

      {esTrivia ? (
        <div className="mt-4 rounded-lg border border-violet-200 bg-white/80 px-3 py-3 text-sm text-slate-700">
          <p className="font-medium text-slate-900">Contenido de la etapa</p>
          <p className="mt-1">
            Preguntas del banco de trivia según las categorías configuradas en la misión.
          </p>
          {(etapa.categoriaIds ?? []).length > 0 && (
            <p className="mt-2 text-xs text-slate-500">
              {etapa.categoriaIds!.length} categoría
              {etapa.categoriaIds!.length === 1 ? '' : 's'} asignada
              {etapa.categoriaIds!.length === 1 ? '' : 's'}.
            </p>
          )}
        </div>
      ) : pistas.length === 0 ? (
        <p className="mt-3 text-sm text-slate-500">Esta etapa no tiene pistas definidas.</p>
      ) : (
        <div className="mt-4">
          <p className="text-sm font-medium text-slate-800">
            Pistas de la etapa ({pistas.length})
          </p>
          <p className="text-xs text-slate-500">
            Se liberarán en juego según tiempo o ganador de etapa (E2).
          </p>
          <ol className="mt-2 space-y-2">
            {pistas.map((pista, index) => (
              <li
                key={`${etapa.orden}-p-${index}`}
                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm"
              >
                <p className="text-slate-800">{pista.contenido}</p>
                <p className="mt-1 text-xs text-slate-500">
                  Liberación: {liberacionTexto(pista)}
                </p>
              </li>
            ))}
          </ol>
        </div>
      )}
    </div>
  )
}

function RecorridoEtapasResumen({ etapas }: { etapas: EtapaSesionDto[] }) {
  const total = etapas.length
  const busqueda = etapas.filter((e) => e.tipoEtapa === 'BusquedaTesoro').length
  const trivia = etapas.filter((e) => e.tipoEtapa === 'Trivia').length

  return (
    <section className={`${cardClass} space-y-3`}>
      <div>
        <h3 className="font-medium text-slate-900">Recorrido de la misión</h3>
        <p className="mt-1 text-sm text-slate-600">
          {total} etapa{total === 1 ? '' : 's'} en total
          {busqueda > 0 && (
            <>
              {' '}
              · {busqueda} de <span className="text-amber-800">búsqueda del tesoro</span>
            </>
          )}
          {trivia > 0 && (
            <>
              {' '}
              · {trivia} de <span className="text-violet-800">trivia</span>
            </>
          )}
        </p>
      </div>
      <ul className="divide-y divide-slate-100 rounded-lg border border-slate-200 bg-slate-50/60">
        {etapas.map((etapa) => {
          const esTrivia = etapa.tipoEtapa === 'Trivia'
          const tipoLabel = TIPO_ETAPA_LABEL[etapa.tipoEtapa] ?? etapa.tipoEtapa
          return (
            <li
              key={etapa.orden}
              className="flex flex-wrap items-center gap-x-3 gap-y-1 px-3 py-2.5 text-sm"
            >
              <span className="font-medium text-slate-500">#{etapa.orden}</span>
              <span className="min-w-0 flex-1 font-medium text-slate-900">{etapa.descripcion}</span>
              <span
                className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
                  esTrivia ? 'bg-violet-100 text-violet-900' : 'bg-amber-100 text-amber-900'
                }`}
              >
                {tipoLabel}
              </span>
            </li>
          )
        })}
      </ul>
    </section>
  )
}

export interface ParticipanteEtapasPanelProps {
  estadoSesion: string
  etapas?: EtapaSesionDto[]
}

export function ParticipanteEtapasPanel({ estadoSesion, etapas }: ParticipanteEtapasPanelProps) {
  if (!Array.isArray(etapas)) {
    return (
      <section className={cardClass}>
        <p className="text-sm text-slate-600">
          No se recibieron las etapas del servidor. Detén y vuelve a iniciar la API, luego recarga
          esta página.
        </p>
      </section>
    )
  }

  const etapasOrdenadas = [...etapas].sort((a, b) => a.orden - b.orden)
  if (etapasOrdenadas.length === 0) {
    return (
      <section className={cardClass}>
        <p className="text-sm text-slate-600">Esta sesión no tiene etapas configuradas.</p>
      </section>
    )
  }

  const preJuego = estadoSesion === 'EnPreparacion' || estadoSesion === 'Programada'
  const destacada = preJuego
    ? etapasOrdenadas[0]
    : etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0]
  const tituloDestacada = preJuego ? 'Primera etapa del recorrido' : 'Etapa actual'

  if (etapasOrdenadas.length === 1) {
    return <EtapaDestacada etapa={destacada} titulo={tituloDestacada} />
  }

  return (
    <div className="space-y-4">
      <EtapaDestacada etapa={destacada} titulo={tituloDestacada} />
      <RecorridoEtapasResumen etapas={etapasOrdenadas} />
    </div>
  )
}

export function esEtapaBusquedaTesoro(etapa: EtapaSesionDto): boolean {
  return etapa.tipoEtapa === 'BusquedaTesoro'
}
