import { createRoot } from 'react-dom/client'
import { AuthProvider } from 'react-oidc-context'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import App from './App.tsx'
import { getOidcConfig } from '@/auth/oidcConfig'
import 'leaflet/dist/leaflet.css'
import './index.css'

// Fix default Leaflet marker icons broken by Vite asset hashing
delete (L.Icon.Default.prototype as unknown as { _getIconUrl?: unknown })._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
})

const root = document.getElementById('root')
if (!root) throw new Error('Root element #root not found')

createRoot(root).render(
  <AuthProvider {...getOidcConfig()}>
    <App />
  </AuthProvider>,
)
