import { useEffect, useState } from 'react'
import { cardClass } from '@/styles/ui'
import { formatDuracion } from '@/lib/sesionUi'
import type { EstadoSesionUi } from '@/types/sesion.types'

interface SesionTimerPanelProps {
  estado: EstadoSesionUi
  iniciadaEn: string | null
  finalizadaEn: string | null
  etapaActualOrden: number
  totalEtapas: number
  etapaDescripcion: string | null
  /** Etiqueta de progreso según tipo de sesión */
  unidadProgreso?: 'etapa' | 'pregunta'
}

export function SesionTimerPanel({
  estado,
  iniciadaEn,
  finalizadaEn,
  etapaActualOrden,
  totalEtapas,
  etapaDescripcion,
  unidadProgreso = 'etapa',
}: SesionTimerPanelProps) {
  const unidadLabel = unidadProgreso === 'pregunta' ? 'Pregunta' : 'Etapa'
  const [elapsedSec, setElapsedSec] = useState(0)

  useEffect(() => {
    if (!iniciadaEn) {
      setElapsedSec(0)
      return
    }

    const startMs = new Date(iniciadaEn).getTime()
    const endMs = finalizadaEn ? new Date(finalizadaEn).getTime() : Date.now()

    const tick = () => {
      const end = finalizadaEn ? new Date(finalizadaEn).getTime() : Date.now()
      setElapsedSec(Math.max(0, Math.floor((end - startMs) / 1000)))
    }

    tick()
    if (estado === 'activa') {
      const id = window.setInterval(tick, 1000)
      return () => window.clearInterval(id)
    }

    setElapsedSec(Math.max(0, Math.floor((endMs - startMs) / 1000)))
  }, [estado, iniciadaEn, finalizadaEn])

  const juegoIniciado = Boolean(iniciadaEn) && estado !== 'programada' && estado !== 'enPreparacion'

  return (
    <section className={`${cardClass} space-y-3`}>
      <h3 className="font-medium text-slate-900">Tiempo y progreso</h3>

      {juegoIniciado ? (
        <>
          <div className="flex flex-wrap items-baseline gap-3">
            <span className="font-mono text-3xl font-semibold tabular-nums text-slate-900">
              {formatDuracion(elapsedSec)}
            </span>
            <span className="text-sm text-slate-600">
              {estado === 'activa'
                ? 'Tiempo de sesión (en curso)'
                : estado === 'pausada'
                  ? 'Tiempo transcurrido (sesión pausada)'
                  : 'Tiempo total de la sesión'}
            </span>
          </div>
          {estado === 'pausada' && (
            <p className="text-xs text-amber-800">
              El reloj se detiene en pantalla al pausar; el dominio aún no descuenta pausas del total.
            </p>
          )}
        </>
      ) : (
        <p className="text-sm text-slate-600">
          El temporizador comienza al <strong>iniciar</strong> la sesión con equipos registrados.
        </p>
      )}

      {totalEtapas > 0 && (
        <div className="rounded-lg border border-slate-200 bg-slate-50/80 px-4 py-3 text-sm">
          <p className="font-medium text-slate-900">
            {unidadLabel} {etapaActualOrden || 1} de {totalEtapas}
          </p>
          {etapaDescripcion && (
            <p className="mt-1 text-slate-600">{etapaDescripcion}</p>
          )}
          {unidadProgreso === 'etapa' && (
            <p className="mt-2 text-xs text-slate-500">
              Las pistas con liberación por tiempo (RB-07) llegan en la segunda entrega con
              WebSockets; el temporizador por etapa también.
            </p>
          )}
          {unidadProgreso === 'pregunta' && (
            <p className="mt-2 text-xs text-slate-500">
              El lanzamiento de preguntas y respuestas en vivo se implementa en la segunda entrega.
            </p>
          )}
        </div>
      )}
    </section>
  )
}
