import { cardClass } from '@/styles/ui'
import type { PenalizacionParticipanteDto } from '@/types/sesion.types'

interface ParticipantePenalizacionesPanelProps {
  penalizaciones: PenalizacionParticipanteDto[]
  /** Aviso en vivo recién recibido por SignalR (antes del refetch). */
  avisoVivo?: { puntos: number; motivo: string } | null
  onDismissAviso?: () => void
}

/** Muestra el motivo de las penalizaciones al participante (HU-16). */
export function ParticipantePenalizacionesPanel({
  penalizaciones,
  avisoVivo,
  onDismissAviso,
}: ParticipantePenalizacionesPanelProps) {
  if (!avisoVivo && penalizaciones.length === 0) return null

  return (
    <section className={`${cardClass} space-y-3 border-amber-200 bg-amber-50/50`}>
      <h3 className="font-medium text-amber-950">Penalizaciones</h3>

      {avisoVivo && (
        <div
          className="flex items-start justify-between gap-3 rounded-lg border border-amber-300 bg-white px-3 py-2 text-sm text-amber-950"
          role="alert"
        >
          <div>
            <p className="font-semibold">
              Te restaron {avisoVivo.puntos} punto{avisoVivo.puntos === 1 ? '' : 's'}
            </p>
            <p className="mt-1 text-amber-900">
              Motivo: <span className="font-medium">{avisoVivo.motivo}</span>
            </p>
          </div>
          {onDismissAviso && (
            <button
              type="button"
              onClick={onDismissAviso}
              className="shrink-0 text-amber-800 hover:text-amber-950"
              aria-label="Cerrar aviso"
            >
              ×
            </button>
          )}
        </div>
      )}

      {penalizaciones.length > 0 && (
        <ul className="space-y-2">
          {penalizaciones.map((p, i) => (
            <li
              key={`${p.ocurridoEn}-${i}`}
              className="rounded-lg border border-amber-200 bg-white px-3 py-2 text-sm"
            >
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <span className="font-semibold text-slate-900">
                  −{p.puntos} punto{p.puntos === 1 ? '' : 's'}
                </span>
                <time className="text-xs text-slate-500">
                  {new Date(p.ocurridoEn).toLocaleString()}
                </time>
              </div>
              <p className="mt-1 text-slate-700">
                <span className="text-slate-500">Motivo:</span> {p.motivo}
              </p>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
