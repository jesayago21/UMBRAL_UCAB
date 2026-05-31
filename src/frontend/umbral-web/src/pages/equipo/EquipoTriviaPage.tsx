import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import {
  SESIONES_DISPONIBLES_TRIVIA_KEY,
  useSesionesDisponiblesTrivia,
} from '@/hooks/useSesiones'
import {
  clearEquipoSesionInscrita,
  getEquipoSesionInscrita,
  rutaPartidaEquipo,
  saveEquipoSesionInscrita,
} from '@/lib/equipoSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { unirseSesion } from '@/services/sesionService'
import { btnPrimary, btnSecondary, cardClass, inputClass } from '@/styles/ui'

export function EquipoTriviaPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const inscripcion = getEquipoSesionInscrita()
  const inscripcionTrivia = inscripcion?.tipoSesion === 'Trivia' ? inscripcion : null
  const { data, isLoading, isError, error, refetch } = useSesionesDisponiblesTrivia()
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()
  const [joiningId, setJoiningId] = useState<string | null>(null)
  const [codigo, setCodigo] = useState('')
  const [formError, setFormError] = useState<string | null>(null)

  const unirse = useMutation({
    mutationFn: ({
      sesionId,
      codigoAcceso,
      titulo,
    }: {
      sesionId: string
      codigoAcceso: string
      titulo: string
    }) => unirseSesion(sesionId, { codigoAcceso }).then((res) => ({ ...res, sesionId, titulo })),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_TRIVIA_KEY })
    },
  })

  const handleUnirse = async (sesionId: string, titulo: string) => {
    if (!codigo.trim()) {
      setFormError('Ingresa el código de acceso de la sesión.')
      return
    }
    setJoiningId(sesionId)
    setFormError(null)
    try {
      const res = await unirse.mutateAsync({
        sesionId,
        codigoAcceso: codigo.trim(),
        titulo,
      })
      saveEquipoSesionInscrita({
        sesionId,
        titulo,
        equipoId: res.equipoId,
        joinedAt: new Date().toISOString(),
        tipoSesion: 'Trivia',
      })
      showSuccess('Te uniste a la sesión trivia. Revisa las preguntas cuando quieras.')
      setCodigo('')
      navigate(`/equipo/trivia/${sesionId}`)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    } finally {
      setJoiningId(null)
    }
  }

  const handleSalirSesion = () => {
    clearEquipoSesionInscrita()
    void refetch()
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Sesiones de trivia"
        description="Sesiones abiertas a inscripción. Pide al operador el código de la sesión."
      />

      {successMessage && (
        <SuccessAlert message={successMessage} onDismiss={clearSuccess} />
      )}
      {formError && <ErrorState message={formError} />}

      {inscripcionTrivia && (
        <section className={`${cardClass} space-y-3`}>
          <h3 className="text-sm font-semibold text-slate-900">Tu inscripción actual</h3>
          <p className="text-sm text-slate-600">
            Estás inscrito en <strong>{inscripcionTrivia.titulo}</strong>.
          </p>
          <div className="flex flex-wrap gap-2">
            <Link to={rutaPartidaEquipo(inscripcionTrivia)} className={btnPrimary}>
              Ir a mi partida
            </Link>
            <button type="button" onClick={handleSalirSesion} className={btnSecondary}>
              Abandonar inscripción (local)
            </button>
          </div>
          <p className="text-xs text-slate-500">
            “Abandonar” solo borra el recordatorio en este navegador; el operador sigue viendo tu
            equipo inscrito.
          </p>
        </section>
      )}

      {!inscripcionTrivia && (
        <section className={`${cardClass} space-y-3`}>
          <label className="block text-sm font-medium text-slate-700">
            Código de acceso
            <input
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.toUpperCase())}
              className={`${inputClass} mt-1 max-w-xs font-mono uppercase`}
              placeholder="Ej. A1B2C3D4"
              autoComplete="off"
            />
          </label>
          <p className="text-xs text-slate-500">
            Un solo código por sesión — compártelo con todos los jugadores de esa partida.
          </p>
        </section>
      )}

      {isLoading && <LoadingState label="Cargando sesiones…" />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && data.length === 0 && !isLoading && !inscripcionTrivia && (
        <EmptyState
          title="No hay sesiones trivia abiertas"
          description="Cuando el operador abra inscripción en una sesión trivia, aparecerá aquí."
        />
      )}

      {data && data.length > 0 && !inscripcionTrivia && (
        <ul className="divide-y divide-slate-100 rounded-lg border border-slate-200 bg-white">
          {data.map((s) => (
            <li
              key={s.id}
              className="flex flex-wrap items-center justify-between gap-3 px-4 py-4"
            >
              <div>
                <p className="font-medium text-slate-900">{s.titulo}</p>
                <p className="text-xs text-slate-500">
                  {s.equiposInscritos} equipo{s.equiposInscritos === 1 ? '' : 's'} inscrito
                  {s.equiposInscritos === 1 ? '' : 's'}
                </p>
              </div>
              <button
                type="button"
                disabled={unirse.isPending}
                onClick={() => void handleUnirse(s.id, s.titulo)}
                className={btnPrimary}
              >
                {unirse.isPending && joiningId === s.id ? 'Uniéndose…' : 'Unirse'}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
