namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionResponse(Guid Id, string CodigoAcceso);

public sealed record SesionResumenResponse(
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
    string? EtapaActivaTipo,
    int MaxParticipantes = 5);

public sealed record ParticipanteSesionResponse(
    Guid ParticipanteId,
    Guid JugadorId,
    string Nombre);

public sealed record PistaSesionResponse(
    Guid PistaId,
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionResponse(
    int Orden,
    string TipoEtapa,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionResponse>? Pistas,
    IReadOnlyList<Guid>? CategoriaIds,
    double? Latitud = null,
    double? Longitud = null,
    int? RadioMetros = null,
    string? CodigoQrSolucion = null);

public sealed record SesionDetalleResponse(
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
    IReadOnlyList<ParticipanteSesionResponse> Participantes,
    IReadOnlyList<EtapaSesionResponse>? Etapas,
    string? TriviaFase = null,
    int MaxParticipantes = 5);

public sealed record SesionDisponibleParticipanteResponse(
    Guid Id,
    string Titulo,
    string Estado,
    int ParticipantesInscritos,
    int MaxParticipantes = 5);

public sealed record UnirseSesionRequest(string CodigoAcceso, string? NombreParticipante);

public sealed record UnirseSesionResponse(Guid ParticipanteId);

public sealed record MiInscripcionParticipanteResponse(
    Guid SesionId,
    string Titulo,
    Guid ParticipanteId,
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaSesionResponse> Etapas,
    IReadOnlyList<PenalizacionParticipanteResponse>? Penalizaciones = null,
    int ParticipantesInscritos = 0,
    int MaxParticipantes = 5);

public sealed record PenalizacionParticipanteResponse(
    int Puntos,
    string Motivo,
    DateTime OcurridoEn);

public sealed record SesionEtapasParticipanteResponse(
    string Estado,
    int TotalEtapas,
    IReadOnlyList<EtapaSesionResponse> Etapas);

public sealed record PreguntaTriviaParticipanteResponse(
    int Orden,
    Guid Id,
    string Enunciado,
    string Dificultad,
    IReadOnlyList<string> Opciones);
