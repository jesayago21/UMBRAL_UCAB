import AsyncStorage from '@react-native-async-storage/async-storage'
import type { ParticipanteSesionInscrita } from '@/types/sesion.types'

const KEY = 'umbral.participante.sesion'

export async function getParticipanteSesionInscrita(): Promise<ParticipanteSesionInscrita | null> {
  const raw = await AsyncStorage.getItem(KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as ParticipanteSesionInscrita
  } catch {
    return null
  }
}

export async function saveParticipanteSesionInscrita(
  value: ParticipanteSesionInscrita,
): Promise<void> {
  await AsyncStorage.setItem(KEY, JSON.stringify(value))
}

export async function clearParticipanteSesionInscrita(): Promise<void> {
  await AsyncStorage.removeItem(KEY)
}
