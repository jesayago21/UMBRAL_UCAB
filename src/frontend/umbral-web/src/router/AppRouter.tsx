import { Navigate, Route, Routes } from 'react-router-dom'
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
            deniedMessage="El catálogo es solo para Administrador. Cierra sesión e inicia como admin, o usa la cuenta operador para sesiones."
          >
            <AdminLayout />
          </RequireRoles>
        }
      >
        <Route index element={<Navigate to="misiones" replace />} />
        <Route path="misiones" element={<MisionesPage />} />
        <Route path="categorias" element={<CategoriasPage />} />
        <Route path="preguntas" element={<PreguntasPage />} />
      </Route>

      <Route
        path="/operador"
        element={
          <RequireRoles
            roles={['Operador']}
            deniedMessage="Las sesiones en vivo son solo para Operador. Usa la cuenta operador o cierra sesión como administrador."
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
