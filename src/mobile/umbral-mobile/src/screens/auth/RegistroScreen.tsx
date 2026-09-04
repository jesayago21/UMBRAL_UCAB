import { useState } from 'react'
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native'
import { SafeAreaView } from 'react-native-safe-area-context'
import type { NativeStackScreenProps } from '@react-navigation/native-stack'
import { getApiErrorMessage } from '@/services/apiClient'
import { registrarParticipante } from '@/services/authService'
import { colors, styles } from '@/styles/ui'
import type { RootStackParamList } from '@/navigation/types'

type Props = NativeStackScreenProps<RootStackParamList, 'Registro'>

export function RegistroScreen({ navigation }: Props) {
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const handleSubmit = async () => {
    setError(null)
    setSuccess(null)
    if (password !== confirmPassword) {
      setError('Las contraseñas no coinciden.')
      return
    }
    if (password.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.')
      return
    }

    setPending(true)
    try {
      const created = await registrarParticipante({
        email: email.trim(),
        username: username.trim(),
        nombre: nombre.trim(),
        apellido: apellido.trim(),
        password,
      })
      setSuccess(`Cuenta creada (${created.username}). Ya puedes iniciar sesión.`)
      setTimeout(() => navigation.replace('Login'), 1200)
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setPending(false)
    }
  }

  return (
    <SafeAreaView style={styles.screen}>
      <ScrollView contentContainerStyle={[styles.content, { paddingBottom: 32 }]}>
        <Text style={[styles.title, { color: colors.indigo }]}>Crear cuenta</Text>

        <View style={styles.card}>
          {success ? <Text style={styles.success}>{success}</Text> : null}
          {error ? <Text style={styles.error}>{error}</Text> : null}

          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            autoCapitalize="none"
            autoCorrect={false}
            keyboardType="email-address"
            placeholder="Email"
            value={email}
            onChangeText={setEmail}
            editable={!pending}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            autoCapitalize="none"
            autoCorrect={false}
            placeholder="Usuario"
            value={username}
            onChangeText={setUsername}
            editable={!pending}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            placeholder="Nombre"
            value={nombre}
            onChangeText={setNombre}
            editable={!pending}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            placeholder="Apellido"
            value={apellido}
            onChangeText={setApellido}
            editable={!pending}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            secureTextEntry
            placeholder="Contraseña (mín. 8)"
            value={password}
            onChangeText={setPassword}
            editable={!pending}
          />
          <TextInput
            style={[styles.input, { marginTop: 8 }]}
            secureTextEntry
            placeholder="Confirmar contraseña"
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            editable={!pending}
          />

          <Pressable
            style={[styles.btnPrimary, pending && { opacity: 0.6 }, { marginTop: 14 }]}
            disabled={pending}
            onPress={() => void handleSubmit()}
          >
            {pending ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.btnPrimaryText}>Registrarme como participante</Text>
            )}
          </Pressable>

          <Pressable
            style={[styles.btnSecondary, { marginTop: 10 }]}
            disabled={pending}
            onPress={() => navigation.goBack()}
          >
            <Text style={styles.btnSecondaryText}>Volver al inicio de sesión</Text>
          </Pressable>
        </View>
      </ScrollView>
    </SafeAreaView>
  )
}
