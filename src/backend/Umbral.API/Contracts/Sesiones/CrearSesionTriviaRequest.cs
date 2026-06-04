namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionTriviaRequest(IReadOnlyList<Guid> CategoriaIds);
