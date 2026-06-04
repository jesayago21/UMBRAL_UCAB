/**
 * Panel web temporal para Participante (pruebas E1).
 * Cuando exista umbral-mobile, poner VITE_PARTICIPANTE_WEB_ENABLED=false en .env.
 */
export function isParticipanteWebEnabled(): boolean {
  const raw = import.meta.env.VITE_PARTICIPANTE_WEB_ENABLED
  if (raw === undefined || raw === '') return true
  return raw !== 'false' && raw !== '0'
}
