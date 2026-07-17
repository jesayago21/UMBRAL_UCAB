import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  View,
} from 'react-native'
import { SafeAreaView } from 'react-native-safe-area-context'
import type { NativeStackScreenProps } from '@react-navigation/native-stack'
import {
  EtapaPanel,
  TableroHeader,
  esEtapaBusquedaTesoro,
} from '@/components/busqueda/EtapaPanels'
import { EvidenciaQrPanel } from '@/components/busqueda/EvidenciaQrPanel'
import { RankingPanel } from '@/components/shared/RankingPanel'
import { TriviaLivePanel } from '@/components/trivia/TriviaLivePanel'
import {
  useAbandonarSesion,
  useEnviarEvidencia,
  useMiInscripcionParticipante,
} from '@/hooks/useSesiones'
import {
  useSesionHub,
  type ParticipanteExpulsadoPayload,
} from '@/hooks/useSesionHub'
import {
  clearParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import type { RootStackParamList } from '@/navigation/types'
import { getApiErrorMessage } from '@/services/apiClient'
import { styles } from '@/styles/ui'

type Props = NativeStackScreenProps<RootStackParamList, 'Partida'>

export function PartidaScreen({ navigation, route }: Props) {
  const { sesionId } = route.params
  const { data: inscripcion, isLoading, refetch } = useMiInscripcionParticipante()
  const abandonar = useAbandonarSesion(sesionId)
  const enviar = useEnviarEvidencia(sesionId)
  const [error, setError] = useState<string | null>(null)

  const onExpulsado = useCallback(
    async (_payload: ParticipanteExpulsadoPayload) => {
      await clearParticipanteSesionInscrita()
      navigation.replace('Lobby')
    },
    [navigation],
  )

  const hubStatus = useSesionHub({
    sesionId,
    participanteId: inscripcion?.participanteId,
    enabled: Boolean(inscripcion && inscripcion.sesionId === sesionId),
    onParticipanteExpulsado: (p) => void onExpulsado(p),
  })

  useEffect(() => {
    if (!inscripcion || inscripcion.sesionId !== sesionId) return
    void saveParticipanteSesionInscrita({
      sesionId: inscripcion.sesionId,
      titulo: inscripcion.titulo,
      participanteId: inscripcion.participanteId,
      joinedAt: new Date().toISOString(),
    })
  }, [inscripcion, sesionId])

  const etapasOrdenadas = useMemo(
    () => [...(inscripcion?.etapas ?? [])].sort((a, b) => a.orden - b.orden),
    [inscripcion?.etapas],
  )

  if (isLoading && !inscripcion) {
    return (
      <SafeAreaView style={styles.screen}>
        <ActivityIndicator style={{ marginTop: 40 }} />
      </SafeAreaView>
    )
  }

  if (!inscripcion || inscripcion.sesionId !== sesionId) {
    return (
      <SafeAreaView style={styles.screen}>
        <View style={styles.content}>
          <Text style={styles.error}>No tienes inscripción en esta sesión.</Text>
          <Pressable style={styles.btnSecondary} onPress={() => navigation.replace('Lobby')}>
            <Text style={styles.btnSecondaryText}>Volver al inicio</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    )
  }

  const preJuego =
    inscripcion.estado === 'EnPreparacion' || inscripcion.estado === 'Programada'
  const sesionEnJuego =
    inscripcion.estado === 'Activa' || inscripcion.estado === 'Pausada'
  const postJuego =
    inscripcion.estado === 'Finalizada' || inscripcion.estado === 'Cancelada'
  const etapaReferencia = preJuego
    ? etapasOrdenadas[0]
    : (etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0])
  const muestraQr =
    sesionEnJuego &&
    etapaReferencia != null &&
    esEtapaBusquedaTesoro(etapaReferencia) &&
    inscripcion.estado === 'Activa'
  const muestraTrivia =
    sesionEnJuego && etapaReferencia != null && etapaReferencia.tipoEtapa === 'Trivia'
  const puedeAbandonar = !sesionEnJuego

  return (
    <SafeAreaView style={styles.screen}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.rowBetween}>
          <Text style={styles.title}>{postJuego ? 'Resultados' : 'Partida'}</Text>
          {puedeAbandonar ? (
            <Pressable
              disabled={abandonar.isPending}
              onPress={() => {
                void (async () => {
                  try {
                    await abandonar.mutateAsync()
                    await clearParticipanteSesionInscrita()
                    navigation.replace('Lobby')
                  } catch (err) {
                    setError(getApiErrorMessage(err))
                  }
                })()
              }}
            >
              <Text style={styles.btnSecondaryText}>
                {postJuego ? 'Salir' : 'Abandonar'}
              </Text>
            </Pressable>
          ) : null}
        </View>

        <View style={styles.card}>
          {postJuego ? (
            <Text style={styles.muted}>
              {inscripcion.estado === 'Cancelada' ? 'Sesión cancelada' : 'Partida finalizada'}
            </Text>
          ) : null}
          <Text style={styles.heading}>{inscripcion.titulo}</Text>
          <Text style={styles.body}>
            {preJuego
              ? 'Espera a que el operador inicie la partida.'
              : postJuego
                ? 'Revisa el ranking final.'
                : 'La sesión está en curso.'}
          </Text>
          {!puedeAbandonar ? (
            <Text style={[styles.muted, { color: '#b45309' }]}>
              No puedes abandonar mientras la sesión está en juego.
            </Text>
          ) : null}
          {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>

        <TableroHeader
          inscripcion={inscripcion}
          participanteId={inscripcion.participanteId}
          etapaActual={postJuego ? undefined : etapaReferencia}
        />

        {muestraTrivia ? (
          <TriviaLivePanel
            sesionId={sesionId}
            enabled={inscripcion.estado === 'Activa' || inscripcion.estado === 'Pausada'}
          />
        ) : null}

        {!postJuego ? (
          <EtapaPanel estadoSesion={inscripcion.estado} etapas={inscripcion.etapas} />
        ) : null}

        {muestraQr ? (
          <EvidenciaQrPanel
            enviando={enviar.isPending}
            onEnviar={async (codigoQr) => {
              try {
                const result = await enviar.mutateAsync({ codigoQr })
                void refetch()
                return result
              } catch (err) {
                throw new Error(getApiErrorMessage(err))
              }
            }}
          />
        ) : null}

        {sesionEnJuego && inscripcion.estado === 'Pausada' ? (
          <Text style={[styles.muted, { color: '#b45309' }]}>Sesión pausada.</Text>
        ) : null}

        <RankingPanel
          sesionId={sesionId}
          participanteId={inscripcion.participanteId}
          title={postJuego ? 'Ranking final' : 'Ranking'}
          hubStatus={hubStatus}
        />
      </ScrollView>
    </SafeAreaView>
  )
}
