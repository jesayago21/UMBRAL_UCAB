import { Link } from 'react-router-dom'
import { EmptyState } from '@/components/shared/EmptyState'
import { LoadingState } from '@/components/shared/LoadingState'
import { mapEstadoSesionFromApi } from '@/lib/sesionUi'
import { cardClass } from '@/styles/ui'
import type { SesionResumenDto } from '@/types/sesion.types'

const ESTADO_LABEL: Record<string, string> = {
  programada: 'Programada',
  enPreparacion: 'En preparación',
  activa: 'Activa',
  pausada: 'Pausada',
}

const ESTADO_CLASS: Record<string, string> = {
  programada: 'bg-slate-100 text-slate-700',
  enPreparacion: 'bg-blue-100 text-blue-800',
  activa: 'bg-green-100 text-green-800',
  pausada: 'bg-amber-100 text-amber-900',
}

interface SesionesOperativasListProps {
  sesiones: SesionResumenDto[] | undefined
  isLoading: boolean
}

export function SesionesOperativasList({ sesiones, isLoading }: SesionesOperativasListProps) {
  if (isLoading) {
    return <LoadingState label="Cargando sesiones en curso…" />
  }

  if (!sesiones?.length) {
    return (
      <EmptyState
        title="No hay sesiones en curso"
        description="Crea una sesión nueva abajo. Puedes tener varias activas o en preparación a la vez."
      />
    )
  }

  return (
    <section className={`${cardClass} space-y-4`}>
      <div>
        <h3 className="font-medium text-slate-900">Sesiones en curso</h3>
        <p className="mt-1 text-sm text-slate-600">
          Puedes gestionar varias sesiones en paralelo. Abre inscripción y comparte el código de sesión.
        </p>
      </div>

      <ul className="divide-y divide-slate-100 rounded-lg border border-slate-200">
        {sesiones.map((s) => {
          const estadoUi = mapEstadoSesionFromApi(s.estado)
          return (
            <li key={s.id}>
              <Link
                to={`/operador/sesiones/${s.id}`}
                className="flex flex-wrap items-center justify-between gap-3 px-4 py-3 transition hover:bg-slate-50"
              >
                <div className="min-w-0">
                  <p className="truncate font-medium text-slate-900">{s.misionNombre}</p>
                  <p className="text-xs text-slate-500">
                    <span className="font-medium text-slate-600">
                      {s.tipoSesion === 'Trivia' ? 'Trivia' : 'Búsqueda'}
                    </span>
                    {' · '}
                    {s.equiposCount} equipo{s.equiposCount === 1 ? '' : 's'}
                    {s.totalEtapas > 0 && (
                      <>
                        {' · '}
                        {s.tipoSesion === 'Trivia' ? 'Pregunta' : 'Etapa'}{' '}
                        {s.etapaActualOrden || 1}/{s.totalEtapas}
                      </>
                    )}
                  </p>
                </div>
                <span
                  className={`shrink-0 rounded-full px-2.5 py-0.5 text-xs font-semibold ${ESTADO_CLASS[estadoUi] ?? ESTADO_CLASS.programada}`}
                >
                  {ESTADO_LABEL[estadoUi] ?? s.estado}
                </span>
              </Link>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
