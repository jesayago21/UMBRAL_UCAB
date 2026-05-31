using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class CodigoAcceso : ValueObject
{
    public string Valor { get; }

    private CodigoAcceso(string valor) => Valor = valor;

    public static CodigoAcceso Generar() =>
        new(Guid.NewGuid().ToString("N")[..8].ToUpperInvariant());

    public static CodigoAcceso Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El código de acceso no puede estar vacío.");
        return new CodigoAcceso(valor.Trim().ToUpperInvariant());
    }

    public bool CoincideCon(string ingresado) =>
        string.Equals(Valor, ingresado.Trim(), StringComparison.OrdinalIgnoreCase);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
