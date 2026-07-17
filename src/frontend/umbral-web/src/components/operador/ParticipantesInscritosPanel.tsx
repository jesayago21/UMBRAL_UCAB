import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { useExpulsarParticipante } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnDangerLink, cardClass } from '@/styles/ui'
import type { EstadoSesionUi, ParticipanteSesionDto } from '@/types/sesion.types'

interface ParticipantesInscritosPanelProps {
  sesionId: string
  participantes: ParticipanteSesionDto[]
  estadoSesion: EstadoSesionUi
  onExpulsado?: () => void
}

/** Lista de sala de espera + expulsión (HU-32). */
export function ParticipantesInscritosPanel({
  sesionId,
  participantes,
  estadoSesion,
  onExpulsado,
}: ParticipantesInscritosPanelProps) {
  const expulsar = useExpulsarParticipante(sesionId)
  const [error, setError] = useState<string | null>(null)
  const [expulsandoId, setExpulsandoId] = useState<string | null>(null)
  const puedeExpulsar = estadoSesion === 'enPreparacion'
  const total = participantes.length

  const handleExpulsar = async (participante: ParticipanteSesionDto) => {
    const motivo = window.prompt(
      `Motivo para expulsar a "${participante.nombre}" (obligatorio):`,
      'Nombre inapropiado',
    )
    if (motivo == null) return
    const motivoTrim = motivo.trim()
    if (!motivoTrim) {
      setError('El motivo de expulsión es obligatorio.')
      return
    }
    if (!window.confirm(`¿Expulsar a ${participante.nombre} de la sala?`)) return

    setError(null)
    setExpulsandoId(participante.participanteId)
    try {
      await expulsar.mutateAsync({
        participanteId: participante.participanteId,
        motivo: motivoTrim,
      })
      onExpulsado?.()
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setExpulsandoId(null)
    }
  }

  return (
    <section className={`${cardClass} flex h-[14rem] flex-col gap-3`}>
      <h3 className="shrink-0 font-medium text-slate-900">
        Participantes inscritos{' '}
        <span className="font-normal text-slate-500">({total})</span>
      </h3>

      {error && <ErrorState message={error} />}

      {total === 0 ? (
        <p className="text-sm text-slate-600">Ninguno aún.</p>
      ) : (
        <ul className="min-h-0 flex-1 divide-y divide-slate-100 overflow-y-auto rounded-lg border border-slate-200">
          {participantes.map((eq) => (
            <li
              key={eq.participanteId}
              className="flex flex-wrap items-center justify-between gap-2 px-4 py-3 text-sm"
            >
              <span className="font-medium text-slate-900">{eq.nombre}</span>
              {puedeExpulsar && (
                <button
                  type="button"
                  className={btnDangerLink}
                  disabled={expulsar.isPending && expulsandoId === eq.participanteId}
                  onClick={() => void handleExpulsar(eq)}
                >
                  {expulsandoId === eq.participanteId ? 'Expulsando…' : 'Expulsar'}
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
