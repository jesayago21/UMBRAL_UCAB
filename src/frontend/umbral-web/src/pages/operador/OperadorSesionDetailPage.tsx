import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { EquiposInscritosPanel } from '@/components/operador/EquiposInscritosPanel'
import { SesionCodigoAccesoPanel } from '@/components/operador/SesionCodigoAccesoPanel'
import { SesionTimerPanel } from '@/components/operador/SesionTimerPanel'
import { PageHeader } from '@/components/admin/PageHeader'
import { SesionRankingPanel } from '@/components/shared/SesionRankingPanel'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import {
  SESIONES_OPERATIVAS_KEY,
  SESION_DETALLE_KEY,
  RANKING_KEY,
  useAbrirInscripcionSesion,
  useCancelarSesion,
  useFinalizarSesion,
  useIniciarSesion,
  usePausarSesion,
  useReanudarSesion,
  useSesionDetalle,
} from '@/hooks/useSesiones'
import { detalleToOperadorState, saveOperadorSesionState } from '@/lib/operadorSessionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import {
  btnDangerLink,
  btnPrimary,
  btnSecondary,
  cardClass,
} from '@/styles/ui'
import type { EstadoSesionUi, OperadorSesionState } from '@/types/sesion.types'

const ESTADO_LABEL: Record<EstadoSesionUi, string> = {
  programada: 'Programada',
  enPreparacion: 'En preparación',
  activa: 'Activa',
  pausada: 'Pausada',
  finalizada: 'Finalizada',
  cancelada: 'Cancelada',
}

const ESTADO_CLASS: Record<EstadoSesionUi, string> = {
  programada: 'bg-slate-100 text-slate-700',
  enPreparacion: 'bg-blue-100 text-blue-800',
  activa: 'bg-green-100 text-green-800',
  pausada: 'bg-amber-100 text-amber-900',
  finalizada: 'bg-indigo-100 text-indigo-800',
  cancelada: 'bg-red-100 text-red-800',
}

export function OperadorSesionDetailPage() {
  const { id: sesionId } = useParams<{ id: string }>()
  const queryClient = useQueryClient()

  const [sesion, setSesion] = useState<OperadorSesionState | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const {
    data: detalle,
    isLoading: detalleLoading,
    isError: detalleError,
    error: detalleErr,
    refetch: refetchDetalle,
  } = useSesionDetalle(sesionId ?? '')

  const abrirInscripcion = useAbrirInscripcionSesion(sesionId ?? '')
  const iniciar = useIniciarSesion(sesionId ?? '')
  const pausar = usePausarSesion(sesionId ?? '')
  const reanudar = useReanudarSesion(sesionId ?? '')
  const finalizar = useFinalizarSesion(sesionId ?? '')
  const cancelar = useCancelarSesion(sesionId ?? '')

  const terminal = sesion?.estado === 'finalizada' || sesion?.estado === 'cancelada'

  useEffect(() => {
    if (!detalle) return
    const next = detalleToOperadorState(detalle)
    setSesion(next)
    saveOperadorSesionState(next)
  }, [detalle])

  const invalidateSesion = async () => {
    await queryClient.invalidateQueries({ queryKey: SESIONES_OPERATIVAS_KEY })
    if (sesionId) {
      await queryClient.invalidateQueries({ queryKey: [...SESION_DETALLE_KEY, sesionId] })
      await queryClient.invalidateQueries({ queryKey: [...RANKING_KEY, sesionId] })
    }
  }

  const isSaving =
    abrirInscripcion.isPending ||
    iniciar.isPending ||
    pausar.isPending ||
    reanudar.isPending ||
    finalizar.isPending ||
    cancelar.isPending

  const runAction = async (action: () => Promise<unknown>, success: string) => {
    setFormError(null)
    try {
      await action()
      await invalidateSesion()
      await refetchDetalle()
      showSuccess(success)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleCancelar = async () => {
    if (!sesion) return
    const motivo = window.prompt('Motivo de cancelación:')
    if (!motivo?.trim()) return
    setFormError(null)
    try {
      await cancelar.mutateAsync({ motivo: motivo.trim() })
      await invalidateSesion()
      await refetchDetalle()
      showSuccess('Sesión cancelada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  if (!sesionId) {
    return <ErrorState message="Identificador de sesión no válido." />
  }

  if (detalleLoading && !sesion) {
    return <LoadingState label="Cargando sesión…" />
  }

  if (detalleError && !sesion) {
    return (
      <ErrorState
        message={getApiErrorMessage(detalleErr)}
        onRetry={() => void refetchDetalle()}
      />
    )
  }

  if (!sesion) {
    return <LoadingState label="Cargando sesión…" />
  }

  const canAbrirInscripcion = sesion.estado === 'programada'
  const canIniciar = sesion.estado === 'enPreparacion' && sesion.equipos.length > 0
  const canPausar = sesion.estado === 'activa'
  const canReanudar = sesion.estado === 'pausada'
  const canFinalizar = sesion.estado === 'activa' || sesion.estado === 'pausada'
  const canCancelar = !terminal

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <Link to="/operador/sesiones" className="text-indigo-600 hover:underline">
          ← Sesiones en curso
        </Link>
      </div>

      <PageHeader
        title={sesion.misionNombre}
        description={`Sesión ${sesionId}`}
        action={
          <span
            className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTADO_CLASS[sesion.estado]}`}
          >
            {ESTADO_LABEL[sesion.estado]}
          </span>
        }
      />

      {successMessage && (
        <SuccessAlert message={successMessage} onDismiss={clearSuccess} />
      )}
      {formError && <ErrorState message={formError} />}

      <SesionCodigoAccesoPanel
        codigoAcceso={sesion.codigoAcceso}
        estado={detalle?.estado ?? 'Programada'}
        canAbrirInscripcion={canAbrirInscripcion}
        isSaving={isSaving}
        onAbrirInscripcion={() =>
          void runAction(() => abrirInscripcion.mutateAsync(), 'Inscripción abierta para jugadores.')
        }
      />

      <SesionTimerPanel
        estado={sesion.estado}
        iniciadaEn={sesion.iniciadaEn ?? null}
        finalizadaEn={sesion.finalizadaEn ?? null}
        etapaActualOrden={sesion.etapaActualOrden ?? 0}
        totalEtapas={sesion.totalEtapas ?? 0}
        etapaDescripcion={sesion.etapaDescripcion ?? null}
        unidadProgreso={
          (detalle?.tipoSesion ?? sesion.tipoSesion) === 'Trivia' ? 'pregunta' : 'etapa'
        }
      />

      <EquiposInscritosPanel equipos={sesion.equipos} />

      <section className={`${cardClass} space-y-3`}>
        <h3 className="font-medium text-slate-900">Controles de sesión</h3>
        <div className="flex flex-wrap gap-2">
          {canIniciar && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => void runAction(() => iniciar.mutateAsync(), 'Sesión iniciada.')}
              className={btnPrimary}
            >
              Iniciar
            </button>
          )}
          {canPausar && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => void runAction(() => pausar.mutateAsync(), 'Sesión pausada.')}
              className={btnSecondary}
            >
              Pausar
            </button>
          )}
          {canReanudar && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => void runAction(() => reanudar.mutateAsync(), 'Sesión reanudada.')}
              className={btnPrimary}
            >
              Reanudar
            </button>
          )}
          {canFinalizar && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => {
                if (!window.confirm('¿Finalizar la sesión?')) return
                void runAction(() => finalizar.mutateAsync(), 'Sesión finalizada.')
              }}
              className={btnSecondary}
            >
              Finalizar
            </button>
          )}
          {canCancelar && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => void handleCancelar()}
              className={btnDangerLink}
            >
              Cancelar sesión
            </button>
          )}
        </div>
        {terminal && (
          <p className="text-sm text-slate-600">La sesión está cerrada. No se permiten más cambios.</p>
        )}
      </section>

      <SesionRankingPanel
        sesionId={sesionId ?? ''}
        enabled={Boolean(sesion && sesion.equipos.length > 0)}
        emptyEquiposMessage="Espera a que los jugadores se unan con el código de sesión."
      />
    </div>
  )
}
