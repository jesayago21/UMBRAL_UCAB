import { useEffect, useState } from 'react'
import { useRankingSesion } from '@/hooks/useSesiones'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import { cardClass } from '@/styles/ui'
import type { EtapaSesionDto, MiInscripcionParticipanteDto } from '@/types/sesion.types'

const TIPO_ETAPA_LABEL: Record<string, string> = {
  BusquedaTesoro: 'Búsqueda del tesoro',
  Trivia: 'Trivia',
}

function formatCountdown(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds))
  const m = Math.floor(s / 60)
  const r = s % 60
  return `${m}:${r.toString().padStart(2, '0')}`
}

function segundosEfectivos(
  etapaIniciadaEn: string,
  segundosPausaAcumulados: number,
  pausadaDesde: string | null | undefined,
  ahoraMs: number,
): number {
  const inicio = Date.parse(etapaIniciadaEn)
  if (Number.isNaN(inicio)) return 0
  const bruto = (ahoraMs - inicio) / 1000
  const pausaActual = pausadaDesde
    ? Math.max(0, (ahoraMs - Date.parse(pausadaDesde)) / 1000)
    : 0
  return Math.max(0, bruto - segundosPausaAcumulados - pausaActual)
}

interface ParticipanteTableroHeaderProps {
  inscripcion: MiInscripcionParticipanteDto
  participanteId: string
  etapaActual: EtapaSesionDto | undefined
}

/** Cabecera del participante: etapa, puntaje y countdown de próxima pista. */
export function ParticipanteTableroHeader({
  inscripcion,
  participanteId,
  etapaActual,
}: ParticipanteTableroHeaderProps) {
  const { data: ranking } = useRankingSesion(
    inscripcion.sesionId,
    inscripcion.estado === 'Activa' ||
      inscripcion.estado === 'Pausada' ||
      inscripcion.estado === 'Finalizada' ||
      inscripcion.estado === 'Cancelada',
  )
  const miPosicion = ranking
    ? ordenarYNumerarRanking(ranking).find((r) => r.participanteId === participanteId)
    : undefined

  const [ahoraMs, setAhoraMs] = useState(() => Date.now())
  const pausada = inscripcion.estado === 'Pausada'
  const pendientes = inscripcion.pistasPorTiempoPendientes ?? []
  const proxima = pendientes[0]
  const puedeContar =
    Boolean(proxima) &&
    Boolean(inscripcion.etapaIniciadaEn) &&
    (inscripcion.estado === 'Activa' || pausada)

  useEffect(() => {
    if (!puedeContar || pausada) return
    const id = window.setInterval(() => setAhoraMs(Date.now()), 1000)
    return () => window.clearInterval(id)
  }, [puedeContar, pausada, inscripcion.etapaIniciadaEn, proxima?.pistaId])

  let segundosRestantes: number | null = null
  if (puedeContar && proxima && inscripcion.etapaIniciadaEn) {
    const efectivos = segundosEfectivos(
      inscripcion.etapaIniciadaEn,
      inscripcion.segundosPausaAcumulados ?? 0,
      pausada ? (inscripcion.pausadaDesde ?? new Date(ahoraMs).toISOString()) : inscripcion.pausadaDesde,
      ahoraMs,
    )
    segundosRestantes = Math.max(0, proxima.segundosLiberacion - efectivos)
  }

  const tipoLabel = etapaActual
    ? (TIPO_ETAPA_LABEL[etapaActual.tipoEtapa] ?? etapaActual.tipoEtapa)
    : null

  return (
    <section className={`${cardClass} border-slate-200 bg-slate-50/60`} aria-label="Progreso">
      <div
        className={`grid gap-3 sm:grid-cols-2 ${
          segundosRestantes != null ? 'lg:grid-cols-3' : ''
        }`}
      >
        <div>
          <p className="text-xs text-slate-500">Etapa</p>
          {etapaActual ? (
            <p className="mt-0.5 text-sm font-semibold text-slate-900">
              {etapaActual.orden}
              {inscripcion.totalEtapas != null ? ` / ${inscripcion.totalEtapas}` : ''}
              {tipoLabel ? (
                <span className="ml-1 font-normal text-slate-600">· {tipoLabel}</span>
              ) : null}
            </p>
          ) : (
            <p className="mt-0.5 text-sm text-slate-500">—</p>
          )}
        </div>
        <div>
          <p className="text-xs text-slate-500">Tu puntaje</p>
          <p className="mt-0.5 text-sm font-semibold text-slate-900">
            {miPosicion != null ? (
              <>
                {miPosicion.puntajeTotal}
                <span className="ml-1 font-normal text-slate-600">
                  (#{miPosicion.posicion})
                </span>
              </>
            ) : (
              '—'
            )}
          </p>
        </div>
        {segundosRestantes != null && (
          <div>
            <p className="text-xs text-slate-500">Próxima pista</p>
            <p className="mt-0.5 font-mono text-sm font-semibold text-indigo-900">
              {pausada ? 'Pausado · ' : ''}
              {segundosRestantes <= 0 ? 'Liberando…' : formatCountdown(segundosRestantes)}
            </p>
          </div>
        )}
      </div>
    </section>
  )
}
