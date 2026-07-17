import { btnLink, btnSecondary, inputClass, selectClass } from '@/styles/ui'
import { PistasEditor } from '@/components/admin/PistasEditor'
import { CodigoQrTesoro } from '@/components/shared/CodigoQrTesoro'
import { MapaTesoroPicker } from '@/components/shared/MapaTesoroPicker'
import type { CrearEtapaRequest, TipoEtapaApi } from '@/types/mision.types'
import { categoriasTriviaUsadas } from '@/utils/misionTriviaCategories'

export const emptyEtapa = (orden: number, tipo: TipoEtapaApi = 'BusquedaTesoro'): CrearEtapaRequest => ({
  tipoEtapa: tipo,
  orden,
  descripcion: '',
  codigoQrSolucion: '',
  pistas: [],
  categoriaIds: [],
  latitud: null,
  longitud: null,
  radioMetros: null,
})

interface EtapasEditorProps {
  etapas: CrearEtapaRequest[]
  onChange: (etapas: CrearEtapaRequest[]) => void
  categoriaOptions: { id: string; nombre: string }[]
  disabled?: boolean
}

export function EtapasEditor({ etapas, onChange, categoriaOptions, disabled }: EtapasEditorProps) {
  const updateEtapa = (index: number, patch: Partial<CrearEtapaRequest>) => {
    onChange(etapas.map((etapa, i) => (i === index ? { ...etapa, ...patch } : etapa)))
  }

  const removeEtapa = (index: number) => {
    if (etapas.length <= 1) return
    const next = etapas.filter((_, i) => i !== index).map((e, i) => ({ ...e, orden: i + 1 }))
    onChange(next)
  }

  const addEtapa = () => {
    onChange([...etapas, emptyEtapa(etapas.length + 1)])
  }

  const toggleCategoria = (index: number, categoriaId: string) => {
    const etapa = etapas[index]
    const usadas = categoriasTriviaUsadas(etapas, { etapaIndex: index })
    if (usadas.has(categoriaId)) return
    const ids = new Set(etapa.categoriaIds ?? [])
    if (ids.has(categoriaId)) ids.delete(categoriaId)
    else ids.add(categoriaId)
    updateEtapa(index, { categoriaIds: [...ids] })
  }

  return (
    <div className="space-y-3 rounded-lg border border-slate-200 bg-slate-50/80 p-4">
      <div>
        <h4 className="text-sm font-medium text-slate-900">Etapas del recorrido</h4>
        <p className="mt-1 text-xs text-slate-600">
          Combina etapas de Búsqueda del Tesoro (QR, mapa y pistas) y Trivia (categorías del banco).
        </p>
      </div>

      {etapas.map((etapa, index) => (
        <div key={index} className="space-y-2 rounded-md border border-slate-200 bg-white p-3">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs font-semibold uppercase tracking-wide text-indigo-700">
              Etapa {etapa.orden}
            </span>
            {etapas.length > 1 && (
              <button type="button" disabled={disabled} onClick={() => removeEtapa(index)} className={btnLink}>
                Quitar
              </button>
            )}
          </div>

          <select
            disabled={disabled}
            value={etapa.tipoEtapa}
            onChange={(e) => {
              const tipo = e.target.value as TipoEtapaApi
              updateEtapa(index, {
                tipoEtapa: tipo,
                descripcion: tipo === 'BusquedaTesoro' ? etapa.descripcion ?? '' : undefined,
                codigoQrSolucion: tipo === 'BusquedaTesoro' ? etapa.codigoQrSolucion ?? '' : undefined,
                pistas: tipo === 'BusquedaTesoro' ? etapa.pistas ?? [] : [],
                categoriaIds: tipo === 'Trivia' ? etapa.categoriaIds ?? [] : [],
                latitud: tipo === 'BusquedaTesoro' ? etapa.latitud ?? null : null,
                longitud: tipo === 'BusquedaTesoro' ? etapa.longitud ?? null : null,
                radioMetros: tipo === 'BusquedaTesoro' ? etapa.radioMetros ?? null : null,
              })
            }}
            className={selectClass}
            aria-label={`Tipo etapa ${index + 1}`}
          >
            <option value="BusquedaTesoro">Búsqueda del Tesoro</option>
            <option value="Trivia">Trivia</option>
          </select>

          {etapa.tipoEtapa === 'BusquedaTesoro' ? (
            <>
              <input
                required
                disabled={disabled}
                value={etapa.descripcion ?? ''}
                onChange={(e) => updateEtapa(index, { descripcion: e.target.value })}
                placeholder="Descripción (ej. Hall central)"
                className={inputClass}
              />
              <input
                required
                disabled={disabled}
                value={etapa.codigoQrSolucion ?? ''}
                onChange={(e) => updateEtapa(index, { codigoQrSolucion: e.target.value })}
                placeholder="Código QR solución"
                className={inputClass}
              />
              <div className="grid gap-3 md:grid-cols-2">
                <MapaTesoroPicker
                  disabled={disabled}
                  value={
                    etapa.latitud != null && etapa.longitud != null && etapa.radioMetros != null
                      ? {
                          latitud: etapa.latitud,
                          longitud: etapa.longitud,
                          radioMetros: etapa.radioMetros,
                        }
                      : null
                  }
                  onChange={(ubicacion) =>
                    updateEtapa(index, {
                      latitud: ubicacion?.latitud ?? null,
                      longitud: ubicacion?.longitud ?? null,
                      radioMetros: ubicacion?.radioMetros ?? null,
                    })
                  }
                />
                <CodigoQrTesoro codigo={etapa.codigoQrSolucion ?? ''} disabled={disabled} />
              </div>
              <PistasEditor
                pistas={etapa.pistas ?? []}
                onChange={(pistas) => updateEtapa(index, { pistas })}
                disabled={disabled}
              />
            </>
          ) : (
            <div className="space-y-1">
              <p className="text-xs text-slate-600">
                Categorías del banco de trivia (RB-33). Cada categoría solo puede usarse en una etapa
                trivia de la misión.
              </p>
              {categoriaOptions.length === 0 ? (
                <p className="text-xs text-amber-700">Crea categorías en Trivia antes de asignar.</p>
              ) : (
                categoriaOptions.map((cat) => {
                  const usadaEnOtraEtapa = categoriasTriviaUsadas(etapas, { etapaIndex: index }).has(
                    cat.id,
                  )
                  const checked = (etapa.categoriaIds ?? []).includes(cat.id)
                  return (
                    <label
                      key={cat.id}
                      className={`flex items-center gap-2 text-sm ${usadaEnOtraEtapa && !checked ? 'text-slate-400' : ''}`}
                    >
                      <input
                        type="checkbox"
                        disabled={disabled || (usadaEnOtraEtapa && !checked)}
                        checked={checked}
                        onChange={() => toggleCategoria(index, cat.id)}
                      />
                      {cat.nombre}
                      {usadaEnOtraEtapa && !checked && (
                        <span className="text-xs text-slate-400">(ya usada en otra etapa)</span>
                      )}
                    </label>
                  )
                })
              )}
            </div>
          )}
        </div>
      ))}

      <button type="button" disabled={disabled} onClick={addEtapa} className={btnSecondary}>
        + Agregar etapa
      </button>
    </div>
  )
}
