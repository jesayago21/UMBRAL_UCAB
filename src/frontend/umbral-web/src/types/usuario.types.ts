export interface UsuarioDto {
  id: string
  keycloakUserId: string
  email: string
  username: string
  nombre: string
  apellido: string
  estado: string
  roles: string[]
}

export interface CrearUsuarioRequest {
  email: string
  username: string
  nombre: string
  apellido: string
  passwordTemporal: string
  roles: string[]
}

export interface AsignarRolesRequest {
  roles: string[]
}
