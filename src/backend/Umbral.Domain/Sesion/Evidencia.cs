using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Registro de un envío de evidencia QR por un participante (HU-18, RF-11).
/// </summary>
public sealed class Evidencia : Entity
{
    public EvidenciaId EvidenciaId { get; private set; } = default!;
    public SesionId SesionId { get; private set; } = default!;
    public ParticipanteId ParticipanteId { get; private set; } = default!;
    public EtapaId EtapaId { get; private set; } = default!;
    public CodigoQR CodigoQR { get; private set; } = default!;
    public DateTime TimestampServidor { get; private set; }
    public ResultadoValidacion Resultado { get; private set; }

    private Evidencia() { }

    internal static Evidencia Registrar(
        SesionId sesionId,
        ParticipanteId participanteId,
        EtapaId etapaId,
        CodigoQR codigoQR,
        ResultadoValidacion resultado)
    {
        ArgumentNullException.ThrowIfNull(sesionId);
        ArgumentNullException.ThrowIfNull(participanteId);
        ArgumentNullException.ThrowIfNull(etapaId);
        ArgumentNullException.ThrowIfNull(codigoQR);

        return new Evidencia
        {
            EvidenciaId        = EvidenciaId.Nuevo(),
            SesionId           = sesionId,
            ParticipanteId           = participanteId,
            EtapaId            = etapaId,
            CodigoQR           = codigoQR,
            TimestampServidor  = DateTime.UtcNow,
            Resultado          = resultado
        };
    }

    protected override bool IdEquals(Entity other) =>
        other is Evidencia e && e.EvidenciaId == EvidenciaId;

    protected override int GetIdHashCode() => EvidenciaId.GetHashCode();
}
