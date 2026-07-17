import { useMemo } from 'react'
import { Text, View, StyleSheet } from 'react-native'
import { WebView } from 'react-native-webview'
import { styles as ui } from '@/styles/ui'

interface MapaTesoroViewerProps {
  latitud: number
  longitud: number
  radioMetros: number
  height?: number
}

/**
 * Mapa con OpenStreetMap + Leaflet (igual que umbral-web).
 * Evita react-native-maps / Google Maps, que en emulador Android suele quedar en blanco.
 */
export function MapaTesoroViewer({
  latitud,
  longitud,
  radioMetros,
  height = 220,
}: MapaTesoroViewerProps) {
  const valid =
    Number.isFinite(latitud) &&
    Number.isFinite(longitud) &&
    Number.isFinite(radioMetros) &&
    radioMetros > 0

  const html = useMemo(() => {
    if (!valid) return ''
    return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1" />
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    html, body, #map { margin:0; padding:0; height:100%; width:100%; background:#f1f5f9; }
  </style>
</head>
<body>
  <div id="map"></div>
  <script>
    var lat = ${latitud};
    var lng = ${longitud};
    var radio = ${radioMetros};
    var map = L.map('map', { zoomControl: true, attributionControl: true })
      .setView([lat, lng], 16);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap'
    }).addTo(map);
    L.marker([lat, lng]).addTo(map);
    L.circle([lat, lng], {
      radius: radio,
      color: '#2563eb',
      fillColor: '#3b82f6',
      fillOpacity: 0.25
    }).addTo(map);
    setTimeout(function () { map.invalidateSize(); }, 120);
  </script>
</body>
</html>`
  }, [latitud, longitud, radioMetros, valid])

  if (!valid) {
    return (
      <Text style={[ui.muted, { marginTop: 8 }]}>
        Ubicación de la etapa no disponible (lat/lon/radio inválidos).
      </Text>
    )
  }

  return (
    <View style={[local.mapWrap, { height }]}>
      <WebView
        originWhitelist={['*']}
        source={{ html }}
        style={StyleSheet.absoluteFill}
        javaScriptEnabled
        domStorageEnabled
        setSupportMultipleWindows={false}
        nestedScrollEnabled
        scrollEnabled={false}
        androidLayerType="hardware"
      />
    </View>
  )
}

function tieneUbicacion(etapa: {
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
}): boolean {
  return etapa.latitud != null && etapa.longitud != null && etapa.radioMetros != null
}

export { tieneUbicacion }

const local = StyleSheet.create({
  mapWrap: {
    marginTop: 10,
    borderRadius: 10,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: '#e2e8f0',
    backgroundColor: '#f1f5f9',
  },
})
