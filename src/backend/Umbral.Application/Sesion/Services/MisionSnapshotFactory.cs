using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Application.Sesion.Services;

internal static class MisionSnapshotFactory
{
    public static async Task<MisionSnapshot> CrearAsync(
        Mision mision,
        IPreguntaRepository preguntaRepository,
        ICategoriaRepository categoriaRepository,
        CancellationToken ct)
    {
        var triviaResueltas = new Dictionary<EtapaId, EtapaTriviaSnapshot>();

        foreach (var etapa in mision.Etapas.OfType<EtapaTrivia>())
        {
            var preguntasOrdenadas = new List<PreguntaId>();
            var nombres            = new List<string>();
            var vistos             = new HashSet<Guid>();

            foreach (var categoriaId in etapa.CategoriaIds)
            {
                var categoria = await categoriaRepository.FindByIdAsync(categoriaId, ct)
                                ?? throw new NotFoundException(nameof(Categoria), categoriaId.Valor);

                nombres.Add(categoria.Nombre);

                var preguntas = await preguntaRepository.FindByCategoriaAsync(categoriaId, ct);
                foreach (var pregunta in preguntas
                             .Where(p => !p.Eliminada && p.CategoriaId is not null)
                             .OrderBy(p => p.Enunciado, StringComparer.OrdinalIgnoreCase))
                {
                    if (vistos.Add(pregunta.PreguntaId.Valor))
                        preguntasOrdenadas.Add(pregunta.PreguntaId);
                }
            }

            if (preguntasOrdenadas.Count == 0)
                throw new DomainException(
                    $"La etapa trivia en orden {etapa.Orden} no tiene preguntas activas en sus categorías.");

            triviaResueltas[etapa.EtapaId] = EtapaTriviaSnapshot.DesdeEtapa(
                etapa,
                preguntasOrdenadas,
                string.Join(", ", nombres));
        }

        return MisionSnapshot.Desde(mision, triviaResueltas);
    }
}
