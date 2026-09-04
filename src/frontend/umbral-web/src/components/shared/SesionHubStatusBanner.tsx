import type { SesionHubConnectionStatus } from '@/hooks/useSesionHub'
import { etiquetaEstadoHub } from '@/hooks/useSesionHub'

interface SesionHubStatusBannerProps {
  status: SesionHubConnectionStatus
}

export function SesionHubStatusBanner({ status }: SesionHubStatusBannerProps) {
  const label = etiquetaEstadoHub(status)
  if (!label) return null

  const tone =
    status === 'reconectando' || status === 'conectando'
      ? 'border-amber-200 bg-amber-50 text-amber-900'
      : 'border-slate-200 bg-slate-50 text-slate-700'

  return (
    <p className={`rounded-md border px-3 py-2 text-sm ${tone}`} role="status">
      {label}
    </p>
  )
}
