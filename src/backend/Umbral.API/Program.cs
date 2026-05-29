using Umbral.API.Extensions;
using Umbral.Application.DependencyInjection;
using Umbral.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUmbralApi(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseUmbralExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    app.MapUmbralTestEndpoints();

app.MapControllers();

app.Run();

public partial class Program;
