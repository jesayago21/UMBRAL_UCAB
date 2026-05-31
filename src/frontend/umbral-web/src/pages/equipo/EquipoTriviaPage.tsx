import { PageHeader } from '@/components/admin/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { LoadingState } from '@/components/shared/LoadingState'
import { useSesionesDisponiblesTrivia } from '@/hooks/useSesiones'
import { getApiErrorMessage } from '@/services/apiClient'

export function EquipoTriviaPage() {
  const { data, isLoading, isError, error, refetch } = useSesionesDisponiblesTrivia()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Sesiones de trivia"
        description="Mismo flujo: el operador crea la sesión y comparte un código único por sesión."
      />

      {isLoading && <LoadingState label="Cargando sesiones…" />}
      {isError && (
        <ErrorState message={getApiErrorMessage(error)} onRetry={() => void refetch()} />
      )}

      {data && data.length === 0 && !isLoading && (
        <EmptyState
          title="Sin sesiones trivia aún"
          description="Las sesiones trivia se habilitarán cuando el operador pueda crearlas desde el panel."
        />
      )}
    </div>
  )
}
