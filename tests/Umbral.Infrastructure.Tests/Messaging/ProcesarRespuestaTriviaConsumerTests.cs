using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;
using Umbral.Infrastructure.Messaging.Consumers;
using Umbral.Infrastructure.Messaging.Contracts;
using Xunit;

namespace Umbral.Infrastructure.Tests.Messaging;

public sealed class ProcesarRespuestaTriviaConsumerTests
{
    [Fact]
    public async Task Consume_CuandoMensajeValido_EnviaCommandAMediator()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<ProcesarRespuestaTriviaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProcesarRespuestaTriviaResult>.Ok(
                new ProcesarRespuestaTriviaResult(Guid.NewGuid(), true, false, 100, 100)));

        await using var provider = new ServiceCollection()
            .AddSingleton(mediator)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProcesarRespuestaTriviaConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var messageId = Guid.NewGuid();
        await harness.Bus.Publish(new RespuestaTriviaRecibidaIntegrationEvent
        {
            MessageId             = messageId,
            SesionId              = Guid.NewGuid(),
            JugadorId             = Guid.NewGuid(),
            PreguntaId            = Guid.NewGuid(),
            IndiceOpcion          = 0,
            DuracionTimerSegundos = 30,
            RecibidoEnUtc         = DateTimeOffset.UtcNow
        });

        (await harness.Consumed.Any<RespuestaTriviaRecibidaIntegrationEvent>()).Should().BeTrue();

        await mediator.Received(1).Send(
            Arg.Is<ProcesarRespuestaTriviaCommand>(c => c.IndiceOpcion == 0),
            Arg.Any<CancellationToken>());

        await harness.Stop();
    }

    [Fact]
    public async Task Consume_CuandoMensajeMalformado_NoLlamaMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var consumer = new ProcesarRespuestaTriviaConsumer(
            mediator,
            NullLogger<ProcesarRespuestaTriviaConsumer>.Instance);

        var context = Substitute.For<ConsumeContext<RespuestaTriviaRecibidaIntegrationEvent>>();
        context.Message.Returns(new RespuestaTriviaRecibidaIntegrationEvent
        {
            MessageId             = Guid.Empty,
            SesionId              = Guid.NewGuid(),
            JugadorId             = Guid.NewGuid(),
            PreguntaId            = Guid.NewGuid(),
            IndiceOpcion          = 0,
            DuracionTimerSegundos = 30,
            RecibidoEnUtc         = DateTimeOffset.UtcNow
        });
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await mediator.DidNotReceive().Send(
            Arg.Any<ProcesarRespuestaTriviaCommand>(),
            Arg.Any<CancellationToken>());
    }
}
