import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { emptyEtapa } from '@/components/admin/EtapasEditor'
import { emptyPista, PistasEditor } from '@/components/admin/PistasEditor'
import { CodigoQrTesoro } from '@/components/shared/CodigoQrTesoro'
import { MapaTesoroPicker } from '@/components/shared/MapaTesoroPicker'
import { ErrorState } from '@/components/shared/ErrorState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useCategorias } from '@/hooks/useCategorias'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { MISIONES_KEY, useMision } from '@/hooks/useMisiones'
import { formatCategoriaIds } from '@/lib/formatCategoriaIds'
import { getApiErrorMessage } from '@/services/apiClient'
import { categoriasTriviaUsadas } from '@/utils/misionTriviaCategories'
import {
  agregarEtapaMision,
  agregarPistaEtapa,
  editarEtapaMision,
  editarPistaEtapa,
  eliminarEtapaMision,
  eliminarPistaEtapa,
} from '@/services/misionService'
import { btnDangerLink, btnPrimary, btnSecondary, cardClass, inputClass, selectClass } from '@/styles/ui'
import type {
  CrearEtapaRequest,
  CrearPistaRequest,
  EtapaDto,
  MisionDto,
  PistaDto,
} from '@/types/mision.types'

interface MisionEtapasPanelProps {
  mision: MisionDto
  onClose: () => void
}

function PistaRow({
  misionId,
  etapaId,
  pista,
  onChanged,
}: {
  misionId: string
  etapaId: string
  pista: PistaDto
  onChanged: (msg?: string) => void
}) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState<CrearPistaRequest>({
    contenido: pista.contenido,
    tipoLiberacion: pista.tipoLiberacion,
    segundosLiberacion: pista.segundosLiberacion,
  })
  const [error, setError] = useState<string | null>(null)

  const editar = useMutation({
    mutationFn: () =>
      editarPistaEtapa(misionId, etapaId, pista.pistaId, {
        contenido: draft.contenido.trim(),
        tipoLiberacion: draft.tipoLiberacion,
        segundosLiberacion:
          draft.tipoLiberacion === 'PorTiempo' ? draft.segundosLiberacion ?? 60 : null,
      }),
    onSuccess: () => {
      setEditing(false)
      onChanged('Pista actualizada.')
    },
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  const eliminar = useMutation({
    mutationFn: () => eliminarPistaEtapa(misionId, etapaId, pista.pistaId),
    onSuccess: () => onChanged('Pista eliminada.'),
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  if (editing) {
    return (
      <li className="rounded border border-indigo-200 bg-white p-2">
        <PistasEditor pistas={[draft]} onChange={(p) => setDraft(p[0] ?? emptyPista())} />
        {error && (
          <div className="mt-2">
            <ErrorState message={error} />
          </div>
        )}
        <div className="mt-2 flex gap-2">
          <button
            type="button"
            disabled={editar.isPending}
            className={btnPrimary}
            onClick={() => {
              setError(null)
              void editar.mutateAsync()
            }}
          >
            {editar.isPending ? 'Guardando…' : 'Guardar cambios'}
          </button>
          <button type="button" className={btnSecondary} onClick={() => setEditing(false)}>
            Cancelar
          </button>
        </div>
      </li>
    )
  }

  return (
    <li className="rounded border border-slate-100 bg-slate-50 px-2 py-1.5 text-xs text-slate-700">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <span className="font-medium">{pista.tipoLiberacion}</span>
          {pista.tipoLiberacion === 'PorTiempo' && pista.segundosLiberacion != null && (
            <span className="text-slate-500"> · {pista.segundosLiberacion}s</span>
          )}
          <p className="mt-0.5 text-slate-600">{pista.contenido}</p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className="text-indigo-600 hover:underline"
            onClick={() => setEditing(true)}
          >
            Editar
          </button>
          <button
            type="button"
            className={btnDangerLink}
            disabled={eliminar.isPending}
            onClick={() => {
              if (!window.confirm('¿Eliminar esta pista?')) return
              setError(null)
              void eliminar.mutateAsync()
            }}
          >
            Eliminar
          </button>
        </div>
      </div>
      {error && (
        <div className="mt-2">
          <ErrorState message={error} />
        </div>
      )}
    </li>
  )
}

function EtapaRow({
  misionId,
  etapa,
  todasLasEtapas,
  totalEtapas,
  misionActiva,
  onChanged,
  categoriaOptions,
}: {
  misionId: string
  etapa: EtapaDto
  todasLasEtapas: ReadonlyArray<EtapaDto>
  totalEtapas: number
  misionActiva: boolean
  onChanged: (msg?: string) => void
  categoriaOptions: ReadonlyArray<{ id: string; nombre: string }>
}) {
  const [editing, setEditing] = useState(false)
  const [adding, setAdding] = useState(false)
  const [descripcion, setDescripcion] = useState(etapa.descripcion ?? '')
  const [codigoQr, setCodigoQr] = useState(etapa.codigoQrSolucion ?? '')
  const [latitud, setLatitud] = useState<number | null>(etapa.latitud ?? null)
  const [longitud, setLongitud] = useState<number | null>(etapa.longitud ?? null)
  const [radioMetros, setRadioMetros] = useState<number | null>(etapa.radioMetros ?? null)
  const [categoriaIds, setCategoriaIds] = useState<string[]>(etapa.categoriaIds ?? [])
  const [pistasDraft, setPistasDraft] = useState<CrearPistaRequest[]>([emptyPista()])
  const [error, setError] = useState<string | null>(null)

  const editar = useMutation({
    mutationFn: () =>
      editarEtapaMision(misionId, etapa.etapaId, {
        descripcion: etapa.tipoEtapa === 'BusquedaTesoro' ? descripcion.trim() : null,
        codigoQrSolucion: etapa.tipoEtapa === 'BusquedaTesoro' ? codigoQr.trim() : null,
        categoriaIds: etapa.tipoEtapa === 'Trivia' ? categoriaIds : null,
        latitud: etapa.tipoEtapa === 'BusquedaTesoro' ? latitud : null,
        longitud: etapa.tipoEtapa === 'BusquedaTesoro' ? longitud : null,
        radioMetros: etapa.tipoEtapa === 'BusquedaTesoro' ? radioMetros : null,
      }),
    onSuccess: () => {
      setEditing(false)
      onChanged('Etapa actualizada.')
    },
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  const eliminar = useMutation({
    mutationFn: () => eliminarEtapaMision(misionId, etapa.etapaId),
    onSuccess: () => onChanged('Etapa eliminada.'),
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  const agregar = useMutation({
    mutationFn: async () => {
      const pista = pistasDraft.find((p) => p.contenido.trim())
      if (!pista) throw new Error('Escribe el contenido de la pista.')
      await agregarPistaEtapa(misionId, etapa.etapaId, {
        contenido: pista.contenido.trim(),
        tipoLiberacion: pista.tipoLiberacion,
        segundosLiberacion:
          pista.tipoLiberacion === 'PorTiempo' ? pista.segundosLiberacion ?? 60 : null,
      })
    },
    onSuccess: () => {
      setAdding(false)
      setPistasDraft([emptyPista()])
      onChanged('Pista agregada.')
    },
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  const canDelete = !(misionActiva && totalEtapas <= 1)

  const startEdit = () => {
    setDescripcion(etapa.descripcion ?? '')
    setCodigoQr(etapa.codigoQrSolucion ?? '')
    setLatitud(etapa.latitud ?? null)
    setLongitud(etapa.longitud ?? null)
    setRadioMetros(etapa.radioMetros ?? null)
    setCategoriaIds(etapa.categoriaIds ?? [])
    setError(null)
    setEditing(true)
  }

  if (editing) {
    return (
      <li className="space-y-2 rounded-md border border-indigo-200 bg-indigo-50/40 p-3">
        <div className="flex flex-wrap items-baseline gap-2">
          <span className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-semibold text-indigo-800">
            Etapa {etapa.orden}
          </span>
          <span className="text-xs text-slate-500">{etapa.tipoEtapa}</span>
        </div>

        {etapa.tipoEtapa === 'BusquedaTesoro' ? (
          <>
            <input
              value={descripcion}
              onChange={(e) => setDescripcion(e.target.value)}
              placeholder="Descripción"
              className={inputClass}
            />
            <input
              value={codigoQr}
              onChange={(e) => setCodigoQr(e.target.value)}
              placeholder="Código QR solución"
              className={inputClass}
            />
            <div className="grid gap-3 md:grid-cols-2">
              <MapaTesoroPicker
                value={
                  latitud != null && longitud != null && radioMetros != null
                    ? { latitud, longitud, radioMetros }
                    : null
                }
                onChange={(ubicacion) => {
                  setLatitud(ubicacion?.latitud ?? null)
                  setLongitud(ubicacion?.longitud ?? null)
                  setRadioMetros(ubicacion?.radioMetros ?? null)
                }}
              />
              <CodigoQrTesoro codigo={codigoQr} />
            </div>
          </>
        ) : (
          <div className="space-y-1">
            <p className="text-xs text-slate-600">
              Categorías (RB-33). No repitas categorías ya usadas en otra etapa trivia.
            </p>
            {categoriaOptions.map((cat) => {
              const usadaEnOtraEtapa = categoriasTriviaUsadas(todasLasEtapas, {
                etapaId: etapa.etapaId,
              }).has(cat.id)
              const checked = categoriaIds.includes(cat.id)
              return (
                <label
                  key={cat.id}
                  className={`flex items-center gap-2 text-sm ${usadaEnOtraEtapa && !checked ? 'text-slate-400' : ''}`}
                >
                  <input
                    type="checkbox"
                    disabled={usadaEnOtraEtapa && !checked}
                    checked={checked}
                    onChange={() => {
                      if (usadaEnOtraEtapa && !checked) return
                      setCategoriaIds((prev) =>
                        prev.includes(cat.id)
                          ? prev.filter((id) => id !== cat.id)
                          : [...prev, cat.id],
                      )
                    }}
                  />
                  {cat.nombre}
                  {usadaEnOtraEtapa && !checked && (
                    <span className="text-xs text-slate-400">(ya usada en otra etapa)</span>
                  )}
                </label>
              )
            })}
          </div>
        )}

        {error && <ErrorState message={error} />}
        <div className="flex gap-2">
          <button
            type="button"
            disabled={editar.isPending}
            className={btnPrimary}
            onClick={() => void editar.mutateAsync()}
          >
            {editar.isPending ? 'Guardando…' : 'Guardar etapa'}
          </button>
          <button type="button" className={btnSecondary} onClick={() => setEditing(false)}>
            Cancelar
          </button>
        </div>
      </li>
    )
  }

  return (
    <li className="rounded-md border border-slate-200 bg-white p-3">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <div className="flex flex-wrap items-baseline gap-2">
            <span className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-semibold text-indigo-800">
              Etapa {etapa.orden}
            </span>
            <span className="text-xs text-slate-500">{etapa.tipoEtapa}</span>
            <span className="font-medium text-slate-900">{etapa.descripcion ?? '—'}</span>
          </div>
          {etapa.tipoEtapa === 'BusquedaTesoro' && (
            <p className="mt-2 font-mono text-xs text-slate-600">
              QR solución: <span className="text-slate-900">{etapa.codigoQrSolucion}</span>
              {etapa.latitud != null && etapa.longitud != null && etapa.radioMetros != null && (
                <span className="ml-2 text-amber-800">
                  · mapa {etapa.latitud.toFixed(4)}, {etapa.longitud.toFixed(4)} · {etapa.radioMetros} m
                </span>
              )}
            </p>
          )}
          {etapa.tipoEtapa === 'Trivia' && (
            <p className="mt-2 text-xs text-slate-600">
              Categorías: {formatCategoriaIds(etapa.categoriaIds, categoriaOptions)}
            </p>
          )}
        </div>
        <div className="flex gap-2">
          <button type="button" className="text-sm text-indigo-600 hover:underline" onClick={startEdit}>
            Editar etapa
          </button>
          <button
            type="button"
            className={btnDangerLink}
            disabled={eliminar.isPending || !canDelete}
            title={
              !canDelete
                ? 'Una misión activa necesita al menos una etapa (RB-09).'
                : undefined
            }
            onClick={() => {
              if (!window.confirm('¿Eliminar esta etapa de la misión?')) return
              setError(null)
              void eliminar.mutateAsync()
            }}
          >
            Eliminar
          </button>
        </div>
      </div>

      {etapa.tipoEtapa === 'BusquedaTesoro' && (etapa.pistas?.length ?? 0) > 0 ? (
        <ul className="mt-2 space-y-1">
          {etapa.pistas!.map((p) => (
            <PistaRow
              key={p.pistaId}
              misionId={misionId}
              etapaId={etapa.etapaId}
              pista={p}
              onChanged={onChanged}
            />
          ))}
        </ul>
      ) : etapa.tipoEtapa === 'BusquedaTesoro' ? (
        <p className="mt-2 text-xs text-slate-500">Sin pistas en esta etapa.</p>
      ) : null}

      {error && (
        <div className="mt-2">
          <ErrorState message={error} />
        </div>
      )}

      {etapa.tipoEtapa === 'BusquedaTesoro' &&
        (!adding ? (
          <button
            type="button"
            className="mt-2 text-sm text-indigo-600 hover:underline"
            onClick={() => setAdding(true)}
          >
            + Agregar pista a esta etapa
          </button>
        ) : (
          <div className="mt-3 space-y-2">
            <PistasEditor pistas={pistasDraft} onChange={setPistasDraft} />
            <div className="flex gap-2">
              <button
                type="button"
                disabled={agregar.isPending}
                onClick={() => {
                  setError(null)
                  void agregar.mutateAsync()
                }}
                className={btnPrimary}
              >
                {agregar.isPending ? 'Guardando…' : 'Guardar pista'}
              </button>
              <button type="button" className={btnSecondary} onClick={() => setAdding(false)}>
                Cancelar
              </button>
            </div>
          </div>
        ))}
    </li>
  )
}

export function MisionEtapasPanel({ mision: misionInicial, onClose }: MisionEtapasPanelProps) {
  const queryClient = useQueryClient()
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()
  const { data: mision = misionInicial, refetch } = useMision(misionInicial.id)
  const { data: categorias } = useCategorias()
  const categoriaOptions = (categorias ?? []).map((c) => ({ id: c.id, nombre: c.nombre }))
  const [addingEtapa, setAddingEtapa] = useState(false)
  const [nuevaEtapa, setNuevaEtapa] = useState<CrearEtapaRequest>(emptyEtapa(1))
  const [addError, setAddError] = useState<string | null>(null)

  const refresh = (msg = 'Cambios guardados.') => {
    void refetch()
    void queryClient.invalidateQueries({ queryKey: MISIONES_KEY })
    showSuccess(msg)
  }

  const agregarEtapa = useMutation({
    mutationFn: async () => {
      if (nuevaEtapa.tipoEtapa === 'Trivia') {
        if ((nuevaEtapa.categoriaIds?.length ?? 0) === 0) {
          throw new Error('Selecciona al menos una categoría.')
        }
        return agregarEtapaMision(mision.id, {
          tipoEtapa: 'Trivia',
          categoriaIds: nuevaEtapa.categoriaIds,
        })
      }
      if (!nuevaEtapa.descripcion?.trim() || !nuevaEtapa.codigoQrSolucion?.trim()) {
        throw new Error('Completa descripción y código QR.')
      }
      return agregarEtapaMision(mision.id, {
        tipoEtapa: 'BusquedaTesoro',
        descripcion: nuevaEtapa.descripcion.trim(),
        codigoQrSolucion: nuevaEtapa.codigoQrSolucion.trim(),
        latitud: nuevaEtapa.latitud ?? null,
        longitud: nuevaEtapa.longitud ?? null,
        radioMetros: nuevaEtapa.radioMetros ?? null,
        pistas: (nuevaEtapa.pistas ?? [])
          .filter((p) => p.contenido.trim())
          .map((p) => ({
            contenido: p.contenido.trim(),
            tipoLiberacion: p.tipoLiberacion,
            segundosLiberacion:
              p.tipoLiberacion === 'PorTiempo' ? p.segundosLiberacion ?? 60 : null,
          })),
      })
    },
    onSuccess: () => {
      setAddingEtapa(false)
      setNuevaEtapa(emptyEtapa(1))
      refresh('Etapa agregada.')
    },
    onError: (err) => setAddError(getApiErrorMessage(err)),
  })

  return (
    <div className={`${cardClass} space-y-4`}>
      {successMessage && <SuccessAlert message={successMessage} onDismiss={clearSuccess} />}
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="font-medium text-slate-900">{mision.nombre}</h3>
          <p className="mt-1 text-sm text-slate-600">
            {mision.totalEtapas} etapa(s) · estado {mision.estado}
          </p>
        </div>
        <button type="button" onClick={onClose} className="text-sm text-indigo-600 hover:underline">
          Cerrar
        </button>
      </div>

      <p className="text-xs text-slate-500">
        Edita descripción/QR o categorías de cada etapa. Si hay una sesión en curso, no se permiten
        cambios estructurales. Activar o desactivar la misión es una acción aparte en el listado.
      </p>

      {(mision.etapas ?? []).length === 0 ? (
        <p className="text-sm text-slate-600">Esta misión no tiene etapas. Agrega al menos una.</p>
      ) : (
        <ol className="space-y-2">
          {(mision.etapas ?? []).map((etapa) => (
            <EtapaRow
              key={etapa.etapaId}
              misionId={mision.id}
              etapa={etapa}
              todasLasEtapas={mision.etapas ?? []}
              totalEtapas={mision.totalEtapas}
              misionActiva={mision.estado === 'Activa'}
              onChanged={refresh}
              categoriaOptions={categoriaOptions}
            />
          ))}
        </ol>
      )}

      {!addingEtapa ? (
        <button
          type="button"
          className="text-sm font-medium text-indigo-600 hover:underline"
          onClick={() => {
            setAddError(null)
            setNuevaEtapa(emptyEtapa((mision.etapas?.length ?? 0) + 1))
            setAddingEtapa(true)
          }}
        >
          + Agregar etapa
        </button>
      ) : (
        <div className="space-y-2 rounded-md border border-slate-200 bg-slate-50 p-3">
          <h4 className="text-sm font-medium text-slate-900">Nueva etapa</h4>
          <select
            value={nuevaEtapa.tipoEtapa}
            onChange={(e) =>
              setNuevaEtapa(emptyEtapa(nuevaEtapa.orden, e.target.value as 'BusquedaTesoro' | 'Trivia'))
            }
            className={selectClass}
          >
            <option value="BusquedaTesoro">Búsqueda del Tesoro</option>
            <option value="Trivia">Trivia</option>
          </select>
          {nuevaEtapa.tipoEtapa === 'BusquedaTesoro' ? (
            <>
              <input
                value={nuevaEtapa.descripcion ?? ''}
                onChange={(e) => setNuevaEtapa({ ...nuevaEtapa, descripcion: e.target.value })}
                placeholder="Descripción"
                className={inputClass}
              />
              <input
                value={nuevaEtapa.codigoQrSolucion ?? ''}
                onChange={(e) => setNuevaEtapa({ ...nuevaEtapa, codigoQrSolucion: e.target.value })}
                placeholder="Código QR solución"
                className={inputClass}
              />
              <div className="grid gap-3 md:grid-cols-2">
                <MapaTesoroPicker
                  value={
                    nuevaEtapa.latitud != null &&
                    nuevaEtapa.longitud != null &&
                    nuevaEtapa.radioMetros != null
                      ? {
                          latitud: nuevaEtapa.latitud,
                          longitud: nuevaEtapa.longitud,
                          radioMetros: nuevaEtapa.radioMetros,
                        }
                      : null
                  }
                  onChange={(ubicacion) =>
                    setNuevaEtapa({
                      ...nuevaEtapa,
                      latitud: ubicacion?.latitud ?? null,
                      longitud: ubicacion?.longitud ?? null,
                      radioMetros: ubicacion?.radioMetros ?? null,
                    })
                  }
                />
                <CodigoQrTesoro codigo={nuevaEtapa.codigoQrSolucion ?? ''} />
              </div>
            </>
          ) : (
            <div className="space-y-1">
              <p className="text-xs text-slate-600">
                Cada categoría solo puede usarse en una etapa trivia de la misión.
              </p>
              {categoriaOptions.map((cat) => {
                const usadaEnOtraEtapa = categoriasTriviaUsadas(mision.etapas ?? []).has(cat.id)
                const checked = (nuevaEtapa.categoriaIds ?? []).includes(cat.id)
                return (
                  <label
                    key={cat.id}
                    className={`flex items-center gap-2 text-sm ${usadaEnOtraEtapa ? 'text-slate-400' : ''}`}
                  >
                    <input
                      type="checkbox"
                      disabled={usadaEnOtraEtapa}
                      checked={checked}
                      onChange={() => {
                        if (usadaEnOtraEtapa) return
                        const ids = new Set(nuevaEtapa.categoriaIds ?? [])
                        if (ids.has(cat.id)) ids.delete(cat.id)
                        else ids.add(cat.id)
                        setNuevaEtapa({ ...nuevaEtapa, categoriaIds: [...ids] })
                      }}
                    />
                    {cat.nombre}
                    {usadaEnOtraEtapa && (
                      <span className="text-xs text-slate-400">(ya usada en otra etapa)</span>
                    )}
                  </label>
                )
              })}
            </div>
          )}
          {addError && <ErrorState message={addError} />}
          <div className="flex gap-2">
            <button
              type="button"
              disabled={agregarEtapa.isPending}
              className={btnPrimary}
              onClick={() => {
                setAddError(null)
                void agregarEtapa.mutateAsync()
              }}
            >
              {agregarEtapa.isPending ? 'Guardando…' : 'Guardar etapa'}
            </button>
            <button type="button" className={btnSecondary} onClick={() => setAddingEtapa(false)}>
              Cancelar
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
