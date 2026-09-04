import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ErrorState } from '@/components/shared/ErrorState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { getApiErrorMessage } from '@/services/apiClient'
import { registrarParticipante } from '@/services/authService'
import { btnPrimary, btnSecondary, inputClass } from '@/styles/ui'

export function RegistroParticipantePage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
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
      setSuccess(
        `Cuenta creada (${created.username}). Ya puedes iniciar sesión con Keycloak.`,
      )
      window.setTimeout(() => navigate('/login', { replace: true }), 1500)
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setPending(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-lg rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="text-2xl font-bold text-indigo-700">Crear cuenta</h1>
        <p className="mt-2 text-sm text-slate-600">
          Registro de <strong>participante</strong>. Tras registrarte inicia sesión y únete a
          una sesión con el código del operador.
        </p>

        {success && (
          <div className="mt-4">
            <SuccessAlert message={success} onDismiss={() => setSuccess(null)} />
          </div>
        )}
        {error && (
          <div className="mt-4">
            <ErrorState message={error} />
          </div>
        )}

        <form onSubmit={(e) => void handleSubmit(e)} className="mt-6 space-y-3">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-700" htmlFor="reg-email">
              Email
            </label>
            <input
              id="reg-email"
              type="email"
              required
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={inputClass}
              disabled={pending}
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-700" htmlFor="reg-username">
              Usuario
            </label>
            <input
              id="reg-username"
              required
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className={inputClass}
              disabled={pending}
            />
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700" htmlFor="reg-nombre">
                Nombre
              </label>
              <input
                id="reg-nombre"
                required
                autoComplete="given-name"
                value={nombre}
                onChange={(e) => setNombre(e.target.value)}
                className={inputClass}
                disabled={pending}
              />
            </div>
            <div>
              <label
                className="mb-1 block text-xs font-medium text-slate-700"
                htmlFor="reg-apellido"
              >
                Apellido
              </label>
              <input
                id="reg-apellido"
                required
                autoComplete="family-name"
                value={apellido}
                onChange={(e) => setApellido(e.target.value)}
                className={inputClass}
                disabled={pending}
              />
            </div>
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-700" htmlFor="reg-password">
              Contraseña
            </label>
            <input
              id="reg-password"
              type="password"
              required
              minLength={8}
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={inputClass}
              disabled={pending}
            />
          </div>
          <div>
            <label
              className="mb-1 block text-xs font-medium text-slate-700"
              htmlFor="reg-confirm"
            >
              Confirmar contraseña
            </label>
            <input
              id="reg-confirm"
              type="password"
              required
              minLength={8}
              autoComplete="new-password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className={inputClass}
              disabled={pending}
            />
          </div>

          <button type="submit" disabled={pending} className={`mt-2 w-full ${btnPrimary}`}>
            {pending ? 'Creando cuenta…' : 'Registrarme como participante'}
          </button>
        </form>

        <div className="mt-4 text-center">
          <Link to="/login" className={`${btnSecondary} inline-block`}>
            Volver al inicio de sesión
          </Link>
        </div>
      </div>
    </div>
  )
}
