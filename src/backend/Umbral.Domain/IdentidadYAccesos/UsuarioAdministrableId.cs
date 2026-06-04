namespace Umbral.Domain.IdentidadYAccesos;

public sealed record UsuarioAdministrableId(Guid Valor)
{
    public static UsuarioAdministrableId Nuevo() => new(Guid.NewGuid());
}
