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
    <section className={`${cardClass} space-y-4`}>
      <div>
        <h3 className="font-medium text-slate-900">Código de acceso de la sesión</h3>
        <p className="mt-1 text-sm text-slate-600">
          Comparte este código con los jugadores. Lo ingresan al unirse desde su panel (login Keycloak).
          <strong> Un solo código por sesión</strong>, no por participante.
        </p>
      </div>

      <p className="inline-block rounded-lg border border-indigo-200 bg-indigo-50 px-4 py-3 font-mono text-2xl font-bold tracking-widest text-indigo-900">
        {codigoAcceso}
      </p>

      {estado === 'Programada' && (
        <div className="space-y-2">
          <p className="text-sm text-amber-800">
            La sesión aún no es visible para jugadores. Ábrela para inscripción cuando quieras recibir
            participantes.
          </p>
          {canAbrirInscripcion && (
            <button
              type="button"
              disabled={isSaving}
              onClick={onAbrirInscripcion}
              className={btnPrimary}
            >
              Abrir inscripción
            </button>
          )}
        </div>
      )}

      {estado === 'EnPreparacion' && (
        <p className="text-sm text-green-800">
          Inscripción abierta: los jugadores ven esta sesión en su panel y pueden unirse con el código.
        </p>
      )}
    </section>
  )
}
