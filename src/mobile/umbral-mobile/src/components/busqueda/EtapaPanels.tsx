import { Text, View } from 'react-native'
import { MapaTesoroViewer, tieneUbicacion } from '@/components/busqueda/MapaTesoroViewer'
import { useRankingSesion } from '@/hooks/useSesiones'
import { ordenarYNumerarRanking } from '@/lib/ranking'
import { styles } from '@/styles/ui'
import type { EtapaSesionDto, MiInscripcionParticipanteDto } from '@/types/sesion.types'

const TIPO_LABEL: Record<string, string> = {
  BusquedaTesoro: 'Búsqueda del tesoro',
  Trivia: 'Trivia',
}

interface TableroHeaderProps {
  inscripcion: MiInscripcionParticipanteDto
  participanteId: string
  etapaActual?: EtapaSesionDto
}

export function TableroHeader({
  inscripcion,
  participanteId,
  etapaActual,
}: TableroHeaderProps) {
  const { data: ranking } = useRankingSesion(
    inscripcion.sesionId,
    inscripcion.estado === 'Activa' ||
      inscripcion.estado === 'Pausada' ||
      inscripcion.estado === 'Finalizada' ||
      inscripcion.estado === 'Cancelada',
  )
  const miPosicion = ranking
    ? ordenarYNumerarRanking(ranking).find((r) => r.participanteId === participanteId)
    : undefined
  const tipoLabel = etapaActual
    ? (TIPO_LABEL[etapaActual.tipoEtapa] ?? etapaActual.tipoEtapa)
    : null

  return (
    <View style={styles.card}>
      <View style={styles.rowBetween}>
        <View>
          <Text style={styles.muted}>Etapa</Text>
          <Text style={styles.heading}>
            {etapaActual
              ? `${etapaActual.orden}${inscripcion.totalEtapas != null ? ` / ${inscripcion.totalEtapas}` : ''}${tipoLabel ? ` · ${tipoLabel}` : ''}`
              : '—'}
          </Text>
        </View>
        <View>
          <Text style={styles.muted}>Tu puntaje</Text>
          <Text style={styles.heading}>
            {miPosicion != null ? `${miPosicion.puntajeTotal} (#${miPosicion.posicion})` : '—'}
          </Text>
        </View>
      </View>
    </View>
  )
}

interface EtapaPanelProps {
  estadoSesion: string
  etapas?: EtapaSesionDto[]
}

export function EtapaPanel({ estadoSesion, etapas }: EtapaPanelProps) {
  if (!Array.isArray(etapas) || etapas.length === 0) {
    return (
      <View style={styles.card}>
        <Text style={styles.muted}>Sin etapas.</Text>
      </View>
    )
  }
  const ordenadas = [...etapas].sort((a, b) => a.orden - b.orden)
  const preJuego = estadoSesion === 'EnPreparacion' || estadoSesion === 'Programada'
  const etapa = preJuego
    ? ordenadas[0]
    : (ordenadas.find((e) => e.esActual) ?? ordenadas[0])
  const esTrivia = etapa.tipoEtapa === 'Trivia'
  const pistas = etapa.pistas ?? []

  return (
    <View style={styles.card}>
      <Text style={styles.muted}>{preJuego ? 'Próxima etapa' : 'Etapa actual'}</Text>
      <Text style={styles.heading}>
        Etapa {etapa.orden} · {TIPO_LABEL[etapa.tipoEtapa] ?? etapa.tipoEtapa}
      </Text>

      {!esTrivia && tieneUbicacion(etapa) ? (
        <MapaTesoroViewer
          latitud={etapa.latitud!}
          longitud={etapa.longitud!}
          radioMetros={etapa.radioMetros!}
        />
      ) : null}

      {!esTrivia && !tieneUbicacion(etapa) ? (
        <Text style={[styles.muted, { marginTop: 8 }]}>
          Esta etapa no tiene ubicación en el mapa (lat/lon/radio).
        </Text>
      ) : null}

      {!esTrivia && pistas.length > 0 ? (
        <View style={{ gap: 6, marginTop: 10 }}>
          <Text style={styles.muted}>Pistas</Text>
          {pistas.map((p, i) => (
            <Text key={`${etapa.orden}-${i}`} style={styles.body}>
              • {p.contenido}
            </Text>
          ))}
        </View>
      ) : null}
    </View>
  )
}

export function esEtapaBusquedaTesoro(etapa: EtapaSesionDto): boolean {
  return etapa.tipoEtapa === 'BusquedaTesoro'
}
