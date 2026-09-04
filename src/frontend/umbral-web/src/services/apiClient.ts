import axios, { type AxiosError } from 'axios'
import { useAuthStore } from '@/store/authStore'
import type { ApiErrorResponse } from '@/types/api.types'

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:8000'

export const apiClient = axios.create({
  baseURL: `${baseURL}/api/v1`,
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
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)

export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const data = error.response?.data
    if (data?.errores) {
      const first = Object.values(data.errores)[0]?.[0]
      if (first) return first
    }
    if (data?.mensaje) return data.mensaje
    if (error.response?.status === 403) {
      return 'No tienes permiso para esta acción (403).'
    }
    if (error.response?.status === 500) {
      return (
        (data?.mensaje ?? 'Error interno del servidor.') +
        ' Si acabas de actualizar el backend, reinicia la API (aplica migraciones al arrancar en Development).'
      )
    }
  }
  if (error instanceof Error) return error.message
  return 'Error inesperado'
}
