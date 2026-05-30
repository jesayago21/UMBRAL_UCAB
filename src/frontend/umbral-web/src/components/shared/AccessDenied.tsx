interface AccessDeniedProps {
  title?: string
  message: string
}

export function AccessDenied({
  title = 'Acceso denegado',
  message,
}: AccessDeniedProps) {
  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="max-w-md rounded-xl border border-red-200 bg-red-50 p-6 text-center">
        <h1 className="text-lg font-semibold text-red-800">{title}</h1>
        <p className="mt-2 text-sm text-red-700">{message}</p>
      </div>
    </div>
  )
}
