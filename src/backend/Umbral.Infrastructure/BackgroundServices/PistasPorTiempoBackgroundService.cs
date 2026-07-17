using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbral.Application.Sesion.Commands.LiberarPistasPorTiempoVencidas;

namespace Umbral.Infrastructure.BackgroundServices;

/// <summary>
/// HU-09 — barra cada ~5s las pistas PorTiempo vencidas en sesiones activas.
/// </summary>
public sealed class PistasPorTiempoBackgroundService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PistasPorTiempoBackgroundService> _logger;

    public PistasPorTiempoBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<PistasPorTiempoBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PistasPorTiempoBackgroundService iniciado (intervalo={Intervalo}s).", Intervalo.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(
                    new LiberarPistasPorTiempoVencidasCommand(),
                    stoppingToken);

                if (result.IsSuccess && result.Value > 0)
                {
                    _logger.LogInformation(
                        "Liberación PorTiempo: {Count} pista(s) entregada(s).",
                        result.Value);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en barrido de pistas PorTiempo.");
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
