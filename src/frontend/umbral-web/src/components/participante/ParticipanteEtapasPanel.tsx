/** Etapas del participante: etapa actual (mapa/pistas) — progreso N/total va en el tablero. */
import { MapaTesoroViewer } from '@/components/shared/MapaTesoroViewer'
import { cardClass } from '@/styles/ui'
import type { EtapaSesionDto } from '@/types/sesion.types'

const TIPO_ETAPA_LABEL: Record<string, string> = {
  BusquedaTesoro: 'Búsqueda del tesoro',
  Trivia: 'Trivia',
}

function tieneUbicacion(etapa: EtapaSesionDto): boolean {
  return etapa.latitud != null && etapa.longitud != null && etapa.radioMetros != null
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

      {!esTrivia && tieneUbicacion(etapa) && (
        <div className="mt-4 space-y-2">
          <MapaTesoroViewer
            latitud={etapa.latitud!}
            longitud={etapa.longitud!}
            radioMetros={etapa.radioMetros!}
          />
        </div>
      )}

      {!esTrivia && pistas.length > 0 && (
        <div className="mt-4">
          <p className="text-sm font-medium text-slate-800">Pistas</p>
          <ol className="mt-2 space-y-2">
            {pistas.map((pista, index) => (
              <li
                key={`${etapa.orden}-p-${index}`}
                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800"
              >
                {pista.contenido}
              </li>
            ))}
          </ol>
        </div>
      )}
    </div>
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
        <p className="text-sm text-slate-600">No se pudieron cargar las etapas. Recarga la página.</p>
      </section>
    )
  }

  const etapasOrdenadas = [...etapas].sort((a, b) => a.orden - b.orden)
  if (etapasOrdenadas.length === 0) {
    return (
      <section className={cardClass}>
        <p className="text-sm text-slate-600">Esta sesión no tiene etapas.</p>
      </section>
    )
  }

  const preJuego = estadoSesion === 'EnPreparacion' || estadoSesion === 'Programada'
  const destacada = preJuego
    ? etapasOrdenadas[0]
    : etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0]
  const tituloDestacada = preJuego ? 'Próxima etapa' : 'Etapa actual'

  return <EtapaDestacada etapa={destacada} titulo={tituloDestacada} />
}

export function esEtapaBusquedaTesoro(etapa: EtapaSesionDto): boolean {
  return etapa.tipoEtapa === 'BusquedaTesoro'
}
