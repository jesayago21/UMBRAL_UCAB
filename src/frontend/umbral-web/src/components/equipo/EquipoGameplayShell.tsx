import { Link } from 'react-router-dom'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import type { EquipoSesionInscrita } from '@/lib/equipoSesionStorage'
import { btnSecondary, cardClass, inputClass } from '@/styles/ui'

interface EquipoGameplayShellProps {
  inscripcion: EquipoSesionInscrita
  onSalir?: () => void
}

function E2Badge() {
  return (
    <span className="rounded bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-900">
      E2
    </span>
  )
}

/** Plantilla de juego BT — lógica real en Entrega 2. */
export function EquipoGameplayShell({ inscripcion, onSalir }: EquipoGameplayShellProps) {
  return (
    <div className="space-y-6">
      <section className={`${cardClass} border-emerald-200 bg-emerald-50/40`}>
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-emerald-800">
              Inscrito en sesión
            </p>
            <h2 className="mt-1 text-lg font-semibold text-slate-900">{inscripcion.titulo}</h2>
            <p className="mt-1 font-mono text-xs text-slate-600">
              Equipo {inscripcion.equipoId.slice(0, 8)}… · desde{' '}
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
          Espera a que el operador <strong>inicie</strong> la partida. Cuando la sesión esté activa,
          aquí verás la etapa actual, pistas liberadas y el escáner QR.
        </p>
      </section>

      <section className={`${cardClass} space-y-3 opacity-90`}>
        <div className="flex items-center gap-2">
          <h3 className="font-medium text-slate-900">Etapa y pistas</h3>
          <E2Badge />
        </div>
        <p className="text-sm text-slate-500">
          Placeholder: listado de pistas según RB-07 (por tiempo / por ganador de etapa).
        </p>
        <ul className="space-y-2 text-sm text-slate-400">
          <li className="rounded border border-dashed border-slate-200 px-3 py-2">
            Pista 1 — bloqueada hasta liberación
          </li>
          <li className="rounded border border-dashed border-slate-200 px-3 py-2">
            Pista 2 — bloqueada
          </li>
        </ul>
      </section>

      <section className={`${cardClass} space-y-3 opacity-90`}>
        <div className="flex items-center gap-2">
          <h3 className="font-medium text-slate-900">Registrar evidencia (QR)</h3>
          <E2Badge />
        </div>
        <input
          disabled
          className={`${inputClass} max-w-md`}
          placeholder="Código QR leído (E2)"
          aria-label="Código QR"
        />
        <button type="button" disabled className={`${btnSecondary} max-w-xs`}>
          Enviar evidencia (E2)
        </button>
      </section>

      <SesionRankingPanel
        sesionId={inscripcion.sesionId}
        enabled
        equipoIdDestacado={inscripcion.equipoId}
        emptyEquiposMessage="Tu equipo aparecerá aquí cuando el operador inicie la sesión."
      />

      <p className="text-center text-xs text-slate-500">
        <Link to="/equipo/busqueda" className="text-indigo-600 hover:underline">
          Volver al listado de sesiones
        </Link>
      </p>
    </div>
  )
}
