import { Route, Routes } from 'react-router-dom'
import { AdminLayout } from '@/components/layout/AdminLayout'
import { OperadorLayout } from '@/components/layout/OperadorLayout'
import { CategoriasPage } from '@/pages/admin/CategoriasPage'
import { MisionesPage } from '@/pages/admin/MisionesPage'
import { PreguntasPage } from '@/pages/admin/PreguntasPage'
import { CallbackPage } from '@/pages/auth/CallbackPage'
import { LoginPage } from '@/pages/auth/LoginPage'
import { OperadorSesionesPage } from '@/pages/operador/OperadorSesionesPage'
import { HomeRedirect, RequireRoles } from '@/router/guards'

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/callback" element={<CallbackPage />} />

      <Route
        path="/admin"
        element={
          <RequireRoles
            roles={['Administrador']}
            deniedMessage="Se requiere rol Administrador para el catálogo. Inicia sesión como admin o usa el panel de operador."
          >
            <AdminLayout />
          </RequireRoles>
        }
      >
        <Route path="misiones" element={<MisionesPage />} />
        <Route path="categorias" element={<CategoriasPage />} />
        <Route path="preguntas" element={<PreguntasPage />} />
      </Route>

      <Route
        path="/operador"
        element={
          <RequireRoles
            roles={['Operador', 'Administrador']}
            deniedMessage="Se requiere rol Operador o Administrador para gestionar sesiones."
          >
            <OperadorLayout />
          </RequireRoles>
        }
      >
        <Route path="sesiones" element={<OperadorSesionesPage />} />
      </Route>

      <Route path="/" element={<HomeRedirect />} />
      <Route path="*" element={<HomeRedirect />} />
    </Routes>
  )
}
