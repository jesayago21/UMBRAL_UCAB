namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

/// <summary>
/// Respuesta de HU-18: evidencia registrada y resultado de validación.
/// Resultado se expone como string para no filtrar el enum de dominio fuera de Application.
/// </summary>
public sealed record SubmitEvidenciaResult(
    Guid EvidenciaId,
    string Resultado);
