import { useEffect, useState } from 'react'
import { ActivityIndicator, Pressable, Text, View } from 'react-native'
import {
  GameFeedbackBanner,
  feedbackDesdeTrivia,
  type GameFeedback,
} from '@/components/shared/GameFeedbackBanner'
import { useEstadoTriviaSesion, useSubmitRespuestaTrivia } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'
import { styles } from '@/styles/ui'

interface TriviaLivePanelProps {
  sesionId: string
  enabled: boolean
}

export function TriviaLivePanel({ sesionId, enabled }: TriviaLivePanelProps) {
  const { data, isLoading, isError, error } = useEstadoTriviaSesion(sesionId, enabled)
  const submit = useSubmitRespuestaTrivia(sesionId)
  const [seleccion, setSeleccion] = useState<number | null>(null)
  const [ahoraMs, setAhoraMs] = useState(() => Date.now())
  const [feedback, setFeedback] = useState<GameFeedback | null>(null)
  const [ultimaPreguntaFeedback, setUltimaPreguntaFeedback] = useState<string | null>(null)

  const fase = data?.fase ?? 'Esperando'
  const timerIso = data?.timerCerradoEnUtc
  const activa = fase === 'PreguntaActiva' && Boolean(timerIso)
  const preguntaId = data?.preguntaId ?? null

  useEffect(() => {
    setSeleccion(data?.indiceOpcionSeleccionada ?? null)
  }, [preguntaId, data?.indiceOpcionSeleccionada])

  useEffect(() => {
    if (!activa && fase !== 'Transicion') return
    const id = setInterval(() => setAhoraMs(Date.now()), 250)
    return () => clearInterval(id)
  }, [activa, fase, timerIso, data?.transicionHastaUtc])

  // Feedback desde estado del servidor (tras confirmar / poll).
  useEffect(() => {
    if (!data?.yaRespondio || data.ultimaRespuestaEsCorrecta == null || !preguntaId) return
    if (ultimaPreguntaFeedback === preguntaId) return
    setUltimaPreguntaFeedback(preguntaId)
    setFeedback(
      feedbackDesdeTrivia({
        esCorrecta: Boolean(data.ultimaRespuestaEsCorrecta),
        fueraDeTiempo: Boolean(data.ultimaRespuestaFueraDeTiempo),
        puntos: data.ultimaRespuestaPuntos ?? 0,
        total: data.puntajeTotalParticipante ?? 0,
      }),
    )
  }, [
    data?.yaRespondio,
    data?.ultimaRespuestaEsCorrecta,
    data?.ultimaRespuestaFueraDeTiempo,
    data?.ultimaRespuestaPuntos,
    data?.puntajeTotalParticipante,
    preguntaId,
    ultimaPreguntaFeedback,
  ])

  useEffect(() => {
    if (fase === 'Transicion' || fase === 'Esperando') {
      setFeedback(null)
      setUltimaPreguntaFeedback(null)
    }
  }, [fase, preguntaId])

  if (!enabled) return null
  if (isLoading && !data) return <ActivityIndicator />
  if (isError) return <Text style={styles.error}>{getApiErrorMessage(error)}</Text>

  if (fase === 'Esperando' || !data?.enunciado) {
    return (
      <View style={styles.card}>
        <Text style={styles.heading}>Esperando inicio de la trivia</Text>
        <Text style={styles.muted}>
          El operador iniciará la etapa. Si no respondes a tiempo, sumas 0 puntos.
        </Text>
        {data && data.totalPreguntas > 0 ? (
          <Text style={styles.muted}>{data.totalPreguntas} preguntas en la etapa</Text>
        ) : null}
      </View>
    )
  }

  if (fase === 'Transicion') {
    return (
      <View style={styles.card}>
        <Text style={styles.heading}>Preparando siguiente pregunta…</Text>
        <Text style={styles.muted}>
          Pregunta {(data.preguntaIndexActual ?? 0) + 1} de {data.totalPreguntas}
        </Text>
        {feedback ? <GameFeedbackBanner feedback={feedback} /> : null}
      </View>
    )
  }

  let segundosRestantes: number | null = null
  if (activa && timerIso) {
    const fin = Date.parse(timerIso)
    if (!Number.isNaN(fin)) segundosRestantes = Math.max(0, (fin - ahoraMs) / 1000)
  }
  const timerAgotado = segundosRestantes != null && segundosRestantes <= 0
  const bloqueado =
    Boolean(data.yaRespondio) || submit.isPending || timerAgotado || !activa

  return (
    <View style={styles.card}>
      <View style={styles.rowBetween}>
        <Text style={styles.muted}>
          Pregunta {data.orden ?? data.preguntaIndexActual + 1} / {data.totalPreguntas}
        </Text>
        {segundosRestantes != null ? (
          <Text style={styles.heading}>{Math.ceil(segundosRestantes)}s</Text>
        ) : null}
      </View>
      <Text style={styles.heading}>{data.enunciado}</Text>

      {feedback ? (
        <GameFeedbackBanner feedback={feedback} onDismiss={() => setFeedback(null)} />
      ) : null}

      {(data.opciones ?? []).map((opcion, idx) => {
        const selected = seleccion === idx
        return (
          <Pressable
            key={`${data.preguntaId}-${idx}`}
            disabled={bloqueado}
            onPress={() => setSeleccion(idx)}
            style={[
              styles.btnSecondary,
              selected && { borderColor: '#4f46e5', backgroundColor: '#eef2ff' },
              bloqueado && { opacity: 0.7 },
            ]}
          >
            <Text style={styles.btnSecondaryText}>{opcion}</Text>
          </Pressable>
        )
      })}
      <Pressable
        style={[styles.btnPrimary, (bloqueado || seleccion == null) && { opacity: 0.6 }]}
        disabled={bloqueado || seleccion == null}
        onPress={() => {
          if (!preguntaId || seleccion == null) return
          setFeedback(null)
          submit.mutate(
            {
              preguntaId,
              indiceOpcion: seleccion,
              duracionTimerSegundos: 30,
            },
            {
              onSuccess: () => {
                // El poll de estado trae ultimaRespuesta*; el effect arma el banner.
              },
            },
          )
        }}
      >
        <Text style={styles.btnPrimaryText}>
          {submit.isPending ? 'Enviando…' : 'Confirmar respuesta'}
        </Text>
      </Pressable>
      {submit.isError ? (
        <Text style={styles.error}>{getApiErrorMessage(submit.error)}</Text>
      ) : null}
    </View>
  )
}
