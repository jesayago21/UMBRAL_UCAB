namespace Umbral.API.Contracts.Sesiones;

public sealed record SubmitEvidenciaResponse(
    Guid EvidenciaId,
    string Resultado);
