using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class NombreParticipante : ValueObject
{
    public string Valor { get; }

    private NombreParticipante(string valor) => Valor = valor;

    public static NombreParticipante Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El nombre del participante no puede estar vacío.");

        return new NombreParticipante(valor.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
