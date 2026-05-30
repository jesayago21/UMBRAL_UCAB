import { createRoot } from 'react-dom/client'
import { AuthProvider } from 'react-oidc-context'
import App from './App.tsx'
import { getOidcConfig } from '@/auth/oidcConfig'
import './index.css'

const root = document.getElementById('root')
if (!root) throw new Error('Root element #root not found')

createRoot(root).render(
  <AuthProvider {...getOidcConfig()}>
    <App />
  </AuthProvider>,
)
