import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminLayout } from '@/components/layout/AdminLayout'
import { CategoriasPage } from '@/pages/admin/CategoriasPage'
import { MisionesPage } from '@/pages/admin/MisionesPage'
import { PreguntasPage } from '@/pages/admin/PreguntasPage'
import { LoginPage } from '@/pages/auth/LoginPage'
import { useAuthStore } from '@/store/authStore'

function RequireAdmin({ children }: { children: React.ReactNode }) {
  const { estaAutenticado, rol } = useAuthStore()

  if (!estaAutenticado()) {
    return <Navigate to="/login" replace />
  }

  if (rol !== 'Administrador') {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="max-w-md rounded-xl border border-red-200 bg-red-50 p-6 text-center">
          <h1 className="text-lg font-semibold text-red-800">Acceso denegado</h1>
          <p className="mt-2 text-sm text-red-700">
            Se requiere rol Administrador para el catálogo (E1-2).
          </p>
        </div>
      </div>
    )
  }

  return <>{children}</>
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/admin"
        element={
          <RequireAdmin>
            <AdminLayout />
          </RequireAdmin>
        }
      >
        <Route index element={<Navigate to="misiones" replace />} />
        <Route path="misiones" element={<MisionesPage />} />
        <Route path="categorias" element={<CategoriasPage />} />
        <Route path="preguntas" element={<PreguntasPage />} />
      </Route>
      <Route path="/" element={<Navigate to="/admin/misiones" replace />} />
      <Route path="*" element={<Navigate to="/admin/misiones" replace />} />
    </Routes>
  )
}
