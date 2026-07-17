import { NavigationContainer } from '@react-navigation/native'
import { createNativeStackNavigator } from '@react-navigation/native-stack'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { ActivityIndicator, View } from 'react-native'
import { LoginScreen } from '@/screens/auth/LoginScreen'
import { RegistroScreen } from '@/screens/auth/RegistroScreen'
import { LobbyScreen } from '@/screens/shared/LobbyScreen'
import { PartidaScreen } from '@/screens/shared/PartidaScreen'
import { clearParticipanteSesionInscrita } from '@/lib/participanteSesionStorage'
import { useAuthStore } from '@/store/authStore'
import type { RootStackParamList } from '@/navigation/types'

const Stack = createNativeStackNavigator<RootStackParamList>()

export function AppNavigator() {
  const queryClient = useQueryClient()
  const hydrated = useAuthStore((s) => s.hydrated)
  const token = useAuthStore((s) => s.token)
  const rol = useAuthStore((s) => s.rol)
  const autenticado = Boolean(token) && rol === 'Participante'

  // Al cerrar sesión: limpia cache de React Query e inscripción local (evita “sesión fantasma”).
  useEffect(() => {
    if (autenticado) return
    queryClient.clear()
    void clearParticipanteSesionInscrita()
  }, [autenticado, queryClient])

  if (!hydrated) {
    return (
      <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
        <ActivityIndicator />
      </View>
    )
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {!autenticado ? (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Registro" component={RegistroScreen} />
          </>
        ) : (
          <>
            <Stack.Screen name="Lobby" component={LobbyScreen} />
            <Stack.Screen name="Partida" component={PartidaScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  )
}
