/**
 * Panel web temporal para Participante (pruebas E1).
 * Oficial: VITE_PARTICIPANTE_WEB_ENABLED=false (jugador en umbral-mobile).
 * true = legacy: permite /participante en web.
 */
export function isParticipanteWebEnabled(): boolean {
  const raw = import.meta.env.VITE_PARTICIPANTE_WEB_ENABLED
  if (raw === undefined || raw === '') return true
  return raw !== 'false' && raw !== '0'
}
