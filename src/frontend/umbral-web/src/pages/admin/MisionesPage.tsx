import { useState } from 'react'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import {
  useActualizarMision,
  useCrearMision,
  useDesactivarMision,
  useMisiones,
} from '@/hooks/useMisiones'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { getApiErrorMessage } from '@/services/apiClient'
import {
  btnDangerLink,
  btnLink,
  btnPrimary,
  btnSecondary,
  cardClass,
  cardHighlightClass,
  inputClass,
  selectClass,
} from '@/styles/ui'
import type { MisionDto } from '@/types/mision.types'

export function MisionesPage() {
  const [nombreFiltro, setNombreFiltro] = useState('')
  const [estadoFiltro, setEstadoFiltro] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const [editTarget, setEditTarget] = useState<MisionDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const params = {
    nombre: nombreFiltro || undefined,
    estado: estadoFiltro || undefined,
  }

  const { data, isLoading, isError, error, refetch, isFetching } = useMisiones(params)
  const crear = useCrearMision()
  const actualizar = useActualizarMision()
  const desactivar = useDesactivarMision()

  const isSaving = crear.isPending || actualizar.isPending || desactivar.isPending
  const hasFilters = Boolean(nombreFiltro || estadoFiltro)

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const form = new FormData(event.currentTarget)
    try {
      await crear.mutateAsync({
        nombre: String(form.get('nombre')),
        activar: form.get('activar') === 'on',
        etapas: [
          {
            descripcion: String(form.get('etapaDescripcion')),
            codigoQrSolucion: String(form.get('etapaQr')),
            pistas: [],
          },
        ],
      })
      setShowCreate(false)
      event.currentTarget.reset()
      showSuccess('Misión creada correctamente.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleUpdate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editTarget) return
    setFormError(null)
    const form = new FormData(event.currentTarget)
    const activarRaw = form.get('activar')
    try {
      await actualizar.mutateAsync({
        id: editTarget.id,
        body: {
          nombre: String(form.get('nombre')),
          activar: activarRaw === 'activa' ? true : activarRaw === 'inactiva' ? false : null,
        },
      })
      setEditTarget(null)
      showSuccess('Misión actualizada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDesactivar = async (mision: MisionDto) => {
    if (!window.confirm(`¿Desactivar la misión "${mision.nombre}"?`)) return
    setFormError(null)
    try {
      await desactivar.mutateAsync(mision.id)
      showSuccess(`Misión "${mision.nombre}" desactivada.`)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const clearFilters = () => {
    setNombreFiltro('')
    setEstadoFiltro('')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Misiones"
        description="Catálogo de búsqueda del tesoro. Las misiones activas las usa el operador en sesiones."
        action={
          <button
            type="button"
            disabled={isSaving}
            onClick={() => {
              setShowCreate(true)
              setEditTarget(null)
              setFormError(null)
            }}
            className={btnPrimary}
          >
            Nueva misión
          </button>
        }
      />

      <div className="flex flex-wrap items-center gap-3">
        <input
          value={nombreFiltro}
          onChange={(e) => setNombreFiltro(e.target.value)}
          placeholder="Filtrar por nombre"
          className={`${inputClass} max-w-xs`}
          aria-label="Filtrar por nombre"
        />
        <select
          value={estadoFiltro}
          onChange={(e) => setEstadoFiltro(e.target.value)}
          className={selectClass}
          aria-label="Filtrar por estado"
        >
          <option value="">Todos los estados</option>
          <option value="activa">Activa</option>
          <option value="inactiva">Inactiva</option>
        </select>
        {hasFilters && (
          <button type="button" onClick={clearFilters} className={btnSecondary}>
            Limpiar filtros
          </button>
        )}
        {isFetching && !isLoading && (
          <span className="text-xs text-slate-500">Actualizando…</span>
        )}
      </div>

      {successMessage && (
        <SuccessAlert message={successMessage} onDismiss={clearSuccess} />
      )}
      {formError && <ErrorState message={formError} />}

      {showCreate && (
        <form onSubmit={handleCreate} className={`${cardClass} space-y-3`}>
          <h3 className="font-medium text-slate-900">Crear misión</h3>
          <input name="nombre" required placeholder="Nombre de la misión" className={inputClass} />
          <input
            name="etapaDescripcion"
            required
            placeholder="Descripción etapa 1"
            className={inputClass}
          />
          <input
            name="etapaQr"
            required
            placeholder="Código QR solución etapa 1"
            className={inputClass}
          />
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <input name="activar" type="checkbox" className="rounded border-slate-300" />
            Activar al crear (visible para el operador)
          </label>
          <div className="flex gap-2">
            <button type="submit" disabled={isSaving} className={btnPrimary}>
              {crear.isPending ? 'Guardando…' : 'Guardar'}
            </button>
            <button
              type="button"
              disabled={isSaving}
              onClick={() => setShowCreate(false)}
              className={btnSecondary}
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {editTarget && (
        <form onSubmit={handleUpdate} className={`${cardHighlightClass} space-y-3`}>
          <h3 className="font-medium text-slate-900">Editar misión</h3>
          <input
            name="nombre"
            required
            defaultValue={editTarget.nombre}
            className={inputClass}
          />
          <select
            name="activar"
            defaultValue={editTarget.estado === 'Activa' ? 'activa' : 'inactiva'}
            className={inputClass}
          >
            <option value="activa">Activa</option>
            <option value="inactiva">Inactiva</option>
          </select>
          <div className="flex gap-2">
            <button type="submit" disabled={isSaving} className={btnPrimary}>
              {actualizar.isPending ? 'Guardando…' : 'Actualizar'}
            </button>
            <button
              type="button"
              disabled={isSaving}
              onClick={() => setEditTarget(null)}
              className={btnSecondary}
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && data.length === 0 && !isLoading && (
        <EmptyState
          title={hasFilters ? 'Sin resultados' : 'Aún no hay misiones'}
          description={
            hasFilters
              ? 'Prueba otros filtros o limpia la búsqueda.'
              : 'Crea la primera misión para que el operador pueda abrir sesiones.'
          }
        />
      )}

      {data && data.length > 0 && (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <table className="min-w-full text-sm">
            <thead className="bg-slate-50 text-left text-slate-600">
              <tr>
                <th className="px-4 py-3 font-medium">Nombre</th>
                <th className="px-4 py-3 font-medium">Estado</th>
                <th className="px-4 py-3 font-medium">Etapas</th>
                <th className="px-4 py-3 font-medium">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {data.map((mision) => (
                <tr key={mision.id} className="border-t border-slate-100 hover:bg-slate-50/50">
                  <td className="px-4 py-3 font-medium text-slate-900">{mision.nombre}</td>
                  <td className="px-4 py-3">
                    <span
                      className={
                        mision.estado === 'Activa'
                          ? 'rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800'
                          : 'rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-600'
                      }
                    >
                      {mision.estado}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-slate-600">{mision.totalEtapas}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-3">
                      <button
                        type="button"
                        disabled={isSaving}
                        onClick={() => {
                          setEditTarget(mision)
                          setShowCreate(false)
                          setFormError(null)
                        }}
                        className={btnLink}
                      >
                        Editar
                      </button>
                      {mision.estado === 'Activa' && (
                        <button
                          type="button"
                          disabled={isSaving}
                          onClick={() => void handleDesactivar(mision)}
                          className={btnDangerLink}
                        >
                          Desactivar
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
