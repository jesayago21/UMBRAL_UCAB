import { btnPrimary, cardClass } from '@/styles/ui'

interface SesionCodigoAccesoPanelProps {
  codigoAcceso: string
  estado: string
  canAbrirInscripcion: boolean
  isSaving: boolean
  onAbrirInscripcion: () => void
}

export function SesionCodigoAccesoPanel({
  codigoAcceso,
  estado,
  canAbrirInscripcion,
  isSaving,
  onAbrirInscripcion,
}: SesionCodigoAccesoPanelProps) {
  return (
    <section className={`${cardClass} flex h-[14rem] flex-col gap-3`}>
      <h3 className="font-medium text-slate-900">Código de acceso</h3>
      <p className="text-sm text-slate-600">Compártelo con los jugadores para unirse.</p>

      <p className="inline-block rounded-lg border border-indigo-200 bg-indigo-50 px-4 py-3 font-mono text-2xl font-bold tracking-widest text-indigo-900">
        {codigoAcceso}
      </p>

      <div className="mt-auto space-y-2">
        {estado === 'Programada' && canAbrirInscripcion && (
          <button
            type="button"
            disabled={isSaving}
            onClick={onAbrirInscripcion}
            className={btnPrimary}
          >
            Abrir inscripción
          </button>
        )}

        {estado === 'EnPreparacion' && (
          <p className="text-sm text-green-800">Inscripción abierta.</p>
        )}
      </div>
    </section>
  )
}
