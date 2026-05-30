interface LoadingStateProps {
  label?: string
}

export function LoadingState({ label = 'Cargando…' }: LoadingStateProps) {
  return (
    <div className="flex items-center justify-center py-16 text-slate-500">
      {label}
    </div>
  )
}
