import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import type { RolUsuario } from '@/types/api.types'

interface AuthState {
  token: string | null
  rol: RolUsuario | null
  username: string | null
  login: (params: { token: string; rol: RolUsuario; username: string }) => void
  logout: () => void
  estaAutenticado: () => boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      rol: null,
      username: null,
      login: ({ token, rol, username }) => set({ token, rol, username }),
      logout: () => set({ token: null, rol: null, username: null }),
      estaAutenticado: () => !!get().token,
    }),
    {
      name: 'umbral-auth',
      storage: createJSONStorage(() => localStorage),
    },
  ),
)
