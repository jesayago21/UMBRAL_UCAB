import { apiClient } from '@/services/apiClient'
import type {
  RegistroParticipanteRequest,
  RegistroParticipanteResponse,
} from '@/types/auth.types'

export async function registrarParticipante(
  body: RegistroParticipanteRequest,
): Promise<RegistroParticipanteResponse> {
  const { data } = await apiClient.post<RegistroParticipanteResponse>(
    '/auth/registro-participante',
    body,
  )
  return data
}
