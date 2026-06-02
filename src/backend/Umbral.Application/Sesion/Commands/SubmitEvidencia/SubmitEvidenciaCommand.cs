using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

/// <summary>
/// HU-18 — Participante envía evidencia QR.
/// </summary>
public sealed record SubmitEvidenciaCommand(
    Guid SesionId,
    Guid ParticipanteId,
    string CodigoQr) : IRequest<Result<SubmitEvidenciaResult>>;
