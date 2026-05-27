using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class CodigoQR : ValueObject
{
    public string Valor { get; }

    private CodigoQR(string valor) => Valor = valor;

    public static CodigoQR Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El código QR no puede estar vacío.");

        return new CodigoQR(valor.Trim());
    }

    public bool CoincideCon(string otro) =>
        !string.IsNullOrWhiteSpace(otro) &&
        string.Equals(Valor, otro.Trim(), StringComparison.OrdinalIgnoreCase);

    public bool CoincideCon(CodigoQR otro) =>
        string.Equals(Valor, otro.Valor, StringComparison.OrdinalIgnoreCase);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor.ToUpperInvariant();
    }
}
