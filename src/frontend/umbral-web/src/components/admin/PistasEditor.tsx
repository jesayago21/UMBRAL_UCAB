import { btnLink, btnSecondary, inputClass } from '@/styles/ui'
import type { CrearPistaRequest } from '@/types/mision.types'

export const emptyPista = (): CrearPistaRequest => ({
  contenido: '',
  tipoLiberacion: 'PorTiempo',
  segundosLiberacion: 60,
})

const TIPOS = [
  { value: 'PorTiempo', label: 'Por tiempo (segundos)' },
  { value: 'PorGanador', label: 'Al inicio de la etapa (con el mapa)' },
] as const

interface PistasEditorProps {
  pistas: CrearPistaRequest[]
  onChange: (pistas: CrearPistaRequest[]) => void
  disabled?: boolean
}

export function PistasEditor({ pistas, onChange, disabled }: PistasEditorProps) {
  const update = (index: number, patch: Partial<CrearPistaRequest>) => {
    onChange(pistas.map((p, i) => (i === index ? { ...p, ...patch } : p)))
  }

  const remove = (index: number) => {
    onChange(pistas.filter((_, i) => i !== index))
  }

  const add = () => onChange([...pistas, emptyPista()])

  return (
    <div className="mt-3 space-y-2 rounded-md border border-dashed border-slate-300 bg-slate-50/50 p-3">
      <p className="text-xs font-medium text-slate-700">Pistas de esta etapa</p>
      {pistas.length === 0 && (
        <p className="text-xs text-slate-500">Sin pistas. Opcional: por tiempo o al inicio de la etapa (con el mapa).</p>
      )}
      {pistas.map((pista, index) => (
        <div key={index} className="space-y-2 rounded border border-slate-200 bg-white p-2">
          <div className="flex items-center justify-between">
            <span className="text-xs text-indigo-700">Pista {index + 1}</span>
            <button type="button" disabled={disabled} onClick={() => remove(index)} className={btnLink}>
              Quitar
            </button>
          </div>
          <textarea
            required
            disabled={disabled}
            value={pista.contenido}
            onChange={(e) => update(index, { contenido: e.target.value })}
            placeholder="Texto de la pista"
            className={`${inputClass} min-h-[4rem]`}
            aria-label={`Contenido pista ${index + 1}`}
          />
          <select
            disabled={disabled}
            value={pista.tipoLiberacion}
            onChange={(e) =>
              update(index, {
                tipoLiberacion: e.target.value,
                segundosLiberacion: e.target.value === 'PorTiempo' ? pista.segundosLiberacion ?? 60 : null,
              })
            }
            className={inputClass}
            aria-label={`Tipo liberación pista ${index + 1}`}
          >
            {TIPOS.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
          {pista.tipoLiberacion === 'PorTiempo' && (
            <input
              type="number"
              min={1}
              required
              disabled={disabled}
              value={pista.segundosLiberacion ?? 60}
              onChange={(e) =>
                update(index, { segundosLiberacion: Number(e.target.value) || 60 })
              }
              className={inputClass}
              aria-label={`Segundos liberación pista ${index + 1}`}
            />
          )}
        </div>
      ))}
      <button type="button" disabled={disabled} onClick={add} className={`${btnSecondary} text-xs`}>
        + Agregar pista
      </button>
    </div>
  )
}
