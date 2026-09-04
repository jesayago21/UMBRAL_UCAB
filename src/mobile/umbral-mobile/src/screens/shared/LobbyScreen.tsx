import { useEffect, useState } from 'react'
import {
  ActivityIndicator,
  AppState,
  FlatList,
  Pressable,
  Text,
  TextInput,
  View,
} from 'react-native'
import { SafeAreaView } from 'react-native-safe-area-context'
import type { NativeStackScreenProps } from '@react-navigation/native-stack'
import { useQueryClient } from '@tanstack/react-query'
import { useKeycloakAuth } from '@/hooks/useKeycloakAuth'
import {
  MI_INSCRIPCION_PARTICIPANTE_KEY,
  SESIONES_DISPONIBLES_KEY,
  useAbandonarSesion,
  useMiInscripcionParticipante,
  useSesionesDisponibles,
  useUnirseSesion,
} from '@/hooks/useSesiones'
import {
  clearParticipanteSesionInscrita,
  saveParticipanteSesionInscrita,
} from '@/lib/participanteSesionStorage'
import { getApiErrorMessage } from '@/services/apiClient'
import { useAuthStore } from '@/store/authStore'
import { styles } from '@/styles/ui'
import type { RootStackParamList } from '@/navigation/types'
import type { MiInscripcionParticipanteDto } from '@/types/sesion.types'
import { MAX_PARTICIPANTES_SESION } from '@/types/sesion.types'

type Props = NativeStackScreenProps<RootStackParamList, 'Lobby'>

/** Solo inscripción en curso bloquea el lobby (no Finalizada/Cancelada). */
function inscripcionVisibleEnLobby(
  inscripcion: MiInscripcionParticipanteDto | null | undefined,
): inscripcion is MiInscripcionParticipanteDto {
  if (!inscripcion?.sesionId) return false
  return (
    inscripcion.estado === 'EnPreparacion' ||
    inscripcion.estado === 'Programada' ||
    inscripcion.estado === 'Activa' ||
    inscripcion.estado === 'Pausada'
  )
}

async function limpiarInscripcionLocal(
  queryClient: ReturnType<typeof useQueryClient>,
): Promise<void> {
  queryClient.setQueryData(MI_INSCRIPCION_PARTICIPANTE_KEY, null)
  await clearParticipanteSesionInscrita()
  await queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
  await queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
}

export function LobbyScreen({ navigation }: Props) {
  const queryClient = useQueryClient()
  const username = useAuthStore((s) => s.username)
  const { logout } = useKeycloakAuth()
  const { data: sesiones, isLoading, isError, error } = useSesionesDisponibles()
  const {
    data: inscripcionRaw,
    isLoading: cargandoInscripcion,
    isFetched,
    isError: errorInscripcion,
  } = useMiInscripcionParticipante()
  const inscripcion = inscripcionVisibleEnLobby(inscripcionRaw) ? inscripcionRaw : null
  const [sesionId, setSesionId] = useState('')
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const unirse = useUnirseSesion(sesionId)
  const abandonar = useAbandonarSesion(inscripcion?.sesionId ?? '')

  // Al volver a la app, refresca lista e inscripción (efecto “en vivo”).
  useEffect(() => {
    const sub = AppState.addEventListener('change', (state) => {
      if (state !== 'active') return
      void queryClient.invalidateQueries({ queryKey: SESIONES_DISPONIBLES_KEY })
      void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
    })
    return () => sub.remove()
  }, [queryClient])

  useEffect(() => {
    // Finalizada/Cancelada no deben borrar el cache de React Query: Partida (montada
    // debajo en el stack) necesita esos datos. Solo limpiamos storage local.
    if (
      inscripcionRaw &&
      (inscripcionRaw.estado === 'Cancelada' || inscripcionRaw.estado === 'Finalizada')
    ) {
      void clearParticipanteSesionInscrita()
      return
    }
    if (inscripcion?.sesionId) {
      void saveParticipanteSesionInscrita({
        sesionId: inscripcion.sesionId,
        titulo: inscripcion.titulo,
        participanteId: inscripcion.participanteId,
        joinedAt: new Date().toISOString(),
      })
      return
    }
    if (isFetched && !errorInscripcion && !inscripcionRaw) {
      void clearParticipanteSesionInscrita()
    }
  }, [inscripcion, inscripcionRaw, isFetched, errorInscripcion])

  const salirDeSesionLocal = async () => {
    setFormError(null)
    try {
      if (inscripcion?.sesionId) {
        try {
          await abandonar.mutateAsync()
        } catch {
          // Sesión ya no existe / no abandonable: limpiamos igual.
        }
      }
      await limpiarInscripcionLocal(queryClient)
    } catch (err) {
      setFormError(getApiErrorMessage(err))
      await limpiarInscripcionLocal(queryClient)
    }
  }

  // Primera carga: no bloquear toda la pantalla (evita “spinner eterno” si la API tarda).
  if (cargandoInscripcion && !isFetched) {
    return (
      <SafeAreaView style={styles.screen}>
        <View style={styles.content}>
          <Text style={styles.title}>Inicio</Text>
          <Text style={styles.muted}>{username ?? '…'}</Text>
          <ActivityIndicator style={{ marginTop: 24 }} />
          <Text style={[styles.muted, { marginTop: 12, textAlign: 'center' }]}>
            Conectando con el servidor…
          </Text>
          <Pressable style={[styles.btnSecondary, { marginTop: 24 }]} onPress={() => void logout()}>
            <Text style={styles.btnSecondaryText}>Cerrar sesión</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    )
  }

  if (errorInscripcion && !inscripcionRaw) {
    return (
      <SafeAreaView style={styles.screen}>
        <View style={styles.content}>
          <Text style={styles.error}>No se pudo cargar tu inscripción.</Text>
          <Pressable
            style={styles.btnPrimary}
            onPress={() =>
              void queryClient.invalidateQueries({ queryKey: MI_INSCRIPCION_PARTICIPANTE_KEY })
            }
          >
            <Text style={styles.btnPrimaryText}>Reintentar</Text>
          </Pressable>
          <Pressable style={styles.btnSecondary} onPress={() => void logout()}>
            <Text style={styles.btnSecondaryText}>Cerrar sesión</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    )
  }

  if (inscripcion) {
    const postJuego = inscripcion.estado === 'Finalizada'
    const enJuego = inscripcion.estado === 'Activa' || inscripcion.estado === 'Pausada'

    return (
      <SafeAreaView style={styles.screen}>
        <View style={styles.content}>
          <View style={styles.rowBetween}>
            <Text style={styles.title}>Inicio</Text>
            <Pressable onPress={() => void logout()}>
              <Text style={styles.btnSecondaryText}>Salir</Text>
            </Pressable>
          </View>
          <Text style={styles.muted}>{username}</Text>
          <View style={styles.card}>
            <Text style={styles.heading}>{inscripcion.titulo}</Text>
            <Text style={styles.muted}>Estado: {inscripcion.estado}</Text>
            <Text style={styles.muted}>
              Participantes:{' '}
              {inscripcion.participantesInscritos ?? '—'}/
              {inscripcion.maxParticipantes ?? MAX_PARTICIPANTES_SESION}
            </Text>
            <Pressable
              style={styles.btnPrimary}
              onPress={() =>
                navigation.navigate('Partida', { sesionId: inscripcion.sesionId })
              }
            >
              <Text style={styles.btnPrimaryText}>
                {postJuego ? 'Ver ranking final' : 'Ir a mi partida'}
              </Text>
            </Pressable>
            {!enJuego ? (
              <Pressable
                style={[styles.btnSecondary, { marginTop: 8 }]}
                disabled={abandonar.isPending}
                onPress={() => void salirDeSesionLocal()}
              >
                <Text style={styles.btnSecondaryText}>
                  {postJuego ? 'Salir de la sesión' : 'Abandonar sesión'}
                </Text>
              </Pressable>
            ) : null}
            <Pressable
              style={{ marginTop: 10, alignItems: 'center' }}
              onPress={() => void salirDeSesionLocal()}
            >
              <Text style={styles.muted}>Limpiar e ir al lobby</Text>
            </Pressable>
            {formError ? <Text style={styles.error}>{formError}</Text> : null}
          </View>
        </View>
      </SafeAreaView>
    )
  }

  return (
    <SafeAreaView style={styles.screen}>
      <View style={styles.content}>
        <View style={styles.rowBetween}>
          <View>
            <Text style={styles.title}>Inicio</Text>
            <Text style={styles.subtitle}>Únete con el código del operador.</Text>
          </View>
          <Pressable onPress={() => void logout()}>
            <Text style={styles.btnSecondaryText}>Salir</Text>
          </Pressable>
        </View>

        {isLoading && !sesiones ? <ActivityIndicator /> : null}
        {isError ? <Text style={styles.error}>{getApiErrorMessage(error)}</Text> : null}

        <FlatList
          data={sesiones ?? []}
          keyExtractor={(item) => item.id}
          ListEmptyComponent={
            !isLoading ? (
              <Text style={styles.muted}>No hay sesiones abiertas a inscripción.</Text>
            ) : null
          }
          renderItem={({ item }) => {
            const selected = sesionId === item.id
            const max = item.maxParticipantes ?? MAX_PARTICIPANTES_SESION
            const llena = item.participantesInscritos >= max
            return (
              <Pressable
                onPress={() => setSesionId(item.id)}
                style={[
                  styles.card,
                  selected && { borderColor: '#4f46e5', backgroundColor: '#eef2ff' },
                ]}
              >
                <Text style={styles.heading}>{item.titulo}</Text>
                <Text style={styles.muted}>
                  {item.estado} · {item.participantesInscritos}/{max} participantes
                  {llena ? ' · Llena' : ''}
                </Text>
              </Pressable>
            )
          }}
          style={{ maxHeight: 220 }}
        />

        <View style={styles.card}>
          <TextInput
            style={styles.input}
            placeholder="Código de acceso"
            autoCapitalize="characters"
            value={codigo}
            onChangeText={setCodigo}
          />
          <TextInput
            style={styles.input}
            placeholder="Nombre (opcional)"
            value={nombre}
            onChangeText={setNombre}
          />
          {formError ? <Text style={styles.error}>{formError}</Text> : null}
          <Pressable
            style={[styles.btnPrimary, unirse.isPending && { opacity: 0.6 }]}
            disabled={unirse.isPending}
            onPress={() => {
              void (async () => {
                setFormError(null)
                if (!sesionId || !codigo.trim()) {
                  setFormError('Selecciona una sesión e ingresa el código.')
                  return
                }
                const seleccionada = sesiones?.find((s) => s.id === sesionId)
                const max = seleccionada?.maxParticipantes ?? MAX_PARTICIPANTES_SESION
                if (seleccionada && seleccionada.participantesInscritos >= max) {
                  setFormError(`La sesión ya está llena (máximo ${max} participantes).`)
                  return
                }
                try {
                  const result = await unirse.mutateAsync({
                    codigoAcceso: codigo.trim(),
                    nombreParticipante: nombre.trim() || undefined,
                  })
                  const titulo =
                    sesiones?.find((s) => s.id === sesionId)?.titulo ?? 'Sesión'
                  await saveParticipanteSesionInscrita({
                    sesionId,
                    titulo,
                    participanteId: result.participanteId,
                    joinedAt: new Date().toISOString(),
                  })
                  navigation.replace('Partida', { sesionId })
                } catch (err) {
                  setFormError(getApiErrorMessage(err))
                }
              })()
            }}
          >
            <Text style={styles.btnPrimaryText}>
              {unirse.isPending ? 'Uniendo…' : 'Unirse a la sesión'}
            </Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  )
}
