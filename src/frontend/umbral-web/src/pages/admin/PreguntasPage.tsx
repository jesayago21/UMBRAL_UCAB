import { useState } from 'react'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useCategorias } from '@/hooks/useCategorias'
import {
  useActualizarPregunta,
  useCrearPregunta,
  useEliminarPregunta,
  usePreguntas,
} from '@/hooks/usePreguntas'
import { getApiErrorMessage } from '@/services/apiClient'
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

  const { data: categorias } = useCategorias()
  const { data, isLoading, isError, error } = usePreguntas({
    enunciado: enunciadoFiltro || undefined,
    dificultad: dificultadFiltro || undefined,
    categoriaId: categoriaFiltro || undefined,
  })
  const crear = useCrearPregunta()
  const actualizar = useActualizarPregunta()
  const eliminar = useEliminarPregunta()

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
    const form = new FormData(event.currentTarget)
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
      event.currentTarget.reset()
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
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDelete = async (pregunta: PreguntaDto) => {
    if (!window.confirm('¿Eliminar esta pregunta?')) return
    try {
      await eliminar.mutateAsync(pregunta.id)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const renderOpcionesEditor = (
    opciones: OpcionRespuestaDto[],
    setOpciones: (next: OpcionRespuestaDto[]) => void,
  ) => (
    <div className="space-y-2">
      <p className="text-sm font-medium text-slate-700">Opciones (mín. 3, una correcta)</p>
      {opciones.map((opcion, index) => (
        <div key={index} className="flex items-center gap-2">
          <input
            type="radio"
            name="correcta"
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
            className="flex-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          {opciones.length > 3 && (
            <button
              type="button"
              onClick={() => setOpciones(opciones.filter((_, i) => i !== index))}
              className="text-sm text-red-600"
            >
              Quitar
            </button>
          )}
        </div>
      ))}
      <button
        type="button"
        onClick={() => setOpciones([...opciones, { texto: '', esCorrecta: false }])}
        className="text-sm text-indigo-600 hover:underline"
      >
        + Añadir opción
      </button>
    </div>
  )

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold">Preguntas de trivia</h2>
          <p className="text-sm text-slate-500">HU-32..35 — banco de preguntas</p>
        </div>
        <button
          type="button"
          onClick={() => {
            setShowCreate(true)
            setEditTarget(null)
            setFormError(null)
            setCreateOpciones(emptyOpciones())
          }}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
        >
          Nueva pregunta
        </button>
      </div>

      <div className="flex flex-wrap gap-3">
        <input
          value={enunciadoFiltro}
          onChange={(e) => setEnunciadoFiltro(e.target.value)}
          placeholder="Filtrar por enunciado"
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
        />
        <select
          value={dificultadFiltro}
          onChange={(e) => setDificultadFiltro(e.target.value)}
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
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
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
        >
          <option value="">Todas las categorías</option>
          {categorias?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.nombre}
            </option>
          ))}
        </select>
      </div>

      {formError && <ErrorState message={formError} />}

      {showCreate && (
        <form
          onSubmit={handleCreate}
          className="space-y-4 rounded-xl border border-slate-200 bg-white p-4"
        >
          <h3 className="font-medium">Crear pregunta</h3>
          <textarea
            name="enunciado"
            required
            rows={2}
            placeholder="Enunciado"
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <div className="flex flex-wrap gap-3">
            <select
              name="dificultad"
              required
              defaultValue="Facil"
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              {DIFICULTADES.map((d) => (
                <option key={d} value={d}>
                  {d}
                </option>
              ))}
            </select>
            <select
              name="categoriaId"
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="">Sin categoría</option>
              {categorias?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </div>
          {renderOpcionesEditor(createOpciones, setCreateOpciones)}
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
          className="space-y-4 rounded-xl border border-indigo-200 bg-indigo-50/40 p-4"
        >
          <h3 className="font-medium">Editar pregunta</h3>
          <textarea
            name="enunciado"
            required
            rows={2}
            defaultValue={editTarget.enunciado}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          <div className="flex flex-wrap gap-3">
            <select
              name="dificultad"
              required
              defaultValue={editTarget.dificultad}
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
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
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="">Sin categoría</option>
              {categorias?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </div>
          {renderOpcionesEditor(editOpciones, setEditOpciones)}
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
        <div className="space-y-3">
          {data.map((pregunta) => (
            <article
              key={pregunta.id}
              className="rounded-xl border border-slate-200 bg-white p-4 text-sm"
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
                    className="text-indigo-600 hover:underline"
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    onClick={() => void handleDelete(pregunta)}
                    className="text-red-600 hover:underline"
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
          {data.length === 0 && (
            <p className="rounded-xl border border-slate-200 bg-white px-4 py-8 text-center text-slate-500">
              No hay preguntas con esos filtros.
            </p>
          )}
        </div>
      )}
    </div>
  )
}
