import type { RolUsuario } from '@/types/api.types'

export function getHomePathForRol(rol: RolUsuario): string {
  switch (rol) {
    case 'Administrador':
      return '/admin/misiones'
    case 'Operador':
      return '/operador/sesiones'
    case 'EquipoParticipante':
      return '/login'
  }
}
