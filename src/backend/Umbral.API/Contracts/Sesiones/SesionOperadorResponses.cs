namespace Umbral.API.Contracts.Sesiones;

public sealed record CrearSesionResponse(Guid Id, string CodigoAcceso);

public sealed record SesionResumenResponse(
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

public sealed record EquipoSesionResponse(
    Guid EquipoId,
    Guid JugadorId,
    string Nombre);

public sealed record PistaSesionResponse(
    string Contenido,
    string TipoLiberacion,
    int? SegundosLiberacion);

public sealed record EtapaSesionResponse(
    int Orden,
    string Descripcion,
    bool EsActual,
    IReadOnlyList<PistaSesionResponse> Pistas);

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
    IReadOnlyList<EquipoSesionResponse> Equipos,
    IReadOnlyList<EtapaSesionResponse>? Etapas);

public sealed record SesionDisponibleEquipoResponse(
    Guid Id,
    string Titulo,
    string Estado,
    int EquiposInscritos);

public sealed record UnirseSesionRequest(string CodigoAcceso, string? NombreEquipo);

public sealed record UnirseSesionResponse(Guid EquipoId);

public sealed record PreguntaTriviaEquipoResponse(
    int Orden,
    Guid Id,
    string Enunciado,
    string Dificultad,
    IReadOnlyList<string> Opciones);
