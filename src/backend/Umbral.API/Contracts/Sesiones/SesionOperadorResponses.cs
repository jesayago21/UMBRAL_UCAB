namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionResponse(Guid Id, string CodigoAcceso);

public sealed record SesionResumenResponse(
    Guid Id,
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

public sealed record ParticipanteSesionResponse(
    Guid ParticipanteId,
    Guid JugadorId,
    string Nombre);

public sealed record PistaSesionResponse(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionResponse(
    int Orden,
    string TipoEtapa,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionResponse>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds);

public sealed record SesionDetalleResponse(
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
    string? EtapaActivaTipo,
    IReadOnlyList<ParticipanteSesionResponse> Participantes,
    IReadOnlyList<EtapaSesionResponse>? Etapas);

public sealed record SesionDisponibleParticipanteResponse(
    Guid Id,
    string Titulo,
    string Estado,
    int ParticipantesInscritos);

public sealed record UnirseSesionRequest(string CodigoAcceso, string? NombreParticipante);

public sealed record UnirseSesionResponse(Guid ParticipanteId);

public sealed record PreguntaTriviaParticipanteResponse(
    int Orden,
    Guid Id,
    string Enunciado,
    string Dificultad,
    IReadOnlyList<string> Opciones);
