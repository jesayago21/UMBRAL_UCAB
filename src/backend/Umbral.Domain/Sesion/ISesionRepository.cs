namespace Umbral.Domain.Sesion;

public interface ISesionRepository
{
    Task<Sesion?> FindByIdAsync(SesionId id, CancellationToken ct = default);
    Task<IReadOnlyList<Sesion>> FindActivasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Sesion>> FindOperativasByOperadorAsync(
        UsuarioId operadorId,
        CancellationToken ct = default);
    Task<IReadOnlyList<Sesion>> FindDisponiblesParaParticipanteAsync(
        TipoSesion? tipo,
        CancellationToken ct = default);
    Task SaveAsync(Sesion sesion, CancellationToken ct = default);
}
