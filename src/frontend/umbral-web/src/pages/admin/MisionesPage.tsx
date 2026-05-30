import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import {
  useActualizarMision,
  useCrearMision,
  useDesactivarMision,
  useMisiones,
} from '@/hooks/useMisiones'
import { getApiErrorMessage } from '@/services/apiClient'
import type { MisionDto } from '@/types/mision.types'

export function MisionesPage() {
  const [nombreFiltro, setNombreFiltro] = useState('')
  const [estadoFiltro, setEstadoFiltro] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const [editTarget, setEditTarget] = useState<MisionDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const params = {
    nombre: nombreFiltro || undefined,
    estado: estadoFiltro || undefined,
  }

  const { data, isLoading, isError, error } = useMisiones(params)
  const crear = useCrearMision()
  const actualizar = useActualizarMision()
  const desactivar = useDesactivarMision()

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
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDesactivar = async (mision: MisionDto) => {
    if (!window.confirm(`¿Desactivar la misión "${mision.nombre}"?`)) return
    try {
      await desactivar.mutateAsync(mision.id)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold">Misiones</h2>
          <p className="text-sm text-slate-500">HU-01..04 — catálogo de búsqueda del tesoro</p>
        </div>
        <button
          type="button"
          onClick={() => {
            setShowCreate(true)
            setEditTarget(null)
            setFormError(null)
          }}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
        >
          Nueva misión
        </button>
      </div>

      <div className="flex flex-wrap gap-3">
        <input
          value={nombreFiltro}
          onChange={(e) => setNombreFiltro(e.target.value)}
          placeholder="Filtrar por nombre"
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
        />
        <select
          value={estadoFiltro}
          onChange={(e) => setEstadoFiltro(e.target.value)}
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
        >
          <option value="">Todos los estados</option>
          <option value="activa">Activa</option>
          <option value="inactiva">Inactiva</option>
        </select>
      </div>

      {formError && <ErrorState message={formError} />}

      {showCreate && (
        <form
          onSubmit={handleCreate}
          className="space-y-3 rounded-xl border border-slate-200 bg-white p-4"
        >
          <h3 className="font-medium">Crear misión</h3>
          <input
            name="nombre"
            required
            placeholder="Nombre de la misión"
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <input
            name="etapaDescripcion"
            required
            placeholder="Descripción etapa 1"
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <input
            name="etapaQr"
            required
            placeholder="Código QR solución etapa 1"
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <label className="flex items-center gap-2 text-sm">
            <input name="activar" type="checkbox" />
            Activar al crear
          </label>
          <div className="flex gap-2">
            <button type="submit" className="rounded-lg bg-indigo-600 px-4 py-2 text-sm text-white">
              Guardar
            </button>
            <button
              type="button"
              onClick={() => setShowCreate(false)}
              className="rounded-lg border px-4 py-2 text-sm"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {editTarget && (
        <form
          onSubmit={handleUpdate}
          className="space-y-3 rounded-xl border border-indigo-200 bg-indigo-50/40 p-4"
        >
          <h3 className="font-medium">Editar misión</h3>
          <input
            name="nombre"
            required
            defaultValue={editTarget.nombre}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <select
            name="activar"
            defaultValue={editTarget.estado === 'Activa' ? 'activa' : 'inactiva'}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="activa">Activa</option>
            <option value="inactiva">Inactiva</option>
          </select>
          <div className="flex gap-2">
            <button type="submit" className="rounded-lg bg-indigo-600 px-4 py-2 text-sm text-white">
              Actualizar
            </button>
            <button
              type="button"
              onClick={() => setEditTarget(null)}
              className="rounded-lg border px-4 py-2 text-sm"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={getApiErrorMessage(error)} />}

      {data && (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <table className="min-w-full text-sm">
            <thead className="bg-slate-50 text-left text-slate-600">
              <tr>
                <th className="px-4 py-3">Nombre</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3">Etapas</th>
                <th className="px-4 py-3">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {data.map((mision) => (
                <tr key={mision.id} className="border-t border-slate-100">
                  <td className="px-4 py-3 font-medium">{mision.nombre}</td>
                  <td className="px-4 py-3">
                    <span
                      className={
                        mision.estado === 'Activa'
                          ? 'rounded-full bg-green-100 px-2 py-0.5 text-green-800'
                          : 'rounded-full bg-slate-100 px-2 py-0.5 text-slate-600'
                      }
                    >
                      {mision.estado}
                    </span>
                  </td>
                  <td className="px-4 py-3">{mision.totalEtapas}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={() => {
                          setEditTarget(mision)
                          setShowCreate(false)
                          setFormError(null)
                        }}
                        className="text-indigo-600 hover:underline"
                      >
                        Editar
                      </button>
                      {mision.estado === 'Activa' && (
                        <button
                          type="button"
                          onClick={() => void handleDesactivar(mision)}
                          className="text-red-600 hover:underline"
                        >
                          Desactivar
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {data.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-slate-500">
                    No hay misiones con esos filtros.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
