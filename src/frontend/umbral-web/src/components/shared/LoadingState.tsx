interface LoadingStateProps {
  label?: string
}

export function LoadingState({ label = 'Cargando…' }: LoadingStateProps) {
  return (
    <div className="flex min-h-[12rem] items-center justify-center text-slate-600">
      <p className="text-sm font-medium">{label}</p>
    </div>
  )
}
