using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class Penalizacion : ValueObject
{
    public int Puntos { get; }
    public string Motivo { get; }
    public UsuarioId OperadorId { get; }

    public Penalizacion(int puntos, string motivo, UsuarioId operadorId)
    {
        if (puntos <= 0)
            throw new DomainException("La penalización debe tener un valor mayor a cero.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo de la penalización no puede estar vacío.");

        ArgumentNullException.ThrowIfNull(operadorId);

        Puntos     = puntos;
        Motivo     = motivo.Trim();
        OperadorId = operadorId;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Puntos;
        yield return Motivo;
        yield return OperadorId;
    }
}
