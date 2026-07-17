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
  etapaDescripcion?: string | null
  unidadProgreso?: 'etapa' | 'pregunta'
  className?: string
}

/** Reloj + progreso en formato compacto (barra superior del operador). */
export function SesionTimerPanel({
  estado,
  iniciadaEn,
  finalizadaEn,
  etapaActualOrden,
  totalEtapas,
  unidadProgreso = 'etapa',
  className = '',
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
    <section
      className={`${cardClass} flex h-[14rem] flex-col gap-3 ${className}`.trim()}
      aria-label="Tiempo y progreso"
    >
      <h3 className="font-medium text-slate-900">Tiempo y progreso</h3>
      <div className="flex flex-1 flex-col justify-center gap-1">
        {juegoIniciado ? (
          <p
            className={`font-mono text-2xl font-semibold tabular-nums ${
              estado === 'pausada' ? 'text-amber-800' : 'text-slate-900'
            }`}
          >
            {formatDuracion(elapsedSec)}
            {estado === 'pausada' ? (
              <span className="ml-2 text-xs font-sans font-medium">pausada</span>
            ) : null}
          </p>
        ) : (
          <p className="text-sm text-slate-500">—</p>
        )}
        {totalEtapas > 0 ? (
          <p className="text-sm font-medium text-slate-800">
            {unidadLabel} {etapaActualOrden || 1}/{totalEtapas}
          </p>
        ) : null}
      </div>
    </section>
  )
}
