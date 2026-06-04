import { useState } from 'react'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useCategorias } from '@/hooks/useCategorias'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import {
  useActualizarPregunta,
  useCrearPregunta,
  useEliminarPregunta,
  usePreguntas,
} from '@/hooks/usePreguntas'
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
import type { DificultadPregunta, OpcionRespuestaDto, PreguntaDto } from '@/types/trivia.types'

const DIFICULTADES: DificultadPregunta[] = ['Facil', 'Media', 'Dificil']

const emptyOpciones = (): OpcionRespuestaDto[] => [
  { texto: '', esCorrecta: true },
  { texto: '', esCorrecta: false },
  { texto: '', esCorrecta: false },
]

function validateOpciones(opciones: OpcionRespuestaDto[]): string | null {
  if (opciones.length < 3) return 'Se requieren al menos 3 opciones.'
  if (opciones.some((o) => !o.texto.trim())) return 'Todas las opciones deben tener texto.'
  if (opciones.filter((o) => o.esCorrecta).length !== 1) {
    return 'Debe haber exactamente una opción correcta.'
  }
  return null
}

export function PreguntasPage() {
  const [enunciadoFiltro, setEnunciadoFiltro] = useState('')
  const [dificultadFiltro, setDificultadFiltro] = useState('')
  const [categoriaFiltro, setCategoriaFiltro] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const [editTarget, setEditTarget] = useState<PreguntaDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [createOpciones, setCreateOpciones] = useState<OpcionRespuestaDto[]>(emptyOpciones)
  const [editOpciones, setEditOpciones] = useState<OpcionRespuestaDto[]>(emptyOpciones)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const { data: categorias } = useCategorias()
  const { data, isLoading, isError, error, refetch, isFetching } = usePreguntas({
    enunciado: enunciadoFiltro || undefined,
    dificultad: dificultadFiltro || undefined,
    categoriaId: categoriaFiltro || undefined,
  })
  const crear = useCrearPregunta()
  const actualizar = useActualizarPregunta()
  const eliminar = useEliminarPregunta()

  const isSaving = crear.isPending || actualizar.isPending || eliminar.isPending
  const hasFilters = Boolean(enunciadoFiltro || dificultadFiltro || categoriaFiltro)

  const categoriaNombre = (id: string | null) =>
    categorias?.find((c) => c.id === id)?.nombre ?? '—'

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const validation = validateOpciones(createOpciones)
    if (validation) {
      setFormError(validation)
      return
    }
    const formEl = event.currentTarget
    const form = new FormData(formEl)
    const categoriaId = String(form.get('categoriaId') || '')
    try {
      await crear.mutateAsync({
        enunciado: String(form.get('enunciado')),
        dificultad: String(form.get('dificultad')),
        categoriaId: categoriaId || null,
        opciones: createOpciones,
      })
      setShowCreate(false)
      setCreateOpciones(emptyOpciones())
      showSuccess('Pregunta creada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleUpdate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editTarget) return
    setFormError(null)
    const validation = validateOpciones(editOpciones)
    if (validation) {
      setFormError(validation)
      return
    }
    const form = new FormData(event.currentTarget)
    const categoriaId = String(form.get('categoriaId') || '')
    try {
      await actualizar.mutateAsync({
        id: editTarget.id,
        body: {
          enunciado: String(form.get('enunciado')),
          dificultad: String(form.get('dificultad')),
          categoriaId: categoriaId || null,
          opciones: editOpciones,
        },
      })
      setEditTarget(null)
      showSuccess('Pregunta actualizada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDelete = async (pregunta: PreguntaDto) => {
    if (!window.confirm('¿Eliminar esta pregunta?')) return
    setFormError(null)
    try {
      await eliminar.mutateAsync(pregunta.id)
      showSuccess('Pregunta eliminada.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const renderOpcionesEditor = (
    opciones: OpcionRespuestaDto[],
    setOpciones: (next: OpcionRespuestaDto[]) => void,
    radioGroupName: string,
  ) => (
    <div className="space-y-2">
      <p className="text-sm font-medium text-slate-700">Opciones (mín. 3, una correcta)</p>
      {opciones.map((opcion, index) => (
        <div key={index} className="flex items-center gap-2">
          <input
            type="radio"
            name={radioGroupName}
            checked={opcion.esCorrecta}
            onChange={() =>
              setOpciones(
                opciones.map((o, i) => ({ ...o, esCorrecta: i === index })),
              )
            }
          />
          <input
            value={opcion.texto}
            onChange={(e) => {
              const next = [...opciones]
              next[index] = { ...next[index], texto: e.target.value }
              setOpciones(next)
            }}
            placeholder={`Opción ${index + 1}`}
            className={`${inputClass} flex-1`}
          />
          {opciones.length > 3 && (
            <button
              type="button"
              disabled={isSaving}
              onClick={() => setOpciones(opciones.filter((_, i) => i !== index))}
              className={btnDangerLink}
            >
              Quitar
            </button>
          )}
        </div>
      ))}
      <button
        type="button"
        onClick={() => setOpciones([...opciones, { texto: '', esCorrecta: false }])}
        className={btnLink}
      >
        + Añadir opción
      </button>
    </div>
  )

  const clearFilters = () => {
    setEnunciadoFiltro('')
    setDificultadFiltro('')
    setCategoriaFiltro('')
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Preguntas"
        description="Banco de trivia: mínimo 3 opciones y exactamente una correcta."
        action={
          <button
            type="button"
            disabled={isSaving}
            onClick={() => {
              setShowCreate(true)
              setEditTarget(null)
              setFormError(null)
              setCreateOpciones(emptyOpciones())
            }}
            className={btnPrimary}
          >
            Nueva pregunta
          </button>
        }
      />

      <div className="flex flex-wrap items-center gap-3">
        <input
          value={enunciadoFiltro}
          onChange={(e) => setEnunciadoFiltro(e.target.value)}
          placeholder="Filtrar por enunciado"
          className={`${inputClass} max-w-xs`}
          aria-label="Filtrar por enunciado"
        />
        <select
          value={dificultadFiltro}
          onChange={(e) => setDificultadFiltro(e.target.value)}
          className={selectClass}
          aria-label="Filtrar por dificultad"
        >
          <option value="">Todas las dificultades</option>
          {DIFICULTADES.map((d) => (
            <option key={d} value={d}>
              {d}
            </option>
          ))}
        </select>
        <select
          value={categoriaFiltro}
          onChange={(e) => setCategoriaFiltro(e.target.value)}
          className={selectClass}
          aria-label="Filtrar por categoría"
        >
          <option value="">Todas las categorías</option>
          {categorias?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.nombre}
            </option>
          ))}
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
        <form
          onSubmit={handleCreate}
          className={`${cardClass} space-y-4`}
        >
          <h3 className="font-medium text-slate-900">Crear pregunta</h3>
          <textarea
            name="enunciado"
            required
            rows={2}
            placeholder="Enunciado"
            className={inputClass}
          />
          <div className="flex flex-wrap gap-3">
            <select name="dificultad" required defaultValue="Facil" className={selectClass}>
              {DIFICULTADES.map((d) => (
                <option key={d} value={d}>
                  {d}
                </option>
              ))}
            </select>
            <select name="categoriaId" className={selectClass}>
              <option value="">Sin categoría</option>
              {categorias?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </div>
          {renderOpcionesEditor(createOpciones, setCreateOpciones, 'correcta-create')}
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
        <form onSubmit={handleUpdate} className={`${cardHighlightClass} space-y-4`}>
          <h3 className="font-medium text-slate-900">Editar pregunta</h3>
          <textarea
            name="enunciado"
            required
            rows={2}
            defaultValue={editTarget.enunciado}
            className={inputClass}
          />
          <div className="flex flex-wrap gap-3">
            <select
              name="dificultad"
              required
              defaultValue={editTarget.dificultad}
              className={selectClass}
            >
              {DIFICULTADES.map((d) => (
                <option key={d} value={d}>
                  {d}
                </option>
              ))}
            </select>
            <select
              name="categoriaId"
              defaultValue={editTarget.categoriaId ?? ''}
              className={selectClass}
            >
              <option value="">Sin categoría</option>
              {categorias?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </div>
          {renderOpcionesEditor(editOpciones, setEditOpciones, 'correcta-edit')}
          <div className="flex gap-2">
            <button type="submit" disabled={isSaving} className={btnPrimary}>
              Actualizar
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
          title={hasFilters ? 'Sin resultados' : 'Sin preguntas'}
          description={
            hasFilters
              ? 'Ajusta los filtros o créalas desde «Nueva pregunta».'
              : 'Crea preguntas con al menos 3 opciones antes de usar trivia en sesión.'
          }
        />
      )}

      {data && data.length > 0 && (
        <div className="space-y-3">
          {data.map((pregunta) => (
            <article
              key={pregunta.id}
              className="rounded-xl border border-slate-200 bg-white p-4 text-sm shadow-sm"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="font-medium">{pregunta.enunciado}</p>
                  <p className="mt-1 text-slate-500">
                    {pregunta.dificultad} · {categoriaNombre(pregunta.categoriaId)}
                  </p>
                </div>
                <div className="flex gap-3">
                  <button
                    type="button"
                    disabled={isSaving}
                    onClick={() => {
                      setEditTarget(pregunta)
                      setShowCreate(false)
                      setEditOpciones(
                        pregunta.opciones.length >= 3
                          ? pregunta.opciones.map((o) => ({ ...o }))
                          : emptyOpciones(),
                      )
                      setFormError(null)
                    }}
                    className={btnLink}
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    disabled={isSaving}
                    onClick={() => void handleDelete(pregunta)}
                    className={btnDangerLink}
                  >
                    Eliminar
                  </button>
                </div>
              </div>
              <ul className="mt-3 list-inside list-disc text-slate-600">
                {pregunta.opciones.map((o, i) => (
                  <li key={i} className={o.esCorrecta ? 'font-medium text-green-700' : ''}>
                    {o.texto}
                    {o.esCorrecta ? ' ✓' : ''}
                  </li>
                ))}
              </ul>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}
