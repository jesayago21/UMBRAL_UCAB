/**
 * Panel web temporal para EquipoParticipante (pruebas E1).
 * Cuando exista umbral-mobile, poner VITE_EQUIPO_WEB_ENABLED=false en .env.
 */
export function isEquipoWebEnabled(): boolean {
  const raw = import.meta.env.VITE_EQUIPO_WEB_ENABLED
  if (raw === undefined || raw === '') return true
  return raw !== 'false' && raw !== '0'
}
