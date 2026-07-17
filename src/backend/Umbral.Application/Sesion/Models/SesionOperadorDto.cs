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
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionDto(
    int Orden,
    string TipoEtapa,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionDto>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null,
    /// <summary>Solo para operador; nunca se expone al participante.</summary>
    string? CodigoQrSolucion = null);

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
    IReadOnlyList<EtapaSesionDto>? Etapas,
    /// <summary>HU-33 — fase trivia si la etapa activa es Trivia; null en otro caso.</summary>
    string? TriviaFase = null);
