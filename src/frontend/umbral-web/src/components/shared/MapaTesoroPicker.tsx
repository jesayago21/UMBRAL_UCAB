import { useEffect, useMemo } from 'react'
import { MapContainer, TileLayer, Marker, Circle, useMapEvents, useMap } from 'react-leaflet'

export interface UbicacionTesoro {
  latitud: number
  longitud: number
  radioMetros: number
}

interface MapaTesoroPickerProps {
  value: UbicacionTesoro | null
  onChange: (value: UbicacionTesoro | null) => void
  disabled?: boolean
  defaultCenter?: [number, number]
  minRadio?: number
  maxRadio?: number
}

const DEFAULT_CENTER: [number, number] = [10.488, -66.847]
const DEFAULT_RADIO = 120

function MapClickHandler({
  disabled,
  radioMetros,
  onPick,
}: {
  disabled?: boolean
  radioMetros: number
  onPick: (lat: number, lng: number, radio: number) => void
}) {
  useMapEvents({
    click(e) {
      if (disabled) return
      onPick(e.latlng.lat, e.latlng.lng, radioMetros)
    },
  })
  return null
}

function FitCenter({ center }: { center: [number, number] }) {
  const map = useMap()
  useEffect(() => {
    map.setView(center, map.getZoom())
  }, [map, center])
  return null
}

export function MapaTesoroPicker({
  value,
  onChange,
  disabled = false,
  defaultCenter = DEFAULT_CENTER,
  minRadio = 10,
  maxRadio = 300,
}: MapaTesoroPickerProps) {
  const radio = value?.radioMetros ?? DEFAULT_RADIO
  const center: [number, number] = useMemo(
    () => (value ? [value.latitud, value.longitud] : defaultCenter),
    [value, defaultCenter],
  )

  const setRadio = (next: number) => {
    if (disabled) return
    if (value) {
      onChange({ ...value, radioMetros: next })
      return
    }
    onChange({
      latitud: defaultCenter[0],
      longitud: defaultCenter[1],
      radioMetros: next,
    })
  }

  return (
    <div className="space-y-2 rounded-lg border border-amber-200 bg-amber-50/50 p-3">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h5 className="text-sm font-semibold text-amber-900">Configuración del tesoro</h5>
        {value && (
          <button
            type="button"
            disabled={disabled}
            className="text-xs text-amber-800 underline hover:no-underline"
            onClick={() => onChange(null)}
          >
            Quitar ubicación
          </button>
        )}
      </div>

      <div className="overflow-hidden rounded-md border border-slate-200">
        <MapContainer
          center={center}
          zoom={16}
          scrollWheelZoom={!disabled}
          className="z-0 h-56 w-full"
          style={{ cursor: disabled ? 'default' : 'crosshair' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <MapClickHandler
            disabled={disabled}
            radioMetros={radio}
            onPick={(lat, lng, r) => onChange({ latitud: lat, longitud: lng, radioMetros: r })}
          />
          {value && (
            <>
              <FitCenter center={[value.latitud, value.longitud]} />
              <Marker position={[value.latitud, value.longitud]} />
              <Circle
                center={[value.latitud, value.longitud]}
                radius={value.radioMetros}
                pathOptions={{ color: '#ea580c', fillColor: '#fb923c', fillOpacity: 0.25 }}
              />
            </>
          )}
        </MapContainer>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-slate-600">
        {value ? (
          <span className="font-mono">
            Lat: {value.latitud.toFixed(6)} · Lon: {value.longitud.toFixed(6)}
          </span>
        ) : (
          <span>Haz clic en el mapa para marcar la ubicación aproximada del tesoro.</span>
        )}
      </div>

      <div className="space-y-1">
        <div className="flex items-center justify-between text-xs font-medium text-slate-700">
          <span>Radio de búsqueda</span>
          <span className="font-semibold text-amber-900">{radio} m</span>
        </div>
        <input
          type="range"
          min={minRadio}
          max={maxRadio}
          step={5}
          value={radio}
          disabled={disabled}
          onChange={(e) => setRadio(Number(e.target.value))}
          className="w-full accent-amber-600"
          aria-label="Radio de búsqueda en metros"
        />
        <div className="flex justify-between text-[10px] text-slate-500">
          <span>{minRadio} m</span>
          <span>{maxRadio} m</span>
        </div>
      </div>

      <ul className="space-y-0.5 text-xs text-slate-600">
        <li>{value ? '✓' : '○'} Ubicación marcada en el mapa</li>
      </ul>
    </div>
  )
}
