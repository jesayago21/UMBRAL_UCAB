import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import {
  useActualizarCategoria,
  useCategorias,
  useCrearCategoria,
  useEliminarCategoria,
} from '@/hooks/useCategorias'
import { getApiErrorMessage } from '@/services/apiClient'
import type { CategoriaDto } from '@/types/trivia.types'

export function CategoriasPage() {
  const [nombreFiltro, setNombreFiltro] = useState('')
  const [editTarget, setEditTarget] = useState<CategoriaDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const { data, isLoading, isError, error } = useCategorias({
    nombre: nombreFiltro || undefined,
  })
  const crear = useCrearCategoria()
  const actualizar = useActualizarCategoria()
  const eliminar = useEliminarCategoria()

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const form = new FormData(event.currentTarget)
    try {
      await crear.mutateAsync({ nombre: String(form.get('nombre')) })
      event.currentTarget.reset()
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
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDelete = async (categoria: CategoriaDto) => {
    if (!window.confirm(`¿Eliminar categoría "${categoria.nombre}"?`)) return
    try {
      await eliminar.mutateAsync(categoria.id)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold">Categorías de trivia</h2>
        <p className="text-sm text-slate-500">HU-28..31 — banco de categorías</p>
      </div>

      <input
        value={nombreFiltro}
        onChange={(e) => setNombreFiltro(e.target.value)}
        placeholder="Filtrar por nombre"
        className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
      />

      {formError && <ErrorState message={formError} />}

      <form
        onSubmit={handleCreate}
        className="flex flex-wrap items-end gap-3 rounded-xl border border-slate-200 bg-white p-4"
      >
        <label className="flex-1 text-sm">
          Nueva categoría
          <input
            name="nombre"
            required
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
          />
        </label>
        <button
          type="submit"
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white"
        >
          Crear
        </button>
      </form>

      {editTarget && (
        <form
          onSubmit={handleUpdate}
          className="flex flex-wrap items-end gap-3 rounded-xl border border-indigo-200 bg-indigo-50/40 p-4"
        >
          <label className="flex-1 text-sm">
            Editar categoría
            <input
              name="nombre"
              required
              defaultValue={editTarget.nombre}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
            />
          </label>
          <button type="submit" className="rounded-lg bg-indigo-600 px-4 py-2 text-sm text-white">
            Guardar
          </button>
          <button
            type="button"
            onClick={() => setEditTarget(null)}
            className="rounded-lg border px-4 py-2 text-sm"
          >
            Cancelar
          </button>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={getApiErrorMessage(error)} />}

      {data && (
        <ul className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-white">
          {data.map((categoria) => (
            <li
              key={categoria.id}
              className="flex items-center justify-between gap-4 px-4 py-3 text-sm"
            >
              <span className="font-medium">{categoria.nombre}</span>
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => setEditTarget(categoria)}
                  className="text-indigo-600 hover:underline"
                >
                  Editar
                </button>
                <button
                  type="button"
                  onClick={() => void handleDelete(categoria)}
                  className="text-red-600 hover:underline"
                >
                  Eliminar
                </button>
              </div>
            </li>
          ))}
          {data.length === 0 && (
            <li className="px-4 py-8 text-center text-slate-500">Sin categorías.</li>
          )}
        </ul>
      )}
    </div>
  )
}
