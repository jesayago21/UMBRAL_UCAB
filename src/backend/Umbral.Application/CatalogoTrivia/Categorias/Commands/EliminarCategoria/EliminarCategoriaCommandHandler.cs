using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;

internal sealed class EliminarCategoriaCommandHandler
    : IRequestHandler<EliminarCategoriaCommand, Result<Guid>>
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IPreguntaRepository _preguntaRepository;

    public EliminarCategoriaCommandHandler(
        ICategoriaRepository categoriaRepository,
        IPreguntaRepository preguntaRepository)
    {
        _categoriaRepository = categoriaRepository;
        _preguntaRepository = preguntaRepository;
    }

    public async Task<Result<Guid>> Handle(
        EliminarCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var categoriaId = new CategoriaId(command.CategoriaId);
        var categoria = await _categoriaRepository.FindByIdAsync(categoriaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), command.CategoriaId);

        if (categoria.Eliminada)
            throw new NotFoundException(nameof(Categoria), command.CategoriaId);

        var preguntas = await _preguntaRepository.FindByCategoriaAsync(categoriaId, cancellationToken);
        foreach (var pregunta in preguntas.Where(p => !p.Eliminada))
        {
            pregunta.QuitarCategoria();
            await _preguntaRepository.SaveAsync(pregunta, cancellationToken);
        }

        categoria.Eliminar();
        await _categoriaRepository.SaveAsync(categoria, cancellationToken);

        return Result<Guid>.Ok(categoria.CategoriaId.Valor);
    }
}
