import { Text, View } from 'react-native'
import { GameFeedbackBanner } from '@/components/shared/GameFeedbackBanner'
import { styles } from '@/styles/ui'
import type { PenalizacionParticipanteDto } from '@/types/sesion.types'

interface PenalizacionesPanelProps {
  penalizaciones: PenalizacionParticipanteDto[]
  avisoVivo?: { puntos: number; motivo: string } | null
  onDismissAviso?: () => void
}

/** Aviso en vivo + historial de penalizaciones (HU-16). */
export function PenalizacionesPanel({
  penalizaciones,
  avisoVivo,
  onDismissAviso,
}: PenalizacionesPanelProps) {
  if (!avisoVivo && penalizaciones.length === 0) return null

  return (
    <View style={{ gap: 10 }}>
      {avisoVivo ? (
        <GameFeedbackBanner
          feedback={{
            tone: 'error',
            title: `Te restaron ${avisoVivo.puntos} punto${avisoVivo.puntos === 1 ? '' : 's'}`,
            detail: `Motivo: ${avisoVivo.motivo}`,
          }}
          onDismiss={onDismissAviso}
        />
      ) : null}

      {penalizaciones.length > 0 ? (
        <View style={[styles.card, { borderColor: '#fcd34d', backgroundColor: '#fffbeb' }]}>
          <Text style={[styles.heading, { color: '#92400e' }]}>Penalizaciones</Text>
          {penalizaciones.map((p, i) => (
            <View
              key={`${p.ocurridoEn}-${i}`}
              style={{
                marginTop: 8,
                paddingVertical: 8,
                borderTopWidth: i === 0 ? 0 : 1,
                borderTopColor: '#fde68a',
              }}
            >
              <Text style={{ fontWeight: '700', color: '#78350f' }}>
                −{p.puntos} punto{p.puntos === 1 ? '' : 's'}
              </Text>
              <Text style={[styles.body, { color: '#92400e', marginTop: 2 }]}>
                {p.motivo}
              </Text>
            </View>
          ))}
        </View>
      ) : null}
    </View>
  )
}
