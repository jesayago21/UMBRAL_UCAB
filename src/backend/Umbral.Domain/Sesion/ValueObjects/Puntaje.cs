using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

public sealed class Puntaje : ValueObject
{
    public int Valor { get; }

    private Puntaje(int valor) => Valor = valor;

    public static Puntaje Crear(int valor)
    {
        if (valor < 0)
            throw new DomainException("El puntaje no puede ser negativo.");
        return new Puntaje(valor);
    }

    public static Puntaje Zero() => new(0);

    public Puntaje Sumar(int cantidad) => new(Valor + cantidad);

    public Puntaje Restar(int cantidad) => new(Math.Max(0, Valor - cantidad));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
