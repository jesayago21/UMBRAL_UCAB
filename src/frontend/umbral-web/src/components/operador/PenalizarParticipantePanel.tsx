import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { useAplicarPenalizacion } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, inputClass, selectClass } from '@/styles/ui'
import type { ParticipanteSesionDto } from '@/types/sesion.types'

interface PenalizarParticipantePanelProps {
  sesionId: string
  participantes: ParticipanteSesionDto[]
  sesionActiva: boolean
  onSuccess?: () => void
}

export function PenalizarParticipantePanel({
  sesionId,
  participantes,
  sesionActiva,
  onSuccess,
}: PenalizarParticipantePanelProps) {
  const [participanteId, setParticipanteId] = useState('')
  const [puntos, setPuntos] = useState(10)
  const [motivo, setMotivo] = useState('')
  const [error, setError] = useState<string | null>(null)

  const penalizar = useAplicarPenalizacion(sesionId)

  if (!sesionActiva) {
    return (
      <section className={`${cardClass} flex h-full min-h-[16rem] flex-col gap-2`}>
        <h3 className="font-medium text-slate-900">Penalización</h3>
        <p className="text-sm text-slate-600">Disponible con sesión activa.</p>
      </section>
    )
  }

  if (participantes.length === 0) {
    return null
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)
    if (!participanteId) {
      setError('Selecciona un participante.')
      return
    }
    if (!motivo.trim()) {
      setError('El motivo es obligatorio.')
      return
    }
    if (puntos <= 0) {
      setError('Los puntos a restar deben ser mayores a cero.')
      return
    }
    try {
      await penalizar.mutateAsync({
        participanteId,
        puntos,
        motivo: motivo.trim(),
      })
      setMotivo('')
      setParticipanteId('')
      onSuccess?.()
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  return (
    <section className={`${cardClass} flex h-full min-h-[16rem] flex-col gap-3`}>
      <h3 className="font-medium text-slate-900">Penalización</h3>
      <p className="text-sm text-slate-600">Resta puntos a un participante.</p>
      {error && <ErrorState message={error} />}
      <form onSubmit={(e) => void handleSubmit(e)} className="grid flex-1 gap-3 sm:grid-cols-2">
        <label className="block text-sm">
          <span className="text-slate-700">Participante</span>
          <select
            className={`${selectClass} mt-1 w-full`}
            value={participanteId}
            onChange={(e) => setParticipanteId(e.target.value)}
          >
            <option value="">Seleccionar…</option>
            {participantes.map((p) => (
              <option key={p.participanteId} value={p.participanteId}>
                {p.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm">
          <span className="text-slate-700">Puntos a restar</span>
          <input
            type="number"
            min={1}
            className={`${inputClass} mt-1 w-full`}
            value={puntos}
            onChange={(e) => setPuntos(Number(e.target.value))}
          />
        </label>
        <label className="block text-sm sm:col-span-2">
          <span className="text-slate-700">Motivo</span>
          <input
            className={`${inputClass} mt-1 w-full`}
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            placeholder="Motivo"
          />
        </label>
        <div className="mt-auto sm:col-span-2">
          <button type="submit" disabled={penalizar.isPending} className={btnPrimary}>
            {penalizar.isPending ? 'Aplicando…' : 'Aplicar'}
          </button>
        </div>
      </form>
    </section>
  )
}
