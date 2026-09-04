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
    Task<Sesion?> FindInscripcionAbiertaPorJugadorAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default);

    /// <summary>
    /// Incluye Finalizada/Cancelada solo si no hay sesión operativa; prioriza Activa/Preparación.
    /// </summary>
    Task<Sesion?> FindInscripcionVigentePorJugadorAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default);

    /// <summary>
    /// Quita al jugador de todas las sesiones Finalizada/Cancelada (limpieza post-partida / fantasmas).
    /// </summary>
    Task<int> EliminarParticipacionesEnSesionesTerminalesAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default);
    Task EliminarParticipanteAsync(ParticipanteId participanteId, CancellationToken ct = default);
    Task<bool> ExisteNombreSesionOperativaAsync(string nombre, CancellationToken ct = default);
    Task<int> CountEventosHistorialAsync(SesionId sesionId, CancellationToken ct = default);
    Task<IReadOnlyList<EventoSesion>> ListEventosHistorialAsync(
        SesionId sesionId,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default);
    Task SaveAsync(Sesion sesion, CancellationToken ct = default);
}
