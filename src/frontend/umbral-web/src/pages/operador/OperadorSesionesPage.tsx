import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { PageHeader } from '@/components/admin/PageHeader'
import { SesionesOperativasList } from '@/components/operador/SesionesOperativasList'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useCategorias } from '@/hooks/useCategorias'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { usePreguntas } from '@/hooks/usePreguntas'
import {
  useCrearSesionBusquedaTesoro,
  useCrearSesionTrivia,
  useMisionesActivas,
  useSesionesOperativas,
} from '@/hooks/useSesiones'
import { createInitialSesionState, saveOperadorSesionState } from '@/lib/operadorSessionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, selectClass } from '@/styles/ui'
import type { TipoSesionApi } from '@/types/sesion.types'

type TabCrear = 'bt' | 'trivia'

export function OperadorSesionesPage() {
  const navigate = useNavigate()
  const [tab, setTab] = useState<TabCrear>('bt')
  const [misionId, setMisionId] = useState('')
  const [categoriaIds, setCategoriaIds] = useState<string[]>([])
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const {
    data: sesiones,
    isLoading: sesionesLoading,
    isError: sesionesError,
    error: sesionesErr,
    refetch: refetchSesiones,
  } = useSesionesOperativas()
  const {
    data: misiones,
    isLoading: misionesLoading,
    isError: misionesError,
    error: misionesErr,
    refetch: refetchMisiones,
  } = useMisionesActivas()
  const {
    data: categorias,
    isLoading: categoriasLoading,
    isError: categoriasError,
    error: categoriasErr,
    refetch: refetchCategorias,
  } = useCategorias()
  const { data: preguntas } = usePreguntas()
  const crearBt = useCrearSesionBusquedaTesoro()
  const crearTrivia = useCrearSesionTrivia()

  const preguntasPorCategoria = useMemo(() => {
    const map = new Map<string, number>()
    if (!preguntas) return map
    for (const p of preguntas) {
      if (!p.categoriaId) continue
      map.set(p.categoriaId, (map.get(p.categoriaId) ?? 0) + 1)
    }
    return map
  }, [preguntas])

  const toggleCategoria = (id: string) => {
    setCategoriaIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    )
  }

  const afterCreate = (
    created: { id: string; codigoAcceso: string },
    tipoSesion: TipoSesionApi,
    misionIdValue: string,
    titulo: string,
  ) => {
    const state = createInitialSesionState(
      created.id,
      tipoSesion,
      misionIdValue,
      titulo,
      created.codigoAcceso,
    )
    saveOperadorSesionState(state)
    showSuccess(`Sesión creada. Código: ${created.codigoAcceso}`)
    navigate(`/operador/sesiones/${created.id}`)
  }

  const handleCreateBt = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!misionId) {
      setFormError('Selecciona una misión activa.')
      return
    }
    setFormError(null)
    const mision = misiones?.find((m) => m.id === misionId)
    try {
      const created = await crearBt.mutateAsync({ misionId })
      afterCreate(created, 'BusquedaTesoro', misionId, mision?.nombre ?? 'Misión')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleCreateTrivia = async (event: React.FormEvent) => {
    event.preventDefault()
    if (categoriaIds.length === 0) {
      setFormError('Selecciona al menos una categoría.')
      return
    }
    setFormError(null)
    const nombres = categorias
      ?.filter((c) => categoriaIds.includes(c.id))
      .map((c) => c.nombre)
      .join(', ')
    try {
      const created = await crearTrivia.mutateAsync({ categoriaIds })
      afterCreate(created, 'Trivia', '', nombres ?? 'Trivia')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const isCreating = crearBt.isPending || crearTrivia.isPending

  return (
    <div className="space-y-6">
      <PageHeader
        title="Sesiones en vivo"
        description="Crea y monitoriza sesiones de búsqueda del tesoro o de trivia: código de acceso, equipos, inicio, pausa y ranking."
      />

      {successMessage && (
        <SuccessAlert message={successMessage} onDismiss={clearSuccess} />
      )}
      {formError && <ErrorState message={formError} />}

      {sesionesError && (
        <ErrorState
          message={getApiErrorMessage(sesionesErr)}
          onRetry={() => void refetchSesiones()}
        />
      )}

      {!sesionesError && (
        <SesionesOperativasList sesiones={sesiones} isLoading={sesionesLoading} />
      )}

      <div className={`${cardClass} space-y-4`}>
        <div className="flex flex-wrap gap-2 border-b border-slate-200 pb-3">
          <button
            type="button"
            onClick={() => setTab('bt')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium ${
              tab === 'bt'
                ? 'bg-indigo-600 text-white'
                : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
            }`}
          >
            Búsqueda del tesoro
          </button>
          <button
            type="button"
            onClick={() => setTab('trivia')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium ${
              tab === 'trivia'
                ? 'bg-indigo-600 text-white'
                : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
            }`}
          >
            Trivia
          </button>
        </div>

        {tab === 'bt' && (
          <>
            {misionesLoading && <LoadingState label="Cargando misiones activas…" />}
            {misionesError && (
              <ErrorState
                message={getApiErrorMessage(misionesErr)}
                onRetry={() => void refetchMisiones()}
              />
            )}
            {misiones && misiones.length === 0 && !misionesLoading && (
              <EmptyState
                title="No hay misiones activas"
                description="Pide al administrador que active una misión en el catálogo."
              />
            )}
            {misiones && misiones.length > 0 && (
              <form onSubmit={(e) => void handleCreateBt(e)} className="space-y-4">
                <h3 className="font-medium text-slate-900">Nueva sesión de búsqueda</h3>
                <p className="text-sm text-slate-600">
                  Cada sesión recorre <strong>una misión completa</strong> (todas sus etapas).
                  Las pistas con liberación por tiempo quedan para la segunda entrega (WebSockets).
                </p>
                <label className="block max-w-md text-sm font-medium text-slate-700">
                  Misión activa
                  <select
                    value={misionId}
                    onChange={(e) => setMisionId(e.target.value)}
                    className={`${selectClass} mt-1 block w-full`}
                    required
                  >
                    <option value="">Seleccionar misión…</option>
                    {misiones.map((m) => (
                      <option key={m.id} value={m.id}>
                        {m.nombre}
                      </option>
                    ))}
                  </select>
                </label>
                <button type="submit" disabled={isCreating} className={btnPrimary}>
                  {crearBt.isPending ? 'Creando sesión…' : 'Crear sesión BT'}
                </button>
              </form>
            )}
          </>
        )}

        {tab === 'trivia' && (
          <>
            {categoriasLoading && <LoadingState label="Cargando categorías…" />}
            {categoriasError && (
              <ErrorState
                message={getApiErrorMessage(categoriasErr)}
                onRetry={() => void refetchCategorias()}
              />
            )}
            {categorias && categorias.length === 0 && !categoriasLoading && (
              <EmptyState
                title="No hay categorías"
                description="El administrador debe crear categorías y asignar preguntas antes de abrir una sesión de trivia."
              />
            )}
            {categorias && categorias.length > 0 && (
              <form onSubmit={(e) => void handleCreateTrivia(e)} className="space-y-4">
                <h3 className="font-medium text-slate-900">Nueva sesión de trivia</h3>
                <p className="text-sm text-slate-600">
                  Elige una o más <strong>categorías</strong>. Entran todas las preguntas activas
                  de esas categorías; las preguntas <strong>sin categoría no participan</strong>.
                  El lanzamiento en vivo queda para la segunda entrega.
                </p>
                <ul className="max-h-64 space-y-2 overflow-y-auto rounded-lg border border-slate-200 p-3">
                  {categorias.map((c) => {
                    const count = preguntasPorCategoria.get(c.id) ?? 0
                    return (
                      <li key={c.id}>
                        <label
                          className={`flex cursor-pointer gap-2 text-sm ${
                            count === 0 ? 'text-slate-400' : 'text-slate-800'
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={categoriaIds.includes(c.id)}
                            onChange={() => toggleCategoria(c.id)}
                            disabled={count === 0}
                            className="mt-0.5"
                          />
                          <span>
                            {c.nombre}
                            <span className="ml-1 text-xs text-slate-500">
                              ({count} pregunta{count === 1 ? '' : 's'})
                            </span>
                          </span>
                        </label>
                      </li>
                    )
                  })}
                </ul>
                <p className="text-xs text-slate-500">
                  {categoriaIds.length} categoría{categoriaIds.length === 1 ? '' : 's'}{' '}
                  seleccionada{categoriaIds.length === 1 ? '' : 's'}
                </p>
                <button type="submit" disabled={isCreating} className={btnPrimary}>
                  {crearTrivia.isPending ? 'Creando sesión…' : 'Crear sesión trivia'}
                </button>
              </form>
            )}
          </>
        )}
      </div>
    </div>
  )
}
