import {
  ParticipanteEtapasPanel,
  esEtapaBusquedaTesoro,
} from '@/components/participante/ParticipanteEtapasPanel'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import type { ParticipanteSesionInscrita } from '@/lib/participanteSesionStorage'
import type { MiInscripcionParticipanteDto } from '@/types/sesion.types'
import { btnSecondary, cardClass, inputClass } from '@/styles/ui'

interface ParticipanteGameplayShellProps {
  inscripcion: ParticipanteSesionInscrita
  inscripcionServidor: MiInscripcionParticipanteDto
  onAbandonar?: () => void
  abandonando?: boolean
  puedeAbandonar?: boolean
}

function E2Badge() {
  return (
    <span className="rounded bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-900">
      E2
    </span>
  )
}

/** Plantilla de juego — lógica interactiva en Entrega 2. */
export function ParticipanteGameplayShell({
  inscripcion,
  inscripcionServidor,
  onAbandonar,
  abandonando = false,
  puedeAbandonar = true,
}: ParticipanteGameplayShellProps) {
  const etapasOrdenadas = [...(inscripcionServidor.etapas ?? [])].sort((a, b) => a.orden - b.orden)
  const preJuego =
    inscripcionServidor.estado === 'EnPreparacion' ||
    inscripcionServidor.estado === 'Programada'
  const etapaReferencia = preJuego
    ? etapasOrdenadas[0]
    : etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0]
  const muestraQr = etapaReferencia != null && esEtapaBusquedaTesoro(etapaReferencia)

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
              Participante {inscripcion.participanteId.slice(0, 8)}… · desde{' '}
              {new Date(inscripcion.joinedAt).toLocaleString()}
            </p>
          </div>
          {onAbandonar && puedeAbandonar && (
            <button
              type="button"
              onClick={onAbandonar}
              className={btnSecondary}
              disabled={abandonando}
            >
              Abandonar sesión
            </button>
          )}
        </div>
        {!puedeAbandonar && (
          <p className="mt-3 text-sm text-amber-800">
            La sesión ya está en juego. No puedes abandonar hasta que finalice o el operador la
            cancele.
          </p>
        )}
        <p className="mt-3 text-sm text-slate-600">
          {preJuego ? (
            <>
              Espera a que el operador <strong>inicie</strong> la partida. Abajo ves la primera
              etapa del recorrido y qué incluye.
            </>
          ) : (
            <>La sesión está en curso. Revisa la etapa actual y el ranking.</>
          )}
        </p>
      </section>

      <ParticipanteEtapasPanel
        estadoSesion={inscripcionServidor.estado}
        etapas={inscripcionServidor.etapas}
      />

      {muestraQr && (
        <section className={`${cardClass} space-y-3 opacity-90`}>
          <div className="flex items-center gap-2">
            <h3 className="font-medium text-slate-900">Registrar evidencia (QR)</h3>
            <E2Badge />
          </div>
          <p className="text-sm text-slate-500">
            Aplica a etapas de <strong>Búsqueda del tesoro</strong>. El escáner y el envío de códigos
            QR estarán disponibles en la segunda entrega.
          </p>
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
      )}

      <SesionRankingPanel
        sesionId={inscripcion.sesionId}
        enabled
        participanteIdDestacado={inscripcion.participanteId}
        emptyParticipantesMessage="Tu participante aparecerá aquí cuando el operador inicie la sesión."
      />
    </div>
  )
}
