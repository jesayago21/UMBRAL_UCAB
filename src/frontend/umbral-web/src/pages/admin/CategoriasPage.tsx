import { useState } from 'react'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import {
  useActualizarCategoria,
  useCategorias,
  useCrearCategoria,
  useEliminarCategoria,
} from '@/hooks/useCategorias'
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
} from '@/styles/ui'
import type { CategoriaDto } from '@/types/trivia.types'

export function CategoriasPage() {
  const [nombreFiltro, setNombreFiltro] = useState('')
  const [editTarget, setEditTarget] = useState<CategoriaDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const { data, isLoading, isError, error, refetch, isFetching } = useCategorias({
    nombre: nombreFiltro || undefined,
  })
  const crear = useCrearCategoria()
  const actualizar = useActualizarCategoria()
  const eliminar = useEliminarCategoria()

  const isSaving = crear.isPending || actualizar.isPending || eliminar.isPending
  const hasFilters = Boolean(nombreFiltro)

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const formEl = event.currentTarget
    const form = new FormData(formEl)
    try {
      await crear.mutateAsync({ nombre: String(form.get('nombre')) })
      formEl.reset()
      showSuccess('Categoría creada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleUpdate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editTarget) return
    setFormError(null)
    const form = new FormData(event.currentTarget)
    try {
      await actualizar.mutateAsync({
        id: editTarget.id,
        body: { nombre: String(form.get('nombre')) },
      })
      setEditTarget(null)
      showSuccess('Categoría actualizada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDelete = async (categoria: CategoriaDto) => {
    if (!window.confirm(`¿Eliminar categoría "${categoria.nombre}"? Las preguntas quedarán sin categoría.`)) {
      return
    }
    setFormError(null)
    try {
      await eliminar.mutateAsync(categoria.id)
      showSuccess('Categoría eliminada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Categorías"
        description="Organiza el banco de trivia. El nombre debe ser único."
      />

      <div className="flex flex-wrap items-center gap-3">
        <input
          value={nombreFiltro}
          onChange={(e) => setNombreFiltro(e.target.value)}
          placeholder="Filtrar por nombre"
          className={`${inputClass} max-w-xs`}
          aria-label="Filtrar por nombre"
        />
        {hasFilters && (
          <button type="button" onClick={() => setNombreFiltro('')} className={btnSecondary}>
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

      <form onSubmit={handleCreate} className={`${cardClass} flex flex-wrap items-end gap-3`}>
        <label className="min-w-[200px] flex-1 text-sm font-medium text-slate-700">
          Nueva categoría
          <input name="nombre" required className={`${inputClass} mt-1`} />
        </label>
        <button type="submit" disabled={isSaving} className={btnPrimary}>
          {crear.isPending ? 'Creando…' : 'Crear'}
        </button>
      </form>

      {editTarget && (
        <form onSubmit={handleUpdate} className={`${cardHighlightClass} flex flex-wrap items-end gap-3`}>
          <label className="min-w-[200px] flex-1 text-sm font-medium text-slate-700">
            Editar categoría
            <input
              name="nombre"
              required
              defaultValue={editTarget.nombre}
              className={`${inputClass} mt-1`}
            />
          </label>
          <button type="submit" disabled={isSaving} className={btnPrimary}>
            Guardar
          </button>
          <button
            type="button"
            disabled={isSaving}
            onClick={() => setEditTarget(null)}
            className={btnSecondary}
          >
            Cancelar
          </button>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && data.length === 0 && !isLoading && (
        <EmptyState
          title={hasFilters ? 'Sin resultados' : 'Sin categorías'}
          description={
            hasFilters
              ? 'No hay categorías que coincidan con el filtro.'
              : 'Crea categorías antes de asignarlas a las preguntas.'
          }
        />
      )}

      {data && data.length > 0 && (
        <ul className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-white shadow-sm">
          {data.map((categoria) => (
            <li
              key={categoria.id}
              className="flex items-center justify-between gap-4 px-4 py-3 text-sm hover:bg-slate-50/50"
            >
              <span className="font-medium text-slate-900">{categoria.nombre}</span>
              <div className="flex gap-3">
                <button
                  type="button"
                  disabled={isSaving}
                  onClick={() => setEditTarget(categoria)}
                  className={btnLink}
                >
                  Editar
                </button>
                <button
                  type="button"
                  disabled={isSaving}
                  onClick={() => void handleDelete(categoria)}
                  className={btnDangerLink}
                >
                  Eliminar
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
