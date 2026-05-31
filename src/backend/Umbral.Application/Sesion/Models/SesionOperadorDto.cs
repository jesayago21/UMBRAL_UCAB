namespace Umbral.Application.Sesion.Models;

public sealed record SesionResumenDto(
    Guid Id,
    string TipoSesion,
    Guid MisionId,
    string MisionNombre,
    string Estado,
    int EquiposCount,
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    int EtapaActualOrden,
    int TotalEtapas,
    string? EtapaActualDescripcion);

public sealed record EquipoSesionDto(
    Guid EquipoId,
    Guid JugadorId,
    string Nombre);

public sealed record PistaSesionDto(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionDto(
    int Orden,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionDto> Pistas);

public sealed record SesionDetalleDto(
    Guid Id,
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
    IReadOnlyList<EquipoSesionDto> Equipos,
    IReadOnlyList<EtapaSesionDto>? Etapas);
