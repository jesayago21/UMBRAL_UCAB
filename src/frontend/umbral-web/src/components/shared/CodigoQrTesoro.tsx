import { useRef } from 'react'
import { QRCodeCanvas } from 'qrcode.react'
import { btnSecondary } from '@/styles/ui'

interface CodigoQrTesoroProps {
  codigo: string
  disabled?: boolean
  size?: number
}

export function CodigoQrTesoro({ codigo, disabled = false, size = 160 }: CodigoQrTesoroProps) {
  const containerRef = useRef<HTMLDivElement | null>(null)
  const valor = codigo.trim()

  if (!valor) {
    return (
      <div className="rounded-md border border-dashed border-slate-300 bg-slate-50 px-3 py-6 text-center text-xs text-slate-500">
        Escribe el código QR solución para generar la imagen.
      </div>
    )
  }

  const descargar = () => {
    const canvas = containerRef.current?.querySelector('canvas')
    if (!canvas) return
    const url = canvas.toDataURL('image/png')
    const a = document.createElement('a')
    a.href = url
    a.download = `umbral-qr-${valor.slice(0, 24)}.png`
    a.click()
  }

  return (
    <div className="space-y-2 rounded-md border border-slate-200 bg-white p-3">
      <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">Código QR</p>
      <div ref={containerRef} className="flex justify-center">
        <QRCodeCanvas value={valor} size={size} includeMargin level="M" />
      </div>
      <p className="break-all text-center font-mono text-[11px] text-slate-600">{valor}</p>
      <button
        type="button"
        disabled={disabled}
        onClick={descargar}
        className={`${btnSecondary} w-full text-xs`}
      >
        Descargar QR para imprimir
      </button>
    </div>
  )
}
