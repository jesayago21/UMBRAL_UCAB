using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Registro de una pista entregada a un participante en una etapa (RB-21).
/// <see cref="Contenido"/> no-null indica pista ad-hoc del operador (RF-15);
/// null indica entrega de pista de catálogo (texto en el snapshot).
/// </summary>
public sealed class PistaEntregada : ValueObject
{
    public PistaId PistaId { get; }
    public int EtapaIndex { get; }
    public ParticipanteId ParticipanteId { get; }
    public DateTimeOffset EntregadaEn { get; }
    public string? Contenido { get; }

    private PistaEntregada(
        PistaId pistaId,
        int etapaIndex,
        ParticipanteId participanteId,
        DateTimeOffset entregadaEn,
        string? contenido)
    {
        PistaId         = pistaId;
        EtapaIndex      = etapaIndex;
        ParticipanteId  = participanteId;
        EntregadaEn     = entregadaEn;
        Contenido       = contenido;
    }

    public static PistaEntregada Crear(
        PistaId pistaId,
        int etapaIndex,
        ParticipanteId participanteId,
        DateTimeOffset entregadaEn,
        string? contenido = null)
    {
        ArgumentNullException.ThrowIfNull(pistaId);
        ArgumentNullException.ThrowIfNull(participanteId);

        if (etapaIndex < 0)
            throw new DomainException("El índice de etapa no puede ser negativo.");

        return new PistaEntregada(pistaId, etapaIndex, participanteId, entregadaEn, contenido);
    }

    internal static PistaEntregada Rehydrate(
        PistaId pistaId,
        int etapaIndex,
        ParticipanteId participanteId,
        DateTimeOffset entregadaEn,
        string? contenido = null) =>
        new(pistaId, etapaIndex, participanteId, entregadaEn, contenido);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PistaId;
        yield return EtapaIndex;
        yield return ParticipanteId;
        yield return EntregadaEn;
        yield return Contenido ?? string.Empty;
    }
}
