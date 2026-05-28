namespace Umbral.API.Contracts.Sesiones;

public sealed record SubmitEvidenciaRequest(
    Guid EquipoId,
    string CodigoQr);
