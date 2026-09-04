import { useCallback, useEffect, useRef, useState } from 'react'
import {
  Modal,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
  ActivityIndicator,
} from 'react-native'
import { CameraView, useCameraPermissions, type BarcodeScanningResult } from 'expo-camera'
import {
  GameFeedbackBanner,
  feedbackDesdeEvidencia,
  type GameFeedback,
} from '@/components/shared/GameFeedbackBanner'
import { colors, styles } from '@/styles/ui'

interface EvidenciaQrPanelProps {
  enviando: boolean
  onEnviar: (codigoQr: string) => Promise<{ resultado: string }>
}

/**
 * Envío de evidencia BT: código manual o escaneo QR con cámara.
 */
export function EvidenciaQrPanel({ enviando, onEnviar }: EvidenciaQrPanelProps) {
  const [codigoQr, setCodigoQr] = useState('')
  const [feedback, setFeedback] = useState<GameFeedback | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [scannerOpen, setScannerOpen] = useState(false)
  const [permission, requestPermission] = useCameraPermissions()
  const lockScan = useRef(false)

  useEffect(() => {
    if (!scannerOpen) lockScan.current = false
  }, [scannerOpen])

  const enviarCodigo = useCallback(
    async (raw: string) => {
      const qr = raw.trim()
      if (!qr) {
        setError('Ingresa o escanea el código QR.')
        setFeedback(null)
        return
      }
      setError(null)
      setFeedback(null)
      try {
        const result = await onEnviar(qr)
        const fb = feedbackDesdeEvidencia(result.resultado)
        setFeedback(fb)
        if (result.resultado === 'Valida') setCodigoQr('')
      } catch (err) {
        setFeedback(null)
        setError(err instanceof Error ? err.message : 'No se pudo enviar la evidencia.')
      }
    },
    [onEnviar],
  )

  const onBarcodeScanned = useCallback(
    (scan: BarcodeScanningResult) => {
      if (lockScan.current || enviando) return
      const data = scan.data?.trim()
      if (!data) return
      lockScan.current = true
      setScannerOpen(false)
      setCodigoQr(data)
      void enviarCodigo(data)
    },
    [enviando, enviarCodigo],
  )

  const abrirScanner = async () => {
    setError(null)
    setFeedback(null)
    if (!permission?.granted) {
      const res = await requestPermission()
      if (!res.granted) {
        setError('Necesitamos permiso de cámara para escanear el QR.')
        return
      }
    }
    lockScan.current = false
    setScannerOpen(true)
  }

  return (
    <View style={styles.card}>
      <Text style={styles.heading}>Encontrar el tesoro</Text>
      <Text style={styles.muted}>
        Escribe el código o apunta la cámara al QR del lugar.
      </Text>

      {feedback ? (
        <GameFeedbackBanner feedback={feedback} onDismiss={() => setFeedback(null)} />
      ) : null}
      {error ? <Text style={styles.error}>{error}</Text> : null}

      <TextInput
        style={styles.input}
        placeholder="Código QR (manual)"
        value={codigoQr}
        onChangeText={setCodigoQr}
        autoCapitalize="none"
        autoCorrect={false}
        editable={!enviando}
      />

      <Pressable
        style={[styles.btnPrimary, enviando && { opacity: 0.6 }]}
        disabled={enviando}
        onPress={() => void enviarCodigo(codigoQr)}
      >
        {enviando ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.btnPrimaryText}>Enviar código</Text>
        )}
      </Pressable>

      <Pressable
        style={[styles.btnSecondary, enviando && { opacity: 0.6 }]}
        disabled={enviando}
        onPress={() => void abrirScanner()}
      >
        <Text style={styles.btnSecondaryText}>Escanear con cámara</Text>
      </Pressable>

      <Modal visible={scannerOpen} animationType="slide" onRequestClose={() => setScannerOpen(false)}>
        <View style={local.scannerRoot}>
          <View style={local.scannerHeader}>
            <Text style={local.scannerTitle}>Apunta al código QR</Text>
            <Pressable onPress={() => setScannerOpen(false)}>
              <Text style={local.closeBtn}>Cerrar</Text>
            </Pressable>
          </View>
          {permission?.granted ? (
            <CameraView
              style={StyleSheet.absoluteFill}
              facing="back"
              barcodeScannerSettings={{ barcodeTypes: ['qr'] }}
              onBarcodeScanned={onBarcodeScanned}
            />
          ) : (
            <View style={local.permissionBox}>
              <Text style={styles.body}>Sin permiso de cámara.</Text>
              <Pressable style={styles.btnPrimary} onPress={() => void requestPermission()}>
                <Text style={styles.btnPrimaryText}>Conceder permiso</Text>
              </Pressable>
            </View>
          )}
          <View style={local.scannerFooter}>
            <Text style={local.hint}>El envío es automático al detectar el QR.</Text>
          </View>
        </View>
      </Modal>
    </View>
  )
}

const local = StyleSheet.create({
  scannerRoot: {
    flex: 1,
    backgroundColor: '#000',
  },
  scannerHeader: {
    zIndex: 2,
    paddingTop: 52,
    paddingHorizontal: 16,
    paddingBottom: 12,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  scannerTitle: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  closeBtn: {
    color: '#c7d2fe',
    fontWeight: '600',
    fontSize: 15,
  },
  scannerFooter: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: 20,
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  hint: {
    color: '#e2e8f0',
    textAlign: 'center',
    fontSize: 13,
  },
  permissionBox: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
    padding: 24,
    backgroundColor: colors.slate50,
  },
})
