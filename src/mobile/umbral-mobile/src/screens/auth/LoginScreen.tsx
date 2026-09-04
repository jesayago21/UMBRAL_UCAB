import { useState } from 'react'
import {
  ActivityIndicator,
  Pressable,
  Text,
  TextInput,
  View,
} from 'react-native'
import { SafeAreaView } from 'react-native-safe-area-context'
import type { NativeStackScreenProps } from '@react-navigation/native-stack'
import { useKeycloakAuth } from '@/hooks/useKeycloakAuth'
import { colors, styles } from '@/styles/ui'
import type { RootStackParamList } from '@/navigation/types'

type Props = NativeStackScreenProps<RootStackParamList, 'Login'>

export function LoginScreen({ navigation }: Props) {
  const { loginWithPassword, error, busy } = useKeycloakAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  return (
    <SafeAreaView style={styles.screen}>
      <View style={[styles.content, { flex: 1, justifyContent: 'center' }]}>
        <Text style={[styles.title, { color: colors.indigo }]}>UMBRAL</Text>
        <View style={styles.card}>
          {error ? <Text style={styles.error}>{error}</Text> : null}

          <TextInput
            style={[styles.input, { marginTop: error ? 12 : 0 }]}
            autoCapitalize="none"
            autoCorrect={false}
            placeholder="Usuario"
            value={username}
            onChangeText={setUsername}
            editable={!busy}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            autoCapitalize="none"
            secureTextEntry
            placeholder="Contraseña"
            value={password}
            onChangeText={setPassword}
            editable={!busy}
          />

          <Pressable
            style={[styles.btnPrimary, busy && { opacity: 0.6 }, { marginTop: 14 }]}
            disabled={busy || !username.trim() || !password}
            onPress={() => void loginWithPassword(username, password)}
          >
            {busy ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.btnPrimaryText}>Iniciar sesión</Text>
            )}
          </Pressable>

          <Pressable
            style={{ marginTop: 14, alignItems: 'center' }}
            disabled={busy}
            onPress={() => navigation.navigate('Registro')}
          >
            <Text style={styles.muted}>
              ¿No tienes cuenta?{' '}
              <Text style={{ color: colors.indigo, fontWeight: '600' }}>Crear cuenta</Text>
            </Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  )
}
