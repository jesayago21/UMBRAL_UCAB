import { btnLink, btnSecondary, inputClass, selectClass } from '@/styles/ui'
import { PistasEditor } from '@/components/admin/PistasEditor'
import type { CrearEtapaRequest, TipoEtapaApi } from '@/types/mision.types'

export const emptyEtapa = (orden: number, tipo: TipoEtapaApi = 'BusquedaTesoro'): CrearEtapaRequest => ({
  tipoEtapa: tipo,
  orden,
  descripcion: '',
  codigoQrSolucion: '',
  pistas: [],
  categoriaIds: [],
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
          Combina etapas de Búsqueda del Tesoro (QR y pistas) y Trivia (categorías del banco).
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
              <PistasEditor
                pistas={etapa.pistas ?? []}
                onChange={(pistas) => updateEtapa(index, { pistas })}
                disabled={disabled}
              />
            </>
          ) : (
            <div className="space-y-1">
              <p className="text-xs text-slate-600">Categorías del banco de trivia (RB-33)</p>
              {categoriaOptions.length === 0 ? (
                <p className="text-xs text-amber-700">Crea categorías en Trivia antes de asignar.</p>
              ) : (
                categoriaOptions.map((cat) => (
                  <label key={cat.id} className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      disabled={disabled}
                      checked={(etapa.categoriaIds ?? []).includes(cat.id)}
                      onChange={() => toggleCategoria(index, cat.id)}
                    />
                    {cat.nombre}
                  </label>
                ))
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
