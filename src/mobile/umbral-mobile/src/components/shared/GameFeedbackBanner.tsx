import { Pressable, Text, View, StyleSheet } from 'react-native'
import { colors } from '@/styles/ui'

export type GameFeedbackTone = 'success' | 'error' | 'neutral'

export interface GameFeedback {
  tone: GameFeedbackTone
  title: string
  detail?: string
}

/** Banner lúdico para resultados de evidencia / trivia. */
export function GameFeedbackBanner({
  feedback,
  onDismiss,
}: {
  feedback: GameFeedback
  onDismiss?: () => void
}) {
  const palette =
    feedback.tone === 'success'
      ? { bg: '#ecfdf5', border: '#6ee7b7', title: '#047857', body: '#065f46' }
      : feedback.tone === 'error'
        ? { bg: '#fef2f2', border: '#fca5a5', title: '#b91c1c', body: '#991b1b' }
        : { bg: colors.slate100, border: colors.slate200, title: colors.slate900, body: colors.slate700 }

  return (
    <View style={[local.box, { backgroundColor: palette.bg, borderColor: palette.border }]}>
      <View style={{ flex: 1, gap: 2 }}>
        <Text style={[local.title, { color: palette.title }]}>{feedback.title}</Text>
        {feedback.detail ? (
          <Text style={[local.detail, { color: palette.body }]}>{feedback.detail}</Text>
        ) : null}
      </View>
      {onDismiss ? (
        <Pressable onPress={onDismiss} hitSlop={8}>
          <Text style={{ color: palette.title, fontWeight: '600' }}>OK</Text>
        </Pressable>
      ) : null}
    </View>
  )
}

export function feedbackDesdeEvidencia(resultado: string): GameFeedback {
  switch (resultado) {
    case 'Valida':
      return {
        tone: 'success',
        title: '¡Tesoro conseguido!',
        detail: 'Código correcto. Avanzas en la misión y sumas puntos.',
      }
    case 'Invalida':
      return {
        tone: 'error',
        title: 'Ese no es el tesoro…',
        detail: 'QR incorrecto, ya usado o de otra etapa. Sigue buscando.',
      }
    case 'Rechazada':
      return {
        tone: 'error',
        title: 'No se pudo registrar',
        detail: 'La sesión no acepta evidencias ahora (¿pausada?).',
      }
    default:
      return {
        tone: 'neutral',
        title: `Resultado: ${resultado}`,
      }
  }
}

export function feedbackDesdeTrivia(opts: {
  esCorrecta: boolean
  fueraDeTiempo: boolean
  puntos: number
  total: number
}): GameFeedback {
  if (opts.fueraDeTiempo) {
    return {
      tone: 'error',
      title: '¡Se acabó el tiempo!',
      detail: `+0 pts · Total: ${opts.total}`,
    }
  }
  if (opts.esCorrecta) {
    return {
      tone: 'success',
      title: '¡Respuesta correcta!',
      detail: `+${opts.puntos} pts · Total: ${opts.total}`,
    }
  }
  return {
    tone: 'error',
    title: 'Respuesta incorrecta',
    detail: `+0 pts · Total: ${opts.total}. ¡La próxima!`,
  }
}

const local = StyleSheet.create({
  box: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    borderWidth: 1,
    borderRadius: 12,
    paddingHorizontal: 14,
    paddingVertical: 12,
  },
  title: {
    fontSize: 16,
    fontWeight: '700',
  },
  detail: {
    fontSize: 13,
    lineHeight: 18,
  },
})
