using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbral.Domain.Ports;
using Umbral.Infrastructure.Messaging.Consumers;
using Umbral.Infrastructure.Messaging.Publishers;

namespace Umbral.Infrastructure.Messaging.DependencyInjection;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddUmbralMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProcesarRespuestaTriviaConsumer>();

            if (environment.IsEnvironment("Testing"))
            {
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var host = configuration["RabbitMQ:Host"] ?? "localhost";
                    var user = configuration["RabbitMQ:Username"] ?? "umbral_user";
                    var pass = configuration["RabbitMQ:Password"] ?? "umbral_pass";
                    var vhost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                    cfg.Host(host, vhost, h =>
                    {
                        h.Username(user);
                        h.Password(pass);
                    });

                    cfg.UseMessageRetry(r =>
                        r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                    cfg.ReceiveEndpoint("umbral.respuestas-trivia", e =>
                    {
                        e.ConfigureConsumer<ProcesarRespuestaTriviaConsumer>(ctx);
                    });
                });
            }
        });

        services.AddScoped<IRespuestaTriviaBus, MassTransitRespuestaTriviaBus>();

        return services;
    }
}
