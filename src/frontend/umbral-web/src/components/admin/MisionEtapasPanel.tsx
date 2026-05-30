import type { EtapaDto, MisionDto } from '@/types/mision.types'
import { cardClass } from '@/styles/ui'

interface MisionEtapasPanelProps {
  mision: MisionDto
  onClose: () => void
}

function EtapaRow({ etapa }: { etapa: EtapaDto }) {
  return (
    <li className="rounded-md border border-slate-200 bg-white p-3">
      <div className="flex flex-wrap items-baseline gap-2">
        <span className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-semibold text-indigo-800">
          Etapa {etapa.orden}
        </span>
        <span className="font-medium text-slate-900">{etapa.descripcion}</span>
      </div>
      <p className="mt-2 font-mono text-xs text-slate-600">
        QR solución: <span className="text-slate-900">{etapa.codigoQrSolucion}</span>
      </p>
      {etapa.pistas.length > 0 && (
        <p className="mt-1 text-xs text-slate-500">{etapa.pistas.length} pista(s)</p>
      )}
    </li>
  )
}

/** Detalle de etapas de una misión (solo lectura). */
export function MisionEtapasPanel({ mision, onClose }: MisionEtapasPanelProps) {
  return (
    <div className={`${cardClass} space-y-4`}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="font-medium text-slate-900">{mision.nombre}</h3>
          <p className="mt-1 text-sm text-slate-600">
            {mision.totalEtapas} etapa(s) · estado {mision.estado}
          </p>
        </div>
        <button type="button" onClick={onClose} className="text-sm text-indigo-600 hover:underline">
          Cerrar
        </button>
      </div>

      <p className="text-xs text-slate-500">
        Estas etapas se copian al crear una sesión en vivo. Editar nombre/estado no cambia etapas
        ya guardadas; para otro recorrido crea una misión nueva.
      </p>

      {mision.etapas.length === 0 ? (
        <p className="text-sm text-slate-600">Esta misión no tiene etapas registradas.</p>
      ) : (
        <ol className="space-y-2">
          {mision.etapas.map((etapa) => (
            <EtapaRow key={etapa.etapaId} etapa={etapa} />
          ))}
        </ol>
      )}
    </div>
  )
}
