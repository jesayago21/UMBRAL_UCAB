/** Resuelve IDs de categoría a nombres legibles para listados de etapas Trivia. */
export function formatCategoriaIds(
  ids: string[] | null | undefined,
  categorias: ReadonlyArray<{ id: string; nombre: string }> | undefined,
): string {
  if (!ids?.length) return '—'
  const byId = new Map((categorias ?? []).map((c) => [c.id, c.nombre]))
  return ids
    .map((id) => byId.get(id) ?? `Categoría desconocida (${id.slice(0, 8)}…)`)
    .join(', ')
}
