import { useState } from 'react'
import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { SuccessAlert } from '@/components/shared/SuccessAlert'
import {
  useActualizarUsuario,
  useCambiarEstadoUsuario,
  useCrearUsuario,
  useEliminarUsuario,
  useUsuarios,
} from '@/hooks/useUsuarios'
import { useSuccessMessage } from '@/hooks/useSuccessMessage'
import { getApiErrorMessage } from '@/services/apiClient'
import {
  btnDangerLink,
  btnLink,
  btnPrimary,
  btnSecondary,
  cardClass,
  cardHighlightClass,
  inputClass,
} from '@/styles/ui'
import { ROLES_USUARIO, type RolUsuarioAdmin, type UsuarioDto } from '@/types/usuario.types'

function puedeEliminar(usuario: UsuarioDto): boolean {
  return !usuario.roles.some((r) => r.toLowerCase() === 'administrador')
}

function rolFromForm(form: FormData): RolUsuarioAdmin | null {
  const value = String(form.get('rol') ?? '').trim()
  if (!value) return null
  return value as RolUsuarioAdmin
}

function RolesFieldset({
  defaultRol,
  name = 'rol',
}: {
  defaultRol?: string
  name?: string
}) {
  return (
    <fieldset className="sm:col-span-2">
      <legend className="text-sm font-medium text-slate-700">Rol (uno solo)</legend>
      <div className="mt-1 flex flex-wrap gap-4">
        {ROLES_USUARIO.map((rol) => (
          <label key={rol} className="text-sm">
            <input
              type="radio"
              name={name}
              value={rol}
              required
              defaultChecked={defaultRol === rol}
              className="mr-1"
            />
            {rol}
          </label>
        ))}
      </div>
    </fieldset>
  )
}

export function UsuariosPage() {
  const [editTarget, setEditTarget] = useState<UsuarioDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const { data, isLoading, isError, error } = useUsuarios()
  const crear = useCrearUsuario()
  const actualizar = useActualizarUsuario()
  const cambiarEstado = useCambiarEstadoUsuario()
  const eliminar = useEliminarUsuario()

  const isSaving =
    crear.isPending || actualizar.isPending || cambiarEstado.isPending || eliminar.isPending

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const formEl = event.currentTarget
    const form = new FormData(formEl)
    const rol = rolFromForm(form)
    if (!rol) {
      setFormError('Selecciona un rol.')
      return
    }
    const username = String(form.get('username')).trim()
    try {
      await crear.mutateAsync({
        email: String(form.get('email')),
        username,
        nombre: String(form.get('nombre')),
        apellido: String(form.get('apellido')),
        passwordTemporal: String(form.get('password')),
        roles: [rol],
      })
      formEl.reset()
      showSuccess(
        `Usuario registrado. Iniciar sesión en Keycloak con username «${username}» y la contraseña indicada.`,
      )
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleUpdate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!editTarget) return
    setFormError(null)
    const form = new FormData(event.currentTarget)
    const rol = rolFromForm(form)
    if (!rol) {
      setFormError('Selecciona un rol.')
      return
    }
    const nuevaPassword = String(form.get('nuevaPassword') ?? '').trim()
    try {
      await actualizar.mutateAsync({
        id: editTarget.id,
        body: {
          nombre: String(form.get('nombre')),
          apellido: String(form.get('apellido')),
          rol,
          nuevaPassword: nuevaPassword || null,
        },
      })
      setEditTarget(null)
      showSuccess('Usuario actualizado.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleToggleEstado = async (usuario: UsuarioDto) => {
    setFormError(null)
    const accion = usuario.estado === 'Activo' ? 'Bloquear' : 'Activar'
    try {
      await cambiarEstado.mutateAsync({ id: usuario.id, accion })
      showSuccess(accion === 'Bloquear' ? 'Usuario bloqueado.' : 'Usuario activado.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const handleDelete = async (usuario: UsuarioDto) => {
    if (!puedeEliminar(usuario)) {
      setFormError('No se puede eliminar un usuario con rol Administrador.')
      return
    }
    if (
      !window.confirm(
        `¿Eliminar a «${usuario.username}»? Se borrará de Keycloak y del registro local.`,
      )
    ) {
      return
    }
    setFormError(null)
    try {
      await eliminar.mutateAsync(usuario.id)
      showSuccess('Usuario eliminado.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  const rolActualEdicion =
    editTarget?.roles.length === 1
      ? editTarget.roles[0]
      : editTarget?.roles[0]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Usuarios"
        description="Un usuario, un rol. La contraseña mostrada es la última fijada por el administrador (para entregar a operadores y participantes). Usuarios demo: Umbral123!"
      />

      {successMessage && <SuccessAlert message={successMessage} onDismiss={clearSuccess} />}
      {formError && <ErrorState message={formError} />}

      {!editTarget && (
        <form onSubmit={handleCreate} className={`${cardClass} grid gap-3 sm:grid-cols-2`}>
          <input name="email" type="email" required placeholder="Email" className={inputClass} />
          <input name="username" required placeholder="Username (login)" className={inputClass} />
          <input name="nombre" required placeholder="Nombre" className={inputClass} />
          <input name="apellido" required placeholder="Apellido" className={inputClass} />
          <input
            name="password"
            type="password"
            required
            minLength={8}
            placeholder="Contraseña"
            className={`${inputClass} sm:col-span-2`}
          />
          <RolesFieldset />
          <button type="submit" disabled={isSaving} className={`${btnPrimary} sm:col-span-2`}>
            Crear usuario
          </button>
        </form>
      )}

      {editTarget && (
        <form
          key={editTarget.id}
          onSubmit={handleUpdate}
          className={`${cardHighlightClass} grid gap-3 sm:grid-cols-2`}
        >
          <p className="sm:col-span-2 text-sm font-medium text-indigo-800">
            Editando: {editTarget.username} ({editTarget.email})
            {editTarget.passwordAsignada && (
              <span className="mt-1 block text-xs font-normal text-slate-600">
                Contraseña actual:{' '}
                <code className="rounded bg-slate-100 px-1 font-mono">{editTarget.passwordAsignada}</code>
              </span>
            )}
            {editTarget.roles.length > 1 && (
              <span className="mt-1 block text-xs font-normal text-amber-700">
                Este usuario tiene varios roles guardados; al guardar quedará solo el rol seleccionado.
              </span>
            )}
          </p>
          <input
            name="nombre"
            required
            defaultValue={editTarget.nombre}
            placeholder="Nombre"
            className={inputClass}
          />
          <input
            name="apellido"
            required
            defaultValue={editTarget.apellido}
            placeholder="Apellido"
            className={inputClass}
          />
          <input
            name="nuevaPassword"
            type="password"
            minLength={8}
            placeholder="Nueva contraseña (opcional)"
            className={`${inputClass} sm:col-span-2`}
          />
          <RolesFieldset defaultRol={rolActualEdicion} />
          <div className="flex gap-2 sm:col-span-2">
            <button type="submit" disabled={isSaving} className={btnPrimary}>
              Guardar cambios
            </button>
            <button type="button" className={btnSecondary} onClick={() => setEditTarget(null)}>
              Cancelar
            </button>
          </div>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={getApiErrorMessage(error)} />}
      {data && data.length === 0 && !isLoading && (
        <EmptyState message="No hay usuarios registrados." />
      )}
      {data && data.length > 0 && (
        <ul className={`${cardClass} divide-y divide-slate-100`}>
          {data.map((u) => (
            <li key={u.id} className="flex flex-wrap items-center justify-between gap-2 py-3 text-sm">
              <div>
                <p className="font-medium text-slate-900">
                  {u.nombre} {u.apellido}{' '}
                  <span className="font-normal text-slate-500">(@{u.username})</span>
                </p>
                <p className="text-slate-600">{u.email}</p>
                <p className="text-xs text-slate-500">
                  {u.roles.join(', ')} · {u.estado}
                </p>
                <p className="mt-1 text-xs text-slate-600">
                  Contraseña para entregar:{' '}
                  {u.passwordAsignada ? (
                    <code className="rounded bg-slate-100 px-1 py-0.5 font-mono text-slate-800">
                      {u.passwordAsignada}
                    </code>
                  ) : (
                    <span className="italic text-slate-400">
                      no registrada (asigne una nueva al editar)
                    </span>
                  )}
                </p>
              </div>
              <div className="flex flex-wrap gap-2">
                <button type="button" className={btnLink} onClick={() => setEditTarget(u)}>
                  Editar
                </button>
                <button
                  type="button"
                  className={btnLink}
                  disabled={isSaving}
                  onClick={() => void handleToggleEstado(u)}
                >
                  {u.estado === 'Activo' ? 'Bloquear' : 'Activar'}
                </button>
                {puedeEliminar(u) ? (
                  <button
                    type="button"
                    className={btnDangerLink}
                    disabled={isSaving}
                    onClick={() => void handleDelete(u)}
                  >
                    Eliminar
                  </button>
                ) : (
                  <span className="text-xs text-slate-400" title="No se eliminan administradores">
                    No eliminable
                  </span>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
