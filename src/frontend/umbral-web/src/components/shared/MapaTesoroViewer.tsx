import { useEffect } from 'react'
import { MapContainer, TileLayer, Marker, Circle, useMap } from 'react-leaflet'

interface MapaTesoroViewerProps {
  latitud: number
  longitud: number
  radioMetros: number
  className?: string
  heightClass?: string
}

function InvalidateSize() {
  const map = useMap()
  useEffect(() => {
    const t = window.setTimeout(() => map.invalidateSize(), 80)
    return () => window.clearTimeout(t)
  }, [map])
  return null
}

export function MapaTesoroViewer({
  latitud,
  longitud,
  radioMetros,
  className = '',
  heightClass = 'h-56',
}: MapaTesoroViewerProps) {
  if (
    !Number.isFinite(latitud) ||
    !Number.isFinite(longitud) ||
    !Number.isFinite(radioMetros) ||
    radioMetros <= 0
  ) {
    return (
      <p className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">
        Ubicación de la etapa no disponible (lat/lon/radio inválidos).
      </p>
    )
  }

  const center: [number, number] = [latitud, longitud]

  return (
    <div className={`overflow-hidden rounded-lg border border-slate-200 ${className}`}>
      <MapContainer
        key={`${latitud.toFixed(6)}-${longitud.toFixed(6)}-${radioMetros}`}
        center={center}
        zoom={16}
        scrollWheelZoom={false}
        className={`z-0 w-full ${heightClass}`}
        style={{ minHeight: 180 }}
      >
        <InvalidateSize />
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Marker position={center} />
        <Circle
          center={center}
          radius={radioMetros}
          pathOptions={{ color: '#2563eb', fillColor: '#3b82f6', fillOpacity: 0.25 }}
        />
      </MapContainer>
    </div>
  )
}
