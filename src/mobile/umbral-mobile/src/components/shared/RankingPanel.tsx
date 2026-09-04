import { ActivityIndicator, FlatList, Text, View } from 'react-native'
import { useRankingSesion } from '@/hooks/useSesiones'
import type { SesionHubConnectionStatus } from '@/hooks/useSesionHub'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import { getApiErrorMessage } from '@/services/apiClient'
import { styles } from '@/styles/ui'

interface RankingPanelProps {
  sesionId: string
  participanteId?: string
  title?: string
  hubStatus?: SesionHubConnectionStatus
}

export function RankingPanel({
  sesionId,
  participanteId,
  title = 'Ranking',
  hubStatus,
}: RankingPanelProps) {
  const { data, isLoading, isError, error } = useRankingSesion(sesionId, true, {
    refetchIntervalMs: hubStatus != null && hubStatus !== 'conectado' ? 3000 : false,
  })
  const ranking = data ? ordenarYNumerarRanking(data) : []

  return (
    <View style={styles.card}>
      <Text style={styles.heading}>{title}</Text>
      {isLoading ? <ActivityIndicator /> : null}
      {isError ? <Text style={styles.error}>{getApiErrorMessage(error)}</Text> : null}
      {!isLoading && ranking.length === 0 ? (
        <Text style={styles.muted}>Sin posiciones aún.</Text>
      ) : (
        <FlatList
          data={ranking}
          keyExtractor={(item) => item.participanteId}
          scrollEnabled={false}
          renderItem={({ item }) => {
            const mine = item.participanteId === participanteId
            return (
              <View style={[styles.rowBetween, { paddingVertical: 6 }]}>
                <Text style={[styles.body, mine && { fontWeight: '700' }]}>
                  #{item.posicion} {item.nombreParticipante}
                  {mine ? ' (tú)' : ''}
                </Text>
                <Text style={styles.body}>{item.puntajeTotal}</Text>
              </View>
            )
          }}
        />
      )}
    </View>
  )
}
