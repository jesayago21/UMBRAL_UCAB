namespace Umbral.Application.Sesion.Models;

public sealed record SesionResumenDto(
    Guid Id,
    string Nombre,
    string TipoSesion,
    Guid MisionId,
    string MisionNombre,
    string Estado,
    int ParticipantesCount,
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    int EtapaActualOrden,
    int TotalEtapas,
    string? EtapaActualDescripcion,
    string? EtapaActivaTipo);

public sealed record ParticipanteSesionDto(
    Guid ParticipanteId,
    Guid JugadorId,
    string Nombre);

public sealed record PistaSesionDto(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionDto(
    int Orden,
    string TipoEtapa,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionDto>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds);

public sealed record SesionDetalleDto(
    Guid Id,
    string Nombre,
    string TipoSesion,
    Guid MisionId,
    string MisionNombre,
    string Estado,
    string CodigoAcceso,
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    int EtapaActualOrden,
    int TotalEtapas,
    string? EtapaActualDescripcion,
    string? EtapaActivaTipo,
    IReadOnlyList<ParticipanteSesionDto> Participantes,
    IReadOnlyList<EtapaSesionDto>? Etapas);
