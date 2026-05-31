import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { emptyPista, PistasEditor } from '@/components/admin/PistasEditor'
import { ErrorState } from '@/components/shared/ErrorState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { MISIONES_KEY, useMision } from '@/hooks/useMisiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { agregarPistaEtapa } from '@/services/misionService'
import { btnPrimary, cardClass } from '@/styles/ui'
import type { CrearPistaRequest, EtapaDto, MisionDto } from '@/types/mision.types'

interface MisionEtapasPanelProps {
  mision: MisionDto
  onClose: () => void
}

function PistaRow({
  pista,
}: {
  pista: { contenido: string; tipoLiberacion: string; segundosLiberacion: number | null }
}) {
  return (
    <li className="rounded border border-slate-100 bg-slate-50 px-2 py-1.5 text-xs text-slate-700">
      <span className="font-medium">{pista.tipoLiberacion}</span>
      {pista.tipoLiberacion === 'PorTiempo' && pista.segundosLiberacion != null && (
        <span className="text-slate-500"> · {pista.segundosLiberacion}s</span>
      )}
      <p className="mt-0.5 text-slate-600">{pista.contenido}</p>
    </li>
  )
}

function EtapaRow({
  misionId,
  etapa,
  onPistaAdded,
}: {
  misionId: string
  etapa: EtapaDto
  onPistaAdded: () => void
}) {
  const [adding, setAdding] = useState(false)
  const [pistasDraft, setPistasDraft] = useState<CrearPistaRequest[]>([emptyPista()])
  const [error, setError] = useState<string | null>(null)

  const agregar = useMutation({
    mutationFn: async () => {
      const pista = pistasDraft.find((p) => p.contenido.trim())
      if (!pista) throw new Error('Escribe el contenido de la pista.')
      await agregarPistaEtapa(misionId, etapa.etapaId, {
        contenido: pista.contenido.trim(),
        tipoLiberacion: pista.tipoLiberacion,
        segundosLiberacion:
          pista.tipoLiberacion === 'PorTiempo' ? pista.segundosLiberacion ?? 60 : null,
      })
    },
    onSuccess: () => {
      setAdding(false)
      setPistasDraft([emptyPista()])
      onPistaAdded()
    },
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  return (
    <li className="rounded-md border border-slate-200 bg-white p-3">
      <div className="flex flex-wrap items-baseline gap-2">
        <span className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-semibold text-indigo-800">
          Etapa {etapa.orden}
        </span>
        <span className="font-medium text-slate-900">{etapa.descripcion}</span>
      </div>
      <p className="mt-2 font-mono text-xs text-slate-600">
        QR solución: <span className="text-slate-900">{etapa.codigoQrSolucion}</span>
      </p>

      {etapa.pistas.length > 0 ? (
        <ul className="mt-2 space-y-1">
          {etapa.pistas.map((p) => (
            <PistaRow key={p.pistaId} pista={p} />
          ))}
        </ul>
      ) : (
        <p className="mt-2 text-xs text-slate-500">Sin pistas en esta etapa.</p>
      )}

      {error && <div className="mt-2"><ErrorState message={error} /></div>}

      {!adding ? (
        <button
          type="button"
          className="mt-2 text-sm text-indigo-600 hover:underline"
          onClick={() => setAdding(true)}
        >
          + Agregar pista a esta etapa
        </button>
      ) : (
        <div className="mt-3 space-y-2">
          <PistasEditor pistas={pistasDraft} onChange={setPistasDraft} />
          <div className="flex gap-2">
            <button
              type="button"
              disabled={agregar.isPending}
              onClick={() => {
                setError(null)
                void agregar.mutateAsync()
              }}
              className={btnPrimary}
            >
              {agregar.isPending ? 'Guardando…' : 'Guardar pista'}
            </button>
            <button
              type="button"
              className="text-sm text-slate-600 hover:underline"
              onClick={() => setAdding(false)}
            >
              Cancelar
            </button>
          </div>
        </div>
      )}
    </li>
  )
}

export function MisionEtapasPanel({ mision: misionInicial, onClose }: MisionEtapasPanelProps) {
  const queryClient = useQueryClient()
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()
  const { data: mision = misionInicial, refetch } = useMision(misionInicial.id)

  const refreshMision = () => {
    void refetch()
    void queryClient.invalidateQueries({ queryKey: MISIONES_KEY })
    showSuccess('Pista agregada.')
  }

  return (
    <div className={`${cardClass} space-y-4`}>
      {successMessage && (
        <SuccessAlert message={successMessage} onDismiss={clearSuccess} />
      )}
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="font-medium text-slate-900">{mision.nombre}</h3>
          <p className="mt-1 text-sm text-slate-600">
            {mision.totalEtapas} etapa(s) · estado {mision.estado}
          </p>
        </div>
        <button type="button" onClick={onClose} className="text-sm text-indigo-600 hover:underline">
          Cerrar
        </button>
      </div>

      <p className="text-xs text-slate-500">
        Puedes añadir pistas a cada etapa. Al crear la sesión en vivo se copia este catálogo.
      </p>

      {mision.etapas.length === 0 ? (
        <p className="text-sm text-slate-600">Esta misión no tiene etapas registradas.</p>
      ) : (
        <ol className="space-y-2">
          {mision.etapas.map((etapa) => (
            <EtapaRow
              key={etapa.etapaId}
              misionId={mision.id}
              etapa={etapa}
              onPistaAdded={refreshMision}
            />
          ))}
        </ol>
      )}
    </div>
  )
}
