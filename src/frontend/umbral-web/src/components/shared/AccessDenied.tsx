import { Link } from 'react-router-dom'
import { getHomePathForRol } from '@/auth/authPaths'
import { useAuthStore } from '@/store/authStore'
import { btnSecondary } from '@/styles/ui'

interface AccessDeniedProps {
  title?: string
  message: string
}

export function AccessDenied({
  title = 'Acceso denegado',
  message,
}: AccessDeniedProps) {
  const rol = useAuthStore((s) => s.rol)
  const home = rol ? getHomePathForRol(rol) : '/login'

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="max-w-md rounded-xl border border-red-200 bg-red-50 p-6 text-center">
        <h1 className="text-lg font-semibold text-red-800">{title}</h1>
        <p className="mt-2 text-sm text-red-700">{message}</p>
        <Link to={home} className={`${btnSecondary} mt-4 inline-block`}>
          Volver a mi inicio
        </Link>
      </div>
    </div>
  )
}
