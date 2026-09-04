import AsyncStorage from '@react-native-async-storage/async-storage'
import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'
import type { RolUsuario } from '@/types/api.types'

interface AuthState {
  token: string | null
  rol: RolUsuario | null
  username: string | null
  login: (params: { token: string; rol: RolUsuario; username: string }) => void
  logout: () => void
  estaAutenticado: () => boolean
  esParticipante: () => boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      rol: null,
      username: null,
      login: ({ token, rol, username }) => set({ token, rol, username }),
      logout: () => set({ token: null, rol: null, username: null }),
      estaAutenticado: () => Boolean(get().token),
      esParticipante: () => get().rol === 'Participante',
    }),
    {
      name: 'umbral-mobile-auth',
      storage: createJSONStorage(() => AsyncStorage),
      partialize: (s) => ({
        token: s.token,
        rol: s.rol,
        username: s.username,
      }),
    },
  ),
)
