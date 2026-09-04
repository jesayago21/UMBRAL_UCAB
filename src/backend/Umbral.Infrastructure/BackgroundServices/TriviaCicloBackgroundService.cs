using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbral.Application.Sesion.Commands.ProcesarCicloTriviaAutomatico;

namespace Umbral.Infrastructure.BackgroundServices;

/// <summary>
/// HU-33 / HU-38 — cierra preguntas al vencer el timer y lanza la siguiente tras transición.
/// </summary>
public sealed class TriviaCicloBackgroundService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TriviaCicloBackgroundService> _logger;

    public TriviaCicloBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TriviaCicloBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TriviaCicloBackgroundService iniciado (intervalo={Intervalo}s).",
            Intervalo.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(
                    new ProcesarCicloTriviaAutomaticoCommand(),
                    stoppingToken);

                if (result.IsSuccess && result.Value > 0)
                {
                    _logger.LogInformation(
                        "Ciclo trivia: {Count} acción(es) aplicada(s).",
                        result.Value);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en barrido de ciclo trivia.");
            }

            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
