import { useEffect, useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useEstadoTriviaSesion, useSubmitRespuestaTrivia } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { cardClass } from '@/styles/ui'
import type { TriviaRespuestaFeedback } from '@/types/sesion.types'

interface ParticipanteTriviaLivePanelProps {
  sesionId: string
  enabled?: boolean
}

function formatCountdown(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds))
  const m = Math.floor(s / 60)
  const r = s % 60
  return `${m}:${r.toString().padStart(2, '0')}`
}

/** HU-33/34 — pregunta activa + confirmación de respuesta. */
export function ParticipanteTriviaLivePanel({
  sesionId,
  enabled = true,
}: ParticipanteTriviaLivePanelProps) {
  const { data, isLoading, isError, error, refetch } = useEstadoTriviaSesion(sesionId, enabled)
  const submit = useSubmitRespuestaTrivia(sesionId)
  const [ahoraMs, setAhoraMs] = useState(() => Date.now())
  const [seleccion, setSeleccion] = useState<number | null>(null)
  const [feedback, setFeedback] = useState<TriviaRespuestaFeedback | null>(null)

  const timerIso = data?.timerCerradoEnUtc
  const fase = data?.fase ?? 'Esperando'
  const activa = fase === 'PreguntaActiva' && Boolean(timerIso)
  const preguntaId = data?.preguntaId ?? null

  useEffect(() => {
    setSeleccion(data?.indiceOpcionSeleccionada ?? null)
    setFeedback(null)
  }, [preguntaId, data?.indiceOpcionSeleccionada])

  useEffect(() => {
    if (!activa && fase !== 'Transicion') return
    const id = window.setInterval(() => setAhoraMs(Date.now()), 250)
    return () => window.clearInterval(id)
  }, [activa, fase, timerIso, data?.transicionHastaUtc])

  let segundosRestantes: number | null = null
  if (activa && timerIso) {
    const fin = Date.parse(timerIso)
    if (!Number.isNaN(fin)) {
      segundosRestantes = Math.max(0, (fin - ahoraMs) / 1000)
    }
  }

  const timerAgotado = segundosRestantes != null && segundosRestantes <= 0
  const bloqueado =
    Boolean(data?.yaRespondio) ||
    Boolean(feedback) ||
    submit.isPending ||
    timerAgotado ||
    !activa

  const confirmar = () => {
    if (!preguntaId || seleccion == null || bloqueado) return
    submit.mutate(
      {
        preguntaId,
        indiceOpcion: seleccion,
        duracionTimerSegundos: 30,
      },
      {
        onSuccess: (res) => setFeedback(res),
      },
    )
  }

  if (!enabled) return null
  if (isLoading && !data) return <LoadingState label="Sincronizando trivia…" />
  if (isError) {
    return <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
  }

  if (fase === 'Transicion') {
    const finTransicion = data?.transicionHastaUtc
    let segundosTransicion: number | null = null
    if (finTransicion) {
      const fin = Date.parse(finTransicion)
      if (!Number.isNaN(fin)) {
        segundosTransicion = Math.max(0, (fin - ahoraMs) / 1000)
      }
    }
    return (
      <section className={`${cardClass} border-indigo-200 bg-indigo-50/50 text-center`}>
        <p className="text-lg font-semibold text-indigo-950">Preparando siguiente pregunta…</p>
        {segundosTransicion != null && (
          <p className="mt-2 font-mono text-2xl font-semibold tabular-nums text-indigo-900">
            {formatCountdown(segundosTransicion)}
          </p>
        )}
        <p className="mt-2 text-sm text-indigo-800">
          Pregunta {((data?.preguntaIndexActual ?? 0) + 1)} de {data?.totalPreguntas ?? '—'}
        </p>
      </section>
    )
  }

  if (fase === 'Esperando' || !data?.enunciado) {
    return (
      <section className={`${cardClass} text-center`}>
        <p className="font-medium text-slate-900">Esperando inicio de la trivia</p>
        <p className="mt-2 text-sm text-slate-600">
          El operador iniciará la etapa. Las preguntas avanzan solas al acabar el tiempo. Si no
          respondes a tiempo, sumas 0 puntos.
        </p>
        {data && data.totalPreguntas > 0 && (
          <p className="mt-2 text-xs text-slate-500">{data.totalPreguntas} preguntas en la etapa</p>
        )}
      </section>
    )
  }

  const mensajeFeedback = feedback
    ? feedback.fueraDeTiempo
      ? 'Llegó tarde — 0 pts'
      : feedback.esCorrecta
        ? `¡Correcta! +${feedback.puntosOtorgados} pts`
        : 'Incorrecta — 0 pts'
    : data.yaRespondio
      ? 'Respuesta confirmada'
      : timerAgotado
        ? 'Tiempo agotado'
        : null

  return (
    <section className={`${cardClass} space-y-4`}>
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <p className="text-xs font-semibold uppercase tracking-wide text-indigo-700">
          Pregunta {data.orden ?? data.preguntaIndexActual + 1} / {data.totalPreguntas}
        </p>
        {segundosRestantes != null && (
          <p
            className={`font-mono text-lg font-semibold tabular-nums ${
              segundosRestantes <= 5 ? 'text-red-600' : 'text-slate-900'
            }`}
          >
            {formatCountdown(segundosRestantes)}
          </p>
        )}
      </div>
      <p className="text-lg font-medium text-slate-900">{data.enunciado}</p>
      <ul className="space-y-2">
        {(data.opciones ?? []).map((opcion, idx) => {
          const selected = seleccion === idx
          return (
            <li key={`${data.preguntaId}-${idx}`}>
              <button
                type="button"
                disabled={bloqueado}
                onClick={() => setSeleccion(idx)}
                className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                  selected
                    ? 'border-indigo-500 bg-indigo-50 text-indigo-950'
                    : 'border-slate-200 bg-slate-50 text-slate-800 hover:border-slate-300'
                } ${bloqueado ? 'cursor-not-allowed opacity-80' : ''}`}
              >
                {opcion}
              </button>
            </li>
          )
        })}
      </ul>
      <div className="flex flex-wrap items-center gap-3">
        <button
          type="button"
          disabled={bloqueado || seleccion == null}
          onClick={confirmar}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:bg-slate-300"
        >
          {submit.isPending ? 'Enviando…' : 'Confirmar respuesta'}
        </button>
        {mensajeFeedback && (
          <p
            className={`text-sm font-medium ${
              feedback?.esCorrecta && !feedback.fueraDeTiempo
                ? 'text-emerald-700'
                : 'text-slate-700'
            }`}
          >
            {mensajeFeedback}
            {feedback ? ` · Total: ${feedback.puntajeTotal}` : null}
          </p>
        )}
      </div>
      {submit.isError && (
        <p className="text-sm text-red-600">{getApiErrorMessage(submit.error)}</p>
      )}
    </section>
  )
}
