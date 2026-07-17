using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.ActualizarMision;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class ActualizarMisionCommandHandlerTests
{
    private readonly IMisionRepository _misionRepo = Substitute.For<IMisionRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly ActualizarMisionCommandHandler _sut;

    public ActualizarMisionCommandHandlerTests()
    {
        _sut = new ActualizarMisionCommandHandler(_misionRepo, _categoriaRepo, _preguntaRepo);
    }

    [Fact]
    public async Task Handle_ActivarConTriviaSinPreguntas_LanzaDomainExceptionRb09()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var mision = Mision.Crear("Misión a activar");
        mision.AgregarEtapaTrivia([categoria.CategoriaId]);

        _misionRepo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        _misionRepo.ExistsByNombreAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _misionRepo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);
        _categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);
        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Pregunta>());

        var act = () => _sut.Handle(
            new ActualizarMisionCommand(mision.MisionId.Valor, mision.Nombre, Activar: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*RB-09*");
        mision.Estado.Should().Be(EstadoMision.Borrador);
    }

    [Fact]
    public async Task Handle_ActivarConTriviaConPreguntas_Activa()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        var mision = Mision.Crear("Misión lista");
        mision.AgregarEtapaTrivia([categoria.CategoriaId]);

        _misionRepo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        _misionRepo.ExistsByNombreAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _misionRepo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);
        _categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);
        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns([pregunta]);

        var result = await _sut.Handle(
            new ActualizarMisionCommand(mision.MisionId.Valor, mision.Nombre, Activar: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mision.Estado.Should().Be(EstadoMision.Activa);
        await _misionRepo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }
}
