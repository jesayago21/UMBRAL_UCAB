import { btnLink, btnSecondary, inputClass } from '@/styles/ui'
import type { CrearEtapaRequest } from '@/types/mision.types'

export const emptyEtapa = (): CrearEtapaRequest => ({
  descripcion: '',
  codigoQrSolucion: '',
  pistas: [],
})

interface EtapasEditorProps {
  etapas: CrearEtapaRequest[]
  onChange: (etapas: CrearEtapaRequest[]) => void
  disabled?: boolean
}

/** Editor de etapas al crear una misión (cada etapa = checkpoint con QR). */
export function EtapasEditor({ etapas, onChange, disabled }: EtapasEditorProps) {
  const updateEtapa = (index: number, patch: Partial<CrearEtapaRequest>) => {
    onChange(etapas.map((etapa, i) => (i === index ? { ...etapa, ...patch } : etapa)))
  }

  const removeEtapa = (index: number) => {
    if (etapas.length <= 1) return
    onChange(etapas.filter((_, i) => i !== index))
  }

  const addEtapa = () => {
    onChange([...etapas, emptyEtapa()])
  }

  return (
    <div className="space-y-3 rounded-lg border border-slate-200 bg-slate-50/80 p-4">
      <div>
        <h4 className="text-sm font-medium text-slate-900">Etapas del recorrido</h4>
        <p className="mt-1 text-xs text-slate-600">
          Cada etapa es un checkpoint con su código QR. Los equipos las completan en orden durante
          la sesión en vivo.
        </p>
      </div>

      {etapas.map((etapa, index) => (
        <div
          key={index}
          className="space-y-2 rounded-md border border-slate-200 bg-white p-3"
        >
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs font-semibold uppercase tracking-wide text-indigo-700">
              Etapa {index + 1}
            </span>
            {etapas.length > 1 && (
              <button
                type="button"
                disabled={disabled}
                onClick={() => removeEtapa(index)}
                className={btnLink}
              >
                Quitar
              </button>
            )}
          </div>
          <input
            required
            disabled={disabled}
            value={etapa.descripcion}
            onChange={(e) => updateEtapa(index, { descripcion: e.target.value })}
            placeholder="Descripción (ej. Hall central)"
            className={inputClass}
            aria-label={`Descripción etapa ${index + 1}`}
          />
          <input
            required
            disabled={disabled}
            value={etapa.codigoQrSolucion}
            onChange={(e) => updateEtapa(index, { codigoQrSolucion: e.target.value })}
            placeholder="Código QR solución (ej. QR-HALL-001)"
            className={inputClass}
            aria-label={`QR solución etapa ${index + 1}`}
          />
        </div>
      ))}

      <button
        type="button"
        disabled={disabled}
        onClick={addEtapa}
        className={btnSecondary}
      >
        + Agregar etapa
      </button>
    </div>
  )
}
