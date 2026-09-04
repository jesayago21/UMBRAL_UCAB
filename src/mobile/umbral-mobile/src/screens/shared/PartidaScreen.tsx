import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
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
import { PenalizacionesPanel } from '@/components/shared/PenalizacionesPanel'
import { TriviaLivePanel } from '@/components/trivia/TriviaLivePanel'
import {
  useAbandonarSesion,
  useEnviarEvidencia,
  useMiInscripcionParticipante,
} from '@/hooks/useSesiones'
import {
  useSesionHub,
  type ParticipanteExpulsadoPayload,
  type PenalizacionAplicadaPayload,
} from '@/hooks/useSesionHub'
import {
  clearParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import type { RootStackParamList } from '@/navigation/types'
import { getApiErrorMessage } from '@/services/apiClient'
import { styles } from '@/styles/ui'
import type { MiInscripcionParticipanteDto } from '@/types/sesion.types'

type Props = NativeStackScreenProps<RootStackParamList, 'Partida'>

/** Pausa antes de mostrar el resumen/ranking final al terminar la partida en vivo. */
const RESUMEN_FINAL_DELAY_MS = 3000

export function PartidaScreen({ navigation, route }: Props) {
  const { sesionId } = route.params
  const {
    data: inscripcion,
    isLoading,
    isError,
    error: errorInscripcion,
    refetch,
  } = useMiInscripcionParticipante()
  const abandonar = useAbandonarSesion(sesionId)
  const enviar = useEnviarEvidencia(sesionId)
  const [error, setError] = useState<string | null>(null)
  // Evita flash de error al finalizar: conserva datos de esta partida durante el refetch.
  const [partidaLocal, setPartidaLocal] = useState<MiInscripcionParticipanteDto | null>(null)
  const [avisoPenalizacion, setAvisoPenalizacion] = useState<{
    puntos: number
    motivo: string
  } | null>(null)
  /** Evidencia válida reciente → aplazar UI de resumen cuando llegue Finalizada. */
  const [esperarResumenTrasEvidencia, setEsperarResumenTrasEvidencia] = useState(false)
  const [resumenFinalListo, setResumenFinalListo] = useState(false)
  /** Estado en el render anterior: detecta finalizaciones vistas en vivo (ej. trivia). */
  const estadoPrevioRef = useRef<string | undefined>(undefined)

  const onExpulsado = useCallback(
    async (_payload: ParticipanteExpulsadoPayload) => {
      await clearParticipanteSesionInscrita()
      setPartidaLocal(null)
      navigation.replace('Lobby')
    },
    [navigation],
  )

  const onPenalizacion = useCallback((payload: PenalizacionAplicadaPayload) => {
    setAvisoPenalizacion({ puntos: payload.puntos, motivo: payload.motivo })
  }, [])

  useEffect(() => {
    setEsperarResumenTrasEvidencia(false)
    setResumenFinalListo(false)
  }, [sesionId])

  useEffect(() => {
    if (inscripcion?.sesionId === sesionId) {
      setPartidaLocal(inscripcion)
    }
  }, [inscripcion, sesionId])

  const vista =
    inscripcion?.sesionId === sesionId
      ? inscripcion
      : partidaLocal?.sesionId === sesionId
        ? partidaLocal
        : null

  useEffect(() => {
    const estadoPrevio = estadoPrevioRef.current
    estadoPrevioRef.current = vista?.estado

    if (vista?.estado !== 'Finalizada') {
      if (
        vista?.estado === 'Activa' ||
        vista?.estado === 'Pausada' ||
        vista?.estado === 'EnPreparacion' ||
        vista?.estado === 'Programada'
      ) {
        setResumenFinalListo(false)
      }
      return
    }

    // Con delay si la partida terminó en vivo (evidencia propia o fin de trivia).
    // Si se entra con la sesión ya finalizada, mostrar el resumen de inmediato.
    const finalizoEnVivo =
      esperarResumenTrasEvidencia ||
      estadoPrevio === 'Activa' ||
      estadoPrevio === 'Pausada'

    if (!finalizoEnVivo) {
      setResumenFinalListo(true)
      return
    }

    setResumenFinalListo(false)
    const timer = setTimeout(() => {
      setResumenFinalListo(true)
      setEsperarResumenTrasEvidencia(false)
    }, RESUMEN_FINAL_DELAY_MS)
    return () => clearTimeout(timer)
  }, [vista?.estado, esperarResumenTrasEvidencia])

  // Si la evidencia no cierra la sesión, no dejar el flag colgado.
  useEffect(() => {
    if (!esperarResumenTrasEvidencia) return
    if (vista?.estado === 'Finalizada') return
    const timer = setTimeout(() => setEsperarResumenTrasEvidencia(false), 10_000)
    return () => clearTimeout(timer)
  }, [esperarResumenTrasEvidencia, vista?.estado])

  const hubStatus = useSesionHub({
    sesionId,
    participanteId: vista?.participanteId,
    enabled: Boolean(vista?.participanteId),
    onParticipanteExpulsado: (p) => void onExpulsado(p),
    onPenalizacionAplicada: onPenalizacion,
    onEstadoSesionCambiado: (estado) => {
      if (!estado) return
      setPartidaLocal((prev) =>
        prev?.sesionId === sesionId ? { ...prev, estado } : prev,
      )
    },
  })

  useEffect(() => {
    if (!vista || vista.sesionId !== sesionId) return
    void saveParticipanteSesionInscrita({
      sesionId: vista.sesionId,
      titulo: vista.titulo,
      participanteId: vista.participanteId,
      joinedAt: new Date().toISOString(),
    })
  }, [vista, sesionId])

  const etapasOrdenadas = useMemo(
    () => [...(vista?.etapas ?? [])].sort((a, b) => a.orden - b.orden),
    [vista?.etapas],
  )

  if (isLoading && !vista) {
    return (
      <SafeAreaView style={styles.screen}>
        <ActivityIndicator style={{ marginTop: 40 }} />
      </SafeAreaView>
    )
  }

  if (isError && !vista) {
    return (
      <SafeAreaView style={styles.screen}>
        <View style={styles.content}>
          <Text style={styles.error}>{getApiErrorMessage(errorInscripcion)}</Text>
          <Pressable style={styles.btnPrimary} onPress={() => void refetch()}>
            <Text style={styles.btnPrimaryText}>Reintentar</Text>
          </Pressable>
          <Pressable style={styles.btnSecondary} onPress={() => navigation.replace('Lobby')}>
            <Text style={styles.btnSecondaryText}>Volver al lobby</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    )
  }

  if (!vista) {
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
    vista.estado === 'EnPreparacion' || vista.estado === 'Programada'
  const sesionEnJuego =
    vista.estado === 'Activa' || vista.estado === 'Pausada'
  const esperandoResumenFinal =
    vista.estado === 'Finalizada' && !resumenFinalListo
  const postJuego =
    vista.estado === 'Cancelada' ||
    (vista.estado === 'Finalizada' && resumenFinalListo)
  const etapaReferencia = preJuego
    ? etapasOrdenadas[0]
    : (etapasOrdenadas.find((e) => e.esActual) ?? etapasOrdenadas[0])
  const muestraQr =
    sesionEnJuego &&
    etapaReferencia != null &&
    esEtapaBusquedaTesoro(etapaReferencia) &&
    vista.estado === 'Activa'
  const muestraTrivia =
    sesionEnJuego && etapaReferencia != null && etapaReferencia.tipoEtapa === 'Trivia'
  const puedeAbandonar = !sesionEnJuego && !esperandoResumenFinal

  return (
    <SafeAreaView style={styles.screen}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.rowBetween}>
          <Text style={styles.title}>
            {esperandoResumenFinal ? '¡Completado!' : postJuego ? 'Resultados' : 'Partida'}
          </Text>
          {puedeAbandonar ? (
            <Pressable
              disabled={abandonar.isPending}
              onPress={() => {
                void (async () => {
                  try {
                    await abandonar.mutateAsync()
                    await clearParticipanteSesionInscrita()
                    setPartidaLocal(null)
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
          {esperandoResumenFinal ? (
            <>
              <Text style={styles.heading}>¡Misión completada!</Text>
              <Text style={styles.body}>
                {esperarResumenTrasEvidencia
                  ? 'Evidencia registrada. Preparando el ranking final…'
                  : 'La partida terminó. Preparando el ranking final…'}
              </Text>
              <ActivityIndicator style={{ marginTop: 12 }} />
            </>
          ) : (
            <>
              {postJuego ? (
                <Text style={styles.muted}>
                  {vista.estado === 'Cancelada' ? 'Sesión cancelada' : 'Partida finalizada'}
                </Text>
              ) : null}
              <Text style={styles.heading}>{vista.titulo}</Text>
              <Text style={styles.body}>
                {preJuego
                  ? 'Espera a que el operador inicie la partida.'
                  : postJuego
                    ? 'Revisa el ranking final.'
                    : 'La sesión está en curso.'}
              </Text>
            </>
          )}
          {!puedeAbandonar && !esperandoResumenFinal ? (
            <Text style={[styles.muted, { color: '#b45309' }]}>
              No puedes abandonar mientras la sesión está en juego.
            </Text>
          ) : null}
          {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>

        {!esperandoResumenFinal ? (
          <TableroHeader
            inscripcion={vista}
            participanteId={vista.participanteId}
            etapaActual={postJuego ? undefined : etapaReferencia}
          />
        ) : null}

        {!esperandoResumenFinal ? (
          <PenalizacionesPanel
            penalizaciones={vista.penalizaciones ?? []}
            avisoVivo={avisoPenalizacion}
            onDismissAviso={() => setAvisoPenalizacion(null)}
          />
        ) : null}

        {muestraTrivia ? (
          <TriviaLivePanel
            sesionId={sesionId}
            enabled={vista.estado === 'Activa' || vista.estado === 'Pausada'}
          />
        ) : null}

        {!postJuego && !esperandoResumenFinal ? (
          <EtapaPanel estadoSesion={vista.estado} etapas={vista.etapas} />
        ) : null}

        {muestraQr ? (
          <EvidenciaQrPanel
            enviando={enviar.isPending}
            onEnviar={async (codigoQr) => {
              try {
                const result = await enviar.mutateAsync({ codigoQr })
                if (result.resultado === 'Valida') {
                  setEsperarResumenTrasEvidencia(true)
                  setResumenFinalListo(false)
                }
                void refetch()
                return result
              } catch (err) {
                throw new Error(getApiErrorMessage(err))
              }
            }}
          />
        ) : null}

        {sesionEnJuego && vista.estado === 'Pausada' ? (
          <Text style={[styles.muted, { color: '#b45309' }]}>Sesión pausada.</Text>
        ) : null}

        {!esperandoResumenFinal ? (
          <RankingPanel
            sesionId={sesionId}
            participanteId={vista.participanteId}
            title={postJuego ? 'Ranking final' : 'Ranking'}
            hubStatus={hubStatus}
          />
        ) : null}
      </ScrollView>
    </SafeAreaView>
  )
}
