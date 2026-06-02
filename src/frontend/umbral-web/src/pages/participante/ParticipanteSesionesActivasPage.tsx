import { useEffect, useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import {
  useAbandonarSesion,
  useMiInscripcionParticipante,
  useUnirseSesion,
  useSesionesDisponibles,
} from '@/hooks/useSesiones'
import {
  clearParticipanteSesionInscrita,
  confirmarAbandonarSesion,
  getParticipanteSesionInscrita,
  rutaPartidaParticipante,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, btnSecondary, cardClass, inputClass } from '@/styles/ui'

export function ParticipanteSesionesActivasPage() {
  const [codigo, setCodigo] = useState('')
  const [sesionId, setSesionId] = useState('')
  const [nombreParticipante, setNombreParticipante] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const { data: sesiones, isLoading, isError, error } = useSesionesDisponibles()
  const {
    data: inscripcionServidor,
    isLoading: cargandoInscripcion,
    isError: errorInscripcion,
    error: errorInscripcionDetalle,
  } = useMiInscripcionParticipante()
  const unirse = useUnirseSesion(sesionId)
  const inscripcionLocal = getParticipanteSesionInscrita()
  const abandonar = useAbandonarSesion(inscripcionServidor?.sesionId ?? '')

  useEffect(() => {
    if (!inscripcionServidor) {
      if (!cargandoInscripcion && inscripcionLocal) clearParticipanteSesionInscrita()
      return
    }
    saveParticipanteSesionInscrita({
      sesionId: inscripcionServidor.sesionId,
      titulo: inscripcionServidor.titulo,
      participanteId: inscripcionServidor.participanteId,
      joinedAt: inscripcionLocal?.joinedAt ?? new Date().toISOString(),
      tipoSesion: 'Mision',
    })
  }, [inscripcionServidor, cargandoInscripcion, inscripcionLocal])

  const handleAbandonar = async () => {
    if (!inscripcionServidor?.sesionId) return
    if (!confirmarAbandonarSesion()) return
    setFormError(null)
    try {
      await abandonar.mutateAsync()
      clearParticipanteSesionInscrita()
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleUnirse = async (event: React.FormEvent) => {
    event.preventDefault()
    if (inscripcionServidor) {
      setFormError('Abandona tu sesión actual antes de unirte a otra.')
      return
    }
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

  if (cargandoInscripcion) return <LoadingState />

  if (inscripcionServidor) {
    const insc = {
    sesionId: inscripcionServidor.sesionId,
    titulo: inscripcionServidor.titulo,
    participanteId: inscripcionServidor.participanteId,
    joinedAt: inscripcionLocal?.joinedAt ?? new Date().toISOString(),
    tipoSesion: 'Mision' as const,
  }
    return (
      <div className="space-y-6">
        <PageHeader
          title="Inicio"
          description="Estás inscrito en una sesión. Continúa la partida o abandónala para unirte a otra."
        />
        <div className={cardClass}>
          <p className="font-medium text-slate-900">{inscripcionServidor.titulo}</p>
          <p className="mt-1 text-sm text-slate-600">Estado: {inscripcionServidor.estado}</p>
          <div className="mt-4 flex flex-wrap gap-2">
            <Link to={rutaPartidaParticipante(insc)} className={btnPrimary}>
              Ir a mi partida
            </Link>
            <button
              type="button"
              onClick={() => void handleAbandonar()}
              className={btnSecondary}
              disabled={abandonar.isPending}
            >
              Abandonar sesión
            </button>
          </div>
          {formError && <p className="mt-3 text-sm text-red-600">{formError}</p>}
        </div>
      </div>
    )
  }

  if (inscripcionLocal && !errorInscripcion) {
    return <Navigate to={rutaPartidaParticipante(inscripcionLocal)} replace />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Inicio"
        description="Únete a una sesión de misión abierta con el código del operador."
      />

      {errorInscripcion && (
        <ErrorState message={getApiErrorMessage(errorInscripcionDetalle)} />
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
