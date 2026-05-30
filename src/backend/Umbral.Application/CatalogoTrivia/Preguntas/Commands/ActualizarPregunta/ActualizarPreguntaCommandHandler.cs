using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.ActualizarPregunta;

internal sealed class ActualizarPreguntaCommandHandler
    : IRequestHandler<ActualizarPreguntaCommand, Result<Guid>>
{
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ActualizarPreguntaCommandHandler(
        IPreguntaRepository preguntaRepository,
        ICategoriaRepository categoriaRepository)
    {
        _preguntaRepository = preguntaRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(
        ActualizarPreguntaCommand command,
        CancellationToken cancellationToken)
    {
        var pregunta = await _preguntaRepository.FindByIdAsync(
            new PreguntaId(command.PreguntaId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Pregunta), command.PreguntaId);

        if (pregunta.Eliminada)
            throw new NotFoundException(nameof(Pregunta), command.PreguntaId);

        var dificultad = TriviaMappings.ParseDificultad(command.Dificultad);
        var opciones = TriviaMappings.ToOpciones(command.Opciones);

        pregunta.ModificarContenido(command.Enunciado, dificultad, opciones);
        await AplicarCategoriaAsync(pregunta, command.CategoriaId, cancellationToken);

        await _preguntaRepository.SaveAsync(pregunta, cancellationToken);

        return Result<Guid>.Ok(pregunta.PreguntaId.Valor);
    }

    private async Task AplicarCategoriaAsync(
        Pregunta pregunta,
        Guid? categoriaId,
        CancellationToken cancellationToken)
    {
        if (!categoriaId.HasValue)
        {
            pregunta.QuitarCategoria();
            return;
        }

        var id = new CategoriaId(categoriaId.Value);
        var categoria = await _categoriaRepository.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), categoriaId.Value);

        if (categoria.Eliminada)
            throw new NotFoundException(nameof(Categoria), categoriaId.Value);

        pregunta.AsignarCategoria(id);
    }
}
