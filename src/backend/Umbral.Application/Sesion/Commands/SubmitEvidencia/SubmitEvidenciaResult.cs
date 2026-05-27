using Umbral.Domain.Sesion;

namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

/// <summary>
/// Respuesta de HU-18: evidencia registrada y resultado de validación.
/// </summary>
public sealed record SubmitEvidenciaResult(
    Guid EvidenciaId,
    ResultadoValidacion Resultado);
