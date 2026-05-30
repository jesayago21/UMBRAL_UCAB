using Umbral.API.Extensions;
using Umbral.Application.DependencyInjection;
using Umbral.Infrastructure.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Umbral.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUmbralApi(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
    await db.Database.MigrateAsync();
}

app.UseUmbralExceptionHandling();

app.UseCors("FrontendDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    app.MapUmbralTestEndpoints();

app.MapControllers();

app.Run();

public partial class Program;
