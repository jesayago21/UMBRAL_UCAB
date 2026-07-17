import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { useLanzarPreguntaTrivia } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass } from '@/styles/ui'

interface TriviaRondaOperadorPanelProps {
  sesionId: string
  visible: boolean
  /** Fase del dominio: Esperando | PreguntaActiva | Transicion */
  triviaFase?: string | null
  onSuccess?: (msg: string) => void
}

/**
 * HU-33 — el operador solo inicia la secuencia; el avance es automático por timer.
 * Sin respuestas (HU-34) las preguntas avanzan igual con 0 pts.
 */
export function TriviaRondaOperadorPanel({
  sesionId,
  visible,
  triviaFase,
  onSuccess,
}: TriviaRondaOperadorPanelProps) {
  const iniciar = useLanzarPreguntaTrivia(sesionId)
  const [error, setError] = useState<string | null>(null)

  if (!visible) return null

  const enCurso = triviaFase === 'PreguntaActiva' || triviaFase === 'Transicion'
  const puedeIniciar = triviaFase === 'Esperando'

  const handleIniciar = async () => {
    setError(null)
    try {
      await iniciar.mutateAsync(undefined)
      onSuccess?.(
        'Secuencia iniciada. Al acabar el tiempo avanza sola (sin respuesta = 0 pts).',
      )
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  return (
    <section className={`${cardClass} space-y-3 border-indigo-200`}>
      <div>
        <h3 className="font-medium text-slate-900">Etapa de trivia</h3>
        <p className="mt-1 text-sm text-slate-600">
          Inicia la secuencia una sola vez. Cada pregunta dura 30 s; al acabarse el tiempo el
          sistema pasa a la siguiente aunque nadie haya contestado.
        </p>
      </div>
      {error && <ErrorState message={error} />}
      {enCurso && (
        <p className="text-sm text-indigo-800">
          Secuencia en curso (
          {triviaFase === 'Transicion' ? 'preparando siguiente…' : 'pregunta activa'}
          ). No vuelvas a iniciar: el avance es automático.
        </p>
      )}
      {puedeIniciar ? (
        <>
          <button
            type="button"
            className={btnPrimary}
            disabled={iniciar.isPending}
            onClick={() => void handleIniciar()}
          >
            {iniciar.isPending ? 'Iniciando…' : 'Iniciar etapa de trivia'}
          </button>
          <p className="text-xs text-slate-500">
            Si la etapa solo tiene 1 pregunta, al terminarla verás este botón de nuevo (o la
            siguiente etapa si la misión continúa). Con varias preguntas en la categoría, avanzan
            solas.
          </p>
        </>
      ) : null}
    </section>
  )
}
