import { useCallback, useState } from 'react'
import {
  ParticipanteEtapasPanel,
  esEtapaBusquedaTesoro,
} from '@/components/participante/ParticipanteEtapasPanel'
import { ParticipantePenalizacionesPanel } from '@/components/participante/ParticipantePenalizacionesPanel'
import { ParticipanteTableroHeader } from '@/components/participante/ParticipanteTableroHeader'
import { ParticipanteTriviaLivePanel } from '@/components/participante/ParticipanteTriviaLivePanel'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import { ErrorState } from '@/components/shared/ErrorState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useEnviarEvidencia } from '@/hooks/useSesiones'
import { useSesionHub, type ParticipanteExpulsadoPayload } from '@/hooks/useSesionHub'
import type { ParticipanteSesionInscrita } from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, btnSecondary, cardClass, inputClass } from '@/styles/ui'
import type { MiInscripcionParticipanteDto } from '@/types/sesion.types'

interface ParticipanteGameplayShellProps {
  inscripcion: ParticipanteSesionInscrita
  inscripcionServidor: MiInscripcionParticipanteDto
  onAbandonar?: () => void
  abandonando?: boolean
  puedeAbandonar?: boolean
  onParticipanteExpulsado?: (payload: ParticipanteExpulsadoPayload) => void
}

function mensajeResultadoEvidencia(resultado: string): string {
  switch (resultado) {
    case 'Valida':
      return '¡Evidencia válida! Ganaste puntos en esta etapa.'
    case 'Invalida':
      return 'Evidencia inválida: QR incorrecto, etapa ya resuelta o duplicada.'
    case 'Rechazada':
      return 'Evidencia rechazada: la sesión no acepta envíos en este momento.'
    default:
      return `Resultado: ${resultado}`
  }
}

export function ParticipanteGameplayShell({
  inscripcion,
  inscripcionServidor,
  onAbandonar,
  abandonando = false,
  puedeAbandonar = true,
  onParticipanteExpulsado,
}: ParticipanteGameplayShellProps) {
  const [codigoQr, setCodigoQr] = useState('')
  const [feedback, setFeedback] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [avisoPenalizacion, setAvisoPenalizacion] = useState<{
    puntos: number
    motivo: string
  } | null>(null)

  const enviar = useEnviarEvidencia(inscripcion.sesionId)

  const onPenalizacionAplicada = useCallback(
    (payload: { puntos: number; motivo: string }) => {
      setAvisoPenalizacion({ puntos: payload.puntos, motivo: payload.motivo })
    },
    [],
  )

  const handleExpulsado = useCallback(
    (payload: ParticipanteExpulsadoPayload) => {
      onParticipanteExpulsado?.(payload)
    },
    [onParticipanteExpulsado],
  )

  const hubStatus = useSesionHub({
    sesionId: inscripcion.sesionId,
    rol: 'participante',
    participanteId: inscripcion.participanteId,
    enabled: true,
    onPenalizacionAplicada,
    onParticipanteExpulsado: handleExpulsado,
  })

  const etapasOrdenadas = [...(inscripcionServidor.etapas ?? [])].sort((a, b) => a.orden - b.orden)
  const preJuego =
    inscripcionServidor.estado === 'EnPreparacion' ||
    inscripcionServidor.estado === 'Programada'
  const sesionEnJuego =
    inscripcionServidor.estado === 'Activa' || inscripcionServidor.estado === 'Pausada'
  const postJuego =
    inscripcionServidor.estado === 'Finalizada' ||
    inscripcionServidor.estado === 'Cancelada'
  const etapaReferencia = preJuego
    ? etapasOrdenadas[0]
    : etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0]
  const muestraQr =
    sesionEnJuego &&
    etapaReferencia != null &&
    esEtapaBusquedaTesoro(etapaReferencia) &&
    inscripcionServidor.estado === 'Activa'
  const muestraTrivia =
    sesionEnJuego &&
    etapaReferencia != null &&
    etapaReferencia.tipoEtapa === 'Trivia'

  const handleEnviarEvidencia = async (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)
    setFeedback(null)
    const qr = codigoQr.trim()
    if (!qr) {
      setError('Ingresa el código QR escaneado.')
      return
    }
    try {
      const result = await enviar.mutateAsync({ codigoQr: qr })
      setFeedback(mensajeResultadoEvidencia(result.resultado))
      if (result.resultado === 'Valida') setCodigoQr('')
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  const etiquetaSalir = postJuego ? 'Salir de la sesión' : 'Abandonar sesión'

  return (
    <div className="space-y-6">
      <section
        className={`${cardClass} ${
          postJuego
            ? 'border-indigo-200 bg-indigo-50/40'
            : 'border-emerald-200 bg-emerald-50/40'
        }`}
      >
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            {postJuego && (
              <p className="text-xs font-semibold uppercase tracking-wide text-indigo-800">
                {inscripcionServidor.estado === 'Cancelada'
                  ? 'Sesión cancelada'
                  : 'Partida finalizada'}
              </p>
            )}
            <h2
              className={`text-lg font-semibold text-slate-900 ${postJuego ? 'mt-1' : ''}`}
            >
              {inscripcion.titulo}
            </h2>
          </div>
          {onAbandonar && puedeAbandonar && (
            <button
              type="button"
              onClick={onAbandonar}
              className={btnSecondary}
              disabled={abandonando}
            >
              {abandonando ? 'Saliendo…' : etiquetaSalir}
            </button>
          )}
        </div>
        {!puedeAbandonar && (
          <p className="mt-3 text-sm text-amber-800">
            La sesión ya está en juego. No puedes abandonar hasta que finalice.
          </p>
        )}
        <p className="mt-3 text-sm text-slate-600">
          {preJuego ? (
            <>
              Espera a que el operador <strong>inicie</strong> la partida.
            </>
          ) : postJuego ? (
            inscripcionServidor.estado === 'Cancelada' ? (
              <>La sesión fue cancelada. Revisa el ranking y sal cuando quieras.</>
            ) : (
              <>
                La partida terminó. Revisa el <strong>ranking final</strong> y sal cuando quieras.
              </>
            )
          ) : (
            <>La sesión está en curso.</>
          )}
        </p>
      </section>

      {(sesionEnJuego || preJuego || postJuego) && (
        <ParticipanteTableroHeader
          inscripcion={inscripcionServidor}
          participanteId={inscripcion.participanteId}
          etapaActual={postJuego ? undefined : etapaReferencia}
        />
      )}

      <ParticipantePenalizacionesPanel
        penalizaciones={inscripcionServidor.penalizaciones ?? []}
        avisoVivo={avisoPenalizacion}
        onDismissAviso={() => setAvisoPenalizacion(null)}
      />

      {muestraTrivia && (
        <ParticipanteTriviaLivePanel
          sesionId={inscripcion.sesionId}
          enabled={inscripcionServidor.estado === 'Activa' || inscripcionServidor.estado === 'Pausada'}
        />
      )}

      {!postJuego && (
        <ParticipanteEtapasPanel
          estadoSesion={inscripcionServidor.estado}
          etapas={inscripcionServidor.etapas}
        />
      )}

      {muestraQr && (
        <section className={`${cardClass} space-y-3`}>
          <h3 className="font-medium text-slate-900">Código QR</h3>
          {feedback && <SuccessAlert message={feedback} onDismiss={() => setFeedback(null)} />}
          {error && <ErrorState message={error} />}
          <form onSubmit={(e) => void handleEnviarEvidencia(e)} className="flex flex-wrap gap-2">
            <input
              className={`${inputClass} min-w-[12rem] flex-1`}
              placeholder="Código QR"
              value={codigoQr}
              onChange={(e) => setCodigoQr(e.target.value)}
              aria-label="Código QR"
              disabled={enviar.isPending}
            />
            <button type="submit" disabled={enviar.isPending} className={btnPrimary}>
              {enviar.isPending ? 'Enviando…' : 'Enviar'}
            </button>
          </form>
        </section>
      )}

      {sesionEnJuego && inscripcionServidor.estado === 'Pausada' && (
        <p className="text-sm text-amber-800">Sesión pausada.</p>
      )}

      <SesionRankingPanel
        sesionId={inscripcion.sesionId}
        enabled
        title={postJuego ? 'Ranking final' : 'Ranking'}
        participanteIdDestacado={inscripcion.participanteId}
        emptyParticipantesMessage="Sin posiciones aún."
        hubStatus={hubStatus}
      />
    </div>
  )
}
