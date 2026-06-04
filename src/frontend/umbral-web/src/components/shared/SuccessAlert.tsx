interface SuccessAlertProps {
  message: string
  onDismiss?: () => void
}

export function SuccessAlert({ message, onDismiss }: SuccessAlertProps) {
  return (
    <div
      className="flex items-start justify-between gap-3 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-800"
      role="status"
    >
      <span>{message}</span>
      {onDismiss && (
        <button
          type="button"
          onClick={onDismiss}
          className="shrink-0 text-green-700 hover:text-green-900"
          aria-label="Cerrar"
        >
          ×
        </button>
      )}
    </div>
  )
}
