import { useEffect, useState } from 'react'
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
  selectClass,
} from '@/styles/ui'
import {
  ROLES_USUARIO,
  type CrearUsuarioResponse,
  type RolUsuarioAdmin,
  type UsuarioDto,
} from '@/types/usuario.types'

function puedeEliminar(usuario: UsuarioDto): boolean {
  return !usuario.roles.some((r) => r.toLowerCase() === 'administrador')
}

function rolUnicoDesdeUsuario(usuario: UsuarioDto): RolUsuarioAdmin | '' {
  if (usuario.roles.length === 0) return ''
  const primero = usuario.roles[0] as RolUsuarioAdmin
  if (ROLES_USUARIO.includes(primero)) return primero
  return ''
}

function RolSelect({
  id,
  value,
  onChange,
  disabled,
}: {
  id: string
  value: RolUsuarioAdmin | ''
  onChange: (rol: RolUsuarioAdmin) => void
  disabled?: boolean
}) {
  return (
    <div className="flex flex-col gap-2 sm:col-span-2">
      <label htmlFor={id} className="text-sm font-medium text-slate-700">
        Rol
      </label>
      <select
        id={id}
        required
        value={value}
        disabled={disabled}
        onChange={(e) => onChange(e.target.value as RolUsuarioAdmin)}
        className={selectClass}
      >
        <option value="" disabled>
          Selecciona un rol…
        </option>
        {ROLES_USUARIO.map((rol) => (
          <option key={rol} value={rol}>
            {rol}
          </option>
        ))}
      </select>
      <p className="text-xs text-slate-500">
        Un solo rol por usuario (Administrador u Operador).
      </p>
    </div>
  )
}

export function UsuariosPage() {
  const [editTarget, setEditTarget] = useState<UsuarioDto | null>(null)
  const [rolCrear, setRolCrear] = useState<RolUsuarioAdmin | ''>('')
  const [rolEditar, setRolEditar] = useState<RolUsuarioAdmin | ''>('')
  const [formError, setFormError] = useState<string | null>(null)
  const { successMessage, showSuccess, clearSuccess } = useSuccessMessage()

  const { data, isLoading, isError, error } = useUsuarios()
  const crear = useCrearUsuario()
  const actualizar = useActualizarUsuario()
  const cambiarEstado = useCambiarEstadoUsuario()
  const eliminar = useEliminarUsuario()

  const isSaving =
    crear.isPending || actualizar.isPending || cambiarEstado.isPending || eliminar.isPending

  useEffect(() => {
    if (!editTarget) {
      setRolEditar('')
      return
    }
    setRolEditar(rolUnicoDesdeUsuario(editTarget))
  }, [editTarget])

  const handleCreate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const formEl = event.currentTarget
    const form = new FormData(formEl)
    if (!rolCrear) {
      setFormError('Selecciona exactamente un rol.')
      return
    }
    const username = String(form.get('username')).trim()
    const password = String(form.get('password'))
    let created: CrearUsuarioResponse | undefined
    try {
      created = await crear.mutateAsync({
        email: String(form.get('email')),
        username,
        nombre: String(form.get('nombre')),
        apellido: String(form.get('apellido')),
        passwordTemporal: password,
        roles: [rolCrear],
      })
      formEl.reset()
      setRolCrear('')
      showSuccess(
        `Usuario registrado. Entregar a «${username}» su contraseña: ${created.passwordTemporal}`,
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
    if (!rolEditar) {
      setFormError('Selecciona exactamente un rol.')
      return
    }
    const nuevaPassword = String(form.get('nuevaPassword') ?? '').trim()
    try {
      await actualizar.mutateAsync({
        keycloakUserId: editTarget.keycloakUserId,
        body: {
          nombre: String(form.get('nombre')),
          apellido: String(form.get('apellido')),
          rol: rolEditar,
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
      await cambiarEstado.mutateAsync({ keycloakUserId: usuario.keycloakUserId, accion })
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
        `¿Eliminar a «${usuario.username}»? Se borrará de Keycloak.`,
      )
    ) {
      return
    }
    setFormError(null)
    try {
      await eliminar.mutateAsync(usuario.keycloakUserId)
      showSuccess('Usuario eliminado.')
    } catch (err) {
      setFormError(getApiErrorMessage(err))
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Usuarios"
        description="Un usuario, un rol. Los usuarios se gestionan directamente en Keycloak."
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
          <RolSelect
            id="rol-crear"
            value={rolCrear}
            onChange={setRolCrear}
            disabled={isSaving}
          />
          <button type="submit" disabled={isSaving || !rolCrear} className={`${btnPrimary} sm:col-span-2`}>
            Crear usuario
          </button>
        </form>
      )}

      {editTarget && (
        <form
          key={editTarget.keycloakUserId}
          onSubmit={handleUpdate}
          className={`${cardHighlightClass} grid gap-3 sm:grid-cols-2`}
        >
          <p className="sm:col-span-2 text-sm font-medium text-indigo-800">
            Editando: {editTarget.username} ({editTarget.email})
            {editTarget.roles.length > 1 && (
              <span className="mt-1 block text-xs font-normal text-amber-700">
                Este usuario tenía varios roles en Keycloak; al guardar quedará solo el rol del desplegable.
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
          <RolSelect
            id="rol-editar"
            value={rolEditar}
            onChange={setRolEditar}
            disabled={isSaving}
          />
          <div className="flex gap-2 sm:col-span-2">
            <button type="submit" disabled={isSaving || !rolEditar} className={btnPrimary}>
              Guardar cambios
            </button>
            <button
              type="button"
              className={btnSecondary}
              onClick={() => setEditTarget(null)}
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={getApiErrorMessage(error)} />}
      {data && data.length === 0 && !isLoading && (
        <EmptyState title="Sin usuarios" description="No hay usuarios registrados." />
      )}
      {data && data.length > 0 && (
        <ul className={`${cardClass} divide-y divide-slate-100`}>
          {data.map((u) => (
            <li key={u.keycloakUserId} className="flex flex-wrap items-center justify-between gap-2 py-3 text-sm">
              <div>
                <p className="font-medium text-slate-900">
                  {u.nombre} {u.apellido}{' '}
                  <span className="font-normal text-slate-500">(@{u.username})</span>
                </p>
                <p className="text-slate-600">{u.email}</p>
                <p className="text-xs text-slate-500">
                  {u.roles.join(', ')} · {u.estado}
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
