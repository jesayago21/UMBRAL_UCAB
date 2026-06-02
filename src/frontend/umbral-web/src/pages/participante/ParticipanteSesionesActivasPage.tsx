import { useState } from 'react'
import { Link } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useUnirseSesion, useSesionesDisponibles } from '@/hooks/useSesiones'
import {
  getParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, inputClass } from '@/styles/ui'

export function ParticipanteSesionesActivasPage() {
  const [codigo, setCodigo] = useState('')
  const [sesionId, setSesionId] = useState('')
  const [nombreParticipante, setNombreParticipante] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const { data: sesiones, isLoading, isError, error } = useSesionesDisponibles()
  const unirse = useUnirseSesion(sesionId)
  const inscripcion = getParticipanteSesionInscrita()

  const handleUnirse = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!sesionId || !codigo.trim()) {
      setFormError('Selecciona una sesión e ingresa el código de acceso.')
      return
    }
    setFormError(null)
    try {
      const result = await unirse.mutateAsync({
        codigoAcceso: codigo.trim(),
        nombreParticipante: nombreParticipante.trim() || undefined,
      })
      const titulo = sesiones?.find((s) => s.id === sesionId)?.titulo ?? 'Sesión'
      saveParticipanteSesionInscrita({
        sesionId,
        titulo,
        participanteId: result.participanteId,
        joinedAt: new Date().toISOString(),
        tipoSesion: 'Mision',
      })
      window.location.href = `/participante/sesiones/${sesionId}`
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Sesiones activas"
        description="Únete a una sesión de misión abierta con el código del operador."
      />

      {inscripcion && (
        <div className={cardClass}>
          <p className="text-sm text-slate-700">Ya estás inscrito en una sesión.</p>
          <Link
            to={`/participante/sesiones/${inscripcion.sesionId}`}
            className="mt-2 inline-block text-sm font-medium text-indigo-600"
          >
            Ir a mi partida
          </Link>
        </div>
      )}

      {isError && <ErrorState message={getApiErrorMessage(error)} />}
      {isLoading && <LoadingState />}

      {!isLoading && !isError && sesiones?.length === 0 && (
        <EmptyState title="No hay sesiones abiertas" description="Vuelve más tarde." />
      )}

      <ul className="space-y-2">
        {sesiones?.map((s) => (
          <li key={s.id}>
            <button
              type="button"
              onClick={() => setSesionId(s.id)}
              className={`w-full rounded-lg border p-3 text-left text-sm ${
                sesionId === s.id
                  ? 'border-indigo-500 bg-indigo-50'
                  : 'border-slate-200 bg-white hover:border-slate-300'
              }`}
            >
              <span className="font-medium">{s.titulo}</span>
              <span className="ml-2 text-slate-500">
                {s.estado} · {s.participantesInscritos} participantes
              </span>
            </button>
          </li>
        ))}
      </ul>

      <form onSubmit={handleUnirse} className={`${cardClass} space-y-3`}>
        <input
          required
          value={codigo}
          onChange={(e) => setCodigo(e.target.value)}
          placeholder="Código de acceso"
          className={inputClass}
        />
        <input
          value={nombreParticipante}
          onChange={(e) => setNombreParticipante(e.target.value)}
          placeholder="Nombre del participante (opcional)"
          className={inputClass}
        />
        {formError && <p className="text-sm text-red-600">{formError}</p>}
        <button type="submit" className={btnPrimary} disabled={unirse.isPending}>
          Unirse a la sesión
        </button>
      </form>
    </div>
  )
}
