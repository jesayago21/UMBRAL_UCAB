import { useCallback, useEffect, useState } from 'react'

const AUTO_DISMISS_MS = 4000

export function useSuccessMessage() {
  const [message, setMessage] = useState<string | null>(null)

  const showSuccess = useCallback((text: string) => {
    setMessage(text)
  }, [])

  const clearSuccess = useCallback(() => setMessage(null), [])

  useEffect(() => {
    if (!message) return
    const timer = window.setTimeout(() => setMessage(null), AUTO_DISMISS_MS)
    return () => window.clearTimeout(timer)
  }, [message])

  return { successMessage: message, showSuccess, clearSuccess }
}
