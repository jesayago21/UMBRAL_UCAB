namespace Umbral.API.Contracts.Sesiones;

public sealed record SubmitEvidenciaRequest(
    Guid ParticipanteId,
    string CodigoQr);
