export interface RegistroParticipanteRequest {
  email: string
  username: string
  nombre: string
  apellido: string
  password: string
}

export interface RegistroParticipanteResponse {
  keycloakUserId: string
  email: string
  username: string
}
