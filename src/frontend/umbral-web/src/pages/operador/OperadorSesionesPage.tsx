import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { SesionesOperativasList } from '@/components/operador/SesionesOperativasList'
import { ErrorState } from '@/components/shared/ErrorState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { useCrearSesionMision, useMisionesActivas, useSesionesOperativas } from '@/hooks/useSesiones'
import { createInitialSesionState, saveOperadorSesionState } from '@/lib/operadorSessionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, selectClass } from '@/styles/ui'

export function OperadorSesionesPage() {
  const navigate = useNavigate()
  const [misionId, setMisionId] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const {
    data: sesiones,
    isLoading: sesionesLoading,
    isError: sesionesError,
    error: sesionesErr,
    refetch: refetchSesiones,
  } = useSesionesOperativas()
  const { data: misiones, isLoading: misionesLoading } = useMisionesActivas()
  const crear = useCrearSesionMision()

  const handleCreate = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!misionId) {
      setFormError('Selecciona una misión activa.')
      return
    }
    setFormError(null)
    const mision = misiones?.find((m) => m.id === misionId)
    try {
      const created = await crear.mutateAsync(misionId)
      const state = createInitialSesionState(
        created.id,
        'Mision',
        misionId,
        created.misionNombre ?? mision?.nombre ?? 'Misión',
        created.codigoAcceso,
      )
      saveOperadorSesionState(state)
      showSuccess(`Sesión de misión creada. Código: ${created.codigoAcceso}`)
      navigate(`/operador/sesiones/${created.id}`)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Sesiones en vivo"
        description="Crea una sesión desde una misión activa (etapas BT y/o Trivia en secuencia)."
      />

      {successMessage && <SuccessAlert message={successMessage} onDismiss={clearSuccess} />}
      {formError && <ErrorState message={formError} />}

      {sesionesError && (
        <ErrorState
          message={getApiErrorMessage(sesionesErr)}
          onRetry={() => void refetchSesiones()}
        />
      )}

      {!sesionesError && <SesionesOperativasList sesiones={sesiones} isLoading={sesionesLoading} />}

      <form onSubmit={handleCreate} className={`${cardClass} space-y-4`}>
        <h3 className="text-sm font-semibold text-slate-900">Nueva sesión de misión</h3>
        <select
          required
          value={misionId}
          onChange={(e) => setMisionId(e.target.value)}
          className={selectClass}
          disabled={misionesLoading || crear.isPending}
        >
          <option value="">Selecciona misión activa…</option>
          {misiones?.map((m) => (
            <option key={m.id} value={m.id}>
              {m.nombre}
            </option>
          ))}
        </select>
        <button type="submit" className={btnPrimary} disabled={crear.isPending || misionesLoading}>
          {crear.isPending ? 'Creando…' : 'Crear sesión'}
        </button>
      </form>
    </div>
  )
}
