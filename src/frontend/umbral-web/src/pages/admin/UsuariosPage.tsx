import { useState } from 'react'
import { PageHeader } from '@/components/admin/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { crearUsuario, listUsuarios } from '@/services/usuarioService'
import { getApiErrorMessage } from '@/services/apiClient'
import { btnPrimary, cardClass, inputClass } from '@/styles/ui'
import { useQuery, useQueryClient } from '@tanstack/react-query'

export function UsuariosPage() {
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()
  const queryClient = useQueryClient()
  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['usuarios'],
    queryFn: () => listUsuarios(),
  })

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const form = new FormData(event.currentTarget)
    const roles = form.getAll('roles') as string[]
    if (roles.length === 0) {
      setFormError('Selecciona al menos un rol (Administrador u Operador). RB-35.')
      return
    }
    try {
      await crearUsuario({
        email: String(form.get('email')),
        username: String(form.get('username')),
        nombre: String(form.get('nombre')),
        apellido: String(form.get('apellido')),
        passwordTemporal: String(form.get('password')),
        roles,
      })
      showSuccess('Usuario registrado (Keycloak + tabla espejo).')
      event.currentTarget.reset()
      void queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Usuarios administrables"
        description="Registro federado con Keycloak. No asignar Participante desde aquí (RB-35)."
      />

      {successMessage && <SuccessAlert message={successMessage} onDismiss={clearSuccess} />}
      {formError && <ErrorState message={formError} />}

      <form onSubmit={handleSubmit} className={`${cardClass} grid gap-3 sm:grid-cols-2`}>
        <input name="email" type="email" required placeholder="Email" className={inputClass} />
        <input name="username" required placeholder="Username" className={inputClass} />
        <input name="nombre" required placeholder="Nombre" className={inputClass} />
        <input name="apellido" required placeholder="Apellido" className={inputClass} />
        <input
          name="password"
          type="password"
          required
          minLength={8}
          placeholder="Contraseña temporal"
          className={`${inputClass} sm:col-span-2`}
        />
        <fieldset className="sm:col-span-2">
          <legend className="text-sm font-medium text-slate-700">Roles</legend>
          <label className="mr-4 text-sm">
            <input type="checkbox" name="roles" value="Administrador" className="mr-1" />
            Administrador
          </label>
          <label className="text-sm">
            <input type="checkbox" name="roles" value="Operador" className="mr-1" />
            Operador
          </label>
        </fieldset>
        <button type="submit" className={`${btnPrimary} sm:col-span-2`}>
          Registrar usuario
        </button>
      </form>

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={getApiErrorMessage(error)} />}
      {data && (
        <ul className={`${cardClass} divide-y divide-slate-100`}>
          {data.map((u) => (
            <li key={u.id} className="py-2 text-sm">
              <span className="font-medium">{u.email}</span> — {u.roles.join(', ')} ({u.estado})
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
