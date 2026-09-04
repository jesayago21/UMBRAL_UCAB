import type { PosicionRankingDto } from '@/types/sesion.types'

/**
 * Ordena por puntaje DESC, luego menor tiempo acumulado (HU-39 / RB-08),
 * luego nombre; renumera #1, #2, …
 */
export function ordenarYNumerarRanking(
  ranking: readonly PosicionRankingDto[],
): PosicionRankingDto[] {
  return [...ranking]
    .filter((r) => Boolean(r.participanteId))
    .sort((a, b) => {
      if (b.puntajeTotal !== a.puntajeTotal) return b.puntajeTotal - a.puntajeTotal
      const tiempoA = Number.isFinite(a.tiempoAcumuladoMs) ? (a.tiempoAcumuladoMs as number) : 0
      const tiempoB = Number.isFinite(b.tiempoAcumuladoMs) ? (b.tiempoAcumuladoMs as number) : 0
      if (tiempoA !== tiempoB) return tiempoA - tiempoB
      return a.nombreParticipante.localeCompare(b.nombreParticipante, 'es', {
        sensitivity: 'base',
      })
    })
    .map((row, index) => ({
      ...row,
      posicion: index + 1,
      puntajeTotal: Number.isFinite(row.puntajeTotal) ? row.puntajeTotal : 0,
      tiempoAcumuladoMs: Number.isFinite(row.tiempoAcumuladoMs)
        ? (row.tiempoAcumuladoMs as number)
        : 0,
    }))
}
