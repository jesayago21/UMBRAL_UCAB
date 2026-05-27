using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class NombreEquipo : ValueObject
{
    public string Valor { get; }

    private NombreEquipo(string valor) => Valor = valor;

    public static NombreEquipo Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El nombre del equipo no puede estar vacío.");

        return new NombreEquipo(valor.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
