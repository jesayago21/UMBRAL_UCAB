using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;

internal sealed class CrearPreguntaCommandHandler : IRequestHandler<CrearPreguntaCommand, Result<Guid>>
{
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public CrearPreguntaCommandHandler(
        IPreguntaRepository preguntaRepository,
        ICategoriaRepository categoriaRepository)
    {
        _preguntaRepository = preguntaRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(CrearPreguntaCommand command, CancellationToken cancellationToken)
    {
        CategoriaId? categoriaId = null;

        if (command.CategoriaId.HasValue)
        {
            categoriaId = new CategoriaId(command.CategoriaId.Value);
            var categoria = await _categoriaRepository.FindByIdAsync(categoriaId, cancellationToken)
                ?? throw new NotFoundException(nameof(Categoria), command.CategoriaId.Value);

            if (categoria.Eliminada)
                throw new NotFoundException(nameof(Categoria), command.CategoriaId.Value);
        }

        var dificultad = TriviaMappings.ParseDificultad(command.Dificultad);
        var opciones = TriviaMappings.ToOpciones(command.Opciones);

        var pregunta = Pregunta.Crear(
            command.Enunciado,
            dificultad,
            opciones,
            categoriaId);

        await _preguntaRepository.SaveAsync(pregunta, cancellationToken);

        return Result<Guid>.Ok(pregunta.PreguntaId.Valor);
    }
}
