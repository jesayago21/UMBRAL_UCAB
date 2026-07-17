import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { useLiberarPistaManual } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, inputClass, selectClass } from '@/styles/ui'
import type { ParticipanteSesionDto } from '@/types/sesion.types'

const DESTINO_TODOS = '__todos__'

interface LiberarPistaManualPanelProps {
  sesionId: string
  participantes: ParticipanteSesionDto[]
  sesionActiva: boolean
  etapaEsBusquedaTesoro: boolean
  onSuccess?: () => void
}

export function LiberarPistaManualPanel({
  sesionId,
  participantes,
  sesionActiva,
  etapaEsBusquedaTesoro,
  onSuccess,
}: LiberarPistaManualPanelProps) {
  const [destinatario, setDestinatario] = useState(DESTINO_TODOS)
  const [contenido, setContenido] = useState('')
  const [error, setError] = useState<string | null>(null)

  const liberar = useLiberarPistaManual(sesionId)

  if (!sesionActiva) {
    return (
      <section className={`${cardClass} flex h-full min-h-[16rem] flex-col gap-2`}>
        <h3 className="font-medium text-slate-900">Pista manual</h3>
        <p className="text-sm text-slate-600">Disponible con sesión activa.</p>
      </section>
    )
  }

  if (!etapaEsBusquedaTesoro) {
    return (
      <section className={`${cardClass} flex h-full min-h-[16rem] flex-col gap-2`}>
        <h3 className="font-medium text-slate-900">Pista manual</h3>
        <p className="text-sm text-slate-600">Solo en etapa de búsqueda del tesoro.</p>
      </section>
    )
  }

  if (participantes.length === 0) {
    return null
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)
    const texto = contenido.trim()
    if (!texto) {
      setError('Escribe el contenido de la pista.')
      return
    }
    try {
      await liberar.mutateAsync({
        contenido: texto,
        participanteId: destinatario === DESTINO_TODOS ? null : destinatario,
      })
      setContenido('')
      setDestinatario(DESTINO_TODOS)
      onSuccess?.()
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  return (
    <section className={`${cardClass} flex h-full min-h-[16rem] flex-col gap-3`}>
      <h3 className="font-medium text-slate-900">Pista manual</h3>
      <p className="text-sm text-slate-600">Envía una ayuda a uno o a todos.</p>
      {error && <ErrorState message={error} />}
      <form onSubmit={(e) => void handleSubmit(e)} className="grid flex-1 gap-3">
        <label className="block text-sm">
          <span className="text-slate-700">Destinatario</span>
          <select
            className={`${selectClass} mt-1 w-full`}
            value={destinatario}
            onChange={(e) => setDestinatario(e.target.value)}
          >
            <option value={DESTINO_TODOS}>Todos</option>
            {participantes.map((p) => (
              <option key={p.participanteId} value={p.participanteId}>
                {p.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="block min-h-0 flex-1 text-sm">
          <span className="text-slate-700">Contenido</span>
          <textarea
            className={`${inputClass} mt-1 h-24 w-full resize-none`}
            value={contenido}
            onChange={(e) => setContenido(e.target.value)}
            placeholder="Texto de la pista"
            maxLength={2000}
          />
        </label>
        <div className="mt-auto">
          <button type="submit" disabled={liberar.isPending} className={btnPrimary}>
            {liberar.isPending ? 'Enviando…' : 'Enviar'}
          </button>
        </div>
      </form>
    </section>
  )
}
