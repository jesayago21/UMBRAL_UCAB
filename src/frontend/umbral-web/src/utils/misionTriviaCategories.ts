/** Categorías ya asignadas a otras etapas trivia de la misión (excluye la etapa indicada). */
export function categoriasTriviaUsadas(
  etapas: ReadonlyArray<{
    tipoEtapa: string
    categoriaIds?: readonly string[] | null
    etapaId?: string
  }>,
  exclude?: { etapaIndex?: number; etapaId?: string },
): Set<string> {
  const used = new Set<string>()
  etapas.forEach((etapa, index) => {
    if (etapa.tipoEtapa !== 'Trivia') return
    if (exclude?.etapaIndex !== undefined && index === exclude.etapaIndex) return
    if (exclude?.etapaId !== undefined && etapa.etapaId === exclude.etapaId) return
    for (const id of etapa.categoriaIds ?? []) used.add(id)
  })
  return used
}
