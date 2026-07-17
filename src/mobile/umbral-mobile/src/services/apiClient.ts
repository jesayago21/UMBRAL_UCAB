import axios, { type AxiosError } from 'axios'
import { resolveApiBaseUrl } from '@/lib/env'
import { useAuthStore } from '@/store/authStore'
import type { ApiErrorResponse } from '@/types/api.types'

export const apiClient = axios.create({
  baseURL: `${resolveApiBaseUrl()}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout()
    }
    return Promise.reject(error)
  },
)

export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status
    const raw = error.response?.data
    if (typeof raw === 'string' && raw.trimStart().startsWith('<')) {
      return (
        `La API devolvió HTML (${status ?? '?'}) en vez de JSON. ` +
        `Revisa EXPO_PUBLIC_API_URL (emulador: http://10.0.2.2:5000).`
      )
    }
    if (
      error.message.includes('JSON Parse error') ||
      error.message.includes('Unexpected character')
    ) {
      return (
        `Respuesta no JSON de la API (${status ?? '?'}). ` +
        `Base: ${resolveApiBaseUrl()}. Usa :5000 directo en emulador.`
      )
    }
    const data = raw as ApiErrorResponse | undefined
    if (data?.errores) {
      const first = Object.values(data.errores)[0]?.[0]
      if (first) return first
    }
    if (data?.mensaje) return data.mensaje
    if (status === 403) return 'No tienes permiso para esta acción.'
    if (status === 401) return 'Sesión expirada o token inválido.'
    if (error.message === 'Network Error') {
      return `Sin conexión a ${resolveApiBaseUrl()}. ¿API levantada?`
    }
  }
  if (error instanceof Error) return error.message
  return 'Error inesperado'
}
